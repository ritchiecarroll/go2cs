namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct relationship /*@string*/;

internal static readonly relationship equivalent = "equivalent"u8;
internal static readonly relationship moreGeneral = "moreGeneral"u8;
internal static readonly relationship moreSpecific = "moreSpecific"u8;

internal static readonly relationship opLoad = "opLoad"u8;
internal static readonly relationship opStore = "opStore"u8;
internal static readonly relationship opDelete = "opDelete"u8;

internal static readonly @string prefix = "op"u8;

internal static @string tag(this relationship r) {
    return "rel:"u8 + ((@string)r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sameˢ = "same"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string describe(relationship r) {
    if (r == equivalent) {
        return sameˢ;
    }
    if (r == moreGeneral || r == moreSpecific) {
        return "related:"u8 + ((@string)r);
    }
    return otherˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string opLoadˢ = "opLoad"u8;
private static readonly @string opLocalˢ = "opLocal"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly relationship localRelᶜ = "moreSpecific"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string plainᶜ = "plain"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly relationship localOpᶜ = "opLocal"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string untypedOpᶜ = "opUntyped";

internal static void Main() {
    fmt.Println(describe(equivalent));
    fmt.Println(describe(moreGeneral));
    fmt.Println(describe(((relationship)(@string)"x"u8)));
    relationship localRel = localRelᶜ;
    fmt.Println(describe(localRel));
    fmt.Println(equivalent.tag());
    @string plain = plainᶜ;
    fmt.Println(plain);
    fmt.Println(opLoad == ((relationship)(@string)opLoadˢ), opStore == opLoad, opStore.tag(), opDelete.tag());
    fmt.Println(describe(opLoad));
    relationship localOp = localOpᶜ;
    fmt.Println(localOp.tag(), localOp == ((relationship)(@string)opLocalˢ));
    @string untypedOp = untypedOpᶜ;
    fmt.Println(untypedOp, len(untypedOp));
}

} // end main_package
