namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

public partial struct inner {
    public nint X;
}

partial struct tagged {
    public nint Y;
}

partial struct nested {
    internal nint hidden;
}

partial struct clashA {
    public nint Y;
}

partial struct clashB {
    public nint Y;
}

partial struct clash {
    /*embed*/ internal clashA clashA;
    /*embed*/ internal clashB clashB;
}

partial struct host {
    public nint Plain;
    /*embed*/ internal inner inner;
    /*embed*/ internal tagged tagged; /*`json:"t"`*/
    public inner Named; /*`json:"n"`*/
}

internal delegate error handler(nint _);

internal static error call(this handler h, nint n) {
    return h(n);
}

internal static void noArgs() {
}

internal static void oneIn(nint _) {
}

internal static error oneOut() {
    return default;
}

internal static (nint, error) twoOut() {
    return (0, default);
}

internal static void variadic(@string _Δp0, params ꓸꓸꓸnint ʗp) {
}

internal static (bool, error) mixed(nint a, @string b) {
    return (false, default);
}

partial struct definedByte /*num:byte*/;

partial struct definedBytes /*[]definedByte*/;

partial struct namedPlainBytes /*[]byte*/;

partial struct holder {
    public slice<ж<inner>> Ps;
}

partial struct mapHolder {
    public map<@string, ж<inner>> Ms;
}

partial struct sliceKey {
    internal any ptr;
    internal nint len;
}

internal static (bool, bool) zeroAgrees<T, Tᴀ>()
    where Tᴀ : IGoArrayDims
{
    var p = GoArrayZero.New<T, Tᴀ>();
    var v = reflect.New(reflect.TypeOf(p.OrTypedNil()).Elem());
    return (reflect.DeepEqual(p.OrTypedNil(), v.Interface()), reflect.DeepEqual((p.ValueSlot).OrTypedNilOf(), v.Elem().Interface()));
}

private sealed class ᴀ3 : IGoArrayDims { public static nint[] Dims { get; } = [3]; }

internal static void Main() {
    var ht = reflect.TypeOf(new host());
    fmt.Println("fields:", ht.NumField());
    foreach (var (_, name) in new @string[]{"Plain", "inner", "tagged", "Named"}.slice()) {
        var (f, ok) = ht.FieldByName(name);
        fmt.Printf("field %s found=%v anonymous=%v tag=%q exported=%v type=%s\n"u8, name, ok, f.Anonymous, (@string)f.Tag, f.IsExported(), f.Type);
    }

    handler named = (nint _Δp0) => default;
    foreach (var (_, f) in new any[]{noArgs, oneIn, oneOut, twoOut, (Actionꓸꓸꓸ<@string, nint>)(variadic), mixed, (named).OrTypedNilFunc()}.slice()) {
        var t = reflect.TypeOf(f);
        fmt.Printf("func %-34s name=%q variadic=%v\n"u8, t.String(), t.Name(), t.IsVariadic());
    }
    fmt.Printf("%T | %T\n"u8, mixed, (named).OrTypedNilFunc());

    var custom = new definedBytes(new definedByte[]{(rune)'h', (rune)'i', (rune)'!'}.slice());
    var cv = reflect.ValueOf(custom);
    var got = cv.Bytes();
    fmt.Printf("bytes(definedBytes)=%q len=%d cap=%d\n"u8, (@string)got, len(got), cap(got));
    got[2] = (byte)'?';
    fmt.Printf("alias write visible in source: %q\n"u8, ((@string)new byte[]{(byte)custom[0], (byte)custom[1], (byte)custom[2]}.slice()));

    var plain = (namedPlainBytes)slice<byte>((@string)"ok"u8);
    fmt.Printf("bytes(namedPlainBytes)=%q\n"u8, (@string)reflect.ValueOf(plain).Bytes());

    ref var arr = ref heap<array<byte>>(new byte[]{(byte)'a', (byte)'b', (byte)'c'}.array(), out var Ꮡarr);
    fmt.Printf("bytes([3]byte)=%q\n"u8, (@string)reflect.ValueOf(Ꮡarr).Elem().Bytes());

    ref var dstPlain = ref heap<slice<byte>>(out var ᏑdstPlain);
    reflect.ValueOf(ᏑdstPlain).Elem().SetBytes(slice<byte>("plain"u8));
    ref var dstNamed = ref heap<namedPlainBytes>(out var ᏑdstNamed);
    reflect.ValueOf(ᏑdstNamed).Elem().SetBytes(slice<byte>("named"u8));
    ref var dstCustom = ref heap<definedBytes>(out var ᏑdstCustom);
    reflect.ValueOf(ᏑdstCustom).Elem().SetBytes(slice<byte>("custom"u8));
    fmt.Printf("setbytes %q %q %d\n"u8, (@string)dstPlain, (@string)(slice<byte>)dstNamed, len(dstCustom));

    ref var h = ref heap(new host(), out var Ꮡh);
    var hv = reflect.ValueOf(Ꮡh).Elem();
    hv.FieldByName("X").SetInt(7);
    var (promoted, promotedOK) = reflect.TypeOf(h).FieldByName("X");
    var (_, ambiguous) = reflect.TypeOf(new clash()).FieldByName("Y");
    fmt.Println("promoted through an unexported embed:", h.inner.X, hv.FieldByName("X").CanSet(), promotedOK, len(promoted.Index), promoted.Name, ambiguous);
    fmt.Println("embed and plain unexported are read-only:", hv.FieldByName("inner").CanSet(), reflect.ValueOf(Ꮡ(new nested())).Elem().Field(0).CanSet());

    var lit = new ж<inner>[]{default}.slice();
    var built = reflect.MakeSlice(reflect.TypeOf(lit), 1, 1);
    fmt.Printf("nil element: %v %v deepequal=%v\n"u8, lit, built.Interface(), reflect.DeepEqual(built.Interface(), lit));

    var want = new holder(Ps: new ж<inner>[]{Ꮡ(new inner(X: 1)), default}.slice());
    ref var target = ref heap(new holder(), out var Ꮡtarget);
    var tv = reflect.ValueOf(Ꮡtarget).Elem().Field(0);
    var grown = reflect.MakeSlice(tv.Type(), 2, 2);
    grown.Index(0).Set(reflect.ValueOf(Ꮡ(new inner(X: 1))));
    tv.Set(grown);
    fmt.Printf("decoded vs literal: %v %v %v\n"u8, reflect.DeepEqual(target, want), want.Ps[1].OrTypedNil(), target.Ps[1].OrTypedNil());

    var mapWant = new mapHolder(Ms: new map<@string, ж<inner>>{["a"] = Ꮡ(new inner(X: 1)), ["b"] = default});
    ref var mapTarget = ref heap(new mapHolder(), out var ᏑmapTarget);
    var mv2 = reflect.ValueOf(ᏑmapTarget).Elem().Field(0);
    mv2.Set(reflect.MakeMap(mv2.Type()));
    mv2.SetMapIndex(reflect.ValueOf("a"), reflect.ValueOf(Ꮡ(new inner(X: 1))));
    mv2.SetMapIndex(reflect.ValueOf("b"), reflect.Zero(mv2.Type().Elem()));
    fmt.Printf("map nil element: %v %v %v %v\n"u8, reflect.DeepEqual(mapTarget, mapWant),
        reflect.DeepEqual(mapWant, mapTarget), mapWant.Ms["b"].OrTypedNil(), mapTarget.Ms["b"].OrTypedNil());

    var nilElem = new map<@string, ж<inner>>{["b"] = default};
    var otherKey = new map<@string, ж<inner>>{["c"] = default};
    var nonNil = new map<@string, ж<inner>>{["b"] = Ꮡ(new inner(X: 1))};
    fmt.Println("map nil element separations:", reflect.DeepEqual(nilElem, otherKey),
        reflect.DeepEqual(nilElem, nonNil), reflect.DeepEqual(nilElem, new map<@string, ж<inner>>{["b"] = default}));

    var anyNil = new map<@string, any>{["a"] = default, ["b"] = (nint)1};
    fmt.Println("map nil interface element:", reflect.DeepEqual(anyNil, new map<@string, any>{["a"] = default, ["b"] = (nint)1}),
        reflect.DeepEqual(anyNil, new map<@string, any>{["a"] = (nint)0, ["b"] = (nint)1}));

    var (sliceEq, sliceElemEq) = zeroAgrees<slice<any>, ᴀnone>();
    var (mapEq, mapElemEq) = zeroAgrees<map<@string, nint>, ᴀnone>();
    var (arrEq, arrElemEq) = zeroAgrees<array<nint>, ᴀ3>();
    var (strEq, strElemEq) = zeroAgrees<inner, ᴀnone>();
    fmt.Println("new==reflect.New:", sliceEq, sliceElemEq, mapEq, mapElemEq, arrEq, arrElemEq, strEq, strElemEq);

    var ps = @new<slice<nint>>();
    var pm = @new<map<@string, nint>>();
    fmt.Println("new(T) is nil:", ps.ValueSlot == nil, pm.ValueSlot == nil, len(ps.ValueSlot), len(pm.ValueSlot));

    var pa = Ꮡ(new array<nint>(5));
    var at = reflect.TypeOf(pa.OrTypedNil()).Elem();
    fmt.Println("array type through a pointer:", at.String(), at.Len(), reflect.New(at).Elem().Len());

    var nan = nanValue();
    var nm = new map<float64, nint>{};
    nm[nan] = 1;
    nm[nan] = 2;
    var (_, found) = nm[nan, ꟷ];
    delete(nm, nan);
    fmt.Println("nan keys:", len(nm), found);

    var seen = new map<any, EmptyStruct>{};
    var backing = new map<@string, nint>{["a"] = 1};
    var mv = reflect.ValueOf(backing);
    seen[@unsafe.Pointer.OrTypedNil((uintptr)mv.UnsafePointer())] = new EmptyStruct();
    var (_, again) = seen[@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(backing).UnsafePointer()), ꟷ];
    var sl = new nint[]{1, 2, 3}.slice();
    var sv = reflect.ValueOf(sl);
    seen[@unsafe.Pointer.OrTypedNil((uintptr)sv.UnsafePointer())] = new EmptyStruct();
    var (_, slAgain) = seen[@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(sl).UnsafePointer()), ꟷ];
    var (_, other) = seen[@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(new map<@string, nint>{["b"] = 2}).UnsafePointer()), ꟷ];
    fmt.Println("pointer identity:", again, slAgain, other, len(seen));

    var boxed = new map<any, EmptyStruct>{};
    boxed[new sliceKey(@unsafe.Pointer.OrTypedNil((uintptr)sv.UnsafePointer()), sv.Len())] = new EmptyStruct();
    var (_, keyAgain) = boxed[new sliceKey(@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(sl).UnsafePointer()), len(sl)), ꟷ];
    var (_, keyOther) = boxed[new sliceKey(@unsafe.Pointer.OrTypedNil((uintptr)reflect.ValueOf(sl).UnsafePointer()), len(sl) - 1), ꟷ];
    fmt.Println("slice key identity:", keyAgain, keyOther, len(boxed));
}

internal static float64 nanValue() {
    var zero = 0.0D;
    return zero / zero;
}

} // end main_package
