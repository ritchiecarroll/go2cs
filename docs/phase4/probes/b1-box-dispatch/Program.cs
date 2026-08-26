// B1 P-F2 precondition microbench — the box dispatch/layout decision, measured.
//
// Four variants of the ж<T> box, each a faithful model of one layout/dispatch choice:
//
//   V1 current           one class, two nullable tuples, null-test branch chain
//                        (transcribed from src/core/golib/ж.cs field-for-field and
//                        branch-for-branch)
//   V2 flattened-switch  one class, tuples flattened to plain fields, byte kind + switch;
//                        inline m_val stays (a field of type T cannot be conditionally
//                        present), so this variant takes the ~28 B tuple win only
//   V3 subclass-virtual  abstract base, per-kind sealed subclasses, virtual Value/ValueSlot;
//                        takes BOTH byte wins (tuples + dead m_val gone from non-standard
//                        kinds) and pays an indirect call that AOT cannot devirtualize
//                        (the hierarchy is open: unsafe.Pointer subclasses the base)
//   V4 kindbyte-downcast per-kind sealed subclasses for STORAGE, but dispatch is a byte
//                        kind in the base + Unsafe.As unchecked downcast — the kind byte
//                        guarantees the runtime type, so no isinst and no virtual call.
//                        Takes both byte wins with branch dispatch.
//
// Workloads (per P-F2): standard-box-dominant, mixed-kind (90/8/1.5/0.5), and a
// reinterpret/native-kind case; Value, ValueSlot-analog (DerefOrNull), field-ref hop.
// Protocol (per P-F4): paired same-machine interleaved rounds, N >= 12, medians reported;
// run on JIT (warmed, PGO) and Native AOT from the same source.
//
// Also reports per-kind allocated BYTES per box for T=long and T=Big (a 560 B struct with
// a reference field, modeling os.FD) — the layout half of the decision.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0649

// ----------------------------------------------------------------------------------------
// Shared shapes
// ----------------------------------------------------------------------------------------

internal delegate ref T FieldRefFunc<T>(object source);

internal interface IArrayM { }

internal interface IArrayM<T> : IArrayM
{
    ref T ElementRef(int index);
}

internal sealed class ArrayM<T> : IArrayM<T>
{
    private readonly T[] m_items;
    public ArrayM(T[] items) => m_items = items;
    public ref T ElementRef(int index) => ref m_items[index];
}

// A 560 B struct with one reference field — models os.FD-scale pointees (managed, so no slot).
[StructLayout(LayoutKind.Sequential)]
internal struct Big
{
    public object? Ref;
    public Blob512 Blob;
    public long A, B, C, D, E, F;
}

[InlineArray(64)]
internal struct Blob512
{
    private long m_e0;
}

internal sealed class Holder
{
    public long Field;
    public Big BigField;
}

// ----------------------------------------------------------------------------------------
// V1 — current: transcription of golib's ж<T> layout and branch chains
// ----------------------------------------------------------------------------------------

internal class V1Box<T>
{
    private readonly (object, FieldRefFunc<T>, Delegate)? m_structFieldRef;
    private readonly (IArrayM, int)? m_arrayIndexRef;
    private readonly bool m_isNull;
    private T m_val;
    private readonly T[]? m_slot;
    private readonly nuint m_nativeAddr;
    private object? m_pin;

    public V1Box(in T value)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            m_val = value;
        else
        {
            m_val = default!;
            m_slot = [value];
        }
    }

    public V1Box(object source, FieldRefFunc<T> accessor, Delegate token)
    {
        m_val = default!;
        m_structFieldRef = (source, accessor, token);
    }

    public V1Box(IArrayM array, int index)
    {
        m_val = default!;
        m_arrayIndexRef = (array, index);
    }

    public V1Box(nuint nativeAddr)
    {
        m_val = default!;
        m_nativeAddr = nativeAddr;
    }

    public bool IsNilStandardPointer => m_isNull;

    public unsafe ref T Value
    {
        get
        {
            if (m_nativeAddr != 0)
                return ref Unsafe.AsRef<T>((void*)m_nativeAddr);

            if (m_structFieldRef is null && m_arrayIndexRef is null)
            {
                if (m_isNull)
                    throw new NullReferenceException();

                return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);
            }

            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> fieldRefFunc, Delegate _) = m_structFieldRef!.Value;
                return ref fieldRefFunc(source);
            }

            (IArrayM array, int index) = m_arrayIndexRef!.Value;

            if (array is IArrayM<T> typedArray)
                return ref typedArray.ElementRef(index);

            throw new InvalidOperationException();
        }
    }

    public unsafe ref T ValueSlot
    {
        get
        {
            if (m_nativeAddr != 0)
                return ref Unsafe.AsRef<T>((void*)m_nativeAddr);

            if (m_structFieldRef is null && m_arrayIndexRef is null)
                return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);

            if (m_structFieldRef is not null)
            {
                (object source, FieldRefFunc<T> fieldRefFunc, Delegate _) = m_structFieldRef!.Value;
                return ref fieldRefFunc(source);
            }

            (IArrayM array, int index) = m_arrayIndexRef!.Value;

            if (array is IArrayM<T> typedArray)
                return ref typedArray.ElementRef(index);

            throw new InvalidOperationException();
        }
    }
}

internal static class V1Ext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this V1Box<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }
}

// ----------------------------------------------------------------------------------------
// V2 — flattened-switch: one class, plain fields, byte kind + switch. m_val stays inline.
// ----------------------------------------------------------------------------------------

internal class V2Box<T>
{
    private const byte KindStandard = 0, KindFieldRef = 1, KindElemRef = 2, KindNative = 3;

    private readonly byte m_kind;
    private readonly bool m_isNull;
    private T m_val;
    private readonly T[]? m_slot;
    private readonly object? m_source;        // fieldRef source | elemRef IArrayM
    private readonly FieldRefFunc<T>? m_accessor;
    private readonly Delegate? m_token;
    private readonly int m_index;
    private readonly nuint m_nativeAddr;
    private object? m_pin;

    public V2Box(in T value)
    {
        m_kind = KindStandard;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            m_val = value;
        else
        {
            m_val = default!;
            m_slot = [value];
        }
    }

    public V2Box(object source, FieldRefFunc<T> accessor, Delegate token)
    {
        m_kind = KindFieldRef;
        m_val = default!;
        m_source = source;
        m_accessor = accessor;
        m_token = token;
    }

    public V2Box(IArrayM array, int index)
    {
        m_kind = KindElemRef;
        m_val = default!;
        m_source = array;
        m_index = index;
    }

    public V2Box(nuint nativeAddr)
    {
        m_kind = KindNative;
        m_val = default!;
        m_nativeAddr = nativeAddr;
    }

    public bool IsNilStandardPointer => m_isNull;

    public unsafe ref T Value
    {
        get
        {
            switch (m_kind)
            {
                case KindStandard:
                    if (m_isNull)
                        throw new NullReferenceException();

                    return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);

                case KindFieldRef:
                    return ref m_accessor!(m_source!);

                case KindElemRef:
                    if (m_source is IArrayM<T> typedArray)
                        return ref typedArray.ElementRef(m_index);

                    throw new InvalidOperationException();

                default:
                    return ref Unsafe.AsRef<T>((void*)m_nativeAddr);
            }
        }
    }

    public unsafe ref T ValueSlot
    {
        get
        {
            switch (m_kind)
            {
                case KindStandard:
                    return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);

                case KindFieldRef:
                    return ref m_accessor!(m_source!);

                case KindElemRef:
                    if (m_source is IArrayM<T> typedArray)
                        return ref typedArray.ElementRef(m_index);

                    throw new InvalidOperationException();

                default:
                    return ref Unsafe.AsRef<T>((void*)m_nativeAddr);
            }
        }
    }
}

internal static class V2Ext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this V2Box<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }
}

// ----------------------------------------------------------------------------------------
// V3 — subclass-virtual: abstract base, sealed per-kind subclasses, virtual accessors.
// The hierarchy is OPEN by construction (unsafe.Pointer subclasses the real base), so
// neither JIT guarded devirt at a polymorphic site nor AOT whole-program sealing applies
// at a call through the base type.
// ----------------------------------------------------------------------------------------

internal abstract class V3Box<T>
{
    public abstract ref T Value { get; }
    public abstract ref T ValueSlot { get; }
    public virtual bool IsNilStandardPointer => false;
}

internal sealed class V3Standard<T> : V3Box<T>
{
    private readonly bool m_isNull;
    private T m_val;
    private readonly T[]? m_slot;
    private object? m_pin;

    public V3Standard(in T value)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            m_val = value;
        else
        {
            m_val = default!;
            m_slot = [value];
        }
    }

    public override bool IsNilStandardPointer => m_isNull;

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

internal sealed class V3FieldRef<T> : V3Box<T>
{
    private readonly object m_source;
    private readonly FieldRefFunc<T> m_accessor;
    private readonly Delegate m_token;

    public V3FieldRef(object source, FieldRefFunc<T> accessor, Delegate token)
    {
        m_source = source;
        m_accessor = accessor;
        m_token = token;
    }

    public override ref T Value => ref m_accessor(m_source);
    public override ref T ValueSlot => ref m_accessor(m_source);
}

internal sealed class V3ElemRef<T> : V3Box<T>
{
    private readonly IArrayM m_array;
    private readonly int m_index;

    public V3ElemRef(IArrayM array, int index)
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

internal sealed class V3Native<T> : V3Box<T>
{
    private readonly nuint m_nativeAddr;
    private object? m_pin;
    private object? m_retainedSource;   // the NetShareAdd source-retention slot

    public V3Native(nuint nativeAddr) => m_nativeAddr = nativeAddr;

    public override unsafe ref T Value => ref Unsafe.AsRef<T>((void*)m_nativeAddr);
    public override unsafe ref T ValueSlot => ref Unsafe.AsRef<T>((void*)m_nativeAddr);
}

internal static class V3Ext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this V3Box<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }
}

// ----------------------------------------------------------------------------------------
// V4 — kind byte in the base + per-kind sealed subclass STORAGE + Unsafe.As downcast.
// The byte guarantees the runtime type, so the downcast is unchecked (no isinst) and the
// accessors stay non-virtual and inlinable. Takes V3's bytes with V1/V2's dispatch shape.
// ----------------------------------------------------------------------------------------

internal class V4Box<T>
{
    protected const byte KindStandard = 0, KindFieldRef = 1, KindElemRef = 2, KindNative = 3;

    // readonly byte set once by the subclass ctor — the discriminant IS the type.
    protected readonly byte m_kind;
    protected readonly bool m_isNull;

    protected V4Box(byte kind, bool isNull = false)
    {
        m_kind = kind;
        m_isNull = isNull;
    }

    public bool IsNilStandardPointer => m_isNull;

    public unsafe ref T Value
    {
        get
        {
            switch (m_kind)
            {
                case KindStandard:
                {
                    if (m_isNull)
                        throw new NullReferenceException();

                    V4Standard<T> self = Unsafe.As<V4Standard<T>>(this);
                    return ref self.m_slot is null ? ref self.m_val! : ref MemoryMarshal.GetArrayDataReference(self.m_slot);
                }
                case KindFieldRef:
                {
                    V4FieldRef<T> self = Unsafe.As<V4FieldRef<T>>(this);
                    return ref self.m_accessor(self.m_source);
                }
                case KindElemRef:
                {
                    V4ElemRef<T> self = Unsafe.As<V4ElemRef<T>>(this);

                    if (self.m_array is IArrayM<T> typedArray)
                        return ref typedArray.ElementRef(self.m_index);

                    throw new InvalidOperationException();
                }
                default:
                {
                    V4Native<T> self = Unsafe.As<V4Native<T>>(this);
                    return ref Unsafe.AsRef<T>((void*)self.m_nativeAddr);
                }
            }
        }
    }

    public unsafe ref T ValueSlot
    {
        get
        {
            switch (m_kind)
            {
                case KindStandard:
                {
                    V4Standard<T> self = Unsafe.As<V4Standard<T>>(this);
                    return ref self.m_slot is null ? ref self.m_val! : ref MemoryMarshal.GetArrayDataReference(self.m_slot);
                }
                case KindFieldRef:
                {
                    V4FieldRef<T> self = Unsafe.As<V4FieldRef<T>>(this);
                    return ref self.m_accessor(self.m_source);
                }
                case KindElemRef:
                {
                    V4ElemRef<T> self = Unsafe.As<V4ElemRef<T>>(this);

                    if (self.m_array is IArrayM<T> typedArray)
                        return ref typedArray.ElementRef(self.m_index);

                    throw new InvalidOperationException();
                }
                default:
                {
                    V4Native<T> self = Unsafe.As<V4Native<T>>(this);
                    return ref Unsafe.AsRef<T>((void*)self.m_nativeAddr);
                }
            }
        }
    }
}

internal sealed class V4Standard<T> : V4Box<T>
{
    internal T m_val;
    internal readonly T[]? m_slot;
    internal object? m_pin;

    public V4Standard(in T value) : base(KindStandard)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            m_val = value;
        else
        {
            m_val = default!;
            m_slot = [value];
        }
    }
}

internal sealed class V4FieldRef<T> : V4Box<T>
{
    internal readonly object m_source;
    internal readonly FieldRefFunc<T> m_accessor;
    internal readonly Delegate m_token;

    public V4FieldRef(object source, FieldRefFunc<T> accessor, Delegate token) : base(KindFieldRef)
    {
        m_source = source;
        m_accessor = accessor;
        m_token = token;
    }
}

internal sealed class V4ElemRef<T> : V4Box<T>
{
    internal readonly IArrayM m_array;
    internal readonly int m_index;

    public V4ElemRef(IArrayM array, int index) : base(KindElemRef)
    {
        m_array = array;
        m_index = index;
    }
}

internal sealed class V4Native<T> : V4Box<T>
{
    internal readonly nuint m_nativeAddr;
    internal object? m_pin;
    internal object? m_retainedSource;   // the NetShareAdd source-retention slot

    public V4Native(nuint nativeAddr) : base(KindNative) => m_nativeAddr = nativeAddr;
}

internal static class V4Ext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this V4Box<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }
}

// ----------------------------------------------------------------------------------------
// Harness
// ----------------------------------------------------------------------------------------

internal static class Program
{
    private const int Rounds = 12;                 // P-F4: N >= 10, medians
    private const int StdIters = 40_000_000;
    private const int MixedIters = 8_000_000;
    private const int FieldIters = 20_000_000;
    private const int NativeIters = 40_000_000;

    private static long s_sink;

    private static readonly Holder s_holder = new();
    private static readonly FieldRefFunc<long> s_accessor = static (object o) => ref Unsafe.As<Holder>(o).Field;
    private static long s_native;

    private static unsafe nuint NativeAddr()
    {
        // A stable address: a pinned static via fixed on a GCHandle-free path — model only.
        fixed (long* p = &s_native)
            return (nuint)p;
    }

    private static double MedianNs(List<double> samples)
    {
        List<double> s = [.. samples];
        s.Sort();
        int n = s.Count;
        return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2.0;
    }

    // ---- workload bodies, one generic shape per variant so the JIT sees each separately ----

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V1_Std(V1Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
        {
            box.Value++;
            acc += box.Value;
        }

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V2_Std(V2Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
        {
            box.Value++;
            acc += box.Value;
        }

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V3_Std(V3Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
        {
            box.Value++;
            acc += box.Value;
        }

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V4_Std(V4Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
        {
            box.Value++;
            acc += box.Value;
        }

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V5_Std(V5Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
        {
            box.Value++;
            acc += box.Value;
        }

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V1_Deref(V1Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.DerefOrNull();

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V2_Deref(V2Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.DerefOrNull();

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V3_Deref(V3Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.DerefOrNull();

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V4_Deref(V4Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.DerefOrNull();

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V5_Deref(V5Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.DerefOrNull();

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V1_Field(V1Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V2_Field(V2Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V3_Field(V3Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V4_Field(V4Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    // Mixed-kind: an array of boxes in the P-F2 ratio (90/8/1.5/0.5), walked in a fixed
    // shuffled order so the branch predictor sees the realistic interleaving, all through
    // the BASE type (the polymorphic site the variants differ on).

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V5_Field(V5Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    private static void MixKinds(out V1Box<long>[] v1, out V2Box<long>[] v2, out V3Box<long>[] v3, out V4Box<long>[] v4, out V5Box<long>[] v5)
    {
        const int N = 4096;
        var kinds = new byte[N];
        var rnd = new Random(20260826);

        for (int i = 0; i < N; i++)
        {
            double r = rnd.NextDouble();
            kinds[i] = r < 0.90 ? (byte)0 : r < 0.98 ? (byte)1 : r < 0.995 ? (byte)2 : (byte)3;
        }

        long[] backing = new long[16];
        var arr = new ArrayM<long>(backing);
        nuint nat = NativeAddr();

        v1 = new V1Box<long>[N];
        v2 = new V2Box<long>[N];
        v3 = new V3Box<long>[N];
        v4 = new V4Box<long>[N];
        v5 = new V5Box<long>[N];

        for (int i = 0; i < N; i++)
        {
            switch (kinds[i])
            {
                case 0:
                    v1[i] = new V1Box<long>(7);
                    v2[i] = new V2Box<long>(7);
                    v3[i] = new V3Standard<long>(7);
                    v4[i] = new V4Standard<long>(7);
                    v5[i] = new V5Standard<long>(7);
                    break;
                case 1:
                    v1[i] = new V1Box<long>(s_holder, s_accessor, s_accessor);
                    v2[i] = new V2Box<long>(s_holder, s_accessor, s_accessor);
                    v3[i] = new V3FieldRef<long>(s_holder, s_accessor, s_accessor);
                    v4[i] = new V4FieldRef<long>(s_holder, s_accessor, s_accessor);
                    v5[i] = new V5FieldRef<long>(s_holder, s_accessor, s_accessor);
                    break;
                case 2:
                    v1[i] = new V1Box<long>(arr, i & 15);
                    v2[i] = new V2Box<long>(arr, i & 15);
                    v3[i] = new V3ElemRef<long>(arr, i & 15);
                    v4[i] = new V4ElemRef<long>(arr, i & 15);
                    v5[i] = new V5ElemRef<long>(arr, i & 15);
                    break;
                default:
                    v1[i] = new V1Box<long>(nat);
                    v2[i] = new V2Box<long>(nat);
                    v3[i] = new V3Native<long>(nat);
                    v4[i] = new V4Native<long>(nat);
                    v5[i] = new V5Native<long>(nat);
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V1_Mixed(V1Box<long>[] boxes, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;
        int n = boxes.Length;

        for (int i = 0; i < iters; i++)
            acc += boxes[i & (n - 1)].ValueSlot;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V2_Mixed(V2Box<long>[] boxes, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;
        int n = boxes.Length;

        for (int i = 0; i < iters; i++)
            acc += boxes[i & (n - 1)].ValueSlot;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V3_Mixed(V3Box<long>[] boxes, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;
        int n = boxes.Length;

        for (int i = 0; i < iters; i++)
            acc += boxes[i & (n - 1)].ValueSlot;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V4_Mixed(V4Box<long>[] boxes, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;
        int n = boxes.Length;

        for (int i = 0; i < iters; i++)
            acc += boxes[i & (n - 1)].ValueSlot;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V1_Native(V1Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V2_Native(V2Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V3_Native(V3Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V4_Native(V4Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    // ---- size census ----

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V5_Mixed(V5Box<long>[] boxes, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;
        int n = boxes.Length;

        for (int i = 0; i < iters; i++)
            acc += boxes[i & (n - 1)].ValueSlot;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double W_V5_Native(V5Box<long> box, int iters)
    {
        Stopwatch sw = Stopwatch.StartNew();
        long acc = 0;

        for (int i = 0; i < iters; i++)
            acc += box.Value;

        sw.Stop();
        s_sink += acc;
        return sw.Elapsed.TotalNanoseconds / iters;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureAlloc(Func<object> mint)
    {
        // Median of 5 batches of 64 mints, GC-quiesced.
        object?[] keep = new object?[64];
        var samples = new List<double>(5);

        for (int b = 0; b < 5; b++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 64; i++)
                keep[i] = mint();

            long delta = GC.GetAllocatedBytesForCurrentThread() - before;
            samples.Add(delta / 64.0);
            GC.KeepAlive(keep);
        }

        samples.Sort();
        return (long)samples[2];
    }

    private static void SizeCensus()
    {
        Console.WriteLine();
        Console.WriteLine("== per-box allocated bytes (median of 5x64 mints; standard kind includes its slot) ==");
        Console.WriteLine($"{"kind/T",-28} {"V1-current",12} {"V2-switch",12} {"V3-subclass",12} {"V4-kindbyte",12} {"V5-landing",12}");

        var holder = s_holder;
        FieldRefFunc<long> acc = s_accessor;
        FieldRefFunc<Big> accBig = static (object o) => ref Unsafe.As<Holder>(o).BigField;
        long[] backing = new long[4];
        var arr = new ArrayM<long>(backing);
        nuint nat = NativeAddr();

        Console.WriteLine($"{"standard  T=long",-28} {MeasureAlloc(() => new V1Box<long>(7)),12} {MeasureAlloc(() => new V2Box<long>(7)),12} {MeasureAlloc(() => new V3Standard<long>(7)),12} {MeasureAlloc(() => new V4Standard<long>(7)),12} {MeasureAlloc(() => new V5Standard<long>(7)),12}");
        Console.WriteLine($"{"standard  T=Big(560B)",-28} {MeasureAlloc(() => new V1Box<Big>(default)),12} {MeasureAlloc(() => new V2Box<Big>(default)),12} {MeasureAlloc(() => new V3Standard<Big>(default)),12} {MeasureAlloc(() => new V4Standard<Big>(default)),12} {MeasureAlloc(() => new V5Standard<Big>(default)),12}");
        Console.WriteLine($"{"fieldRef  T=long",-28} {MeasureAlloc(() => new V1Box<long>(holder, acc, acc)),12} {MeasureAlloc(() => new V2Box<long>(holder, acc, acc)),12} {MeasureAlloc(() => new V3FieldRef<long>(holder, acc, acc)),12} {MeasureAlloc(() => new V4FieldRef<long>(holder, acc, acc)),12} {MeasureAlloc(() => new V5FieldRef<long>(holder, acc, acc)),12}");
        Console.WriteLine($"{"fieldRef  T=Big(560B)",-28} {MeasureAlloc(() => new V1Box<Big>(holder, accBig, accBig)),12} {MeasureAlloc(() => new V2Box<Big>(holder, accBig, accBig)),12} {MeasureAlloc(() => new V3FieldRef<Big>(holder, accBig, accBig)),12} {MeasureAlloc(() => new V4FieldRef<Big>(holder, accBig, accBig)),12} {MeasureAlloc(() => new V5FieldRef<Big>(holder, accBig, accBig)),12}");
        Console.WriteLine($"{"elemRef   T=long",-28} {MeasureAlloc(() => new V1Box<long>(arr, 1)),12} {MeasureAlloc(() => new V2Box<long>(arr, 1)),12} {MeasureAlloc(() => new V3ElemRef<long>(arr, 1)),12} {MeasureAlloc(() => new V4ElemRef<long>(arr, 1)),12} {MeasureAlloc(() => new V5ElemRef<long>(arr, 1)),12}");
        Console.WriteLine($"{"native    T=long",-28} {MeasureAlloc(() => new V1Box<long>(nat)),12} {MeasureAlloc(() => new V2Box<long>(nat)),12} {MeasureAlloc(() => new V3Native<long>(nat)),12} {MeasureAlloc(() => new V4Native<long>(nat)),12} {MeasureAlloc(() => new V5Native<long>(nat)),12}");
        Console.WriteLine($"{"native    T=Big(560B)",-28} {MeasureAlloc(() => new V1Box<Big>(nat)),12} {MeasureAlloc(() => new V2Box<Big>(nat)),12} {MeasureAlloc(() => new V3Native<Big>(nat)),12} {MeasureAlloc(() => new V4Native<Big>(nat)),12} {MeasureAlloc(() => new V5Native<Big>(nat)),12}");
    }

    private static void Main()
    {
        bool isAot = !RuntimeFeature.IsDynamicCodeSupported;
        Console.WriteLine($"runtime: {(isAot ? "Native AOT" : "JIT (CoreCLR)")}  {Environment.Version}  {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($"rounds: {Rounds} interleaved; medians reported (ns/op)");

        var std1 = new V1Box<long>(1);
        var std2 = new V2Box<long>(1);
        var std3 = (V3Box<long>)new V3Standard<long>(1);
        var std4 = (V4Box<long>)new V4Standard<long>(1);
        var std5 = (V5Box<long>)new V5Standard<long>(1);

        var fr1 = new V1Box<long>(s_holder, s_accessor, s_accessor);
        var fr2 = new V2Box<long>(s_holder, s_accessor, s_accessor);
        var fr3 = (V3Box<long>)new V3FieldRef<long>(s_holder, s_accessor, s_accessor);
        var fr4 = (V4Box<long>)new V4FieldRef<long>(s_holder, s_accessor, s_accessor);
        var fr5 = (V5Box<long>)new V5FieldRef<long>(s_holder, s_accessor, s_accessor);

        nuint nat = NativeAddr();
        var nb1 = new V1Box<long>(nat);
        var nb2 = new V2Box<long>(nat);
        var nb3 = (V3Box<long>)new V3Native<long>(nat);
        var nb4 = (V4Box<long>)new V4Native<long>(nat);
        var nb5 = (V5Box<long>)new V5Native<long>(nat);

        MixKinds(out var mix1, out var mix2, out var mix3, out var mix4, out var mix5);

        // Warmup: every workload body once at reduced iterations (tiering/PGO settle).
        for (int w = 0; w < 3; w++)
        {
            W_V1_Std(std1, 2_000_000); W_V2_Std(std2, 2_000_000); W_V3_Std(std3, 2_000_000); W_V4_Std(std4, 2_000_000); W_V5_Std(std5, 2_000_000);
            W_V1_Deref(std1, 2_000_000); W_V2_Deref(std2, 2_000_000); W_V3_Deref(std3, 2_000_000); W_V4_Deref(std4, 2_000_000); W_V5_Deref(std5, 2_000_000);
            W_V1_Field(fr1, 1_000_000); W_V2_Field(fr2, 1_000_000); W_V3_Field(fr3, 1_000_000); W_V4_Field(fr4, 1_000_000); W_V5_Field(fr5, 1_000_000);
            W_V1_Mixed(mix1, 500_000); W_V2_Mixed(mix2, 500_000); W_V3_Mixed(mix3, 500_000); W_V4_Mixed(mix4, 500_000); W_V5_Mixed(mix5, 500_000);
            W_V1_Native(nb1, 2_000_000); W_V2_Native(nb2, 2_000_000); W_V3_Native(nb3, 2_000_000); W_V4_Native(nb4, 2_000_000); W_V5_Native(nb5, 2_000_000);
        }

        string[] workloads = ["std-Value(rw)", "std-DerefOrNull", "fieldRef-Value", "mixed-90/8/1.5/.5", "native-Value"];
        var results = new List<double>[workloads.Length, 5];

        for (int w = 0; w < workloads.Length; w++)
            for (int v = 0; v < 5; v++)
                results[w, v] = [];

        for (int round = 0; round < Rounds; round++)
        {
            results[0, 0].Add(W_V1_Std(std1, StdIters));
            results[0, 1].Add(W_V2_Std(std2, StdIters));
            results[0, 2].Add(W_V3_Std(std3, StdIters));
            results[0, 3].Add(W_V4_Std(std4, StdIters));
            results[0, 4].Add(W_V5_Std(std5, StdIters));

            results[1, 0].Add(W_V1_Deref(std1, StdIters));
            results[1, 1].Add(W_V2_Deref(std2, StdIters));
            results[1, 2].Add(W_V3_Deref(std3, StdIters));
            results[1, 3].Add(W_V4_Deref(std4, StdIters));
            results[1, 4].Add(W_V5_Deref(std5, StdIters));

            results[2, 0].Add(W_V1_Field(fr1, FieldIters));
            results[2, 1].Add(W_V2_Field(fr2, FieldIters));
            results[2, 2].Add(W_V3_Field(fr3, FieldIters));
            results[2, 3].Add(W_V4_Field(fr4, FieldIters));
            results[2, 4].Add(W_V5_Field(fr5, FieldIters));

            results[3, 0].Add(W_V1_Mixed(mix1, MixedIters));
            results[3, 1].Add(W_V2_Mixed(mix2, MixedIters));
            results[3, 2].Add(W_V3_Mixed(mix3, MixedIters));
            results[3, 3].Add(W_V4_Mixed(mix4, MixedIters));
            results[3, 4].Add(W_V5_Mixed(mix5, MixedIters));

            results[4, 0].Add(W_V1_Native(nb1, NativeIters));
            results[4, 1].Add(W_V2_Native(nb2, NativeIters));
            results[4, 2].Add(W_V3_Native(nb3, NativeIters));
            results[4, 3].Add(W_V4_Native(nb4, NativeIters));
            results[4, 4].Add(W_V5_Native(nb5, NativeIters));
        }

        Console.WriteLine();
        Console.WriteLine($"{"workload",-20} {"V1-current",12} {"V2-switch",12} {"V3-subclass",12} {"V4-kindbyte",12} {"V5-landing",12}   (vs V1)");

        for (int w = 0; w < workloads.Length; w++)
        {
            double m1 = MedianNs(results[w, 0]);
            double m2 = MedianNs(results[w, 1]);
            double m3 = MedianNs(results[w, 2]);
            double m4 = MedianNs(results[w, 3]);
            double m5 = MedianNs(results[w, 4]);
            Console.WriteLine($"{workloads[w],-20} {m1,12:F3} {m2,12:F3} {m3,12:F3} {m4,12:F3} {m5,12:F3}   {m2 / m1,5:F2}x {m3 / m1,5:F2}x {m4 / m1,5:F2}x {m5 / m1,5:F2}x");
        }

        SizeCensus();
        Console.WriteLine();
        Console.WriteLine($"(sink {s_sink})");
    }
}
