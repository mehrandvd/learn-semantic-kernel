using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Test.TestInfra;

namespace LearnSemanticKernel.Test.UnitTests
{
    public class ConversationTestCaseTests
    {
        [Fact]
        public void ConversationTestCase_LoadScenario_MustWork()
        {
            var scenario = ConversationScenarioUtil.LoadScenario("Scenario_Test_Parse");
            Assert.NotNull(scenario);
        }

        [Fact]
        public void ConversationTestCase_Parse_MustWork()
        {
            var scenario = ConversationScenarioUtil.LoadScenario("Scenario_Test_Parse");
            var conversation = ConversationTestCase.Parse(scenario);

            Assert.Equal(4, conversation.History.Count);
            Assert.NotNull(conversation.Input);
            Assert.NotNull(conversation.AnswerCriteria);
        }
    }
}
