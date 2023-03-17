// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Confluence;

/// <summary>
///     The is the marker interface to the content functionality of the Confluence API
/// </summary>
public interface IContentDomain : IConfluenceDomain
{
}

/// <summary>
///     The is the implementation to the content functionality of the Confluence API
/// </summary>
public static class ContentExtensions
{
    /// <summary>
    ///     Create content
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentType">Type of content, usually page</param>
    /// <param name="title">Title for the content</param>
    /// <param name="spaceKey">Key of the space to add the content to</param>
    /// <param name="body">the complete body (HTML)</param>
    /// <param name="ancestorId">Optional ID for the ancestor (parent)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static Task<Content> CreateAsync(this IContentDomain confluenceClient, ContentTypes contentType, string title, string spaceKey, string body, long? ancestorId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        if (string.IsNullOrEmpty(spaceKey)) throw new ArgumentNullException(nameof(spaceKey));
        if (string.IsNullOrEmpty(body)) throw new ArgumentNullException(nameof(body));
        var contentBody = new Body
        {
            Storage = new BodyContent
            {
                Value = body,
                Representation = "storage"
            }
        };
        return confluenceClient.CreateAsync(contentType, title, spaceKey, contentBody, ancestorId, cancellationToken);
    }

    /// <summary>
    ///     Create content
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentType">Type of content, usually page</param>
    /// <param name="title">Title for the content</param>
    /// <param name="spaceKey">Key of the space to add the content to</param>
    /// <param name="body">Body</param>
    /// <param name="ancestorId">Optional ID for the ancestor (parent)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static Task<Content> CreateAsync(this IContentDomain confluenceClient, ContentTypes contentType, string title, string spaceKey, Body body, long? ancestorId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        if (string.IsNullOrEmpty(spaceKey)) throw new ArgumentNullException(nameof(spaceKey));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var content = new Content
        {
            Type = contentType,
            Title = title,
            Space = new Space
            {
                Key = spaceKey
            },
            Body = body,
            Ancestors = !ancestorId.HasValue ? null : new List<Content>
            {
                new Content
                {
                    Id = ancestorId.Value
                }
            }
        };
        return confluenceClient.CreateAsync(content, cancellationToken);
    }

    /// <summary>
    ///     Create content
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="content">Content (e.g. Page) to create</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static async Task<Content> CreateAsync(this IContentDomain confluenceClient, Content content, CancellationToken cancellationToken = default)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content");

        confluenceClient.Behaviour.MakeCurrent();
        var response = await contentUri.PostAsync<HttpResponse<Content, Error>>(content, cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Delete content (attachments are also content)
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">ID for the content which needs to be deleted</param>
    /// <param name="isTrashed">If the content is trash-able, you will need to call DeleteAsync twice, second time with isTrashed = true</param>
    /// <param name="cancellationToken">CancellationToken</param>
    public static async Task DeleteAsync(this IContentDomain confluenceClient, long contentId, bool isTrashed = false, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId);

        if (isTrashed)
        {
            contentUri = contentUri.ExtendQuery("status", "trashed");
        }
        confluenceClient.Behaviour.MakeCurrent();

        var response = await contentUri.DeleteAsync<HttpResponse>(cancellationToken).ConfigureAwait(false);
        response.HandleStatusCode(isTrashed ? HttpStatusCode.OK : HttpStatusCode.NoContent);
    }

    /// <summary>
    ///     Get Content information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id (as content implements an implicit cast, you can also pass the content instance)</param>
    /// <param name="expandGetContent">Specify the expand values, if null the default from the configuration is used</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static async Task<Content> GetAsync(this IContentDomain confluenceClient, long contentId, IEnumerable<string> expandGetContent = null, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId);

        var expand = string.Join(",", expandGetContent ?? ConfluenceClientConfig.ExpandGetContent ?? Enumerable.Empty<string>());
        if (!string.IsNullOrEmpty(expand))
        {
            contentUri = contentUri.ExtendQuery("expand", expand);
        }

        confluenceClient.Behaviour.MakeCurrent();

        var response = await contentUri.GetAsAsync<HttpResponse<Content, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Get content by title
    ///     See: https://docs.atlassian.com/confluence/REST/latest/#d2e4539
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="spaceKey">Space key</param>
    /// <param name="title">Title of the content</param>
    /// <param name="pagingInformation">PagingInformation used for paging</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Results with content items</returns>
    public static async Task<Result<Content>> GetByTitleAsync(this IContentDomain confluenceClient, string spaceKey, string title, PagingInformation pagingInformation = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        if (string.IsNullOrEmpty(spaceKey)) throw new ArgumentNullException(nameof(spaceKey));

        confluenceClient.Behaviour.MakeCurrent();
        pagingInformation ??= new PagingInformation
        {
            Limit = 200,
            Start = 0
        };
        var searchUri = confluenceClient.ConfluenceApiUri.AppendSegments("content").ExtendQuery(new Dictionary<string, object>
        {
            {
                "start", pagingInformation.Start
            },
            {
                "limit", pagingInformation.Limit
            },
            {
                "type", "page"
            },
            {
                "spaceKey", spaceKey
            },
            {
                "title", title
            }
        });

        var expand = string.Join(",", ConfluenceClientConfig.ExpandGetContentByTitle ?? Enumerable.Empty<string>());
        if (!string.IsNullOrEmpty(expand))
        {
            searchUri = searchUri.ExtendQuery("expand", expand);
        }

        var response = await searchUri.GetAsAsync<HttpResponse<Result<Content>, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Get Content information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <param name="pagingInformation">PagingInformation</param>
    /// <param name="parentVersion">int representing the version of the content to retrieve children for.</param>
    /// <returns>List with Content</returns>
    public static async Task<Result<Content>> GetChildrenAsync(this IContentDomain confluenceClient, long contentId, PagingInformation pagingInformation = null, int? parentVersion = null, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "child", "page");

        if (pagingInformation?.Start != null)
        {
            contentUri = contentUri.ExtendQuery("start", pagingInformation.Start);
        }

        if (pagingInformation?.Limit != null)
        {
            contentUri = contentUri.ExtendQuery("limit", pagingInformation.Limit);
        }

        if (parentVersion.HasValue)
        {
            contentUri = contentUri.ExtendQuery("parentVersion", parentVersion);
        }

        var expand = string.Join(",", ConfluenceClientConfig.ExpandGetChildren ?? Enumerable.Empty<string>());
        if (!string.IsNullOrEmpty(expand))
        {
            contentUri = contentUri.ExtendQuery("expand", expand);
        }
        confluenceClient.Behaviour.MakeCurrent();

        var response = await contentUri.GetAsAsync<HttpResponse<Result<Content>, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Get Content History information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static async Task<History> GetHistoryAsync(this IContentDomain confluenceClient, long contentId, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        var historyUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "history");

        confluenceClient.Behaviour.MakeCurrent();

        var response = await historyUri.GetAsAsync<HttpResponse<History, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Possible since 5.7
    ///     Search for issues, with a CQL (e.g. from a filter) see
    ///     <a href="https://docs.atlassian.com/confluence/REST/latest/#d2e4539">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="cqlClause">Confluence Query Language, like SQL, for the search</param>
    /// <param name="cqlContext">
    ///     the execution context for CQL functions, provides current space key and content id. If this is
    ///     not provided some CQL functions will not be available.
    /// </param>
    /// <param name="cursor">Cursor identifier to get the next pages of the results</param>
    /// <param name="pagingInformation">PagingInformation</param>
    /// <param name="expandSearch">The expand value for the search, when null the value from the ConfluenceClientConfig.ExpandSearch is taken</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result with content items</returns>
    public static Task<CursorBasedResult<Content>> SearchAsync(this IContentDomain confluenceClient, IFinalClause cqlClause, string cqlContext = null, string cursor = null, PagingInformation pagingInformation = null, IEnumerable<string> expandSearch = null,
        CancellationToken cancellationToken = default)
    {
        var searchDetails = new SearchDetails(cqlClause)
        {
            Start = pagingInformation?.Start,
            Limit = pagingInformation?.Limit
        };

        if (cqlContext != null)
        {
            searchDetails.CqlContext = cqlContext;
        }
        if (expandSearch != null)
        {
            searchDetails.ExpandSearch = expandSearch;
        }
        if (cursor != null)
        {
            searchDetails.Cursor = cursor;
        }

        return confluenceClient.SearchAsync(searchDetails, cancellationToken);
    }

    /// <summary>
    ///     Possible since 5.7
    ///     Search for issues, with a CQL (e.g. from a filter) see
    ///     <a href="https://docs.atlassian.com/confluence/REST/latest/#d2e4539">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="searchDetails">All the details needed for a search</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result with content items</returns>
    public static async Task<CursorBasedResult<Content>> SearchAsync(this IContentDomain confluenceClient, SearchDetails searchDetails, CancellationToken cancellationToken = default)
    {
        if (searchDetails == null) throw new ArgumentNullException(nameof(searchDetails));

        confluenceClient.Behaviour.MakeCurrent();

        var searchUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", "search").ExtendQuery("cql", searchDetails.Cql);

        if (searchDetails.Limit.HasValue)
        {
            searchUri = searchUri.ExtendQuery("limit", searchDetails.Limit);
        }
        if (searchDetails.Start.HasValue)
        {
            searchUri = searchUri.ExtendQuery("start", searchDetails.Start);
        }
        if (!string.IsNullOrEmpty(searchDetails.Cursor))
        {
            searchUri = searchUri.ExtendQuery("cursor", searchDetails.Cursor);
            searchUri = searchUri.ExtendQuery("next", "true");
        }


        var expand = string.Join(",", searchDetails.ExpandSearch ?? ConfluenceClientConfig.ExpandSearch ?? Enumerable.Empty<string>());
        if (!string.IsNullOrEmpty(expand))
        {
            searchUri = searchUri.ExtendQuery("expand", expand);
        }

        if (searchDetails.CqlContext != null)
        {
            searchUri = searchUri.ExtendQuery("cqlcontext", searchDetails.CqlContext);
        }

        var response = await searchUri.GetAsAsync<HttpResponse<CursorBasedResult<Content>, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Move the content specified by the contentId to the position relative to the targetContentId.
    ///     Documentation for this can be found <a href="https://developer.atlassian.com/cloud/confluence/rest/api-group-content---children-and-descendants/#api-wiki-rest-api-content-pageid-move-position-targetid-put">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">long with the content ID to move</param>
    /// <param name="position">Positions with position describing how to move to the targetContentId. Check the enum documentation for the possibilities and their effect.</param>
    /// <param name="targetContentId">long with the target content ID, that is where the content needs to be moved to using the position for the relation.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>string with the contentId</returns>
    public static async Task<string> MoveAsync(this IContentDomain confluenceClient, long contentId, Positions position, long targetContentId, CancellationToken cancellationToken = default)
    {
        if (!await confluenceClient.IsCloudServer(cancellationToken))
        {
            throw new NotSupportedException("The content move operation is not supported on Confluence server, you need Confluence cloud for this.");
        }
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "move", position.ToString().ToLowerInvariant(), targetContentId);

        confluenceClient.Behaviour.MakeCurrent();
        var response = await contentUri.PutAsync<HttpResponse<string, Error>>(null, cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Copy the content specified by the contentId using the information from the CopyContent
    ///     Documentation for this can be found <a href="https://developer.atlassian.com/cloud/confluence/rest/api-group-content---children-and-descendants/#api-wiki-rest-api-content-id-copy-post">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">long with the Content ID to copy</param>
    /// <param name="copyContent">CopyContent describing how and where to copy the specified content</param>
    /// <param name="expandCopy">strings with the optional expand values</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static async Task<Content> CopyAsync(this IContentDomain confluenceClient, long contentId, CopyContent copyContent, IEnumerable<string> expandCopy = null, CancellationToken cancellationToken = default)
    {
        if (!await confluenceClient.IsCloudServer(cancellationToken))
        {
            throw new NotSupportedException("The content copy operation is not supported on Confluence server, you need Confluence cloud for this.");
        }
        if (copyContent is null) throw new ArgumentNullException(nameof(copyContent));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "copy");

        var expand = string.Join(",", expandCopy ?? Enumerable.Empty<string>());
        if (!string.IsNullOrEmpty(expand))
        {
            contentUri = contentUri.ExtendQuery("expand", expand);
        }

        confluenceClient.Behaviour.MakeCurrent();
        var response = await contentUri.PostAsync<HttpResponse<Content, Error>>(copyContent, cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Update content
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="content">Content to update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Content</returns>
    public static async Task<Content> UpdateAsync(this IContentDomain confluenceClient, Content content, CancellationToken cancellationToken = default)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", content.Id);

        confluenceClient.Behaviour.MakeCurrent();
        var response = await contentUri.PutAsync<HttpResponse<Content, Error>>(content, cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Get Labels for content see <a href="https://docs.atlassian.com/confluence/REST/latest/#content/{id}/label">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result with labels</returns>
    public static async Task<Result<Label>> GetLabelsAsync(this IContentDomain confluenceClient, long contentId, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        var labelUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "label");
        confluenceClient.Behaviour.MakeCurrent();

        var response = await labelUri.GetAsAsync<HttpResponse<Result<Label>, Error>>(cancellationToken).ConfigureAwait(false);
        return response.HandleErrors();
    }

    /// <summary>
    ///     Add Labels to content see <a href="https://docs.atlassian.com/confluence/REST/latest/#content/{id}/label">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id</param>
    /// <param name="labels">IEnumerable labels</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task AddLabelsAsync(this IContentDomain confluenceClient, long contentId, IEnumerable<Label> labels, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        if (labels == null || !labels.Any()) throw new ArgumentNullException(nameof(labels));
        var labelUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "label");
        confluenceClient.Behaviour.MakeCurrent();

        var response = await labelUri.PostAsync<HttpResponseWithError<Error>>(labels, cancellationToken).ConfigureAwait(false);
        response.HandleStatusCode();
    }

    /// <summary>
    ///     Delete Label for content see <a href="https://docs.atlassian.com/confluence/REST/latest/#content/{id}/label">here</a>
    /// </summary>
    /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
    /// <param name="contentId">content id</param>
    /// <param name="label">Name of label</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Task</returns>
    public static async Task DeleteLabelAsync(this IContentDomain confluenceClient, long contentId, string label, CancellationToken cancellationToken = default)
    {
        if (contentId == 0) throw new ArgumentNullException(nameof(contentId));
        if (string.IsNullOrEmpty(label)) throw new ArgumentNullException(nameof(label));

        var labelUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "label", label);
        confluenceClient.Behaviour.MakeCurrent();

        var response = await labelUri.DeleteAsync<HttpResponse>(cancellationToken).ConfigureAwait(false);
        response.HandleStatusCode(HttpStatusCode.NoContent);
    }
}