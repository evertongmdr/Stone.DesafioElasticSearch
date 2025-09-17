using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Stone.Common.Extensions
{
    public static class PollyExtensions
    {

        public static RetryPolicy RetryKafka(ILogger logger)
        {
            return Policy.Handle<Exception>().WaitAndRetry(new[]
            {
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
            }, (ex, time) => { logger.LogWarning($"Falha ao processar mensagem: {ex.Message}. Tentando novamente em {time.TotalSeconds}s"); });
        }


        public static AsyncRetryPolicy RetryElasticAsync(ILogger logger)
        {
            return Policy.Handle<Exception>().WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(0.5),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromMinutes(1),
            }, (ex, time) =>
            { logger.LogWarning($"Falha ao salvar o(s) dado(s): {ex.Message}. Tentando novamente em {time.TotalSeconds}s"); });
        }

    }
}

