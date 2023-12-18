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

public class MelkRadarAgentTests
{
    private Kernel MyKernel { get; set; }
    private KernelFunction AnswerChat { get; set; }

    public MelkRadarAgentTests()
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
        builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "MelkRadarAgentPlugin"));
        builder.Plugins.AddFromType<MelkRadarAgentPlanner>();
        MyKernel = builder.Build();

        AnswerChat = MyKernel.Plugins.GetFunction("MelkRadarAgentPlanner", "AnswerChat");
    }

    [Fact]
    public async Task HelpWithProduct_SimpleFlow_MustWork()
    {
        ChatHistory chatHistory = new ChatHistory();

        chatHistory.AddUserMessage("سلام");
        chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");

        var history = chatHistory.ToHistory();

        var input = "رادار آگهی چیه؟.";

        var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("رادار آگهی", result?.ToLower());


        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("رادار آگهی (یا فایلینگ ملک‌رادار)، یک سرویس ارائه شده توسط ملک‌رادار است که تمام آگهی‌های سایت‌های دیوار، شیپور، همشهری و دلتا را لحظه به لحظه جمع آوری کرده و به مشاورین املاک ارسال می‌کند. این سرویس به مشاورین کمک می‌کند تا برای مشتریان خود آگهی‌های واقعی و شخصی را با دقت و سرعت بالا دریافت کنند.");
        history = chatHistory.ToHistory();

        input = "در خراسان هم دارین؟";

        result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("خراسان", result?.ToLower());

        chatHistory.AddUserMessage(input);
        chatHistory.AddAssistantMessage("رادار آگهی در حال حاضر در استان‌های «تهران»، «البرز» (کرج)، «خراسان»، «استان‌های شمالی» (مازندران، گلستان، گلیان) فعال است. منبع فایل‌ها در همه این شهرها سایت‌های «دیوار»، «شیپور» و همشهری است.");
        history = chatHistory.ToHistory();

        input = "چطور می‌تونم نصب کنم؟";

        result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
        {
            ["history"] = history,
            ["input"] = input
        });

        Assert.Contains("تلگرام", result?.ToLower());
    }

    //[Fact]
    //public async Task HelpWithProduct_CorrectingQuestion_MustWork()
    //{
    //    ChatHistory chatHistory = new ChatHistory();

    //    chatHistory.AddUserMessage("سلام");
    //    chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");
    //    chatHistory.AddUserMessage("آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟");
    //    chatHistory.AddAssistantMessage("سوال شما اینه که «آیا گوشی لومیای ۱۰۵۰ من اینترنت نسل چهار رو هم پشتیبانی می‌کنه؟»، آیا درست متوجه شدم؟");
    //    var history = chatHistory.ToHistory();

    //    var input = "نه من می‌خوام بفهمم سوال من در مورد نسل پنج هست؟";

    //    var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
    //    {
    //        ["history"] = history,
    //        ["input"] = input
    //    });

    //    Assert.Contains("نسل پنج", result?.ToLower());

    //    chatHistory.AddUserMessage(input);
    //    chatHistory.AddAssistantMessage("سوال شما اینه که آیا گوشی لومیای ۱۰۵۰ پشتیبانی از اینترنت نسل پنج (5G) داره؟ من متوجه شدم.");
    //    history = chatHistory.ToHistory();

    //    input = "بله درسته";

    //    result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
    //    {
    //        ["history"] = history,
    //        ["input"] = input
    //    });

    //    Assert.Contains("ملک‌رادار", result?.ToLower());
    //}
}