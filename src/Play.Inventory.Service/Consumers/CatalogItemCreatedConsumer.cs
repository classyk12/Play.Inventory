using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Catalog.Contract.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemCreatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> _repository = repository;
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;

            //prevent duplicates from reprocessing the same message
            var item = await _repository.GetAsync(message.Id);
            if (item is not null)
            {
                return;
            }

            item = new CatalogItem
            {
                Id = message.Id,
                Name = message.Name,
                Description = message.Description
            };

            await _repository.CreateAsync(item);
        }
    }
}