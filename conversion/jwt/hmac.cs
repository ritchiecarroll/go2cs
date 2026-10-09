namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using hmac = go.crypto.hmac_package;
using errors = errors_package;
using go.crypto;
using hash = hash_package;

partial class jwt_package {

partial struct SigningMethodHMAC {
    public @string Name;
    public crypto.Hash Hash;
}

public static ж<SigningMethodHMAC> SigningMethodHS256;
public static ж<SigningMethodHMAC> SigningMethodHS384;
public static ж<SigningMethodHMAC> SigningMethodHS512;
public static error ErrSignatureInvalid = errors.New("signature is invalid"u8);

[GoInit] internal static void initΔ2() {
    SigningMethodHS256 = Ꮡ(new SigningMethodHMAC("HS256"u8, crypto.SHA256));
    RegisterSigningMethod(SigningMethodHS256.Alg(), () => new SigningMethodHMACжSigningMethod(SigningMethodHS256));
    SigningMethodHS384 = Ꮡ(new SigningMethodHMAC("HS384"u8, crypto.SHA384));
    RegisterSigningMethod(SigningMethodHS384.Alg(), () => new SigningMethodHMACжSigningMethod(SigningMethodHS384));
    SigningMethodHS512 = Ꮡ(new SigningMethodHMAC("HS512"u8, crypto.SHA512));
    RegisterSigningMethod(SigningMethodHS512.Alg(), () => new SigningMethodHMACжSigningMethod(SigningMethodHS512));
}

public static @string Alg(this ref SigningMethodHMAC m) {
    return m.Name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hmacVerifyExpectsByteˢ = "HMAC verify expects []byte"u8;

public static error Verify(this ж<SigningMethodHMAC> Ꮡm, @string signingString, slice<byte> sig, any key) {
    ref var m = ref Ꮡm.DerefOrNull();

    var (keyBytes, ok) = key._<slice<byte>>(ᐧ);
    if (!ok) {
        return newError(hmacVerifyExpectsByteˢ, ErrInvalidKeyType);
    }
    if (!m.Hash.Available()) {
        return ErrHashUnavailable;
    }
    var recvʗ1 = m.Hash;
    var hasher = hmac.New(() => recvʗ1.New(), keyBytes);
    hasher.Write(slice<byte>(signingString));
    if (!hmac.Equal(sig, hasher.Sum(default!))) {
        return ErrSignatureInvalid;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hmacSignExpectsByteˢ = "HMAC sign expects []byte"u8;

public static (slice<byte>, error) Sign(this ж<SigningMethodHMAC> Ꮡm, @string signingString, any key) {
    ref var m = ref Ꮡm.DerefOrNull();

    {
        var (keyBytes, ok) = key._<slice<byte>>(ᐧ); if (ok) {
            if (!m.Hash.Available()) {
                return (default!, ErrHashUnavailable);
            }
            var recvʗ1 = m.Hash;
            var hasher = hmac.New(() => recvʗ1.New(), keyBytes);
            hasher.Write(slice<byte>(signingString));
            return (hasher.Sum(default!), default!);
        }
    }
    return (default!, newError(hmacSignExpectsByteˢ, ErrInvalidKeyType));
}

} // end jwt_package
