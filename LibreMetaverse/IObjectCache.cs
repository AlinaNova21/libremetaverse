namespace LibreMetaverse
{
    /// <summary>The result of looking an object up in an <see cref="IObjectCache"/>.</summary>
    public enum ObjectCacheResult
    {
        /// <summary>The object is not in the cache.</summary>
        Miss,

        /// <summary>The object is in the cache, but it has changed since.</summary>
        Stale,

        /// <summary>The cached copy is current.</summary>
        Hit
    }

    /// <summary>
    /// Keeps objects between sessions, as a viewer's object cache does. A region names objects it
    /// believes the client holds, by local ID and CRC, and only sends those that do not match.
    /// Entries belong to a region and its cache ID, which changes when the cache must be dropped.
    /// </summary>
    public interface IObjectCache
    {
        /// <summary>Whether anything is held for this region. The region is told at handshake.</summary>
        bool HasEntries(ulong regionHandle, UUID cacheId);

        /// <summary>Looks up an object, giving its compressed update on a hit.</summary>
        ObjectCacheResult TryGet(ulong regionHandle, UUID cacheId, uint localId, uint crc, out byte[]? update);

        /// <summary>Keeps a compressed object update.</summary>
        void Store(ulong regionHandle, UUID cacheId, uint localId, uint crc, byte[] update);

        /// <summary>The object's PBR override message, if one was kept.</summary>
        bool TryGetOverride(ulong regionHandle, UUID cacheId, uint localId, out byte[]? message);

        /// <summary>Keeps an object's PBR override message, which arrives apart from object updates.</summary>
        void StoreOverride(ulong regionHandle, UUID cacheId, uint localId, byte[] message);
    }
}
