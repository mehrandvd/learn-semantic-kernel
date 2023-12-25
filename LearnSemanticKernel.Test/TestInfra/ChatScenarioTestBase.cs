using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using skUnit;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit.Abstractions;

namespace LearnSemanticKernel.Test.TestInfra;

public class ChatScenarioTestBase
{
    protected Kernel MyKernel { get; set; }
    protected ITestOutputHelper Output { get; set; }
    protected SemanticKernelAssert SemanticKernelAssert { get; set; }
    public ChatScenarioTestBase(ITestOutputHelper output)
    {
        Output = output;

        var apiKey =
            Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
            throw new Exception("No ApiKey in environment variables.");
        var endpoint =
            Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
            throw new Exception("No Endpoint in environment variables.");
        var deploymentName =
            Environment.GetEnvironmentVariable("openai-deployment-name", EnvironmentVariableTarget.User) ??
            throw new Exception("No DeploymentName in environment variables.");

        SemanticKernelAssert = new SemanticKernelAssert(deploymentName, endpoint, apiKey, message => Output.WriteLine(message));

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        builder.Services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug();
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });

        MyKernel = builder.Build();

    }
    protected async Task<List<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        var testContent = await LoadChatTestAsync(scenario);
        var scenarios = ChatScenarioParser.Parse(testContent, "");
        return scenarios;
    }

    private async Task<string> LoadChatTestAsync(string scenario)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"LearnSemanticKernel.Test.{scenario}.md";
        await using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();
        return result ?? "";
    }
}