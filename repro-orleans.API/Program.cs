using Azure.Core;
using Azure.Identity;
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

app.Run();
