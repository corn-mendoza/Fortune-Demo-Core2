using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pivotal.Helper
{
    public class CFEnvironmentVariables
    {
        public dynamic vcap_services_data { get; set; }
        public dynamic vcap_application_data { get; set; }

        public CFEnvironmentVariables()
        {
            string raw_vcap_app = Environment.GetEnvironmentVariable("VCAP_APPLICATION");
            string raw_vcap_services = Environment.GetEnvironmentVariable("VCAP_SERVICES");

            // If there's a vcap services entry, parse to a dynamic JObject;
            if (raw_vcap_services != null)
                vcap_services_data = JObject.Parse(raw_vcap_services);

            // If there's a vcap application entry, parse to a dynamic JObject;
            if (raw_vcap_app != null)
                vcap_application_data = JObject.Parse(raw_vcap_app);
        }

        public dynamic getInfoForUserProvidedService(string serviceName)
        {
            // Try to access the user-provided service.
            // Unfortunately, we can't do the dot notation here, since user-provided would be
            // an invalid property name.
            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                var upsArray = vcap_services_data["user-provided"];

                if (upsArray != null && upsArray.HasValues)
                {
                    foreach (var ups in upsArray)
                        if (ups.name == serviceName)
                            return ups;
                }
            }

            return null;
        }

        public dynamic getInfoForService(string serviceTypeName, string serviceInstanceName = "")
        {

            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceArray = vcap_services_data[serviceTypeName];

                if (serviceArray != null && serviceArray.HasValues)
                {
                    // If serviceInstanceName is empty, just return the first element of our services info array
                    if (serviceInstanceName == "")
                    {
                        return serviceArray[0];
                    }

                    foreach (var service in serviceArray)
                        if (service.name == serviceInstanceName)
                            return service;
                }
            }

            return null;
        }

        public dynamic getInfoForApplication(string appTypeName)
        {

            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic appArray = vcap_application_data[appTypeName];

                if (appArray != null && appArray.HasValues)
                {
                    return appArray;
                }
            }

            return null;
        }

        public string getConnectionStringForAzureDbService(string serviceTypeName, string serviceInstanceName = "", IDbConnectionStringFormatter formatter = null)
        {
            string connectionString = null;

            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceInfo = getInfoForService(serviceTypeName, serviceInstanceName);

                if (Object.ReferenceEquals(null, serviceInfo) == false)
                {
                    var cstring = Convert.ToString(serviceInfo.credentials.connectionstring);
                    if (string.IsNullOrEmpty(cstring))
                    {
                        // Default to use a Basic MS SQL Server connection string, if our formatter was not specified.
                        if (formatter == null)
                            formatter = new BasicSQLServerConnectionStringFormatter();

                        var host = Convert.ToString(serviceInfo.credentials.sqlServerFullyQualifiedDomainName);
                        var username = Convert.ToString(serviceInfo.credentials.databaseLogin);
                        var password = Convert.ToString(serviceInfo.credentials.databaseLoginPassword);
                        //var port = Convert.ToString(serviceInfo.credentials.port);
                        var databaseName = Convert.ToString(serviceInfo.credentials.sqldbName);

                        connectionString = formatter.Format(host, username, password, databaseName, "1433");
                    }
                    else
                    {
                        connectionString = cstring;
                    }
                }
            }
            return connectionString;
        }

        public string getConnectionStringForDbService(string serviceTypeName, string serviceInstanceName = "", IDbConnectionStringFormatter formatter = null)
        {
            string connectionString = "";

            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceInfo = getInfoForService(serviceTypeName, serviceInstanceName);

                if (Object.ReferenceEquals(null, serviceInfo) == false)
                {
                    // If there's a connection string defined, return that directly and do not build a connection string
                    var cstring = Convert.ToString(serviceInfo.credentials.connectionstring);
                    if (string.IsNullOrEmpty(cstring))
                    {
                        // Default to use a Basic MS SQL Server connection string, if our formatter was not specified.
                        if (formatter == null)
                            formatter = new BasicSQLServerConnectionStringFormatter();

                        var host = Convert.ToString(serviceInfo.credentials.hostname);
                        var username = Convert.ToString(serviceInfo.credentials.username);
                        var password = Convert.ToString(serviceInfo.credentials.password);
                        var port = Convert.ToString(serviceInfo.credentials.port);
                        var databaseName = Convert.ToString(serviceInfo.credentials.name);

                        connectionString = formatter.Format(host, username, password, databaseName, port);
                    }
                    else
                    {
                        connectionString = cstring;
                    }
                }
            }
            return connectionString;
        }

        public string getConnectionStringForMessagingService(string serviceTypeName, string serviceInstanceName = "", IMessageBusConnectionStringFormatter formatter = null)
        {
            string connectionString = null;

            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceInfo = getInfoForService(serviceTypeName, serviceInstanceName);

                if (Object.ReferenceEquals(null, serviceInfo) == false)
                {
                    // Default to use a Basic Azure Message Bus connection string, if our formatter was not specified.
                    if (formatter == null)
                        formatter = new BasicAzureMessageBusConnectionStringFormatter();

                    var endpoint = Convert.ToString(serviceInfo.credentials.namespace_name);
                    var keyname = Convert.ToString(serviceInfo.credentials.shared_access_key_name);
                    var token = Convert.ToString(serviceInfo.credentials.shared_access_key_value);

                    connectionString = formatter.Format(endpoint, keyname, token);
                }
            }
            return connectionString;
        }

        public AzureSearchCredentials getAzureSearchCredentials(string serviceTypeName, string serviceInstanceName = "")
        {
            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceInfo = getInfoForService(serviceTypeName, serviceInstanceName);

                if (Object.ReferenceEquals(null, serviceInfo) == false)
                {
                    //var protocol = Convert.ToString(serviceInfo.credentials.protocol);
                    var token = Convert.ToString(serviceInfo.credentials.SearchServiceApiKey);
                    var name = Convert.ToString(serviceInfo.credentials.SearchServiceName);
                    var index = Convert.ToString(serviceInfo.credentials.IndexName);

                    var _ret = new AzureSearchCredentials()
                    {
                        ServiceName = serviceInstanceName,
                        ServiceType = serviceTypeName,
                        AccountName = name,
                        Token = token,
                        Index = index
                    };
                    return _ret;
                }
            }
            return null;
        }


        public AzureStorageCredentials getAzureStorageCredentials(string serviceTypeName, string serviceInstanceName = "")
        {
            if (Object.ReferenceEquals(null, vcap_services_data) == false)
            {
                dynamic serviceInfo = getInfoForService(serviceTypeName, serviceInstanceName);

                if (Object.ReferenceEquals(null, serviceInfo) == false)
                {
                    var acctname = Convert.ToString(serviceInfo.credentials.storage_account_name);
                    var token = Convert.ToString(serviceInfo.credentials.primary_access_key);

                    var _ret = new AzureStorageCredentials()
                    {
                        ServiceName = serviceInstanceName,
                        ServiceType = serviceTypeName,
                        AccountName = acctname,
                        Token = token
                    };
                    return _ret;
                }
            }
            return null;
        }
    }

    public class AzureSearchCredentials : AzureCredentials
    {
        public string Index;
    }

    public class AzureStorageCredentials : AzureCredentials
    {
        public string Protocol = "https";

        public string ConnectionString
        {
            get
            {
                var formatter = new BasicAzureStorageConnectionStringFormatter();

                return formatter.Format(Protocol, AccountName, Token);
            }
        }

        public string Endpoint
        {
            get
            {
                if (!string.IsNullOrEmpty(AccountName))
                {
                    return string.Format("https://{0}.blob.core.windows.net", AccountName);
                }

                return null;
            }
        }
    }

    public class AzureCredentials
    {
        public string ServiceName;
        public string ServiceType;

        public string AccountName;
        public string Token;
    }

    // Connection String Formatting helper classes
    public interface IDbConnectionStringFormatter
    {
        string Format(string host, string username, string password, string databaseName, string port = null);
    }

    public interface IMessageBusConnectionStringFormatter
    {
        string Format(string endpoint, string keyname, string token);
    }

    public interface IStorageConnectionStringFormatter
    {
        string Format(string protocol, string acctname, string token);
    }

    public class BasicMySQLConnectionStringFormatter : IDbConnectionStringFormatter
    {
        public string Format(string host, string username, string password, string databaseName, string port = null)
        {
            string connectionString = $"Server={host};Database={databaseName};Uid={username};Pwd={password};";

            if (port != null)
                connectionString += $"Port={port};";


            return connectionString;
        }
    }

    public class BasicSQLServerConnectionStringFormatter : IDbConnectionStringFormatter
    {
        public string Format(string host, string username, string password, string databaseName, string port = null)
        {
            return $"Server={host};Database={databaseName};User ID={username};Password={password};";
        }
    }

    public class BasicAzureMessageBusConnectionStringFormatter : IMessageBusConnectionStringFormatter
    {
        public string Format(string endpoint, string keyname, string token)
        {
            return $"Endpoint=sb://{endpoint}.servicebus.windows.net;SharedAccessKeyName={keyname};SharedAccessKey={token};";
        }
    }

    public class BasicAzureStorageConnectionStringFormatter : IStorageConnectionStringFormatter
    {
        public string Format(string protocol, string acctname, string token)
        {
            return $"DefaultEndpointsProtocol={protocol};AccountName={acctname};AccountKey={token}";
        }
    }

}