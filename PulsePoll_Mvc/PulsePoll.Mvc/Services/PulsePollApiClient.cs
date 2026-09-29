using System.Net.Http.Json;
using System.Text.Json;
using PulsePoll.Mvc.Models;

namespace PulsePoll.Mvc.Services;

public class PulsePollApiClient : IPulsePollApiClient
{
    private readonly HttpClient _httpClient;

    public PulsePollApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PulsePollApiResult<TemplateResponse>> CreateTemplateAsync(CreateTemplateRequest request)
        => await PostAsync<TemplateResponse>("api/templates", request);

    public async Task<PulsePollApiResult<PollResponse>> CreatePollAsync(CreatePollRequest request)
        => await PostAsync<PollResponse>("api/polls", request);

    private async Task<PulsePollApiResult<T>> PostAsync<T>(string path, object body)
    {
        var response = await _httpClient.PostAsJsonAsync(path, body);

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response);
            return PulsePollApiResult<T>.Fail((int)response.StatusCode, error);
        }

        var data = await response.Content.ReadFromJsonAsync<T>();
        return PulsePollApiResult<T>.Ok((int)response.StatusCode, data!);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("error", out var errorProp))
            {
                return errorProp.GetString() ?? $"Request failed ({(int)response.StatusCode})";
            }
        }
        catch (JsonException)
        {
        }

        return $"Request failed ({(int)response.StatusCode})";
    }
}
