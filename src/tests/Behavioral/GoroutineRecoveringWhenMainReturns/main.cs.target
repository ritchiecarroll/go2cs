namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

internal static partial void Main() {
    runtime.GOMAXPROCS(1);

    fmt.Println("before goroutine panic");

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);

    var release = new channel<EmptyStruct>(0);

    var releaseʗ1 = release;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var releaseʗ2 = releaseʗ1;
            defer(() => {
                recover();
                ᐸꟷ(releaseʗ2);
                fmt.Println("recovered");
            }, ref ᒐ);
            defer(Ꮡwg.Done, ref ᒐ);
            throw panic("recovered later");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    Ꮡwg.Wait();

    var releaseʗ3 = release;
    time.AfterFunc(500 * time.Millisecond, () => {
        close(releaseʗ3);
    });
}

} // end main_package
