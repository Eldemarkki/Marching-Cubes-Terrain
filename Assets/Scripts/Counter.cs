using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct Counter : IDisposable
{
    private Allocator allocator;
    [NativeDisableUnsafePtrRestriction] private int* _counter;

    public int Count
    {
        get => *_counter;
        set => (*_counter) = value;
    }

    public Counter(Allocator allocator)
    {
        this.allocator = allocator;
        _counter = (int*)UnsafeUtility.Malloc(sizeof(int), 4, allocator);
        Count = 0;
    }

    public int Increment()
    {
        return Interlocked.Increment(ref *_counter) - 1;
    }

    public void Dispose()
    {
        UnsafeUtility.Free(_counter, allocator);
    }
}
