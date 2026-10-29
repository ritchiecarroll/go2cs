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

    return xs == nil;
}

internal static bool sliceIsNil<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return s.IsNil;
}

internal static void collections() {
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    fmt.Println("nil slice:", s == nil, nil == s, s != nil);
    fmt.Println("nil slice resliced:", s[..] == nil, s[0..0] == nil);
    fmt.Println("append nothing to nil:", appendꓸꓸꓸ(s, new nint[]{}.slice()) == nil);
    fmt.Println("make([]int, 0):", new slice<nint>(0) == nil, nil != new slice<nint>(0, 8));
    fmt.Println("empty literal:", new nint[]{}.slice() == nil);

    ref var arr = ref heap<array<nint>>(new nint[]{1, 2, 3}.array(), out var Ꮡarr);
    fmt.Println("array resliced to zero:", arr[..0] == nil, arr[3..] == nil);
    fmt.Println("[]byte(\"\"):", slice<byte>(""u8) == nil);
    fmt.Println("unsafe.Slice(nil, 0):", @unsafe.Slice((ж<byte>)nil, 0) == nil);
    fmt.Println("unsafe.Slice(&arr[0], 0):", @unsafe.Slice(Ꮡarr.at<nint>(0), 0) == nil);

    byteSlice bs = default;
    fmt.Println("nil named slice:", bs == nil, nil == bs);
    bs = new byteSlice(new byte[]{}.slice());
    fmt.Println("empty named slice:", bs == nil, bs != nil);

    map<@string, nint> m = default;
    fmt.Println("nil map:", m == nil, nil == m);
    fmt.Println("made map:", new map<@string, nint>() == nil, new map<@string, nint>{} != nil);

    counts c = default;
    fmt.Println("nil named map:", c == nil);
    c = new counts(new map<@string, nint>{["a"] = 1});
    fmt.Println("named map with an entry:", c != nil, nil != c);

    channel<nint> ch = default;
    fmt.Println("nil chan:", ch == nil, nil == ch);
    channel/*<-*/<nint> send = channel/*<-*/<nint>.SendOnly;
    /*<-*/channel<nint> recv = /*<-*/channel<nint>.RecvOnly;
    fmt.Println("nil directional chans:", send == nil, recv == nil);
    ch = new channel<nint>(1);
    (send, recv) = (ch.AsSendOnly(), ch.AsRecvOnly());
    fmt.Println("made chan:", ch == nil, send != nil, nil != recv);

    events ev = default;
    fmt.Println("nil named chan:", ev == nil);
    ev = new events(0);
    fmt.Println("made named chan:", ev == nil);

    bag b = default;
    fmt.Println("zero struct fields:", b.items == nil, b.index == nil, b.done == nil);
    b = new bag(items: new @string[]{}.slice(), index: new map<@string, nint>{}, done: new channel<EmptyStruct>(0));
    fmt.Println("assigned struct fields:", b.items == nil, b.index == nil, b.done == nil);

    var p = Ꮡs;
    fmt.Println("through a pointer:", p.ValueSlot == nil);

    var lookup = new map<@string, slice<nint>>{["x"] = new nint[]{1}.slice()};
    fmt.Println("map lookups:", lookup["x"] == nil, lookup["y"] == nil);

    fmt.Println("variadic:", variadic(), variadic(1), variadic(s.ꓸꓸꓸ));
    fmt.Println("type parameter:", sliceIsNil<slice<nint>, nint>(s), sliceIsNil<slice<nint>, nint>(new nint[]{}.slice()), sliceIsNil<byteSlice, byte>(bs));
}

} // end main_package
