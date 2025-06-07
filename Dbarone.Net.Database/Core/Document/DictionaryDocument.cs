using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dbarone.Net.Extensions;

namespace Dbarone.Net.Database;

/// <summary>
/// Represents a document as a dictionary of string / <see cref="DocumentValue"/> pairs.
/// </summary>
public class DictionaryDocument : DocumentValue, IDictionary<string, DocumentValue>
{
    /// <summary>
    /// Creates an empty document.
    /// </summary>
    public DictionaryDocument()
        : base(DocumentType.Document, new Dictionary<string, DocumentValue>(StringComparer.OrdinalIgnoreCase))
    {
    }

    /// <summary>
    /// Creates a new document using a dictionary of values.
    /// </summary>
    /// <param name="dict">The dictionary containing the values.</param>
    /// <exception cref="ArgumentNullException">Throws an error if a null dictionary value is passed in.</exception>
    public DictionaryDocument(ConcurrentDictionary<string, DocumentValue> dict)
        : this()
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));

        foreach (var element in dict)
        {
            this.Add(element);
        }
    }

    /// <summary>
    /// Creates a new document using a dictionary of values.
    /// </summary>
    /// <param name="dict">The dictionary containing the values.</param>
    /// <exception cref="ArgumentNullException">Throws an error if a null dictionary value is passed in.</exception>
    public DictionaryDocument(IDictionary<string, DocumentValue> dict)
        : this()
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));

        foreach (var element in dict)
        {
            this.Add(element);
        }
    }

    /// <summary>
    /// Returns the raw value of the document, as an IDictionary.
    /// </summary>
    public new IDictionary<string, DocumentValue> RawValue => base.RawValue as IDictionary<string, DocumentValue>;

    /// <summary>
    /// Get / set a field for document. Fields are case sensitive
    /// </summary>
    public override DocumentValue this[string key]
    {
        get
        {
            return this.RawValue.GetOrDefault(key, DocumentValue.Null);
        }
        set
        {
            this.RawValue[key] = value ?? DocumentValue.Null;
        }
    }

    #region CompareTo

    public override int CompareTo(DocumentValue other)
    {
        // if types are different, returns sort type order
        if (other.Type != DocumentType.Document) return this.Type.CompareTo(other.Type);

        var thisKeys = this.Keys.ToArray();
        var thisLength = thisKeys.Length;

        var otherDoc = other.AsDocument;
        var otherKeys = otherDoc.Keys.ToArray();
        var otherLength = otherKeys.Length;

        var result = 0;
        var i = 0;
        var stop = Math.Min(thisLength, otherLength);

        for (; 0 == result && i < stop; i++)
            result = this[thisKeys[i]].CompareTo(otherDoc[thisKeys[i]]);

        // are different
        if (result != 0) return result;

        // test keys length to check which is bigger
        if (i == thisLength) return i == otherLength ? 0 : -1;

        return 1;
    }

    #endregion

    #region IDictionary

    public ICollection<string> Keys => this.RawValue.Keys;

    public ICollection<DocumentValue> Values => this.RawValue.Values;

    public int Count => this.RawValue.Count;

    public bool IsReadOnly => false;

    public bool ContainsKey(string key) => this.RawValue.ContainsKey(key);

    /// <summary>
    /// Get all document elements - Return "_id" as first of all (if exists)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, DocumentValue>> GetElements()
    {
        if (this.RawValue.TryGetValue("_id", out var id))
        {
            yield return new KeyValuePair<string, DocumentValue>("_id", id);
        }

        foreach (var item in this.RawValue.Where(x => x.Key != "_id"))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Adds a new member to the document.
    /// </summary>
    /// <param name="key">The new member key.</param>
    /// <param name="value">The new member value.</param>
    public void Add(string key, DocumentValue value) => this.RawValue.Add(key, value ?? DocumentValue.Null);

    /// <summary>
    /// Removes a member from the document.
    /// </summary>
    /// <param name="key">The member key to remove.</param>
    public bool Remove(string key) => this.RawValue.Remove(key);

    public void Clear() => this.RawValue.Clear();

    public bool TryGetValue(string key, out DocumentValue value) => this.RawValue.TryGetValue(key, out value);

    public void Add(KeyValuePair<string, DocumentValue> item) => this.Add(item.Key, item.Value);

    public bool Contains(KeyValuePair<string, DocumentValue> item) => this.RawValue.Contains(item);

    public bool Remove(KeyValuePair<string, DocumentValue> item) => this.Remove(item.Key);

    public IEnumerator<KeyValuePair<string, DocumentValue>> GetEnumerator() => this.RawValue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.RawValue.GetEnumerator();

    public void CopyTo(KeyValuePair<string, DocumentValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, DocumentValue>>)this.RawValue).CopyTo(array, arrayIndex);
    }

    public void CopyTo(DictionaryDocument other)
    {
        foreach (var element in this)
        {
            other[element.Key] = element.Value;
        }
    }

    #endregion

    private int _length = 0;

    /// <summary>
    /// Gets the byte count of the document.
    /// </summary>
    /// <param name="recalc">Set to true to recalc.</param>
    /// <returns>Returns the document byte count.</returns>
    internal override int GetBytesCount(bool recalc)
    {
        if (recalc == false && _length > 0) return _length;

        var length = 5;

        foreach (var element in this.RawValue)
        {
            length += this.GetBytesCountElement(element.Key, element.Value);
        }

        return _length = length;
    }
}
