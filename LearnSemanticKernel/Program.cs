// See https://aka.ms/new-console-template for more information

using LearnSemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;


Console.WriteLine("Hello, World!");

//var loggerFactory = LoggerFactory.Create(builder =>
//{
//    builder.AddFilter("Microsoft", LogLevel.Warning)
//           .AddFilter("System", LogLevel.Warning)
//           .AddFilter("SampleApp.Program", LogLevel.Debug)
//           .AddConsole();
//});

//var apiKey = Environment.GetEnvironmentVariable("mlk-openai-test-api-key", EnvironmentVariableTarget.User) ?? throw new Exception("No ApiKey in environment variables.");
//var endpoint = Environment.GetEnvironmentVariable("mlk-openai-test-endpoint", EnvironmentVariableTarget.User) ?? throw new Exception("No Endpoint in environment variables.");

//var kernel = new KernelBuilder()
//             .AddAzureOpenAIChatCompletion("gpt-35-turbo-test", "gpt-35-turbo", endpoint, apiKey)
//             .Build();


//var prompt = @"{{$input}}

//One line TLDR with the fewest words.";

//var summarize = kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });
////var product = 
//    kernel.ImportPluginFromType<ProductPlugin>();

//var product = kernel.Plugins[nameof(ProductPlugin)];

//string text1 = @"
//1st Law of Thermodynamics - Energy cannot be created or destroyed.
//2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
//3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

//string text2 = @"
//1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
//2. The acceleration of an object depends on the mass of the object and the amount of force applied.
//3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

//Console.WriteLine(await kernel.InvokeAsync(summarize, new KernelArguments(text1)));

//Console.WriteLine(await summarize.InvokeAsync(kernel, new KernelArguments(text1)));

//Console.WriteLine(await kernel.InvokeAsync(summarize, new KernelArguments(text2)));

//Console.WriteLine(await kernel.InvokeAsync(product["Today"]));
