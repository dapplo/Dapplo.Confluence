// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Confluence.Query;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests
/// </summary>
public class SpaceQueryTests
{
    public SpaceQueryTests(ITestOutputHelper testOutputHelper)
    {
        LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
    }


    [Fact]
    public void TestClause_Space_Is()
    {
        var clause = Where.Space.Is("DEV");
        Assert.Equal("space = \"DEV\"", clause.ToString());
    }

    [Fact]
    public void TestClause_Space_Not_Is()
    {
        var clause = Where.Space.Not.Is("DEV");
        Assert.Equal("space != \"DEV\"", clause.ToString());
    }

    [Fact]
    public void TestClause_Space_In()
    {
        var clause = Where.Space.In("DEV", "PRODUCTION");
        Assert.Equal("space in (\"DEV\", \"PRODUCTION\")", clause.ToString());
    }

    [Fact]
    public void TestClause_Space_Not_In()
    {
        var clause = Where.Space.Not.In("DEV", "PRODUCTION");
        Assert.Equal("space not in (\"DEV\", \"PRODUCTION\")", clause.ToString());
    }

    [Fact]
    public void TestClause_Space_InFavouriteSpacesAnd()
    {
        var clause = Where.Space.InFavouriteSpacesAnd("DEV", "PRODUCTION");
        Assert.Equal("space in (favouriteSpaces(), \"DEV\", \"PRODUCTION\")", clause.ToString());
    }

    [Fact]
    public void TestClause_Space_Not_InFavouriteSpacesAnd()
    {
        var clause = Where.Space.Not.InFavouriteSpacesAnd("DEV", "PRODUCTION");
        Assert.Equal("space not in (favouriteSpaces(), \"DEV\", \"PRODUCTION\")", clause.ToString());
    }
}