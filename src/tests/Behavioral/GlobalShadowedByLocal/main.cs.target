namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct snapshot {
    internal nint addr;
}

internal static snapshot Δtrace = new snapshot(addr: 42);

partial struct acquirer {
    internal nint n;
}

internal static nint trace(this acquirer _) {
    return 1;
}

internal static partial nint collisionGlobalShadow() {
    nint a = main_package.Δtrace.addr;
    nint traceΔ1 = 7;
    return a + traceΔ1;
}

internal static nint plainCounter = 100;

internal static partial nint plainGlobalShadow() {
    nint x = main_package.plainCounter * 2;
    nint plainCounterΔ1 = 5;
    return x + plainCounterΔ1;
}

internal static partial nint acquire(nint n) {
    return n * 10;
}

internal static partial nint nestedBlockShadow(nint kind) {
    nint total = 0;
    nint Δtrace = acquire(1);
    total += Δtrace;
    if (kind == 0){
        total = -1;
    } else {
        if (kind > 1) {
            nint traceΔ1 = acquire(2);
            total += traceΔ1;
        }
        nint traceΔ2 = acquire(3);
        total += traceΔ2;
    }
    total += Δtrace;
    return total;
}

internal static map<@string, slice<@string>> hosts = new map<@string, slice<@string>>{["a"u8] = new @string[]{"x"u8, "y"u8}.slice()};

internal static partial nint tupleInitShadow(@string key) {
    {
        var (hostsΔ1, ok) = hosts[key, ꟷ]; if (ok) {
            return len(hostsΔ1);
        }
    }
    return -1;
}

internal static UntypedInt mlkemQ => 3329;

internal static partial nint constSelfInitShadow() {
    nint mlkemQ = main_package.mlkemQ * 2;
    return mlkemQ + 1;
}

internal static partial nint constNestedInitShadow() {
    {
        nint mlkemQ = main_package.mlkemQ / 1000; if (mlkemQ > 2) {
            return mlkemQ;
        }
    }
    return -1;
}

partial struct qbox {
    internal nint v;
}

internal static partial ж<qbox> newQbox(nint n) {
    return Ꮡ(new qbox(v: n));
}

internal static partial nint constVarInitShadow() {
    var mlkemQ = newQbox(main_package.mlkemQ);
    return (~mlkemQ).v;
}

internal static partial nint constUnshadowedElsewhere() {
    return mlkemQ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mainFieldˢ = (@string)"main field:"u8;

internal static void Main() {
    fmt.Println(constSelfInitShadow(), constNestedInitShadow(), constVarInitShadow(), constUnshadowedElsewhere());
    fmt.Println(collisionGlobalShadow());
    fmt.Println(plainGlobalShadow());
    fmt.Println(Δtrace.addr, plainCounter);
    fmt.Println(nestedBlockShadow(2), nestedBlockShadow(1));
    fmt.Println(tupleInitShadow("a"u8), tupleInitShadow("z"u8));
    var bi = new buildRec(Main: "mod/a"u8, Path: "p"u8);
    fmt.Println(mainFieldˢ, bi.Main, mainField(bi));
}

partial struct buildRec {
    public @string Main;
    public @string Path;
}

internal static @string mainField(buildRec b) {
    return b.Main;
}

} // end main_package
