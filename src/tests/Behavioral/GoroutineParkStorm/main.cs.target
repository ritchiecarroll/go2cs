namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt n => 1000;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object arrivedˢ = (@string)"arrived:"u8;
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object rendezvousˢ = (@string)"rendezvous:"u8;
private static readonly object totalˢ = (@string)"total:"u8;
private static readonly object matchˢ = (@string)"match:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    ref var start = ref heap(new Δsync.WaitGroup(), out var Ꮡstart);
    Ꮡstart.Add(1);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    Ꮡdone.Add(n);
    var arrived = new channel<nint>(n);
    var results = new channel<nint>(0);
    for (nint i = 0; i < n; i++) {
        var arrivedʗ1 = arrived;
        var resultsʗ1 = results;
        goǃ((nint id) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡdone.Done, ref ᒐ);
                arrivedʗ1.ᐸꟷ(id);
                Ꮡstart.Wait();
                resultsʗ1.ᐸꟷ(id);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    nint sum = 0;
    for (nint i = 0; i < n; i++) {
        sum += ᐸꟷ(arrived);
    }
    fmt.Println(arrivedˢ, (nint)(n), sumˢ, sum);
    Ꮡstart.Done();
    nint total = 0;
    for (nint i = 0; i < n; i++) {
        total += ᐸꟷ(results);
    }
    Ꮡdone.Wait();
    fmt.Println(rendezvousˢ, (nint)(n), totalˢ, total);
    fmt.Println(matchˢ, sum == total);
}

} // end main_package
