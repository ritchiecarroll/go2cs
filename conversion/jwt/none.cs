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
    NoneSignatureTypeDisallowedError = newError("'none' signature type is not allowed", ErrTokenUnverifiable);

    RegisterSigningMethod(SigningMethodNone.Alg(), () => new signingMethodNoneжSigningMethod(SigningMethodNone));
}

public static @string Alg(this ref signingMethodNone m) {
    return "none";
}

public static error /*err*/ Verify(this ref signingMethodNone m, @string signingString, slice<byte> sig, any key) {
    {
        var (_, ok) = key._<unsafeNoneMagicConstant>(ᐧ); if (!ok) {
            return NoneSignatureTypeDisallowedError;
        }
    }
    if (len(sig) != 0) {
        return newError("'none' signing method with non-empty signature", ErrTokenUnverifiable);
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
