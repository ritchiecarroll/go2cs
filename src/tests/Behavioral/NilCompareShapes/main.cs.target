namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal @string msg;
}

internal static @string Error(this ref myErr e) {
    return e.msg;
}

internal static error typedNilErr() {
    ж<myErr> p = default;
    return new myErrжerror(p);
}

internal static error nilErr() {
    return default;
}

// type handler is a methodless func type — rendered inline as its base delegate

partial struct holder {
    internal error err;
    internal Func<@string> fn;
}

internal static bool /*recovered*/ tryRecover(bool doPanic) {
    bool recovered = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recovered = recover() != nil;
        }, ref ᒐ);

        if (doPanic) {
            throw panic("boom");
        }

        recovered = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return recovered;
}

internal static @string describe(any v) {
    switch (v) {
    case var ᴛ when AreEqual(ᴛ, default): {
        return "nil interface";
    }
    default: {
        return fmt.Sprintf("non-nil %T"u8, v);
    }}

}

internal static void Main() {
    var err = typedNilErr();
    fmt.Println("typed nil pointer in error != nil:", err != nil);
    fmt.Println("nil == typed nil pointer in error:", nil == err);

    err = nilErr();
    fmt.Println("nil error == nil:", err == nil);

    Action fn = default;
    fmt.Println("nil func == nil:", fn == nil);
    fn = () => {
    };
    fmt.Println("assigned func != nil:", fn != nil);

    Func<nint, nint> h = default;
    fmt.Println("nil named func == nil:", h == nil);
    h = (nint x) => x * 2;
    fmt.Println("assigned named func != nil:", nil != h, h(21));

    fmt.Println("recover() != nil after panic:", tryRecover(true));
    fmt.Println("recover() != nil without panic:", tryRecover(false));

    map<@string, nint> m = default;
    any a = m;
    fmt.Println("any holding nil map != nil:", a != nil);

    slice<nint> sl = default;
    any @as = sl;
    fmt.Println("any holding nil slice != nil:", @as != nil, nil == @as);

    any empty = default;
    fmt.Println("empty any == nil:", empty == nil);

    holder hd = default;
    fmt.Println("zero struct fields are nil:", hd.err == nil, hd.fn == nil);
    hd.err = fmt.Errorf("wrapped"u8);
    hd.fn = @string () => "set";
    fmt.Println("assigned fields are non-nil:", hd.err != nil, hd.fn != nil, hd.fn());

    fmt.Println(describe(default));
    fmt.Println(describe(m));
    fmt.Println(describe(typedNilErr()));

    collections();
}

} // end main_package
