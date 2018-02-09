using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workshop_UI.Models
{
    public class ConfigServerData
    {
        public string AppsManagerUrl { get; set; }
        public string AppBaseUrl { get; set; }
        public string EurekaDashboardUrl { get; set; }
        public string HystrixDashboardUrl { get; set; }
        public string OpsManagerUrl { get; set; }
        public string ConfigServerUrl { get; set; }
        public string ConfigRepoUrl { get; set; }
        public string PCFMetricsUrl { get; set; }
        public string ExchangeUrl { get; set; }
        public string GithubRepoUrl { get; set; }
        public string ManifestUrl { get; set; }
    }
}
