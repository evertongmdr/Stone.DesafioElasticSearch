using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stone.Common.Infrastructure.SearchEngine;
using Stone.Transactions.Consumer.Consumers;
using Stone.Transactions.Consumer.Extensions;
using Stone.Transactions.Domain.Entities;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        services.Configure<AppTransactionsConsumerSettings>(context.Configuration);


        var containerInstance = Environment.GetEnvironmentVariable("CONTAINER_INSTANCE");

        var settings = context.Configuration.Get<AppTransactionsConsumerSettings>();

        services.AddElasticSearch(settings.SearchEngine);

        var containerConfig = settings.Containers.Find(c => c.Name == containerInstance);

        for (int i = 0; i < containerConfig.ConsumerNames.Count; i++)
        {
            var consumerName = containerConfig.ConsumerNames[i];

            services.AddSingleton<IHostedService>(sp => new TransactionConsumer(
                sp.GetRequiredService<IOptions<AppTransactionsConsumerSettings>>(),
                sp.GetRequiredService<ILogger<TransactionConsumer>>(),
                sp.GetRequiredService<ISearchEngine<Transaction>>(),
                containerConfig.Name,
                consumerName));

        }

    })
    .RunConsoleAsync();
