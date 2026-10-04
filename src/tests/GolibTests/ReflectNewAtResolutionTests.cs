using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// reflect.NewAt's pointer resolution (GoReflect.ResolveNewAtPointee; go-cmp class L, ruled 2026-10-04).
// go-cmp reads an unexported field as reflect.NewAt(f.Type, unsafe.Pointer(v.UnsafeAddr() + f.Offset)),
// and NewAt used to ignore its pointer. The resolution is EXACT ONLY: a live token at offset 0, or a
// live token's block plus an offset landing exactly on a node of the requested type. These are the
// ruling's controls: the hits, the miss path, a type mismatch, and a token whose box is gone.
[TestClass]
public class ReflectNewAtResolutionTests
{
    // Reference-bearing, so its pointer is an order token rather than a pinned address.
    private struct Holder
    {
        internal Action first;
        internal long count;
        internal Action third;
    }

    private static nuint tokenOf<T>(ж<T> box) => (nuint)(uintptr)box;

    [TestMethod]
    public void AnExactTokenResolvesToItsOwnBox()
    {
        ж<Holder> box = new StandardBox<Holder>(new Holder { count = 3 });

        object? resolved = GoReflect.ResolveNewAtPointee(tokenOf(box), typeof(Holder));

        Assert.AreSame(box, resolved, "a token at offset 0 with the box's own type is the box");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void ATokenPlusAFieldOffsetResolvesToThatField()
    {
        ж<Holder> box = new StandardBox<Holder>(new Holder { count = 3 });

        // count is the second field: Go offset 8.
        ж<long>? count = GoReflect.ResolveNewAtPointee(tokenOf(box) + 8, typeof(long)) as ж<long>;

        Assert.IsNotNull(count, "offset 8 lands exactly on the int64 field");
        Assert.AreEqual(3L, count!.Value, "and reads the real field");

        count.ValueSlot = 30;
        Assert.AreEqual(30L, box.Value.count, "a write through the result lands in the struct");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void AnOffsetThatIsNotAFieldOfTheRequestedTypeIsAMiss()
    {
        ж<Holder> box = new StandardBox<Holder>(new Holder());

        Assert.IsNull(GoReflect.ResolveNewAtPointee(tokenOf(box) + 8, typeof(int)), "a type mismatch is never reinterpreted");
        Assert.IsNull(GoReflect.ResolveNewAtPointee(tokenOf(box) + 4, typeof(long)), "an offset inside a field is not a node");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void ANumberNoProjectionHandedOutIsAMiss()
    {
        Assert.IsNull(GoReflect.ResolveNewAtPointee(0, typeof(long)), "nil");
        Assert.IsNull(GoReflect.ResolveNewAtPointee(0x1234, typeof(long)), "a number the table never saw keeps NewAt's zero box");
    }

    [TestMethod]
    public void ATokenWhoseBoxWasCollectedIsAMiss()
    {
        // The table holds boxes WEAKLY: once the box is unreachable, its token resolves to nothing.
        (nuint token, WeakReference alive) = mintAndDrop();

        for (int i = 0; i < 5 && alive.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(alive.IsAlive, "the premise: the box was collected");
        Assert.IsNull(GoReflect.ResolveNewAtPointee(token, typeof(Holder)), "a stale token answers nothing");
        Assert.IsNull(GoReflect.ResolveNewAtPointee(token + 8, typeof(long)), "nor does arithmetic on it");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (nuint, WeakReference) mintAndDrop()
    {
        ж<Holder> box = new StandardBox<Holder>(new Holder());
        return (tokenOf(box), new WeakReference(box));
    }
}
