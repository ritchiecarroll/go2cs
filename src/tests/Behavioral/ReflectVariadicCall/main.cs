namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using sort = sort_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

internal static @string join(@string prefix, params ꓸꓸꓸnint partsʗp) {
    var parts = partsʗp.sslice();

    @string @out = prefix;
    foreach (var (_, p) in parts) {
        @out += fmt.Sprintf("/%d"u8, p);
    }
    return @out;
}

internal static nint sum(params ꓸꓸꓸnint numsʗp) {
    var nums = numsʗp.sslice();

    nint total = 0;
    foreach (var (_, n) in nums) {
        total += n;
    }
    return total;
}

internal static @string describe(@string tag, params ꓸꓸꓸany valsʗp) {
    var vals = valsʗp.slice();

    return fmt.Sprintf("%s%v"u8, tag, vals);
}

internal static (nint, error) firstNonZero(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    foreach (var (_, v) in vals) {
        if (v != 0) {
            return (v, default);
        }
    }
    return (0, errors.New("all zero"));
}

internal static slice<@string> recorded;

internal static void record(@string label, params ꓸꓸꓸstring partsʗp) {
    var parts = partsʗp.sslice();

    @string line = label;
    foreach (var (_, p) in parts) {
        line += ":"u8 + p;
    }
    recorded = append(recorded, line);
}

internal static nint between(nint lo, nint hi, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint n = 0;
    foreach (var (_, v) in vals) {
        if (v >= lo && v <= hi) {
            n++;
        }
    }
    return n;
}

partial struct counter {
    internal nint @base;
}

internal static nint Total(this counter c, params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint t = c.@base;
    foreach (var (_, v) in vals) {
        t += v;
    }
    return t;
}

internal static @string callString(any fn, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var @in = new slice<reflectꓸValue>(len(args), () => new(nil));
    foreach (var (i, a) in args) {
        @in[i] = reflect.ValueOf(a);
    }
    var @out = reflect.ValueOf(fn).Call(@in);
    var parts = new slice<@string>(len(@out));
    foreach (var (i, o) in @out) {
        parts[i] = fmt.Sprintf("%v"u8, o.Interface());
    }
    return "["u8 + fmt.Sprint(parts) + "]"u8;
}

internal static void Main() {
    var namesᴷ = new stackAppend<@string>();

    fmt.Println("join3    :", callString(join, "go", (nint)1, (nint)2, (nint)3));
    fmt.Println("join0    :", callString(join, "go"));

    fmt.Println("sum      :", callString(sum, (nint)4, (nint)5, (nint)6));
    fmt.Println("sum0     :", callString(sum));

    fmt.Println("describe :", callString(describe, "tag=", (nint)1, "two", true));

    fmt.Println("firstA   :", callString(firstNonZero, (nint)0, (nint)0, (nint)9));
    fmt.Println("firstB   :", callString(firstNonZero));

    fmt.Println("recordN  :", len(reflect.ValueOf(record).Call(new reflectꓸValue[]{
        reflect.ValueOf("a"), reflect.ValueOf("x"), reflect.ValueOf("y")
    }.slice())));
    fmt.Println("recorded :", recorded);

    fmt.Println("between  :", callString(between, (nint)2, (nint)4, (nint)1, (nint)2, (nint)3, (nint)4, (nint)5));

    var m = reflect.ValueOf(new counter(@base: 100)).MethodByName("Total");
    fmt.Println("method   :", callString(m.Interface(), (nint)1, (nint)2));

    var funcs = new map<@string, any>{
        ["cat"] = ((Funcꓸꓸꓸ<@string, @string, @string>)(@string (@string sep, params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.sslice();
            @string @out = "";
            foreach (var (i, p) in parts) {
                if (i > 0) {
                    @out += sep;
                }
                @out += p;
            }
            return @out;
        })),
        ["count"] = ((Funcꓸꓸꓸ<any, nint>)(nint (params ꓸꓸꓸany valsʗp) => {
            var vals = valsʗp.sslice();
            return len(vals);
        })),
        ["pair"] = ((Funcꓸꓸꓸ<nint, nint, (nint, nint)>)((nint, nint) (nint a, params ꓸꓸꓸnint restʗp) => {
            var rest = restʗp.sslice();
            nint t = 0;
            foreach (var (_, r) in rest) {
                t += r;
            }
            return (a, t);
        }))
    };
    var names = new slice<@string>(0, len(funcs));
    foreach (var (name, _) in funcs) {
        names = append(namesᴷ, names, name);
    }
    sort.Strings(names);
    foreach (var (_, name) in names) {
        var fn = reflect.ValueOf(funcs[name]);
        fmt.Printf("funcmap %-6s: variadic=%v numIn=%d in-last=%v\n"u8,
            name, fn.Type().IsVariadic(), fn.Type().NumIn(), fn.Type().In(fn.Type().NumIn() - 1));
    }
    fmt.Println("cat      :", callString(funcs["cat"], "-", "a", "b", "c"));
    fmt.Println("count    :", callString(funcs["count"], (nint)1, (nint)2, (nint)3, (nint)4));
    fmt.Println("pair     :", callString(funcs["pair"], (nint)7, (nint)1, (nint)2));

    fmt.Println("toofew   :", recoverText(() => {
        reflect.ValueOf(join).Call(default);
    }));
    fmt.Println("ok-empty :", recoverText(() => {
        reflect.ValueOf(sum).Call(default);
    }));
}

internal static @string /*msg*/ recoverText(Action f) {
    @string msg = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    msg = fmt.Sprintf("%v"u8, r);
                }
            }
        }, ref ᒐ);
        f();
        msg = "<no panic>";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package
