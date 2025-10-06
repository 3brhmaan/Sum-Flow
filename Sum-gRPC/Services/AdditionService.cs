using Grpc.Core;
using RabbitMQ.Client;
using Sum_gRPC.Data;
using Sum_gRPC.Models;
using Sum_gRPC.Protos;
using System.Text;
using System.Text.Json;

namespace Sum_gRPC.Services;
public class AdditionService : Addition.AdditionBase
{
    private readonly ILogger<AdditionService> _logger;
    private readonly AppDbContext appDbContext;

    public AdditionService(ILogger<AdditionService> logger , AppDbContext appDbContext)
    {
        _logger = logger;
        this.appDbContext = appDbContext;
    }

    public async override Task<AddResponse> Add(AddRequest request , ServerCallContext context)
    {
        var message = new OutboxMessage
        {
            Content = (request.A + request.B).ToString()
        };

        appDbContext.OutboxMessages.Add(message);
        await appDbContext.SaveChangesAsync();

        int sum = request.A + request.B;
        return new AddResponse { Result = sum };
    }
}
