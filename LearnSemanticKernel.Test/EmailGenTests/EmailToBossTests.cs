using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.EmailGen;
using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using skUnit;
using skUnit.Scenarios;
using Xunit.Abstractions;

namespace LearnSemanticKernel.Test.EmailGenTests
{
    public class EmailToBossTests : ChatScenarioTestBase
    {
        public EmailToBossTests(ITestOutputHelper output) : base(output)
        {
            Output = output;

            MyKernel.ImportPluginFromType<EmailPlugin>();
            MyKernel.ImportPluginFromType<AuthorEmailPlanner>();

            ChatHistory chatMessages = new ChatHistory("""
                        You are a friendly assistant who likes to follow the rules. You will complete required steps
                        and request approval before taking any consequential actions. If the user doesn't provide
                        enough information for you to complete a task, you will keep asking questions until you have
                        enough information to complete the task.
                        """);
        }

        [Fact]
        public async Task SampleScenario()
        {
            var scenarios = await LoadChatScenarioAsync("EmailGenTests.Scenarios.Scenario_WebsiteSample");
            var scenario = scenarios.First();

            scenario.ChatItems.Insert(0, new ChatItem(AuthorRole.System,
                """
                You are a friendly assistant who likes to follow the rules. You will complete required steps
                and request approval before taking any consequential actions. If the user doesn't provide
                enough information for you to complete a task, you will keep asking questions until you have
                enough information to complete the task.
                """));

            await SemanticKernelAssert.ScenarioChatSuccessAsync(MyKernel, scenario);
        }
    }
}
