using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Catalog.Contract.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer(IRepository<CatalogItem> repository) : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> _repository = repository;
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;

            //prevent duplicates from reprocessing the same message
            var item = await _repository.GetAsync(message.Id);
            if (item is null)
            {
                return;
            }

            await _repository.DeleteAsync(message.Id);
        }
    }
}
