namespace go.github.com.golang_jwt.jwt;

using time = time_package;
using ꓸꓸꓸstring = Span<@string>;

partial class jwt_package {

// type ParserOption is a methodless func type — rendered inline as its base delegate

public static Action<ж<Parser>> WithValidMethods(slice<@string> methods) {
    var methodsʗ1 = methods;
    return (ж<Parser> p) => {
        p.validMethods = methodsʗ1;
    };
}

public static Action<ж<Parser>> WithJSONNumber() {
    return (ж<Parser> p) => {
        p.useJSONNumber = true;
    };
}

public static Action<ж<Parser>> WithoutClaimsValidation() {
    return (ж<Parser> p) => {
        p.skipClaimsValidation = true;
    };
}

public static Action<ж<Parser>> WithLeeway(time.Duration leeway) {
    return (ж<Parser> p) => {
        p.validator.leeway = leeway;
    };
}

public static Action<ж<Parser>> WithTimeFunc(Func<time.Time> f) {
    return (ж<Parser> p) => {
        p.validator.timeFunc = f;
    };
}

public static Action<ж<Parser>> WithIssuedAt() {
    return (ж<Parser> p) => {
        p.validator.verifyIat = true;
    };
}

public static Action<ж<Parser>> WithExpirationRequired() {
    return (ж<Parser> p) => {
        p.validator.requireExp = true;
    };
}

public static Action<ж<Parser>> WithNotBeforeRequired() {
    return (ж<Parser> p) => {
        p.validator.requireNbf = true;
    };
}

public static Action<ж<Parser>> WithAudience(params ꓸꓸꓸstring audʗp) {
    var aud = audʗp.slice();

    var audʗ1 = aud;
    return (ж<Parser> p) => {
        p.validator.expectedAud = audʗ1;
    };
}

public static Action<ж<Parser>> WithAllAudiences(params ꓸꓸꓸstring audʗp) {
    var aud = audʗp.slice();

    var audʗ1 = aud;
    return (ж<Parser> p) => {
        p.validator.expectedAud = audʗ1;
        p.validator.expectAllAud = true;
    };
}

public static Action<ж<Parser>> WithIssuer(@string iss) {
    return (ж<Parser> p) => {
        p.validator.expectedIss = iss;
    };
}

public static Action<ж<Parser>> WithSubject(@string sub) {
    return (ж<Parser> p) => {
        p.validator.expectedSub = sub;
    };
}

public static Action<ж<Parser>> WithPaddingAllowed() {
    return (ж<Parser> p) => {
        p.decodePaddingAllowed = true;
    };
}

public static Action<ж<Parser>> WithStrictDecoding() {
    return (ж<Parser> p) => {
        p.decodeStrict = true;
    };
}

} // end jwt_package
