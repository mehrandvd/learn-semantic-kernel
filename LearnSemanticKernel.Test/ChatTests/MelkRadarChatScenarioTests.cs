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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class MelkRadarChatScenarioTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction TestCriteria { get; set; }

        public MelkRadarChatScenarioTests()
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

            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"), "OrchestrationPlugin");
            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "MelkRadarAgentPlugin"));
            builder.Plugins.AddFromType<MelkRadarAgentPlanner>();
            MyKernel = builder.Build();

            AnswerChat = MyKernel.Plugins.GetFunction("MelkRadarAgentPlanner", "AnswerChat");
            TestCriteria = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "TestCriteria");
        }

        [Theory]
        [MemberData(nameof(GetScenarios))]
        public async Task ChatScenario_MustWork(string scenario)
        {
            await TestScenarioAsync(scenario);
        }

        private async Task TestScenarioAsync(string scenarioName)
        {
            var scenario = ConversationScenario.Parse(ConversationScenarioUtil.LoadScenario(scenarioName));

            var history = new ChatHistory();
            var input = "";

            var chats = new Queue<ChatItem>(scenario.History);

            while (chats.Count > 0)
            {
                var userChat = chats.Dequeue();

                if (userChat.Role != AuthorRole.User)
                    throw new InvalidOperationException($"Expected User chat: {userChat.Content}");
                input = userChat.Content;

                var agentChat = chats.Dequeue();
                if (agentChat.Role != AuthorRole.Assistant)
                    throw new InvalidOperationException($"Expected Assistant chat: {agentChat.Content}");

                if (!string.IsNullOrWhiteSpace(agentChat.Criteria))
                {
                    var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
                    {
                        ["history"] = history.ToHistory(),
                        ["input"] = input
                    });

                    Console.WriteLine(result);

                    var status = await TestCriteria.InvokeAsync<string>(MyKernel, new KernelArguments()
                    {
                        ["input"] = result,
                        ["criteria"] = agentChat.Criteria
                    });

                    var message = $"{result} {Environment.NewLine} Failed Criteria: {Environment.NewLine}{agentChat.Criteria}";
                    Assert.True(status == "True", message);
                }
                

                

                history.AddUserMessage(userChat.Content);
                history.AddAssistantMessage(agentChat.Content);
            }
        }

        public static IEnumerable<object[]> GetScenarios()
        {
            var scenarios = ConversationScenarioUtil.GetScenarioNames();
            return scenarios.Select(s=>new object[]{s});
        }

    }
}
