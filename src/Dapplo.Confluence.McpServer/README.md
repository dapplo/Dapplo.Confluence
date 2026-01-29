# Confluence MCP Server

A Model Context Protocol (MCP) server that provides Confluence integration for AI assistants like Claude Desktop and Microsoft 365 Copilot.

## Overview

This MCP server exposes Confluence functionality through a standardized protocol, allowing AI assistants to:
- Search for Confluence content using CQL (Confluence Query Language)
- Retrieve page content and metadata
- Create new pages
- Update existing pages
- List spaces
- Get user information

## Requirements

- .NET 10.0 or later
- Confluence Cloud or Server instance
- API token or credentials for authentication

## Installation

### Build from Source

```bash
cd src/Dapplo.Confluence.McpServer
dotnet build -c Release
```

### Publish Self-Contained

For cross-platform deployment:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

## Configuration

The server supports two authentication methods:

### Option 1: Bearer Token (Recommended)

1. Copy `appsettings.example.json` to `appsettings.json`
2. Configure via `appsettings.json`:

```json
{
  "ConfluenceUrl": "https://yourcompany.atlassian.net",
  "AuthType": "bearer",
  "BearerToken": "your-api-token-here"
}
```

### Option 2: Basic Authentication

```json
{
  "ConfluenceUrl": "https://yourcompany.atlassian.net",
  "AuthType": "basic",
  "Username": "your-email@example.com",
  "Password": "your-api-token"
}
```

### Environment Variables

You can also configure using environment variables (prefixed with `CONFLUENCE_`):

```bash
export CONFLUENCE_ConfluenceUrl="https://yourcompany.atlassian.net"
export CONFLUENCE_AuthType="bearer"
export CONFLUENCE_BearerToken="your-api-token"
```

## Getting a Confluence API Token

1. Log in to [https://id.atlassian.com/manage-profile/security/api-tokens](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click "Create API token"
3. Give it a name (e.g., "MCP Server")
4. Copy the token and use it as your `BearerToken`

## Available Tools

### confluence_search
Search for content using CQL.

**Parameters:**
- `query` (required): CQL query string (e.g., "type=page and space=DEV")
- `limit` (optional): Maximum results (default: 25)

**Example:**
```json
{
  "query": "type=page and title~'API' and space=DEV",
  "limit": 10
}
```

### confluence_get_page
Get a specific page by ID or title.

**Parameters:**
- `pageId`: Page ID (use either this or title+spaceKey)
- `title`: Page title
- `spaceKey`: Space key (required with title)
- `expand`: Properties to expand (default: "body.storage,version")

### confluence_create_page
Create a new page.

**Parameters:**
- `spaceKey` (required): Space key
- `title` (required): Page title
- `content` (required): HTML content in Confluence storage format
- `parentId` (optional): Parent page ID

### confluence_update_page
Update an existing page.

**Parameters:**
- `pageId` (required): Page ID
- `title` (required): New title
- `content` (required): New HTML content
- `version` (required): Current version number

### confluence_list_spaces
List accessible spaces.

**Parameters:**
- `limit` (optional): Maximum results (default: 25)

### confluence_get_user_info
Get current user information.

**Parameters:** None

## Integration with Claude Desktop

Add to your Claude Desktop configuration (`~/Library/Application Support/Claude/claude_desktop_config.json` on macOS):

```json
{
  "mcpServers": {
    "confluence": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/Dapplo.Confluence.McpServer/Dapplo.Confluence.McpServer.csproj"
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

Or using the published binary:

```json
{
  "mcpServers": {
    "confluence": {
      "command": "/path/to/published/Dapplo.Confluence.McpServer",
      "env": {
        "CONFLUENCE_ConfluenceUrl": "https://yourcompany.atlassian.net",
        "CONFLUENCE_AuthType": "bearer",
        "CONFLUENCE_BearerToken": "your-api-token"
      }
    }
  }
}
```

## Integration with Microsoft 365 Copilot

Microsoft 365 Copilot can integrate with MCP servers through the following approaches:

### Option 1: Through Teams AI Library

Use the Teams AI Library to create a bot that bridges MCP servers:

1. **Create a Teams Bot Application:**
   ```bash
   npm install @microsoft/teams-ai
   ```

2. **Configure Bot to Use MCP Server:**
   ```typescript
   import { Application } from '@microsoft/teams-ai';
   import { spawn } from 'child_process';
   
   // Start MCP server as child process
   const mcpServer = spawn('dotnet', [
     'run',
     '--project',
     '/path/to/Dapplo.Confluence.McpServer/Dapplo.Confluence.McpServer.csproj'
   ], {
     env: {
       CONFLUENCE_ConfluenceUrl: 'https://yourcompany.atlassian.net',
       CONFLUENCE_AuthType: 'bearer',
       CONFLUENCE_BearerToken: process.env.CONFLUENCE_TOKEN
     }
   });
   
   // Create Teams app with MCP integration
   const app = new Application({
     storage: new MemoryStorage()
   });
   
   // Register actions that call MCP tools
   app.ai.action('searchConfluence', async (context, state, parameters) => {
     // Send JSON-RPC request to MCP server
     const request = {
       jsonrpc: '2.0',
       id: 1,
       method: 'tools/call',
       params: {
         name: 'confluence_search',
         arguments: parameters
       }
     };
     
     // Write to MCP server stdin
     mcpServer.stdin.write(JSON.stringify(request) + '\n');
     
     // Read response from stdout
     // ... implementation details
   });
   ```

3. **Deploy to Azure:**
   Deploy your Teams bot to Azure App Service or Azure Functions.

4. **Configure in Teams Admin Center:**
   - Go to Teams Admin Center
   - Add your bot to the organization
   - Enable Copilot integration in the bot settings

### Option 2: Through Power Platform Connector

Create a custom connector in Power Platform:

1. **Wrap MCP Server in REST API:**
   Create an ASP.NET Core API that hosts the MCP server and exposes REST endpoints.

2. **Create Custom Connector:**
   - Go to Power Platform Admin Center
   - Create a new custom connector
   - Define OpenAPI spec for your Confluence operations
   - Point to your REST API wrapper

3. **Enable in Copilot Studio:**
   - Open Copilot Studio
   - Add your custom connector as a plugin
   - Configure authentication
   - Enable for M365 Copilot

### Option 3: Direct Plugin Integration (Future)

Microsoft is working on direct MCP support for M365 Copilot. When available:

1. **Register MCP Server:**
   ```json
   {
     "plugins": [
       {
         "type": "mcp",
         "name": "Confluence",
         "command": "dotnet",
         "args": ["run", "--project", "/path/to/Dapplo.Confluence.McpServer/..."],
         "env": { ... }
       }
     ]
   }
   ```

2. **Deploy to Organization:**
   Upload plugin configuration through M365 Admin Center.

### Recommended Approach for Production

For production M365 Copilot integration, we recommend:

1. **Host MCP Server as Azure Function or Container:**
   ```bash
   # Create Docker image
   docker build -t confluence-mcp-server .
   docker run -p 8080:8080 \
     -e CONFLUENCE_ConfluenceUrl="..." \
     -e CONFLUENCE_BearerToken="..." \
     confluence-mcp-server
   ```

2. **Create REST API Wrapper:**
   Build a thin REST API layer that translates HTTP requests to MCP JSON-RPC calls.

3. **Secure with Azure AD:**
   Use Azure Active Directory for authentication between M365 Copilot and your service.

4. **Monitor and Scale:**
   Use Azure Application Insights for monitoring and set up auto-scaling based on demand.

## Security Considerations

- **Never commit** API tokens or credentials to source control
- Use environment variables or Azure Key Vault for secrets in production
- Restrict API tokens to minimum required permissions
- Enable audit logging in Confluence to track API usage
- Consider implementing rate limiting for production deployments
- Use HTTPS for all Confluence connections

## Architecture

The MCP server follows a scalable, modular design:

```
┌─────────────────┐
│   AI Assistant  │ (Claude, M365 Copilot, etc.)
└────────┬────────┘
         │ JSON-RPC over stdio
         │
┌────────▼────────────┐
│  McpServerHandler   │ Processes MCP protocol requests
└────────┬────────────┘
         │
┌────────▼────────────┐
│   ToolRegistry      │ Manages available Confluence tools
└────────┬────────────┘
         │
┌────────▼────────────┐
│ IConfluenceClient   │ Dapplo.Confluence library
└────────┬────────────┘
         │
┌────────▼────────────┐
│  Confluence API     │ (REST API)
└─────────────────────┘
```

## Troubleshooting

### Server won't start
- Check that `ConfluenceUrl` is configured correctly
- Verify your API token is valid
- Ensure .NET 10.0 is installed

### Authentication errors
- Verify API token hasn't expired
- Check that you're using the correct authentication method
- For Confluence Cloud, use email address as username with API token as password

### No results from searches
- Verify CQL syntax (use Confluence's built-in CQL editor to test)
- Check that your user has permissions to view the content
- Try simpler queries first (e.g., just "type=page")

## Development

### Project Structure
```
Dapplo.Confluence.McpServer/
├── Models/           # MCP protocol models (Request, Response, Error)
├── Tools/            # Tool definitions and implementations
├── McpServerHandler.cs    # Main request handler
├── ConfluenceSettings.cs  # Configuration model
└── Program.cs       # Entry point and stdio handling
```

### Adding New Tools

1. Add tool definition to `ToolRegistry.GetToolDefinitions()`
2. Implement handler in `ToolRegistry.ExecuteToolAsync()`
3. Update this README with tool documentation

### Testing Manually

```bash
# Run the server
dotnet run

# Send test request (in another terminal)
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run

# Test search
echo '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"confluence_search","arguments":{"query":"type=page","limit":5}}}' | dotnet run
```

## License

This project follows the same license as Dapplo.Confluence (MIT License).

## Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Confluence REST API Documentation](https://developer.atlassian.com/cloud/confluence/rest/v2/intro/)
- [Dapplo.Confluence Library](https://github.com/dapplo/Dapplo.Confluence)
- [Microsoft Teams AI Library](https://github.com/microsoft/teams-ai)
- [Power Platform Custom Connectors](https://learn.microsoft.com/en-us/connectors/custom-connectors/)
