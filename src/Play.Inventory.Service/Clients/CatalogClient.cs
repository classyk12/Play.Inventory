using System.Collections;

namespace Play.Inventory.Service.Clients
{
    public class CatalogClientSettings
    {
        public string HostAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simple wrapper around <see cref="HttpClient"/> that knows how to
    /// talk to the catalog microservice.  The service is expected to expose
    /// a REST API managing <see cref="CatalogItemDto"/> resources.
    /// </summary>
    public class CatalogClient(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        /// <summary>
        /// Retrieves a single catalog item by identifier.
        /// </summary>
        public async Task<CatalogItemDto?> GetItemAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be provided", nameof(id));

            return await _httpClient.GetFromJsonAsync<CatalogItemDto>($"items/{id}");
        }

        /// <summary>
        /// Retrieves multiple catalog items.  The caller can supply an
        /// enumerable of identifiers; they will be joined into the query string.
        /// </summary>
        public async Task<IEnumerable<CatalogItemDto>?> GetItemsAsync()
        {
            return [];
            return await _httpClient.GetFromJsonAsync<IEnumerable<CatalogItemDto>>($"items");
        }
    }
}