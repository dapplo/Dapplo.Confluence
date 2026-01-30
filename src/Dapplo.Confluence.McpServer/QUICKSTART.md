# Quick Start Guide - Confluence MCP Server

Get started with the Confluence MCP Server in 3 simple steps!

## Step 1: Get Your Confluence API Token

1. Go to [https://id.atlassian.com/manage-profile/security/api-tokens](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click **Create API token**
3. Give it a name like "MCP Server"
4. Copy the token (you won't be able to see it again!)

## Step 2: Configure the Server

1. Navigate to the MCP Server directory:
   ```bash
   cd src/Dapplo.Confluence.McpServer
   ```

2. Copy the example configuration:
   ```bash
   cp appsettings.example.json appsettings.json
   ```

3. Edit `appsettings.json` and add your details:
   ```json
   {
     "ConfluenceUrl": "https://yourcompany.atlassian.net",
     "AuthType": "bearer",
     "BearerToken": "YOUR_API_TOKEN_HERE"
   }
   ```

## Step 3: Run the Server

### Test it directly:
```bash
dotnet run
```

The server will start and wait for MCP requests on stdin. You should see:
```
Confluence MCP Server started
Server URL: https://yourcompany.atlassian.net
Auth Type: bearer
Waiting for MCP requests on stdin...
```

### Test with a sample request:
```bash
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run
```

You should get a JSON response with server information and capabilities.

## Using with Claude Desktop

Add this to your Claude Desktop config file:

**macOS/Linux:** `~/Library/Application Support/Claude/claude_desktop_config.json`
**Windows:** `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "confluence": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/full/path/to/Dapplo.Confluence.McpServer/Dapplo.Confluence.McpServer.csproj"
      ],
      "env": {
        "CONFLUENCE_ConfluenceUrl": "https://yourcompany.atlassian.net",
        "CONFLUENCE_AuthType": "bearer",
        "CONFLUENCE_BearerToken": "your-api-token"
      }
    }
  }
}
```

Restart Claude Desktop, and you'll now have access to Confluence tools!

## What Can You Do?

Ask Claude to:
- "Search for pages about API documentation in our Confluence"
- "Show me the content of the page titled 'Team Guidelines'"
- "Create a new page in the DEV space with a summary of our discussion"
- "List all our Confluence spaces"
- "Update the Release Notes page with new information"

## Troubleshooting

### "ConfluenceUrl is not configured"
Make sure your `appsettings.json` file exists and has the correct URL.

### "Authentication errors"
- Verify your API token is correct
- For Confluence Cloud, use your email as username if using basic auth
- Make sure the token hasn't expired

### "Page not found"
- Check that your user has permission to view/edit the content
- Verify the space key and page title are correct

## Next Steps

- Read the [full README](README.md) for detailed documentation
- Learn about [M365 Copilot integration](README.md#integration-with-microsoft-365-copilot)
- Understand [security best practices](README.md#security-considerations)

Need help? Check the main README or open an issue on GitHub!
