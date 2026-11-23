namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

internal static void exitFromHelper(channel/*<-*/<@string> log) {
    GoFrame ᒐ = default;
    try {
        var logʗ1 = log;
        defer([GoFunc("exitFromHelper.func1")] () => {
            logʗ1.ᐸꟷ("helper defer ran");
        }, ref ᒐ);

        log.ᐸꟷ("helper calling runtime.Goexit");
        runtime.Goexit();
        log.ᐸꟷ("helper resumed (UNREACHABLE)");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    var log = new channel<@string>(16);
    var done = new channel<EmptyStruct>(0);

    var doneʗ1 = done;
    var logʗ1 = log;
    goǃ([GoFunc("main.func1")] () => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), doneʗ1, ref ᒐ);

            var logʗ2 = logʗ1;
            defer([GoFunc("main.func1.1")] () => {
                logʗ2.ᐸꟷ("goroutine defer 2 ran");
            }, ref ᒐ);

            var logʗ3 = logʗ1;
            defer([GoFunc("main.func1.2")] () => {
                if (recover() == nil) {
                    logʗ3.ᐸꟷ("recover() during Goexit returned nil");
                } else {
                    logʗ3.ᐸꟷ("recover() during Goexit returned a value (WRONG)");
                }
                logʗ3.ᐸꟷ("goroutine defer 1 ran");
            }, ref ᒐ);

            exitFromHelper(logʗ1.AsSendOnly());
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
    goǃ([GoFunc("main.func2")] () => {
        afterʗ1.ᐸꟷ("a later goroutine still runs");
    });
    fmt.Println(ᐸꟷ(after));

    fmt.Println("main continues past the exited goroutine");
}

} // end main_package
