namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct stringer {
    internal @string s;
}

internal static @string String(this stringer b) {
    return b.s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object plChanOfBothDirArrayOf3ˢ = (@string)"pl ChanOf(BothDir, ArrayOf(3,int)) String() Elem().Len()"u8;

internal static void Main() {
    @string name = "x"u8;
    nint n = 3;
    var b = new stringer("bx"u8);
    fmt.Printf("constructed row: ChanOf(BothDir, ArrayOf(3,int)) String()=%s Elem().Len()=%d\n"u8, name, n);
    fmt.Printf("constructed row: ChanOf(BothDir, ArrayOf(3,int)) String()=%s Elem().Len()=%d\n"u8,
        name, n);
    fmt.Printf("f(a,b) s=%s d=%d\n"u8, name, n);
    fmt.Printf("f(a, b) s=%s d=%d\n"u8, name, n);
    fmt.Printf("f(g(1,2)) s=%s d=%d\n"u8, name, n);
    fmt.Printf("a,b s=%s d=%d\n"u8, name, n);
    fmt.Printf("String() s=%s d=%d\n"u8, name, n);
    fmt.Printf("Elem().Len()=%d\n"u8, n);
    fmt.Printf("open( s=%s d=%d\n"u8, name, n);
    fmt.Printf("close) s=%s d=%d\n"u8, name, n);
    fmt.Printf("q(\"a\",b) s=%s\n"u8, name);
    fmt.Printf("one(a,b) s=%s\n"u8, name);
    fmt.Printf("none(a,b)\n"u8);
    fmt.Printf("three(a,b) %s %d %s\n"u8,
        name, n, name);
    fmt.Printf("pct(a,b) 100%% s=%s\n"u8, name);
    fmt.Printf("noNL f(a,b) s=%s d=%d"u8, name, n);
    fmt.Println();
    fmt.Printf("m1 f(a,b) s=%s d=%d\n"u8, b.String(), n);
    fmt.Printf("m2 s=%s d=%d\n"u8, b.String(), n);
    fmt.Printf("m3 f(a,b) s=%s d=%d\n"u8,
        b.String(), n);
    fmt.Printf("m4 f(a,b) s=%s\n"u8, b);
    fmt.Printf("m5 f(a,b) %d %s\n"u8, n, b.String());
    fmt.Println(fmt.Sprintf("sp1 ChanOf(BothDir, ArrayOf(3,int)) String()=%s Elem().Len()=%d"u8, name, n));
    fmt.Println(fmt.Sprintf("sp2 f(a,b) s=%s d=%d"u8,
        name, n));
    fmt.Println(plChanOfBothDirArrayOf3ˢ);
}

} // end main_package
