namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate void releaser(error _);

internal delegate (@string path, bool ok) lookup(@string _);

internal static releaser makeReleaser(@string tag, ж<slice<@string>> log) {
    return [GoFunc("makeReleaser.func1")] (error err) => {
        if (err != nil) {
            log.ValueSlot = append(log.ValueSlot, tag + ":"u8 + err.Error());
        } else {
            log.ValueSlot = append(log.ValueSlot, tag + ":ok"u8);
        }
    };
}

internal static void consume(Action<error> r, error err) {
    r(err);
}

partial struct holder {
    internal releaser release;
    internal @string name;
}

internal static (nint, releaser, error) grab(@string tag, ж<slice<@string>> log) {
    return (len(tag), makeReleaser(tag, log), default);
}

internal static void main() {
    ref var log = ref heap<slice<@string>>(out var Ꮡlog);

    var (n, rel, err) = grab("a", Ꮡlog);
    consume(builtin.NilSafeDelegateConversion<Action<error>, releaser>(rel), default);
    fmt.Println("n:", n, "err:", err == nil);

    var h = new holder(release: makeReleaser("field", Ꮡlog), name: "h");
    h.release(fmt.Errorf("boom"u8));
    consume(builtin.NilSafeDelegateConversion<Action<error>, releaser>(h.release), default);

    releaser r = makeReleaser("direct", Ꮡlog);
    r(default);
    consume(builtin.NilSafeDelegateConversion<Action<error>, releaser>(r), fmt.Errorf("late"u8));

    lookup find = default;
    find = [GoFunc("main.func1")] (@string s) => (s + "!", len(s) > 0);
    var (p, okp) = find("q");
    fmt.Println("lookup:", p, okp);

    foreach (var (_, line) in range(log)) {
        fmt.Println(line);
    }
}

} // end main_package
