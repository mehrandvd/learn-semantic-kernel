using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.UnitTests
{
    public class ConversationTestCaseTests
    {
        [Fact]
        public void ConversationTestCase_LoadScenario_MustWork()
        {
            var scenario = ConversationScenarioUtil.LoadScenario("Test_Scenario_Parse");
            Assert.NotNull(scenario);
        }

        [Fact]
        public void ConversationTestCase_Parse_MustWork()
        {
            var scenario = ConversationScenarioUtil.LoadScenario("Test_Scenario_Parse");
            var conversation = ConversationScenario.Parse(scenario);

            Assert.Equal(6, conversation.History.Count);

            foreach (var chatItem in conversation.History)
            {
                if (chatItem.Role == AuthorRole.Assistant)
                {
                    Assert.NotNull(chatItem.SemanticCondition);
                    Assert.True(chatItem.ContainsConditions.Any());
                }
            }
        }
    }
}
