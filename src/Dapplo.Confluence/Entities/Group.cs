// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Dapplo.Confluence.Entities;

/// <summary>
///     Group information
///     See: https://docs.atlassian.com/confluence/REST/latest
/// </summary>
[JsonObject]
public class Group : BaseEntity<string>
{
    /// <summary>
    ///     Name of the group
    /// </summary>
    [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Name { get; set; }
}