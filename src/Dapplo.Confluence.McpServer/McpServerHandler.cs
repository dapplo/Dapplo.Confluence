using Dapplo.Confluence.McpServer.Models;
using Dapplo.Confluence.McpServer.Tools;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dapplo.Confluence.McpServer;

/// <summary>
/// MCP server handler that processes JSON-RPC requests
/// </summary>
public class McpServerHandler
{
    private readonly ToolRegistry _toolRegistry;
    private const string McpVersion = "2024-11-05";

    public McpServerHandler(IConfluenceClient confluenceClient)
    {
        _toolRegistry = new ToolRegistry(confluenceClient);
    }

    /// <summary>
    /// Process an MCP request and return a response
    /// </summary>
    public async Task<McpResponse> ProcessRequestAsync(McpRequest request)
    {
        try
        {
            var result = request.Method switch
            {
                "initialize" => HandleInitialize(request.Params),
                "tools/list" => HandleToolsList(),
                "tools/call" => await HandleToolsCallAsync(request.Params),
                "ping" => new { },
                _ => throw new InvalidOperationException($"Unknown method: {request.Method}")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = ex.Message,
                    Data = new { exception = ex.GetType().Name }
                }
            };
        }
    }

    private object HandleInitialize(object? parameters)
    {
        return new
        {
            protocolVersion = McpVersion,
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "confluence-mcp-server",
                version = "1.0.0"
            }
        };
    }

    private object HandleToolsList()
    {
        var tools = _toolRegistry.GetToolDefinitions();
        return new { tools };
    }

    private async Task<object> HandleToolsCallAsync(object? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters required for tools/call");

        var jsonElement = JsonSerializer.SerializeToElement(parameters);
        
        var toolName = jsonElement.GetProperty("name").GetString() 
            ?? throw new ArgumentException("Tool name is required");

        JsonElement? toolParams = null;
        if (jsonElement.TryGetProperty("arguments", out var argsElement))
        {
            toolParams = argsElement;
        }

        var result = await _toolRegistry.ExecuteToolAsync(toolName, toolParams);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = JsonSerializer.Serialize(result, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    })
                }
            }
        };
    }
}
