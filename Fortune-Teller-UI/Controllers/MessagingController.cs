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

namespace Fortune_Teller_UI.Controllers
{
    public class MessagingController : Controller
    {
        ConnectionFactory ConnectionFactory { get; set; }

        public MessagingController([FromServices] ConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;

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

        protected void CreateQueue(IModel channel)
        {
            channel.QueueDeclare(queue: "rabbit-test",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        }


        // POST: Messaging/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(string message)
        {
            try
            {
                // TODO: Add insert logic here
                if (message != null && message != "")
                {
                    using (var connection = ConnectionFactory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        CreateQueue(channel);

                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "",
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
    }
}