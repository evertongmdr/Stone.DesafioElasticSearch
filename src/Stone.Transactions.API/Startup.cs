using Stone.Transactions.API.Configuration;
using Stone.Common.Infrastructure.SearchEngine;

namespace Stone.Transactions.API
{
    public class Startup : IAppStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddApiConfiguration(Configuration);

            services.AddSwaggerConfiguration();

            services.RegisterServices();

         


        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseSwaggerConfiguration();

            app.UseApiConfiguration(env);
        }
    }

    public interface IAppStartup
    {
        IConfiguration Configuration { get; }
        void Configure(WebApplication app, IWebHostEnvironment env);

        void ConfigureServices(IServiceCollection services);
    }

    public static class StartupExtensions
    {
        public static WebApplicationBuilder UseStartup<TStartup>(this WebApplicationBuilder webAppBuilder) where TStartup : IAppStartup
        {
            var startup = Activator.CreateInstance(typeof(TStartup), webAppBuilder.Configuration) as IAppStartup;

            if (startup == null) throw new ArgumentException($"Classe Startup.cs inválida!");

            startup.ConfigureServices(webAppBuilder.Services);

            var app = webAppBuilder.Build();

            startup.Configure(app, app.Environment);

            app.Run();

            return webAppBuilder;
        }
    }

}

