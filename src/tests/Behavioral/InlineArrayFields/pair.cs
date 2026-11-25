namespace go;

using System.Runtime.InteropServices;

partial class main_package {

[global::go.golib.GoInlineLayout(Size = 8, Align = 4), StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Pair {
    [FieldOffset(0)] public int32 X;
    [FieldOffset(4)] public int32 Y;
}

} // end main_package
