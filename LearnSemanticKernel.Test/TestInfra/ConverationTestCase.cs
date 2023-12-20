using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using static LearnSemanticKernel.Test.TestInfra.ConversationScenario;

namespace LearnSemanticKernel.Test.TestInfra
{
    public class ConversationScenario
    {
        public List<ChatItem> History { get; set; } = new();

        public static ConversationScenario Parse(string text)
        {
            var chatPart = text;

            var matches = Regex.Matches(chatPart, @"\[\#(?<role>USER|AGENT)\][\n\r]*(?<body>[^\[]*)");
            //var matches = Regex.Matches(chatPart, @"\[\#(?<role>USER|AGENT)\][\n\r]*(?<body>.*)");

            var chats = matches.Select(m =>
            {
                var parts = m.Groups["body"].Value.Split("#ANSWER_CRITERIA");
                var content = parts[0].Trim('\n', '\r');
                var criteriaPart = parts.ElementAtOrDefault(1)?.Trim('\n', '\r');

                var conditions = criteriaPart?.Split('\n', '\r')?.ToList();
                
                var semanticConditions = new StringBuilder();
                var containsConditions = new List<string[]>();
                
                if (conditions is not null)
                {
                    

                    foreach (var condition in conditions)
                    {
                        var match = Regex.Match(condition, "Contains: (?<text>.*)");
                        if (match.Success)
                        {
                            var texts = match.Groups["text"].Value.Split(',', '،').Select(t=>t.Trim()).ToArray();
                            containsConditions.Add(texts);
                        }
                        else
                        {
                            semanticConditions.AppendLine(condition);
                        }
                    }
                }
                
                return new ChatItem(
                    m.Groups["role"].Value switch
                    {
                        "USER" => AuthorRole.User,
                        "AGENT" => AuthorRole.Assistant,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    content,
                    semanticConditions.ToString(),
                    containsConditions
                );
            }).ToList();

            var history = chats;

            return new ConversationScenario()
            {
                History = history,
            };
        }
    }

    public class ChatItem
    {
        public ChatItem(AuthorRole role, string content, string? semanticCondition = null, List<string[]>? containsConditions = null)
        {
            Role = role;
            Content = content;
            SemanticCondition = semanticCondition;
            ContainsConditions = containsConditions ?? new List<string[]>();
        }
        public AuthorRole Role { get; set; }
        public string Content { get; set; }
        public string? SemanticCondition { get; set; } 
        public List<string[]> ContainsConditions { get; set; }

        public override string ToString()
        {
            return $"{Role}: {Content}";
        }
    }

    public static class ChatItemExtensions
    {
        public static string ToHistory(this List<ChatItem> chatHistory)
        {
            return string.Join(
                Environment.NewLine,
                chatHistory.Select(c => $"[{c.Role}]: {c.Content}")
            );
        }
    }
}
