namespace go.DeltaNameProjection;

partial class lib_package {

public static UntypedInt ΔK => 7;

public static readonly ж<nint> ᏑΔV = new StaticBox<nint>(3);
public static ref nint ΔV => ref ((StaticBox<nint>)ᏑΔV).ValueSlot;

partial struct Rec {
    [GoName("Δt")] public nint Δt;
}

[GoName("ΔF")] public static nint ΔF(nint n) { return n * 2; }

partial interface Fooer {
    [GoName("ΔFoo")] @string ΔFoo();
}

public static @string Call(Fooer f) { return f.ΔFoo(); }

partial interface Lower {
    [GoName("Δfoo")] @string Δfoo();
}

public static @string CallLower(Lower f) { return f.Δfoo(); }

public partial struct ΔEmbL {
    public nint N;
}

[GoName("Δm")] internal static @string Δm⁔(/*this*/ ΔEmbL e) { return "Δm of ΔEmbL"; }

public partial struct Δrec {
    [GoName("Δt")] public nint Δt;
}

public partial struct Δ {
    [GoName("Δ")] public nint ΔΔ;
}

partial struct WrapRec {
    /*embed*/ public Rec Rec;
}

} // end lib_package
