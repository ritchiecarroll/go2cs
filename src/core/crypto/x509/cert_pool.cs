// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using sha256 = go.crypto.sha256_package;
using pem = encoding.pem_package;
using sync = sync_package;
using encoding;
using go.crypto;

partial class x509_package {

/* [sha256.Size224]byte */
partial struct sum224 /*[28]byte*/;

// CertPool is a set of certificates.
partial struct CertPool {
    internal map<@string, slice<nint>> byName; // cert.RawSubject => index into lazyCerts
    // lazyCerts contains funcs that return a certificate,
    // lazily parsing/decompressing it as needed.
    internal slice<lazyCert> lazyCerts;
    // haveSum maps from sum224(cert.Raw) to true. It's used only
    // for AddCert duplicate detection, to avoid CertPool.contains
    // calls in the AddCert path (because the contains method can
    // call getCert and otherwise negate savings from lazy getCert
    // funcs).
    internal map<sum224, bool> haveSum;
    // systemPool indicates whether this is a special pool derived from the
    // system roots. If it includes additional roots, it requires doing two
    // verifications, one using the roots provided by the caller, and one using
    // the system platform verifier.
    internal bool systemPool;
}

// lazyCert is minimal metadata about a Cert and a func to retrieve it
// in its normal expanded *Certificate form.
partial struct lazyCert {
    // rawSubject is the Certificate.RawSubject value.
    // It's the same as the CertPool.byName key, but in []byte
    // form to make CertPool.Subjects (as used by crypto/tls) do
    // fewer allocations.
    internal slice<byte> rawSubject;
    // constraint is a function to run against a chain when it is a candidate to
    // be added to the chain. This allows adding arbitrary constraints that are
    // not specified in the certificate itself.
    internal Func<slice<ж<Certificate>>, error> constraint;
    // getCert returns the certificate.
    //
    // It is not meant to do network operations or anything else
    // where a failure is likely; the func is meant to lazily
    // parse/decompress data that is already known to be good. The
    // error in the signature primarily is meant for use in the
    // case where a cert file existed on local disk when the program
    // started up is deleted later before it's read.
    internal Func<(ж<Certificate>, error)> getCert;
}

// NewCertPool returns a new, empty CertPool.
public static ж<CertPool> NewCertPool() {
    return Ꮡ(new CertPool(
        byName: new map<@string, slice<nint>>(),
        haveSum: new map<sum224, bool>()
    ));
}

// len returns the number of certs in the set.
// A nil set is a valid empty set.
internal static nint len(this ж<CertPool> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (Ꮡs == nil) {
        return 0;
    }
    return builtin.len(s.lazyCerts);
}

// cert returns cert index n in s.
internal static (ж<Certificate>, Func<slice<ж<Certificate>>, error>, error) cert(this ref CertPool s, nint n) {
    var (cert, err) = s.lazyCerts[n].getCert();
    return (cert, s.lazyCerts[n].constraint, err);
}

// Clone returns a copy of s.
public static ж<CertPool> Clone(this ref CertPool s) {
    var p = Ꮡ(new CertPool(
        byName: new map<@string, slice<nint>>(builtin.len(s.byName)),
        lazyCerts: new slice<lazyCert>(builtin.len(s.lazyCerts)),
        haveSum: new map<sum224, bool>(builtin.len(s.haveSum)),
        systemPool: s.systemPool
    ));
    foreach (var (k, v) in s.byName) {
        var indexes = new slice<nint>(builtin.len(v));
        copy(indexes, v);
        p.Value.byName[k] = indexes;
    }
    foreach (var (kᴛ1, _) in s.haveSum) {
        var k = kᴛ1.Clone();

        p.Value.haveSum[k] = true;
    }
    copy((~p).lazyCerts, s.lazyCerts);
    return p;
}

// SystemCertPool returns a copy of the system cert pool.
//
// On Unix systems other than macOS the environment variables SSL_CERT_FILE and
// SSL_CERT_DIR can be used to override the system default locations for the SSL
// certificate file and SSL certificate files directory, respectively. The
// latter can be a colon-separated list.
//
// Any mutations to the returned pool are not written to disk and do not affect
// any other pool returned by SystemCertPool.
//
// New changes in the system cert pool might not be reflected in subsequent calls.
public static (ж<CertPool>, error) SystemCertPool() {
    {
        var sysRoots = systemRootsPool(); if (sysRoots != nil) {
            return (sysRoots.Clone(), default!);
        }
    }
    return loadSystemRoots();
}

partial struct potentialParent {
    internal ж<Certificate> cert;
    internal Func<slice<ж<Certificate>>, error> constraint;
}

// findPotentialParents returns the certificates in s which might have signed
// cert.
internal static slice<potentialParent> findPotentialParents(this ж<CertPool> Ꮡs, ж<Certificate> Ꮡcert) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();

    if (Ꮡs == nil) {
        return default!;
    }
    // consider all candidates where cert.Issuer matches cert.Subject.
    // when picking possible candidates the list is built in the order
    // of match plausibility as to save cycles in buildChains:
    //   AKID and SKID match
    //   AKID present, SKID missing / AKID missing, SKID present
    //   AKID and SKID don't match
    slice<potentialParent> matchingKeyID = default!;
    slice<potentialParent> oneKeyID = default!;
    slice<potentialParent> mismatchKeyID = default!;
    foreach (var (_, c) in s.byName[tmpstring(cert.RawIssuer)]) {
        var (candidate, constraint, err) = s.cert(c);
        if (err != default!) {
            continue;
        }
        var kidMatch = bytes.Equal((~candidate).SubjectKeyId, cert.AuthorityKeyId);
        switch (ᐧ) {
        case {} when kidMatch: {
            matchingKeyID = append(matchingKeyID, new potentialParent(candidate, constraint));
            break;
        }
        case {} when (builtin.len((~candidate).SubjectKeyId) == 0 && builtin.len(cert.AuthorityKeyId) > 0) || (builtin.len((~candidate).SubjectKeyId) > 0 && builtin.len(cert.AuthorityKeyId) == 0): {
            oneKeyID = append(oneKeyID, new potentialParent(candidate, constraint));
            break;
        }
        default: {
            mismatchKeyID = append(mismatchKeyID, new potentialParent(candidate, constraint));
            break;
        }}

    }
    nint found = builtin.len(matchingKeyID) + builtin.len(oneKeyID) + builtin.len(mismatchKeyID);
    if (found == 0) {
        return default!;
    }
    var candidates = new slice<potentialParent>(0, found);
    candidates = appendꓸꓸꓸ(candidates, matchingKeyID);
    candidates = appendꓸꓸꓸ(candidates, oneKeyID);
    candidates = appendꓸꓸꓸ(candidates, mismatchKeyID);
    return candidates;
}

internal static bool contains(this ж<CertPool> Ꮡs, ж<Certificate> Ꮡcert) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();

    if (Ꮡs == nil) {
        return false;
    }
    return s.haveSum[sha256.Sum224(cert.Raw)];
}

// AddCert adds a certificate to a pool.
public static void AddCert(this ref CertPool s, ж<Certificate> Ꮡcert) {
    ref var cert = ref Ꮡcert.DerefOrNull();

    if (Ꮡcert == nil) {
        throw panic("adding nil Certificate to CertPool");
    }
    s.addCertFunc(sha256.Sum224(cert.Raw), ((@string)cert.RawSubject), () => (Ꮡcert, default!), default!);
}

// addCertFunc adds metadata about a certificate to a pool, along with
// a func to fetch that certificate later when needed.
//
// The rawSubject is Certificate.RawSubject and must be non-empty.
// The getCert func may be called 0 or more times.
internal static void addCertFunc(this ref CertPool s, sum224 rawSum224, @string rawSubject, Func<(ж<Certificate>, error)> getCert, Func<slice<ж<Certificate>>, error> constraint) {
    rawSum224 = rawSum224.Clone();

    if (getCert == default!) {
        throw panic("getCert can't be nil");
    }
    // Check that the certificate isn't being added twice.
    if (s.haveSum[rawSum224]) {
        return;
    }
    s.haveSum[rawSum224] = true;
    s.lazyCerts = append(s.lazyCerts, new lazyCert(
        rawSubject: slice<byte>(rawSubject),
        getCert: getCert,
        constraint: constraint
    ));
    s.byName[rawSubject] = append(s.byName[rawSubject], builtin.len(s.lazyCerts) - 1);
}

internal partial struct AppendCertsFromPEM_lazyCert /*dyn*/ {
    public partial ref sync_package.Once Once { get; }
    internal ж<Certificate> v;
}

// AppendCertsFromPEM attempts to parse a series of PEM encoded certificates.
// It appends any certificates found to s and reports whether any certificates
// were successfully parsed.
//
// On many Linux systems, /etc/ssl/cert.pem will contain the system wide set
// of root CAs in a format suitable for this function.
public static bool /*ok*/ AppendCertsFromPEM(this ref CertPool s, slice<byte> pemCerts) {
    bool ok = default!;

    while (builtin.len(pemCerts) > 0) {
        ж<pem.Block> block = default!;
        (block, pemCerts) = pem.Decode(pemCerts);
        if (block == nil) {
            break;
        }
        if ((~block).Type != "CERTIFICATE"u8 || builtin.len((~block).Headers) != 0) {
            continue;
        }
        ref var certBytes = ref heap<slice<byte>>(out var ᏑcertBytes);
        certBytes = block.Value.Bytes;
        var (cert, err) = ParseCertificate(certBytes);
        if (err != default!) {
            continue;
        }
        ref var lazyCert = ref heap(new AppendCertsFromPEM_lazyCert(), out var ᏑlazyCert);
        s.addCertFunc(sha256.Sum224((~cert).Raw), ((@string)(~cert).RawSubject), () => {
            ᏑlazyCert.of(AppendCertsFromPEM_lazyCert.ᏑOnce).Do(() => {
                // This can't fail, as the same bytes already parsed above.
                (ᏑlazyCert.Value.v, _) = ParseCertificate(ᏑcertBytes.ValueSlot);
                ᏑcertBytes.ValueSlot = default!;
            });
            return (ᏑlazyCert.Value.v, default!);
        }, default!);
        ok = true;
    }
    return ok;
}

// Subjects returns a list of the DER-encoded subjects of
// all of the certificates in the pool.
//
// Deprecated: if s was returned by [SystemCertPool], Subjects
// will not include the system roots.
public static slice<slice<byte>> Subjects(this ж<CertPool> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var res = new slice<slice<byte>>(Ꮡs.len());
    foreach (var (i, lc) in s.lazyCerts) {
        res[i] = lc.rawSubject;
    }
    return res;
}

// Equal reports whether s and other are equal.
public static bool Equal(this ж<CertPool> Ꮡs, ж<CertPool> Ꮡother) {
    ref var s = ref Ꮡs.DerefOrNull();
    ref var other = ref Ꮡother.DerefOrNull();

    if (Ꮡs == nil || Ꮡother == nil) {
        return Ꮡs == Ꮡother;
    }
    if (s.systemPool != other.systemPool || builtin.len(s.haveSum) != builtin.len(other.haveSum)) {
        return false;
    }
    foreach (var (kᴛ1, _) in s.haveSum) {
        var h = kᴛ1.Clone();

        if (!other.haveSum[h]) {
            return false;
        }
    }
    return true;
}

// AddCertWithConstraint adds a certificate to the pool with the additional
// constraint. When Certificate.Verify builds a chain which is rooted by cert,
// it will additionally pass the whole chain to constraint to determine its
// validity. If constraint returns a non-nil error, the chain will be discarded.
// constraint may be called concurrently from multiple goroutines.
public static void AddCertWithConstraint(this ref CertPool s, ж<Certificate> Ꮡcert, Func<slice<ж<Certificate>>, error> constraint) {
    ref var cert = ref Ꮡcert.DerefOrNull();

    if (Ꮡcert == nil) {
        throw panic("adding nil Certificate to CertPool");
    }
    s.addCertFunc(sha256.Sum224(cert.Raw), ((@string)cert.RawSubject), () => (Ꮡcert, default!), constraint);
}

} // end x509_package
