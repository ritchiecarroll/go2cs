namespace go.github.com.itchyny;

using json = encoding.json_package;
using maps = maps_package;
using math = math_package;
using big = go.math.big_package;
using strings = strings_package;
using encoding;
using go.math;

partial class gojq_package {

partial struct Operator /*num:nint*/;

public static Operator OpPipe => /* iota + 1 */ 1;
public static Operator OpComma => 2;
public static Operator OpAdd => 3;
public static Operator OpSub => 4;
public static Operator OpMul => 5;
public static Operator OpDiv => 6;
public static Operator OpMod => 7;
public static Operator OpEq => 8;
public static Operator OpNe => 9;
public static Operator OpGt => 10;
public static Operator OpLt => 11;
public static Operator OpGe => 12;
public static Operator OpLe => 13;
public static Operator OpAnd => 14;
public static Operator OpOr => 15;
public static Operator OpAlt => 16;
public static Operator OpAssign => 17;
public static Operator OpModify => 18;
public static Operator OpUpdateAdd => 19;
public static Operator OpUpdateSub => 20;
public static Operator OpUpdateMul => 21;
public static Operator OpUpdateDiv => 22;
public static Operator OpUpdateMod => 23;
public static Operator OpUpdateAlt => 24;

public static @string String(this Operator op) {
    switch (op) {
    case var ᴛ when ᴛ == OpPipe: {
        return "|";
    }
    case var ᴛ when ᴛ == OpComma: {
        return ",";
    }
    case var ᴛ when ᴛ == OpAdd: {
        return "+";
    }
    case var ᴛ when ᴛ == OpSub: {
        return "-";
    }
    case var ᴛ when ᴛ == OpMul: {
        return "*";
    }
    case var ᴛ when ᴛ == OpDiv: {
        return "/";
    }
    case var ᴛ when ᴛ == OpMod: {
        return "%";
    }
    case var ᴛ when ᴛ == OpEq: {
        return "==";
    }
    case var ᴛ when ᴛ == OpNe: {
        return "!=";
    }
    case var ᴛ when ᴛ == OpGt: {
        return ">";
    }
    case var ᴛ when ᴛ == OpLt: {
        return "<";
    }
    case var ᴛ when ᴛ == OpGe: {
        return ">=";
    }
    case var ᴛ when ᴛ == OpLe: {
        return "<=";
    }
    case var ᴛ when ᴛ == OpAnd: {
        return "and";
    }
    case var ᴛ when ᴛ == OpOr: {
        return "or";
    }
    case var ᴛ when ᴛ == OpAlt: {
        return "//";
    }
    case var ᴛ when ᴛ == OpAssign: {
        return "=";
    }
    case var ᴛ when ᴛ == OpModify: {
        return "|=";
    }
    case var ᴛ when ᴛ == OpUpdateAdd: {
        return "+=";
    }
    case var ᴛ when ᴛ == OpUpdateSub: {
        return "-=";
    }
    case var ᴛ when ᴛ == OpUpdateMul: {
        return "*=";
    }
    case var ᴛ when ᴛ == OpUpdateDiv: {
        return "/=";
    }
    case var ᴛ when ᴛ == OpUpdateMod: {
        return "%=";
    }
    case var ᴛ when ᴛ == OpUpdateAlt: {
        return "//=";
    }
    default: {
        throw panic(op);
        break;
    }}

}

public static @string /*str*/ GoString(this Operator op) {
    @string str = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            str = "gojq."u8 + str;
        }, ref ᒐ);
        switch (op) {
        case var ᴛ when ᴛ == ((Operator)0): {
            str = "Operator(0)"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpPipe: {
            str = "OpPipe"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpComma: {
            str = "OpComma"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpAdd: {
            str = "OpAdd"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpSub: {
            str = "OpSub"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpMul: {
            str = "OpMul"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpDiv: {
            str = "OpDiv"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpMod: {
            str = "OpMod"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpEq: {
            str = "OpEq"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpNe: {
            str = "OpNe"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpGt: {
            str = "OpGt"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpLt: {
            str = "OpLt"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpGe: {
            str = "OpGe"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpLe: {
            str = "OpLe"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpAnd: {
            str = "OpAnd"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpOr: {
            str = "OpOr"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpAlt: {
            str = "OpAlt"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpAssign: {
            str = "OpAssign"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpModify: {
            str = "OpModify"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateAdd: {
            str = "OpUpdateAdd"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateSub: {
            str = "OpUpdateSub"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateMul: {
            str = "OpUpdateMul"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateDiv: {
            str = "OpUpdateDiv"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateMod: {
            str = "OpUpdateMod"; goto ᒐdone;
        }
        case var ᴛ when ᴛ == OpUpdateAlt: {
            str = "OpUpdateAlt"; goto ᒐdone;
        }
        default: {
            throw panic(op);
            break;
        }}

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return str;
}

internal static @string getFunc(this Operator op) {
    switch (op) {
    case var ᴛ when ᴛ == OpAdd || ᴛ == OpUpdateAdd: {
        return "_add";
    }
    case var ᴛ when ᴛ == OpSub || ᴛ == OpUpdateSub: {
        return "_subtract";
    }
    case var ᴛ when ᴛ == OpMul || ᴛ == OpUpdateMul: {
        return "_multiply";
    }
    case var ᴛ when ᴛ == OpDiv || ᴛ == OpUpdateDiv: {
        return "_divide";
    }
    case var ᴛ when ᴛ == OpMod || ᴛ == OpUpdateMod: {
        return "_modulo";
    }
    case var ᴛ when ᴛ == OpEq: {
        return "_equal";
    }
    case var ᴛ when ᴛ == OpNe: {
        return "_notequal";
    }
    case var ᴛ when ᴛ == OpGt: {
        return "_greater";
    }
    case var ᴛ when ᴛ == OpLt: {
        return "_less";
    }
    case var ᴛ when ᴛ == OpGe: {
        return "_greatereq";
    }
    case var ᴛ when ᴛ == OpLe: {
        return "_lesseq";
    }
    case var ᴛ when ᴛ == OpAssign: {
        return "_assign";
    }
    case var ᴛ when ᴛ == OpModify: {
        return "_modify";
    }
    case var ᴛ when ᴛ == OpUpdateAlt: {
        return "_alternative";
    }
    default: {
        throw panic(op);
        break;
    }}

}

internal static T binopTypeSwitch<T>(any l, any r, Func<nint, nint, T> callbackInts, Func<float64, float64, T> callbackFloats, Func<ж<bigꓸInt>, ж<bigꓸInt>, T> callbackBigInts, Func<@string, @string, T> callbackStrings, Func<slice<any>, slice<any>, T> callbackArrays, Func<map<@string, any>, map<@string, any>, T> callbackMaps, Func<any, any, T> fallback) {
    {
        var (n, ok) = l._<json.Number>(ᐧ); if (ok) {
            l = parseNumber(n);
        }
    }
    {
        var (n, ok) = r._<json.Number>(ᐧ); if (ok) {
            r = parseNumber(n);
        }
    }
    switch (l.type()) {
    case nint lΔ1: {
        switch (r.type()) {
        case nint rΔ1: {
            return callbackInts(lΔ1, rΔ1);
        }
        case float64 rΔ1: {
            return callbackFloats((float64)lΔ1, rΔ1);
        }
        case ж<bigꓸInt> rΔ1: {
            return callbackBigInts(big.NewInt((int64)lΔ1), rΔ1);
        }
        default: {
            var rΔ1 = r;
            return fallback(lΔ1, rΔ1);
        }}
        break;
    }
    case float64 lΔ1: {
        switch (r.type()) {
        case nint rΔ2: {
            return callbackFloats(lΔ1, (float64)rΔ2);
        }
        case float64 rΔ2: {
            return callbackFloats(lΔ1, rΔ2);
        }
        case ж<bigꓸInt> rΔ2: {
            return callbackFloats(lΔ1, bigToFloat(rΔ2));
        }
        default: {
            var rΔ2 = r;
            return fallback(lΔ1, rΔ2);
        }}
        break;
    }
    case ж<bigꓸInt> lΔ1: {
        switch (r.type()) {
        case nint rΔ3: {
            return callbackBigInts(lΔ1, big.NewInt((int64)rΔ3));
        }
        case float64 rΔ3: {
            return callbackFloats(bigToFloat(lΔ1), rΔ3);
        }
        case ж<bigꓸInt> rΔ3: {
            return callbackBigInts(lΔ1, rΔ3);
        }
        default: {
            var rΔ3 = r;
            return fallback(lΔ1.OrTypedNil(), rΔ3);
        }}
        break;
    }
    case @string lΔ1: {
        switch (r.type()) {
        case @string rΔ4: {
            return callbackStrings(lΔ1, rΔ4);
        }
        default: {
            var rΔ4 = r;
            return fallback(lΔ1, rΔ4);
        }}
        break;
    }
    case slice<any> lΔ1: {
        switch (r.type()) {
        case slice<any> rΔ5: {
            return callbackArrays(lΔ1, rΔ5);
        }
        default: {
            var rΔ5 = r;
            return fallback(lΔ1, rΔ5);
        }}
        break;
    }
    case map<@string, any> lΔ1: {
        switch (r.type()) {
        case map<@string, any> rΔ6: {
            return callbackMaps(lΔ1, rΔ6);
        }
        default: {
            var rΔ6 = r;
            return fallback(lΔ1, rΔ6);
        }}
        break;
    }
    default: {
        var lΔ1 = l;
        return fallback(lΔ1, r);
    }}
}

internal static any funcOpPlus(any v) {
    switch (v.type()) {
    case nint vΔ1: {
        return vΔ1;
    }
    case float64 vΔ1: {
        return vΔ1;
    }
    case ж<bigꓸInt> vΔ1: {
        return vΔ1.OrTypedNil();
    }
    case json.Number vΔ1: {
        return vΔ1;
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new unaryTypeError("plus", vΔ1));
    }}
}

internal static any negate(nint v) {
    if (v == math.MinInt) {
        return @new<bigꓸInt>().Neg(big.NewInt((int64)v)).OrTypedNil();
    }
    return -v;
}

internal static any funcOpNegate(any v) {
    switch (v.type()) {
    case nint vΔ1: {
        return negate(vΔ1);
    }
    case float64 vΔ1: {
        return -vΔ1;
    }
    case ж<bigꓸInt> vΔ1: {
        return @new<bigꓸInt>().Neg(vΔ1).OrTypedNil();
    }
    case json.Number vΔ1: {
        if (strings.HasPrefix(vΔ1.String(), "-")) {
            return vΔ1[1..];
        }
        return "-"u8 + vΔ1;
    }
    default: {
        var vΔ1 = v;
        return Ꮡ(new unaryTypeError("negate", vΔ1));
    }}
}

internal static any funcOpAdd(any _, any l, any r) {
    return binopTypeSwitch(l, r,
        any (nint lΔ1, nint rΔ1) => {
            nint v = lΔ1 + rΔ1; if ((v >= lΔ1) == (rΔ1 >= 0)) {
                return v;
            }
            var (x, y) = (big.NewInt((int64)lΔ1), big.NewInt((int64)rΔ1));
            return x.Add(x, y).OrTypedNil();
        },
        any (float64 lΔ2, float64 rΔ2) => lΔ2 + rΔ2,
        any (ж<bigꓸInt> lΔ3, ж<bigꓸInt> rΔ3) => @new<bigꓸInt>().Add(lΔ3, rΔ3).OrTypedNil(),
        any (@string lΔ4, @string rΔ4) => lΔ4 + rΔ4,
        any (slice<any> lΔ5, slice<any> rΔ5) => {
            if (len(lΔ5) == 0) {
                return rΔ5;
            }
            if (len(rΔ5) == 0) {
                return lΔ5;
            }
            var v = new slice<any>(len(lΔ5) + len(rΔ5));
            copy(v, lΔ5);
            copy(v.slice(len(lΔ5)), rΔ5);
            return v;
        },
        any (map<@string, any> lΔ6, map<@string, any> rΔ6) => {
            if (len(lΔ6) == 0) {
                return rΔ6;
            }
            if (len(rΔ6) == 0) {
                return lΔ6;
            }
            var m = new map<@string, any>(len(lΔ6) + len(rΔ6));
            maps.Copy<map<@string, any>, map<@string, any>, @string, any>(m, lΔ6);
            maps.Copy<map<@string, any>, map<@string, any>, @string, any>(m, rΔ6);
            return m;
        },
        any (any lΔ7, any rΔ7) => {
            if (lΔ7 == nil) {
                return rΔ7;
            }
            if (rΔ7 == nil) {
                return lΔ7;
            }
            return Ꮡ(new binopTypeError("add", lΔ7, rΔ7));
        });
}

internal static any funcOpSub(any _, any l, any r) {
    return binopTypeSwitch(l, r,
        any (nint lΔ1, nint rΔ1) => {
            nint v = lΔ1 - rΔ1; if ((v <= lΔ1) == (rΔ1 >= 0)) {
                return v;
            }
            var (x, y) = (big.NewInt((int64)lΔ1), big.NewInt((int64)rΔ1));
            return x.Sub(x, y).OrTypedNil();
        },
        any (float64 lΔ2, float64 rΔ2) => lΔ2 - rΔ2,
        any (ж<bigꓸInt> lΔ3, ж<bigꓸInt> rΔ3) => @new<bigꓸInt>().Sub(lΔ3, rΔ3).OrTypedNil(),
        any (@string lΔ4, @string rΔ4) => Ꮡ(new binopTypeError("subtract", lΔ4, rΔ4)),
        any (slice<any> lΔ5, slice<any> rΔ5) => {
            var v = new slice<any>(0, len(lΔ5));
L:
            foreach (var (_, lΔ6) in range(lΔ5)) {
                foreach (var (_, rΔ6) in range(rΔ5)) {
                    if (Compare(lΔ6, rΔ6) == 0) {
                        goto continue_L;
                    }
                }
                v = builtin.append(v, lΔ6);
continue_L:;
            }
break_L:;
            return v;
        },
        any (map<@string, any> lΔ7, map<@string, any> rΔ7) => Ꮡ(new binopTypeError("subtract", lΔ7, rΔ7)),
        any (any lΔ8, any rΔ8) => Ꮡ(new binopTypeError("subtract", lΔ8, rΔ8)));
}

internal static any funcOpMul(any _, any l, any r) {
    return binopTypeSwitch<any>(l, r,
        any (nint lΔ1, nint rΔ1) => {
            if (rΔ1 == -1) {
                return negate(lΔ1);
            }
            nint v = lΔ1 * rΔ1; if (rΔ1 == 0 || quo(v, rΔ1) == lΔ1) {
                return v;
            }
            var (x, y) = (big.NewInt((int64)lΔ1), big.NewInt((int64)rΔ1));
            return x.Mul(x, y).OrTypedNil();
        },
        any (float64 lΔ2, float64 rΔ2) => lΔ2 * rΔ2,
        any (ж<bigꓸInt> lΔ3, ж<bigꓸInt> rΔ3) => @new<bigꓸInt>().Mul(lΔ3, rΔ3).OrTypedNil(),
        any (@string lΔ4, @string rΔ4) => Ꮡ(new binopTypeError("multiply", lΔ4, rΔ4)),
        any (slice<any> lΔ5, slice<any> rΔ5) => Ꮡ(new binopTypeError("multiply", lΔ5, rΔ5)),
        deepMergeObjects,
        any (any lΔ6, any rΔ6) => {
            if (lΔ6._<@string>(out var lΔ7)) {
                var (rΔ7, okΔ1) = toFloat(rΔ6); if (okΔ1) {
                    return repeatString(lΔ7, rΔ7);
                }
            }
            if (rΔ6._<@string>(out var rΔ8)) {
                var (lΔ8, okΔ1) = toFloat(lΔ6); if (okΔ1) {
                    return repeatString(rΔ8, lΔ8);
                }
            }
            return Ꮡ(new binopTypeError("multiply", lΔ6, rΔ6));
        });
}

internal static any deepMergeObjects(map<@string, any> l, map<@string, any> r) {
    var m = new map<@string, any>(len(l) + len(r));
    maps.Copy<map<@string, any>, map<@string, any>, @string, any>(m, l);
    foreach (var (k, vᴛ1) in r) {
        var v = vᴛ1;

        var (mk, ok) = m[k, ꟷ]; if (ok) {
            var (mkΔ1, okΔ1) = mk._<map<@string, any>>(ᐧ); if (okΔ1) {
                var (w, okΔ2) = v._<map<@string, any>>(ᐧ); if (okΔ2) {
                    v = deepMergeObjects(mkΔ1, w);
                }
            }
        }
        m[k] = v;
    }
    return m;
}

internal static any repeatString(@string s, float64 n) {
    if (lt(n, 0D)) {
        return default;
    }
    nint c = (nint)min(n, (float64)(math.MaxInt32));
    if ((uint64)len(s) * (uint64)c >= math.MaxInt32) {
        return Ꮡ(new repeatStringTooLargeError(s, n));
    }
    return strings.Repeat(s, c);
}

internal static any funcOpDiv(any _, any l, any r) {
    return binopTypeSwitch(l, r,
        any (nint lΔ1, nint rΔ1) => {
            switch (rΔ1) {
            case 0: {
                return Ꮡ(new zeroDivisionError(lΔ1, rΔ1));
            }
            case var ᴛ when ᴛ == -1: {
                return negate(lΔ1);
            }
            default: {
                if (rem(lΔ1, rΔ1) == 0) {
                    return quo(lΔ1, rΔ1);
                }
                return (float64)lΔ1 / (float64)rΔ1;
            }}

        },
        any (float64 lΔ2, float64 rΔ2) => {
            if (rΔ2 == 0.0D) {
                return Ꮡ(new zeroDivisionError(lΔ2, rΔ2));
            }
            return lΔ2 / rΔ2;
        },
        any (ж<bigꓸInt> lΔ3, ж<bigꓸInt> rΔ3) => {
            if (rΔ3.Sign() == 0) {
                return Ꮡ(new zeroDivisionError(lΔ3.OrTypedNil(), rΔ3.OrTypedNil()));
            }
            var (d, m) = @new<bigꓸInt>().DivMod(lΔ3, rΔ3, @new<bigꓸInt>());
            if (m.Sign() == 0) {
                return d.OrTypedNil();
            }
            return bigToFloat(lΔ3) / bigToFloat(rΔ3);
        },
        any (@string lΔ4, @string rΔ4) => {
            if (lΔ4 == ""u8) {
                return slice<any>([]);
            }
            var xs = strings.Split(lΔ4, rΔ4);
            var vs = new slice<any>(len(xs));
            foreach (var (i, x) in range(xs)) {
                vs[i] = x;
            }
            return vs;
        },
        any (slice<any> lΔ5, slice<any> rΔ5) => Ꮡ(new binopTypeError("divide", lΔ5, rΔ5)),
        any (map<@string, any> lΔ6, map<@string, any> rΔ6) => Ꮡ(new binopTypeError("divide", lΔ6, rΔ6)),
        any (any lΔ7, any rΔ7) => Ꮡ(new binopTypeError("divide", lΔ7, rΔ7)));
}

internal static any funcOpMod(any _, any l, any r) {
    return binopTypeSwitch(l, r,
        any (nint lΔ1, nint rΔ1) => {
            switch (rΔ1) {
            case 0: {
                return Ꮡ(new zeroModuloError(lΔ1, rΔ1));
            }
            case var ᴛ when ᴛ == -1: {
                return (nint)0;
            }
            default: {
                return rem(lΔ1, rΔ1);
            }}

        },
        any (float64 lΔ2, float64 rΔ2) => {
            if (math.IsNaN(lΔ2) || math.IsNaN(rΔ2)) {
                return math.NaN();
            }
            nint ri = floatToInt(rΔ2);
            if (ri == 0) {
                return Ꮡ(new zeroModuloError(lΔ2, rΔ2));
            }
            return rem(floatToInt(lΔ2), ri);
        },
        any (ж<bigꓸInt> lΔ3, ж<bigꓸInt> rΔ3) => {
            if (rΔ3.Sign() == 0) {
                return Ꮡ(new zeroModuloError(lΔ3.OrTypedNil(), rΔ3.OrTypedNil()));
            }
            return @new<bigꓸInt>().Rem(lΔ3, rΔ3).OrTypedNil();
        },
        any (@string lΔ4, @string rΔ4) => Ꮡ(new binopTypeError("modulo", lΔ4, rΔ4)),
        any (slice<any> lΔ5, slice<any> rΔ5) => Ꮡ(new binopTypeError("modulo", lΔ5, rΔ5)),
        any (map<@string, any> lΔ6, map<@string, any> rΔ6) => Ꮡ(new binopTypeError("modulo", lΔ6, rΔ6)),
        any (any lΔ7, any rΔ7) => Ꮡ(new binopTypeError("modulo", lΔ7, rΔ7)));
}

internal static any funcOpAlt(any _, any l, any r) {
    if (l == nil || AreEqual(l, false)) {
        return r;
    }
    return l;
}

internal static any funcOpEq(any _, any l, any r) {
    return Compare(l, r) == 0;
}

internal static any funcOpNe(any _, any l, any r) {
    return Compare(l, r) != 0;
}

internal static any funcOpGt(any _, any l, any r) {
    return Compare(l, r) > 0;
}

internal static any funcOpLt(any _, any l, any r) {
    return Compare(l, r) < 0;
}

internal static any funcOpGe(any _, any l, any r) {
    return Compare(l, r) >= 0;
}

internal static any funcOpLe(any _, any l, any r) {
    return Compare(l, r) <= 0;
}

} // end gojq_package
