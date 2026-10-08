namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static error errNilArg = errors.New("nil argument"u8);

partial struct node {
    internal @string name;
    internal nint n;
    internal ж<node> next;
}

internal static error checkArg(ж<node> Ꮡp, @string op) {
    if (Ꮡp == nil) {
        return errNilArg;
    }
    return default!;
}

internal static ж<node> store(ж<node> Ꮡp) {
    return Ꮡ(new node(name: "holder"u8, next: Ꮡp));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string guardedˢ = "guarded"u8;

internal static @string guarded(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    {
        var err = checkArg(Ꮡp, guardedˢ); if (err != default!) {
            return "guarded: "u8 + err.Error();
        }
    }
    return "guarded: "u8 + p.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string readnameˢ = "readname"u8;

internal static @string readName(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    _ = checkArg(Ꮡp, readnameˢ);
    return p.name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sideEffectRanParamShapeˢ = (@string)"  side effect ran (param shape)"u8;
private static readonly @string announceˢ = "announce"u8;

internal static @string announce(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    fmt.Println(sideEffectRanParamShapeˢ);
    _ = checkArg(Ꮡp, announceˢ);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string setErrNilDstˢ = "setErr: nil dst"u8;

internal static @string setErr(ж<error> Ꮡdst, error e) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    if (Ꮡdst == nil) {
        return setErrNilDstˢ;
    }
    dst = e;
    return fmt.Sprintf("setErr: %v"u8, dst);
}

internal static @string normalize(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    Ꮡp = orDefault(Ꮡp); p = ref Ꮡp.DerefOrNull();
    return p.name;
}

internal static ж<node> fallback = Ꮡ(new node(name: "fallback"u8, n: 7));

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
                var r = recover(); if (r != default!) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilArgumentDelegatedˢ = (@string)"== 1. nil argument, DELEGATED guard: must not panic =="u8;
private static readonly @string checkArgNilˢ = "checkArg(nil)"u8;
private static readonly @string storeNilˢ = "store(nil)"u8;
private static readonly @string guardedNilˢ = "guarded(nil)"u8;
private static readonly object nilArgumentUnconditionalˢ = (@string)"== 2. nil argument, UNCONDITIONAL deref: must panic =="u8;
private static readonly @string readNameNilˢ = "readName(nil)"u8;
private static readonly object nilArgumentSideEffectˢ = (@string)"== 3. nil argument, side effect BEFORE the deref: effect then panic =="u8;
private static readonly @string announceNilˢ = "announce(nil)"u8;
private static readonly object nilTerminatedWalkˢ = (@string)"== 4. nil-terminated walk: repointing to the terminator must not panic =="u8;
private static readonly @string walkLenChainˢ = "walkLen(chain)"u8;
private static readonly @string walkLenNilˢ = "walkLen(nil)"u8;
private static readonly object errorParameterHoldingNilˢ = (@string)"== 5. *error parameter: holding nil is not a dereference =="u8;
private static readonly @string setErrHeldˢ = "setErr(&held)"u8;
private static readonly @string setErrNilˢ = "setErr(nil)"u8;
private static readonly object normalizationRepointedˢ = (@string)"== 6. normalization: repointed before any read =="u8;
private static readonly @string normalizeNilˢ = "normalize(nil)"u8;
private static readonly @string normalizeChainˢ = "normalize(chain)"u8;
private static readonly object nonNilArgumentsˢ = (@string)"== 7. non-nil arguments: everything unchanged =="u8;
private static readonly @string checkArgChainˢ = "checkArg(chain)"u8;
private static readonly @string guardedChainˢ = "guarded(chain)"u8;
private static readonly @string readNameChainˢ = "readName(chain)"u8;
private static readonly @string announceChainˢ = "announce(chain)"u8;
private static readonly @string storeChainˢ = "store(chain)"u8;
private static readonly object aWriteThroughAPointerˢ = (@string)"== 8. a write through a pointer parameter still lands =="u8;
private static readonly @string renamedˢ = "renamed"u8;

internal static void Main() {
    ж<node> nilNode = default!;
    fmt.Println(nilArgumentDelegatedˢ);
    var nilNodeʗ1 = nilNode;
    @try(checkArgNilˢ, () => {
        var err = checkArg(nilNodeʗ1, "x"u8);
        fmt.Printf("  checkArg -> %v (is errNilArg: %v)\n"u8, err, AreEqual(err, errNilArg));
    });
    var nilNodeʗ2 = nilNode;
    @try(storeNilˢ, () => {
        var h = store(nilNodeʗ2);
        fmt.Printf("  store -> %s, next nil: %v\n"u8, (~h).name, (~h).next == nil);
    });
    var nilNodeʗ3 = nilNode;
    @try(guardedNilˢ, () => {
        fmt.Printf("  %s\n"u8, guarded(nilNodeʗ3));
    });
    fmt.Println();
    fmt.Println(nilArgumentUnconditionalˢ);
    var nilNodeʗ4 = nilNode;
    @try(readNameNilˢ, () => {
        _ = readName(nilNodeʗ4);
    });
    fmt.Println();
    fmt.Println(nilArgumentSideEffectˢ);
    var nilNodeʗ5 = nilNode;
    @try(announceNilˢ, () => {
        _ = announce(nilNodeʗ5);
    });
    fmt.Println();
    fmt.Println(nilTerminatedWalkˢ);
    var chain = Ꮡ(new node(name: "a"u8, n: 1, next: Ꮡ(new node(name: "b"u8, n: 2, next: Ꮡ(new node(name: "c"u8, n: 3))))));
    var chainʗ1 = chain;
    @try(walkLenChainˢ, () => {
        fmt.Printf("  walkLen -> %d\n"u8, walkLen(chainʗ1));
    });
    var nilNodeʗ6 = nilNode;
    @try(walkLenNilˢ, () => {
        fmt.Printf("  walkLen -> %d\n"u8, walkLen(nilNodeʗ6));
    });
    fmt.Println();
    fmt.Println(errorParameterHoldingNilˢ);
    ref var held = ref heap<error>(out var Ꮡheld);
    @try(setErrHeldˢ, () => {
        fmt.Printf("  %s\n"u8, setErr(Ꮡheld, errNilArg));
    });
    @try(setErrNilˢ, () => {
        fmt.Printf("  %s\n"u8, setErr(nil, errNilArg));
    });
    fmt.Println();
    fmt.Println(normalizationRepointedˢ);
    var nilNodeʗ7 = nilNode;
    @try(normalizeNilˢ, () => {
        fmt.Printf("  normalize -> %s\n"u8, normalize(nilNodeʗ7));
    });
    var chainʗ2 = chain;
    @try(normalizeChainˢ, () => {
        fmt.Printf("  normalize -> %s\n"u8, normalize(chainʗ2));
    });
    fmt.Println();
    fmt.Println(nonNilArgumentsˢ);
    var chainʗ3 = chain;
    @try(checkArgChainˢ, () => {
        fmt.Printf("  checkArg -> %v\n"u8, checkArg(chainʗ3, "x"u8));
    });
    var chainʗ4 = chain;
    @try(guardedChainˢ, () => {
        fmt.Printf("  %s\n"u8, guarded(chainʗ4));
    });
    var chainʗ5 = chain;
    @try(readNameChainˢ, () => {
        fmt.Printf("  readName -> %s\n"u8, readName(chainʗ5));
    });
    var chainʗ6 = chain;
    @try(announceChainˢ, () => {
        fmt.Printf("  announce -> %s\n"u8, announce(chainʗ6));
    });
    var chainʗ7 = chain;
    @try(storeChainˢ, () => {
        var h = store(chainʗ7);
        fmt.Printf("  store -> %s, next: %s\n"u8, (~h).name, (~(~h).next).name);
    });
    fmt.Println();
    fmt.Println(aWriteThroughAPointerˢ);
    rename(ref (chain).DerefOrNull(), renamedˢ);
    fmt.Printf("  chain.name -> %s\n"u8, (~chain).name);
}

internal static void rename(ref node p, @string s) {
    p.name = s;
}

} // end main_package
