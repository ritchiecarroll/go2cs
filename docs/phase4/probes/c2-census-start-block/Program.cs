using System;
using go;

// THE START-BLOCK CONTROL, BOTH DIRECTIONS, OUT OF PROCESS.
//
// The property under test cannot be observed from inside a test host: it is what a process leaves
// behind when it arms the census and then does NOTHING. The host's own process always converts, so
// the arms have to be separate processes and the harness reads their files.
//
//   argv[0] = the number of real conversions to perform (0 for the zero arms).
//   argv[1] = "hang" to sleep forever afterwards, so the harness can KILL this process before its
//             exit hook runs. That is the arm the start block exists for: a clean exit already
//             writes a final block, so a clean zero was never the ambiguous case -- a process that
//             DIES having converted nothing was, and it left no file at all.
//
// Every conversion here is arm 4 -- a number that resolves to nothing -- because it is the cheapest
// real one and the arm the count lands in does not matter to this control.
static class StartBlockProbe
{
    static int Main(string[] args)
    {
        long want = args.Length > 0 && long.TryParse(args[0], out long n) ? n : 0;

        for (long i = 0; i < want; i++)
            _ = (ж<long>)(uintptr)(nuint)(0x7000_0000_0000_0000UL + (ulong)((i + 1) & 0xFFFF));

        Console.Out.WriteLine($"PROBE-DID {want}");
        Console.Out.Flush();

        if (args.Length > 1 && args[1] == "hang")
        {
            while (true)
                System.Threading.Thread.Sleep(1000);
        }

        return 0;
    }
}
