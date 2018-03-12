﻿using FortuneTeller.Models;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Collections.Generic;

namespace FortuneTeller.ViewModels.Workshop
{
    public class CloudFoundryViewModel
    {
        public CloudFoundryViewModel(CloudFoundryApplicationOptions appOptions, CloudFoundryServicesOptions servOptions, ConfigServerData configData, IDiscoveryClient client, SortedList<int, int> appCounts, SortedList<int, int> srvCounts, List<string> fortunes, Dictionary<string,string> connectionStrings)
        {
            CloudFoundryServices = servOptions;
            CloudFoundryApplication = appOptions;
            ConfigData = configData;
            discoveryClient = client;
            ServiceInstanceCounts = srvCounts;
            FortuneHistory = fortunes;
            AppInstanceCounts = appCounts;
            ConnectionStrings = connectionStrings;
        }

        public IDiscoveryClient discoveryClient { get; }
        public CloudFoundryServicesOptions CloudFoundryServices { get; }
        public ConfigServerData ConfigData { get; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; }
        public SortedList<int, int> ServiceInstanceCounts { get; }
        public SortedList<int, int> AppInstanceCounts { get; }
        public List<string> FortuneHistory { get; }
        public Dictionary<string, string> ConnectionStrings { get; }

        public List<KeyValuePair<int, int>> GetServiceInstanceCounts()
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach(var key in ServiceInstanceCounts.Keys)
            {
                KeyValuePair<int, int> kp = new KeyValuePair<int, int>(key, ServiceInstanceCounts[key]);
                ret.Add(kp);
            }

            return ret;
        }

        public List<KeyValuePair<int, int>> GetAppInstanceCounts()
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach (var key in AppInstanceCounts.Keys)
            {
                KeyValuePair<int, int> kp = new KeyValuePair<int, int>(key, AppInstanceCounts[key]);
                ret.Add(kp);
            }

            return ret;
        }

    }
}
