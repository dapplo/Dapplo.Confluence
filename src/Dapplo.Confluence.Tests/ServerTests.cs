// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class ServerTests : ConfluenceIntegrationTests
{
    public ServerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task Test_IsCloud()
    {
        var isCloudServer = await ConfluenceTestClient.IsCloudServer(cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(isCloudServer);
    }
}