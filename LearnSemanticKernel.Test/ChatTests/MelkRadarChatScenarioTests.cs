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
using skUnit;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class MelkRadarChatScenarioTests : ChatScenarioTestBase
    {
        private KernelFunction AnswerChat { get; set; }


        public MelkRadarChatScenarioTests(ITestOutputHelper output) : base(output)
        {
            var testDir = Environment.CurrentDirectory;
            MyKernel.ImportPluginFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"));
            MyKernel.ImportPluginFromPromptDirectory(Path.Combine(testDir, "Plugins", "MelkRadarAgentPlugin"));
            MyKernel.ImportPluginFromType<MelkRadarAgentPlanner>();

            AnswerChat = MyKernel.Plugins.GetFunction("MelkRadarAgentPlanner", "AnswerChat");
        }

        [Theory]
        [MemberData(nameof(GetScenarios))]
        public async Task ChatScenario_MustWork(string scenarioName)
        {
            var scenarios = await LoadChatScenarioAsync(scenarioName);
            await SemanticKernelAssert.CheckChatScenarioAsync(MyKernel, scenarios.First(), async chatHistory =>
            {
                var history = new ChatHistory(chatHistory.Take(chatHistory.Count - 1));
                var input = chatHistory.Last().Content;

                var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
                {
                    ["history"] = history.ToHistory(),
                    ["input"] = input
                });

                return result ?? "";
            });

            //await TestScenarioAsync(scenario);
        }

        

        //private async Task TestScenarioAsync(string scenarioName)
        //{
        //    var scenario = ConversationScenario.Parse(ConversationScenarioUtil.LoadScenario(scenarioName));

        //    var history = new ChatHistory();
        //    var input = "";

        //    var chats = new Queue<ChatItem>(scenario.History);

        //    while (chats.Count > 0)
        //    {
        //        var userChat = chats.Dequeue();

        //        if (userChat.Role != AuthorRole.User)
        //            throw new InvalidOperationException($"Expected User chat: {userChat.Content}");
        //        input = userChat.Content;

        //        Output.WriteLine($"""
        //            [#USER]
        //            {input}
    
        //            """);

        //        var agentChat = chats.Dequeue();
        //        if (agentChat.Role != AuthorRole.Assistant)
        //            throw new InvalidOperationException($"Expected Assistant chat: {agentChat.Content}");

        //        if (!string.IsNullOrWhiteSpace(agentChat.SemanticCondition) || agentChat.ContainsConditions.Any())
        //        {
        //            var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
        //            {
        //                ["history"] = history.ToHistory(),
        //                ["input"] = input
        //            });

        //            Output.WriteLine($"""
        //            [#AGENT]
        //            {result}
    
        //            """);

        //            if (result is null)
        //            {
        //                Assert.Fail("result is null");
        //            }

        //            if (!string.IsNullOrWhiteSpace(agentChat.SemanticCondition))
        //            {
        //                var status = await TestCriteria.InvokeAsync<string>(MyKernel, new KernelArguments()
        //                {
        //                    ["input"] = result,
        //                    ["criteria"] = agentChat.SemanticCondition
        //                });

        //                var message = $"""
        //                    Failed criteria: 
        //                    {status}
        //                    Result:
        //                    {result}
        //                    Whole Criteria:
        //                    {agentChat.SemanticCondition}
        //                    """;
        //                Assert.True(status == "True", message);
        //            }

        //            if (agentChat.ContainsConditions.Any())
        //            {
        //                var missed = new List<string>();
        //                agentChat.ContainsConditions.SelectMany(c=>c).ToList().ForEach(c =>
        //                {
        //                    var contains = result.Contains(c, StringComparison.OrdinalIgnoreCase);
        //                    if (!contains)
        //                        missed.Add(c);
        //                });

        //                Assert.True(!missed.Any(), $"""
        //                        NOT CONTAIN: Answer doesn't contain these:
        //                           - { string.Join(", ", missed)} 
        //                        Answer:
        //                        { result} 
        //                        """ );
        //            }
        //        }
        //        else
        //        {
        //            Output.WriteLine($"""
        //            [#AGENT] (DEFAULT - NOT AI GENERATED)
        //            {agentChat.Content}
    
        //            """);
        //        }

                
                

                

        //        history.AddUserMessage(userChat.Content);
        //        history.AddAssistantMessage(agentChat.Content);
        //    }
        //}

        public static IEnumerable<object[]> GetScenarios()
        {
            var scenarios = ConversationScenarioUtil.GetScenarioNames();
            return scenarios.Select(s=>new object[]{s});
        }

    }
}
