// proxyprobe: can a Box<E>[] (golib's ж<E>[] stand-in) be reinterpreted as an ItemProxy[] (a one-field readonly struct
// over the box), GC-safely and type-safely, so a slice of boxes is a slice of proxies in place? Each arm prints PASS or
// FAIL with what it observed. Standalone; nothing here is golib.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

interface INamed { string Name(); }

abstract class Box<E> where E : class { public abstract E Value { get; } }

sealed class StandardBox<E>(E v) : Box<E> where E : class { public override E Value => v; }

sealed class OtherBox<E>(E v) : Box<E> where E : class { public override E Value => v; }

sealed class Item(string n) { public string N = n; }

readonly struct ItemProxy(Box<Item> box) : INamed
{
    private readonly Box<Item> m_box = box;
    public Box<Item> Box => m_box;
    public string Name() => m_box.Value.N;
}

static class Program
{
    static int failures;

    static void Check(string arm, bool ok, string observed)
    {
        Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  {arm}: {observed}");
        if (!ok) failures++;
    }

    // A generic consumer constrained by the interface, as converted code is (`where T : Named`).
    [MethodImpl(MethodImplOptions.NoInlining)]
    static string Join<T>(T[] xs) where T : INamed
    {
        var parts = new List<string>();
        foreach (var x in xs) parts.Add(x.Name());
        return string.Join(",", parts);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void SwapFirstTwo<T>(T[] xs) => (xs[0], xs[1]) = (xs[1], xs[0]);

    static Box<Item>[] Boxes(params string[] names)
    {
        var a = new Box<Item>[names.Length];
        for (int i = 0; i < names.Length; i++) a[i] = new StandardBox<Item>(new Item(names[i]));
        return a;
    }

    static void Main()
    {
        Console.WriteLine($"runtime {Environment.Version}, 64-bit {Environment.Is64BitProcess}, " +
                          $"tiered={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default"}, " +
                          $"heapverify={Environment.GetEnvironmentVariable("DOTNET_HeapVerify") ?? "off"}");

        // 1. Layout: the proxy is exactly one reference wide.
        Check("layout", Unsafe.SizeOf<ItemProxy>() == IntPtr.Size, $"SizeOf<ItemProxy> = {Unsafe.SizeOf<ItemProxy>()}, IntPtr.Size = {IntPtr.Size}");

        // 2. The runtime's own safe reinterpretation, MemoryMarshal.Cast<TFrom, TTo>, does not even COMPILE for a
        // reference element: TFrom is constrained `struct` (CS0453 for Box<Item>). The span route that does compile is a
        // by-ref pun of element 0 into a span view; it aliases, but a span is stack-only, so slice<T> cannot hold it.
        Box<Item>[] spanArr = Boxes("p", "q");
        Span<ItemProxy> spanView = MemoryMarshal.CreateSpan(ref Unsafe.As<Box<Item>, ItemProxy>(ref spanArr[0]), spanArr.Length);
        spanView[1] = new ItemProxy(new StandardBox<Item>(new Item("q2")));
        Check("by-ref span view aliases (stack-only)", spanView[0].Name() == "p" && spanArr[1].Value.N == "q2", $"{spanView[0].Name()},{spanArr[1].Value.N}");

        // 3. The object pun: reads, the constrained generic consumer, writes, swaps, and Array.Sort, all aliasing.
        Box<Item>[] arr = Boxes("b", "a", "c");
        ItemProxy[] view = Unsafe.As<ItemProxy[]>(arr);
        Check("read through view", Join(view) == "b,a,c", Join(view));
        SwapFirstTwo(view);
        Check("swap through view is seen by the box array", arr[0].Value.N == "a" && arr[1].Value.N == "b", $"{arr[0].Value.N},{arr[1].Value.N},{arr[2].Value.N}");
        var fresh = new StandardBox<Item>(new Item("z"));
        view[2] = new ItemProxy(fresh);
        Check("write through view is the same box", ReferenceEquals(arr[2], fresh), $"arr[2] is fresh: {ReferenceEquals(arr[2], fresh)}");
        Array.Sort(view, (x, y) => string.CompareOrdinal(y.Name(), x.Name()));
        Check("Array.Sort<T> through view", Join(view) == "z,b,a" && arr[0] == fresh, Join(view));
        var copy = new ItemProxy[3];
        new Span<ItemProxy>(view).CopyTo(copy);
        Check("Span ctor + CopyTo (no covariance check for a struct T)", ReferenceEquals(copy[0].Box, arr[0]), "copied 3");

        // 4. What sees the TRUE array type.
        Check("GetType sees Box<Item>[]", view.GetType() == typeof(Box<Item>[]), view.GetType().Name);
        object o = view;
        Check("`is ItemProxy[]` is false on the punned object", o is not ItemProxy[], $"o is ItemProxy[]: {o is ItemProxy[]}");
        try
        {
            Array.Copy(view, 0, new ItemProxy[3], 0, 3);
            Check("Array.Copy refuses", false, "it copied");
        }
        catch (ArrayTypeMismatchException e) { Check("Array.Copy refuses", true, e.GetType().Name); }
        try
        {
            var c = (ItemProxy[])view.Clone();
            Check("Clone cast refuses", false, $"it cast {c.Length}");
        }
        catch (InvalidCastException e) { Check("Clone cast refuses", true, e.GetType().Name); }

        // 5. The covariance hole: a Box<Item>[] that is really a StandardBox<Item>[].
        Box<Item>[] cov = new StandardBox<Item>[2];
        try { cov[0] = new OtherBox<Item>(new Item("x")); Check("direct covariant store refused", false, "stored"); }
        catch (ArrayTypeMismatchException) { Check("direct covariant store refused", true, "ArrayTypeMismatchException"); }
        Unsafe.As<ItemProxy[]>(cov)[0] = new ItemProxy(new OtherBox<Item>(new Item("x")));
        object held = ((object[])cov)[0];
        Check("struct-view store into a covariant array is NOT refused (type hole)", held is OtherBox<Item>,
              $"a {held.GetType().Name} now sits in a {cov.GetType().Name}");
        Check("the exact-type gate tells them apart", arr.GetType() == typeof(Box<Item>[]) && cov.GetType() != typeof(Box<Item>[]),
              $"arr exact: {arr.GetType() == typeof(Box<Item>[])}, cov exact: {cov.GetType() == typeof(Box<Item>[])}");

        // 6. GC: a gen2 box array, written through the view with gen0 boxes only it references, under repeated
        // gen0 and full collections. A missing write barrier lets a gen0 collection free a box the old array still holds.
        const int N = 4096;
        Box<Item>[] old = Boxes(new string[N].AsSpan().ToArray().Length > 0 ? MakeNames(N) : []);
        GC.Collect(); GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        int gen = GC.GetGeneration(old);
        ItemProxy[] oldView = Unsafe.As<ItemProxy[]>(old);
        var weak = new WeakReference[N];
        int bad = 0;
        for (int round = 0; round < 200; round++)
        {
            for (int i = 0; i < N; i++)
            {
                var b = new StandardBox<Item>(new Item($"r{round}i{i}"));
                oldView[i] = new ItemProxy(b);
                weak[i] = new WeakReference(b);
            }
            GC.Collect(0, GCCollectionMode.Forced, blocking: true);
            if (round % 50 == 49) GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            for (int i = 0; i < N; i++)
                if (!weak[i].IsAlive || old[i].Value.N != $"r{round}i{i}") bad++;
        }
        Check("GC card marking: gen0 boxes written through the view into a gen2 array survive and read back",
              bad == 0 && gen == 2, $"array generation {gen}, 200 rounds x {N} writes, {bad} lost or wrong");

        Console.WriteLine(failures == 0 ? "PROBE-PASS" : $"PROBE-FAIL {failures}");
    }

    static string[] MakeNames(int n) { var a = new string[n]; for (int i = 0; i < n; i++) a[i] = "s" + i; return a; }
}
