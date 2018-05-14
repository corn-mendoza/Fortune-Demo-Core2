using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FortuneTeller.Models;
using FortuneTeller.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pivotal.Helper;
using Steeltoe.Security.DataProtection.CredHub;

namespace FortuneTeller.Views
{
    public class SecurityController : Controller
    {
        private static HttpClient _httpClient;
        private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        //private IHubContext<ObservationHub> _hubContext;
        private Utils _utils;
        private ILoggerFactory _logFactory;

        public SecurityController(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _httpClient = new HttpClient();
            //_hubContext = hubContext;
            _logFactory = loggerFactory;
            _utils = new Utils(config.GetValue<string>("cognitiveServicesKey"), _httpClient);
        }

        public IActionResult Index()
        {
            var results = new List<string>();
            for (double s = 0; s < 1; s += .02d)
            {
                results.Add(_utils.HexColorFromDouble(s));
            }
            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> CredHubColorize([FromBody]PasswordGenerationParameters options)
        {
            if (options == null) { options = new PasswordGenerationParameters(); }

            // call credhub to generate a password
            NewPasswordCommand command = new NewPasswordCommand(options, _logFactory);
            var newPassword = await command.ExecuteAsync();

            // this library returns password strength on a scale of 0 to 4
            var analysis = Zxcvbn.Zxcvbn.MatchPassword(newPassword);
            var passwordStrength = (double)analysis.Score / 4;
            Console.WriteLine($"Password stats -- calcTime: {analysis.CalcTime} crack time: {analysis.CrackTime} ctDisplay: {analysis.CrackTimeDisplay} entropy: {analysis.Entropy} score: {analysis.Score} strength: {passwordStrength}");
            var color = _utils.HexColorFromDouble(passwordStrength);
            var response = new ColorChangeResponse { HexColor = color, TextInput = newPassword + "|~|~|" + analysis.CrackTimeDisplay, Sentiment = passwordStrength };
            await SetColorNotifyObservers(response, false);
            return Json(response);
        }

        private async Task SetColorNotifyObservers(ColorChangeResponse response, bool? notify = true, double? duration = 1)
        {
            //await _lifxClient.SetState(new All(), new SentState { Color = $"#{response.HexColor}", Duration = (double)duration });
            if (notify == true)
            {
                //await _hubContext.Clients.All.SendAsync("Messages", new List<ColorChangeResponse> { response });
            }
        }
    }
}