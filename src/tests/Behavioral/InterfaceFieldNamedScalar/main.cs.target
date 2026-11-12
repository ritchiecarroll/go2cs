namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct myErr /*num:nint*/;

} partial class mainᴛmyErr {

internal static @string Error(this myErr e) {
    return fmt.Sprintf("myErr(%d)"u8, (nint)e);
}

} partial class main_package {

partial struct tag /*@string*/;

} partial class mainᴛtag {

internal static @string Name(this tag t) { return "tag:"u8 + ((@string)t); }

} partial class main_package {

partial interface named {
    @string Name();
}

partial struct box {
    internal error err;
}

partial struct holder {
    internal named n;
}

internal static void main() {
    var b = new box((myErr)7);
    fmt.Println(b.err.Error());

    var b2 = new box(err: ((myErr)42));
    fmt.Println(b2.err.Error());

    var h = new holder((tag)(@string)"x");
    fmt.Println(h.n.Name());
}

} // end main_package
