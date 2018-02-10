# Installing the Fortune Teller Application

## Requirements

### Platform
The fortune application is built and tested using PCF 1.12 or PCF 2.0. Other versions can work but will need to be tested prior to delivery. The fortune application has been tested on the following cloud platforms:

- Azure
- vSphere
- Google Cloud

### Services
The following services are required to be installed on the foundation for all of the features of the fortune application application to function correctly.
- Spring Cloud Services (v 1.4)
- RabbitMQ (v 1.10)
- Redis (v 1.11)
- MySQL (v 1.10)
- Single Sign-On (v 1.4)

To setup the services, use the batch command file in the [scripts folder](https://github.com/corn-pivotal/Fortune-Demo-Core2/tree/master/scripts). You can also execute the following commands in a console window. See the connection string information section for more information on configuring the database.

##### Services Entries
- myConfigServer

`> cf create-service p-config-server standard myConfigServer -c config-server.json`

- myDiscoveryService

`> cf create-service p-service-registry standard myDiscoveryService`

- myMySqlService

`> cf create-service p-mysql 100mb myMySqlService`

- myRedisService

`> cf create-service p-redis shared-vm myRedisService`

- myHystrixService

`> cf create-service p-circuit-breaker-dashboard standard myHystrixService`

- myRabbitMQService

`> cf create-service p-rabbitmq standard myRabbitMQService`

- myOAuthService

`> cf cups myOAuthService -p "{\"client_id\": \"myWorkshop\",\"client_secret\": \"mySecret\",\"uri\": \"uaa://login.system.testcloud.com\"}"`

- AttendeeContext

`> cf cups AttendeeContext -p "{\"connectionstring": \"{AttendeeContextConnectionString}\"}"`


### Workshop UI Configuration

#### Environment Variables
Environment variables are used to configure the Workshop UI. The initial set of variables can be set in the manifest file of the project prior to pushing the Workshop UI application.

- ASPNETCORE_ENVIRONMENT: Environment to load using Config Server

#### Config Server
The Workshop application uses data from the config server for the URLs used in the running application.  This allows the ability to demonstrate different URLs for different environments. 

To complete the configuration, update the location of the Config Server repository using the cf CLI. 

`> cf update-service myConfigServer -c {pathto/config.json}`

or

`> cf update-service myConfigServer -c '{\"git\":{\"uri\":\"https://github.com/corn-pivotal/configserver-
repo\"}}'`

Add a configuration file for the environment that is being setup. For example: if the ASPNETCORE_ENVIRONMENT is set to Azure, a WorkshopUI-Azure.yml file should be placed in the config server repository. The configuration file should be modified to provide the following URLs:

- AppsManagerUrl: URI for Apps Manager Portal
- AppBaseUrl: Base URI for Apps Manager Applications - this can be found in the apps manager portal and usually ends with applications/
- EurekaDashboardUrl: Eureka Dashboard URI
- HystrixDashboardUrl: Hystrix Dashboard URI
- OpsManagerUrl: Ops Manager Portal URI
- ConfigServerUrl: Config Server Dashboard URI
- ConfigRepoUrl: Config Server Repo Location URI
- PCFMetricsUrl: PCF Metrics URI
- GithubRepoUrl: Workshop Source Repo URI

#### Connection Strings
The workshop application demonstrates the ability to load connection string information from both a user provided service and from the config server. In order for the application to function correctly for this demonstration, a SQL Server database 
needs to be setup to access. The following fields are required for the AttendeeContext database:

##### Database Schema
        int Id
        string Name
        string Email
        string Title
        string Department

##### ConnectionString
Configure the AttendeeContext connection string in the workshopui.yml, appsettings.json, or update the user provided service.



