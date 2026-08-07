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

        var segments = FindDifferingSegments(left, right);
        
        return segments.Count == 0 ? DiffResult.Equal() : DiffResult.ContentMismatch(segments);
    }
    
    /// <summary>
    /// Finds all contiguous segments where <paramref name="left"/> and <paramref name="right"/> differ.
    /// </summary>
    private static List<DiffSegment> FindDifferingSegments(byte[] left, byte[] right)
    {
        var segments = new List<DiffSegment>();
        var index = 0;

        while (index < left.Length)
        {
            if (left[index] == right[index])
            {
                index++;
                continue;
            }

            var runStart = index;
            while (index < left.Length && left[index] != right[index])
            {
                index++;
            }

            segments.Add(new DiffSegment(runStart, index - runStart));
        }

        return segments;
    }
}