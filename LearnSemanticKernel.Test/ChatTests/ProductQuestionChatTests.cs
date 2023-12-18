﻿using LearnSemanticKernel.NativePlugins;
using LearnSemanticKernel.Test.TestInfra;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LearnSemanticKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LearnSemanticKernel.Test.ChatTests
{
    public class ProductQuestionChatTests
    {
        private Kernel MyKernel { get; set; }
        private KernelFunction AnswerChat { get; set; }
        private KernelFunction TestCriteria { get; set; }

        public ProductQuestionChatTests()
        {
            var testDir = Environment.CurrentDirectory;

            var apiKey =
                Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                throw new Exception("No ApiKey in environment variables.");
            var endpoint =
                Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                throw new Exception("No Endpoint in environment variables.");

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", endpoint, apiKey);
            builder.Services.AddLogging(loggerBuilder =>
            {
                loggerBuilder.SetMinimumLevel(LogLevel.Information);
                loggerBuilder.ClearProviders();
                loggerBuilder.AddConsole();
            });

            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "OrchestrationPlugin"), "OrchestrationPlugin");
            builder.Plugins.AddFromPromptDirectory(Path.Combine(testDir, "Plugins", "MelkRadarAgentPlugin"));
            builder.Plugins.AddFromType<MelkRadarAgentPlanner>();
            MyKernel = builder.Build();

            AnswerChat = MyKernel.Plugins.GetFunction("MelkRadarAgentPlanner", "AnswerChat");
            TestCriteria = MyKernel.Plugins.GetFunction("OrchestrationPlugin", "TestCriteria");
        }

        [Fact]
        public async Task ChatScenario_1000_01()
        {
            var scenario = ConversationScenario.Parse(ConversationScenarioUtil.LoadScenario("ChatScenario_1000_01"));

            var history = new ChatHistory();
            var input = "";

            var chats = new Queue<ChatItem>(scenario.History);

            while (chats.Count > 0)
            {
                var userChat = chats.Dequeue();

                if (userChat.Role != AuthorRole.User)
                    throw new InvalidOperationException($"Expected User chat: {userChat.Content}");
                input = userChat.Content;

                var agentChat = chats.Dequeue();
                if (agentChat.Role != AuthorRole.Assistant)
                    throw new InvalidOperationException($"Expected Assistant chat: {agentChat.Content}");

                var result = await AnswerChat.InvokeAsync<string>(MyKernel, new KernelArguments()
                {
                    ["history"] = history.ToHistory(),
                    ["input"] = input
                });

                var status = await TestCriteria.InvokeAsync<string>(MyKernel, new KernelArguments()
                {
                    ["input"] = result,
                    ["criteria"] = agentChat.Criteria
                });

                var message = $"{result} failed criteria: {agentChat.Criteria}";

                Assert.True(status == "True", message);

                history.AddUserMessage(userChat.Content);
                history.AddAssistantMessage(agentChat.Content);
            }
        }
    }
}
