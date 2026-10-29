// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using errors = errors_package;
using slices = slices_package;
using strings = strings_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using encoding;

partial class x509_package {

internal static (ж<CertPool>, error) loadSystemRoots() {
    return (Ꮡ(new CertPool(systemPool: true)), default);
}

// Creates a new *syscall.CertContext representing the leaf certificate in an in-memory
// certificate store containing itself and all of the intermediate certificates specified
// in the opts.Intermediates CertPool.
//
// A pointer to the in-memory store is available in the returned CertContext's Store field.
// The store is automatically freed when the CertContext is freed using
// syscall.CertFreeCertificateContext.
internal static (ж<syscall.CertContext>, error) createStoreContext(ж<Certificate> Ꮡleaf, ref VerifyOptions opts) {
    GoFrame ᒐ = default;
    try {
        ref var leaf = ref Ꮡleaf.DerefOrNull();

        ref var storeCtx = ref heap<ж<syscall.CertContext>>(out var ᏑstoreCtx);

        var (leafCtx, err) = syscall.CertCreateCertificateContext((uint32)((uint32)syscall.X509_ASN_ENCODING | (uint32)syscall.PKCS_7_ASN_ENCODING), Ꮡ(leaf.Raw, 0), (uint32)builtin.len(leaf.Raw));
        if (err != nil) {
            return (default, err);
        }
        defer(syscall.CertFreeCertificateContext, leafCtx, ref ᒐ);

        (var handle, err) = syscall.CertOpenStore(syscall.CERT_STORE_PROV_MEMORY, 0, 0, syscall.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 0);
        if (err != nil) {
            return (default, err);
        }
        defer(syscall.CertCloseStore, handle, (uint32)0, ref ᒐ);

        err = syscall.CertAddCertificateContextToStore(handle, leafCtx, syscall.CERT_STORE_ADD_ALWAYS, ᏑstoreCtx);
        if (err != nil) {
            return (default, err);
        }

        if (opts.Intermediates != nil) {
            for (nint i = 0; i < opts.Intermediates.len(); i++) {
                var (intermediate, _, errΔ1) = opts.Intermediates.cert(i);
                if (errΔ1 != nil) {
                    return (default, errΔ1);
                }
                (var ctx, errΔ1) = syscall.CertCreateCertificateContext((uint32)((uint32)syscall.X509_ASN_ENCODING | (uint32)syscall.PKCS_7_ASN_ENCODING), Ꮡ(intermediate.Raw, 0), (uint32)builtin.len(intermediate.Raw));
                if (errΔ1 != nil) {
                    return (default, errΔ1);
                }

                errΔ1 = syscall.CertAddCertificateContextToStore(handle, ctx, syscall.CERT_STORE_ADD_ALWAYS, nil);
                syscall.CertFreeCertificateContext(ctx);
                if (errΔ1 != nil) {
                    return (default, errΔ1);
                }
            }
        }

        return (storeCtx, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidSimpleChainˢ = "x509: invalid simple chain"u8;

// extractSimpleChain extracts the final certificate chain from a CertSimpleChain.
internal static (slice<ж<Certificate>> chain, error err) extractSimpleChain(ж<ж<syscall.CertSimpleChain>> simpleChain, nint count) {
    slice<ж<Certificate>> chain = default;
    var chainᴷ = new stackAppend<ж<Certificate>>(moves: true);

    if (simpleChain == nil || count == 0) {
        return (default, errors.New(x509InvalidSimpleChainˢ));
    }

    var simpleChains = @unsafe.Slice(simpleChain, count);
    var lastChain = simpleChains[count - 1];
    var elements = @unsafe.Slice(lastChain.Elements, lastChain.NumElements);
    for (nint i = 0; i < (nint)lastChain.NumElements; i++) {
        // Copy the buf, since ParseCertificate does not create its own copy.
        var cert = elements[i].CertContext;
        var encodedCert = @unsafe.Slice(cert.EncodedCert, cert.Length);
        var buf = bytes.Clone(encodedCert);
        var (parsedCert, errΔ1) = ParseCertificate(buf);
        if (errΔ1 != nil) {
            return (default, errΔ1);
        }
        chain = append(chainᴷ, chain, parsedCert);
    }

    chain = move2heap(chainᴷ, chain);
    return (chain, default);
}

// checkChainTrustStatus checks the trust status of the certificate chain, translating
// any errors it finds into Go errors in the process.
internal static error checkChainTrustStatus(ж<Certificate> c, ref syscall.CertChainContext chainCtx) {
    if (chainCtx.TrustStatus.ErrorStatus != syscall.CERT_TRUST_NO_ERROR) {
        var status = chainCtx.TrustStatus.ErrorStatus;
        switch (status) {
        case var ᴛ when ᴛ == syscall.CERT_TRUST_IS_NOT_TIME_VALID: {
            return new CertificateInvalidError(c, Expired, ""u8);
        }
        case var ᴛ when ᴛ == syscall.CERT_TRUST_IS_NOT_VALID_FOR_USAGE: {
            return new CertificateInvalidError(c, IncompatibleUsage, ""u8);
        }
        // TODO(filippo): surface more error statuses.
        default: {
            return new UnknownAuthorityError(c, default, nil);
        }}
    }
    return default;
}

// checkChainSSLServerPolicy checks that the certificate chain in chainCtx is valid for
// use as a certificate chain for a SSL/TLS server.
internal static error checkChainSSLServerPolicy(ж<Certificate> c, ж<syscall.CertChainContext> chainCtx, ref VerifyOptions opts) {
    var (servernamep, err) = syscall.UTF16PtrFromString(strings.TrimSuffix(opts.DNSName, "."u8));
    if (err != nil) {
        return err;
    }
    var sslPara = Ꮡ(new syscall.SSLExtraCertChainPolicyPara(
        AuthType: syscall.AUTHTYPE_SERVER,
        ServerName: servernamep
    ));
    sslPara.Size = (uint32)/* unsafe.Sizeof(*sslPara) */ (uintptr)24;

    var para = Ꮡ(new syscall.CertChainPolicyPara(
        ExtraPolicyPara: ((syscall.Pointer)ManagedPointerTokens.MintOpaque(sslPara))
    ));
    para.Size = (uint32)/* unsafe.Sizeof(*para) */ (uintptr)16;

    ref var status = ref heap<syscall.CertChainPolicyStatus>(new syscall.CertChainPolicyStatus(), out var Ꮡstatus);
    err = syscall.CertVerifyCertificateChainPolicy(syscall.CERT_CHAIN_POLICY_SSL, chainCtx, para, Ꮡstatus);
    if (err != nil) {
        return err;
    }

    // TODO(mkrautz): use the lChainIndex and lElementIndex fields
    // of the CertChainPolicyStatus to provide proper context, instead
    // using c.
    if (status.Error != 0) {
        switch (status.Error) {
        case var ᴛ when ᴛ == syscall.CERT_E_EXPIRED: {
            return new CertificateInvalidError(c, Expired, ""u8);
        }
        case var ᴛ when ᴛ == syscall.CERT_E_CN_NO_MATCH: {
            return new HostnameError(c, opts.DNSName);
        }
        case var ᴛ when ᴛ == syscall.CERT_E_UNTRUSTEDROOT: {
            return new UnknownAuthorityError(c, default, nil);
        }
        default: {
            return new UnknownAuthorityError(c, default, nil);
        }}
    }

    return default;
}

// windowsExtKeyUsageOIDs are the C NUL-terminated string representations of the
// OIDs for use with the Windows API.
internal static map<ExtKeyUsage, slice<byte>> windowsExtKeyUsageOIDs;
internal static void initᴛwindowsExtKeyUsageOIDs() { windowsExtKeyUsageOIDs = new map<ExtKeyUsage, slice<byte>>(builtin.len(extKeyUsageOIDs)); }

[GoInit] internal static void initΔ1() {
    foreach (var (_, eku) in extKeyUsageOIDs) {
        windowsExtKeyUsageOIDs[eku.extKeyUsage] = slice<byte>(eku.oid.String() + "\x00");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InternalErrorSystemˢ = "x509: internal error: system verifier returned an empty chain"u8;

internal static (slice<ж<Certificate>> chain, error err) verifyChain(ж<Certificate> c, ж<syscall.CertChainContext> ᏑchainCtx, ж<VerifyOptions> Ꮡopts) {
    slice<ж<Certificate>> chain = default;
    error err = default;
    ref var chainCtx = ref ᏑchainCtx.DerefOrNull();
    ref var opts = ref Ꮡopts.DerefOrNull();

    err = checkChainTrustStatus(c, ref (ᏑchainCtx).DerefOrNull());
    if (err != nil) {
        return (default, err);
    }

    if (Ꮡopts != nil && builtin.len(opts.DNSName) > 0) {
        err = checkChainSSLServerPolicy(c, ᏑchainCtx, ref (Ꮡopts).DerefOrNull());
        if (err != nil) {
            return (default, err);
        }
    }

    (chain, err) = extractSimpleChain(chainCtx.Chains, (nint)chainCtx.ChainCount);
    if (err != nil) {
        return (default, err);
    }
    if (builtin.len(chain) == 0) {
        return (default, errors.New(x509InternalErrorSystemˢ));
    }

    // Mitigate CVE-2020-0601, where the Windows system verifier might be
    // tricked into using custom curve parameters for a trusted root, by
    // double-checking all ECDSA signatures. If the system was tricked into
    // using spoofed parameters, the signature will be invalid for the correct
    // ones we parsed. (We don't support custom curves ourselves.)
    foreach (var (i, parent) in chain[1..]) {
        if (parent.PublicKeyAlgorithm != ECDSA) {
            continue;
        }
        {
            var errΔ1 = parent.CheckSignature(chain[i].SignatureAlgorithm,
                chain[i].RawTBSCertificate, chain[i].Signature); if (errΔ1 != nil) {
                return (default, errΔ1);
            }
        }
    }
    return (chain, default);
}

// systemVerify is like Verify, except that it uses CryptoAPI calls
// to build certificate chains and verify them.
internal static (slice<slice<ж<Certificate>>> chains, error err) systemVerify(this ж<Certificate> Ꮡc, ж<VerifyOptions> Ꮡopts) {
    slice<slice<ж<Certificate>>> chains = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var opts = ref Ꮡopts.DerefOrNull();

        (var storeCtx, err) = createStoreContext(Ꮡc, ref (Ꮡopts).DerefOrNull());
        if (err != nil) {
            (chains, err) = (default, err); goto ᒐdone;
        }
        defer(syscall.CertFreeCertificateContext, storeCtx, ref ᒐ);

        var para = @new<syscall.CertChainPara>();
        para.Size = (uint32)/* unsafe.Sizeof(*para) */ (uintptr)80;

        var keyUsages = opts.KeyUsages;
        if (builtin.len(keyUsages) == 0) {
            keyUsages = slice<ExtKeyUsage>([ExtKeyUsageServerAuth]);
        }
        var oids = new slice<ж<byte>>(0, builtin.len(keyUsages));
        foreach (var (_, eku) in keyUsages) {
            if (eku == ExtKeyUsageAny) {
                oids = default;
                break;
            }
            {
                var (oid, ok) = windowsExtKeyUsageOIDs[eku, ꟷ]; if (ok) {
                    oids = append(oids, Ꮡ(oid, 0));
                }
            }
        }
        if (oids != nil) {
            para.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_OR;
            para.RequestedUsage.Usage.Length = (uint32)builtin.len(oids);
            para.RequestedUsage.Usage.UsageIdentifiers = Ꮡ(oids, 0);
        } else {
            para.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_AND;
            para.RequestedUsage.Usage.Length = 0;
            para.RequestedUsage.Usage.UsageIdentifiers = default;
        }

        ж<syscall.Filetime> verifyTime = default;
        if (Ꮡopts != nil && !opts.CurrentTime.IsZero()) {
            ref var ft = ref heap<syscall.Filetime>(syscall.NsecToFiletime(opts.CurrentTime.UnixNano()), out var Ꮡft);
            verifyTime = Ꮡft;
        }

        // The default is to return only the highest quality chain,
        // setting this flag will add additional lower quality contexts.
        // These are returned in the LowerQualityChains field.
        const uint32 CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS = 0x00000080;

        // CertGetCertificateChain will traverse Windows's root stores in an attempt to build a verified certificate chain
        ref var topCtx = ref heap<ж<syscall.CertChainContext>>(out var ᏑtopCtx);
        err = syscall.CertGetCertificateChain((syscallꓸHandle)0, storeCtx, verifyTime, storeCtx.Store, para, CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS, 0, ᏑtopCtx);
        if (err != nil) {
            (chains, err) = (default, err); goto ᒐdone;
        }
        defer(syscall.CertFreeCertificateChain, topCtx, ref ᒐ);

        var (chain, topErr) = verifyChain(Ꮡc, topCtx, Ꮡopts);
        if (topErr == nil) {
            chains = append(chains, chain);
        }

        {
            var lqCtxCount = topCtx.LowerQualityChainCount; if (lqCtxCount > 0) {
                var lqCtxs = @unsafe.Slice(topCtx.LowerQualityChains, lqCtxCount);
                foreach (var (_, ctx) in lqCtxs) {
                    var (chainΔ1, errΔ1) = verifyChain(Ꮡc, ctx, Ꮡopts);
                    if (errΔ1 == nil) {
                        chains = append(chains, chainΔ1);
                    }
                }
            }
        }

        chains = slices.DeleteFunc<slice<slice<ж<Certificate>>>, slice<ж<Certificate>>>(chains, containsDistrustedSPKIHash);

        if (builtin.len(chains) == 0) {
            if (topErr == nil) {
                (chains, err) = (default, new CertificateInvalidError(Ꮡc, NoValidChains, ""u8)); goto ᒐdone;
            }
            // Return the error from the highest quality context.
            (chains, err) = (default, topErr); goto ᒐdone;
        }

        (chains, err) = (chains, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (chains, err);
}

} // end x509_package
