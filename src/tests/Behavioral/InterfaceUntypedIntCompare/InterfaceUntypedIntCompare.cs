namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Element {
    public any Value;
}

internal static ж<Element> push(any v) {
    return Ꮡ(new Element(Value: v));
}

internal static void Main() {
    var e = push((nint)1);

    if (!AreEqual((~e).Value, (nint)1)) {
        fmt.Println("BUG: e.Value != 1");
    } else {
        fmt.Println("ok: e.Value == 1");
    }

    fmt.Println(AreEqual((~e).Value, (nint)1));
    fmt.Println(!AreEqual((~e).Value, (nint)1));
    fmt.Println(AreEqual((nint)1, (~e).Value));
    fmt.Println(AreEqual((~e).Value, (nint)2));

    var n = push((nint)(-5));
    fmt.Println(AreEqual((~n).Value, (nint)(-5)));
    fmt.Println(AreEqual((~n).Value, (nint)5));

    any x = (nint)42;
    fmt.Println(AreEqual(x, (nint)42));
    fmt.Println(!AreEqual(x, (nint)42));
}

} // end main_package
