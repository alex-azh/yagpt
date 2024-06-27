using System.Numerics;

namespace YaGPT;

public record EmbResult<T>(List<T> embedding, string numTokens, string modelVersion)
    where T : IFloatingPoint<T>;