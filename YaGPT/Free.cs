using YandexGPTWrapper;

namespace YaGPT;

public static class YaGPTFree
{
    public static async Task<string> Chat(string message, int timeOutSeconds = 10)
    {
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeOutSeconds));
        using YaGPTFreeWrapper yagpt = new("ru-RU", cts.Token);
        return await yagpt.SendMessageAsync(message);
    }
}