namespace Coursework1;

using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicArray<T> : ICollection<T>
{
    private T[] _items;
    private int _size;
    private const int DefaultCapacity = 4;
    
    public DynamicArray()
    {
        _items = new T[DefaultCapacity];
        _size = 0;
    }
    
    public DynamicArray(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
        
        _items = new T[capacity];
        _size = 0;
    }
    
    public int Count => _size;
    public int Capacity => _items.Length;
    
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"Index {index} out of range.");
            return _items[index];
        }
        set
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"Index {index} out of range.");
            _items[index] = value;
        }
    }
    
    public void Add(T item)
    {
        if (_size == _items.Length)
            Resize(_items.Length * 2);
        
        _items[_size++] = item;
    }
    
    public bool RemoveAt(int index)
    {
        if (index < 0 || index >= _size)
            return false;

        _size--;
        
        if (index < _size)
            Array.Copy(_items, index + 1, _items, index, _size - index);
        
        _items[_size] = default!;
        return true;
    }
    
    public bool RemoveAtUnordered(int index)
    {
        if (index < 0 || index >= _size)
            return false;

        _size--;
        
        if (index < _size)
            _items[index] = _items[_size];
        
        _items[_size] = default!;
        return true;
    }
    
    public void Insert(int index, T item)
    {
        if (index < 0 || index > _size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_size == _items.Length)
            Resize(_items.Length * 2);

        if (index < _size)
            Array.Copy(_items, index, _items, index + 1, _size - index);

        _items[index] = item;
        _size++;
    }
    
    public void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_items, 0, _size);
            _size = 0;
        }
    }
    
    public bool Contains(T item)
    {
        for (var i = 0; i < _size; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_items[i], item))
                return true;
        }
        return false;
    }
    
    public int IndexOf(T item)
    {
        for (var i = 0; i < _size; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_items[i], item))
                return i;
        }
        return -1;
    }
    
    public void TrimExcess()
    {
        if (_size < _items.Length)
            Resize(_size);
    }
    
    private void Resize(int newCapacity)
    {
        var newItems = new T[newCapacity];
        Array.Copy(_items, 0, newItems, 0, _size);
        _items = newItems;
    }
    

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < _size; i++)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public bool IsReadOnly => false;

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public T[] ToArray()
    {
        var result = new T[_size];
        Array.Copy(_items, 0, result, 0, _size);

        return result;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _size)
            throw new ArgumentException();

        Array.Copy(_items, 0, array, arrayIndex, _size);
    }
}