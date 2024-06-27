using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;

namespace YaGPT;

public record YaGPT(string FOLDER_ID, string API_KEY)
{
    public enum GPTModel : byte
    {
        Lite = 0,
        LiteRC = 1,
        Pro = 2
    }
    private const string chatUrl = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";
    private const string embUrl = "https://llm.api.cloud.yandex.net/foundationModels/v1/textEmbedding";
    private string EmbModelUrlShort => $"emb://{FOLDER_ID}/text-search-query/latest";
    private string EmbModelUrlLong => $"emb://{FOLDER_ID}/text-search-doc/latest";

    private readonly Dictionary<string, string> headers = new()
    {
    {"Authorization", $"Api-Key {API_KEY}" },
    {"x-folder-id", $"{FOLDER_ID}" },
    {"x-data-logging-enabled", "false" },
    };

    public async Task<ChatResult> Chat(List<Message> messages, float temperature, int maxTokens, GPTModel model, int timeOutSeconds = 10)
    {
        var bodyJSON = new
        {
            modelUri = model switch
            {
                GPTModel.Lite => $"gpt://{FOLDER_ID}/yandexgpt-lite/latest",
                GPTModel.LiteRC => $"gpt://{FOLDER_ID}/yandexgpt-lite/rc",
                GPTModel.Pro => $"gpt://{FOLDER_ID}/yandexgpt/latest",
                _ => ""
            },
            completionOptions = new
            {
                stream = false,
                temperature = temperature,
                maxTokens = maxTokens
            },
            messages = messages
        };

        // httpClient
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        foreach (KeyValuePair<string, string> header in headers)
            client.DefaultRequestHeaders.Add(header.Key, header.Value);

        client.Timeout = TimeSpan.FromSeconds(timeOutSeconds);

        // http request
        HttpResponseMessage resp = await client.PostAsJsonAsync(chatUrl, bodyJSON);
        string result = await resp.Content.ReadAsStringAsync();

        if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception(result);

        var des = JsonSerializer.Deserialize<ChatResult>(result);
        if (des == null || des.result == null || des.result.alternatives == null || des.result.alternatives.Length == 0)
            throw new Exception($"Code={resp.StatusCode}, Content={resp.Content}\nResponse={result}");

        return des;
    }

    public async Task<EmbResult<T>> Embeddings<T>(bool doc, string text, int timeOutSeconds = 10)
        where T : IFloatingPoint<T>
    {
        var body = new { modelUri = doc ? EmbModelUrlLong : EmbModelUrlShort, text = text };

        // httpClient
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        foreach (KeyValuePair<string, string> header in headers)
            client.DefaultRequestHeaders.Add(header.Key, header.Value);

        client.Timeout = TimeSpan.FromSeconds(timeOutSeconds);

        // http request
        HttpResponseMessage resp = await client.PostAsJsonAsync(embUrl, body);
        var response = await resp.Content.ReadAsStringAsync();

        if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception(response);

        var des = JsonSerializer.Deserialize<EmbResult<T>>(response);

        if (des == null || des.embedding == null || des.embedding.Count == 0)
            throw new Exception($"Code={resp.StatusCode}, Content={resp.Content}\nResponse={response}");
        return des;
    }
}