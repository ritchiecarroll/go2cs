namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    fmt.Println("before goroutine panic");

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);

    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
                time.Sleep(500 * time.Millisecond);
                fmt.Println("recovered");
            }, ref ᒐ);
            defer(Ꮡwg.Done, ref ᒐ);
            throw panic("recovered later");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    Ꮡwg.Wait();
}

} // end main_package
