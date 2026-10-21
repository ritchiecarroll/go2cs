namespace go;

using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using time = time_package;
using io;

partial class main_package {

partial struct counter {
    internal nint n;
}

internal static nint Len(this counter c) {
    return c.n;
}

partial struct box {
    internal nint n;
}

internal static nint Len(this ref box b) {
    return b.n;
}

internal partial interface run_type /*dyn*/ {
    nint Len();
}

internal static nint run(nint n) {
    nint total = 0;

    for (nint i = 0; i < n; i++) {
        any v = new counter(n: (i & 7));

        {
            var (l, ok) = v._<run_type>(ᐧ); if (ok) {
                total += l.Len();
            }
        }

        any p = Ꮡ(new box(n: (i & 3)));

        {
            var (l, ok) = p._<run_type>(ᐧ); if (ok) {
                total += l.Len();
            }
        }

        var err = ((error)new fs.PathErrorжerror(Ꮡ(new fs.PathError(Op: "open", Path: "churn", Err: fs.ErrNotExist))));

        if (errors.Is(err, fs.ErrNotExist)) {
            total++;
        }
    }

    return total;
}

internal static void Main() {
    var start = time.Now().UnixNano();

    nint total = run(1000000);

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
