// READ-ONLY MEASUREMENT (ledger 2026-09-28 15:03 (3)): does a managed exception raised inside a
// Windows callback propagate through EnumTimeFormatsEx's native frames via the reverse-P/Invoke shim,
// or does the process fail fast? The shim shape is syscall/windows/syscall_windows_callback_impl.cs's
// exactly: a NON-generic [UnmanagedFunctionPointer(Winapi)] delegate returning a native word, marshalled
// with Marshal.GetFunctionPointerForDelegate, with NO catch of its own. Go's callback has 2 arguments
// (timeFormatString, lparam), so this is GoCallbackShim2's shape.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

sealed class GoLikePanic(string value) : Exception(value);

static class Probe
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nuint Shim2(nuint a1, nuint a2);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern int EnumTimeFormatsEx(nint lpTimeFmtEnumProcEx, nint lpLocaleName, uint dwFlags, nint lParam);

    private static readonly Shim2 s_shim = (a1, a2) => { s_calls++; throw new GoLikePanic("callback panic"); };
    private static readonly nint s_code = Marshal.GetFunctionPointerForDelegate(s_shim);
    private static int s_calls;

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static nuint UcoCallback(nuint a1, nuint a2) => throw new GoLikePanic("callback panic");

    // One nestedCall: returns what the caller observed.
    private static string NestedCall()
    {
        bool finallyRan = false;
        try
        {
            try
            {
                int rc = EnumTimeFormatsEx(s_code, 0, 0, 0);
                return $"RETURNED rc={rc} (no exception reached the caller)";
            }
            finally
            {
                finallyRan = true;
            }
        }
        catch (GoLikePanic p)
        {
            return $"CAUGHT {p.GetType().Name}(\"{p.Message}\") finallyRan={finallyRan}";
        }
        catch (Exception e)
        {
            return $"CAUGHT-OTHER {e.GetType().FullName}: {e.Message} finallyRan={finallyRan}";
        }
    }

    public static int Main(string[] args)
    {
        string arm = args.Length > 0 ? args[0] : "driver";

        switch (arm)
        {
            case "main":
                Console.WriteLine("main-thread: " + NestedCall() + $" calls={s_calls}");
                return 0;
            case "thread":
            {
                string result = "";
                var t = new Thread(() => result = NestedCall());
                t.Start(); t.Join();
                Console.WriteLine("new-thread: " + result + $" calls={s_calls}");
                return 0;
            }
            case "uco":
            {
                // CONTROL: the same throw from an [UnmanagedCallersOnly] entry, where the CLR's contract is
                // FAIL FAST. If this arm does not die, the driver cannot see a fail-fast and the other arms
                // prove nothing.
                unsafe
                {
                    delegate* unmanaged[Stdcall]<nuint, nuint, nuint> fp = &UcoCallback;
                    bool caught = false;
                    try { EnumTimeFormatsEx((nint)fp, 0, 0, 0); }
                    catch (Exception) { caught = true; }
                    Console.WriteLine($"uco: returned to caller, caught={caught} (the control did NOT fail fast)");
                }
                return 0;
            }
            case "failfast":
            {
                // CONTROL of the OBSERVATION CHANNEL: a certain fail-fast from inside the callback. The
                // driver must report a non-zero exit and the fail-fast text here, or it cannot see one.
                Shim2 ff = (a1, a2) => { Environment.FailFast("probe control: fail-fast inside the callback"); return 0; };
                nint code = Marshal.GetFunctionPointerForDelegate(ff);
                EnumTimeFormatsEx(code, 0, 0, 0);
                GC.KeepAlive(ff);
                Console.WriteLine("failfast: returned to caller (the observation channel is BROKEN)");
                return 0;
            }
            case "loop":
            {
                var sw = Stopwatch.StartNew();
                string last = "";
                int caught = 0;
                for (int i = 0; i < 100000; i++)
                {
                    last = NestedCall();
                    if (last.StartsWith("CAUGHT GoLikePanic", StringComparison.Ordinal)) caught++;
                }
                Console.WriteLine($"loop-100000: caught={caught} last={last} calls={s_calls} ms={sw.ElapsedMilliseconds} ws={Environment.WorkingSet / (1 << 20)}MB");
                return 0;
            }
        }

        // DRIVER: each arm in its own child process, so a fail-fast is observed rather than suffered.
        string self = Environment.ProcessPath!;
        foreach (string a in new[] { "failfast", "uco", "main", "thread", "loop" })
        {
            var psi = new ProcessStartInfo(self, a) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            using var p = Process.Start(psi)!;
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine($"[{a}] exit=0x{p.ExitCode:X8}");
            if (stdout.Length > 0) Console.WriteLine("  out: " + stdout.Trim());
            if (stderr.Length > 0) Console.WriteLine("  err: " + string.Join("\n       ", stderr.Trim().Split('\n')[..Math.Min(6, stderr.Trim().Split('\n').Length)]));
        }
        return 0;
    }
}
