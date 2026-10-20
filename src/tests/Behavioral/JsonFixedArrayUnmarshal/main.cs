namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using encoding;

partial class main_package {

internal partial struct main_point /*dyn*/ {
    public nint X; /*`json:"x"`*/
    public nint Y; /*`json:"y"`*/
}

internal static void Main() {
    ref var a = ref heap(new array<any>(1), out var Ꮡa);
    fmt.Println("iface1:", unmarshal("[42]", Ꮡa), a);

    ref var b = ref heap(new array<nint>(2), out var Ꮡb);
    fmt.Println("int2:", unmarshal("[7,8]", Ꮡb), b);

    ref var c = ref heap(new array<nint>(2), out var Ꮡc);
    fmt.Println("trunc:", unmarshal("[1,2,3,4]", Ꮡc), c);

    ref var d = ref heap(new array<@string>(3), out var Ꮡd);
    fmt.Println("zerofill:", unmarshal(@"[""x""]", Ꮡd), d);

    ref var e = ref heap(new array<nint>(2), out var Ꮡe);
    e = new nint[]{9, 9}.array();
    fmt.Println("empty:", unmarshal("[]", Ꮡe), e);

    ref var f = ref heap(new array<array<nint>>(2, () => new(2)), out var Ꮡf);
    fmt.Println("nested:", unmarshal("[[1,2],[3,4]]", Ꮡf), f);
    ref var g = ref heap(new array<main_point>(2), out var Ꮡg);
    fmt.Println("structs:", unmarshal(@"[{""x"":1,""y"":2},{""x"":3,""y"":4}]", Ꮡg), g);

    ref var h = ref heap<slice<nint>>(out var Ꮡh);
    fmt.Println("slice:", unmarshal("[3,4,5]", Ꮡh), h);

    ref var i = ref heap<any>(out var Ꮡi);
    fmt.Println("anyiface:", unmarshal("[1,2]", Ꮡi), i);

    ref var j = ref heap(new nint(), out var Ꮡj);
    fmt.Println("badtarget:", unmarshal("[1,2]", Ꮡj), j);

    ref var k = ref heap(new array<nint>(2), out var Ꮡk);
    fmt.Println("badmap:", unmarshal(@"{""a"":1}", Ꮡk), k);
}

internal static @string unmarshal(@string data, any v) {
    {
        var err = json.Unmarshal(slice<byte>(data), v); if (err != default!) {
            return "err("u8 + err.Error() + ")"u8;
        }
    }
    return "ok";
}

} // end main_package
