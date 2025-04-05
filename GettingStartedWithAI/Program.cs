using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

// reference https://www.youtube.com/watch?v=VAP2ARHkbe4

ServiceCollection serviceCollection = new();
serviceCollection.AddKernel();
serviceCollection.AddOllamaChatCompletion("llama3.2", new Uri("http://localhost:11434/"));
IServiceProvider services = serviceCollection.BuildServiceProvider();

var kernel = services.GetRequiredService<Kernel>();
kernel.ImportPluginFromType<Demographics>();
PromptExecutionSettings settings = new OllamaPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

var chatService = services.GetRequiredService<IChatCompletionService>();

ChatHistory chatHistory = [];

while (true)
{
    Console.Write("Q: ");
    chatHistory.AddUserMessage(Console.ReadLine() ?? string.Empty);

    var assistant = await chatService.GetChatMessageContentsAsync(chatHistory, settings, kernel);
    chatHistory.AddAssistantMessage(assistant[0].ToString());
    Console.WriteLine(assistant[0]);
}

internal class Demographics
{
    [KernelFunction]
    public int GetPersonAge(string name)
    {
        return name switch
        {
            "David" => 46,
            "Neria" => 41,
            "Lior" => 39,
            _ => 44
        };
    }
}