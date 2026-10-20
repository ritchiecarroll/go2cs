namespace go;

using fmt = fmt_package;

partial class main_package {

// type Stringy is a methodless func type — rendered inline as its base delegate

internal static @string foo() {
    return "Stringy function";
}

internal static void takesAFunction(Func<@string> foo) {
    fmt.Printf("takesAFunction \u0049: %v\n"u8, foo());
}

internal static Func<@string> returnsAFunction() {
    return () => {
        fmt.Printf("Inner stringy function\n"u8);
        return "bar";
    };
}

internal static (nint, error) half(nint n) {
    if (n % 2 != 0) {
        return (0, fmt.Errorf("odd"u8));
    }
    return (n / 2, default!);
}

internal static void Main() {
    error probe(nint n, error errΔ1) {
        if (errΔ1 != default!) {
            return errΔ1;
        }
        (var m, errΔ1) = half(n);
        if (errΔ1 != default!) {
            return errΔ1;
        }
        (var k, errΔ1) = half(m);
        fmt.Println("halved", m, k);
        return errΔ1;
    }
    fmt.Println(probe(8, default!), probe(3, default!));

    takesAFunction(new Func<@string>(foo));
    Func<@string> f = returnsAFunction();
    f();
    Func<@string> baz = () => "anonymous stringy\n";
    fmt.Print(baz());
    fmt.Println(cached(), cached());
    loader = (slice<byte>, error) (@string name) => (slice<byte>(name), default!);
    var (b, err) = loader("zone");
    fmt.Println(len(b), err == default!);
}

internal static Func<@string, (slice<byte>, error)> loader;

internal static Func<nint> cached = memo(() => {
    nint n = default!;
    for (nint i = 1; i <= 4; i++) {
        n += i;
    }
    return n;
});

internal static Func<nint> memo(Func<nint> f) {
    var done = false;
    nint v = 0;
    return () => {
        if (!done) {
            v = f();
            done = true;
        }
        return v;
    };
}

} // end main_package
