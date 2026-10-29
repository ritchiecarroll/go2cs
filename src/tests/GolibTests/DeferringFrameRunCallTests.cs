// DeferringFrameRunCallTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace go
{
    // The converter's deferring-function shape: statements, a defer, and the one `finally { ᒐ.Run(); }` that
    // runs the deferred calls as the function returns (Go's deferreturn).
    internal static class deferringrun_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void work(nint x) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void deferred() { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void deferring()
        {
            GoFrame ᒐ = default;

            try
            {
                work(1);
                builtin.defer(deferred, ref ᒐ);
                work(2);
            }
            finally { ᒐ.Run(); }
        }

        // Two Run calls: an offset cannot say which one a frame is suspended in.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void twoRuns()
        {
            GoFrame ᒐ = default;

            try
            {
                work(1);
                ᒐ.Run();
            }
            finally { ᒐ.Run(); }
        }
    }
}

namespace GolibTests
{
    /// <summary>
    /// A frame suspended in its finally's <c>GoFrame.Run</c> -- a deferred call's caller -- is not always
    /// reported at that call: the JIT can report IL offset 0 for it (measured 2026-10-29 on linux, Release: at
    /// TieredCompilation=0 for a method-group defer, `defer(where, s, ref ᒐ)`, and under tiering for every
    /// deferring method of the behavioral LineMarkerDeferEndTC0). "The first call after" offset 0 is the
    /// method's first call, so runtime.Caller named the function's first statement where Go names its
    /// deferreturn line. The frame's callee settles it: a method holds exactly ONE call of Run, so a frame
    /// suspended calling Run is at that call (runtime's calleeSettledCallSite). Two Run calls settle nothing.
    /// The end-to-end guard is LineMarkerDeferEndTC0; this pins the rule from the IL, which no JIT setting
    /// changes.
    /// </summary>
    [TestClass]
    public class DeferringFrameRunCallTests
    {
        private static readonly Type s_runtime = typeof(runtime_package);
        private static readonly MethodInfo s_run = typeof(GoFrame).GetMethod(nameof(GoFrame.Run))!;

        private static T Invoke<T>(string name, params object[] args)
        {
            MethodInfo method = s_runtime.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new AssertFailedException($"runtime_package.{name} is missing");

            return (T)method.Invoke(null, args)!;
        }

        private static MethodInfo Method(string name) =>
            typeof(deferringrun_package).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

        private static int[] RunCalls(MethodInfo method) =>
            Invoke<int[]>("callSiteOffsets", method)
                .Where(call => Invoke<MethodBase?>("callTarget", method, call) is { } target && target.MetadataToken == s_run.MetadataToken)
                .ToArray();

        [TestMethod]
        public void AFrameSuspendedInRunResolvesToTheOneRunCall()
        {
            MethodInfo deferring = Method("deferring");
            int runCall = RunCalls(deferring).Single();

            // The reading the JIT's offset 0 gets unsettled: the method's first call, `work(1)`.
            Assert.AreNotEqual(runCall, Invoke<int>("returnSiteILOffset", deferring, 0), "offset 0 alone names the first call");

            // CAPTURE keeps the callee, since a call of Run follows offset 0.
            Assert.IsTrue(Invoke<bool>("callsCalleeAfter", deferring, 0, s_run));

            // RESOLVE settles the call from anywhere in the method, the Run call's own offset aside (that one is
            // settled at capture, as suspendedAtReportedCall).
            Assert.AreEqual(runCall, Invoke<int>("calleeSettledCallSite", deferring, 0, s_run),
                "a frame suspended calling Run, reported at offset 0, must resolve to the method's one Run call");

            Assert.IsTrue(Invoke<bool>("suspendedAtReportedCall", deferring, runCall, s_run));
        }

        [TestMethod]
        public void TwoRunCallsSettleNothing()
        {
            MethodInfo twoRuns = Method("twoRuns");

            Assert.AreEqual(2, RunCalls(twoRuns).Length, "the fixture calls Run twice");
            Assert.AreEqual(-1, Invoke<int>("calleeSettledCallSite", twoRuns, 0, s_run),
                "two Run calls cannot be told apart from offset 0, so nothing is settled");
        }
    }
}
