#!/usr/bin/env bash

cf delete-service myConfigServer
cf delete-service myDiscoveryService 
cf delete-service myMySqlService
cf delete-service myRedisService
cf delete-service myHystrixService
cf delete-service myRabbitMQService
cf delete-service myOAuthService
cf delete=service AttendeeContext