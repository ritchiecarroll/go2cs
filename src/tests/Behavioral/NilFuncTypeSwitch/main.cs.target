namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using typelib = NilFuncTypeSwitch.typelib_package;
using NilFuncTypeSwitch;

partial class main_package {

partial interface namer {
    @string Name();
}

internal delegate nint localStep(nint _);

internal static @string Name(this localStep s) { return "local"; }

internal delegate nint localStepE(nint _);

internal static @string Name(this localStepE s) { return "locale"; }

internal static @string Error(this localStepE s) { return "locale error"; }

internal delegate nint hook(@string _);

internal delegate bool left(bool _);

internal delegate bool right(bool _);

partial struct probe {
    internal @string name;
    internal Func<any, @string> run;
}

internal static namer viaAny(any a) {
    return a._<namer>();
}

internal static slice<@string> kinds = [
    "step", "stepe", "local", "locale", "bare", "hook", "left", "right",
    "livestep", "livelocale", "livebare", "livehook", "nilptr"
];

internal static slice<@string> armsFor(@string kind) {
    switch (ᐧ) {
    case {} when kind == "bare"u8 || kind == "hook"u8 || kind == "left"u8 || kind == "right"u8 || kind == "livebare"u8 || kind == "livehook"u8: {
        return ["direct"];
    }
    case {} when kind == "stepe"u8 || kind == "locale"u8 || kind == "livelocale"u8: {
        return ["direct", "va", "sh", "er"];
    }}

    return ["direct", "va", "sh"];
}

internal static bool hasName(@string kind) {
    return len(armsFor(kind)) > 1;
}

internal static any concrete(@string kind) {
    switch (ᐧ) {
    case {} when kind == "step"u8: {
        return (default(typelib.Step)!).OrTypedNilFunc();
    }
    case {} when kind == "stepe"u8: {
        return (default(typelib.StepE)!).OrTypedNilFunc();
    }
    case {} when kind == "local"u8: {
        return (default(localStep)!).OrTypedNilFunc();
    }
    case {} when kind == "locale"u8: {
        return (default(localStepE)!).OrTypedNilFunc();
    }
    case {} when kind == "bare"u8: {
        return ((Func<nint, nint>)(default)).OrTypedNilFunc();
    }
    case {} when kind == "hook"u8: {
        return (default(hook)!).OrTypedNilFunc();
    }
    case {} when kind == "left"u8: {
        return (default(left)!).OrTypedNilFunc();
    }
    case {} when kind == "right"u8: {
        return (default(right)!).OrTypedNilFunc();
    }
    case {} when kind == "livestep"u8: {
        return (typelib.NewStep()).OrTypedNilFunc();
    }
    case {} when kind == "livelocale"u8: {
        return new localStepE((nint x) => x + 2);
    }
    case {} when kind == "livebare"u8: {
        return nint (nint x) => x + 3;
    }
    case {} when kind == "livehook"u8: {
        return new hook((@string s) => len(s));
    }
    case {} when kind == "nilptr"u8: {
        return (ж<typelib.Box>)nil;
    }}

    return default;
}

internal static namer @static(@string kind) {
    namer n = default;
    switch (ᐧ) {
    case {} when kind == "step"u8: {
        n = new typelib_Stepᴠnamer(default(typelib.Step)!);
        break;
    }
    case {} when kind == "stepe"u8: {
        n = new typelib_StepEᴠnamer(default(typelib.StepE)!);
        break;
    }
    case {} when kind == "local"u8: {
        n = new localStepᴠnamer(default(localStep)!);
        break;
    }
    case {} when kind == "locale"u8: {
        n = new localStepEᴠnamer(default(localStepE)!);
        break;
    }
    case {} when kind == "livestep"u8: {
        n = new typelib_Stepᴠnamer(typelib.NewStep());
        break;
    }
    case {} when kind == "livelocale"u8: {
        n = new localStepEᴠnamer(new localStepE((nint x) => x + 2));
        break;
    }
    case {} when kind == "nilptr"u8: {
        n = new typelib_Boxжnamer(((ж<typelib.Box>)nil));
        break;
    }}

    return n;
}

internal static error asError(@string kind) {
    error e = default;
    switch (ᐧ) {
    case {} when kind == "stepe"u8: {
        e = new typelib_StepEᴠerror(default(typelib.StepE)!);
        break;
    }
    case {} when kind == "locale"u8: {
        e = new localStepEᴠerror(default(localStepE)!);
        break;
    }
    case {} when kind == "livelocale"u8: {
        e = new localStepEᴠerror(new localStepE((nint x) => x + 2));
        break;
    }}

    return e;
}

internal static any build(@string arm, @string kind) {
    switch (ᐧ) {
    case {} when arm == "direct"u8: {
        return concrete(kind);
    }
    case {} when arm == "va"u8: {
        any a = @static(kind);
        return a;
    }
    case {} when arm == "sh"u8: {
        any a = viaAny(concrete(kind));
        return a;
    }
    case {} when arm == "er"u8: {
        any a = asError(kind);
        return a;
    }}

    return default;
}

internal static @string which(any a) {
    switch (a.type()) {
    case null: {
        return "nil";
    }
    case {} Δv when Δv._<typelib.Step>(out var v): {
        if (v != nil) {
            return "Step live "u8 + fmt.Sprint(v(1));
        }
        return "Step nil";
    }
    case {} Δv when Δv._<typelib.StepE>(out var v): {
        if (v != nil) {
            return "StepE live "u8 + fmt.Sprint(v(1));
        }
        return "StepE nil";
    }
    case {} Δv when Δv._<localStep>(out var v): {
        if (v != nil) {
            return "localStep live "u8 + fmt.Sprint(v(1));
        }
        return "localStep nil";
    }
    case {} Δv when Δv._<localStepE>(out var v): {
        if (v != nil) {
            return "localStepE live "u8 + fmt.Sprint(v(1));
        }
        return "localStepE nil";
    }
    case {} Δv when Δv._<Func<nint, nint>>(out var v): {
        if (v != nil) {
            return "func live "u8 + fmt.Sprint(v(1));
        }
        return "func nil";
    }
    case {} Δv when Δv._<hook>(out var v): {
        if (v != nil) {
            return "hook live "u8 + fmt.Sprint(v("abc"));
        }
        return "hook nil";
    }
    case ж<typelib.Box> v: {
        return "Box "u8 + fmt.Sprint(v == nil);
    }}
    return "other";
}

internal static @string unbound(any a) {
    switch (a.type()) {
    case null: {
        return "nil";
    }
    case {} ᴛ0 when ᴛ0._<typelib.Step>(out var _): {
        return "Step";
    }
    case {} ᴛ0 when ᴛ0._<typelib.StepE>(out var _): {
        return "StepE";
    }
    case {} ᴛ0 when ᴛ0._<localStep>(out var _): {
        return "localStep";
    }
    case {} ᴛ0 when ᴛ0._<localStepE>(out var _): {
        return "localStepE";
    }
    case {} ᴛ0 when ᴛ0._<Func<nint, nint>>(out var _): {
        return "func";
    }
    case {} ᴛ0 when ᴛ0._<hook>(out var _): {
        return "hook";
    }
    case ж<typelib.Box>: {
        return "Box";
    }}

    return "other";
}

internal static @string multi(any a) {
    switch (a.type()) {
    case null: {
        return "nil";
    }
    case {} ᴛ0 when ᴛ0._<typelib.Step>(out var _):
    case {} ᴛ1 when ᴛ1._<typelib.StepE>(out var _): {
        var v = a;
        return "lib "u8 + fmt.Sprint(v == nil);
    }
    case {} ᴛ0 when ᴛ0._<localStep>(out var _):
    case {} ᴛ1 when ᴛ1._<localStepE>(out var _): {
        var v = a;
        return "local "u8 + fmt.Sprint(v == nil);
    }
    case {} ᴛ0 when ᴛ0._<Func<nint, nint>>(out var _):
    case {} ᴛ1 when ᴛ1._<hook>(out var _): {
        var v = a;
        return "plain "u8 + fmt.Sprint(v == nil);
    }
    case ж<typelib.Box> v: {
        return "Box "u8 + fmt.Sprint(v == nil);
    }}
    return "other";
}

internal static @string mixed(any a) {
    switch (a.type()) {
    case {} Δv when Δv._<error>(out var v): {
        return "error "u8 + v.Error();
    }
    case {} Δv when Δv._<typelib.Step>(out var v): {
        return "Step "u8 + fmt.Sprint(v == nil);
    }
    case {} Δv when Δv._<Func<nint, nint>>(out var v): {
        return "func "u8 + fmt.Sprint(v == nil);
    }
    case {} Δv when Δv._<namer>(out var v): {
        return "namer "u8 + v.Name();
    }
    case {} Δv when Δv._<hook>(out var v): {
        return "hook "u8 + fmt.Sprint(v == nil);
    }}
    return "other";
}

internal static bool stepIsNil(ref typelib.Step p) {
    return p == nil;
}

internal static bool bareIsNil(ref Func<nint, nint> p) {
    return p == nil;
}

internal static bool hookIsNil(ref hook p) {
    return p == nil;
}

internal static @string escape(any a) {
    switch (a.type()) {
    case {} Δv when Δv._<typelib.Step>(out var v): {
        return "Step "u8 + fmt.Sprint(stepIsNil(ref v));
    }
    case {} Δv when Δv._<Func<nint, nint>>(out var v): {
        return "func "u8 + fmt.Sprint(bareIsNil(ref v));
    }
    case {} Δv when Δv._<hook>(out var v): {
        return "hook "u8 + fmt.Sprint(hookIsNil(ref v));
    }}
    return "other";
}

internal static @string merged(any a) {
    switch (a.type()) {
    case {} ᴛ0 when ᴛ0._<left>(out var _): {
        return "pred";
    }
    case {} ᴛ0 when ᴛ0._<right>(out var _): {
        return "pred";
    }}

    return "other";
}

[GoLazy] partial class lazyᴛprobes { internal static slice<probe> probes = [
    new("bound", which),
    new("unbound", unbound),
    new("multi", multi),
    new("mixed", mixed),
    new("escape", escape),
    new("merged", merged),
    new("reswitch", (any a) => {
        any b = a._<namer>();
        return which(b);
    }),
    new("ifacesw", (any a) => which(reflect.ValueOf(a).Interface()))
]; }

internal static void runOne(@string arm, @string kind, probe p) {
    GoFrame ᒐ = default;
    try {
        var pʗ1 = p;
        defer(() => {
            var r = recover(); if (r != nil) {
                fmt.Println(arm, kind, pʗ1.name, "= PANIC", r);
            }
        }, ref ᒐ);
        var a = build(arm, kind);
        fmt.Println(arm, kind, p.name, "=", p.run(a));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool skip(@string kind, @string name) {
    return name == "reswitch"u8 && !hasName(kind);
}

internal static void main() {
    foreach (var (_, kind) in range(kinds)) {
        foreach (var (_, arm) in range(armsFor(kind))) {
            foreach (var (_, p) in range(probes)) {
                if (skip(kind, p.name)) {
                    continue;
                }
                runOne(arm, kind, p);
            }
        }
    }
}

} // end main_package
