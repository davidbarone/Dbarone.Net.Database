/// <summary>
/// Generic Binary Search algorithm. Finds the position of an element in a sorted array.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BinarySearch<T> where T : IComparable, IEquatable<T>
{
    public T[] Array { get; set; }
    public BinarySearch(T[] arr)
    {
        Array = arr;
    }

    public BinarySearch(IEnumerable<T> arr)
    {
        Array = arr.ToArray();
    }


    /// <summary>
    /// Searches for key in an array and returns the index if matched.
    /// Returns -1 if no match.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int Search(T key)
    {
        return Search(0, Array.Length - 1, key);
    }

    private int Search(int left, int right, T key)
    {

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (Array[mid].Equals(key))
            {
                return mid;
            }

            if (Array[mid].CompareTo(key) > 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return -1;
    }
}