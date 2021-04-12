using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SmtpServer;
using SmtpServer.Storage;
using Xunit;

namespace SmtpToRestService.UnitTests
{
    public class SmtpServerBackgroundServiceTests
    {
        [Fact]
        public async Task ProcessMessages_ShouldNotInvokeRestClient_WhenMappingNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<SmtpServerBackgroundService>>();
            var config = new Mock<IConfiguration>();
            var messageStoreFactory = new Mock<IMessageStoreFactory>();
            BlockingCollection<IMimeMessage> messageQueue = null;
            messageStoreFactory.Setup(f => f.Create(It.IsAny<ILogger<SmtpServerBackgroundService>>(), It.IsAny<BlockingCollection<IMimeMessage>>()))
                .Returns(new Mock<IMessageStore>().Object)
                .Callback((ILogger<SmtpServerBackgroundService> _, BlockingCollection <IMimeMessage> mq) => { messageQueue = mq; });
            var smtpServerFactory = new Mock<ISmtpServerFactory>();
            smtpServerFactory.Setup(f => f.Create(It.IsAny<ISmtpServerOptions>(), It.IsAny<IServiceProvider>())).Returns(new Mock<ISmtpServer>().Object);
            var restClient = new Mock<IRestClient>();
            var tokenSource = new CancellationTokenSource();
            var sync = new ManualResetEventSlim();
            var service = new SmtpServerBackgroundService(logger.Object, config.Object, messageStoreFactory.Object, smtpServerFactory.Object, restClient.Object);
            service.MessageProcessed += (sender, args) => sync.Set();
            var serviceTask = service.StartAsync(tokenSource.Token);

            var message = new Mock<IMimeMessage>();
            message.SetupGet(m => m.Address).Returns("uknown@address.com");

            // Act
            messageQueue.Add(message.Object, CancellationToken.None);
            sync.Wait(CancellationToken.None);
            
            // Cleanup
            tokenSource.Cancel();
            await serviceTask;

            // Assert
            restClient.Verify(c => c.InvokeService(It.IsAny<ConfigurationMapping>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessages_ShouldInvokeRestClient_WhenMappingFound()
        {
            // Arrange
            var knownAddress = "some@address.com";
            var mapping = new ConfigurationMapping();
            var logger = new Mock<ILogger<SmtpServerBackgroundService>>();
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.TryGetMapping(knownAddress, out mapping)).Returns(true);
            var messageStoreFactory = new Mock<IMessageStoreFactory>();
            BlockingCollection<IMimeMessage> messageQueue = null;
            messageStoreFactory.Setup(f => f.Create(It.IsAny<ILogger<SmtpServerBackgroundService>>(), It.IsAny<BlockingCollection<IMimeMessage>>()))
                .Returns(new Mock<IMessageStore>().Object)
                .Callback((ILogger<SmtpServerBackgroundService> _, BlockingCollection<IMimeMessage> mq) => { messageQueue = mq; });
            var smtpServerFactory = new Mock<ISmtpServerFactory>();
            smtpServerFactory.Setup(f => f.Create(It.IsAny<ISmtpServerOptions>(), It.IsAny<IServiceProvider>())).Returns(new Mock<ISmtpServer>().Object);
            var restClient = new Mock<IRestClient>();
            var tokenSource = new CancellationTokenSource();
            var sync = new ManualResetEventSlim();
            var service = new SmtpServerBackgroundService(logger.Object, config.Object, messageStoreFactory.Object, smtpServerFactory.Object, restClient.Object);
            service.MessageProcessed += (sender, args) => sync.Set();
            var serviceTask = service.StartAsync(tokenSource.Token);

            var message = new Mock<IMimeMessage>();
            message.SetupGet(m => m.Address).Returns(knownAddress);

            // Act
            messageQueue.Add(message.Object, CancellationToken.None);
            sync.Wait(CancellationToken.None);

            // Cleanup
            tokenSource.Cancel();
            await serviceTask;

            // Assert
            restClient.Verify(c => c.InvokeService(mapping, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}