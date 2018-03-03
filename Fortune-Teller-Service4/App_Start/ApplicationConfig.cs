
using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;

namespace FortuneTellerService4
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationConfig
    {

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Registers the configuration.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public static void RegisterConfig(string environment)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(GetContentRoot())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddCloudFoundry()
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        /// <summary>
        /// Gets the content root.
        /// </summary>
        /// <returns></returns>
        public static string GetContentRoot()
        {
            var basePath = (string)AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") ??
               AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(basePath);
        }
    }

}