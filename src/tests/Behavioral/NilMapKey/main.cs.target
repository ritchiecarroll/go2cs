namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal @string msg;
}

internal static @string Error(this ref myErr e) {
    return e.msg;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilKeyˢ = "nil-key"u8;
private static readonly @string alphaˢ = "alpha"u8;
private static readonly @string sevenˢ = "seven"u8;
private static readonly object lenˢ = (@string)"len:"u8;
private static readonly object indexˢ = (@string)"index:"u8;
private static readonly object commaOkˢ = (@string)"comma-ok:"u8;
private static readonly object rangeNilEntryˢ = (@string)"range nil entry:"u8;
private static readonly object rangeˢ = (@string)"range:"u8;
private static readonly @string againˢ = "again"u8;
private static readonly object overwriteˢ = (@string)"overwrite:"u8;
private static readonly object neighborsˢ = (@string)"neighbors:"u8;
private static readonly object afterDeleteˢ = (@string)"after delete:"u8;
private static readonly object deleteAgainˢ = (@string)"delete again:"u8;
private static readonly @string backˢ = "back"u8;
private static readonly object reAddˢ = (@string)"re-add:"u8;
private static readonly object afterClearˢ = (@string)"after clear:"u8;
private static readonly object literalˢ = (@string)"literal:"u8;
private static readonly object errorMapˢ = (@string)"error map:"u8;
private static readonly object errorCommaOkˢ = (@string)"error comma-ok:"u8;
private static readonly object errorAfterDeleteˢ = (@string)"error after delete:"u8;
private static readonly object nilMapˢ = (@string)"nil map:"u8;
private static readonly object emptyMapˢ = (@string)"empty map:"u8;
private static readonly object printˢ = (@string)"print:"u8;
private static readonly object printOnlyNilˢ = (@string)"print only nil:"u8;
private static readonly object printErrorMapˢ = (@string)"print error map:"u8;
private static readonly object printPtrMapˢ = (@string)"print ptr map:"u8;

internal static void Main() {
    var m = new map<any, @string>();
    m[default!] = nilKeyˢ;
    m[(@string)"a"u8] = alphaˢ;
    m[(nint)(7)] = sevenˢ;
    fmt.Println(lenˢ, len(m));
    fmt.Println(indexˢ, m[default!]);
    var (v, ok) = m[default!, ꟷ];
    fmt.Println(commaOkˢ, v, ok);
    nint seen = 0;
    var sawNil = false;
    foreach (var (k, kv) in m) {
        seen++;
        if (k == default!) {
            sawNil = true;
            fmt.Println(rangeNilEntryˢ, kv);
        }
    }
    fmt.Println(rangeˢ, seen, sawNil);
    m[default!] = againˢ;
    fmt.Println(overwriteˢ, len(m), m[default!]);
    fmt.Println(neighborsˢ, m[(@string)"a"u8], m[(nint)(7)]);
    delete(m, default!);
    (v, ok) = m[default!, ꟷ];
    fmt.Println(afterDeleteˢ, len(m), ok, v == ""u8);
    delete(m, default!);
    fmt.Println(deleteAgainˢ, len(m));
    m[default!] = backˢ;
    fmt.Println(reAddˢ, len(m), m[default!]);
    clear(m);
    (_, ok) = m[default!, ꟷ];
    fmt.Println(afterClearˢ, len(m), ok);
    var lit = new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2};
    fmt.Println(literalˢ, len(lit), lit[default!], lit[(@string)"b"u8]);
    var em = new map<error, nint>();
    em[default!] = 10;
    em[new myErrжerror(Ꮡ(new myErr("boom"u8)))] = 20;
    fmt.Println(errorMapˢ, len(em), em[default!]);
    var (ev, eok) = em[default!, ꟷ];
    fmt.Println(errorCommaOkˢ, ev, eok);
    delete(em, default!);
    (_, eok) = em[default!, ꟷ];
    fmt.Println(errorAfterDeleteˢ, len(em), eok);
    map<any, @string> nm = default!;
    var (nv, nok) = nm[default!, ꟷ];
    fmt.Println(nilMapˢ, len(nm), nv == ""u8, nok);
    var empty = new map<any, @string>();
    var (_, eok2) = empty[default!, ꟷ];
    fmt.Println(emptyMapˢ, len(empty), eok2);
    var pm = new map<any, nint>{[default!] = 1, [(@string)"b"u8] = 2};
    fmt.Println(printˢ, pm);
    fmt.Printf("printf: %v\n"u8, pm);
    var only = new map<any, nint>{[default!] = 5};
    fmt.Println(printOnlyNilˢ, only);
    var pe = new map<error, nint>{[default!] = 7};
    fmt.Println(printErrorMapˢ, pe);
    ж<nint> pp = default!;
    var pptr = new map<ж<nint>, nint>{[pp] = 8};
    fmt.Println(printPtrMapˢ, pptr);
}

} // end main_package
