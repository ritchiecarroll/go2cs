namespace go;

using fmt = fmt_package;

partial class main_package {

// type applyFunc is a methodless func type — rendered inline as its base delegate

partial struct machine {
    internal Func<nint, Func<nint, (nint, error)>, (nint, error)> apply;
    internal Func<nint, Func<nint, (nint, error)>, (nint, error)> named;
    internal Func<@string, (@string val, bool ok)> lookup;
}

internal static (nint, error) @double(nint x) {
    return (x * 2, default!);
}

internal static void Main() {
    var m = new machine(
        apply: (nint seed, Func<nint, (nint, error)> op) => {
            var (v, err) = op(seed);
            return (v + 1, err);
        },
        named: (nint seed, Func<nint, (nint, error)> op) => {
            var (v, err) = op(seed);
            return (v * 10, err);
        },
        lookup: (@string name) => {
            return (name + "!", true);
        }
    );
    var (v1, err1) = m.apply(20, @double);
    fmt.Println(v1, err1);
    var (v2, err2) = m.named(3, @double);
    fmt.Println(v2, err2);
    var (s, ok) = m.lookup("go"u8);
    fmt.Println(s, ok);
}

} // end main_package
