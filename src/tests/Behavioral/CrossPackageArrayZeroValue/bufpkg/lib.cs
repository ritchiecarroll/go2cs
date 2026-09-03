namespace go.CrossPackageArrayZeroValue;

partial class bufpkg_package {

[GoType] partial struct State {
    public array<uint64> Buf = new(4);
    public array<uint64> Seed = new(2);
    public uint32 N;
}

[GoType] partial struct Nested {
    public State Inner;
    public @string Label;
}

[GoRecv] public static void Fill(this ref State s) {
    foreach (var (i, _) in s.Buf) {
        s.Buf[i] = (uint64)i + 1;
    }
    s.N = (uint32)len(s.Buf);
}

public static void FillArray([GoArrayDims(4)] ж<array<uint64>> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    foreach (var (i, _) in buf) {
        buf[i] = (uint64)i * 3;
    }
}

public static uint64 Sum(ж<State> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var total = (uint64)0;
    foreach (var (_, v) in s.Buf.ΔRangeSnapshot()) {
        total += v;
    }
    return total;
}

} // end bufpkg_package
