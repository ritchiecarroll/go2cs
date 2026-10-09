namespace go.github.com.golang_jwt.jwt.v5;

using http = net.http_package;
using jwt = go.github.com.golang_jwt.jwt.jwt_package;
using go.github.com.golang_jwt.jwt;
using net;

partial class request_package {

public static (ж<jwt.Token> token, error err) ParseFromRequest(ж<http.Request> Ꮡreq, Extractor extractor, Func<ж<jwt.Token>, (any, error)> keyFunc, params Span<Action<ж<fromRequestParser>>> optionsʗp) {
    error err = default!;
    var options = optionsʗp.sslice();

    var p = Ꮡ(new fromRequestParser(Ꮡreq, extractor, default!, nil));
    foreach (var (_, option) in options) {
        option(p);
    }
    if ((~p).claims == default!) {
        p.Value.claims = new jwt.MapClaims(new map<@string, any>{});
    }
    if ((~p).parser == nil) {
        p.Value.parser = Ꮡ(new jwt.Parser(nil));
    }
    (var tokenString, err) = (~p).extractor.ExtractToken(Ꮡreq);
    if (err != default!) {
        return (default!, err);
    }
    return (~p).parser.ParseWithClaims(tokenString, (~p).claims, keyFunc);
}

public static (ж<jwt.Token> token, error err) ParseFromRequestWithClaims(ж<http.Request> Ꮡreq, Extractor extractor, jwt.Claims claims, Func<ж<jwt.Token>, (any, error)> keyFunc) {
    return ParseFromRequest(Ꮡreq, extractor, keyFunc, WithClaims(claims));
}

public partial struct fromRequestParser {
    internal ж<http.Request> req;
    internal Extractor extractor;
    internal jwt.Claims claims;
    internal ж<jwt.Parser> parser;
}

// type ParseFromRequestOption is a methodless func type — rendered inline as its base delegate

public static Action<ж<fromRequestParser>> WithClaims(jwt.Claims claims) {
    return (ж<fromRequestParser> p) => {
        p.Value.claims = claims;
    };
}

public static Action<ж<fromRequestParser>> WithParser(ж<jwt.Parser> Ꮡparser) {
    ref var parser = ref Ꮡparser.DerefOrNull();

    return (ж<fromRequestParser> p) => {
        p.Value.parser = Ꮡparser;
    };
}

} // end request_package
