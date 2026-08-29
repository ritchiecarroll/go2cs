namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸfmt() {
    builtin.initPackage(typeof(fmt_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸreflect() {
    builtin.initPackage(typeof(reflect_package));
}

[GoType] partial struct filler {
}

internal static void FillPtr(this filler _, nint i, [GoArrayDims(3)] ж<array<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply[0] = i;
    reply[2] = i * 2;
}

internal static nint SumArray(this filler _, [GoArrayDims(4)] array<nint> @in) {
    @in = @in.Clone();

    nint total = 0;
    foreach (var (_, v) in @in) {
        total += v;
    }
    return total;
}

[GoType] partial struct wrap {
    public array<byte> Buf = new(8);
}

internal static nint declared([GoArrayDims(16)] array<byte> @in) {
    @in = @in.Clone();

    return len(@in);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string f32ˢ = "f32"u8;
private static readonly @string f64ˢ = "f64"u8;
private static readonly @string nestedˢ = "nested"u8;
private static readonly @string plainˢ = "plain"u8;
private static readonly @string declaredˢ = "declared"u8;
private static readonly object distinctIn0Typesˢ = (@string)"distinct in0 types:"u8;
private static readonly object sameAsItselfˢ = (@string)"same as itself:    "u8;
private static readonly object nestedElemˢ = (@string)"nested elem:"u8;
private static readonly object structFieldˢ = (@string)"struct field:"u8;
private static readonly object generatedCallˢ = (@string)"generated call:"u8;

internal static void Main() {
    var f32 = ([GoArrayDims(32)] array<byte> @in) => {
        @in = @in.Clone();
        return len(@in) == 32;
    };
    var f64 = ([GoArrayDims(64)] array<byte> @in, wrap w) => {
        @in = @in.Clone();
        w = w.ΔClone();
        return len(@in) + len(w.Buf);
    };
    var nested = ([GoArrayDims(2, 3)] array<array<nint>> @in) => {
        @in = @in.Clone();
        return len(@in) * len(@in[0]);
    };
    var plain = (nint a, slice<byte> s) => a + len(s);
    report(f32ˢ, reflect.TypeOf(f32.OrTypedNilFunc()));
    report(f64ˢ, reflect.TypeOf(f64.OrTypedNilFunc()));
    report(nestedˢ, reflect.TypeOf(nested.OrTypedNilFunc()));
    report(plainˢ, reflect.TypeOf(plain.OrTypedNilFunc()));
    report(declaredˢ, reflect.TypeOf(declared));
    fmt.Println(distinctIn0Typesˢ, !AreEqual(reflect.TypeOf(f32.OrTypedNilFunc()).In(0), reflect.TypeOf(f64.OrTypedNilFunc()).In(0)));
    fmt.Println(sameAsItselfˢ, AreEqual(reflect.TypeOf(f32.OrTypedNilFunc()).In(0), reflect.TypeOf(f32.OrTypedNilFunc()).In(0)));
    var inner = reflect.TypeOf(nested.OrTypedNilFunc()).In(0).Elem();
    fmt.Println(nestedElemˢ, inner, inner.Len());
    var field = reflect.TypeOf(f64.OrTypedNilFunc()).In(1).Field(0);
    fmt.Println(structFieldˢ, field.Name, field.Type, field.Type.Len());
    fmt.Println(generatedCallˢ, generateAndCall(reflect.ValueOf(f32.OrTypedNilFunc())));
    fmt.Println(generatedCallˢ, generateAndCall(reflect.ValueOf(declared)));
    reportMethod();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fillPtrˢ = "FillPtr"u8;
private static readonly object methodPtrParamKindˢ = (@string)"method ptr param: kind ="u8;
private static readonly object elemˢ = (@string)"elem ="u8;
private static readonly object lenˢ = (@string)"len ="u8;
private static readonly object filledThroughThePointerˢ = (@string)"filled through the pointer:"u8;
private static readonly @string sumArrayˢ = "SumArray"u8;
private static readonly object methodValueParamˢ = (@string)"method value param:"u8;
private static readonly object methodSumˢ = (@string)"method sum:"u8;

internal static void reportMethod() {
    var t = reflect.TypeOf(new filler(nil));
    var (m, ok) = t.MethodByName(fillPtrˢ);
    if (!ok) {
        throw panic("FillPtr not found");
    }
    var ptr = m.Type.In(2);
    var elem = ptr.Elem();
    fmt.Println(methodPtrParamKindˢ, ptr.Kind(), elemˢ, elem, lenˢ, elem.Len());
    var reply = reflect.New(elem);
    m.Func.Call(new reflectꓸValue[]{reflect.ValueOf(new filler(nil)), reflect.ValueOf((nint)(7)), reply}.slice());
    fmt.Println(filledThroughThePointerˢ, reply.Elem().Interface());
    (var s, ok) = t.MethodByName(sumArrayˢ);
    if (!ok) {
        throw panic("SumArray not found");
    }
    var @in = s.Type.In(1);
    fmt.Println(methodValueParamˢ, @in, @in.Len());
    var arg = reflect.New(@in).Elem();
    for (nint i = 0; i < arg.Len(); i++) {
        arg.Index(i).SetInt((int64)(i + 1));
    }
    var @out = s.Func.Call(new reflectꓸValue[]{reflect.ValueOf(new filler(nil)), arg}.slice());
    fmt.Println(methodSumˢ, @out[0].Int());
}

internal static void report(@string name, reflectꓸType t) {
    var in0 = t.In(0);
    @string line = fmt.Sprintf("%-9s in0=%-10v kind=%-7v len=%d"u8, name, in0, in0.Kind(), lenOf(in0));
    if (in0.Kind() == reflect.Array) {
        line += fmt.Sprintf(" new=%d zero=%d"u8, reflect.New(in0).Elem().Len(), reflect.Zero(in0).Len());
    }
    fmt.Println(line);
}

internal static nint lenOf(reflectꓸType t) {
    if (t.Kind() == reflect.Array) {
        return t.Len();
    }
    return 0;
}

internal static slice<any> generateAndCall(reflectꓸValue fn) {
    var argType = fn.Type().In(0);
    var arg = reflect.New(argType).Elem();
    for (nint i = 0; i < arg.Len(); i++) {
        arg.Index(i).SetUint((uint64)(i % 251));
    }
    var @out = fn.Call(new reflectꓸValue[]{arg}.slice());
    var results = new slice<any>(len(@out));
    foreach (var (i, r) in @out) {
        results[i] = r.Interface();
    }
    return results;
}

} // end main_package
