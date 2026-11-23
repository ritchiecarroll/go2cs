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
        defer([GoFunc("normalReturnPanic.func1")] () => {
            recover();
            got = frames("normalReturnPanic");
        }, ref ᒐ);
        defer([GoFunc("normalReturnPanic.func2")] () => {
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
        defer([GoFunc("panicAfterRecovery.func1")] () => {
            recover();
            got = frames("panicAfterRecovery");
        }, ref ᒐ);
        defer([GoFunc("panicAfterRecovery.func2")] () => {
            throw panic("p3");
        }, ref ᒐ);
        defer([GoFunc("panicAfterRecovery.func3")] () => {
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
        defer([GoFunc("recoverThenPanic.func1")] () => {
            recover();
            got = frames("recoverThenPanic");
        }, ref ᒐ);
        defer([GoFunc("recoverThenPanic.func2")] () => {
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
        defer([GoFunc("parenAndArgument.func1")] () => {
            recover();
            got = frames("parenAndArgument");
        }, ref ᒐ);
        defer([GoFunc("parenAndArgument.func2")] () => {
            throw panic("paren");
        }, ref ᒐ);
        defer([GoFunc("parenAndArgument.func3")] (@string v) => {
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
        defer([GoFunc("twoLinkChain.func1")] () => {
            recover();
        }, ref ᒐ);
        defer([GoFunc("twoLinkChain.func2")] () => {
            got = frames("twoLinkChain");
        }, ref ᒐ);
        defer([GoFunc("twoLinkChain.func3")] () => {
            throw panic("p3");
        }, ref ᒐ);
        defer([GoFunc("twoLinkChain.func4")] () => {
            throw panic("p2");
        }, ref ᒐ);
        throw panic("p1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return got;
}

internal static void main() {
    fmt.Println("normal return:   ", normalReturnPanic());
    fmt.Println("after recovery:  ", panicAfterRecovery());
    fmt.Println("recover, panic:  ", recoverThenPanic());
    fmt.Println("paren, argument: ", parenAndArgument());
    fmt.Println("two links:       ", twoLinkChain());
}

} // end main_package
