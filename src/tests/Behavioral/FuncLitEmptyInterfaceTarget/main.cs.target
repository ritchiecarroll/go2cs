global using Hook = object;

namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal any h;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string negativeˢ = "negative"u8;

internal static Func<nint, (any, error)> scaled(nint n) {
    return (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * n, default!);
    };
}

internal static any retMulti() {
    return (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 2, default!);
    };
}

internal static Hook retNamed() {
    return (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 3, default!);
    };
}

internal static any retConst() {
    return int64 () => 1;
}

internal static any take(any h) {
    return h;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string wrongDynamicTypeˢ = "WRONG DYNAMIC TYPE"u8;

internal static @string call(any h) {
    switch (h.type()) {
    case Func<nint, (any, error)> f: {
        var (@out, err) = f(5);
        var (_, negErr) = f(-1);
        return fmt.Sprint(@out, (@string)" "u8, err, (@string)" "u8, negErr);
    }
    case Func<int64> f: {
        return fmt.Sprint(f());
    }}
    return wrongDynamicTypeˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sevenˢ = "seven"u8;

internal static void Main() {
    any declared = (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 4, default!);
    };
    any assigned = default!;
    assigned = (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 5, default!);
    };
    var argument = take((any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 6, default!);
    });
    var keyed = new holder(h: (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 7, default!);
    }
    );
    var elements = new any[]{(any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 8, default!);
    }, int64 () => 2}.slice();
    var converted = (any)((any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 9, default!);
    });
    var namedConverted = ((Hook)(int64 () => 3));
    var ch = new channel<any>(1);
    ch.ᐸꟷ((any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 10, default!);
    });
    var sent = ᐸꟷ(ch);
    var positional = new holder(int64 () => 4);
    any first = int64 () => 5;
    any second = (any, error) (nint x) => {
        if (x < 0) {
            return (default!, errors.New(negativeˢ));
        }
        return (x * 11, default!);
    };
    any declaredConst = int64 () => 6;
    var byName = new map<@string, any>{["seven"u8] = int64 () => 7};
    any named = (scaled(12)).OrTypedNilFunc();
    var all = new any[]{retMulti(), retNamed(), retConst(), declared, assigned, argument, keyed.h,
        elements[0], elements[1], converted, namedConverted, sent, positional.h, first, second,
        declaredConst, byName[sevenˢ], named}.slice();
    foreach (var (i, f) in all) {
        fmt.Printf("%2d: %s\n"u8, i, call(f));
    }
}

} // end main_package
