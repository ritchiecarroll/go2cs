namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct @event {
    internal nint kind;
    internal @string label;
}

internal static Label Label⁔(/*this*/ @event e) {
    return new Label(ΔLabel: e.label, Resource: e.kind);
}

partial struct Label {
    public @string ΔLabel;
    public nint Resource;
}

internal static @string describe(this Label l) {
    return fmt.Sprintf("%s@%d"u8, l.ΔLabel, l.Resource);
}

internal static void main() {
    var e = new @event(kind: 7, label: "cpu");
    var l = e.Label();
    fmt.Println(l.ΔLabel, l.Resource);
    fmt.Println(l.describe());

    var m = new Label(ΔLabel: "mem", Resource: 3);
    m.ΔLabel = m.ΔLabel + "!"u8;
    fmt.Println(m.ΔLabel, m.Resource);
}

} // end main_package
