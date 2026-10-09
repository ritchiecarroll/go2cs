namespace go.github.com.golang_jwt.jwt;

using sync = sync_package;

partial class jwt_package {

internal static map<@string, Func<SigningMethod>> signingMethods = new map<@string, Func<SigningMethod>>{};

internal static ж<sync.RWMutex> signingMethodLock = @new<sync.RWMutex>();

partial interface SigningMethod {
    error Verify(@string signingString, slice<byte> sig, any key);
    (slice<byte>, error) Sign(@string signingString, any key);
    @string Alg();
}

public static void RegisterSigningMethod(@string alg, Func<SigningMethod> f) {
    GoFrame ᒐ = default;
    try {
        signingMethodLock.Lock();
        defer(signingMethodLock.Unlock, ref ᒐ);
        signingMethods[alg] = f;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static SigningMethod /*method*/ GetSigningMethod(@string alg) {
    SigningMethod method = default!;
    GoFrame ᒐ = default;
    try {
        signingMethodLock.RLock();
        defer(signingMethodLock.RUnlock, ref ᒐ);
        {
            var (methodF, ok) = signingMethods[alg, ꟷ]; if (ok) {
                method = methodF();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return method;
}

public static slice<@string> /*algs*/ GetAlgorithms() {
    slice<@string> algs = default!;
    GoFrame ᒐ = default;
    try {
        signingMethodLock.RLock();
        defer(signingMethodLock.RUnlock, ref ᒐ);
        foreach (var (alg, _) in signingMethods) {
            algs = append(algs, alg);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return algs;
}

} // end jwt_package
