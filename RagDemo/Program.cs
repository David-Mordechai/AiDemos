using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using OllamaSharp;

const string question = "What do you know about Neria and David?";
Console.WriteLine($"This program will answer the following question: {question}");
Console.WriteLine("First approach will be to ask the question directly the chat model");

Console.WriteLine();

ServiceCollection serviceCollection = new();
serviceCollection.AddKernel();

#pragma warning disable SKEXP0070
serviceCollection.AddOllamaChatCompletion("llama3.2", new Uri("http://localhost:11434/"));
serviceCollection.AddOllamaTextEmbeddingGeneration(new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2"));
IServiceProvider services = serviceCollection.BuildServiceProvider();

var kernel = services.GetRequiredService<Kernel>();


var response = kernel.InvokePromptStreamingAsync(question);
await foreach (var chunk in response)
{
    Console.Write(chunk);
}


Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("#############################################################################");
Console.WriteLine();
Console.WriteLine("Second approach will be to add facts to a semantic memory and ask the question again");
Console.WriteLine();
Console.WriteLine();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning disable SKEXP0050
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);
const string memoryCollectionName = "facts";
await memory.SaveInformationAsync(memoryCollectionName, "Neria is 41 years old, and He is Software architect. He works at Aeronautics", $"{Guid.NewGuid()}");
await memory.SaveInformationAsync(memoryCollectionName, "David is 46 years old, and one day he will be a programmer. He works at Aeronautics", $"{Guid.NewGuid()}");
TextMemoryPlugin memoryPlugin = new(memory);
kernel.ImportPluginFromObject(memoryPlugin);
OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

const string prompt = """
                          Question: {{$input}} 
                          Answer the question using the memory content: {{Recall}}
                      """;

var arguments = new KernelArguments(settings)
{
    {"input", question},
    {"collection", memoryCollectionName}
};

response = kernel.InvokePromptStreamingAsync(prompt, arguments);
await foreach (var result in response)
{
    Console.Write(result);
}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("#############################################################################");
Console.WriteLine();