// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dapplo.Confluence.Tests;

/// <summary>
///     Tests for the attachment domain
/// </summary>
[CollectionDefinition("Dapplo.Confluence")]
public class AttachmentTests : ConfluenceIntegrationTests
{
    public AttachmentTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    // TODO: Enable again whenever I got the rights working
    //[Fact]
    public async Task TestGetAttachments()
    {
        var attachments = await ConfluenceTestClient.Attachment.GetAttachmentsAsync(950274);
        Assert.NotNull(attachments);
        Assert.True(attachments.Results.Count > 0);
        await using var attachmentMemoryStream = await ConfluenceTestClient.Attachment.GetContentAsync<MemoryStream>(attachments.FirstOrDefault());
        Assert.True(attachmentMemoryStream.Length > 0);
    }

    /// <summary>
    ///     Doesn't work yet, as deleting an attachment (with multiple versions) is not supported
    ///     See <a href="https://jira.atlassian.com/browse/CONF-36015">CONF-36015</a>
    /// </summary>
    /// <returns></returns>
    //[Fact]
    public async Task TestAttach()
    {
        const long testPageId = 950274;
        var attachments = await ConfluenceTestClient.Attachment.GetAttachmentsAsync(testPageId);
        Assert.NotNull(attachments);

        // Delete all attachments
        foreach (var attachment in attachments.Results)
        {
            // Attachments are content!!
            await ConfluenceTestClient.Attachment.DeleteAsync(attachment);
        }

        const string attachmentContent = "Testing 1 2 3";
        attachments = await ConfluenceTestClient.Attachment.AttachAsync(testPageId, attachmentContent, "test.txt", "This is a test");
        Assert.NotNull(attachments);

        attachments = await ConfluenceTestClient.Attachment.GetAttachmentsAsync(testPageId);
        Assert.NotNull(attachments);
        Assert.True(attachments.Results.Count > 0);

        // Test if the content is correct
        foreach (var attachment in attachments.Results)
        {
            var content = await ConfluenceTestClient.Attachment.GetContentAsync<string>(attachment);
            Assert.Equal(attachmentContent, content);
        }
        // Delete all attachments
        foreach (var attachment in attachments.Results)
        {
            // Btw. Attachments are content!!
            await ConfluenceTestClient.Attachment.DeleteAsync(attachment.Id);
        }
        attachments = await ConfluenceTestClient.Attachment.GetAttachmentsAsync(testPageId);
        Assert.NotNull(attachments);
        Assert.True(attachments.Results.Count == 0);
    }

    /// <summary>
    ///     Doesn't work yet, as deleting an attachment (with multiple versions) is not supported
    ///     See <a href="https://jira.atlassian.com/browse/CONF-36015">CONF-36015</a>
    /// </summary>
    /// <returns></returns>
    //[Fact]
    public async Task TestAttachBitmap()
    {
        const long testPageId = 550731777;

        // TODO: make the test work like this, delete all attachments, AttachAsync, UpdateAsync

        using Stream stream = File.OpenRead(@"TestFiles\icon.png");

        var attachments = await ConfluenceTestClient.Attachment.AttachAsync(testPageId, stream, "streamed.png", "Just attached a bitmap");
        Assert.NotNull(attachments);

        attachments = await ConfluenceTestClient.Attachment.GetAttachmentsAsync(testPageId);
        Assert.NotNull(attachments);
        Assert.True(attachments.Results.Count > 0);
    }
}