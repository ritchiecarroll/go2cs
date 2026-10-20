namespace go;

using asn1 = encoding.asn1_package;
using json = encoding.json_package;
using fmt = fmt_package;
using reflect = reflect_package;
using encoding;

partial class main_package {

public partial struct celsius /*num:float64*/;

public static (slice<byte>, error) MarshalJSON(this celsius c) {
    return (slice<byte>(fmt.Sprintf(@"""%.1fC"""u8, (float64)c)), default!);
}

partial struct reading {
    public @string Where;
    public celsius Temp;
}

partial struct record {
    public nint N;
    public @string Name;
}

partial interface namer {
    @string Name();
}

partial struct thing {
    internal @string s;
}

internal static @string Name(this thing t) {
    return t.s;
}

internal static void Main() {
    var (b, err) = json.Marshal(new reading("lab", 21.5D));
    fmt.Println(((@string)b), err);

    (var der, err) = asn1.Marshal(new record(7, "seven"));
    fmt.Printf("%x %v\n"u8, der, err);

    ref var back = ref heap(new record(), out var Ꮡback);
    (_, err) = asn1.Unmarshal(der, Ꮡback);
    fmt.Println(back.N, back.Name, err);

    var (i, ok) = reflect.TypeAssert<nint>(reflect.ValueOf((nint)(42)));
    fmt.Println("concrete:", i, ok);

    (var n, ok) = reflect.TypeAssert<namer>(reflect.ValueOf(new thing("t")));
    fmt.Println("interface over concrete:", n.Name(), ok);

    ref var held = ref heap<any>(out var Ꮡheld);

    held = new thing("held");
    (var h, ok) = reflect.TypeAssert<thing>(reflect.ValueOf(Ꮡheld).Elem());
    fmt.Println("interface value:", h.s, ok);

    ref var none = ref heap<namer>(out var Ꮡnone);
    (var z, ok) = reflect.TypeAssert<namer>(reflect.ValueOf(Ꮡnone).Elem());
    fmt.Println("nil interface value:", z == default!, ok);

    (var a, ok) = reflect.TypeAssert<array<nint>>(reflect.ValueOf(new nint[]{3, 4}.array()));
    fmt.Println("array:", a, ok);

    (var s, ok) = reflect.TypeAssert<@string>(reflect.ValueOf((nint)(42)));
    fmt.Printf("mismatch: %q %v\n"u8, s, ok);
}

} // end main_package
