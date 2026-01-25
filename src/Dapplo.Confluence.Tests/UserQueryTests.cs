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
public class UserQueryTests
{
    public UserQueryTests(ITestOutputHelper testOutputHelper)
    {
        LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
    }


    [Fact]
    public void TestClause_AndClause()
    {
        var clause1 = Where.Creator.IsCurrentUser;
        var clause2 = Where.Mention.Not.IsCurrentUser;
        Assert.Equal("(creator = currentUser() and mention != currentUser())", Where.And(clause1, clause2).ToString());
    }

    [Fact]
    public void TestClause_OrClause()
    {
        var clause1 = Where.Creator.IsCurrentUser;
        var clause2 = Where.Mention.Not.IsCurrentUser;
        Assert.Equal("(creator = currentUser() or mention != currentUser())", Where.Or(clause1, clause2).ToString());
    }

    [Fact]
    public void TestClause_IsCurrentUser()
    {
        var clause = Where.Creator.IsCurrentUser;
        Assert.Equal("creator = currentUser()", clause.ToString());
    }

    [Fact]
    public void TestClause_NotIsCurrentUser()
    {
        var clause = Where.Creator.Not.IsCurrentUser;
        Assert.Equal("creator != currentUser()", clause.ToString());
    }

    [Fact]
    public void TestClause_IsUser()
    {
        var clause = Where.Creator.Is("jsmith");
        Assert.Equal("creator = \"jsmith\"", clause.ToString());
    }

    [Fact]
    public void TestClause_InCurrentUserAnd()
    {
        var clause = Where.Creator.InCurrentUserAnd("jsmith");
        Assert.Equal("creator in (currentUser(), \"jsmith\")", clause.ToString());
    }

    [Fact]
    public void TestClause_IsUserIn()
    {
        var clause = Where.Creator.In("jsmith");
        Assert.Equal("creator in (\"jsmith\")", clause.ToString());
    }

    [Fact]
    public void TestClause_NotIsUser()
    {
        var clause = Where.Creator.Not.Is("jsmith");
        Assert.Equal("creator != \"jsmith\"", clause.ToString());
    }

    [Fact]
    public void TestClause_OrderBy()
    {
        var clause = Where.Creator.Is("jsmith").OrderBy(Fields.Space).OrderByAscending(Fields.Title);
        Assert.Equal("creator = \"jsmith\" order by space, title asc", clause.ToString());
    }
}