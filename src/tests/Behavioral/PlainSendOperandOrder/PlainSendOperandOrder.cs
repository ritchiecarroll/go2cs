namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static slice<@string> log;

internal static (nint, error) pair() {
    log = append(log, "pair"u8);
    return (7, default!);
}

internal static nint must(nint v, error err) {
    if (err != default!) {
        throw panic(err);
    }
    return v;
}

internal static channel<nint> chanFor(channel<nint> ch) {
    log = append(log, "chan"u8);
    return ch;
}

internal static nint idx() {
    log = append(log, "idx"u8);
    return 1;
}

internal static void deferredSend(channel<nint> ch) {
    GoFrame ᒐ = default;
    try {
        var chʗ1 = ch;
        defer(() => {
            var chanᴛ1 = chanFor(chʗ1);
            var (ᴛ1, ᴛ2) = pair();
            chanᴛ1.ᐸꟷ(must(ᴛ1, ᴛ2));
        }, ref ᒐ);
        log = append(log, "body"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void Main() {
    var ch = new channel<nint>(8);

    var chanᴛ2 = chanFor(ch);
    var (ᴛ3, ᴛ4) = pair();
    chanᴛ2.ᐸꟷ(must(ᴛ3, ᴛ4));
    fmt.Println("call:", log, ᐸꟷ(ch));

    log = default!;
    var chans = new channel<nint>[]{default!, ch}.slice();
    var chanᴛ3 = chans[idx()];
    var (ᴛ5, ᴛ6) = pair();
    chanᴛ3.ᐸꟷ(must(ᴛ5, ᴛ6));
    fmt.Println("index:", log, ᐸꟷ(ch));

    log = default!;
    var done = new channel<bool>(0);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var chanᴛ4 = chanFor(chʗ1);
        var (ᴛ7, ᴛ8) = pair();
        chanᴛ4.ᐸꟷ(must(ᴛ7, ᴛ8));
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    fmt.Println("go:", log, ᐸꟷ(ch));

    log = default!;
    deferredSend(ch);
    fmt.Println("defer:", log, ᐸꟷ(ch));

    log = default!;
    var (ᴛ9, ᴛ10) = pair();
    ch.ᐸꟷ(must(ᴛ9, ᴛ10));
    fmt.Println("bare:", log, ᐸꟷ(ch));
}

} // end main_package
