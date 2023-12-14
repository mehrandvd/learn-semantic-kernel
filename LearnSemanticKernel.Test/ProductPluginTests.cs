using LearnSemanticKernel.NativePlugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test;

public class ProductPluginTests
{
    private Kernel MyKernel { get; set; }

    public ProductPluginTests()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("mlk-openai-test-api-key", EnvironmentVariableTarget.User) ??
            throw new Exception("No ApiKey in environment variables.");
        var endpoint =
            Environment.GetEnvironmentVariable("mlk-openai-test-endpoint", EnvironmentVariableTarget.User) ??
            throw new Exception("No Endpoint in environment variables.");

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", endpoint, apiKey);
        builder.Plugins.AddFromType<ProductPlugin>();
        MyKernel = builder.Build();
    }

    [Fact]
    public async Task ProductPlugin_Today_MustWork()
    {
        var chat = MyKernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory(new List<ChatMessageContent>()
        {
            new(AuthorRole.User, "What is current time?")
        });

        var result = await chat.GetChatMessageContentsAsync(chatHistory);

        Assert.NotEmpty(result);
    }
}