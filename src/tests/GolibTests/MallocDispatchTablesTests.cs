// MallocDispatchTablesTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using gc = go.@internal.runtime.gc_package;

namespace GolibTests;

/// <summary>
/// Go 1.26's size-specialized malloc dispatch tables (runtime/malloc_tables_generated.go), which the converter builds
/// on their FIRST READ (lazyPackageVars, COORD's ruling (A), 2026-10-22): the runtime's static constructor no longer
/// builds them, and a read that does come finds them populated, entry for entry what Go's mkmalloc generates.
/// </summary>
/// <remarks>
/// mallocgc is their only reader, through a branch gated by sizeSpecializedMallocEnabled, a const false here (the
/// SizeSpecializedMalloc experiment is off), so no program path reads them; the guard reads through the holder the
/// generated ref property reads, which runs the holder's initializer exactly as the first real read would. The oracle is
/// the rule mkmalloc writes the tables by, over the converted internal/runtime/gc size classes: entry 0 is mallocPanic;
/// the scan table holds mallocgcSmallScanNoHeaderSC{class} for every size 1..512; the no-scan table holds
/// mallocgcTinySize{size} below 16 and mallocgcSmallNoScanSC{class} from 16 on.
/// </remarks>
[TestClass]
public class MallocDispatchTablesTests
{
    private const int TableLength = 513;

    // The class of a small size, as Go's runtime takes it: size_to_class8[divRoundUp(size, smallSizeDiv)].
    private static int SizeClass(int size) => gc.SizeToSizeClass8[(size + 7) / 8];

    // The first read: the holder's static field, which the generated `ref` property returns.
    private static Delegate[] Read(string table)
    {
        Type? holder = typeof(runtime_package).GetNestedType("lazyᴛ" + table, BindingFlags.NonPublic);

        Assert.IsNotNull(holder, $"{table} is built on its first read through a holder class (lazyᴛ{table})");

        // array<T> over Func<uintptr, ж<_type>, bool, unsafe.Pointer>, whose _type this assembly cannot name.
        object value = holder!.GetField("value", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        return [.. ((System.Collections.IEnumerable)value).Cast<Delegate>()];
    }

    private static void AssertTable(string table, Func<int, string> expected)
    {
        Delegate[] entries = Read(table);

        Assert.AreEqual(TableLength, entries.Length, $"{table} has Go's 513 entries");

        for (int size = 0; size < TableLength; size++)
        {
            Assert.IsNotNull(entries[size], $"{table}[{size}] is populated");
            Assert.AreEqual(expected(size), entries[size].Method.Name, $"{table}[{size}] is the function Go's mkmalloc writes there");
        }
    }

    [TestMethod]
    public void TheScanTableIsPopulatedOnItsFirstReadAsGoBuildsIt() =>
        AssertTable("mallocScanTable", static size => size == 0 ? "mallocPanic" : $"mallocgcSmallScanNoHeaderSC{SizeClass(size)}");

    [TestMethod]
    public void TheNoScanTableIsPopulatedOnItsFirstReadAsGoBuildsIt() =>
        AssertTable("mallocNoScanTable", static size => size switch
        {
            0 => "mallocPanic",
            < 16 => $"mallocgcTinySize{size}",
            _ => $"mallocgcSmallNoScanSC{SizeClass(size)}"
        });

    [TestMethod]
    public void TheRuntimeStaticConstructorNoLongerBuildsThem()
    {
        // The ordered constructor (package_init.cs) calls an initᴛ method per relocated var; a lazily built var has none.
        foreach (string table in new[] { "mallocScanTable", "mallocNoScanTable" })
            Assert.IsNull(typeof(runtime_package).GetMethod("initᴛ" + table, BindingFlags.NonPublic | BindingFlags.Static), $"{table} has no initᴛ method");
    }
}
