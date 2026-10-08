namespace go;

using fmt = fmt_package;

partial class main_package {


partial struct PublicNestedShapeᴛ1_A /*dyn*/ {
    public nint X;
}

partial struct PublicNestedShapeᴛ1 /*dyn*/ {
    public PublicNestedShapeᴛ1_A A;
}
public static PublicNestedShapeᴛ1 PublicNestedShape = new PublicNestedShapeᴛ1(A: new PublicNestedShapeᴛ1_A(X: 7));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object publicNestedˢ = (@string)"public nested:"u8;

internal static void Main() {
    RegisterInternalShape();
    fmt.Println(publicNestedˢ, PublicNestedShape.A.X);
}

} // end main_package
