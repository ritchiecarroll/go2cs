namespace go.github.com.golang_jwt.jwt;

using time = time_package;
using ꓸꓸꓸstring = Span<@string>;

partial class jwt_package {

// type ParserOption is a methodless func type — rendered inline as its base delegate

public static Action<ж<Parser>> WithValidMethods(slice<@string> methods) {
    var methodsʗ1 = methods;
    return (ж<Parser> p) => {
        p.Value.validMethods = methodsʗ1;
    };
}

public static Action<ж<Parser>> WithJSONNumber() {
    return (ж<Parser> p) => {
        p.Value.useJSONNumber = true;
    };
}

public static Action<ж<Parser>> WithoutClaimsValidation() {
    return (ж<Parser> p) => {
        p.Value.skipClaimsValidation = true;
    };
}

public static Action<ж<Parser>> WithLeeway(time.Duration leeway) {
    return (ж<Parser> p) => {
        p.Value.validator.Value.leeway = leeway;
    };
}

public static Action<ж<Parser>> WithTimeFunc(Func<time.Time> f) {
    return (ж<Parser> p) => {
        p.Value.validator.Value.timeFunc = f;
    };
}

public static Action<ж<Parser>> WithIssuedAt() {
    return (ж<Parser> p) => {
        p.Value.validator.Value.verifyIat = true;
    };
}

public static Action<ж<Parser>> WithExpirationRequired() {
    return (ж<Parser> p) => {
        p.Value.validator.Value.requireExp = true;
    };
}

public static Action<ж<Parser>> WithNotBeforeRequired() {
    return (ж<Parser> p) => {
        p.Value.validator.Value.requireNbf = true;
    };
}

public static Action<ж<Parser>> WithAudience(params ꓸꓸꓸstring audʗp) {
    var aud = audʗp.slice();

    var audʗ1 = aud;
    return (ж<Parser> p) => {
        p.Value.validator.Value.expectedAud = audʗ1;
    };
}

public static Action<ж<Parser>> WithAllAudiences(params ꓸꓸꓸstring audʗp) {
    var aud = audʗp.slice();

    var audʗ1 = aud;
    return (ж<Parser> p) => {
        p.Value.validator.Value.expectedAud = audʗ1;
        p.Value.validator.Value.expectAllAud = true;
    };
}

public static Action<ж<Parser>> WithIssuer(@string iss) {
    return (ж<Parser> p) => {
        p.Value.validator.Value.expectedIss = iss;
    };
}

public static Action<ж<Parser>> WithSubject(@string sub) {
    return (ж<Parser> p) => {
        p.Value.validator.Value.expectedSub = sub;
    };
}

public static Action<ж<Parser>> WithPaddingAllowed() {
    return (ж<Parser> p) => {
        p.Value.decodePaddingAllowed = true;
    };
}

public static Action<ж<Parser>> WithStrictDecoding() {
    return (ж<Parser> p) => {
        p.Value.decodeStrict = true;
    };
}

} // end jwt_package
