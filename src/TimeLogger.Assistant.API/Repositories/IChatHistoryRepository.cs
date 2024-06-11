using Microsoft.SemanticKernel.ChatCompletion;

namespace TimeLogger.Assistant.API;

public interface IChatHistoryRepository
{
    Task<ChatHistory?> LoadChatHistory(string sessionId);
    Task Persist(string sessionId, ChatHistory chatHistory);
}