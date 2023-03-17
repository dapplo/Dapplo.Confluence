// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Confluence.Entities;

/// <summary>
///     Details to perform a search with
/// </summary>
public class SearchDetails : PagingInformation
{
    /// <summary>
    /// Default constructor using a IFinalClause
    /// </summary>
    /// <param name="cql">IFinalClause</param>
    public SearchDetails(IFinalClause cql)
    {
        Cql = cql;
    }

    /// <summary>
    /// Specify the Confluence query language
    /// </summary>
    public IFinalClause Cql { get; set; }

    /// <summary>
    /// The context to execute a cql search in, this is the json serialized form of SearchContext
    /// </summary>
    public string CqlContext { get; set; }

    /// <summary>
    /// Specify the search expand values, default is what is specified in the ConfluenceClientConfig.ExpandSearch
    /// </summary>
    public IEnumerable<string> ExpandSearch { get; set; } = ConfluenceClientConfig.ExpandSearch;

    /// <summary>
    /// Cursor used to page trought search results.
    /// </summary>
    public string Cursor { get; set; }
}