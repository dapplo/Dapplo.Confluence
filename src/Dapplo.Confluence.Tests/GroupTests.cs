// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests for group related functionality
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class GroupTests : ConfluenceIntegrationTests
{
    public GroupTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    /// <summary>
    ///     Test if the list of Groups is returned correctly
    /// </summary>
    [Fact]
    public async Task TestGetGroups()
    {
        var groups = await ConfluenceTestClient.Group.GetGroupsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotEmpty(groups);
    }

    /// <summary>
    ///     Test if one of the groups the current user belongs to, also has the current user as a member
    /// </summary>
    [Fact]
    public async Task TestGetGroupMembersAsync()
    {
        var currentUser = await ConfluenceTestClient.User.GetCurrentUserAsync(cancellationToken: TestContext.Current.CancellationToken);
        var groupsForUser = await ConfluenceTestClient.User.GetGroupMembershipsAsync(currentUser, cancellationToken: TestContext.Current.CancellationToken);
        var usersInGroup = await ConfluenceTestClient.Group.GetGroupMembersByGroupIdAsync(groupsForUser.Where(g => g.Name.StartsWith("confluence")).First().Id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Contains(currentUser.AccountId, usersInGroup.Select(u => u.AccountId));
    }
}