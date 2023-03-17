using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapplo.Confluence.Entities
{
    /// <summary>
    ///     A container to store pageable results that need a cursor to be paged.
    ///     See: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-search/
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class CursorBasedResult<TResult> : Result<TResult>
    {
        private string cursor = null;

        /// <summary>
        /// Cursor needed to page trought the results.
        /// </summary>
        [JsonIgnore]
        public string Cursor
        {
            get
            {
                if (!HasNext) return null;
                if (cursor == null)
                {
                    var querystring = Links.Next.OriginalString.Substring(Links.Next.OriginalString.IndexOf('?'));
                    cursor = UriParseExtensions.QueryStringToDictionary(querystring)?["cursor"];
                }
                return cursor;
            }
        }
    }
}
