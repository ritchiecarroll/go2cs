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
    return fmt.Sprint(h.Name, "/", h.Size);
}

internal partial struct writeOps_opWrite /*dyn*/ {
    internal @string str;
}

internal partial struct writeOps_opClose /*dyn*/ {
    internal @string err;
}

internal static void writeOps() {
    var ops = slice<writeOps_testFnc>([new writeOps_opWrite("abc"), new writeOps_opClose("eof"), new writeOps_opWrite("de")]);

    foreach (var (_, op) in ops) {
        switch (op.type()) {
        case writeOps_opWrite v: {
            fmt.Println("write:", v.str);
            break;
        }
        case writeOps_opClose v: {
            fmt.Println("close:", v.err);
            break;
        }}
    }

    fmt.Println("writeOps ops:", len(ops));
}

internal partial struct fileOps_makeReg /*dyn*/ {
    internal int64 size;
}

internal partial struct fileOps_makeSparse /*dyn*/ {
    internal int64 size;
    internal int64 holes;
}

internal static void fileOps() {
    var makers = slice<fileOps_fileMaker>([new fileOps_makeReg(4), new fileOps_makeSparse(8, 2)]);
    var tests = slice<fileOps_testFnc>([new fileOps_makeReg(1), new fileOps_makeSparse(3, 1), new fileOps_makeReg(2)]);

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

    fmt.Println("fileOps makers:", len(makers), "tests:", len(tests), "total:", total);
}

internal static void localAliases() {
    var h = new localAliases_hdr(Name: "small.txt", Size: 5);
    Header plain = h;

    fmt.Println("localAliases:", h, plain, plain.String(), new Header(Name: "raw", Size: 1));
}

partial interface namer {
    @string String();
}

internal static void aliasedIfaceConv() {
    aliasedIfaceConv_S s = new Header(Name: "alias.txt", Size: 2);
    namer n = new fmt_Stringerᴠnamer(s);

    fmt.Stringer direct = new Header(Name: "direct.txt", Size: 4);
    namer d = new fmt_Stringerᴠnamer(direct);

    fmt.Println("aliasedIfaceConv:", n.String(), d.String());
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
