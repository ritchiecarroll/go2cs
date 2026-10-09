namespace go.github.com.golang_jwt.jwt.v5;

using errors = errors_package;
using http = net.http_package;
using strings = strings_package;
using net;

partial class request_package {

public static error ErrNoTokenInRequest = errors.New("no token present in request"u8);

partial interface Extractor {
    (@string, error) ExtractToken(ж<http.Request> _);
}

partial struct HeaderExtractor /*[]@string*/;

public static (@string, error) ExtractToken(this HeaderExtractor e, ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    foreach (var (_, header) in e) {
        {
            @string ah = req.Header.Get(header); if (ah != ""u8) {
                return (ah, default!);
            }
        }
    }
    return ("", ErrNoTokenInRequest);
}

partial struct ArgumentExtractor /*[]@string*/;

public static (@string, error) ExtractToken(this ArgumentExtractor e, ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    _ = Ꮡreq.ParseMultipartForm(10000000);
    foreach (var (_, arg) in e) {
        {
            @string ah = req.Form.Get(arg); if (ah != ""u8) {
                return (ah, default!);
            }
        }
    }
    return ("", ErrNoTokenInRequest);
}

partial struct MultiExtractor /*[]Extractor*/;

public static (@string, error) ExtractToken(this MultiExtractor e, ж<http.Request> Ꮡreq) {
    foreach (var (_, extractor) in e) {
        {
            var (tok, err) = extractor.ExtractToken(Ꮡreq); if (tok != ""u8){
                return (tok, default!);
            } else 
            if (!errors.Is(err, ErrNoTokenInRequest)) {
                return ("", err);
            }
        }
    }
    return ("", ErrNoTokenInRequest);
}

partial struct PostExtractionFilter {
    /*embed*/ public Extractor Extractor;
    public Func<@string, (@string, error)> Filter;
}

public static (@string, error) ExtractToken(this ref PostExtractionFilter e, ж<http.Request> Ꮡreq) {
    {
        var (tok, err) = e.Extractor.ExtractToken(Ꮡreq); if (tok != ""u8){
            return e.Filter(tok);
        } else {
            return ("", err);
        }
    }
}

partial struct BearerExtractor {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string authorizationˢ = "Authorization"u8;
internal static readonly @string bearerˢ = "bearer "u8;

public static (@string, error) ExtractToken(this BearerExtractor e, ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.DerefOrNull();

    @string tokenHeader = req.Header.Get(authorizationˢ);
    if (len(tokenHeader) < 7 || !strings.EqualFold(tokenHeader[..7], bearerˢ)) {
        return ("", ErrNoTokenInRequest);
    }
    return (tokenHeader[7..], default!);
}

} // end request_package
