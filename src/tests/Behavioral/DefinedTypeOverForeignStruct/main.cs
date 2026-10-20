namespace go;

using fmt = fmt_package;
using ptlike = DefinedTypeOverForeignStruct.ptlike_package;
using DefinedTypeOverForeignStruct;

partial class main_package {

partial struct alias /*global::go.DefinedTypeOverForeignStruct.ptlike_package.Outer*/;

internal static @string describe(this ref alias a) {
    return fmt.Sprintf("%s/%d"u8, a.Name, a.In.Len());
}

internal static void Main() {
    var a = (alias)ptlike.New("first", 1, 2, 3);

    fmt.Println(a.Name, a.In.Len());
    fmt.Println(a.describe());

    a.Name = "second";
    fmt.Println(a.Name);

    a.In.Push(4);
    a.In.Push(5);
    fmt.Println(a.In.Len(), a.In.Vals);
    fmt.Println(a.describe());

    a.In.Vals[0] = 99;
    fmt.Println(a.In.Vals);

    var o = (ptlike.Outer)a;
    fmt.Println(o.Name, o.In.Vals);

    alias z = default!;
    fmt.Println(z.Name == ""u8, z.In.Len());
}

} // end main_package
