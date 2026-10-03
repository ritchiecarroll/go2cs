namespace go;

using XpkgPromotedInnerLib = XpkgPromotedInnerLib_package;

partial class XpkgPromotedMidLib_package {

[GoType] partial struct Mid {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

[GoType] partial struct loc2 {
}

internal static XpkgPromotedInnerLib.Token Tok(this loc2 _) {
    return new XpkgPromotedInnerLib.Token(N: 9);
}

[GoType] partial struct Mid2 {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
    internal partial ref loc2 loc2 { get; }
}

[GoType] partial struct r1 {
    internal nint n;
}

[GoRecv] internal static nint Ping(this ref r1 r) {
    r.n++;
    return r.n;
}

[GoType] partial struct SameP {
    internal partial ref ж<r1> r1 { get; }
}

[GoType] partial struct SameV {
    internal partial ref r1 r1 { get; }
}

public static SameP NewSameP() {
    return new SameP(Ꮡ(new r1(nil)));
}

[GoType] partial struct wrap {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
}

[GoType] partial struct Mid3 {
    internal partial ref wrap wrap { get; }
}

[GoType] partial struct HasTok {
    public nint Tok;
}

[GoType] partial struct MidF {
    public partial ref XpkgPromotedInnerLib_package.Inner Inner { get; }
    public partial ref HasTok HasTok { get; }
}

[GoType] partial struct inner3 {
}

internal static XpkgPromotedInnerLib.Token Tok(this inner3 _) {
    return new XpkgPromotedInnerLib.Token(N: 3);
}

[GoType] partial struct PS {
    internal partial ref inner3 inner3 { get; }
    public nint Tok;
}

} // end XpkgPromotedMidLib_package
