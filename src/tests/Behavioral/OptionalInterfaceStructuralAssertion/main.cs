namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Describer {
    @string Describe();
}

partial interface Tagger {
    @string Describe();
    @string Tag();
}

partial struct widget {
    internal @string name;
}

internal static @string Describe(this widget w) {
    return "describe:"u8 + w.name;
}

internal static @string Tag(this widget w) {
    return "tag:"u8 + w.name;
}

partial struct plain {
}

internal static Describer newDescriber(@string name) {
    return new widget(name: name);
}

internal static void Main() {
    var d = newDescriber("alpha");
    fmt.Println(d.Describe());

    var items = new any[]{new widget(name: "beta"), new plain(nil)}.slice();

    foreach (var (_, it) in items) {
        {
            var (t, ok) = it._<Tagger>(ᐧ); if (ok){
                fmt.Println(t.Describe(), t.Tag());
            } else {
                fmt.Println("not a Tagger");
            }
        }
    }
}

} // end main_package
