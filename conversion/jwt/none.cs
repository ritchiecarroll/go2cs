namespace go.github.com.golang_jwt.jwt;

partial class jwt_package {

public static ж<signingMethodNone> SigningMethodNone;

public static readonly unsafeNoneMagicConstant UnsafeAllowNoneSignatureType = "none signing method allowed"u8;

public static error NoneSignatureTypeDisallowedError;

public partial struct signingMethodNone {
}

public partial struct unsafeNoneMagicConstant /*@string*/;

[GoInit] internal static void initΔ3() {
    SigningMethodNone = Ꮡ(new signingMethodNone(nil));
    NoneSignatureTypeDisallowedError = newError("'none' signature type is not allowed"u8, ErrTokenUnverifiable);
    RegisterSigningMethod(SigningMethodNone.Alg(), () => new signingMethodNoneжSigningMethod(SigningMethodNone));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noneˢ = "none"u8;

public static @string Alg(this ref signingMethodNone m) {
    return noneˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noneSigningMethodWithNonˢ = "'none' signing method with non-empty signature"u8;

public static error /*err*/ Verify(this ref signingMethodNone m, @string signingString, slice<byte> sig, any key) {
    {
        var (_, ok) = key._<unsafeNoneMagicConstant>(ᐧ); if (!ok) {
            return NoneSignatureTypeDisallowedError;
        }
    }
    if (len(sig) != 0) {
        return newError(noneSigningMethodWithNonˢ, ErrTokenUnverifiable);
    }
    return default!;
}

public static (slice<byte>, error) Sign(this ref signingMethodNone m, @string signingString, any key) {
    {
        var (_, ok) = key._<unsafeNoneMagicConstant>(ᐧ); if (ok) {
            return (new byte[]{}.slice(), default!);
        }
    }
    return (default!, NoneSignatureTypeDisallowedError);
}

} // end jwt_package
