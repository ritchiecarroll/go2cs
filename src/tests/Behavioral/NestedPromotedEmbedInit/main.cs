namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct flags {
    internal nint width;
    internal @string name;
}

partial struct formatter {
    internal partial ref flags flags { get; }
    internal array<byte> pad = new(3);
}

partial struct printer {
    internal formatter f;
}

internal static void Main() {
    var p = @new<printer>();
    p.Value.f.width = 7;
    p.Value.f.name = "hi"u8;
    p.Value.f.pad[0] = 65;
    fmt.Println((~p).f.width, (~p).f.name, (~p).f.pad[0]);
    var r = Ꮡ(new printer(nil));
    r.Value.f.width = 9;
    r.Value.f.pad[2] = 90;
    fmt.Println((~r).f.width, (~r).f.pad[2]);
}

} // end main_package
