namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate nint Compressor(nint w);

internal static Compressor lookup(any i) {
    var (c, ok) = i._ᶠ<Compressor>(ᐧ); if (ok) {
        return c;
    }
    return default;
}

partial interface Named {
    @string Name();
}
partial interface hidden {
    @string name();
}

public delegate nint Handler(nint _);

public static @string Name(this Handler _) { return "handler"; }
internal static @string name(this Handler _) { return "handler"; }
public static @string Error(this Handler _) { return "handler"; }

partial struct tag {
}

internal static @string Name(this tag _) { return "tag"; }
internal static @string name(this tag _) { return "tag"; }
internal static @string Error(this tag _) { return "tag"; }

internal static void panicText(Action f) {
    GoFrame ᒐ = default;
    try {
        defer([GoFunc("panicText.func1")] () => {
            fmt.Println(recover());
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void main() {
    any i = new Compressor([GoFunc("main.func1")] (nint w) => w * 2);
    var c = lookup(i);
    fmt.Println(c(21));

    any j = (nint)7;
    fmt.Println(lookup(j) == nil);

    ref var n = ref heap<Named>(out var Ꮡn);

    n = new tag();
    ref var h = ref heap<hidden>(out var Ꮡh);

    h = new tag();
    ref var e = ref heap<error>(out var Ꮡe);

    e = new tag();
    Named nilNamed = default;
    any nilAny = default;

    var jʗ1 = j;
    panicText([GoFunc("main.func2")] () => {
        _ = jʗ1._ᶠ<Compressor>();
    });
    panicText([GoFunc("main.func3")] () => {
        _ = Ꮡn.ValueSlot._ᶠ<Named, Handler>();
    });
    panicText([GoFunc("main.func4")] () => {
        _ = Ꮡh.ValueSlot._ᶠ<hidden, Handler>();
    });
    panicText([GoFunc("main.func5")] () => {
        _ = Ꮡe.ValueSlot._ᶠ<error, Handler>();
    });
    var nilNamedʗ1 = nilNamed;
    panicText([GoFunc("main.func6")] () => {
        _ = nilNamedʗ1._ᶠ<Named, Handler>();
    });
    var nilAnyʗ1 = nilAny;
    panicText([GoFunc("main.func7")] () => {
        _ = nilAnyʗ1._ᶠ<Compressor>();
    });
    var iʗ1 = i;
    panicText([GoFunc("main.func8")] () => {
        _ = iʗ1._ᶠ<Func<nint, nint>>();
    });
    fmt.Println(i._ᶠ<Compressor>()(4));

    var @double = new Handler([GoFunc("main.func9")] (nint w) => w * 2);
    (n, h, e) = (new HandlerᴠNamed(@double), new Handlerᴠhidden(@double), new Handlerᴠerror(@double));
    fmt.Println(n._ᶠ<Named, Handler>()(1), h._ᶠ<hidden, Handler>()(2), e._ᶠ<error, Handler>()(3));
}

} // end main_package
