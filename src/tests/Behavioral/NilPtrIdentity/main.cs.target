namespace go;

using fmt = fmt_package;
using slices = slices_package;
using lib = NilPtrIdentityLib_package;
using NilPtrIdentityLib = NilPtrIdentityLib_package;

partial class main_package {

partial struct node {
    public nint Code;
}

partial class nodePtr /*ж<node>*/;

partial struct holder {
    internal ж<node> p;
}

internal static T orElse<T>(T value, T orElse) {
    if (!AreEqual(value, @new<T>().ValueSlot)) {
        return value;
    }

    return orElse;
}

internal static bool same<T>(T a, T b) {
    return AreEqual(a, b);
}

internal static nint keyCount<K>(params Span<K> keysʗp) {
    var keys = keysʗp.sslice();

    var m = new map<K, nint>{};

    foreach (var (_, k) in range(keys)) {
        m[k]++;
    }

    return len(m);
}

internal static array<T> one<T>(T v) {
    return new T[]{v}.array();
}

internal static any asAny<T>(T v) {
    return (v).OrTypedNilOf();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilˢ = (@string)"nil"u8;

internal static any code(ж<lib.Message> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (Ꮡm == nil) {
        return nilˢ;
    }

    return m.Code;
}

internal static any nodeCode(ж<node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (Ꮡn == nil) {
        return nilˢ;
    }

    return n.Code;
}

internal static void main() {
    ж<lib.Message> local = default;
    holder field = default;

    fmt.Println("phase 1:");
    fmt.Println(code(lib.OrElse((ж<lib.Message>)(nil), lib.Default)), lib.OrElse((ж<lib.Message>)(nil), lib.Default) == lib.Default);
    fmt.Println(code(lib.OrElse(local, lib.Default)), lib.OrElse(local, lib.Default) == lib.Default);
    fmt.Println(code(lib.OrElse((ж<lib.Message>)nil, lib.Default)), code(lib.OrElse(@new<ж<lib.Message>>().ValueSlot, lib.Default)));
    fmt.Println(lib.IsZero<ж<lib.Message>>(nil), lib.IsZero(local), lib.IsZero(lib.Default));

    fmt.Println("phase 2:");
    fmt.Println(nodeCode(orElse((ж<node>)(nil), Ꮡ(new node(Code: 7)))), nodeCode(orElse(field.p, Ꮡ(new node(Code: 8)))));
    fmt.Println(same<ж<node>>(nil, nil), same((ж<node>)(nil), field.p), same(field.p, (ж<node>)(nil)), same(field.p, @new<ж<node>>().ValueSlot));
    fmt.Println(lib.Same<ж<lib.Message>>(nil, local), lib.Same(local, (ж<lib.Message>)(nil)), lib.Same(lib.Default, (ж<lib.Message>)(nil)));
    var n = Ꮡ(new node(Code: 1));
    fmt.Println(same(n, n), same(n, Ꮡ(new node(Code: 1))), same(n, (ж<node>)(nil)));

    fmt.Println("phase 3:");
    var m = new map<ж<node>, nint>{};
    m[default]++;
    m[field.p]++;
    m[@new<ж<node>>().ValueSlot]++;
    fmt.Println(len(m), m[default], m[field.p]);
    delete(m, field.p);
    fmt.Println(len(m));
    fmt.Println(keyCount<ж<node>>((ж<node>)(nil), field.p, @new<ж<node>>().ValueSlot), keyCount(n, (ж<node>)(nil), n, field.p));

    fmt.Println("phase 4:");
    any fromLit = ((ж<node>)nil);
    any fromField = field.p.OrTypedNil();
    fmt.Println(fromLit == nil, fromField == nil, AreEqual(fromLit, fromField));
    fmt.Println(AreEqual(asAny<ж<node>>(nil), fromField), AreEqual(asAny(field.p), fromLit), asAny<ж<node>>(nil) == nil);
    fmt.Println(AreEqual(fromLit, ((any)((ж<holder>)nil))));
    fmt.Println(same<any>(fromLit, fromField), same<any>(fromLit, default));

    fmt.Println("phase 5:");
    fmt.Println(new holder(p: nil) == field, new holder(p: nil) == new holder(), new holder(p: n) == field);
    fmt.Println(new ж<node>[]{default, field.p}.array() == new ж<node>[]{}.array(2), same(new holder(p: nil), field));

    fmt.Println("phase 6:");
    nodePtr named = default;
    fmt.Println(same<nodePtr>(nil, named), same(named, (nodePtr)nil), keyCount<nodePtr>((nodePtr)(nil), named));
    fmt.Println(slices.Contains(slice<ж<node>>([field.p]), (ж<node>)(nil)), slices.Index(slice<ж<node>>([n, default]), field.p));
    fmt.Println(slices.Contains(slice<ж<node>>([default]), field.p), slices.Equal<slice<ж<node>>, ж<node>>(slice<ж<node>>([default]), slice<ж<node>>([field.p])));

    fmt.Println("phase 7:");
    fmt.Println(one<ж<node>>(nil) == new ж<node>[]{}.array(1), one(field.p) == one<ж<node>>(nil), one(n) == one<ж<node>>(nil));
    var a = new ж<node>[]{n, n}.array();
    a[1] = default;
    a[0] = field.p;
    fmt.Println(a == new ж<node>[]{}.array(2), same(a, new ж<node>[]{}.array(2)));
    var byArray = new map<array<ж<node>>, nint>{};
    byArray[one<ж<node>>(nil)]++;
    byArray[new ж<node>[]{}.array(1)]++;
    byArray[one(field.p)]++;
    fmt.Println(len(byArray), byArray[new ж<node>[]{default}.array()]);
    var byStruct = new map<holder, nint>{};
    byStruct[new holder(p: nil)]++;
    byStruct[field]++;
    byStruct[new holder()]++;
    fmt.Println(len(byStruct), byStruct[new holder(p: nil)], keyCount(new holder(p: nil), field, new holder()));
}

} // end main_package
