namespace go.NamedBytesConstConversion;

using fmt = fmt_package;
using static lib_package;

partial class lib_package {

partial struct Value /*[]byte*/;

} partial class libᴛValue {

public static bool IsNull(this Value v) { return ((@string)(slice<byte>)v) == "null"u8; }

} partial class lib_package {

public static void Print(Value v) { fmt.Printf("lib.Print %s len=%d\n"u8, v, len(v)); }

partial struct Name /*@string*/;

} partial class libᴛName {

public static nint Len(this Name n) { return len(n); }

} partial class lib_package {

public delegate nint Handler(nint _);

} partial class libᴛHandler {

public static nint Run(this Handler h, nint x) {
    if (h == nil) {
        return -1;
    }
    return h(x);
}

} // end libᴛHandler
