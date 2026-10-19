namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct Plain {
    internal nint v;
}

public static nint Get(this Plain p) {
    return p.v;
}

public static @string Describe(this Plain p, @string prefix) {
    return fmt.Sprint(prefix, p.v);
}

public static void Set(this ref Plain p, nint v) {
    p.v = v;
}

partial struct Box<T> {
    internal T v;
}

public static T Get<T>(this Box<T> b) {
    return b.v;
}

public static @string Describe<T>(this Box<T> b, @string prefix) {
    return fmt.Sprint(prefix, (b.v).OrTypedNilOf());
}

public static void Set<T>(this ref Box<T> b, T v) {
    b.v = v;
}

partial struct Pair<K, V> {
    internal K k;
    internal V v;
}

public static K Key<K, V>(this Pair<K, V> p) {
    return p.k;
}

public static @string String<K, V>(this Pair<K, V> p) {
    return fmt.Sprintf("%v=%v"u8, (p.k).OrTypedNilOf(), (p.v).OrTypedNilOf());
}

public static V /*old*/ Swap<K, V>(this ref Pair<K, V> p, V v) {
    V old = GoZero<V>();

    (old, p.v) = (p.v, v);
    return old;
}

partial struct age /*num:nint*/;

partial struct Ord2<T>
    where T : /* ~int */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal T a, b;
}

public static bool Less<T>(this Ord2<T> o)
    where T : /* ~int */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return o.a < o.b;
}

partial struct Cell<T> {
    public T V;
    public bool Valid;
}

public static (any, error) Value<T>(this Cell<T> c) {
    if (!c.Valid) {
        return (default!, default!);
    }
    return ((c.V).OrTypedNilOf(), default!);
}

partial interface valuer {
    (any, error) Value();
}

partial interface swapper {
    nint Swap(nint v);
}

internal static void @try(@string label, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%-36s PANIC %v\n"u8, label, r);
                }
            }
        }, ref ᒐ);
        fmt.Printf("%-36s %s\n"u8, label, f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string show(slice<reflectꓸValue> results) {
    @string s = "["u8;
    foreach (var (i, r) in results) {
        if (i > 0) {
            s += " "u8;
        }
        s += fmt.Sprint(r.Interface());
    }
    return s + "]"u8;
}

internal static @string names(reflectꓸType t) {
    @string s = fmt.Sprint(t.NumMethod(), (@string)":"u8);
    for (nint i = 0; i < t.NumMethod(); i++) {
        s += " "u8 + t.Method(i).Name;
    }
    return s;
}

internal static slice<reflectꓸValue> args(params ꓸꓸꓸany valuesʗp) {
    var values = valuesʗp.sslice();

    slice<reflectꓸValue> @in = default!;
    foreach (var (_, a) in values) {
        @in = append(@in, reflect.ValueOf(a));
    }
    return @in;
}

internal static void probe(@string name, any value, any pointer, map<@string, slice<any>> @in) {
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = reflect.ValueOf(value);
    ref var pv = ref heap<reflectꓸValue>(out var Ꮡpv);
    pv = reflect.ValueOf(pointer);

    var vʗ1 = v;
    @try(name + " value methods"u8, () => names(vʗ1.Type()));
    var pvʗ1 = pv;
    @try(name + " pointer methods"u8, () => names(pvʗ1.Type()));

    foreach (var (i, n) in goSets[name, () => new array<slice<@string>>(2)][0]) {
        var inʗ1 = @in;
        var vʗ2 = v;
        @try(fmt.Sprintf("%s v.Method(%d)"u8, name, i), () => n + " "u8 + show(vʗ2.Method(i).Call(args(inʗ1[n].ꓸꓸꓸ))));
        var inʗ2 = @in;
        var vʗ3 = v;
        @try(fmt.Sprintf("%s T.Method(%d).Func"u8, name, i), () => {
            var m = vʗ3.Type().Method(i);
            return m.Name + " "u8 + show(m.Func.Call(appendꓸꓸꓸ(new reflectꓸValue[]{vʗ3}.slice(), args(inʗ2[n].ꓸꓸꓸ))));
        });
        var inʗ3 = @in;
        var vʗ4 = v;
        @try(fmt.Sprintf("%s v.MethodByName(%s)"u8, name, n), () => show(vʗ4.MethodByName(n).Call(args(inʗ3[n].ꓸꓸꓸ))));
    }

    foreach (var (i, n) in goSets[name, () => new array<slice<@string>>(2)][1]) {
        var inʗ4 = @in;
        var pvʗ2 = pv;
        @try(fmt.Sprintf("%s pv.Method(%d)"u8, name, i), () => n + " "u8 + show(pvʗ2.Method(i).Call(args(inʗ4[n].ꓸꓸꓸ))));
        var inʗ5 = @in;
        var pvʗ3 = pv;
        @try(fmt.Sprintf("%s *T.Method(%d).Func"u8, name, i), () => {
            var m = pvʗ3.Type().Method(i);
            return m.Name + " "u8 + show(m.Func.Call(appendꓸꓸꓸ(new reflectꓸValue[]{pvʗ3}.slice(), args(inʗ5[n].ꓸꓸꓸ))));
        });
    }

    var pvʗ4 = pv;
    @try(name + " after pointer calls"u8, () => fmt.Sprint(pvʗ4.Elem().Interface()));
}

internal static map<@string, array<slice<@string>>> goSets = new map<@string, array<slice<@string>>>{
    ["Plain"u8] = new slice<@string>[]{new @string[]{"Describe"u8, "Get"u8}.slice(), new @string[]{"Describe"u8, "Get"u8, "Set"u8}.slice()}.array(),
    ["Box[int]"u8] = new slice<@string>[]{new @string[]{"Describe"u8, "Get"u8}.slice(), new @string[]{"Describe"u8, "Get"u8, "Set"u8}.slice()}.array(),
    ["Box[string]"u8] = new slice<@string>[]{new @string[]{"Describe"u8, "Get"u8}.slice(), new @string[]{"Describe"u8, "Get"u8, "Set"u8}.slice()}.array(),
    ["Box[[]int]"u8] = new slice<@string>[]{new @string[]{"Describe"u8, "Get"u8}.slice(), new @string[]{"Describe"u8, "Get"u8, "Set"u8}.slice()}.array(),
    ["Pair[string,int]"u8] = new slice<@string>[]{new @string[]{"Key"u8, "String"u8}.slice(), new @string[]{"Key"u8, "String"u8, "Swap"u8}.slice()}.array(),
    ["Ord2[age]"u8] = new slice<@string>[]{new @string[]{"Less"u8}.slice(), new @string[]{"Less"u8}.slice()}.array()
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string plainˢ = "Plain"u8;
private static readonly @string boxIntˢ = "Box[int]"u8;
private static readonly @string boxStringˢ = "Box[string]"u8;
private static readonly @string boxIntˢ2 = "Box[[]int]"u8;
private static readonly @string pairStringIntˢ = "Pair[string,int]"u8;
private static readonly @string ord2Ageˢ = "Ord2[age]"u8;
private static readonly @string fmtGenericStringerˢ = "fmt generic Stringer"u8;
private static readonly @string fmtGenericStringerˢ2 = "fmt *generic Stringer"u8;
private static readonly @string fmtNonGenericStringerˢ = "fmt non-generic Stringer"u8;
private static readonly @string assertGenericToStringerˢ = "assert generic to Stringer"u8;
private static readonly @string falseˢ = "false"u8;
private static readonly @string assertGenericToStringerˢ2 = "assert *generic to Stringer"u8;
private static readonly @string assertGenericToSwapperˢ = "assert generic to swapper"u8;
private static readonly @string assertGenericToSwapperˢ2 = "assert *generic to swapper"u8;
private static readonly @string assertGenericCellToˢ = "assert generic Cell to valuer"u8;

internal static void Main() {
    probe(plainˢ, new Plain(v: 7), Ꮡ(new Plain(v: 1)), new map<@string, slice<any>>{["Describe"u8] = new any[]{(@string)"p="u8}.slice(), ["Set"u8] = new any[]{(nint)(9)}.slice()});
    probe(boxIntˢ, new Box<nint>(v: 7), Ꮡ(new Box<nint>(v: 1)), new map<@string, slice<any>>{["Describe"u8] = new any[]{(@string)"b="u8}.slice(), ["Set"u8] = new any[]{(nint)(9)}.slice()});
    probe(boxStringˢ, new Box<@string>(v: "s"u8), Ꮡ(new Box<@string>(v: "t"u8)), new map<@string, slice<any>>{["Describe"u8] = new any[]{(@string)"b="u8}.slice(), ["Set"u8] = new any[]{(@string)"u"u8}.slice()});
    probe(boxIntˢ2, new Box<slice<nint>>(v: new nint[]{1}.slice()), Ꮡ(new Box<slice<nint>>(v: new nint[]{2}.slice())), new map<@string, slice<any>>{["Describe"u8] = new any[]{(@string)"b="u8}.slice(), ["Set"u8] = new any[]{new nint[]{3, 4}.slice()}.slice()});
    probe(pairStringIntˢ, new Pair<@string, nint>(k: "a"u8, v: 1), Ꮡ(new Pair<@string, nint>(k: "b"u8, v: 2)), new map<@string, slice<any>>{["Swap"u8] = new any[]{(nint)(5)}.slice()});
    probe(ord2Ageˢ, new Ord2<age>(a: 1, b: 2), Ꮡ(new Ord2<age>(a: 3, b: 2)), default!);

    @try(fmtGenericStringerˢ, () => fmt.Sprint(new Pair<@string, nint>(k: "z"u8, v: 26)));
    @try(fmtGenericStringerˢ2, () => fmt.Sprint(Ꮡ(new Pair<@string, nint>(k: "y"u8, v: 25))));
    @try(fmtNonGenericStringerˢ, () => fmt.Sprint(new Pair<@string, nint>(k: "x"u8, v: 24).String()));

    @try(assertGenericToStringerˢ, () => {
        var (s, ok) = ((any)new Pair<@string, nint>(k: "z"u8, v: 26))._<fmt.Stringer>(ᐧ);
        if (!ok) {
            return falseˢ;
        }
        return "true "u8 + s.String();
    });
    @try(assertGenericToStringerˢ2, () => {
        var (s, ok) = ((any)(Ꮡ(new Pair<@string, nint>(k: "z"u8, v: 26))))._<fmt.Stringer>(ᐧ);
        if (!ok) {
            return falseˢ;
        }
        return "true "u8 + s.String();
    });
    @try(assertGenericToSwapperˢ, () => {
        var (_, ok) = ((any)new Pair<@string, nint>(k: "z"u8, v: 26))._<swapper>(ᐧ);
        return fmt.Sprint(ok);
    });
    @try(assertGenericToSwapperˢ2, () => {
        var p = Ꮡ(new Pair<@string, nint>(k: "w"u8, v: 1));
        var (s, ok) = ((any)p.OrTypedNil())._<swapper>(ᐧ);
        if (!ok) {
            return falseˢ;
        }
        nint old = s.Swap(2);
        return fmt.Sprint(ok, (@string)" "u8, old, (@string)" "u8, (~p).v);
    });
    @try(assertGenericCellToˢ, () => {
        var (c, ok) = ((any)new Cell<nuint>(V: 1, Valid: true))._<valuer>(ᐧ);
        if (!ok) {
            return falseˢ;
        }
        var (v, err) = c.Value();
        return fmt.Sprint(ok, (@string)" "u8, v, (@string)" "u8, err);
    });
}

} // end main_package
