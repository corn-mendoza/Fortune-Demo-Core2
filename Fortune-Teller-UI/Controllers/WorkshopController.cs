
using FortuneService.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pivotal.Helper;
using Pivotal.Utilities;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop_UI.Models;
using Workshop_UI.ViewModels.Workshop;

namespace Workshop_UI.Controllers
{
    public class WorkshopController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        ILogger<WorkshopController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        IOptionsSnapshot<FortuneServiceOptions> _fortunesConfig;
        IDiscoveryClient discoveryClient;
        IDistributedCache RedisCacheStore { get; set; }
        IConfiguration Config { get; set; }
        IConfigurationRoot ConfigRoot { get; set; }

        SortedList<int, int> appInstCount = new SortedList<int, int>();
        SortedList<int, int> srvInstCount = new SortedList<int, int>();
        List<string> fortunes = new List<string>();

        private FortuneServiceCommand _fortunes;
        public WorkshopController(
            ILogger<WorkshopController> logger,
            IOptionsSnapshot<FortuneServiceOptions> config,
            IOptionsSnapshot<ConfigServerData> configServerData,
            FortuneServiceCommand fortunes,
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions,
            IConfiguration configApp,
            IConfigurationRoot configRoot,
            IDistributedCache cache,
            [FromServices] IDiscoveryClient client
            )
        {
            if (configServerData != null)
                IConfigServerData = configServerData;

            _logger = logger;
            _fortunes = fortunes;
            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
            _fortunesConfig = config;
            discoveryClient = client;
            RedisCacheStore = cache;
            Config = configApp;
            ConfigRoot = configRoot;
        }
        
        public IActionResult Index()
        {
            _logger?.LogDebug("Index");
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        public IActionResult Steeltoe()
        {
            _logger?.LogDebug("Steeltoe");
            return View();
        }

        public IActionResult ResetServiceStats()
        {
            srvInstCount = new SortedList<int, int>();

            string output2 = JsonConvert.SerializeObject(srvInstCount);
            RedisCacheStore?.SetString("SrvInstance", output2);

            return RedirectToAction(nameof(WorkshopController.Services), "Workshop");
        }

        public IActionResult ResetApplicationStats()
        {
            appInstCount = new SortedList<int, int>();

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return RedirectToAction(nameof(WorkshopController.Platform), "Workshop");
        }

        public IActionResult ResetBlueGreenStats()
        {
            return RedirectToAction(nameof(WorkshopController.BlueGreen), "Workshop"); 
        }

        public IActionResult Refresh()
        {
            return RedirectToAction(nameof(WorkshopController.Services), "Workshop"); ;
        }

        // Lab10 Start
        //[Authorize(Policy = "read.fortunes")] 
        // Lab10 End
        public async Task<IActionResult> Services()
        {
            _logger?.LogDebug("RandomFortune");

            ViewData["FortuneUrl"] = _fortunesConfig.Value.RandomFortuneURL;

            // Lab05 Start
            var fortune = await _fortunes.RandomFortuneAsync();
            // Lab05 End

            var _fortuneHistory = RedisCacheStore?.GetString("FortuneHistory");
            if (!string.IsNullOrEmpty(_fortuneHistory))
                fortunes = JsonConvert.DeserializeObject<List<string>>(_fortuneHistory);

            fortunes.Insert(0, fortune.Text);

            if (fortunes.Count > 10)
            {
                fortunes.RemoveAt(10);
            }

            string fortuneoutput = JsonConvert.SerializeObject(fortunes);
            RedisCacheStore?.SetString("FortuneHistory", fortuneoutput);
            
            HttpContext.Session.SetString("MyFortune", fortune.Text);
            
            var _appInstCount = RedisCacheStore?.GetString("AppInstance");
            if (!string.IsNullOrEmpty(_appInstCount))
            {
                _logger?.LogInformation($"App Session Data: {_appInstCount}");
                appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
            }

            var _srvInstCount = RedisCacheStore?.GetString("SrvInstance");
            if (!string.IsNullOrEmpty(_srvInstCount))
            {
                _logger?.LogInformation($"Servlet Session Data: {_srvInstCount}");
                srvInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_srvInstCount);
            }

            var _count2 = srvInstCount.GetValueOrDefault(fortune.InstanceIndex, 0);
            srvInstCount[fortune.InstanceIndex] = ++_count2;
            
            string output2 = JsonConvert.SerializeObject(srvInstCount);
            RedisCacheStore?.SetString("SrvInstance", output2);

            ViewData["MyFortune"] = fortune.Text;
            ViewData["FortuneIndex"] = $"{fortune.InstanceIndex}";
            ViewData["FortuneDiscoveryUrl"] = discoveryClient.GetInstances("fortuneService")?[fortune.InstanceIndex]?.Uri?.ToString();
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        public IActionResult Platform()
        {
            _logger?.LogDebug("Platform");

            SortedList<int, int> appInstCount = new SortedList<int, int>();
            SortedList<int, int> srvInstCount = new SortedList<int, int>();
            List<string> fortunes = new List<string>();

            var _appInstCount = RedisCacheStore?.GetString("AppInstance");
            if (!string.IsNullOrEmpty(_appInstCount))
            {
                _logger?.LogInformation($"App Session Data: {_appInstCount}");
                appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
            }

            var _count = appInstCount.GetValueOrDefault(CloudFoundryApplication.Instance_Index, 0);
            appInstCount[CloudFoundryApplication.Instance_Index] = ++_count;

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        public IActionResult BlueGreen()
        {
            _logger?.LogDebug("BlueGreen");

            SortedList<int, int> appInstCount = new SortedList<int, int>();
            SortedList<int, int> srvInstCount = new SortedList<int, int>();
            List<string> fortunes = new List<string>();

            ViewData["AppColor"] = WebStyleUtilities.GetColorFromString(CloudFoundryApplication.ApplicationName, "blue");

            var _appInstCount = RedisCacheStore?.GetString("AppInstance");
            if (!string.IsNullOrEmpty(_appInstCount))
            {
                _logger?.LogInformation($"App Session Data: {_appInstCount}");
                appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
            }

            var _count = appInstCount.GetValueOrDefault(CloudFoundryApplication.Instance_Index, 0);
            appInstCount[CloudFoundryApplication.Instance_Index] = ++_count;

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            await HttpContext.Session.CommitAsync();
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        public IActionResult Configuration()
        {
            _logger?.LogDebug("Index");

            var _index = Environment.GetEnvironmentVariable("INSTANCE_INDEX");
            if (_index == null)
            {
                _index = "Running Local";
            }

            var _prodmode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (_prodmode == null)
            {
                _prodmode = "Production";
            }

            var _port = Environment.GetEnvironmentVariable("PORT");
            if (_port == null)
            {
                _port = "localhost";
            }

            ViewData["Index"] = $"Application Instance: {_index}";
            ViewData["ProdMode"] = $"ASPNETCORE Environment: {_prodmode}";
            ViewData["Port"] = $"Port: {_port}";
            ViewData["Uptime"] = $"Uptime: {DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount))}";

            ViewData["appId"] = CloudFoundryApplication.ApplicationId;
            ViewData["appName"] = CloudFoundryApplication.ApplicationName;
            ViewData["uri0"] = CloudFoundryApplication.ApplicationUris[0];
            ViewData["disk"] = Config["vcap:application:limits:disk"];
            ViewData["sourceString"] = "appsettings.json";

            IConfigurationSection configurationSection = Config.GetSection("ConnectionStrings");
            if (configurationSection != null)
            {
                if (configurationSection.GetValue<string>("AttendeeContext") != null)
                {
                    ViewData["sourceString"] = "Config Server";
                }
            }

            var _connectJson = Config.GetConnectionString("AttendeeContext");
            if (!string.IsNullOrEmpty(_connectJson))
                ViewData["jsonDBString"] = StringCleaner.GetDisplayString("Password=", ";", _connectJson ,"*****");
            var cfe = new CFEnvironmentVariables();
            var _connect = cfe.getConnectionStringForDbService("user-provided", "AttendeeContext");
            if (!string.IsNullOrEmpty(_connect))
                ViewData["boundDBString"] = StringCleaner.GetDisplayString("Password=", ";", _connect, "*****");

            //if (Services.Value != null)
            //{
            //    foreach (var service in Services.Value.ServicesList)
            //    {
            //        ViewData[service.Name] = service.Name;
            //        ViewData[service.Plan] = service.Plan;
            //    }
            //}


            if (Config.GetSection("spring") != null)
            {
                ViewData["AccessTokenUri"] = Config["spring:cloud:config:access_token_uri"];
                ViewData["ClientId"] = Config["spring:cloud:config:client_id"];
                ViewData["ClientSecret"] = Config["spring:cloud:config:client_secret"];
                ViewData["Enabled"] = Config["spring:cloud:config:enabled"];
                ViewData["Environment"] = Config["spring:cloud:config:env"];
                ViewData["FailFast"] = Config["spring:cloud:config:failFast"];
                ViewData["Label"] = Config["spring:cloud:config:label"];
                ViewData["Name"] = Config["spring:cloud:config:name"];
                ViewData["Password"] = Config["spring:cloud:config:password"];
                ViewData["Uri"] = Config["spring:cloud:config:uri"];
                ViewData["Username"] = Config["spring:cloud:config:username"];
                ViewData["ValidateCertificates"] = Config["spring:cloud:config:validate_certificates"];
            }
            else
            {
                ViewData["AccessTokenUri"] = "Not Available";
                ViewData["ClientId"] = "Not Available";
                ViewData["ClientSecret"] = "Not Available";
                ViewData["Enabled"] = "Not Available";
                ViewData["Environment"] = "Not Available";
                ViewData["FailFast"] = "Not Available";
                ViewData["Label"] = "Not Available";
                ViewData["Name"] = "Not Available";
                ViewData["Password"] = "Not Available";
                ViewData["Uri"] = "Not Available";
                ViewData["Username"] = "Not Available";
                ViewData["ValidateCertificates"] = "Not Available";
            }
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }
        [HttpGet]
        // Lab10 Start
        [Authorize]
        // Lab10 Start
        public IActionResult Login()
        {
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        [Authorize]
        public IActionResult Kill()
        {
            Environment.Exit(-99);
            return RedirectToAction(nameof(WorkshopController.Platform), "Workshop"); 
        }

        public IActionResult ReloadConfig()
        {
            ConfigRoot.Reload();
            return RedirectToAction(nameof(WorkshopController.Configuration), "Workshop");
        }

        public IActionResult Manage()
        {
            ViewData["Message"] = "Manage accounts using UAA or CF command line.";
            return View();
        }

        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Insufficient permissions.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

    }
}
