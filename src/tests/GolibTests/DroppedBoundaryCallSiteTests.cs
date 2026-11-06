// DroppedBoundaryCallSiteTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace go
{
    // The shape of testing's testHelper (helperfuncs_test.go:84-87): a parameter captured by a closure, then a statement
    // that reads it from the display class and passes a hoisted literal through its `ref readonly` accessor, the form the
    // converter emits for a hoisted literal (`genericFloat64ˢ`). The optimized JIT keeps no IL boundary for such a statement,
    // so a frame suspended in its call reports the previous statement's offset.
    internal static class droppedboundary_package
    {
        private static ref readonly string firstˢ => ref firstˢᵇ;
        private static readonly string firstˢᵇ = "first";
        private static ref readonly string secondˢ => ref secondˢᵇ;
        private static readonly string secondˢᵇ = "second";

        internal static readonly Action<object, string> viaDelegate = helper;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void previous(object t, string s) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void helper(object t, string s) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void genericHelper<G>(object t, string s) { }

        // The one call of `helper` in the statement after `previous`: settled.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void settles(object t)
        {
            Action fn = () => previous(t, "closure");
            fn();
            previous(t, "previous");
            helper(t, firstˢ);
        }

        // Two calls of `helper` in the widened region: the offset cannot tell them apart.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void twoCalls(object t)
        {
            Action fn = () => previous(t, "closure");
            fn();
            previous(t, "previous");
            helper(t, firstˢ);
            helper(t, secondˢ);
        }

        // A direct call of `helper` and a delegate Invoke whose target is `helper`: the Invoke may be the suspended call.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void directAndInvoke(object t)
        {
            Action fn = () => previous(t, "closure");
            fn();
            previous(t, "previous");
            helper(t, firstˢ);
            viaDelegate(t, secondˢ);
        }

        // Two delegate Invokes whose target is `helper`, and no direct call: never settled by an Invoke.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void twoInvokes(object t)
        {
            Action fn = () => previous(t, "closure");
            fn();
            previous(t, "previous");
            viaDelegate(t, firstˢ);
            viaDelegate(t, secondˢ);
        }

        // A generic callee: a frame names it by its open definition, so its instantiations' calls are one observation.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void generic(object t)
        {
            Action fn = () => previous(t, "closure");
            fn();
            previous(t, "previous");
            genericHelper<double>(t, firstˢ);
        }
    }
}

namespace GolibTests
{
    /// <summary>
    /// The dropped boundary (runtime's droppedBoundarySettledCallSite). The optimized JIT keeps no IL boundary for a
    /// statement that reads a closure-captured variable and then calls a hoisted-literal accessor, so a frame suspended in
    /// that statement's call reports the previous statement's offset (measured 2026-11-05, linux, Release, tiering off:
    /// testing's TestTBHelper and TestArtifactDirSpecified). The region the JIT's own boundaries bound then ends before the
    /// call; it is widened across each following statement whose first call is such an accessor, and settles the ONLY
    /// call of a non-generic callee there. Two calls, a delegate Invoke, or a generic callee keep today's reading. The
    /// end-to-end reading is testing's TestArtifactDirSpecified frame; this pins the rule from the IL.
    /// </summary>
    [TestClass]
    public class DroppedBoundaryCallSiteTests
    {
        private static readonly Type s_runtime = typeof(runtime_package);

        private static T Invoke<T>(string name, params object[] args)
        {
            MethodInfo method = s_runtime.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new AssertFailedException($"runtime_package.{name} is missing");

            return (T)method.Invoke(null, args)!;
        }

        private static MethodInfo Fixture(string name) =>
            typeof(droppedboundary_package).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

        private static bool JitOptimizerDisabled =>
            typeof(droppedboundary_package).Assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true;

        // The fixture's reported offset: the start of the `previous` statement, where the optimized JIT reports a frame
        // suspended in the next statements' calls. Asserts the shape the rule is about, so a compiler that stops emitting
        // it fails here rather than passing vacuously.
        private static (MethodInfo shape, int reported, int[] calls) Shape(string name)
        {
            if (JitOptimizerDisabled)
                Assert.Inconclusive("the shape is the optimizing C# compiler's; this is a JIT-optimizer-disabled build. Run `dotnet test -c Release`.");

            MethodInfo shape = Fixture(name);
            int[] calls = Invoke<int[]>("callSiteOffsets", shape);
            int[] statements = Invoke<int[]>("sequencePointOffsets", shape);
            int previousCall = CallsOf(shape, calls, "previous").Single();
            int reported = statements.Last(point => point < previousCall);
            int next = statements.First(point => point > previousCall);

            Assert.AreEqual("get_firstˢ", Invoke<MethodBase?>("callTarget", shape, calls.First(call => call > next))?.Name,
                $"{name}: the statement after `previous` starts with the hoisted literal's accessor, the fixture's premise");
            Assert.AreEqual(previousCall, Invoke<int>("returnSiteILOffset", shape, reported), $"{name}: today's reading names the `previous` call");
            return (shape, reported, calls);
        }

        private static int[] CallsOf(MethodInfo shape, int[] calls, string target) =>
            calls.Where(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.Name == target).ToArray();

        [TestMethod]
        public void TheOneCallOfTheCalleeAfterADroppedBoundaryIsTheSuspendedCall()
        {
            (MethodInfo shape, int reported, int[] calls) = Shape("settles");

            Assert.AreEqual(CallsOf(shape, calls, "helper").Single(), Invoke<int>("calleeSettledCallSite", shape, reported, Fixture("helper")),
                "the region widened over the accessor statement holds one call of `helper`: that call");
        }

        [TestMethod]
        public void TwoCallsOfTheCalleeAfterADroppedBoundarySettleNothing()
        {
            (MethodInfo shape, int reported, _) = Shape("twoCalls");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, reported, Fixture("helper")),
                "two calls of `helper` lie in the widened region: the offset cannot tell them apart");
        }

        [TestMethod]
        public void ADelegateInvokeAfterADroppedBoundaryRefusesIt()
        {
            (MethodInfo shape, int reported, _) = Shape("directAndInvoke");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, reported, Fixture("helper")),
                "a delegate Invoke whose target is `helper` sits beside the direct call: either may be the suspended call");
        }

        [TestMethod]
        public void TwoMatchingInvokesAfterADroppedBoundarySettleNothing()
        {
            (MethodInfo shape, int reported, _) = Shape("twoInvokes");

            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, reported, Fixture("helper")),
                "two Invokes of a delegate to `helper` and no direct call: an Invoke never settles the frame");
        }

        [TestMethod]
        public void AGenericCalleeAfterADroppedBoundaryIsRefused()
        {
            (MethodInfo shape, int reported, _) = Shape("generic");
            MethodInfo definition = Fixture("genericHelper");

            Assert.IsTrue(definition.ContainsGenericParameters, "a frame names a generic callee by its open definition, the case pinned");
            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", shape, reported, definition),
                "an open callee matches every instantiation's call, so one call of it is not one observation");
        }
    }
}
