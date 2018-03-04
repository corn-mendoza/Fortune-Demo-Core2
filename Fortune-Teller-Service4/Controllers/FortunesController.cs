
using FortuneTeller.Models;
using FortuneTellerService4.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace FortuneTellerService4.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    public class FortunesController : ApiController
    {
        private IFortuneRepository _fortunes;
        private ILogger<FortunesController> _logger;
        CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FortunesController" /> class.
        /// </summary>
        /// <param name="fortunes">The fortunes.</param>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="appOptions">The application options.</param>
        /// <param name="servOption">The serv option.</param>
        public FortunesController(IFortuneRepository fortunes, ILoggerFactory logFactory = null,
            IOptions < CloudFoundryApplicationOptions > appOptions = null,
            IOptions<CloudFoundryServicesOptions> servOption = null)
        {
            _fortunes = fortunes;
            _logger = logFactory?.CreateLogger<FortunesController>();

            //if (appOptions != null)
            //    CloudFoundryApplication = appOptions.Value;
            //if (servOption != null)
            //    CloudFoundryServices = servOption.Value;
        }

        // 
        /// <summary>
        /// GET: api/health
        /// </summary>
        /// <returns>Status</returns>
        [HttpGet]
        public IHttpActionResult Health()
        {
            return Ok();
        }

        // 
        /// <summary>
        /// Return all fortunes
        /// </summary>
        /// <returns>IEnumerable</returns>
        [HttpGet, Route("all")]
        public IEnumerable<Fortune> All()
        {
            _logger?.LogInformation("api/fortunes");
            return _fortunes.GetAll();
        }

        // 
        /// <summary>
        /// Return a random fortune
        /// </summary>
        /// <returns>Fortune</returns>
        [HttpGet, Route("random")]
        public Fortune Random()
        {
            _logger?.LogInformation("api/fortunes/random");
            return _fortunes.RandomFortune();
        }
    }
}
