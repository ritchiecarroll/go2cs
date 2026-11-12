namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct simpleErr {
    internal @string msg;
}

} partial class mainᴛsimpleErr {

internal static @string Error(this simpleErr e) { return e.msg; }

} partial class main_package {

partial struct multiWrap {
    internal slice<error> errs;
}

} partial class mainᴛmultiWrap {

internal static slice<error> Unwrap(this multiWrap m) { return m.errs; }

} partial class main_package {

partial struct singleWrap {
    internal error err;
}

} partial class mainᴛsingleWrap {

internal static error Unwrap(this singleWrap s) { return s.err; }

} partial class main_package {

internal partial interface classify_type /*dyn*/ {
    error Unwrap();
}

internal partial interface classify_typeᴛ1 /*dyn*/ {
    slice<error> Unwrap();
}

internal static (bool single, bool multi) classify(any v) {
    bool single = default;
    bool multi = default;

    (_, single) = v._<classify_type>(ᐧ);
    (_, multi) = v._<classify_typeᴛ1>(ᐧ);
    return (single, multi);
}

internal static void main() {
    var e1 = new simpleErr("boom");
    var m = new multiWrap(errs: [e1]);
    var s = new singleWrap(err: e1);

    var (ms, mm) = classify(m);
    fmt.Printf("multiWrap:  single=%v multi=%v\n"u8, ms, mm);
    var (ss, sm) = classify(s);
    fmt.Printf("singleWrap: single=%v multi=%v\n"u8, ss, sm);
    var (es, em) = classify(e1);
    fmt.Printf("simpleErr:  single=%v multi=%v\n"u8, es, em);

    {
        var (u, ok) = ((any)m)._<classify_typeᴛ1>(ᐧ); if (ok) {
            fmt.Println("multi unwrap count:", len(u.Unwrap()));
        }
    }
    {
        var (u, ok) = ((any)s)._<classify_type>(ᐧ); if (ok) {
            fmt.Println("single unwrap:", u.Unwrap());
        }
    }
}

} // end main_package
