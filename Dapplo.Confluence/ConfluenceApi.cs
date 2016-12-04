#region Dapplo 2016 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2016 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.Confluence
// 
// Dapplo.Confluence is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.Confluence is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.Confluence. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Internals;
using Dapplo.HttpExtensions;

#endregion

namespace Dapplo.Confluence
{
	/// <summary>
	///     Confluence API, using Dapplo.HttpExtensions
	/// </summary>
	public class ConfluenceApi
	{
		/// <summary>
		///     Store the specific HttpBehaviour, which contains a IHttpSettings and also some additional logic for making a
		///     HttpClient which works with Confluence
		/// </summary>
		protected readonly IHttpBehaviour _behaviour;

		private string _password;

		private string _user;

		/// <summary>
		///     Create the ConfluenceApi object, here the HttpClient is configured
		/// </summary>
		/// <param name="baseUri">Base URL, e.g. https://yourConfluenceserver</param>
		/// <param name="httpSettings">IHttpSettings or null for default</param>
		public ConfluenceApi(Uri baseUri, IHttpSettings httpSettings = null)
		{
			if (baseUri == null)
			{
				throw new ArgumentNullException(nameof(baseUri));
			}
			ConfluenceDownloadBaseUri = baseUri;
			ConfluenceApiBaseUri = baseUri.AppendSegments("rest", "api");

			_behaviour = ConfigureBehaviour(new HttpBehaviour(), httpSettings);
		}

		/// <summary>
		/// Helper method to configure the IChangeableHttpBehaviour
		/// </summary>
		/// <param name="behaviour">IChangeableHttpBehaviour</param>
		/// <param name="httpSettings">IHttpSettings</param>
		/// <returns>the behaviour, but configured as IHttpBehaviour </returns>
		protected IHttpBehaviour ConfigureBehaviour(IChangeableHttpBehaviour behaviour, IHttpSettings httpSettings = null)
		{
			behaviour.HttpSettings = httpSettings ?? HttpExtensionsGlobals.HttpSettings;
			behaviour.OnHttpRequestMessageCreated = httpMessage =>
			{
				httpMessage?.Headers.TryAddWithoutValidation("X-Atlassian-Token", "no-check");
				if (!string.IsNullOrEmpty(_user) && _password != null)
				{
					httpMessage?.SetBasicAuthorization(_user, _password);
				}
				return httpMessage;
			};
			return behaviour;
		}

		/// <summary>
		/// The IHttpBehaviour for this Confluence instance
		/// </summary>
		public IHttpBehaviour HttpBehaviour => _behaviour;


		#region Read

		/// <summary>
		///     Retrieve the picture for the supplied Picture entity
		/// </summary>
		/// <typeparam name="TResponse">the type to return the result into. e.g. Bitmap,BitmapSource or MemoryStream</typeparam>
		/// <param name="picture">Picture from User, Space, History etc</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Bitmap,BitmapSource or MemoryStream (etc) depending on TResponse</returns>
		public async Task<TResponse> GetPictureAsync<TResponse>(Picture picture, CancellationToken cancellationToken = default(CancellationToken))
			where TResponse : class
		{
			PromoteContext();
			var pictureUriBuilder = new UriBuilder(ConfluenceApiBaseUri)
			{
				Path = picture.Path
			};
			var response = await pictureUriBuilder.Uri.GetAsAsync<HttpResponse<TResponse, string>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		#endregion

		#region Supporting

		/// <summary>
		///     Set Basic Authentication for the current client
		/// </summary>
		/// <param name="user">username</param>
		/// <param name="password">password</param>
		public void SetBasicAuthentication(string user, string password)
		{
			_user = user;
			_password = password;
		}

		/// <summary>
		///     This makes sure that the HttpBehavior is promoted for the following Http call.
		/// </summary>
		protected internal void PromoteContext()
		{
			_behaviour.MakeCurrent();
		}

		/// <summary>
		///     The base URI for your Confluence server api calls
		/// </summary>
		public Uri ConfluenceApiBaseUri { get; }

		/// <summary>
		///     The base URI for your Confluence server downloads
		/// </summary>
		protected Uri ConfluenceDownloadBaseUri { get; }

		#endregion

		#region Attachment

		/// <summary>
		///     Add an attachment to content
		/// </summary>
		/// <typeparam name="TContent">The content to upload</typeparam>
		/// <param name="contentId">content to add the attachment to</param>
		/// <param name="content">content of type TContent tfor the attachment</param>
		/// <param name="filename">Filename of the attachment</param>
		/// <param name="comment">Comment in the attachments information</param>
		/// <param name="contentType">Content-Type for the content, or null</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Result with Attachment</returns>
		public async Task<Result<Attachment>> AttachAsync<TContent>(string contentId, TContent content, string filename, string comment = null, string contentType = null,
			CancellationToken cancellationToken = default(CancellationToken))
			where TContent : class
		{
			var attachment = new AttachmentContainer<TContent>
			{
				Comment = comment,
				FileName = filename,
				Content = content,
				ContentType = contentType
			};
			PromoteContext();
			var postAttachmentUri = ConfluenceApiBaseUri.AppendSegments("content", contentId, "child", "attachment");
			var response = await postAttachmentUri.PostAsync<HttpResponse<Result<Attachment>, string>>(attachment, cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		/// <summary>
		///     Retrieve the attachments for the content
		/// </summary>
		/// <param name="contentId">string with the content id</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Result with Attachment(s)</returns>
		public async Task<Result<Attachment>> GetAttachmentsAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			PromoteContext();
			var attachmentsUri = ConfluenceApiBaseUri.AppendSegments("content", contentId, "child", "attachment");
			if (ConfluenceConfig.ExpandGetAttachments != null && ConfluenceConfig.ExpandGetAttachments.Length != 0)
			{
				attachmentsUri = attachmentsUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetAttachments));
			}
			var response = await attachmentsUri.GetAsAsync<HttpResponse<Result<Attachment>, string>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		/// <summary>
		///     Delete attachment
		///     Can't work yet, see <a href="https://jira.atlassian.com/browse/CONF-36015">CONF-36015</a>
		/// </summary>
		/// <param name="attachment">Attachment which needs to be deleted</param>
		/// <param name="cancellationToken">cancellationToken</param>
		public async Task DeleteAttachmentAsync(Attachment attachment, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentUri =
				ConfluenceDownloadBaseUri.AppendSegments("json", "removeattachmentversion.action")
					.ExtendQuery("pageId", attachment.Container.Id)
					.ExtendQuery("fileName", attachment.Title);
			PromoteContext();

			await contentUri.GetAsAsync<string>(cancellationToken).ConfigureAwait(false);
		}


		/// <summary>
		///     Retrieve the attachment for the supplied Attachment entity
		/// </summary>
		/// <typeparam name="TResponse">the type to return the result into. e.g. Bitmap,BitmapSource or MemoryStream</typeparam>
		/// <param name="attachment">Attachment</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Bitmap,BitmapSource or MemoryStream (etc) depending on TResponse</returns>
		public async Task<TResponse> GetAttachmentContentAsync<TResponse>(Attachment attachment, CancellationToken cancellationToken = default(CancellationToken))
			where TResponse : class
		{
			PromoteContext();
			var attachmentUriBuilder = new UriBuilder(ConfluenceDownloadBaseUri)
			{
				Path = attachment.Links.Download
			};
			var response = await attachmentUriBuilder.Uri.GetAsAsync<HttpResponse<TResponse, string>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		#endregion

		#region Space

		/// <summary>
		///     Create a space
		/// </summary>
		/// <param name="key">Key for the space</param>
		/// <param name="name">Name for the space</param>
		/// <param name="description">Description for the space</param>
		/// <param name="isPrivate">true if the space needs to be private</param>
		/// <param name="cancellationToken"></param>
		/// <returns>created Space</returns>
		public async Task<Space> CreateSpaceAsync(string key, string name, string description, bool isPrivate = false,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var space = new Space
			{
				Key = key,
				Name = name,
				Description = new Description
				{
					Plain = new Plain
					{
						Value = description
					}
				}
			};
			PromoteContext();
			var spaceUri = ConfluenceApiBaseUri.AppendSegments("space");
			// Create private space?
			if (isPrivate)
			{
				spaceUri = spaceUri.AppendSegments("_private");
			}
			var response = await spaceUri.PostAsync<HttpResponse<Space, string>>(space, cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		/// <summary>
		///     Update a space
		/// </summary>
		/// <param name="key">Key for the space</param>
		/// <param name="name">Name for the space</param>
		/// <param name="description">Description for the space</param>
		/// <param name="cancellationToken"></param>
		/// <returns>updated Space</returns>
		public async Task<Space> UpdateSpaceAsync(string key, string name, string description, CancellationToken cancellationToken = default(CancellationToken))
		{
			var space = new Space
			{
				Key = key,
				Name = name,
				Description = new Description
				{
					Plain = new Plain
					{
						Value = description
					}
				}
			};
			PromoteContext();
			var spaceUri = ConfluenceApiBaseUri.AppendSegments("space");
			var response = await spaceUri.PutAsync<HttpResponse<Space, string>>(space, cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse);
			}
			return response.Response;
		}

		/// <summary>
		///     Delete a space
		/// </summary>
		/// <param name="key">Key for the space</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Long task for deleting the space</returns>
		public async Task<string> DeleteSpaceAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
		{
			PromoteContext();
			var spaceUri = ConfluenceApiBaseUri.AppendSegments("space");
			return await spaceUri.DeleteAsync<string>(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		///     Get Space information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
		/// </summary>
		/// <param name="spaceKey">the space key</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Space</returns>
		public async Task<Space> GetSpaceAsync(string spaceKey, CancellationToken cancellationToken = default(CancellationToken))
		{
			var spaceUri = ConfluenceApiBaseUri.AppendSegments("space", spaceKey);
			if (ConfluenceConfig.ExpandGetSpace != null && ConfluenceConfig.ExpandGetSpace.Length != 0)
			{
				spaceUri = spaceUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetSpace));
			}

			PromoteContext();
			var response = await spaceUri.GetAsAsync<HttpResponse<Space, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get Spaces see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>List of Space</returns>
		public async Task<IList<Space>> GetSpacesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var spacesUri = ConfluenceApiBaseUri.AppendSegments("space");

			if (ConfluenceConfig.ExpandGetSpace != null && ConfluenceConfig.ExpandGetSpace.Length != 0)
			{
				spacesUri = spacesUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetSpace));
			}

			PromoteContext();
			var response = await spacesUri.GetAsAsync<HttpResponse<Result<Space>, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response.Results;
		}

		#endregion

		#region Content

		/// <summary>
		///     Get Content information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
		/// </summary>
		/// <param name="contentId">content id</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Content</returns>
		public async Task<Content> GetContentAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentUri = ConfluenceApiBaseUri.AppendSegments("content", contentId);
			if (ConfluenceConfig.ExpandGetContent != null && ConfluenceConfig.ExpandGetContent.Length != 0)
			{
				contentUri = contentUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetContent));
			}

			PromoteContext();
			var response = await contentUri.GetAsAsync<HttpResponse<Content, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get Content History information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
		/// </summary>
		/// <param name="contentId">content id</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Content</returns>
		public async Task<History> GetContentHistoryAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var historyUri = ConfluenceApiBaseUri.AppendSegments("content", contentId, "history");

			PromoteContext();

			var response = await historyUri.GetAsAsync<HttpResponse<History, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Delete content (attachments are also content)
		/// </summary>
		/// <param name="contentId">ID for the content which needs to be deleted</param>
		/// <param name="isTrashed">
		///     If the content is trashable, you will need to call DeleteAsyc twice, second time with isTrashed
		///     = true
		/// </param>
		/// <param name="cancellationToken">cancellationToken</param>
		public async Task DeleteContentAsync(string contentId, bool isTrashed = false, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentUri = ConfluenceApiBaseUri.AppendSegments("content", contentId);
			if (isTrashed)
			{
				contentUri = contentUri.ExtendQuery("status", "trashed");
			}
			PromoteContext();

			var response = await contentUri.DeleteAsync<HttpResponse<Content, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
		}

		/// <summary>
		///     Create content
		/// </summary>
		/// <param name="type">Type of content, usually page</param>
		/// <param name="title">Title for the content</param>
		/// <param name="spaceKey">Key of the space to add the content to</param>
		/// <param name="body">the complete body (HTML)</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Content</returns>
		public async Task<Content> CreateContentAsync(string type, string title, string spaceKey, string body, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentUri = ConfluenceApiBaseUri.AppendSegments("content");

			var newPage = new Content
			{
				Type = type,
				Title = title,
				Space = new Space
				{
					Key = spaceKey
				},
				Body = new Body
				{
					Storage = new BodyContent
					{
						Value = body,
						Representation = "storage"
					}
				}
			};
			PromoteContext();
			var response = await contentUri.PostAsync<HttpResponse<Content, Error>>(newPage, cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get Content information see <a href="https://docs.atlassian.com/confluence/REST/latest/#d3e164">here</a>
		/// </summary>
		/// <param name="contentId">content id</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>List with Content</returns>
		public async Task<IList<Content>> GetChildrenAsync(string contentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentUri = ConfluenceApiBaseUri.AppendSegments("content", contentId, "child");
			if (ConfluenceConfig.ExpandGetChildren != null && ConfluenceConfig.ExpandGetChildren.Length != 0)
			{
				contentUri = contentUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetChildren));
			}
			PromoteContext();
			var response = await contentUri.GetAsAsync<HttpResponse<Child, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response.Result.Results;
		}

		/// <summary>
		///     Possible since 5.7
		///     Search for issues, with a CQL (e.g. from a filter) see
		///     <a href="https://docs.atlassian.com/confluence/REST/latest/#d2e4539">here</a>
		/// </summary>
		/// <param name="cql">Confluence Query Language, like SQL, for the search</param>
		/// <param name="cqlContext">
		///     the execution context for CQL functions, provides current space key and content id. If this is
		///     not provided some CQL functions will not be available.
		/// </param>
		/// <param name="limit">Maximum number of results returned, default is 20</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Result with content items</returns>
		public async Task<Result<Content>> SearchAsync(string cql, string cqlContext = null, int limit = 20, CancellationToken cancellationToken = default(CancellationToken))
		{
			PromoteContext();

			var searchUri = ConfluenceApiBaseUri.AppendSegments("content", "search").ExtendQuery("cql", cql).ExtendQuery("limit", limit);
			if (ConfluenceConfig.ExpandSearch != null && ConfluenceConfig.ExpandSearch.Length != 0)
			{
				searchUri = searchUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandSearch));
			}
			if (cqlContext != null)
			{
				searchUri = searchUri.ExtendQuery("cqlcontext", cqlContext);
			}

			var response = await searchUri.GetAsAsync<HttpResponse<Result<Content>, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get content by title
		///     See: https://docs.atlassian.com/confluence/REST/latest/#d2e4539
		/// </summary>
		/// <param name="spaceKey">Space key</param>
		/// <param name="title">Title of the content</param>
		/// <param name="start">Start of the results, used for paging</param>
		/// <param name="limit">Maximum number of results returned, default is 20</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Results with content items</returns>
		public async Task<Result<Content>> GetContentByTitleAsync(string spaceKey, string title, int start = 0, int limit = 20,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			PromoteContext();
			var searchUri = ConfluenceApiBaseUri.AppendSegments("content").ExtendQuery(new Dictionary<string, object>
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
			if (ConfluenceConfig.ExpandGetContentByTitle != null && ConfluenceConfig.ExpandGetContentByTitle.Length != 0)
			{
				searchUri = searchUri.ExtendQuery("expand", string.Join(",", ConfluenceConfig.ExpandGetContentByTitle));
			}
			var response = await searchUri.GetAsAsync<HttpResponse<Result<Content>, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		#endregion

		#region User

		/// <summary>
		///     Get currrent user information, introduced with 6.6
		///     See: https://docs.atlassian.com/confluence/REST/latest/#user-getCurrent
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>User</returns>
		public async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var myselfUri = ConfluenceApiBaseUri.AppendSegments("user", "current");
			PromoteContext();
			var response = await myselfUri.GetAsAsync<HttpResponse<User, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get Anonymous user information, introduced with 6.6
		///     See: https://docs.atlassian.com/confluence/REST/latest/#user-getAnonymous
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>User</returns>
		public async Task<User> GetAnonymousUserAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var myselfUri = ConfluenceApiBaseUri.AppendSegments("user", "anonymous");
			PromoteContext();
			var response = await myselfUri.GetAsAsync<HttpResponse<User, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get user information, introduced with 6.6
		///     See: https://docs.atlassian.com/confluence/REST/latest/#user-getUser
		/// </summary>
		/// <param name="username">string with username</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>user information</returns>
		public async Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
		{
			var userUri = ConfluenceApiBaseUri.AppendSegments("user").ExtendQuery("username", username);
			PromoteContext();
			var response = await userUri.GetAsAsync<HttpResponse<User, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response;
		}

		/// <summary>
		///     Get the groups for a user
		/// </summary>
		/// <param name="username">string with username</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>List with Groups</returns>
		public async Task<IList<Group>> GetGroupsAsync(string username, CancellationToken cancellationToken = default(CancellationToken))
		{
			var groupUri = ConfluenceApiBaseUri.AppendSegments("user", "memberof").ExtendQuery("username", username);
			PromoteContext();
			var response = await groupUri.GetAsAsync<HttpResponse<Result<Group>, Error>>(cancellationToken).ConfigureAwait(false);
			if (response.HasError)
			{
				throw new Exception(response.ErrorResponse.Message);
			}
			return response.Response.Results;
		}

		#endregion
	}
}