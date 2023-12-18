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
        public required string AnswerCriteria { get; set; }
        public required string AnswerSample { get; set; }

        public static ConversationScenario Parse(string text)
        {
            var parts = text.Split("------------------\r\n[#ANSWER_CRITERIA]");
            var chatPart = parts[0];

            var matches = Regex.Matches(chatPart, @"\[\#(?<role>USER|AGENT)\][\n\r]*(?<body>[^\[]*)");
            //var matches = Regex.Matches(chatPart, @"\[\#(?<role>USER|AGENT)\][\n\r]*(?<body>.*)");

            var chats = matches.Select(m =>
            {
                var parts = m.Groups["body"].Value.Split("#ANSWER_CRITERIA");
                var content = parts[0].Trim('\n', '\r');
                var criteria = parts.ElementAtOrDefault(1)?.Trim('\n', '\r');

                return new ChatItem(
                    m.Groups["role"].Value switch
                    {
                        "USER" => AuthorRole.User,
                        "AGENT" => AuthorRole.Assistant,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    content,
                    criteria
                );
            }).ToList();

            var history = chats;

            var answerPart = parts[1];
            var answerParts = answerPart.Split("[#ANSWER_SAMPLE]");
            var criteria = answerParts[0].Trim('\n', '\r');
            var sample = answerParts[1].Trim('\n', '\r');
            return new ConversationScenario()
            {
                History = history,
                AnswerCriteria = criteria,
                AnswerSample = sample
            };
        }
    }

    public class ChatItem
    {
        public ChatItem(AuthorRole role, string content, string? criteria = null)
        {
            Role = role;
            Content = content;
            Criteria = criteria;
        }
        public AuthorRole Role { get; set; }
        public string Content { get; set; }
        public string? Criteria { get; set; }

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
