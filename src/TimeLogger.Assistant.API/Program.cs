using Microsoft.Extensions.DependencyInjection.Extensions;
using TimeLogger.Assistant.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<VirtualAssistantService>();
builder.Services.AddSingleton<IChatHistoryRepository, ChatHistoryRepository>();
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/chat", (string sessionId, string question, VirtualAssistantService assistent) 
        => assistent.Chat(question, sessionId))
    .WithName("chat")
    .WithOpenApi();

app.Run();