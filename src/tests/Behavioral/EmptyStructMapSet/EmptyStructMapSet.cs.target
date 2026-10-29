namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void add(map<nint, EmptyStruct> seen, nint k) {
    seen[k] = new EmptyStruct();
}

internal static bool contains(map<nint, EmptyStruct> seen, nint k) {
    var (_, ok) = seen[k, ꟷ];
    return ok;
}

partial struct entry {
    internal @string tag;
    internal nint size;
}

partial struct registry /*map[uint32, entry]*/;

internal static (@string, bool) lookup(registry reg, uint32 id) {
    var (e, ok) = reg[id, ꟷ];
    if (!ok) {
        return ("missing", false);
    }
    return (e.tag, true);
}

internal static void Main() {
    var seen = new map<nint, EmptyStruct>();

    add(seen, 3);
    add(seen, 7);
    add(seen, 3);

    fmt.Println("len:", len(seen));

    foreach (var (_, k) in slice<nint>([1, 3, 5, 7])) {
        fmt.Printf("contains(%d) = %t\n"u8, k, contains(seen, k));
    }

    var lit = new map<@string, EmptyStruct>{
        ["a"] = new(),
        ["b"] = new()
    };
    lit["c"] = new EmptyStruct();
    fmt.Println("lit len:", len(lit));

    foreach (var (_, s) in slice<@string>(["a", "b", "c", "d"])) {
        var (_, ok) = lit[s, ꟷ];
        fmt.Printf("lit[%s] = %t\n"u8, s, ok);
    }

    var reg = new registry(new map<uint32, entry>{[2] = new(tag: "leaf", size: 8)});
    var (t1, ok1) = lookup(reg, 2);
    var (t2, ok2) = lookup(reg, 9);
    fmt.Println(t1, ok1, t2, ok2);

    var reg2 = new registry(4);
    reg2[7] = new entry(tag: "cap", size: 1);
    var (e7, ok7) = lookup(reg2, 7);
    fmt.Println(len(reg2), e7, ok7);

    var ports = new map<@string, map<@string, nint>>{
        ["udp"] = new map<@string, nint>{
            ["domain"] = 53},
        ["tcp"] = new map<@string, nint>{
            ["smtp"] = 25,
            ["ssh"] = 22}
    };
    fmt.Println(ports["tcp"]["ssh"], ports["udp"]["domain"], len(ports["tcp"]));

    var done = new channel<EmptyStruct>(1);
    any anyDone = done;
    var (ch, chOK) = anyDone._<channel<EmptyStruct>>(ᐧ);
    ch.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(done);
    fmt.Println("chan assert:", chOK, len(done));
}

} // end main_package
