namespace Dapplo.Confluence.McpServer;

/// <summary>
/// Configuration settings for the Confluence MCP server
/// </summary>
public class ConfluenceSettings
{
    /// <summary>
    /// The Confluence server URL (e.g., https://yourcompany.atlassian.net)
    /// </summary>
    public string ConfluenceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Authentication method: "basic" or "bearer"
    /// </summary>
    public string AuthType { get; set; } = "bearer";

    /// <summary>
    /// Username for basic authentication
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password or API token for basic authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Bearer token for token-based authentication
    /// </summary>
    public string? BearerToken { get; set; }
}
