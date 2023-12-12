using Json.Schema.Generation.Intents;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace LearnSemanticKernel.Test
{
    public class OrchestrationPluginTests
    {
        private Kernel Kernel { get; set; }
        private IKernelPlugin OrchestrationPlugin { get; set; }
        private KernelFunction GetIntent { get; set; }

        public OrchestrationPluginTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("mlk-openai-test-api-key", EnvironmentVariableTarget.User) ?? throw new Exception("No ApiKey in environment variables.");
            var endpoint = Environment.GetEnvironmentVariable("mlk-openai-test-endpoint", EnvironmentVariableTarget.User) ?? throw new Exception("No Endpoint in environment variables.");

            Kernel = new KernelBuilder()
                         .AddAzureOpenAIChatCompletion("gpt-35-turbo-test", "gpt-35-turbo", endpoint, apiKey)
                         .Build();

            var testDir = Environment.CurrentDirectory;

            OrchestrationPlugin = Kernel.CreatePluginFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"));
            GetIntent = OrchestrationPlugin["GetIntent"];

            Kernel.PromptRendered += (sender, args) => Console.WriteLine(args.RenderedPrompt);
        }

        [Fact]
        public async Task GetIntent_Food_MustWork()
        {
            var result = await GetIntent.InvokeAsync(Kernel, new KernelArguments(new Dictionary<string, object?>()
            {
                ["input"] = "I want some pizza",
                ["options"] = "OrderFood, OrderHotel, OrderCar"
            }));

            Assert.Equal("OrderFood", result.GetValue<string>());
        }

        [Fact]
        public async Task GetIntent_Hotel_MustWork()
        {
            var result = await GetIntent.InvokeAsync(Kernel, new KernelArguments(new Dictionary<string, object?>()
            {
                ["input"] = "I want a good room",
                ["options"] = "OrderFood, OrderHotel, OrderCar"
            }));

            Assert.Equal("OrderHotel", result.GetValue<string>());
        }
    }
}