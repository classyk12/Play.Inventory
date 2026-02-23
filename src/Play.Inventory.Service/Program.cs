using Play.Common;
using Play.Common.MongoDb;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Endpoints;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMongoServices();

// register the open-generic repository; MongoRepository<T> now computes its own collection name
builder.Services.AddSingleton(typeof(IRepository<>), typeof(MongoRepository<>));

AddCatalogClientHttpConfiguration(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1");
        c.RoutePrefix = string.Empty;
    });
}

// Map minimal API endpoints (Item endpoints)
app.MapItemEndpoints();

app.UseHttpsRedirection();

app.Run();

static void AddCatalogClientHttpConfiguration(WebApplicationBuilder builder)
{
    //if we have multiple services making HTTP calls to the catalog service, we introduce randomness to our retry attempts, which can help to prevent a thundering herd problem where all services retry at the same time, potentially overwhelming the catalog service. By adding a random jitter to the retry delay, we can spread out the retry attempts and reduce the likelihood of simultaneous retries causing additional load on the catalog service.
    var jitterer = new Random();

    builder.Services.AddHttpClient<CatalogClient>(c =>
    {
        var settings = builder.Configuration.GetSection("PlayCatalogService").Get<CatalogClientSettings>();
        c.BaseAddress = new Uri(settings?.HostAddress ?? throw new InvalidOperationException("Catalog service host address is not configured"));
    }).
    //the below policy will retry up to 3 times with an exponential backoff strategy, and it will also handle timeout exceptions by retrying the request.
    AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(3, retryAttempt =>
    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 100)),
    onRetry: (outcome, timespan, retryAttempt, context) =>
    {
        // Log the retry attempt and the reason for the retry
        Console.WriteLine($"Retrying... Attempt: {retryAttempt}, Reason: {outcome.Exception?.Message}");
    }
    )).
    AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(3, TimeSpan.FromSeconds(15),
    onBreak: (outcome, timespan) =>
    {
        // Log the circuit breaker state change
        Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message}");
    },

    onReset: () =>
    {
        // Log the circuit breaker reset
        Console.WriteLine("Circuit breaker reset.");
    }
    )).
    AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));
}