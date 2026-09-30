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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
/// <param name="Records">
/// The execution's log records as a LIST, on a terminal <c>fail</c>/<c>skip</c> event only. <c>Output</c>
/// joins the same records with <see cref="Environment.NewLine"/>, which is also the separator inside one
/// record (a panic's stack frames), so the record boundaries survive only here. A disclosure pins the
/// record COUNT, so a failure printing an extra assert beside the pinned one cannot be absorbed.
/// </param>
/// <param name="RecordsDropped">Records the log cap dropped, when it dropped any: a count pin cannot hold then.</param>
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
    int? Line = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? Records = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? RecordsDropped = null);

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
                WriteEventLine(JsonSerializer.Serialize(testEvent, JsonOptions));
                return;
            }

            if (testEvent.Action == "run" && !verbose)
                return;

            string output = string.IsNullOrWhiteSpace(testEvent.Output) ? "" : $" — {testEvent.Output}";

            // A package-level event with nothing to say prints as Go's binary prints its summary: the bare
            // word, no column padding. A parent that re-executes this binary reads it back, and runtime's
            // TestFinalizerRegisterABI requires "PASS\n" in its -test.v child's output.
            WriteEventLine(string.IsNullOrEmpty(testEvent.Test) && output.Length == 0
                ? testEvent.Action.ToUpperInvariant()
                : $"{testEvent.Action.ToUpperInvariant(),-20} {testEvent.Test}{output}");
        }
    }

    // Serializes every event line this process writes, across reporters and the host's own
    // infrastructure-error line.
    private static readonly object s_eventLineLock = new();

    // The process's real stdout, opened once. Only written while Console.Out is still the console's own
    // writer (ConsoleOutIsTheProcessConsole).
    private static readonly Lazy<Stream> s_stdout = new(Console.OpenStandardOutput);

    // Console.SetOut sets this private flag, and it stays set after a restore, so a host that has ever
    // redirected its output keeps writing through Console.Out. A runtime without the field reads as
    // redirected too: the fallback is the behaviour this helper replaced, never a lost capture.
    internal const string ConsoleRedirectFlagName = "s_isOutTextWriterRedirected";

    private static readonly FieldInfo? s_outRedirectedFlag =
        typeof(Console).GetField(ConsoleRedirectFlagName, BindingFlags.NonPublic | BindingFlags.Static);

    // The resolved flag, for the guard that fails LOUDLY when a runtime renames or retypes it: the
    // fallback above is safe for captures but silently re-opens the tear on the real console.
    internal static FieldInfo? ConsoleRedirectFlagForGuard => s_outRedirectedFlag;

    private static bool ConsoleOutIsTheProcessConsole() => s_outRedirectedFlag?.GetValue(null) is false;

    // GUARD SEAMS (GolibTests' ConsoleEventLineAtomicityTests). MSTest replaces Console.Out itself, so a
    // guard cannot reach the process-console branch without them: one forces the branch, and one supplies
    // the stream the process's stdout would be. Null in every real run.
    internal static bool? ProcessConsoleForGuard { get; set; }
    internal static Stream? StdoutForGuard { get; set; }

    /// <summary>
    /// Writes one event line, with its newline, as ONE write to the process's stdout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The comparer reads the C# side's events from stdout and stderr merged onto one pipe. Console's own
    /// writer hands a line to the pipe in pieces of its 256-character buffer, so a line longer than that
    /// is several writes, and another writer on the same pipe can land between them: a raw-handle writer
    /// (converted Go code's os.Stdout and os.Stderr) on every OS, and Console.Error on windows, whose
    /// console writes take no shared lock. The line is torn and its event is lost. Measured (linux, a
    /// raw-handle writer beside 200 parallel tests whose JSON lines run past 300 characters): 53 to 146
    /// of 200 pass events arrived whole; the i9 measured 15% to 65% lost on windows.
    /// </para>
    /// <para>
    /// One write of the whole line is atomic on a pipe up to PIPE_BUF (4096 bytes on linux), so a longer
    /// line can still interleave with a raw writer; nothing in-process can lock a writer it does not own.
    /// When Console.Out has been replaced (Console.SetOut, as an in-process host capture does), the line
    /// goes to that writer instead, in one call, because that is where the caller asked output to go.
    /// </para>
    /// <para>
    /// The line ends in a bare "\n" on every OS, as Go's test binary's lines do, never the writer's NewLine
    /// ("\r\n" on windows). A Go test that re-executes its own binary reads this output back: runtime's
    /// TestFinalizerRegisterABI requires strings.Contains(out, "PASS\n"), and a child that passed but printed
    /// "PASS\r\n" failed it (D5). The comparer splits on "\n" and reads either ending.
    /// </para>
    /// </remarks>
    internal static void WriteEventLine(string line)
    {
        lock (s_eventLineLock)
        {
            if (!(ProcessConsoleForGuard ?? ConsoleOutIsTheProcessConsole()))
            {
                Console.Out.Write(line + "\n");
                return;
            }

            byte[] bytes = Console.OutputEncoding.GetBytes(line + "\n");
            Stream stdout = StdoutForGuard ?? s_stdout.Value;

            // Anything already written through Console.Out stays ahead of this line.
            Console.Out.Flush();
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
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
