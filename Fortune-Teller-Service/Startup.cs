using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Fortune_Teller_Service.Models;


// Lab07 Start
using Pivotal.Discovery.Client;
// Lab07 End

// Lab08 Start
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
// Lab08 End

// Lab10 Start
using Steeltoe.Security.Authentication.CloudFoundry;
// Lab10 End

// Lab11 Start
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.Logging;
using Steeltoe.Security.DataProtection.CredHubCore;
// Lab11 End

namespace Fortune_Teller_Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            Environment = env;
            this.logFactory = logFactory;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        private ILoggerFactory logFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            if (Environment.IsDevelopment())
            {
                services.AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<FortuneContext>(
                        options => options.UseInMemoryDatabase("Fortunes"));
            }
            else
            {
                services.AddDbContext<FortuneContext>(
                    options => options.UseMySql(Configuration));
            }

            services.AddScoped<IFortuneRepository, FortuneRepository>();

            services.AddDiscoveryClient(Configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddCloudFoundryJwtBearer(Configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("read.fortunes", policy => policy.RequireClaim("scope", "read.fortunes"));
            });

            services.AddSingleton<IHealthContributor, MySqlHealthContributor>();
            services.AddCloudFoundryActuators(Configuration);

            // Add Credhub Client
            services.AddCredHubClient(Configuration, logFactory);
            //

            services.ConfigureCloudFoundryOptions(Configuration);

            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Fortune Service .NET Core 2.x API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Lab11
            app.UseCloudFoundryActuators();
            // Lab11

            // Lab10 Start
            app.UseAuthentication();
            // Lab10 End

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fortune Service .NET Core 2.x");
            });
            // Lab05 Start
            SampleData.InitializeFortunesAsync(app.ApplicationServices).Wait();
            // Lab05 End

            // Lab07 Start
            app.UseDiscoveryClient();
            // Lab07 End
        }
    }
}
