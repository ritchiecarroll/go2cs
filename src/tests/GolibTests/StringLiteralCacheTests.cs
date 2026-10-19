// StringLiteralCacheTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Pins <c>StringLiteralCache</c>, the cache behind <c>implicit operator @string(string)</c>: a C#
/// string LITERAL converts once and is shared; every other string converts exactly as before.
/// </summary>
/// <remarks>
/// The arms: the bytes are today's bytes (including a lone surrogate, NUL and the empty string);
/// a literal is cached and shared; a run-time string is never cached, even when its content equals
/// a cached literal; the shared value cannot be mutated through Go's copying exits; concurrent
/// first conversions agree on one value while the table grows; a collectible module's literal is
/// not admitted; and the allocation counter charges a miss and never a hit.
/// </remarks>
[TestClass]
public partial class StringLiteralCacheTests
{
    private static bool SameBacking(@string a, @string b) =>
        Unsafe.AreSame(ref MemoryMarshal.GetReference(a.ToSpan()), ref MemoryMarshal.GetReference(b.ToSpan()));

    // A string with the same content as a literal, built at run time (gen 0, never frozen).
    private static string RuntimeCopy(string literal) => new(literal.AsSpan());

    [TestMethod]
    public void ConversionBytesMatchTodaysTranscodingMissThenHit()
    {
        string[] cases =
        [
            "plc-ascii",
            "plc-héllo, 世界 \U0001F600",
            "plc-a\0b",
            "plc-lone-\uD800-high",
            "plc-lone-\uDC00-low",
            "plc-reversed-\uDC00\uD800",
            "",
        ];

        foreach (string literal in cases)
        {
            byte[] expected = Encoding.UTF8.GetBytes(literal);
            @string miss = literal;
            @string hit = literal;

            CollectionAssert.AreEqual(expected, miss.ToSpan().ToArray(), $"miss bytes for {Escape(literal)}");
            CollectionAssert.AreEqual(expected, hit.ToSpan().ToArray(), $"hit bytes for {Escape(literal)}");
            Assert.AreEqual(new @string(literal), hit, "equal to the uncached constructor's conversion");
            Assert.AreEqual(new @string(literal).GetHashCode(), hit.GetHashCode(), "hash equal to the uncached conversion");
        }

        // The lone-surrogate rule is the encoder's replacement character, EF BF BD, as before.
        @string lone = "plc-lone-\uD800-high";
        Assert.IsTrue(lone.ToSpan().IndexOf(new byte[] { 0xEF, 0xBF, 0xBD }) >= 0);
        Assert.AreEqual((nint)7, ((@string)"plc-a\0b").Length, "NUL is one byte, inside the string");
    }

    [TestMethod]
    public void ALiteralIsCachedAndEveryEvaluationSharesOneValue()
    {
        @string first = "plc-shared-literal";
        @string second = "plc-shared-literal";

        Assert.IsTrue(StringLiteralCache.Contains("plc-shared-literal"), "a frozen literal is admitted");
        Assert.IsTrue(SameBacking(first, second), "every evaluation of one literal yields one backing array");
        Assert.AreEqual(int.MaxValue, GC.GetGeneration("plc-shared-literal"), "precondition: the literal is frozen");
    }

    [TestMethod]
    public void ARunTimeStringIsNeverCachedEvenWithALiteralsContent()
    {
        @string literal = "plc-runtime-twin";
        string runtime = RuntimeCopy("plc-runtime-twin");
        int before = StringLiteralCache.Count;

        @string a = runtime;
        @string b = runtime;

        Assert.AreNotEqual(int.MaxValue, GC.GetGeneration(runtime), "precondition: a run-time string is not frozen");
        Assert.IsFalse(StringLiteralCache.Contains(runtime), "a run-time string is never admitted");
        Assert.AreEqual(before, StringLiteralCache.Count, "converting a run-time string adds no entry");
        Assert.IsFalse(SameBacking(a, b), "each run-time conversion allocates its own backing, as before");
        Assert.IsFalse(SameBacking(a, literal), "a content-equal run-time string does not hit the literal's entry");
        Assert.AreEqual(literal, a);
        Assert.AreEqual(literal, b);
        Assert.IsTrue(StringLiteralCache.Contains("plc-runtime-twin"), "the literal stays cached");
    }

    [TestMethod]
    public void NullConvertsToTheEmptyStringAndIsNotCached()
    {
        string none = null;
        @string converted = none!;

        Assert.AreEqual((nint)0, converted.Length);
        Assert.AreEqual((@string)"", converted);
        Assert.AreEqual(new @string((string?)null), converted);
    }

    [TestMethod]
    public void TheSharedValueCannotBeMutatedThroughGosCopyingExits()
    {
        @string literal = "plc-immutable";

        slice<byte> asSlice = literal;      // Go []byte(s): a copy
        asSlice[0] = (byte)'X';
        byte[] asArray = literal;           // the byte[] exit: a copy
        asArray[1] = (byte)'Y';
        slice<byte> sub = ((@string)"plc-immutable")[4..];
        sub[0] = (byte)'Z';

        @string again = "plc-immutable";
        Assert.AreEqual("plc-immutable", again.ToString(), "the cached value is unchanged after writes to every exit");
        Assert.IsTrue(SameBacking(literal, again));
        Assert.AreEqual((@string)"plc-immutable"u8, again);
    }

    [TestMethod]
    public void ConcurrentFirstConversionsAgreeOnOneValueWhileTheTableGrows()
    {
        const int threads = 16;
        @string[][] seen = new @string[threads][];
        using Barrier start = new(threads);
        List<Exception> errors = [];

        Thread[] workers = Enumerable.Range(0, threads).Select(t => new Thread(() =>
        {
            try
            {
                @string[] mine = new @string[s_literals.Length];
                start.SignalAndWait();

                // Each thread walks the literals in its own order, many times, racing the inserts and
                // the table growth the 600 entries force.
                for (int round = 0; round < 50; round++)
                {
                    for (int k = 0; k < s_literals.Length; k++)
                    {
                        int i = (k * (2 * t + 1) + round) % s_literals.Length;
                        @string v = s_literals[i];

                        if (mine[i].Length == 0)
                            mine[i] = v;
                        else if (!SameBacking(mine[i], v))
                            throw new InvalidOperationException($"thread {t}: {s_literals[i]} changed backing between evaluations");
                    }
                }

                seen[t] = mine;
            }
            catch (Exception ex)
            {
                lock (errors) errors.Add(ex);
            }
        })).ToArray();

        foreach (Thread w in workers) w.Start();
        foreach (Thread w in workers) w.Join();

        Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors));

        for (int i = 0; i < s_literals.Length; i++)
        {
            byte[] expected = Encoding.UTF8.GetBytes(s_literals[i]);

            for (int t = 0; t < threads; t++)
            {
                CollectionAssert.AreEqual(expected, seen[t][i].ToSpan().ToArray(), $"bytes of {s_literals[i]} on thread {t}");
                Assert.IsTrue(SameBacking(seen[0][i], seen[t][i]), $"{s_literals[i]}: threads 0 and {t} got different values");
            }

            Assert.IsTrue(StringLiteralCache.Contains(s_literals[i]), $"{s_literals[i]} cached");
        }

        Assert.IsTrue(StringLiteralCache.Count >= s_literals.Length);
    }

    [TestMethod]
    public void AHitChargesNothingAndARunTimeMissChargesOneAllocation()
    {
        AllocationCounter.Enable();
        @string warm = "plc-alloc-literal";
        string runtime = RuntimeCopy("plc-alloc-literal");

        long c0 = AllocationCounter.CurrentThreadCount;
        @string hit = "plc-alloc-literal";
        long c1 = AllocationCounter.CurrentThreadCount;
        @string miss = runtime;
        long c2 = AllocationCounter.CurrentThreadCount;

        Assert.AreEqual(0L, c1 - c0, "a cache hit allocates nothing (Go: a literal is RODATA)");
        Assert.AreEqual(1L, c2 - c1, "a run-time conversion charges its UTF-8 backing, as before");

        long b0 = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) hit = "plc-alloc-literal";
        long b1 = GC.GetAllocatedBytesForCurrentThread();

        Assert.AreEqual(0L, b1 - b0, "1,000 hits allocate 0 bytes");
        GC.KeepAlive(warm);
        GC.KeepAlive(miss);
    }

    [TestMethod]
    public void ACollectibleModulesLiteralIsNotAdmittedAndTheModuleUnloads()
    {
        WeakReference alc = ConvertInCollectibleContext(out bool frozen, out bool cached, out string bytes, out bool sharedIsGlobal);

        Assert.IsFalse(frozen, "precondition: a collectible module's literal is not frozen");
        Assert.IsFalse(cached, "a collectible module's literal is never admitted (it would pin the module)");
        Assert.AreEqual(string.Concat("plc-alc-", "only-text".AsSpan()), bytes, "the module's literal converted with its own bytes");

        // The runtime's global intern table resolves a collectible module's literal to the instance a
        // NON-collectible module already interned: that one is frozen and owned by the default
        // context, so admitting it pins nothing of the collectible module (measured, this host).
        Assert.IsTrue(sharedIsGlobal, "a literal the default context also holds resolves to its frozen instance");

        for (int i = 0; alc.IsAlive && i < 20; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(alc.IsAlive, "the collectible context unloaded: the cache holds nothing of it");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ConvertInCollectibleContext(out bool frozen, out bool cached, out string text, out bool sharedIsGlobal)
    {
        const string source = """
            public static class PlcCollectible
            {
                public static string Raw() => "plc-alc-only-text";
                public static go.@string Converted() => "plc-alc-only-text";
                public static string Shared() => "plc-shared-literal";
            }
            """;

        string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        MetadataReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(@string).Assembly.Location),
        ];

        CSharpCompilation compilation = CSharpCompilation.Create("PlcCollectible",
            [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = compilation.Emit(image);
        Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));

        // golib resolves from the default context (Load returns null), so the module converts through
        // the SAME cache the rest of the process uses.
        AssemblyLoadContext context = new("plc-collectible", isCollectible: true);
        image.Position = 0;
        Type type = context.LoadFromStream(image).GetType("PlcCollectible")!;

        string raw = (string)type.GetMethod("Raw")!.Invoke(null, null)!;
        @string converted = (@string)type.GetMethod("Converted")!.Invoke(null, null)!;
        @string again = (@string)type.GetMethod("Converted")!.Invoke(null, null)!;

        frozen = GC.GetGeneration(raw) == int.MaxValue;
        cached = StringLiteralCache.Contains(raw);
        string shared = (string)type.GetMethod("Shared")!.Invoke(null, null)!;
        sharedIsGlobal = ReferenceEquals(shared, "plc-shared-literal") && GC.GetGeneration(shared) == int.MaxValue;
        text = converted.ToString();
        Assert.AreEqual(converted, again);

        context.Unload();
        return new WeakReference(context);
    }

    private static string Escape(string s) =>
        string.Concat(s.Select(c => c < 0x20 || c > 0x7E ? $"\\u{(int)c:X4}" : c.ToString()));
}
