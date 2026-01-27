// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Dapplo.Confluence.Entities;

/// <summary>
///     Space information
///     See: https://docs.atlassian.com/confluence/REST/latest
///     Should be called with expand=icon,description.plain,homepage
/// </summary>
[JsonObject]
public class Space : BaseEntity<long>
{
    /// <summary>
    ///     Description
    /// </summary>
    [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Description Description { get; set; }

    /// <summary>
    ///     The values that are expandable
    /// </summary>
    [JsonProperty("_expandable")]
    public IDictionary<string, string> Expandables { get; set; }

    /// <summary>
    ///     Icon for the space
    /// </summary>
    [JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Picture Icon { get; set; }

    /// <summary>
    ///     Test if this space is a personal space, this is true when the Key starts with a ~
    /// </summary>
    [JsonIgnore]
    public bool IsPersonal => true == Key?.StartsWith("~");

    /// <summary>
    ///     Key for the space
    /// </summary>
    [JsonProperty("key", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Key { get; set; }

    /// <summary>
    ///     The name of the space
    /// </summary>
    [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the author of the space.
    /// </summary>
    [JsonProperty("authorId", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the homepage associated with the space.
    /// </summary>
    [JsonProperty("homepageId", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public long HomepageId { get; set; }
}