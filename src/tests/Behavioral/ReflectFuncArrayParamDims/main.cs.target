namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct filler {
}

internal static void FillPtr(this filler _, nint i, /*[3]*/ ж<array<nint>> Ꮡreply) {
    ref var reply = ref Ꮡreply.DerefOrNull();

    reply[0] = i;
    reply[2] = i * 2;
}

internal static nint SumArray(this filler _, /*[4]*/ array<nint> @in) {
    @in = @in.Clone();

    nint total = 0;
    foreach (var (_, v) in @in.ΔRangeSnapshot()) {
        total += v;
    }
    return total;
}

partial struct wrap {
    public array<byte> Buf = new(8);
}

internal static nint declared(/*[16]*/ array<byte> @in) {
    @in = @in.Clone();

    return len(@in);
}

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

    report("f32", reflect.TypeOf((f32).OrTypedNilFunc()));
    report("f64", reflect.TypeOf((f64).OrTypedNilFunc()));
    report("nested", reflect.TypeOf((nested).OrTypedNilFunc()));
    report("plain", reflect.TypeOf((plain).OrTypedNilFunc()));
    report("declared", reflect.TypeOf(declared));

    fmt.Println("distinct in0 types:", !AreEqual(reflect.TypeOf((f32).OrTypedNilFunc()).In(0), reflect.TypeOf((f64).OrTypedNilFunc()).In(0)));
    fmt.Println("same as itself:    ", AreEqual(reflect.TypeOf((f32).OrTypedNilFunc()).In(0), reflect.TypeOf((f32).OrTypedNilFunc()).In(0)));

    var inner = reflect.TypeOf((nested).OrTypedNilFunc()).In(0).Elem();
    fmt.Println("nested elem:", inner, inner.Len());

    var field = reflect.TypeOf((f64).OrTypedNilFunc()).In(1).Field(0);
    fmt.Println("struct field:", field.Name, field.Type, field.Type.Len());

    fmt.Println("generated call:", generateAndCall(reflect.ValueOf((f32).OrTypedNilFunc())));
    fmt.Println("generated call:", generateAndCall(reflect.ValueOf(declared)));

    reportMethod();
}

internal static void reportMethod() {
    var t = reflect.TypeOf(new filler());

    var (m, ok) = t.MethodByName("FillPtr");
    if (!ok) {
        throw panic("FillPtr not found");
    }

    var ptr = m.Type.In(2);
    var elem = ptr.Elem();
    fmt.Println("method ptr param: kind =", ptr.Kind(), "elem =", elem, "len =", elem.Len());

    var reply = reflect.New(elem);
    m.Func.Call(slice<reflectꓸValue>([reflect.ValueOf(new filler()), reflect.ValueOf((nint)7), reply]));
    fmt.Println("filled through the pointer:", reply.Elem().Interface());

    (var s, ok) = t.MethodByName("SumArray");
    if (!ok) {
        throw panic("SumArray not found");
    }

    var @in = s.Type.In(1);
    fmt.Println("method value param:", @in, @in.Len());

    var arg = reflect.New(@in).Elem();

    for (nint i = 0; i < arg.Len(); i++) {
        arg.Index(i).SetInt((int64)(i + 1));
    }

    var @out = s.Func.Call(slice<reflectꓸValue>([reflect.ValueOf(new filler()), arg]));
    fmt.Println("method sum:", @out[0].Int());
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

    var @out = fn.Call(slice<reflectꓸValue>([arg]));
    var results = new slice<any>(len(@out));

    foreach (var (i, r) in @out) {
        results[i] = r.Interface();
    }

    return results;
}

} // end main_package
