namespace go.github.com.golang_jwt.jwt.v5;

using strings = strings_package;
using http = net.http_package;

partial class request_package {

internal static (@string, error) stripBearerPrefixFromTokenString(@string tok) {
    if (len(tok) > 6 && strings.EqualFold(tok[..7], bearerˢ)) {
        return (tok[7..], default!);
    }
    return (tok, default!);
}

public static ж<PostExtractionFilter> AuthorizationHeaderExtractor;
internal static void initᴛAuthorizationHeaderExtractor() { AuthorizationHeaderExtractor = Ꮡ(new PostExtractionFilter(
    new HeaderExtractor(new @string[]{"Authorization"u8}.slice()),
    stripBearerPrefixFromTokenString
)); }

public static ж<MultiExtractor> OAuth2Extractor;
internal static void initᴛOAuth2Extractor() { OAuth2Extractor = Ꮡ(new MultiExtractor(new Extractor[]{new PostExtractionFilterжExtractor(AuthorizationHeaderExtractor), new ArgumentExtractor(new @string[]{"access_token"u8}.slice())
}.slice())); }

} // end request_package
