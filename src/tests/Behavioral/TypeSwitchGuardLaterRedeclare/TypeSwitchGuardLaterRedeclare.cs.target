namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

internal static @string kind(error err, any expected) {
    switch (expected.type()) {
    case Func<error, bool> expectedTypeΔ1: {
        return fmt.Sprint("func:", expectedTypeΔ1(err));
    }
    case {} ΔexpectedTypeΔ1 when ΔexpectedTypeΔ1._<reflectꓸType>(out var expectedTypeΔ1): {
        return "type:"u8 + expectedTypeΔ1.Kind().String();
    }
    case null: {
        return "nil";
    }}

    var expectedType = reflect.TypeOf(expected);
    return "after:"u8 + expectedType.Kind().String();
}

partial struct Result /*@string*/;

internal static Func<Result> errorType(error err, any expected) {
    return () => {
        switch (expected.type()) {
        case Func<error, bool> expectedTypeΔ1: {
            return (Result)fmt.Sprint("func:", expectedTypeΔ1(err));
        }
        case {} ΔexpectedTypeΔ1 when ΔexpectedTypeΔ1._<reflectꓸType>(out var expectedTypeΔ1): {
            return (Result)("type:"u8 + expectedTypeΔ1.Kind().String());
        }}

        var expectedType = reflect.TypeOf(expected);
        return (Result)("after:"u8 + expectedType.Kind().String());
    };
}

internal static @string describe(any v, bool nested) {
    if (nested) {
        switch (v.type()) {
        case nint _:
        case int64 _: {
            var xΔ1 = v;
            return fmt.Sprintf("nested-num:%v"u8, xΔ1);
        }
        default: {
            var xΔ1 = v;
            _ = xΔ1;
            break;
        }}

        @string xΔ2 = fmt.Sprintf("nested-after:%T"u8, v);
        return xΔ2;
    }

    switch (v.type()) {
    case @string xΔ3: {
        return "str:"u8 + xΔ3;
    }
    default: {
        var xΔ3 = v;
        return fmt.Sprintf("default:%v"u8, xΔ3);
    }
    case bool xΔ3: {
        break;
    }}

    @string x = "bool-after";
    return x;
}

internal static void main() {
    var e = errors.New("x");
    fmt.Println(kind(e, bool (error _) => true));
    fmt.Println(kind(e, reflect.TypeOf((nint)1)));
    fmt.Println(kind(e, default));
    fmt.Println(kind(e, new EmptyStruct()));

    var eʗ1 = e;
    fmt.Println(errorType(e, bool (error err) => AreEqual(err, eʗ1))());
    fmt.Println(errorType(e, reflect.TypeOf(""))());
    fmt.Println(errorType(e, 2.5D)());

    fmt.Println(describe((nint)7, true));
    fmt.Println(describe("s", true));
    fmt.Println(describe("s", false));
    fmt.Println(describe(1.5D, false));
    fmt.Println(describe(true, false));
}

} // end main_package
