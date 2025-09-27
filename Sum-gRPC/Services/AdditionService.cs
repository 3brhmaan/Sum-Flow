using Grpc.Core;
using Sum_gRPC.Protos;

namespace Sum_gRPC.Services;
public class AdditionService: Addition.AdditionBase
{
    private readonly HttpClient _httpClient;
    private readonly string baseUrl = "https://localhost:7290/api/sum";

    public AdditionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async override Task<AddResponse> Add(AddRequest request , ServerCallContext context)
    {
        int sum = request.A + request.B;
        var response = await _httpClient.PutAsJsonAsync(baseUrl, new {addition = sum});

        var responseContent = await response.Content.ReadAsStringAsync();
        var addResponse = AddResponse.Parser.ParseJson(responseContent);

        return addResponse;
    }
}
