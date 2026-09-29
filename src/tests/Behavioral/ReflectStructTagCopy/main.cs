namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using System.Runtime.InteropServices;

partial class main_package {

[GoType] partial struct record {
    public nint Version;
    [GoTag(@"json:""name"" asn1:""optional,explicit,tag:0""")]
    public @string Name;
    [GoTag(@"json:""data,omitempty""")]
    public slice<byte> Data;
    public bool Untagged;
}

[GoType("[]nint")] partial struct intSET;

[GoType("[4]byte")] partial struct byteArray;

[GoType("map[@string, nint]")] partial struct stringMap;

[GoType("chan nint")] partial struct intChan;

[GoType("ж<nint>")] partial class intPtr;

[GoType("num:nint")] partial struct counter;

[GoType] partial struct empty {
}

[GoType] [StructLayout(LayoutKind.Explicit, Size = 24)] partial struct layout {
    [FieldOffset(0)] internal readonly empty pad;
    [FieldOffset(0)] internal uint32 small;
    [FieldOffset(8)] internal int64 big;
    [FieldOffset(16)] internal uint8 tail;
}

[GoType] partial struct inner {
    public uint32 X;
    public int64 Y;
}

[GoType] partial struct outer {
    public uint16 Head;
    internal partial ref inner inner { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string jsonˢ = "json"u8;
private static readonly @string asn1ˢ = "asn1"u8;
private static readonly object lookupJsonˢ = (@string)"lookup json:"u8;
private static readonly @string missingˢ = "missing"u8;
private static readonly object lookupMissingˢ = (@string)"lookup missing:"u8;
private static readonly object copySliceˢ = (@string)"copy slice:"u8;
private static readonly object copyShortSrcˢ = (@string)"copy short src:"u8;
private static readonly object helloˢ = (@string)"hello"u8;
private static readonly object copyStringˢ = (@string)"copy string:"u8;
private static readonly object copyWindowˢ = (@string)"copy window:"u8;
private static readonly object copyArrayˢ = (@string)"copy array:"u8;
private static readonly object copyNilDstˢ = (@string)"copy nil dst:"u8;
private static readonly object setSuffixˢ = (@string)"SET suffix:"u8;

internal static void Main() {
    var t = reflect.TypeOf(new record(nil));
    for (nint i = 0; i < t.NumField(); i++) {
        var fΔ1 = t.Field(i);
        fmt.Printf("%s raw=%q json=%q asn1=%q\n"u8, fΔ1.Name, ((@string)fΔ1.Tag), fΔ1.Tag.Get(jsonˢ), fΔ1.Tag.Get(asn1ˢ));
    }
    var f = t.Field(1);
    var (v, ok) = f.Tag.Lookup(jsonˢ);
    fmt.Println(lookupJsonˢ, v, ok);
    (v, ok) = f.Tag.Lookup(missingˢ);
    fmt.Println(lookupMissingˢ, v, ok);
    var dst = new slice<byte>(4);
    nint n = reflect.Copy(reflect.ValueOf(dst), reflect.ValueOf(new byte[]{1, 2, 3, 4, 5, 6}.slice()));
    fmt.Println(copySliceˢ, n, dst);
    var @short = new slice<byte>(8);
    n = reflect.Copy(reflect.ValueOf(@short), reflect.ValueOf(new byte[]{9, 8, 7}.slice()));
    fmt.Println(copyShortSrcˢ, n, @short);
    var sdst = new slice<byte>(3);
    n = reflect.Copy(reflect.ValueOf(sdst), reflect.ValueOf(helloˢ));
    fmt.Println(copyStringˢ, n, sdst);
    var backing = new nint[]{0, 0, 0, 0}.slice();
    var window = backing[1..3];
    n = reflect.Copy(reflect.ValueOf(window), reflect.ValueOf(new nint[]{5, 6, 7}.slice()));
    fmt.Println(copyWindowˢ, n, backing);
    ref var arr = ref heap(new array<nint>(3), out var Ꮡarr);
    n = reflect.Copy(reflect.ValueOf(Ꮡarr).Elem(), reflect.ValueOf(new nint[]{11, 12, 13, 14}.slice()));
    fmt.Println(copyArrayˢ, n, arr);
    slice<byte> nilDst = default!;
    n = reflect.Copy(reflect.ValueOf(nilDst), reflect.ValueOf(new byte[]{1, 2}.slice()));
    fmt.Println(copyNilDstˢ, n);
    foreach (var (_, vΔ1) in new any[]{new intSET(new nint[]{10}.slice()), new byteArray(new byte[4].array()), new stringMap(new map<@string, nint>{}), new intChan(0), new intPtr(Ꮡarr.at<nint>(0)), ((counter)0), new record(nil)}.slice()) {
        var rt = reflect.TypeOf(vΔ1);
        fmt.Printf("named %-12s name=%q pkg=%q\n"u8, rt.Kind(), rt.Name(), rt.PkgPath());
    }
    foreach (var (_, vΔ2) in new any[]{new nint[]{}.slice(), new byte[]{}.array(4), new map<@string, nint>{}, new channel<nint>(0), Ꮡarr, (nint)(0)}.slice()) {
        var rt = reflect.TypeOf(vΔ2);
        fmt.Printf("plain %-12s name=%q pkg=%q\n"u8, rt.Kind(), rt.Name(), rt.PkgPath());
    }
    @string nm = reflect.TypeOf(new intSET(new nint[]{}.slice())).Name();
    fmt.Println(setSuffixˢ, len(nm) >= 3 && nm.slice(len(nm) - 3) == "SET");
    var lt = reflect.TypeOf(new layout(nil));
    for (nint i = 0; i < lt.NumField(); i++) {
        var fΔ2 = lt.Field(i);
        fmt.Printf("offset %-6s %d\n"u8, fΔ2.Name, fΔ2.Offset);
    }
    var (of, _) = reflect.TypeOf(new outer(nil)).FieldByName("Y"u8);
    fmt.Printf("promoted Y offset=%d index=%v\n"u8, of.Offset, of.Index);
}

} // end main_package
