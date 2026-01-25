// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Dapplo.Confluence.Entities;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.WinForms.ContentConverter;
using Dapplo.HttpExtensions.Wpf.ContentConverter;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests for the user domain
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class UserTests : ConfluenceIntegrationTests
{
    public UserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        // Add BitmapHttpContentConverter if it was not yet added
        if (HttpExtensionsGlobals.HttpContentConverters.All(x => x.GetType() != typeof(BitmapHttpContentConverter)))
        {
            HttpExtensionsGlobals.HttpContentConverters.Add(BitmapHttpContentConverter.Instance.Value);
        }
        // Add BitmapSourceHttpContentConverter if it was not yet added
        if (HttpExtensionsGlobals.HttpContentConverters.All(x => x.GetType() != typeof(BitmapSourceHttpContentConverter)))
        {
            HttpExtensionsGlobals.HttpContentConverters.Add(BitmapSourceHttpContentConverter.Instance.Value);
        }
    }

    /// <summary>
    ///     Test if the current user is correctly retrieved
    /// </summary>
    [Fact]
    public async Task TestCurrentUser()
    {
        var currentUser = await ConfluenceTestClient.User.GetCurrentUserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(currentUser);
        Assert.True(currentUser.HasIdentifier());
        Assert.NotNull(currentUser.ProfilePicture);
        Assert.DoesNotContain("Anonymous", currentUser.DisplayName);

        var currentUserIndirectly = await ConfluenceTestClient.User.GetUserAsync(currentUser, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(currentUser.DisplayName, currentUserIndirectly.DisplayName);
    }

    /// <summary>
    ///     Test if the picture can be downloaded
    /// </summary>
    [Fact]
    public async Task TestCurrentUserPicture()
    {
        var currentUser = await ConfluenceTestClient.User.GetCurrentUserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(currentUser);
        Assert.NotNull(currentUser.ProfilePicture);
        Assert.DoesNotContain("Anonymous", currentUser.DisplayName);

        var bitmapSource = await ConfluenceTestClient.Misc.GetPictureAsync<BitmapSource>(currentUser.ProfilePicture, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(bitmapSource);
        Assert.True(bitmapSource.Width > 0);
    }

    /// <summary>
    /// Test if the GetGroupMembershipsAsync returns at least a group
    /// </summary>
    [Fact]
    public async Task TestGetGroupMembershipsAsync()
    {
        var currentUser = await ConfluenceTestClient.User.GetCurrentUserAsync(cancellationToken: TestContext.Current.CancellationToken);
        var groupsForUser = await ConfluenceTestClient.User.GetGroupMembershipsAsync(currentUser, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotEmpty(groupsForUser);
    }

    /// <summary>
    /// Test the label watcher functionality
    /// </summary>
    [Fact]
    public async Task TestLabelWatcher()
    {
        const string testLabel = "Dappl0";
        const long contentId = 550731777;

        var label = new Label
        {
            Name = testLabel
        };

        // Make sure there is a label
        await ConfluenceTestClient.Content.AddLabelsAsync(contentId, Enumerable.Repeat(label, 1), cancellationToken: TestContext.Current.CancellationToken);

        try
        {
            if (await ConfluenceTestClient.User.IsLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken))
            {
                await ConfluenceTestClient.User.RemoveLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken);
            }
            Assert.False(await ConfluenceTestClient.User.IsLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken));

            // Add the current user as a label watcher
            await ConfluenceTestClient.User.AddLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken);
            Assert.True(await ConfluenceTestClient.User.IsLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken));

            await ConfluenceTestClient.User.RemoveLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken);
            Assert.False(await ConfluenceTestClient.User.IsLabelWatcher(testLabel, cancellationToken: TestContext.Current.CancellationToken));
        }
        finally
        {
            // Make sure the label is removed again
            await ConfluenceTestClient.Content.DeleteLabelAsync(contentId, testLabel, cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Test the space watcher functionality
    /// </summary>
    [Fact]
    public async Task TestSpaceWatcher()
    {
        string testSpace = "TEST";

        if (await ConfluenceTestClient.User.IsSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken))
        {
            await ConfluenceTestClient.User.RemoveSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken);
        }
        Assert.False(await ConfluenceTestClient.User.IsSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken));

        // Add the current user as a space watcher
        await ConfluenceTestClient.User.AddSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(await ConfluenceTestClient.User.IsSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken));

        await ConfluenceTestClient.User.RemoveSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(await ConfluenceTestClient.User.IsSpaceWatcher(testSpace, cancellationToken: TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Test the content watcher functionality
    /// </summary>
    [Fact]
    public async Task TestContentWatcher()
    {
        long contentId = 550731777;
        if (await ConfluenceTestClient.User.IsContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken))
        {
            await ConfluenceTestClient.User.RemoveContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken);
        }
        Assert.False(await ConfluenceTestClient.User.IsContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken));

        // Add the current user as a content watcher
        await ConfluenceTestClient.User.AddContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(await ConfluenceTestClient.User.IsContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken));

        await ConfluenceTestClient.User.RemoveContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(await ConfluenceTestClient.User.IsContentWatcher(contentId, cancellationToken: TestContext.Current.CancellationToken));
    }
}