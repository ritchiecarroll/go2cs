using System;
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

        Console.WriteLine($"failed: {fail}");
        return fail;
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
