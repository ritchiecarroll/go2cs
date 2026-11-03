using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The box constructors must not keep an rvalue argument alive (1.26.10.2; COORD folded this into the Ꮡ seat once it
// was shown to root at DEFAULT inlining, a shipping configuration).
//
// THE DEFECT. StandardBox(in T value) and SlottedStandardBox(in T value) took `in`, so an rvalue argument
// (`new StandardBox<X>(<call>)`, 37 hand-owned sites in src/core and 3 in golib) was spilled into a hidden local of
// the caller and passed by address. Where the constructor is not inlined, that local is address-exposed and reported
// live for the caller's whole frame. Measured at default inlining, full optimization (.NET 10.0.400 JIT): in a small
// frame the constructor inlines and the value dies; in a LARGE frame (the shape below: sixty closures ahead of the
// box, as TestCache's lambdas and goroutines were) it does not inline. The disassembly reads
// `mov [rsp+0x20], rax; lea rdx, [rsp+0x20]; call StandardBox`1[__Canon]:.ctor(byref)`, and the value stays rooted.
//
// THE GUARDS:
//   - THE IL GUARD, deterministic: a frame whose only value of T is the rvalue it hands the constructor declares no
//     local of type T;
//   - THE REACHABILITY ARMS, at default inlining in the large frame, for each constructor. The inliner's decision is
//     a heuristic, so the IL guard is the half no JIT version can move; these arms are the measured symptom. The
//     slot-filled box is the positive control.
[TestClass]
public class BoxConstructorRvalueRootingTests
{
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

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool StandardSmall()
    {
        ж<ж<long>> box = new StandardBox<ж<long>>(NewKey());
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool SlottedSmall()
    {
        ж<ж<long>> box = new SlottedStandardBox<ж<long>>(NewKey());
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    // The large frame: sixty closures ahead of the box leave the constructor uninlined (measured).
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool StandardLarge()
    {
        int s = 0;
        Func<int, int> f1 = x => x + 1 + s;
        s += f1(1);
        if (s == -1) Console.WriteLine(f1);
        Func<int, int> f2 = x => x + 2 + s;
        s += f2(2);
        if (s == -2) Console.WriteLine(f2);
        Func<int, int> f3 = x => x + 3 + s;
        s += f3(3);
        if (s == -3) Console.WriteLine(f3);
        Func<int, int> f4 = x => x + 4 + s;
        s += f4(4);
        if (s == -4) Console.WriteLine(f4);
        Func<int, int> f5 = x => x + 5 + s;
        s += f5(5);
        if (s == -5) Console.WriteLine(f5);
        Func<int, int> f6 = x => x + 6 + s;
        s += f6(6);
        if (s == -6) Console.WriteLine(f6);
        Func<int, int> f7 = x => x + 7 + s;
        s += f7(7);
        if (s == -7) Console.WriteLine(f7);
        Func<int, int> f8 = x => x + 8 + s;
        s += f8(8);
        if (s == -8) Console.WriteLine(f8);
        Func<int, int> f9 = x => x + 9 + s;
        s += f9(9);
        if (s == -9) Console.WriteLine(f9);
        Func<int, int> f10 = x => x + 10 + s;
        s += f10(10);
        if (s == -10) Console.WriteLine(f10);
        Func<int, int> f11 = x => x + 11 + s;
        s += f11(11);
        if (s == -11) Console.WriteLine(f11);
        Func<int, int> f12 = x => x + 12 + s;
        s += f12(12);
        if (s == -12) Console.WriteLine(f12);
        Func<int, int> f13 = x => x + 13 + s;
        s += f13(13);
        if (s == -13) Console.WriteLine(f13);
        Func<int, int> f14 = x => x + 14 + s;
        s += f14(14);
        if (s == -14) Console.WriteLine(f14);
        Func<int, int> f15 = x => x + 15 + s;
        s += f15(15);
        if (s == -15) Console.WriteLine(f15);
        Func<int, int> f16 = x => x + 16 + s;
        s += f16(16);
        if (s == -16) Console.WriteLine(f16);
        Func<int, int> f17 = x => x + 17 + s;
        s += f17(17);
        if (s == -17) Console.WriteLine(f17);
        Func<int, int> f18 = x => x + 18 + s;
        s += f18(18);
        if (s == -18) Console.WriteLine(f18);
        Func<int, int> f19 = x => x + 19 + s;
        s += f19(19);
        if (s == -19) Console.WriteLine(f19);
        Func<int, int> f20 = x => x + 20 + s;
        s += f20(20);
        if (s == -20) Console.WriteLine(f20);
        Func<int, int> f21 = x => x + 21 + s;
        s += f21(21);
        if (s == -21) Console.WriteLine(f21);
        Func<int, int> f22 = x => x + 22 + s;
        s += f22(22);
        if (s == -22) Console.WriteLine(f22);
        Func<int, int> f23 = x => x + 23 + s;
        s += f23(23);
        if (s == -23) Console.WriteLine(f23);
        Func<int, int> f24 = x => x + 24 + s;
        s += f24(24);
        if (s == -24) Console.WriteLine(f24);
        Func<int, int> f25 = x => x + 25 + s;
        s += f25(25);
        if (s == -25) Console.WriteLine(f25);
        Func<int, int> f26 = x => x + 26 + s;
        s += f26(26);
        if (s == -26) Console.WriteLine(f26);
        Func<int, int> f27 = x => x + 27 + s;
        s += f27(27);
        if (s == -27) Console.WriteLine(f27);
        Func<int, int> f28 = x => x + 28 + s;
        s += f28(28);
        if (s == -28) Console.WriteLine(f28);
        Func<int, int> f29 = x => x + 29 + s;
        s += f29(29);
        if (s == -29) Console.WriteLine(f29);
        Func<int, int> f30 = x => x + 30 + s;
        s += f30(30);
        if (s == -30) Console.WriteLine(f30);
        Func<int, int> f31 = x => x + 31 + s;
        s += f31(31);
        if (s == -31) Console.WriteLine(f31);
        Func<int, int> f32 = x => x + 32 + s;
        s += f32(32);
        if (s == -32) Console.WriteLine(f32);
        Func<int, int> f33 = x => x + 33 + s;
        s += f33(33);
        if (s == -33) Console.WriteLine(f33);
        Func<int, int> f34 = x => x + 34 + s;
        s += f34(34);
        if (s == -34) Console.WriteLine(f34);
        Func<int, int> f35 = x => x + 35 + s;
        s += f35(35);
        if (s == -35) Console.WriteLine(f35);
        Func<int, int> f36 = x => x + 36 + s;
        s += f36(36);
        if (s == -36) Console.WriteLine(f36);
        Func<int, int> f37 = x => x + 37 + s;
        s += f37(37);
        if (s == -37) Console.WriteLine(f37);
        Func<int, int> f38 = x => x + 38 + s;
        s += f38(38);
        if (s == -38) Console.WriteLine(f38);
        Func<int, int> f39 = x => x + 39 + s;
        s += f39(39);
        if (s == -39) Console.WriteLine(f39);
        Func<int, int> f40 = x => x + 40 + s;
        s += f40(40);
        if (s == -40) Console.WriteLine(f40);
        Func<int, int> f41 = x => x + 41 + s;
        s += f41(41);
        if (s == -41) Console.WriteLine(f41);
        Func<int, int> f42 = x => x + 42 + s;
        s += f42(42);
        if (s == -42) Console.WriteLine(f42);
        Func<int, int> f43 = x => x + 43 + s;
        s += f43(43);
        if (s == -43) Console.WriteLine(f43);
        Func<int, int> f44 = x => x + 44 + s;
        s += f44(44);
        if (s == -44) Console.WriteLine(f44);
        Func<int, int> f45 = x => x + 45 + s;
        s += f45(45);
        if (s == -45) Console.WriteLine(f45);
        Func<int, int> f46 = x => x + 46 + s;
        s += f46(46);
        if (s == -46) Console.WriteLine(f46);
        Func<int, int> f47 = x => x + 47 + s;
        s += f47(47);
        if (s == -47) Console.WriteLine(f47);
        Func<int, int> f48 = x => x + 48 + s;
        s += f48(48);
        if (s == -48) Console.WriteLine(f48);
        Func<int, int> f49 = x => x + 49 + s;
        s += f49(49);
        if (s == -49) Console.WriteLine(f49);
        Func<int, int> f50 = x => x + 50 + s;
        s += f50(50);
        if (s == -50) Console.WriteLine(f50);
        Func<int, int> f51 = x => x + 51 + s;
        s += f51(51);
        if (s == -51) Console.WriteLine(f51);
        Func<int, int> f52 = x => x + 52 + s;
        s += f52(52);
        if (s == -52) Console.WriteLine(f52);
        Func<int, int> f53 = x => x + 53 + s;
        s += f53(53);
        if (s == -53) Console.WriteLine(f53);
        Func<int, int> f54 = x => x + 54 + s;
        s += f54(54);
        if (s == -54) Console.WriteLine(f54);
        Func<int, int> f55 = x => x + 55 + s;
        s += f55(55);
        if (s == -55) Console.WriteLine(f55);
        Func<int, int> f56 = x => x + 56 + s;
        s += f56(56);
        if (s == -56) Console.WriteLine(f56);
        Func<int, int> f57 = x => x + 57 + s;
        s += f57(57);
        if (s == -57) Console.WriteLine(f57);
        Func<int, int> f58 = x => x + 58 + s;
        s += f58(58);
        if (s == -58) Console.WriteLine(f58);
        Func<int, int> f59 = x => x + 59 + s;
        s += f59(59);
        if (s == -59) Console.WriteLine(f59);
        Func<int, int> f60 = x => x + 60 + s;
        s += f60(60);
        if (s == -60) Console.WriteLine(f60);
        ж<ж<long>> box = new StandardBox<ж<long>>(NewKey());
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        GC.KeepAlive(s);
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool SlottedLarge()
    {
        int s = 0;
        Func<int, int> f1 = x => x + 1 + s;
        s += f1(1);
        if (s == -1) Console.WriteLine(f1);
        Func<int, int> f2 = x => x + 2 + s;
        s += f2(2);
        if (s == -2) Console.WriteLine(f2);
        Func<int, int> f3 = x => x + 3 + s;
        s += f3(3);
        if (s == -3) Console.WriteLine(f3);
        Func<int, int> f4 = x => x + 4 + s;
        s += f4(4);
        if (s == -4) Console.WriteLine(f4);
        Func<int, int> f5 = x => x + 5 + s;
        s += f5(5);
        if (s == -5) Console.WriteLine(f5);
        Func<int, int> f6 = x => x + 6 + s;
        s += f6(6);
        if (s == -6) Console.WriteLine(f6);
        Func<int, int> f7 = x => x + 7 + s;
        s += f7(7);
        if (s == -7) Console.WriteLine(f7);
        Func<int, int> f8 = x => x + 8 + s;
        s += f8(8);
        if (s == -8) Console.WriteLine(f8);
        Func<int, int> f9 = x => x + 9 + s;
        s += f9(9);
        if (s == -9) Console.WriteLine(f9);
        Func<int, int> f10 = x => x + 10 + s;
        s += f10(10);
        if (s == -10) Console.WriteLine(f10);
        Func<int, int> f11 = x => x + 11 + s;
        s += f11(11);
        if (s == -11) Console.WriteLine(f11);
        Func<int, int> f12 = x => x + 12 + s;
        s += f12(12);
        if (s == -12) Console.WriteLine(f12);
        Func<int, int> f13 = x => x + 13 + s;
        s += f13(13);
        if (s == -13) Console.WriteLine(f13);
        Func<int, int> f14 = x => x + 14 + s;
        s += f14(14);
        if (s == -14) Console.WriteLine(f14);
        Func<int, int> f15 = x => x + 15 + s;
        s += f15(15);
        if (s == -15) Console.WriteLine(f15);
        Func<int, int> f16 = x => x + 16 + s;
        s += f16(16);
        if (s == -16) Console.WriteLine(f16);
        Func<int, int> f17 = x => x + 17 + s;
        s += f17(17);
        if (s == -17) Console.WriteLine(f17);
        Func<int, int> f18 = x => x + 18 + s;
        s += f18(18);
        if (s == -18) Console.WriteLine(f18);
        Func<int, int> f19 = x => x + 19 + s;
        s += f19(19);
        if (s == -19) Console.WriteLine(f19);
        Func<int, int> f20 = x => x + 20 + s;
        s += f20(20);
        if (s == -20) Console.WriteLine(f20);
        Func<int, int> f21 = x => x + 21 + s;
        s += f21(21);
        if (s == -21) Console.WriteLine(f21);
        Func<int, int> f22 = x => x + 22 + s;
        s += f22(22);
        if (s == -22) Console.WriteLine(f22);
        Func<int, int> f23 = x => x + 23 + s;
        s += f23(23);
        if (s == -23) Console.WriteLine(f23);
        Func<int, int> f24 = x => x + 24 + s;
        s += f24(24);
        if (s == -24) Console.WriteLine(f24);
        Func<int, int> f25 = x => x + 25 + s;
        s += f25(25);
        if (s == -25) Console.WriteLine(f25);
        Func<int, int> f26 = x => x + 26 + s;
        s += f26(26);
        if (s == -26) Console.WriteLine(f26);
        Func<int, int> f27 = x => x + 27 + s;
        s += f27(27);
        if (s == -27) Console.WriteLine(f27);
        Func<int, int> f28 = x => x + 28 + s;
        s += f28(28);
        if (s == -28) Console.WriteLine(f28);
        Func<int, int> f29 = x => x + 29 + s;
        s += f29(29);
        if (s == -29) Console.WriteLine(f29);
        Func<int, int> f30 = x => x + 30 + s;
        s += f30(30);
        if (s == -30) Console.WriteLine(f30);
        Func<int, int> f31 = x => x + 31 + s;
        s += f31(31);
        if (s == -31) Console.WriteLine(f31);
        Func<int, int> f32 = x => x + 32 + s;
        s += f32(32);
        if (s == -32) Console.WriteLine(f32);
        Func<int, int> f33 = x => x + 33 + s;
        s += f33(33);
        if (s == -33) Console.WriteLine(f33);
        Func<int, int> f34 = x => x + 34 + s;
        s += f34(34);
        if (s == -34) Console.WriteLine(f34);
        Func<int, int> f35 = x => x + 35 + s;
        s += f35(35);
        if (s == -35) Console.WriteLine(f35);
        Func<int, int> f36 = x => x + 36 + s;
        s += f36(36);
        if (s == -36) Console.WriteLine(f36);
        Func<int, int> f37 = x => x + 37 + s;
        s += f37(37);
        if (s == -37) Console.WriteLine(f37);
        Func<int, int> f38 = x => x + 38 + s;
        s += f38(38);
        if (s == -38) Console.WriteLine(f38);
        Func<int, int> f39 = x => x + 39 + s;
        s += f39(39);
        if (s == -39) Console.WriteLine(f39);
        Func<int, int> f40 = x => x + 40 + s;
        s += f40(40);
        if (s == -40) Console.WriteLine(f40);
        Func<int, int> f41 = x => x + 41 + s;
        s += f41(41);
        if (s == -41) Console.WriteLine(f41);
        Func<int, int> f42 = x => x + 42 + s;
        s += f42(42);
        if (s == -42) Console.WriteLine(f42);
        Func<int, int> f43 = x => x + 43 + s;
        s += f43(43);
        if (s == -43) Console.WriteLine(f43);
        Func<int, int> f44 = x => x + 44 + s;
        s += f44(44);
        if (s == -44) Console.WriteLine(f44);
        Func<int, int> f45 = x => x + 45 + s;
        s += f45(45);
        if (s == -45) Console.WriteLine(f45);
        Func<int, int> f46 = x => x + 46 + s;
        s += f46(46);
        if (s == -46) Console.WriteLine(f46);
        Func<int, int> f47 = x => x + 47 + s;
        s += f47(47);
        if (s == -47) Console.WriteLine(f47);
        Func<int, int> f48 = x => x + 48 + s;
        s += f48(48);
        if (s == -48) Console.WriteLine(f48);
        Func<int, int> f49 = x => x + 49 + s;
        s += f49(49);
        if (s == -49) Console.WriteLine(f49);
        Func<int, int> f50 = x => x + 50 + s;
        s += f50(50);
        if (s == -50) Console.WriteLine(f50);
        Func<int, int> f51 = x => x + 51 + s;
        s += f51(51);
        if (s == -51) Console.WriteLine(f51);
        Func<int, int> f52 = x => x + 52 + s;
        s += f52(52);
        if (s == -52) Console.WriteLine(f52);
        Func<int, int> f53 = x => x + 53 + s;
        s += f53(53);
        if (s == -53) Console.WriteLine(f53);
        Func<int, int> f54 = x => x + 54 + s;
        s += f54(54);
        if (s == -54) Console.WriteLine(f54);
        Func<int, int> f55 = x => x + 55 + s;
        s += f55(55);
        if (s == -55) Console.WriteLine(f55);
        Func<int, int> f56 = x => x + 56 + s;
        s += f56(56);
        if (s == -56) Console.WriteLine(f56);
        Func<int, int> f57 = x => x + 57 + s;
        s += f57(57);
        if (s == -57) Console.WriteLine(f57);
        Func<int, int> f58 = x => x + 58 + s;
        s += f58(58);
        if (s == -58) Console.WriteLine(f58);
        Func<int, int> f59 = x => x + 59 + s;
        s += f59(59);
        if (s == -59) Console.WriteLine(f59);
        Func<int, int> f60 = x => x + 60 + s;
        s += f60(60);
        if (s == -60) Console.WriteLine(f60);
        ж<ж<long>> box = new SlottedStandardBox<ж<long>>(NewKey());
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        GC.KeepAlive(s);
        return dies;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool SlotFilled()
    {
        ж<ж<long>> box = new StandardBox<ж<long>>(default(ж<long>)!);
        box.ValueSlot = NewKey();
        WeakReference weak = new(box.ValueSlot);
        box.ValueSlot = default!;
        bool dies = Dies(weak);
        GC.KeepAlive(box);
        return dies;
    }

    private static Type[] LocalTypes(string frame) =>
        typeof(BoxConstructorRvalueRootingTests).GetMethod(frame, BindingFlags.NonPublic | BindingFlags.Static)!
            .GetMethodBody()!.LocalVariables.Select(local => local.LocalType).ToArray();

    [TestMethod]
    [DataRow(nameof(StandardSmall))]
    [DataRow(nameof(SlottedSmall))]
    public void ConstructingFromAnRvalueSpillsNoLocalOfItsType(string frame)
    {
        // The frame's only ж<long> is the rvalue argument, so any local of that type is the compiler's spill of it.
        Type[] locals = LocalTypes(frame);

        Assert.IsFalse(locals.Contains(typeof(ж<long>)),
            $"{frame} declares [{string.Join(", ", locals.Select(type => type.Name))}]: constructing a box from an " +
            "rvalue spilled it into a local, so the constructor's parameter is taking an address (`in`)");
    }

    [TestMethod]
    [DataRow(nameof(StandardLarge))]
    [DataRow(nameof(SlottedLarge))]
    public void ABoxConstructedFromAnRvalueInALargeFrameDropsItOnceCleared(string frame)
    {
        if (!SlotFilled())
            Assert.Inconclusive("POSITIVE CONTROL: the slot-filled box's value did not die either, so this run cannot observe a collection");

        bool dies = (bool)typeof(BoxConstructorRvalueRootingTests)
            .GetMethod(frame, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;

        Assert.IsTrue(dies,
            $"{frame}: a box constructed from an rvalue kept it alive after the box was cleared, at default inlining in " +
            "a large frame: the constructor must take its value without an address");
    }
}
