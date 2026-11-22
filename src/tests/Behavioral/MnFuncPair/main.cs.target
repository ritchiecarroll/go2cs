namespace go;

using fmt = fmt_package;
using Δsync = sync_package;

partial class main_package {

internal static UntypedInt n => 2000;

internal static channel<EmptyStruct> ready = new channel<EmptyStruct>(n);
internal static channel<EmptyStruct> entered = new channel<EmptyStruct>(n);
internal static channel<EmptyStruct> gate = new channel<EmptyStruct>(0);
internal static readonly ж<Δsync.WaitGroup> Ꮡwg = new StaticBox<Δsync.WaitGroup>(default(Δsync.WaitGroup));
internal static ref Δsync.WaitGroup wg => ref ((StaticBox<Δsync.WaitGroup>)Ꮡwg).ValueSlot;
internal static slice<nint> results = new slice<nint>(n);

internal static void run(Func<nint> f, nint i) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡwg.Done, ref ᒐ);
        ᐸꟷ(ready);
        nint v = f();
        results[i] = v;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    nint k = 1;
    var wait = () => {
        entered.ᐸꟷ(new EmptyStruct());
        ᐸꟷ(gate);
        return k;
    };

    for (nint i = 0; i < n; i++) {
        ready.ᐸꟷ(new EmptyStruct());
    }

    for (nint i = 0; i < n; i++) {
        Ꮡwg.Add(1);
        goǃ(run, wait, i);
    }

    for (nint i = 0; i < n; i++) {
        ᐸꟷ(entered);
    }

    close(gate);
    Ꮡwg.Wait();

    nint sum = 0;
    foreach (var (_, r) in range(results)) {
        sum += r;
    }

    fmt.Println("parked behind a func value and released:", (nint)(n), "sum", sum);
}

} // end main_package
