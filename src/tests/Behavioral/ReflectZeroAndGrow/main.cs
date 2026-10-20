namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct NS /*@string*/;

partial struct NA /*[2]uint8*/;

partial struct NB /*[]byte*/;

partial struct NI /*num:nint*/;

public partial struct inner {
    public NS S;
    public nint N;
}

partial struct outer {
    public inner I;
    public NA A;
    public ж<nint> P;
    public slice<nint> Sl;
    public map<@string, nint> M;
    public error E;
}

internal static void z(@string label, any v) {
    var rv = reflect.ValueOf(v);
    fmt.Printf("%-22s kind=%-9v IsZero=%-5v"u8, label, rv.Kind(), rv.IsZero());
    var exprᴛ1 = rv.Kind();
    if (exprᴛ1 == reflect.ΔString || exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.Array || exprᴛ1 == reflect.Map) {
        fmt.Printf(" Len=%d"u8, rv.Len());
    }

    fmt.Println();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string valˢ = "val"u8;

internal static void Main() {
    z("string non-empty", "val");
    z("string empty", "");
    z("NS non-empty", ((NS)(@string)valˢ));
    z("NS empty", ((NS)(@string)""u8));
    z("NI non-zero", ((NI)3));
    z("NI zero", ((NI)0));

    z("[2]uint8{1,2}", new uint8[]{1, 2}.array());
    z("[2]uint8{}", new uint8[]{}.array(2));
    z("NA{1,2}", new NA(new uint8[]{1, 2}.array()));
    z("NA{}", new NA(new uint8[2].array()));
    z("[3]string{'','',''}", new @string[]{}.array(3));
    z("[2]NS{'a',''}", new NS[]{((NS)(@string)"a"u8), ((NS)(@string)""u8)}.array());

    z("NB(nil)", ((NB)default!));
    z("NB{}", new NB(new byte[]{}.slice()));
    z("[]byte(nil)", slice<byte>(default!));
    z("[]byte{0}", new byte[]{0}.slice());
    map<@string, nint> nilMap = default!;
    z("map nil", nilMap);
    z("map empty", new map<@string, nint>{});
    z("*int nil", ((ж<nint>)nil));

    z("inner{}", new inner(nil));
    z("inner{S:val}", new inner(S: ((NS)(@string)valˢ)));
    z("inner{N:1}", new inner(N: 1));
    z("outer{}", new outer(nil));
    z("outer{A:NA{0,1}}", new outer(A: new NA(new uint8[]{0, 1}.array())));
    z("outer{I.S:x}", new outer(I: new inner(S: ((NS)(@string)"x"u8))));
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 0;
    z("outer{P:&n}", new outer(P: Ꮡn));
    z("outer{Sl:[]}", new outer(Sl: new nint[]{}.slice()));

    fmt.Println("any(NS(\"\")) IsZero:", reflect.ValueOf(((any)((NS)(@string)""u8))).IsZero());
    fmt.Println("any(NS(\"v\")) IsZero:", reflect.ValueOf(((any)((NS)(@string)"v"u8))).IsZero());

    z("outer{E:nil}", new outer(nil));
    z("outer{E:err}", new outer(E: errors.New("x")));
    var ev = reflect.ValueOf(Ꮡ(new outer(nil))).Elem().FieldByName("E");
    fmt.Println("field E kind:", ev.Kind(), "IsZero:", ev.IsZero());
    var ev2 = reflect.ValueOf(Ꮡ(new outer(E: errors.New("x")))).Elem().FieldByName("E");
    fmt.Println("field E set kind:", ev2.Kind(), "IsZero:", ev2.IsZero());

    ref var s = ref heap<slice<byte>>(out var Ꮡs);
    s = new byte[]{1, 2, 3, 4}.slice();
    var rv = reflect.ValueOf(Ꮡs).Elem();
    rv.Grow(1);
    fmt.Println("Grow(1) len/cap:", rv.Len(), rv.Cap() >= 5, len(s), cap(s) >= 5);

    ref var empty = ref heap<slice<byte>>(out var Ꮡempty);
    var re = reflect.ValueOf(Ꮡempty).Elem();
    re.Grow(3);
    fmt.Println("Grow(3) from nil:", re.Len(), re.Cap() >= 3, empty == default!);

    ref var big = ref heap<slice<nint>>(out var Ꮡbig);
    big = new slice<nint>(2, 2);
    var rb = reflect.ValueOf(Ꮡbig).Elem();
    rb.Grow(8);
    rb.SetLen(3);
    rb.Index(2).SetInt(99);
    fmt.Println("Grow+SetLen:", len(big), big[2], cap(big) >= 10);

    ref var keep = ref heap<slice<nint>>(out var Ꮡkeep);
    keep = new nint[]{7, 8}.slice();
    var rk = reflect.ValueOf(Ꮡkeep).Elem();
    rk.Grow(0);
    fmt.Println("Grow(0):", len(keep), keep[0], keep[1]);
}

} // end main_package
