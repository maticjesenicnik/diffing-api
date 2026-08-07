using System.Collections.Concurrent;

namespace DiffingApi.Api.Storage;

/// <summary>
/// In-memory implementation of <see cref="IDiffEntryStore"/>.
/// </summary>
/// <remarks>
/// Assumption: the assignment does not ask for persistence, so data lives only for the
/// lifetime of the process. Registered as a singleton (see Program.cs), so
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> is used to make entry creation/lookup safe
/// under concurrent requests. Setting <see cref="DiffEntry.Left"/> and
/// <see cref="DiffEntry.Right"/> themselves are simple property writes rather than
/// synchronized operations - two concurrent PUTs to the *same* side of the *same* ID could
/// race, with the last write winning. That's considered acceptable here since the assignment
/// doesn't specify ordering guarantees for concurrent writes to a single side.
/// </remarks>
public sealed class InMemoryDiffEntryStore : IDiffEntryStore
{
    private readonly ConcurrentDictionary<string, DiffEntry> _entries = new();
    
    public DiffEntry GetOrCreate(string id) => _entries.GetOrAdd(id, static _ => new DiffEntry());
    
    public bool TryGet(string id, out DiffEntry? entry) => _entries.TryGetValue(id, out entry);
}