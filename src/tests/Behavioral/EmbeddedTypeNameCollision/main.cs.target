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

internal static void Main() {
    ref var b = ref heap<Buffer>(new Buffer(ΔBuffer: inner.NewBuffer("alpha", 3), Tag: "tagged"), out var Ꮡb);

    fmt.Println(b.ΔBuffer.Data, b.ΔBuffer.N);
    fmt.Println(b.Data, b.N);
    fmt.Println(b.ΔBuffer.Describe());
    fmt.Println(Renamed(b));

    var p = Ꮡb;
    p.of(Buffer.ᏑΔBuffer).Bump();
    p.of(Buffer.ᏑΔBuffer).Bump();
    fmt.Println((~p).ΔBuffer.N, (~p).N, (~p).ΔBuffer.Describe());

    p.Value.ΔBuffer.Data = "beta";
    p.of(Buffer.ᏑΔBuffer).Append("!");
    fmt.Println((~p).Data, (~p).ΔBuffer.N, (~p).ΔBuffer.Describe());

    var z = @new<Buffer>();
    z.of(Buffer.ᏑΔBuffer).Append("gamma");
    z.Value.Tag = "fresh";
    fmt.Println((~z).ΔBuffer.Describe(), (~z).ΔBuffer.N, Renamed(z.Value));
}

} // end main_package
