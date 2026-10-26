namespace go.github.com.golang_jwt.jwt;

using json = encoding.json_package;
using fmt = fmt_package;
using encoding;

partial class jwt_package {

partial struct MapClaims /*map[@string, any]*/;

public static (ж<NumericDate>, error) GetExpirationTime(this MapClaims m) {
    return m.parseNumericDate("exp");
}

public static (ж<NumericDate>, error) GetNotBefore(this MapClaims m) {
    return m.parseNumericDate("nbf");
}

public static (ж<NumericDate>, error) GetIssuedAt(this MapClaims m) {
    return m.parseNumericDate("iat");
}

public static (ClaimStrings, error) GetAudience(this MapClaims m) {
    return m.parseClaimsString("aud");
}

public static (@string, error) GetIssuer(this MapClaims m) {
    return m.parseString("iss");
}

public static (@string, error) GetSubject(this MapClaims m) {
    return m.parseString("sub");
}

internal static (ж<NumericDate>, error) parseNumericDate(this MapClaims m, @string key) {
    var (v, ok) = m[key, ꟷ];
    if (!ok) {
        return (default!, default!);
    }

    switch (v.type()) {
    case float64 exp: {
        if (exp == 0D) {
            return (default!, default!);
        }

        return (newNumericDateFromSeconds(exp), default!);
    }
    case json.Number exp: {
        var (vΔ1, _) = exp.Float64();

        return (newNumericDateFromSeconds(vΔ1), default!);
    }}

    return (default!, newError(fmt.Sprintf("%s is invalid"u8, key), ErrInvalidType));
}

internal static (ClaimStrings, error) parseClaimsString(this MapClaims m, @string key) {
    slice<@string> cs = default!;
    switch (m[key].type()) {
    case @string v: {
        cs = append(cs, v);
        break;
    }
    case slice<@string> v: {
        cs = v;
        break;
    }
    case slice<any> v: {
        foreach (var (_, a) in v) {
            var (vs, ok) = a._<@string>(ᐧ);
            if (!ok) {
                return (default!, newError(fmt.Sprintf("%s is invalid"u8, key), ErrInvalidType));
            }
            cs = append(cs, vs);
        }
        break;
    }}

    return (cs, default!);
}

internal static (@string, error) parseString(this MapClaims m, @string key) {
    bool ok = default!;
    any raw = default!;
    @string iss = default!;
    (raw, ok) = m[key, ꟷ];
    if (!ok) {
        return ("", default!);
    }

    (iss, ok) = raw._<@string>(ᐧ);
    if (!ok) {
        return ("", newError(fmt.Sprintf("%s is invalid"u8, key), ErrInvalidType));
    }

    return (iss, default!);
}

} // end jwt_package
