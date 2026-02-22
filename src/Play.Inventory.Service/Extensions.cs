using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem item, CatalogItemDto? catalogItem = null)
        {
            return new InventoryItemDto(item.Id, item.UserId, item.CatalogItemId, catalogItem?.Name ?? string.Empty, catalogItem?.Description ?? string.Empty, item.Quantity, item.AcquiredDate);
        }

        public static InventoryItem AsEntity(this InventoryItemDto itemDto)
        {
            return new InventoryItem
            {
                Id = itemDto.Id,
                UserId = itemDto.UserId,
                CatalogItemId = itemDto.CatalogItemId,
                Quantity = itemDto.Quantity,
                AcquiredDate = itemDto.AcquiredDate
            };
        }
    }
}