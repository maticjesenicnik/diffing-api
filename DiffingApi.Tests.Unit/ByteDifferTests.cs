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