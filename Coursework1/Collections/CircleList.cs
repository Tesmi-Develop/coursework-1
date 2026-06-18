using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coursework1.Collections;

public class CircleList<TValue> : IEnumerable<TValue> where TValue : IComparable<TValue>
{
    public bool Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _head is null;
    }

    public int Count { get; private set; }
    
    private Node? _head;

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
    
    public static CircleList<TValue> operator |(CircleList<TValue> first, CircleList<TValue> second)
    {
        return Union(first, second);
    }
    
    public static CircleList<TValue> Union(CircleList<TValue> first, CircleList<TValue> second)
    {
        var result = new CircleList<TValue>();

        if (first.Empty && second.Empty)
            return result;
        
        if (first.Empty)
        {
            var node = second._head!;
            do
            {
                result.Add(node.Data);
                node = node.Next;
            } while (node != second._head);

            return result;
        }
        
        if (second.Empty)
        {
            var node = first._head!;
            do
            {
                result.Add(node.Data);
                node = node.Next;
            } while (node != first._head);
            
            return result;
        }

        var firstFinished = false;
        var secondFinished = false;
        var node1 = first._head;
        var node2 = second._head;
        
        while (!firstFinished && !secondFinished)
        {
            if (node1!.Data.CompareTo(node2!.Data) > 0)
            {
                result.Add(node1.Data);
                node1 = node1.Next;
                if (node1 == first._head) firstFinished = true;
                continue;
            }
            if (node1.Data.CompareTo(node2.Data) < 0)
            {
                result.Add(node2.Data);
                node2 = node2.Next;
                if (node2 == second._head) secondFinished = true;
                continue;
            }
            
            result.Add(node1.Data);
            result.Add(node2.Data);
            node1 = node1.Next;
            node2 = node2.Next;
            if (node1 == first._head) firstFinished = true;
            if (node2 == second._head) secondFinished = true;
        }
        
        while (!firstFinished)
        {
            result.Add(node1!.Data);
            node1 = node1.Next;
            if (node1 == first._head) firstFinished = true;
        }
        
        while (!secondFinished)
        {
            result.Add(node2!.Data);
            node2 = node2.Next;
            if (node2 == second._head) secondFinished = true;
        }

        return result;
    }
    
    public CircleList() {}
    
    public CircleList(TValue[] data)
    {
        foreach (var value in data)
            Add(value);
    }

    public void Add(TValue data)
    {
        if (Empty)
        {
            AddNode(data, null);
            return;
        }
        
        if (data.CompareTo(_head!.Previous.Data) <= 0 || data.CompareTo(_head!.Data) >= 0)
        {
            AddNode(data, _head.Previous);
            return;
        }

        var current = _head!;
        while (current.Next != _head && current.Next.Data.CompareTo(data) > 0)
            current = current.Next;
        
        AddNode(data, current);
    }
    
    public bool Replace(TValue oldValue, TValue newValue)
    {
        var node = FindNode(oldValue);
        if (node == null)
            return false;
        
        node.Data = newValue;

        return true;
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

        var current = _head!;
        do
        {
            var next = current.Next;
            if (current.Data.CompareTo(data) == 0)
                RemoveNode(current);
            
            current = next;
        } while (current != _head);
        
        if (_head.Data.CompareTo(data) == 0)
            RemoveNode(_head);
    }

    public void RemoveAllBefore(TValue data)
    {
        if (Empty) return;

        var current = _head!;
        do
        {
            if (current.Data.CompareTo(data) == 0)
                RemoveNode(current.Previous);
            
            current = current.Next;
        } while (current != _head);
    }

    public bool Find(TValue data, out TValue? foundData)
    {
        foundData = default;
        if (Empty)
            return false;

        var current = _head;
        do
        {
            if (current!.Data.CompareTo(data) == 0) {
                foundData = current.Data;
                return true;
            }

            current = current.Next;

        } while (current != _head);
        
        return false;
    }

    public void Print()
    {
        if (Empty)
        {
            Console.WriteLine("List is empty");
            return;
        }

        var current = _head!;

        do
        {
            Console.WriteLine(current.Data);
            current = current.Next;
        } while (current != _head);
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

        if (data.CompareTo(_head!.Data) >= 0 && previous.Next == _head)
            _head = node;

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