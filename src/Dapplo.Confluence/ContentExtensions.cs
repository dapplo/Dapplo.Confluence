// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Internals;
using Dapplo.Confluence.Query;
using Dapplo.HttpExtensions;

namespace Dapplo.Confluence
{
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
        /// <param name="isTrashed">If the content is trashable, you will need to call DeleteAsyc twice, second time with isTrashed = true</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public static async Task DeleteAsync(this IContentDomain confluenceClient, long contentId, bool isTrashed = false, CancellationToken cancellationToken = default)
        {
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
        /// <param name="start">Start of the results, used for paging</param>
        /// <param name="limit">Maximum number of results returned, default is 20</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Results with content items</returns>
        public static async Task<Result<Content>> GetByTitleAsync(this IContentDomain confluenceClient, string spaceKey, string title, int start = 0, int limit = 20, CancellationToken cancellationToken = default)
        {
            confluenceClient.Behaviour.MakeCurrent();

            var searchUri = confluenceClient.ConfluenceApiUri.AppendSegments("content").ExtendQuery(new Dictionary<string, object>
            {
                {
                    "start", start
                },
                {
                    "limit", limit
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
        /// <param name="start">int specifying where to start, used for paging.</param>
        /// <param name="limit">int used to limit the amount of results, used for paging.</param>
        /// <param name="parentVersion">int representing the version of the content to retrieve children for.</param>
        /// <returns>List with Content</returns>
        public static async Task<Result<Content>> GetChildrenAsync(this IContentDomain confluenceClient, long contentId, int? start = null, int? limit = null, int? parentVersion = null, CancellationToken cancellationToken = default)
        {
            var contentUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "child", "page");

            if (start.HasValue)
            {
                contentUri = contentUri.ExtendQuery("start", start);
            }

            if (limit.HasValue)
            {
                contentUri = contentUri.ExtendQuery("limit", limit);
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
        /// <param name="cqlClause">Confluence Query Language clause, created via Where</param>
        /// <param name="cqlContext">
        ///     the execution context for CQL functions, provides current space key and content id. If this is
        ///     not provided some CQL functions will not be available.
        /// </param>
        /// <param name="limit">Maximum number of results returned, default is 20</param>
        /// <param name="expandSearch">The expand value for the search, when null the value from the ConfluenceClientConfig.ExpandSearch is taken</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result with content items</returns>
        public static Task<Result<Content>> SearchAsync(this IContentDomain confluenceClient, IFinalClause cqlClause, string cqlContext = null, int start = 0, int limit = 20, IEnumerable<string> expandSearch = null, CancellationToken cancellationToken = default)
        {
            return confluenceClient.SearchAsync(cqlClause.ToString(), cqlContext, start, limit, expandSearch, cancellationToken);
        }

        /// <summary>
        ///     Possible since 5.7
        ///     Search for issues, with a CQL (e.g. from a filter) see
        ///     <a href="https://docs.atlassian.com/confluence/REST/latest/#d2e4539">here</a>
        /// </summary>
        /// <param name="confluenceClient">IContentDomain to bind the extension method to</param>
        /// <param name="cql">Confluence Query Language, like SQL, for the search</param>
        /// <param name="cqlContext">
        ///     the execution context for CQL functions, provides current space key and content id. If this is
        ///     not provided some CQL functions will not be available.
        /// </param>
        /// <param name="limit">Maximum number of results returned, default is 20</param>
        /// <param name="expandSearch">The expand value for the search, when null the value from the ConfluenceClientConfig.ExpandSearch is taken</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Result with content items</returns>
        public static async Task<Result<Content>> SearchAsync(this IContentDomain confluenceClient, string cql, string cqlContext = null, int start = 0, int limit = 20, IEnumerable<string> expandSearch = null, CancellationToken cancellationToken = default)
        {
            confluenceClient.Behaviour.MakeCurrent();

            var searchUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", "search").ExtendQuery("cql", cql).ExtendQuery("limit", limit).ExtendQuery("start", start);

            var expand = string.Join(",", expandSearch ?? ConfluenceClientConfig.ExpandSearch ?? Enumerable.Empty<string>());
            if (!string.IsNullOrEmpty(expand))
            {
                searchUri = searchUri.ExtendQuery("expand", expand);
            }

            if (cqlContext != null)
            {
                searchUri = searchUri.ExtendQuery("cqlcontext", cqlContext);
            }

            var response = await searchUri.GetAsAsync<HttpResponse<Result<Content>, Error>>(cancellationToken).ConfigureAwait(false);
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
            var labelUri = confluenceClient.ConfluenceApiUri.AppendSegments("content", contentId, "label", label);
            confluenceClient.Behaviour.MakeCurrent();

            var response = await labelUri.DeleteAsync<HttpResponse>(cancellationToken).ConfigureAwait(false);
            response.HandleStatusCode(HttpStatusCode.NoContent);
        }
    }
}