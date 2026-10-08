namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr /*num:nint*/;

internal static @string Error(this myErr e) {
    return fmt.Sprintf("myErr(%d)"u8, (nint)e);
}

partial struct tag /*@string*/;

internal static @string Name(this tag t) {
    return "tag:"u8 + ((@string)t);
}

partial interface named {
    @string Name();
}

partial struct box {
    internal error err;
}

partial struct holder {
    internal named n;
}

internal static void Main() {
    var b = new box(((myErr)7));
    fmt.Println(b.err.Error());
    var b2 = new box(err: ((myErr)42));
    fmt.Println(b2.err.Error());
    var h = new holder(((tag)(@string)"x"u8));
    fmt.Println(h.n.Name());
}

} // end main_package
