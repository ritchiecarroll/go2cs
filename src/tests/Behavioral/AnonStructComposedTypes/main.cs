namespace go;

using fmt = fmt_package;

partial class main_package {


partial struct ptrElemsᴛ1 /*dyn*/ {
    internal nint @in;
    internal @string str;
    /*embed*/ internal error error;
}
[GoLazy] partial class lazyᴛptrElems { internal static slice<ж<ptrElemsᴛ1>> ptrElems = [
    Ꮡ(new ptrElemsᴛ1(1, "one", default)),
    Ꮡ(new ptrElemsᴛ1(2, "two", default))
]; }


partial struct mapPtrValuesᴛ1 /*dyn*/ {
    internal nint n;
}
internal static map<@string, ж<mapPtrValuesᴛ1>> mapPtrValues = new map<@string, ж<mapPtrValuesᴛ1>>{
    ["a"] = Ꮡ(new mapPtrValuesᴛ1(10)),
    ["b"] = Ꮡ(new mapPtrValuesᴛ1(20))
};


partial struct nestedᴛ1 /*dyn*/ {
    internal @string tag;
}
internal static slice<slice<nestedᴛ1>> nested = [
    [new("x"), new("y")]
];

internal static void main() {
    foreach (var (_, e) in range(ptrElems)) {
        fmt.Println((~e).@in, (~e).str, (~e).error == nil);
    }

    fmt.Println((~mapPtrValues["a"]).n + (~mapPtrValues["b"]).n);
    fmt.Println(nested[0][0].tag, nested[0][1].tag);

    ptrElems[0].Value.@in = 42;
    fmt.Println((~ptrElems[0]).@in);
}

} // end main_package
