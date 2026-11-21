// KeptArrayDoorCensus.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// THE KEPT-ARRAY DOOR'S ARRIVAL COUNTER (docs/phase4/DESIGN-array-pointer-doors.md 20). Door C opens a native window at
// the place arm 5 refused and arm 2b fell through (ж<T>.FromNumber), and this file counts those arrivals: how many a
// run took, and how many were arm 5 (a number no Go value owns) against arm 2b (a pinned real address of another
// pointee). It is the reading that says whether the corpus's converted-but-inert sites are reached by a program at
// all.
//
// OFF BY DEFAULT AND FREE WHEN OFF: one static bool read per arrival. Enabled by naming a file in
// GO2CS_KEPT_ARRAY_CENSUS_FILE, never by writing to stderr, because a -tests host's stderr is part of what the
// comparison reads. Q44RegistryCensus's two rules are followed: the path is made PER PROCESS (a "{pid}" in it is
// replaced by the process id, and a path without one gets "-<pid>" before its extension), because one shared file
// keeps only the last process's block; and a START block is written at module initialization, so a process that never
// reaches the door still leaves a file reading total=0 and a zero is a measurement, not an absence. The file is
// rewritten whole at the first arrival, every FlushEvery after it and at process exit, so a host that dies keeps its
// last block. The block names the process and the first SampleCount arrivals (number, arm and element type), so an
// arrival can be read for whether its number is a mapped address.

using System;
using System.IO;
using System.Threading;

namespace go;

internal static class KeptArrayDoorCensus
{
    private const long FlushEvery = 4096;

    private const int SampleCount = 8;

    // Read once, so the totals of a run cannot be split by a switch mid-run.
    private static readonly string? s_path = resolvePath(Environment.GetEnvironmentVariable("GO2CS_KEPT_ARRAY_CENSUS_FILE"));

    internal static readonly bool Enabled = s_path is not null;

    private static long s_arm5;
    private static long s_arm2b;
    private static readonly string?[] s_samples = Enabled ? new string?[SampleCount] : [];

    private static string? resolvePath(string? named)
    {
        if (named is not { Length: > 0 })
            return null;

        string pid = Environment.ProcessId.ToString();

        if (named.Contains("{pid}", StringComparison.Ordinal))
            return named.Replace("{pid}", pid);

        string? dir = Path.GetDirectoryName(named);
        string perProcess = $"{Path.GetFileNameWithoutExtension(named)}-{pid}{Path.GetExtension(named)}";

        return string.IsNullOrEmpty(dir) ? perProcess : Path.Combine(dir, perProcess);
    }

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Arm()
    {
        if (!Enabled)
            return;

        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Write();
        Write();
    }

    internal static void NativeArm(bool unowned, nuint address, Type type)
    {
        long arm5 = unowned ? Interlocked.Increment(ref s_arm5) : Interlocked.Read(ref s_arm5);
        long arm2b = unowned ? Interlocked.Read(ref s_arm2b) : Interlocked.Increment(ref s_arm2b);
        long total = arm5 + arm2b;

        if (total <= SampleCount)
            s_samples[total - 1] = $"0x{address:x} {(unowned ? "arm5" : "arm2b")} {type}";

        if (total == 1 || total % FlushEvery == 0)
            Write();
    }

    internal static (long arm5, long arm2b) Snapshot() => (Interlocked.Read(ref s_arm5), Interlocked.Read(ref s_arm2b));

    private static void Write()
    {
        (long arm5, long arm2b) = Snapshot();

        try
        {
            using StreamWriter writer = new(s_path!, append: false);

            writer.WriteLine($"kept-array door native arm: process={Path.GetFileName(Environment.ProcessPath) ?? "?"} total={arm5 + arm2b} arm5={arm5} arm2b={arm2b}");

            foreach (string? sample in s_samples)
            {
                if (sample is not null)
                    writer.WriteLine($"  {sample}");
            }
        }
        catch (IOException)
        {
            // A census that cannot write says nothing; it never changes what the program does.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
