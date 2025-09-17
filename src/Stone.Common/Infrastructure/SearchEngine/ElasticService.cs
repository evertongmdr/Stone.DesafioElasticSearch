using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;

namespace Stone.Common.Infrastructure.SearchEngine
{
    public class ElasticService<T> : IElasticService<T> where T : class
    {
        private readonly ElasticsearchClient _client;
        private readonly int _bulkSize;

        public ElasticService(ElasticsearchClient client, int bulkSize = 5000)
        {
            _client = client;
            _bulkSize = bulkSize;
        }

        public async Task BulkInsertAsync(IEnumerable<T> items)
        {
            await BulkInsertAsync(items, null, CancellationToken.None);
        }

        public async Task BulkInsertAsync(IEnumerable<T> items, CancellationToken cancellationToken)
        {
            await BulkInsertAsync(items, null, cancellationToken);
        }

        public async Task BulkInsertAsync(IEnumerable<T> items, int? bulkSize, CancellationToken cancellationToken)
        {
            int chunkSize = bulkSize ?? _bulkSize;

            foreach (var chunk in items.Chunk(chunkSize))
            {
                var bulkRequest = new BulkRequest
                {
                    Operations = chunk
                        .Select(i => new BulkIndexOperation<T>(i))
                        .Cast<IBulkOperation>()
                        .ToList()
                };

                var response = await _client.BulkAsync(bulkRequest, cancellationToken);

                if (response.Errors)
                    throw new Exception($"Bulk insert falhou: {response.Items.FirstOrDefault()?.Error?.Reason}");
            }
        }


        //TODO? porque o ToLower?
        public async Task IndexAsync(T item, CancellationToken cancellationToken = default)
        {
            await _client.IndexAsync(item, idx => idx.Index(typeof(T).Name.ToLower()), cancellationToken);
        }
    }
}
