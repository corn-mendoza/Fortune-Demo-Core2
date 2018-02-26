# Fortune Teller Demo and Source Code

## Overview
This demo and accompanying source code is designed to help facilitate a deeper understanding of Pivotal Application Services, Spring Cloud Services, and Steeltoe.  The source code is based on two existing projects:

A live demonstration can be found [here](https://fortuneui.apps.islands.cloud/).  This project is still in development and will be updated regularly.

### Technology
This application is intended to be deployed using Pivotal Application Services on Cloud Foundry. The source code for this project is implemented using dependency injection in .NET Core v2 and the following technologies:

1. Steeltoe v2 Open Source Libraries
2. Syncfusion UI Controls
3. Configuration Server
4. Eureka Discovery Server
5. Hystrix Circuit Breakers
6. RabbitMQ Messaging
7. Redis Caching
8. Pivotal UAC/SSO
9. MySQL Connector
10. Microsoft SQL Server
11. User Provided Services

### Requirements
The following are required for building and deploying the applications in this workshop.

- [Cloud Foundry CLI](https://github.com/cloudfoundry/cli)
- [Git Client](https://git-scm.com/downloads)
- [.NET Core SDK 2.0](https://www.microsoft.com/net/download)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2017](https://www.visualstudio.com/downloads/)
- [Java 8 JDK](http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html) - Optional, needed to run Eureka and Config servers locally

## Design
While the Steeltoe workshop application helps demonstrate the functionality of Pivotal Application Services and Spring Cloud Services using Steeltoe, there was a need to expose some of the underlying workings of these applications running on PAS. This workshop and demo will help 
users better understand how PAS can help developers deploy at scale. Some additional techniques are used in the development of these applications such as utilizing service client libraries and shared model libraries. 

The following are some of the patterns used in development:

- Micro services using service client shared libraries
- Configuration Services to handle application configuration
- User Provided Services and Configurtion Services to handle service connections
- User and Service Security using OAuth and JWT methods
- Application support for Blue Green Deployments
- Continuous integration and deployment using Visual Studio Team Services
- Messaging using RabbitMQ and Steeltoe connector

## Features
The workshop application can be navigated through the home page links through the various topics. The workshop can also be used as a demo for the capabilities of PAS. The following areas are designed to support the navigation of many of the features of PAS, SCS, and Steeltoe.

### Platform
The Platform section provides a demonstration of the platform's capabilities and enhancements provided by Steeltoe. Links to logging, tasks, metrics, and health accutators are included on the page to assist in navigation to key functionality.

### Configuration
The Configuration section provides a demonstration of configuration best practices and using steeltoe configuration services. Links to environment variables, the config server dashboard, the config server repository, and build information are included on the page to assist in navigation to key functionality.

### Services
The Services section provides a demonstration of service discovery and circuit breaker patterns. Redis is used to store the service counters. Service security is demonstrated through the SSO feature of the portal. If the user is not logged in, the service will respond with "You will have a happy day!". 
Links to the Hystrix and Eureka dashboards are included on the page to assist in navigation to key functionality.

### Connections
The Connections section provides a demonstration of how to leverage user provided services and/or the config server to manage connection string data for services. This can be demonstrated by binding and unbinding the user provided service and by changing the environment for the config server.

### Zero Downtime
The Zero Downtime section provides a demonstration of a Blue/Green deployment. The code leverages Redis cache and config server to coordinate the cutover between applications by consitently showing the usage counters increment. The page will use colors to highlight the two applications participating on the mapped route.

### Security
The application provides the ability to log in and log out of the application using Single Sign-On. If a user is already logged into the Apps Manager, the user will be automatically signed into the Workshop application.

### CI/CD Pipeline using VSTS
The entire project is setup to demonstrate continuous delivery via CI/CD pipelines setup in Visual Studio Team Services. When a change is checked in by a developer, the pipeline will build the application, deploy the artifacts back into the github repository, and push the applications on to PAS.

## Projects
The following are the projects found in this repository and a short description of the functionality that each is designed to demonstrate.

### Fortune Teller UI
Main demo application web application

### Fortune Teller Service
Provides fortunes as a service used to demonstrate the circuit breaker and service discovery design patterns. 

### RabbitConsoleService
Example of a console application processing RabbitMQ message queues. 

### Fortune Service Client
Provides a reusable client for accessing the fortune teller service.

### Pivotal Utilities
Common set of functions used by several applications.

## Installation Instructions
### Deploying through the VSTS Pipeline
This demo source code repository has been integrated into a CI/CD pipeline using Visual Studio Team Services. The build and release jobs developed leverage the Cloud Foundry plug-in to deploy the application on to Pivotal Cloud Foundry. 
The portal for the VSTS CI/CD pipeline can be found [here](https://pivotal-workshops.visualstudio.com/Fortune%20Teller/). Access is required to the portal to view the job definitions.

### Installation Packages without Visual Studio
Installation packages that are ready to push are available in the Release section of this repository.  You can find them [here](https://pivotal-workshops.visualstudio.com/Fortune%20Teller/).

### Installation using .Net CLI
Projects can be built using .NET Core 2.0 SDK from a console window. Use the publish commmand to create a deployment package or push the source code directly onto cloud foundry.

	`dotnet publish {nameofproject}.csproj -o {outputpath}`

### Cloning and Building the Solution
This project is developed using Visual Studio 2017. To build this solution, clone this repo and open the solution file. The projects can then be published and pushed from the publish folder. 

### Configuration
Additional configuration instructions can be found in the content directory of this project.


