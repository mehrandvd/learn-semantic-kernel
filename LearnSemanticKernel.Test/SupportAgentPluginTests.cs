using Json.Schema.Generation.Intents;
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
            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"), "OrchestrationPlugin");
            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "SupportAgentPlugin"), "SupportAgentPlugin");
            MyKernel = builder.Build();

            GetIntent = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "GetIntent");
            AnswerChat = MyKernel.Plugins.GetFunction("SupportAgentPlugin", "AnswerChat");
        }

        [Fact]
        public async Task AnswerChat_ProductQuestion_MustWork()
        {
            var input = "What are your products for sale?";

            //var intent = await GetIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
            //{
            //    ["input"] = input,
            //    ["options"] = "QuestionAboutProduct,WantToPurchase,AngryWithSomething"
            //}));

            //Assert.Equal("QuestionAboutProduct", intent.GetValue<string>());


            var result = (
                await AnswerChat.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                }))
            ).GetValue<string>();

            Assert.Contains("QuestionAboutProduct", result ?? "");
        }
    }
}