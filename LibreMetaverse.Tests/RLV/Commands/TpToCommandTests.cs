using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace LibreMetaverse.Tests.RLV.Commands
{
    [TestFixture]
    public class TpToCommandTests : RlvTestBase
    {
        #region @tpto:<region_name>/<X_local>/<Y_local>/<Z_local>[;lookat]=force

        [Test]
        public async Task TpTo_Default()
        {
            _actionCallbacks
                .Setup(e => e.TpToAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<string?>(), It.IsAny<float?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessageAsync("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.TpToAsync(1.5f, 2.5f, 3.5f, null, null, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TpTo_WithRegion()
        {
            _actionCallbacks
                .Setup(e => e.TpToAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<string?>(), It.IsAny<float?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessageAsync("@tpto:Region Name/1.5/2.5/3.5=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.TpToAsync(1.5f, 2.5f, 3.5f, "Region Name", null, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TpTo_WithRegionAndLookAt()
        {
            _actionCallbacks
                .Setup(e => e.TpToAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<string?>(), It.IsAny<float?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessageAsync("@tpto:Region Name/1.5/2.5/3.5;3.1415=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.TpToAsync(1.5f, 2.5f, 3.5f, "Region Name", 3.1415f, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TpTo_RestrictedUnsit_ByAnotherObject_WhileSitting()
        {
            await _rlv.ProcessMessageAsync("@unsit=n", Guid.NewGuid(), "Other");
            _queryCallbacks.Setup(e => e.IsSittingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            Assert.That(await _rlv.ProcessMessageAsync("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name), Is.False);

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TpTo_RestrictedUnsit_ByAnotherObject_NotSitting()
        {
            await _rlv.ProcessMessageAsync("@unsit=n", Guid.NewGuid(), "Other");
            _queryCallbacks.Setup(e => e.IsSittingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _actionCallbacks
                .Setup(e => e.TpToAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<float?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Assert.That(await _rlv.ProcessMessageAsync("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name), Is.True);
        }

        [Test]
        public async Task TpTo_RestrictedTpLoc_ByAnotherObject()
        {
            await _rlv.ProcessMessageAsync("@tploc=n", Guid.NewGuid(), "Other");

            // Act
            Assert.That(await _rlv.ProcessMessageAsync("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name), Is.False);

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TpTo_OwnRestrictions_DoNotStopIt()
        {
            await _rlv.ProcessMessageAsync("@tploc=n,unsit=n", _sender.Id, _sender.Name);
            _queryCallbacks.Setup(e => e.IsSittingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _actionCallbacks
                .Setup(e => e.TpToAsync(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<string>(), It.IsAny<float?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Assert.That(await _rlv.ProcessMessageAsync("@tpto:1.5/2.5/3.5=force", _sender.Id, _sender.Name), Is.True);
        }

        #endregion
    }
}
