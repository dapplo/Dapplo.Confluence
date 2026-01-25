// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System.Runtime.Serialization;

namespace Dapplo.Confluence.Query;

/// <summary>
/// Possible types of content, used in CQL
/// </summary>
public enum ContentTypes
{
	/// <summary>
	/// The content is global content
	/// </summary>
	[EnumMember(Value = "global")] Global,
	/// <summary>
	/// The content is a page
	/// </summary>
	[EnumMember(Value = "page")] Page,
	/// <summary>
	/// The content is a blogpost
	/// </summary>
	[EnumMember(Value = "blogpost")] BlogPost,
	/// <summary>
	/// The content is a comment
	/// </summary>
	[EnumMember(Value = "comment")] Comment,
	/// <summary>
	/// The content is an attachment
	/// </summary>
	[EnumMember(Value = "attachment")] Attachment,
	/// <summary>
	/// The content is a personal page
	/// </summary>
	[EnumMember(Value = "personal")] Personal,
	/// <summary>
	/// The content is details about collaboration
	/// </summary>
	[EnumMember(Value = "collaboration")] Collaboration
}