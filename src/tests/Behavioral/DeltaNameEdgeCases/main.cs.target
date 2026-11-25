namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class main_package {

public static float64 Δx = 2.5D;
public static nint Δ = 7;

public static UntypedInt ΔK => 3;

[GoName("Δf")] public static partial @string Δf() {
    var (pc, _, _, ok) = runtime.Caller(0);

    if (!ok) {
        return "no caller";
    }

    return runtime.FuncForPC(pc).Name();
}

internal static float64 scale(float64 Δv) {
    return Δv * 2D;
}

internal static void deltaBesideLocal() {
    var t = 1.5D;
    var Δt = 0.25D;
    fmt.Println("t:", t, "Δt:", Δt, "t+Δt:", t + Δt);
}

internal static slice<nint> slice2 = [9, 8];

internal static void reservedLocals() {
    var Δslice = slice<nint>([1, 2, 3]);
    var Δarray = array<@string>(["a", "b"]);
    Δslice = append(Δslice, len(Δarray));
    fmt.Println("slice:", Δslice, "array:", Δarray, "slice2:", slice2);
}

internal static void shadowedLevels() {
    nint x = 1;
    fmt.Println("level 0:", x);
    {
        nint xΔ1 = 2;
        fmt.Println("level 1:", xΔ1);
        {
            nint xΔ2 = 3;
            fmt.Println("level 2:", xΔ2);
        }
        fmt.Println("level 1 again:", xΔ1);
    }
    fmt.Println("level 0 again:", x);
}

internal static @string describe(any v) {
    switch (v.type()) {
    case nint w: {
        return fmt.Sprint("int ", w + 1);
    }
    case @string w: {
        return "string "u8 + strings.ToUpper(w);
    }
    case slice<nint> w: {
        return fmt.Sprint("slice of ", len(w));
    }
    case {} Δw when Δw._<Texter>(out var w): {
        return "texter "u8 + w.Text();
    }
    default: {
        var w = v;
        return fmt.Sprintf("other %v"u8, w);
    }}
}

internal static void importAlias() {
    fmt.Println("IsUpper(Δ):", Δunicode.IsUpper('Δ'), "RuneLen(Δ):", utf8.RuneLen('Δ'));
}

partial interface Texter {
    @string Text();
}

partial struct word {
    internal @string s;
}

internal static @string Text(this word w) { return "word "u8 + w.s; }

partial struct count /*num:nint*/;

internal static @string Text(this count c) { return fmt.Sprint("count ", (nint)c); }

partial struct pair<T> {
    internal T first, second;
}

internal static (pair<T>, @string) keep<T, TΔTexter>(T v)
    where TΔTexter : struct, TexterΔshape<T>
{ return (new pair<T>(v, v), "[" + TΔTexter.Text(v) + "]"); }

partial struct Q {
}

[GoName("Δ")] internal static partial @string Δ⁔(/*this*/ Q _Δp0) {
    var (pc, _, _, _) = runtime.Caller(0);
    return "Q.Δ in "u8 + runtime.FuncForPC(pc).Name();
}

internal partial interface bareDeltaMethod_type /*dyn*/ {
    [GoName("Δ")] @string Δ();
}

internal static void bareDeltaMethod() {
    fmt.Println(new Q().Δ());
    any i = new Q();
    var (d, ok) = i._<bareDeltaMethod_type>(ᐧ);
    fmt.Println("assert:", ok, d.Δ());
    fmt.Println("reflect method:", reflect.TypeOf(new Q()).Method(0).Name);
}

internal static nint yΔ1 = 11;
internal static nint yΔp = 12;
public static @string Δbytes = "not an import";
public static nint QΔTexter = 21;
public static nint TΔOther = 22;
public static nint Δp = 31;
public static nint Δpx = 32;
public static nint Δe = 33;

partial struct R {
    internal nint tΔ;
}

public static nint UΔ(this R r) { return r.tΔ * 2; }

internal static void nonLeadingDelta() {
    var r = new R(tΔ: 5);
    fmt.Println(fmt.Sprintf("%+v"u8, r), r.UΔ(), reflect.TypeOf(r).Field(0).Name, reflect.TypeOf(r).Method(0).Name);
}

internal static @string twice<ΔT>(ΔT v) { return fmt.Sprint((v).OrTypedNilOf(), (v).OrTypedNilOf()); }

partial struct label {
    internal @string s;
}

internal static @string String(this label l) { return "label "u8 + l.s; }

internal static (slice<S>, @string) strs<S>(S v)
    where S : fmt.Stringer
{ return (slice<S>([v]), v.String()); }

internal static void main() {
    fmt.Println("Δx:", Δx, "Δ:", Δ, "ΔK:", (nint)(ΔK), "Δx*ΔK:", Δx * (float64)ΔK);
    fmt.Println("scale(Δx):", scale(Δx));
    fmt.Println("Δf:", Δf());
    deltaBesideLocal();
    reservedLocals();
    shadowedLevels();
    fmt.Println(describe((nint)41), "|", describe("go"), "|", describe(slice<nint>([1, 2])), "|", describe(new word("w")), "|", describe(1.5D));
    importAlias();

    ref var n = ref heap<count>((count)3, out var Ꮡn);
    var (ws, wText) = keep<ж<word>, wordжTexterΔshape>(Ꮡ(new word("Δ")));
    var (cs, cText) = keep<ж<count>, countжTexterΔshape>(Ꮡn);
    fmt.Println(wText, cText, ws.second.s, (nint)(cs.first.Value));
    bareDeltaMethod();
    fmt.Println("names:", yΔ1, yΔp, Δbytes, QΔTexter, TΔOther, Δp, Δpx, Δe);
    nonLeadingDelta();
    fmt.Println("twice:", twice<nint>(4), twice((@string)"ab"));
    var (ls, lText) = strs(new label("x"));
    fmt.Println("strs:", len(ls), lText);
}

} // end main_package
