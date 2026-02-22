using System.ComponentModel.DataAnnotations;

namespace Play.Inventory.Service;

public record CreateInventoryItemDto(
    [property: Required]
    Guid UserId,
    [property: Required]
    Guid CatalogItemId,
    [property: Range(0, 9999999)]
    int Quantity
);

public record InventoryItemDto(
    Guid Id,
    Guid UserId,
    Guid CatalogItemId,
    string CatalogItemName,
    string CatalogItemDescription,
    int Quantity,
    DateTimeOffset AcquiredDate
);

public record UpdateInventoryItemDto(
    [property: Required]
    Guid UserId,
    [property: Required]
    Guid CatalogItemId,
    [property: Range(0, 9999999)]
    int Quantity
);

public record CatalogItemDto(
    Guid Id,
    string Name,
    string Description
);
