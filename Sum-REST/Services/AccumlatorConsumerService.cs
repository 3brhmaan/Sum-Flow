
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sum_REST.Metrics;
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
            var channels = new List<IChannel>();
            var consumers = new List<AsyncEventingBasicConsumer>();

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "rabbitmq" ,
                    UserName = "guest" ,
                    Password = "guest" ,
                    VirtualHost = "/"
                };

                await using var connection = await factory.CreateConnectionAsync(stoppingToken);

                for (int i = 0 ; i < 50 ; i++)
                {
                    var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    channels.Add(channel);

                    await channel.QueueDeclareAsync(
                        queue: "accumlator" ,
                        durable: true ,
                        exclusive: false ,
                        autoDelete: false ,
                        cancellationToken: stoppingToken
                    );

                    // fair dispatch
                    await channel.BasicQosAsync(prefetchSize: 0 , prefetchCount: 1 , global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumers.Add(consumer);

                    var consumerId = i;

                    consumer.ReceivedAsync += async (model , eventArgs) =>
                    {
                        try
                        {
                            var body = eventArgs.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var request = JsonSerializer.Deserialize<AccumlatorRequest>(message);

                            var messageWaitingTimeInQueueSeconds = (DateTime.UtcNow - request.ProcessedOn).TotalSeconds;
                            ApplicationMetrics.QueueWaitingTime.Record(messageWaitingTimeInQueueSeconds);

                            await accumlatorService.AddAsync(request.Value);

                            var requestProcessedOn = DateTime.UtcNow;
                            var requestWholeTimeSeconds = (requestProcessedOn - request.OccurredOn).TotalSeconds;

                            logger.LogInformation(
                                $"Consume Id: {consumerId}\nTime Details:\n\tRequest Whole Time: {requestWholeTimeSeconds}\n\tMessage Waiting Time In Queue: {messageWaitingTimeInQueueSeconds}"
                            );

                            ApplicationMetrics.RequestWholeTime.Record(requestWholeTimeSeconds);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex , "Error Processing Message");
                        }
                    };

                    await channel.BasicConsumeAsync(
                        queue: "accumlator" ,
                        autoAck: true ,
                        consumer: consumer ,
                        cancellationToken: stoppingToken
                    );
                }

                logger.LogInformation("Consumer started. Waiting for messages...");

                await Task.Delay(Timeout.Infinite , stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex , "Consumer Error, Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(2) , stoppingToken);
            }
            finally
            {
                foreach (var channel in channels)
                {
                    try
                    {
                        await channel.CloseAsync();
                        await channel.DisposeAsync();
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
