// MnWaitSense: the probe behind DESIGN-mn-n5-invisible-blocking.md section 2. Run: dotnet run -c Release.
// N5 probe: does Thread.ThreadState, read from ANOTHER thread, report WaitSleepJoin for a thread blocked in each
// kind of wait a carrier could be stuck in, and Running for a native block outside the runtime's knowledge or a
// spin? And what does one read cost? Prints one line per case: name, state read at +50 ms, state at +200 ms.
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

static class Probe
{
    [DllImport("kernel32.dll", EntryPoint = "Sleep")] static extern void WinSleep(uint ms);
    [DllImport("libc", EntryPoint = "usleep")] static extern int usleep(uint us);
    [DllImport("libc", EntryPoint = "read")] static extern nint read(int fd, byte[] buf, nint count);
    [DllImport("libc", EntryPoint = "pipe")] static extern int pipe(int[] fds);

    static readonly ManualResetEventSlim Release = new(false);
    static volatile bool s_stop;

    static void NativeSleep(int ms)
    {
        if (OperatingSystem.IsWindows()) WinSleep((uint)ms); else usleep((uint)ms * 1000);
    }

    static void Main()
    {
        object monitorLock = new();
        var gate = new Lock();
        var sem = new SemaphoreSlim(0);
        var mres = new ManualResetEventSlim(false);
        var mre = new ManualResetEvent(false);
        var tcs = new TaskCompletionSource();
        var server = new AnonymousPipeServerStream(PipeDirection.Out);
        var client = new AnonymousPipeClientStream(PipeDirection.In, server.ClientSafePipeHandle);
        int[] fds = new int[2];
        if (!OperatingSystem.IsWindows()) pipe(fds);

        var cases = new (string Name, Action Body)[]
        {
            ("Thread.Sleep", () => Thread.Sleep(400)),
            ("Monitor.Enter contended", () => { lock (monitorLock) { } }),
            ("Monitor.Wait", () => { lock (monitorLock) { } }), // replaced below
            ("Lock.Enter contended", () => { lock (gate) { } }),
            ("SemaphoreSlim.Wait", () => sem.Wait(400)),
            ("ManualResetEventSlim.Wait", () => mres.Wait(400)),
            ("ManualResetEvent.WaitOne", () => mre.WaitOne(400)),
            ("Task.Wait", () => tcs.Task.Wait(400)),
            ("pipe FileStream.Read", () => { var b = new byte[1]; client.Read(b, 0, 1); }),
            ("native sleep P/Invoke", () => NativeSleep(400)),
            ("native read(2) on a pipe", () => { if (!OperatingSystem.IsWindows()) read(fds[0], new byte[1], 1); else NativeSleep(400); }),
            ("spin", () => { var sw = Stopwatch.StartNew(); while (sw.ElapsedMilliseconds < 400 && !s_stop) { } }),
        };
        cases[2] = ("Monitor.Wait", () => { lock (monitorLock) Monitor.Wait(monitorLock, 400); });

        foreach (var (name, body) in cases)
        {
            bool holdMonitor = name == "Monitor.Enter contended";
            bool holdLock = name == "Lock.Enter contended";
            if (holdMonitor) Monitor.Enter(monitorLock);
            if (holdLock) gate.Enter();

            var t = new Thread(() => body()) { IsBackground = true };
            t.Start();
            Thread.Sleep(50);
            var s1 = t.ThreadState;
            Thread.Sleep(150);
            var s2 = t.ThreadState;

            if (holdMonitor) Monitor.Exit(monitorLock);
            if (holdLock) gate.Exit();
            if (name == "pipe FileStream.Read") server.WriteByte(1);
            if (name.StartsWith("native read") && !OperatingSystem.IsWindows()) { }
            t.Join(1000);
            Console.WriteLine($"{name,-28} +50ms={s1,-14} +200ms={s2}");
        }

        // Cost of one cross-thread read of a running thread's state.
        s_stop = false;
        var spinner = new Thread(() => { while (!s_stop) { } }) { IsBackground = true };
        spinner.Start();
        Thread.Sleep(20);
        const int N = 2_000_000;
        long acc = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < N; i++) acc += (int)spinner.ThreadState;
        sw.Stop();
        s_stop = true;
        Console.WriteLine($"cost: {sw.Elapsed.TotalNanoseconds / N:F1} ns per ThreadState read (acc {acc & 1}) os={RuntimeInformation.OSDescription}");
    }
}
