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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nameˢ = (@string)"Name:"u8;
private static readonly object deadlineˢ = (@string)"deadline:"u8;
private static readonly object noDeadlineˢ = (@string)"no deadline"u8;

internal partial interface commandContext_type /*dyn*/ :
    TB
{
    (nint, bool) Deadline();
}

internal static void commandContext(TB t) {
    {
        var (td, ok) = t._<commandContext_type>(ᐧ); if (ok){
            var (d, _) = td.Deadline();
            fmt.Println(nameˢ, td.Name(), deadlineˢ, d);
        } else {
            fmt.Println(noDeadlineˢ);
        }
    }
}

partial struct gate {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sealedˢ = (@string)"sealed:"u8;

internal static void @private(this gate g) {
    fmt.Println(sealedˢ, g.name);
}

internal static @string Kind(this gate g) {
    return "gate:"u8 + g.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directˢ = (@string)"direct:"u8;
private static readonly object noDirectˢ = (@string)"no direct"u8;

internal partial interface direct_type /*dyn*/ {
    void @private();
    @string Kind();
}

internal static void direct(any v) {
    {
        var (d, ok) = v._<direct_type>(ᐧ); if (ok){
            d.@private();
            fmt.Println(directˢ, d.Kind());
        } else {
            fmt.Println(noDirectˢ);
        }
    }
}

internal static void Main() {
    commandContext(new harness(name: "cmd"u8, deadline: 100));
    direct(new gate(name: "v"u8));
    direct(Ꮡ(new gate(name: "p"u8)));
}

} // end main_package
