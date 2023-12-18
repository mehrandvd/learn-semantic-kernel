using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class ProductQuestionChatTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction TestCriteria { get; set; }

        public ProductQuestionChatTests()
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
            TestCriteria = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "TestCriteria");
        }

        [Fact]
        public async Task ChatScenario_1000_01()
        {
            var scenario = ConversationTestCase.Parse(ConversationScenarioUtil.LoadScenario("ChatScenario_1000_01"));

            var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["history"] = scenario.History.ToHistory(),
                ["input"] = scenario.Input
            });

            var status = await TestCriteria.InvokeAsync<string>(MyKernel, new KernelArguments()
            {
                ["input"] = result,
                ["criteria"] = scenario.AnswerCriteria
            });

            Assert.Equal("True", status);
        }
    }
}
