// ViewCensusControlTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Controls for the view-cache miss census (golib <c>ViewCensus.cs</c>, a scratch instrument for board oo). The RECORDER
/// arms run in every configuration against a tally of their own. The WIRING arms need the live census, which is read once
/// from <c>GO2CS_VIEW_CENSUS</c>: with it off they report NOT MEASURED, never green, and the off arm asserts that the
/// hooks counted nothing.
/// </summary>
[TestClass]
public unsafe class ViewCensusControlTests
{
    // A pointee no other test mints, so the live census's rows for it move only here.
    private struct censusProbe
    {
        public nint a;
        public nint b;

        internal static ref nint Ꮡb(ref censusProbe p) => ref p.b;
    }

    private struct censusProbeOff
    {
        public nint a;
        public nint b;

        internal static ref nint Ꮡb(ref censusProbeOff p) => ref p.b;
    }

    // ---- the recorder ----

    [TestMethod]
    public void TheRecorderCountsMintsNilMintsAndDistinctAddresses()
    {
        ViewCensusTally tally = new(sample: 0);

        tally.Mint(typeof(censusProbe), 0x1000, sampleStack: false);
        tally.Mint(typeof(censusProbe), 0x1000, sampleStack: false);
        tally.Mint(typeof(censusProbe), 0x2000, sampleStack: false);
        tally.Mint(typeof(censusProbe), 0, sampleStack: false);
        tally.Mint(typeof(long), 0x1000, sampleStack: false);

        Assert.AreEqual((5L, 1L, 0L, 0L, 3L), tally.Snapshot(), "mints, nil mints, slow, native fields, distinct (T, address) pairs");
        Assert.AreEqual(4L, tally.MintRow(typeof(censusProbe)));
        Assert.AreEqual(1L, tally.MintRow(typeof(long)));
    }

    [TestMethod]
    public void TheRecorderKeysSlowRowsByKindWhereAndOutcome()
    {
        ViewCensusTally tally = new(sample: 0);
        Type box = typeof(NativeBox<censusProbe>);

        tally.Slow(box, ViewCensus.WhereWeakNew, found: false);
        tally.Slow(box, ViewCensus.WhereWeakNew, found: false);
        tally.Slow(box, ViewCensus.WhereWeakLive, found: true);
        tally.NativeField(box);

        Assert.AreEqual(2L, tally.SlowRow(box, ViewCensus.WhereWeakNew, found: false));
        Assert.AreEqual(1L, tally.SlowRow(box, ViewCensus.WhereWeakLive, found: true));
        Assert.AreEqual(0L, tally.SlowRow(box, ViewCensus.WhereWeakLive, found: false));
        Assert.AreEqual(1L, tally.NativeFieldRow(box));
        Assert.AreEqual((0L, 0L, 3L, 1L, 0L), tally.Snapshot());
    }

    [TestMethod]
    public void ASampledMintNamesItsCallerOutsideGolib()
    {
        ViewCensusTally tally = new(sample: 2);

        for (int i = 0; i < 4; i++)
            tally.Mint(typeof(censusProbe), 0x1000, sampleStack: true);

        string[] sites = tally.Lines(final: true).Where(line => line.StartsWith("VCENSUS-SITE", StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(1, sites.Length, string.Join("\n", sites));
        StringAssert.Contains(sites[0], " 2  ");
        StringAssert.Contains(sites[0], "caller=GolibTests.ViewCensusControlTests.");
    }

    [TestMethod]
    public void AFinalBlockReconcilesAndIsWrittenWhole()
    {
        string path = Path.Combine(Path.GetTempPath(), $"g2cs-vcensus-{Guid.NewGuid():N}.txt");
        ViewCensusTally tally = new(sample: 1) { Path = path };

        try
        {
            tally.Mint(typeof(censusProbe), 0x1000, sampleStack: true);
            tally.Slow(typeof(NativeBox<censusProbe>), ViewCensus.WhereWeakNew, found: false);
            tally.NativeField(typeof(NativeBox<censusProbe>));
            tally.Write(final: true);

            string[] lines = File.ReadAllLines(path);

            StringAssert.StartsWith(lines[0], "VCENSUS mints=1 nil-mints=0 distinct=1 slow=1 native-fields=1 sampled=1 sample-every=1");
            Assert.IsTrue(lines.Any(line => line.StartsWith("VCENSUS-RECONCILES", StringComparison.Ordinal)), string.Join("\n", lines));
            Assert.AreEqual(1, lines.Count(line => line.StartsWith("VCENSUS-SLOW", StringComparison.Ordinal)
                && line.Contains("kind=NativeBox  where=weak-new  outcome=minted", StringComparison.Ordinal)), string.Join("\n", lines));
            Assert.IsFalse(File.Exists(path + ".tmp"), "the temporary sibling was moved over the file");
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ---- the wiring (the live census) ----

    [TestMethod]
    public void WithTheCensusOffTheHooksCountNothing()
    {
        if (ViewCensus.Enabled)
            Assert.Inconclusive("the census is on: this arm is the off half of the control.");

        nuint memory = (nuint)NativeMemory.AllocZeroed((nuint)sizeof(censusProbeOff));

        try
        {
            ж<censusProbeOff> root = new NativeBox<censusProbeOff>(memory);
            _ = root.of(censusProbeOff.Ꮡb);
            _ = root.of(censusProbeOff.Ꮡb);

            Assert.AreEqual((0L, 0L, 0L, 0L, 0L), ViewCensus.Live.Snapshot());
            Assert.IsNull(ViewCensus.StartBlockPath, "and no file was started");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // The per-access signature, driven on purpose: a native root asked for one field twice finds its view the second
    // time (weak-live), and a FRESH root over the same address creates its weak-table entry again (weak-new). Each of()
    // answered by a native box at the field's Go offset is a NATIVE-FIELD row and a MINT of its own.
    [TestMethod]
    public void TheLiveCensusCountsANativeRootsFieldViews()
    {
        if (!ViewCensus.Enabled)
            Assert.Inconclusive("NOT MEASURED: the view census is off. Set GO2CS_VIEW_CENSUS to run this control.");

        ViewCensusTally live = ViewCensus.Live;
        Type rootType = typeof(NativeBox<censusProbe>);
        nuint memory = (nuint)NativeMemory.AllocZeroed((nuint)sizeof(censusProbe));

        long mints0 = live.MintRow(typeof(censusProbe)), fieldMints0 = live.MintRow(typeof(nint));
        long weakNew0 = live.SlowRow(rootType, ViewCensus.WhereWeakNew, found: false);
        long weakLive0 = live.SlowRow(rootType, ViewCensus.WhereWeakLive, found: true);
        long native0 = live.NativeFieldRow(rootType);

        try
        {
            ж<censusProbe> root = new NativeBox<censusProbe>(memory);
            ж<nint> first = root.of(censusProbe.Ꮡb);
            ж<nint> second = root.of(censusProbe.Ꮡb);
            ж<censusProbe> fresh = new NativeBox<censusProbe>(memory);
            ж<nint> third = fresh.of(censusProbe.Ꮡb);

            int nativeAnswers = new[] { first, second, third }.Count(view => view is not FieldRefBox<nint> && view.NativeAddress != 0);

            Assert.AreEqual(2L, live.MintRow(typeof(censusProbe)) - mints0, "two roots minted");
            Assert.AreEqual(2L, live.SlowRow(rootType, ViewCensus.WhereWeakNew, found: false) - weakNew0, "each root's first of() creates its entry");
            Assert.AreEqual(1L, live.SlowRow(rootType, ViewCensus.WhereWeakLive, found: true) - weakLive0, "the live root's second of() finds its view");
            Assert.AreEqual((long)nativeAnswers, live.NativeFieldRow(rootType) - native0, $"one NATIVE-FIELD row per native answer ({nativeAnswers} of 3)");
            Assert.IsTrue(live.MintRow(typeof(nint)) - fieldMints0 >= nativeAnswers, "each native answer is a mint of its own");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }
}
