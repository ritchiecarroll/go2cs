// DarwinThreadCpu.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// darwin's per-thread CPU time for the sampler's magnitude piece, keyed by the id EventPipe stamps on a sample.
//
// EventPipeSampler keys its per-thread CPU readings and its samples on one id and takes it to be the OS thread id.
// On linux and windows System.Diagnostics.ProcessThread answers in that id. On macOS it does not, and its times do
// not hold either. Measured on the hosted runners (EventPipeSamplerHostProbe, run 42595674585, osx-arm64): two hog
// threads whose pthread_threadid_np is 19734 and 19735, each 842-846 ms on a CPU by thread_info, were absent from the
// ProcessThread readings. EventPipe wrote their samples under exactly 19734 and 19735. ProcessThread named threads by
// ten-digit ids and read the two busiest at 81 s apiece inside a 1.75 s session, with the process at 2.3 s of CPU.
// Every thread then looked like a non-Go thread, its absurd CPU time was charged to _ExternalCode, and the profile
// carried 17,703 samples where linux carries about 200.
//
// So on macOS the threads are enumerated through mach, the documented per-thread APIs golib's ThreadCpuTime already
// reads: task_threads on the task's own port, then per thread THREAD_IDENTIFIER_INFO for thread_id (the
// system-wide 64-bit id pthread_threadid_np answers, and EventPipe's ThreadID) and THREAD_BASIC_INFO for user plus
// system time, microsecond resolution. Each thread port and the array are given back. Every failure reads as no
// reading, and the sampler then keeps its wall-time grid.

namespace go.CpuProfiler;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal static class DarwinThreadCpu
{
    private const string LibSystem = "/usr/lib/libSystem.B.dylib";

    private const int ThreadBasicInfoFlavor = 3;        // THREAD_BASIC_INFO
    private const uint ThreadBasicInfoCount = 10;       // sizeof(thread_basic_info_data_t) / sizeof(natural_t)
    private const int ThreadIdentifierInfoFlavor = 4;   // THREAD_IDENTIFIER_INFO
    private const uint ThreadIdentifierInfoCount = 6;   // sizeof(thread_identifier_info_data_t) / sizeof(natural_t)

    private static uint s_taskSelf;

    /// <summary>
    /// Every thread of this process as (EventPipe's ThreadID, CPU time used), or false when the enumeration itself
    /// failed. A thread whose own reads fail is left out.
    /// </summary>
    internal static bool TryRead(List<(int Thread, TimeSpan Cpu)> readings)
    {
        try
        {
            uint task = TaskSelf();

            if (task == 0 || task_threads(task, out nint list, out uint count) != 0)
                return false;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    uint port = unchecked((uint)Marshal.ReadInt32(list, i * sizeof(uint)));

                    try
                    {
                        uint idCount = ThreadIdentifierInfoCount;
                        uint basicCount = ThreadBasicInfoCount;

                        if (thread_info(port, ThreadIdentifierInfoFlavor, out ThreadIdentifierInfo identity, ref idCount) != 0 ||
                            thread_info(port, ThreadBasicInfoFlavor, out ThreadBasicInfo times, ref basicCount) != 0)
                        {
                            continue;
                        }

                        long micros = ((long)times.UserSeconds + times.SystemSeconds) * 1_000_000 + times.UserMicroseconds + times.SystemMicroseconds;
                        readings.Add((unchecked((int)identity.ThreadId), TimeSpan.FromTicks(micros * 10)));
                    }
                    finally
                    {
                        mach_port_deallocate(task, port);
                    }
                }
            }
            finally
            {
                vm_deallocate(task, (nuint)list, (nuint)(count * sizeof(uint)));
            }

            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return false;
        }
    }

    // mach_task_self() is a macro over libSystem's mach_task_self_ global, read once, as Go's runtime reads it.
    private static uint TaskSelf()
    {
        if (s_taskSelf != 0)
            return s_taskSelf;

        if (!NativeLibrary.TryLoad(LibSystem, out nint library) || !NativeLibrary.TryGetExport(library, "mach_task_self_", out nint address))
            return 0;

        return s_taskSelf = unchecked((uint)Marshal.ReadInt32(address));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ThreadIdentifierInfo
    {
        public ulong ThreadId;
        public ulong ThreadHandle;
        public ulong DispatchQueueAddress;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ThreadBasicInfo
    {
        public int UserSeconds;
        public int UserMicroseconds;
        public int SystemSeconds;
        public int SystemMicroseconds;
        public int CpuUsage;
        public int Policy;
        public int RunState;
        public int Flags;
        public int SuspendCount;
        public int SleepTime;
    }

    [DllImport(LibSystem)]
    private static extern int task_threads(uint task, out nint threads, out uint count);

    [DllImport(LibSystem)]
    private static extern int thread_info(uint thread, int flavor, out ThreadIdentifierInfo info, ref uint count);

    [DllImport(LibSystem)]
    private static extern int thread_info(uint thread, int flavor, out ThreadBasicInfo info, ref uint count);

    [DllImport(LibSystem)]
    private static extern int mach_port_deallocate(uint task, uint name);

    [DllImport(LibSystem)]
    private static extern int vm_deallocate(uint task, nuint address, nuint size);
}
