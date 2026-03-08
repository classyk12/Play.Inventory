using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Catalog.Contract.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> _repository = repository;
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;

            //prevent duplicates from reprocessing the same message
            var item = await _repository.GetAsync(message.Id);
            if (item is null)
            {
                item = new CatalogItem
                {
                    Id = message.Id,
                    Name = message.Name,
                    Description = message.Description
                };

                await _repository.CreateAsync(item);
                return;
            }

            item.Name = message.Name;
            item.Description = message.Description;

            await _repository.UpdateAsync(item);
        }
    }
}