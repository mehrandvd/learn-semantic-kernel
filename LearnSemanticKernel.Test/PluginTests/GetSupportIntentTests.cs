using Json.Schema.Generation.Intents;
using LearnSemanticKernel.Extensions;
using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.PluginTests
{
    public class GetSupportIntentTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction GetSupportIntent { get; set; }

        public GetSupportIntentTests()
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
                loggerBuilder.ClearProviders();
                loggerBuilder.AddConsole();
            });

            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"));
            builder.Plugins.AddFromType<SupportAgentPlanner>();
            MyKernel = builder.Build();

            GetSupportIntent = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "GetSupportIntent");
        }

        [Fact]
        public async Task GetSupportIntent_ProductQuestion_Simple_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("Hi");
            chatHistory.AddAssistantMessage("Hi, How can I help you?");

            var history = chatHistory.ToHistory();

            var input = "Does my phone (Lumia 1050XL) support 4G?";

            var result = (
                await GetSupportIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Equal(SupportIntent.QuestionAboutProduct.ToString(), result ?? "");
        }

        [Fact]
        public async Task GetSupportIntent_ProductQuestion_Persian_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("سلام");
            chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");

            var history = chatHistory.ToHistory();

            var input = "آیا گوشی من نسل جدید رو هم پشتیبانی می‌کنه؟";

            var result = (
                await GetSupportIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Equal(SupportIntent.QuestionAboutProduct.ToString(), result ?? "");
        }

        [Fact]
        public async Task GetSupportIntent_WantToPurchase_Persian_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("سلام");
            chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");

            var history = chatHistory.ToHistory();

            var input = "من می‌خوام یه اکانت یه ماهه بخرم؟";

            var result = (
                await GetSupportIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Equal(SupportIntent.WantToPurchase.ToString(), result ?? "");
        }

        [Fact]
        public async Task GetSupportIntent_AngryWithSomething_Persian_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("سلام");
            chatHistory.AddAssistantMessage("سلام، چطور می‌تونم کمکتون کنم؟");

            var history = chatHistory.ToHistory();

            var input = "این چه وضعشه،‌ همش دارین آگهی تکراری می‌فرستین واسه من؟";

            var result = (
                await GetSupportIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Equal(SupportIntent.AngryWithSomething.ToString(), result ?? "");
        }
    }
}