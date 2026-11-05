// TimingLines.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The timing lines a benchmark prints, read the same way by the in-tree run and by a release leg. Like
// ReleaseLegs.cs it is pure and LINKED into PerformanceRunnerTests, which pins it (TimingLinesTests).
//
// Every benchmark prints its workload time on one `elapsed_ns: <n>` line, which Verify drops before it
// compares the three binaries' output and Measure reads as the row's time.

namespace PerformanceRunner;

internal static class TimingLines
{
    // The row's workload time.
    internal const string Total = "elapsed_ns:";

    // Whether a line is a timing line, which differs run to run and is not compared.
    internal static bool IsTiming(string line) => line.StartsWith(Total, StringComparison.Ordinal);

    // The output with its timing lines removed and its line endings made '\n', as Verify compares it.
    internal static string Filter(string output) =>
        string.Join('\n', output.Replace("\r", "").Split('\n').Where(l => !IsTiming(l)));

    // The row's workload time in milliseconds, from its `elapsed_ns:` line; 0 when it printed none.
    internal static double TotalMs(string stdOut)
    {
        foreach (string line in stdOut.Split('\n'))
        {
            if (!line.StartsWith(Total, StringComparison.Ordinal))
                continue;

            if (long.TryParse(line[Total.Length..].Trim(), out long ns))
                return ns / 1_000_000.0;
        }

        return 0.0;
    }

    // The row's named parts, in the order it printed them.
    internal static IReadOnlyList<(string Name, double Ms)> Parts(string stdOut) => [];
}
