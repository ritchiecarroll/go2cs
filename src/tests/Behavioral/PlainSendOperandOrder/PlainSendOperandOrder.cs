namespace go;

using fmt = fmt_package;

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
            var (ᴛ1, ᴛ2) = pair();
            chanFor(chʗ1).ᐸꟷ(must(ᴛ1, ᴛ2));
        }, ref ᒐ);
        log = append(log, "body"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object callˢ = (@string)"call:"u8;
private static readonly object indexˢ = (@string)"index:"u8;
private static readonly object deferˢ = (@string)"defer:"u8;
private static readonly object bareˢ = (@string)"bare:"u8;

internal static void Main() {
    var ch = new channel<nint>(8);
    var (ᴛ3, ᴛ4) = pair();
    chanFor(ch).ᐸꟷ(must(ᴛ3, ᴛ4));
    fmt.Println(callˢ, log, ᐸꟷ(ch));
    log = default!;
    var chans = new channel<nint>[]{default!, ch}.slice();
    var (ᴛ5, ᴛ6) = pair();
    chans[idx()].ᐸꟷ(must(ᴛ5, ᴛ6));
    fmt.Println(indexˢ, log, ᐸꟷ(ch));
    log = default!;
    var done = new channel<bool>(0);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var (ᴛ7, ᴛ8) = pair();
        chanFor(chʗ1).ᐸꟷ(must(ᴛ7, ᴛ8));
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    fmt.Println((@string)"go:"u8, log, ᐸꟷ(ch));
    log = default!;
    deferredSend(ch);
    fmt.Println(deferˢ, log, ᐸꟷ(ch));
    log = default!;
    var (ᴛ9, ᴛ10) = pair();
    ch.ᐸꟷ(must(ᴛ9, ᴛ10));
    fmt.Println(bareˢ, log, ᐸꟷ(ch));
}

} // end main_package
