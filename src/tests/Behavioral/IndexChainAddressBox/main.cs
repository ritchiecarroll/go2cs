namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static void inc(this ref counter c) {
    c.n++;
}

partial struct hist {
    internal array<atomic.Uint64> counts = new(4);
    internal array<counter> local = new(2);
    internal array<nint> plain = new(3);
    internal slice<nint> sl;
}

partial struct wrap {
    internal hist h;
}

internal static uint64 closureLocal() {
    uint64 record(slice<nint> samples) {
        ref var h = ref heap(new hist(), out var Ꮡh);
        foreach (var (_, s) in samples) {
            Ꮡh.at(hist.Ꮡcounts, s % 4).Add(1);
        }
        return Ꮡh.at(hist.Ꮡcounts, 1).Load();
    }
    return record(new nint[]{1, 5, 2, 9}.slice());
}

internal static uint64 fieldArrayMethod() {
    ref var h = ref heap(new hist(), out var Ꮡh);
    Ꮡh.at(hist.Ꮡcounts, 2).Add(3);
    Ꮡh.at(hist.Ꮡcounts, 2).Add(4);
    return Ꮡh.at(hist.Ꮡcounts, 2).Load();
}

internal static uint64 nestedFieldArrayMethod() {
    ref var w = ref heap(new wrap(), out var Ꮡw);
    Ꮡw.of(wrap.Ꮡh).at(hist.Ꮡcounts, 3).Add(5);
    return Ꮡw.of(wrap.Ꮡh).at(hist.Ꮡcounts, 3).Load();
}

internal static uint64 localArrayMethod() {
    ref var a = ref heap(new array<atomic.Uint64>(4), out var Ꮡa);
    Ꮡa.at<atomic.Uint64>(2).Add(6);
    return Ꮡa.at<atomic.Uint64>(2).Load();
}

internal static nint explicitAddress() {
    ref var h = ref heap(new hist(), out var Ꮡh);
    var p = Ꮡh.at(hist.Ꮡplain, 1);
    p.Value = 7;
    p.Value += 1;
    return h.plain[1];
}

internal static nint paramAddress(hist hʗp) {
    ref var h = ref heap(hʗp.ΔClone(), out var Ꮡh);

    var p = Ꮡh.at(hist.Ꮡplain, 2);
    p.Value = 9;
    return h.plain[2];
}

internal static nint methodValue() {
    ref var h = ref heap(new hist(), out var Ꮡh);
    var f = Ꮡh.at(hist.Ꮡlocal, 1).inc;
    f();
    f();
    return h.local[1].n;
}

internal static nint localTypeCall() {
    hist h = new();
    h.local[0].inc();
    return h.local[0].n;
}

internal static nint sliceFieldAddress() {
    hist h = new();
    h.sl = new slice<nint>(2);
    var p = Ꮡ(h.sl, 1);
    p.Value = 11;
    return h.sl[1];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object closureLocalˢ = (@string)"closureLocal:"u8;
private static readonly object fieldArrayMethodˢ = (@string)"fieldArrayMethod:"u8;
private static readonly object nestedFieldArrayMethodˢ = (@string)"nestedFieldArrayMethod:"u8;
private static readonly object localArrayMethodˢ = (@string)"localArrayMethod:"u8;
private static readonly object explicitAddressˢ = (@string)"explicitAddress:"u8;
private static readonly object paramAddressˢ = (@string)"paramAddress:"u8;
private static readonly object methodValueˢ = (@string)"methodValue:"u8;
private static readonly object localTypeCallˢ = (@string)"localTypeCall:"u8;
private static readonly object sliceFieldAddressˢ = (@string)"sliceFieldAddress:"u8;

internal static void Main() {
    fmt.Println(closureLocalˢ, closureLocal());
    fmt.Println(fieldArrayMethodˢ, fieldArrayMethod());
    fmt.Println(nestedFieldArrayMethodˢ, nestedFieldArrayMethod());
    fmt.Println(localArrayMethodˢ, localArrayMethod());
    fmt.Println(explicitAddressˢ, explicitAddress());
    fmt.Println(paramAddressˢ, paramAddress(new hist(nil)));
    fmt.Println(methodValueˢ, methodValue());
    fmt.Println(localTypeCallˢ, localTypeCall());
    fmt.Println(sliceFieldAddressˢ, sliceFieldAddress());
}

} // end main_package
