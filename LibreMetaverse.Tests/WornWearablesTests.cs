using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace LibreMetaverse.Tests
{
    /// <summary>
    /// The Current Outfit folder holds links, but fetching it also returns the items they point to.
    /// Each worn item must still appear once in the wearables list, as
    /// <see cref="Appearance.CurrentOutfitFolder"/> builds a dictionary keyed on item ID from it.
    /// </summary>
    [TestFixture]
    [Category("Inventory")]
    public class WornWearablesTests
    {
        private GridClient _client;
        private Inventory _store;

        [SetUp]
        public void SetUp()
        {
            _client = new GridClient();
            _store = new Inventory(_client, UUID.Random());
            typeof(InventoryManager)
                .GetField("_Store", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_client.Inventory, _store);
        }

        [TearDown]
        public void TearDown()
        {
            try { _client.Dispose(); } catch { }
        }

        private InventoryWearable Wearable(WearableType type = WearableType.Alpha)
        {
            var item = new InventoryWearable(UUID.Random())
            {
                AssetType = AssetType.Clothing,
                WearableType = type,
                AssetUUID = UUID.Random()
            };
            _store[item.UUID] = item;
            return item;
        }

        private static InventoryWearable LinkTo(InventoryWearable target) => new InventoryWearable(UUID.Random())
        {
            AssetType = AssetType.Link,
            WearableType = target.WearableType,
            AssetUUID = target.UUID
        };

        private UUID[] Worn() => _client.Appearance.GetWearables().Select(w => w.ItemID).ToArray();

        [Test]
        public void LinkReturnedWithItsTarget_IsWornOnce()
        {
            var target = Wearable();

            _client.Appearance.UpdateWornFromContents(new List<InventoryBase> { LinkTo(target), target });

            Assert.That(Worn(), Is.EqualTo(new[] { target.UUID }));
        }

        [Test]
        public void TargetReturnedBeforeItsLink_IsWornOnce()
        {
            var target = Wearable();

            _client.Appearance.UpdateWornFromContents(new List<InventoryBase> { target, LinkTo(target) });

            Assert.That(Worn(), Is.EqualTo(new[] { target.UUID }));
        }

        [Test]
        public void MisclassifiedLinkReturnedWithItsTarget_IsWornOnce()
        {
            var target = Wearable();
            var link = new InventoryTexture(UUID.Random()) { AssetType = AssetType.Link, AssetUUID = target.UUID };

            _client.Appearance.UpdateWornFromContents(new List<InventoryBase> { link, target });

            Assert.That(Worn(), Is.EqualTo(new[] { target.UUID }));
        }

        [Test]
        public void DifferentLayersOfOneType_AreAllWornInOrder()
        {
            var bottom = Wearable();
            var top = Wearable();

            _client.Appearance.UpdateWornFromContents(
                new List<InventoryBase> { LinkTo(bottom), LinkTo(top), bottom, top });

            Assert.That(Worn(), Is.EqualTo(new[] { bottom.UUID, top.UUID }));
        }
    }
}
