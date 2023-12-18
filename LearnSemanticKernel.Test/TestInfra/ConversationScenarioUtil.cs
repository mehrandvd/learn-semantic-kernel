using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LearnSemanticKernel.Test.TestInfra
{
    public class ConversationScenarioUtil
    {
        public static string LoadScenario(string scenario)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"LearnSemanticKernel.Test.TestConversations.{scenario}.txt";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string result = reader.ReadToEnd();
            return result;
        }

        public static List<string> GetScenarioNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            var names = assembly.GetManifestResourceNames()
                    .Where(s => s.StartsWith("LearnSemanticKernel.Test.TestConversations.ChatScenario"))
                    .Select(s=>Regex.Match(s, @"LearnSemanticKernel\.Test\.TestConversations\.(?<scenario>ChatScenario.*)\.txt").Groups["scenario"].Value)
                    .ToList();

            return names;
        }
    }
}
