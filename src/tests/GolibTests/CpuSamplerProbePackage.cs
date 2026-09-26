// A Go package stand-in for EventPipeSamplerTests' frames arm.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace go;

// A Go package stand-in for the frames arm: runtime counts a frame as Go when its top-level class
// is `<pkg>_package` in namespace go, and GoSyntheticPC names it `cpusamplerprobe.cpuHog1`.
internal static class cpusamplerprobe_package
{
    private static ulong s_salt;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ulong cpuHog1(ulong x)
    {
        ulong f = x;

        for (int i = 0; i < 100000; i++)
            f = f % 2 == 0 ? f * 3 + 1 : f / 2 + 7;

        return f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void cpuHogger(int milliseconds)
    {
        var clock = Stopwatch.StartNew();
        ulong salt = 1;

        while (clock.ElapsedMilliseconds < milliseconds)
            salt += cpuHog1(salt);

        s_salt += salt;
    }
}
