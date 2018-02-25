@ECHO OFF

cf delete-service myConfigServer
cf delete-service myDiscoveryService 
cf delete-service myMyFortuneDB
cf delete-service myRedisService
cf delete-service myHystrixService
cf delete-service myRabbitMQService
cf delete-service myOAuthService
cf delete-service AttendeeContext
