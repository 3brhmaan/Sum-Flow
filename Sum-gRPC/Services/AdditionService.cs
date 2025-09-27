using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sum_gRPC.Protos;

namespace Sum_gRPC.Services;
public class AdditionService : Addition.AdditionBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdditionService> _logger;
    private readonly string baseUrl = "https://localhost:7290/api/accumulator";

    public AdditionService(HttpClient httpClient , ILogger<AdditionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async override Task<AddResponse> Add(AddRequest request , ServerCallContext context)
    {
        int sum = request.A + request.B;
        await _httpClient.PutAsJsonAsync(baseUrl , new { addition = sum });

        _logger.LogInformation($"{request.A} + {request.B} = {sum}");

        return new AddResponse { Result = sum };
    }
}
