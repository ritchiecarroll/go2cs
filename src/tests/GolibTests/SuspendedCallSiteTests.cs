// SuspendedCallSiteTests.cs - Gbtc
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
    internal sealed class calleesiteChain;

    // A method chain in one statement, the shape of CallerLineMultiLine's S1: every call after the first keeps
    // the previous result on the evaluation stack, so the optimized JIT has no mapping between them.
    internal static class calleesite_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static calleesiteChain add(this calleesiteChain c) => c;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void done(this calleesiteChain c) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void unrelated() { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void shape(calleesiteChain c)
        {
            c.add()
             .add()
             .done();
        }
    }
}

namespace GolibTests
{
    /// <summary>
    /// The optimized JIT reports a frame suspended inside a method chain at the chain's FIRST offset, for every
    /// call in it (measured on CallerLineMultiLine's S1: Main reported IL 0xB1 for the call of Done at 0xD7), so
    /// "the first call after the reported offset" named the wrong call and the line fell back to the statement's.
    /// runtime's suspendedCallSite settles it from the callee, the raw frame inside: the one call of the callee
    /// in the statement at or after the reported offset. It never guesses: a callee called more than once there,
    /// or not at all, leaves the CLR's reading. The end-to-end guard is the behavioral CallerLineMultiLine in
    /// Release (S1-terminal 89 where Go has 92 before the fix, on two boxes); this pins the rule itself from the
    /// IL, which no JIT setting changes.
    /// </summary>
    [TestClass]
    public class SuspendedCallSiteTests
    {
        private static readonly Type s_runtime = typeof(runtime_package);

        private static T Invoke<T>(string name, params object[] args)
        {
            MethodInfo method = s_runtime.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new AssertFailedException($"runtime_package.{name} is missing");

            return (T)method.Invoke(null, args)!;
        }

        private static MethodInfo Method(string name) =>
            typeof(calleesite_package).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

        [TestMethod]
        public void TheChainStartResolvesToTheOneCallOfTheCallee()
        {
            MethodInfo shape = Method("shape"), add = Method("add"), done = Method("done");

            int[] calls = Invoke<int[]>("callSiteOffsets", shape);
            int[] statements = Invoke<int[]>("sequencePointOffsets", shape);
            int[] adds = calls.Where(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.MetadataToken == add.MetadataToken).ToArray();
            int doneCall = calls.Single(call => Invoke<MethodBase?>("callTarget", shape, call) is { } t && t.MetadataToken == done.MetadataToken);

            Assert.AreEqual(2, adds.Length, "the fixture's chain calls add twice");

            // The offset the optimized JIT reports for every call in the chain: the chain statement's start.
            int chainStart = statements.Where(point => point <= adds[0]).Max();

            Assert.AreEqual(doneCall, Invoke<int>("suspendedCallSite", shape, chainStart, done),
                "a frame suspended calling `done` (called once in the statement) must resolve to that call");

            Assert.AreEqual(-1, Invoke<int>("suspendedCallSite", shape, chainStart, add),
                "`add` is called twice after the chain's start: the offset cannot tell them apart, so nothing is settled");

            Assert.AreEqual(-1, Invoke<int>("suspendedCallSite", shape, chainStart, Method("unrelated")),
                "a callee no call in the statement targets (one the JIT inlined away) settles nothing");

            // The reported offset IS a call of the callee: that call, as suspendedAtReportedCall answered before.
            Assert.AreEqual(doneCall, Invoke<int>("suspendedCallSite", shape, doneCall, done));
        }
    }
}
