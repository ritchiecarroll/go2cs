namespace go;

using fmt = fmt_package;
using Δio = io_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

partial struct slow {
    internal channel<nint> ch;
}

internal static @string String(this slow s) {
    nint v = ᐸꟷ(s.ch);
    return fmt.Sprintf("slow(%d)"u8, v);
}

internal static partial @string hookBlocks() {
    var gate = new channel<EmptyStruct>(0);
    var ch = new channel<nint>(0);
    var done = new channel<@string>(0);

    var chʗ1 = ch;
    var doneʗ1 = done;
    var gateʗ1 = gate;
    goǃ([GoFunc("hookBlocks.func1")] () => {
        ᐸꟷ(gateʗ1);
        doneʗ1.ᐸꟷ(fmt.Sprint(new slow(chʗ1)));
    });

    var chʗ2 = ch;
    var gateʗ2 = gate;
    goǃ([GoFunc("hookBlocks.func2")] () => {
        ᐸꟷ(gateʗ2);
        time.Sleep(10 * time.Millisecond);
        chʗ2.ᐸꟷ(9);
    });

    close(gate);
    return ᐸꟷ(done);
}

internal static partial nint readerBlocks() {
    var gate = new channel<EmptyStruct>(0);
    var (r, w) = Δio.Pipe();
    var done = new channel<nint>(0);

    var doneʗ1 = done;
    var gateʗ1 = gate;
    var rʗ1 = r;
    goǃ([GoFunc("readerBlocks.func1")] () => {
        ᐸꟷ(gateʗ1);
        ref var n = ref heap(new nint(), out var Ꮡn);
        fmt.Fscan(new Δio.PipeReaderжReader(rʗ1), Ꮡn);
        doneʗ1.ᐸꟷ(n);
    });

    var gateʗ2 = gate;
    var wʗ1 = w;
    goǃ([GoFunc("readerBlocks.func2")] () => {
        ᐸꟷ(gateʗ2);
        time.Sleep(10 * time.Millisecond);
        fmt.Fprint(new Δio.PipeWriterжWriter(wʗ1), "10 ");
        wʗ1.Close();
    });

    close(gate);
    return ᐸꟷ(done);
}

internal static nint plainLock(ж<Δsync.Mutex> mu, ref nint n) {
    GoFrame ᒐ = default;
    try {
        mu.Lock();
        defer(mu.Unlock, ref ᒐ);
        n++;
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static partial (nint, nint) weakLockBlocks() {
    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    ref var n = ref heap<nint>(0, out var Ꮡn);
    var held = new channel<EmptyStruct>(0);
    var wake = new channel<EmptyStruct>(0);
    var holder = new channel<nint>(0);
    var locker = new channel<nint>(0);

    var heldʗ1 = held;
    var holderʗ1 = holder;
    var wakeʗ1 = wake;
    goǃ([GoFunc("weakLockBlocks.func1")] () => {
        Ꮡmu.Lock();
        close(heldʗ1);
        ᐸꟷ(wakeʗ1);
        Ꮡn.Value += 10;
        Ꮡmu.Unlock();
        holderʗ1.ᐸꟷ(1);
    });

    var heldʗ2 = held;
    var lockerʗ1 = locker;
    goǃ([GoFunc("weakLockBlocks.func2")] () => {
        ᐸꟷ(heldʗ2);
        lockerʗ1.ᐸꟷ(plainLock(Ꮡmu, ref (Ꮡn).DerefOrNull()));
    });

    var heldʗ3 = held;
    var wakeʗ2 = wake;
    goǃ([GoFunc("weakLockBlocks.func3")] () => {
        ᐸꟷ(heldʗ3);
        time.Sleep(10 * time.Millisecond);
        close(wakeʗ2);
    });

    return (ᐸꟷ(holder), ᐸꟷ(locker));
}

internal static void main() {
    runtime.GOMAXPROCS(1);

    fmt.Println("N1-9 hook:", hookBlocks());
    fmt.Println("N1-10 reader:", readerBlocks());
    var (h, l) = weakLockBlocks();
    fmt.Println("N1-11 weak lock:", h, l);
}

} // end main_package
