using Xunit;
using Xunit.Sdk;

public class BinarySearchTests
{
    [Theory]
    [InlineData(new int[] { }, 1, -1)] // empty array. Should return -1
    [InlineData(new int[] { 1 }, 1, 0)] // test with 1 item in array. Should return 0
    [InlineData(new int[] { 2, 5, 6, 9 }, 6, 2)]    // Even number in array
    [InlineData(new int[] { 2, 5, 6, 9, 12 }, 9, 3)]    // Odd number in array
    [InlineData(new int[] { 2, 5, 6, 6, 9, 12 }, 6, 2)]    // duplicates - finds 1st match
    public void BinarySearchTestTheory(int[] array, int key, int expectedIndex)
    {
        BinarySearch<int> bs = new BinarySearch<int>(array);
        var actual = bs.Search(key);
        Assert.Equal(expectedIndex, actual);
    }
}