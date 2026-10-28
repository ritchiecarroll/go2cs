namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static error errNilArg = errors.New("nil argument");

partial struct node {
    internal @string name;
    internal nint n;
    internal ж<node> next;
}

internal static error checkArg(ж<node> Ꮡp, @string op) {
    if (Ꮡp == nil) {
        return errNilArg;
    }
    return default;
}

internal static ж<node> store(ж<node> Ꮡp) {
    return Ꮡ(new node(name: "holder", next: Ꮡp));
}

internal static @string guarded(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    {
        var err = checkArg(Ꮡp, "guarded"); if (err != nil) {
            return "guarded: "u8 + err.Error();
        }
    }
    return "guarded: "u8 + p.name;
}

internal static @string readName(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    _ = checkArg(Ꮡp, "readname");
    return p.name;
}

internal static @string announce(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    fmt.Println("  side effect ran (param shape)");
    _ = checkArg(Ꮡp, "announce");
    return p.name;
}

internal static nint walkLen(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint n = 0;
    while (Ꮡp != nil) {
        n += p.n;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return n;
}

internal static @string setErr(ж<error> Ꮡdst, error e) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    if (Ꮡdst == nil) {
        return "setErr: nil dst";
    }
    dst = e;
    return fmt.Sprintf("setErr: %v"u8, dst);
}

internal static @string normalize(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    Ꮡp = orDefault(Ꮡp); p = ref Ꮡp.DerefOrNull();
    return p.name;
}

internal static ж<node> fallback = Ꮡ(new node(name: "fallback", n: 7));

internal static ж<node> orDefault(ж<node> Ꮡp) {
    if (Ꮡp == nil) {
        return fallback;
    }
    return Ꮡp;
}

internal static void @try(@string label, Action fn) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Printf("%s -> panic: %v\n"u8, label, r);
                    return;
                }
            }
        }, ref ᒐ);
        fn();
        fmt.Printf("%s -> returned\n"u8, label);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    ж<node> nilNode = default;

    fmt.Println("== 1. nil argument, DELEGATED guard: must not panic ==");
    var nilNodeʗ1 = nilNode;
    @try("checkArg(nil)", () => {
        var err = checkArg(nilNodeʗ1, "x");
        fmt.Printf("  checkArg -> %v (is errNilArg: %v)\n"u8, err, AreEqual(err, errNilArg));
    });
    var nilNodeʗ2 = nilNode;
    @try("store(nil)", () => {
        var h = store(nilNodeʗ2);
        fmt.Printf("  store -> %s, next nil: %v\n"u8, (~h).name, (~h).next == nil);
    });
    var nilNodeʗ3 = nilNode;
    @try("guarded(nil)", () => {
        fmt.Printf("  %s\n"u8, guarded(nilNodeʗ3));
    });

    fmt.Println();
    fmt.Println("== 2. nil argument, UNCONDITIONAL deref: must panic ==");
    var nilNodeʗ4 = nilNode;
    @try("readName(nil)", () => {
        _ = readName(nilNodeʗ4);
    });

    fmt.Println();
    fmt.Println("== 3. nil argument, side effect BEFORE the deref: effect then panic ==");
    var nilNodeʗ5 = nilNode;
    @try("announce(nil)", () => {
        _ = announce(nilNodeʗ5);
    });

    fmt.Println();
    fmt.Println("== 4. nil-terminated walk: repointing to the terminator must not panic ==");
    var chain = Ꮡ(new node(name: "a", n: 1, next: Ꮡ(new node(name: "b", n: 2, next: Ꮡ(new node(name: "c", n: 3))))));
    var chainʗ1 = chain;
    @try("walkLen(chain)", () => {
        fmt.Printf("  walkLen -> %d\n"u8, walkLen(chainʗ1));
    });
    var nilNodeʗ6 = nilNode;
    @try("walkLen(nil)", () => {
        fmt.Printf("  walkLen -> %d\n"u8, walkLen(nilNodeʗ6));
    });

    fmt.Println();
    fmt.Println("== 5. *error parameter: holding nil is not a dereference ==");
    ref var held = ref heap<error>(out var Ꮡheld);
    @try("setErr(&held)", () => {
        fmt.Printf("  %s\n"u8, setErr(Ꮡheld, errNilArg));
    });
    @try("setErr(nil)", () => {
        fmt.Printf("  %s\n"u8, setErr(nil, errNilArg));
    });

    fmt.Println();
    fmt.Println("== 6. normalization: repointed before any read ==");
    var nilNodeʗ7 = nilNode;
    @try("normalize(nil)", () => {
        fmt.Printf("  normalize -> %s\n"u8, normalize(nilNodeʗ7));
    });
    var chainʗ2 = chain;
    @try("normalize(chain)", () => {
        fmt.Printf("  normalize -> %s\n"u8, normalize(chainʗ2));
    });

    fmt.Println();
    fmt.Println("== 7. non-nil arguments: everything unchanged ==");
    var chainʗ3 = chain;
    @try("checkArg(chain)", () => {
        fmt.Printf("  checkArg -> %v\n"u8, checkArg(chainʗ3, "x"));
    });
    var chainʗ4 = chain;
    @try("guarded(chain)", () => {
        fmt.Printf("  %s\n"u8, guarded(chainʗ4));
    });
    var chainʗ5 = chain;
    @try("readName(chain)", () => {
        fmt.Printf("  readName -> %s\n"u8, readName(chainʗ5));
    });
    var chainʗ6 = chain;
    @try("announce(chain)", () => {
        fmt.Printf("  announce -> %s\n"u8, announce(chainʗ6));
    });
    var chainʗ7 = chain;
    @try("store(chain)", () => {
        var h = store(chainʗ7);
        fmt.Printf("  store -> %s, next: %s\n"u8, (~h).name, (~(~h).next).name);
    });

    fmt.Println();
    fmt.Println("== 8. a write through a pointer parameter still lands ==");
    rename(ref (chain).DerefOrNull(), "renamed");
    fmt.Printf("  chain.name -> %s\n"u8, (~chain).name);
}

internal static void rename(ref node p, @string s) {
    p.name = s;
}

} // end main_package
