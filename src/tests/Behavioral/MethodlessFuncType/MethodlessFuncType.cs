namespace go;

using fmt = fmt_package;

partial class main_package {

// type releaser is a methodless func type — rendered inline as its base delegate

// type lookup is a methodless func type — rendered inline as its base delegate

internal static Action<error> makeReleaser(@string tag, ж<slice<@string>> Ꮡlog) {
    return (error err) => {
        if (err != default!){
            Ꮡlog.ValueSlot = append(Ꮡlog.ValueSlot, tag + ":"u8 + err.Error());
        } else {
            Ꮡlog.ValueSlot = append(Ꮡlog.ValueSlot, tag + ":ok"u8);
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

internal static (nint, Action<error>, error) grab(@string tag, ж<slice<@string>> Ꮡlog) {
    return (len(tag), makeReleaser(tag, Ꮡlog), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object errˢ = (@string)"err:"u8;
private static readonly @string fieldˢ = "field"u8;
private static readonly @string directˢ = "direct"u8;
private static readonly object lookupˢ = (@string)"lookup:"u8;

internal static void Main() {
    ref var log = ref heap<slice<@string>>(out var Ꮡlog);
    var (n, rel, err) = grab("a"u8, Ꮡlog);
    consume(rel, default!);
    fmt.Println((@string)"n:"u8, n, errˢ, err == default!);
    var h = new holder(release: makeReleaser(fieldˢ, Ꮡlog), name: "h"u8);
    h.release(fmt.Errorf("boom"u8));
    consume(h.release, default!);
    Action<error> r = makeReleaser(directˢ, Ꮡlog);
    r(default!);
    consume(r, fmt.Errorf("late"u8));
    Func<@string, (@string, bool)> find = default!;
    find = (@string s) => (s + "!", len(s) > 0);
    var (p, okp) = find("q"u8);
    fmt.Println(lookupˢ, p, okp);
    foreach (var (_, line) in log) {
        fmt.Println(line);
    }
}

} // end main_package
