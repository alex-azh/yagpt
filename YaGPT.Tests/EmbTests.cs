using System.Diagnostics;
using static YaGPT.YaGPT;

namespace YaGPT.Tests;

[TestClass]
public class EmbTests
{
    static YaGPT gpt = new YaGPT(
        Config.Get("YA_FOLDER_ID"),
        Config.Get("YA_API_KEY"));
    
    [TestMethod]
    public async Task Test1()
    {
        var result = await gpt.Embeddings<float>(false, "hello", 20);
        Assert.AreEqual(256, result.embedding.Count);
    }
}