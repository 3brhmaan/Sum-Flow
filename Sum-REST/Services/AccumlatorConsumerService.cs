
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sum_REST.Models;
using System.Text;
using System.Text.Json;

namespace Sum_REST.Services;

public class AccumlatorConsumerService : BackgroundService
{
    private readonly ILogger<AccumlatorConsumerService> logger;
    private readonly AccumlatorService accumlatorService;

    public AccumlatorConsumerService(ILogger<AccumlatorConsumerService> logger , AccumlatorService accumlatorService)
    {
        this.logger = logger;
        this.accumlatorService = accumlatorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost" ,
            UserName = "guest" ,
            Password = "guest" ,
            VirtualHost = "/"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("accumlator" , durable: true , exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model , eventArgs) =>
        {
            logger.LogInformation($"Current Thread: {Thread.CurrentThread.ManagedThreadId}");

            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var request = JsonSerializer.Deserialize<AccumlatorRequest>(message);

            var CurrentAccumlatorString = await accumlatorService.ReadAccumlatorValueAsync();

            int.TryParse(CurrentAccumlatorString , out var currentAccumlatorValue);

            await accumlatorService.UpdateAccumlatorValueAsync(currentAccumlatorValue + request.value);

            logger.LogInformation($"Current Sum: {currentAccumlatorValue + request.value}");
            Console.WriteLine("============================================================");
        };

        await channel.BasicConsumeAsync("accumlator" , true , consumer);
    }
}
