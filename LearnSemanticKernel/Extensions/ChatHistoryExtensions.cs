using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Extensions
{
    public static class ChatHistoryExtensions
    {
        public static string ToHistory(this ChatHistory chatHistory)
        {
            return string.Join(
                Environment.NewLine,
                chatHistory.Select(c => $"[{c.Role}]: {c.Content}")
            );
        }
    }
}
