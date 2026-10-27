namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint calls;

internal static any next(any v) {
    calls++;
    return v;
}

internal static @string classify(any x) {
    var switchᴛ1 = next(x);
    switch (switchᴛ1.type()) {
    case nint v: {
        return fmt.Sprintf("int:%d"u8, v);
    }
    case @string _:
    case bool _: {
        var v = switchᴛ1;
        return fmt.Sprintf("strbool:%v"u8, v);
    }
    default: {
        var v = switchᴛ1;
        return fmt.Sprintf("other:%v"u8, v);
    }}
}

internal static @string /*result*/ recovered(any v) {
    @string result = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var switchᴛ2 = recover();
            switch (switchᴛ2.type()) {
            case @string _:
            case nint _: {
                var p = switchᴛ2;
                result = fmt.Sprintf("caught:%v"u8, p);
                break;
            }
            case null: {
                result = "no panic?";
                break;
            }
            default: {
                var p = switchᴛ2;
                result = fmt.Sprintf("caught-other:%v"u8, p);
                break;
            }}
        }, ref ᒐ);
        throw panic(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return result;
}

internal static @string fromChan(channel<any> ch) {
    var switchᴛ3 = (ᐸꟷ(ch));
    switch (switchᴛ3.type()) {
    case nint _:
    case int64 _: {
        var v = switchᴛ3;
        return fmt.Sprintf("num:%v"u8, v);
    }
    default: {
        var v = switchᴛ3;
        return fmt.Sprintf("chan-other:%v"u8, v);
    }}
}

internal static void Main() {
    fmt.Println(classify((nint)7));
    fmt.Println(classify("hi"));
    fmt.Println(classify(true));
    fmt.Println(classify(2.5D));
    fmt.Println("calls:", calls);

    fmt.Println(recovered("boom"));
    fmt.Println(recovered((nint)9));
    fmt.Println(recovered(1.25D));

    var ch = new channel<any>(1);
    ch.ᐸꟷ((nint)5);
    fmt.Println(fromChan(ch));
    @string msg = "text";
    ch.ᐸꟷ(msg);
    fmt.Println(fromChan(ch));
}

} // end main_package
