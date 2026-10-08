namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct code /*num:uint32*/;

public static @string String(this code c) {
    return fmt.Sprintf("code(%d)"u8, (uint32)c);
}

partial struct failure {
    public code Code;
    public @string Msg;
}

internal static @string Error(this failure f) {
    return f.Code.String() + ": "u8 + f.Msg;
}

internal static void Main() {
    error err = default!;
    err = new failure(Code: ((code)7), Msg: "boom"u8);
    fmt.Println(err);
}

} // end main_package
