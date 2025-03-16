using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQConsumer.Service
{
    public class RabbitMQConsumer
    {

        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public RabbitMQConsumer(IConfiguration config, EmailService emailService)
        {
            _config = config;
            _emailService = emailService;
        }

        public async Task StartListeningAsync()
        {
            Console.WriteLine("🚀 Consumer started. Waiting for messages...");

            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"] ?? "localhost",
                UserName = _config["RabbitMQ:Username"] ?? "guest",
                Password = _config["RabbitMQ:Password"] ?? "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _config["RabbitMQ:QueueName"] ?? "email_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"📩 Received message: {message}");

                try
                {
                    var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
                    Console.WriteLine($"📨 Sending email to: {emailMessage.To}");

                    await _emailService.SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body);

                    Console.WriteLine($"✅ Email Sent to: {emailMessage.To}");
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error sending email: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: _config["RabbitMQ:QueueName"], autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite); // Keep the service running
        }
    }

    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
