namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using go.crypto;
using hash = hash_package;
using io = io_package;

partial class jwt_package {

partial struct SigningMethodRSAPSS {
    public partial ref ж<SigningMethodRSA> SigningMethodRSA { get; }
    public ж<rsa.PSSOptions> Options;
    public ж<rsa.PSSOptions> VerifyOptions;
}

public static ж<SigningMethodRSAPSS> SigningMethodPS256;
public static ж<SigningMethodRSAPSS> SigningMethodPS384;
public static ж<SigningMethodRSAPSS> SigningMethodPS512;

[GoInit] internal static void initΔ5() {
    SigningMethodPS256 = Ꮡ(new SigningMethodRSAPSS(
        SigningMethodRSA: Ꮡ(new SigningMethodRSA(
            Name: "PS256"u8,
            Hash: crypto.SHA256
        )),
        Options: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthEqualsHash
        )),
        VerifyOptions: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthAuto
        ))
    ));
    RegisterSigningMethod(SigningMethodPS256.Alg(), () => new SigningMethodRSAPSSжSigningMethod(SigningMethodPS256));
    SigningMethodPS384 = Ꮡ(new SigningMethodRSAPSS(
        SigningMethodRSA: Ꮡ(new SigningMethodRSA(
            Name: "PS384"u8,
            Hash: crypto.SHA384
        )),
        Options: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthEqualsHash
        )),
        VerifyOptions: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthAuto
        ))
    ));
    RegisterSigningMethod(SigningMethodPS384.Alg(), () => new SigningMethodRSAPSSжSigningMethod(SigningMethodPS384));
    SigningMethodPS512 = Ꮡ(new SigningMethodRSAPSS(
        SigningMethodRSA: Ꮡ(new SigningMethodRSA(
            Name: "PS512"u8,
            Hash: crypto.SHA512
        )),
        Options: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthEqualsHash
        )),
        VerifyOptions: Ꮡ(new rsa.PSSOptions(
            SaltLength: rsa.PSSSaltLengthAuto
        ))
    ));
    RegisterSigningMethod(SigningMethodPS512.Alg(), () => new SigningMethodRSAPSSжSigningMethod(SigningMethodPS512));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaPssVerifyExpectsRsaˢ = "RSA-PSS verify expects *rsa.PublicKey"u8;

public static error Verify(this ref SigningMethodRSAPSS m, @string signingString, slice<byte> sig, any key) {
    ж<rsa.PublicKey> rsaKey = default!;
    switch (key.type()) {
    case ж<rsa.PublicKey> k: {
        rsaKey = k;
        break;
    }
    default: {
        var k = key;
        return newError(rsaPssVerifyExpectsRsaˢ, ErrInvalidKeyType);
    }}
    if (!m.Hash.Available()) {
        return ErrHashUnavailable;
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    var opts = m.Options;
    if (m.VerifyOptions != nil) {
        opts = m.VerifyOptions;
    }
    return rsa.VerifyPSS(rsaKey, m.Hash, hasher.Sum(default!), sig, opts);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rsaPssSignExpectsRsaˢ = "RSA-PSS sign expects *rsa.PrivateKey"u8;

public static (slice<byte>, error) Sign(this ref SigningMethodRSAPSS m, @string signingString, any key) {
    ж<rsa.PrivateKey> rsaKey = default!;
    switch (key.type()) {
    case ж<rsa.PrivateKey> k: {
        rsaKey = k;
        break;
    }
    default: {
        var k = key;
        return (default!, newError(rsaPssSignExpectsRsaˢ, ErrInvalidKeyType));
    }}
    if (!m.Hash.Available()) {
        return (default!, ErrHashUnavailable);
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    {
        var (sigBytes, err) = rsa.SignPSS(rand.Reader, rsaKey, m.Hash, hasher.Sum(default!), m.Options); if (err == default!){
            return (sigBytes, default!);
        } else {
            return (default!, err);
        }
    }
}

} // end jwt_package
