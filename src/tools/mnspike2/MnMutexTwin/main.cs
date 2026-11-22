namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

internal static readonly ж<Δsync.Once> Ꮡonce = new StaticBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once once => ref ((StaticBox<Δsync.Once>)Ꮡonce).ValueSlot;
internal static readonly ж<Δsync.Once> Ꮡonce2 = new StaticBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once once2 => ref ((StaticBox<Δsync.Once>)Ꮡonce2).ValueSlot;

internal static partial void main() {
    var ch = new channel<nint>(1);
    var chʗ1 = ch;
    goǃ(() => {
        chʗ1.ᐸꟷ(1);
    });
    ᐸꟷ(ch);

    Ꮡonce.Do(() => {
        fmt.Println("once");
    });

    ref var rw = ref heap(new Δsync.RWMutex(), out var Ꮡrw);
    Ꮡrw.Lock();
    Ꮡrw.Unlock();
    fmt.Println("rwmutex");

    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    Ꮡmu.Lock();
    Ꮡmu.Unlock();
    fmt.Println("done");

    var locked = new channel<bool>(0);
    var release = new channel<bool>(0);
    var finished = new channel<bool>(0);
    var finishedʗ1 = finished;
    var lockedʗ1 = locked;
    var releaseʗ1 = release;
    goǃ(() => {
        var lockedʗ2 = lockedʗ1;
        var releaseʗ2 = releaseʗ1;
        Ꮡonce2.Do(() => {
            lockedʗ2.ᐸꟷ(true);
            ᐸꟷ(releaseʗ2);
        });
        finishedʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(locked);
    var releaseʗ3 = release;
    goǃ(() => {
        time.Sleep(20 * time.Millisecond);
        releaseʗ3.ᐸꟷ(true);
    });
    Ꮡonce2.Do(() => {
        fmt.Println("never");
    });
    ᐸꟷ(finished);
    fmt.Println("contended");
}

} // end main_package
