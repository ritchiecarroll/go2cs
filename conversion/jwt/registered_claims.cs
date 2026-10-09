namespace go.github.com.golang_jwt.jwt;

partial class jwt_package {

partial struct RegisteredClaims {
    public @string Issuer; /*`json:"iss,omitempty"`*/
    public @string Subject; /*`json:"sub,omitempty"`*/
    public ClaimStrings Audience; /*`json:"aud,omitempty"`*/
    public ж<NumericDate> ExpiresAt; /*`json:"exp,omitempty"`*/
    public ж<NumericDate> NotBefore; /*`json:"nbf,omitempty"`*/
    public ж<NumericDate> IssuedAt; /*`json:"iat,omitempty"`*/
    public @string ID; /*`json:"jti,omitempty"`*/
}

public static (ж<NumericDate>, error) GetExpirationTime(this RegisteredClaims c) {
    return (c.ExpiresAt, default!);
}

public static (ж<NumericDate>, error) GetNotBefore(this RegisteredClaims c) {
    return (c.NotBefore, default!);
}

public static (ж<NumericDate>, error) GetIssuedAt(this RegisteredClaims c) {
    return (c.IssuedAt, default!);
}

public static (ClaimStrings, error) GetAudience(this RegisteredClaims c) {
    return (c.Audience, default!);
}

public static (@string, error) GetIssuer(this RegisteredClaims c) {
    return (c.Issuer, default!);
}

public static (@string, error) GetSubject(this RegisteredClaims c) {
    return (c.Subject, default!);
}

} // end jwt_package
