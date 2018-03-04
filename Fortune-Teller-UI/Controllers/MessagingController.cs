using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace FortuneTeller.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class MessagingController : Controller
    {
        ConnectionFactory ConnectionFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingController"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        public MessagingController([FromServices] ConnectionFactory connectionFactory)
        {
            // Set up RabbitMQ Connection
            ConnectionFactory = connectionFactory;

            SslOption opt = ConnectionFactory.Ssl;
            if (opt != null && opt.Enabled)
            {
                opt.Version = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

                // Only needed if want to disable certificate validations
                opt.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors |
                    SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }
        }

        // GET: Messaging
        public ActionResult Index()
        {
            return View();
        }

        // GET: Messaging/Receive
        /// <summary>
        /// Receives a message.
        /// </summary>
        /// <returns></returns>
        public ActionResult Receive()
        {

            using (var connection = ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                CreateQueue(channel);

                var data = channel.BasicGet("rabbit-test", true);
                if (data != null)
                {
                    ViewData["message"] = Encoding.UTF8.GetString(data.Body);
                }
            }

            return View();
        }

        /// <summary>
        /// Creates the queue.
        /// </summary>
        /// <param name="channel">The channel.</param>
        protected void CreateQueue(IModel channel)
        {
            channel.QueueDeclare(queue: "rabbit-test",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        }


        // POST: Messaging/Send
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(string message)
        {
            const string EXCHANGE_NAME = "EXCHANGE3";

            try
            {
                // TODO: Add insert logic here
                if (message != null && message != "")
                {
                    using (var connection = ConnectionFactory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, false, true, null);

                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: EXCHANGE_NAME,
                                             routingKey: "rabbit-test",
                                             basicProperties: null,
                                             body: body);
                    }
                }

                //return View();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // POST: Messaging/Send
        /// <summary>
        /// Sends messages based on the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Blast(int? count)
        {
            const string EXCHANGE_NAME = "EXCHANGE3";

            try
            {
                if (count != null && count > 0)
                {
                    for (var idx = 1; idx <= count; idx++)
                    {
                        using (var connection = ConnectionFactory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, false, true, null);

                            var message = $"Message number {idx}";
                            var body = Encoding.UTF8.GetBytes(message);
                            channel.BasicPublish(exchange: EXCHANGE_NAME,
                                                 routingKey: "rabbit-test",
                                                 basicProperties: null,
                                                 body: body);
                        }
                    }
                }

                //return View();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}