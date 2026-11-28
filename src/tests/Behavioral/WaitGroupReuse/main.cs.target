namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using time = time_package;

partial class main_package {

internal static UntypedInt rounds => 20;
internal static UntypedInt waiters => 8;

internal static partial void main() {
    nint reports = 0;
    var panicked = false;
    @string text = "";

    for (nint round = 0; round < rounds; round++) {
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);

        var starting = new channel<EmptyStruct>(waiters);
        var report = new channel<@string>(waiters);

        for (nint i = 0; i < waiters; i++) {
            var reportʗ1 = report;
            var startingʗ1 = starting;
            goǃ([GoFunc("main.func1")] () => {
                GoFrame ᒐ = default;
                try {
                    var reportʗ2 = reportʗ1;
                    defer([GoFunc("main.func1.1")] () => {
                        var x = recover(); if (x != nil) {
                            var evalᴛ1 = fmt.Sprint(x);
                            reportʗ2.ᐸꟷ(evalᴛ1);
                            return;
                        }

                        reportʗ2.ᐸꟷ("");
                    }, ref ᒐ);

                    startingʗ1.ᐸꟷ(new EmptyStruct());
                    Ꮡwg.Wait();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }

        for (nint i = 0; i < waiters; i++) {
            ᐸꟷ(starting);
        }

        time.Sleep(10 * time.Millisecond);

        Ꮡwg.Done();
        Ꮡwg.Add(1);

        var lowered = false;
        var late = time.After(500 * time.Millisecond);

        for (nint got = 0; got < waiters; ) {
            var reportᴛ1 = report;
            var lateᴛ1 = late;
            switch (select(ᐸꟷ(reportᴛ1, ꓸꓸꓸ), ᐸꟷ(lateᴛ1, ꓸꓸꓸ))) {
            case 0 when reportᴛ1.ꟷᐳ(out var s): {
                got++;
                reports++;

                if (s != ""u8) {
                    panicked = true;
                    text = s;
                }
                break;
            }
            case 1 when lateᴛ1.ꟷᐳ(out _): {
                lowered = true;
                Ꮡwg.Done();
                break;
            }}
        }

        if (!lowered) {
            Ꮡwg.Done();
        }
    }

    fmt.Println("reports:", reports);
    fmt.Println("a waiter panicked:", panicked);
    fmt.Println(text);
}

} // end main_package
