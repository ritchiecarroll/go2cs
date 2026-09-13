using System;
using go;

// COORD ruling 3's control: a host that DIES before its exit hook must still leave a reading.
// Drive real arm-4 conversions forever; the harness kills this mid-run and reads the file.
static class KillProbe
{
    static int Main()
    {
        Console.Out.WriteLine("PROBE-RUNNING");
        Console.Out.Flush();
        long i = 0;
        while (true)
        {
            // A number that resolves to nothing is arm 4 -- the cheapest real conversion.
            _ = (ж<long>)(uintptr)(nuint)(0x7000_0000_0000_0000UL + (ulong)(++i & 0xFFFF));
        }
    }
}
