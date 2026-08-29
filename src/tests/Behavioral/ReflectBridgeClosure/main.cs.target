namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸfmt() {
    builtin.initPackage(typeof(fmt_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸreflect() {
    builtin.initPackage(typeof(reflect_package));
}

[GoType] public partial struct inner {
    public nint X;
}

[GoType] partial struct tagged {
    public nint Y;
}

[GoType] partial struct nested {
    internal nint hidden;
}

[GoType] partial struct clashA {
    public nint Y;
}

[GoType] partial struct clashB {
    public nint Y;
}

[GoType] partial struct clash {
    internal partial ref clashA clashA { get; }
    internal partial ref clashB clashB { get; }
}

[GoType] partial struct host {
    public nint Plain;
    internal partial ref inner inner { get; }
    [GoTag(@"json:""t""")]
    internal partial ref tagged tagged { get; }
    [GoTag(@"json:""n""")]
    public inner Named;
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
    return default!;
}

internal static (nint, error) twoOut() {
    return (0, default!);
}

internal static void variadic(@string _Δp0, params ꓸꓸꓸnint ʗp) {
}

internal static (bool, error) mixed(nint a, @string b) {
    return (false, default!);
}

[GoType("num:byte")] partial struct definedByte;

[GoType("[]definedByte")] partial struct definedBytes;

[GoType("[]byte")] partial struct namedPlainBytes;

[GoType] partial struct holder {
    public slice<ж<inner>> Ps;
}

[GoType] partial struct mapHolder {
    public map<@string, ж<inner>> Ms;
}

[GoType] partial struct sliceKey {
    internal any ptr;
    internal nint len;
}

internal static (bool, bool) zeroAgrees<T>() {
    var p = @new<T>();
    var v = reflect.New(reflect.TypeOf(p.OrTypedNil()).Elem());
    return (reflect.DeepEqual(p.OrTypedNil(), v.Interface()), reflect.DeepEqual(p.ValueSlot, v.Elem().Interface()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fieldsˢ = (@string)"fields:"u8;
private static readonly object promotedThroughAnˢ = (@string)"promoted through an unexported embed:"u8;
private static readonly object embedAndPlainUnexportedˢ = (@string)"embed and plain unexported are read-only:"u8;
private static readonly @string innerˢ = "inner"u8;
private static readonly object mapNilElementSeparationsˢ = (@string)"map nil element separations:"u8;
private static readonly object mapNilInterfaceElementˢ = (@string)"map nil interface element:"u8;
private static readonly object newReflectNewˢ = (@string)"new==reflect.New:"u8;
private static readonly object newTIsNilˢ = (@string)"new(T) is nil:"u8;
private static readonly object arrayTypeThroughAPointerˢ = (@string)"array type through a pointer:"u8;
private static readonly object nanKeysˢ = (@string)"nan keys:"u8;
private static readonly object pointerIdentityˢ = (@string)"pointer identity:"u8;
private static readonly object sliceKeyIdentityˢ = (@string)"slice key identity:"u8;

internal static void Main() {
    var ht = reflect.TypeOf(new host(nil));
    fmt.Println(fieldsˢ, ht.NumField());
    foreach (var (_, name) in new @string[]{"Plain"u8, "inner"u8, "tagged"u8, "Named"u8}.slice()) {
        var (f, ok) = ht.FieldByName(name);
        fmt.Printf("field %s found=%v anonymous=%v tag=%q exported=%v type=%s\n"u8, name, ok, f.Anonymous, ((@string)f.Tag), f.IsExported(), f.Type);
    }
    handler named = (nint _Δp0) => default!;
    foreach (var (_, f) in new any[]{noArgs, oneIn, oneOut, twoOut, (Actionꓸꓸꓸ<@string, nint>)(variadic), mixed, named}.slice()) {
        var t = reflect.TypeOf(f);
        fmt.Printf("func %-34s name=%q variadic=%v\n"u8, t.String(), t.Name(), t.IsVariadic());
    }
    fmt.Printf("%T | %T\n"u8, mixed, named.OrTypedNilFunc());
    var custom = new definedBytes(new definedByte[]{(rune)'h', (rune)'i', (rune)'!'}.slice());
    var cv = reflect.ValueOf(custom);
    var got = cv.Bytes();
    fmt.Printf("bytes(definedBytes)=%q len=%d cap=%d\n"u8, ((@string)got), len(got), cap(got));
    got[2] = (rune)'?';
    fmt.Printf("alias write visible in source: %q\n"u8, ((@string)new byte[]{(byte)custom[0], (byte)custom[1], (byte)custom[2]}.slice()));
    var plain = ((namedPlainBytes)slice<byte>((@string)"ok"u8));
    fmt.Printf("bytes(namedPlainBytes)=%q\n"u8, ((@string)reflect.ValueOf(plain).Bytes()));
    ref var arr = ref heap<array<byte>>(out var Ꮡarr);
    arr = new byte[]{(rune)'a', (rune)'b', (rune)'c'}.array();
    fmt.Printf("bytes([3]byte)=%q\n"u8, ((@string)reflect.ValueOf(Ꮡarr).Elem().Bytes()));
    ref var dstPlain = ref heap<slice<byte>>(out var ᏑdstPlain);
    reflect.ValueOf(ᏑdstPlain).Elem().SetBytes(slice<byte>("plain"u8));
    ref var dstNamed = ref heap<namedPlainBytes>(out var ᏑdstNamed);
    reflect.ValueOf(ᏑdstNamed).Elem().SetBytes(slice<byte>("named"u8));
    ref var dstCustom = ref heap<definedBytes>(out var ᏑdstCustom);
    reflect.ValueOf(ᏑdstCustom).Elem().SetBytes(slice<byte>("custom"u8));
    fmt.Printf("setbytes %q %q %d\n"u8, ((@string)dstPlain), ((@string)(slice<byte>)dstNamed), len(dstCustom));
    ref var h = ref heap(new host(), out var Ꮡh);
    var hv = reflect.ValueOf(Ꮡh).Elem();
    hv.FieldByName("X"u8).SetInt(7);
    var (promoted, promotedOK) = reflect.TypeOf(h).FieldByName("X"u8);
    var (_, ambiguous) = reflect.TypeOf(new clash(nil)).FieldByName("Y"u8);
    fmt.Println(promotedThroughAnˢ, h.inner.X, hv.FieldByName("X"u8).CanSet(), promotedOK, len(promoted.Index), promoted.Name, ambiguous);
    fmt.Println(embedAndPlainUnexportedˢ, hv.FieldByName(innerˢ).CanSet(), reflect.ValueOf(Ꮡ(new nested(nil))).Elem().Field(0).CanSet());
    var lit = new ж<inner>[]{default!}.slice();
    var built = reflect.MakeSlice(reflect.TypeOf(lit), 1, 1);
    fmt.Printf("nil element: %v %v deepequal=%v\n"u8, lit, built.Interface(), reflect.DeepEqual(built.Interface(), lit));
    var want = new holder(Ps: new ж<inner>[]{Ꮡ(new inner(X: 1)), default!}.slice());
    ref var target = ref heap(new holder(), out var Ꮡtarget);
    var tv = reflect.ValueOf(Ꮡtarget).Elem().Field(0);
    var grown = reflect.MakeSlice(tv.Type(), 2, 2);
    grown.Index(0).Set(reflect.ValueOf(Ꮡ(new inner(X: 1))));
    tv.Set(grown);
    fmt.Printf("decoded vs literal: %v %v %v\n"u8, reflect.DeepEqual(target, want), want.Ps[1].OrTypedNil(), target.Ps[1].OrTypedNil());
    var mapWant = new mapHolder(Ms: new map<@string, ж<inner>>{["a"u8] = Ꮡ(new inner(X: 1)), ["b"u8] = default!});
    ref var mapTarget = ref heap(new mapHolder(), out var ᏑmapTarget);
    var mv2 = reflect.ValueOf(ᏑmapTarget).Elem().Field(0);
    mv2.Set(reflect.MakeMap(mv2.Type()));
    mv2.SetMapIndex(reflect.ValueOf((@string)"a"u8), reflect.ValueOf(Ꮡ(new inner(X: 1))));
    mv2.SetMapIndex(reflect.ValueOf((@string)"b"u8), reflect.Zero(mv2.Type().Elem()));
    fmt.Printf("map nil element: %v %v %v %v\n"u8, reflect.DeepEqual(mapTarget, mapWant),
        reflect.DeepEqual(mapWant, mapTarget), mapWant.Ms["b"u8].OrTypedNil(), mapTarget.Ms["b"u8].OrTypedNil());
    var nilElem = new map<@string, ж<inner>>{["b"u8] = default!};
    var otherKey = new map<@string, ж<inner>>{["c"u8] = default!};
    var nonNil = new map<@string, ж<inner>>{["b"u8] = Ꮡ(new inner(X: 1))};
    fmt.Println(mapNilElementSeparationsˢ, reflect.DeepEqual(nilElem, otherKey),
        reflect.DeepEqual(nilElem, nonNil), reflect.DeepEqual(nilElem, new map<@string, ж<inner>>{["b"u8] = default!}));
    var anyNil = new map<@string, any>{["a"u8] = default!, ["b"u8] = (nint)(1)};
    fmt.Println(mapNilInterfaceElementˢ, reflect.DeepEqual(anyNil, new map<@string, any>{["a"u8] = default!, ["b"u8] = (nint)(1)}),
        reflect.DeepEqual(anyNil, new map<@string, any>{["a"u8] = (nint)(0), ["b"u8] = (nint)(1)}));
    var (sliceEq, sliceElemEq) = zeroAgrees<slice<any>>();
    var (mapEq, mapElemEq) = zeroAgrees<map<@string, nint>>();
    var (arrEq, arrElemEq) = zeroAgrees<array<nint>>();
    var (strEq, strElemEq) = zeroAgrees<inner>();
    fmt.Println(newReflectNewˢ, sliceEq, sliceElemEq, mapEq, mapElemEq, arrEq, arrElemEq, strEq, strElemEq);
    var ps = @new<slice<nint>>();
    var pm = @new<map<@string, nint>>();
    fmt.Println(newTIsNilˢ, ps.ValueSlot == default!, pm.ValueSlot == default!, len(ps.ValueSlot), len(pm.ValueSlot));
    var pa = Ꮡ(new array<nint>(5));
    var at = reflect.TypeOf(pa.OrTypedNil()).Elem();
    fmt.Println(arrayTypeThroughAPointerˢ, at.String(), at.Len(), reflect.New(at).Elem().Len());
    var nan = nanValue();
    var nm = new map<float64, nint>{};
    nm[nan] = 1;
    nm[nan] = 2;
    var (_, found) = nm[nan, ꟷ];
    delete(nm, nan);
    fmt.Println(nanKeysˢ, len(nm), found);
    var seen = new map<any, EmptyStruct>{};
    var backing = new map<@string, nint>{["a"u8] = 1};
    var mv = reflect.ValueOf(backing);
    seen[(uintptr)mv.UnsafePointer()] = new EmptyStruct();
    var (_, again) = seen[(uintptr)reflect.ValueOf(backing).UnsafePointer(), ꟷ];
    var sl = new nint[]{1, 2, 3}.slice();
    var sv = reflect.ValueOf(sl);
    seen[(uintptr)sv.UnsafePointer()] = new EmptyStruct();
    var (_, slAgain) = seen[(uintptr)reflect.ValueOf(sl).UnsafePointer(), ꟷ];
    var (_, other) = seen[(uintptr)reflect.ValueOf(new map<@string, nint>{["b"u8] = 2}).UnsafePointer(), ꟷ];
    fmt.Println(pointerIdentityˢ, again, slAgain, other, len(seen));
    var boxed = new map<any, EmptyStruct>{};
    boxed[new sliceKey((uintptr)sv.UnsafePointer(), sv.Len())] = new EmptyStruct();
    var (_, keyAgain) = boxed[new sliceKey((uintptr)reflect.ValueOf(sl).UnsafePointer(), len(sl)), ꟷ];
    var (_, keyOther) = boxed[new sliceKey((uintptr)reflect.ValueOf(sl).UnsafePointer(), len(sl) - 1), ꟷ];
    fmt.Println(sliceKeyIdentityˢ, keyAgain, keyOther, len(boxed));
}

internal static float64 nanValue() {
    var zero = 0.0D;
    return zero / zero;
}

} // end main_package
