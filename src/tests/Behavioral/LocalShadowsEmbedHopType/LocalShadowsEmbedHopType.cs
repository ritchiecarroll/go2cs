namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint total;
}

internal static void Add(this ref inner g, slice<byte> p) {
    foreach (var (_, b) in p) {
        g.total += (nint)b;
    }
}

internal static void Store(this ref inner g, /*[4]*/ ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    @out[0] = (byte)g.total;
    @out[1] = (byte)((g.total >> (int)(8)));
    @out[2] = (byte)((g.total >> (int)(16)));
    @out[3] = (byte)((g.total >> (int)(24)));
}

partial struct acc {
    internal partial ref inner inner { get; }
}

partial struct deep {
    internal partial ref acc acc { get; }
}

partial struct Hash {
    internal partial ref acc acc { get; }
    internal bool finalized;
}

partial struct DeepHash {
    internal partial ref deep deep { get; }
}

public static void Write(this ref Hash h, slice<byte> p) {
    h.acc.inner.Add(p);
}

public static slice<byte> Sum(this ref Hash h, slice<byte> b) {
    ref var acc = ref heap(new array<byte>(4), out var Ꮡacc);
    h.acc.inner.Store(Ꮡacc);
    h.finalized = true;
    return appendꓸꓸꓸ(b, acc[..]);
}

public static bool Verify(this ref DeepHash d, slice<byte> expected) {
    nint acc = 0;
    {
        ref var deep = ref heap(new array<byte>(4), out var Ꮡdeep);
        d.deep.acc.inner.Store(Ꮡdeep);
        foreach (var (i, b) in deep.ΔRangeSnapshot()) {
            if (i < len(expected) && b == expected[i]) {
                acc++;
            }
        }
    }
    return acc == len(expected);
}

internal static array<byte> resetVia(ж<DeepHash> Ꮡp) {
    ref var acc = ref heap(new array<byte>(4), out var Ꮡacc);
    Ꮡp.of(DeepHash.Ꮡdeep).of(deep.Ꮡacc).of(main_package.acc.Ꮡinner).Store(Ꮡacc);
    return acc.Clone();
}

internal static void Main() {
    var h = Ꮡ(new Hash(nil));
    h.Write(slice<byte>("go2cs"u8));
    fmt.Println(h.Sum(default!));
    fmt.Println((~h).finalized);
    var d = Ꮡ(new DeepHash(nil));
    d.of(DeepHash.Ꮡdeep).of(deep.Ꮡacc).of(acc.Ꮡinner).Add(slice<byte>("go2cs"u8));
    ref var @out = ref heap(new array<byte>(4), out var Ꮡout);
    d.of(DeepHash.Ꮡdeep).of(deep.Ꮡacc).of(acc.Ꮡinner).Store(Ꮡout);
    fmt.Println(@out);
    fmt.Println(d.Verify(@out[..]));
    fmt.Println(d.Verify(new byte[]{0, 0}.slice()));
    fmt.Println(resetVia(d));
}

} // end main_package
