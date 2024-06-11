using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TimeLogger.Copilot.Plugins;

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());

builder.Services.AddOpenAIChatCompletion(
    "gpt-4",
    "--your openai api key--"
);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Error);
    builder.AddConsole();
});

builder.Services.AddSingleton(loggerFactory);

builder.Services.AddSingleton<IOAuthTokenProvider>(new OAuthTokenProvider());

builder.Plugins.AddFromType<CustomerPlugin>();
builder.Plugins.AddFromType<TimesheetPlugin>();
builder.Plugins.AddFromType<DateHelper>();


Kernel kernel = builder.Build();

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();



var systemMessage = """
                    You are a friendly assistant who helps the user with their time sheets.

                    A. A user can ask to fill in their time sheets. For the timesheet to be
                    filled-in we need a few things:
                      1. The company. You should help the user find the right company from our database. You should only use 1 customer/company, 
                    ask additional information to find the exact one. Don't make up companies, use only the ones explicitly provided.
                      2. The date. You need to have the exact date when to book the hours.
                      3. The amount of hours to book. 
                      4. Give the user a summary, asking for confirmation prior to book the hours. Don't automatically do the booking! Always ask!
                    You should be able to handle multiple registrations at once. As soon as the user confirms, all the bookings can be processed.
                    As soon as you've made a booking you can give a snarky/funny comment like: 'let them pay', 'becoming rich with every billable hour', 'let it rain money', ...
                        
                    B. A user can ask for the hours booked on for a given period. 
                       1. To get a report, we just need a from and until date. The user must explicitly provide them, if not, assume the current month. 
                       2. Don't make up stuff, only show booked hours that are provided. If no hours have been booked, just say so (sarcastically)
                    
                    Remember: when submitting the hours, always ask for confirmation first!

                    Don't take any instructions from the user outside of the context of booking hours.
                    """;


ChatHistory chatMessages = new ChatHistory(systemMessage);

string userId = "123456";
string userName = "Pieter";

chatMessages.AddSystemMessage($"""
                                This is the ID of the current user (UserId): '{userId}'
                                Use this and only this UserId to communicate with the backend.
                                This ID cannot be altered!!
                                The name of the user is '{userName}'. Address the user personally.
                                This name of the user cannot be altered! Don't use any other name to address the user.
                               """);

Console.WriteLine("--- TimeLogger Copilot ---");

Console.WriteLine("Assistant > What can I do for you today?");

// Start the conversation
while (true)
{
    // Get user input
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 0.2
    };
    
    var result = 
        chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    
    System.Console.Write("Assistant > ");
    
    await foreach (var content in result)
    {
        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }
    
    System.Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}