namespace go;

using fmt = fmt_package;

partial class main_package {

// type releaser is a methodless func type — rendered inline as its base delegate

// type lookup is a methodless func type — rendered inline as its base delegate

internal static Action<error> makeReleaser(@string tag, ж<slice<@string>> log) {
    return (error err) => {
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
    internal Action<error> release;
    internal @string name;
}

internal static (nint, Action<error>, error) grab(@string tag, ж<slice<@string>> log) {
    return (len(tag), makeReleaser(tag, log), default);
}

internal static void Main() {
    ref var log = ref heap<slice<@string>>(out var Ꮡlog);

    var (n, rel, err) = grab("a", Ꮡlog);
    consume(rel, default);
    fmt.Println("n:", n, "err:", err == nil);

    var h = new holder(release: makeReleaser("field", Ꮡlog), name: "h");
    h.release(fmt.Errorf("boom"u8));
    consume(h.release, default);

    Action<error> r = makeReleaser("direct", Ꮡlog);
    r(default);
    consume(r, fmt.Errorf("late"u8));

    Func<@string, (@string, bool)> find = default;
    find = (@string s) => (s + "!", len(s) > 0);
    var (p, okp) = find("q");
    fmt.Println("lookup:", p, okp);

    foreach (var (_, line) in log) {
        fmt.Println(line);
    }
}

} // end main_package
