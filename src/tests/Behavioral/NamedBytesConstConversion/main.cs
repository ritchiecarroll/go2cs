namespace go;

using fmt = fmt_package;
using lib = NamedBytesConstConversion.lib_package;
using wrap = NamedBytesConstConversion.wrap_package;
using NamedBytesConstConversion;

partial class main_package {

partial struct Value /*[]byte*/;

partial struct Label /*@string*/;

internal static readonly @string @null = "null"u8;

internal static readonly Label typedLabel = "label"u8;

internal static lib.Value global = ((lib.Value)"global"u8);

internal static void show(@string name, slice<byte> b) { fmt.Printf("%s: %q len=%d\n"u8, name, b, len(b)); }

internal static lib.Value ret() { return (lib.Value)"returned"u8; }

internal static void Main() {
    show("local literal", (Value)slice<byte>((@string)"null"u8));
    show("local const", (Value)slice<byte>(@null));
    show("local concat", (Value)slice<byte>("nu"u8 + "ll"u8));
    show("local typed const", (Value)slice<byte>((@string)typedLabel));
    show("local raw", (Value)slice<byte>((@string)@"raw"u8));
    show("local high byte", (Value)slice<byte>((@string)((@string)(new byte[]{0xff, 0x00}))));

    show("imported literal", (lib.Value)slice<byte>((@string)"true"u8));
    show("re-exported literal", (lib.Value)"null"u8);
    show("re-exported const", (lib.Value)@null);
    show("package-level", global);
    show("returned", ret());

    var m = (lib.Value)"abc"u8;
    m[0] = (byte)'X';
    var n = (lib.Value)"abc"u8;
    show("mutated", m);
    show("fresh", n);

    lib.Print((lib.Value)"argument"u8);
    fmt.Println(((lib.Value)"x"u8).IsNull(), ((lib.Value)"null"u8).IsNull(), ((lib.Value)@null).IsNull());

    @string s = "variable";
    show("variable", (lib.Value)s);
}

internal static nint @double(nint x) { return x * 2; }

[GoInit] internal static void init() {
    @string s = "variable";
    fmt.Println(((lib.Name)(@string)"constant").Len(), ((lib.Name)@null).Len(), ((lib.Name)s).Len());

    var f = @double;
    fmt.Println(((lib.Handler)f).Run(3), ((lib.Handler)@double).Run(4), ((lib.Handler)((nint x) => x + 1)).Run(5), ((lib.Handler)default).Run(6));
}

} // end main_package
