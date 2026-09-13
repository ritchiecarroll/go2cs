// TestReporter.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using go.golib;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale and the measured clobber are in
// testing.cs). No converted source emits at this path, so this marker declares ownership rather than
// resolving a collision; the mechanical guards are the -stdlib skip list (isNonConvertedStdLibPackage)
// and testConversion.go's -tests refusal (requireConvertibleTestTarget).
[module: go.GoManualConversion]

namespace go.testing_runtime;
/// <summary>
/// One reported moment in a run — a test starting, passing, failing, skipping, or emitting output —
/// shaped after the events <c>go test -json</c> emits.
/// </summary>
/// <param name="Package">Go import path of the package under test.</param>
/// <param name="Test">Test name, or <c>""</c> for a package-level event.</param>
/// <param name="Action">What happened: <c>run</c>, <c>pass</c>, <c>fail</c>, <c>skip</c>, <c>output</c>, …</param>
/// <param name="Elapsed">Seconds the test took, on a terminal event.</param>
/// <param name="Output">Log or failure text, when the event carries any.</param>
/// <param name="Source">Go source file of the declaration, when known.</param>
/// <param name="Line">Go source line of the declaration, when known.</param>
/// <remarks>
/// The shape is deliberate: the Phase-4 pipeline runs the same suite twice — once here and once
/// under real <c>go test -json</c> — and diffs the two verdict streams. Matching Go's event
/// vocabulary is what lets that comparison be mechanical rather than a text heuristic.
/// </remarks>
public sealed record TestEvent(
    string Package,
    string Test,
    string Action,
    double Elapsed = 0.0D,
    string? Output = null,
    string? Source = null,
    int? Line = null);

/// <summary>
/// Collects every <see cref="TestEvent"/> of a run and writes it out, either as <c>go test
/// -json</c> lines or in the human-readable form.
/// </summary>
/// <param name="package">Go import path, stamped onto package-level events.</param>
/// <param name="json">Emit one JSON object per event (the machine-comparable form).</param>
/// <param name="verbose">Report <c>run</c> events too, mirroring <c>go test -v</c>.</param>
/// <remarks>
/// Every method locks, and that is not defensive habit: tests run in parallel by design, so
/// reporting is genuinely concurrent, and both the retained list and the console writes have to be
/// serialized or JSON lines interleave mid-object and become unparseable. The retained
/// <see cref="Events"/> are what the host writes the result and JUnit files from at the end, so
/// they must survive even a run that ends in a package timeout.
/// </remarks>
internal sealed class TestReporter(string package, bool json, bool verbose)
{
    private readonly object m_syncRoot = new();
    private readonly List<TestEvent> m_events = [];

    public IReadOnlyList<TestEvent> Events
    {
        get
        {
            lock (m_syncRoot)
                return m_events.ToArray();
        }
    }

    public void Report(TestEvent testEvent)
    {
        lock (m_syncRoot)
        {
            m_events.Add(testEvent);

            if (json)
            {
                Console.WriteLine(JsonSerializer.Serialize(testEvent, JsonOptions));
                return;
            }

            if (testEvent.Action == "run" && !verbose)
                return;

            string output = string.IsNullOrWhiteSpace(testEvent.Output) ? "" : $" — {testEvent.Output}";
            Console.WriteLine($"{testEvent.Action.ToUpperInvariant(),-20} {testEvent.Test}{output}");
        }
    }

    public void ReportPackage(string action, double elapsed = 0.0D, string? output = null) =>
        Report(new TestEvent(package, "", action, elapsed, output));

    /// <summary>
    /// Retains a package-level event WITHOUT writing it to the console -- for the host's own record
    /// of an exit the process is already taking.
    /// </summary>
    /// <remarks>
    /// Go's test binary prints nothing on os.Exit: the PASS line was M.Run's, and the `fail` action a
    /// non-zero status implies is appended by `go test` (the parent), never by the binary. The host's
    /// results FILE is where that fact belongs, and stdout must stay exactly what the converted
    /// program left there -- a helper process re-executed by os/exec's tests has its stdout read back
    /// by the test that spawned it, and one printed line broke twenty of them (measured 2026-09-04:
    /// `echo: want "foo bar baz\n", got "foo bar baz\nPASS ... exit status 0 ..."`).
    /// </remarks>
    public void RecordPackage(string action, double elapsed = 0.0D, string? output = null)
    {
        lock (m_syncRoot)
            m_events.Add(new TestEvent(package, "", action, elapsed, output));
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
