using Microsoft.Extensions.Configuration;

namespace YaGPT.Tests;

public static class Config
{
    static IConfigurationRoot config = new ConfigurationBuilder()
        .AddUserSecrets<EmbTests>()
        .Build();

    public static string? Get(string key) => config[key];
}