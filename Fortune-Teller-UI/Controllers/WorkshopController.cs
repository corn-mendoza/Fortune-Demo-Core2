
using FortuneService.Client;
using FortuneTeller.Models;
using FortuneTeller.ViewModels.Workshop;
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
using RabbitMQ.Client;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FortuneTeller.Controllers
{
    /// <summary>
    /// Workshop Controller Class
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class WorkshopController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ILogger<WorkshopController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        private IOptionsSnapshot<FortuneServiceOptions> _fortunesConfig;
        private IDiscoveryClient discoveryClient;
        private IDistributedCache RedisCacheStore { get; set; }
        private IConfiguration Config { get; set; }
        private IConfigurationRoot ConfigRoot { get; set; }
        private ConnectionFactory ConnectionFactory { get; set; }

        private SortedList<int, int> appInstCount = new SortedList<int, int>();
        private SortedList<int, int> srvInstCount = new SortedList<int, int>();
        private List<string> fortunes = new List<string>();
        private Dictionary<string, string> connects = new Dictionary<string,string>();

        private FortuneServiceCommand _fortunes;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="configServerData">The configuration server data.</param>
        /// <param name="fortunes">The fortunes.</param>
        /// <param name="appOptions">The application options.</param>
        /// <param name="servOptions">The serv options.</param>
        /// <param name="configApp">The configuration application.</param>
        /// <param name="configRoot">The configuration root.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="client">The client.</param>
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
            [FromServices] ConnectionFactory connectionFactory,
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

            // Set up RabbitMQ Connection
            ConnectionFactory = connectionFactory;

            SslOption opt = ConnectionFactory.Ssl;
            if (opt != null && opt.Enabled)
            {
                opt.Version = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                // Only needed if want to disable certificate validations
                opt.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors |
                    SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }
        }

        /// <summary>
        /// Index Page.
        /// </summary>
        /// <returns></returns>
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
                fortunes,
                connects));
        }

        /// <summary>
        /// Workshop Page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Workshop()
        {
            _logger?.LogDebug("Workshop");
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes,
                connects));
        }

        /// <summary>
        /// Steeltoe Page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Resources()
        {
            _logger?.LogDebug("Resource Page");
            return View();
        }

        /// <summary>
        /// Resets the service stats.
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetServiceStats()
        {
            srvInstCount = new SortedList<int, int>();

            string output2 = JsonConvert.SerializeObject(srvInstCount);
            RedisCacheStore?.SetString("SrvInstance", output2);

            return RedirectToAction(nameof(WorkshopController.Services), "Workshop");
        }

        /// <summary>
        /// Resets the application stats.
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetApplicationStats()
        {
            appInstCount = new SortedList<int, int>();

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return RedirectToAction(nameof(WorkshopController.Platform), "Workshop");
        }

        /// <summary>
        /// Resets the blue green stats.
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetBlueGreenStats()
        {
            return RedirectToAction(nameof(WorkshopController.BlueGreen), "Workshop");
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        /// <returns></returns>
        public IActionResult Refresh()
        {
            return RedirectToAction(nameof(WorkshopController.Services), "Workshop"); ;
        }

        // Enable this for security
        //[Authorize(Policy = "read.fortunes")] 
        /// <summary>
        /// Services Page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Services()
        {
            _logger?.LogDebug("RandomFortune");

            ViewData["FortuneUrl"] = _fortunesConfig.Value.RandomFortuneURL;

            var fortune = await _fortunes.RandomFortuneAsync();

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
                fortunes,
                connects));
        }

        /// <summary>
        /// Platform Page
        /// </summary>
        /// <returns></returns>
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
                fortunes,
                connects));
        }

        /// <summary>
        /// Blue Green Page.
        /// </summary>
        /// <returns></returns>
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
                fortunes,
                connects));
        }


        /// <summary>
        /// Configuration Page.
        /// </summary>
        /// <returns></returns>
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
            ViewData["sourceString"] = "appsettings.json/Config Server";

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

            var cstrings = Config.GetSection("ConnectionStrings");
            foreach(var s in cstrings.GetChildren())
            {
                string connect = s.Value;
                if (s.Value.Contains("Password"))
                {
                    connect = StringCleaner.GetDisplayString("Password=", ";", connect, "*****");
                }
                if (s.Value.Contains("User ID"))
                {
                    connect = StringCleaner.GetDisplayString("User ID=", ";", connect, "*****");
                }
                connects.Add(s.Key, connect);
            }

            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes,
                connects));
        }

        /// <summary>
        /// Creates a load.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateLoad(int? count)
        {
            _logger?.LogInformation($"Starting load generation");
            for (var i = 0; i < count.GetValueOrDefault(1); i++)
            {                
                var _f = await _fortunes._fortuneService.RandomFortuneAsync();                
            }

            _logger?.LogInformation($"End load generation");

            return RedirectToAction(nameof(WorkshopController.Services), "Workshop");
        }

        /// <summary>
        /// Login Page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        /// <summary>
        /// Kills this instance.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Kill()
        {
            Environment.Exit(-99);
            return RedirectToAction(nameof(WorkshopController.Platform), "Workshop"); 
        }

        /// <summary>
        /// Reloads the configuration.
        /// </summary>
        /// <returns></returns>
        public IActionResult ReloadConfig()
        {
            ConfigRoot.Reload();
            return RedirectToAction(nameof(WorkshopController.Configuration), "Workshop");
        }

        /// <summary>
        /// Manages this instance.
        /// </summary>
        /// <returns></returns>
        public IActionResult Manage()
        {
            ViewData["Message"] = "Manage accounts using UAA or CF command line.";
            return View();
        }

        /// <summary>
        /// Logoff.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            await HttpContext.Session.CommitAsync();
            return RedirectToAction(nameof(WorkshopController.Index), "Workshop");
        }

        /// <summary>
        /// Accesses denied.
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Insufficient permissions.";
            return View();
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View();
        }

    }
}
