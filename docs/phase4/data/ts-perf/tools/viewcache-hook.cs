// Startup hook for one experiment: P4_VIEWCACHE_OFF=1 sets golib's FieldViewCache.Disabled (views minted per call,
// not retained per box). Measurement only.
using System;
using System.Linq;
using System.Reflection;
internal class StartupHook
{
    public static void Initialize()
    {
        if (Environment.GetEnvironmentVariable("P4_VIEWCACHE_OFF") != "1") return;
        Assembly golib = Assembly.Load("golib");
        Type t = golib.GetType("go.FieldViewCache", throwOnError: true)!;
        t.GetField("Disabled")!.SetValue(null, true);
        Console.Error.WriteLine("P4 hook: FieldViewCache.Disabled = true");
    }
}
