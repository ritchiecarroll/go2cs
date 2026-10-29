namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using System.Runtime.InteropServices;

partial class main_package {

partial struct record {
    public nint Version;
    public @string Name; /*`json:"name" asn1:"optional,explicit,tag:0"`*/
    public slice<byte> Data; /*`json:"data,omitempty"`*/
    public bool Untagged;
}

partial struct intSET /*[]nint*/;

partial struct byteArray /*[4]byte*/;

partial struct stringMap /*map[@string, nint]*/;

partial struct intChan /*chan nint*/;

partial class intPtr /*ж<nint>*/;

partial struct counter /*num:nint*/;

partial struct empty {
}

[StructLayout(LayoutKind.Explicit, Size = 24)] partial struct layout {
    [FieldOffset(0)] internal readonly empty pad;
    [FieldOffset(0)] internal uint32 small;
    [FieldOffset(8)] internal int64 big;
    [FieldOffset(16)] internal uint8 tail;
}

partial struct inner {
    public uint32 X;
    public int64 Y;
}

partial struct outer {
    public uint16 Head;
    /*embed*/ internal inner inner;
}

internal static void Main() {
    var t = reflect.TypeOf(new record());

    for (nint i = 0; i < t.NumField(); i++) {
        var fΔ1 = t.Field(i);
        fmt.Printf("%s raw=%q json=%q asn1=%q\n"u8, fΔ1.Name, (@string)fΔ1.Tag, fΔ1.Tag.Get("json"), fΔ1.Tag.Get("asn1"));
    }

    var f = t.Field(1);
    var (v, ok) = f.Tag.Lookup("json");
    fmt.Println("lookup json:", v, ok);
    (v, ok) = f.Tag.Lookup("missing");
    fmt.Println("lookup missing:", v, ok);

    var dst = new slice<byte>(4);
    nint n = reflect.Copy(reflect.ValueOf(dst), reflect.ValueOf(slice<byte>([1, 2, 3, 4, 5, 6])));
    fmt.Println("copy slice:", n, dst);

    var @short = new slice<byte>(8);
    n = reflect.Copy(reflect.ValueOf(@short), reflect.ValueOf(slice<byte>([9, 8, 7])));
    fmt.Println("copy short src:", n, @short);

    var sdst = new slice<byte>(3);
    n = reflect.Copy(reflect.ValueOf(sdst), reflect.ValueOf("hello"));
    fmt.Println("copy string:", n, sdst);

    var backing = slice<nint>([0, 0, 0, 0]);
    var window = backing[1..3];
    n = reflect.Copy(reflect.ValueOf(window), reflect.ValueOf(slice<nint>([5, 6, 7])));
    fmt.Println("copy window:", n, backing);

    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    n = reflect.Copy(reflect.ValueOf(Ꮡarr).Elem(), reflect.ValueOf(slice<nint>([11, 12, 13, 14])));
    fmt.Println("copy array:", n, arr);

    slice<byte> nilDst = default;
    n = reflect.Copy(reflect.ValueOf(nilDst), reflect.ValueOf(slice<byte>([1, 2])));
    fmt.Println("copy nil dst:", n);

    foreach (var (_, vΔ1) in slice<any>([new intSET(new nint[]{10}.slice()), new byteArray(new byte[4].array()), new stringMap(new map<@string, nint>{}), new intChan(0), new intPtr(Ꮡarr.at<nint>(0)), (counter)0, new record()])) {
        var rt = reflect.TypeOf(vΔ1);
        fmt.Printf("named %-12s name=%q pkg=%q\n"u8, rt.Kind(), rt.Name(), rt.PkgPath());
    }

    foreach (var (_, vΔ2) in slice<any>([slice<nint>([]), new byte[]{}.array(4), new map<@string, nint>{}, new channel<nint>(0), Ꮡarr, (nint)0])) {
        var rt = reflect.TypeOf(vΔ2);
        fmt.Printf("plain %-12s name=%q pkg=%q\n"u8, rt.Kind(), rt.Name(), rt.PkgPath());
    }

    @string nm = reflect.TypeOf(new intSET(new nint[]{}.slice())).Name();
    fmt.Println("SET suffix:", len(nm) >= 3 && nm.slice(len(nm) - 3) == "SET");

    var lt = reflect.TypeOf(new layout());
    for (nint i = 0; i < lt.NumField(); i++) {
        var fΔ2 = lt.Field(i);
        fmt.Printf("offset %-6s %d\n"u8, fΔ2.Name, fΔ2.Offset);
    }

    var (of, _) = reflect.TypeOf(new outer()).FieldByName("Y");
    fmt.Printf("promoted Y offset=%d index=%v\n"u8, of.Offset, of.Index);
}

} // end main_package
