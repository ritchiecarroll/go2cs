namespace go.vlib;

partial class vlib_package {

partial interface Source {
    uint64 Uint64();
}

partial struct PCG {
    internal uint64 state;
}

public static ж<PCG> NewPCG(uint64 seed) {
    return Ꮡ(new PCG(state: seed));
}

public static uint64 Uint64(this ref PCG p) {
    p.state ^= (uint64)((p.state << (int)(13)));
    p.state ^= (uint64)((p.state >> (int)(7)));
    p.state ^= (uint64)((p.state << (int)(17)));
    return p.state;
}

partial struct Rand {
    internal Source src;
}

public static ж<Rand> New(Source src) {
    return Ꮡ(new Rand(src: src));
}

public static nint IntN(this ref Rand r, nint n) {
    return (nint)(r.src.Uint64() % (uint64)n);
}

} // end vlib_package
