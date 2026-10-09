namespace go.github.com.golang_jwt.jwt;

using fmt = fmt_package;
using slices = slices_package;
using time = time_package;

partial class jwt_package {

partial interface ClaimsValidator :
    Claims
{
    error Validate();
}

partial struct Validator {
    internal time.Duration leeway;
    internal Func<time.Time> timeFunc;
    internal bool requireExp;
    internal bool requireNbf;
    internal bool verifyIat;
    internal slice<@string> expectedAud;
    internal bool expectAllAud;
    internal @string expectedIss;
    internal @string expectedSub;
}

public static ж<Validator> NewValidator(params Span<Action<ж<Parser>>> optsʗp) {
    var opts = optsʗp.sslice();

    var p = NewParser(opts.ꓸꓸꓸ);
    return (~p).validator;
}

public static error Validate(this ref Validator v, Claims claims) {
    time.Time now = default!;
    slice<error> errs = new slice<error>(0, 6);
    error err = default!;
    if (v.timeFunc != default!){
        now = v.timeFunc();
    } else {
        now = time.Now();
    }
    {
        err = v.verifyExpiresAt(claims, now, v.requireExp); if (err != default!) {
            errs = append(errs, err);
        }
    }
    {
        err = v.verifyNotBefore(claims, now, v.requireNbf); if (err != default!) {
            errs = append(errs, err);
        }
    }
    if (v.verifyIat) {
        {
            err = v.verifyIssuedAt(claims, now, false); if (err != default!) {
                errs = append(errs, err);
            }
        }
    }
    if (len(v.expectedAud) > 0) {
        {
            err = v.verifyAudience(claims, v.expectedAud, v.expectAllAud); if (err != default!) {
                errs = append(errs, err);
            }
        }
    }
    if (v.expectedIss != ""u8) {
        {
            err = v.verifyIssuer(claims, v.expectedIss, true); if (err != default!) {
                errs = append(errs, err);
            }
        }
    }
    if (v.expectedSub != ""u8) {
        {
            err = v.verifySubject(claims, v.expectedSub, true); if (err != default!) {
                errs = append(errs, err);
            }
        }
    }
    var (cvt, ok) = claims._<ClaimsValidator>(ᐧ);
    if (ok) {
        {
            var errΔ1 = cvt.Validate(); if (errΔ1 != default!) {
                errs = append(errs, errΔ1);
            }
        }
    }
    if (len(errs) == 0) {
        return default!;
    }
    return joinErrors(errs.ꓸꓸꓸ);
}

internal static error verifyExpiresAt(this ref Validator v, Claims claims, time.Time cmp, bool @required) {
    var (exp, err) = claims.GetExpirationTime();
    if (err != default!) {
        return err;
    }
    if (exp == nil) {
        return errorIfRequired(@required, expˢ);
    }
    return errorIfFalse(cmp.Before(((~exp).Time).Add(+v.leeway)), ErrTokenExpired);
}

internal static error verifyIssuedAt(this ref Validator v, Claims claims, time.Time cmp, bool @required) {
    var (iat, err) = claims.GetIssuedAt();
    if (err != default!) {
        return err;
    }
    if (iat == nil) {
        return errorIfRequired(@required, iatˢ);
    }
    return errorIfFalse(!cmp.Before(iat.Value.Time.Add(-v.leeway)), ErrTokenUsedBeforeIssued);
}

internal static error verifyNotBefore(this ref Validator v, Claims claims, time.Time cmp, bool @required) {
    var (nbf, err) = claims.GetNotBefore();
    if (err != default!) {
        return err;
    }
    if (nbf == nil) {
        return errorIfRequired(@required, nbfˢ);
    }
    return errorIfFalse(!cmp.Before(nbf.Value.Time.Add(-v.leeway)), ErrTokenNotValidYet);
}

internal static error verifyAudience(this ref Validator v, Claims claims, slice<@string> cmp, bool expectAllAud) {
    var (aud, err) = claims.GetAudience();
    if (err != default!) {
        return err;
    }
    if (len(aud) == 0 || len(aud) == 1 && aud[0] == "") {
        var @required = len(v.expectedAud) > 0;
        return errorIfRequired(@required, audˢ);
    }
    if (!expectAllAud) {
        foreach (var (_, a) in aud) {
            if (slices.Contains(cmp, a)) {
                return default!;
            }
        }
        return ErrTokenInvalidAudience;
    }
    foreach (var (_, a) in cmp) {
        if (!slices.Contains(aud, a)) {
            return ErrTokenInvalidAudience;
        }
    }
    return default!;
}

internal static error verifyIssuer(this ref Validator v, Claims claims, @string cmp, bool @required) {
    var (iss, err) = claims.GetIssuer();
    if (err != default!) {
        return err;
    }
    if (iss == ""u8) {
        return errorIfRequired(@required, issˢ);
    }
    return errorIfFalse(iss == cmp, ErrTokenInvalidIssuer);
}

internal static error verifySubject(this ref Validator v, Claims claims, @string cmp, bool @required) {
    var (sub, err) = claims.GetSubject();
    if (err != default!) {
        return err;
    }
    if (sub == ""u8) {
        return errorIfRequired(@required, subˢ);
    }
    return errorIfFalse(sub == cmp, ErrTokenInvalidSubject);
}

internal static error errorIfFalse(bool value, error err) {
    if (value){
        return default!;
    } else {
        return err;
    }
}

internal static error errorIfRequired(bool @required, @string claim) {
    if (@required){
        return newError(fmt.Sprintf("%s claim is required"u8, claim), ErrTokenRequiredClaimMissing);
    } else {
        return default!;
    }
}

} // end jwt_package
