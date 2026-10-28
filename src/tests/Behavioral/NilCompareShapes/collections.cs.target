namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

partial struct byteSlice /*[]byte*/;

partial struct counts /*map[@string, nint]*/;

partial struct events /*chan nint*/;

partial struct bag {
    internal slice<@string> items;
    internal map<@string, nint> index;
    internal channel<EmptyStruct> done;
}

internal static bool variadic(params ꓸꓸꓸnint xsʗp) {
    var xs = xsʗp.slice();

    return xs == default;
}

internal static bool sliceIsNil<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return s.IsNil;
}

internal static void collections() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    fmt.Println("nil slice:", s == default, default == s, s != default);
    fmt.Println("nil slice resliced:", s[..] == default, s[0..0] == default);
    fmt.Println("append nothing to nil:", appendꓸꓸꓸ(s, new nint[]{}.slice()) == default);
    fmt.Println("make([]int, 0):", new slice<nint>(0) == default, default != new slice<nint>(0, 8));
    fmt.Println("empty literal:", new nint[]{}.slice() == default);

    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3}.array();
    fmt.Println("array resliced to zero:", arr[..0] == default, arr[3..] == default);
    fmt.Println("[]byte(\"\"):", slice<byte>(""u8) == default);
    fmt.Println("unsafe.Slice(nil, 0):", @unsafe.Slice((ж<byte>)nil, 0) == default);
    fmt.Println("unsafe.Slice(&arr[0], 0):", @unsafe.Slice(Ꮡarr.at<nint>(0), 0) == default);

    byteSlice bs = default;
    fmt.Println("nil named slice:", bs == default, default == bs);
    bs = new byteSlice(new byte[]{}.slice());
    fmt.Println("empty named slice:", bs == default, bs != default);

    map<@string, nint> m = default;
    fmt.Println("nil map:", m == default, default == m);
    fmt.Println("made map:", new map<@string, nint>() == default, new map<@string, nint>{} != default);

    counts c = default;
    fmt.Println("nil named map:", c == default);
    c = new counts(new map<@string, nint>{["a"] = 1});
    fmt.Println("named map with an entry:", c != default, default != c);

    channel<nint> ch = default;
    fmt.Println("nil chan:", ch == default, default == ch);
    channel/*<-*/<nint> send = channel/*<-*/<nint>.SendOnly;
    /*<-*/channel<nint> recv = /*<-*/channel<nint>.RecvOnly;
    fmt.Println("nil directional chans:", send == default, recv == default);
    ch = new channel<nint>(1);
    (send, recv) = (ch.AsSendOnly(), ch.AsRecvOnly());
    fmt.Println("made chan:", ch == default, send != default, default != recv);

    events ev = default;
    fmt.Println("nil named chan:", ev == default);
    ev = new events(0);
    fmt.Println("made named chan:", ev == default);

    bag b = default;
    fmt.Println("zero struct fields:", b.items == default, b.index == default, b.done == default);
    b = new bag(items: new @string[]{}.slice(), index: new map<@string, nint>{}, done: new channel<EmptyStruct>(0));
    fmt.Println("assigned struct fields:", b.items == default, b.index == default, b.done == default);

    var p = Ꮡs;
    fmt.Println("through a pointer:", p.ValueSlot == default);

    var lookup = new map<@string, slice<nint>>{["x"] = new nint[]{1}.slice()};
    fmt.Println("map lookups:", lookup["x"] == default, lookup["y"] == default);

    fmt.Println("variadic:", variadic(), variadic(1), variadic(s.ꓸꓸꓸ));
    fmt.Println("type parameter:", sliceIsNil<slice<nint>, nint>(s), sliceIsNil<slice<nint>, nint>(new nint[]{}.slice()), sliceIsNil<byteSlice, byte>(bs));
}

} // end main_package
