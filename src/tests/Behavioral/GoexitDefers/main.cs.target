namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static void exitFromHelper(channel/*<-*/<@string> log) {
    GoFrame ᒐ = default;
    try {
        var logʗ1 = log;
        defer(() => {
            logʗ1.ᐸꟷ("helper defer ran");
        }, ref ᒐ);

        log.ᐸꟷ("helper calling runtime.Goexit");
        runtime.Goexit();
        log.ᐸꟷ("helper resumed (UNREACHABLE)");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mainContinuesPastTheˢ = (@string)"main continues past the exited goroutine"u8;

internal static partial void Main() {
    var log = new channel<@string>(16);
    var done = new channel<EmptyStruct>(0);

    var doneʗ1 = done;
    var logʗ1 = log;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);

            var logʗ2 = logʗ1;
            defer(() => {
                logʗ2.ᐸꟷ("goroutine defer 2 ran");
            }, ref ᒐ);

            var logʗ3 = logʗ1;
            defer(() => {
                if (recover() == default!){
                    logʗ3.ᐸꟷ("recover() during Goexit returned nil");
                } else {
                    logʗ3.ᐸꟷ("recover() during Goexit returned a value (WRONG)");
                }
                logʗ3.ᐸꟷ("goroutine defer 1 ran");
            }, ref ᒐ);

            exitFromHelper(logʗ1.WithDirection(GoChanDir.Send));
            logʗ1.ᐸꟷ("goroutine resumed (UNREACHABLE)");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    ᐸꟷ(done);

    close(log);
    foreach (var line in log) {
        fmt.Println(line);
    }

    var after = new channel<@string>(0);
    var afterʗ1 = after;
    goǃ(() => {
        afterʗ1.ᐸꟷ("a later goroutine still runs");
    });
    fmt.Println(ᐸꟷ(after));

    fmt.Println(mainContinuesPastTheˢ);
}

} // end main_package
