# Dapplo.Confluence
This is a simple REST based Confluence client, written for Greenshot, by using Dapplo.HttpExtension

- Current build status: [![Build Status](https://dev.azure.com/Dapplo/Dapplo%20framework/_apis/build/status/dapplo.Dapplo.Confluence?branchName=master)](https://dev.azure.com/Dapplo/Dapplo%20framework/_build/latest?definitionId=11&branchName=master)
- Coverage Status: [![Coverage Status](https://coveralls.io/repos/github/dapplo/Dapplo.Confluence/badge.svg?branch=master)](https://coveralls.io/github/dapplo/Dapplo.Confluence?branch=master)
- NuGet package: [![NuGet package](https://badge.fury.io/nu/Dapplo.Confluence.svg)](https://badge.fury.io/nu/Dapplo.Confluence)

## 🆕 MCP Server for AI Integration

**NEW**: Integrate Confluence with AI assistants like Claude Desktop and Microsoft 365 Copilot!

The [Dapplo.Confluence.McpServer](src/Dapplo.Confluence.McpServer) provides a Model Context Protocol (MCP) server that exposes Confluence functionality to AI assistants. 

**Quick start:**
```bash
cd src/Dapplo.Confluence.McpServer
cp appsettings.example.json appsettings.json
# Edit appsettings.json with your Confluence URL and API token
dotnet run
```

See the [Quick Start Guide](src/Dapplo.Confluence.McpServer/QUICKSTART.md) and [full documentation](src/Dapplo.Confluence.McpServer/README.md) for details.

## Confluence Client Library

The Confluence client supports most REST methods, and has a fluent API for building a CQL (Confluence Query Language) string to search with.

An example on how to use this Confluence client:
```
var confluenceClient = ConfluenceClient.Create(new Uri("https://confluence"));
confluenceClient.SetBasicAuthentication(username, password);
var query = Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home"));
var searchResult = await confluenceClient.Content.SearchAsync(query, limit:1);
foreach (var contentDigest in searchResult.Results)
{
	// As the content from the Search is a digest, get the details (it's also possible to get the details during the search)
	var content = await confluenceClient.Content.GetAsync(contentDigest, ConfluenceClientConfig.ExpandGetContentWithStorage);
	// Output the information
	Debug.WriteLine(content.Body);
}
```

If you want to extend the API for a specific use-case where it doesn't make sense to provide it to the rest of the world via a pull-request, for example to add logic for a *plugin*, you can write an extension method to extend the IConfluenceClientPlugins.
Your "plugin" extension will now be available, if the developer has a using statement of your namespace, on the .Plugins property of the IConfluenceClient

*Hint:*
For Confluence Cloud, the username & password are a bit different than for Confluence Server.
For cloud your username is the email-address you use to login to atlassian and the PW is an API token which needs to be generated here: https://id.atlassian.com/manage/api-tokens

Also the URL to use for Confluence cloud is most likely something like the following (where {domain} is your domain): https://{domain}.atlassian.net/wiki
