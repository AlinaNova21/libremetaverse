using System.Collections.Generic;
using System.Linq;
using LibreMetaverse.Packets;
using NUnit.Framework;

namespace LibreMetaverse.Tests
{
    [TestFixture]
    [Category("Objects")]
    public class ObjectCacheTests
    {
        private sealed class FakeCache : IObjectCache
        {
            public readonly Dictionary<uint, (uint Crc, byte[] Update)> Objects = new();

            public bool HasEntries(ulong regionHandle, UUID cacheId) => Objects.Count > 0;

            public ObjectCacheResult TryGet(ulong regionHandle, UUID cacheId, uint localId, uint crc, out byte[]? update)
            {
                update = null;
                if (!Objects.TryGetValue(localId, out var held)) { return ObjectCacheResult.Miss; }
                if (held.Crc != crc) { return ObjectCacheResult.Stale; }
                update = held.Update;
                return ObjectCacheResult.Hit;
            }

            public void Store(ulong regionHandle, UUID cacheId, uint localId, uint crc, byte[] update) => Objects[localId] = (crc, update);
            public bool TryGetOverride(ulong regionHandle, UUID cacheId, uint localId, out byte[]? message) { message = null; return false; }
            public void StoreOverride(ulong regionHandle, UUID cacheId, uint localId, byte[] message) { }
        }

        private static ObjectUpdateCachedPacket.ObjectDataBlock Notice(uint id, uint crc) =>
            new ObjectUpdateCachedPacket.ObjectDataBlock { ID = id, CRC = crc, UpdateFlags = 7 };

        [Test]
        public void Notices_split_into_hits_and_typed_misses()
        {
            var cache = new FakeCache();
            cache.Objects[1] = (100, new byte[] { 1 });
            cache.Objects[2] = (200, new byte[] { 2 });

            var plan = ObjectManager.PlanCachedNotices(cache, 1, UUID.Zero, new[] { Notice(1, 100), Notice(2, 999), Notice(3, 300) });

            Assert.That(plan.Hits.Select(h => h.Id), Is.EqualTo(new uint[] { 1 }));
            Assert.That(plan.Hits[0].Flags, Is.EqualTo(7u));
            Assert.That(plan.Misses, Is.EquivalentTo(new (uint, byte)[] { (2, ObjectManager.CacheMissCrc), (3, ObjectManager.CacheMissTotal) }));
        }

        [Test]
        public void Miss_types_match_the_viewer()
        {
            Assert.That(ObjectManager.CacheMissTotal, Is.EqualTo(0));
            Assert.That(ObjectManager.CacheMissCrc, Is.EqualTo(1));
        }

        [Test]
        public void Crc_is_read_after_the_object_header()
        {
            var data = new byte[30];
            Utils.UIntToBytes(0xAABBCCDD).CopyTo(data, 22);

            Assert.That(ObjectManager.CompressedUpdateCrc(data), Is.EqualTo(0xAABBCCDD));
        }
    }
}
