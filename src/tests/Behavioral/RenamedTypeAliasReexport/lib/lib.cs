global using Token = object;

namespace go.RenamedTypeAliasReexport;

partial class lib_package {

partial struct Encoder {
    internal nint n;
}

partial struct export {
}

internal static nint Encoder⁔(/*this*/ export _, ж<Encoder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    return e.n;
}

public static ж<Encoder> NewEncoder(nint n) { return Ꮡ(new Encoder(n: n)); }

internal static nint N⁔(/*this*/ ref Encoder e) { return e.n; }

internal static void Bump⁔(/*this*/ ref Encoder e) { e.n++; }

partial struct Kind /*num:byte*/;

partial struct Tok {
    internal Kind k;
}

internal static Kind Kind⁔(/*this*/ Tok t) { return t.k; }

public static Tok NewTok(Kind k) { return new Tok(k: k); }

partial struct Reader {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tokˢ = (@string)"tok"u8;

internal static Token Token⁔(/*this*/ Reader _) { return tokˢ; }

} // end lib_package
