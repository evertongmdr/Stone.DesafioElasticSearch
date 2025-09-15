using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Stone.Transactions.Producer.Services
{
    public class MenuDataGeneratorWorker : BackgroundService
    {
        private readonly ILogger<MenuDataGeneratorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MenuDataGeneratorWorker(ILogger<MenuDataGeneratorWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunMenuDataGenerationLoopAsync();
        }

        public async Task RunMenuDataGenerationLoopAsync()
        {
            DisplayDataGenerationMenu();

            var options = new Dictionary<string, (int batchSize, int maxBatchesPerSend, int delayMs)>
            {
                { "1", (5000, 100, 500) },    // Alta carga (500k msgs por transação)
                { "2", (1000, 50, 250) },     // Média carga (50k msgs por transação)
                { "3", (100, 10, 200) }       // Baixa carga (1k msgs por transação)
            };

            var choice = Console.ReadLine();

            while (choice != "0")
            {
                try
                {
                    if (options.TryGetValue(choice, out var option))
                    {
                        Console.WriteLine($"Você escolheu o cenário {choice} (batchSize={option.batchSize}, delayMs={option.delayMs})");

                        using var scope = _serviceProvider.CreateScope();
                        var transactionDataGenerator = scope.ServiceProvider.GetRequiredService<ITransactionDataGenerator>();

                        await transactionDataGenerator.GenerateAndPublishDataAsync(option.batchSize,option.maxBatchesPerSend, option.delayMs);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Opção inválida! Por favor, escolha 1, 2, 3 ou 0 (Sair).");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    _logger.LogError($"Ocorreu um erro: {ex.Message}\n\n");

                    DisplayDataGenerationMenu();
                }

                choice = Console.ReadLine();
            }
        }



        private void DisplayDataGenerationMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(StoneArt());
            Console.ResetColor();

            Console.WriteLine("===================================");
            Console.WriteLine("        GERAR MASSA DE DADOS       ");
            Console.WriteLine("===================================\n");

            Console.WriteLine("Escolha o cenário de estresse:\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1 - Cenário de Alta Carga (Throughput Alto): Envio rápido de 500.000 mensagens por transação, simulando carga máxima e testes de escalabilidade.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("2 - Cenário de Carga Média (Throughput Médio): Envio de 50.000 mensagens por transação, simulando condições normais de operação.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("3 - Cenário de Baixa Carga (Throughput Baixo / Ineficiente): Envio de 1.000 mensagens por transação, simulando picos ou condições adversas.");
            Console.ResetColor();
            Console.WriteLine("0 - Sair\n");

            // Observação sobre o volume de dados
            Console.WriteLine("OBS: Todos os cenários gerarão um total de 20GB de dados.");
        }


        private string StoneArt()
        {
            return @"
   _____ _                   
  / ____| |                  
 | (___ | |_ ___  _ __   ___ 
  \___ \| __/ _ \| '_ \ / _ \
  ____) | || (_) | | | |  __/
 |_____/ \__\___/|_| |_|\___|
";
        }
    }
}
