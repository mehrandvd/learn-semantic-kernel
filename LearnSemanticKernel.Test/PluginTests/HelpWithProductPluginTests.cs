using Azure.AI.OpenAI;
using LearnSemanticKernel.NativePlugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging.Console;
using Json.Schema.Generation.Intents;
using LearnSemanticKernel.Extensions;

namespace LearnSemanticKernel.Test.PluginTests;

public class HelpWithProductPluginTests
{
    private Kernel MyKernel { get; set; }
    private KernelFunction HelpWithProduct { get; set; }

    public HelpWithProductPluginTests()
    {
        var testDir = Environment.CurrentDirectory;

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
            loggerBuilder.SetMinimumLevel(LogLevel.Information);
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });

        builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"), "OrchestrationPlugin");
        builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "SupportAgentPlugin"), "SupportAgentPlugin");
        builder.Plugins.AddFromType<SupportAgentPlanner>();
        MyKernel = builder.Build();

        HelpWithProduct = MyKernel.Plugins.GetFunction("SupportAgentPlugin", "HelpWithProduct");
    }

    [Fact]
    public async Task HelpWithProduct_SimpleFlow_MustWork()
    {
        ChatHistory chatHistory = new ChatHistory();

        chatHistory.AddUserMessage("Hi");
        chatHistory.AddAssistantMessage("Hi, How can I help you?");

        var history = chatHistory.ToHistory();

        var input = "Does my phone (Lumia 1050XL) support 4G?";

        var result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("lumia 1050xl", result?.ToLower());


        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 4G?");
        history = chatHistory.ToHistory();

        input = "Yes";

        result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("tech", result?.ToLower());
    }

    [Fact]
    public async Task HelpWithProduct_CorrectingQuestion_MustWork()
    {
        ChatHistory chatHistory = new ChatHistory();

        chatHistory.AddUserMessage("Hi");
        chatHistory.AddAssistantMessage("Hi, How can I help you?");
        chatHistory.AddUserMessage("Does my phone (Lumia 1050XL) support 4G?");
        chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 4G?");
        var history = chatHistory.ToHistory();

        var input = "No, I want to see if it supports 5G.";

        var result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("5g", result?.ToLower());

        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 5G?");
        history = chatHistory.ToHistory();

        input = "Yes, it's correct";

        result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("tech", result?.ToLower());
    }
}