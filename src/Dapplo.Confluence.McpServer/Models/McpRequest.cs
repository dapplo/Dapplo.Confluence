using System.Text.Json.Serialization;

namespace Dapplo.Confluence.McpServer.Models;

/// <summary>
/// Represents a JSON-RPC 2.0 request for MCP
/// </summary>
public class McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}
