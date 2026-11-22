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

} // end lib_package
