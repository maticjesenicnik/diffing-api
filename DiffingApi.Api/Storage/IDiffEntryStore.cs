namespace DiffingApi.Api.Storage;

/// <summary>
/// Stores diff entries keyed by ID. Intentionally minimal - just enough surface area for the
/// three endpoints the assignment requires.
/// </summary>
public interface IDiffEntryStore
{
    /// <summary>Gets the entry for <paramref name="id"/>, creating an empty one if it doesn't exist yet.</summary>
    DiffEntry GetOrCreate(string id);

    /// <summary>Attempts to fetch an existing entry without creating one.</summary>
    bool TryGet(string id, out DiffEntry? entry);
}