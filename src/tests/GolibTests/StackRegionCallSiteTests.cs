// StackRegionCallSiteTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace go
{
    internal class stackregionHeld
    {
        public nint n;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual nint virt(nint a) => a;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public nint plain(nint a) => a;
    }

    // Shapes whose local the optimizing C# compiler keeps on the evaluation stack (`dup` where a local would be
    // `ldloc`), the shape of the behavioral CallerLineStackLocal: no statement boundary is stack-empty, so the JIT
    // maps only its own boundaries (stack-empty points and the instruction after a void call), and a frame suspended
    // in `probe` reports the instruction after the previous VOID call, a statement before the call.
    internal static class stackregion_package
    {
        internal static stackregionHeld other = new();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void bare(stackregionHeld h) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static nint value(stackregionHeld h, nint a) => a;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static nint probe() => 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void record(nint[] got, int index, nint line) => got[index] = line;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void record2(nint[] got, nint a, nint b) => got[0] = a + b;

        // The one call of `probe` before the next void call: settled.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void settles(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint r = value(h, 2);
            record(got, 0, probe() + r - r);
            bare(h);
        }

        // Two calls of `probe` in one region: the offset cannot tell them apart.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ambiguous(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint s = value(h, 3);
            record2(got, probe() + s - s, probe());
            bare(h);
        }

        // Today's call is a virtual call, which may BE the callee: not provably wrong.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void namedIndirect(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint v = h.virt(4);
            record(got, 1, probe() + v - v);
            bare(h);
        }

        // A virtual call among the region's calls: refused.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void regionIndirect(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint w = value(h, 5);
            record(got, 2, probe() + other.virt(w) - w);
            bare(h);
        }

        // Today's call is a callvirt of a NON-virtual method (the C# compiler's null check): its target is fixed, but it
        // is not a direct `call`, so today's reading is not provably wrong.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void namedCallvirt(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint p = h.plain(6);
            record(got, 3, probe() + p - p);
            bare(h);
        }

        // Today's call is a newobj: not a direct `call` either.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void namedNewobj(nint[] got)
        {
            var h = new stackregionHeld();
            bare(h);
            nint q = value(h, new stackregionHeld().n + 7);
            record(got, 4, probe() + q - q);
            bare(h);
        }

        // CallerLineMultiLine's pkgSlice: ONE statement whose elements are stored into an inline array one by one. Each
        // store leaves the stack empty, so each element is a region of its own though the statement holds every call --
        // the statement settle is AMBIGUOUS there, and the region still names one call.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void sliceElements(nint[] got)
        {
            ReadOnlySpan<nint> s = [probe(), probe(), probe()];
            got[0] = s[0] + s[1] + s[2];
        }
    }
}

namespace GolibTests
{
    /// <summary>
    /// The JIT's boundaries and the region they bound (runtime's ilBoundaryOffsets and calleeSettledCallSite). Where the
    /// optimizing C# compiler keeps a local on the evaluation stack, no statement boundary is stack-empty, and tier-0's
    /// IL-to-native map holds only its own boundaries: stack-empty points and the instruction after a VOID call
    /// (measured 2026-10-31, windows, a plain C# probe: a popped value call left no mapping, a void call did, and the
    /// frame of the next call reported the instruction after the void call). So the reported offset can sit a whole
    /// statement before the suspended call, and the statement settle finds nothing. The region from the reported offset
    /// to the next boundary holds the suspended call; when it holds exactly ONE call of the callee, and today's reading
    /// names a direct call of another method, that call it is. Zero or two, or any indirect call there, keep today's
    /// reading. The end-to-end guard is the behavioral CallerLineStackLocal; this pins the rule from the IL.
    /// </summary>
    [TestClass]
    public class StackRegionCallSiteTests
    {
        private static readonly Type s_runtime = typeof(runtime_package);

        private static T Invoke<T>(string name, params object[] args)
        {
            MethodInfo method = s_runtime.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new AssertFailedException($"runtime_package.{name} is missing");

            return (T)method.Invoke(null, args)!;
        }

        private static MethodInfo Fixture(string name) =>
            typeof(stackregion_package).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

        private static int[] Boundaries(MethodBase method) =>
            Invoke<int[]?>("ilBoundaryOffsets", method) ?? throw new AssertFailedException($"ilBoundaryOffsets read no boundaries for {method.Name}");

        private static bool JitOptimizerDisabled =>
            typeof(stackregion_package).Assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true;

        // ---- The boundary source: an IL stack-depth pass, on IL emitted exactly (no C# compiler in between). ----

        public static void Void() { }

        public static int Value() => 1;

        public static void Consume(int value) { }

        public static int Thrower(int value) => throw new InvalidOperationException();

        private static MethodInfo Emit(string name, Type returnType, Type[] parameters, Action<ILGenerator> body)
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("stackregion_" + name), AssemblyBuilderAccess.Run);
            TypeBuilder type = assembly.DefineDynamicModule("stackregion").DefineType("plants", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            MethodBuilder method = type.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Static, returnType, parameters);
            body(method.GetILGenerator());
            return type.CreateType().GetMethod(name)!;
        }

        private static MethodInfo Plant(string name) => typeof(StackRegionCallSiteTests).GetMethod(name)!;

        [TestMethod]
        public void AVoidCallIsABoundaryAndAPoppedValueCallIsNot()
        {
            // 0 ldc.i4.1 / 1 call Void / 6 call Value / 11 pop / 12 call Consume / 17 ret
            MethodInfo method = Emit("voidVersusPopped", typeof(void), Type.EmptyTypes, il =>
            {
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Call, Plant(nameof(Void)));
                il.Emit(OpCodes.Call, Plant(nameof(Value)));
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Call, Plant(nameof(Consume)));
                il.Emit(OpCodes.Ret);
            });

            int[] boundaries = Boundaries(method);

            CollectionAssert.Contains(boundaries, 0, "the entry is stack-empty");
            CollectionAssert.Contains(boundaries, 6, "the instruction after a VOID call is a boundary, with the stack held (depth 1)");
            CollectionAssert.DoesNotContain(boundaries, 11, "the instruction after a value call is not, its value on the stack");
            CollectionAssert.DoesNotContain(boundaries, 12, "nor is the one after the value is popped: the held value keeps the stack at depth 1");
            CollectionAssert.Contains(boundaries, 17, "after Consume the stack is empty");
            CollectionAssert.DoesNotContain(boundaries, 1, "a value pushed (depth 1) is no boundary");
        }

        [TestMethod]
        public void ABranchTargetTakesTheDepthItIsReachedWith()
        {
            // 0 ldc.i4.1 / 1 ldarg.0 / 2 brtrue.s L1 / 4 ldc.i4.2 / 5 br.s L2 / L1=7 ldc.i4.3 / L2=8 add / 9 pop / 10 ret
            MethodInfo method = Emit("branchJoin", typeof(void), [typeof(bool)], il =>
            {
                Label l1 = il.DefineLabel(), l2 = il.DefineLabel();
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, l1);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Br_S, l2);
                il.MarkLabel(l1);
                il.Emit(OpCodes.Ldc_I4_3);
                il.MarkLabel(l2);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
            });

            int[] boundaries = Boundaries(method);

            CollectionAssert.DoesNotContain(boundaries, 7,
                "the instruction after an unconditional branch takes the depth its forward branch reaches it with (1), not 0");
            CollectionAssert.DoesNotContain(boundaries, 8, "the join is reached at depth 2 from both edges");
            CollectionAssert.Contains(boundaries, 10, "after the pop the stack is empty");
        }

        [TestMethod]
        public void ACatchHandlerIsEnteredWithTheExceptionOnTheStack()
        {
            int handler = -1, after = -1, end = -1;
            MethodInfo method = Emit("handlerEntry", typeof(void), [typeof(int)], il =>
            {
                Label done = il.DefineLabel();
                il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, Plant(nameof(Thrower)));
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Leave, done);
                il.BeginCatchBlock(typeof(InvalidOperationException));
                handler = il.ILOffset;
                il.Emit(OpCodes.Pop);
                after = il.ILOffset;
                il.Emit(OpCodes.Leave, done);
                il.EndExceptionBlock();
                il.MarkLabel(done);
                end = il.ILOffset;
                il.Emit(OpCodes.Ret);
            });

            // The emitter may add its own leave at the end of the catch; read the handler's offsets from the body.
            ExceptionHandlingClause clause = method.GetMethodBody()!.ExceptionHandlingClauses.Single();
            Assert.AreEqual(handler, clause.HandlerOffset, "the handler starts where the fixture marked it");

            int[] boundaries = Boundaries(method);

            CollectionAssert.DoesNotContain(boundaries, handler, "a catch handler is entered at depth 1, the exception object");
            CollectionAssert.Contains(boundaries, after, "after the exception is popped the stack is empty");
            CollectionAssert.Contains(boundaries, end, "a leave empties the stack: its target is a boundary");
        }

        [TestMethod]
        public void ACalliReadsNoBoundaries()
        {
            MethodInfo method = Emit("indirect", typeof(void), Type.EmptyTypes, il =>
            {
                il.Emit(OpCodes.Ldftn, Plant(nameof(Void)));
                il.EmitCalli(OpCodes.Calli, System.Runtime.InteropServices.CallingConvention.StdCall, typeof(void), Type.EmptyTypes);
                il.Emit(OpCodes.Ret);
            });

            Assert.IsNull(Invoke<int[]?>("ilBoundaryOffsets", method),
                "a calli's stack effect is not read from a method token, so the pass reads nothing rather than a guess");
        }

        // ---- The region rule, on the C# compiler's own stack-scheduled shapes. ----

        // The fixture's region start: the instruction after the first call of `bare`, which is where the tier-0 map
        // reports a frame suspended in the next statements' calls. Asserts the shape the rule is about, so a compiler
        // that stops stack-scheduling the local fails here rather than passing vacuously.
        private static (MethodInfo shape, int regionStart, int[] calls) Shape(string name)
        {
            if (JitOptimizerDisabled)
                Assert.Inconclusive("the shape is the optimizing C# compiler's (a local kept on the evaluation stack); this is a JIT-optimizer-disabled build. Run `dotnet test -c Release`.");

            MethodInfo shape = Fixture(name);
            MethodInfo bare = Fixture("bare");
            int[] calls = Invoke<int[]>("callSiteOffsets", shape);
            int bareCall = calls.First(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.MetadataToken == bare.MetadataToken);
            byte[] il = shape.GetMethodBody()!.GetILAsByteArray()!;

            Assert.AreEqual((byte)0x25, il[bareCall + 5], $"{name}: the local is held on the stack (`dup` after the first `bare`), the fixture's premise");
            return (shape, bareCall + 5, calls);
        }

        private static int CallOf(MethodInfo shape, int[] calls, string target) =>
            calls.Single(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.Name == target);

        [TestMethod]
        public void TheOneCallOfTheCalleeBeforeTheNextVoidCallIsTheSuspendedCall()
        {
            (MethodInfo shape, int start, int[] calls) = Shape("settles");
            MethodInfo probe = Fixture("probe");

            // Today's reading names `value`, a direct call of another method, and the statement settle finds nothing.
            Assert.AreEqual(CallOf(shape, calls, "value"), Invoke<int>("returnSiteILOffset", shape, start), "the reported offset reads as the `value` call");

            Assert.AreEqual(CallOf(shape, calls, "probe"), Invoke<int>("calleeSettledCallSite", shape, start, probe),
                "the region from the reported offset to the next boundary (after `record`, a void call) holds one call of `probe`: that call");
        }

        [TestMethod]
        public void TwoCallsOfTheCalleeInTheRegionSettleNothing()
        {
            (MethodInfo shape, int start, _) = Shape("ambiguous");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, start, Fixture("probe")),
                "two calls of `probe` lie between the reported offset and the next boundary: the offset cannot tell them apart");
        }

        [TestMethod]
        public void AVirtualCallTodayIsNotProvablyWrong()
        {
            (MethodInfo shape, int start, _) = Shape("namedIndirect");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, start, Fixture("probe")),
                "today's reading names a virtual call, which may be the callee's frame: kept");
        }

        [TestMethod]
        public void AVirtualCallInTheRegionRefusesIt()
        {
            (MethodInfo shape, int start, _) = Shape("regionIndirect");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, start, Fixture("probe")),
                "a virtual call among the region's calls could hide the callee: nothing is settled");
        }

        [TestMethod]
        public void ACallvirtNamedTodayIsNotADirectCall()
        {
            (MethodInfo shape, int start, int[] calls) = Shape("namedCallvirt");

            Assert.AreEqual(CallOf(shape, calls, "plain"), Invoke<int>("returnSiteILOffset", shape, start), "the reported offset reads as the `plain` callvirt");
            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, start, Fixture("probe")),
                "today's reading names a callvirt, not a direct `call`: kept, though the region holds one call of `probe`");
        }

        [TestMethod]
        public void ANewobjNamedTodayIsNotADirectCall()
        {
            (MethodInfo shape, int start, int[] calls) = Shape("namedNewobj");

            Assert.AreEqual(".ctor", Invoke<MethodBase?>("callTarget", shape, Invoke<int>("returnSiteILOffset", shape, start))?.Name, "the reported offset reads as the newobj");
            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, start, Fixture("probe")),
                "today's reading names a newobj, not a direct `call`: kept, though the region holds one call of `probe`");
        }

        [TestMethod]
        public void PkgSliceElementsSettleInTheirOwnRegions()
        {
            if (JitOptimizerDisabled)
                Assert.Inconclusive("the shape is the optimizing C# compiler's; this is a JIT-optimizer-disabled build. Run `dotnet test -c Release`.");

            MethodInfo shape = Fixture("sliceElements"), probe = Fixture("probe");
            int[] calls = Invoke<int[]>("callSiteOffsets", shape);
            int[] statements = Invoke<int[]>("sequencePointOffsets", shape);
            int[] probes = calls.Where(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.MetadataToken == probe.MetadataToken).ToArray();
            byte[] il = shape.GetMethodBody()!.GetILAsByteArray()!;

            Assert.AreEqual(3, probes.Length, "the fixture's slice holds three calls of `probe`");

            // The second element's region starts after the first element's store, where the stack is empty: the `stind.i`
            // right after the first element's `call probe`.
            Assert.AreEqual((byte)0xDF, il[probes[0] + 5], "each element is stored (stind.i) right after its call, the premise");
            int start = probes[0] + 6;

            Assert.IsFalse(statements.Any(point => point > probes[0] && point <= probes[2]), "the three elements are ONE statement, the premise");
            Assert.AreNotEqual(probe.Name, Invoke<MethodBase?>("callTarget", shape, Invoke<int>("returnSiteILOffset", shape, start))?.Name,
                "today's reading names the element's store helper, not `probe`");

            Assert.AreEqual(probes[1], Invoke<int>("calleeSettledCallSite", shape, start, probe),
                "the statement holds two calls of `probe` after the offset (ambiguous), and the region up to the next stack-empty point holds one: that call");
        }
    }
}
