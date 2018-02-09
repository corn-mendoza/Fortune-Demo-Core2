
using Fortune_Teller_Service.Models;
using FortuneTeller.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fortune_Teller_Service.Controllers
{
    [Route("api/[controller]")]
    public class FortunesController : Controller
    {
        ILogger<FortunesController> _logger;
        CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        // Lab05 Start
        private IFortuneRepository _fortunes;
        public FortunesController(ILogger<FortunesController> logger, IFortuneRepository fortunes,
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions)
        {
            _logger = logger;
            _fortunes = fortunes;

            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
        }
        // Lab05 End


        // GET: api/fortunes/all
        [HttpGet("all")]
        // Lab10 Start
        [Authorize(Policy = "read.fortunes")]
        // Lab10 End
        public async Task<List<Fortune>> AllFortunesAsync()
        {
            _logger?.LogDebug("AllFortunesAsync");

            var idx = CloudFoundryApplication.InstanceIndex;

            // Lab05 Start
            var entities = await _fortunes.GetAllAsync();
            var result = new List<Fortune>();
            foreach(var entity in entities)
            {
                result.Add(new Fortune() { Id = entity.Id, InstanceIndex = idx, Text = entity.Text });
            }
            return result;
            // Lab05 End
        }

        // GET api/fortunes/random
        [HttpGet("random")]
        // Lab10 Start
        //[Authorize(Policy = "read.fortunes")]
        // Lab10 End
        public async Task<Fortune> RandomFortuneAsync()
        {
            _logger?.LogDebug("RandomFortuneAsync");

            var idx = CloudFoundryApplication.InstanceIndex;

            // Lab05 Start
            var entity = await _fortunes.RandomFortuneAsync();
            return new Fortune() { Id = entity.Id, InstanceIndex = idx, Text = entity.Text };
            // Lab05 End
        }
    }
}
