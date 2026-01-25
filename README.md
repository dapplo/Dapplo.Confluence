# Dapplo.Confluence

[![Build Status](https://dev.azure.com/Dapplo/Dapplo%20framework/_apis/build/status/dapplo.Dapplo.Confluence?branchName=master)](https://dev.azure.com/Dapplo/Dapplo%20framework/_build/latest?definitionId=11&branchName=master)
[![Coverage Status](https://coveralls.io/repos/github/dapplo/Dapplo.Confluence/badge.svg?branch=master)](https://coveralls.io/github/dapplo/Dapplo.Confluence?branch=master)
[![NuGet package](https://badge.fury.io/nu/Dapplo.Confluence.svg)](https://badge.fury.io/nu/Dapplo.Confluence)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

**Dapplo.Confluence** is a .NET client library for interacting with Atlassian Confluence via its REST API. Originally developed for [Greenshot](https://getgreenshot.org/), this library provides a clean, intuitive interface for programmatically managing Confluence content, spaces, users, attachments, and more.

Built on top of [Dapplo.HttpExtensions](https://github.com/dapplo/Dapplo.HttpExtensions), it offers a fluent API for building CQL (Confluence Query Language) queries and supports both Confluence Server and Confluence Cloud deployments.

## Key Features

- **Full REST API Support**: Interact with Content, Spaces, Users, Attachments, Groups, and more
- **Fluent CQL Query Builder**: Build type-safe Confluence Query Language queries with IntelliSense support
- **Multiple Authentication Methods**: Basic Authentication, Bearer Token (PAT), and OAuth support
- **Multi-Target Framework Support**: Works with .NET Standard 1.3/2.0, .NET Framework 4.7.1, .NET Core 3.1, and .NET 6.0
- **Async/Await Pattern**: All API operations are fully asynchronous
- **Extensible Plugin System**: Easily extend the client with custom functionality
- **Cloud & Server Compatible**: Works with both Confluence Cloud and Confluence Server/Data Center

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Dapplo.Confluence
```

Or via the Package Manager Console:

```powershell
Install-Package Dapplo.Confluence
```

For OAuth authentication support, also install:

```bash
dotnet add package Dapplo.Confluence.OAuth
```

## Prerequisites

- .NET Standard 1.3+ compatible runtime, or
- .NET Framework 4.7.1+, or
- .NET Core 3.1+, or
- .NET 6.0+

## Quick Start

### Creating a Client

```csharp
using Dapplo.Confluence;

// Create a client instance
var confluenceClient = ConfluenceClient.Create(new Uri("https://your-confluence-url"));
```

### Authentication

**Basic Authentication (Confluence Server):**
```csharp
confluenceClient.SetBasicAuthentication("username", "password");
```

**Basic Authentication (Confluence Cloud):**
```csharp
// For Confluence Cloud, use your email as username and an API token as password
// Generate API token at: https://id.atlassian.com/manage/api-tokens
confluenceClient.SetBasicAuthentication("your-email@example.com", "your-api-token");
```

**Bearer Token / Personal Access Token:**
```csharp
confluenceClient.SetBearerAuthentication("your-personal-access-token");
```

### Basic Usage Example

```csharp
using Dapplo.Confluence;
using Dapplo.Confluence.Query;

// Create and authenticate client
var confluenceClient = ConfluenceClient.Create(new Uri("https://your-domain.atlassian.net/wiki"));
confluenceClient.SetBasicAuthentication("email@example.com", "api-token");

// Build a CQL query
var query = Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home"));

// Search for content
var searchResult = await confluenceClient.Content.SearchAsync(query, limit: 10);

foreach (var contentDigest in searchResult.Results)
{
    // Get full content details with body storage format
    var content = await confluenceClient.Content.GetAsync(
        contentDigest, 
        ConfluenceClientConfig.ExpandGetContentWithStorage
    );
    
    Console.WriteLine($"Title: {content.Title}");
    Console.WriteLine($"Body: {content.Body?.Storage?.Value}");
}
```

## API Domains

The client is organized into logical domains for different Confluence operations:

### Content Domain (`confluenceClient.Content`)
- Create, read, update, and delete pages and blog posts
- Search content using CQL queries
- Manage content labels
- Get content history and children
- Copy and move content (Cloud only)

```csharp
// Create a new page
var page = await confluenceClient.Content.CreateAsync(
    ContentTypes.Page, 
    "Page Title", 
    "SPACEKEY", 
    "<p>Page content in HTML</p>"
);

// Get content by ID
var content = await confluenceClient.Content.GetAsync(contentId);

// Update existing content
content.Body.Storage.Value = "<p>Updated content</p>";
// Confluence requires incrementing the version number for updates to prevent conflicts
content.Version.Number++;
await confluenceClient.Content.UpdateAsync(content);

// Delete content
await confluenceClient.Content.DeleteAsync(contentId);
```

### Space Domain (`confluenceClient.Space`)
- Get space information and list spaces
- Access space contents

```csharp
// Get all spaces
var spaces = await confluenceClient.Space.GetAllAsync();

// Get a specific space
var space = await confluenceClient.Space.GetAsync("SPACEKEY");
```

### User Domain (`confluenceClient.User`)
- Get user information
- Search for users

```csharp
// Get current user
var currentUser = await confluenceClient.User.GetCurrentUserAsync();

// Get user by username
var user = await confluenceClient.User.GetAsync("username");
```

### Attachment Domain (`confluenceClient.Attachment`)
- Upload, download, and manage attachments

```csharp
// Get attachments for content
var attachments = await confluenceClient.Attachment.GetAttachmentsAsync(contentId);

// Download attachment content
var attachmentData = await confluenceClient.Attachment.GetContentAsync<byte[]>(attachment);
```

### Group Domain (`confluenceClient.Group`)
- Get group information and members

```csharp
// Get all groups
var groups = await confluenceClient.Group.GetGroupsAsync();

// Get group members
var members = await confluenceClient.Group.GetMembersAsync("group-name");
```

### Misc Domain (`confluenceClient.Misc`)
- Get system information
- Access other utility endpoints

```csharp
// Get system information
var systemInfo = await confluenceClient.Misc.GetSystemInfoAsync();

// Check if connected to cloud server
var isCloud = await confluenceClient.IsCloudServer();
```

## CQL Query Builder

The library includes a fluent API for building CQL (Confluence Query Language) queries:

```csharp
using Dapplo.Confluence.Query;

// Simple queries
var pageQuery = Where.Type.IsPage;
var blogQuery = Where.Type.IsBlogpost;

// Combined queries with AND
var query = Where.And(
    Where.Space.Is("DEV"),
    Where.Type.IsPage,
    Where.Title.Contains("documentation")
);

// Combined queries with OR
var orQuery = Where.Or(
    Where.Creator.Is("john.doe"),
    Where.Contributor.Is("john.doe")
);

// Date-based queries
var recentQuery = Where.LastModified.After(DateTime.Now.AddDays(-7));
var createdQuery = Where.Created.Before(DateTime.Now.AddMonths(-1));

// Label queries
var labelQuery = Where.Label.Is("important");

// Text search
var textQuery = Where.Text.Contains("API documentation");

// Complex nested queries
var complexQuery = Where.And(
    Where.Space.Is("DEV"),
    Where.Type.IsPage,
    Where.Or(
        Where.Title.Contains("guide"),
        Where.Label.Is("tutorial")
    )
);
```

### Available Query Fields

| Field | Description | Example |
|-------|-------------|---------|
| `Where.Type` | Content type (page, blogpost, attachment, comment) | `Where.Type.IsPage` |
| `Where.Space` | Space key | `Where.Space.Is("DEV")` |
| `Where.Title` | Content title | `Where.Title.Contains("guide")` |
| `Where.Text` | Full-text search | `Where.Text.Contains("search term")` |
| `Where.Label` | Content labels | `Where.Label.Is("important")` |
| `Where.Creator` | Content creator | `Where.Creator.Is("username")` |
| `Where.Contributor` | Content contributors | `Where.Contributor.Is("username")` |
| `Where.Created` | Creation date | `Where.Created.After(date)` |
| `Where.LastModified` | Last modified date | `Where.LastModified.Before(date)` |
| `Where.Id` | Content ID | `Where.Id.Is(12345)` |
| `Where.Ancestor` | Ancestor content ID | `Where.Ancestor.Is(parentId)` |
| `Where.Parent` | Parent content ID | `Where.Parent.Is(parentId)` |

## Extending the Client (Plugin System)

You can extend the Confluence client with custom functionality by creating extension methods on `IConfluenceClientPlugins`:

```csharp
public static class MyConfluenceExtensions
{
    public static async Task<string> MyCustomOperationAsync(
        this IConfluenceClientPlugins plugins, 
        string parameter)
    {
        // Your custom implementation
        // Access the client via plugins as IConfluenceDomain
        var confluenceClient = plugins as IConfluenceDomain;
        
        // Make custom API calls using the client's behavior
        confluenceClient.Behaviour.MakeCurrent();
        
        // ... your custom logic
        
        return result;
    }
}
```

Usage:
```csharp
// Use your extension via the Plugins property
var result = await confluenceClient.Plugins.MyCustomOperationAsync("param");
```

## Confluence Cloud vs Server

### URL Differences

| Type | URL Format |
|------|------------|
| **Server/Data Center** | `https://confluence.yourcompany.com` |
| **Cloud** | `https://your-domain.atlassian.net/wiki` |

### Authentication Differences

| Type | Username | Password/Token |
|------|----------|----------------|
| **Server** | Your username | Your password |
| **Cloud** | Your email address | API token from [Atlassian account](https://id.atlassian.com/manage/api-tokens) |

### Feature Differences

Some features are only available on Confluence Cloud:
- `Content.MoveAsync()` - Move content to a different location
- `Content.CopyAsync()` - Copy content

You can check if you're connected to a cloud instance:
```csharp
var isCloud = await confluenceClient.IsCloudServer();
```

## Configuration

You can customize the default expand parameters for various operations:

```csharp
// Configure what fields to expand when getting content
ConfluenceClientConfig.ExpandGetContent = new[] { 
    "version", 
    "body.storage", 
    "ancestors" 
};

// Configure expand for search results
ConfluenceClientConfig.ExpandSearch = new[] { 
    "version", 
    "space", 
    "history.lastUpdated" 
};

// Use predefined configurations
var content = await confluenceClient.Content.GetAsync(
    contentId, 
    ConfluenceClientConfig.ExpandGetContentWithStorage
);
```

## Error Handling

The library throws `ConfluenceException` for API errors:

```csharp
try
{
    var content = await confluenceClient.Content.GetAsync(invalidId);
}
catch (ConfluenceException ex)
{
    Console.WriteLine($"Confluence error: {ex.Message}");
    // Access detailed error information if available
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Building from Source

```bash
# Clone the repository
git clone https://github.com/dapplo/Dapplo.Confluence.git
cd Dapplo.Confluence

# Restore dependencies and build
dotnet restore src/Dapplo.Confluence.sln
dotnet build src/Dapplo.Confluence.sln --configuration Release

# Run tests (requires Confluence test instance configuration)
dotnet test src/Dapplo.Confluence.sln
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Originally developed for [Greenshot](https://getgreenshot.org/)
- Built on [Dapplo.HttpExtensions](https://github.com/dapplo/Dapplo.HttpExtensions)

## Related Projects

- [Dapplo.Jira](https://github.com/dapplo/Dapplo.Jira) - Similar client library for Atlassian Jira
- [Dapplo.HttpExtensions](https://github.com/dapplo/Dapplo.HttpExtensions) - HTTP client extensions library
