// ReadOnlyStringDataTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// unsafe.StringData over STATIC string bytes (a cached string literal, a one-byte string from @string's table) hands
// out a pointer into storage the whole process shares: one backing per literal, one per byte value. By default a write
// through it changes every later equal literal and every later equal one-byte string, in every package. Go keeps those
// bytes in read-only memory, where the same write faults and nothing else ever sees the byte.
//
// GO2CS_CHECK_STRING_WRITES=1 is the opt-in diagnostic: StringData over static bytes then points into ONE private copy
// per backing (ReadOnlyStringBacking), so the write reaches nothing else, and the NEXT access through a hook panics by
// name: a dereference of the box, or a later StringData over the same backing. A HEAP string is the same either way:
// Go mutates its sharers and so does go2cs.
//
// The switch is read once per process, so the rows that need it ON run in a child: this assembly again under
// `dotnet test`, filtered to ReadOnlyStringDataSwitchOnRows, with the switch and GO2CS_D5_CHILD set. Outside such a
// child those rows return at once, as Go's helper processes do. The rows here run in this process, switch OFF.
[TestClass]
public class ReadOnlyStringDataTests
{
    internal const string ChildVariable = "GO2CS_D5_CHILD";
    private const string SwitchVariable = "GO2CS_CHECK_STRING_WRITES";

    // The rows of ReadOnlyStringDataSwitchOnRows. The parent fails unless the child ran exactly this many.
    private const int SwitchOnRowCount = 7;

    [TestMethod]
    public void WithTheSwitchOffAWriteThroughALiteralReachesEveryEqualLiteral()
    {
        if (ReadOnlyStringBacking.Enabled)
            Assert.Inconclusive($"{SwitchVariable}=1 is set in this process; the default is measured with it unset");

        @string lit = "d5-default-arm";
        ж<byte> p = @unsafe.StringData(lit);

        try
        {
            p.Value = (byte)'D';

            @string again = "d5-default-arm";
            Assert.AreEqual("D5-default-arm", lit.ToString(), "the literal the pointer came from shares the write");
            Assert.AreEqual("D5-default-arm", again.ToString(), "a later evaluation of the same literal shares the write");
            Assert.AreEqual((byte)'D', @unsafe.StringData(again).Value, "a later StringData reads the write and does not panic");
        }
        finally
        {
            p.Value = (byte)'d';
        }

        Assert.AreEqual("d5-default-arm", lit.ToString(), "the restore put the literal back");
    }

    [TestMethod]
    public void WithTheSwitchOnTheRowsInAChildPass()
    {
        string filter = $"FullyQualifiedName~{typeof(ReadOnlyStringDataSwitchOnRows).FullName}.";

        ProcessStartInfo start = new("dotnet", ["test", typeof(ReadOnlyStringDataTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ChildVariable] = "1";
        start.Environment[SwitchVariable] = "1";

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("the switch-on child did not finish in 180 s");
        }

        string output = stdout.Result + stderr.Result;
        Assert.AreEqual(0, process.ExitCode, $"switch-on child:\n{output}");

        Match passed = Regex.Match(output, @"Passed:\s+(\d+)");
        Assert.IsTrue(passed.Success, $"the switch-on child printed no pass count:\n{output}");
        Assert.AreEqual(SwitchOnRowCount, int.Parse(passed.Groups[1].Value), $"rows the switch-on child passed:\n{output}");
    }
}

// The rows that need GO2CS_CHECK_STRING_WRITES=1. ReadOnlyStringDataTests.WithTheSwitchOnTheRowsInAChildPass runs them.
//
// The child process is shared by these rows, so each one that writes into a shadow puts the static byte back before it
// returns (through unsafe.Slice, the one view that takes no hook), or a later StringData over the same backing would
// panic in another row.
[TestClass]
public class ReadOnlyStringDataSwitchOnRows
{
    private const string PanicText = "write through a string's data pointer";

    // False outside the child, where the row returns at once. Inside it, the switch must have reached the process.
    private static bool InChild()
    {
        if (Environment.GetEnvironmentVariable(ReadOnlyStringDataTests.ChildVariable) != "1")
            return false;

        Assert.IsTrue(ReadOnlyStringBacking.Enabled, "the child runs without GO2CS_CHECK_STRING_WRITES=1");
        return true;
    }

    private static void AssertPanicsByName(Action access, string what)
    {
        PanicException panic = Assert.ThrowsException<PanicException>(access, $"{what} did not report the write");
        StringAssert.Contains(panic.Message, PanicText, what);
    }

    // Puts the static bytes back into the shadow p points into: unsafe.Slice reads the storage directly.
    private static void Restore(ж<byte> p, ReadOnlySpan<byte> original)
    {
        slice<byte> view = @unsafe.Slice(p, (uintptr)original.Length);

        for (int i = 0; i < original.Length; i++)
            view[i] = original[i];
    }

    [TestMethod]
    public void AWriteThroughAOneByteStringIsPrivateAndReportedAtTheNextAccess()
    {
        if (!InChild())
            return;

        // 0xA7 is not a byte any other test is likely to share a one-byte string with.
        @string s = new(new slice<byte>(new byte[] { 0xA7 }));
        ж<byte> p = @unsafe.StringData(s);

        try
        {
            p.Value = (byte)'z';

            @string again = new(new slice<byte>(new byte[] { 0xA7 }));
            Assert.AreEqual((byte)0xA7, s[0], "the string the pointer came from changed");
            Assert.AreEqual((byte)0xA7, again[0], "a later equal one-byte string changed");

            AssertPanicsByName(() => _ = p.Value, "the next dereference of the box");
            AssertPanicsByName(() => @unsafe.StringData(again), "the next StringData over the same backing");
        }
        finally
        {
            Restore(p, [0xA7]);
        }

        Assert.AreEqual((byte)0xA7, @unsafe.StringData(s).Value, "after the restore the pointer reads the static byte");
    }

    [TestMethod]
    public void AWriteThroughALiteralIsPrivateAndReportedAtTheNextAccess()
    {
        if (!InChild())
            return;

        @string lit = "d5-literal-arm";
        ж<byte> p = @unsafe.StringData(lit);

        try
        {
            p.Value = (byte)'Z';

            @string again = "d5-literal-arm";
            Assert.AreEqual("d5-literal-arm", lit.ToString(), "the literal the pointer came from changed");
            Assert.AreEqual("d5-literal-arm", again.ToString(), "a later evaluation of the same literal changed");

            AssertPanicsByName(() => _ = p.Value, "the next dereference of the box");
            AssertPanicsByName(() => @unsafe.StringData(again), "the next StringData over the same literal");
        }
        finally
        {
            Restore(p, "d5-literal-arm"u8);
        }

        Assert.AreEqual((byte)'d', @unsafe.StringData(lit).Value, "after the restore the pointer reads the static byte");
    }

    [TestMethod]
    public void AWriteThroughAHeapStringMutatesItAndItsSharersWithoutAPanic()
    {
        if (!InChild())
            return;

        @string h = new(new slice<byte>("hello, heap"u8.ToArray()));
        @string sharer = h;
        ж<byte> p = @unsafe.StringData(h);

        p.Value = (byte)'J';

        Assert.AreEqual("Jello, heap", h.ToString(), "Go mutates a heap string through its data pointer");
        Assert.AreEqual("Jello, heap", sharer.ToString(), "Go mutates every string sharing the bytes");
        Assert.AreEqual((byte)'J', p.Value, "the pointer reads what it wrote");
        Assert.AreEqual((byte)'J', @unsafe.StringData(h).Value, "a later StringData does not panic over heap bytes");
    }

    [TestMethod]
    public void ARunTimeStringEqualToALiteralIsHeapDataAndLeavesTheLiteralAlone()
    {
        if (!InChild())
            return;

        @string lit = "d5-near-miss";
        @string runTime = new(new slice<byte>("d5-near-miss"u8.ToArray()));
        ж<byte> p = @unsafe.StringData(runTime);

        p.Value = (byte)'D';

        Assert.AreEqual("D5-near-miss", runTime.ToString(), "the run-time string is heap data and is mutated");
        Assert.AreEqual("d5-near-miss", lit.ToString(), "the equal literal is untouched");
        Assert.AreEqual((byte)'d', @unsafe.StringData(lit).Value, "StringData over the literal does not panic");
    }

    [TestMethod]
    public void PointersIntoALiteralKeepGosIdentity()
    {
        if (!InChild())
            return;

        @string lit = "d5-identity";

        Assert.IsTrue(@unsafe.StringData(lit) == @unsafe.StringData(lit), "StringData(lit) == StringData(lit)");
        Assert.IsTrue(@unsafe.Add(@unsafe.StringData(lit), 1) == @unsafe.StringData(lit[1..]), "Add(StringData(lit), 1) == StringData(lit[1:])");

        slice<byte> view = @unsafe.Slice(@unsafe.StringData(lit), (uintptr)lit.Length);
        Assert.IsTrue(builtin.Ꮡ(view, 2) == @unsafe.StringData(lit[2..]), "&unsafe.Slice(p, n)[2] == StringData(lit[2:])");
        Assert.AreEqual("d5-identity", @unsafe.String(@unsafe.StringData(lit), (uintptr)lit.Length).ToString(), "unsafe.String reads the bytes back");
    }

    [TestMethod]
    public void AWriteThroughAnUnsafeSliceViewIsPrivateAndReportedOnlyAtTheNextHook()
    {
        if (!InChild())
            return;

        // The named limit: unsafe.Slice reads the storage directly, so the write itself is not seen. It still lands in
        // the shadow (the literal is untouched), and the next StringData over the backing reports it.
        @string lit = "d5-slice-limit";
        ж<byte> p = @unsafe.StringData(lit);
        slice<byte> view = @unsafe.Slice(p, (uintptr)lit.Length);

        try
        {
            view[0] = (byte)'X';

            Assert.AreEqual("d5-slice-limit", lit.ToString(), "the literal changed through an unsafe.Slice view");
            Assert.AreEqual((byte)'X', view[0], "the view reads its own write: no hook sees it");
            AssertPanicsByName(() => @unsafe.StringData(lit), "the next StringData over the same literal");
        }
        finally
        {
            Restore(p, "d5-slice-limit"u8);
        }
    }

    // Literals no other row converts: each round's StringData is the FIRST over its backing, so both threads race to make
    // the record and its shadow.
    private static readonly string[] s_raceLiterals =
    [
        "d5-race-0", "d5-race-1", "d5-race-2", "d5-race-3", "d5-race-4", "d5-race-5", "d5-race-6", "d5-race-7"
    ];

    [TestMethod]
    public void TwoThreadsTakingStringDataOfOneLiteralAtOnceGetEqualPointers()
    {
        if (!InChild())
            return;

        using Barrier start = new(2);

        foreach (string literal in s_raceLiterals)
        {
            @string lit = literal;
            ж<byte>? first = null, second = null;

            Thread other = new(() =>
            {
                start.SignalAndWait();
                second = @unsafe.StringData(lit);
            });

            other.Start();
            start.SignalAndWait();
            first = @unsafe.StringData(lit);
            other.Join();

            Assert.IsTrue(first == second, $"StringData({literal}) taken on two threads at once gave unequal pointers");
            Assert.AreEqual((byte)'d', first!.Value, $"StringData({literal}) reads the literal's first byte");
        }
    }
}
