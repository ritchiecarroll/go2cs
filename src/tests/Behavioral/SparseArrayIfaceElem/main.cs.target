namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
}

partial struct circle {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string circleˢ = "circle"u8;

internal static @string name(this circle _) {
    return circleˢ;
}

partial struct square {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string squareˢ = "square"u8;

internal static @string name(this square _) {
    return squareˢ;
}

internal static UntypedInt kCircle => /* iota + 2 */ 2;
internal static UntypedInt kSquare => 3;
internal static UntypedInt kLast => 4;

internal static shape lookup(nint i) {
    return new golib.SparseArray<shape>{[kCircle] = new circle(nil), [kSquare] = new square(nil)
    }.array(4)[i];
}

internal static array<shape> registry = new golib.SparseArray<shape>{[kCircle] = new circle(nil), [kSquare] = new square(nil)
}.array(4);

partial struct hashKind /*num:nuint*/;

internal static hashKind hCircle => 5;
internal static hashKind hSquare => 6;

internal static array<shape> byKind = new golib.SparseArray<shape>{[(int)((nuint)hCircle)] = new circle(nil), [(int)((nuint)hSquare)] = new square(nil)
}.array(7);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object registryˢ = (@string)"registry:"u8;
private static readonly object byKindˢ = (@string)"byKind:"u8;

internal static void Main() {
    fmt.Println(lookup(kCircle).name());
    fmt.Println(lookup(kSquare).name());
    fmt.Println(lookup(0) == default!);
    fmt.Println(registryˢ, registry[kCircle].name(), registry[kSquare].name(), len(registry));
    fmt.Println(byKindˢ, byKind[hCircle].name(), byKind[hSquare].name(), len(byKind));
}

} // end main_package
