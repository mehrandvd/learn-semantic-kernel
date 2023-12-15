using Json.Schema.Generation.Intents;
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
            var input = "Does your products meet any specific standard?";

            var result = (
                await AnswerChat.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                }))
            ).GetValue<string>();

            Assert.Contains("standard", result ?? "");
        }
    }
}