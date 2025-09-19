using Elastic.Clients.Elasticsearch;

namespace Stone.Common.Infrastructure.SearchEngine
{
    public interface ISearchEngine<T> where T : class
    {
        Task BulkInsertAsync(IEnumerable<T> items, string indexName);

        Task BulkInsertAsync(IEnumerable<T> items, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Insere uma coleção de itens no Elasticsearch em lotes (bulk insert).
        /// </summary>
        /// <param name="bulkSize">
        /// O tamanho máximo de cada lote de inserção. Se <c>null</c>, será usado o valor padrão(5000) definido no serviço.
        /// </param>

        Task BulkInsertAsync(IEnumerable<T> items, string indexName, int? bulkSize, CancellationToken cancellationToken);

        Task<List<T>> SearchSafeAsync(SearchRequest searchRequest);

    }
}
