namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct @event {
    internal nint kind;
    internal @string label;
}

internal static ΔLabel Label(this @event e) {
    return new ΔLabel(ΔΔLabel: e.label, Resource: e.kind);
}

partial struct ΔLabel {
    public @string ΔΔLabel;
    public nint Resource;
}

internal static @string describe(this ΔLabel l) {
    return fmt.Sprintf("%s@%d"u8, l.ΔΔLabel, l.Resource);
}

internal static void Main() {
    var e = new @event(kind: 7, label: "cpu"u8);
    var l = e.Label();
    fmt.Println(l.ΔΔLabel, l.Resource);
    fmt.Println(l.describe());
    var m = new ΔLabel(ΔΔLabel: "mem"u8, Resource: 3);
    m.ΔΔLabel = m.ΔΔLabel + "!"u8;
    fmt.Println(m.ΔΔLabel, m.Resource);
}

} // end main_package
