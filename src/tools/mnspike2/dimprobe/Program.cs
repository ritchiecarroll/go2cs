// dimprobe: R5b's K2 probe. Can a hidden async twin be ADDED to a public converted interface without breaking a
// C# type that implements the interface by hand?
//
// The shape is io.Writer's: `partial interface Writer { (nint n, error err) Write(slice<byte> p); }`. The twin
// generator would add a second member to the same partial interface. A hand-written C# implementer knows nothing
// about it, so the member carries a default body that runs the synchronous method.
//
// Arms:
//   hand class      implements Write only            -> the twin call must reach the default body
//   hand struct     implements Write only            -> same, boxed through the interface and under a constraint
//   converted type  implements Write and the twin    -> the twin call must reach the type's own twin
//   old converted   implements Write only (a package built before twins existed) -> the default body
// Each arm is called three ways: through the interface, through a generic constrained to the interface, and
// through a delegate bound to the interface member.
using System;
using System.Threading.Tasks;

namespace dimprobe;

public partial interface Writer
{
    (nint n, object? err) Write(byte[] p);
}

// What the twin generator would add, in the generated half of the partial interface.
public partial interface Writer
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    ValueTask<(nint n, object? err)> Writeꓽasync(byte[] p) => new(Write(p));
}

sealed class HandClass : Writer
{
    public int Calls;
    public (nint n, object? err) Write(byte[] p) { Calls++; return (p.Length, "sync:class"); }
}

struct HandStruct : Writer
{
    public (nint n, object? err) Write(byte[] p) => (p.Length, "sync:struct");
}

struct Converted : Writer
{
    public (nint n, object? err) Write(byte[] p) => (p.Length, "sync:converted");

    public async ValueTask<(nint n, object? err)> Writeꓽasync(byte[] p)
    {
        await Task.Yield();     // a real park: the continuation runs elsewhere
        return (p.Length, "twin:converted");
    }
}

struct OldConverted : Writer
{
    public (nint n, object? err) Write(byte[] p) => (p.Length, "sync:old");
}

static class Program
{
    static int s_failed;

    static void Check(string name, object? got, string want)
    {
        bool ok = (got as string) == want;

        if (!ok)
            s_failed++;

        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {got} (want {want})");
    }

    static async Task<object?> ViaInterface(Writer w) => (await w.Writeꓽasync(new byte[3])).err;

    static async Task<object?> ViaConstraint<T>(T w) where T : Writer => (await w.Writeꓽasync(new byte[3])).err;

    static async Task<object?> ViaDelegate(Writer w)
    {
        Func<byte[], ValueTask<(nint n, object? err)>> f = w.Writeꓽasync;
        return (await f(new byte[3])).err;
    }

    static async Task Arm<T>(string name, T w, string want) where T : Writer
    {
        Check($"{name} via interface", await ViaInterface(w), want);
        Check($"{name} via constraint", await ViaConstraint(w), want);
        Check($"{name} via delegate", await ViaDelegate(w), want);
    }

    static async Task<int> Main()
    {
        await Arm("hand class", new HandClass(), "sync:class");
        await Arm("hand struct", new HandStruct(), "sync:struct");
        // DIMPROBE_RED=1 expects the default body where the type's own twin must run: the probe's red control.
        await Arm("converted", new Converted(), Environment.GetEnvironmentVariable("DIMPROBE_RED") == "1" ? "sync:converted" : "twin:converted");
        await Arm("old converted", new OldConverted(), "sync:old");

        // The default body must not be reachable as a member of the implementing type: a consumer's own surface
        // gains nothing.
        Check("hand class surface", typeof(HandClass).GetMethod("Writeꓽasync") is null ? "absent" : "present", "absent");

        // The synchronous path allocates nothing extra when the default body completes at once.
        HandClass counted = new();
        Writer iface = counted;
        byte[] buffer = new byte[3];

        for (int i = 0; i < 1000; i++)
            _ = iface.Writeꓽasync(buffer).Result;

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 100000; i++)
            _ = iface.Writeꓽasync(buffer).Result;

        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Console.WriteLine($"READING default body, completed at once: {bytes} bytes over 100000 calls ({counted.Calls} sync calls)");
        Console.WriteLine($"READING runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}, dynamic code {System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}");
        Console.WriteLine(s_failed == 0 ? "PASS" : $"FAILED {s_failed}");
        return s_failed == 0 ? 0 : 1;
    }
}
