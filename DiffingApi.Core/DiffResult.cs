namespace DiffingApi.Core;

/// <summary>
/// The result of comparing two byte arrays. <see cref="Diffs"/> is only populated when
/// <see cref="DiffResultType"/> is <see cref="Core.DiffResultType.ContentDoNotMatch"/>.
/// </summary>
/// <param name="DiffResultType">The outcome of the comparison.</param>
/// <param name="Diffs">
/// The list of differing byte runs, present only for <see cref="Core.DiffResultType.ContentDoNotMatch"/>.
/// Left null (rather than an empty list) for the Equals/SizeDoNotMatch cases so it is omitted
/// from the serialized JSON, matching the assignment's sample responses.
/// </param>
public sealed record DiffResult(DiffResultType DiffResultType, IReadOnlyList<DiffSegment>? Diffs = null)
{
    /// <summary>Convenience factory for the "identical inputs" outcome.</summary>
    public static DiffResult Equal() => new(DiffResultType.Equals);

    /// <summary>Convenience factory for the "different length" outcome.</summary>
    public static DiffResult SizeMismatch() => new(DiffResultType.SizeDoNotMatch);

    /// <summary>Convenience factory for the "same length, different content" outcome.</summary>
    public static DiffResult ContentMismatch(IReadOnlyList<DiffSegment> diffs) =>  
        new(DiffResultType.ContentDoNotMatch, diffs);
}