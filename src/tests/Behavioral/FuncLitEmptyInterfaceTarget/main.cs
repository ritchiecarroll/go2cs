global using Hook = object;

namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal any h;
}

internal static Func<nint, (any, error)> scaled(nint n) {
    return [GoFunc("scaled.func1")] (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * n, default);
    };
}

internal static any retMulti() {
    return [GoFunc("retMulti.func1")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 2, default);
    };
}

internal static Hook retNamed() {
    return [GoFunc("retNamed.func1")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 3, default);
    };
}

internal static any retConst() { return [GoFunc("retConst.func1")] int64 () => 1; }

internal static any take(any h) { return h; }

internal static @string call(any h) {
    switch (h.type()) {
    case {} Δf when Δf._<Func<nint, (any, error)>>(out var f): {
        var (@out, err) = f(5);
        var (_, negErr) = f(-1);
        return fmt.Sprint(@out, " ", err, " ", negErr);
    }
    case {} Δf when Δf._<Func<int64>>(out var f): {
        return fmt.Sprint(f());
    }}
    return "WRONG DYNAMIC TYPE";
}

internal static void main() {
    any declared = [GoFunc("main.func1")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 4, default);
    };
    any assigned = default;
    assigned = [GoFunc("main.func2")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 5, default);
    };
    var argument = take([GoFunc("main.func3")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 6, default);
    });
    var keyed = new holder(h: [GoFunc("main.func4")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 7, default);
    }
    );
    var elements = new any[]{[GoFunc("main.func5")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 8, default);
    }, [GoFunc("main.func6")] int64 () => 2}.slice();
    var converted = (any)([GoFunc("main.func7")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 9, default);
    });
    var namedConverted = (Hook)([GoFunc("main.func8")] int64 () => 3);
    var ch = new channel<any>(1);
    ch.ᐸꟷ([GoFunc("main.func9")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 10, default);
    });
    var sent = ᐸꟷ(ch);
    var positional = new holder([GoFunc("main.func10")] int64 () => 4);
    any first = [GoFunc("main.func11")] int64 () => 5;
    any second = [GoFunc("main.func12")] (any, error) (nint x) => {
        if (x < 0) {
            return (default, errors.New("negative"));
        }
        return (x * 11, default);
    };
    any declaredConst = [GoFunc("main.func13")] int64 () => 6;
    var byName = new map<@string, any>{["seven"] = [GoFunc("main.func14")] int64 () => 7};
    any named = (scaled(12)).OrTypedNilFunc();

    var all = slice<any>([retMulti(), retNamed(), retConst(), declared, assigned, argument, keyed.h,
        elements[0], elements[1], converted, namedConverted, sent, positional.h, first, second,
        declaredConst, byName["seven"], named]);

    foreach (var (i, f) in range(all)) {
        fmt.Printf("%2d: %s\n"u8, i, call(f));
    }
}

} // end main_package
