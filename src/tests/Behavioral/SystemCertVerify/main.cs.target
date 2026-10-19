namespace go;

using bytes = bytes_package;
using ecdsa = crypto.ecdsa_package;
using elliptic = crypto.elliptic_package;
using rand = crypto.rand_package;
using x509 = crypto.x509_package;
using pkix = crypto.x509.pkix_package;
using fmt = fmt_package;
using big = math.big_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using crypto;
using crypto.x509;
using math;
using Δio = io_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object generateKeyErrorˢ = (@string)"generate key error:"u8;
private static readonly object createCertificateErrorˢ = (@string)"create certificate error:"u8;
private static readonly object createdDerˢ = (@string)"created der:"u8;
private static readonly object parseCertificateErrorˢ = (@string)"parse certificate error:"u8;
private static readonly object parsedCnˢ = (@string)"parsed cn:"u8;
private static readonly object verifyChainsˢ = (@string)"verify chains:"u8;
private static readonly object verifyErrorˢ = (@string)"verify error:"u8;
private static readonly object unknownAuthorityˢ = (@string)"unknown authority:"u8;
private static readonly object verifyWithDnsnameErrorˢ = (@string)"verify with dnsname error:"u8;
private static readonly object hostnameMismatchˢ = (@string)"hostname mismatch:"u8;

internal static void Main() {
    var (key, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        fmt.Println(generateKeyErrorˢ, err);
        return;
    }

    var now = time.Now();
    var template = Ꮡ(new x509.Certificate(
        SerialNumber: big.NewInt(20260818),
        Subject: new pkix.Name(CommonName: "go2cs.example"),
        NotBefore: now.Add(-time.ΔHour),
        NotAfter: now.Add(time.ΔHour),
        KeyUsage: (x509.KeyUsage)(x509.KeyUsageDigitalSignature | x509.KeyUsageCertSign),
        ExtKeyUsage: new x509.ExtKeyUsage[]{x509.ExtKeyUsageServerAuth}.slice(),
        DNSNames: new @string[]{"go2cs.example"}.slice(),
        IsCA: true,
        BasicConstraintsValid: true
    ));

    (var der, err) = x509.CreateCertificate(rand.Reader, template, template, key.of(ecdsa.PrivateKey.ᏑPublicKey), key.OrTypedNil());
    if (err != default!) {
        fmt.Println(createCertificateErrorˢ, err);
        return;
    }
    fmt.Println(createdDerˢ, len(der) > 0);

    (var cert, err) = x509.ParseCertificate(der);
    if (err != default!) {
        fmt.Println(parseCertificateErrorˢ, err);
        return;
    }
    fmt.Println(parsedCnˢ, (~cert).Subject.CommonName);

    (var chains, err) = cert.Verify(new x509.VerifyOptions(nil));
    fmt.Println(verifyChainsˢ, len(chains));
    fmt.Println(verifyErrorˢ, err);
    fmt.Println(unknownAuthorityˢ, isUnknownAuthority(err));

    (_, err) = cert.Verify(new x509.VerifyOptions(DNSName: "go2cs.example"));
    fmt.Println(verifyWithDnsnameErrorˢ, err);

    fmt.Println(hostnameMismatchˢ, cert.VerifyHostname("other.example") != default!);

    walkChain(der);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object createContextErrorˢ = (@string)"create context error:"u8;
private static readonly object openStoreErrorˢ = (@string)"open store error:"u8;
private static readonly object addToStoreErrorˢ = (@string)"add to store error:"u8;
private static readonly object storeContextNonNilˢ = (@string)"store context non-nil:"u8;
private static readonly object storeHandleRoundTripsˢ = (@string)"store handle round-trips:"u8;
private static readonly object storeContextDerLengthˢ = (@string)"store context der length:"u8;
private static readonly object getChainErrorˢ = (@string)"get chain error:"u8;
private static readonly object chainCountˢ = (@string)"chain count:"u8;
private static readonly object chainReportsUntrustedˢ = (@string)"chain reports untrusted root:"u8;
private static readonly object simpleChainElementsˢ = (@string)"simple chain elements:"u8;
private static readonly object leafDerRoundTripsˢ = (@string)"leaf der round-trips:"u8;

internal static void walkChain(slice<byte> der) {
    GoFrame ᒐ = default;
    try {
        var (ctx, err) = syscall.CertCreateCertificateContext((uint32)((uint32)syscall.X509_ASN_ENCODING | (uint32)syscall.PKCS_7_ASN_ENCODING), Ꮡ(der, 0), (uint32)len(der));
        if (err != default!) {
            fmt.Println(createContextErrorˢ, err);
            return;
        }
        defer(syscall.CertFreeCertificateContext, ctx, ref ᒐ);

        (var store, err) = syscall.CertOpenStore(syscall.CERT_STORE_PROV_MEMORY, 0, 0, syscall.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 0);
        if (err != default!) {
            fmt.Println(openStoreErrorˢ, err);
            return;
        }
        defer(syscall.CertCloseStore, store, (uint32)(0), ref ᒐ);

        ref var storeCtx = ref heap<ж<syscall.CertContext>>(out var ᏑstoreCtx);
        {
            var errΔ1 = syscall.CertAddCertificateContextToStore(store, ctx, syscall.CERT_STORE_ADD_ALWAYS, ᏑstoreCtx); if (errΔ1 != default!) {
                fmt.Println(addToStoreErrorˢ, errΔ1);
                return;
            }
        }
        defer(syscall.CertFreeCertificateContext, storeCtx, ref ᒐ);

        fmt.Println(storeContextNonNilˢ, storeCtx != nil);
        fmt.Println(storeHandleRoundTripsˢ, (~storeCtx).Store == store);
        fmt.Println(storeContextDerLengthˢ, (nint)(~storeCtx).Length == len(der));

        var serverAuth = slice<byte>("1.3.6.1.5.5.7.3.1\x00"u8);
        var oids = new ж<byte>[]{Ꮡ(serverAuth, 0)}.slice();

        var para = @new<syscall.CertChainPara>();
        para.Value.Size = (uint32)/* unsafe.Sizeof(*para) */ (uintptr)80;
        para.Value.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_OR;
        para.Value.RequestedUsage.Usage.Length = (uint32)len(oids);
        para.Value.RequestedUsage.Usage.UsageIdentifiers = Ꮡ(oids, 0);

        ref var chainCtx = ref heap<ж<syscall.CertChainContext>>(out var ᏑchainCtx);
        {
            var errΔ2 = syscall.CertGetCertificateChain(((syscallꓸHandle)0), storeCtx, nil, (~storeCtx).Store, para, 0, 0, ᏑchainCtx); if (errΔ2 != default!) {
                fmt.Println(getChainErrorˢ, errΔ2);
                return;
            }
        }
        defer(syscall.CertFreeCertificateChain, chainCtx, ref ᒐ);

        fmt.Println(chainCountˢ, (~chainCtx).ChainCount);
        fmt.Println(chainReportsUntrustedˢ, (uint32)((~chainCtx).TrustStatus.ErrorStatus & (uint32)syscall.CERT_TRUST_IS_UNTRUSTED_ROOT) != 0);

        var simpleChains = @unsafe.Slice((~chainCtx).Chains, (~chainCtx).ChainCount);
        var last = simpleChains[(~chainCtx).ChainCount - 1];
        fmt.Println(simpleChainElementsˢ, (~last).NumElements);

        var elements = @unsafe.Slice((~last).Elements, (~last).NumElements);
        var leaf = elements[0].Value.CertContext;
        var encoded = @unsafe.Slice((~leaf).EncodedCert, (~leaf).Length);
        fmt.Println(leafDerRoundTripsˢ, bytes.Equal(encoded, der));

        checkSSLPolicy(chainCtx);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object policyMatchˢ = (@string)"policy match:"u8;
private static readonly object policyMismatchˢ = (@string)"policy mismatch:"u8;
private static readonly object policyUntrustedˢ = (@string)"policy untrusted:"u8;

internal static void checkSSLPolicy(ж<syscall.CertChainContext> ᏑchainCtx) {
    const uint32 allowUnknownCA = 0x00000010;

    fmt.Println(policyMatchˢ, sslPolicyError(ᏑchainCtx, "go2cs.example", allowUnknownCA));
    fmt.Println(policyMismatchˢ, sslPolicyError(ᏑchainCtx, "other.example", allowUnknownCA));
    fmt.Println(policyUntrustedˢ, sslPolicyError(ᏑchainCtx, "go2cs.example", 0));
}

internal static @string sslPolicyError(ж<syscall.CertChainContext> ᏑchainCtx, @string serverName, uint32 flags) {
    var (servernamep, err) = syscall.UTF16PtrFromString(serverName);
    if (err != default!) {
        return "utf16 error";
    }

    var sslPara = Ꮡ(new syscall.SSLExtraCertChainPolicyPara(
        AuthType: syscall.AUTHTYPE_SERVER,
        ServerName: servernamep
    ));
    sslPara.Value.Size = (uint32)/* unsafe.Sizeof(*sslPara) */ (uintptr)24;

    var para = Ꮡ(new syscall.CertChainPolicyPara(
        Flags: flags,
        ExtraPolicyPara: ((syscall.Pointer)ManagedPointerTokens.MintOpaque(sslPara))
    ));
    para.Value.Size = (uint32)/* unsafe.Sizeof(*para) */ (uintptr)16;

    ref var status = ref heap<syscall.CertChainPolicyStatus>(out var Ꮡstatus);
    status = new syscall.CertChainPolicyStatus(nil);
    {
        var errΔ1 = syscall.CertVerifyCertificateChainPolicy(syscall.CERT_CHAIN_POLICY_SSL, ᏑchainCtx, para, Ꮡstatus); if (errΔ1 != default!) {
            return "call error: "u8 + errΔ1.Error();
        }
    }

    var exprᴛ1 = status.Error;
    if (exprᴛ1 is 0) {
        return "ok";
    }
    if (exprᴛ1 == syscall.CERT_E_CN_NO_MATCH) {
        return "cn mismatch";
    }
    if (exprᴛ1 == syscall.CERT_E_UNTRUSTEDROOT) {
        return "untrusted root";
    }
    { /* default: */
        return fmt.Sprintf("0x%08x"u8, status.Error);
    }

}

internal static bool isUnknownAuthority(error err) {
    var (_, ok) = err._<x509.UnknownAuthorityError>(ᐧ);
    return ok;
}

} // end main_package
