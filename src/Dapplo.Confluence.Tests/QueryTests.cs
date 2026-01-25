// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dapplo.Confluence.Query;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests
/// </summary>
public class QueryTests
{
    public QueryTests(ITestOutputHelper testOutputHelper)
    {
        LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
    }

    [Fact]
    public void TestClause_Type_IsPage()
    {
        var clause = Where.Type.IsPage;
        Assert.Equal("type = page", clause.ToString());
    }

    [Fact]
    public void TestClause_Type_In()
    {
        var clause = Where.Type.In(ContentTypes.Page, ContentTypes.BlogPost);
        Assert.Equal("type in (page, blogpost)", clause.ToString());
    }

    [Fact]
    public void TestClause_Type_Not_In()
    {
        var clause = Where.Type.Not.In(ContentTypes.Page, ContentTypes.BlogPost);
        Assert.Equal("type not in (page, blogpost)", clause.ToString());
    }

    [Fact]
    public void TestClause_Created_StartOfYear()
    {
        var clause = Where.Created.Before.StartOfYear();
        Assert.Equal("created < startOfYear()", clause.ToString());
    }

    [Fact]
    public void TestClause_Created_StartOfDay_WithNegativeIncrement()
    {
        // Find content created in the last 7 days
        // created > startOfDay("-7d")

        var clause = Where.Created.After.StartOfDay(TimeSpan.FromDays(-7));
        Assert.Equal("created > startOfDay(\"-7d\")", clause.ToString());
    }

    [Fact]
    public void TestClause_Created_On()
    {
        // Find content created today
        // created = yyyy-mm-dd

        var clause = Where.Created.On.DateTime(DateTime.Today);
        Assert.Equal($"created = \"{DateTime.Today:yyyy-MM-dd}\"", clause.ToString());
    }

    [Fact]
    public void TestClause_Text_Contains()
    {
        var clause = Where.Text.Contains("hello");
        Assert.Equal("text ~ \"hello\"", clause.ToString());
    }
}