// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Newtonsoft.Json;

namespace Dapplo.Confluence.Entities;

/// <summary>
///     The long running tasks
/// </summary>
[JsonObject]
public class LongRunningTask
{
    /// <summary>
    ///     Id of the task
    /// </summary>
    [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Id { get; set; }

    /// <summary>
    ///     In this the link for the status is returned
    /// </summary>
    [JsonProperty("links", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Links Links { get; set; }

    /// <summary>
    ///     Status of the task
    /// </summary>
    [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Status { get; set; }

    /// <summary>
    /// This was added later
    /// </summary>
    [JsonProperty("additionalDetails", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IDictionary<string, string> AdditionalDetails { get; set; }
}