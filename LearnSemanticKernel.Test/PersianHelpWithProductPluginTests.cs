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

namespace LearnSemanticKernel.Test;

public class PersianHelpWithProductPluginTests
{
    private Kernel MyKernel { get; set; }
    private KernelFunction HelpWithProduct { get; set; }

    public PersianHelpWithProductPluginTests()
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
        builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "PersianSupportAgentPlugin"), "PersianSupportAgentPlugin");
        builder.Plugins.AddFromType<SupportAgentPlanner>();
        MyKernel = builder.Build();
        
        HelpWithProduct = MyKernel.Plugins.GetFunction("PersianSupportAgentPlugin", "HelpWithProduct");
    }

    [Fact]
    public async Task HelpWithProduct_SimpleFlow_MustWork()
    {
        ChatHistory chatHistory = new ChatHistory();

        chatHistory.AddUserMessage("سلام");
        chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");

        var history = chatHistory.ToHistory();

        var input = "آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟";

        var result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("آیا درست متوجه شدم", result?.ToLower());


        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("سوال شما اینه که «آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟»، آیا درست متوجه شدم؟");
        history = chatHistory.ToHistory();

        input = "بله";

        result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("ملک‌رادار", result?.ToLower());
    }

    [Fact]
    public async Task HelpWithProduct_CorrectingQuestion_MustWork()
    {
        ChatHistory chatHistory = new ChatHistory();

        chatHistory.AddUserMessage("سلام");
        chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");
        chatHistory.AddUserMessage("آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟");
        chatHistory.AddAssistantMessage("سوال شما اینه که «آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟»، آیا درست متوجه شدم؟");
        var history = chatHistory.ToHistory();

        var input = "نه من می‌خوام بفهمم سوال من در مورد نسل پنج هست؟";

        var result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("نسل پنج", result?.ToLower());

        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("سوال شما اینه که آیا گوشی لومیای ۱۰۵۰ پشتیبانی از اینترنت نسل پنج (5G) داره؟ من متوجه شدم.");
        history = chatHistory.ToHistory();

        input = "بله درسته";

        result = await HelpWithProduct.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("ملک‌رادار", result?.ToLower());
    }
}