namespace go;

using fmt = fmt_package;

partial class main_package {


partial struct ptrElemsᴛ1 /*dyn*/ {
    internal nint @in;
    internal @string str;
    /*embed*/ internal error error;
}
internal static slice<ж<ptrElemsᴛ1>> ptrElems = new ж<ptrElemsᴛ1>[]{
    Ꮡ(new ptrElemsᴛ1(1, "one"u8, default!)),
    Ꮡ(new ptrElemsᴛ1(2, "two"u8, default!))
}.slice();


partial struct mapPtrValuesᴛ1 /*dyn*/ {
    internal nint n;
}
internal static map<@string, ж<mapPtrValuesᴛ1>> mapPtrValues = new map<@string, ж<mapPtrValuesᴛ1>>{
    ["a"u8] = Ꮡ(new mapPtrValuesᴛ1(10)),
    ["b"u8] = Ꮡ(new mapPtrValuesᴛ1(20))
};


partial struct nestedᴛ1 /*dyn*/ {
    internal @string tag;
}
internal static slice<slice<nestedᴛ1>> nested = new slice<nestedᴛ1>[]{
    new nestedᴛ1[]{new("x"u8), new("y"u8)}.slice()
}.slice();

internal static void Main() {
    foreach (var (_, e) in ptrElems) {
        fmt.Println((~e).@in, (~e).str, (~e).error == default!);
    }
    fmt.Println((~mapPtrValues["a"u8]).n + (~mapPtrValues["b"u8]).n);
    fmt.Println(nested[0][0].tag, nested[0][1].tag);
    ptrElems[0].Value.@in = 42;
    fmt.Println((~ptrElems[0]).@in);
}

} // end main_package
