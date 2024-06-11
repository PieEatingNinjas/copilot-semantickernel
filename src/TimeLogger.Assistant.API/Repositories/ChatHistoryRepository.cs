using Microsoft.SemanticKernel.ChatCompletion;

namespace TimeLogger.Assistant.API;

public class ChatHistoryRepository : IChatHistoryRepository
{
    private Dictionary<string, string> _cache = new();
    public Task<ChatHistory?> LoadChatHistory(string sessionId)
    {
        if (_cache.TryGetValue(sessionId, out string? json))
        {
            return Task.FromResult(System.Text.Json.JsonSerializer.Deserialize<ChatHistory>(json));
        }

        return Task.FromResult<ChatHistory?>(null);
    }

    public Task Persist(string sessionId, ChatHistory chatHistory)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(chatHistory);
        _cache[sessionId] = json;
        return Task.CompletedTask;
    }
}