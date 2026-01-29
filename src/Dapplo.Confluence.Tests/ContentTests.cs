// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Query;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.WinForms.ContentConverter;
using Dapplo.HttpExtensions.Wpf.ContentConverter;
using Dapplo.Log;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class ContentTests : ConfluenceIntegrationTests
{

#pragma warning disable IDE0090 // Use 'new(...)'
    private static readonly LogSource Log = new LogSource();
#pragma warning restore IDE0090 // Use 'new(...)'
    public ContentTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

    [Fact]
    public async Task Test_IsDefault()
    {
        var query = Where.And(Where.Space.Is("TEST"), Where.Type.IsPage, Where.Title.Contains("Doesn't exist"));
        var searchResults = await ConfluenceTestClient.Content.SearchAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        var searchResult = searchResults.FirstOrDefault();

        Assert.True(searchResult == default);
    }

    [Fact]
    public async Task Test_ContentVersion()
    {
        var query = Where.And(Where.Space.Is("TEST"), Where.Type.IsPage, Where.Title.Contains("Test Home"));
        var searchResults = await ConfluenceTestClient.Content.SearchAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        var searchResult = searchResults.First();
        Log.Info().WriteLine("Version = {0}", searchResult.Version.Number);
        query = Where.Title.Contains("Test Home");
        searchResults = await ConfluenceTestClient.Content.SearchAsync(query, cancellationToken: TestContext.Current.CancellationToken);
        searchResult = searchResults.First();
        Log.Info().WriteLine("Version = {0}", searchResult.Version.Number);

        var content = await ConfluenceTestClient.Content.GetAsync(searchResult, ConfluenceClientConfig.ExpandGetContentWithStorage, cancellationToken: TestContext.Current.CancellationToken);
        Log.Info().WriteLine("Version = {0}", content.Version.Number);
    }

    /// <summary>
    ///     Test GetAsync
    /// </summary>
    //[Fact]
#pragma warning disable xUnit1013 // Public method should be marked as test
    public async Task TestGetContent()
#pragma warning restore xUnit1013 // Public method should be marked as test
    {
        var content = await ConfluenceTestClient.Content.GetAsync(950274);
        Assert.NotNull(content);
        Assert.NotNull(content.Version);
        Assert.NotNull(content.Ancestors);
        Assert.True(content.Ancestors.Count > 0);
    }

    /// <summary>
    ///     Test UpdateAsync
    /// </summary>
    [Fact]
    public async Task TestContentUpdate()
    {
        var content = await ConfluenceTestClient.Content.GetAsync(550731777, ConfluenceClientConfig.ExpandGetContentForUpdate, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(content);
        Assert.NotNull(content.Version);
        content.Body.Storage.Value += $"\r\nTesting 1 - 2 -3 {DateTimeOffset.Now}";
        content.Version = new Entities.Version { IsMinorEdit = false, Number = content.Version.Number + 1 };
        await ConfluenceTestClient.Content.UpdateAsync(content, cancellationToken: TestContext.Current.CancellationToken);
    }


    /// <summary>
    ///     Test CopyAsync and MoveAsync
    /// </summary>
    [Fact]
    public async Task TestContentCopyAndMove()
    {
        var copyContent = new CopyContent
        {
            Destination = new CopyPageRequestDestination
            {
                DestinationType = CopyDestinations.ParentPage,
                Value = "" + 550731777
            },
            PageTitle = "Copied page"
        };
        var content = await ConfluenceTestClient.Content.CopyAsync(550731777, copyContent, cancellationToken: TestContext.Current.CancellationToken);
        try
        {
            Assert.NotNull(content);
            await ConfluenceTestClient.Content.MoveAsync(content.Id, Positions.Append, 550731777, cancellationToken: TestContext.Current.CancellationToken);
        }
        finally
        {
            await ConfluenceTestClient.Content.DeleteAsync(content.Id, cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    ///     Test .GetChildren
    /// </summary>
    [Fact]
    public async Task TestGetChildren()
    {
        var results = await ConfluenceTestClient.Content.GetChildrenAsync(550731777, new PagingInformation {Limit = 1}, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(results);
        Assert.True(results.HasNext);
        Assert.True(results.Size > 0);
    }

    /// <summary>
    ///     Test GetHistoryAsync
    /// </summary>
    //[Fact]
#pragma warning disable xUnit1013 // Public method should be marked as test
    public async Task TestGetContentHistory()
#pragma warning restore xUnit1013 // Public method should be marked as test
    {
        var history = await ConfluenceTestClient.Content.GetHistoryAsync(950274);
        Assert.NotNull(history);
        Assert.NotNull(history.CreatedBy);
    }

    [Fact]
    public async Task TestCreateContent()
    {
        var query = Where.And(Where.Space.Is("TEST"), Where.Type.IsPage, Where.Title.Contains("Testing 1 2 3"));
        var searchResults = await ConfluenceTestClient.Content.SearchAsync(query, cancellationToken: TestContext.Current.CancellationToken);
        var oldPage = searchResults.Results.FirstOrDefault();
        if (oldPage != null)
        {
            await ConfluenceTestClient.Content.DeleteAsync(oldPage, cancellationToken: TestContext.Current.CancellationToken);
        }
        await Task.Delay(1000, cancellationToken: TestContext.Current.CancellationToken);
        var page = await ConfluenceTestClient.Content.CreateAsync(ContentTypes.Page, "Testing 1 2 3", "TEST", "<p>This is a test</p>", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(page);
        Assert.True(page.Id > 0);
        await Task.Delay(1000, cancellationToken: TestContext.Current.CancellationToken);
        await ConfluenceTestClient.Content.DeleteAsync(page, cancellationToken: TestContext.Current.CancellationToken);
    }

    //[Fact]
#pragma warning disable xUnit1013 // Public method should be marked as test
    public async Task TestDeleteContent()
#pragma warning restore xUnit1013 // Public method should be marked as test
    {
        await ConfluenceTestClient.Content.DeleteAsync(30375945);
    }

    [Fact]
    public async Task TestSearch()
    {
        ConfluenceClientConfig.ExpandSearch = new[] { "version", "space", "space.icon", "space.description", "space.homepage", "history.lastUpdated" };

        var searchResult = await ConfluenceTestClient.Content.SearchAsync(Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home")), pagingInformation: new PagingInformation {Limit = 20}, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(ContentTypes.Page, searchResult.First().Type);
        var uri = ConfluenceTestClient.CreateWebUiUri(searchResult.FirstOrDefault()?.Links);
        Assert.NotNull(uri);
    }

    [Fact]
    public async Task TestSearchAttachment()
    {
        ConfluenceClientConfig.ExpandSearch = new[] { "version", "space", "space.icon", "space.description", "space.homepage", "history.lastUpdated" };

        var query = Where.And(Where.Type.IsAttachment, Where.Text.Contains("404"));

        var searchResult = await ConfluenceTestClient.Content.SearchAsync(query, pagingInformation: new PagingInformation { Limit = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        var attachment = searchResult.First();
        Assert.Equal(ContentTypes.Attachment, attachment.Type);
        Assert.NotNull(ConfluenceTestClient.Attachment.CreateDownloadUri(attachment.Links));
        // I know the attachment is a bitmap, this should work
        var bitmap = await ConfluenceTestClient.Attachment.GetContentAsync<Bitmap>(attachment, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(bitmap.Width > 0);
    }

    [Fact]
    public async Task TestSearchLabels()
    {
        var searchResult = await ConfluenceTestClient.Content.SearchAsync(Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home")), pagingInformation: new PagingInformation { Limit = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        var contentId = searchResult.First().Id;

        var labels = new[] { "test1", "test2" };
        await ConfluenceTestClient.Content.AddLabelsAsync(contentId, labels.Select(s => new Label { Name = s }), cancellationToken: TestContext.Current.CancellationToken);

        ConfluenceClientConfig.ExpandSearch = new[] { "version", "space", "space.icon", "space.description", "space.homepage", "history.lastUpdated", "metadata.labels" };

        searchResult = await ConfluenceTestClient.Content.SearchAsync(Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home")), pagingInformation: new PagingInformation { Limit = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        var labelEntities = searchResult.First().Metadata.Labels.Results;

        Assert.NotEmpty(labelEntities);

        // Delete all
        foreach (var label in labelEntities)
        {
            await ConfluenceTestClient.Content.DeleteLabelAsync(contentId, label.Name, cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task TestLabels()
    {
        var searchResult = await ConfluenceTestClient.Content.SearchAsync(Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home")), pagingInformation: new PagingInformation { Limit = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        var contentId = searchResult.First().Id;

        var labels = new[] { "test1", "test2" };
        await ConfluenceTestClient.Content.AddLabelsAsync(contentId, labels.Select(s => new Label { Name = s }), cancellationToken: TestContext.Current.CancellationToken);
        var labelsForContent = await ConfluenceTestClient.Content.GetLabelsAsync(contentId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(labels.Length, labelsForContent.Count(label => labels.Contains(label.Name)));

        // Delete all
        foreach (var label in labelsForContent)
        {
            await ConfluenceTestClient.Content.DeleteLabelAsync(contentId, label.Name, cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task TestGetPdf()
    {
        var searchResult = await ConfluenceTestClient.Content.SearchAsync(Where.And(Where.Type.IsPage, Where.Text.Contains("Test Home")), pagingInformation: new PagingInformation { Limit = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        var contentId = searchResult.First().Id;

        var pdfBytes = await ConfluenceTestClient.Content.GetPdfAsync<byte[]>(contentId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
        
        // Verify it's a PDF by checking the magic bytes
        Assert.Equal(0x25, pdfBytes[0]); // %
        Assert.Equal(0x50, pdfBytes[1]); // P
        Assert.Equal(0x44, pdfBytes[2]); // D
        Assert.Equal(0x46, pdfBytes[3]); // F
    }
}