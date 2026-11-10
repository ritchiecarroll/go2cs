// ChildTimeoutDiagnostics.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Runtime;

namespace GolibTests;

/// <summary>
/// What a test prints when a child process it launched does not finish in time: where every .NET process in
/// the child's tree is parked, then the output the child wrote before the kill.
/// </summary>
/// <remarks>
/// <para>
/// A timed-out child that is killed and whose output is dropped leaves nothing to root the hang from. The
/// linux-arm64 CallerLine runs (42740891770) are the case: two converted programs did not finish in 120 s, and
/// the runs kept no frame and no output.
/// </para>
/// <para>
/// THE STACKS come from a dump the child's own runtime writes: <see cref="DiagnosticsClient.WriteDump"/> asks it
/// over the diagnostics port, and the runtime launches createdump with itself as the permitted tracer. An
/// external createdump launched from here would be the child's sibling, not its ancestor, and a host that
/// limits ptrace to ancestors (Yama scope 1, the Ubuntu default) refuses it. ClrMD then reads the dump file
/// and names each managed thread's frames.
/// </para>
/// <para>
/// THE TREE: the process that hangs is not always the direct child. `dotnet test` runs the tests in a
/// testhost grandchild, and `dotnet build` in worker nodes. So every .NET process at or under the child is
/// described. On Windows only the child itself is, because the parent link is not read there.
/// </para>
/// <para>
/// Describing never throws and never replaces the timeout: each step that fails says why in the text, and
/// the caller still fails the test.
/// </para>
/// </remarks>
internal static class ChildTimeoutDiagnostics
{
    // Frames printed per thread, and threads per process: enough to name where a thread is parked, small
    // enough for a test log.
    private const int MaxFrames = 40;
    private const int MaxThreads = 64;

    // How long the dump of one process may take before it is abandoned.
    private static readonly TimeSpan DumpTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Describes a child that did not finish in time, then kills its tree: the stacks of every .NET process at
    /// or under it, then its partial stdout and stderr. Call it in place of a bare Kill.
    /// </summary>
    public static string DescribeAndKill(Process process, Task<string> stdout, Task<string> stderr)
    {
        StringBuilder text = new();
        int pid = process.Id;

        try
        {
            foreach (int target in DotnetProcessesInTree(pid))
                AppendStacks(text, target);
        }
        catch (Exception ex)
        {
            text.AppendLine($"[stacks unavailable: {ex.GetType().Name}: {ex.Message}]");
        }

        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(10_000);
        }
        catch (Exception ex)
        {
            text.AppendLine($"[kill: {ex.GetType().Name}: {ex.Message}]");
        }

        text.AppendLine("--- child stdout before the kill ---");
        text.AppendLine(Drain(stdout));
        text.AppendLine("--- child stderr before the kill ---");
        text.AppendLine(Drain(stderr));

        return text.ToString();
    }

    // The output read so far. Killing the tree closes the pipes, so the reads finish; a reader still blocked
    // after that (a descendant that escaped the kill holds the pipe) is reported rather than waited on.
    private static string Drain(Task<string> read) =>
        read.Wait(TimeSpan.FromSeconds(10)) ? read.Result : "[the stream was still open 10 s after the kill]";

    // pid and its descendants that publish a .NET diagnostics port, pid first. The port is found the way
    // DiagnosticsClient finds it, by its socket in the temp directory, one pid at a time:
    // DiagnosticsClient.GetPublishedProcesses throws on the first entry it cannot read, and one stale entry
    // would hide the whole tree. On Windows the port is a named pipe, so the child is tried as it is.
    private static List<int> DotnetProcessesInTree(int pid)
    {
        if (OperatingSystem.IsWindows())
            return [pid];

        return Descendants(pid).Prepend(pid).Where(HasDiagnosticsPort).ToList();
    }

    private static bool HasDiagnosticsPort(int pid)
    {
        try
        {
            return Directory.GetFiles(Path.GetTempPath(), $"dotnet-diagnostic-{pid}-*-socket").Length > 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    // Every descendant of pid, read from `ps` (Linux and macOS). Windows reports none.
    private static IEnumerable<int> Descendants(int pid)
    {
        if (OperatingSystem.IsWindows())
            return [];

        ProcessStartInfo start = new("ps", ["-A", "-o", "pid=,ppid="])
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using Process ps = Process.Start(start)!;
        string listing = ps.StandardOutput.ReadToEnd();
        ps.WaitForExit();

        Dictionary<int, List<int>> children = [];

        foreach (string line in listing.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length == 2 && int.TryParse(fields[0], out int child) && int.TryParse(fields[1], out int parent))
            {
                if (!children.TryGetValue(parent, out List<int>? list))
                    children[parent] = list = [];

                list.Add(child);
            }
        }

        List<int> found = [];
        Queue<int> pending = new([pid]);

        while (pending.Count > 0)
        {
            if (!children.TryGetValue(pending.Dequeue(), out List<int>? list))
                continue;

            foreach (int child in list)
            {
                found.Add(child);
                pending.Enqueue(child);
            }
        }

        return found;
    }

    // ClrMD's own DAC lookup can miss a DAC that is on disk beside the runtime the dump names (measured:
    // "Could not find matching DAC for this runtime" with libmscordaccore.so present in that directory). The
    // child ran on this machine, so that file is the right one: it is used when the lookup fails.
    private static ClrRuntime CreateRuntime(ClrInfo clr)
    {
        try
        {
            return clr.CreateRuntime();
        }
        catch (FileNotFoundException)
        {
            string dac = OperatingSystem.IsWindows() ? "mscordaccore.dll" : OperatingSystem.IsMacOS() ? "libmscordaccore.dylib" : "libmscordaccore.so";
            string beside = Path.Combine(Path.GetDirectoryName(clr.ModuleInfo.FileName) ?? "", dac);

            if (!File.Exists(beside))
                throw;

            return clr.CreateRuntime(beside, ignoreMismatch: true);
        }
    }

    private static void AppendStacks(StringBuilder text, int pid)
    {
        string dump = Path.Combine(Path.GetTempPath(), $"g2cs-child-{pid}-{Guid.NewGuid():N}.dmp");
        text.AppendLine($"--- .NET process {pid}: managed stacks at the timeout ---");

        try
        {
            Task write = Task.Run(() => new DiagnosticsClient(pid).WriteDump(DumpType.WithHeap, dump, logDumpGeneration: false));

            if (!write.Wait(DumpTimeout))
            {
                text.AppendLine($"[the dump did not finish in {DumpTimeout.TotalSeconds} s]");
                return;
            }

            using DataTarget target = DataTarget.LoadDump(dump);

            if (target.ClrVersions.Length == 0)
            {
                text.AppendLine("[the dump holds no CLR]");
                return;
            }

            using ClrRuntime runtime = CreateRuntime(target.ClrVersions[0]);
            int shown = 0;

            foreach (ClrThread thread in runtime.Threads)
            {
                if (!thread.IsAlive)
                    continue;

                List<string> frames = thread.EnumerateStackTrace().Take(MaxFrames).Select(f => f.ToString() ?? f.Kind.ToString()).ToList();

                if (frames.Count == 0)
                    continue;

                if (++shown > MaxThreads)
                {
                    text.AppendLine($"[more threads than {MaxThreads}; the rest are not shown]");
                    break;
                }

                text.AppendLine($"thread os={thread.OSThreadId} managed={thread.ManagedThreadId}:");

                foreach (string frame in frames)
                    text.AppendLine("    " + frame);
            }

            if (shown == 0)
                text.AppendLine("[no live thread has a managed frame]");
        }
        catch (Exception ex)
        {
            text.AppendLine($"[stacks unavailable: {ex.GetType().Name}: {ex.Message}]");
        }
        finally
        {
            try
            {
                File.Delete(dump);
            }
            catch (IOException)
            {
            }
        }
    }
}
