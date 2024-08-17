using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace RabbitMQ.RabbitMQConnectionManager
{
    public static class RabbitMQConnectionManager
    {
        private static IConnection _connection;
        public static string exchangeName = "DemoExchange";
        public static string routingKey = "demo-routing-key";
        public static string queueName = "DemoQueue";
        public static IModel GetChannel(string clientProvidedName)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = InitializeConnection(clientProvidedName);
            }

            var channel = _connection.CreateModel();


            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            channel.BasicQos(0, 1, false);

            return channel;
        }

        private static IConnection InitializeConnection(string clientProvidedName)
        {

            ConnectionFactory factory = new()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672"),
                ClientProvidedName = clientProvidedName
            };

            return factory.CreateConnection();
        }

        public static void CloseConnection()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        public static void SendMessage(IModel channel, string exchangeName, string routingKey, int messageNumber)
        {
            Console.WriteLine($"Sending Message {messageNumber}");
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes($"Message #{messageNumber}");
            channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
        }
        public static void ReceiveMessages(string clientProvidedName)
        {
            using var channel = GetChannel(clientProvidedName);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (sender, args) =>
            {
                try
                {

                    var body = args.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"Message received: {message}");

                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    // Optionally, reject the message if processing fails.
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            };

            string consumerTag = channel.BasicConsume(queueName, false, consumer);
            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();

            // Cleanup
            channel.BasicCancel(consumerTag);
        }
    }
}
