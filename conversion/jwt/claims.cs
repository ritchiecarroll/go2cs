namespace go.github.com.golang_jwt.jwt;

partial class jwt_package {

partial interface Claims {
    (ж<NumericDate>, error) GetExpirationTime();
    (ж<NumericDate>, error) GetIssuedAt();
    (ж<NumericDate>, error) GetNotBefore();
    (@string, error) GetIssuer();
    (@string, error) GetSubject();
    (ClaimStrings, error) GetAudience();
}

} // end jwt_package
