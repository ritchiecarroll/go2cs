namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface Field {
    bool IsPresent();
    @string Describe();
}

partial struct Expected<T> {
    public T Value;
    public bool Valid;
}

} partial class mainᴛExpected {

public static bool IsPresent<T>(this ref Expected<T> e) { return e.Valid; }

public static @string Describe<T>(this ref Expected<T> e) { return fmt.Sprintf("%v"u8, (e.Value).OrTypedNilOf()); }

} partial class main_package {

partial struct Pair<K, V> {
    public K Key;
    public V Val;
}

} partial class mainᴛPair {

public static bool IsPresent<K, V>(this ref Pair<K, V> p) { return true; }

public static @string Describe<K, V>(this ref Pair<K, V> p) { return fmt.Sprintf("%v=%v"u8, (p.Key).OrTypedNilOf(), (p.Val).OrTypedNilOf()); }

} partial class main_package {

partial struct Fields {
    public Expected<@string> Name;
    public Expected<map<@string, @string>> Imports;
    public Pair<map<@string, nint>, map<nint, @string>> Mapping;
}

partial struct Snapshot {
    internal ж<Expected<map<@string, @string>>> watched;
    internal ж<Expected<@string>> named;
}

internal static void main() {
    ref var f = ref heap<Fields>(new Fields(), out var Ꮡf);
    f.Name = new Expected<@string>(Value: "pkg", Valid: true);
    f.Imports = new Expected<map<@string, @string>>(Value: new map<@string, @string>{["a"] = "b"}, Valid: true);
    f.Mapping = new Pair<map<@string, nint>, map<nint, @string>>(Key: new map<@string, nint>{["one"] = 1}, Val: new map<nint, @string>{[2] = "two"});

    var fields = slice<Field>([new ExpectedжField<@string>(Ꮡf.ᏑName), new ExpectedжField<map<@string, @string>>(Ꮡf.ᏑImports), new PairжField<map<@string, nint>, map<nint, @string>>(Ꮡf.ᏑMapping)]);

    foreach (var (i, x) in range(fields)) {
        fmt.Println(i, x.IsPresent(), x.Describe());
    }

    Func<ж<Expected<map<@string, @string>>>, nint> count = (ж<Expected<map<@string, @string>>> e) => len((~e).Value);
    Func<Expected<map<@string, @string>>> make1 = () => new Expected<map<@string, @string>>(Value: new map<@string, @string>{["x"] = "y", ["z"] = "w"}, Valid: true);

    fmt.Println(count(Ꮡf.ᏑImports), len(make1().Value));

    var s = new Snapshot(watched: Ꮡf.ᏑImports, named: Ꮡf.ᏑName);
    ж<Expected<map<@string, @string>>> local = s.watched;
    local.Value.Value["c"] = "d";
    fmt.Println(len(f.Imports.Value), (~s.named).Value, local.IsPresent());
}

} // end main_package
