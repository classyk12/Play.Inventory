using Play.Catalog.Service;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Endpoints;

public static class ItemEndpoints
{
    public static WebApplication MapItemEndpoints(this WebApplication app)
    {
        var itemGroup = app.MapGroup("/api/items")
        .WithOpenApi()
        .WithTags("Items");

        itemGroup.MapGet("/", async (IRepository<InventoryItem> repository) => Results.Ok(await repository.GetAllAsync()))
           .WithName("GetItemsAsync");

        itemGroup.MapGet("user/{userId:guid}", async (Guid UserId, IRepository<InventoryItem> repository, CatalogClient catalogClient) =>
        {
            if (UserId == Guid.Empty)
            {
                return Results.BadRequest();
            }

            var catalogItems = await catalogClient.GetItemsAsync();
            var item = await repository.GetAllAsync(i => i.UserId == UserId);

            var result = from i in item
                         join c in catalogItems! on i.CatalogItemId equals c.Id
                         select i.AsDto(c);

            return item is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetItemByUserIdAsync");

        itemGroup.MapGet("{id:guid}", async (Guid id, IRepository<InventoryItem> repository) =>
        {
            var item = await repository.GetAsync(id);
            return item is not null ? Results.Ok(item.AsDto()) : Results.NotFound();
        })
        .WithName("GetItemByIdAsync");

        itemGroup.MapPost("/", async (CreateInventoryItemDto dto, IRepository<InventoryItem> repository) =>
        {
            var errors = ModelValidator.ValidateDto(dto);
            if (errors.Count > 0)
                return Results.BadRequest(errors);

            var inventoryItem = await repository.GetAsync(i => i.UserId == dto.UserId && i.CatalogItemId == dto.CatalogItemId);
            if (inventoryItem is not null)
            {
                inventoryItem.Quantity += dto.Quantity;
                await repository.UpdateAsync(inventoryItem);
                return Results.Ok(inventoryItem.AsDto());
            }

            var item = new InventoryItemDto(Guid.NewGuid(), dto.UserId, dto.CatalogItemId, string.Empty, string.Empty, dto.Quantity, DateTimeOffset.UtcNow);
            await repository.CreateAsync(item.AsEntity());
            return Results.Created($"/items/{item.Id}", item);
        })
        .WithName("CreateItemAsync");

        return app;
    }
}
