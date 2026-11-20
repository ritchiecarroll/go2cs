global using PathAlias = go.main_package.Path;

namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Path /*@string*/;

partial interface PrinterHost {
    @string Ref(Path p);
}

partial interface SpecifierHost {
    @string Ref(Path p);
    @string Dir();
}

partial interface EmitHost :
    PrinterHost,
    SpecifierHost
{
    bool Blocked(@string @file);
    new @string Ref(Path p);
}

partial interface MayBeEmittedHost {
    @string Ref(Path p);
}

partial interface left :
    PrinterHost
{
}
partial interface right :
    PrinterHost
{
}
partial interface shared :
    left,
    right
{
}

partial interface Labeler {
    @string Ref(Path p);
    @string Label();
}

partial interface LabeledHost :
    EmitHost,
    Labeler
{
    new @string Ref(Path p);
}

partial interface AliasHost {
    @string Ref(PathAlias q);
    @string Root();
}

partial interface AliasedHost :
    PrinterHost,
    AliasHost
{
    new @string Ref(Path p);
}

partial interface OwnHost :
    PrinterHost,
    SpecifierHost
{
    @string Ref(Path p);
}

partial interface Big {
    @string Ref(Path p);
    @string Dir();
    bool Blocked(@string @file);
    nint Extra();
}

partial struct host {
}

} partial class mainᴛhost {

internal static @string Ref(this host _, Path p) { return "ref:"u8 + ((@string)p); }
internal static @string Dir(this host _) { return "/src"; }
internal static bool Blocked(this host _, @string @file) { return @file == "x"u8; }
internal static @string Label(this host _) { return "L"; }
internal static nint Extra(this host _) { return 7; }
internal static @string Root(this host _) { return "/"; }

} partial class main_package {

partial struct phost {
    internal nint n;
}

} partial class mainᴛphost {

internal static @string Ref(this ref phost h, Path p) {
    h.n++;
    return fmt.Sprint("pref:", p, h.n);
}

internal static @string Dir(this ref phost h) { return "/ptr"; }
internal static bool Blocked(this ref phost h, @string @file) { return true; }

} partial class main_package {

internal static @string emit(MayBeEmittedHost h, Path p) { return h.Ref(p); }

internal static @string viaEmit(EmitHost h, Path p) { return ((PrinterHost)(h)).Ref(p) + "|"u8 + h.Dir(); }

internal static @string generic<T>(T h, Path p)
    where T : EmitHost
{ return h.Ref(p); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string anonˢ = "anon"u8;

internal partial interface anon_h /*dyn*/ :
    PrinterHost,
    SpecifierHost
{
    new @string Ref(Path p);
}

internal static @string anon(anon_h h) {
    return ((PrinterHost)(h)).Ref(anonˢ) + h.Dir();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string aTsˢ = "a.ts"u8;
private static readonly @string bTsˢ = "b.ts"u8;
private static readonly @string sharedˢ = "shared"u8;
private static readonly @string nestedˢ = "nested"u8;
private static readonly @string aliasˢ = "alias"u8;
private static readonly @string ownˢ = "own"u8;
private static readonly @string bigˢ = "big"u8;
private static readonly @string assertˢ = "assert"u8;
private static readonly @string assert2ˢ = "assert2"u8;
private static readonly @string localˢ = "local"u8;

internal partial interface main_local /*dyn*/ :
    PrinterHost,
    Labeler
{
    new @string Ref(Path p);
}

internal static void main() {
    EmitHost h = new host();
    fmt.Println(((PrinterHost)(h)).Ref(aTsˢ), emit(new EmitHostᴠMayBeEmittedHost(h), bTsˢ), h.Dir());

    shared s = new host();
    fmt.Println(s.Ref(sharedˢ), emit(new sharedᴠMayBeEmittedHost(s), (@string)"s2"));

    LabeledHost lh = new host();
    fmt.Println(((PrinterHost)(lh)).Ref(nestedˢ), lh.Label(), emit(new LabeledHostᴠMayBeEmittedHost(lh), (@string)"n2"), viaEmit(lh, (@string)"n3"));

    AliasedHost ah = new host();
    fmt.Println(((PrinterHost)(ah)).Ref(aliasˢ), ah.Root(), emit(new AliasedHostᴠMayBeEmittedHost(ah), (@string)"a2"));

    OwnHost oh = new host();
    fmt.Println(oh.Ref(ownˢ), emit(new OwnHostᴠMayBeEmittedHost(oh), (@string)"o2"), oh.Dir());

    Big b = new host();
    fmt.Println(viaEmit(new BigᴠEmitHost(b), bigˢ), b.Extra());

    EmitHost ph = new phostжEmitHost(Ꮡ(new phost()));
    fmt.Println(((PrinterHost)(ph)).Ref((@string)"p1"), emit(new EmitHostᴠMayBeEmittedHost(ph), (@string)"p2"), ph.Blocked("q"));

    any a = new host();
    if (a._<EmitHost>(out var eh)) {
        fmt.Println(((PrinterHost)(eh)).Ref(assertˢ), eh.Blocked("x"));
    }
    if (a._<LabeledHost>(out var lh2)) {
        fmt.Println(((PrinterHost)(lh2)).Ref(assert2ˢ));
    }

    fmt.Println(generic(h, (@string)"g1"), generic<EmitHost>(new host(), (@string)"g2"));
    
    var hʗ1 = h;
    var f = hʗ1.Ref;
    fmt.Println(f((@string)"mv"));
    main_local l = new host();
    fmt.Println(((PrinterHost)(l)).Ref(localˢ), l.Label());

    fmt.Println(anon(new host()), anon(new EmitHostᴠanon_h(h)));
}

} // end main_package
