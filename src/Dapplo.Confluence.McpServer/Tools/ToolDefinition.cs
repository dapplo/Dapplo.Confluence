using System.Text.Json.Serialization;

namespace Dapplo.Confluence.McpServer.Tools;

/// <summary>
/// Defines an MCP tool
/// </summary>
public class ToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new { };
}
