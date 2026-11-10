global using ΔToken = object;

namespace go.RenamedTypeAliasReexport;

partial class lib_package {

partial struct ΔEncoder {
    internal nint n;
}

partial struct export {
}

internal static nint Encoder(this export _, ж<ΔEncoder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    return e.n;
}

public static ж<ΔEncoder> NewEncoder(nint n) { return Ꮡ(new ΔEncoder(n: n)); }

public static nint N(this ref ΔEncoder e) { return e.n; }

public static void Bump(this ref ΔEncoder e) { e.n++; }

partial struct ΔKind /*num:byte*/;

partial struct Tok {
    internal ΔKind k;
}

public static ΔKind Kind(this Tok t) { return t.k; }

public static Tok NewTok(ΔKind k) { return new Tok(k: k); }

partial struct Reader {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tokˢ = (@string)"tok"u8;

public static ΔToken Token(this Reader _) { return tokˢ; }

} // end lib_package
