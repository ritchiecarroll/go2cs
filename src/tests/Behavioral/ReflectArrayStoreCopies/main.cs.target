namespace go;

using bytes = bytes_package;
using gob = encoding.gob_package;
using json = encoding.json_package;
using fmt = fmt_package;
using reflect = reflect_package;
using encoding;
using Δio = io_package;

partial class main_package {

partial struct holder {
    public array<nint> A = new(2);
}

partial struct marr {
    public /*[2]*/ map</*[2]*/ array<@string>, array<ж<float64>>> Marr;
}

internal static void main() {
    setMapIndexThenSetZero();
    setMapIndexThenSet();
    setMapIndexStruct();
    appendThenSetZero();
    sendThenSetZero();
    interfaceSlotThenSetZero();
    interfaceThenSetZero();
    gobRoundTrip();
    jsonMapOfArrays();
}

internal static void setMapIndexThenSetZero() {
    var m = new map<array<@string>, array<ж<float64>>>{};
    var mv = reflect.ValueOf(m);
    var keyP = reflect.New(reflect.TypeOf(new @string[]{}.array(2)));
    var elemP = reflect.New(reflect.TypeOf(new ж<float64>[]{}.array(2)));
    ref var a = ref heap<float64>(out var Ꮡa);
    a = 1.5D;
    ref var b = ref heap<float64>(out var Ꮡb);
    b = 2.5D;
    keyP.Elem().Index(0).SetString("x");
    keyP.Elem().Index(1).SetString("y");
    elemP.Elem().Index(0).Set(reflect.ValueOf(Ꮡa));
    elemP.Elem().Index(1).Set(reflect.ValueOf(Ꮡb));
    mv.SetMapIndex(keyP.Elem(), elemP.Elem());
    keyP.Elem().SetZero();
    elemP.Elem().SetZero();
    var v = m[array<@string>(["x", "y"]), () => new array<ж<float64>>(2)].Clone();
    fmt.Println("SetMapIndex then SetZero:", len(m), v[0] != nil && v[0].Value == 1.5D, v[1] != nil && v[1].Value == 2.5D);
}

internal static void setMapIndexThenSet() {
    var m = new map<@string, array<nint>>{};
    var mv = reflect.ValueOf(m);
    var elemP = reflect.New(reflect.TypeOf(new nint[]{}.array(2)));
    elemP.Elem().Index(0).SetInt(7);
    elemP.Elem().Index(1).SetInt(8);
    mv.SetMapIndex(reflect.ValueOf("k"), elemP.Elem());
    elemP.Elem().Set(reflect.ValueOf(array<nint>([1, 2])));
    fmt.Println("SetMapIndex then Set:", m["k", () => new array<nint>(2)]);
}

internal static void setMapIndexStruct() {
    var m = new map<@string, holder>{};
    var mv = reflect.ValueOf(m);
    var elemP = reflect.New(reflect.TypeOf(new holder()));
    elemP.Elem().Field(0).Index(1).SetInt(9);
    mv.SetMapIndex(reflect.ValueOf("k"), elemP.Elem());
    elemP.Elem().SetZero();
    fmt.Println("SetMapIndex struct then SetZero:", m["k"]);
}

internal static void appendThenSetZero() {
    var src = reflect.New(reflect.TypeOf(new nint[]{}.array(2)));
    src.Elem().Index(0).SetInt(3);
    var s = reflect.Append(reflect.ValueOf(GoReflect.WithElemDims(new array<nint>[]{}.slice(), 2)), src.Elem());
    src.Elem().SetZero();
    fmt.Println("Append then SetZero:", s.Interface());
}

internal static void sendThenSetZero() {
    var ch = new channel<array<nint>>(1, ChanCargo.Of(null, new nint[] { 2 }));
    var src = reflect.New(reflect.TypeOf(new nint[]{}.array(2)));
    src.Elem().Index(0).SetInt(4);
    reflect.ValueOf(ch).Send(src.Elem());
    src.Elem().SetZero();
    fmt.Println("Send then SetZero:", ᐸꟷ(ch));
}

internal static void interfaceSlotThenSetZero() {
    ref var x = ref heap<any>(out var Ꮡx);
    var src = reflect.New(reflect.TypeOf(new nint[]{}.array(2)));
    src.Elem().Index(0).SetInt(5);
    reflect.ValueOf(Ꮡx).Elem().Set(src.Elem());
    src.Elem().SetZero();
    fmt.Println("Set into interface then SetZero:", x);
}

internal static void interfaceThenSetZero() {
    var src = reflect.New(reflect.TypeOf(new nint[]{}.array(2)));
    src.Elem().Index(0).SetInt(6);
    var got = src.Elem().Interface();
    src.Elem().SetZero();
    fmt.Println("Interface then SetZero:", got);
}

internal static void gobRoundTrip() {
    ref var e = ref heap<float64>(out var Ꮡe);
    e = 2.71828D;
    ref var pi = ref heap<float64>(out var Ꮡpi);
    pi = 3.14159D;
    var @in = new marr(Marr: new map<array<@string>, array<ж<float64>>>{
        [array<@string>(["string1", "string2"])] = array<ж<float64>>([Ꮡpi, Ꮡe]),
        [array<@string>(["string2", "string1"])] = array<ж<float64>>([Ꮡe, Ꮡpi])
    }
    );
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = gob.NewEncoder(new bytes_BufferжWriter(Ꮡbuf)).Encode(@in); if (err != nil) {
            fmt.Println("encode:", err);
            return;
        }
    }
    ref var @out = ref heap(new marr(), out var Ꮡout);
    {
        var err = gob.NewDecoder(new bytes_BufferжReader(Ꮡbuf)).Decode(Ꮡout); if (err != nil) {
            fmt.Println("decode:", err);
            return;
        }
    }
    foreach (var (_, vᴛ1) in range(GoReflect.WithElemDims(new array<@string>[]{array<@string>(["string1", "string2"]), array<@string>(["string2", "string1"])}.slice(), 2))) {
        var k = vᴛ1.Clone();

        var v = @out.Marr[k, () => new array<ж<float64>>(2)].Clone();
        fmt.Println("gob", k, v[0] != nil && v[0].Value == @in.Marr[k, () => new array<ж<float64>>(2)][0].Value, v[1] != nil && v[1].Value == @in.Marr[k, () => new array<ж<float64>>(2)][1].Value);
    }
}

internal partial struct jsonMapOfArrays_v /*dyn*/ {
    public /*[2]*/ map<@string, array<nint>> M;
}

internal static void jsonMapOfArrays() {
    ref var v = ref heap(new jsonMapOfArrays_v(), out var Ꮡv);
    var err = json.Unmarshal(slice<byte>(@"{""M"":{""a"":[1,2],""b"":[3,4]}}"u8), Ꮡv); if (err != nil) {
        fmt.Println("json:", err);
        return;
    }
    fmt.Println("json", v.M["a", () => new array<nint>(2)], v.M["b", () => new array<nint>(2)]);
}

} // end main_package
