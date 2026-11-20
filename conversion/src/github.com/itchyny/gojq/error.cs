namespace go.github.com.itchyny;

using strconv = strconv_package;
using static gojq_package;

partial class gojq_package {

partial interface ValueError :
    error
{
    any Value();
}

partial struct expectedObjectError {
    internal any v;
}

} partial class gojqᴛexpectedObjectError {

internal static @string Error(this ref expectedObjectError err) {
    return "expected an object but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct expectedArrayError {
    internal any v;
}

} partial class gojqᴛexpectedArrayError {

internal static @string Error(this ref expectedArrayError err) {
    return "expected an array but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct iteratorError {
    internal any v;
}

} partial class gojqᴛiteratorError {

internal static @string Error(this ref iteratorError err) {
    return "cannot iterate over: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct arrayIndexNegativeError {
    internal nint v;
}

} partial class gojqᴛarrayIndexNegativeError {

internal static @string Error(this ref arrayIndexNegativeError err) {
    return "array index should not be negative: "u8 + Preview(err.v);
}

} partial class gojq_package {

partial struct arrayIndexTooLargeError {
    internal any v;
}

} partial class gojqᴛarrayIndexTooLargeError {

internal static @string Error(this ref arrayIndexTooLargeError err) {
    return "array index too large: "u8 + Preview(err.v);
}

} partial class gojq_package {

partial struct repeatStringTooLargeError {
    internal @string s;
    internal float64 n;
}

} partial class gojqᴛrepeatStringTooLargeError {

internal static @string Error(this ref repeatStringTooLargeError err) {
    return "repeat string result too large: "u8 + Preview(err.s) + " * "u8 + Preview(err.n);
}

} partial class gojq_package {

partial struct objectKeyNotStringError {
    internal any v;
}

} partial class gojqᴛobjectKeyNotStringError {

internal static @string Error(this ref objectKeyNotStringError err) {
    return "expected a string for object key but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct arrayIndexNotNumberError {
    internal any v;
}

} partial class gojqᴛarrayIndexNotNumberError {

internal static @string Error(this ref arrayIndexNotNumberError err) {
    return "expected a number for indexing an array but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct stringIndexNotNumberError {
    internal any v;
}

} partial class gojqᴛstringIndexNotNumberError {

internal static @string Error(this ref stringIndexNotNumberError err) {
    return "expected a number for indexing a string but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct expectedStartEndError {
    internal any v;
}

} partial class gojqᴛexpectedStartEndError {

internal static @string Error(this ref expectedStartEndError err) {
    return @"expected ""start"" and ""end"" for slicing but got: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct lengthMismatchError {
}

} partial class gojqᴛlengthMismatchError {

internal static @string Error(this ref lengthMismatchError _) {
    return "length mismatch";
}

} partial class gojq_package {

partial struct inputNotAllowedError {
}

} partial class gojqᴛinputNotAllowedError {

internal static @string Error(this ref inputNotAllowedError _) {
    return "input(s)/0 is not allowed";
}

} partial class gojq_package {

partial struct funcNotFoundError {
    internal ж<Func> f;
}

} partial class gojqᴛfuncNotFoundError {

internal static @string Error(this ref funcNotFoundError err) {
    return "function not defined: "u8 + err.f.Name + "/"u8 + strconv.Itoa(len(err.f.Args));
}

} partial class gojq_package {

partial struct func0TypeError {
    internal @string name;
    internal any v;
}

} partial class gojqᴛfunc0TypeError {

internal static @string Error(this ref func0TypeError err) {
    return err.name + " cannot be applied to: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct func1TypeError {
    internal @string name;
    internal any v, w;
}

} partial class gojqᴛfunc1TypeError {

internal static @string Error(this ref func1TypeError err) {
    return err.name + "("u8 + Preview(err.w) + ") cannot be applied to: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct func2TypeError {
    internal @string name;
    internal any v, w, x;
}

} partial class gojqᴛfunc2TypeError {

internal static @string Error(this ref func2TypeError err) {
    return err.name + "("u8 + Preview(err.w) + "; "u8 + Preview(err.x) + ") cannot be applied to: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct func0WrapError {
    internal @string name;
    internal any v;
    internal error err;
}

} partial class gojqᴛfunc0WrapError {

internal static @string Error(this ref func0WrapError err) {
    return err.name + " cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

} partial class gojq_package {

partial struct func1WrapError {
    internal @string name;
    internal any v, w;
    internal error err;
}

} partial class gojqᴛfunc1WrapError {

internal static @string Error(this ref func1WrapError err) {
    return err.name + "("u8 + Preview(err.w) + ") cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

} partial class gojq_package {

partial struct func2WrapError {
    internal @string name;
    internal any v, w, x;
    internal error err;
}

} partial class gojqᴛfunc2WrapError {

internal static @string Error(this ref func2WrapError err) {
    return err.name + "("u8 + Preview(err.w) + "; "u8 + Preview(err.x) + ") cannot be applied to "u8 + Preview(err.v) + ": "u8 + err.err.Error();
}

} partial class gojq_package {

public partial struct exitCodeError {
    internal any value;
    internal nint code;
}

} partial class gojqᴛexitCodeError {

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

} partial class gojq_package {

partial struct HaltError /*exitCodeError*/;

} partial class gojqᴛHaltError {

public static @string Error(this ж<HaltError> Ꮡerr) {
    return "halt "u8 + (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).Error();
}

public static any Value(this ж<HaltError> Ꮡerr) {
    return (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).Value();
}

public static nint ExitCode(this ж<HaltError> Ꮡerr) {
    return (Ꮡerr.Reinterpret<HaltError, exitCodeError>()).ExitCode();
}

} partial class gojq_package {

partial struct flattenDepthError {
    internal float64 v;
}

} partial class gojqᴛflattenDepthError {

internal static @string Error(this ref flattenDepthError err) {
    return "flatten depth should not be negative: "u8 + Preview(err.v);
}

} partial class gojq_package {

partial struct timeArrayError {
}

} partial class gojqᴛtimeArrayError {

internal static @string Error(this ref timeArrayError _) {
    return "expected an array of 8 numbers";
}

} partial class gojq_package {

partial struct unaryTypeError {
    internal @string name;
    internal any v;
}

} partial class gojqᴛunaryTypeError {

internal static @string Error(this ref unaryTypeError err) {
    return "cannot "u8 + err.name + ": "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct binopTypeError {
    internal @string name;
    internal any l, r;
}

} partial class gojqᴛbinopTypeError {

internal static @string Error(this ref binopTypeError err) {
    return "cannot "u8 + err.name + ": "u8 + typeErrorPreview(err.l) + " and "u8 + typeErrorPreview(err.r);
}

} partial class gojq_package {

partial struct zeroDivisionError {
    internal any l, r;
}

} partial class gojqᴛzeroDivisionError {

internal static @string Error(this ref zeroDivisionError err) {
    return "cannot divide "u8 + typeErrorPreview(err.l) + " by: "u8 + typeErrorPreview(err.r);
}

} partial class gojq_package {

partial struct zeroModuloError {
    internal any l, r;
}

} partial class gojqᴛzeroModuloError {

internal static @string Error(this ref zeroModuloError err) {
    return "cannot modulo "u8 + typeErrorPreview(err.l) + " by: "u8 + typeErrorPreview(err.r);
}

} partial class gojq_package {

partial struct formatNotFoundError {
    internal @string n;
}

} partial class gojqᴛformatNotFoundError {

internal static @string Error(this ref formatNotFoundError err) {
    return "format not defined: "u8 + err.n;
}

} partial class gojq_package {

partial struct formatRowError {
    internal @string typ;
    internal any v;
}

} partial class gojqᴛformatRowError {

internal static @string Error(this ref formatRowError err) {
    return "@"u8 + err.typ + " cannot format an array including: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct tooManyVariableValuesError {
}

} partial class gojqᴛtooManyVariableValuesError {

internal static @string Error(this ref tooManyVariableValuesError _) {
    return "too many variable values provided";
}

} partial class gojq_package {

partial struct expectedVariableError {
    internal @string n;
}

} partial class gojqᴛexpectedVariableError {

internal static @string Error(this ref expectedVariableError err) {
    return "variable defined but not bound: "u8 + err.n;
}

} partial class gojq_package {

partial struct variableNotFoundError {
    internal @string n;
}

} partial class gojqᴛvariableNotFoundError {

internal static @string Error(this ref variableNotFoundError err) {
    return "variable not defined: "u8 + err.n;
}

} partial class gojq_package {

partial struct variableNameError {
    internal @string n;
}

} partial class gojqᴛvariableNameError {

internal static @string Error(this ref variableNameError err) {
    return "invalid variable name: "u8 + err.n;
}

} partial class gojq_package {

partial struct breakError {
    internal @string n;
    internal any v;
}

} partial class gojqᴛbreakError {

internal static @string Error(this ref breakError err) {
    return "label not defined: "u8 + err.n;
}

internal static nint ExitCode(this ref breakError _) {
    return 3;
}

} partial class gojq_package {

partial struct tryEndError {
    internal error err;
}

} partial class gojqᴛtryEndError {

internal static @string Error(this ref tryEndError err) {
    return err.err.Error();
}

} partial class gojq_package {

partial struct invalidPathError {
    internal any v;
}

} partial class gojqᴛinvalidPathError {

internal static @string Error(this ref invalidPathError err) {
    return "invalid path against: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct invalidPathIterError {
    internal any v;
}

} partial class gojqᴛinvalidPathIterError {

internal static @string Error(this ref invalidPathIterError err) {
    return "invalid path on iterating against: "u8 + typeErrorPreview(err.v);
}

} partial class gojq_package {

partial struct queryParseError {
    internal @string fname, contents;
    internal error err;
}

} partial class gojqᴛqueryParseError {

internal static (@string, @string, error) QueryParseError(this ref queryParseError err) {
    return (err.fname, err.contents, err.err);
}

internal static @string Error(this ref queryParseError err) {
    return "invalid query: "u8 + err.fname + ": "u8 + err.err.Error();
}

} partial class gojq_package {

partial struct jsonParseError {
    internal @string fname, contents;
    internal error err;
}

} partial class gojqᴛjsonParseError {

internal static (@string, @string, error) JSONParseError(this ref jsonParseError err) {
    return (err.fname, err.contents, err.err);
}

internal static @string Error(this ref jsonParseError err) {
    return "invalid json: "u8 + err.fname + ": "u8 + err.err.Error();
}

} partial class gojq_package {

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
