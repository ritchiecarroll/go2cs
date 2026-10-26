namespace go.github.com.golang_jwt.jwt;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using json = encoding.json_package;
using fmt = fmt_package;
using strings = strings_package;
using encoding;
using io = io_package;

partial class jwt_package {

internal static readonly @string tokenDelimiter = "."u8;

partial struct Parser {
    internal slice<@string> validMethods;
    internal bool useJSONNumber;
    internal bool skipClaimsValidation;
    internal ж<Validator> validator;
    internal bool decodeStrict;
    internal bool decodePaddingAllowed;
}

public static ж<Parser> NewParser(params Span<Action<ж<Parser>>> optionsʗp) {
    var options = optionsʗp.sslice();

    var p = Ꮡ(new Parser(
        validator: Ꮡ(new Validator(nil))
    ));

    foreach (var (_, option) in options) {
        option(p);
    }

    return p;
}

public static (ж<Token>, error) Parse(this ref Parser p, @string tokenString, Func<ж<Token>, (any, error)> keyFunc) {
    return p.ParseWithClaims(tokenString, new MapClaims(new map<@string, any>{}), keyFunc);
}

public static (ж<Token>, error) ParseWithClaims(this ref Parser p, @string tokenString, Claims claims, Func<ж<Token>, (any, error)> keyFunc) {
    var (token, parts, err) = p.ParseUnverified(tokenString, claims);
    if (err != default!) {
        return (token, err);
    }

    if (p.validMethods != default!) {
        bool signingMethodValid = false;
        @string alg = (~token).Method.Alg();
        foreach (var (_, m) in p.validMethods) {
            if (m == alg) {
                signingMethodValid = true;
                break;
            }
        }
        if (!signingMethodValid) {
            return (token, newError(fmt.Sprintf("signing method %v is invalid"u8, alg), ErrTokenSignatureInvalid));
        }
    }

    if (keyFunc == default!) {
        return (token, newError("no keyfunc was provided", ErrTokenUnverifiable));
    }

    (var got, err) = keyFunc(token);
    if (err != default!) {
        return (token, newError("error while executing keyfunc", ErrTokenUnverifiable, err));
    }

    @string text = strings.Join(parts[0..2], ".");
    switch (got.type()) {
    case VerificationKeySet have: {
        if (len(have.Keys) == 0) {
            return (token, newError("keyfunc returned empty verification key set", ErrTokenUnverifiable));
        }

        foreach (var (_, key) in have.Keys) {
            {
                err = (~token).Method.Verify(text, (~token).Signature, key); if (err == default!) {
                    break;
                }
            }
        }
        break;
    }
    default: {
        var have = got;
        err = (~token).Method.Verify(text, (~token).Signature, have);
        break;
    }}
    if (err != default!) {
        return (token, newError("", ErrTokenSignatureInvalid, err));
    }

    if (!p.skipClaimsValidation) {
        if (p.validator == nil) {
            p.validator = NewValidator();
        }

        {
            var errΔ1 = p.validator.Validate(claims); if (errΔ1 != default!) {
                return (token, newError("", ErrTokenInvalidClaims, errΔ1));
            }
        }
    }

    token.Value.Valid = true;

    return (token, default!);
}

public static (ж<Token> token, slice<@string> parts, error err) ParseUnverified(this ref Parser p, @string tokenString, Claims claimsʗp) {
    ж<Token> token = default!;
    slice<@string> parts = default!;
    error err = default!;
    ref var claims = ref heap(claimsʗp, out var Ꮡclaims);

    bool ok = default!;
    (parts, ok) = splitToken(tokenString);
    if (!ok) {
        return (default!, default!, newError("token contains an invalid number of segments", ErrTokenMalformed));
    }

    token = Ꮡ(new Token(Raw: tokenString));

    slice<byte> headerBytes = default!;
    {
        (headerBytes, err) = p.DecodeSegment(parts[0]); if (err != default!) {
            return (token, parts, newError("could not base64 decode header", ErrTokenMalformed, err));
        }
    }
    {
        err = json.Unmarshal(headerBytes, token.of(Token.ᏑHeader)); if (err != default!) {
            return (token, parts, newError("could not JSON decode header", ErrTokenMalformed, err));
        }
    }

    token.Value.Claims = claims;

    (var claimBytes, err) = p.DecodeSegment(parts[1]);
    if (err != default!) {
        return (token, parts, newError("could not base64 decode claim", ErrTokenMalformed, err));
    }

    if (!p.useJSONNumber){
        {
            ref var c = ref heap<MapClaims>(out var Ꮡc);
            (c, var okΔ1) = (~token).Claims._<MapClaims>(ᐧ); if (okΔ1){
                err = json.Unmarshal(claimBytes, Ꮡc);
            } else {
                err = json.Unmarshal(claimBytes, Ꮡclaims);
            }
        }
    } else {
        var dec = json.NewDecoder(new bytes_BufferжReader(bytes.NewBuffer(claimBytes)));
        dec.UseNumber();
        {
            ref var c = ref heap<MapClaims>(out var Ꮡc);
            (c, var okΔ2) = (~token).Claims._<MapClaims>(ᐧ); if (okΔ2){
                err = dec.Decode(Ꮡc);
            } else {
                err = dec.Decode(Ꮡclaims);
            }
        }
    }
    if (err != default!) {
        return (token, parts, newError("could not JSON decode claim", ErrTokenMalformed, err));
    }

    {
        var (method, okΔ3) = (~token).Header["alg"]._<@string>(ᐧ); if (okΔ3){
            {
                token.Value.Method = GetSigningMethod(method); if ((~token).Method == default!) {
                    return (token, parts, newError("signing method (alg) is unavailable", ErrTokenUnverifiable));
                }
            }
        } else {
            return (token, parts, newError("signing method (alg) is unspecified", ErrTokenUnverifiable));
        }
    }

    (token.Value.Signature, err) = p.DecodeSegment(parts[2]);
    if (err != default!) {
        return (token, parts, newError("could not base64 decode signature", ErrTokenMalformed, err));
    }

    return (token, parts, default!);
}

internal static (slice<@string>, bool) splitToken(@string token) {
    var parts = new slice<@string>(3);
    var (header, remain, ok) = strings.Cut(token, tokenDelimiter);
    if (!ok) {
        return (default!, false);
    }
    parts[0] = header;
    (var claims, remain, ok) = strings.Cut(remain, tokenDelimiter);
    if (!ok) {
        return (default!, false);
    }
    parts[1] = claims;
    var (signature, _, unexpected) = strings.Cut(remain, tokenDelimiter);
    if (unexpected) {
        return (default!, false);
    }
    parts[2] = signature;

    return (parts, true);
}

public static (slice<byte>, error) DecodeSegment(this ref Parser p, @string seg) {
    var encoding = base64.RawURLEncoding;

    if (p.decodePaddingAllowed) {
        {
            nint l = len(seg) % 4; if (l > 0) {
                seg += strings.Repeat("=", 4 - l);
            }
        }
        encoding = base64.URLEncoding;
    }

    if (p.decodeStrict) {
        encoding = (~encoding).Strict();
    }
    return encoding.DecodeString(seg);
}

public static (ж<Token>, error) Parse(@string tokenString, Func<ж<Token>, (any, error)> keyFunc, params Span<Action<ж<Parser>>> optionsʗp) {
    var options = optionsʗp.sslice();

    return NewParser(options.ꓸꓸꓸ).Parse(tokenString, keyFunc);
}

public static (ж<Token>, error) ParseWithClaims(@string tokenString, Claims claims, Func<ж<Token>, (any, error)> keyFunc, params Span<Action<ж<Parser>>> optionsʗp) {
    var options = optionsʗp.sslice();

    return NewParser(options.ꓸꓸꓸ).ParseWithClaims(tokenString, claims, keyFunc);
}

} // end jwt_package
