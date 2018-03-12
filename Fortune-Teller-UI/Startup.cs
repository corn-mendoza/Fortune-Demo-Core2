using FortuneService.Client;
using FortuneTeller.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Pivotal.Helper;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Security.Authentication.CloudFoundry;
using Steeltoe.Security.DataProtection;
using System;

namespace FortuneTeller
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

            // Enable Redis function if not offline
            if (!Environment.IsDevelopment())
            {
                // Use Redis cache on CloudFoundry to DataProtection Keys
                services.AddRedisConnectionMultiplexer(Configuration);
                services.AddDataProtection()
                    .PersistKeysToRedis()
                    .SetApplicationName("workshopui");
            }
            // End Redis

            // Add service client library for calling the Fortune Service
            services.AddScoped<IFortuneService, FortuneServiceClient>();
            // End add service client

            // Load Fortune Service Options 
            services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));
            // End load Fortune Service

            // Workshop Configuration
            services.Configure<ConfigServerData>(Configuration.GetSection("workshopConfig"));
            services.AddConfiguration(Configuration);

            // Add for Service Options
            services.ConfigureCloudFoundryOptions(Configuration);
            //

            // Add Service Discovery
            services.AddDiscoveryClient(Configuration);
            // End Service Discovery

            // Add Session Caching function
            if (Environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                // Use Redis cache on CloudFoundry to store session data
                services.AddDistributedRedisCache(Configuration);
            }
            services.AddSession();
            // End Session Cache

            // Add Single Sign-on functionality
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
            // End Add SSO

            // Add Circuit Breaker function
            services.AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration);
            services.AddHystrixMetricsStream(Configuration);
            // End Add CB

            services.AddSingleton<IHealthContributor, SqlServerHealthContributor>();

            // Add Cloud Foundry Management actuator endpoint functions
            services.AddCloudFoundryActuators(Configuration);
            // End CF Management

            services.AddMvc();

            // Update the connection strings from appSettings.json or Config Server from any User Provided Service of the same name
            // User Provided Service will take presidence over other sources
            CFEnvironmentVariables.UpdateConnectionStrings(Configuration);
            var dbString = Configuration.GetConnectionString("AttendeeContext");

            services.AddDbContext<AttendeeContext>(options => options.UseSqlServer(dbString));
            // End connection strings

            // Add RabbitMQ function
            services.AddRabbitMQConnection(Configuration);
            // End RabbitMQ
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

            // Perform some database initialisation.
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<AttendeeContext>();

                dbContext.Database.EnsureCreated();
            }

            app.UseStaticFiles();

            app.UseCloudFoundryActuators();

            app.UseHystrixRequestContext();

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Workshop}/{action=Index}/{id?}");
            });

            app.UseDiscoveryClient();

            if (!Environment.IsDevelopment())
            {
                app.UseHystrixMetricsStream();
            }
        }
    }
}
