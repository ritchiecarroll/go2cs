using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// A heap-boxed local initialized in ONE call must not keep its initial value alive (1.26.10.2 freeze finding,
// crypto/internal/fips140cache).
//
// THE DEFECT. Seat #29 (H2) folds Go's `k := newKey()`, for a k whose address escapes, into
// `ref var k = ref heap<ж<key>>(newKey(), out var Ꮡk);`. golib's initializing overload took its value as
// `in T`, and an `in` parameter needs an address: for an rvalue argument the compiler spills the value into a hidden
// local and passes `ldloca`. That local is never cleared, and once its address is taken the JIT reports it live for
// the whole frame unless it can undo the exposure, which it does only when heap is INLINED. In a small frame heap
// inlines and the key dies. In fips140cache's TestCache, a large frame of lambdas and goroutines, it did not: after
// `k = nil` the old key stayed reachable, AddCleanup's callback never ran, and waitUnreachable's runtime.GC() loop
// spun to the 10-minute deadline at full optimization.
//
// THE IL GUARD runs everywhere and asks no JIT decision: a folded frame declares exactly the IL locals its split
// twin declares. The split shape (`heap<T>(out var Ꮡx); x = rhs;`) writes through the ref and spills nothing; with
// a by-value initializing parameter the fold matches it, and with an `in` one it adds a T-typed local.
//
// THE REACHABILITY ARMS reproduce the fips140cache condition itself, heap NOT inlined at full optimization, so they
// measure only under DOTNET_JitNoInline=1 (read once per process) and are inconclusive otherwise. Each fold site runs
// in its own full-opt frame, with the capped GC loop inside it, as waitUnreachable's is, and a WeakReference observes
// the referent. The split shape is each arm's positive control. Measured 2026-11-02 under DOTNET_JitNoInline=1:
//   - a reference T (the fips140cache shape): the `in` fold kept the key alive, and the split control collected it;
//   - a struct T holding a reference, cleared through the field: the split control did NOT collect either. With
//     nothing inlined, the constructor's `newobj` temp is passed as `this` and roots the field in BOTH shapes, so
//     this arm is inconclusive there. That is a pre-existing struct-temp hazard, not the fold's.
[TestClass]
public class HeapInitRootingTests
{
    // A struct as the converter emits one for a Go struct literal: a positional constructor (`new pair(v, 1)`).
    private struct Holder
    {
        public ж<long>? p;
        public nint n;

        public Holder(ж<long>? p, nint n)
        {
            this.p = p;
            this.n = n;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ж<long> NewKey() => new StandardBox<long>(1L);

    // Capped: a referent that is still rooted after this many full collections stays rooted.
    private static bool Dies(WeakReference weak)
    {
        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!weak.IsAlive)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool ReferenceFolded()
    {
        ref var k = ref heap<ж<long>>(NewKey(), out var Ꮡk);
        WeakReference weak = new(k);
        k = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(Ꮡk);     // the box stays live, as Go's does; only its old value may die
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool ReferenceSplit()
    {
        ref var k = ref heap<ж<long>>(out var Ꮡk);
        k = NewKey();
        WeakReference weak = new(k);
        k = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(Ꮡk);
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool StructFieldFolded()
    {
        ref var x = ref heap<Holder>(new Holder(NewKey(), 1), out var Ꮡx);
        WeakReference weak = new(x.p);
        x.p = default;
        bool dies = Dies(weak);
        GC.KeepAlive(Ꮡx);
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool StructFieldSplit()
    {
        ref var x = ref heap<Holder>(out var Ꮡx);
        x = new Holder(NewKey(), 1);
        WeakReference weak = new(x.p);
        x.p = default;
        bool dies = Dies(weak);
        GC.KeepAlive(Ꮡx);
        return dies;
    }

    private static Type[] LocalTypes(string frame) =>
        typeof(HeapInitRootingTests).GetMethod(frame, BindingFlags.NonPublic | BindingFlags.Static)!
            .GetMethodBody()!.LocalVariables.Select(local => local.LocalType).ToArray();

    [TestMethod]
    [DataRow(nameof(ReferenceFolded), nameof(ReferenceSplit))]
    [DataRow(nameof(StructFieldFolded), nameof(StructFieldSplit))]
    public void AFoldedInitSpillsNoLocalTheSplitShapeDoesNot(string folded, string split)
    {
        // Compile-time, so no JIT decision can hide it: the twins name the same locals, and any extra one in the
        // folded frame is the compiler's spill of the initial value, the slot that roots it.
        static string Names(Type[] types) => string.Join(", ", types.Select(type => type.Name));
        Type[] foldedLocals = LocalTypes(folded), splitLocals = LocalTypes(split);

        CollectionAssert.AreEqual(splitLocals, foldedLocals,
            $"{folded} declares [{Names(foldedLocals)}] where {split} declares [{Names(splitLocals)}]: the fold spills " +
            "its initial value into a local, so heap's initializing parameter is taking an address (`in`)");
    }

    [TestMethod]
    public void HeapClearsItsParameterOnceTheBoxHoldsTheValue()
    {
        // The SECOND root, measured with a dump of the hung converted TestCache at the by-value fix: heap was
        // INLINED there, so its parameter became a slot of TestCache's frame (rsp+0x1D8), and Ꮡ(in T) -- not
        // inlined, a shared generic -- took that slot's address. The slot was address-exposed, reported live for
        // the whole frame, and rooted the key. heap must clear its parameter once the box holds the value.
        // `target = default!` over a generic T compiles to `ldarga.s 0; initobj !!T` (0F 00 FE 15).
        MethodInfo heapInit = typeof(builtin).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "heap" && method.GetParameters().Length == 2);
        byte[] il = heapInit.GetMethodBody()!.GetILAsByteArray()!;

        bool clearsTarget = Enumerable.Range(0, il.Length - 3)
            .Any(i => il[i] == 0x0F && il[i + 1] == 0x00 && il[i + 2] == 0xFE && il[i + 3] == 0x15);

        Assert.IsTrue(clearsTarget,
            $"heap<T>(T, out ж<T>) leaves its parameter holding the initial value ({il.Length} IL bytes, no " +
            "`ldarga.s 0; initobj`): inlined into a large frame, that parameter is an address-taken caller slot that " +
            "roots the value for the whole frame");
    }

    private static void RequireNoInlining()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_JitNoInline") != "1")
            Assert.Inconclusive("measures fips140cache's condition, heap NOT inlined at full optimization: run GolibTests " +
                "-c Release with DOTNET_JitNoInline=1 (in a small frame heap inlines, and the spill is undone)");
    }

    [TestMethod]
    public void AFoldedReferenceInitIsCollectedOnceTheLocalIsCleared()
    {
        RequireNoInlining();

        if (!ReferenceSplit())
            Assert.Inconclusive("POSITIVE CONTROL: the split shape's key did not die either, so this run cannot observe a collection");

        Assert.IsTrue(ReferenceFolded(),
            "`heap<T>(rhs, out var Ꮡx)` kept the initial value alive after the local was cleared: the initializing " +
            "overload must take its value without an address, or the call site roots it for the whole frame");
    }

    [TestMethod]
    public void AFoldedStructInitDropsAFieldOnceTheFieldIsCleared()
    {
        RequireNoInlining();

        if (!StructFieldSplit())
            Assert.Inconclusive("POSITIVE CONTROL: the split shape's field did not die either (with nothing inlined, the " +
                "constructor's newobj temp roots it in both shapes), so this run cannot observe a collection");

        Assert.IsTrue(StructFieldFolded(),
            "`heap<S>(new S(ref, …), out var Ꮡx)` kept the field's old referent alive after `x.p = nil`: the struct " +
            "rvalue, or the caller-made copy it is passed through, roots it for the whole frame");
    }
}
