namespace go.NamedBytesConstConversion;

using fmt = fmt_package;

partial class lib_package {

partial struct Value /*[]byte*/;

public static bool IsNull(this Value v) { return ((@string)(slice<byte>)v) == "null"u8; }

public static void Print(Value v) { fmt.Printf("lib.Print %s len=%d\n"u8, v, len(v)); }

partial struct Name /*@string*/;

public static nint Len(this Name n) { return len(n); }

public delegate nint Handler(nint _);

public static nint Run(this Handler h, nint x) {
    if (h == nil) {
        return -1;
    }
    return h(x);
}

} // end lib_package
