using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;
using Microsoft.AspNetCore.ResponseCompression;
using Polly;

namespace Stone.Common.Infrastructure.SearchEngine
{
    public class ElasticSearchService<T> : ISearchEngine<T> where T : class
    {
        private const int BulkSize = 5000;
        protected ElasticsearchClient _client;
        private readonly SearchEngineSettings _settings;


        public ElasticSearchService(SearchEngineSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

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

            int chunkSize = bulkSize ?? BulkSize;

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
                    throw new InvalidOperationException($"Bulk insert falhou: {response.Items.FirstOrDefault()?.Error?.Reason}");
            }
        }

        public async Task<List<T>> SearchSafeAsync(SearchRequest searchRequest)
        {
            EnsureConnected();

            try
            {
                var response = await _client.SearchAsync<T>(searchRequest);

                if (!response.IsValidResponse || response.ElasticsearchServerError != null)
                {

                    throw new InvalidOperationException(
                        $"Erro ao buscar dados no Elasticsearch: {response.ElasticsearchServerError?.Error?.Reason ?? "Erro Desconhecido"}");
                }

                return response.Documents.ToList();
            }
            catch (Exception e)
            {

                throw new InvalidOperationException("Erro inesperado ao executar a busca no Elasticsearch.", e);
            }
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
                    var clientSettings = new ElasticsearchClientSettings(new Uri(_settings.Uri)).DisableDirectStreaming();

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
