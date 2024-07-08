using API;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddAzureClients(azureBuilder =>
{
    DefaultAzureCredential credential = new (options: new()
    {
        ExcludeManagedIdentityCredential = Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT") == null,
        ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
    });

    azureBuilder.UseCredential(credential);
    builder.Services.AddSingleton<TokenCredential>(credential);
});

// Configure orleans storage
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureBlobClient("grainstate");

// Add orleans host
builder.UseOrleans();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.MapPost("/{user}/join/{room}", async (IClusterClient client, string user, string room) =>
{
    var participant = client.GetGrain<IParticipantGrain>(user);
    await participant.JoinAsync(room);
    return Results.Ok();
});

app.MapPost("/{user}/message", async (IClusterClient client, string user, [FromBody] Message message) =>
{
    var participant = client.GetGrain<IParticipantGrain>(user);
    await participant.SayAsync(message.Value);
    return Results.Ok();
});

app.MapGet("/{user}/messages", async (IClusterClient client, string user) =>
{
    var participant = client.GetGrain<IParticipantGrain>(user);
    var messages = await participant.GetMessagesAsync();

    return Results.Ok(messages.Select(p => new { p.user, p.message }));
});

app.Run();

record Message(string Value);
