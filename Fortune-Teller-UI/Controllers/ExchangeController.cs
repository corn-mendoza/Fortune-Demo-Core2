using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace Workshop_UI.Controllers
{
    public class ExchangeController : Controller
    {
        ILogger<ExchangeController> _logger;
        private IConfiguration Config { get; set; }
        IDiscoveryClient discoveryClient;

        public ExchangeController(
            ILogger<ExchangeController> logger, 
            IConfiguration config,
            [FromServices] IDiscoveryClient client
            )
        {
            _logger = logger;
            Config = config;
            discoveryClient = client;
        }

        //[FromServices]
        //IDiscoveryClient discoveryClient
        public IActionResult Index()
        {
            if (discoveryClient != null)
            {
                _logger?.LogDebug("Index");

                var omsUrl = discoveryClient.GetInstances("ORDERMANAGER")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:8080";
                omsUrl = omsUrl.Replace("https://", "http://"); // need to force http due to self signed cert

                _logger?.LogInformation($"Order Manager URL: {omsUrl}");

                ViewBag.OMS = omsUrl;
                ViewBag.MDS = discoveryClient.GetInstances("MDS")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:53809";
                _logger?.LogInformation($"Market Data Server URL: {ViewBag.MDS}");
            }
            return View();
        }
    }
}