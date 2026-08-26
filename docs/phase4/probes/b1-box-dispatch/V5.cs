// V5 — the landing candidate: V3's per-kind sealed subclass STORAGE + virtual accessors,
// with m_isNull as a NON-VIRTUAL base field so DerefOrNull pays one indirect call, not two.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal abstract class V5Box<T>
{
    // The one field every kind carries: the nil mark, read non-virtually by DerefOrNull.
    protected readonly bool m_isNull;

    protected V5Box(bool isNull = false) => m_isNull = isNull;

    public bool IsNilStandardPointer => m_isNull;

    public abstract ref T Value { get; }
    public abstract ref T ValueSlot { get; }
}

internal sealed class V5Standard<T> : V5Box<T>
{
    private T m_val;
    private readonly T[]? m_slot;
    private object? m_pin;

    public V5Standard(in T value)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            m_val = value;
        else
        {
            m_val = default!;
            m_slot = [value];
        }
    }

    public override ref T Value
    {
        get
        {
            if (m_isNull)
                throw new NullReferenceException();

            return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);
        }
    }

    public override ref T ValueSlot =>
        ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);
}

internal sealed class V5FieldRef<T> : V5Box<T>
{
    private readonly object m_source;
    private readonly FieldRefFunc<T> m_accessor;
    private readonly Delegate m_token;

    public V5FieldRef(object source, FieldRefFunc<T> accessor, Delegate token)
    {
        m_source = source;
        m_accessor = accessor;
        m_token = token;
    }

    public override ref T Value => ref m_accessor(m_source);
    public override ref T ValueSlot => ref m_accessor(m_source);
}

internal sealed class V5ElemRef<T> : V5Box<T>
{
    private readonly IArrayM m_array;
    private readonly int m_index;

    public V5ElemRef(IArrayM array, int index)
    {
        m_array = array;
        m_index = index;
    }

    public override ref T Value
    {
        get
        {
            if (m_array is IArrayM<T> typedArray)
                return ref typedArray.ElementRef(m_index);

            throw new InvalidOperationException();
        }
    }

    public override ref T ValueSlot => ref Value;
}

internal sealed class V5Native<T> : V5Box<T>
{
    private readonly nuint m_nativeAddr;
    private object? m_pin;
    private object? m_retainedSource;

    public V5Native(nuint nativeAddr) => m_nativeAddr = nativeAddr;

    public override unsafe ref T Value => ref Unsafe.AsRef<T>((void*)m_nativeAddr);
    public override unsafe ref T ValueSlot => ref Unsafe.AsRef<T>((void*)m_nativeAddr);
}

internal static class V5Ext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this V5Box<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }
}
