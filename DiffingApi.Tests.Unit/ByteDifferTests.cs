using DiffingApi.Core;

namespace DiffingApi.Tests.Unit;

public class ByteDifferTests
{
    [Fact]
    public void Compare_IdenticalArrays_ReturnsEquals()
    {
        byte[] left = [1, 2, 3, 4];
        byte[] right = [1, 2, 3, 4];
        
        var result = ByteDiffer.Compare(left, right);
        
        Assert.Equal(DiffResultType.Equals, result.DiffResultType);
        Assert.Null(result.Diffs);
    }
    
    [Fact]
    public void Compare_BothEmpty_ReturnsEquals()
    {
        var result = ByteDiffer.Compare([], []);

        Assert.Equal(DiffResultType.Equals, result.DiffResultType);
        Assert.Null(result.Diffs);
    }
    
    [Theory]
    [InlineData(2, 4)]
    [InlineData(4, 2)]
    [InlineData(0, 3)]
    public void Compare_DifferentLengths_ReturnsSizeDoNotMatch(int leftLength, int rightLength)
    {
        var left = new byte[leftLength];
        var right = new byte[rightLength];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.SizeDoNotMatch, result.DiffResultType);
        Assert.Null(result.Diffs);
    }
    
    [Fact]
    public void Compare_SizeMismatch_DoesNotReportDiffs_EvenWhenContentAlsoDiffers()
    {
        // Per the spec: "If not of equal size just return that" - size mismatch short-circuits
        // before any content comparison happens, regardless of what the content looks like.
        byte[] left = [1, 2, 3];
        byte[] right = [9, 9];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.SizeDoNotMatch, result.DiffResultType);
        Assert.Null(result.Diffs);
    }
    
    [Fact]
    public void Compare_SingleByteDiffersAtStart_ReturnsOneSegment()
    {
        byte[] left = [0, 5, 5, 5];
        byte[] right = [9, 5, 5, 5];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.ContentDoNotMatch, result.DiffResultType);
        Assert.Equal([new DiffSegment(0, 1)], result.Diffs);
    }

    [Fact]
    public void Compare_SingleByteDiffersAtEnd_ReturnsOneSegmentAtCorrectOffset()
    {
        byte[] left = [5, 5, 5, 0];
        byte[] right = [5, 5, 5, 9];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.ContentDoNotMatch, result.DiffResultType);
        Assert.Equal([new DiffSegment(3, 1)], result.Diffs);
    }

    [Fact]
    public void Compare_EveryByteDiffers_ReturnsOneSegmentSpanningWholeArray()
    {
        byte[] left = [1, 1, 1, 1];
        byte[] right = [2, 2, 2, 2];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.ContentDoNotMatch, result.DiffResultType);
        Assert.Equal([new DiffSegment(0, 4)], result.Diffs);
    }

    [Fact]
    public void Compare_NonContiguousDiffs_AreMergedIntoSeparateRuns()
    {
        // This is the exact scenario from the assignment's own sample:
        // left  = AAAAAA== -> 00 00 00 00
        // right = AQABAQ== -> 01 00 01 01
        // Byte-by-byte: differ, same, differ, differ -> expected [{0,1},{2,2}], NOT four
        // separate single-byte entries.
        byte[] left = [0x00, 0x00, 0x00, 0x00];
        byte[] right = [0x01, 0x00, 0x01, 0x01];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.ContentDoNotMatch, result.DiffResultType);
        Assert.Equal([new DiffSegment(0, 1), new DiffSegment(2, 2)], result.Diffs);
    }

    [Fact]
    public void Compare_MultipleSeparateRuns_AreAllReported()
    {
        byte[] left = [0, 0, 0, 0, 0, 0, 0, 0];
        byte[] right = [9, 9, 0, 0, 9, 0, 9, 9];

        var result = ByteDiffer.Compare(left, right);

        Assert.Equal(DiffResultType.ContentDoNotMatch, result.DiffResultType);
        Assert.Equal(
            [new DiffSegment(0, 2), new DiffSegment(4, 1), new DiffSegment(6, 2)],
            result.Diffs);
    }
    
    [Fact]
    public void Compare_NullLeft_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ByteDiffer.Compare(null!, []));
    }

    [Fact]
    public void Compare_NullRight_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ByteDiffer.Compare([], null!));
    }
}