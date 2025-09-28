
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost" ,
                    UserName = "guest" ,
                    Password = "guest" ,
                    VirtualHost = "/"
                };

                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: "accumlator" ,
                    durable: true ,
                    exclusive: false ,
                    autoDelete: false ,
                    cancellationToken: stoppingToken
                );
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model , eventArgs) =>
                {
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var request = JsonSerializer.Deserialize<AccumlatorRequest>(message);

                        var CurrentAccumlatorString = await accumlatorService.ReadAccumlatorValueAsync();

                        int.TryParse(CurrentAccumlatorString , out var currentAccumlatorValue);

                        await accumlatorService.UpdateAccumlatorValueAsync(currentAccumlatorValue + request.value);

                        logger.LogInformation($"Current Sum: {currentAccumlatorValue + request.value}");
                        Console.WriteLine("============================================================");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex , "Error Processing Message");
                    }
                };

                await channel.BasicConsumeAsync("accumlator" , true , consumer);

                logger.LogInformation("Consumer started. Waiting for messages...");

                await Task.Delay(Timeout.Infinite , stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex , "Consumer Error, Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(1) , stoppingToken);
            }
        }
    }
}
