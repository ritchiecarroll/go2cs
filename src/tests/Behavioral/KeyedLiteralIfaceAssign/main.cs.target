namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

public partial struct code /*num:uint32*/;

} partial class mainᴛcode {

public static @string String(this code c) {
    return fmt.Sprintf("code(%d)"u8, (uint32)c);
}

} partial class main_package {

partial struct failure {
    public code Code;
    public @string Msg;
}

} partial class mainᴛfailure {

internal static @string Error(this failure f) {
    return f.Code.String() + ": "u8 + f.Msg;
}

} partial class main_package {

internal static void main() {
    error err = default;
    err = new failure(Code: ((code)7), Msg: "boom");
    fmt.Println(err);
}

} // end main_package
