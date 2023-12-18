using Azure.AI.OpenAI;
using LearnSemanticKernel.NativePlugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging.Console;

namespace LearnSemanticKernel.Test.PluginTests;

public class LightPluginTests
{
    private Kernel MyKernel { get; set; }

    public LightPluginTests()
    {
        var apiKey =
            Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
            throw new Exception("No ApiKey in environment variables.");
        var endpoint =
            Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
            throw new Exception("No Endpoint in environment variables.");

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", endpoint, apiKey);
        builder.Services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });

        builder.Plugins.AddFromType<LightPlugin>();
        MyKernel = builder.Build();
    }

    [Fact]
    public async Task LightPlugin_DirectTurnOn_MustWork()
    {
        var plugin = MyKernel.Plugins[nameof(LightPlugin)];

        var oldState = await plugin["GetState"].InvokeAsync<string>(MyKernel);

        Assert.Equal("off", oldState);

        var newState = await plugin["ChangeState"].InvokeAsync<string>(MyKernel, new KernelArguments(new Dictionary<string, object?>()
        {
            ["newState"] = true
        }));

        Assert.Equal("on", newState);
    }

    [Fact]
    public async Task LightPlugin_TurnOn_MustWork()
    {
        var chatService = MyKernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Can you turn on the lights");

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = await chatService.GetChatMessageContentsAsync(chatHistory, settings, MyKernel);

        Assert.NotEmpty(result);
    }
}