// W8 round-trip probe (kept as data beside DESIGN-sqlite-ccgo-ts.md; dropped into GolibTests, run filtered, removed): the &b[0] round-trip window that (H) reuses. A heap string's element-0 box mints
// a number (pinned and registered); then the box and the string are dropped, compacting collections with finalizers
// run, and a pointer is re-formed from the number. Reports whether the registry still resolved it, and whether the
// bytes read through it are the string's.
using System;
using System.Runtime.CompilerServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

[TestClass]
public unsafe class ProbeRoundTrip
{
    private const int Trials = 200, Size = 64;

    [TestMethod]
    public void Measure()
    {
        string label = Environment.GetEnvironmentVariable("PROBE_LABEL") ?? "?";
        int controlWrong = 0, windowResolved = 0, windowWrong = 0;
        var keep = new object[Trials];

        for (int t = 0; t < Trials; t++)
        {
            // CONTROL: the element box is held across the collections.
            (nuint n, byte[] expected, object box) = Mint(t, holdBox: true);
            keep[t] = box;
            Churn();
            if (!Same(n, expected)) controlWrong++;
            GC.KeepAlive(box);
        }

        for (int t = 0; t < Trials; t++)
        {
            // THE WINDOW: nothing holds the box or the string; a collection lands before the pointer is re-formed.
            (nuint n, byte[] expected, _) = Mint(t, holdBox: false);
            Churn();
            if (ManagedPointerTokens.Resolve(n) is not null) windowResolved++;
            if (!Same(n, expected)) windowWrong++;
        }

        string line = $"{label} control (box held): wrong bytes {controlWrong}/{Trials} | window (box dropped, compacting GC + finalizers between): resolved {windowResolved}/{Trials}, wrong bytes {windowWrong}/{Trials}";
        Console.WriteLine(line);
        System.IO.File.AppendAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")!, [line]);
        GC.KeepAlive(keep);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (nuint, byte[], object?) Mint(int seed, bool holdBox)
    {
        byte[] bytes = new byte[Size];
        new Random(seed).NextBytes(bytes);
        @string s = new(bytes);
        ж<byte> element = builtin.Ꮡ(s.UnsafeBackingWindow(), 0);
        nuint n = (nuint)(uintptr)element;
        return (n, (byte[])bytes.Clone(), holdBox ? element : null);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Churn()
    {
        for (int i = 0; i < 2000; i++) _ = new byte[256];
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        for (int i = 0; i < 2000; i++) _ = new byte[64];
    }

    // The pointer re-formed from the number, read as libc would: through (ж<byte>)(uintptr), byte by byte.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Same(nuint n, byte[] expected)
    {
        for (int i = 0; i < expected.Length; i++)
        {
            ж<byte> p = (ж<byte>)(uintptr)(n + (nuint)i);
            if (p.Value != expected[i]) return false;
        }
        return true;
    }
}
