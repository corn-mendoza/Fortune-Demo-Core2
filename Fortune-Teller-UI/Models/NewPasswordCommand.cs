using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Security.DataProtection.CredHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FortuneTeller.Models
{
    public class NewPasswordCommand : HystrixCommand<string>
    {
        private static readonly Random rand = new Random();
        private ILoggerFactory logFactory;
        private PasswordGenerationParameters _options;

        public NewPasswordCommand(PasswordGenerationParameters options, ILoggerFactory loggerFactory) : base(HystrixCommandGroupKeyDefault.AsKey("NewPasswordGroup"))
        {
            _options = options;
            logFactory = loggerFactory;
        }

        protected override async Task<string> RunAsync()
        {
            var credHubClient = await CredHubClient.CreateMTLSClientAsync(new CredHubOptions(), logFactory.CreateLogger("CredHub"));
            var credRequest = new PasswordGenerationRequest("credbulb", _options, overwriteMode: OverwiteMode.overwrite);
            var newPassword = (await credHubClient.GenerateAsync<PasswordCredential>(credRequest)).Value;
            Console.WriteLine("success path");
            return newPassword.ToString();
        }

        protected override Task<string> RunFallbackAsync()
        {
            Console.WriteLine("fallback path");
            var newPassword = Guid.NewGuid().ToString();
            if (_options.Length != null && newPassword.Length > _options.Length)
            {
                newPassword = newPassword.Substring(0, (int)_options.Length);
            }

            return Task.FromResult(newPassword);
        }
    }
}
