namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myErr {
    internal @string msg;
}

internal static @string Error(this ref myErr e) {
    return e.msg;
}

internal static void Main() {
    var m = new map<any, @string>();

    m[default] = "nil-key";
    m[(@string)"a"u8] = "alpha";
    m[(nint)7] = "seven";

    fmt.Println("len:", len(m));
    fmt.Println("index:", m[default]);

    var (v, ok) = m[default, ꟷ];
    fmt.Println("comma-ok:", v, ok);

    nint seen = 0;
    var sawNil = false;
    foreach (var (k, kv) in m) {
        seen++;
        if (k == nil) {
            sawNil = true;
            fmt.Println("range nil entry:", kv);
        }
    }
    fmt.Println("range:", seen, sawNil);

    m[default] = "again";
    fmt.Println("overwrite:", len(m), m[default]);

    fmt.Println("neighbors:", m[(@string)"a"u8], m[(nint)7]);

    delete(m, default);
    (v, ok) = m[default, ꟷ];
    fmt.Println("after delete:", len(m), ok, v == ""u8);

    delete(m, default);
    fmt.Println("delete again:", len(m));

    m[default] = "back";
    fmt.Println("re-add:", len(m), m[default]);
    clear(m);
    (_, ok) = m[default, ꟷ];
    fmt.Println("after clear:", len(m), ok);

    var lit = new map<any, nint>{[default] = 1, [(@string)"b"u8] = 2};
    fmt.Println("literal:", len(lit), lit[default], lit[(@string)"b"u8]);

    var em = new map<error, nint>();
    em[default] = 10;
    em[new myErrжerror(Ꮡ(new myErr("boom")))] = 20;
    fmt.Println("error map:", len(em), em[default]);

    var (ev, eok) = em[default, ꟷ];
    fmt.Println("error comma-ok:", ev, eok);

    delete(em, default);
    (_, eok) = em[default, ꟷ];
    fmt.Println("error after delete:", len(em), eok);

    map<any, @string> nm = default;
    var (nv, nok) = nm[default, ꟷ];
    fmt.Println("nil map:", len(nm), nv == ""u8, nok);

    var empty = new map<any, @string>();
    var (_, eok2) = empty[default, ꟷ];
    fmt.Println("empty map:", len(empty), eok2);

    var pm = new map<any, nint>{[default] = 1, [(@string)"b"u8] = 2};
    fmt.Println("print:", pm);
    fmt.Printf("printf: %v\n"u8, pm);

    var only = new map<any, nint>{[default] = 5};
    fmt.Println("print only nil:", only);

    var pe = new map<error, nint>{[default] = 7};
    fmt.Println("print error map:", pe);

    ж<nint> pp = default;
    var pptr = new map<ж<nint>, nint>{[pp] = 8};
    fmt.Println("print ptr map:", pptr);
}

} // end main_package
