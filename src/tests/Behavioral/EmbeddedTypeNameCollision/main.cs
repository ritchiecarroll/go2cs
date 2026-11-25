namespace go;

using fmt = fmt_package;
using inner = EmbeddedTypeNameCollision.inner_package;
using EmbeddedTypeNameCollision;

partial class main_package {

partial struct Buffer {
    /*embed*/ public EmbeddedTypeNameCollision.inner_package.Buffer ΔBuffer;
    public @string Tag;
}

public static @string Renamed(Buffer b) {
    return fmt.Sprintf("%s/%s/%d"u8, b.Tag, b.ΔBuffer.Data, b.ΔBuffer.N);
}

internal static void main() {
    ref var b = ref heap<Buffer>(new Buffer(ΔBuffer: inner.NewBuffer("alpha", 3), Tag: "tagged"), out var Ꮡb);

    fmt.Println(b.ΔBuffer.Data, b.ΔBuffer.N);
    fmt.Println(b.Data, b.N);
    fmt.Println(b.ΔBuffer.Describe());
    fmt.Println(Renamed(b));

    var p = Ꮡb;
    p.of(Buffer.ᏑΔBuffer).Bump();
    p.of(Buffer.ᏑΔBuffer).Bump();
    var evalᴛ1 = p.ΔBuffer.Describe();
    fmt.Println(p.ΔBuffer.N, p.N, evalᴛ1);

    p.ΔBuffer.Data = "beta";
    p.of(Buffer.ᏑΔBuffer).Append("!");
    var evalᴛ2 = p.ΔBuffer.Describe();
    fmt.Println(p.Data, p.ΔBuffer.N, evalᴛ2);

    var z = @new<Buffer>();
    z.of(Buffer.ᏑΔBuffer).Append("gamma");
    z.Tag = "fresh";
    var evalᴛ3 = z.ΔBuffer.Describe();
    var evalᴛ4 = Renamed(z.Value);
    fmt.Println(evalᴛ3, z.ΔBuffer.N, evalᴛ4);
}

} // end main_package
