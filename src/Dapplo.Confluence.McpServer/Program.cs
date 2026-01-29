using Dapplo.Confluence;
using Dapplo.Confluence.McpServer;
using Dapplo.Confluence.McpServer.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

// Load configuration from appsettings.json and environment variables
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables(prefix: "CONFLUENCE_")
    .Build();

var settings = new ConfluenceSettings();
configuration.Bind(settings);

// Validate configuration
if (string.IsNullOrEmpty(settings.ConfluenceUrl))
{
    await Console.Error.WriteLineAsync("Error: ConfluenceUrl is not configured. Set via appsettings.json or environment variable CONFLUENCE_ConfluenceUrl");
    return 1;
}

// Create Confluence client based on auth type
IConfluenceClient confluenceClient;
try
{
    var confluenceUri = new Uri(settings.ConfluenceUrl);
    
    if (settings.AuthType.Equals("basic", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrEmpty(settings.Username) || string.IsNullOrEmpty(settings.Password))
        {
            await Console.Error.WriteLineAsync("Error: Username and Password are required for basic authentication");
            return 1;
        }
        
        confluenceClient = ConfluenceClient.Create(confluenceUri);
        confluenceClient.SetBasicAuthentication(settings.Username, settings.Password);
    }
    else if (settings.AuthType.Equals("bearer", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrEmpty(settings.BearerToken))
        {
            await Console.Error.WriteLineAsync("Error: BearerToken is required for bearer authentication");
            return 1;
        }
        
        confluenceClient = ConfluenceClient.Create(confluenceUri);
        confluenceClient.SetBearerAuthentication(settings.BearerToken);
    }
    else
    {
        await Console.Error.WriteLineAsync($"Error: Invalid AuthType '{settings.AuthType}'. Use 'basic' or 'bearer'");
        return 1;
    }
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Error initializing Confluence client: {ex.Message}");
    return 1;
}

// Create MCP server handler
var serverHandler = new McpServerHandler(confluenceClient);

// Write startup message to stderr (stdout is reserved for MCP protocol)
await Console.Error.WriteLineAsync($"Confluence MCP Server started");
await Console.Error.WriteLineAsync($"Server URL: {settings.ConfluenceUrl}");
await Console.Error.WriteLineAsync($"Auth Type: {settings.AuthType}");
await Console.Error.WriteLineAsync("Waiting for MCP requests on stdin...");

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

// Process JSON-RPC requests from stdin
using var stdin = Console.OpenStandardInput();
using var reader = new StreamReader(stdin, Encoding.UTF8);

while (!reader.EndOfStream)
{
    try
    {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line))
            continue;

        // Parse the JSON-RPC request
        var request = JsonSerializer.Deserialize<McpRequest>(line, jsonOptions);
        if (request == null)
        {
            await Console.Error.WriteLineAsync("Error: Failed to parse request");
            continue;
        }

        // Process the request
        var response = await serverHandler.ProcessRequestAsync(request);

        // Write response to stdout
        var responseJson = JsonSerializer.Serialize(response, jsonOptions);
        await Console.Out.WriteLineAsync(responseJson);
        await Console.Out.FlushAsync();
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Error processing request: {ex.Message}");
        
        // Send error response
        var errorResponse = new McpResponse
        {
            Error = new McpError
            {
                Code = -32603,
                Message = ex.Message
            }
        };
        
        var errorJson = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await Console.Out.WriteLineAsync(errorJson);
        await Console.Out.FlushAsync();
    }
}

return 0;
