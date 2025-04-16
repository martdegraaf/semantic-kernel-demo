using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Ollama;

// Build configuration
// Load .env file
Env.Load();

// Now you can access them as environment variables
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
                     ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT is not set.");
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
               ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
             ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");


var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var ollamaendpoint = "http://localhost:11434";
var ollamamodel = "llama3.1";
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
// builder
//     .AddOllamaChatCompletion(
//                 modelId: ollamamodel,
//                 endpoint: new Uri(ollamaendpoint));
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Add enterprise components
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));


var kernel = builder.Build();

kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MartFilePlugin>());
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel.Plugins.Add(KernelPluginFactory.CreateFromType<Microsoft.SemanticKernel.Plugins.Core.MathPlugin>());
kernel.Plugins.Add(KernelPluginFactory.CreateFromType<Microsoft.SemanticKernel.Plugins.Core.TextPlugin>());
kernel.Plugins.Add(KernelPluginFactory.CreateFromType<Microsoft.SemanticKernel.Plugins.Core.TimePlugin>());
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

ChatCompletionAgent agent =
    new()
    {
        Name = "SK-Assistant",
        Instructions = "You are a helpful assistant.",
        Kernel = kernel,
        Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
        

    };

await foreach (AgentResponseItem<ChatMessageContent> response 
    in agent.InvokeAsync("Vertel welke dag het vandaag is, en zeg hallo tegen de mensen van de RDW Techday. Wat is de wortel van de wortel van 42? Wat zit er in dit project? waar vind ik de infra as code?"))
{
    Console.WriteLine(response.Message);
}