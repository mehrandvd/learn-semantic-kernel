using Json.Schema.Generation.Intents;
using LearnSemanticKernel.Extensions;
using LearnSemanticKernel.NativePlugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test
{
    public class SupportAgentPluginTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction GetIntent { get; set; }

        public SupportAgentPluginTests()
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

            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"), "OrchestrationPlugin");
            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "SupportAgentPlugin"), "SupportAgentPlugin");
            builder.Plugins.AddFromType<SupportAgentPlanner>();
            MyKernel = builder.Build();

            GetIntent = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "GetIntent");
            AnswerChat = MyKernel.Plugins.GetFunction("SupportAgentPlanner", "AnswerChat");
        }

        [Fact]
        public async Task AnswerChat_ProductQuestion_MustWork()
        {
            var input = "Does your Lumia 1050XL meet any specific standard?";
            
            var history =
                """
                User: Hi
                Agent: How can I help you?
                """;

            var result = (
                await AnswerChat.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Contains("standard", result ?? "");
        }

        [Fact]
        public async Task AnswerChat_ProductQuestion_SimpleFlow_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("Hi");
            chatHistory.AddAssistantMessage("Hi, How can I help you?");

            var history = chatHistory.ToHistory();

            var input = "Does my phone (Lumia 1050XL) support 4G?";

            var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["history"] = history,
                ["input"] = input
            });

            Assert.Contains("lumia 1050xl", result?.ToLower());


            chatHistory.AddUserMessage(input);
            chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 4G?");
            history = chatHistory.ToHistory();

            input = "Yes";

            result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["history"] = history,
                ["input"] = input
            });

            Assert.Contains("tech", result?.ToLower());
        }

        [Fact]
        public async Task AnswerChat_ProductQuestion_CorrectingQuestion_MustWork()
        {
            ChatHistory chatHistory = new ChatHistory();

            chatHistory.AddUserMessage("Hi");
            chatHistory.AddAssistantMessage("Hi, How can I help you?");
            chatHistory.AddUserMessage("Does my phone (Lumia 1050XL) support 4G?");
            chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 4G?");
            var history = chatHistory.ToHistory();

            var input = "No, I want to see if it supports 5G.";

            var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["history"] = history,
                ["input"] = input
            });

            Assert.Contains("5g", result?.ToLower());

            chatHistory.AddUserMessage(input);
            chatHistory.AddAssistantMessage("Do I get your question correctly? you are asking whether your phone (Lumia 1050XL) supports 5G?");
            history = chatHistory.ToHistory();

            input = "Yes, it's correct";

            result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["history"] = history,
                ["input"] = input
            });

            Assert.Contains("tech", result?.ToLower());
        }
    }
}