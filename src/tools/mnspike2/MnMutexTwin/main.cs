namespace go;

using fmt = fmt_package;
using Δsync = sync_package;

partial class main_package {

internal static readonly ж<Δsync.Once> Ꮡonce = new StaticBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once once => ref ((StaticBox<Δsync.Once>)Ꮡonce).ValueSlot;

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
}

} // end main_package
