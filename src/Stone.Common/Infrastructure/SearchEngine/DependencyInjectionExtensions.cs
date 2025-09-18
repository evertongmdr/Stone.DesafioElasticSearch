using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace Stone.Common.Infrastructure.SearchEngine
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, SearchEngineSettings settings)
        {
            if (settings == null) throw new ArgumentNullException();

            services.AddSingleton(settings);

            services.AddSingleton(typeof(ISearchEngine<>), typeof(ElasticSearchService<>));

            return services;
        }
    }
}