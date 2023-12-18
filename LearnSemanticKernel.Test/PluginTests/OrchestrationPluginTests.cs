using Json.Schema.Generation.Intents;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.PluginTests
{
    public class OrchestrationPluginTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction GetIntent { get; set; }

        public OrchestrationPluginTests()
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
            MyKernel = builder.Build();

            GetIntent = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "GetIntent");
        }

        [Fact]
        public async Task GetIntent_Food_MustWork()
        {
            var result = await GetIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
            {
                ["input"] = "I want some pizza",
                ["options"] = "OrderFood, OrderHotel, OrderCar"
            }));

            Assert.Equal("OrderFood", result.GetValue<string>());
        }

        [Fact]
        public async Task GetIntent_Hotel_MustWork()
        {
            var result = await GetIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
            {
                ["input"] = "I want a good room",
                ["options"] = "OrderFood, OrderHotel, OrderCar"
            }));

            Assert.Equal("OrderHotel", result.GetValue<string>());
        }
    }
}