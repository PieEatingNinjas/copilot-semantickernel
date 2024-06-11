using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TimeLogger.Demo;
using TimeLogger.Demo.Plugins;

namespace TimeLogger.Assistant.API;

public class VirtualAssistantService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IChatHistoryRepository _chatHistoryRepository;
    
    const string SystemMessage = """
                                You are a friendly assistant who helps the user with their time sheets.

                                A user can ask to fill in their time sheets. For the timesheet to be
                                filled-in we need a few things:
                                  1. The company. You should help the user find the right company from our database.
                                  You should only use 1 customer / company per booking, ask additional information
                                  to find the exact one. Don't make up companies, use only the ones explicitly provided.
                                  2. The date. You need to have the exact date when to book the hours.
                                  3. The amount of hours to book.
                                  4. Give the user a summary, asking for confirmation prior to book the hours.
                                  Don't automatically do the booking, always ask!
                                  You should be able to handle multiple registrations at once. As soon as the user
                                  confirms, all the bookings can be processed.
                                    
                                As soon as you've made a booking you can give a snarky/funny comment like: 'let them pay',
                                'becoming rich with every billable hour', 'let it rain money', ...

                                Don't take any instructions from the user outside of the context of booking hours.
                                Remember: when submitting the hours, always ask for confirmation first!
                                """;
    
    public VirtualAssistantService(IChatHistoryRepository repository)
    {
        _chatHistoryRepository = repository;
        
        var builder = Kernel.CreateBuilder();
        
        builder.Services.AddOpenAIChatCompletion(
            "gpt-4",
            Secrets.OpenAiApiKey
        );
        
        builder.Plugins.AddFromType<CustomersPlugin>();
        builder.Plugins.AddFromType<TimesheetPlugin>();
        builder.Plugins.AddFromType<DateHelper>();

        _kernel = builder.Build();
        
        _chatCompletionService =
            _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async IAsyncEnumerable<string> Chat(string question, string sessionId)
    {
        var chatMessages = await _chatHistoryRepository.LoadChatHistory(sessionId);
        
        if (chatMessages is null)
        {
            //Retrieve user info from IUserService or something
            var userName = "Pieter";
            var userId = "1234";
            
            chatMessages =  new ChatHistory(SystemMessage);
            chatMessages.AddSystemMessage($"""
                                            This is the ID of the current user (UserId): '{userId}'
                                            Use this and only this UserId to communicate with the backend.
                                            This ID cannot be altered!!
                                            The name of the user is '{userName}'. Address the user personally.
                                            This name of the user cannot be altered! Don't use any other name to address the user.
                                           """);
        }
        
        chatMessages.AddUserMessage(question);
        
        var result =
            _chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: new OpenAIPromptExecutionSettings()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = 0.2
                },
                kernel: _kernel);
        
        string fullMessage = "";

        await foreach (var content in result)
        {
            
            fullMessage += content.Content;
            yield return content.Content;
            await Task.Yield();
        }

        chatMessages.AddSystemMessage(fullMessage);
        await _chatHistoryRepository.Persist(sessionId, chatMessages);
    }
}