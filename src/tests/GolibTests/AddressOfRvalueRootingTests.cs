using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// `Ꮡ(rvalue)` must not keep the value it boxed alive (1.26.10.2, the sibling of HeapInitRootingTests; COORD ruled it
// in-hop).
//
// THE DEFECT. Go's `&T{...}` converts to `Ꮡ(new T(...))`, and golib's one-argument Ꮡ took `in T`. For an rvalue
// argument the compiler spills the value into a hidden local of the CALLER and passes its address. That local is
// never cleared, and once its address is taken the JIT reports it live for the caller's whole frame unless Ꮡ is
// inlined. The dump of crypto/internal/fips140cache's hung TestCache showed one such spill, `Ꮡ(new value())` at
// [rsp+0x288] passed by `lea`. There `value`'s field was nil, but a struct whose fields hold references keeps
// them alive the same way after the box's copy drops them. Ꮡ never aliases its argument: it always copies into a new
// box, so taking the value by value changes no meaning.
//
// THE GUARDS, as for heap:
//   - the caller's IL: a frame whose only T value is the rvalue it boxes declares no local of type T;
//   - Ꮡ's own IL clears its parameter once the box holds the value (`ldarga.s 0; initobj`). Inlined into a large
//     frame, that parameter is a caller slot, and the box constructor takes it `in`;
//   - reachability, a reference T, measured only under DOTNET_JitNoInline=1 (Ꮡ not inlined), inconclusive
//     otherwise. A struct T's reachability cannot be measured there: with nothing inlined, the struct's own
//     constructor temp roots its fields in every shape (a post-hop finding), so the IL guard covers it.
[TestClass]
public class AddressOfRvalueRootingTests
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

    // `p := &k` over an rvalue of reference type: Ꮡ(newKey()).
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool ReferenceRvalueBoxed()
    {
        ж<ж<long>> box = Ꮡ(NewKey());
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    // The control: the same box, filled through the slot, with no rvalue argument to spill.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool ReferenceSlotFilled()
    {
        ж<ж<long>> box = Ꮡ<ж<long>>(default!);
        box.ValueSlot = NewKey();
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    // `p := &S{p: obj}`: Ꮡ(new S(obj, 1)).
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool StructRvalueBoxed()
    {
        ж<Holder> box = Ꮡ(new Holder(NewKey(), 1));
        WeakReference weak = new(box.ValueSlot.p);
        box.ValueSlot.p = default;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    private static Type[] LocalTypes(string frame) =>
        typeof(AddressOfRvalueRootingTests).GetMethod(frame, BindingFlags.NonPublic | BindingFlags.Static)!
            .GetMethodBody()!.LocalVariables.Select(local => local.LocalType).ToArray();

    [TestMethod]
    [DataRow(nameof(ReferenceRvalueBoxed), typeof(ж<long>))]
    [DataRow(nameof(StructRvalueBoxed), typeof(Holder))]
    public void BoxingAnRvalueSpillsNoLocalOfItsType(string frame, Type boxed)
    {
        // Compile-time, so no JIT decision can hide it: the frame's only value of the boxed type is the rvalue
        // argument, so any local of that type is the compiler's spill of it, the slot that roots it.
        Type[] locals = LocalTypes(frame);

        Assert.IsFalse(locals.Contains(boxed),
            $"{frame} declares [{string.Join(", ", locals.Select(type => type.Name))}]: boxing an rvalue spilled a " +
            $"{boxed.Name} local, so Ꮡ's one-argument parameter is taking an address (`in`)");
    }

    [TestMethod]
    public void AddressOfClearsItsParameterOnceTheBoxHoldsTheValue()
    {
        // `target = default!` over a generic T compiles to `ldarga.s 0; initobj !!T` (0F 00 FE 15).
        MethodInfo addressOf = typeof(builtin).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "Ꮡ" && method.IsGenericMethodDefinition && method.GetParameters().Length == 1);
        byte[] il = addressOf.GetMethodBody()!.GetILAsByteArray()!;

        bool clearsTarget = Enumerable.Range(0, il.Length - 3)
            .Any(i => il[i] == 0x0F && il[i + 1] == 0x00 && il[i + 2] == 0xFE && il[i + 3] == 0x15);

        Assert.IsTrue(clearsTarget,
            $"Ꮡ<T>(T) leaves its parameter holding the boxed value ({il.Length} IL bytes, no `ldarga.s 0; initobj`): " +
            "inlined into a large frame, that parameter is a caller slot the box constructor takes by address");
    }

    [TestMethod]
    public void ABoxedReferenceRvalueIsCollectedOnceTheBoxIsCleared()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_JitNoInline") != "1")
            Assert.Inconclusive("measures the caller-spill condition, Ꮡ NOT inlined at full optimization: run GolibTests " +
                "-c Release with DOTNET_JitNoInline=1 (in a small frame Ꮡ inlines, and the spill is undone)");

        if (!ReferenceSlotFilled())
            Assert.Inconclusive("POSITIVE CONTROL: the slot-filled box's value did not die either, so this run cannot observe a collection");

        Assert.IsTrue(ReferenceRvalueBoxed(),
            "`Ꮡ(rvalue)` kept the boxed value alive after the box was cleared: Ꮡ's one-argument overload must take " +
            "its value without an address, or the call site roots it for the whole frame");
    }
}
