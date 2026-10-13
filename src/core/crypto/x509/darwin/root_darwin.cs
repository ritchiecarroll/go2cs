// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using macOS = go.crypto.x509.@internal.macOS_package;
using errors = errors_package;
using fmt = fmt_package;
using time = time_package;

partial class x509_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidLeafCertificateˢ = "invalid leaf certificate"u8;
internal static readonly @string x509MacOSCertificateˢ = "x509: macOS certificate verification internal error"u8;

internal static (slice<slice<ж<Certificate>>> chains, error err) systemVerify(this ж<Certificate> Ꮡc, ж<VerifyOptions> Ꮡopts) {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        ref var opts = ref Ꮡopts.DerefOrNull();

        var certs = macOS.CFArrayCreateMutable();
        defer(macOS.ReleaseCFArray, certs, ref ᒐ);
        (var leaf, err) = macOS.SecCertificateCreateWithData(c.Raw);
        if (err != default!) {
            (chains, err) = (default!, errors.New(invalidLeafCertificateˢ)); goto ᒐdone;
        }
        macOS.CFArrayAppendValue(certs, leaf);
        if (opts.Intermediates != nil) {
            foreach (var (_, lc) in (~opts.Intermediates).lazyCerts) {
                var (cΔ1, errΔ1) = lc.getCert();
                if (errΔ1 != default!) {
                    (chains, err) = (default!, errΔ1); goto ᒐdone;
                }
                (var sc, errΔ1) = macOS.SecCertificateCreateWithData((~cΔ1).Raw);
                if (errΔ1 != default!) {
                    (chains, err) = (default!, errΔ1); goto ᒐdone;
                }
                macOS.CFArrayAppendValue(certs, sc);
            }
        }

        var policies = macOS.CFArrayCreateMutable();
        defer(macOS.ReleaseCFArray, policies, ref ᒐ);
        (var sslPolicy, err) = macOS.SecPolicyCreateSSL(opts.DNSName);
        if (err != default!) {
            (chains, err) = (default!, err); goto ᒐdone;
        }
        macOS.CFArrayAppendValue(policies, sslPolicy);

        (var trustObj, err) = macOS.SecTrustCreateWithCertificates(certs, policies);
        if (err != default!) {
            (chains, err) = (default!, err); goto ᒐdone;
        }
        defer(macOS.CFRelease, trustObj, ref ᒐ);

        if (!opts.CurrentTime.IsZero()) {
            var dateRef = macOS.TimeToCFDateRef(opts.CurrentTime);
            defer(macOS.CFRelease, dateRef, ref ᒐ);
            {
                var errΔ2 = macOS.SecTrustSetVerifyDate(trustObj, dateRef); if (errΔ2 != default!) {
                    (chains, err) = (default!, errΔ2); goto ᒐdone;
                }
            }
        }

        // TODO(roland): we may want to allow passing in SCTs via VerifyOptions and
        // set them via SecTrustSetSignedCertificateTimestamps, since Apple will
        // always enforce its SCT requirements, and there are still _some_ people
        // using TLS or OCSP for that.

        {
            var (ret, errΔ3) = macOS.SecTrustEvaluateWithError(trustObj); if (errΔ3 != default!) {
                var exprᴛ1 = ret;
                if (exprᴛ1 == macOS.ErrSecCertificateExpired) {
                    (chains, err) = (default!, new CertificateInvalidError(Ꮡc, Expired, errΔ3.Error())); goto ᒐdone;
                }
                if (exprᴛ1 == macOS.ErrSecHostNameMismatch) {
                    (chains, err) = (default!, new HostnameError(Ꮡc, opts.DNSName)); goto ᒐdone;
                }
                if (exprᴛ1 == macOS.ErrSecNotTrusted) {
                    (chains, err) = (default!, new UnknownAuthorityError(Cert: Ꮡc)); goto ᒐdone;
                }
                { /* default: */
                    (chains, err) = (default!, fmt.Errorf("x509: %s"u8, errΔ3)); goto ᒐdone;
                }
            }
        }

        var chain = new slice<ж<Certificate>>[]{new ж<Certificate>[]{}.slice()}.slice();
        nint numCerts = macOS.SecTrustGetCertificateCount(trustObj);
        for (nint i = 0; i < numCerts; i++) {
            var (certRef, errΔ4) = macOS.SecTrustGetCertificateAtIndex(trustObj, i);
            if (errΔ4 != default!) {
                (chains, err) = (default!, errΔ4); goto ᒐdone;
            }
            (var cert, errΔ4) = exportCertificate(certRef);
            if (errΔ4 != default!) {
                (chains, err) = (default!, errΔ4); goto ᒐdone;
            }
            chain[0] = append(chain[0], cert);
        }
        if (builtin.len(chain[0]) == 0) {
            // This should _never_ happen, but to be safe
            (chains, err) = (default!, errors.New(x509MacOSCertificateˢ)); goto ᒐdone;
        }

        if (opts.DNSName != ""u8) {
            // If we have a DNS name, apply our own name verification
            {
                var errΔ5 = chain[0][0].VerifyHostname(opts.DNSName); if (errΔ5 != default!) {
                    (chains, err) = (default!, errΔ5); goto ᒐdone;
                }
            }
        }

        var keyUsages = opts.KeyUsages;
        if (builtin.len(keyUsages) == 0) {
            keyUsages = new ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
        }

        // If any key usage is acceptable then we're done.
        foreach (var (_, usage) in keyUsages) {
            if (usage == ExtKeyUsageAny) {
                (chains, err) = (chain, default!); goto ᒐdone;
            }
        }

        if (!checkChainForKeyUsage(chain[0], keyUsages)) {
            (chains, err) = (default!, new CertificateInvalidError(Ꮡc, IncompatibleUsage, ""u8)); goto ᒐdone;
        }

        (chains, err) = (chain, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (chains, err);
}

// exportCertificate returns a *Certificate for a SecCertificateRef.
internal static (ж<Certificate>, error) exportCertificate(macOS.CFRef cert) {
    var (data, err) = macOS.SecCertificateCopyData(cert);
    if (err != default!) {
        return (default!, err);
    }
    return ParseCertificate(data);
}

internal static (ж<CertPool>, error) loadSystemRoots() {
    return (Ꮡ(new CertPool(systemPool: true)), default!);
}

} // end x509_package
