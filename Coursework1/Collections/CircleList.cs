using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coursework1.Collections;

public class CircleList<TValue> : IDisposable, IEnumerable<TValue>
{
    private class Node
    {
        public Node(TValue data, Node next, Node previous)
        {
            Data = data;
            Next = next;
            Previous = previous;
        }

        public TValue Data { get; set; } 
        public Node Next { get; set; }
        public Node Previous { get; set; }
    }
    
    public bool Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _head is null;
    }

    public int Count { get; private set; }
    
    private Node? _head;

    public CircleList() {}
    
    public CircleList(TValue[] data)
    {
        foreach (var value in data)
            Add(value);
    }
    
    public void Add(TValue data)
    {
        AddNode(data, Empty ? null : _head!.Previous);
    }
    
    public bool Replace(TValue oldValue, TValue newValue)
    {
        var node = FindNode(oldValue);
        if (node == null)
            return false;
        
        node.Data = newValue;

        return true;
    }
    
    public bool TryRemove(TValue data, [MaybeNullWhen(false)] out TValue removed)
    {
        var current = _head;
        removed = default;
        
        while (current != null)
        {
            if (EqualityComparer<TValue>.Default.Equals(current.Data, data))
            {
                RemoveNode(current);
                removed = current.Data;
                return true;
            }
            current = current.Next;
        }
        
        return false;
    }

    public void RemoveAll(TValue data)
    {
        if (Empty) return;

        var comparer = EqualityComparer<TValue>.Default;
        var current = _head!;
        do
        {
            var next = current.Next;
            if (comparer.Equals(current.Data, data))
                RemoveNode(current);
            
            current = next;
        } while (current != _head && !Empty);
    }

    public void RemoveAllBefore(TValue data)
    {
        if (Empty) return;

        var comparer = EqualityComparer<TValue>.Default;
        var current = _head!;
        do
        {
            if (comparer.Equals(current.Data, data))
            {
                RemoveNode(current.Previous);
            }
            
            current = current.Next;
        } while (current != _head && !Empty);
    }

    public bool TryFind(TValue data, out TValue? foundData)
    {
        foundData = default;
        
        if (Empty)
            return false;

        var comparer = EqualityComparer<TValue>.Default;
        var current = _head;
        do
        {
            if (comparer.Equals(current!.Data, data)) {
                foundData = current.Data;
                return true;
            }

            current = current.Next;

        } while (current != _head);
        
        return false;
    }
    
    public void Clear()
    {
        if (Empty)
            return;

        var current = _head!;
        do
        {
            var next = current.Next;
            current.Next = null!;
            current.Previous = null!;
            current = next;
        } while (current != _head);

        _head = null;
        Count = 0;
    }
    
    public void Dispose()
    {
        Clear();
    }
    
    private Node? FindNode(TValue data)
    {
        var current = _head;
        while (current != null)
        {
            if (EqualityComparer<TValue>.Default.Equals(current.Data, data))
                return current;
            current = current.Next;
        }
        return null;
    }
    
    private void AddNode(TValue data, Node? previous)
    {
        Count++;
        Node node;
        
        if (previous is null)
        {
            node = new Node(data, null!, null!);
            node.Next = node;
            node.Previous = node;
            _head = node;
            return;
        }

        node = new Node(data, previous.Next, previous);
        previous.Next.Previous = node;
        previous.Next = node;
    }
    
    private void RemoveNode(Node node)
    {
        Count--;
        
        if (node.Next == node)
        {
            _head = null;
            return;
        }

        if (node == _head)
            _head = _head.Next;

        node.Previous.Next = node.Next;
        node.Next.Previous = node.Previous;

        node.Next = null!;
        node.Previous = null!;
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        if (Empty) 
            yield break;

        var current = _head!;
        do
        {
            yield return current.Data;
            current = current.Next;
        } while (current != _head); 
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}