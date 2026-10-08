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
private static readonly @string stringNonEmptyˢ = "string non-empty"u8;
private static readonly @string valˢ = "val"u8;
private static readonly @string stringEmptyˢ = "string empty"u8;
private static readonly @string nsNonEmptyˢ = "NS non-empty"u8;
private static readonly @string nsEmptyˢ = "NS empty"u8;
private static readonly @string niNonZeroˢ = "NI non-zero"u8;
private static readonly @string niZeroˢ = "NI zero"u8;
private static readonly @string uint812ˢ = "[2]uint8{1,2}"u8;
private static readonly @string uint8ˢ = "[2]uint8{}"u8;
private static readonly @string na12ˢ = "NA{1,2}"u8;
private static readonly @string stringˢ = "[3]string{'','',''}"u8;
private static readonly @string nsAˢ = "[2]NS{'a',''}"u8;
private static readonly @string nbNilˢ = "NB(nil)"u8;
private static readonly @string byteNilˢ = "[]byte(nil)"u8;
private static readonly @string byte0ˢ = "[]byte{0}"u8;
private static readonly @string mapNilˢ = "map nil"u8;
private static readonly @string mapEmptyˢ = "map empty"u8;
private static readonly @string intNilˢ = "*int nil"u8;
private static readonly @string innerˢ = "inner{}"u8;
private static readonly @string innerSValˢ = "inner{S:val}"u8;
private static readonly @string innerN1ˢ = "inner{N:1}"u8;
private static readonly @string outerˢ = "outer{}"u8;
private static readonly @string outerANa01ˢ = "outer{A:NA{0,1}}"u8;
private static readonly @string outerISXˢ = "outer{I.S:x}"u8;
private static readonly @string outerPNˢ = "outer{P:&n}"u8;
private static readonly @string outerSlˢ = "outer{Sl:[]}"u8;
private static readonly object anyNsIsZeroˢ = (@string)"any(NS(\"\")) IsZero:"u8;
private static readonly object anyNsVIsZeroˢ = (@string)"any(NS(\"v\")) IsZero:"u8;
private static readonly @string outerENilˢ = "outer{E:nil}"u8;
private static readonly @string outerEErrˢ = "outer{E:err}"u8;
private static readonly object fieldEKindˢ = (@string)"field E kind:"u8;
private static readonly object isZeroˢ = (@string)"IsZero:"u8;
private static readonly object fieldESetKindˢ = (@string)"field E set kind:"u8;
private static readonly object grow1LenCapˢ = (@string)"Grow(1) len/cap:"u8;
private static readonly object grow3FromNilˢ = (@string)"Grow(3) from nil:"u8;
private static readonly object growSetLenˢ = (@string)"Grow+SetLen:"u8;
private static readonly object grow0ˢ = (@string)"Grow(0):"u8;

internal static void Main() {
    z(stringNonEmptyˢ, valˢ);
    z(stringEmptyˢ, (@string)""u8);
    z(nsNonEmptyˢ, ((NS)(@string)valˢ));
    z(nsEmptyˢ, ((NS)(@string)""u8));
    z(niNonZeroˢ, ((NI)3));
    z(niZeroˢ, ((NI)0));
    z(uint812ˢ, new uint8[]{1, 2}.array());
    z(uint8ˢ, new uint8[]{}.array(2));
    z(na12ˢ, new NA(new uint8[]{1, 2}.array()));
    z("NA{}"u8, new NA(new uint8[2].array()));
    z(stringˢ, new @string[]{}.array(3));
    z(nsAˢ, new NS[]{((NS)(@string)"a"u8), ((NS)(@string)""u8)}.array());
    z(nbNilˢ, ((NB)default!));
    z("NB{}"u8, new NB(new byte[]{}.slice()));
    z(byteNilˢ, slice<byte>(default!));
    z(byte0ˢ, new byte[]{0}.slice());
    map<@string, nint> nilMap = default!;
    z(mapNilˢ, nilMap);
    z(mapEmptyˢ, new map<@string, nint>{});
    z(intNilˢ, ((ж<nint>)nil));
    z(innerˢ, new inner(nil));
    z(innerSValˢ, new inner(S: ((NS)(@string)valˢ)));
    z(innerN1ˢ, new inner(N: 1));
    z(outerˢ, new outer(nil));
    z(outerANa01ˢ, new outer(A: new NA(new uint8[]{0, 1}.array())));
    z(outerISXˢ, new outer(I: new inner(S: ((NS)(@string)"x"u8))));
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 0;
    z(outerPNˢ, new outer(P: Ꮡn));
    z(outerSlˢ, new outer(Sl: new nint[]{}.slice()));
    fmt.Println(anyNsIsZeroˢ, reflect.ValueOf(((any)((NS)(@string)""u8))).IsZero());
    fmt.Println(anyNsVIsZeroˢ, reflect.ValueOf(((any)((NS)(@string)"v"u8))).IsZero());
    z(outerENilˢ, new outer(nil));
    z(outerEErrˢ, new outer(E: errors.New("x"u8)));
    var ev = reflect.ValueOf(Ꮡ(new outer(nil))).Elem().FieldByName("E"u8);
    fmt.Println(fieldEKindˢ, ev.Kind(), isZeroˢ, ev.IsZero());
    var ev2 = reflect.ValueOf(Ꮡ(new outer(E: errors.New("x"u8)))).Elem().FieldByName("E"u8);
    fmt.Println(fieldESetKindˢ, ev2.Kind(), isZeroˢ, ev2.IsZero());
    ref var s = ref heap<slice<byte>>(out var Ꮡs);
    s = new byte[]{1, 2, 3, 4}.slice();
    var rv = reflect.ValueOf(Ꮡs).Elem();
    rv.Grow(1);
    fmt.Println(grow1LenCapˢ, rv.Len(), rv.Cap() >= 5, len(s), cap(s) >= 5);
    ref var empty = ref heap<slice<byte>>(out var Ꮡempty);
    var re = reflect.ValueOf(Ꮡempty).Elem();
    re.Grow(3);
    fmt.Println(grow3FromNilˢ, re.Len(), re.Cap() >= 3, empty == default!);
    ref var big = ref heap<slice<nint>>(out var Ꮡbig);
    big = new slice<nint>(2, 2);
    var rb = reflect.ValueOf(Ꮡbig).Elem();
    rb.Grow(8);
    rb.SetLen(3);
    rb.Index(2).SetInt(99);
    fmt.Println(growSetLenˢ, len(big), big[2], cap(big) >= 10);
    ref var keep = ref heap<slice<nint>>(out var Ꮡkeep);
    keep = new nint[]{7, 8}.slice();
    var rk = reflect.ValueOf(Ꮡkeep).Elem();
    rk.Grow(0);
    fmt.Println(grow0ˢ, len(keep), keep[0], keep[1]);
}

} // end main_package
