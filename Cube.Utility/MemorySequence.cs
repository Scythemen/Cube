using System;
using System.Collections;
using System.Collections.Generic;

namespace Cube.Utility;

public class MemorySequence<T> : IEnumerable<ReadOnlyMemory<T>>, IDisposable
{
    public static MemorySequence<T> Empty => new MemorySequence<T>();

    private readonly LinkedList<ReadOnlyMemory<T>> _list = new LinkedList<ReadOnlyMemory<T>>();
    public int Length { get; private set; } = 0;
    public bool IsEmpty => Length == 0;

    public MemorySequence()
    {
    }

    public MemorySequence(T[] val)
    {
        ArgumentNullException.ThrowIfNull(val);
        AddLast(new ReadOnlyMemory<T>(val));
    }

    public MemorySequence(ReadOnlyMemory<T> memory)
    {
        AddLast(memory);
    }

    public MemorySequence<T> AddLast(T[] val)
    {
        ArgumentNullException.ThrowIfNull(val);
        AddLast(new ReadOnlyMemory<T>(val));
        return this;
    }

    public MemorySequence<T> AddLast(ReadOnlyMemory<T> memory)
    {
        _list.AddLast(memory);
        Length += memory.Length;
        return this;
    }

    public MemorySequence<T> AddFirst(T[] val)
    {
        ArgumentNullException.ThrowIfNull(val);
        AddFirst(new ReadOnlyMemory<T>(val));
        return this;
    }

    public MemorySequence<T> AddFirst(ReadOnlyMemory<T> memory)
    {
        _list.AddFirst(memory);
        Length += memory.Length;
        return this;
    }

    public ReadOnlyMemory<T>? RemoveFirst()
    {
        if (_list.Count == 0)
        {
            return null;
        }

        var memory = _list.First.Value;
        _list.RemoveFirst();
        Length -= memory.Length;
        return memory;
    }

    public ReadOnlyMemory<T>? RemoveLast()
    {
        if (_list.Count == 0)
        {
            return null;
        }

        var memory = _list.Last.Value;
        _list.RemoveLast();
        Length -= memory.Length;
        return memory;
    }


    public MemorySequence<T> Clear()
    {
        _list.Clear();
        Length = 0;
        return this;
    }


    public IEnumerator<ReadOnlyMemory<T>> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        _list.Clear();
    }

    // private class ReadOnlySequenceMemory<TT> : ReadOnlySequenceSegment<TT>
    // {
    //     public ReadOnlySequenceMemory(ReadOnlyMemory<TT> m)
    //     {
    //         this.Memory = m;
    //         this.RunningIndex = 0;
    //         this.Next = null;
    //     }
    //
    //     public void Append(ReadOnlyMemory<TT> m)
    //     {
    //         if (m.IsEmpty)
    //         {
    //             return;
    //         }
    //
    //         var fk = new ReadOnlySequenceMemory<TT>(m)
    //         {
    //             Memory = m,
    //             RunningIndex = this.RunningIndex + this.Memory.Length,
    //         };
    //
    //         this.Next = fk;
    //     }
    // }
    //
}