using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitProducer.Service
{
   public class RabbitMqProducer
    {


        private readonly IConfiguration _config;

        public RabbitMqProducer(IConfiguration config)
        {
            _config = config;
        }

        public void PublishMessage(object message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMQ:Host"],
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _config["RabbitMQ:QueueName"], durable: true, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "", routingKey: _config["RabbitMQ:QueueName"], basicProperties: properties, body: body);

            Console.WriteLine($"[x] Sent: {json}");
        }
    }
}
