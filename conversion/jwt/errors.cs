namespace go.github.com.golang_jwt.jwt;

using errors = errors_package;
using fmt = fmt_package;
using strings = strings_package;
using ꓸꓸꓸerror = Span<error>;

partial class jwt_package {

public static error ErrInvalidKey = errors.New("key is invalid");
public static error ErrInvalidKeyType = errors.New("key is of invalid type");
public static error ErrHashUnavailable = errors.New("the requested hash function is unavailable");
public static error ErrTokenMalformed = errors.New("token is malformed");
public static error ErrTokenUnverifiable = errors.New("token is unverifiable");
public static error ErrTokenSignatureInvalid = errors.New("token signature is invalid");
public static error ErrTokenRequiredClaimMissing = errors.New("token is missing required claim");
public static error ErrTokenInvalidAudience = errors.New("token has invalid audience");
public static error ErrTokenExpired = errors.New("token is expired");
public static error ErrTokenUsedBeforeIssued = errors.New("token used before issued");
public static error ErrTokenInvalidIssuer = errors.New("token has invalid issuer");
public static error ErrTokenInvalidSubject = errors.New("token has invalid subject");
public static error ErrTokenNotValidYet = errors.New("token is not valid yet");
public static error ErrTokenInvalidId = errors.New("token has invalid id");
public static error ErrTokenInvalidClaims = errors.New("token has invalid claims");
public static error ErrInvalidType = errors.New("invalid type for claim");

partial struct joinedError {
    internal slice<error> errs;
}

internal static @string Error(this joinedError je) {
    var msgᴷ = new stackAppend<@string>();

    var msg = new @string[]{}.slice();
    foreach (var (_, err) in je.errs) {
        msg = append(msgᴷ, msg, err.Error());
    }

    return strings.Join(msg, ", ");
}

internal static error joinErrors(params ꓸꓸꓸerror errsʗp) {
    var errs = errsʗp.slice();

    return new joinedErrorжerror(Ꮡ(new joinedError(
        errs: errs
    )));
}

internal static slice<error> Unwrap(this joinedError je) {
    return je.errs;
}

internal static error newError(@string message, error err, params ꓸꓸꓸerror moreʗp) {
    var more = moreʗp.sslice();
    var argsᴷ = new stackAppend<any>();

    @string format = default!;
    slice<any> args = default!;
    if (message != ""u8){
        format = "%w: %s";
        args = new any[]{err, message}.slice();
    } else {
        format = "%w";
        args = new any[]{err}.slice();
    }

    foreach (var (_, e) in more) {
        format += ": %w"u8;
        args = append(argsᴷ, args, (any)(e));
    }

    err = fmt.Errorf(format, args.ꓸꓸꓸ);
    return err;
}

} // end jwt_package
