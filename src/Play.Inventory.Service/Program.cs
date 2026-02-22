using Play.Common;
using Play.Common.MongoDb;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Endpoints;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMongoServices();

// register the open-generic repository; MongoRepository<T> now computes its own collection name
builder.Services.AddSingleton(typeof(IRepository<>), typeof(MongoRepository<>));
builder.Services.AddHttpClient<CatalogClient>(c =>
{
    var settings = builder.Configuration.GetSection("PlayCatalogService").Get<CatalogClientSettings>();
    c.BaseAddress = new Uri(settings?.HostAddress ?? throw new InvalidOperationException("Catalog service host address is not configured"));
}).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));

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