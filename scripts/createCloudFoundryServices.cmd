@ECHO OFF

cf create-service p-config-server standard myConfigServer -c config-server.json
cf create-service p-service-registry standard myDiscoveryService 
cf create-service p-mysql 100mb myMySqlService
cf create-service p-redis shared-vm myRedisService
cf create-service p-circuit-breaker-dashboard standard myHystrixService
cf create-service p-rabbitmq standard myRabbitMQService
cf cups myOAuthService -p "{\"client_id\": \"myWorkshop\",\"client_secret\": \"mySecret\",\"uri\": \"uaa://login.system.testcloud.com\"}"
cf cups AttendeeContext -p "{\"connectionstring": \"myconnectionstring\"}"
