namespace DiffingApi.Core;

public static class ByteDiffer
{
    /// <summary>
    /// Compares <paramref name="left"/> and <paramref name="right"/> byte-for-byte.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if either input is null.</exception>
    public static DiffResult Compare(byte[] left, byte[] right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Length != right.Length)
        {
            return DiffResult.SizeMismatch();
        }

        // Temporary implementation before creating the method for checking differing segments
        return left.AsSpan().SequenceEqual(right) ? DiffResult.Equal() : DiffResult.ContentMismatch([]);
    }
}