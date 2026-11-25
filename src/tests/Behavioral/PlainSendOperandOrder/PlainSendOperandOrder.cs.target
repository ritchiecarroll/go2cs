namespace go;

using fmt = fmt_package;

partial class main_package {

internal static slice<@string> log;

internal static (nint, error) pair() {
    log = append(log, (@string)"pair");
    return (7, default);
}

internal static nint must(nint v, error err) {
    if (err != nil) {
        throw panic(err);
    }
    return v;
}

internal static channel<nint> chanFor(channel<nint> ch) {
    log = append(log, (@string)"chan");
    return ch;
}

internal static nint idx() {
    log = append(log, (@string)"idx");
    return 1;
}

internal static void deferredSend(channel<nint> ch) {
    GoFrame ᒐ = default;
    try {
        var chʗ1 = ch;
        defer([GoFunc("deferredSend.func1")] () => {
            var chanᴛ1 = chanFor(chʗ1);
            var (ᴛ1, ᴛ2) = pair();
            chanᴛ1.ᐸꟷ(must(ᴛ1, ᴛ2));
        }, ref ᒐ);
        log = append(log, (@string)"body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void main() {
    var ch = new channel<nint>(8);

    var chanᴛ2 = chanFor(ch);
    var (ᴛ3, ᴛ4) = pair();
    chanᴛ2.ᐸꟷ(must(ᴛ3, ᴛ4));
    var evalᴛ1 = ᐸꟷ(ch);
    fmt.Println("call:", log, evalᴛ1);

    log = default;
    var chans = slice<channel<nint>>([default, ch]);
    var chanᴛ3 = chans[idx()];
    var (ᴛ5, ᴛ6) = pair();
    chanᴛ3.ᐸꟷ(must(ᴛ5, ᴛ6));
    var evalᴛ2 = ᐸꟷ(ch);
    fmt.Println("index:", log, evalᴛ2);

    log = default;
    var done = new channel<bool>(0);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ([GoFunc("main.func1")] () => {
        var chanᴛ4 = chanFor(chʗ1);
        var (ᴛ7, ᴛ8) = pair();
        chanᴛ4.ᐸꟷ(must(ᴛ7, ᴛ8));
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    var evalᴛ3 = ᐸꟷ(ch);
    fmt.Println("go:", log, evalᴛ3);

    log = default;
    deferredSend(ch);
    var evalᴛ4 = ᐸꟷ(ch);
    fmt.Println("defer:", log, evalᴛ4);

    log = default;
    var (ᴛ9, ᴛ10) = pair();
    ch.ᐸꟷ(must(ᴛ9, ᴛ10));
    var evalᴛ5 = ᐸꟷ(ch);
    fmt.Println("bare:", log, evalᴛ5);
}

} // end main_package
