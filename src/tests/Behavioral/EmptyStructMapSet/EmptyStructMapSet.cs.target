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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string missingˢ = "missing"u8;

internal static (@string, bool) lookup(registry reg, uint32 id) {
    var (e, ok) = reg[id, ꟷ];
    if (!ok) {
        return (missingˢ, false);
    }
    return (e.tag, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object litLenˢ = (@string)"lit len:"u8;
private static readonly @string tcpˢ = "tcp"u8;
private static readonly @string sshˢ = "ssh"u8;
private static readonly @string udpˢ = "udp"u8;
private static readonly @string domainˢ = "domain"u8;
private static readonly object chanAssertˢ = (@string)"chan assert:"u8;

internal static void Main() {
    var seen = new map<nint, EmptyStruct>();
    add(seen, 3);
    add(seen, 7);
    add(seen, 3);
    fmt.Println(lenˢ, len(seen));
    foreach (var (_, k) in new nint[]{1, 3, 5, 7}.slice()) {
        fmt.Printf("contains(%d) = %t\n"u8, k, contains(seen, k));
    }
    var lit = new map<@string, EmptyStruct>{
        ["a"u8] = new(),
        ["b"u8] = new()
    };
    lit["c"u8] = new EmptyStruct();
    fmt.Println(litLenˢ, len(lit));
    foreach (var (_, s) in new @string[]{"a"u8, "b"u8, "c"u8, "d"u8}.slice()) {
        var (_, ok) = lit[s, ꟷ];
        fmt.Printf("lit[%s] = %t\n"u8, s, ok);
    }
    var reg = new registry(new map<uint32, entry>{[2] = new(tag: "leaf"u8, size: 8)});
    var (t1, ok1) = lookup(reg, 2);
    var (t2, ok2) = lookup(reg, 9);
    fmt.Println(t1, ok1, t2, ok2);
    var reg2 = new registry(4);
    reg2[7] = new entry(tag: "cap"u8, size: 1);
    var (e7, ok7) = lookup(reg2, 7);
    fmt.Println(len(reg2), e7, ok7);
    var ports = new map<@string, map<@string, nint>>{
        ["udp"u8] = new map<@string, nint>{
            ["domain"u8] = 53},
        ["tcp"u8] = new map<@string, nint>{
            ["smtp"u8] = 25,
            ["ssh"u8] = 22}
    };
    fmt.Println(ports[tcpˢ][sshˢ], ports[udpˢ][domainˢ], len(ports[tcpˢ]));
    var done = new channel<EmptyStruct>(1);
    any anyDone = done;
    var (ch, chOK) = anyDone._<channel<EmptyStruct>>(ᐧ);
    ch.ᐸꟷ(new EmptyStruct());
    ᐸꟷ(done);
    fmt.Println(chanAssertˢ, chOK, len(done));
}

} // end main_package
