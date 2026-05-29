using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Coursework1;

public class DoubleLinkedList<T> : IEnumerable<T>
{
    public bool Empty => Head is null;
    public int Count { get; private set; }

    public Node? Head { get; private set; }
    public Node? Tail { get; private set; }

    public class Node
    {
        public Node(T data, Node? next = null, Node? previous = null)
        {
            Data = data;
            Next = next;
            Previous = previous;
        }

        public T Data { get; internal set; }
        public Node? Next { get; internal set; }
        public Node? Previous { get; internal set; }
    }

    public DoubleLinkedList() { }

    public DoubleLinkedList(T[] data)
    {
        foreach (var value in data)
            Add(value);
    }
    
    public void Add(T data)
    {
        AddLast(data);
    }
    
    public void AddFirst(T data)
    {
        var newNode = new Node(data, Head, null);
        if (Head != null)
            Head.Previous = newNode;
        else
            Tail = newNode;

        Head = newNode;
        Count++;
    }
    
    public void AddLast(T data)
    {
        if (Empty)
        {
            AddFirst(data);
            return;
        }

        var newNode = new Node(data, null, Tail);
        Tail!.Next = newNode;
        Tail = newNode;
        Count++;
    }
    
    public bool Replace(T oldValue, T newValue)
    {
        var node = FindNode(oldValue);
        if (node == null)
            return false;
        
        node.Data = newValue;

        return true;
    }
    
    public bool TryRemove(T data, [MaybeNullWhen(false)] out T removed)
    {
        var current = Head;
        removed = default;
        
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, data))
            {
                RemoveNode(current);
                removed = current.Data;
                return true;
            }
            current = current.Next;
        }
        
        return false;
    }
    
    private Node? FindNode(T data)
    {
        var current = Head;
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, data))
                return current;
            current = current.Next;
        }
        return null;
    }
    
    public bool TryFind(T data, out Node? foundNode)
    {
        foundNode = FindNode(data);
        return foundNode != null;
    }
    
    private void RemoveNode(Node node)
    {
        if (node.Previous != null)
            node.Previous.Next = node.Next;
        else
            Head = node.Next;

        if (node.Next != null)
            node.Next.Previous = node.Previous;
        else
            Tail = node.Previous;

        node.Next = null;
        node.Previous = null;
        Count--;
    }
    
    public void Clear()
    {
        var current = Head;
        while (current != null)
        {
            var next = current.Next;
            current.Next = null;
            current.Previous = null;
            current = next;
        }
        Head = null;
        Tail = null;
        Count = 0;
    }
    
    public IEnumerable<T> AsEnumerable()
    {
        var current = Head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }
    
    public T[] ToArray()
    {
        if (Empty)
            return [];

        var array = new T[Count];
        var current = Head;
        var index = 0;

        while (current != null)
        {
            array[index++] = current.Data;
            current = current.Next;
        }

        return array;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var current = Head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}