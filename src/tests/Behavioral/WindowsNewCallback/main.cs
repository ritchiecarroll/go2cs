namespace go;

using fmt = fmt_package;
using syscall = syscall_package;

partial class main_package {

internal static bool invoked;

internal static uintptr onLocale(uintptr lpLocaleString) {
    invoked = true;
    return 1;
}

internal static uintptr onLocaleOther(uintptr lpLocaleString) {
    return 0;
}

internal static void nonConforming() {
}

internal static uintptr wantLParam => /* uintptr(0x5A5A5A5A) */ 1515870810;

internal static uintptr seenLParam;

internal static uintptr onTimeFormat(uintptr timeFormatString, uintptr lparam) {
    seenLParam = lparam;
    return 0;
}

internal static readonly @string callbackPanicText = "callback panic"u8;

internal static uintptr onTimeFormatPanics(uintptr timeFormatString, uintptr lparam) {
    throw panic(callbackPanicText);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sameFuncSamePointerˢ = (@string)"same-func-same-pointer:"u8;
private static readonly object differentFuncDifferentˢ = (@string)"different-func-different-pointer:"u8;
private static readonly object callbackInvokedˢ = (@string)"callback-invoked:"u8;
private static readonly object nonconformingRefusalˢ = (@string)"nonconforming-refusal:"u8;
private static readonly object callbackPanicUnwindsˢ = (@string)"callback-panic-unwinds:"u8;

internal static void Main() {
    var kernel32 = syscall.NewLazyDLL("kernel32.dll");
    var enumSystemLocalesW = kernel32.NewProc("EnumSystemLocalesW");

    var cb1 = syscall.NewCallback(onLocale);
    var cb2 = syscall.NewCallback(onLocale);
    fmt.Println(sameFuncSamePointerˢ, cb1 == cb2);

    var cbOther = syscall.NewCallback(onLocaleOther);
    fmt.Println(differentFuncDifferentˢ, cbOther != cb1);

    enumSystemLocalesW.Call(cb1, (uintptr)0x1);
    fmt.Println(callbackInvokedˢ, invoked);

    var enumTimeFormatsEx = kernel32.NewProc("EnumTimeFormatsEx");
    uintptr localeNameUserDefault = 0;
    enumTimeFormatsEx.Call(syscall.NewCallback(onTimeFormat), localeNameUserDefault, 0, wantLParam);
    fmt.Printf("lparam-round-trip: %#x (want %#x)\n"u8, seenLParam, wantLParam);

    fmt.Println(nonconformingRefusalˢ, refusalText());

    fmt.Println(callbackPanicUnwindsˢ, panicUnwindText(enumTimeFormatsEx));
}

internal static @string /*out*/ panicUnwindText(ж<syscall.LazyProc> ᏑenumTimeFormatsEx) {
    @string @out = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                @out = "<no-panic>";
                break;
            }
            case @string r: {
                @out = r;
                break;
            }
            default: {
                var r = switchᴛ1;
                @out = fmt.Sprintf("<non-string-panic:%T>"u8, r);
                break;
            }}
        }, ref ᒐ);
        uintptr localeNameUserDefault = 0;
        ᏑenumTimeFormatsEx.Call(syscall.NewCallback(onTimeFormatPanics), localeNameUserDefault, 0, wantLParam);
        @out = "<no-panic>";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

internal static @string /*out*/ refusalText() {
    @string @out = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var switchᴛ2 = recover();
            switch (switchᴛ2.type()) {
            case null: {
                @out = "<no-panic>";
                break;
            }
            case @string r: {
                @out = r;
                break;
            }
            default: {
                var r = switchᴛ2;
                @out = fmt.Sprintf("<non-string-panic:%T>"u8, r);
                break;
            }}
        }, ref ᒐ);
        syscall.NewCallback(nonConforming);
        @out = "<no-panic>";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return @out;
}

} // end main_package
