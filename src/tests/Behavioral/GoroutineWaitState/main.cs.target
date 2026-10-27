namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt dumpSize => /* 1 << 18 */ 262144;

internal static UntypedInt pollLimit => 2000;

internal static slice<@string> parked = new @string[]{
    "sync.Mutex.Lock",
    "chan receive",
    "select",
    "semacquire"
}.slice();

internal static bool present(@string dump, @string state) {
    return strings.Contains(dump, "["u8 + state + "]:"u8);
}

internal static partial @string dump(slice<byte> buf) {
    return (@string)(buf.slice(0, runtime.Stack(buf, true)));
}

internal static @string await(slice<byte> buf, slice<@string> want, bool sense) {
    @string d = default!;

    for (nint i = 0; i < pollLimit; i++) {
        d = dump(buf);
        var ok = true;

        foreach (var (_, state) in want) {
            if (present(d, state) != sense) {
                ok = false;
                break;
            }
        }

        if (ok) {
            return d;
        }

        runtime.Gosched();
        time.Sleep(time.Millisecond);
    }

    return d;
}

internal static partial void Main() {
    var buf = new slice<byte>(dumpSize);

    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();

    var recv = new channel<nint>(0);

    var selA = new channel<nint>(0);
    var selB = new channel<nint>(0);

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);

    var done = new channel<@string>(len(parked));

    var doneʗ1 = done;
    goǃ(() => {
        Ꮡmu.Lock();
        Ꮡmu.Unlock();
        doneʗ1.ᐸꟷ("mutex");
    }, GoStack.Bounded);

    var doneʗ2 = done;
    var recvʗ1 = recv;
    goǃ(() => {
        ᐸꟷ(recvʗ1);
        doneʗ2.ᐸꟷ("recv");
    }, GoStack.Bounded);

    var doneʗ3 = done;
    var selAʗ1 = selA;
    var selBʗ1 = selB;
    goǃ(() => {
        var selᴛ1 = selAʗ1;
        var selᴛ2 = selBʗ1;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            break;
        }}
        doneʗ3.ᐸꟷ("select");
    }, GoStack.Bounded);

    var doneʗ4 = done;
    goǃ(() => {
        Ꮡwg.Wait();
        doneʗ4.ᐸꟷ("waitgroup");
    }, GoStack.Bounded);

    @string blocked = await(buf, parked, true);

    foreach (var (_, state) in parked) {
        fmt.Printf("parked %s: %v\n"u8, state, present(blocked, state));
    }

    fmt.Printf("parked running: %v\n"u8, present(blocked, "running"));

    fmt.Printf("parked chan send: %v\n"u8, present(blocked, "chan send"));

    Ꮡmu.Unlock();
    close(recv);
    close(selA);
    Ꮡwg.Done();

    foreach (var _ᴛ1 in range(parked)) {
        ᐸꟷ(done);
    }

    @string released = await(buf, parked, false);

    foreach (var (_, state) in parked) {
        fmt.Printf("released %s: %v\n"u8, state, present(released, state));
    }

    fmt.Printf("released running: %v\n"u8, present(released, "running"));
}

} // end main_package
