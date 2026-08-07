namespace DiffingApi.Core;

/// <summary>
/// A single contiguous run of bytes that differs between the two compared inputs.
/// </summary>
/// <param name="Offset">Zero-based byte index where this differing run starts.</param>
/// <param name="Length">Number of consecutive bytes in this differing run.</param>
public sealed record DiffSegment(int Offset, int Length);