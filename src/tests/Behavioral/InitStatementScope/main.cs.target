namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial interface Shape {
    nint Area();
}

partial interface Named {
    @string Name();
}

partial struct Square {
    internal nint side;
}

public static nint Area(this Square s) {
    return s.side * s.side;
}

partial struct Label /*@string*/;

public static @string Name(this Label l) {
    return (@string)l;
}

internal static UntypedInt limit => 5;

internal static (nint, error) pair(nint n) {
    if (n < 0) {
        return (0, errors.New("negative"));
    }
    return (n * 2, default);
}

internal static nint next(nint n) {
    return n + 1;
}

internal static @string dropped(nint n) {
    nint m = next(n); if (m > 3) {
        return fmt.Sprint("big ", m);
    } else if (m > 1) {
        return fmt.Sprint("mid ", m);
    }
    nint k = n * 3;
    switch (k) {
    case 3: {
        return "three";
    }}

    return "small";
}

internal static @string describe(any x) {
    var y = x;
    switch (y.type()) {
    case {} Δv when Δv._<Shape>(out var v): {
        return fmt.Sprint("shape ", v.Area());
    }
    case {} Δv when Δv._<Named>(out var v): {
        return "named "u8 + v.Name();
    }}
    return "other";
}

internal static @string classify(any x) {
    if (x._<Shape>(out var s)) {
        return fmt.Sprint("area ", s.Area());
    }
    if (x._<Named>(out var l)) {
        return "name "u8 + l.Name();
    }
    if (!x._<error>(out _)) {
        return "not an error";
    }
    return "error";
}

internal static nint lookup(map<@string, nint> m, @string k) {
    if (m[k, ꟷ] is (var v, true)) {
        return v;
    }
    if (m[k + k, ꟷ] is (var w, true)) {
        return -w;
    }
    if (m["zero", ꟷ] is not (_, true)) {
        return 0;
    }
    return 100;
}

internal static @string chain(any x, map<@string, nint> m, nint n) {
    if (x == nil) {
        return "nil";
    } else if (x._<Shape>(out var s)) {
        return fmt.Sprint("shape ", s.Area());
    } else if (m["k", ꟷ] is not (var v, true)) {
        return "no k";
    } else if (pair(n - v) is var (d, err) && (err != nil || d > 10)) {
        return fmt.Sprint("pair ", d, " ", err);
    } else if (errors.New("fallback") is var e && e != nil) {
        return e.Error();
    }
    return "end";
}

internal static nint rebinding() {
    {
        nint limit = next(0); if (limit > 0) {
            fmt.Println("inner limit", limit);
        }
    }
    return main_package.limit;
}

internal static slice<nint> captures() {
    var fnsᴷ = new stackAppend<Func<nint>>();
    var outᴷ = new stackAppend<nint>(moves: true);

    slice<Func<nint>> fns = default;
    nint i = 0;
loop:
    {
        nint x = i; if (x < 3) {
            fns = append(fnsᴷ, fns, () => x);
            i++;
            goto loop;
        }
    }
    slice<nint> @out = default;
    foreach (var (_, f) in fns) {
        @out = append(outᴷ, @out, f());
    }
    @out = move2heap(outᴷ, @out);
    return @out;
}

internal static error siblings(nint n) {
    {
        var (_, err) = pair(n); if (err != nil) {
            return err;
        }
    }
    {
        var (_, err) = pair(n - 10); if (err != nil) {
            return fmt.Errorf("second: %w"u8, err);
        }
    }
    return default;
}

internal static void Main() {
    fmt.Println(dropped(0), "|", dropped(1), "|", dropped(5));
    fmt.Println(describe(new Square(3)), "|", describe((Label)(@string)"go"), "|", describe((nint)7));
    fmt.Println(classify(new Square(2)), "|", classify((Label)(@string)"x"), "|", classify((nint)42), "|", classify(errors.New("e")));
    var m = new map<@string, nint>{["a"] = 1, ["bb"] = 2};
    fmt.Println(lookup(m, "a"), lookup(m, "b"), lookup(m, "c"), lookup(new map<@string, nint>{["zero"] = 0}, "c"));
    fmt.Println(chain(default, default, 0), "|", chain(new Square(4), default, 0), "|", chain((nint)1, default, 0));
    fmt.Println(chain((nint)1, new map<@string, nint>{["k"] = 1}, 0), "|", chain((nint)1, new map<@string, nint>{["k"] = 1}, 9), "|", chain((nint)1, new map<@string, nint>{["k"] = 1}, 3));
    fmt.Println(rebinding(), captures());
    fmt.Println(siblings(-1), siblings(5), siblings(20));
}

} // end main_package
