// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class SpaceTests : ConfluenceIntegrationTests
{
    public SpaceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    /// <summary>
    ///     Test GetAsync
    /// </summary>
    [Fact]
    public async Task TestGetSpace()
    {
        var space = await ConfluenceTestClient.Space.GetAsync("TEST", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(space);
        Assert.NotNull(space.Description);
    }

    /// <summary>
    ///     Test Space.GetAllAsync
    /// </summary>
    [Fact]
    public async Task TestGetSpaces()
    {
        var spaces = await ConfluenceTestClient.Space.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(spaces);
        Assert.True(spaces.Count > 0);
    }

    /// <summary>
    ///     Test GetContentsAsync
    /// </summary>
    [Fact]
    public async Task TestGetContentsAsync()
    {
        var spaceContents = await ConfluenceTestClient.Space.GetContentsAsync("TEST", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(spaceContents);
        Assert.NotNull(spaceContents.Pages);
        Assert.True(spaceContents.Pages.Any());
    }

    /// <summary>
    ///     Test Space.CreateAsync
    /// </summary>
    [Fact]
    public async Task TestCreateAsync()
    {
        const string key = "TESTTMP";
        var createdSpace = await ConfluenceTestClient.Space.CreatePrivateAsync(key, "Dummy for test", "Created and deleted during test", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdSpace);
        Assert.Equal(key, createdSpace.Key);

        try
        {
            var space = await ConfluenceTestClient.Space.GetAsync(key, cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(space);
            Assert.Equal(key, space.Key);
        }
        finally
        {
            await ConfluenceTestClient.Space.DeleteAsync(key, cancellationToken: TestContext.Current.CancellationToken);
        }
    }
}