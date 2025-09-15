using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stone.Transactions.Producer.Extensions;
using Stone.Transactions.Producer.Producers;
using Stone.Transactions.Producer.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        services.Configure<AppTransactionsProducerSettings>(context.Configuration);

        // Registro de serviços
        services.AddSingleton<ITransactionProducer, TransactionProducer>();
        services.AddSingleton<ITransactionDataGenerator, TransactionDataGenerator>();

        // Worker que orquestra o fluxo
        services.AddHostedService<MenuDataGeneratorWorker>();
    })
    .RunConsoleAsync();