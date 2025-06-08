using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dbarone.Net.Database;

public class Table : TableCell, IList<TableCell>
{
    public Table()
        : base(DocumentType.Array, new List<TableCell>())
    {
    }

    public Table(List<TableCell> array)
        : this()
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        this.AddRange(array);
    }

    public Table(params TableCell[] array)
        : this()
    {
        if (array == null) throw new ArgumentNullException(nameof(array));

        this.AddRange(array);
    }

    public Table(IEnumerable<TableCell> items)
        : this()
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        this.AddRange(items);
    }

    public new IList<TableCell> RawValue => (IList<TableCell>)base.RawValue;

    public override TableCell this[int index]
    {
        get
        {
            return this.RawValue[index];
        }
        set
        {
            this.RawValue[index] = value ?? TableCell.Null;
        }
    }

    public int Count => this.RawValue.Count;

    public bool IsReadOnly => false;

    public void Add(TableCell item) => this.RawValue.Add(item ?? TableCell.Null);

    public void AddRange<TCollection>(TCollection collection)
        where TCollection : ICollection<TableCell>
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        var list = (List<TableCell>)base.RawValue;

        var listEmptySpace = list.Capacity - list.Count;
        if (listEmptySpace < collection.Count)
        {
            list.Capacity += collection.Count;
        }

        foreach (var bsonValue in collection)
        {
            list.Add(bsonValue ?? Null);
        }

    }

    public void AddRange(IEnumerable<TableCell> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            this.Add(item ?? TableCell.Null);
        }
    }

    public void Clear() => this.RawValue.Clear();

    public bool Contains(TableCell item) => this.RawValue.Contains(item ?? TableCell.Null);

    public void CopyTo(TableCell[] array, int arrayIndex) => this.RawValue.CopyTo(array, arrayIndex);

    public IEnumerator<TableCell> GetEnumerator() => this.RawValue.GetEnumerator();

    public int IndexOf(TableCell item) => this.RawValue.IndexOf(item ?? TableCell.Null);

    public void Insert(int index, TableCell item) => this.RawValue.Insert(index, item ?? TableCell.Null);

    public bool Remove(TableCell item) => this.RawValue.Remove(item);

    public void RemoveAt(int index) => this.RawValue.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var value in this.RawValue)
        {
            yield return value;
        }
    }

    public override int CompareTo(TableCell other)
    {
        // if types are different, returns sort type order
        if (other.Type != DocumentType.Array) return this.Type.CompareTo(other.Type);

        var otherArray = other.AsArray;

        var result = 0;
        var i = 0;
        var stop = Math.Min(this.Count, otherArray.Count);

        // compare each element
        for (; 0 == result && i < stop; i++)
            result = this[i].CompareTo(otherArray[i]);

        if (result != 0) return result;
        if (i == this.Count) return i == otherArray.Count ? 0 : -1;
        return 1;
    }

    private int _length;

    internal override int GetBytesCount(bool recalc)
    {
        if (recalc == false && _length > 0) return _length;

        var length = 5;
        var array = this.RawValue;

        for (var i = 0; i < array.Count; i++)
        {
            length += this.GetBytesCountElement(i.ToString(), array[i]);
        }

        return _length = length;
    }
}
