using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Play.Common.Common;

namespace Play.Inventory.Service.Entities
{
    public class CatalogItem : IEntity
    {
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}