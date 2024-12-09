
using Helper;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string exchangeName = "userAlertExchange";
string queueName = "SmsToUser";
var connectionFactory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};
var connection = await connectionFactory.CreateConnectionAsync();
var channel = await connection.CreateChannelAsync();
await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: "*.register");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender, args) => {
    var body = args.Body.ToArray();
    var bodyString = Encoding.UTF8.GetString(body);
    var user = JsonConvert.DeserializeObject<User>(bodyString);
    if (user != null)
    {
        Console.WriteLine($"Sms sent to: {user.PhoneNumber}");
        await channel.BasicAckAsync(args.DeliveryTag, false);
    }
};

await channel.BasicConsumeAsync(queueName, false, consumer);
Console.ReadLine();