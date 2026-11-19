global using Token = object;

namespace go.RenamedTypeAliasReexport;

using static lib_package;

partial class lib_package {

partial struct Encoder {
    internal nint n;
}

partial struct export {
}

} partial class libᴛexport {

internal static nint Encoder(this export _, ж<Encoder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    return e.n;
}

} partial class lib_package {

public static ж<Encoder> NewEncoder(nint n) { return Ꮡ(new Encoder(n: n)); }

} partial class libᴛEncoder {

public static nint N(this ref Encoder e) { return e.n; }

public static void Bump(this ref Encoder e) { e.n++; }

} partial class lib_package {

partial struct Kind /*num:byte*/;

partial struct Tok {
    internal Kind k;
}

} partial class libᴛTok {

public static Kind Kind(this Tok t) { return t.k; }

} partial class lib_package {

public static Tok NewTok(Kind k) { return new Tok(k: k); }

partial struct Reader {
}

} partial class libᴛReader {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tokˢ = (@string)"tok"u8;

public static Token Token(this Reader _) { return tokˢ; }

} // end libᴛReader
