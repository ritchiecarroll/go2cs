namespace go.github.com.itchyny;

using base64 = encoding.base64_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using iter = iter_package;
using maps = maps_package;
using math = math_package;
using big = go.math.big_package;
using url = net.url_package;
using reflect = reflect_package;
using regexp = regexp_package;
using slices = slices_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using timefmt = go.github.com.itchyny.timefmt_go.timefmt_package;
using encoding;
using go.github.com.itchyny.timefmt_go;
using go.math;
using go.unicode;
using net;

partial class gojq_package {

internal static map<@string, slice<ж<FuncDef>>> builtinFuncDefs;

internal static UntypedInt argcount0 => /* 1 << iota */ 1;
internal static UntypedInt argcount1 => 2;
internal static UntypedInt argcount2 => 4;
internal static UntypedInt argcount3 => 8;

partial struct function {
    internal nint argcount;
    internal bool iter;
    internal Func<any, slice<any>, any> callback;
}

internal static bool accept(this function fn, nint cnt) {
    return (fn.argcount & (((nint)1).Lsh((int64)(cnt)))) != 0;
}

internal static map<@string, function> internalFuncs;

[GoInit] internal static void initΔ1() {
    internalFuncs = new map<@string, function>{
        ["empty"] = argFunc0(default),
        ["path"] = argFunc1(default),
        ["env"] = argFunc0(default),
        ["builtins"] = argFunc0(default),
        ["input"] = argFunc0(default),
        ["modulemeta"] = argFunc0(default),
        ["debug"] = argFunc1(default),
        ["abs"] = argFunc0(funcAbs),
        ["length"] = argFunc0(funcLength),
        ["utf8bytelength"] = argFunc0(funcUtf8ByteLength),
        ["keys"] = argFunc0(funcKeys),
        ["has"] = argFunc1(funcHas),
        ["add"] = argFunc0(funcAdd),
        ["toboolean"] = argFunc0(funcToBoolean),
        ["tonumber"] = argFunc0(funcToNumber),
        ["tostring"] = argFunc0(funcToString),
        ["type"] = argFunc0(funcType),
        ["reverse"] = argFunc0(funcReverse),
        ["contains"] = argFunc1(funcContains),
        ["inside"] = argFunc1(funcInside),
        ["indices"] = argFunc1(funcIndices),
        ["index"] = argFunc1(funcIndex),
        ["rindex"] = argFunc1(funcRindex),
        ["startswith"] = argFunc1(funcStartsWith),
        ["endswith"] = argFunc1(funcEndsWith),
        ["ltrimstr"] = argFunc1(funcLtrimstr),
        ["rtrimstr"] = argFunc1(funcRtrimstr),
        ["trimstr"] = argFunc1(funcTrimstr),
        ["ltrim"] = argFunc0(funcLtrim),
        ["rtrim"] = argFunc0(funcRtrim),
        ["trim"] = argFunc0(funcTrim),
        ["explode"] = argFunc0(funcExplode),
        ["implode"] = argFunc0(funcImplode),
        ["split"] = argFunc1(funcSplit),
        ["join"] = argFunc1(funcJoin),
        ["ascii_downcase"] = argFunc0(funcASCIIDowncase),
        ["ascii_upcase"] = argFunc0(funcASCIIUpcase),
        ["tojson"] = argFunc0(funcToJSON),
        ["fromjson"] = argFunc0(funcFromJSON),
        ["format"] = argFunc1(funcFormat),
        ["_tohtml"] = argFunc0(funcToHTML),
        ["_touri"] = argFunc0(funcToURI),
        ["_tourid"] = argFunc0(funcToURId),
        ["_tocsv"] = argFunc0(funcToCSV),
        ["_totsv"] = argFunc0(funcToTSV),
        ["_tosh"] = argFunc0(funcToSh),
        ["_tobase64"] = argFunc0(funcToBase64),
        ["_tobase64d"] = argFunc0(funcToBase64d),
        ["_index"] = argFunc2(funcIndex2),
        ["_slice"] = argFunc3(funcSlice),
        ["_plus"] = argFunc0(funcOpPlus),
        ["_negate"] = argFunc0(funcOpNegate),
        ["_add"] = argFunc2(funcOpAdd),
        ["_subtract"] = argFunc2(funcOpSub),
        ["_multiply"] = argFunc2(funcOpMul),
        ["_divide"] = argFunc2(funcOpDiv),
        ["_modulo"] = argFunc2(funcOpMod),
        ["_alternative"] = argFunc2(funcOpAlt),
        ["_equal"] = argFunc2(funcOpEq),
        ["_notequal"] = argFunc2(funcOpNe),
        ["_greater"] = argFunc2(funcOpGt),
        ["_less"] = argFunc2(funcOpLt),
        ["_greatereq"] = argFunc2(funcOpGe),
        ["_lesseq"] = argFunc2(funcOpLe),
        ["flatten"] = new((nint)((nint)argcount0 | (nint)argcount1), false, funcFlatten),
        ["_range"] = new(argcount3, true, funcRange),
        ["min"] = argFunc0(funcMin),
        ["_min_by"] = argFunc1(funcMinBy),
        ["max"] = argFunc0(funcMax),
        ["_max_by"] = argFunc1(funcMaxBy),
        ["sort"] = argFunc0(funcSort),
        ["_sort_by"] = argFunc1(funcSortBy),
        ["_group_by"] = argFunc1(funcGroupBy),
        ["unique"] = argFunc0(funcUnique),
        ["_unique_by"] = argFunc1(funcUniqueBy),
        ["sin"] = mathFunc("sin", math.Sin),
        ["cos"] = mathFunc("cos", math.Cos),
        ["tan"] = mathFunc("tan", math.Tan),
        ["asin"] = mathFunc("asin", math.Asin),
        ["acos"] = mathFunc("acos", math.Acos),
        ["atan"] = mathFunc("atan", math.Atan),
        ["sinh"] = mathFunc("sinh", math.Sinh),
        ["cosh"] = mathFunc("cosh", math.Cosh),
        ["tanh"] = mathFunc("tanh", math.Tanh),
        ["asinh"] = mathFunc("asinh", math.Asinh),
        ["acosh"] = mathFunc("acosh", math.Acosh),
        ["atanh"] = mathFunc("atanh", math.Atanh),
        ["floor"] = mathFunc("floor", math.Floor),
        ["round"] = mathFunc("round", math.Round),
        ["nearbyint"] = mathFunc("nearbyint", math.RoundToEven),
        ["rint"] = mathFunc("rint", math.RoundToEven),
        ["ceil"] = mathFunc("ceil", math.Ceil),
        ["trunc"] = mathFunc("trunc", math.Trunc),
        ["significand"] = mathFunc("significand", funcSignificand),
        ["fabs"] = mathFunc("fabs", math.Abs),
        ["sqrt"] = mathFunc("sqrt", math.Sqrt),
        ["cbrt"] = mathFunc("cbrt", math.Cbrt),
        ["exp"] = mathFunc("exp", math.Exp),
        ["exp10"] = mathFunc("exp10", funcExp10),
        ["exp2"] = mathFunc("exp2", math.Exp2),
        ["expm1"] = mathFunc("expm1", math.Expm1),
        ["frexp"] = argFunc0(funcFrexp),
        ["modf"] = argFunc0(funcModf),
        ["log"] = mathFunc("log", math.Log),
        ["log10"] = mathFunc("log10", math.Log10),
        ["log1p"] = mathFunc("log1p", math.Log1p),
        ["log2"] = mathFunc("log2", math.Log2),
        ["logb"] = mathFunc("logb", math.Logb),
        ["gamma"] = mathFunc("gamma", math.Gamma),
        ["tgamma"] = mathFunc("tgamma", math.Gamma),
        ["lgamma"] = mathFunc("lgamma", funcLgamma),
        ["erf"] = mathFunc("erf", math.Erf),
        ["erfc"] = mathFunc("erfc", math.Erfc),
        ["j0"] = mathFunc("j0", math.J0),
        ["j1"] = mathFunc("j1", math.J1),
        ["y0"] = mathFunc("y0", math.Y0),
        ["y1"] = mathFunc("y1", math.Y1),
        ["atan2"] = mathFunc2("atan2", math.Atan2),
        ["copysign"] = mathFunc2("copysign", math.Copysign),
        ["drem"] = mathFunc2("drem", funcDrem),
        ["fdim"] = mathFunc2("fdim", math.Dim),
        ["fmax"] = mathFunc2("fmax", funcFmax),
        ["fmin"] = mathFunc2("fmin", funcFmin),
        ["fmod"] = mathFunc2("fmod", math.Mod),
        ["hypot"] = mathFunc2("hypot", math.Hypot),
        ["jn"] = mathFunc2("jn", funcJn),
        ["nextafter"] = mathFunc2("nextafter", math.Nextafter),
        ["nexttoward"] = mathFunc2("nexttoward", math.Nextafter),
        ["remainder"] = mathFunc2("remainder", math.Remainder),
        ["ldexp"] = mathFunc2("ldexp", funcLdexp),
        ["scalb"] = mathFunc2("scalb", funcLdexp),
        ["scalbln"] = mathFunc2("scalbln", funcLdexp),
        ["yn"] = mathFunc2("yn", funcYn),
        ["pow"] = mathFunc2("pow", math.Pow),
        ["fma"] = mathFunc3("fma", math.FMA),
        ["infinite"] = argFunc0(funcInfinite),
        ["isfinite"] = argFunc0(funcIsfinite),
        ["isinfinite"] = argFunc0(funcIsinfinite),
        ["nan"] = argFunc0(funcNan),
        ["isnan"] = argFunc0(funcIsnan),
        ["isnormal"] = argFunc0(funcIsnormal),
        ["setpath"] = argFunc2(funcSetpath),
        ["delpaths"] = argFunc1(funcDelpaths),
        ["getpath"] = argFunc1(funcGetpath),
        ["transpose"] = argFunc0(funcTranspose),
        ["bsearch"] = argFunc1(funcBsearch),
        ["gmtime"] = argFunc0(funcGmtime),
        ["localtime"] = argFunc0(funcLocaltime),
        ["mktime"] = argFunc0(funcMktime),
        ["strftime"] = argFunc1(funcStrftime),
        ["strflocaltime"] = argFunc1(funcStrflocaltime),
        ["strptime"] = argFunc1(funcStrptime),
        ["now"] = argFunc0(funcNow),
        ["_match"] = argFunc3(default),
        ["_captures"] = argFunc0(funcCaptures),
        ["error"] = new((nint)((nint)argcount0 | (nint)argcount1), false, funcError),
        ["halt"] = argFunc0(funcHalt),
        ["halt_error"] = new((nint)((nint)argcount0 | (nint)argcount1), false, funcHaltError)
    };
}

internal static function argFunc0(Func<any, any> f) {
    return new function(
        argcount0, false, (any v, slice<any> _) => f(v)
    );
}

internal static function argFunc1(Func<any, any, any> f) {
    return new function(
        argcount1, false, (any v, slice<any> args) => f(v, args[0])
    );
}

internal static function argFunc2(Func<any, any, any, any> f) {
    return new function(
        argcount2, false, (any v, slice<any> args) => f(v, args[0], args[1])
    );
}

internal static function argFunc3(Func<any, any, any, any, any> f) {
    return new function(
        argcount3, false, (any v, slice<any> args) => f(v, args[0], args[1], args[2])
    );
}

internal static function mathFunc(@string name, Func<float64, float64> f) {
    return argFunc0((any v) => {
        var (x, ok) = toFloat(v);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, v));
        }
        return f(x);
    });
}

internal static function mathFunc2(@string name, Func<float64, float64, float64> f) {
    return argFunc2((any _, any x, any y) => {
        var (l, ok) = toFloat(x);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, x));
        }
        (var r, ok) = toFloat(y);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, y));
        }
        return f(l, r);
    });
}

internal static function mathFunc3(@string name, Func<float64, float64, float64, float64> f) {
    return argFunc3((any _, any a, any b, any c) => {
        var (x, ok) = toFloat(a);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, a));
        }
        (var y, ok) = toFloat(b);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, b));
        }
        (var z, ok) = toFloat(c);
        if (!ok) {
            return Ꮡ(new func0TypeError(name, c));
        }
        return f(x, y, z);
    });
}

internal static any funcAbs(any v) {
    switch (v.type()) {
    case nint vΔ1: {
        if (vΔ1 >= 0) {
            return vΔ1;
        }
        return negate(vΔ1);
    }
    case float64 vΔ1: {
        return math.Abs(vΔ1);
    }
    case ж<bigꓸInt> vΔ1: {
        if (vΔ1.Sign() >= 0) {
            return vΔ1.OrTypedNil();
        }
        return @new<bigꓸInt>().Abs(vΔ1).OrTypedNil();
    }
    case json.Number vΔ1: {
        if (!strings.HasPrefix(vΔ1.String(), "-")) {
            return vΔ1;
        }
        return vΔ1[1..];
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func0TypeError("abs", vΔ1));
    }}
}

internal static any funcLength(any v) {
    switch (v.type()) {
    case null: {
        return (nint)0;
    }
    case nint vΔ1: {
        if (vΔ1 >= 0) {
            return vΔ1;
        }
        return negate(vΔ1);
    }
    case float64 vΔ1: {
        return math.Abs(vΔ1);
    }
    case ж<bigꓸInt> vΔ1: {
        if (vΔ1.Sign() >= 0) {
            return vΔ1.OrTypedNil();
        }
        return @new<bigꓸInt>().Abs(vΔ1).OrTypedNil();
    }
    case json.Number vΔ1: {
        if (!strings.HasPrefix(vΔ1.String(), "-")) {
            return vΔ1;
        }
        return vΔ1[1..];
    }
    case @string vΔ1: {
        return len(slice<rune>(vΔ1));
    }
    case slice<any> vΔ1: {
        return len(vΔ1);
    }
    case map<@string, any> vΔ1: {
        return len(vΔ1);
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func0TypeError("length", vΔ1));
    }}
}

internal static any funcUtf8ByteLength(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("utf8bytelength", v));
    }
    return len(s);
}

internal static any funcKeys(any v) {
    switch (v.type()) {
    case slice<any> vΔ1: {
        var w = new slice<any>(len(vΔ1));
        foreach (var i in range(len(vΔ1))) {
            w[i] = i;
        }
        return w;
    }
    case map<@string, any> vΔ1: {
        var w = new slice<any>(len(vΔ1));
        foreach (var (i, k) in range(keys(vΔ1))) {
            w[i] = k;
        }
        return w;
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func0TypeError("keys", vΔ1));
    }}
}

internal static slice<@string> keys(map<@string, any> v) {
    var w = new slice<@string>(len(v));
    nint i = default;
    foreach (var (k, _) in v) {
        w[i] = k;
        i++;
    }
    sort.Strings(w);
    return w;
}

internal static (slice<any>, bool) values(any v) {
    switch (v.type()) {
    case slice<any> vΔ1: {
        return (vΔ1, true);
    }
    case map<@string, any> vΔ1: {
        var vs = new slice<any>(len(vΔ1));
        foreach (var (i, k) in range(keys(vΔ1))) {
            vs[i] = vΔ1[k];
        }
        return (vs, true);
    }
    default: {
        var vΔ1 = v;
        return (default, false);
    }}
}

internal static any funcHas(any v, any x) {
    switch (v.type()) {
    case slice<any> vΔ1: {
        var (xΔ1, ok) = toInt(x); if (ok) {
            return 0 <= xΔ1 && xΔ1 < len(vΔ1);
        }
        break;
    }
    case map<@string, any> vΔ1: {
        if (x._<@string>(out var xΔ2)) {
            var (_, okΔ1) = vΔ1[xΔ2, ꟷ];
            return okΔ1;
        }
        break;
    }
    case null: {
        return false;
    }}
    return Ꮡ(new func1TypeError("has", v, x));
}

internal static any funcAdd(any v) {
    var (vs, ok) = values(v);
    if (!ok) {
        return Ꮡ(new func0TypeError("add", v));
    }
    return add(slices.Values<slice<any>, any>(vs));
}

internal static any add(iter.Seq<any> xs) {
    any v = default;
    foreach (var x in range<any>(xs.Invoke)) {
        switch (x.type()) {
        case null: {
            continue;
            break;
        }
        case @string xΔ1: {
            switch (v.type()) {
            case null: {
                ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
                Ꮡsb.WriteString(xΔ1);
                v = Ꮡsb;
                continue;
                break;
            }
            case ж<strings.Builder> w: {
                w.WriteString(xΔ1);
                continue;
                break;
            }}
            break;
        }
        case slice<any> xΔ1: {
            switch (v.type()) {
            case null: {
                var s = new slice<any>(len(xΔ1));
                copy(s, xΔ1);
                v = s;
                continue;
                break;
            }
            case slice<any> w: {
                v = builtin.appendꓸꓸꓸ(w, xΔ1);
                continue;
                break;
            }}
            break;
        }
        case map<@string, any> xΔ1: {
            switch (v.type()) {
            case null: {
                v = maps.Clone<map<@string, any>, @string, any>(xΔ1);
                continue;
                break;
            }
            case map<@string, any> w: {
                maps.Copy<map<@string, any>, map<@string, any>, @string, any>(w, xΔ1);
                continue;
                break;
            }}
            break;
        }}
        {
            var (sb, ok) = v._<ж<strings.Builder>>(ᐧ); if (ok) {
                v = sb.String();
            }
        }
        v = funcOpAdd(default, v, x);
        if (v._<error>(out var err)) {
            return err;
        }
    }
    {
        var (sb, ok) = v._<ж<strings.Builder>>(ᐧ); if (ok) {
            v = sb.String();
        }
    }
    return v;
}

internal static any funcToBoolean(any v) {
    switch (v.type()) {
    case bool vΔ1: {
        return vΔ1;
    }
    case @string vΔ1ᴛ1: {
        ref var vΔ1 = ref heap(vΔ1ᴛ1, out var ᏑvΔ1);
        var exprᴛ1 = vΔ1;
        switch (ᐧ) {
        case {} when exprᴛ1 == "true"u8: {
            return true;
        }
        case {} when exprᴛ1 == "false"u8: {
            return false;
        }
        default: {
            return Ꮡ(new func0WrapError("toboolean", vΔ1, errors.New("invalid boolean")));
        }}

        break;
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func0TypeError("toboolean", vΔ1));
    }}
}

internal static any funcToNumber(any v) {
    switch (v.type()) {
    case nint _:
    case float64 _:
    case ж<bigꓸInt> _:
    case json.Number _: {
        var vΔ1 = v;
        return vΔ1;
    }
    case @string vΔ1ᴛ2: {
        ref var vΔ1 = ref heap(vΔ1ᴛ2, out var ᏑvΔ1);
        if (!newLexer(vΔ1).validNumber()) {
            return Ꮡ(new func0WrapError("tonumber", vΔ1, errors.New("invalid number")));
        }
        return toNumber(vΔ1);
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func0TypeError("tonumber", vΔ1));
    }}
}

internal static any toNumber(@string v) {
    return parseNumber((json.Number)v);
}

internal static any funcToString(any v) {
    var (s, ok) = v._<@string>(ᐧ); if (ok) {
        return s;
    }
    return funcToJSON(v);
}

internal static any funcType(any v) {
    return TypeOf(v);
}

internal static any funcReverse(any v) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("reverse", v));
    }
    var ws = new slice<any>(len(vs));
    foreach (var (i, vΔ1) in range(vs)) {
        ws[len(ws) - i - 1] = vΔ1;
    }
    return ws;
}

internal static any funcContains(any v, any x) {
    return binopTypeSwitch(v, x,
        any (nint l, nint r) => l == r,
        any (float64 l, float64 r) => l == r,
        any (ж<bigꓸInt> l, ж<bigꓸInt> r) => l.Cmp(r) == 0,
        any (@string l, @string r) => strings.Contains(l, r),
        any (slice<any> l, slice<any> r) => {
R:
            foreach (var (_, rΔ1) in range(r)) {
                foreach (var (_, lΔ1) in range(l)) {
                    if (AreEqual(funcContains(lΔ1, rΔ1), true)) {
                        goto continue_R;
                    }
                }
                return false;
continue_R:;
            }
break_R:;
            return true;
        },
        any (map<@string, any> l, map<@string, any> r) => {
            if (len(l) < len(r)) {
                return false;
            }
            foreach (var (k, rΔ1) in r) {
                var (lΔ1, ok) = l[k, ꟷ]; if (!ok || !AreEqual(funcContains(lΔ1, rΔ1), true)) {
                    return false;
                }
            }
            return true;
        },
        any (any l, any r) => {
            if (AreEqual(l, r)) {
                return true;
            }
            return Ꮡ(new func1TypeError("contains", l, r));
        });
}

internal static any funcInside(any v, any x) {
    return funcContains(x, v);
}

internal static any funcIndices(any v, any x) {
    return indexFunc("indices", v, x, indices);
}

internal static any indices(slice<any> vs, slice<any> xs) {
    var rs = slice<any>([]);
    if (len(xs) == 0) {
        return rs;
    }
    foreach (var i in range(len(vs) - len(xs) + 1)) {
        if (Compare(vs.slice(i, i + len(xs)), xs) == 0) {
            rs = builtin.append(rs, (any)(i));
        }
    }
    return rs;
}

internal static any funcIndex(any v, any x) {
    return indexFunc("index", v, x, (slice<any> vs, slice<any> xs) => {
        if (len(xs) == 0) {
            return default;
        }
        foreach (var i in range(len(vs) - len(xs) + 1)) {
            if (Compare(vs.slice(i, i + len(xs)), xs) == 0) {
                return i;
            }
        }
        return default;
    });
}

internal static any funcRindex(any v, any x) {
    return indexFunc("rindex", v, x, (slice<any> vs, slice<any> xs) => {
        if (len(xs) == 0) {
            return default;
        }
        for (nint i = len(vs) - len(xs); i >= 0; i--) {
            if (Compare(vs.slice(i, i + len(xs)), xs) == 0) {
                return i;
            }
        }
        return default;
    });
}

internal static any indexFunc(@string name, any v, any x, Func<slice<any>, slice<any>, any> f) {
    switch (v.type()) {
    case null: {
        return default;
    }
    case slice<any> vΔ1: {
        switch (x.type()) {
        case slice<any> xΔ1: {
            return f(vΔ1, xΔ1);
        }
        default: {
            var xΔ1 = x;
            return f(vΔ1, [xΔ1]);
        }}
        break;
    }
    case @string vΔ1ᴛ3: {
        ref var vΔ1 = ref heap(vΔ1ᴛ3, out var ᏑvΔ1);
        var (xΔ2, ok) = x._<@string>(ᐧ); if (ok) {
            return f(explode(vΔ1), explode(xΔ2));
        }
        return Ꮡ(new func1TypeError(name, vΔ1, x));
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new func1TypeError(name, vΔ1, x));
    }}
}

internal static any funcStartsWith(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("startswith", v, x));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("startswith", v, x));
    }
    return strings.HasPrefix(s, t);
}

internal static any funcEndsWith(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("endswith", v, x));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("endswith", v, x));
    }
    return strings.HasSuffix(s, t);
}

internal static any funcLtrimstr(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("ltrimstr", v, x));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("ltrimstr", v, x));
    }
    return strings.TrimPrefix(s, t);
}

internal static any funcRtrimstr(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("rtrimstr", v, x));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("rtrimstr", v, x));
    }
    return strings.TrimSuffix(s, t);
}

internal static any funcTrimstr(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("trimstr", v, x));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("trimstr", v, x));
    }
    return strings.TrimSuffix(strings.TrimPrefix(s, t), t);
}

internal static any funcLtrim(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("ltrim", v));
    }
    return strings.TrimLeftFunc(s, unicode.IsSpace);
}

internal static any funcRtrim(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("rtrim", v));
    }
    return strings.TrimRightFunc(s, unicode.IsSpace);
}

internal static any funcTrim(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("trim", v));
    }
    return strings.TrimSpace(s);
}

internal static any funcExplode(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("explode", v));
    }
    return explode(s);
}

internal static slice<any> explode(@string s) {
    var xs = new slice<any>(len(slice<rune>(s)));
    nint i = default;
    foreach (var (_, r) in s) {
        xs[i] = (nint)r;
        i++;
    }
    return xs;
}

internal static any funcImplode(any v) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("implode", v));
    }
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    Ꮡsb.Grow(len(vs));
    foreach (var (_, vΔ1) in range(vs)) {
        var (r, okΔ1) = toInt(vΔ1); if (okΔ1) {
            if (0 <= r && r <= utf8.MaxRune) {
                Ꮡsb.WriteRune((rune)r);
            } else {
                Ꮡsb.WriteRune(utf8.RuneError);
            }
        } else {
            return Ꮡ(new func0TypeError("implode", vs));
        }
    }
    return sb.String();
}

internal static any funcSplit(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("split", v));
    }
    (var t, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("split", x));
    }
    var ss = strings.Split(s, t);
    var xs = new slice<any>(len(ss));
    foreach (var (i, sΔ1) in range(ss)) {
        xs[i] = sΔ1;
    }
    return xs;
}

internal static any funcJoin(any v, any x) {
    var (vs, ok) = values(v);
    if (!ok) {
        return Ꮡ(new func1TypeError("join", v, x));
    }
    if (len(vs) == 0) {
        return (@string)""u8;
    }
    var vsʗ1 = vs;
    return add((Func<any, bool> yield) => {
        foreach (var (i, vᴛ1) in range(vsʗ1)) {
            var vΔ1 = vᴛ1;

            var s = x;
            if (i == 0) {
                s = (@string)""u8;
            }
            if (!yield(s)) {
                return;
            }
            switch (vΔ1.type()) {
            case bool _:
            case nint _:
            case float64 _:
            case ж<bigꓸInt> _:
            case json.Number _: {
                var w = vΔ1;
                vΔ1 = jsonMarshal(w);
                break;
            }}
            if (!yield(vΔ1)) {
                return;
            }
        }
    });
}

internal static any funcASCIIDowncase(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("ascii_downcase", v));
    }
    return strings.Map((rune r) => {
        if ('A' <= r && r <= 'Z') {
            return r + ((rune)'a' - (rune)'A');
        }
        return r;
    }, s);
}

internal static any funcASCIIUpcase(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("ascii_upcase", v));
    }
    return strings.Map((rune r) => {
        if ('a' <= r && r <= 'z') {
            return r - ((rune)'a' - (rune)'A');
        }
        return r;
    }, s);
}

internal static any funcToJSON(any v) {
    return jsonMarshal(v);
}

internal static any funcFromJSON(any v) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("fromjson", v));
    }
    ref var w = ref heap<any>(out var Ꮡw);
    var dec = json.NewDecoder(new strings_ReaderжReader(strings.NewReader(s)));
    dec.UseNumber();
    {
        var err = dec.Decode(Ꮡw); if (err != nil) {
            return Ꮡ(new func0WrapError("fromjson", v, err));
        }
    }
    {
        var (_, err) = dec.Token(); if (!AreEqual(err, io.EOF)) {
            return Ꮡ(new func0TypeError("fromjson", v));
        }
    }
    return w;
}

internal static any funcFormat(any v, any x) {
    var (s, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("format", x));
    }
    ref var format = ref heap<@string>("@"u8 + s, out var Ꮡformat);
    var f = formatToFunc(format);
    if (f == nil) {
        return Ꮡ(new formatNotFoundError(format));
    }
    return internalFuncs[f.Name].callback(v, default);
}

internal static ж<strings.Replacer> htmlEscaper = strings.NewReplacer(
    @"<", "&lt;",
    @">", "&gt;",
    @"&", "&amp;",
    @"'", "&apos;",
    @"""", "&quot;");

internal static any funcToHTML(any v) {
    var switchᴛ2 = funcToString(v);
    switch (switchᴛ2.type()) {
    case @string x: {
        return htmlEscaper.Replace(x);
    }
    default: {
        var x = switchᴛ2;
        return x;
    }}
}

internal static any funcToURI(any v) {
    var switchᴛ3 = funcToString(v);
    switch (switchᴛ3.type()) {
    case @string x: {
        return strings.ReplaceAll(url.QueryEscape(x), "+", "%20");
    }
    default: {
        var x = switchᴛ3;
        return x;
    }}
}

internal static any funcToURId(any v) {
    var switchᴛ4 = funcToString(v);
    switch (switchᴛ4.type()) {
    case @string x: {
        (x, var err) = url.QueryUnescape(strings.ReplaceAll(x, "+", "%2B"));
        if (err != nil) {
            return Ꮡ(new func0WrapError("@urid", v, err));
        }
        return x;
    }
    default: {
        var x = switchᴛ4;
        return x;
    }}
}

internal static ж<strings.Replacer> csvEscaper = strings.NewReplacer(
    @"""", @"""""",
    "\x00", @"\0");

internal static any funcToCSV(any v) {
    return formatJoin("csv", v, ",", (@string s) => @""""u8 + csvEscaper.Replace(s) + @""""u8);
}

internal static ж<strings.Replacer> tsvEscaper = strings.NewReplacer(
    "\t", @"\t",
    "\r", @"\r",
    "\n", @"\n",
    "\\", @"\\",
    "\x00", @"\0");

internal static any funcToTSV(any v) {
    return formatJoin("tsv", v, "\t", tsvEscaper.Replace);
}

internal static ж<strings.Replacer> shEscaper = strings.NewReplacer(
    "'", @"'\''",
    "\x00", @"\0");

internal static any funcToSh(any v) {
    var (_, ok) = v._<slice<any>>(ᐧ); if (!ok) {
        v = slice<any>([v]);
    }
    return formatJoin("sh", v, " ", (@string s) => "'"u8 + shEscaper.Replace(s) + "'"u8);
}

internal static any formatJoin(@string typ, any v, @string sep, Func<@string, @string> escape) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("@"u8 + typ, v));
    }
    var ss = new slice<@string>(len(vs));
    foreach (var (i, vΔ1) in range(vs)) {
        switch (vΔ1.type()) {
        case slice<any> _:
        case map<@string, any> _: {
            var vΔ2 = vΔ1;
            return Ꮡ(new formatRowError(typ, vΔ2));
        }
        case @string vΔ2: {
            ss[i] = escape(vΔ2);
            break;
        }
        default: {
            var vΔ2 = vΔ1;
            @string s = jsonMarshal(vΔ2); if (s != "null"u8 || typ == "sh"u8) {
                ss[i] = s;
            }
            break;
        }}
    }
    return strings.Join(ss, sep);
}

internal static any funcToBase64(any v) {
    var switchᴛ5 = funcToString(v);
    switch (switchᴛ5.type()) {
    case @string x: {
        return base64.StdEncoding.EncodeToString(slice<byte>(x));
    }
    default: {
        var x = switchᴛ5;
        return x;
    }}
}

internal static any funcToBase64d(any v) {
    var switchᴛ6 = funcToString(v);
    switch (switchᴛ6.type()) {
    case @string x: {
        nint i = strings.IndexRune(x, base64.StdPadding); if (i >= 0) {
            x = x.slice(0, i);
        }
        var (y, err) = base64.RawStdEncoding.DecodeString(x);
        if (err != nil) {
            return Ꮡ(new func0WrapError("@base64d", v, err));
        }
        return (@string)y;
    }
    default: {
        var x = switchᴛ6;
        return x;
    }}
}

internal static any funcIndex2(any _Δp0, any v, any x) {
    switch (x.type()) {
    case @string xΔ1: {
        switch (v.type()) {
        case null: {
            return default;
        }
        case map<@string, any> vΔ1: {
            return vΔ1[xΔ1];
        }
        default: {
            var vΔ1 = v;
            return Ꮡ(new expectedObjectError(vΔ1));
        }}
        break;
    }
    case nint _:
    case float64 _:
    case ж<bigꓸInt> _:
    case json.Number _: {
        var xΔ1 = x;
        var (i, _) = toInt(xΔ1);
        switch (v.type()) {
        case null: {
            return default;
        }
        case slice<any> vΔ2: {
            return index(vΔ2, i);
        }
        case @string vΔ2: {
            return indexString(vΔ2, i);
        }
        default: {
            var vΔ2 = v;
            return Ꮡ(new expectedArrayError(vΔ2));
        }}
        break;
    }
    case slice<any> xΔ1: {
        switch (v.type()) {
        case null: {
            return default;
        }
        case slice<any> vΔ3: {
            return indices(vΔ3, xΔ1);
        }
        default: {
            var vΔ3 = v;
            return Ꮡ(new expectedArrayError(vΔ3));
        }}
        break;
    }
    case map<@string, any> xΔ1: {
        if (v == nil) {
            return default;
        }
        var (start, ok) = xΔ1["start", ꟷ];
        if (!ok) {
            return Ꮡ(new expectedStartEndError(xΔ1));
        }
        (var end, ok) = xΔ1["end", ꟷ];
        if (!ok) {
            return Ꮡ(new expectedStartEndError(xΔ1));
        }
        return funcSlice(default, v, end, start);
    }
    default: {
        var xΔ1 = x;
        switch (v.type()) {
        case slice<any>: {
            return Ꮡ(new arrayIndexNotNumberError(xΔ1));
        }
        case @string: {
            return Ꮡ(new stringIndexNotNumberError(xΔ1));
        }
        default: {
            return Ꮡ(new objectKeyNotStringError(xΔ1));
        }}

        break;
    }}
}

internal static any index(slice<any> vs, nint i) {
    i = clampIndex(i, -1, len(vs));
    if (0 <= i && i < len(vs)) {
        return vs[i];
    }
    return default;
}

internal static any indexString(@string s, nint i) {
    nint l = len(slice<rune>(s));
    i = clampIndex(i, -1, l);
    if (0 <= i && i < l) {
        foreach (var (_, r) in s) {
            i--; if (i < 0) {
                return (@string)r;
            }
        }
    }
    return default;
}

internal static any /*r*/ funcSlice(any _, any v, any e, any s) {
    switch (v.type()) {
    case null: {
        return default;
    }
    case slice<any> vΔ1: {
        return Δslice(vΔ1, e, s);
    }
    case @string vΔ1: {
        return sliceString(vΔ1, e, s);
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new expectedArrayError(vΔ1));
    }}
}

internal static any Δslice(slice<any> vs, any e, any s) {
    nint start = default;
    nint end = default;
    if (s != nil) {
        var (i, ok) = toInt(s); if (ok) {
            start = clampIndex(i, 0, len(vs));
        } else {
            return Ꮡ(new arrayIndexNotNumberError(s));
        }
    }
    if (e != nil) {
        var (i, ok) = toIntCeil(e); if (ok) {
            end = clampIndex(i, start, len(vs));
        } else {
            return Ꮡ(new arrayIndexNotNumberError(e));
        }
    } else {
        end = len(vs);
    }
    return vs.slice(start, end);
}

internal static any sliceString(@string v, any e, any s) {
    nint start = default;
    nint end = default;
    nint l = len(slice<rune>(v));
    if (s != nil) {
        var (i, ok) = toInt(s); if (ok) {
            start = clampIndex(i, 0, l);
        } else {
            return Ꮡ(new stringIndexNotNumberError(s));
        }
    }
    if (e != nil) {
        var (i, ok) = toIntCeil(e); if (ok) {
            end = clampIndex(i, start, l);
        } else {
            return Ꮡ(new stringIndexNotNumberError(e));
        }
    } else {
        end = l;
    }
    if (start < l) {
        foreach (var (i, _) in v) {
            start--; if (start < 0) {
                start = i;
                break;
            }
        }
    } else {
        start = len(v);
    }
    if (end < l) {
        foreach (var (i, _) in v) {
            end--; if (end < 0) {
                end = i;
                break;
            }
        }
    } else {
        end = len(v);
    }
    return v.slice(start, end);
}

internal static nint clampIndex(nint i, nint minimum, nint maximum) {
    if (i < 0) {
        i += maximum;
    }
    if (i < minimum) {
        return minimum;
    } else if (i < maximum) {
        return i;
    } else {
        return maximum;
    }
}

internal static any funcFlatten(any v, slice<any> args) {
    var (vs, ok) = values(v);
    if (!ok) {
        return Ꮡ(new func0TypeError("flatten", v));
    }
    ref var depth = ref heap(new float64(), out var Ꮡdepth);
    if (len(args) == 0) {
        depth = -1D;
    } else {
        (depth, ok) = toFloat(args[0]);
        if (!ok) {
            return Ꮡ(new func0TypeError("flatten", args[0]));
        }
        if (lt(depth, 0D)) {
            return Ꮡ(new flattenDepthError(depth));
        }
    }
    return flatten([], vs, depth);
}

internal static slice<any> flatten(slice<any> xs, slice<any> vs, float64 depth) {
    foreach (var (_, v) in range(vs)) {
        var (vsΔ1, ok) = v._<slice<any>>(ᐧ); if (ok && depth != 0D) {
            xs = flatten(xs, vsΔ1, depth - 1D);
        } else {
            xs = builtin.append(xs, v);
        }
    }
    return xs;
}

partial struct rangeIter {
    internal any value, end, step;
}

internal static (any, bool) Next(this ref rangeIter iter) {
    if (Compare(iter.step, (nint)0) * Compare(iter.value, iter.end) >= 0) {
        return (default, false);
    }
    var v = iter.value;
    iter.value = funcOpAdd(default, v, iter.step);
    return (v, true);
}

internal static any funcRange(any _, slice<any> xs) {
    foreach (var (_, x) in range(xs)) {
        switch (x.type()) {
        case nint _:
        case float64 _:
        case ж<bigꓸInt> _:
        case json.Number _: {
            break;
        }
        default: {
            return Ꮡ(new func0TypeError("range", x));
        }}

    }
    return Ꮡ(new rangeIter(xs[0], xs[1], xs[2]));
}

internal static any funcMin(any v) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("min", v));
    }
    return minMaxBy(vs, vs, true);
}

internal static any funcMinBy(any v, any x) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("min_by", v, x));
    }
    (var xs, ok) = x._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("min_by", v, x));
    }
    if (len(vs) != len(xs)) {
        return Ꮡ(new func1WrapError("min_by", v, x, new lengthMismatchErrorжerror(Ꮡ(new lengthMismatchError()))));
    }
    return minMaxBy(vs, xs, true);
}

internal static any funcMax(any v) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("max", v));
    }
    return minMaxBy(vs, vs, false);
}

internal static any funcMaxBy(any v, any x) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("max_by", v, x));
    }
    (var xs, ok) = x._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("max_by", v, x));
    }
    if (len(vs) != len(xs)) {
        return Ꮡ(new func1WrapError("max_by", v, x, new lengthMismatchErrorжerror(Ꮡ(new lengthMismatchError()))));
    }
    return minMaxBy(vs, xs, false);
}

internal static any minMaxBy(slice<any> vs, slice<any> xs, bool isMin) {
    if (len(vs) == 0) {
        return default;
    }
    nint i = 0;
    nint j = 0;
    var x = xs[0];
    for (i++; i < len(xs); i++) {
        if (Compare(x, xs[i]) > 0 == isMin) {
            (j, x) = (i, xs[i]);
        }
    }
    return vs[j];
}

partial struct sortItem {
    internal any value, key;
}

internal static (slice<ж<sortItem>>, error) sortItems(@string name, any v, any x) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        if (strings.HasSuffix(name, "_by")) {
            return (default, new func1TypeErrorжerror(Ꮡ(new func1TypeError(name, v, x))));
        }
        return (default, new func0TypeErrorжerror(Ꮡ(new func0TypeError(name, v))));
    }
    (var xs, ok) = x._<slice<any>>(ᐧ);
    if (!ok) {
        return (default, new func1TypeErrorжerror(Ꮡ(new func1TypeError(name, v, x))));
    }
    if (len(vs) != len(xs)) {
        return (default, new func1WrapErrorжerror(Ꮡ(new func1WrapError(name, v, x, new lengthMismatchErrorжerror(Ꮡ(new lengthMismatchError()))))));
    }
    var items = new slice<ж<sortItem>>(len(vs));
    foreach (var (i, vΔ1) in range(vs)) {
        items[i] = Ꮡ(new sortItem(vΔ1, xs[i]));
    }
    var itemsʗ1 = items;
    sort.SliceStable(items, (nint i, nint j) => Compare(itemsʗ1[i].key, itemsʗ1[j].key) < 0);
    return (items, default);
}

internal static any funcSort(any v) {
    return sortBy("sort", v, v);
}

internal static any funcSortBy(any v, any x) {
    return sortBy("sort_by", v, x);
}

internal static any sortBy(@string name, any v, any x) {
    var (items, err) = sortItems(name, v, x);
    if (err != nil) {
        return err;
    }
    var rs = new slice<any>(len(items));
    foreach (var (i, xΔ1) in range(items)) {
        rs[i] = xΔ1.value;
    }
    return rs;
}

internal static any funcGroupBy(any v, any x) {
    var (items, err) = sortItems("group_by", v, x);
    if (err != nil) {
        return err;
    }
    var rs = slice<any>([]);
    any last = default;
    foreach (var (i, r) in range(items)) {
        if (i == 0 || Compare(last, r.key) != 0) {
            (rs, last) = (builtin.append(rs, (any)(slice<any>([r.value]))), r.key);
        } else {
            rs[len(rs) - 1] = builtin.append(rs[len(rs) - 1]._<slice<any>>(), r.value);
        }
    }
    return rs;
}

internal static any funcUnique(any v) {
    return uniqueBy("unique", v, v);
}

internal static any funcUniqueBy(any v, any x) {
    return uniqueBy("unique_by", v, x);
}

internal static any uniqueBy(@string name, any v, any x) {
    var (items, err) = sortItems(name, v, x);
    if (err != nil) {
        return err;
    }
    var rs = slice<any>([]);
    any last = default;
    foreach (var (i, r) in range(items)) {
        if (i == 0 || Compare(last, r.key) != 0) {
            (rs, last) = (builtin.append(rs, r.value), r.key);
        }
    }
    return rs;
}

internal static float64 funcSignificand(float64 v) {
    var (frac, _) = math.Frexp(v);
    return frac * 2D;
}

internal static float64 funcExp10(float64 v) {
    return math.Pow(10D, v);
}

internal static any funcFrexp(any v) {
    var (x, ok) = toFloat(v);
    if (!ok) {
        return Ꮡ(new func0TypeError("frexp", v));
    }
    var (f, e) = math.Frexp(x);
    return slice<any>([f, e]);
}

internal static any funcModf(any v) {
    var (x, ok) = toFloat(v);
    if (!ok) {
        return Ꮡ(new func0TypeError("modf", v));
    }
    if (math.IsInf(x, 0)) {
        return slice<any>([math.Copysign(0D, x), x]);
    }
    var (i, f) = math.Modf(x);
    return slice<any>([f, i]);
}

internal static float64 funcLgamma(float64 v) {
    (v, _) = math.Lgamma(v);
    return v;
}

internal static float64 funcDrem(float64 l, float64 r) {
    var x = math.Remainder(l, r);
    if (x == 0.0D) {
        return math.Copysign(x, l);
    }
    return x;
}

internal static float64 funcFmax(float64 l, float64 r) {
    if (math.IsNaN(l)) {
        return r;
    }
    if (math.IsNaN(r)) {
        return l;
    }
    return max(l, r);
}

internal static float64 funcFmin(float64 l, float64 r) {
    if (math.IsNaN(l)) {
        return r;
    }
    if (math.IsNaN(r)) {
        return l;
    }
    return min(l, r);
}

internal static float64 funcJn(float64 l, float64 r) {
    return math.Jn((nint)l, r);
}

internal static float64 funcLdexp(float64 l, float64 r) {
    return math.Ldexp(l, (nint)r);
}

internal static float64 funcYn(float64 l, float64 r) {
    return math.Yn((nint)l, r);
}

internal static any funcInfinite(any _) {
    return math.Inf(1);
}

internal static any funcIsfinite(any v) {
    var (x, ok) = toFloat(v);
    return ok && !math.IsInf(x, 0);
}

internal static any funcIsinfinite(any v) {
    var (x, ok) = toFloat(v);
    return ok && math.IsInf(x, 0);
}

internal static any funcNan(any _) {
    return math.NaN();
}

internal static any funcIsnan(any v) {
    var (x, ok) = toFloat(v);
    if (!ok) {
        if (v == nil) {
            return false;
        }
        return Ꮡ(new func0TypeError("isnan", v));
    }
    return math.IsNaN(x);
}

internal static any funcIsnormal(any v) {
    var (vΔ1, ok) = toFloat(v); if (ok) {
        var e = (math.Float64bits(vΔ1) & 0x7ff0000000000000UL) >> 52;
        return 0 < e && e < 0x7ff;
    }
    return false;
}

partial struct allocator /*map[uintptr, EmptyStruct]*/;

internal static any funcAllocator(any _Δp0, slice<any> _Δp1) {
    return new allocator(new map<uintptr, EmptyStruct>{});
}

internal static bool allocated(this allocator a, any v) {
    var (_, ok) = a[reflect.ValueOf(v).Pointer(), ꟷ];
    return ok;
}

internal static map<@string, any> makeObject(this allocator a, nint l) {
    var v = new map<@string, any>(l);
    if (a != nil) {
        a[reflect.ValueOf(v).Pointer()] = new EmptyStruct();
    }
    return v;
}

internal static slice<any> makeArray(this allocator a, nint l, nint c) {
    var v = new slice<any>(l, max(l, c));
    if (a != nil) {
        a[reflect.ValueOf(v).Pointer()] = new EmptyStruct();
    }
    return v;
}

internal static any funcSetpath(any v, any p, any n) {
    return setpath(v, p, n, default);
}

internal static any funcSetpathWithAllocator(any v, slice<any> args) {
    return setpath(v, args[0], args[1], args[2]._<allocator>());
}

internal static any setpath(any v, any p, any n, allocator a) {
    var (path, ok) = p._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("setpath", v, p));
    }
    var (u, err) = update(v, path, n, a);
    if (err != nil) {
        return Ꮡ(new func2WrapError("setpath", v, p, n, err));
    }
    return u;
}

internal static any funcDelpaths(any v, any p) {
    return delpaths(v, p, new allocator(new map<uintptr, EmptyStruct>{}));
}

internal static any funcDelpathsWithAllocator(any v, slice<any> args) {
    return delpaths(v, args[0], args[1]._<allocator>());
}

internal static any delpaths(any v, any p, allocator a) {
    var (paths, ok) = p._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("delpaths", v, p));
    }
    if (len(paths) == 0) {
        return v;
    }
    EmptyStruct empty = default;
    error err = default;
    var u = v;
    foreach (var (_, q) in range(paths)) {
        var (path, okΔ1) = q._<slice<any>>(ᐧ);
        if (!okΔ1) {
            return Ꮡ(new func1WrapError("delpaths", v, p, new expectedArrayErrorжerror(Ꮡ(new expectedArrayError(q)))));
        }
        (u, err) = update(u, path, empty, a);
        if (err != nil) {
            return Ꮡ(new func1WrapError("delpaths", v, p, err));
        }
    }
    return deleteEmpty(u);
}

internal static (any, error) update(any v, slice<any> path, any n, allocator a) {
    if (len(path) == 0) {
        return (n, default);
    }
    switch (path[0].type()) {
    case @string p: {
        switch (v.type()) {
        case null: {
            return updateObject(default, p, path[1..], n, a);
        }
        case map<@string, any> vΔ1: {
            return updateObject(vΔ1, p, path[1..], n, a);
        }
        case EmptyStruct vΔ1: {
            return (vΔ1, default);
        }
        default: {
            var vΔ1 = v;
            return (default, new expectedObjectErrorжerror(Ꮡ(new expectedObjectError(vΔ1))));
        }}
        break;
    }
    case nint _:
    case float64 _:
    case ж<bigꓸInt> _:
    case json.Number _: {
        var p = path[0];
        var (i, _) = toInt(p);
        switch (v.type()) {
        case null: {
            return updateArrayIndex(default, i, path[1..], n, a);
        }
        case slice<any> vΔ2: {
            return updateArrayIndex(vΔ2, i, path[1..], n, a);
        }
        case EmptyStruct vΔ2: {
            return (vΔ2, default);
        }
        default: {
            var vΔ2 = v;
            return (default, new expectedArrayErrorжerror(Ꮡ(new expectedArrayError(vΔ2))));
        }}
        break;
    }
    case map<@string, any> p: {
        switch (v.type()) {
        case null: {
            return updateArraySlice(default, p, path[1..], n, a);
        }
        case slice<any> vΔ3: {
            return updateArraySlice(vΔ3, p, path[1..], n, a);
        }
        case EmptyStruct vΔ3: {
            return (vΔ3, default);
        }
        default: {
            var vΔ3 = v;
            return (default, new expectedArrayErrorжerror(Ꮡ(new expectedArrayError(vΔ3))));
        }}
        break;
    }
    default: {
        var p = path[0];
        switch (v.type()) {
        case slice<any>: {
            return (default, new arrayIndexNotNumberErrorжerror(Ꮡ(new arrayIndexNotNumberError(p))));
        }
        default: {
            return (default, new objectKeyNotStringErrorжerror(Ꮡ(new objectKeyNotStringError(p))));
        }}

        break;
    }}
}

internal static (any, error) updateObject(map<@string, any> v, @string k, slice<any> path, any n, allocator a) {
    var (x, ok) = v[k, ꟷ];
    if (!ok && AreEqual(n, new EmptyStruct())) {
        if (v == nil) {
            return (default, default);
        }
        return (v, default);
    }
    var (u, err) = update(x, path, n, a);
    if (err != nil) {
        return (default, err);
    }
    if (a.allocated(v)) {
        v[k] = u;
        return (v, default);
    }
    var w = a.makeObject(len(v) + 1);
    maps.Copy<map<@string, any>, map<@string, any>, @string, any>(w, v);
    w[k] = u;
    return (w, default);
}

internal static (any, error) updateArrayIndex(slice<any> v, nint i, slice<any> path, any n, allocator a) {
    any x = default;
    nint j = clampIndex(i, -1, len(v)); if (j < 0) {
        if (AreEqual(n, new EmptyStruct())) {
            if (v == nil) {
                return (default, default);
            }
            return (v, default);
        }
        return (default, new arrayIndexNegativeErrorжerror(Ꮡ(new arrayIndexNegativeError(i))));
    } else if (j < len(v)) {
        i = j;
        x = v[i];
    } else {
        if (AreEqual(n, new EmptyStruct())) {
            if (v == nil) {
                return (default, default);
            }
            return (v, default);
        }
        if (i >= 0x20000000) {
            return (default, new arrayIndexTooLargeErrorжerror(Ꮡ(new arrayIndexTooLargeError(i))));
        }
    }
    var (u, err) = update(x, path, n, a);
    if (err != nil) {
        return (default, err);
    }
    nint l = len(v);
    nint c = cap(v);
    if (a.allocated(v)) {
        if (i < c) {
            if (i >= l) {
                v = v.slice(0, i + 1);
            }
            v[i] = u;
            return (v, default);
        }
        c *= 2;
    }
    if (i >= l) {
        l = i + 1;
    }
    var w = a.makeArray(l, c);
    copy(w, v);
    w[i] = u;
    return (w, default);
}

internal static (any, error) updateArraySlice(slice<any> v, map<@string, any> m, slice<any> path, any n, allocator a) {
    var (s, ok) = m["start", ꟷ];
    if (!ok) {
        return (default, new expectedStartEndErrorжerror(Ꮡ(new expectedStartEndError(m))));
    }
    (var e, ok) = m["end", ꟷ];
    if (!ok) {
        return (default, new expectedStartEndErrorжerror(Ꮡ(new expectedStartEndError(m))));
    }
    nint start = default;
    nint end = default;
    if (s != nil) {
        var (i, okΔ1) = toInt(s); if (okΔ1) {
            start = clampIndex(i, 0, len(v));
        } else {
            return (default, new arrayIndexNotNumberErrorжerror(Ꮡ(new arrayIndexNotNumberError(s))));
        }
    }
    if (e != nil) {
        var (i, okΔ2) = toIntCeil(e); if (okΔ2) {
            end = clampIndex(i, start, len(v));
        } else {
            return (default, new arrayIndexNotNumberErrorжerror(Ꮡ(new arrayIndexNotNumberError(e))));
        }
    } else {
        end = len(v);
    }
    if (start == end && AreEqual(n, new EmptyStruct())) {
        if (v == nil) {
            return (default, default);
        }
        return (v, default);
    }
    var (u, err) = update(v.slice(start, end), path, n, a);
    if (err != nil) {
        return (default, err);
    }
    switch (u.type()) {
    case slice<any> uΔ1: {
        slice<any> w = default;
        if (len(uΔ1) == end - start && a.allocated(v)) {
            w = v;
        } else {
            w = a.makeArray(len(v) - (end - start) + len(uΔ1), 0);
            copy(w, v.slice(0, start));
            copy(w.slice(start + len(uΔ1)), v.slice(end));
        }
        copy(w.slice(start), uΔ1);
        return (w, default);
    }
    case EmptyStruct uΔ1: {
        slice<any> w = default;
        if (a.allocated(v)) {
            w = v;
        } else {
            w = a.makeArray(len(v), 0);
            copy(w, v);
        }
        for (nint i = start; i < end; i++) {
            w[i] = uΔ1;
        }
        return (w, default);
    }
    default: {
        var uΔ1 = u;
        return (default, new expectedArrayErrorжerror(Ꮡ(new expectedArrayError(uΔ1))));
    }}
}

internal static any deleteEmpty(any v) {
    switch (v.type()) {
    case EmptyStruct vΔ1: {
        return default;
    }
    case map<@string, any> vΔ1: {
        foreach (var (k, w) in vΔ1) {
            if (AreEqual(w, new EmptyStruct())) {
                delete(vΔ1, k);
            } else {
                vΔ1[k] = deleteEmpty(w);
            }
        }
        return vΔ1;
    }
    case slice<any> vΔ1: {
        nint j = default;
        foreach (var (_, w) in range(vΔ1)) {
            if (!AreEqual(w, new EmptyStruct())) {
                vΔ1[j] = deleteEmpty(w);
                j++;
            }
        }
        for (nint i = j; i < len(vΔ1); i++) {
            vΔ1[i] = default;
        }
        return vΔ1.slice(0, j);
    }
    default: {
        var vΔ1 = v;
        return vΔ1;
    }}
}

internal static any funcGetpath(any v, any p) {
    var (path, ok) = p._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("getpath", v, p));
    }
    var u = v;
    foreach (var (_, x) in range(path)) {
        switch (v.type()) {
        case null:
        case slice<any> _:
        case map<@string, any> _: {
            v = funcIndex2(default, v, x);
            var (err, okΔ1) = v._<error>(ᐧ); if (okΔ1) {
                return Ꮡ(new func1WrapError("getpath", u, p, err));
            }
            break;
        }
        default: {
            return Ꮡ(new func1TypeError("getpath", u, p));
        }}

    }
    return v;
}

internal static any funcTranspose(any v) {
    var (vss, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("transpose", v));
    }
    if (len(vss) == 0) {
        return slice<any>([]);
    }
    nint l = default;
    foreach (var (_, vs) in range(vss)) {
        var (vsΔ1, okΔ1) = vs._<slice<any>>(ᐧ);
        if (!okΔ1) {
            return Ꮡ(new func0TypeError("transpose", v));
        }
        nint k = len(vsΔ1); if (l < k) {
            l = k;
        }
    }
    var wss = new slice<slice<any>>(l);
    var xs = new slice<any>(l);
    for ((nint i, nint k) = (0, len(vss)); i < l; i++) {
        var s = new slice<any>(k);
        wss[i] = s;
        xs[i] = s;
    }
    foreach (var (i, vs) in range(vss)) {
        foreach (var (j, vΔ1) in range(vs._<slice<any>>())) {
            wss[j][i] = vΔ1;
        }
    }
    return xs;
}

internal static any funcBsearch(any v, any t) {
    var (vs, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("bsearch", v, t));
    }
    var vsʗ1 = vs;
    nint i = sort.Search(len(vs), (nint iΔ1) => Compare(vsʗ1[iΔ1], t) >= 0);
    if (i < len(vs) && Compare(vs[i], t) == 0) {
        return i;
    }
    return -i - 1;
}

internal static any funcGmtime(any v) {
    var (vΔ1, ok) = toFloat(v); if (ok) {
        return epochToArray(vΔ1, time.ΔUTC);
    }
    return Ꮡ(new func0TypeError("gmtime", v));
}

internal static any funcLocaltime(any v) {
    var (vΔ1, ok) = toFloat(v); if (ok) {
        return epochToArray(vΔ1, time.ΔLocal);
    }
    return Ꮡ(new func0TypeError("localtime", v));
}

internal static slice<any> epochToArray(float64 v, ж<timeꓸLocation> loc) {
    var t = time.Unix((int64)v, (int64)((v - math.Floor(v)) * 1e9D)).In(loc);
    return slice<any>([
        t.Year(),
        (nint)t.Month() - 1,
        t.Day(),
        t.Hour(),
        t.Minute(),
        (float64)t.Second() + (float64)t.Nanosecond() / 1e9D,
        (nint)t.Weekday(),
        t.YearDay() - 1
    ]);
}

internal static any funcMktime(any v) {
    var (a, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func0TypeError("mktime", v));
    }
    var (t, err) = arrayToTime(a, time.ΔUTC);
    if (err != nil) {
        return Ꮡ(new func0WrapError("mktime", v, err));
    }
    return timeToEpoch(t);
}

internal static float64 timeToEpoch(time.Time t) {
    return (float64)t.Unix() + (float64)t.Nanosecond() / 1e9D;
}

internal static any funcStrftime(any v, any x) {
    var (w, okΔ1) = toFloat(v); if (okΔ1) {
        v = epochToArray(w, time.ΔUTC);
    }
    var (a, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strftime", v, x));
    }
    (var format, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strftime", v, x));
    }
    var (t, err) = arrayToTime(a, time.ΔUTC);
    if (err != nil) {
        return Ꮡ(new func1WrapError("strftime", v, x, err));
    }
    return timefmt.Format(t, format);
}

internal static any funcStrflocaltime(any v, any x) {
    var (w, okΔ1) = toFloat(v); if (okΔ1) {
        v = epochToArray(w, time.ΔLocal);
    }
    var (a, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strflocaltime", v, x));
    }
    (var format, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strflocaltime", v, x));
    }
    var (t, err) = arrayToTime(a, time.ΔLocal);
    if (err != nil) {
        return Ꮡ(new func1WrapError("strflocaltime", v, x, err));
    }
    return timefmt.Format(t, format);
}

internal static any funcStrptime(any v, any x) {
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strptime", v, x));
    }
    (var format, ok) = x._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func1TypeError("strptime", v, x));
    }
    var (t, err) = timefmt.Parse(s, format);
    if (err != nil) {
        return Ꮡ(new func1WrapError("strptime", v, x, err));
    }
    if (t.Equal(new time.Time())) {
        return Ꮡ(new func1TypeError("strptime", v, x));
    }
    return epochToArray(timeToEpoch(t), time.ΔUTC);
}

internal static (time.Time, error) arrayToTime(slice<any> a, ж<timeꓸLocation> loc) {
    time.Time t = default;
    ref var year = ref heap(new nint(), out var Ꮡyear);
    ref var month = ref heap(new nint(), out var Ꮡmonth);
    ref var day = ref heap(new nint(), out var Ꮡday);
    ref var hour = ref heap(new nint(), out var Ꮡhour);
    ref var minute = ref heap(new nint(), out var Ꮡminute);
    ref var second = ref heap(new nint(), out var Ꮡsecond);
    nint nanosecond = default;
    ref var weekday = ref heap(new nint(), out var Ꮡweekday);
    ref var yearday = ref heap(new nint(), out var Ꮡyearday);
    foreach (var (i, p) in range(slice<ж<nint>>([
        Ꮡyear, Ꮡmonth, Ꮡday, Ꮡhour, Ꮡminute,
        Ꮡsecond, Ꮡweekday, Ꮡyearday
    ]))) {
        if (i >= len(a)) {
            break;
        }
        if (i == 5) {
            var (v, ok) = toFloat(a[i]); if (ok) {
                p.Value = (nint)v;
                nanosecond = (nint)((v - math.Floor(v)) * 1e9D);
            } else {
                return (t, new timeArrayErrorжerror(Ꮡ(new timeArrayError())));
            }
        } else if (toInt(a[i]) is var (v, ok) && ok) {
            p.Value = v;
        } else {
            return (t, new timeArrayErrorжerror(Ꮡ(new timeArrayError())));
        }
    }
    return (time.Date(year, (timeꓸMonth)(month + 1), day,
        hour, minute, second, nanosecond, loc), default);
}

internal static any funcNow(any _) {
    return timeToEpoch(time.Now());
}

internal static any funcMatch(any v, any re, any fs, any testing, ж<sync.Map> cache) {
    ref var name = ref heap(new @string(), out var Ꮡname);
    if (AreEqual(testing, true)) {
        name = "test";
    } else {
        name = "match";
    }
    @string flags = default;
    if (fs != nil) {
        bool okΔ1 = default;
        (flags, okΔ1) = fs._<@string>(ᐧ);
        if (!okΔ1) {
            return Ꮡ(new func2TypeError(name, v, re, fs));
        }
    }
    var (s, ok) = v._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func2TypeError(name, v, re, fs));
    }
    (var restr, ok) = re._<@string>(ᐧ);
    if (!ok) {
        return Ꮡ(new func2TypeError(name, v, re, fs));
    }
    var (r, err) = compileRegexp(restr, flags, cache);
    if (err != nil) {
        return err;
    }
    if (AreEqual(testing, true)) {
        return r.MatchString(s);
    }
    nint n = default;
    if (strings.ContainsRune(flags, 'g')) {
        n = -1;
    } else {
        n = 1;
    }
    var xs = r.FindAllStringSubmatchIndex(s, n);
    var (res, names) = (new slice<any>(len(xs)), r.SubexpNames());
    foreach (var (i, x) in range(xs)) {
        var captures = new slice<any>((len(x) - 2) / 2);
        for (nint j = 1; j < len(x) / 2; j++) {
            any nameΔ1 = default;
            @string nΔ1 = names[j]; if (nΔ1 != ""u8) {
                nameΔ1 = nΔ1;
            }
            if (x[j * 2] < 0) {
                captures[j - 1] = new map<@string, any>{
                    ["name"] = nameΔ1,
                    ["offset"] = (nint)(-1),
                    ["length"] = (nint)0,
                    ["string"] = default
                };
                continue;
            }
            captures[j - 1] = new map<@string, any>{
                ["name"] = nameΔ1,
                ["offset"] = len(slice<rune>(s.slice(0, x[j * 2]))),
                ["length"] = len(slice<rune>(s.slice(0, x[j * 2 + 1]))) - len(slice<rune>(s.slice(0, x[j * 2]))),
                ["string"] = s.slice(x[j * 2], x[j * 2 + 1])
            };
        }
        res[i] = new map<@string, any>{
            ["offset"] = len(slice<rune>(s.slice(0, x[0]))),
            ["length"] = len(slice<rune>(s.slice(0, x[1]))) - len(slice<rune>(s.slice(0, x[0]))),
            ["string"] = s.slice(x[0], x[1]),
            ["captures"] = captures
        };
    }
    return res;
}

internal static (ж<regexp.Regexp>, error) compileRegexp(@string re, @string flags, ж<sync.Map> cache) {
    var key = new @string[]{re, flags}.array();
    var (rΔ1, ok) = cache.Load(key); if (ok) {
        return (rΔ1._<ж<regexp.Regexp>>(), default);
    }
    if (strings.IndexFunc(flags, (rune rΔ2) => rΔ2 != 'g' && rΔ2 != 'i' && rΔ2 != 'm') >= 0) {
        return (default, fmt.Errorf("unsupported regular expression flag: %q"u8, flags));
    }
    if (strings.ContainsRune(flags, 'i')) {
        re = "(?i)"u8 + re;
    }
    if (strings.ContainsRune(flags, 'm')) {
        re = "(?s)"u8 + re;
    }
    var (r, err) = regexp.Compile(re);
    if (err != nil) {
        return (default, fmt.Errorf("invalid regular expression %q: %s"u8, re, err));
    }
    cache.Store(key, r.OrTypedNil());
    return (r, default);
}

internal static any funcCaptures(any v) {
    var (captures, ok) = v._<slice<any>>(ᐧ);
    if (!ok) {
        return Ꮡ(new expectedArrayError(v));
    }
    var w = new map<@string, any>(len(captures));
    foreach (var (_, capture) in range(captures)) {
        var (captureΔ1, okΔ1) = capture._<map<@string, any>>(ᐧ); if (okΔ1) {
            var (name, okΔ2) = captureΔ1["name"]._<@string>(ᐧ); if (okΔ2) {
                w[name] = captureΔ1["string"];
            }
        }
    }
    return w;
}

internal static any funcError(any v, slice<any> args) {
    if (len(args) > 0) {
        v = args[0];
    }
    return Ꮡ(new exitCodeError(v, 5));
}

internal static any funcHalt(any _) {
    return Ꮡ(new HaltError(new exitCodeError(default, 0)));
}

internal static any funcHaltError(any v, slice<any> args) {
    ref var code = ref heap<nint>(5, out var Ꮡcode);
    if (len(args) > 0) {
        bool ok = default;
        (code, ok) = toInt(args[0]); if (!ok) {
            return Ꮡ(new func0TypeError("halt_error", args[0]));
        }
    }
    return Ꮡ(new HaltError(new exitCodeError(v, code)));
}

internal static (nint, bool) toInt(any x) {
    switch (x.type()) {
    case nint xΔ1: {
        return (xΔ1, true);
    }
    case float64 xΔ1: {
        return (floatToInt(xΔ1), true);
    }
    case ж<bigꓸInt> xΔ1: {
        if (xΔ1.IsInt64()) {
            var i = xΔ1.Int64(); if (math.MinInt <= i && i <= math.MaxInt) {
                return ((nint)i, true);
            }
        }
        if (xΔ1.Sign() > 0) {
            return (math.MaxInt, true);
        }
        return (math.MinInt, true);
    }
    case json.Number xΔ1: {
        return toInt(parseNumber(xΔ1));
    }
    default: {
        var xΔ1 = x;
        return (0, false);
    }}
}

internal static (nint, bool) toIntCeil(any x) {
    var (f, ok) = x._<float64>(ᐧ); if (ok) {
        x = math.Ceil(f);
    }
    return toInt(x);
}

internal static nint floatToInt(float64 x) {
    if (math.MinInt <= x && x < math.MaxInt) {
        return (nint)x;
    }
    if (x > 0D) {
        return math.MaxInt;
    }
    return math.MinInt;
}

internal static (float64, bool) toFloat(any x) {
    switch (x.type()) {
    case nint xΔ1: {
        return ((float64)xΔ1, true);
    }
    case float64 xΔ1: {
        return (xΔ1, true);
    }
    case ж<bigꓸInt> xΔ1: {
        return (bigToFloat(xΔ1), true);
    }
    case json.Number xΔ1: {
        var (v, err) = xΔ1.Float64();
        return (v, err == nil);
    }
    default: {
        var xΔ1 = x;
        return (0.0D, false);
    }}
}

internal static float64 bigToFloat(ж<bigꓸInt> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (x.IsInt64()) {
        return (float64)x.Int64();
    }
    var (f, err) = strconv.ParseFloat(Ꮡx.String(), 64); if (err == nil) {
        return f;
    }
    return math.Inf(x.Sign());
}

internal static any parseNumber(json.Number v) {
    {
        var (i, err) = v.Int64(); if (err == nil && math.MinInt <= i && i <= math.MaxInt) {
            return (nint)i;
        }
    }
    if (strings.ContainsAny(v.String(), ".eE")) {
        var (f, err) = v.Float64(); if (err == nil) {
            return f;
        }
    }
    var (bi, ok) = @new<bigꓸInt>().SetString(v.String(), 10); if (ok) {
        return bi.OrTypedNil();
    }
    if (strings.HasPrefix(v.String(), "-")) {
        return math.Inf(-1);
    }
    return math.Inf(1);
}

} // end gojq_package
