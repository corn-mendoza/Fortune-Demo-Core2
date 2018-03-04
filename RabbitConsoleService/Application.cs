using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;

namespace RabbitConsoleService
{
    public class Application
    {
        ILogger<Application> _logger;
        IConfiguration Config { get; set; }
        ConnectionFactory ConnectionFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configApp">The configuration application.</param>
        /// <param name="connectionFactory">The connection factory.</param>
        public Application(ILogger<Application> logger, IConfiguration configApp, [FromServices] ConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
            SslOption opt = ConnectionFactory.Ssl;
            if (opt != null && opt.Enabled)
            {
                opt.Version = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                // Only needed if want to disable certificate validations
                opt.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors |
                    SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }

            Config = configApp;
            _logger = logger;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            try
            {                
                _logger?.LogInformation($"Starting RabbitService");
                _logger?.LogInformation($"Logging Startup");

                //queueClient = new QueueClient(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock);

                _logger?.LogInformation($"Processing started");

                ReceiveMessages();

                _logger?.LogInformation($"Processing Stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.ToString());
            }
        }

        // Receives messages from the queue in a loop
        /// <summary>
        /// Receives the messages.
        /// </summary>
        private void ReceiveMessages()
        {
            try
            {
                _logger?.LogInformation("Starting Listener");

                const string EXCHANGE_NAME = "EXCHANGE3";

                using (IConnection connection = ConnectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, false, true, null);

                        var queueName = "rabbit-queue";
                        var queueDeclareOk = channel.QueueDeclare(queueName, true, false, true, null);

                        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (o, e) =>
                        {
                            string data = Encoding.ASCII.GetString(e.Body);
                            _logger?.LogInformation($"Received message: {data}");
                            channel.BasicAck(e.DeliveryTag, true);
                        };

                        string consumerTag = channel.BasicConsume(consumer, queueName, false, "Queue", true, false);

                        channel.QueueBind(queueName, EXCHANGE_NAME, "rabbit-test");

                        _logger?.LogInformation("Listening...");

                        Console.ReadLine();
                        _logger?.LogInformation("Listerner Stopped");

                        channel.QueueUnbind(queueName, EXCHANGE_NAME, "rabbit-test", null);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger?.LogInformation($"{DateTime.Now} > Exception: {exception.Message}");
            }
        }
    }
}
