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
// comparison reads. The file is rewritten whole at the first arrival, every FlushEvery after it and at process exit,
// so a host that dies keeps its last block.

using System;
using System.IO;
using System.Threading;

namespace go;

internal static class KeptArrayDoorCensus
{
    private const long FlushEvery = 4096;

    // Read once, so the totals of a run cannot be split by a switch mid-run.
    private static readonly string? s_path = Environment.GetEnvironmentVariable("GO2CS_KEPT_ARRAY_CENSUS_FILE") is { Length: > 0 } path ? path : null;

    internal static readonly bool Enabled = s_path is not null;

    private static long s_arm5;
    private static long s_arm2b;
    private static int s_hooked;

    internal static void NativeArm(bool unowned)
    {
        long arm5 = unowned ? Interlocked.Increment(ref s_arm5) : Interlocked.Read(ref s_arm5);
        long arm2b = unowned ? Interlocked.Read(ref s_arm2b) : Interlocked.Increment(ref s_arm2b);
        long total = arm5 + arm2b;

        if (Interlocked.Exchange(ref s_hooked, 1) == 0)
            AppDomain.CurrentDomain.ProcessExit += static (_, _) => Write();

        if (total == 1 || total % FlushEvery == 0)
            Write();
    }

    internal static (long arm5, long arm2b) Snapshot() => (Interlocked.Read(ref s_arm5), Interlocked.Read(ref s_arm2b));

    private static void Write()
    {
        (long arm5, long arm2b) = Snapshot();

        try
        {
            File.WriteAllText(s_path!, $"kept-array door native arm: total={arm5 + arm2b} arm5={arm5} arm2b={arm2b}{Environment.NewLine}");
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
