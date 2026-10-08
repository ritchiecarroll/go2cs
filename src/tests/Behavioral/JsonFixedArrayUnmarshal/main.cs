namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using encoding;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object iface1ˢ = (@string)"iface1:"u8;
private static readonly object int2ˢ = (@string)"int2:"u8;
private static readonly object truncˢ = (@string)"trunc:"u8;
private static readonly object zerofillˢ = (@string)"zerofill:"u8;
private static readonly object emptyˢ = (@string)"empty:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object structsˢ = (@string)"structs:"u8;
private static readonly @string x1Y2X3Y4ˢ = @"[{""x"":1,""y"":2},{""x"":3,""y"":4}]"u8;
private static readonly object sliceˢ = (@string)"slice:"u8;
private static readonly object anyifaceˢ = (@string)"anyiface:"u8;
private static readonly object badtargetˢ = (@string)"badtarget:"u8;
private static readonly object badmapˢ = (@string)"badmap:"u8;

internal partial struct main_point /*dyn*/ {
    public nint X; /*`json:"x"`*/
    public nint Y; /*`json:"y"`*/
}

internal static void Main() {
    ref var a = ref heap(new array<any>(1), out var Ꮡa);
    fmt.Println(iface1ˢ, unmarshal("[42]"u8, Ꮡa), a);
    ref var b = ref heap(new array<nint>(2), out var Ꮡb);
    fmt.Println(int2ˢ, unmarshal("[7,8]"u8, Ꮡb), b);
    ref var c = ref heap(new array<nint>(2), out var Ꮡc);
    fmt.Println(truncˢ, unmarshal("[1,2,3,4]"u8, Ꮡc), c);
    ref var d = ref heap(new array<@string>(3), out var Ꮡd);
    fmt.Println(zerofillˢ, unmarshal(@"[""x""]"u8, Ꮡd), d);
    ref var e = ref heap(new array<nint>(2), out var Ꮡe);
    e = new nint[]{9, 9}.array();
    fmt.Println(emptyˢ, unmarshal("[]"u8, Ꮡe), e);
    ref var f = ref heap(new array<array<nint>>(2, () => new(2)), out var Ꮡf);
    fmt.Println(nestedˢ, unmarshal("[[1,2],[3,4]]"u8, Ꮡf), f);
    ref var g = ref heap(new array<main_point>(2), out var Ꮡg);
    fmt.Println(structsˢ, unmarshal(x1Y2X3Y4ˢ, Ꮡg), g);
    ref var h = ref heap<slice<nint>>(out var Ꮡh);
    fmt.Println(sliceˢ, unmarshal("[3,4,5]"u8, Ꮡh), h);
    ref var i = ref heap<any>(out var Ꮡi);
    fmt.Println(anyifaceˢ, unmarshal("[1,2]"u8, Ꮡi), i);
    ref var j = ref heap(new nint(), out var Ꮡj);
    fmt.Println(badtargetˢ, unmarshal("[1,2]"u8, Ꮡj), j);
    ref var k = ref heap(new array<nint>(2), out var Ꮡk);
    fmt.Println(badmapˢ, unmarshal(@"{""a"":1}"u8, Ꮡk), k);
}

internal static @string unmarshal(@string data, any v) {
    {
        var err = json.Unmarshal(slice<byte>(data), v); if (err != default!) {
            return "err("u8 + err.Error() + ")"u8;
        }
    }
    return "ok"u8;
}

} // end main_package
