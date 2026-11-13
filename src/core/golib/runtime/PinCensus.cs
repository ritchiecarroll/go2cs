// A MEASUREMENT BUILD ONLY: the registration census of DESIGN-cgo-phase3-marshaling section 17 (QA-4). It never lands.
// Off unless GO2CS_PIN_CENSUS names a directory. Off, each counted site pays one static read.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

public static class PinCensus
{
    private static readonly string? s_directory = Environment.GetEnvironmentVariable("GO2CS_PIN_CENSUS");

    public static readonly bool Enabled = !string.IsNullOrEmpty(s_directory);

    private static long s_pinbuf, s_reint, s_entries, s_exit, s_exitNew, s_add;
    private static readonly ConditionalWeakTable<object, object> s_exited = new();
    private static readonly object s_writeLock = new();

    static PinCensus()
    {
        if (Enabled)
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Write("total " + Format(Read()));
    }

    public static void PinnedBuffer() => Interlocked.Increment(ref s_pinbuf);

    public static void PinnedReinterpret() => Interlocked.Increment(ref s_reint);

    public static void NewEntry() => Interlocked.Increment(ref s_entries);

    // unsafe.Add over an unsafe.Pointer that carries its box (finding 4 of the review of increment 3.1).
    public static void AddCarrying() => Interlocked.Increment(ref s_add);

    // A pin-holding native box leaving through the operator's native arm. `missed` says the exact read did not know
    // its address; the first such exit of a box is what registering at the exit would add.
    public static void Exit(object box, bool missed)
    {
        Interlocked.Increment(ref s_exit);

        if (!missed || s_exited.TryGetValue(box, out _))
            return;

        s_exited.AddOrUpdate(box, box);
        Interlocked.Increment(ref s_exitNew);
    }

    public static (long pinbuf, long reint, long entries, long exit, long exitNew, long add) Read() =>
        (Interlocked.Read(ref s_pinbuf), Interlocked.Read(ref s_reint), Interlocked.Read(ref s_entries),
         Interlocked.Read(ref s_exit), Interlocked.Read(ref s_exitNew), Interlocked.Read(ref s_add));

    public static string Format((long pinbuf, long reint, long entries, long exit, long exitNew, long add) v) =>
        $"pinbuf={v.pinbuf} reint={v.reint} entries={v.entries} exit={v.exit} exitnew={v.exitNew} add={v.add}";

    public static string Delta((long pinbuf, long reint, long entries, long exit, long exitNew, long add) a,
                               (long pinbuf, long reint, long entries, long exit, long exitNew, long add) b) =>
        Format((b.pinbuf - a.pinbuf, b.reint - a.reint, b.entries - a.entries, b.exit - a.exit, b.exitNew - a.exitNew, b.add - a.add));

    public static void Write(string line)
    {
        try
        {
            lock (s_writeLock)
                File.AppendAllText(Path.Combine(s_directory!, $"census-{Environment.ProcessId}.txt"), line + "\n");
        }
        catch (Exception)
        {
            // A census that cannot write must not change a verdict.
        }
    }
}
