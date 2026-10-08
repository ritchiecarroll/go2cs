namespace go;

using fmt = fmt_package;
using bufpkg = CrossPackageArrayZeroValue.bufpkg_package;
using CrossPackageArrayZeroValue;

partial class main_package {

partial struct Holder {
    internal bufpkg.State state;
    internal array<byte> readBuf = new(8);
    internal @string tag;
}

partial struct Deep {
    internal bufpkg.Nested nested;
}

internal static void Main() {
    var h = @new<Holder>();
    h.Value.state.Buf[2] = 42;
    fmt.Println((~h).state.Buf[2]);
    h.of(Holder.Ꮡstate).Fill();
    fmt.Println(bufpkg.Sum(h.of(Holder.Ꮡstate)));
    fmt.Println(len((~h).state.Buf), len((~h).state.Seed), (~h).state.N);
    bufpkg.FillArray(h.of(Holder.Ꮡstate).of(bufpkg.State.ᏑBuf));
    fmt.Println(bufpkg.Sum(h.of(Holder.Ꮡstate)));
    fmt.Println(len((~h).readBuf), (~h).readBuf[7], (~h).tag == ""u8);
    var d = @new<Deep>();
    d.of(Deep.Ꮡnested).of(bufpkg.Nested.ᏑInner).Fill();
    fmt.Println(bufpkg.Sum(d.of(Deep.Ꮡnested).of(bufpkg.Nested.ᏑInner)), (~d).nested.Label == ""u8);
}

} // end main_package
