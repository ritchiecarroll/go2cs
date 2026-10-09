namespace go.github.com.golang_jwt.jwt;

using crypto = crypto_package;
using ed25519 = go.crypto.ed25519_package;
using rand = go.crypto.rand_package;
using errors = errors_package;
using go.crypto;
using io = io_package;

partial class jwt_package {

public static error ErrEd25519Verification = errors.New("ed25519: verification error"u8);

partial struct SigningMethodEd25519 {
}

public static ж<SigningMethodEd25519> SigningMethodEdDSA;

[GoInit] internal static void initΔ1() {
    SigningMethodEdDSA = Ꮡ(new SigningMethodEd25519(nil));
    RegisterSigningMethod(SigningMethodEdDSA.Alg(), () => new SigningMethodEd25519жSigningMethod(SigningMethodEdDSA));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string edDSAˢ = "EdDSA"u8;

public static @string Alg(this ref SigningMethodEd25519 m) {
    return edDSAˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ed25519VerifyExpectsˢ = "Ed25519 verify expects ed25519.PublicKey"u8;

public static error Verify(this ref SigningMethodEd25519 m, @string signingString, slice<byte> sig, any key) {
    ed25519.PublicKey ed25519Key = default!;
    bool ok = default!;
    {
        (ed25519Key, ok) = key._<ed25519.PublicKey>(ᐧ); if (!ok) {
            return newError(ed25519VerifyExpectsˢ, ErrInvalidKeyType);
        }
    }
    if (len(ed25519Key) != ed25519.PublicKeySize) {
        return ErrInvalidKey;
    }
    if (!ed25519.Verify(ed25519Key, slice<byte>(signingString), sig)) {
        return ErrEd25519Verification;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ed25519SignExpectsCryptoˢ = "Ed25519 sign expects crypto.Signer"u8;

public static (slice<byte>, error) Sign(this ref SigningMethodEd25519 m, @string signingString, any key) {
    crypto.Signer ed25519Key = default!;
    bool ok = default!;
    {
        (ed25519Key, ok) = key._<crypto.Signer>(ᐧ); if (!ok) {
            return (default!, newError(ed25519SignExpectsCryptoˢ, ErrInvalidKeyType));
        }
    }
    {
        var (_, okΔ1) = ed25519Key.Public()._<ed25519.PublicKey>(ᐧ); if (!okΔ1) {
            return (default!, ErrInvalidKey);
        }
    }
    var (sig, err) = ed25519Key.Sign(rand.Reader, slice<byte>(signingString), ((crypto.Hash)0));
    if (err != default!) {
        return (default!, err);
    }
    return (sig, default!);
}

} // end jwt_package
