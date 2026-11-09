namespace go.github.com.itchyny;

using json = encoding.json_package;
using fmt = fmt_package;
using big = go.math.big_package;
using encoding;
using go.math;

partial class gojq_package {

public static @string TypeOf(any v) {
    switch (v.type()) {
    case null: {
        return "null";
    }
    case bool: {
        return "boolean";
    }
    case nint _:
    case float64 _:
    case ж<bigꓸInt> _:
    case json.Number _: {
        return "number";
    }
    case @string: {
        return "string";
    }
    case slice<any>: {
        return "array";
    }
    case map<@string, any>: {
        return "object";
    }
    default: {
        throw panic(fmt.Sprintf("invalid type: %[1]T (%[1]v)"u8, v));
        break;
    }}

}

} // end gojq_package
