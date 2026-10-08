namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct A {
    internal nint v;
}

partial struct B {
    internal nint v;
}

partial struct C {
    internal nint v;
}

internal static @string describe(any c) {
    switch (c.type()) {
    case A cΔ1: {
        return fmt.Sprintf("A:%d"u8, cΔ1.v);
    }
    case C cΔ1: {
        break;
    }
    case B cΔ1: {
        return fmt.Sprintf("B:%d"u8, cΔ1.v);
    }}
    return fmt.Sprintf("other:%v"u8, c);
}

internal static void Main() {
    fmt.Println(describe(new A(1)));
    fmt.Println(describe(new B(2)));
    fmt.Println(describe(new C(3)));
    fmt.Println(describe((nint)(99)));
}

} // end main_package
