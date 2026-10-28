namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    var e1 = @unsafe.StringData(""u8);
    @string empty = default;
    var e2 = @unsafe.StringData(empty);

    fmt.Println(e1 == nil, e1 == e2);
    fmt.Println(e1.OrTypedNil());

    var q = @unsafe.StringData("abc"u8);
    @string t = fmt.Sprintf("%v"u8, q.OrTypedNil());
    fmt.Println(q != nil, len(t) > 2, t[0] == '0', t[1] == 'x');

    @unsafe.Pointer z = (@unsafe.Pointer)(uintptr)0;
    @unsafe.Pointer back = (@unsafe.Pointer)(uintptr)z;

    fmt.Println(z == nil, back == nil, (uintptr)z == 0);
}

} // end main_package
