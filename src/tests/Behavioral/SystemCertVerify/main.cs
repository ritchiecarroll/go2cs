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

internal static void Main() {
    var (key, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != nil) {
        fmt.Println("generate key error:", err);
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
    if (err != nil) {
        fmt.Println("create certificate error:", err);
        return;
    }
    fmt.Println("created der:", len(der) > 0);

    (var cert, err) = x509.ParseCertificate(der);
    if (err != nil) {
        fmt.Println("parse certificate error:", err);
        return;
    }
    fmt.Println("parsed cn:", (~cert).Subject.CommonName);

    (var chains, err) = cert.Verify(new x509.VerifyOptions());
    fmt.Println("verify chains:", len(chains));
    fmt.Println("verify error:", err);
    fmt.Println("unknown authority:", isUnknownAuthority(err));

    (_, err) = cert.Verify(new x509.VerifyOptions(DNSName: "go2cs.example"));
    fmt.Println("verify with dnsname error:", err);

    fmt.Println("hostname mismatch:", cert.VerifyHostname("other.example") != nil);

    walkChain(der);
}

internal static void walkChain(slice<byte> der) {
    GoFrame ᒐ = default;
    try {
        var (ctx, err) = syscall.CertCreateCertificateContext((uint32)((uint32)syscall.X509_ASN_ENCODING | (uint32)syscall.PKCS_7_ASN_ENCODING), Ꮡ(der, 0), (uint32)len(der));
        if (err != nil) {
            fmt.Println("create context error:", err);
            return;
        }
        defer(syscall.CertFreeCertificateContext, ctx, ref ᒐ);

        (var store, err) = syscall.CertOpenStore(syscall.CERT_STORE_PROV_MEMORY, 0, 0, syscall.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 0);
        if (err != nil) {
            fmt.Println("open store error:", err);
            return;
        }
        defer(ᴛ1 => syscall.CertCloseStore(ᴛ1, 0), store, ref ᒐ);

        ref var storeCtx = ref heap<ж<syscall.CertContext>>(out var ᏑstoreCtx);
        {
            var errΔ1 = syscall.CertAddCertificateContextToStore(store, ctx, syscall.CERT_STORE_ADD_ALWAYS, ᏑstoreCtx); if (errΔ1 != nil) {
                fmt.Println("add to store error:", errΔ1);
                return;
            }
        }
        defer(syscall.CertFreeCertificateContext, storeCtx, ref ᒐ);

        fmt.Println("store context non-nil:", storeCtx != nil);
        fmt.Println("store handle round-trips:", (~storeCtx).Store == store);
        fmt.Println("store context der length:", (nint)(~storeCtx).Length == len(der));

        var serverAuth = slice<byte>("1.3.6.1.5.5.7.3.1\x00"u8);
        var oids = new ж<byte>[]{Ꮡ(serverAuth, 0)}.slice();

        var para = @new<syscall.CertChainPara>();
        para.Value.Size = (uint32)/* unsafe.Sizeof(*para) */ (uintptr)80;
        para.Value.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_OR;
        para.Value.RequestedUsage.Usage.Length = (uint32)len(oids);
        para.Value.RequestedUsage.Usage.UsageIdentifiers = Ꮡ(oids, 0);

        ref var chainCtx = ref heap<ж<syscall.CertChainContext>>(out var ᏑchainCtx);
        {
            var errΔ2 = syscall.CertGetCertificateChain((syscallꓸHandle)0, storeCtx, nil, (~storeCtx).Store, para, 0, 0, ᏑchainCtx); if (errΔ2 != nil) {
                fmt.Println("get chain error:", errΔ2);
                return;
            }
        }
        defer(syscall.CertFreeCertificateChain, chainCtx, ref ᒐ);

        fmt.Println("chain count:", (~chainCtx).ChainCount);
        fmt.Println("chain reports untrusted root:", ((~chainCtx).TrustStatus.ErrorStatus & (uint32)syscall.CERT_TRUST_IS_UNTRUSTED_ROOT) != 0);

        var simpleChains = @unsafe.Slice((~chainCtx).Chains, (~chainCtx).ChainCount);
        var last = simpleChains[(~chainCtx).ChainCount - 1];
        fmt.Println("simple chain elements:", (~last).NumElements);

        var elements = @unsafe.Slice((~last).Elements, (~last).NumElements);
        var leaf = elements[0].Value.CertContext;
        var encoded = @unsafe.Slice((~leaf).EncodedCert, (~leaf).Length);
        fmt.Println("leaf der round-trips:", bytes.Equal(encoded, der));

        checkSSLPolicy(chainCtx);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void checkSSLPolicy(ж<syscall.CertChainContext> chainCtx) {
    const uint32 allowUnknownCA = 0x00000010;

    fmt.Println("policy match:", sslPolicyError(chainCtx, "go2cs.example", allowUnknownCA));
    fmt.Println("policy mismatch:", sslPolicyError(chainCtx, "other.example", allowUnknownCA));
    fmt.Println("policy untrusted:", sslPolicyError(chainCtx, "go2cs.example", 0));
}

internal static @string sslPolicyError(ж<syscall.CertChainContext> chainCtx, @string serverName, uint32 flags) {
    var (servernamep, err) = syscall.UTF16PtrFromString(serverName);
    if (err != nil) {
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

    ref var status = ref heap<syscall.CertChainPolicyStatus>(new syscall.CertChainPolicyStatus(), out var Ꮡstatus);
    {
        var errΔ1 = syscall.CertVerifyCertificateChainPolicy(syscall.CERT_CHAIN_POLICY_SSL, chainCtx, para, Ꮡstatus); if (errΔ1 != nil) {
            return "call error: "u8 + errΔ1.Error();
        }
    }

    switch (status.Error) {
    case 0: {
        return "ok";
    }
    case var ᴛ when ᴛ == syscall.CERT_E_CN_NO_MATCH: {
        return "cn mismatch";
    }
    case var ᴛ when ᴛ == syscall.CERT_E_UNTRUSTEDROOT: {
        return "untrusted root";
    }
    default: {
        return fmt.Sprintf("0x%08x"u8, status.Error);
    }}

}

internal static bool isUnknownAuthority(error err) {
    var (_, ok) = err._<x509.UnknownAuthorityError>(ᐧ);
    return ok;
}

} // end main_package
