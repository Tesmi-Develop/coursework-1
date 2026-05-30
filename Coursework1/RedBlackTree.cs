using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Coursework1;

public enum Color : byte { Red, Black }

public class RedBlackTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private readonly Node _nil;
    private Node _root;

    public class Node
    {
        public TKey Key;
        public Color Color;
        public Node Left = null!;
        public Node Right = null!;
        public Node Parent = null!;
        public DoubleLinkedList<TValue> Values = [];

        public Node(TKey key)
        {
            Key = key;
            Color = Color.Red;
        }
    }
    
    public RedBlackTree()
    {
        _nil = new Node(default!) { Color = Color.Black };
        _root = _nil;
    }
    
    public string ToVisualString()
    {
        if (_root == _nil)
            return "Tree is empty";

        var sb = new StringBuilder();
        BuildString(_root, sb, "", true, true);
        return sb.ToString();
    }

    private void BuildString(Node node, StringBuilder sb, string indent, bool isLeft, bool isRoot)
    {
        if (node == _nil)
            return;

        if (node.Right != _nil)
        {
            BuildString(node.Right, sb, indent + (isLeft && !isRoot ? "│   " : "    "), false, false);
        }

        sb.Append(indent);
    
        if (!isRoot)
        {
            sb.Append(isLeft ? "└── " : "┌── ");
        }
        else
        {
            sb.Append("─── ");
        }

        var valuesList = string.Join(", ", node.Values.AsEnumerable());
        var colorMark = node.Color == Color.Red ? "R" : "B";

        sb.AppendLine($"[{colorMark}] {node.Key}: {{{valuesList}}}");
        
        if (node.Left != _nil)
        {
            BuildString(node.Left, sb, indent + (!isLeft || isRoot ? "    " : "│   "), true, false);
        }
    }

    public List<Node> RightLeftTraversal()
    {
        var list = new List<Node>();
        RightLeftTraversalInternal(list, _root);
        return list;
    }
    
    private void RightLeftTraversalInternal(List<Node> result, Node node)
    {
        if (node == _nil)
            return;

        RightLeftTraversalInternal(result, node.Right);
        result.Add(node);
        RightLeftTraversalInternal(result, node.Left);
    }

    public void Insert(TKey key, TValue value)
    {
        var existingNode = FindInternal(_root, key);
       
        if (existingNode != _nil)
        {
            existingNode.Values.Add(value);
            return;
        }
        
        var z = new Node(key)
        {
            Left = _nil,
            Right = _nil,
            Parent = _nil
        };
        z.Values.Add(value);
        
        TreeInsert(z);
        RbInsertFixup(z);
    }
    
    public bool Replace(TKey key, TValue oldValue, TValue newValue)
    {
        var node = FindInternal(_root, key);
        if (node == _nil || node.Values.Empty)
            return false;
        
        return node.Values.Replace(oldValue, newValue);
    }

    public TValue[] GetValuesInRange(TKey from, TKey to, Predicate<TValue> match)
    {
        if (from.CompareTo(to) > 0)
            throw new ArgumentException("Начальный ключ не может быть больше конечного.");

        if (_root == _nil)
            return [];

        var splitNode = _root;

        while (splitNode != _nil)
        {
            if (to.CompareTo(splitNode.Key) < 0)
            {
                splitNode = splitNode.Left;
                continue;
            }

            if (from.CompareTo(splitNode.Key) > 0)
            {
                splitNode = splitNode.Right;
                continue;
            }
            
            break;
        }

        if (splitNode == _nil)
            return [];
        
        var (fromNode, leftCount) = FindBoundary(splitNode.Left, from, false, match);
        var (toNode, rightCount) = FindBoundary(splitNode.Right, to, true, match);
        var splitCount = CountMatchingInNode(splitNode, match);

        if (leftCount + splitCount + rightCount == 0)
            return [];

        var result = new TValue[leftCount + splitCount + rightCount];
        var i = 0;
    
        var current = fromNode != _nil ? fromNode : splitNode;
        var endNode = toNode != _nil ? toNode : splitNode;

        while (current != _nil)
        {
            FillArrayFromNode(current, result, ref i, match);
        
            if (current == endNode)
                break;
        
            current = GetSuccessor(current);
        }

        return result;
    }
    
    public Node GetSuccessor(Node node)
    {
        if (node.Right != _nil)
        {
            node = node.Right;
            while (node.Left != _nil)
                node = node.Left;
            return node;
        }

        var parent = node.Parent;
        while (node == parent.Right)
        {
            node = parent;
            parent = parent.Parent;
        }
    
        return parent;
    }

    private int CountMatchingInNode(Node node, Predicate<TValue> match)
    {
        if (node == _nil) 
            return 0;
        
        var count = 0;
        
        foreach (var val in node.Values)
            if (match(val)) 
                count++;
        
        return count;
    }

    public (Node node, int count) FindBoundary(Node startNode, TKey limit, bool isSearchingMax, Predicate<TValue> match)
    {
        var current = startNode;
        var boundaryNode = _nil;
        var totalCount = 0;

        while (current != _nil)
        {
            var comparison = limit.CompareTo(current.Key);
            if (isSearchingMax)
            {
                if (comparison >= 0)
                {
                    boundaryNode = current;
                    totalCount += CountMatchingInNode(current, match);
                    totalCount += CountAll(current.Left, match);
                    current = current.Right;
                    continue;
                }
                current = current.Left;
                continue;
            }
        
            if (comparison <= 0)
            {
                boundaryNode = current;
                totalCount += CountMatchingInNode(current, match);
                totalCount += CountAll(current.Right, match);
                current = current.Left;
                continue;
            }
            current = current.Right;
        }
        
        return (boundaryNode, totalCount);
    }

    private int CountAll(Node node, Predicate<TValue> match)
    {
        if (node == _nil) 
            return 0;
        
        return CountMatchingInNode(node, match) + 
               CountAll(node.Left, match) + 
               CountAll(node.Right, match);
    }

    public void FillArrayFromNode(Node node, TValue[] array, ref int startIndex, Predicate<TValue> match)
    {
        foreach (var val in node.Values)
        {
            if (!match(val)) 
                continue;
            
            array[startIndex] = val;
            startIndex++;
        }
    }

    private void TreeInsert(Node z)
    {
        var y = _nil;
        var x = _root;

        while (x != _nil)
        {
            y = x;
            x = z.Key.CompareTo(x.Key) < 0 ? x.Left : x.Right;
        }

        z.Parent = y;

        if (y == _nil)
            _root = z;
        else if (z.Key.CompareTo(y.Key) < 0)
            y.Left = z;
        else
            y.Right = z;
    }

    private void RbInsertFixup(Node z)
    {
        while (z.Parent.Color == Color.Red)
        {
            if (z.Parent == z.Parent.Parent.Left)
            {
                var y = z.Parent.Parent.Right;
                if (y.Color == Color.Red)
                {
                    z.Parent.Color = Color.Black;
                    y.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Right)
                    {
                        z = z.Parent;
                        LeftRotate(z);
                    }
                    z.Parent.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    RightRotate(z.Parent.Parent);
                }
            }
            else
            {
                var y = z.Parent.Parent.Left;
                if (y.Color == Color.Red)
                {
                    z.Parent.Color = Color.Black;
                    y.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        z = z.Parent;
                        RightRotate(z);
                    }
                    z.Parent.Color = Color.Black;
                    z.Parent.Parent.Color = Color.Red;
                    LeftRotate(z.Parent.Parent);
                }
            }
        }
        _root.Color = Color.Black;
    }

    public bool TryRemove(TKey key, TValue value, [MaybeNullWhen(false)] out TValue removed)
    {
        var z = FindInternal(_root, key);
        removed = default;
        
        if (z == _nil) 
            return false;

        if (!z.Values.Empty && z.Values.TryRemove(value, out removed))
        {
            if (z.Values.Empty)
                RbDelete(z);
            
            return true;
        }

        if (z.Values.Empty)
            RbDelete(z);

        return false;
    }

    private void RbDelete(Node z)
    {
        Node y;

        if (z.Left == _nil || z.Right == _nil)
            y = z;
        else
            y = TreeMaximum(z.Left);

        var x = y.Left != _nil ? y.Left : y.Right;

        x.Parent = y.Parent;

        if (y.Parent == _nil)
            _root = x;
        else if (y == y.Parent.Left)
            y.Parent.Left = x;
        else
            y.Parent.Right = x;

        if (y != z)
        {
            z.Key = y.Key;
            z.Values = y.Values;
        }

        if (y.Color == Color.Black)
            RbDeleteFixup(x);
    }

    private void RbDeleteFixup(Node x)
    {
        while (x != _root && x.Color == Color.Black)
        {
            if (x == x.Parent.Left)
            {
                var w = x.Parent.Right;
                if (w.Color == Color.Red)
                {
                    w.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    LeftRotate(x.Parent);
                    w = x.Parent.Right;
                }

                if (w.Left.Color == Color.Black && w.Right.Color == Color.Black)
                {
                    w.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Right.Color == Color.Black)
                    {
                        w.Left.Color = Color.Black;
                        w.Color = Color.Red;
                        RightRotate(w);
                        w = x.Parent.Right;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    w.Right.Color = Color.Black;
                    LeftRotate(x.Parent);
                    x = _root;
                }
            }
            else
            {
                var w = x.Parent.Left;
                if (w.Color == Color.Red)
                {
                    w.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    RightRotate(x.Parent);
                    w = x.Parent.Left;
                }

                if (w.Right.Color == Color.Black && w.Left.Color == Color.Black)
                {
                    w.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (w.Left.Color == Color.Black)
                    {
                        w.Right.Color = Color.Black;
                        w.Color = Color.Red;
                        LeftRotate(w);
                        w = x.Parent.Left;
                    }
                    w.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    w.Left.Color = Color.Black;
                    RightRotate(x.Parent);
                    x = _root;
                }
            }
        }
        x.Color = Color.Black;
    }

    private void LeftRotate(Node x)
    {
        var y = x.Right;
        x.Right = y.Left;

        if (y.Left != _nil)
            y.Left.Parent = x;

        y.Parent = x.Parent;

        if (x.Parent == _nil)
            _root = y;
        else if (x == x.Parent.Left)
            x.Parent.Left = y;
        else
            x.Parent.Right = y;

        y.Left = x;
        x.Parent = y;
    }

    private void RightRotate(Node y)
    {
        var x = y.Left;
        y.Left = x.Right;

        if (x.Right != _nil)
            x.Right.Parent = y;

        x.Parent = y.Parent;

        if (y.Parent == _nil)
            _root = x;
        else if (y == y.Parent.Right)
            y.Parent.Right = x;
        else
            y.Parent.Left = x;

        x.Right = y;
        y.Parent = x;
    }

    private Node TreeMaximum(Node x)
    {
        while (x.Right != _nil)
            x = x.Right;
        
        return x;
    }

    private Node FindInternal(Node x, TKey key)
    {
        while (x != _nil && key.CompareTo(x.Key) != 0)
            x = key.CompareTo(x.Key) < 0 ? x.Left : x.Right;
        
        return x;
    }

    public bool TryFind(TKey key, [MaybeNullWhen(false)] out DoubleLinkedList<TValue> result)
    {
        result = null;
        
        var node = FindInternal(_root, key);
        if (node == _nil || node.Values.Empty)
            return false;

        result = node.Values;
        return true;
    }
}