/// <summary>
/// Generic MergeSort implementation.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class MergeSort<T> where T : IComparable
{
    public T[] Array { get; set; }

    public MergeSort(T[] arr)
    {
        Array = arr;
    }

    public MergeSort(IEnumerable<T> arr)
    {
        Array = arr.ToArray();
    }

    public T[] Sort()
    {
        Sort(0, Array.Length - 1);
        return Array;
    }

    private void Sort(int left, int right)
    {
        if (left < right)
        {
            int mid = (left + right) / 2;

            // Recursively sort the left and right halves
            Sort(left, mid);
            Sort(mid + 1, right);

            // Merge the sorted halves
            Merge(left, mid, right);
        }
    }

    private void Merge(int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        T[] leftArray = new T[n1];
        T[] rightArray = new T[n2];

        // Copy data to temporary arrays
        System.Array.Copy(Array, left, leftArray, 0, n1);
        System.Array.Copy(Array, mid + 1, rightArray, 0, n2);

        int i = 0, j = 0, k = left;

        // Merge the temporary arrays back into the original array
        while (i < n1 && j < n2)
        {
            if (leftArray[i].CompareTo(rightArray[j]) <= 0)
                Array[k++] = leftArray[i++];
            else
                Array[k++] = rightArray[j++];
        }

        // Copy remaining elements of leftArray, if any
        while (i < n1)
            Array[k++] = leftArray[i++];

        // Copy remaining elements of rightArray, if any
        while (j < n2)
            Array[k++] = rightArray[j++];
    }
}