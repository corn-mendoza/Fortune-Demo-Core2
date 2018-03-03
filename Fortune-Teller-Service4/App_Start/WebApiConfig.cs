using Swagger.Net.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace FortuneTellerService4
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
      
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "HealthApi",
                routeTemplate: "api/{controller}/health",
                defaults: new { controller = "Fortunes", action = "Health" }
            );

            config.Routes.MapHttpRoute(
                name: "swagger_root",
                routeTemplate: "",
                defaults: null,
                constraints: null,
                handler: new RedirectHandler(message => SwaggerConfig.GetRootUrlFromAppConfig(message), "swagger"));

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}",
                defaults: new { controller = "Fortunes", action = "Get" }
            );

            config.Routes.MapHttpRoute(
                name: "RandomApi",
                routeTemplate: "api/{controller}/random",
          defaults: new { controller = "Fortunes", action = "Random" }
            );
        }

    }
}
