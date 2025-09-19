using Stone.Common.Core.Notifications;
using Stone.Transactions.Application.Interfaces;
using Stone.Transactions.Application.Services;
using Stone.Transactions.Domain.Interfaces.Services;
using Stone.Transactions.Infrastructure.SearchEngine.Queries;

namespace Stone.Transactions.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {

            services.AddScoped<NotificationContext>();

            // Application
            services.AddScoped<ITransactionService, TransactionService>();

            // Search Engine

            services.AddScoped<ITransactoinSearchEngine, TransactionElasticSearchService>();

        }
    }
}
