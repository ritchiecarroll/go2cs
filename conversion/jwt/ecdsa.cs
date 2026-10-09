namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using rand = go.crypto.rand_package;
using errors = errors_package;
using big = go.math.big_package;
using elliptic = go.crypto.elliptic_package;
using go.crypto;
using go.math;
using hash = hash_package;
using io = io_package;

partial class jwt_package {

public static error ErrECDSAVerification = errors.New("crypto/ecdsa: verification error"u8);

partial struct SigningMethodECDSA {
    public @string Name;
    public crypto.Hash Hash;
    public nint KeySize;
    public nint CurveBits;
}

public static ж<SigningMethodECDSA> SigningMethodES256;
public static ж<SigningMethodECDSA> SigningMethodES384;
public static ж<SigningMethodECDSA> SigningMethodES512;

[GoInit] internal static void init() {
    SigningMethodES256 = Ꮡ(new SigningMethodECDSA("ES256"u8, crypto.SHA256, 32, 256));
    RegisterSigningMethod(SigningMethodES256.Alg(), () => new SigningMethodECDSAжSigningMethod(SigningMethodES256));
    SigningMethodES384 = Ꮡ(new SigningMethodECDSA("ES384"u8, crypto.SHA384, 48, 384));
    RegisterSigningMethod(SigningMethodES384.Alg(), () => new SigningMethodECDSAжSigningMethod(SigningMethodES384));
    SigningMethodES512 = Ꮡ(new SigningMethodECDSA("ES512"u8, crypto.SHA512, 66, 521));
    RegisterSigningMethod(SigningMethodES512.Alg(), () => new SigningMethodECDSAжSigningMethod(SigningMethodES512));
}

public static @string Alg(this ref SigningMethodECDSA m) {
    return m.Name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaVerifyExpectsEcdsaˢ = "ECDSA verify expects *ecdsa.PublicKey"u8;

public static error Verify(this ref SigningMethodECDSA m, @string signingString, slice<byte> sig, any key) {
    ж<ecdsa.PublicKey> ecdsaKey = default!;
    switch (key.type()) {
    case ж<ecdsa.PublicKey> k: {
        ecdsaKey = k;
        break;
    }
    default: {
        var k = key;
        return newError(ecdsaVerifyExpectsEcdsaˢ, ErrInvalidKeyType);
    }}
    if (len(sig) != 2 * m.KeySize) {
        return ErrECDSAVerification;
    }
    var r = big.NewInt(0).SetBytes(sig.slice(0, m.KeySize));
    var s = big.NewInt(0).SetBytes(sig.slice(m.KeySize));
    if (!m.Hash.Available()) {
        return ErrHashUnavailable;
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    {
        var verifystatus = ecdsa.Verify(ecdsaKey, hasher.Sum(default!), r, s); if (verifystatus) {
            return default!;
        }
    }
    return ErrECDSAVerification;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ecdsaSignExpectsEcdsaˢ = "ECDSA sign expects *ecdsa.PrivateKey"u8;

public static (slice<byte>, error) Sign(this ref SigningMethodECDSA m, @string signingString, any key) {
    ж<ecdsa.PrivateKey> ecdsaKey = default!;
    switch (key.type()) {
    case ж<ecdsa.PrivateKey> k: {
        ecdsaKey = k;
        break;
    }
    default: {
        var k = key;
        return (default!, newError(ecdsaSignExpectsEcdsaˢ, ErrInvalidKeyType));
    }}
    if (!m.Hash.Available()) {
        return (default!, ErrHashUnavailable);
    }
    var hasher = m.Hash.New();
    hasher.Write(slice<byte>(signingString));
    {
        var (r, s, err) = ecdsa.Sign(rand.Reader, ecdsaKey, hasher.Sum(default!)); if (err == default!){
            nint curveBits = (~ecdsaKey).Curve.Params().Value.BitSize;
            if (m.CurveBits != curveBits) {
                return (default!, ErrInvalidKey);
            }
            nint keyBytes = curveBits / 8;
            if (curveBits % 8 > 0) {
                keyBytes += 1;
            }
            var @out = new slice<byte>(2 * keyBytes);
            r.FillBytes(@out.slice(0, keyBytes));
            s.FillBytes(@out.slice(keyBytes));
            return (@out, default!);
        } else {
            return (default!, err);
        }
    }
}

} // end jwt_package
