namespace go.github.com.itchyny;

using strconv = strconv_package;

partial class gojq_package {

partial interface ValueError :
    error
{
    any Value();
}

partial struct expectedObjectError {
    internal any v;
}

internal static @string Error(this ref expectedObjectError err) {
    return "expected an object but got: "u8 + typeErrorPreview(err.v);
}

partial struct expectedArrayError {
    internal any v;
}

internal static @string Error(this ref expectedArrayError err) {
    return "expected an array but got: "u8 + typeErrorPreview(err.v);
}

partial struct iteratorError {
    internal any v;
}

internal static @string Error(this ref iteratorError err) {
    return "cannot iterate over: "u8 + typeErrorPreview(err.v);
}

partial struct arrayIndexNegativeError {
    internal nint v;
}

internal static @string Error(this ref arrayIndexNegativeError err) {
    return "array index should not be negative: "u8 + Preview(err.v);
}

partial struct arrayIndexTooLargeError {
    internal any v;
}

internal static @string Error(this ref arrayIndexTooLargeError err) {
    return "array index too large: "u8 + Preview(err.v);
}

partial struct repeatStringTooLargeError {
    internal @string s;
    internal float64 n;
}

internal static @string Error(this ref repeatStringTooLargeError err) {
    return "repeat string result too large: "u8 + Preview(err.s) + " * "u8 + Preview(err.n);
}

partial struct objectKeyNotStringError {
    internal any v;
}

internal static @string Error(this ref objectKeyNotStringError err) {
    return "expected a string for object key but got: "u8 + typeErrorPreview(err.v);
}

partial struct arrayIndexNotNumberError {
    internal any v;
}

internal static @string Error(this ref arrayIndexNotNumberError err) {
    return "expected a number for indexing an array but got: "u8 + typeErrorPreview(err.v);
}

partial struct stringIndexNotNumberError {
    internal any v;
}

internal static @string Error(this ref stringIndexNotNumberError err) {
    return "expected a number for indexing a string but got: "u8 + typeErrorPreview(err.v);
}

partial struct expectedStartEndError {
    internal any v;
}

internal static @string Error(this ref expectedStartEndError err) {
    return @"expected ""start"" and ""end"" for slicing but got: "u8 + typeErrorPreview(err.v);
}

partial struct lengthMismatchError {
}

internal static @string Error(this ref lengthMismatchError _) {
    return "length mismatch";
}

partial struct inputNotAllowedError {
}

internal static @string Error(this ref inputNotAllowedError _) {
    return "input(s)/0 is not allowed";
}

partial struct funcNotFoundError {
    internal ж<Func> f;
}

internal static @string Error(this ref funcNotFoundError err) {
    return "function not defined: "u8 + err.f.Name + "/"u8 + strconv.Itoa(len(err.f.Args));
}

partial struct func0TypeError {
    internal @string name;
    internal any v;
}

internal static @string Error(this ref func0TypeError err) {
    return err.name + " cannot be applied to: "u8 + typeErrorPreview(err.v);
}

partial struct func1TypeError {
    internal @string name;
    internal any v, w;
}

internal static @string Error(this ref func1TypeError err) {
    return err.name + "("u8 + Preview(err.w) + ") cannot be applied to: "u8 + typeErrorPreview(err.v);
}

partial struct func2TypeError {
    internal @string name;
    internal any v, w, x;
}

internal static @string Error(this ref func2TypeError err) {
    return err.name + "("u8 + Preview(err.w) + "; "u8 + Preview(err.x) + ") cannot be applied to: "u8 + typeErrorPreview(err.v);
}

partial struct func0WrapError {
    internal @string name;
    internal any v;
    internal error err;
}

internal static @string Error(this ref func0WrapError err) {
    return err.name + " cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

partial struct func1WrapError {
    internal @string name;
    internal any v, w;
    internal error err;
}

internal static @string Error(this ref func1WrapError err) {
    return err.name + "("u8 + Preview(err.w) + ") cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

partial struct func2WrapError {
    internal @string name;
    internal any v, w, x;
    internal error err;
}

internal static @string Error(this ref func2WrapError err) {
    return err.name + "("u8 + Preview(err.w) + "; "u8 + Preview(err.x) + ") cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

public partial struct exitCodeError {
    internal any value;
    internal nint code;
}

public static @string Error(this ref exitCodeError err) {
    var (s, ok) = err.value._<@string>(ᐧ); if (ok) {
        return "error: "u8 + s;
    }
    return "error: "u8 + jsonMarshal(err.value);
}

public static any Value(this ref exitCodeError err) {
    return err.value;
}

public static nint ExitCode(this ref exitCodeError err) {
    return err.code;
}

partial struct HaltError /*exitCodeError*/;

public static @string Error(this ж<HaltError> Ꮡerr) {
    return "halt "u8 + (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).Error();
}

public static any Value(this ж<HaltError> Ꮡerr) {
    return (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).Value();
}

public static nint ExitCode(this ж<HaltError> Ꮡerr) {
    return (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).ExitCode();
}

partial struct flattenDepthError {
    internal float64 v;
}

internal static @string Error(this ref flattenDepthError err) {
    return "flatten depth should not be negative: "u8 + Preview(err.v);
}

partial struct timeArrayError {
}

internal static @string Error(this ref timeArrayError _) {
    return "expected an array of 8 numbers";
}

partial struct unaryTypeError {
    internal @string name;
    internal any v;
}

internal static @string Error(this ref unaryTypeError err) {
    return "cannot "u8 + err.name + ": "u8 + typeErrorPreview(err.v);
}

partial struct binopTypeError {
    internal @string name;
    internal any l, r;
}

internal static @string Error(this ref binopTypeError err) {
    return "cannot "u8 + err.name + ": "u8 + typeErrorPreview(err.l) + " and "u8 + typeErrorPreview(err.r);
}

partial struct zeroDivisionError {
    internal any l, r;
}

internal static @string Error(this ref zeroDivisionError err) {
    return "cannot divide "u8 + typeErrorPreview(err.l) + " by: "u8 + typeErrorPreview(err.r);
}

partial struct zeroModuloError {
    internal any l, r;
}

internal static @string Error(this ref zeroModuloError err) {
    return "cannot modulo "u8 + typeErrorPreview(err.l) + " by: "u8 + typeErrorPreview(err.r);
}

partial struct formatNotFoundError {
    internal @string n;
}

internal static @string Error(this ref formatNotFoundError err) {
    return "format not defined: "u8 + err.n;
}

partial struct formatRowError {
    internal @string typ;
    internal any v;
}

internal static @string Error(this ref formatRowError err) {
    return "@"u8 + err.typ + " cannot format an array including: "u8 + typeErrorPreview(err.v);
}

partial struct tooManyVariableValuesError {
}

internal static @string Error(this ref tooManyVariableValuesError _) {
    return "too many variable values provided";
}

partial struct expectedVariableError {
    internal @string n;
}

internal static @string Error(this ref expectedVariableError err) {
    return "variable defined but not bound: "u8 + err.n;
}

partial struct variableNotFoundError {
    internal @string n;
}

internal static @string Error(this ref variableNotFoundError err) {
    return "variable not defined: "u8 + err.n;
}

partial struct variableNameError {
    internal @string n;
}

internal static @string Error(this ref variableNameError err) {
    return "invalid variable name: "u8 + err.n;
}

partial struct breakError {
    internal @string n;
    internal any v;
}

internal static @string Error(this ref breakError err) {
    return "label not defined: "u8 + err.n;
}

internal static nint ExitCode(this ref breakError _) {
    return 3;
}

partial struct tryEndError {
    internal error err;
}

internal static @string Error(this ref tryEndError err) {
    return err.err.Error();
}

partial struct invalidPathError {
    internal any v;
}

internal static @string Error(this ref invalidPathError err) {
    return "invalid path against: "u8 + typeErrorPreview(err.v);
}

partial struct invalidPathIterError {
    internal any v;
}

internal static @string Error(this ref invalidPathIterError err) {
    return "invalid path on iterating against: "u8 + typeErrorPreview(err.v);
}

partial struct queryParseError {
    internal @string fname, contents;
    internal error err;
}

internal static (@string, @string, error) QueryParseError(this ref queryParseError err) {
    return (err.fname, err.contents, err.err);
}

internal static @string Error(this ref queryParseError err) {
    return "invalid query: "u8 + err.fname + ": "u8 + err.err.Error();
}

partial struct jsonParseError {
    internal @string fname, contents;
    internal error err;
}

internal static (@string, @string, error) JSONParseError(this ref jsonParseError err) {
    return (err.fname, err.contents, err.err);
}

internal static @string Error(this ref jsonParseError err) {
    return "invalid json: "u8 + err.fname + ": "u8 + err.err.Error();
}

internal static @string typeErrorPreview(any v) {
    switch (v.type()) {
    case null: {
        return "null";
    }
    case {} ᴛ0 when ᴛ0._<Iter>(out var _): {
        return "gojq.Iter";
    }
    default: {
        return TypeOf(v) + " ("u8 + Preview(v) + ")"u8;
    }}

}

} // end gojq_package
