namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial struct Sum /*[4]byte*/;

internal static (Sum, error) build(/*[4]*/ array<byte> src) {
    src = src.Clone();

    if (src[0] == 0) {
        return (new Sum(new byte[4].array()), errors.New("B was read before it was initialized"));
    }

    return ((Sum)src, default);
}

internal static Sum must(Sum s, error err) {
    s = s.Clone();

    if (err != nil) {
        throw panic(err);
    }

    return s.Clone();
}

public static Sum A;
internal static void initᴛA() {
    var (ᴛ1, ᴛ2) = build(B);
    A = must(ᴛ1, ᴛ2);
}

public static readonly ж<Sum> ᏑAddressed = new StaticBox<Sum>(default(Sum));
public static ref Sum Addressed => ref ((StaticBox<Sum>)ᏑAddressed).ValueSlot;
internal static void initᴛAddressed() {
    var (ᴛ3, ᴛ4) = build(B);
    Addressed = must(ᴛ3, ᴛ4);
}

internal static void Main() {
    var p = ᏑAddressed;
    fmt.Println("A:", A);
    fmt.Println("Addressed:", p.Value);
}

} // end main_package
