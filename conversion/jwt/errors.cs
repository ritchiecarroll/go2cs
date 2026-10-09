namespace go.github.com.golang_jwt.jwt;

using errors = errors_package;
using fmt = fmt_package;
using strings = strings_package;
using ꓸꓸꓸerror = Span<error>;

partial class jwt_package {

public static error ErrInvalidKey = errors.New("key is invalid"u8);
public static error ErrInvalidKeyType = errors.New("key is of invalid type"u8);
public static error ErrHashUnavailable = errors.New("the requested hash function is unavailable"u8);
public static error ErrTokenMalformed = errors.New("token is malformed"u8);
public static error ErrTokenUnverifiable = errors.New("token is unverifiable"u8);
public static error ErrTokenSignatureInvalid = errors.New("token signature is invalid"u8);
public static error ErrTokenRequiredClaimMissing = errors.New("token is missing required claim"u8);
public static error ErrTokenInvalidAudience = errors.New("token has invalid audience"u8);
public static error ErrTokenExpired = errors.New("token is expired"u8);
public static error ErrTokenUsedBeforeIssued = errors.New("token used before issued"u8);
public static error ErrTokenInvalidIssuer = errors.New("token has invalid issuer"u8);
public static error ErrTokenInvalidSubject = errors.New("token has invalid subject"u8);
public static error ErrTokenNotValidYet = errors.New("token is not valid yet"u8);
public static error ErrTokenInvalidId = errors.New("token has invalid id"u8);
public static error ErrTokenInvalidClaims = errors.New("token has invalid claims"u8);
public static error ErrInvalidType = errors.New("invalid type for claim"u8);

partial struct joinedError {
    internal slice<error> errs;
}

internal static @string Error(this joinedError je) {
    var msg = new @string[]{}.slice();
    foreach (var (_, err) in je.errs) {
        msg = append(msg, err.Error());
    }
    return strings.Join(msg, ", "u8);
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

    @string format = default!;
    slice<any> args = default!;
    if (message != ""u8){
        format = "%w: %s"u8;
        args = new any[]{err, message}.slice();
    } else {
        format = "%w"u8;
        args = new any[]{err}.slice();
    }
    foreach (var (_, e) in more) {
        format += ": %w"u8;
        args = append(args, (any)(e));
    }
    err = fmt.Errorf(format, args.ꓸꓸꓸ);
    return err;
}

} // end jwt_package
