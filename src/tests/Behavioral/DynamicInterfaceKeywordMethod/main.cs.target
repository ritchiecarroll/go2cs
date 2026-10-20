namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface TB {
    @string Name();
    void @private();
}

partial struct harness {
    internal @string name;
    internal nint deadline;
}

internal static @string Name(this harness h) {
    return h.name;
}

internal static void @private(this harness h) {
}

internal static (nint, bool) Deadline(this harness h) {
    return (h.deadline, true);
}

internal partial interface commandContext_type /*dyn*/ :
    TB
{
    (nint, bool) Deadline();
}

internal static void commandContext(TB t) {
    {
        var (td, ok) = t._<commandContext_type>(ᐧ); if (ok){
            var (d, _) = td.Deadline();
            fmt.Println("Name:", td.Name(), "deadline:", d);
        } else {
            fmt.Println("no deadline");
        }
    }
}

partial struct gate {
    internal @string name;
}

internal static void @private(this gate g) {
    fmt.Println("sealed:", g.name);
}

internal static @string Kind(this gate g) {
    return "gate:"u8 + g.name;
}

internal partial interface direct_type /*dyn*/ {
    void @private();
    @string Kind();
}

internal static void direct(any v) {
    {
        var (d, ok) = v._<direct_type>(ᐧ); if (ok){
            d.@private();
            fmt.Println("direct:", d.Kind());
        } else {
            fmt.Println("no direct");
        }
    }
}

internal static void Main() {
    commandContext(new harness(name: "cmd", deadline: 100));

    direct(new gate(name: "v"));

    direct(Ꮡ(new gate(name: "p")));
}

} // end main_package
