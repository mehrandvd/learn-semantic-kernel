using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Utils;
using Microsoft.SemanticKernel;

namespace LearnSemanticKernel.NativePlugins
{
    public class MelkRadarAgentPlanner
    {
        [KernelFunction, Description("Answer the user")]
        public async Task<string> AnswerChat(Kernel kernel, string input, string history)
        {
            var getSupportIntent = kernel.Plugins["OrchestrationPlugin"]["GetSupportIntent"];

            var intentText = (
                await getSupportIntent.InvokeAsync(kernel, new KernelArguments(new Dictionary<string, object?>()
                {
                    ["input"] = input,
                    ["history"] = history
                }))
            ).GetValue<string>();

            Console.WriteLine($"INTENT: {intentText}");

            if (!Enum.TryParse(typeof(SupportIntent), intentText ?? "", out var retIntent))
            {
                Console.WriteLine($"UNABLE TO PARSE INTENT: {intentText}");
            }
            
            SupportIntent intent = (SupportIntent)(retIntent ?? SupportIntent.QuestionAboutProduct) ;

            var helpWithProduct = kernel.Plugins["MelkRadarAgentPlugin"]["HelpWithProduct"];
            var helpWithPurchase = kernel.Plugins["MelkRadarAgentPlugin"]["HelpWithPurchase"];

            var result = intent switch
            {
                SupportIntent.QuestionAboutProduct =>
                    await kernel.InvokeAsync<string>(helpWithProduct,
                        new KernelArguments()
                        {
                            ["input"] = input,
                            ["history"] = history
                        }),
                SupportIntent.WantToPurchase =>
                    await kernel.InvokeAsync<string>(helpWithPurchase,
                        new KernelArguments()
                        {
                            ["input"] = input,
                            ["history"] = history
                        }),
                _ => "Oh I don't know what to do with this Mehran!"
            };

            return result ?? "";
        }

        //[KernelFunction, Description("Get intent of the user chat")]
        //public async Task<string> GetSupportIntent(Kernel kernel, string input, string history)
        //{
        //    var intent = await kernel.Plugins["OrchestrationPlugin"]["GetIntent"].InvokeAsync<string>(kernel, new KernelArguments()
        //    {
        //        ["input"] = input,
        //        ["history"] = history,
        //        ["options"] = SupportIntentUtil.GetIntentsText()
        //    });

        //    if (intent == null)
        //    {
        //        return "None";
        //    }

        //    return intent;
        //}
    }
}
