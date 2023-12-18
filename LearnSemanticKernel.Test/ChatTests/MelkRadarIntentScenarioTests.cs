using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.SemanticKernel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Extensions;
using LearnSemanticKernel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class MelkRadarIntentScenarioTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction GetSupportIntent { get; set; }

        public MelkRadarIntentScenarioTests()
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
                loggerBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug();
                loggerBuilder.ClearProviders();
                loggerBuilder.AddConsole();
            });

            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"));
            MyKernel = builder.Build();

            GetSupportIntent = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "GetSupportIntent");
        }

        [Fact]
        public async Task ChatScenario_MustWork()
        {
            await TestScenarioAsync("Scenario_Pricing_AdverRadar", SupportIntent.AskPriceOrWantToPurchase.ToString());
        }

        private async Task TestScenarioAsync(string scenarioName, string intent)
        {
            var scenario = ConversationScenario.Parse(ConversationScenarioUtil.LoadScenario(scenarioName));

            var history = new ChatHistory(scenario.History.Take(scenario.History.Count-1).Select(c=>new ChatMessageContent(c.Role, c.Content)));
            var input = scenario.History.Last().Content ?? "";

            var result = (
                await GetSupportIntent.InvokeAsync(MyKernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Assert.Equal(intent, result);
        }

        public static IEnumerable<object[]> GetScenarios()
        {
            var scenarios = ConversationScenarioUtil.GetScenarioNames();
            return scenarios.Select(s=>new object[]{s});
        }

    }
}
