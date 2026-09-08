using System;
using go;

// HALF (b) OF THE CALLBACK ROW, MEASURED: can the converted `callback` recover its box?
//
// The emitted callback (runtime/syscall_windows_test.cs, converter at 44f858717) is:
//     internal static uintptr callback(@unsafe.Pointer timeFormatString, uintptr lparamʗp) {
//         ref var lparam = ref heap(lparamʗp, out var Ꮡlparam);
//         (Ꮡlparam.Reinterpret<uintptr, Action>()).ValueSlot();
//         return 0;
//     }
// so the inbound edge is a Reinterpret of the box holding the CARRIED NUMBER, then an invoke.
//
// ONE ARM PER PROCESS (argv[0] = arm), because a type-confused managed reference can take the
// process down and a crash in one arm must not be read as a verdict on another.
//
//   token  -- the real shape: the number is a TOKEN, minted from a box over a reference-BEARING
//             pointee, exactly as nestedCall's argument 3 is.
//   plain  -- THE VARIED AXIS: identical shape, but the number comes from a box over a
//             reference-FREE pointee, so it is a real pinned ADDRESS and not a token. This is what
//             separates "tokens cannot round-trip" from "this Reinterpret shape never round-trips".
static class CallbackRecoveryProbe
{
    private struct RefBearing { internal string name; internal nint scalar; }
    private struct RefFree    { internal nint a; internal nint b; }

    static int Main(string[] args)
    {
        string arm = args.Length > 0 ? args[0] : "token";
        Console.Out.WriteLine($"ARM {arm}");

        if (arm == "token")
        {
            // The OUTBOUND edge, as nestedCall emits it: a box over a reference-bearing pointee.
            bool fired = false;
            Action original = () => { fired = true; };
            ref var f = ref builtin.heap(original, out var boxF);
            uintptr outbound = (uintptr)boxF;
            Console.Out.WriteLine($"  outbound number      = 0x{(nuint)outbound:X}");
            Console.Out.WriteLine($"  IsTaggedToken        = {ManagedPointerTokens.IsTaggedToken((nuint)outbound)}");
            Console.Out.WriteLine($"  Resolve -> same box  = {ReferenceEquals(ManagedPointerTokens.Resolve((nuint)outbound), boxF)}");

            // THE INBOUND EDGE, verbatim in shape: the number arrives as a uintptr parameter, is
            // heap-boxed, and the box is reinterpreted as the delegate type and invoked.
            uintptr lparam = outbound;
            ref var l = ref builtin.heap(lparam, out var boxL);
            Console.Out.WriteLine("  --- Reinterpret<uintptr, Action>() ---");
            try
            {
                var derived = boxL.Reinterpret<uintptr, Action>();
                Console.Out.WriteLine($"  Reinterpret returned : {(derived is null ? "null" : derived.GetType().Name)}");
                Action recovered = derived.ValueSlot;
                Console.Out.WriteLine($"  recovered is null    = {recovered is null}");
                Console.Out.WriteLine($"  recovered SAME as f  = {ReferenceEquals(recovered, original)}");
                if (recovered is not null)
                {
                    recovered();
                    Console.Out.WriteLine($"  invoked; fired       = {fired}");
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"  THREW {e.GetType().Name}: {Trim(e.Message)}");
            }
            GC.KeepAlive(original);
            GC.KeepAlive(boxF);
        }
        else
        {
            // THE CONTROL: reference-FREE pointee, so the outbound number is a pinned address.
            ref var v = ref builtin.heap(new RefFree { a = 0x1111, b = 0x2222 }, out var boxV);
            uintptr outbound = (uintptr)boxV;
            Console.Out.WriteLine($"  outbound number      = 0x{(nuint)outbound:X}");
            Console.Out.WriteLine($"  IsTaggedToken        = {ManagedPointerTokens.IsTaggedToken((nuint)outbound)}");
            Console.Out.WriteLine($"  Resolve -> same box  = {ReferenceEquals(ManagedPointerTokens.Resolve((nuint)outbound), boxV)}");

            uintptr lparam = outbound;
            ref var l = ref builtin.heap(lparam, out var boxL);
            Console.Out.WriteLine("  --- Reinterpret<uintptr, RefFree>() ---");
            try
            {
                var derived = boxL.Reinterpret<uintptr, RefFree>();
                Console.Out.WriteLine($"  Reinterpret returned : {(derived is null ? "null" : derived.GetType().Name)}");
                RefFree back = derived.ValueSlot;
                Console.Out.WriteLine($"  recovered a=0x{back.a:X} b=0x{back.b:X}  (source was a=0x1111 b=0x2222)");
                Console.Out.WriteLine($"  round trip EXACT     = {back.a == 0x1111 && back.b == 0x2222}");
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"  THREW {e.GetType().Name}: {Trim(e.Message)}");
            }
            GC.KeepAlive(boxV);
        }

        Console.Out.WriteLine($"ARM {arm} COMPLETED");
        return 0;
    }

    static string Trim(string s) => s.Length <= 200 ? s : s.Substring(0, 200) + " ...";
}
