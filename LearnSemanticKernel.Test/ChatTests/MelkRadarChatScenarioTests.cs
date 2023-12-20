using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.SemanticKernel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LearnSemanticKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit.Abstractions;
using System.ComponentModel;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class MelkRadarChatScenarioTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction TestCriteria { get; set; }

        private ITestOutputHelper Output { get; set; }

        public MelkRadarChatScenarioTests(ITestOutputHelper output)
        {
            Output = output;

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

                if (!string.IsNullOrWhiteSpace(agentChat.SemanticCondition) || agentChat.ContainsConditions.Any())
                {
                    var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
                    {
                        ["history"] = history.ToHistory(),
                        ["input"] = input
                    });

                    if (result is null)
                    {
                        Assert.Fail("result is null");
                    }

                    if (!string.IsNullOrWhiteSpace(agentChat.SemanticCondition))
                    {
                        var status = await TestCriteria.InvokeAsync<string>(MyKernel, new KernelArguments()
                        {
                            ["input"] = result,
                            ["criteria"] = agentChat.SemanticCondition
                        });

                        var message = $"""
                            Failed criteria: 
                            {status}
                            Result:
                            {result}
                            Whole Criteria:
                            {agentChat.SemanticCondition}
                            """;
                        Assert.True(status == "True", message);

                        Output.WriteLine(result);
                    }

                    if (agentChat.ContainsConditions.Any())
                    {
                        var missed = new List<string>();
                        agentChat.ContainsConditions.SelectMany(c=>c).ToList().ForEach(c =>
                        {
                            var contains = result.Contains(c, StringComparison.OrdinalIgnoreCase);
                            if (!contains)
                                missed.Add(c);
                        });

                        Assert.True(!missed.Any(), $"""
                                NOT CONTAIN: Answer doesn't contain these:
                                   - { string.Join(", ", missed)} 
                                Answer:
                                { result} 
                                """ );
                    }
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
