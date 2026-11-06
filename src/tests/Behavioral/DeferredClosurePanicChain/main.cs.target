namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using strings = strings_package;

partial class main_package {

internal static partial @string frames(@string owner) {
    var namesᴷ = new stackAppend<@string>();

    var pcs = new slice<uintptr>(64);
    nint n = runtime.Callers(2, pcs);
    var iter = runtime.CallersFrames(pcs.slice(0, n));
    slice<@string> names = default;

    for (;;) {
        var (frame, more) = iter.Next();
        names = append(namesᴷ, names, frame.Function);

        if (frame.Function == "main."u8 + owner || !more) {
            break;
        }
    }

    return strings.Join(names, " | ");
}

internal static @string /*got*/ normalReturnPanic() {
    @string got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
            got = frames("normalReturnPanic");
        }, ref ᒐ);
        defer(() => {
            throw panic("d");
        }, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static @string /*got*/ panicAfterRecovery() {
    @string got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
            got = frames("panicAfterRecovery");
        }, ref ᒐ);
        defer(() => {
            throw panic("p3");
        }, ref ᒐ);
        defer(() => {
            recover();
        }, ref ᒐ);
        throw panic("p1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static @string /*got*/ recoverThenPanic() {
    @string got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
            got = frames("recoverThenPanic");
        }, ref ᒐ);
        defer(() => {
            recover();
            throw panic("p2");
        }, ref ᒐ);
        throw panic("p1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static @string /*got*/ parenAndArgument() {
    @string got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
            got = frames("parenAndArgument");
        }, ref ᒐ);
        defer(() => {
            throw panic("paren");
        }, ref ᒐ);
        defer((@string v) => {
            recover();
            throw panic(v);
        }, (@string)"argument", ref ᒐ);
        throw panic("p1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static @string /*got*/ twoLinkChain() {
    @string got = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
        }, ref ᒐ);
        defer(() => {
            got = frames("twoLinkChain");
        }, ref ᒐ);
        defer(() => {
            throw panic("p3");
        }, ref ᒐ);
        defer(() => {
            throw panic("p2");
        }, ref ᒐ);
        throw panic("p1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static void Main() {
    fmt.Println("normal return:   ", normalReturnPanic());
    fmt.Println("after recovery:  ", panicAfterRecovery());
    fmt.Println("recover, panic:  ", recoverThenPanic());
    fmt.Println("paren, argument: ", parenAndArgument());
    fmt.Println("two links:       ", twoLinkChain());
}

} // end main_package
