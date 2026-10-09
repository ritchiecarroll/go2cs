global using VerificationKey = object;

namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using base64 = encoding.base64_package;
using json = encoding.json_package;
using encoding;

partial class jwt_package {

// type Keyfunc is a methodless func type — rendered inline as its base delegate

partial interface VerificationKeyᴛ1 /*dyn*/ {
    //  Type constraints: crypto.PublicKey | []uint8
    // Derived operators: none
}

partial struct VerificationKeySet {
    public slice<VerificationKey> Keys;
}

partial struct Token {
    public @string Raw;
    public SigningMethod Method;
    public map<@string, any> Header;
    public Claims Claims;
    public slice<byte> Signature;
    public bool Valid;
}

public static ж<Token> New(SigningMethod method, params Span<Action<ж<Token>>> optsʗp) {
    var opts = optsʗp.sslice();

    return NewWithClaims(method, new MapClaims(new map<@string, any>{}), opts.ꓸꓸꓸ);
}

public static ж<Token> NewWithClaims(SigningMethod method, Claims claims, params Span<Action<ж<Token>>> optsʗp) {
    var opts = optsʗp.sslice();

    return Ꮡ(new Token(
        Header: new map<@string, any>{
            ["typ"u8] = (@string)"JWT"u8,
            ["alg"u8] = method.Alg()
        },
        Claims: claims,
        Method: method
    ));
}

public static (@string, error) SignedString(this ref Token t, any key) {
    var (sstr, err) = t.SigningString();
    if (err != default!) {
        return ("", err);
    }
    (var sig, err) = t.Method.Sign(sstr, key);
    if (err != default!) {
        return ("", err);
    }
    t.Signature = sig;
    return (sstr + "." + t.EncodeSegment(sig), default!);
}

public static (@string, error) SigningString(this ref Token t) {
    var (h, err) = json.Marshal(t.Header);
    if (err != default!) {
        return ("", err);
    }
    (var c, err) = json.Marshal(t.Claims);
    if (err != default!) {
        return ("", err);
    }
    return (t.EncodeSegment(h) + "." + t.EncodeSegment(c), default!);
}

public static @string EncodeSegment(this ref Token _, slice<byte> seg) {
    return base64.RawURLEncoding.EncodeToString(seg);
}

} // end jwt_package
