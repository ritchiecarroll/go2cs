using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// A generic per-N inline array: N in the attribute, T a type parameter.
[InlineArray(201)] public struct Arr201<T> { private T _e; }
[InlineArray(4)] public struct Arr4<T> { private T _e; }

// Go: struct{ FforceAlignment [0]int16; Fa [201]int8; X int32 } -> Fa@0 (size 201), X@204, size 208, align 4.
public struct DigitPairsSeq { public Arr201<sbyte> Fa; public int X; }

// The same at Go's own offsets; the zero-length array as an explicit-layout zero-width field is impossible, so it is
// expressed only through the struct's alignment (here int32's 4 dominates int16's 2).
[StructLayout(LayoutKind.Explicit, Size = 208)]
public struct DigitPairsExplicit { [FieldOffset(0)] public Arr201<sbyte> Fa; [FieldOffset(204)] public int X; }

public struct WithRefs { public Arr4<string> Names; public int N; }

public sealed class Box<T> { public T Value; }

// ---- Review revision (2026-11-13): the shapes the Fable review named. ----
[InlineArray(2)] public struct Arr2<T> { private T _e; }
[InlineArray(1)] public struct Arr1<T> { private T _e; }
[InlineArray(8)] public struct Arr8<T> { private T _e; }
[InlineArray(64)] public struct Arr64<T> { private T _e; }
[InlineArray(128)] public struct Arr128<T> { private T _e; }

// S22's REAL shape: struct{ FforceAlignment [0]int16; Fa [201]int8; F__ccgo_pad2 [1]byte }. Go: size 202, align 2.
// (a) sequential, the [0]int16 dropped (an inline array cannot have length 0)
public struct S22Seq { public Arr201<sbyte> Fa; public byte F__ccgo_pad2; }
public struct OuterSeq { public byte B; public S22Seq S; }
// (b) explicit at Go's offsets with Go's Size, the [0]int16 still carrying nothing
[StructLayout(LayoutKind.Explicit, Size = 202)]
public struct S22ExplicitNoCarrier { [FieldOffset(0)] public Arr201<sbyte> Fa; [FieldOffset(201)] public byte F__ccgo_pad2; }
public struct OuterExplicitNoCarrier { public byte B; public S22ExplicitNoCarrier S; }
// (c) explicit, the [0]int16 carried as an overlapped READONLY field of its alignment at its Go offset
[StructLayout(LayoutKind.Explicit, Size = 202)]
public struct S22Carrier { [FieldOffset(0)] public readonly short FforceAlignment; [FieldOffset(0)] public Arr201<sbyte> Fa; [FieldOffset(201)] public byte F__ccgo_pad2; }
public struct OuterCarrier { public byte B; public S22Carrier S; }

// struct{ A byte; _ [0][2]int64 }: Go size 16, align 8. The carrier is a primitive of the element's ALIGNMENT, not the
// element: carrying the element itself ([2]int64, 16 bytes at offset 8) overshoots to 24.
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TrailCarrierElem { [FieldOffset(0)] public byte A; [FieldOffset(8)] public readonly Arr2<long> Z; }
[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TrailCarrierAlign { [FieldOffset(0)] public byte A; [FieldOffset(8)] public readonly long Z; }
// struct{ A byte; Z [0]int32; B byte }: Go Z@4, B@4, size 8, align 4; the carrier overlaps B.
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct MidCarrier { [FieldOffset(0)] public byte A; [FieldOffset(4)] public readonly int Z; [FieldOffset(4)] public byte B; }

// Equality: the default ValueType.Equals/GetHashCode on an inline array and on a holder.
public struct HoldsInline { public Arr4<int> A; public int N; }
// A family member that implements equality itself, and a holder of it that keeps the default.
[InlineArray(4)]
public struct Arr4Eq<T> : IEquatable<Arr4Eq<T>> where T : IEquatable<T>
{
    private T _e;
    private ReadOnlySpan<T> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Arr4Eq<T>, T>(ref Unsafe.AsRef(in this)), 4);
    public bool Equals(Arr4Eq<T> other) => AsSpan().SequenceEqual(other.AsSpan());
    public override bool Equals(object obj) => obj is Arr4Eq<T> o && Equals(o);
    public override int GetHashCode() { var h = new HashCode(); foreach (T e in AsSpan()) h.Add(e); return h.ToHashCode(); }
}
public struct HoldsInlineEq { public Arr4Eq<int> A; public int N; }
public struct HoldsInlineEqGenerated : IEquatable<HoldsInlineEqGenerated>
{
    public Arr4Eq<int> A; public int N;
    public bool Equals(HoldsInlineEqGenerated o) => A.Equals(o.A) && N == o.N;
    public override bool Equals(object obj) => obj is HoldsInlineEqGenerated o && Equals(o);
    public override int GetHashCode() => HashCode.Combine(A, N);
}

public struct ReflectHolder { public Arr201<sbyte> Fa; public int X; }
public struct ReflectHolderArray { public sbyte[] Fa; public int X; }   // the array<T> shape: a reference to storage

// The composed family: any N from golib-only power-of-two blocks, joined by a generic pair.
public struct Cat<A, B> { public A First; public B Second; }
public struct Pad16 { public long L; public byte B; }

public struct AsyncHolder { public Arr4<int> a; }


public static class Program
{
    public static unsafe int Main()
    {
        int fail = 0;
        void Check(string what, bool ok, string detail = "") { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what} {detail}"); if (!ok) fail++; }

        // 1. layout and size
        Check("generic Arr201<sbyte> size 201", Unsafe.SizeOf<Arr201<sbyte>>() == 201, $"{Unsafe.SizeOf<Arr201<sbyte>>()}");
        var seq = new DigitPairsSeq();
        long seqX = Unsafe.ByteOffset(ref Unsafe.As<DigitPairsSeq, byte>(ref seq), ref Unsafe.As<int, byte>(ref seq.X));
        Check("sequential: X at Go offset 204, size 208", seqX == 204 && Unsafe.SizeOf<DigitPairsSeq>() == 208, $"X@{seqX} size {Unsafe.SizeOf<DigitPairsSeq>()}");
        Check("sequential struct is unmanaged (blittable)", !RuntimeHelpers.IsReferenceOrContainsReferences<DigitPairsSeq>());
        Check("explicit at Go offsets: size 208", Unsafe.SizeOf<DigitPairsExplicit>() == 208, $"{Unsafe.SizeOf<DigitPairsExplicit>()}");
        Check("Marshal.SizeOf agrees (blittable for interop)", Marshal.SizeOf<DigitPairsExplicit>() == 208, $"{Marshal.SizeOf<DigitPairsExplicit>()}");

        // 2. read native memory as the struct with no copy (a ref over native bytes), and write back
        byte* native = (byte*)NativeMemory.AllocZeroed(208);
        for (int i = 0; i < 201; i++) native[i] = (byte)(i % 100);
        *(int*)(native + 204) = 0x01020304;
        ref DigitPairsExplicit view = ref Unsafe.AsRef<DigitPairsExplicit>(native);
        Check("native view reads Fa[200] and X in place", view.Fa[200] == 0 && view.Fa[199] == 99 && view.X == 0x01020304, $"Fa[199]={view.Fa[199]} X={view.X:x}");
        view.Fa[5] = -1; view.X = 7;
        Check("a write through the view lands in native memory", native[5] == 0xFF && *(int*)(native + 204) == 7);
        Span<sbyte> faSpan = view.Fa; // C# 12 inline-array to Span conversion
        faSpan[6] = -2;
        Check("a Span over the native field aliases it", native[6] == 0xFE);
        NativeMemory.Free(native);

        // 3. value semantics: a copy copies the inline array (Go's array value copy)
        var a = new DigitPairsSeq(); a.Fa[0] = 1;
        var b = a; b.Fa[0] = 2;
        Check("struct copy is a deep copy of the inline array", a.Fa[0] == 1 && b.Fa[0] == 2);

        // 4. managed elements are accepted
        var w = new WithRefs(); w.Names[3] = "x";
        Check("managed element inline array works", w.Names[3] == "x" && RuntimeHelpers.IsReferenceOrContainsReferences<WithRefs>());

        // 5. a slice-like (owner, byte offset) over an inline field inside a heap box survives a compacting GC
        var box = new Box<DigitPairsSeq>();
        box.Value.Fa[10] = 42;
        object owner = box;
        nint offset = Unsafe.ByteOffset(ref RawData(owner), ref Unsafe.As<sbyte, byte>(ref box.Value.Fa[0]));
        for (int i = 0; i < 3; i++) { _ = new byte[1 << 20]; GC.Collect(2, GCCollectionMode.Forced, true, true); }
        ref sbyte e10 = ref Unsafe.As<byte, sbyte>(ref Unsafe.AddByteOffset(ref RawData(owner), offset + 10));
        Check("(owner, offset) reads through after a compacting GC", e10 == 42, $"offset {offset}");
        e10 = 43;
        Check("(owner, offset) writes are seen through the box", box.Value.Fa[10] == 43);

        // 6. limits: lengths via Reflection.Emit (the attribute takes a constant)
        foreach (int n in new[] { 1, 65535, 1 << 20, (1 << 20) + 1, 1 << 24 })
            Console.WriteLine($"LIMIT byte[{n}]: {TryLoad(n, typeof(byte))}");
        foreach (int n in new[] { 1 << 17, (1 << 17) + 1 })
            Console.WriteLine($"LIMIT long[{n}] ({(long)n * 8} bytes): {TryLoad(n, typeof(long))}");
        Console.WriteLine($"LIMIT length 0: {TryLoad(0, typeof(byte))}");

        // ---- Review revision (2026-11-13) ----
        RevisionS22(Check);
        RevisionEquality(Check);
        RevisionAwait(Check).GetAwaiter().GetResult();
        RevisionOwnerMoved(Check);
        RevisionReflect(Check);
        RevisionComposed(Check);
        RevisionMarshalGate(Check);

        Console.WriteLine($"failed: {fail}");
        return fail;
    }


    private static long Off<TOuter, TField>(ref TOuter o, ref TField f) =>
        Unsafe.ByteOffset(ref Unsafe.As<TOuter, byte>(ref o), ref Unsafe.As<TField, byte>(ref f));

    private static void RevisionS22(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- S22 real shape (Go: S22 size 202 align 2; Outer{B byte; S S22}: S@2 size 204; [2]S22 404)");
        var a = new OuterSeq(); var b = new OuterExplicitNoCarrier(); var c = new OuterCarrier();
        long sa = Off(ref a, ref a.S), sb = Off(ref b, ref b.S), sc = Off(ref c, ref c.S);
        Console.WriteLine($"S22 sizes: seq {Unsafe.SizeOf<S22Seq>()}, explicit-no-carrier {Unsafe.SizeOf<S22ExplicitNoCarrier>()}, carrier {Unsafe.SizeOf<S22Carrier>()}");
        Check("REPRODUCES the review: sequential, [0]int16 dropped, nests S at 1 and outer size 203 (Go 2, 204)", sa == 1 && Unsafe.SizeOf<OuterSeq>() == 203, $"S@{sa} size {Unsafe.SizeOf<OuterSeq>()}");
        Check("REPRODUCES: explicit at Go offsets and Go Size, no carrier, ALSO nests S at 1, size 203", sb == 1 && Unsafe.SizeOf<OuterExplicitNoCarrier>() == 203, $"S@{sb} size {Unsafe.SizeOf<OuterExplicitNoCarrier>()}");
        Check("explicit, [0]int16 as an overlapped readonly short: nested S at 2 and size 204", sc == 2 && Unsafe.SizeOf<OuterCarrier>() == 204, $"S@{sc} size {Unsafe.SizeOf<OuterCarrier>()}");
        Check("carrier: [2]S22 is 404", Unsafe.SizeOf<Arr2<S22Carrier>>() == 404, $"{Unsafe.SizeOf<Arr2<S22Carrier>>()}");
        Check("carrier: unmanaged", !RuntimeHelpers.IsReferenceOrContainsReferences<OuterCarrier>(), "");
        var s = new S22Carrier(); s.Fa[0] = 5; s.Fa[1] = 6;
        Console.WriteLine($"carrier aliasing: after Fa[0]=5, Fa[1]=6 the readonly carrier reads {s.FforceAlignment} (Go never reads a [0]int16; equality and the image encoder must skip it)");
        Console.WriteLine("-- carrier by alignment, not by element (Go: struct{A byte; _ [0][2]int64} size 16 align 8; struct{A byte; Z [0]int32; B byte} Z@4 B@4 size 8)");
        Check("carrying the element [2]int64 overshoots Go's 16", Unsafe.SizeOf<TrailCarrierElem>() != 16, $"{Unsafe.SizeOf<TrailCarrierElem>()}");
        Check("carrying a long (the element's alignment) gives Go's 16", Unsafe.SizeOf<TrailCarrierAlign>() == 16, $"{Unsafe.SizeOf<TrailCarrierAlign>()}");
        var m = new MidCarrier();
        Check("mid-struct carrier overlapping B: B@4, size 8", Off(ref m, ref m.B) == 4 && Unsafe.SizeOf<MidCarrier>() == 8, $"B@{Off(ref m, ref m.B)} size {Unsafe.SizeOf<MidCarrier>()}");
    }

    private static string Try(Func<object> f)
    {
        try { return $"returned {f()}"; }
        catch (Exception ex) { return $"{ex.GetType().Name}: {ex.Message.Split('\n')[0]}"; }
    }

    private static void RevisionEquality(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- equality");
        var x = new Arr4<int>(); var y = new Arr4<int>();
        string r1 = Try(() => x.Equals(y)), r2 = Try(() => x.GetHashCode());
        var h1 = new HoldsInline(); var h2 = new HoldsInline();
        string r3 = Try(() => h1.Equals(h2)), r4 = Try(() => h1.GetHashCode());
        string r5 = Try(() => { var d = new System.Collections.Generic.Dictionary<HoldsInline, int>(); d[h1] = 1; return d.Count; });
        string r5b = Try(() => { var d = new System.Collections.Generic.Dictionary<HoldsInline, int>(); d[h1] = 1; return d.ContainsKey(h2); });
        Console.WriteLine($"holder as Dictionary key, then a lookup of an equal key: {r5b}");
        Console.WriteLine($"Arr4<int>.Equals: {r1}\nArr4<int>.GetHashCode: {r2}\nholder.Equals: {r3}\nholder.GetHashCode: {r4}\nholder as Dictionary key: {r5}");
        Check("default Equals on an inline array throws NotSupportedException", r1.StartsWith("NotSupportedException"), "");
        Check("default Equals on a holder of one throws NotSupportedException", r3.StartsWith("NotSupportedException"), "");
        var e1 = new HoldsInlineEq(); var e2 = new HoldsInlineEq(); e2.A[3] = 1;
        string r6 = Try(() => e1.Equals(e2));
        Console.WriteLine($"holder (default Equals) of a member that implements Equals, differing in A[3]: {r6}");
        var g1 = new HoldsInlineEqGenerated(); var g2 = new HoldsInlineEqGenerated(); g2.A[3] = 1;
        var g3 = new HoldsInlineEqGenerated();
        Check("generated holder equality: unequal when A[3] differs, equal when not, usable as a key",
            !g1.Equals(g2) && g1.Equals(g3) && g1.GetHashCode() == g3.GetHashCode() && new System.Collections.Generic.HashSet<HoldsInlineEqGenerated> { g1, g3 }.Count == 1, "");
    }

    private static async Task<int> One() { await Task.Yield(); return 1; }

    private static async Task RevisionAwait(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- await (the compile-error cases are in probe-await/, which must FAIL to build)");
        var p = new AsyncHolder();
        p.a[0] = await One();                       // a plain store across an await
        p.a[1] = 5;
        int t = await One(); p.a[1] += t;           // the split compound assignment the twin emitter would write
        Check("plain store and split compound across await, in an async method's local", p.a[0] == 1 && p.a[1] == 6, $"a[0]={p.a[0]} a[1]={p.a[1]}");
    }

    private static unsafe nint AddrOf(object o) => *(nint*)Unsafe.AsPointer(ref o);

    private static void RevisionOwnerMoved(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- (owner, offset) across a MOVE, with controls");
        var junk = new object[4096];
        for (int i = 0; i < junk.Length; i++) junk[i] = new byte[64];
        var box = new Box<DigitPairsSeq>();
        var junk2 = new object[4096];
        for (int i = 0; i < junk2.Length; i++) junk2[i] = new byte[64];
        box.Value.Fa[10] = 42;
        object owner = box;
        nint offset = Unsafe.ByteOffset(ref RawData(owner), ref Unsafe.As<sbyte, byte>(ref box.Value.Fa[0]));
        var pinnedBox = new Box<DigitPairsSeq>();
        var handle = GCHandle.Alloc(pinnedBox, GCHandleType.Pinned);
        nint before = AddrOf(owner), pinnedBefore = AddrOf(pinnedBox);
        int genBefore = GC.GetGeneration(owner);
        junk = null; junk2 = null;
        for (int i = 0; i < 3; i++) GC.Collect(2, GCCollectionMode.Forced, true, true);
        nint after = AddrOf(owner), pinnedAfter = AddrOf(pinnedBox);
        Console.WriteLine($"owner gen {genBefore} -> {GC.GetGeneration(owner)}; address moved by {after - before} bytes; pinned control moved by {pinnedAfter - pinnedBefore}");
        Check("the owner object MOVED during the compacting collections", after != before, "");
        Check("negative control: a pinned owner did NOT move (the address read is not noise)", pinnedAfter == pinnedBefore, "");
        handle.Free();
        ref sbyte e10 = ref Unsafe.As<byte, sbyte>(ref Unsafe.AddByteOffset(ref RawData(owner), offset + 10));
        Check("(owner, offset) reads through the moved owner", e10 == 42, $"offset {offset}");
        e10 = 43;
        Check("(owner, offset) writes are seen through the moved box", box.Value.Fa[10] == 43, "");
        // Negative control: an owner MINTED at slicing time (boxing a copy of a by-value local) loses the write.
        var local = new DigitPairsSeq(); local.Fa[10] = 42;
        object minted = local;
        nint mOffset = Unsafe.ByteOffset(ref RawData(minted), ref Unsafe.As<sbyte, byte>(ref Unsafe.Unbox<DigitPairsSeq>(minted).Fa[0]));
        Unsafe.As<byte, sbyte>(ref Unsafe.AddByteOffset(ref RawData(minted), mOffset + 10)) = 43;
        Check("negative control: a box minted at slicing time does NOT alias the local (the write is lost)", local.Fa[10] == 42, $"local.Fa[10]={local.Fa[10]}");
    }

    private static void RevisionReflect(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- reflection");
        object boxed = new ReflectHolder();
        FieldInfo fi = typeof(ReflectHolder).GetField("Fa");
        var copy = (Arr201<sbyte>)fi.GetValue(boxed);
        copy[0] = 9;
        Check("FieldInfo.GetValue on an inline field is a COPY: an element write through it is lost", Unsafe.Unbox<ReflectHolder>(boxed).Fa[0] == 0, "");
        object boxedArr = new ReflectHolderArray { Fa = new sbyte[201] };
        var shared = (sbyte[])typeof(ReflectHolderArray).GetField("Fa").GetValue(boxedArr);
        shared[0] = 9;
        Check("control: the same through a reference-backed field (array<T>'s shape) lands", ((ReflectHolderArray)boxedArr).Fa[0] == 9, "");
        nint off = Unsafe.ByteOffset(ref Unsafe.As<ReflectHolder, byte>(ref Unsafe.Unbox<ReflectHolder>(boxed)), ref Unsafe.As<Arr201<sbyte>, byte>(ref Unsafe.Unbox<ReflectHolder>(boxed).Fa));
        Unsafe.As<byte, sbyte>(ref Unsafe.AddByteOffset(ref Unsafe.As<ReflectHolder, byte>(ref Unsafe.Unbox<ReflectHolder>(boxed)), off)) = 9;
        Check("by-ref route (owner, field offset): the write lands", Unsafe.Unbox<ReflectHolder>(boxed).Fa[0] == 9, "");
    }

    private static void RevisionComposed(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- composed family: 201 = 128 + 64 + 8 + 1 as Cat<Cat<Arr128,Arr64>,Cat<Arr8,Arr1>>");
        Composed<sbyte>(Check); Composed<long>(Check); Composed<Pad16>(Check); Composed<string>(Check);
    }

    private static void Composed<T>(Action<string, bool, string> Check)
    {
        var c = new Cat<Cat<Arr128<T>, Arr64<T>>, Cat<Arr8<T>, Arr1<T>>>();
        int es = Unsafe.SizeOf<T>();
        long o128 = Off(ref c, ref c.First.First), o64 = Off(ref c, ref c.First.Second), o8 = Off(ref c, ref c.Second.First), o1 = Off(ref c, ref c.Second.Second);
        bool contiguous = o128 == 0 && o64 == 128L * es && o8 == 192L * es && o1 == 200L * es && Unsafe.SizeOf<Cat<Cat<Arr128<T>, Arr64<T>>, Cat<Arr8<T>, Arr1<T>>>>() == 201 * es;
        Check($"composed 201 of {typeof(T).Name} is contiguous and 201 elements long", contiguous,
            $"blocks at {o128}/{o64}/{o8}/{o1} (want 0/{128 * es}/{192 * es}/{200 * es}), size {Unsafe.SizeOf<Cat<Cat<Arr128<T>, Arr64<T>>, Cat<Arr8<T>, Arr1<T>>>>()} (want {201 * es})");
    }

    private static void RevisionMarshalGate(Action<string, bool, string> Check)
    {
        Console.WriteLine("-- the gate's instrument: Marshal.OffsetOf (what ClrLayoutMismatch reads) over inline fields");
        Console.WriteLine($"Marshal.OffsetOf(S22Carrier, F__ccgo_pad2): {Try(() => Marshal.OffsetOf<S22Carrier>("F__ccgo_pad2"))}");
        Console.WriteLine($"Marshal.OffsetOf(OuterCarrier, S): {Try(() => Marshal.OffsetOf<OuterCarrier>("S"))}");
        Console.WriteLine($"Marshal.OffsetOf(DigitPairsSeq, X): {Try(() => Marshal.OffsetOf<DigitPairsSeq>("X"))}");
        Console.WriteLine($"Marshal.SizeOf(OuterCarrier): {Try(() => Marshal.SizeOf<OuterCarrier>())}");
        Console.WriteLine($"Marshal.OffsetOf(Cat<Arr8<long>,Arr1<long>>, Second): {Try(() => Marshal.OffsetOf(typeof(Cat<Arr8<long>, Arr1<long>>), "Second"))}");
        var o = new OuterCarrier();
        Check("Unsafe.ByteOffset reads the CLR offset with no marshaling (the instrument a non-vacuous gate can use)", Off(ref o, ref o.S) == 2, "");
    }

    private sealed class RawHolder { public byte Data; }
    private static ref byte RawData(object o) => ref Unsafe.As<RawHolder>(o).Data;

    private static string TryLoad(int length, Type element)
    {
        try
        {
            var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ia" + length + element.Name), AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule("m");
            var tb = mb.DefineType("A" + length, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout, typeof(ValueType));
            tb.DefineField("_e", element, FieldAttributes.Private);
            tb.SetCustomAttribute(new CustomAttributeBuilder(typeof(InlineArrayAttribute).GetConstructor([typeof(int)])!, [length]));
            Type t = tb.CreateType();
            object v = Activator.CreateInstance(t)!;
            int size = (int)typeof(Unsafe).GetMethod("SizeOf")!.MakeGenericMethod(t).Invoke(null, null)!;
            return $"loaded, size {size}";
        }
        catch (Exception ex) { var inner = ex is TargetInvocationException tie ? tie.InnerException! : ex; return $"{inner.GetType().Name}: {inner.Message.Split('\n')[0]}"; }
    }
}
