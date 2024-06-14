using System.Net.Http.Json;

using HttpClient client = new();
client.BaseAddress = new Uri("https://localhost:7027");

Console.WriteLine("--- TimeLogger Copilot ---");

Console.WriteLine("Assistant > What can I do for you today?");

string session = Guid.NewGuid().ToString();

// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");

    var question = Console.ReadLine();

    Console.Write("Assistant > ");
    await foreach (var msg in client.GetFromJsonAsAsyncEnumerable<string>(
                       $"/chat?sessionId={session}&question={question}"))
    {
        Console.Write(msg);
    }
    Console.WriteLine();
}
