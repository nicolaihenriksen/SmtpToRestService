using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Moq;
using SmtpServer;
using Xunit;

namespace SmtpToRest.UnitTests;

public class SimpleMessageStoreTests
{
    [Fact]
    public async Task SaveAsync_ShouldPlaceMimeMessageInQueue_WhenCorrectlyParsed()
    {
        // Arrange
        var logger = new Mock<ILogger<SmtpServerBackgroundService>>();
        var messageQueue = new BlockingCollection<IMimeMessage>();
        var messageStore = new SimpleMessageStore(logger.Object, messageQueue);
        var message = new MimeMessage(new[] {new MailboxAddress(Encoding.UTF8, "Some sender", "some@sender.com")},
            new[] {new MailboxAddress(Encoding.UTF8, "Some recipient", "some@recipient.com")},
            "subject",
            new TextPart(new TextFormat()));
        await using var stream = new MemoryStream();
        await message.WriteToAsync(FormatOptions.Default, stream, CancellationToken.None);

        // Act
        await messageStore.SaveAsync(new Mock<ISessionContext>().Object, new Mock<IMessageTransaction>().Object, new ReadOnlySequence<byte>(stream.GetBuffer()), CancellationToken.None);

        // Assert
        messageQueue.Should().HaveCount(1);
        messageQueue.First().Address.Should().Be("some@sender.com");
    }

    [Fact]
    public async Task SaveAsync_ShouldNotPlaceMimeMessageInQueue_WhenUnableToParse()
    {
        // Arrange
        var logger = new Mock<ILogger<SmtpServerBackgroundService>>();
        var messageQueue = new BlockingCollection<IMimeMessage>();
        var messageStore = new SimpleMessageStore(logger.Object, messageQueue);
        var messageBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };

        // Act
        await messageStore.SaveAsync(new Mock<ISessionContext>().Object, new Mock<IMessageTransaction>().Object, new ReadOnlySequence<byte>(messageBytes), CancellationToken.None);

        // Assert
        messageQueue.Should().BeEmpty();
    }
}