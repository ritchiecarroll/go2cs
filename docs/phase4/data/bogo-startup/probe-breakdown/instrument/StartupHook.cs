// P1 probe start-up instrument: a DOTNET_STARTUP_HOOKS assembly. Records, in process and without packages:
//  - the exec-to-hook gap (P1_T0, ns since the epoch, set by the wrapper just before exec);
//  - the runtime provider's AssemblyLoader (0x4), Loader (0x8) and JIT (0x10) events, verbose;
//  - AppDomain.AssemblyLoad, and the ProcessExit time.
// Written at ProcessExit to $P1HOOK_OUT as TSV: kind, t_us (since exec), thread, fields...
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;
using System.Threading;

internal class StartupHook
{
    internal static readonly ConcurrentQueue<string> Rows = new();
    internal static long T0Ns;          // exec time, ns since epoch
    internal static long SyncSw;        // Stopwatch at sync
    internal static long SyncNs;        // epoch ns at sync
    static Listener listener;

    internal static double UsFromSw(long sw) =>
        (SyncNs - T0Ns) / 1000.0 + (sw - SyncSw) * 1e6 / Stopwatch.Frequency;
    internal static double UsFromDt(DateTime dt) =>
        ((dt.ToUniversalTime() - DateTime.UnixEpoch).Ticks * 100L - T0Ns) / 1000.0;

    static int probed;
    static void TryProbe(System.Reflection.Assembly a)
    {
        if (probed != 0 || a.GetName().Name != "golib" || Environment.GetEnvironmentVariable("P1HOOK_SCAN") != "1") return;
        foreach (var t in a.GetTypes())
        {
            var fld = t.GetField("ScanProbeForTest", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (fld == null) continue;
            Action<System.Reflection.Assembly> d = asm => Rows.Enqueue($"SCAN\t{UsFromSw(Stopwatch.GetTimestamp()):F1}\t{Environment.CurrentManagedThreadId}\t{asm.GetName().Name}");
            fld.SetValue(null, d); probed = 1;
            Rows.Enqueue($"SCANHOOK\t{UsFromSw(Stopwatch.GetTimestamp()):F1}\t{Environment.CurrentManagedThreadId}\t{t.FullName}");
            return;
        }
    }

    public static void Initialize()
    {
        var path = Environment.GetEnvironmentVariable("P1HOOK_OUT");
        if (string.IsNullOrEmpty(path)) return;
        SyncSw = Stopwatch.GetTimestamp();
        SyncNs = (DateTime.UtcNow - DateTime.UnixEpoch).Ticks * 100L;
        long.TryParse(Environment.GetEnvironmentVariable("P1_T0"), out T0Ns);
        if (T0Ns == 0) T0Ns = SyncNs;
        Rows.Enqueue($"SYNC\t{SyncSw}\t{SyncNs}\t{T0Ns}\t{Stopwatch.Frequency}");
        Rows.Enqueue($"HOOK\t{UsFromSw(SyncSw):F1}\t{Environment.CurrentManagedThreadId}");
        AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
        {
            Rows.Enqueue($"ASM\t{UsFromSw(Stopwatch.GetTimestamp()):F1}\t{Environment.CurrentManagedThreadId}\t{e.LoadedAssembly.GetName().Name}");
            TryProbe(e.LoadedAssembly);
        };
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) TryProbe(a);
        if (Environment.GetEnvironmentVariable("P1HOOK_EVENTS") != "0")
            listener = new Listener();
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            long exitSw = Stopwatch.GetTimestamp();
            Rows.Enqueue($"EXIT\t{UsFromSw(exitSw):F1}\t{Environment.CurrentManagedThreadId}");
            if (listener != null) { Thread.Sleep(400); listener.Dispose(); }
            var sb = new StringBuilder();
            foreach (var r in Rows) sb.Append(r).Append('\n');
            File.WriteAllText(path, sb.ToString());
        };
    }
}

internal sealed class Listener : EventListener
{
    protected override void OnEventSourceCreated(EventSource es)
    {
        if (es.Name == "Microsoft-Windows-DotNETRuntime")
            EnableEvents(es, EventLevel.Verbose, (EventKeywords)Convert.ToInt64(Environment.GetEnvironmentVariable("P1HOOK_KW") ?? "0x1c", 16));
    }

    protected override void OnEventWritten(EventWrittenEventArgs e)
    {
        string f(string n)
        {
            if (e.PayloadNames == null) return "";
            int i = e.PayloadNames.IndexOf(n);
            return i < 0 ? "" : Convert.ToString(e.Payload[i])?.Replace('\t', ' ').Replace('\n', ' ');
        }
        string extra = e.EventId switch
        {
            290 => $"{f("AssemblyName")}\t{f("RequestingAssembly")}",
            291 => $"{f("AssemblyName")}\t{f("Success")}\t{f("Cached")}",
            143 or 144 => $"{f("MethodNamespace")}\t{f("MethodName")}\t{f("MethodFlags")}\t{f("MethodID")}",
            145 => $"{f("MethodNamespace")}\t{f("MethodName")}\t{f("MethodID")}",
            152 or 151 => $"{f("ModuleILPath")}\t{f("ModuleFlags")}",
            154 => $"{f("FullyQualifiedAssemblyName")}",
            73 => $"{f("TypeLoadStartID")}",
            74 => $"{f("TypeLoadStartID")}\t{f("TypeName")}\t{f("LoadLevel")}",
            _ => ""
        };
        StartupHook.Rows.Enqueue($"EV{e.EventId}\t{StartupHook.UsFromDt(e.TimeStamp):F1}\t{e.OSThreadId}\t{e.EventName}\t{extra}");
    }
}
