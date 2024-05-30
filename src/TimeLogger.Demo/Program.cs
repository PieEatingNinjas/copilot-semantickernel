using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TimeLogger.Demo;
using TimeLogger.Demo.Plugins;

var builder = Kernel.CreateBuilder();


builder.Services.AddOpenAIChatCompletion(
    "gpt-4",
    Secrets.OpenAiApiKey
);


// builder.Services.AddAzureOpenAIChatCompletion(
//     ...
// );


//ToDo: demo1.9 (add plugins)

//ToDo: demo2.3 (add date helper plugin)

Kernel kernel = builder.Build();

IChatCompletionService chatCompletionService =
    kernel.GetRequiredService<IChatCompletionService>();


//ToDo: demo1.1 (system prompt)
var systemMessage = "";

var chatMessages = new ChatHistory(systemMessage);


//ToDo: demo1.2 (userId)

Console.WriteLine("--- TimeLogger Copilot ---");

Console.WriteLine("Assistant > What can I do for you today?");


// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    var result =
        chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatMessages,
            executionSettings: new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.2
            },
            kernel: kernel);

    // Stream the results
    string fullMessage = "";

    Console.Write("Assistant > ");

    await foreach (var content in result)
    {
        Console.Write(content.Content);
        fullMessage += content.Content;
    }

    Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);

    var redPowerRanger = System.Text.Json.JsonSerializer.Serialize(chatMessages);
}