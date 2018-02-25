using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Management.CloudFoundry;

namespace RabbitConsoleService
{
    public class Startup
    {
        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging();
            //// Lab08 Start
            //if (!Environment.IsDevelopment())
            //{
            //    // Use Redis cache on CloudFoundry to DataProtection Keys
            //    services.AddRedisConnectionMultiplexer(Configuration);
            //    services.AddDataProtection()
            //        .PersistKeysToRedis()
            //        .SetApplicationName("RabbitConsoleService");
            //}
            //Lab08 End

            //// Lab05 Start
            //services.AddScoped<IFortuneService, FortuneServiceClient>();
            //// Lab05 End

            //// Lab05 Start
            //services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));
            //// Lab05 End

            // Add for Service Options
            services.ConfigureCloudFoundryOptions(Configuration);
            services.Configure<Application>(Configuration);

            // Lab08 Start
            //if (Environment.IsDevelopment())
            //{
            //    services.AddDistributedMemoryCache();
            //}
            //else
            //{
            //    // Use Redis cache on CloudFoundry to store session data
            //    services.AddDistributedRedisCache(Configuration);
            //}
            // Lab08 End
            services.AddRabbitMQConnection(Configuration);

            //services.AddSingleton<IHealthContributor, SqlServerHealthContributor>();
            
            services.AddCloudFoundryActuators(Configuration);

            services.AddTransient<Application>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCloudFoundryActuators();
        }
    }
}
