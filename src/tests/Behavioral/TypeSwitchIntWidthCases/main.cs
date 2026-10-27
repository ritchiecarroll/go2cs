namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string whatAmI(any i) {
    switch (i.type()) {
    case nint: {
        return "I'm an int";
    }
    case nuint: {
        return "I'm a uint";
    }
    default: {
        return fmt.Sprintf("Don't know type %T"u8, i);
    }}

}

internal static UntypedInt c => 5;

internal static void Main() {
    fmt.Println("int(5):", whatAmI((nint)5));
    fmt.Println("int32(5):", whatAmI((int32)5));
    fmt.Println("int64(5):", whatAmI((int64)5));
    fmt.Println("uint(5):", whatAmI((nuint)5));
    fmt.Println("uint32(5):", whatAmI((uint32)5));
    fmt.Println("uint64(5):", whatAmI((uint64)5));
    fmt.Println("rune(5):", whatAmI((rune)5));
    fmt.Println("untyped 5:", whatAmI((nint)5));
    fmt.Println("untyped const:", whatAmI((nint)(c)));
    nint x = 5;
    fmt.Println("inferred var:", whatAmI(x));
    fmt.Println("len:", whatAmI(len("abc")));
}

} // end main_package
