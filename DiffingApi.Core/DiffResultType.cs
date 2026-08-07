namespace DiffingApi.Core;

/// <summary>
/// Describes the outcome of comparing two byte arrays.
/// </summary>
public enum DiffResultType
{
    /// <summary>The two inputs are byte-for-byte identical.</summary>
    Equals,

    /// <summary>The two inputs have different lengths. Per the assignment spec we just return that.</summary>
    SizeDoNotMatch,

    /// <summary>The two inputs are the same length but differ in content at one or more places.</summary>
    ContentDoNotMatch
}