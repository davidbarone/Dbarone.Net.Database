/// <summary>
/// Generic Quicksort implementation.
/// Recursively sorts the array by dividing it into smaller subarrays.
/// Partition Method: Places the pivot element in its correct position and rearranges elements around it.
/// Swap Method: Swaps two elements in the array.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class QuickSort<T> where T : IComparable, IEquatable<T>
{
    public T[] Array { get; set; }

    public QuickSort(T[] arr)
    {
        Array = arr;
    }

    public QuickSort(IEnumerable<T> arr)
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
            int pivotIndex = Partition(Array, left, right);
            Sort(left, pivotIndex - 1);
            Sort(pivotIndex + 1, right);
        }
    }

    private int Partition(T[] Array, int left, int right)
    {
        T pivot = Array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (Array[j].CompareTo(pivot) <= 0)
            {
                i++;
                Swap(Array, i, j);
            }
        }

        Swap(Array, i + 1, right);
        return i + 1;
    }

    private void Swap(T[] array, int a, int b)
    {
        T temp = Array[a];
        array[a] = Array[b];
        array[b] = temp;
    }
}