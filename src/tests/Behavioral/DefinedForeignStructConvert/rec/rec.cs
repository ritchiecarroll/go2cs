namespace go.DefinedForeignStructConvert;

using static rec_package;

partial class rec_package {

partial struct Hidden {
    public nint N;
    internal @string s;
}

partial struct Open {
    public nint N;
    public @string S;
}

public static Hidden NewHidden(nint n, @string s) { return new Hidden(N: n, s: s); }

} partial class recᴛHidden {

public static @string S(this Hidden h) { return h.s; }

} // end recᴛHidden
