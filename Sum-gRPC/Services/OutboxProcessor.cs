using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Sum_gRPC.Data;
using Sum_gRPC.Models;
using System.Text;
using System.Text.Json;

namespace Sum_gRPC.Services;

public class OutboxProcessor : BackgroundService
{
    private readonly ILogger<OutboxProcessor> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private IConnection rabbitConnection;

    public OutboxProcessor(ILogger<OutboxProcessor> logger , IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq" ,
            UserName = "guest" ,
            Password = "guest" ,
            VirtualHost = "/"
        };

        rabbitConnection = await factory.CreateConnectionAsync(stoppingToken);

        logger.LogInformation("OutboxProcessor Started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await appDbContext.OutboxMessages
                    .Where(x => x.ProcessedOn == null)
                    .OrderBy(x => x.OccurredOn)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (!messages.Any())
                {
                    logger.LogWarning("No messages in the database, retry again after 1 second...");
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    continue;
                }

                var tasks = messages.Select(msg => PublishWithRetryAsync(msg , stoppingToken));
                await Task.WhenAll(tasks);
                await appDbContext.SaveChangesAsync(stoppingToken);

            }
            catch (Exception ex)
            {
                logger.LogError(ex , "Unexpected error in OutboxProcessor");
                await Task.Delay(TimeSpan.FromSeconds(1) , stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (rabbitConnection?.IsOpen == true)
            await rabbitConnection.CloseAsync();

        await rabbitConnection!.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }

    private async Task PublishWithRetryAsync(OutboxMessage message , CancellationToken ct )
    {
        int maxRetries = 3;

        for (int attempt = 0 ; attempt < maxRetries ; attempt++)
        {
            try
            {
                await using var channel = await rabbitConnection.CreateChannelAsync(cancellationToken: ct);

                await channel.QueueDeclareAsync(
                    queue: "accumlator" ,
                    durable: true ,
                    exclusive: false ,
                    autoDelete: false ,
                    cancellationToken: ct
                );

                message.ProcessedOn = DateTime.UtcNow;

                var messageText = JsonSerializer.Serialize(new
                {
                    Value = int.Parse(message.Content) ,
                    message.OccurredOn ,
                    message.ProcessedOn
                });

                var body = Encoding.UTF8.GetBytes(messageText);

                await channel.BasicPublishAsync(
                    exchange: "" ,
                    routingKey: "accumlator" ,
                    body: body ,
                    cancellationToken: ct
                );

                logger.LogInformation("Published outbox message {Id} and marked as processed" , message.Id);

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex , "RabbitMQ unreachable for message {Id} (attempt {Attempt}/{Max})" ,
                    message.Id , attempt + 1 , maxRetries);

                await Task.Delay(TimeSpan.FromSeconds(1) , ct);
            }
        }

        logger.LogError(
            "Failed to publish outbox message {Id} after {Max} attempts. Will retry later." ,
            message.Id , maxRetries
        );
    }
}
