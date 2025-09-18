using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;
using Polly;

namespace Stone.Common.Infrastructure.SearchEngine
{
    // TODO avaliar os nome depois
    public class ElasticSearchService<T> : ISearchEngine<T> where T : class
    {
        private ElasticsearchClient _client;
        private readonly SearchEngineSettings _settings;

        private readonly int _bulkSize;

        public ElasticSearchService(SearchEngineSettings settings, int bulkSize = 5000)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _bulkSize = bulkSize;

            ConnectWithRetry();
        }


        public async Task BulkInsertAsync(IEnumerable<T> items, string indexName)
        {
            await BulkInsertAsync(items, null, CancellationToken.None);
        }

        public async Task BulkInsertAsync(IEnumerable<T> items, string indexName, CancellationToken cancellationToken)
        {
            await BulkInsertAsync(items, null, cancellationToken);
        }

        public async Task BulkInsertAsync(IEnumerable<T> items, string indexName, int? bulkSize, CancellationToken cancellationToken)
        {
            EnsureConnected();

            int chunkSize = bulkSize ?? _bulkSize;

            foreach (var chunk in items.Chunk(chunkSize))
            {
                var bulkRequest = new BulkRequest(indexName)
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


        //TODO: porque o ToLower?
        public async Task IndexAsync(T item, CancellationToken cancellationToken = default)
        {
            await _client.IndexAsync(item, idx => idx.Index(typeof(T).Name.ToLower()), cancellationToken);
        }

        private void EnsureConnected()
        {
            if (_client == null)
                ConnectWithRetry();
        }

        private void ConnectWithRetry()
        {
            if (_client != null) return;

            try
            {
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetry(
                        retryCount: 3,
                        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
                    );

                policy.Execute(() =>
                {
                    var clientSettings = new ElasticsearchClientSettings(new Uri(_settings.Uri));

                    if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
                        clientSettings = clientSettings.Authentication(
                            new BasicAuthentication(_settings.Username, _settings.Password));

                    _client = new ElasticsearchClient(clientSettings);
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Falha ao conectar com o Elasticsearch em '{_settings.Uri}' após múltiplas tentativas. " +
                    "Verifique se o servidor está acessível e se as credenciais estão corretas.", ex);
            }
        }
    }
}
