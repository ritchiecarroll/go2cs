namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Namer {
    @string Name();
}

partial struct T {
    internal nint n;
}

public static @string Name(this ж<T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt == nil) {
        return "nil T";
    }
    return fmt.Sprint("T", t.n);
}

partial struct U {
}

public static @string Name(this ref U _) {
    return "U";
}

internal static @string classify(Namer i) {
    switch (i.type()) {
    case null: {
        return "nil interface";
    }
    case ж<T> v: {
        return fmt.Sprint("*T, nil pointee: ", v == nil, ", Name: ", v.Name());
    }
    default: {
        var v = i;
        return "default";
    }}
}

internal static @string classifyAny(any x) {
    switch (x.type()) {
    case null: {
        return "nil any";
    }
    case ж<T> v: {
        return fmt.Sprint("*T in any, nil pointee: ", v == nil);
    }
    default: {
        var v = x;
        return "default";
    }}
}

internal static @string multi(Namer i) {
    switch (i.type()) {
    case ж<T> _:
    case ж<U> _: {
        var v = i;
        return fmt.Sprint("*T or *U, interface nil: ", v == default!, ", Name: ", v.Name());
    }
    case null: {
        return "nil interface";
    }}
    return "default";
}

internal static void Main() {
    ж<T> p = default!;
    fmt.Println(classify(new TжNamer(p)));
    fmt.Println(classify(new TжNamer(Ꮡ(new T(1)))));
    fmt.Println(classify(default!));

    fmt.Println(classifyAny(p.OrTypedNil()));
    fmt.Println(classifyAny(default!));

    ж<U> u = default!;
    fmt.Println(multi(new TжNamer(p)));
    fmt.Println(multi(new UжNamer(u)));
    fmt.Println(multi(default!));
}

} // end main_package
