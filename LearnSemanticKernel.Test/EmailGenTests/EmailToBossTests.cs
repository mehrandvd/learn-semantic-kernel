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
        private Kernel MyKernel { get; set; }
        IChatCompletionService ChatCompletionService { get; set; }
        
        private ITestOutputHelper Output { get; set; }

        public EmailToBossTests(ITestOutputHelper output)
        {
            Output = output;

            var testDir = Environment.CurrentDirectory;

            var apiKey =
                Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                throw new Exception("No ApiKey in environment variables.");
            var endpoint =
                Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                throw new Exception("No Endpoint in environment variables.");
            var deploymentName =
                Environment.GetEnvironmentVariable("openai-deployment-name", EnvironmentVariableTarget.User) ??
                throw new Exception("No DeploymentName in environment variables.");

            SemanticKernelAssert.Initialize(deploymentName, endpoint, apiKey, message => Output.WriteLine(message));

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            builder.Services.AddLogging(loggerBuilder =>
            {
                loggerBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug();
                loggerBuilder.ClearProviders();
                loggerBuilder.AddConsole();
            });
            
            //builder.Services.AddChatCompletionService(kernelSettings);
            
            builder.Plugins.AddFromType<EmailPlugin>();
            builder.Plugins.AddFromType<AuthorEmailPlanner>();
            MyKernel = builder.Build();

            ChatHistory chatMessages = new ChatHistory("""
                        You are a friendly assistant who likes to follow the rules. You will complete required steps
                        and request approval before taking any consequential actions. If the user doesn't provide
                        enough information for you to complete a task, you will keep asking questions until you have
                        enough information to complete the task.
                        """);

            ChatCompletionService = MyKernel.GetRequiredService<IChatCompletionService>();

#pragma warning disable SKEXP0004
            MyKernel.FunctionInvoked += (sender, args) => output.WriteLine($"FUNCTION CALL: {args.Function.Name}");
            MyKernel.PromptRendered += (sender, args) => output.WriteLine($"PROMPT RENDERED: {args.RenderedPrompt}");
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
