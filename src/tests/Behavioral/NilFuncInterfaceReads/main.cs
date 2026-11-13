namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using typelib = NilFuncInterfaceReads.typelib_package;
using NilFuncInterfaceReads;

partial class main_package {

partial interface namer {
    @string Name();
}

internal delegate nint localStep(nint _);

internal static @string Name(this localStep s) { return "local"; }

internal delegate nint localStepE(nint _);

internal static @string Name(this localStepE s) { return "locale"; }

internal static @string Error(this localStepE s) { return "locale error"; }

partial struct probe {
    internal @string name;
    internal Func<any, @string> run;
}

internal static namer viaAny(any a) {
    return a._<namer>();
}

internal static slice<@string> kinds = ["step", "stepe", "local", "locale", "bare", "livestep", "nilptr"];

internal static slice<@string> armsFor(@string kind) {
    var exprᴛ1 = kind;
    switch (ᐧ) {
    case {} when exprᴛ1 == "bare"u8: {
        return ["direct"];
    }
    case {} when exprᴛ1 == "stepe"u8 || exprᴛ1 == "locale"u8: {
        return ["direct", "va", "sh", "er"];
    }}

    return ["direct", "va", "sh"];
}

internal static any concrete(@string kind) {
    var exprᴛ1 = kind;
    switch (ᐧ) {
    case {} when exprᴛ1 == "step"u8: {
        return (default(typelib.Step)!).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "stepe"u8: {
        return (default(typelib.StepE)!).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "local"u8: {
        return (default(localStep)!).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "locale"u8: {
        return (default(localStepE)!).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "bare"u8: {
        return ((Func<nint, nint>)(default)).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "livestep"u8: {
        return (typelib.NewStep()).OrTypedNilFunc();
    }
    case {} when exprᴛ1 == "nilptr"u8: {
        return (ж<typelib.Box>)nil;
    }}

    return default;
}

internal static namer @static(@string kind) {
    namer n = default;
    var exprᴛ1 = kind;
    switch (ᐧ) {
    case {} when exprᴛ1 == "step"u8: {
        n = new typelib_Stepᴠnamer(default(typelib.Step)!);
        break;
    }
    case {} when exprᴛ1 == "stepe"u8: {
        n = new typelib_StepEᴠnamer(default(typelib.StepE)!);
        break;
    }
    case {} when exprᴛ1 == "local"u8: {
        n = new localStepᴠnamer(default(localStep)!);
        break;
    }
    case {} when exprᴛ1 == "locale"u8: {
        n = new localStepEᴠnamer(default(localStepE)!);
        break;
    }
    case {} when exprᴛ1 == "livestep"u8: {
        n = new typelib_Stepᴠnamer(typelib.NewStep());
        break;
    }
    case {} when exprᴛ1 == "nilptr"u8: {
        n = new typelib_Boxжnamer(((ж<typelib.Box>)nil));
        break;
    }}

    return n;
}

internal static error asError(@string kind) {
    error e = default;
    var exprᴛ1 = kind;
    switch (ᐧ) {
    case {} when exprᴛ1 == "stepe"u8: {
        e = new typelib_StepEᴠerror(default(typelib.StepE)!);
        break;
    }
    case {} when exprᴛ1 == "locale"u8: {
        e = new localStepEᴠerror(default(localStepE)!);
        break;
    }}

    return e;
}

internal static any build(@string arm, @string kind) {
    var exprᴛ1 = arm;
    switch (ᐧ) {
    case {} when exprᴛ1 == "direct"u8: {
        return concrete(kind);
    }
    case {} when exprᴛ1 == "va"u8: {
        any a = @static(kind);
        return a;
    }
    case {} when exprᴛ1 == "sh"u8: {
        any a = viaAny(concrete(kind));
        return a;
    }
    case {} when exprᴛ1 == "er"u8: {
        any a = asError(kind);
        return a;
    }}

    return default;
}

internal static reflectꓸType namerType = reflect.TypeOf((ж<namer>)nil).Elem();

[GoLazy] partial class lazyᴛprobes { internal static slice<probe> probes = [
    new("l-isnil", (any a) => fmt.Sprint(a == nil)),
    new("l-namer", (any a) => {
        var (n, ok) = a._<namer>(ᐧ);
        if (!ok) {
            return "miss";
        }
        return n.Name();
    }),
    new("l-reassert", (any a) => {
        any b = a._<namer>();
        var (n, ok) = b._<namer>(ᐧ);
        if (!ok) {
            return "miss";
        }
        return n.Name();
    }),
    new("l-namernil", (any a) => fmt.Sprint(a._<namer>() == nil)),
    new("l-error", (any a) => {
        var (e, ok) = a._<error>(ᐧ);
        if (!ok) {
            return "miss";
        }
        return e.Error();
    }),
    new("l-errnamer", (any a) => {
        any b = a._<error>();
        var (n, ok) = b._<namer>(ᐧ);
        if (!ok) {
            return "miss";
        }
        return n.Name();
    }),
    new("l-eqself", (any a) => fmt.Sprint(AreEqual(a, a))),
    new("l-fmtT", (any a) => fmt.Sprintf("%T"u8, a)),
    new("l-fmtv", (any a) => fmt.Sprintf("%v"u8, a)),
    new("kind", (any a) => reflect.ValueOf(a).Kind().String()),
    new("type", (any a) => reflect.ValueOf(a).Type().String()),
    new("typeof", (any a) => reflect.TypeOf(a).String()),
    new("implements", (any a) => fmt.Sprint(reflect.TypeOf(a).Implements(namerType))),
    new("zero", (any a) => fmt.Sprint(reflect.Zero(reflect.TypeOf(a)).IsNil())),
    new("isnil", (any a) => fmt.Sprint(reflect.ValueOf(a).IsNil())),
    new("iszero", (any a) => fmt.Sprint(reflect.ValueOf(a).IsZero())),
    new("pointer", (any a) => fmt.Sprint(reflect.ValueOf(a).Pointer() == 0)),
    new("nummethod", (any a) => fmt.Sprint(reflect.ValueOf(a).NumMethod())),
    new("iface", (any a) => reflect.ValueOf(a).Interface()._<namer>().Name()),
    new("ifacenil", (any a) => fmt.Sprint(reflect.ValueOf(a).Interface() == nil)),
    new("ifaceT", (any a) => fmt.Sprintf("%T"u8, reflect.ValueOf(a).Interface())),
    new("method", (any a) => reflect.ValueOf(a).MethodByName("Name").Call(default)[0].String()),
    new("method0", (any a) => {
        var v = reflect.ValueOf(a);
        return reflect.TypeOf(a).Method(0).Name + " "u8 + v.Method(0).Call(default)[0].String();
    }),
    new("call", (any a) => {
        var @out = reflect.ValueOf(a).Call(slice<reflectꓸValue>([reflect.ValueOf((nint)1)]));
        return fmt.Sprint(@out[0].Int());
    }),
    new("same", (any a) => fmt.Sprint(reflect.ValueOf(a) == reflect.ValueOf(a))),
    new("deep", (any a) => fmt.Sprint(reflect.DeepEqual(a, a))),
    new("setslot", (any a) => {
        ref var slot = ref heap<namer>(out var Ꮡslot);
        reflect.ValueOf(Ꮡslot).Elem().Set(reflect.ValueOf(a));
        return Ꮡslot.ValueSlot.Name();
    }),
    new("setany", (any a) => {
        ref var slot = ref heap<any>(out var Ꮡslot);
        reflect.ValueOf(Ꮡslot).Elem().Set(reflect.ValueOf(a));
        return fmt.Sprintf("%T"u8, Ꮡslot.ValueSlot) + " "u8 + fmt.Sprint(Ꮡslot.ValueSlot == nil);
    }),
    new("setself", (any a) => {
        var v = reflect.ValueOf(a);
        var s = reflect.New(v.Type()).Elem();
        s.Set(v);
        return fmt.Sprint(s.IsNil());
    }),
    new("convert", (any a) => reflect.ValueOf(a).Convert(namerType).Interface()._<namer>().Name()),
    new("fmtvalue", (any a) => fmt.Sprintf("%v"u8, reflect.ValueOf(a)))
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
    return kind == "livestep"u8 && (name == "l-fmtv"u8 || name == "fmtvalue"u8);
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
