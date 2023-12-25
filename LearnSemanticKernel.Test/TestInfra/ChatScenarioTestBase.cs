using System.Reflection;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;

namespace LearnSemanticKernel.Test.TestInfra;

public class ChatScenarioTestBase
{
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