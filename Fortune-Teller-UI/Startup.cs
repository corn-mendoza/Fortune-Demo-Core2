
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

// Lab07 Start
using Pivotal.Discovery.Client;
// Lab07 End

// Lab08 Start
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Security.DataProtection;
// Lab08 End

// Lab10 Start
using Steeltoe.Security.Authentication.CloudFoundry;
// Lab10 End

// Lab09 Start
using Steeltoe.CircuitBreaker.Hystrix;
// Lab09 End

// Lab11 Start
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using Workshop_UI.Models;
using Microsoft.EntityFrameworkCore;
using Pivotal.Extensions.Configuration.ConfigServer;
using System.Linq;
using Pivotal.Helper;
using Steeltoe.Management.Endpoint.Health;
using FortuneService.Client;
// Lab11 End



namespace Workshop_UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            // Lab08 Start
            if (!Environment.IsDevelopment())
            {
                // Use Redis cache on CloudFoundry to DataProtection Keys
                services.AddRedisConnectionMultiplexer(Configuration);
                services.AddDataProtection()
                    .PersistKeysToRedis()
                    .SetApplicationName("workshopui");
            }
            // Lab08 End

            // Lab05 Start
            services.AddScoped<IFortuneService, FortuneServiceClient>();
            // Lab05 End

            // Lab05 Start
            services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));
            // Lab05 End

            // Workshop Configuration
            services.Configure<ConfigServerData>(Configuration.GetSection("workshopConfig"));
            services.AddConfiguration(Configuration);

            // Add for Service Options
            //services.Configure<CloudFoundryServicesOptions>(Configuration);
            //services.Configure<CloudFoundryApplicationOptions>(Configuration);
            services.ConfigureCloudFoundryOptions(Configuration);
            //

            // Lab07 Start
            services.AddDiscoveryClient(Configuration);

            // Lab07 End

            // Lab08 Start
            if (Environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                // Use Redis cache on CloudFoundry to store session data
                services.AddDistributedRedisCache(Configuration);
            }
            // Lab08 End

            // Lab10 Start
            services.AddAuthentication((options) =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CloudFoundryDefaults.AuthenticationScheme;

            })
            .AddCookie((options) =>
            {
                options.AccessDeniedPath = new PathString("/Workshop/AccessDenied");

            })
            .AddCloudFoundryOAuth(Configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("read.fortunes", policy => policy.RequireClaim("scope", "read.fortunes"));

            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Lab10 End

            // Lab09 Start
            services.AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration);
            services.AddHystrixMetricsStream(Configuration);
            // Lab09 End

            services.AddSession();

            services.AddSingleton<IHealthContributor, SqlServerHealthContributor>();

            // Lab11 Start
            services.AddCloudFoundryActuators(Configuration);
            // Lab11 End

            services.AddMvc();

            // Use the Bound Service for connection string if it is found in a User Provided Service
            string sourceString = "appsettings.json";
            string dbString = Configuration.GetConnectionString("AttendeeContext");
            IConfigurationSection configurationSection = Configuration.GetSection("ConnectionStrings");
            if (configurationSection != null)
            {
                if (configurationSection.GetValue<string>("AttendeeContext") != null)
                {
                    dbString = configurationSection.GetValue<string>("AttendeeContext");
                    sourceString = "Config Server";
                }
            }
            else
            {
                var cfe = new CFEnvironmentVariables();
                var _connect = cfe.getConnectionStringForDbService("user-provided", "AttendeeContext");
                if (!string.IsNullOrEmpty(_connect))
                {
                    sourceString = "User Provided Service";
                }
            }

            Console.WriteLine($"Using connection string from the {sourceString}");

            services.AddDbContext<AttendeeContext>(options =>
                    options.UseSqlServer(dbString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Workshop/Error");
            }

            app.UseStaticFiles();

            // Lab11 Start
            app.UseCloudFoundryActuators();
            // Lab11 End

            // Lab09 Start
            app.UseHystrixRequestContext();
            // Lab09 End

            app.UseSession();

            // Lab10 Start
            app.UseAuthentication();
            // Lab10 End

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Workshop}/{action=Index}/{id?}");
            });


            // Lab07 Start
            app.UseDiscoveryClient();
            // Lab07 End

            // Lab09 Start
            if (!Environment.IsDevelopment())
            {
                app.UseHystrixMetricsStream();
            }
            // Lab09 End
        }
    }
}
