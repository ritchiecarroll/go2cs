global using writeOps_testFnc = object;
global using fileOps_testFnc = object;
global using fileOps_fileMaker = object;
global using localAliases_hdr = go.main_package.Header;
global using aliasedIfaceConv_S = go.fmt_package.Stringer;

namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Header {
    public @string Name;
    public int64 Size;
}

public static @string String(this Header h) {
    return fmt.Sprint(h.Name, (@string)"/"u8, h.Size);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object writeˢ = (@string)"write:"u8;
private static readonly object closeˢ = (@string)"close:"u8;
private static readonly object writeOpsOpsˢ = (@string)"writeOps ops:"u8;

internal partial struct writeOps_opWrite /*dyn*/ {
    internal @string str;
}

internal partial struct writeOps_opClose /*dyn*/ {
    internal @string err;
}

internal static void writeOps() {
    var ops = new writeOps_testFnc[]{new writeOps_opWrite("abc"u8), new writeOps_opClose("eof"u8), new writeOps_opWrite("de"u8)}.slice();
    foreach (var (_, op) in ops) {
        switch (op.type()) {
        case writeOps_opWrite v: {
            fmt.Println(writeˢ, v.str);
            break;
        }
        case writeOps_opClose v: {
            fmt.Println(closeˢ, v.err);
            break;
        }}
    }
    fmt.Println(writeOpsOpsˢ, len(ops));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fileOpsMakersˢ = (@string)"fileOps makers:"u8;
private static readonly object testsˢ = (@string)"tests:"u8;
private static readonly object totalˢ = (@string)"total:"u8;

internal partial struct fileOps_makeReg /*dyn*/ {
    internal int64 size;
}

internal partial struct fileOps_makeSparse /*dyn*/ {
    internal int64 size;
    internal int64 holes;
}

internal static void fileOps() {
    var makers = new fileOps_fileMaker[]{new fileOps_makeReg(4), new fileOps_makeSparse(8, 2)}.slice();
    var tests = new fileOps_testFnc[]{new fileOps_makeReg(1), new fileOps_makeSparse(3, 1), new fileOps_makeReg(2)}.slice();
    var total = (int64)0;
    foreach (var (_, m) in makers) {
        switch (m.type()) {
        case fileOps_makeReg v: {
            total += v.size;
            break;
        }
        case fileOps_makeSparse v: {
            total += v.size - v.holes;
            break;
        }}
    }
    fmt.Println(fileOpsMakersˢ, len(makers), testsˢ, len(tests), totalˢ, total);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localAliasesˢ = (@string)"localAliases:"u8;

internal static void localAliases() {
    var h = new localAliases_hdr(Name: "small.txt"u8, Size: 5);
    Header plain = h;
    fmt.Println(localAliasesˢ, h, plain, plain.String(), new Header(Name: "raw"u8, Size: 1));
}

partial interface namer {
    @string String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aliasedIfaceConvˢ = (@string)"aliasedIfaceConv:"u8;

internal static void aliasedIfaceConv() {
    aliasedIfaceConv_S s = new Header(Name: "alias.txt"u8, Size: 2);
    namer n = new fmt_Stringerᴠnamer(s);
    fmt.Stringer direct = new Header(Name: "direct.txt"u8, Size: 4);
    namer d = new fmt_Stringerᴠnamer(direct);
    fmt.Println(aliasedIfaceConvˢ, n.String(), d.String());
}

internal static void Main() {
    writeOps();
    fileOps();
    localAliases();
    aliasedIfaceConv();
    secondWriteOps();
    secondAliases();
}

} // end main_package
