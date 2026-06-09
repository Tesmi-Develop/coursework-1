using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Coursework1.Collections;

public class HashTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    public int Count { get; private set; }
    
    public IEnumerable<TKey> Keys
    {
        get
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (_table[i].IsOccupied) 
                    yield return _table[i].Key;
            }
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (_table[i].IsOccupied) 
                    yield return _table[i].Value;
            }
        }
    }

    private struct Entry
    {
        public TKey Key;
        public TValue Value;
        public bool IsOccupied;
    }

    private Entry[] _table;
    private int _capacity;
    private int _k;
    private float LoadFactor => (float)Count / _capacity;
    private const double LoadFactorThreshold = 0.7;
    private const double ShrinkThreshold = 0.2;
    private const int MinCapacity = 1; 

    public HashTable(int initialSize = 11)
    {
        if (initialSize < MinCapacity)
            throw new ArgumentOutOfRangeException(nameof(initialSize));
        
        _capacity = initialSize;
        _k = CalculateStep(_capacity);
        _table = new Entry[_capacity];
    }
    
    private int CalculateStep(int capacity)
    {
        if (capacity <= 1) 
            return 1;
        
        var step = capacity / 2 + 1;
        
        while (step > 1)
        {
            if (Gcd(step, capacity) == 1)
                return step;
            step--;
        }
        
        return 1;
    }
    
    
    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private int MidSquareHash(TKey key, int m)
    {
        var hash = (ulong)Math.Abs(key.GetHashCode());
        var square = hash * hash;
        var middleBits = (int)((square >> 24) & 0xFFFF);

        return middleBits % m;
    }
    
    private int PrimaryHash(TKey key)
    {
        return MidSquareHash(key, _capacity);
    }
    
    private int SecondaryHash(int currentIndex)
    {
        return (currentIndex + _k) % _capacity;
    }

    private int GetIndex(TKey key, out int steps)
    {
        var i = PrimaryHash(key);
        var start = i;
        steps = 0;

        while (_table[i].IsOccupied)
        {
            steps++;
            if (_table[i].Key.Equals(key))
                return i;
            
            i = SecondaryHash(i);
            
            if (i == start) 
                break;
        }
        
        steps++; 
        return i; 
    }
    
    private int GetIndex(TKey key)
    {
        var i = PrimaryHash(key);
        var start = i;

        while (_table[i].IsOccupied)
        {
            if (_table[i].Key.Equals(key))
                return i;
            
            i = SecondaryHash(i);
            
            if (i == start) 
                break;
        }
        
        return i; 
    }

    private void Resize(int newSize)
    {
        var oldTable = _table;
        var oldCapacity = _capacity;
        
        _capacity = newSize;
        _k = CalculateStep(_capacity);
        _table = new Entry[_capacity];
        Count = 0;
        
        for (var i = 0; i < oldCapacity; i++)
        {
            if (!oldTable[i].IsOccupied) 
                continue;
            
            Add(oldTable[i].Key, oldTable[i].Value);
        }
    }
    
    private void Update(TKey key, TValue value)
    {
        var index = GetIndex(key, out _);
        if (index != -1 && _table[index].IsOccupied)
            _table[index].Value = value;
    }

    public bool Remove(TKey key)
    {
        return Remove(key, out _);
    }
    
    public bool Remove(TKey key, out TValue? removedValue)
    {
        var index = GetIndex(key, out _);

        if (index != -1 && _table[index].IsOccupied)
        {
            removedValue = _table[index].Value;
            _table[index] = new Entry { IsOccupied = false };
            Count--;
            
            if (LoadFactor < ShrinkThreshold && Count > MinCapacity)
                Resize(_capacity / 2);
        
            FixCluster(SecondaryHash(index));
            return true;
        }

        removedValue = default;
        return false;
    }
    
    public bool Remove(TKey key, TValue value)
    {
        var index = GetIndex(key, out _);

        if (index == -1 || !_table[index].IsOccupied ||
            !EqualityComparer<TValue>.Default.Equals(_table[index].Value, value)) 
            return false;
        
        _table[index] = new Entry { IsOccupied = false };
        Count--;
        
        if (LoadFactor < ShrinkThreshold && Count > MinCapacity)
            Resize(_capacity / 2);
            
        FixCluster((index + _k) % _capacity);
        return true;
    }
    
    public bool Replace(TKey key, TValue newValue)
    {
        var index = GetIndex(key, out _);

        if (index != -1 && _table[index].IsOccupied && 
            _table[index].Key.Equals(key))
        {
            _table[index].Value = newValue;
            return true;
        }

        return false;
    }

    private void FixCluster(int index)
    {
        while (_table[index].IsOccupied)
        {
            var keyToMove = _table[index].Key;
            var valueToMove = _table[index].Value;
            
            _table[index] = new Entry { IsOccupied = false };
            Count--;
            
            Add(keyToMove, valueToMove);
            index = (index + _k) % _capacity;
        }
    }
    
    public bool Add(TKey key, TValue value)
    {
        if (LoadFactor >= LoadFactorThreshold)
            Resize(_capacity * 2 + 1);

        if (ContainsKey(key))
            return false;
        
        var index = GetIndex(key);
        
        if (index == -1 || _table[index].IsOccupied) 
            return false;

        _table[index] = new Entry { Key = key, Value = value, IsOccupied = true };
        Count++;
        return true;
    }
    
    public bool Find(TKey key, [MaybeNullWhen(false)]out TValue value, out int steps)
    {
        var index = GetIndex(key, out steps);
        
        if (index != -1 && _table[index].IsOccupied && _table[index].Key.Equals(key))
        {
            value = _table[index].Value;
            return true;
        }

        value = default;
        return false;
    }
    
    public TValue this[TKey key]
    {
        get => Find(key, out var value, out _) ? value! : throw new KeyNotFoundException($"Key '{key}' not found in table.");
        set
        {
            if (Find(key, out _, out _))
                Update(key, value);
            else
                Add(key, value);
        }
    }
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)]out TValue value)
    {
        return Find(key, out value, out _);
    }
    
    public bool ContainsKey(TKey key) => Find(key, out _, out _);
    
    public void Clear()
    {
        _table = new Entry[_capacity];
        Count = 0;
    }
    
    public TValue Ensure(TKey key, TValue defaultValue = default!)
    {
        var index = GetIndex(key, out _);
        
        if (index != -1 && _table[index].IsOccupied)
            return _table[index].Value;
        
        Add(key, defaultValue);
        return defaultValue;
    }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (var i = 0; i < _capacity; i++)
            if (_table[i].IsOccupied)
                yield return new KeyValuePair<TKey, TValue>(_table[i].Key, _table[i].Value);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public string PrintDebugInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- Hash Table Debug Dump ---");
        sb.AppendLine($"Capacity: {_capacity}, Count: {Count}, Load Factor: {(double)Count/_capacity:F2}");
        sb.AppendLine($"Hash Function: MidSquare, Step (K): {_k}");
        sb.AppendLine($"--------------------------------");
        sb.AppendLine($"{"Index", -8} | {"Status", -10} | {"Key", -20} | {"First hash index", -10} | {"Steps", -5}");
        sb.AppendLine(new string('-', 60));

        for (var i = 0; i < _capacity; i++)
        {
            if (_table[i].IsOccupied)
            {
                var key = _table[i].Key;
                var firstHashIndex = MidSquareHash(key, _capacity);
                
                var steps = 0;
                var curr = firstHashIndex;
                while (curr != i)
                {
                    steps++;
                    curr = (curr + _k) % _capacity;
                    if (steps > _capacity) 
                        break; 
                }

                sb.AppendLine($"{i, -8} | {"Occupied", -10} | {key?.ToString() ?? "null", -20} | {firstHashIndex, -10} | {steps, -5}");
            }
            else
            {
                sb.AppendLine($"{i, -8} | {"Empty", -10} | {"-", -20} | {"-", -10} | {"-", -5}");
            }
        }
        sb.AppendLine("--------------------------------");
        return sb.ToString();
    }
}