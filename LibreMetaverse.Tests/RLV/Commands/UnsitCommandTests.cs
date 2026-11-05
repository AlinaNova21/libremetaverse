using System;
using System.Threading;
using Moq;
using System.Threading.Tasks;
using NUnit.Framework;
namespace LibreMetaverse.Tests.RLV.Commands
{
    [TestFixture]
    public class UnsitCommandTests : RlvTestBase
    {
        #region @unsit=force

        [Test]
        public async Task ForceUnSit()
        {
            Assert.That(await _rlv.ProcessMessageAsync("@unsit=force", _sender.Id, _sender.Name), Is.True);
        }

        [Test]
        public async Task ForceUnSit_RestrictedUnsit_ByAnotherObject()
        {
            await _rlv.ProcessMessageAsync("@unsit=n", Guid.NewGuid(), "Other");

            Assert.That(await _rlv.ProcessMessageAsync("@unsit=force", _sender.Id, _sender.Name), Is.False);
        }

        [Test]
        public async Task ForceUnSit_OwnRestriction_DoesNotStopIt()
        {
            await _rlv.ProcessMessageAsync("@unsit=n", _sender.Id, _sender.Name);
            _actionCallbacks.Setup(e => e.UnsitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Assert.That(await _rlv.ProcessMessageAsync("@unsit=force", _sender.Id, _sender.Name), Is.True);
        }

        #endregion
    }
}
