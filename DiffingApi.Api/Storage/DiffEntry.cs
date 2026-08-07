namespace DiffingApi.Api.Storage;

/// <summary>
/// Holds the left and right binary payloads uploaded for a given diff ID. Either side may be
/// unset (null) until the corresponding PUT endpoint has been called at least once.
/// </summary>
public sealed class DiffEntry
{
    public byte[]? Left { get; set; }

    public byte[]? Right { get; set; }

    /// <summary>True, once both sides have been uploaded, i.e., the entry is ready to be diffed.</summary>
    public bool CanCompare => Left is not null && Right is not null;
}