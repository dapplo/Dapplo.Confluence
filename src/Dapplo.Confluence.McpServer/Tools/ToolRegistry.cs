using Dapplo.Confluence.Query;
using Dapplo.Confluence.Entities;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Reflection;

namespace Dapplo.Confluence.McpServer.Tools;

/// <summary>
/// Registry of available Confluence tools for MCP
/// </summary>
public class ToolRegistry
{
    private readonly IConfluenceClient _confluenceClient;

    public ToolRegistry(IConfluenceClient confluenceClient)
    {
        _confluenceClient = confluenceClient;
    }

    /// <summary>
    /// Helper to create a Clause from a CQL string using reflection (since Clause is internal)
    /// </summary>
    private IFinalClause CreateClauseFromCql(string cql)
    {
        // Get the internal Clause type
        var clauseType = typeof(IConfluenceClient).Assembly.GetType("Dapplo.Confluence.Query.Clause");
        if (clauseType == null)
            throw new InvalidOperationException("Could not find Clause type");

        // Create instance using the string constructor
        var clause = Activator.CreateInstance(clauseType, new object[] { cql });
        return clause as IFinalClause ?? throw new InvalidOperationException("Failed to create Clause");
    }

    /// <summary>
    /// Get all available tool definitions
    /// </summary>
    public List<ToolDefinition> GetToolDefinitions()
    {
        return new List<ToolDefinition>
        {
            new()
            {
                Name = "confluence_search",
                Description = "Search for Confluence content using CQL (Confluence Query Language). Returns pages and blog posts matching the query.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        query = new
                        {
                            type = "string",
                            description = "CQL query string (e.g., 'type=page and space=DEV')"
                        },
                        limit = new
                        {
                            type = "integer",
                            description = "Maximum number of results to return (default: 25)",
                            @default = 25
                        }
                    },
                    required = new[] { "query" }
                }
            },
            new()
            {
                Name = "confluence_get_page",
                Description = "Get a specific Confluence page by ID or title. Returns the page content and metadata.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pageId = new
                        {
                            type = "string",
                            description = "The page ID (use either pageId or title)"
                        },
                        title = new
                        {
                            type = "string",
                            description = "The page title (use either pageId or title)"
                        },
                        spaceKey = new
                        {
                            type = "string",
                            description = "The space key (required when using title)"
                        },
                        expand = new
                        {
                            type = "string",
                            description = "Comma-separated list of properties to expand (e.g., 'body.storage,version,space')",
                            @default = "body.storage,version"
                        }
                    }
                }
            },
            new()
            {
                Name = "confluence_create_page",
                Description = "Create a new Confluence page in a specific space.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        spaceKey = new
                        {
                            type = "string",
                            description = "The space key where the page will be created"
                        },
                        title = new
                        {
                            type = "string",
                            description = "The page title"
                        },
                        content = new
                        {
                            type = "string",
                            description = "The page content in Confluence storage format (HTML)"
                        },
                        parentId = new
                        {
                            type = "string",
                            description = "Optional parent page ID"
                        }
                    },
                    required = new[] { "spaceKey", "title", "content" }
                }
            },
            new()
            {
                Name = "confluence_update_page",
                Description = "Update an existing Confluence page.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        pageId = new
                        {
                            type = "string",
                            description = "The ID of the page to update"
                        },
                        title = new
                        {
                            type = "string",
                            description = "The new page title"
                        },
                        content = new
                        {
                            type = "string",
                            description = "The new page content in Confluence storage format (HTML)"
                        },
                        version = new
                        {
                            type = "integer",
                            description = "Current version number (required for update)"
                        }
                    },
                    required = new[] { "pageId", "title", "content", "version" }
                }
            },
            new()
            {
                Name = "confluence_list_spaces",
                Description = "List all Confluence spaces accessible to the user.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        limit = new
                        {
                            type = "integer",
                            description = "Maximum number of spaces to return (default: 25)",
                            @default = 25
                        }
                    }
                }
            },
            new()
            {
                Name = "confluence_get_user_info",
                Description = "Get information about the current authenticated user.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        };
    }

    /// <summary>
    /// Execute a tool with the given parameters
    /// </summary>
    public async Task<object> ExecuteToolAsync(string toolName, JsonElement? parameters)
    {
        return toolName switch
        {
            "confluence_search" => await SearchContentAsync(parameters),
            "confluence_get_page" => await GetPageAsync(parameters),
            "confluence_create_page" => await CreatePageAsync(parameters),
            "confluence_update_page" => await UpdatePageAsync(parameters),
            "confluence_list_spaces" => await ListSpacesAsync(parameters),
            "confluence_get_user_info" => await GetUserInfoAsync(),
            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }

    private async Task<object> SearchContentAsync(JsonElement? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters required for search");

        var query = parameters.Value.GetProperty("query").GetString() 
            ?? throw new ArgumentException("Query is required");
        var limit = parameters.Value.TryGetProperty("limit", out var limitProp) 
            ? limitProp.GetInt32() : 25;

        // Create clause from CQL string
        var clause = CreateClauseFromCql(query);
        var pagingInfo = new PagingInformation { Limit = limit };
        var searchResult = await _confluenceClient.Content.SearchAsync(clause, pagingInformation: pagingInfo);

        return new
        {
            totalSize = searchResult.Size,
            results = searchResult.Results.Select(c => new
            {
                id = c.Id,
                type = c.Type.ToString(),
                title = c.Title,
                spaceKey = c.Space?.Key,
                spaceName = c.Space?.Name,
                url = c.Links?.Self,
                lastModified = c.Version?.When
            }).ToList()
        };
    }

    private async Task<object> GetPageAsync(JsonElement? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters required");

        var expand = parameters.Value.TryGetProperty("expand", out var expandProp) 
            ? expandProp.GetString()?.Split(',') : new[] { "body.storage", "version" };

        Content? page = null;

        if (parameters.Value.TryGetProperty("pageId", out var pageIdProp))
        {
            var pageIdStr = pageIdProp.GetString() 
                ?? throw new ArgumentException("Page ID cannot be null");
            if (!long.TryParse(pageIdStr, out var pageId))
                throw new ArgumentException("Page ID must be a valid number");
            
            page = await _confluenceClient.Content.GetAsync(pageId, expand);
        }
        else if (parameters.Value.TryGetProperty("title", out var titleProp) && 
                 parameters.Value.TryGetProperty("spaceKey", out var spaceKeyProp))
        {
            var title = titleProp.GetString() 
                ?? throw new ArgumentException("Title cannot be null");
            var spaceKey = spaceKeyProp.GetString() 
                ?? throw new ArgumentException("Space key cannot be null");
            
            var clause = Where.And(Where.Type.IsPage, Where.Title.Is(title), Where.Space.Is(spaceKey));
            var pagingInfo = new PagingInformation { Limit = 1 };
            var results = await _confluenceClient.Content.SearchAsync(clause, pagingInformation: pagingInfo);
            
            if (results.Results.Any())
            {
                page = await _confluenceClient.Content.GetAsync(results.Results.First().Id, expand);
            }
        }
        else
        {
            throw new ArgumentException("Either pageId or (title and spaceKey) must be provided");
        }

        if (page == null)
            throw new InvalidOperationException("Page not found");

        return new
        {
            id = page.Id,
            type = page.Type.ToString(),
            title = page.Title,
            spaceKey = page.Space?.Key,
            spaceName = page.Space?.Name,
            content = page.Body?.Storage?.Value,
            version = page.Version?.Number,
            lastModified = page.Version?.When,
            url = page.Links?.Self
        };
    }

    private async Task<object> CreatePageAsync(JsonElement? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters required");

        var spaceKey = parameters.Value.GetProperty("spaceKey").GetString() 
            ?? throw new ArgumentException("Space key is required");
        var title = parameters.Value.GetProperty("title").GetString() 
            ?? throw new ArgumentException("Title is required");
        var content = parameters.Value.GetProperty("content").GetString() 
            ?? throw new ArgumentException("Content is required");

        var newContent = new Content
        {
            Type = ContentTypes.Page,
            Title = title,
            Space = new Space { Key = spaceKey },
            Body = new Body
            {
                Storage = new BodyContent
                {
                    Value = content,
                    Representation = "storage"
                }
            }
        };

        if (parameters.Value.TryGetProperty("parentId", out var parentIdProp))
        {
            var parentIdStr = parentIdProp.GetString();
            if (!string.IsNullOrEmpty(parentIdStr) && long.TryParse(parentIdStr, out var parentId))
            {
                newContent.Ancestors = new List<Content> { new() { Id = parentId } };
            }
        }

        var createdPage = await _confluenceClient.Content.CreateAsync(newContent);

        return new
        {
            id = createdPage.Id,
            title = createdPage.Title,
            spaceKey = createdPage.Space?.Key,
            url = createdPage.Links?.Self,
            version = createdPage.Version?.Number
        };
    }

    private async Task<object> UpdatePageAsync(JsonElement? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters required");

        var pageIdStr = parameters.Value.GetProperty("pageId").GetString() 
            ?? throw new ArgumentException("Page ID is required");
        if (!long.TryParse(pageIdStr, out var pageId))
            throw new ArgumentException("Page ID must be a valid number");
        
        var title = parameters.Value.GetProperty("title").GetString() 
            ?? throw new ArgumentException("Title is required");
        var content = parameters.Value.GetProperty("content").GetString() 
            ?? throw new ArgumentException("Content is required");
        var version = parameters.Value.GetProperty("version").GetInt32();

        var updateContent = new Content
        {
            Id = pageId,
            Type = ContentTypes.Page,
            Title = title,
            Version = new Entities.Version { Number = version + 1 },
            Body = new Body
            {
                Storage = new BodyContent
                {
                    Value = content,
                    Representation = "storage"
                }
            }
        };

        var updatedPage = await _confluenceClient.Content.UpdateAsync(updateContent);

        return new
        {
            id = updatedPage.Id,
            title = updatedPage.Title,
            version = updatedPage.Version?.Number,
            url = updatedPage.Links?.Self
        };
    }

    private async Task<object> ListSpacesAsync(JsonElement? parameters)
    {
        var limit = parameters?.TryGetProperty("limit", out var limitProp) == true 
            ? limitProp.GetInt32() : 25;

        var pagingInfo = new PagingInformation { Limit = limit };
        var spaces = await _confluenceClient.Space.GetAllWithParametersAsync(pagingInformation: pagingInfo);

        return new
        {
            totalSize = spaces.Count,
            spaces = spaces.Select(s => new
            {
                key = s.Key,
                name = s.Name,
                type = s.Type,
                url = s.Links?.Self
            }).ToList()
        };
    }

    private async Task<object> GetUserInfoAsync()
    {
        var user = await _confluenceClient.User.GetCurrentUserAsync();

        return new
        {
            username = user.Username,
            displayName = user.DisplayName,
            email = user.Email,
            profilePicture = user.ProfilePicture?.Path
        };
    }
}
