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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string allZeroˢ = "all zero"u8;

internal static (nint, error) firstNonZero(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    foreach (var (_, v) in vals) {
        if (v != 0) {
            return (v, default!);
        }
    }
    return (0, errors.New(allZeroˢ));
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object join3ˢ = (@string)"join3    :"u8;
private static readonly object join0ˢ = (@string)"join0    :"u8;
private static readonly object sumˢ = (@string)"sum      :"u8;
private static readonly object sum0ˢ = (@string)"sum0     :"u8;
private static readonly object describeˢ = (@string)"describe :"u8;
private static readonly object tagˢ = (@string)"tag="u8;
private static readonly object twoˢ = (@string)"two"u8;
private static readonly object firstAˢ = (@string)"firstA   :"u8;
private static readonly object firstBˢ = (@string)"firstB   :"u8;
private static readonly object recordNˢ = (@string)"recordN  :"u8;
private static readonly object recordedˢ = (@string)"recorded :"u8;
private static readonly object betweenˢ = (@string)"between  :"u8;
private static readonly @string totalˢ = "Total"u8;
private static readonly object methodˢ = (@string)"method   :"u8;
private static readonly object catˢ = (@string)"cat      :"u8;
private static readonly @string catˢ2 = "cat"u8;
private static readonly object countˢ = (@string)"count    :"u8;
private static readonly @string countˢ2 = "count"u8;
private static readonly object pairˢ = (@string)"pair     :"u8;
private static readonly @string pairˢ2 = "pair"u8;
private static readonly object toofewˢ = (@string)"toofew   :"u8;
private static readonly object okEmptyˢ = (@string)"ok-empty :"u8;

internal static void Main() {
    fmt.Println(join3ˢ, callString(join, (@string)"go"u8, (nint)(1), (nint)(2), (nint)(3)));
    fmt.Println(join0ˢ, callString(join, (@string)"go"u8));
    fmt.Println(sumˢ, callString(sum, (nint)(4), (nint)(5), (nint)(6)));
    fmt.Println(sum0ˢ, callString(sum));
    fmt.Println(describeˢ, callString(describe, tagˢ, (nint)(1), twoˢ, true));
    fmt.Println(firstAˢ, callString(firstNonZero, (nint)(0), (nint)(0), (nint)(9)));
    fmt.Println(firstBˢ, callString(firstNonZero));
    fmt.Println(recordNˢ, len(reflect.ValueOf(record).Call(new reflectꓸValue[]{
        reflect.ValueOf((@string)"a"u8), reflect.ValueOf((@string)"x"u8), reflect.ValueOf((@string)"y"u8)
    }.slice())));
    fmt.Println(recordedˢ, recorded);
    fmt.Println(betweenˢ, callString(between, (nint)(2), (nint)(4), (nint)(1), (nint)(2), (nint)(3), (nint)(4), (nint)(5)));
    var m = reflect.ValueOf(new counter(@base: 100)).MethodByName(totalˢ);
    fmt.Println(methodˢ, callString(m.Interface(), (nint)(1), (nint)(2)));
    var funcs = new map<@string, any>{
        ["cat"u8] = ((Funcꓸꓸꓸ<@string, @string, @string>)(@string (@string sep, params ꓸꓸꓸstring partsʗp) => {
            var parts = partsʗp.sslice();
            @string @out = ""u8;
            foreach (var (i, p) in parts) {
                if (i > 0) {
                    @out += sep;
                }
                @out += p;
            }
            return @out;
        })),
        ["count"u8] = ((Funcꓸꓸꓸ<any, nint>)(nint (params ꓸꓸꓸany valsʗp) => {
            var vals = valsʗp.sslice();
            return len(vals);
        })),
        ["pair"u8] = ((Funcꓸꓸꓸ<nint, nint, (nint, nint)>)((nint, nint) (nint a, params ꓸꓸꓸnint restʗp) => {
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
        names = append(names, name);
    }
    sort.Strings(names);
    foreach (var (_, name) in names) {
        var fn = reflect.ValueOf(funcs[name]);
        fmt.Printf("funcmap %-6s: variadic=%v numIn=%d in-last=%v\n"u8,
            name, fn.Type().IsVariadic(), fn.Type().NumIn(), fn.Type().In(fn.Type().NumIn() - 1));
    }
    fmt.Println(catˢ, callString(funcs[catˢ2], (@string)"-"u8, (@string)"a"u8, (@string)"b"u8, (@string)"c"u8));
    fmt.Println(countˢ, callString(funcs[countˢ2], (nint)(1), (nint)(2), (nint)(3), (nint)(4)));
    fmt.Println(pairˢ, callString(funcs[pairˢ2], (nint)(7), (nint)(1), (nint)(2)));
    fmt.Println(toofewˢ, recoverText(() => {
        reflect.ValueOf(join).Call(default!);
    }));
    fmt.Println(okEmptyˢ, recoverText(() => {
        reflect.ValueOf(sum).Call(default!);
    }));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "<no panic>"u8;

internal static @string /*msg*/ recoverText(Action f) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprintf("%v"u8, r);
                }
            }
        }, ref ᒐ);
        f();
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package
