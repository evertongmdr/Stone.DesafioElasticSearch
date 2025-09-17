using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stone.Transactions.Consumer.Consumers;
using Stone.Transactions.Consumer.Extensions;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        services.Configure<AppTransactionsConsumerSettings>(context.Configuration);

        var containerInstance = Environment.GetEnvironmentVariable("CONTAINER_INSTANCE");

        var settings = context.Configuration.Get<AppTransactionsConsumerSettings>();

        var containerConfig = settings.Containers.Find(c => c.Name == containerInstance);

        for (int i = 0; i < containerConfig.ConsumerNames.Count; i++)
        {
            var consumerName = containerConfig.ConsumerNames[i];

            services.AddHostedService(sp => new TransactionConsumer(
                sp.GetRequiredService<ILogger<TransactionConsumer>>(),
                sp.GetRequiredService<IOptions<AppTransactionsConsumerSettings>>(),
                containerConfig.Name,
                consumerName));

        }

    })
    .RunConsoleAsync();
