namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using Δsync = sync_package;

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

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸsync() {
    builtin.initPackage(typeof(sync_package));
}

[GoType] partial struct point {
    internal nint x, y;
    internal slice<@string> tags;
}

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

[GoType("map[@string, nint]")] partial struct named;

[GoType("map[any, nint]")] partial struct namedAny;

[GoType("map[@string, slice<nint>]")] partial struct namedSlices;

[GoType("[]byte")] partial struct charData;

[GoType("num:byte")] partial struct myByte;

[GoType("[]myByte")] partial struct myBytes;

[GoType("[]@string")] partial struct names;

[GoType("[]any")] partial struct recur;

[GoType] partial struct wrap {
    internal named m;
}

[GoType] partial struct hooks {
    internal @string name;
    internal Func<nint, nint> fill;
    internal Action step;
}

[GoType] partial struct guarded {
    internal Δsync.Mutex mu;
    internal Δsync.RWMutex rw;
    internal Δsync.Once once;
    internal nint n;
    internal @string name;
}

internal static void Main() {
    fmt.Println(reflect.DeepEqual(new @string[]{"a"u8, "bc"u8}.slice(), new @string[]{"a"u8, "bc"u8}.slice()));
    fmt.Println(reflect.DeepEqual(new @string[]{"a"u8}.slice(), new @string[]{"b"u8}.slice()));
    fmt.Println(reflect.DeepEqual(new nint[]{1, 2, 3}.slice(), new nint[]{1, 2, 3}.slice()));
    fmt.Println(reflect.DeepEqual(new nint[]{1, 2, 3}.slice(), new nint[]{1, 2, 4}.slice()));
    fmt.Println(reflect.DeepEqual(new slice<byte>[]{slice<byte>("ab"u8), default!}.slice(), new slice<byte>[]{slice<byte>("ab"u8), default!}.slice()));
    fmt.Println(reflect.DeepEqual(new slice<byte>[]{slice<byte>("ab"u8)}.slice(), new slice<byte>[]{slice<byte>("ac"u8)}.slice()));
    fmt.Println(reflect.DeepEqual(slice<byte>(default!), new byte[]{}.slice()));
    fmt.Println(reflect.DeepEqual(new byte[]{}.slice(), new byte[]{}.slice()));
    slice<nint> nilInts = default!;
    fmt.Println(reflect.DeepEqual(nilInts, nilInts));
    fmt.Println(reflect.DeepEqual(nilInts, new nint[]{}.slice()));
    var zero = 0.0D;
    var nan = new float64[]{zero / zero}.slice();
    fmt.Println(reflect.DeepEqual(nan, nan));
    fmt.Println(reflect.DeepEqual(nan, new float64[]{nan[0]}.slice()));
    var p1 = new point(1, 2, new @string[]{"n"u8}.slice());
    var p2 = new point(1, 2, new @string[]{"n"u8}.slice());
    var p3 = new point(1, 3, new @string[]{"n"u8}.slice());
    fmt.Println(reflect.DeepEqual(p1, p2));
    fmt.Println(reflect.DeepEqual(p1, p3));
    var m1 = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    var m2 = new map<@string, nint>{["b"u8] = 2, ["a"u8] = 1};
    fmt.Println(reflect.DeepEqual(m1, m2));
    fmt.Println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1}));
    fmt.Println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1, ["c"u8] = 2}));
    fmt.Println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1, ["b"u8] = 3}));
    map<@string, nint> nilMap = default!;
    fmt.Println(reflect.DeepEqual(nilMap, nilMap));
    fmt.Println(reflect.DeepEqual(nilMap, new map<@string, nint>{}));
    fmt.Println(reflect.DeepEqual(m1, m1));
    var q1 = Ꮡ(new point(1, 2, default!));
    var q2 = Ꮡ(new point(1, 2, default!));
    var q3 = q1;
    fmt.Println(reflect.DeepEqual(q1.OrTypedNil(), q2.OrTypedNil()));
    fmt.Println(reflect.DeepEqual(q1.OrTypedNil(), q3.OrTypedNil()));
    q2.Value.y = 9;
    fmt.Println(reflect.DeepEqual(q1.OrTypedNil(), q2.OrTypedNil()));
    ж<point> np1 = default!;
    ж<point> np2 = default!;
    fmt.Println(reflect.DeepEqual(np1.OrTypedNil(), np2.OrTypedNil()));
    fmt.Println(reflect.DeepEqual(np1.OrTypedNil(), q1.OrTypedNil()));
    var a = Ꮡ(new node(val: 1));
    a.Value.next = a;
    var b = Ꮡ(new node(val: 1));
    b.Value.next = b;
    fmt.Println(reflect.DeepEqual(a.OrTypedNil(), b.OrTypedNil()));
    var c = Ꮡ(new node(val: 2));
    c.Value.next = c;
    fmt.Println(reflect.DeepEqual(a.OrTypedNil(), c.OrTypedNil()));
    fmt.Println(reflect.DeepEqual(new nint[]{1}.slice(), new @string[]{"1"u8}.slice()));
    fmt.Println(reflect.DeepEqual(default!, default!));
    var n1 = new named(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var n2 = new named(new map<@string, nint>{["b"u8] = 2, ["a"u8] = 1});
    var n3 = new named(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 3});
    var n4 = new named(new map<@string, nint>{["a"u8] = 1, ["c"u8] = 2});
    fmt.Println(reflect.DeepEqual(n1, n2));
    fmt.Println(reflect.DeepEqual(n1, n3));
    fmt.Println(reflect.DeepEqual(n1, n4));
    fmt.Println(reflect.DeepEqual(n1, new named(new map<@string, nint>{["a"u8] = 1})));
    fmt.Println(reflect.DeepEqual(n1, n1));
    named nilNamed = default!;
    fmt.Println(reflect.DeepEqual(nilNamed, nilNamed));
    fmt.Println(reflect.DeepEqual(nilNamed, new named(new map<@string, nint>{})));
    fmt.Println(reflect.DeepEqual(new wrap(n1), new wrap(n2)));
    fmt.Println(reflect.DeepEqual(new wrap(n1), new wrap(n3)));
    var s1 = new namedSlices(new map<@string, slice<nint>>{["k"u8] = new nint[]{1, 2}.slice()});
    var s2 = new namedSlices(new map<@string, slice<nint>>{["k"u8] = new nint[]{1, 2}.slice()});
    var s3 = new namedSlices(new map<@string, slice<nint>>{["k"u8] = new nint[]{1, 3}.slice()});
    fmt.Println(reflect.DeepEqual(s1, s2));
    fmt.Println(reflect.DeepEqual(s1, s3));
    var k1 = new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2};
    var k2 = new map<any, nint>{[(@string)"b"u8] = 2, [default!] = 1};
    var k3 = new map<any, nint>{[default!] = 9, [(@string)"b"u8] = 2};
    var k4 = new map<any, nint>{[(@string)"b"u8] = 2, [(@string)"c"u8] = 1};
    fmt.Println(reflect.DeepEqual(k1, k2));
    fmt.Println(reflect.DeepEqual(k1, k3));
    fmt.Println(reflect.DeepEqual(k1, k4));
    var j1 = new namedAny(new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2});
    var j2 = new namedAny(new map<any, nint>{[(@string)"b"u8] = 2, [default!] = 1});
    var j3 = new namedAny(new map<any, nint>{[default!] = 9, [(@string)"b"u8] = 2});
    var j4 = new namedAny(new map<any, nint>{[(@string)"b"u8] = 2, [(@string)"c"u8] = 1});
    fmt.Println(reflect.DeepEqual(j1, j2));
    fmt.Println(reflect.DeepEqual(j1, j3));
    fmt.Println(reflect.DeepEqual(j1, j4));
    var h1 = new hooks(name: "a"u8);
    var h2 = new hooks(name: "a"u8);
    var h3 = new hooks(name: "a"u8, step: () => {
    });
    fmt.Println(reflect.DeepEqual(h1, h2));
    fmt.Println(reflect.DeepEqual(h1, h3));
    fmt.Println(reflect.DeepEqual(h3, h3));
    fmt.Println(reflect.DeepEqual(new hooks(name: "a"u8), new hooks(name: "b"u8)));
    fmt.Println(reflect.DeepEqual(new Action[]{default!, default!}.slice(), new Action[]{default!, default!}.slice()));
    fmt.Println(reflect.DeepEqual(new Action[]{default!}.slice(), new Action[]{() => {
    }}.slice()));
    fmt.Println(reflect.DeepEqual(new map<@string, Action>{["k"u8] = default!}, new map<@string, Action>{["k"u8] = default!}));
    Action nilFn = default!;
    fmt.Println(reflect.DeepEqual(nilFn.OrTypedNilFunc(), nilFn.OrTypedNilFunc()));
    fmt.Println(reflect.DeepEqual(nilFn.OrTypedNilFunc(), () => {
    }));
    var g1 = Ꮡ(new guarded(n: 1, name: "a"u8));
    var g2 = Ꮡ(new guarded(n: 1, name: "a"u8));
    g1.of(guarded.Ꮡmu).Lock();
    g1.of(guarded.Ꮡmu).Unlock();
    g1.of(guarded.Ꮡrw).RLock();
    g1.of(guarded.Ꮡrw).RUnlock();
    fmt.Println(reflect.DeepEqual(g1.OrTypedNil(), g2.OrTypedNil()));
    fmt.Println(reflect.DeepEqual(g1.OrTypedNil(), g1.OrTypedNil()));
    fmt.Println(reflect.DeepEqual(g1.OrTypedNil(), Ꮡ(new guarded(n: 2, name: "a"u8))));
    fmt.Println(reflect.DeepEqual(g1.OrTypedNil(), Ꮡ(new guarded(n: 1, name: "b"u8))));
    fmt.Println(reflect.DeepEqual(new ж<guarded>[]{g1}.slice(), new ж<guarded>[]{g2}.slice()));
    fmt.Println(reflect.DeepEqual(new map<@string, ж<guarded>>{["k"u8] = g1}, new map<@string, ж<guarded>>{["k"u8] = g2}));
    var g3 = Ꮡ(new guarded(n: 1, name: "a"u8));
    g3.of(guarded.Ꮡonce).Do(() => {
    });
    fmt.Println(reflect.DeepEqual(g3.OrTypedNil(), g2.OrTypedNil()));
    var data = slice<byte>("same data"u8);
    var c1 = ((charData)data);
    var c2 = ((charData)appendꓸꓸꓸ(slice<byte>(default!), data));
    fmt.Println(reflect.DeepEqual(c1, c2));
    data[1] = (rune)'o';
    fmt.Println(reflect.DeepEqual(c1, c2));
    fmt.Println(reflect.DeepEqual(c1, c1));
    fmt.Println(reflect.DeepEqual(((charData)slice<byte>((@string)"ab"u8)), ((charData)slice<byte>((@string)"ab"u8))));
    fmt.Println(reflect.DeepEqual(((charData)slice<byte>((@string)"ab"u8)), ((charData)slice<byte>((@string)"ac"u8))));
    fmt.Println(reflect.DeepEqual(((charData)slice<byte>((@string)"ab"u8)), ((charData)slice<byte>((@string)"abc"u8))));
    any tok1 = ((charData)data);
    any tok2 = ((charData)appendꓸꓸꓸ(slice<byte>(default!), data));
    fmt.Println(reflect.DeepEqual(tok1, tok2));
    tok2 = ((any)((charData)data));
    fmt.Println(reflect.DeepEqual(tok1, tok2));
    fmt.Println(reflect.DeepEqual(new charData[]{((charData)slice<byte>((@string)"ab"u8))}.slice(), new charData[]{((charData)slice<byte>((@string)"ac"u8))}.slice()));
    fmt.Println(reflect.DeepEqual(new map<@string, charData>{["k"u8] = ((charData)slice<byte>((@string)"ab"u8))}, new map<@string, charData>{["k"u8] = ((charData)slice<byte>((@string)"ac"u8))}));
    charData nilCD = default!;
    fmt.Println(reflect.DeepEqual(nilCD, nilCD));
    fmt.Println(reflect.DeepEqual(nilCD, new charData(new byte[]{}.slice())));
    fmt.Println(reflect.DeepEqual(new charData(new byte[]{}.slice()), new charData(new byte[]{}.slice())));
    fmt.Println(reflect.DeepEqual(new myBytes(new myByte[]{1, 2}.slice()), new myBytes(new myByte[]{1, 2}.slice())));
    fmt.Println(reflect.DeepEqual(new myBytes(new myByte[]{1, 2}.slice()), new myBytes(new myByte[]{1, 3}.slice())));
    fmt.Println(reflect.DeepEqual(new names(new @string[]{"a"u8, "b"u8}.slice()), new names(new @string[]{"a"u8, "b"u8}.slice())));
    fmt.Println(reflect.DeepEqual(new names(new @string[]{"a"u8, "b"u8}.slice()), new names(new @string[]{"a"u8, "c"u8}.slice())));
    var nanCD = new names(new @string[]{"a"u8}.slice());
    fmt.Println(reflect.DeepEqual(nanCD, nanCD));
    var r1 = new recur(1);
    r1[0] = r1;
    var r2 = new recur(1);
    r2[0] = r2;
    fmt.Println(reflect.DeepEqual(r1, r2));
    var r3 = new recur(1);
    r3[0] = (@string)"x"u8;
    fmt.Println(reflect.DeepEqual(r1, r3));
}

} // end main_package
