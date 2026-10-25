// TailCallFrameGuardTests.cs - Gbtc
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

namespace GolibTests;

/// <summary>
/// Guards the mechanism that keeps a Go caller's frame against the CLR's implicit tail calls: a method carrying golib's
/// <c>[System.Security.DynamicSecurityMethod]</c> is never implicitly tail-called, so the method that called it stays on
/// the stack for <c>runtime.Caller</c>, <c>t.Helper</c> and log's <c>Lshortfile</c> to see.
/// </summary>
/// <remarks>
/// <para>
/// Go never tail-calls. The optimizing JIT does, opportunistically, and on linux/amd64 a closure such as
/// <c>func() { log.Fatalf("...") }</c> lost its frame to one (slog <c>TestCallDepthConnection</c>, 2026-10-25). The fix rests on
/// two behaviors that are NOT documented contracts: the C# compiler writes <c>MethodAttributes.RequireSecObject</c> for an
/// attribute named <c>System.Security.DynamicSecurityMethodAttribute</c>, and the VM's <c>canTailCall</c> refuses an implicit
/// tail call into a method with that flag. Each half has its own test here, so the .NET upgrade that drops either one goes
/// red here instead of silently moving every caller line (COORD's condition on the tail-call seat).
/// </para>
/// <para>
/// The behavioral pair needs OPTIMIZED code, which a Debug (JIT-optimizer-disabled) build never runs: there the control
/// keeps its frame too and the pair measures nothing, so both report INCONCLUSIVE at Debug (the AliasOverlapRaceTests
/// precedent). Run <c>dotnet test -c Release</c> for the live guard. The metadata test runs in every configuration.
/// </para>
/// </remarks>
[TestClass]
public class TailCallFrameGuardTests
{
    private static void RequireOptimizedCode()
    {
        if (typeof(TailCallFrameGuardTests).Assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled == true)
        {
            Assert.Inconclusive("GolibTests is a JIT-optimizer-disabled (Debug) build: it makes no tail calls, so the unmarked " +
                "control keeps its frame and this guard measures nothing; run dotnet test -c Release for the live guard");
        }
    }

    // The frame that called the probe: frame 0 is CallerName, frame 1 the probe, frame 2 its caller.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string CallerName() => new StackTrace(2, false).GetFrame(0)?.GetMethod()?.Name ?? "<none>";

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Unmarked(int a, int b) => CallerName();

    [MethodImpl(MethodImplOptions.NoInlining), global::System.Security.DynamicSecurityMethod]
    private static string Marked(int a, int b) => CallerName();

    // Each lambda's whole body is one call in tail position, the shape of `func() { log.Fatalf(...) }`. The JIT compiles
    // an AggressiveOptimization method optimized at its first call, with no tiering in between.
    private static readonly Func<string> s_callsUnmarked = [MethodImpl(MethodImplOptions.AggressiveOptimization)] () => Unmarked(1, 2);
    private static readonly Func<string> s_callsMarked = [MethodImpl(MethodImplOptions.AggressiveOptimization)] () => Marked(1, 2);

    private static string LambdaName(Func<string> lambda) => lambda.Method.Name;

    [TestMethod]
    public void UnmarkedCalleeLosesItsCallersFrame_TheControl()
    {
        RequireOptimizedCode();

        // If this ever fails, the JIT no longer tail-calls this shape, and the marked half below proves nothing: re-derive
        // the probe (a different argument shape) before trusting the guard again.
        Assert.AreNotEqual(LambdaName(s_callsUnmarked), s_callsUnmarked(),
            "the optimized lambda's tail call into an UNMARKED method kept the lambda's frame: the control no longer sees the " +
            "tail call this guard exists to stop, so the guard is vacuous until the probe is re-derived");
    }

    [TestMethod]
    public void MarkedCalleeKeepsItsCallersFrame()
    {
        RequireOptimizedCode();

        Assert.AreEqual(LambdaName(s_callsMarked), s_callsMarked(),
            "the optimized lambda's tail call into a [DynamicSecurityMethod] method ERASED the lambda's frame: this .NET no " +
            "longer honors RequireSecObject in canTailCall, and every converted caller line that rests on it can move " +
            "(golib's DynamicSecurityMethodAttribute)");
    }

    [TestMethod]
    public void FrameApisCarryRequireSecObject()
    {
        // The compiler half: the flag on the methods that carry the attribute, written three ways -- by hand in golib's
        // own probe above, by hand on runtime's Callers and testing's reporting members, and by go2cs-gen on a converted
        // no-inline carrier (runtime.Caller's generated declaring part).
        static bool Flagged(MethodInfo method) => method is not null && (method.Attributes & MethodAttributes.RequireSecObject) != 0;

        Assert.IsTrue(Flagged(typeof(TailCallFrameGuardTests).GetMethod(nameof(Marked), BindingFlags.NonPublic | BindingFlags.Static)),
            "the C# compiler no longer writes RequireSecObject for [System.Security.DynamicSecurityMethod]");
        Assert.IsFalse(Flagged(typeof(TailCallFrameGuardTests).GetMethod(nameof(Unmarked), BindingFlags.NonPublic | BindingFlags.Static)),
            "an unmarked method carries RequireSecObject: the flag's control is broken");

        MethodInfo[] runtimeMethods = typeof(runtime_package).GetMethods(BindingFlags.Public | BindingFlags.Static);

        Assert.IsTrue(Flagged(runtimeMethods.SingleOrDefault(m => m.Name == "Callers")), "runtime.Callers (hand-owned) lost its mark");
        Assert.IsTrue(Flagged(runtimeMethods.SingleOrDefault(m => m.Name == "Caller")), "runtime.Caller (a go2cs-gen no-inline carrier) lost its mark");

        // T's two Errorf overloads (`ref T` and `ж<T>`) report a caller line; B's and F's are detached no-ops and unmarked.
        static string ReceiverName(MethodInfo method)
        {
            Type receiver = method.GetParameters()[0].ParameterType;
            receiver = receiver.IsByRef ? receiver.GetElementType()! : receiver;
            return receiver.IsGenericType ? receiver.GetGenericArguments()[0].Name : receiver.Name;
        }

        MethodInfo[] errorf = typeof(testing_package).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Errorf" && ReceiverName(m) == "T").ToArray();

        Assert.AreEqual(2, errorf.Length, "testing's T.Errorf overloads were not both found: the census below reads nothing");
        Assert.IsTrue(errorf.All(Flagged), "testing's T.Errorf (hand-owned) lost its mark");
    }
}
