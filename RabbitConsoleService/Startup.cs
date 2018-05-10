using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Security.DataProtection.CredHubCore;

namespace RabbitConsoleService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            Environment = env;
            this.logFactory = logFactory;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        private ILoggerFactory logFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddLogging();

            //services.AddCloudFoundryActuators(Configuration);

            //if (!Environment.IsDevelopment())
            //{
            //    // Use Redis cache on CloudFoundry to DataProtection Keys
            //    services.AddRedisConnectionMultiplexer(Configuration);
            //    services.AddDataProtection()
            //        .PersistKeysToRedis()
            //        .SetApplicationName("RabbitConsoleService");
            //}

            //services.AddScoped<IFortuneService, FortuneServiceClient>();

            //services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));

            // Add for Service Options
            services.ConfigureCloudFoundryOptions(Configuration);
            services.Configure<Application>(Configuration);

            // Add Credhub Client
            services.AddCredHubClient(Configuration, logFactory);
            //

            //if (Environment.IsDevelopment())
            //{
            //    services.AddDistributedMemoryCache();
            //}
            //else
            //{
            //    // Use Redis cache on CloudFoundry to store session data
            //    services.AddDistributedRedisCache(Configuration);
            //}

            services.AddRabbitMQConnection(Configuration);

            services.AddTransient<Application>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseCloudFoundryActuators();
        }
    }
}
