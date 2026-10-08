namespace go;

using fmt = fmt_package;
using inner = EmbeddedTypeNameCollision.inner_package;
using EmbeddedTypeNameCollision;

partial class main_package {

partial struct Buffer {
    public partial ref EmbeddedTypeNameCollision.inner_package.Buffer ΔBuffer { get; }
    public @string Tag;
}

public static @string Renamed(Buffer b) {
    return fmt.Sprintf("%s/%s/%d"u8, b.Tag, b.ΔBuffer.Data, b.ΔBuffer.N);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string betaˢ = "beta"u8;
private static readonly @string gammaˢ = "gamma"u8;
private static readonly @string freshˢ = "fresh"u8;

internal static void Main() {
    ref var b = ref heap<Buffer>(out var Ꮡb);
    b = new Buffer(ΔBuffer: inner.NewBuffer(alphaˢ, 3), Tag: "tagged"u8);
    fmt.Println(b.ΔBuffer.Data, b.ΔBuffer.N);
    fmt.Println(b.Data, b.N);
    fmt.Println(b.ΔBuffer.Describe());
    fmt.Println(Renamed(b));
    var p = Ꮡb;
    p.of(Buffer.ᏑΔBuffer).Bump();
    p.of(Buffer.ᏑΔBuffer).Bump();
    fmt.Println((~p).ΔBuffer.N, (~p).N, (~p).ΔBuffer.Describe());
    p.Value.ΔBuffer.Data = betaˢ;
    p.of(Buffer.ᏑΔBuffer).Append("!"u8);
    fmt.Println((~p).Data, (~p).ΔBuffer.N, (~p).ΔBuffer.Describe());
    var z = @new<Buffer>();
    z.of(Buffer.ᏑΔBuffer).Append(gammaˢ);
    z.Value.Tag = freshˢ;
    fmt.Println((~z).ΔBuffer.Describe(), (~z).ΔBuffer.N, Renamed(z.Value));
}

} // end main_package
