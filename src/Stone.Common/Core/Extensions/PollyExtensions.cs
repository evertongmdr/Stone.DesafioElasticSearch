using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Stone.Common.Core.Extensions
{
    public static class PollyExtensions
    {
        public static AsyncRetryPolicy RetryKafka(ILogger logger)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(5),
                },
                (ex, time) =>
                {
                    logger.LogWarning($"Falha ao processar mensagem: {ex.Message}. Tentando novamente em {time.TotalSeconds}s");
                });
        }
    }
}
