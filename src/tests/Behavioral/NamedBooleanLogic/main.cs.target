namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct boolVal /*bool*/;

internal static bool isSet(this boolVal b) {
    return (bool)b;
}

partial interface Value {
    bool isSet();
}

internal static Value unary(Value x) {
    var y = x._<boolVal>();
    return ((boolVal)(!(bool)y));
}

internal static Value binary(@string op, Value x, Value y) {
    var a = x._<boolVal>();
    var b = y._<boolVal>();
    if (op == "and"u8) {
        return ((boolVal)((bool)a  &&  (bool)b));
    }
    return ((boolVal)((bool)a  ||  (bool)b));
}

internal static void Main() {
    Value t = ((boolVal)true);
    Value f = ((boolVal)false);

    fmt.Println(unary(t).isSet());
    fmt.Println(unary(f).isSet());
    fmt.Println(binary("and", t, f).isSet());
    fmt.Println(binary("and", t, t).isSet());
    fmt.Println(binary("or", t, f).isSet());
    fmt.Println(binary("or", f, f).isSet());
}

} // end main_package
