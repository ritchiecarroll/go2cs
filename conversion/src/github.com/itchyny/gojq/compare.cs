namespace go.github.com.itchyny;

using cmp = cmp_package;
using json = encoding.json_package;
using math = math_package;
using big = go.math.big_package;
using encoding;
using go.math;

partial class gojq_package {

public static nint Compare(any l, any r) {
    return binopTypeSwitch<nint>(l, r,
        cmp.Compare<nint>,
        nint (float64 lΔ1, float64 rΔ1) => {
            switch (ᐧ) {
            case {} when lt(lΔ1, rΔ1): {
                return -1;
            }
            case {} when lΔ1 == rΔ1: {
                return 0;
            }
            default: {
                return 1;
            }}

        },
        ((Func<ж<bigꓸInt>, ж<bigꓸInt>, nint>)(big.Cmp)),
        cmp.Compare<@string>,
        nint (slice<any> lΔ2, slice<any> rΔ2) => {
            foreach (var i in range(min(len(lΔ2), len(rΔ2)))) {
                nint cmpΔ1 = Compare(lΔ2[i], rΔ2[i]); if (cmpΔ1 != 0) {
                    return cmpΔ1;
                }
            }
            return cmp.Compare(len(lΔ2), len(rΔ2));
        },
        nint (map<@string, any> lΔ3, map<@string, any> rΔ3) => {
            var (lk, rk) = (funcKeys(lΔ3), funcKeys(rΔ3));
            nint cmpΔ2 = Compare(lk, rk); if (cmpΔ2 != 0) {
                return cmpΔ2;
            }
            foreach (var (_, k) in range(lk._<slice<any>>())) {
                nint cmpΔ3 = Compare(lΔ3[k._<@string>()], rΔ3[k._<@string>()]); if (cmpΔ3 != 0) {
                    return cmpΔ3;
                }
            }
            return 0;
        },
        nint (any lΔ4, any rΔ4) => cmp.Compare(typeIndex(lΔ4), typeIndex(rΔ4)));
}

internal static bool lt(float64 l, float64 r) {
    return l < r || math.IsNaN(l);
}

internal static nint typeIndex(any v) {
    switch (v.type()) {
    default: {
        var vΔ1 = v;
        return 0;
    }
    case bool vΔ1: {
        if (!vΔ1) {
            return 1;
        }
        return 2;
    }
    case nint _:
    case float64 _:
    case ж<bigꓸInt> _:
    case json.Number _: {
        var vΔ1 = v;
        return 3;
    }
    case @string vΔ1: {
        return 4;
    }
    case slice<any> vΔ1: {
        return 5;
    }
    case map<@string, any> vΔ1: {
        return 6;
    }}
}

} // end gojq_package
