using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.TestInfra
{
    public class ConversationTestCase
    {
        public ChatHistory History { get; set; } = new();
        public string? Input { get; set; }
        public required string AnswerCriteria { get; set; }
        public required string AnswerSample { get; set; }

        public static ConversationTestCase Parse(string text)
        {
            var parts = text.Split("------------------\r\n[#ANSWER_CRITERIA]");
            var chatPart = parts[0];

            var matches = Regex.Matches(chatPart, @"\[\#(?<role>USER|AGENT)\][\n\r]*(?<content>.*)");

            var chats = matches.Select(m => new ChatMessageContent(
                    m.Groups["role"].Value switch
                    {
                        "USER" => AuthorRole.User,
                        "AGENT" => AuthorRole.Assistant,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    m.Groups["content"].Value
                )
            ).ToList();

            var input = chats.Last().Content?.Trim();

            var history = new ChatHistory(chats.Take(chats.Count-1));

            var answerPart = parts[1];
            var answerParts = answerPart.Split("[#ANSWER_SAMPLE]");
            var criteria = answerParts[0].Trim();
            var sample = answerParts[1].Trim();
            return new ConversationTestCase()
            {
                Input = input,
                History = history,
                AnswerCriteria = criteria,
                AnswerSample = sample
            };
        }
    }
}
