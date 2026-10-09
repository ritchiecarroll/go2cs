namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using go.crypto;
using hash = hash_package;
using io = io_package;

partial class jwt_package {

partial struct SigningMethodRSA {
    public @string Name;
    public crypto.Hash Hash;
}

public static ж<SigningMethodRSA> SigningMethodRS256;
public static ж<SigningMethodRSA> SigningMethodRS384;
public static ж<SigningMethodRSA> SigningMethodRS512;

[GoInit] internal static void initΔ4() {
    SigningMethodRS256 = Ꮡ(new SigningMethodRSA("RS256"u8, crypto.SHA256));
    RegisterSigningMethod(SigningMethodRS256.Alg(), () => new SigningMethodRSAжSigningMethod(SigningMethodRS256));
    SigningMethodRS384 = Ꮡ(new SigningMethodRSA("RS384"u8, crypto.SHA384));
    RegisterSigningMethod(SigningMethodRS384.Alg(), () => new SigningMethodRSAжSigningMethod(SigningMethodRS384));
    SigningMethodRS512 = Ꮡ(new SigningMethodRSA("RS512"u8, crypto.SHA512));
    RegisterSigningMethod(SigningMethodRS512.Alg(), () => new SigningMethodRSAжSigningMethod(SigningMethodRS512));
}

public static @string Alg(this ref SigningMethodRSA m) {
    return m.Name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaVerifyExpectsRsaˢ = "RSA verify expects *rsa.PublicKey"u8;

public static error Verify(this ref SigningMethodRSA m, @string signingString, slice<byte> sig, any key) {
    ж<rsa.PublicKey> rsaKey = default!;
    bool ok = default!;
    {
        (rsaKey, ok) = key._<ж<rsa.PublicKey>>(ᐧ); if (!ok) {
            return newError(rsaVerifyExpectsRsaˢ, ErrInvalidKeyType);
        }
    }
    if (!m.Hash.Available()) {
        return ErrHashUnavailable;
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    return rsa.VerifyPKCS1v15(rsaKey, m.Hash, hasher.Sum(default!), sig);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaSignExpectsRsaˢ = "RSA sign expects *rsa.PrivateKey"u8;

public static (slice<byte>, error) Sign(this ref SigningMethodRSA m, @string signingString, any key) {
    ж<rsa.PrivateKey> rsaKey = default!;
    bool ok = default!;
    {
        (rsaKey, ok) = key._<ж<rsa.PrivateKey>>(ᐧ); if (!ok) {
            return (default!, newError(rsaSignExpectsRsaˢ, ErrInvalidKeyType));
        }
    }
    if (!m.Hash.Available()) {
        return (default!, ErrHashUnavailable);
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    {
        var (sigBytes, err) = rsa.SignPKCS1v15(rand.Reader, rsaKey, m.Hash, hasher.Sum(default!)); if (err == default!){
            return (sigBytes, default!);
        } else {
            return (default!, err);
        }
    }
}

} // end jwt_package
