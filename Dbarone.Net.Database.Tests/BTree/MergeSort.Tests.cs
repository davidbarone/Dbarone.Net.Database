using Xunit;

public class MergeSortTests
{
    [Theory]
    [InlineData(new int[] { }, new int[] { })]
    [InlineData(new int[] { 1 }, new int[] { 1 })]
    [InlineData(new int[] { 5, 4, 3, 2, 1 }, new int[] { 1, 2, 3, 4, 5 })]
    [InlineData(new int[] { 10, 19, 5, 3, 7, 21, 11 }, new int[] { 3, 5, 7, 10, 11, 19, 21 })]
    public void TestSort(int[] array, int[] expected)
    {
        var qs = new MergeSort<int>(array);
        var sorted = qs.Sort();
        Assert.Equal(expected, sorted);
    }
}