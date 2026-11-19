namespace go;

using fmt = fmt_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using static main_package;

partial class main_package {

partial struct noCopy {
}

} partial class mainᴛnoCopy {

internal static void Lock(this ref noCopy _) { }
internal static void Unlock(this ref noCopy _) { }

} partial class main_package {

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Counter {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal int64 v;
}

} partial class mainᴛCounter {

public static void Add(this ref Counter c, int64 n) { c.v += n; }
public static int64 Value(this ref Counter c) { return c.v; }

} partial class main_package {

internal static void main() {
    Counter c = default;
    c.Add(5);
    c.Add(3);
    fmt.Println("counter:", c.Value());

    Counter d = default;
    fmt.Println("zero:", d.Value());
}

} // end main_package
