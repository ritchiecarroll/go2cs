// TestOptions.cs - Gbtc
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
/// The <c>go test</c> command line, parsed — the flags this host understands, with Go's defaults.
/// </summary>
/// <remarks>
/// <para>
/// The flag names, their meanings and their defaults track <c>go test</c> deliberately, because the
/// Phase-4 comparison runs the SAME arguments against both sides. A flag that means something
/// slightly different here would make the two runs incomparable without any error being reported —
/// the run would simply be answering a different question.
/// </para>
/// <para>
/// <c>-run</c> is compiled to a regex ARRAY, one element per <c>/</c>-separated segment, because
/// Go's filter matches subtest paths segment by segment rather than the joined name.
/// <c>ParseDuration</c> accepts Go's duration syntax (<c>2m</c>, <c>500ms</c>, <c>1h30m</c>) rather
/// than .NET's, for the same reason.
/// </para>
/// </remarks>
internal sealed class TestOptions
{
    public bool Json { get; private set; }
    public bool Verbose { get; private set; }
    public bool Short { get; private set; }
    public int Count { get; private set; } = 1;
    public int Parallel { get; private set; } = Environment.ProcessorCount;
    public int? ShuffleSeed { get; private set; }
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromMinutes(10.0D);
    public string? ResultFile { get; private set; }
    public string? JUnitFile { get; private set; }
    private Regex[]? Filters { get; set; }

    // -skip is -run's inverse and MUST compile identically: same `/` split, same per-segment regexes.
    // Go documents it as "run only those tests that do NOT match", "like for -run". If the two sides
    // compiled it differently the converted host would withdraw a different set than the `go test`
    // oracle handed the same string -- two runs answering different questions with nothing failing.
    private Regex[]? SkipFilters { get; set; }

    // The RAW text of -run and -shuffle, kept beside the compiled forms because TestFlagBridge
    // republishes this run's command line to the converted flag package and a converted test READS
    // it back: os/exec's TestMain gates on flag.Lookup("test.run").Value.String() being empty. A
    // Regex[] cannot answer that question — Regex.ToString() would report the first SEGMENT of a
    // `/`-split pattern — so the value the host was given is what gets carried.
    public string RunPattern { get; private set; } = "";
    public string SkipPattern { get; private set; } = "";
    public string ShuffleValue { get; private set; } = "off";

    /// <summary>
    /// The first flag name this host does not define, undashed, or <c>null</c> when the parse met
    /// none. Its verdict is deferred rather than decided here — see <see cref="Parse"/>.
    /// </summary>
    public string? UnrecognizedFlag { get; private set; }

    /// <summary>
    /// Parses the host's command line with Go's <c>flag</c> package semantics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Parsing STOPS at the first non-flag token</b>, which is <c>flag.(*FlagSet).parseOne</c>'s
    /// own rule and not a convenience: a Go test binary is a program in its own right, and its
    /// <c>TestMain</c> is free to take arguments. <c>os/exec</c> is the corpus's example and drives
    /// its entire helper protocol this way — it re-executes the test binary as
    /// <c>exec.Command(exePath(t), "cat")</c> and dispatches on <c>flag.Args()[0]</c>. Rejecting
    /// <c>cat</c> as an unknown host option killed every helper child at startup with exit 2 before
    /// <c>TestMain</c> was entered, and the parent then reported the DOWNSTREAM symptom (<c>echo:
    /// want "foo bar baz\n", got ""</c>), so one host defect read as twenty unrelated failures.
    /// </para>
    /// <para>
    /// Stopping is not the same as ignoring, and the difference is load-bearing: <c>exe cat -n</c>
    /// must leave <c>-n</c> to the child rather than parse it here. A lone <c>-</c> is a non-flag
    /// (Go's length test), and <c>--</c> terminates the flags and is itself consumed.
    /// </para>
    /// <para>
    /// <b>A flag name this host does not define STOPS the parse too, and its verdict is DEFERRED</b>
    /// to <see cref="UnrecognizedFlag"/> rather than raised here. Go's test binary never faces this
    /// question, because it reaches exactly ONE <c>flag.Parse()</c> — by which time
    /// <c>testing.Init()</c> has defined the <c>-test.*</c> set AND the package's own package-level
    /// <c>flag.Bool</c>/<c>flag.String</c> variables have initialized, so both vocabularies are
    /// present in a single flag set. This host's parse necessarily runs EARLIER than the package's
    /// initialization, so a name it does not recognize is not yet knowably wrong: it may be the
    /// package's. <c>crypto/tls</c> is the corpus's example — the BoGo runner re-executes the test
    /// binary as its own TLS shim (<c>-shim-path=os.Args[0] -shim-extra-flags=-bogo-mode</c>), and
    /// <c>-bogo-mode</c> is a package-level <c>flag.Bool</c> in <c>handshake_test.go</c>. Rejecting
    /// it here killed every BoGo case at startup before <c>TestMain</c> ran.
    /// </para>
    /// <para>
    /// The rejection is not dropped, only moved: <see cref="TestHost"/> initializes the package the
    /// way Go does — before main — and then asks the converted <c>flag.CommandLine</c> whether the
    /// name is defined, reporting this same wording and exit code when it is not. Stopping (rather
    /// than skipping and continuing) is what keeps the host out of a question it cannot answer:
    /// nothing here knows a foreign flag's ARITY, so <c>-port 5000</c>'s value is indistinguishable
    /// from a program argument. Everything from the unrecognized name onward is therefore the
    /// program's, exactly as after a non-flag token — which also means a HOST flag placed after a
    /// package flag is left to <c>flag.Parse()</c> rather than read here. The pipeline places the
    /// host's own flags first, and Go's single parse makes the ordering irrelevant on its side.
    /// </para>
    /// <para>
    /// What is left over is deliberately NOT recorded here. The program reads its own arguments the
    /// way any Go program does — through <c>os.Args</c>, which the converted <c>os</c> package fills
    /// from the real command line independently of this parser, or through <c>flag.Args()</c> after
    /// its own <c>flag.Parse()</c>. The host's whole obligation is to stop, and to leave them
    /// untouched on the way past.
    /// </para>
    /// </remarks>
    public static TestOptions Parse(string[] args)
    {
        TestOptions options = new();

        for (int index = 0; index < args.Length; index++)
        {
            string arg = args[index];

            // parseOne's stopping test, verbatim: a token shorter than two characters, or one that
            // does not open with '-', ends the flag section — it and everything after it are the
            // program's. This is what `-` alone is, too.
            if (arg.Length < 2 || arg[0] != '-')
                break;

            int dashes = 1;

            if (arg[1] == '-')
            {
                dashes = 2;

                // "--" terminates the flags and is itself consumed.
                if (arg.Length == 2)
                    break;
            }

            // Go strips one OR two leading dashes and matches on the NAME, so `--json` and `-json`
            // are one flag — the equivalence TestFlagBridge already relies on when it republishes
            // these same options to the converted flag.CommandLine under their undashed names.
            string name = arg[dashes..];

            if (name.Length == 0 || name[0] is '-' or '=')
                throw new ArgumentException($"bad flag syntax: {arg}");

            string? value = null;
            int equals = name.IndexOf('=');

            if (equals >= 0)
            {
                value = name[(equals + 1)..];
                name = name[..equals];
            }

            switch (name)
            {
                case "json":
                    options.Json = true;
                    // `go test -json` implies -v (cmd/go passes -test.v), so the Go side of the
                    // differential runs with testing.Verbose() == true; mirror it or tests that
                    // gate on Verbose() (sort's countOps) skip here while passing there.
                    options.Verbose = true;
                    break;
                case "v":
                case "test.v":
                    options.Verbose = value is null || bool.Parse(value);
                    break;
                case "short":
                case "test.short":
                    // Backs testing.Short() in the shim; go test's default (flag absent) is false.
                    options.Short = value is null || bool.Parse(value);
                    break;
                case "run":
                case "test.run":
                    value ??= NextValue(args, ref index, name);
                    options.RunPattern = value;
                    options.Filters = value.Split('/').Select(part =>
                        new Regex(part, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1.0D))).ToArray();
                    break;
                case "skip":
                case "test.skip":
                    value ??= NextValue(args, ref index, name);
                    options.SkipPattern = value;
                    options.SkipFilters = value.Split('/').Select(part =>
                        new Regex(part, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1.0D))).ToArray();
                    break;
                case "count":
                case "test.count":
                    value ??= NextValue(args, ref index, name);
                    options.Count = Math.Max(1, int.Parse(value, CultureInfo.InvariantCulture));
                    break;
                case "parallel":
                case "test.parallel":
                    // Caps simultaneously RUNNING parallel tests, like go test's -parallel flag
                    // (whose default is GOMAXPROCS — the processor count matches that default).
                    value ??= NextValue(args, ref index, name);
                    options.Parallel = Math.Max(1, int.Parse(value, CultureInfo.InvariantCulture));
                    break;
                case "shuffle":
                case "test.shuffle":
                    value ??= NextValue(args, ref index, name);
                    options.ShuffleValue = value;
                    options.ShuffleSeed = value == "on"
                        ? Random.Shared.Next()
                        : value == "off" ? null : int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "timeout":
                case "test.timeout":
                    value ??= NextValue(args, ref index, name);
                    options.Timeout = ParseDuration(value);
                    break;
                case "result":
                    value ??= NextValue(args, ref index, name);
                    options.ResultFile = value;
                    break;
                case "junit":
                    value ??= NextValue(args, ref index, name);
                    options.JUnitFile = value;
                    break;
                default:
                    // Not this host's flag — and not yet knowably nobody's. Record it and stop; the
                    // package's own flag set does not exist until it initializes, and TestHost asks
                    // the question there. See the remarks.
                    options.UnrecognizedFlag = name;
                    return options;
            }
        }

        return options;
    }

    public void ResolveOutputPaths(string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(ResultFile) && !Path.IsPathRooted(ResultFile))
            ResultFile = Path.GetFullPath(Path.Combine(baseDirectory, ResultFile));
        if (!string.IsNullOrWhiteSpace(JUnitFile) && !Path.IsPathRooted(JUnitFile))
            JUnitFile = Path.GetFullPath(Path.Combine(baseDirectory, JUnitFile));
    }

    public bool ShouldRun(string fullName) =>
        Matches(Filters, fullName) && !(SkipFilters is not null && Matches(SkipFilters, fullName));

    // ONE matcher for both directions on purpose. -run and -skip differ only in the SIGN of the
    // answer, so sharing the segment walk is what keeps them from drifting -- a -skip matching
    // subtests differently from -run would withdraw a different set on each side of the comparison.
    private static bool Matches(Regex[]? filters, string fullName)
    {
        if (filters is null)
            return true;

        string[] nameParts = fullName.Split('/');
        int count = Math.Min(nameParts.Length, filters.Length);
        for (int i = 0; i < count; i++)
        {
            if (!filters[i].IsMatch(nameParts[i]))
                return false;
        }
        return true;
    }

    // A non-boolean flag whose value did not arrive as `-name=value` takes the NEXT token, whatever
    // it looks like — Go's parseOne does the same, so `-run -v` makes "-v" the pattern rather than a
    // second flag, and the stopping rule never sees it.
    private static string NextValue(string[] args, ref int index, string option)
    {
        if (++index >= args.Length)
            throw new ArgumentException($"flag needs an argument: -{option}");
        return args[index];
    }

    // Go's duration syntax is a SEQUENCE of decimal-and-unit pairs ("30m0s", "1h30m", "1.5s") — which
    // is exactly what time.Duration.String() emits and what `go test -timeout` accepts. A single-pair
    // parser rejected the pipeline's own `-test-timeout` value the moment it was threaded through
    // (`-timeout 30m0s` -> "invalid Go-style duration", exit 2 before a single test ran), so parse the
    // real grammar. Units are Go's: ns, us (µs / μs), ms, s, m, h.
    private const string DurationUnits = "ns|us|\u00B5s|\u03BCs|ms|s|m|h";

    private static TimeSpan ParseDuration(string value)
    {
        if (!Regex.IsMatch(value, $@"^(?:\d+(?:\.\d+)?(?:{DurationUnits}))+$", RegexOptions.CultureInvariant))
            throw new ArgumentException($"invalid Go-style duration: {value}");

        TimeSpan total = TimeSpan.Zero;

        foreach (Match match in Regex.Matches(value, $@"(?<number>\d+(?:\.\d+)?)(?<unit>{DurationUnits})", RegexOptions.CultureInvariant))
        {
            double number = double.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);

            total += match.Groups["unit"].Value switch
            {
                "ns" => TimeSpan.FromTicks((long)(number / 100.0D)),
                "us" or "\u00B5s" or "\u03BCs" => TimeSpan.FromMilliseconds(number / 1000.0D),
                "ms" => TimeSpan.FromMilliseconds(number),
                "s" => TimeSpan.FromSeconds(number),
                "m" => TimeSpan.FromMinutes(number),
                "h" => TimeSpan.FromHours(number),
                _ => throw new ArgumentException($"invalid duration unit: {value}")
            };
        }

        return total;
    }
}
