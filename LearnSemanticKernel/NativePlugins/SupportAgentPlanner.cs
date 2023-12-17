using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace LearnSemanticKernel.NativePlugins
{
    public class SupportAgentPlanner
    {
        [KernelFunction, Description("Answer the user")]
        public async Task<string> AnswerChat(Kernel kernel, string input, string history)
        {
            var intent = await kernel.Plugins["OrchestrationPlugin"]["GetIntent"].InvokeAsync<string>(kernel, new KernelArguments()
            {
                ["input"] = input,
                ["options"] = "QuestionAboutProduct,WantToPurchase,AngryWithSomething"
            });

            var result = intent switch
            {
                "QuestionAboutProduct" =>
                    await kernel.Plugins["SupportAgentPlugin"]["HelpWithProduct"].InvokeAsync<string>(kernel,
                        new KernelArguments()
                        {
                            ["input"] = input,
                            ["history"] = history
                        }),
                _ => "Oh I don't know what to do with this Mehran!"
            };

            return result ?? "";
        }
    }
}
