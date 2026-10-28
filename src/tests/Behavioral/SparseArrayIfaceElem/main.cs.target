namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface shape {
    @string name();
}

partial struct circle {
}

internal static @string name(this circle _) {
    return "circle";
}

partial struct square {
}

internal static @string name(this square _) {
    return "square";
}

internal static UntypedInt kCircle => /* iota + 2 */ 2;
internal static UntypedInt kSquare => 3;
internal static UntypedInt kLast => 4;

internal static shape lookup(nint i) {
    return new golib.SparseArray<shape>{[kCircle] = new circle(), [kSquare] = new square()
    }.array(4)[i];
}

internal static array<shape> registry = new golib.SparseArray<shape>{[kCircle] = new circle(), [kSquare] = new square()
}.array(4);

partial struct hashKind /*num:nuint*/;

internal static hashKind hCircle => 5;
internal static hashKind hSquare => 6;

internal static array<shape> byKind = new golib.SparseArray<shape>{[(int)((nuint)hCircle)] = new circle(), [(int)((nuint)hSquare)] = new square()
}.array(7);

internal static void Main() {
    fmt.Println(lookup(kCircle).name());
    fmt.Println(lookup(kSquare).name());
    fmt.Println(lookup(0) == nil);

    fmt.Println("registry:", registry[kCircle].name(), registry[kSquare].name(), len(registry));

    fmt.Println("byKind:", byKind[hCircle].name(), byKind[hSquare].name(), len(byKind));
}

} // end main_package
