// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.github.com.spf13;

using goflag = flag_package;
using reflect = reflect_package;
using strings = strings_package;
using time = time_package;
using flag = flag_package;

partial class pflag_package {

internal static bool isGotestFlag(@string flag) {
    return strings.HasPrefix(flag, "-test.");
}

internal static bool isGotestShorthandFlag(@string flag) {
    return strings.HasPrefix(flag, "test.");
}

partial struct flagValueWrapper {
    internal goflag.Value inner;
    internal @string flagType;
}

partial interface goBoolFlag :
    goflag.Value
{
    bool IsBoolFlag();
}

internal static Value wrapFlagValue(goflag.Value v) {
    {
        var (pvΔ1, ok) = v._<Value>(ᐧ); if (ok) {
            return pvΔ1;
        }
    }

    var pv = Ꮡ(new flagValueWrapper(
        inner: v
    ));

    var t = reflect.TypeOf(v);
    if (t.Kind() == reflect.ΔInterface || t.Kind() == reflect.Ptr) {
        t = t.Elem();
    }

    pv.Value.flagType = strings.TrimSuffix(t.Name(), "Value");
    return new flagValueWrapperжValue(pv);
}

internal static @string String(this ref flagValueWrapper v) {
    return v.inner.String();
}

internal static error Set(this ref flagValueWrapper v, @string s) {
    return v.inner.Set(s);
}

internal static @string Type(this ref flagValueWrapper v) {
    return v.flagType;
}

public static ж<Flag> PFlagFromGoFlag(ж<goflag.Flag> Ꮡgoflag) {
    ref var goflagΔ1 = ref Ꮡgoflag.DerefOrNull();

    var flag = Ꮡ(new Flag(
        Name: goflagΔ1.Name,
        Usage: goflagΔ1.Usage,
        Value: wrapFlagValue(goflagΔ1.Value),
        DefValue: goflagΔ1.Value.String()
    ));
    if (len((~flag).Name) == 1) {
        flag.Value.Shorthand = flag.Value.Name;
    }
    {
        var (fv, ok) = goflagΔ1.Value._<goBoolFlag>(ᐧ); if (ok && fv.IsBoolFlag()) {
            flag.Value.NoOptDefVal = "true";
        }
    }
    return flag;
}

public static void AddGoFlag(this ж<FlagSet> Ꮡf, ж<goflag.Flag> Ꮡgoflag) {
    ref var goflagΔ1 = ref Ꮡgoflag.DerefOrNull();

    if (Ꮡf.Lookup(goflagΔ1.Name) != nil) {
        return;
    }
    var newflag = PFlagFromGoFlag(Ꮡgoflag);
    Ꮡf.AddFlag(newflag);
}

public static void AddGoFlagSet(this ж<FlagSet> Ꮡf, ж<goflag.FlagSet> ᏑnewSet) {
    ref var f = ref Ꮡf.DerefOrNull();
    ref var newSet = ref ᏑnewSet.DerefOrNull();

    if (ᏑnewSet == nil) {
        return;
    }
    newSet.VisitAll((ж<goflag.Flag> goflagΔ1) => {
        Ꮡf.AddGoFlag(goflagΔ1);
    });
    if (f.addedGoFlagSets == default!) {
        f.addedGoFlagSets = new slice<ж<goflag.FlagSet>>(0);
    }
    f.addedGoFlagSets = append(f.addedGoFlagSets, ᏑnewSet);
}

public static void CopyToGoFlagSet(this ref FlagSet f, ж<goflag.FlagSet> ᏑnewSet) {
    f.VisitAll((ж<Flag> flag) => {
        @string usage = flag.Value.Usage;
        if ((~flag).Deprecated != ""u8) {
            usage += " (DEPRECATED: "u8 + (~flag).Deprecated + ")"u8;
        }

        switch ((~flag).Value.type()) {
        case ж<stringValue> value: {
            ᏑnewSet.Value.StringVar(value.Reinterpret<stringValue, @string>(), (~flag).Name, (~flag).DefValue, usage);
            break;
        }
        case ж<intValue> value: {
            ᏑnewSet.Value.IntVar(value.Reinterpret<intValue, nint>(), (~flag).Name, ~value.Reinterpret<intValue, nint>(), usage);
            break;
        }
        case ж<int64Value> value: {
            ᏑnewSet.Value.Int64Var(value.Reinterpret<int64Value, int64>(), (~flag).Name, ~value.Reinterpret<int64Value, int64>(), usage);
            break;
        }
        case ж<uintValue> value: {
            ᏑnewSet.Value.UintVar(value.Reinterpret<uintValue, nuint>(), (~flag).Name, ~value.Reinterpret<uintValue, nuint>(), usage);
            break;
        }
        case ж<uint64Value> value: {
            ᏑnewSet.Value.Uint64Var(value.Reinterpret<uint64Value, uint64>(), (~flag).Name, ~value.Reinterpret<uint64Value, uint64>(), usage);
            break;
        }
        case ж<durationValue> value: {
            ᏑnewSet.Value.DurationVar(value.Reinterpret<durationValue, time.Duration>(), (~flag).Name, ~value.Reinterpret<durationValue, time.Duration>(), usage);
            break;
        }
        case ж<float64Value> value: {
            ᏑnewSet.Value.Float64Var(value.Reinterpret<float64Value, float64>(), (~flag).Name, ~value.Reinterpret<float64Value, float64>(), usage);
            break;
        }
        default: {
            var value = (~flag).Value;
            ᏑnewSet.Value.Var((~flag).Value, (~flag).Name, usage);
            break;
        }}
    });
}

public static error ParseSkippedFlags(slice<@string> osArgs, ж<goflag.FlagSet> ᏑgoFlagSet) {
    slice<@string> skippedFlags = default!;
    foreach (var (_, f) in osArgs) {
        if (isGotestFlag(f)) {
            skippedFlags = append(skippedFlags, f);
        }
    }
    return ᏑgoFlagSet.Parse(skippedFlags);
}

} // end pflag_package
