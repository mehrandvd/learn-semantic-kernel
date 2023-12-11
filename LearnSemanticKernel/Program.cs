﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;


Console.WriteLine("Hello, World!");

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFilter("Microsoft", LogLevel.Warning)
           .AddFilter("System", LogLevel.Warning)
           .AddFilter("SampleApp.Program", LogLevel.Debug)
           .AddConsole();
});

var apiKey = Environment.GetEnvironmentVariable("mlk-openai-test-api-key", EnvironmentVariableTarget.User) ?? throw new Exception("No ApiKey in environment variables.");
var endpoint = Environment.GetEnvironmentVariable("mlk-openai-test-endpoint", EnvironmentVariableTarget.User) ?? throw new Exception("No Endpoint in environment variables.");

var kernel = new KernelBuilder()
             .AddAzureOpenAIChatCompletion("gpt-35-turbo-test", "gpt-35-turbo", endpoint, apiKey)
             .Build();


var prompt = @"{{$input}}

One line TLDR with the fewest words.";

var summarize = kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });

string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

string text2 = @"
1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
2. The acceleration of an object depends on the mass of the object and the amount of force applied.
3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

Console.WriteLine(await kernel.InvokeAsync(summarize, new KernelArguments(text1)));

Console.WriteLine(await kernel.InvokeAsync(summarize, new KernelArguments(text2)));

// Output:
//   Energy conserved, entropy increases, zero entropy at 0K.
//   Objects move in response to forces.











//var time = kernel.ImportPluginFromType<TimePlugin>();
//var result = await kernel.RunAsync(time["Today"]);

//Console.WriteLine(result);




//var kernelWithConfiguration = Kernel.Builder
//                                    .WithLoggerFactory(loggerFactory)
//                                    .WithAzureChatCompletionService(
//                                        AzureOpenAIDeploymentName,  // The name of your deployment (e.g., "gpt-35-turbo")
//                                        AzureOpenAIEndpoint,        // The endpoint of your Azure OpenAI service
//                                        AzureOpenAIApiKey           // The API key of your Azure OpenAI service
//                                    )
//                                    .Build();