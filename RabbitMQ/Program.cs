using RabbitMQ.Client;
using RabbitMQ.RabbitMQConnectionManager;

IModel channel = RabbitMQConnectionManager.GetChannel("Rabbit Sender App");

for (int i = 0; i < 60; i++)
{
    RabbitMQConnectionManager.SendMessage(channel, RabbitMQConnectionManager.exchangeName, RabbitMQConnectionManager.routingKey, i);
    Thread.Sleep(1000);
}

channel.Close();
RabbitMQConnectionManager.CloseConnection();
