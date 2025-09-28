using Grpc.Core;
using RabbitMQ.Client;
using Sum_gRPC.Protos;
using System.Text;
using System.Text.Json;

namespace Sum_gRPC.Services;
public class AdditionService : Addition.AdditionBase
{
    private readonly ILogger<AdditionService> _logger;

    public AdditionService(ILogger<AdditionService> logger)
    {
        _logger = logger;
    }

    public async override Task<AddResponse> Add(AddRequest request , ServerCallContext context)
    {
        int sum = request.A + request.B;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("accumlator", durable:true, exclusive:false, autoDelete: false);

        var message = JsonSerializer.Serialize(new {value = sum});
        var body = Encoding.UTF8.GetBytes(message);

        // publish and don't wait
        channel.BasicPublishAsync("", "accumlator" , body);

        _logger.LogInformation($"{request.A} + {request.B} = {sum}");

        return new AddResponse { Result = sum };
    }
}
