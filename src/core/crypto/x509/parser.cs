// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using dsa = go.crypto.dsa_package;
using ecdh = go.crypto.ecdh_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rsa = go.crypto.rsa_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = go.@internal.godebug_package;
using big = go.math.big_package;
using net = net_package;
using url = go.net.url_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = vendor.golang.org.x.crypto.cryptobyte.asn1_package;
using encoding;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using go.math;
using go.net;
using go.unicode;
using vendor.golang.org.x.crypto;
using vendor.golang.org.x.crypto.cryptobyte;

partial class x509_package {

// isPrintable reports whether the given b is in the ASN.1 PrintableString set.
// This is a simplified version of encoding/asn1.isPrintable.
internal static bool isPrintable(byte b) {
    return (rune)'a' <= b && b <= (rune)'z' || (rune)'A' <= b && b <= (rune)'Z' || (rune)'0' <= b && b <= (rune)'9' || (rune)'\'' <= b && b <= (rune)')' || (rune)'+' <= b && b <= (rune)'/' || b == (rune)' ' || b == (rune)':' || b == (rune)'=' || b == (rune)'?' || b == (rune)'*' || b == (rune)'&';
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPrintableStringˢ = "invalid PrintableString"u8;
internal static readonly @string invalidUtf8Stringˢ = "invalid UTF-8 string"u8;
internal static readonly @string invalidBMPStringˢ = "invalid BMPString"u8;
internal static readonly @string invalidIA5Stringˢ = "invalid IA5String"u8;
internal static readonly @string invalidNumericStringˢ = "invalid NumericString"u8;

// This is technically not allowed in a PrintableString.
// However, x509 certificates with wildcard strings don't
// always use the correct string type so we permit it.
// This is not technically allowed either. However, not
// only is it relatively common, but there are also a
// handful of CA certificates that contain it. At least
// one of which will not expire until 2027.

// parseASN1String parses the ASN.1 string types T61String, PrintableString,
// UTF8String, BMPString, IA5String, and NumericString. This is mostly copied
// from the respective encoding/asn1.parse... methods, rather than just
// increasing the API surface of that package.
internal static (@string, error) parseASN1String(cryptobyte_asn1.Tag tag, slice<byte> value) {
    var exprᴛ1 = tag;
    if (exprᴛ1 == cryptobyte_asn1.T61String) {
        return (((@string)value), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.PrintableString) {
        foreach (var (_, b) in value) {
            if (!isPrintable(b)) {
                return ("", errors.New(invalidPrintableStringˢ));
            }
        }
        return (((@string)value), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.UTF8String) {
        if (!utf8.Valid(value)) {
            return ("", errors.New(invalidUtf8Stringˢ));
        }
        return (((@string)value), default!);
    }
    if (exprᴛ1 == ((cryptobyte_asn1.Tag)asn1.TagBMPString)) {
        if (builtin.len(value) % 2 != 0) {
            return ("", errors.New(invalidBMPStringˢ));
        }
        {
            nint l = builtin.len(value); if (l >= 2 && value[l - 1] == 0 && value[l - 2] == 0) {
                // Strip terminator if present.
                value = value.slice(0, l - 2);
            }
        }
        var s = new slice<uint16>(0, builtin.len(value) / 2);
        while (builtin.len(value) > 0) {
            s = append(s, (uint16)((uint16)((uint16)value[0] << (int)(8)) + (uint16)value[1]));
            value = value[2..];
        }
        return (((@string)utf16.Decode(s)), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.IA5String) {
        @string s = ((@string)value);
        if (isIA5String(s) != default!) {
            return ("", errors.New(invalidIA5Stringˢ));
        }
        return (s, default!);
    }
    if (exprᴛ1 == ((cryptobyte_asn1.Tag)asn1.TagNumericString)) {
        foreach (var (_, b) in value) {
            if (!((rune)'0' <= b && b <= (rune)'9' || b == (rune)' ')) {
                return ("", errors.New(invalidNumericStringˢ));
            }
        }
        return (((@string)value), default!);
    }

    return ("", fmt.Errorf("unsupported string type: %v"u8, tag));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidRDNSequenceˢ = "x509: invalid RDNSequence"u8;
internal static readonly @string x509InvalidRDNSequenceˢ2 = "x509: invalid RDNSequence: invalid attribute"u8;
internal static readonly @string x509InvalidRDNSequenceˢ3 = "x509: invalid RDNSequence: invalid attribute type"u8;
internal static readonly @string x509InvalidRDNSequenceˢ4 = "x509: invalid RDNSequence: invalid attribute value"u8;

// parseName parses a DER encoded Name as defined in RFC 5280. We may
// want to export this function in the future for use in crypto/tls.
internal static (ж<pkix.RDNSequence>, error) parseName(cryptobyte.String rawʗp) {
    ref var raw = ref heap(rawʗp, out var Ꮡraw);

    if (!raw.ReadASN1(Ꮡraw, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509InvalidRDNSequenceˢ));
    }
    ref var rdnSeq = ref heap<pkix.RDNSequence>(out var ᏑrdnSeq);
    while (!raw.Empty()) {
        pkix.RelativeDistinguishedNameSET rdnSet = default!;
        ref var set = ref heap<cryptobyte.String>(out var Ꮡset);
        if (!raw.ReadASN1(Ꮡset, cryptobyte_asn1.SET)) {
            return (default!, errors.New(x509InvalidRDNSequenceˢ));
        }
        while (!set.Empty()) {
            ref var atav = ref heap<cryptobyte.String>(out var Ꮡatav);
            if (!set.ReadASN1(Ꮡatav, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New(x509InvalidRDNSequenceˢ2));
            }
            ref var attr = ref heap(new pkix.AttributeTypeAndValue(), out var Ꮡattr);
            if (!atav.ReadASN1ObjectIdentifier(Ꮡattr.of(pkix.AttributeTypeAndValue.ᏑType))) {
                return (default!, errors.New(x509InvalidRDNSequenceˢ3));
            }
            ref var rawValue = ref heap<cryptobyte.String>(out var ᏑrawValue);
            ref var valueTag = ref heap(new cryptobyte_asn1.Tag(), out var ᏑvalueTag);
            if (!atav.ReadAnyASN1(ᏑrawValue, ᏑvalueTag)) {
                return (default!, errors.New(x509InvalidRDNSequenceˢ4));
            }
            error err = default!;
            (attr.Value, err) = parseASN1String(valueTag, rawValue);
            if (err != default!) {
                return (default!, fmt.Errorf("x509: invalid RDNSequence: invalid attribute value: %s"u8, err));
            }
            rdnSet = append(rdnSet, attr);
        }
        rdnSeq = append(rdnSeq, rdnSet);
    }
    return (ᏑrdnSeq, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509MalformedOidˢ = "x509: malformed OID"u8;
internal static readonly @string x509MalformedParametersˢ = "x509: malformed parameters"u8;

internal static (pkix.AlgorithmIdentifier, error) parseAI(cryptobyte.String der) {
    ref var ai = ref heap<pkix.AlgorithmIdentifier>(out var Ꮡai);
    ai = new pkix.AlgorithmIdentifier(nil);
    if (!der.ReadASN1ObjectIdentifier(Ꮡai.of(pkix.AlgorithmIdentifier.ᏑAlgorithm))) {
        return (ai, errors.New(x509MalformedOidˢ));
    }
    if (der.Empty()) {
        return (ai, default!);
    }
    ref var @params = ref heap<cryptobyte.String>(out var Ꮡparams);
    ref var tag = ref heap(new cryptobyte_asn1.Tag(), out var Ꮡtag);
    if (!der.ReadAnyASN1Element(Ꮡparams, Ꮡtag)) {
        return (ai, errors.New(x509MalformedParametersˢ));
    }
    ai.Parameters.Tag = (nint)(uint8)tag;
    ai.Parameters.FullBytes = @params;
    return (ai, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509MalformedUTCTimeˢ = "x509: malformed UTCTime"u8;
internal static readonly @string x509Malformedˢ = "x509: malformed GeneralizedTime"u8;
internal static readonly @string x509UnsupportedTimeˢ = "x509: unsupported time format"u8;

internal static (time.Time, error) parseTime(ж<cryptobyte.String> Ꮡder) {
    ref var der = ref Ꮡder.DerefOrNull();

    ref var t = ref heap(new time.Time(), out var Ꮡt);
    switch (ᐧ) {
    case {} when der.PeekASN1Tag(cryptobyte_asn1.UTCTime): {
        if (!der.ReadASN1UTCTime(Ꮡt)) {
            return (t, errors.New(x509MalformedUTCTimeˢ));
        }
        break;
    }
    case {} when der.PeekASN1Tag(cryptobyte_asn1.GeneralizedTime): {
        if (!der.ReadASN1GeneralizedTime(Ꮡt)) {
            return (t, errors.New(x509Malformedˢ));
        }
        break;
    }
    default: {
        return (t, errors.New(x509UnsupportedTimeˢ));
    }}

    return (t, default!);
}

internal static (time.Time, time.Time, error) parseValidity(cryptobyte.String derʗp) {
    ref var der = ref heap(derʗp, out var Ꮡder);

    var (notBefore, err) = parseTime(Ꮡder);
    if (err != default!) {
        return (new time.Time(nil), new time.Time(nil), err);
    }
    (var notAfter, err) = parseTime(Ꮡder);
    if (err != default!) {
        return (new time.Time(nil), new time.Time(nil), err);
    }
    return (notBefore, notAfter, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509MalformedExtensionˢ = "x509: malformed extension OID field"u8;
internal static readonly @string x509MalformedExtensionˢ2 = "x509: malformed extension critical field"u8;
internal static readonly @string x509MalformedExtensionˢ3 = "x509: malformed extension value field"u8;

internal static (pkix.Extension, error) parseExtension(cryptobyte.String der) {
    ref var ext = ref heap(new pkix.Extension(), out var Ꮡext);
    if (!der.ReadASN1ObjectIdentifier(Ꮡext.of(pkix.Extension.ᏑId))) {
        return (ext, errors.New(x509MalformedExtensionˢ));
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(Ꮡext.of(pkix.Extension.ᏑCritical))) {
            return (ext, errors.New(x509MalformedExtensionˢ2));
        }
    }
    ref var val = ref heap<cryptobyte.String>(out var Ꮡval);
    if (!der.ReadASN1(Ꮡval, cryptobyte_asn1.OCTET_STRING)) {
        return (ext, errors.New(x509MalformedExtensionˢ3));
    }
    ext.Value = val;
    return (ext, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509RsaKeyMissingNullˢ = "x509: RSA key missing NULL parameters"u8;
internal static readonly @string x509InvalidRsaPublicKeyˢ = "x509: invalid RSA public key"u8;
internal static readonly @string x509InvalidRsaModulusˢ = "x509: invalid RSA modulus"u8;
internal static readonly @string x509InvalidRsaPublicˢ = "x509: invalid RSA public exponent"u8;
internal static readonly @string x509RsaModulusIsNotAˢ = "x509: RSA modulus is not a positive number"u8;
internal static readonly @string x509RsaPublicExponentIsˢ = "x509: RSA public exponent is not a positive number"u8;
internal static readonly @string x509InvalidEcdsaˢ = "x509: invalid ECDSA parameters"u8;
internal static readonly @string x509UnsupportedEllipticˢ = "x509: unsupported elliptic curve"u8;
internal static readonly @string x509FailedToUnmarshalˢ = "x509: failed to unmarshal elliptic curve point"u8;
internal static readonly @string x509Ed25519KeyEncodedˢ = "x509: Ed25519 key encoded with illegal parameters"u8;
internal static readonly @string x509WrongEd25519Publicˢ = "x509: wrong Ed25519 public key size"u8;
internal static readonly @string x509X25519KeyEncodedWithˢ = "x509: X25519 key encoded with illegal parameters"u8;
internal static readonly @string x509InvalidDsaPublicKeyˢ = "x509: invalid DSA public key"u8;
internal static readonly @string x509InvalidDsaParametersˢ = "x509: invalid DSA parameters"u8;
internal static readonly @string x509ZeroOrNegativeDsaˢ = "x509: zero or negative DSA parameter"u8;
internal static readonly @string x509UnknownPublicKeyˢ = "x509: unknown public key algorithm"u8;

internal static (any, error) parsePublicKey(ref publicKeyInfo keyData) {
    var oid = keyData.Algorithm.Algorithm;
    var @params = keyData.Algorithm.Parameters;
    ref var der = ref heap<cryptobyte.String>(out var Ꮡder);
    der = ((cryptobyte.String)keyData.PublicKey.RightAlign());
    switch (ᐧ) {
    case {} when oid.Equal(oidPublicKeyRSA): {
        if (!bytes.Equal(@params.FullBytes, // RSA public keys must have a NULL in the parameters.
 // See RFC 3279, Section 2.3.1.
 asn1.NullBytes)) {
            return (default!, errors.New(x509RsaKeyMissingNullˢ));
        }
        var p = Ꮡ(new pkcs1PublicKey(N: @new<bigꓸInt>()));
        if (!der.ReadASN1(Ꮡder, cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New(x509InvalidRsaPublicKeyˢ));
        }
        if (!der.ReadASN1Integer((~p).N.OrTypedNil())) {
            return (default!, errors.New(x509InvalidRsaModulusˢ));
        }
        if (!der.ReadASN1Integer(p.of(pkcs1PublicKey.ᏑE))) {
            return (default!, errors.New(x509InvalidRsaPublicˢ));
        }
        if ((~p).N.Sign() <= 0) {
            return (default!, errors.New(x509RsaModulusIsNotAˢ));
        }
        if ((~p).E <= 0) {
            return (default!, errors.New(x509RsaPublicExponentIsˢ));
        }
        var pub = Ꮡ(new rsa.PublicKey(
            E: (~p).E,
            N: (~p).N
        ));
        return (pub.OrTypedNil(), default!);
    }
    case {} when oid.Equal(oidPublicKeyECDSA): {
        var paramsDer = ((cryptobyte.String)@params.FullBytes);
        var namedCurveOID = @new<asn1.ObjectIdentifier>();
        if (!paramsDer.ReadASN1ObjectIdentifier(namedCurveOID)) {
            return (default!, errors.New(x509InvalidEcdsaˢ));
        }
        var namedCurve = namedCurveFromOID(namedCurveOID.ValueSlot);
        if (namedCurve == default!) {
            return (default!, errors.New(x509UnsupportedEllipticˢ));
        }
        var (x, y) = elliptic.Unmarshal(namedCurve, der);
        if (x == nil) {
            return (default!, errors.New(x509FailedToUnmarshalˢ));
        }
        var pub = Ꮡ(new ecdsa.PublicKey(
            Curve: namedCurve,
            X: x,
            Y: y
        ));
        return (pub.OrTypedNil(), default!);
    }
    case {} when oid.Equal(oidPublicKeyEd25519): {
        if (builtin.len(@params.FullBytes) != 0) {
            // RFC 8410, Section 3
            // > For all of the OIDs, the parameters MUST be absent.
            return (default!, errors.New(x509Ed25519KeyEncodedˢ));
        }
        if (builtin.len(der) != ed25519.PublicKeySize) {
            return (default!, errors.New(x509WrongEd25519Publicˢ));
        }
        return (((ed25519.PublicKey)(slice<byte>)(der)), default!);
    }
    case {} when oid.Equal(oidPublicKeyX25519): {
        if (builtin.len(@params.FullBytes) != 0) {
            // RFC 8410, Section 3
            // > For all of the OIDs, the parameters MUST be absent.
            return (default!, errors.New(x509X25519KeyEncodedWithˢ));
        }
        var (ᴛ1, ᴛ2) = ecdh.X25519().NewPublicKey(der);
        return (ᴛ1.OrTypedNil(), ᴛ2);
    }
    case {} when oid.Equal(oidPublicKeyDSA): {
        var y = @new<bigꓸInt>();
        if (!der.ReadASN1Integer(y.OrTypedNil())) {
            return (default!, errors.New(x509InvalidDsaPublicKeyˢ));
        }
        var pub = Ꮡ(new dsa.PublicKey(
            Y: y,
            Parameters: new dsa.Parameters(
                P: @new<bigꓸInt>(),
                Q: @new<bigꓸInt>(),
                G: @new<bigꓸInt>()
            )
        ));
        ref var paramsDer = ref heap<cryptobyte.String>(out var ᏑparamsDer);
        paramsDer = ((cryptobyte.String)@params.FullBytes);
        if (!paramsDer.ReadASN1(ᏑparamsDer, cryptobyte_asn1.SEQUENCE) || !paramsDer.ReadASN1Integer((~pub).Parameters.P.OrTypedNil()) || !paramsDer.ReadASN1Integer((~pub).Parameters.Q.OrTypedNil()) || !paramsDer.ReadASN1Integer((~pub).Parameters.G.OrTypedNil())) {
            return (default!, errors.New(x509InvalidDsaParametersˢ));
        }
        if ((~pub).Y.Sign() <= 0 || (~pub).Parameters.P.Sign() <= 0 || (~pub).Parameters.Q.Sign() <= 0 || (~pub).Parameters.G.Sign() <= 0) {
            return (default!, errors.New(x509ZeroOrNegativeDsaˢ));
        }
        return (pub.OrTypedNil(), default!);
    }
    default: {
        return (default!, errors.New(x509UnknownPublicKeyˢ));
    }}

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidKeyUsageˢ = "x509: invalid key usage"u8;

internal static (KeyUsage, error) parseKeyUsageExtension(cryptobyte.String der) {
    ref var usageBits = ref heap(new asn1.BitString(), out var ᏑusageBits);
    if (!der.ReadASN1BitString(ᏑusageBits)) {
        return (0, errors.New(x509InvalidKeyUsageˢ));
    }
    nint usage = default!;
    for (nint i = 0; i < 9; i++) {
        if (usageBits.At(i) != 0) {
            usage |= (nint)(((nint)1).Lsh((nuint)i));
        }
    }
    return (((KeyUsage)usage), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidBasicˢ = "x509: invalid basic constraints"u8;

internal static (bool, nint, error) parseBasicConstraintsExtension(cryptobyte.String derʗp) {
    ref var der = ref heap(derʗp, out var Ꮡder);

    ref var isCA = ref heap(new bool(), out var ᏑisCA);
    if (!der.ReadASN1(Ꮡder, cryptobyte_asn1.SEQUENCE)) {
        return (false, 0, errors.New(x509InvalidBasicˢ));
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(ᏑisCA)) {
            return (false, 0, errors.New(x509InvalidBasicˢ));
        }
    }
    ref var maxPathLen = ref heap<nint>(out var ᏑmaxPathLen);
    maxPathLen = -1;
    if (der.PeekASN1Tag(cryptobyte_asn1.INTEGER)) {
        if (!der.ReadASN1Integer(ᏑmaxPathLen)) {
            return (false, 0, errors.New(x509InvalidBasicˢ));
        }
    }
    // TODO: map out.MaxPathLen to 0 if it has the -1 default value? (Issue 19285)
    return (isCA, maxPathLen, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidSubjectˢ = "x509: invalid subject alternative names"u8;
internal static readonly @string x509InvalidSubjectˢ2 = "x509: invalid subject alternative name"u8;

internal static error forEachSAN(cryptobyte.String derʗp, Func<nint, slice<byte>, error> callback) {
    ref var der = ref heap(derʗp, out var Ꮡder);

    if (!der.ReadASN1(Ꮡder, cryptobyte_asn1.SEQUENCE)) {
        return errors.New(x509InvalidSubjectˢ);
    }
    while (!der.Empty()) {
        ref var san = ref heap<cryptobyte.String>(out var Ꮡsan);
        ref var tag = ref heap(new cryptobyte_asn1.Tag(), out var Ꮡtag);
        if (!der.ReadAnyASN1(Ꮡsan, Ꮡtag)) {
            return errors.New(x509InvalidSubjectˢ2);
        }
        {
            var err = callback((nint)(uint8)((cryptobyte_asn1.Tag)(tag ^ 0x80)), san); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509SanRfc822NameIsˢ = "x509: SAN rfc822Name is malformed"u8;
internal static readonly @string x509SanDNSNameIsˢ = "x509: SAN dNSName is malformed"u8;
internal static readonly @string x509Sanˢ = "x509: SAN uniformResourceIdentifier is malformed"u8;

internal static (slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<ж<url.URL>> uris, error err) parseSANExtension(cryptobyte.String der) {
    slice<@string> dnsNames = default!;
    slice<@string> emailAddresses = default!;
    slice<net.IP> ipAddresses = default!;
    slice<ж<url.URL>> uris = default!;
    error err = default!;

    err = forEachSAN(der, error (nint tag, slice<byte> data) => {
        var exprᴛ1 = tag;
        if (exprᴛ1 == nameTypeEmail) {
            @string email = ((@string)data);
            {
                var errΔ5 = isIA5String(email); if (errΔ5 != default!) {
                    return errors.New(x509SanRfc822NameIsˢ);
                }
            }
            emailAddresses = append(emailAddresses, email);
        }
        else if (exprᴛ1 == nameTypeDNS) {
            @string name = ((@string)data);
            {
                var errΔ6 = isIA5String(name); if (errΔ6 != default!) {
                    return errors.New(x509SanDNSNameIsˢ);
                }
            }
            dnsNames = append(dnsNames, ((@string)name));
        }
        else if (exprᴛ1 == nameTypeURI) {
            @string uriStr = ((@string)data);
            {
                var errΔ7 = isIA5String(uriStr); if (errΔ7 != default!) {
                    return errors.New(x509Sanˢ);
                }
            }
            var (uri, errΔ8) = url.Parse(uriStr);
            if (errΔ8 != default!) {
                return fmt.Errorf("x509: cannot parse URI %q: %s"u8, uriStr, errΔ8);
            }
            if (builtin.len((~uri).Host) > 0 && !domainNameValid((~uri).Host, false)) {
                return fmt.Errorf("x509: cannot parse URI %q: invalid domain"u8, uriStr);
            }
            uris = append(uris, uri);
        }
        else if (exprᴛ1 == nameTypeIP) {
            var exprᴛ2 = builtin.len(data);
            if (exprᴛ2 == net.IPv4len || exprᴛ2 == net.IPv6len) {
                ipAddresses = append(ipAddresses, (net.IP)(data));
            }
            else { /* default: */
                return errors.New("x509: cannot parse IP address of length "u8 + strconv.Itoa(builtin.len(data)));
            }

        }

        return default!;
    });
    return (dnsNames, emailAddresses, ipAddresses, uris, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509AuthorityKeyˢ = "x509: authority key identifier incorrectly marked critical"u8;
internal static readonly @string x509InvalidAuthorityKeyˢ = "x509: invalid authority key identifier"u8;

internal static (slice<byte>, error) parseAuthorityKeyIdentifier(pkix.Extension e) {
    // RFC 5280, Section 4.2.1.1
    if (e.Critical) {
        // Conforming CAs MUST mark this extension as non-critical
        return (default!, errors.New(x509AuthorityKeyˢ));
    }
    var val = ((cryptobyte.String)e.Value);
    ref var akid = ref heap<cryptobyte.String>(out var Ꮡakid);
    if (!val.ReadASN1(Ꮡakid, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509InvalidAuthorityKeyˢ));
    }
    if (akid.PeekASN1Tag(((cryptobyte_asn1.Tag)0).ContextSpecific())) {
        if (!akid.ReadASN1(Ꮡakid, ((cryptobyte_asn1.Tag)0).ContextSpecific())) {
            return (default!, errors.New(x509InvalidAuthorityKeyˢ));
        }
        return (akid, default!);
    }
    return (default!, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidExtendedKeyˢ = "x509: invalid extended key usages"u8;

internal static (slice<ExtKeyUsage>, slice<asn1.ObjectIdentifier>, error) parseExtKeyUsageExtension(cryptobyte.String derʗp) {
    ref var der = ref heap(derʗp, out var Ꮡder);

    slice<ExtKeyUsage> extKeyUsages = default!;
    slice<asn1.ObjectIdentifier> unknownUsages = default!;
    if (!der.ReadASN1(Ꮡder, cryptobyte_asn1.SEQUENCE)) {
        return (default!, default!, errors.New(x509InvalidExtendedKeyˢ));
    }
    while (!der.Empty()) {
        ref var eku = ref heap<asn1.ObjectIdentifier>(out var Ꮡeku);
        if (!der.ReadASN1ObjectIdentifier(Ꮡeku)) {
            return (default!, default!, errors.New(x509InvalidExtendedKeyˢ));
        }
        {
            var (extKeyUsage, ok) = extKeyUsageFromOID(eku); if (ok){
                extKeyUsages = append(extKeyUsages, extKeyUsage);
            } else {
                unknownUsages = append(unknownUsages, eku);
            }
        }
    }
    return (extKeyUsages, unknownUsages, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidCertificateˢ = "x509: invalid certificate policies"u8;

internal static (slice<OID>, error) parseCertificatePoliciesExtension(cryptobyte.String derʗp) {
    ref var der = ref heap(derʗp, out var Ꮡder);

    slice<OID> oids = default!;
    var seenOIDs = new map<@string, bool>{};
    if (!der.ReadASN1(Ꮡder, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509InvalidCertificateˢ));
    }
    while (!der.Empty()) {
        ref var cp = ref heap<cryptobyte.String>(out var Ꮡcp);
        ref var OIDBytes = ref heap<cryptobyte.String>(out var ᏑOIDBytes);
        if (!der.ReadASN1(Ꮡcp, cryptobyte_asn1.SEQUENCE) || !cp.ReadASN1(ᏑOIDBytes, cryptobyte_asn1.OBJECT_IDENTIFIER)) {
            return (default!, errors.New(x509InvalidCertificateˢ));
        }
        if (seenOIDs[((@string)(slice<byte>)OIDBytes)]) {
            return (default!, errors.New(x509InvalidCertificateˢ));
        }
        seenOIDs[((@string)(slice<byte>)OIDBytes)] = true;
        var (oid, ok) = newOIDFromDER(OIDBytes);
        if (!ok) {
            return (default!, errors.New(x509InvalidCertificateˢ));
        }
        oids = append(oids, oid);
    }
    return (oids, default!);
}

// isValidIPMask reports whether mask consists of zero or more 1 bits, followed by zero bits.
internal static bool isValidIPMask(slice<byte> mask) {
    var seenZero = false;
    foreach (var (_, b) in mask) {
        if (seenZero) {
            if (b != 0) {
                return false;
            }
            continue;
        }
        switch (b) {
        case 0x00 or 0x80 or 0xc0 or 0xe0 or 0xf0 or 0xf8 or 0xfc or 0xfe: {
            seenZero = true;
            break;
        }
        case 0xff: {
            break;
        }
        default: {
            return false;
        }}

    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509Invalidˢ = "x509: invalid NameConstraints extension"u8;
internal static readonly @string x509EmptyNameConstraintsˢ = "x509: empty name constraints extension"u8;

internal static (bool unhandled, error err) parseNameConstraintsExtension(ref Certificate @out, pkix.Extension e) {
    bool unhandled = default!;
    error err = default!;

    // RFC 5280, 4.2.1.10
    // NameConstraints ::= SEQUENCE {
    //      permittedSubtrees       [0]     GeneralSubtrees OPTIONAL,
    //      excludedSubtrees        [1]     GeneralSubtrees OPTIONAL }
    //
    // GeneralSubtrees ::= SEQUENCE SIZE (1..MAX) OF GeneralSubtree
    //
    // GeneralSubtree ::= SEQUENCE {
    //      base                    GeneralName,
    //      minimum         [0]     BaseDistance DEFAULT 0,
    //      maximum         [1]     BaseDistance OPTIONAL }
    //
    // BaseDistance ::= INTEGER (0..MAX)
    var outer = ((cryptobyte.String)e.Value);
    ref var toplevel = ref heap<cryptobyte.String>(out var Ꮡtoplevel);
    ref var permitted = ref heap<cryptobyte.String>(out var Ꮡpermitted);
    ref var excluded = ref heap<cryptobyte.String>(out var Ꮡexcluded);
    ref var havePermitted = ref heap(new bool(), out var ᏑhavePermitted);
    ref var haveExcluded = ref heap(new bool(), out var ᏑhaveExcluded);
    if (!outer.ReadASN1(Ꮡtoplevel, cryptobyte_asn1.SEQUENCE) || !outer.Empty() || !toplevel.ReadOptionalASN1(Ꮡpermitted, ᏑhavePermitted, ((cryptobyte_asn1.Tag)0).ContextSpecific().Constructed()) || !toplevel.ReadOptionalASN1(Ꮡexcluded, ᏑhaveExcluded, ((cryptobyte_asn1.Tag)1).ContextSpecific().Constructed()) || !toplevel.Empty()) {
        return (false, errors.New(x509Invalidˢ));
    }
    if (!havePermitted && !haveExcluded || builtin.len(permitted) == 0 && builtin.len(excluded) == 0) {
        // From RFC 5280, Section 4.2.1.10:
        //   “either the permittedSubtrees field
        //   or the excludedSubtrees MUST be
        //   present”
        return (false, errors.New(x509EmptyNameConstraintsˢ));
    }
    (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains, error err) getValues(cryptobyte.String subtrees) {
        slice<@string> dnsNames = default!;
        slice<ж<net.IPNet>> ips = default!;
        slice<@string> emails = default!;
        slice<@string> uriDomains = default!;
        while (!subtrees.Empty()) {
            ref var seq = ref heap<cryptobyte.String>(out var Ꮡseq);
            ref var value = ref heap<cryptobyte.String>(out var Ꮡvalue);
            ref var tag = ref heap(new cryptobyte_asn1.Tag(), out var Ꮡtag);
            if (!subtrees.ReadASN1(Ꮡseq, cryptobyte_asn1.SEQUENCE) || !seq.ReadAnyASN1(Ꮡvalue, Ꮡtag)) {
                return (default!, default!, default!, default!, fmt.Errorf("x509: invalid NameConstraints extension"u8));
            }
            cryptobyte_asn1.Tag dnsTag = ((cryptobyte_asn1.Tag)2).ContextSpecific();
            cryptobyte_asn1.Tag emailTag = ((cryptobyte_asn1.Tag)1).ContextSpecific();
            cryptobyte_asn1.Tag ipTag = ((cryptobyte_asn1.Tag)7).ContextSpecific();
            cryptobyte_asn1.Tag uriTag = ((cryptobyte_asn1.Tag)6).ContextSpecific();
            var exprᴛ1 = tag;
            if (exprᴛ1 == dnsTag) {
                @string domain = ((@string)(slice<byte>)value);
                {
                    var errΔ5 = isIA5String(domain); if (errΔ5 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ5.Error()));
                    }
                }
                if (!domainNameValid(domain, true)) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse dnsName constraint %q"u8, domain));
                }
                dnsNames = append(dnsNames, domain);
            }
            else if (exprᴛ1 == ipTag) {
                nint l = builtin.len(value);
                slice<byte> ip = default!;
                slice<byte> mask = default!;
                switch (l) {
                case 8: {
                    ip = value[..4];
                    mask = value[4..];
                    break;
                }
                case 32: {
                    ip = value[..16];
                    mask = value[16..];
                    break;
                }
                default: {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: IP constraint contained value of length %d"u8, l));
                }}

                if (!isValidIPMask(mask)) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: IP constraint contained invalid mask %x"u8, mask));
                }
                ips = append(ips, Ꮡ(new net.IPNet(IP: ((net.IP)ip), Mask: ((net.IPMask)mask))));
            }
            else if (exprᴛ1 == emailTag) {
                @string constraint = ((@string)(slice<byte>)value);
                {
                    var errΔ6 = isIA5String(constraint); if (errΔ6 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ6.Error()));
                    }
                }
                if (strings.Contains(constraint, // If the constraint contains an @ then
 // it specifies an exact mailbox name.
 "@"u8)){
                    {
                        var (_, ok) = parseRFC2821Mailbox(constraint); if (!ok) {
                            return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse rfc822Name constraint %q"u8, constraint));
                        }
                    }
                } else {
                    if (!domainNameValid(constraint, true)) {
                        return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse rfc822Name constraint %q"u8, constraint));
                    }
                }
                emails = append(emails, constraint);
            }
            else if (exprᴛ1 == uriTag) {
                @string domain = ((@string)(slice<byte>)value);
                {
                    var errΔ7 = isIA5String(domain); if (errΔ7 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ7.Error()));
                    }
                }
                if (net.ParseIP(domain) != default!) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse URI constraint %q: cannot be IP address"u8, domain));
                }
                if (!domainNameValid(domain, true)) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse URI constraint %q"u8, domain));
                }
                uriDomains = append(uriDomains, domain);
            }
            else { /* default: */
                unhandled = true;
            }

        }
        return (dnsNames, ips, emails, uriDomains, default!);
    }
    {
        (@out.PermittedDNSDomains, @out.PermittedIPRanges, @out.PermittedEmailAddresses, @out.PermittedURIDomains, err) = getValues(permitted); if (err != default!) {
            return (false, err);
        }
    }
    {
        (@out.ExcludedDNSDomains, @out.ExcludedIPRanges, @out.ExcludedEmailAddresses, @out.ExcludedURIDomains, err) = getValues(excluded); if (err != default!) {
            return (false, err);
        }
    }
    @out.PermittedDNSDomainsCritical = e.Critical;
    return (unhandled, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InvalidCrlˢ = "x509: invalid CRL distribution points"u8;
internal static readonly @string x509InvalidCrlˢ2 = "x509: invalid CRL distribution point"u8;
internal static readonly @string x509InvalidPolicyˢ = "x509: invalid policy constraints extension"u8;
internal static readonly @string x509PolicyConstraintsˢ = "x509: policy constraints requireExplicitPolicy field overflows int"u8;
internal static readonly @string x509PolicyConstraintsˢ2 = "x509: policy constraints inhibitPolicyMapping field overflows int"u8;
internal static readonly @string x509SubjectKeyIdentifierˢ = "x509: subject key identifier incorrectly marked critical"u8;
internal static readonly @string x509InvalidSubjectKeyˢ = "x509: invalid subject key identifier"u8;
internal static readonly @string x509InvalidPolicyˢ2 = "x509: invalid policy mappings extension"u8;
internal static readonly @string x509InvalidInhibitAnyˢ = "x509: invalid inhibit any policy extension"u8;
internal static readonly @string x509AuthorityInfoAccessˢ = "x509: authority info access incorrectly marked critical"u8;
internal static readonly @string x509InvalidAuthorityInfoˢ = "x509: invalid authority info access"u8;

internal static error processExtensions(ж<Certificate> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    error err = default!;
    foreach (var (_, e) in @out.Extensions) {
        var unhandled = false;
        if (builtin.len(e.Id) == 4 && e.Id[0] == 2 && e.Id[1] == 5 && e.Id[2] == 29){
            switch (e.Id[3]) {
            case 15: {
                (@out.KeyUsage, err) = parseKeyUsageExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 19: {
                (@out.IsCA, @out.MaxPathLen, err) = parseBasicConstraintsExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                @out.BasicConstraintsValid = true;
                @out.MaxPathLenZero = @out.MaxPathLen == 0;
                break;
            }
            case 17: {
                (@out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err) = parseSANExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                if (builtin.len(@out.DNSNames) == 0 && builtin.len(@out.EmailAddresses) == 0 && builtin.len(@out.IPAddresses) == 0 && builtin.len(@out.URIs) == 0) {
                    // If we didn't parse anything then we do the critical check, below.
                    unhandled = true;
                }
                break;
            }
            case 30: {
                (unhandled, err) = parseNameConstraintsExtension(ref (Ꮡout).DerefOrNull(), e);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 31: {
                ref var val = ref heap<cryptobyte.String>(out var Ꮡval);
                val = ((cryptobyte.String)e.Value);
                if (!val.ReadASN1(Ꮡval, // RFC 5280, 4.2.1.13
 // CRLDistributionPoints ::= SEQUENCE SIZE (1..MAX) OF DistributionPoint
 //
 // DistributionPoint ::= SEQUENCE {
 //     distributionPoint       [0]     DistributionPointName OPTIONAL,
 //     reasons                 [1]     ReasonFlags OPTIONAL,
 //     cRLIssuer               [2]     GeneralNames OPTIONAL }
 //
 // DistributionPointName ::= CHOICE {
 //     fullName                [0]     GeneralNames,
 //     nameRelativeToCRLIssuer [1]     RelativeDistinguishedName }
 cryptobyte_asn1.SEQUENCE)) {
                    return errors.New(x509InvalidCrlˢ);
                }
                while (!val.Empty()) {
                    ref var dpDER = ref heap<cryptobyte.String>(out var ᏑdpDER);
                    if (!val.ReadASN1(ᏑdpDER, cryptobyte_asn1.SEQUENCE)) {
                        return errors.New(x509InvalidCrlˢ2);
                    }
                    ref var dpNameDER = ref heap<cryptobyte.String>(out var ᏑdpNameDER);
                    ref var dpNamePresent = ref heap(new bool(), out var ᏑdpNamePresent);
                    if (!dpDER.ReadOptionalASN1(ᏑdpNameDER, ᏑdpNamePresent, ((cryptobyte_asn1.Tag)0).Constructed().ContextSpecific())) {
                        return errors.New(x509InvalidCrlˢ2);
                    }
                    if (!dpNamePresent) {
                        continue;
                    }
                    if (!dpNameDER.ReadASN1(ᏑdpNameDER, ((cryptobyte_asn1.Tag)0).Constructed().ContextSpecific())) {
                        return errors.New(x509InvalidCrlˢ2);
                    }
                    while (!dpNameDER.Empty()) {
                        if (!dpNameDER.PeekASN1Tag(((cryptobyte_asn1.Tag)6).ContextSpecific())) {
                            break;
                        }
                        ref var uri = ref heap<cryptobyte.String>(out var Ꮡuri);
                        if (!dpNameDER.ReadASN1(Ꮡuri, ((cryptobyte_asn1.Tag)6).ContextSpecific())) {
                            return errors.New(x509InvalidCrlˢ2);
                        }
                        @out.CRLDistributionPoints = append(@out.CRLDistributionPoints, ((@string)(slice<byte>)uri));
                    }
                }
                break;
            }
            case 35: {
                (@out.AuthorityKeyId, err) = parseAuthorityKeyIdentifier(e);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 36: {
                ref var val = ref heap<cryptobyte.String>(out var Ꮡval);
                val = ((cryptobyte.String)e.Value);
                if (!val.ReadASN1(Ꮡval, cryptobyte_asn1.SEQUENCE)) {
                    return errors.New(x509InvalidPolicyˢ);
                }
                if (val.PeekASN1Tag(((cryptobyte_asn1.Tag)0).ContextSpecific())) {
                    ref var v = ref heap(new int64(), out var Ꮡv);
                    if (!val.ReadASN1Int64WithTag(Ꮡv, ((cryptobyte_asn1.Tag)0).ContextSpecific())) {
                        return errors.New(x509InvalidPolicyˢ);
                    }
                    @out.RequireExplicitPolicy = (nint)v;
                    // Check for overflow.
                    if ((int64)@out.RequireExplicitPolicy != v) {
                        return errors.New(x509PolicyConstraintsˢ);
                    }
                    @out.RequireExplicitPolicyZero = @out.RequireExplicitPolicy == 0;
                }
                if (val.PeekASN1Tag(((cryptobyte_asn1.Tag)1).ContextSpecific())) {
                    ref var v = ref heap(new int64(), out var Ꮡv);
                    if (!val.ReadASN1Int64WithTag(Ꮡv, ((cryptobyte_asn1.Tag)1).ContextSpecific())) {
                        return errors.New(x509InvalidPolicyˢ);
                    }
                    @out.InhibitPolicyMapping = (nint)v;
                    // Check for overflow.
                    if ((int64)@out.InhibitPolicyMapping != v) {
                        return errors.New(x509PolicyConstraintsˢ2);
                    }
                    @out.InhibitPolicyMappingZero = @out.InhibitPolicyMapping == 0;
                }
                break;
            }
            case 37: {
                (@out.ExtKeyUsage, @out.UnknownExtKeyUsage, err) = parseExtKeyUsageExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 14: {
                if (e.Critical) {
                    // RFC 5280, 4.2.1.2
                    // Conforming CAs MUST mark this extension as non-critical
                    return errors.New(x509SubjectKeyIdentifierˢ);
                }
                var val = ((cryptobyte.String)e.Value);
                ref var skid = ref heap<cryptobyte.String>(out var Ꮡskid);
                if (!val.ReadASN1(Ꮡskid, cryptobyte_asn1.OCTET_STRING)) {
                    return errors.New(x509InvalidSubjectKeyˢ);
                }
                @out.SubjectKeyId = skid;
                break;
            }
            case 32: {
                (@out.Policies, err) = parseCertificatePoliciesExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                @out.PolicyIdentifiers = new slice<asn1.ObjectIdentifier>(0, builtin.len(@out.Policies));
                foreach (var (_, oid) in @out.Policies) {
                    {
                        var (oidΔ1, ok) = oid.toASN1OID(); if (ok) {
                            @out.PolicyIdentifiers = append(@out.PolicyIdentifiers, oidΔ1);
                        }
                    }
                }
                break;
            }
            case 33: {
                ref var val = ref heap<cryptobyte.String>(out var Ꮡval);
                val = ((cryptobyte.String)e.Value);
                if (!val.ReadASN1(Ꮡval, cryptobyte_asn1.SEQUENCE)) {
                    return errors.New(x509InvalidPolicyˢ2);
                }
                while (!val.Empty()) {
                    ref var s = ref heap<cryptobyte.String>(out var Ꮡs);
                    ref var issuer = ref heap<cryptobyte.String>(out var Ꮡissuer);
                    ref var subject = ref heap<cryptobyte.String>(out var Ꮡsubject);
                    if (!val.ReadASN1(Ꮡs, cryptobyte_asn1.SEQUENCE) || !s.ReadASN1(Ꮡissuer, cryptobyte_asn1.OBJECT_IDENTIFIER) || !s.ReadASN1(Ꮡsubject, cryptobyte_asn1.OBJECT_IDENTIFIER)) {
                        return errors.New(x509InvalidPolicyˢ2);
                    }
                    @out.PolicyMappings = append(@out.PolicyMappings, new PolicyMapping(new OID(issuer), new OID(subject)));
                }
                break;
            }
            case 54: {
                var val = ((cryptobyte.String)e.Value);
                if (!val.ReadASN1Integer(Ꮡout.of(Certificate.ᏑInhibitAnyPolicy))) {
                    return errors.New(x509InvalidInhibitAnyˢ);
                }
                @out.InhibitAnyPolicyZero = @out.InhibitAnyPolicy == 0;
                break;
            }
            default: {
                unhandled = true;
                break;
            }}

        } else 
        if (e.Id.Equal(oidExtensionAuthorityInfoAccess)){
            // Unknown extensions are recorded if critical.
            // RFC 5280 4.2.2.1: Authority Information Access
            if (e.Critical) {
                // Conforming CAs MUST mark this extension as non-critical
                return errors.New(x509AuthorityInfoAccessˢ);
            }
            ref var val = ref heap<cryptobyte.String>(out var Ꮡval);
            val = ((cryptobyte.String)e.Value);
            if (!val.ReadASN1(Ꮡval, cryptobyte_asn1.SEQUENCE)) {
                return errors.New(x509InvalidAuthorityInfoˢ);
            }
            while (!val.Empty()) {
                ref var aiaDER = ref heap<cryptobyte.String>(out var ᏑaiaDER);
                if (!val.ReadASN1(ᏑaiaDER, cryptobyte_asn1.SEQUENCE)) {
                    return errors.New(x509InvalidAuthorityInfoˢ);
                }
                ref var method = ref heap<asn1.ObjectIdentifier>(out var Ꮡmethod);
                if (!aiaDER.ReadASN1ObjectIdentifier(Ꮡmethod)) {
                    return errors.New(x509InvalidAuthorityInfoˢ);
                }
                if (!aiaDER.PeekASN1Tag(((cryptobyte_asn1.Tag)6).ContextSpecific())) {
                    continue;
                }
                if (!aiaDER.ReadASN1(ᏑaiaDER, ((cryptobyte_asn1.Tag)6).ContextSpecific())) {
                    return errors.New(x509InvalidAuthorityInfoˢ);
                }
                switch (ᐧ) {
                case {} when method.Equal(oidAuthorityInfoAccessOcsp): {
                    @out.OCSPServer = append(@out.OCSPServer, ((@string)(slice<byte>)aiaDER));
                    break;
                }
                case {} when method.Equal(oidAuthorityInfoAccessIssuers): {
                    @out.IssuingCertificateURL = append(@out.IssuingCertificateURL, ((@string)(slice<byte>)aiaDER));
                    break;
                }}

            }
        } else {
            // Unknown extensions are recorded if critical.
            unhandled = true;
        }
        if (e.Critical && unhandled) {
            @out.UnhandledCriticalExtensions = append(@out.UnhandledCriticalExtensions, e.Id);
        }
    }
    return default!;
}

internal static ж<godebug.Setting> x509negativeserial = godebug.New("x509negativeserial"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509MalformedCertificateˢ = "x509: malformed certificate"u8;
internal static readonly @string x509MalformedTbsˢ = "x509: malformed tbs certificate"u8;
internal static readonly @string x509MalformedVersionˢ = "x509: malformed version"u8;
internal static readonly @string x509InvalidVersionˢ = "x509: invalid version"u8;
internal static readonly @string x509MalformedSerialˢ = "x509: malformed serial number"u8;
internal static readonly @string x509NegativeSerialNumberˢ = "x509: negative serial number"u8;
internal static readonly @string x509MalformedSignatureˢ = "x509: malformed signature algorithm identifier"u8;
internal static readonly @string x509MalformedAlgorithmˢ = "x509: malformed algorithm identifier"u8;
internal static readonly @string x509InnerAndOuterˢ = "x509: inner and outer signature algorithm identifiers don't match"u8;
internal static readonly @string x509MalformedIssuerˢ = "x509: malformed issuer"u8;
internal static readonly @string x509MalformedValidityˢ = "x509: malformed validity"u8;
internal static readonly @string x509MalformedSpkiˢ = "x509: malformed spki"u8;
internal static readonly @string x509MalformedPublicKeyˢ = "x509: malformed public key algorithm identifier"u8;
internal static readonly @string x509Malformedˢ2 = "x509: malformed subjectPublicKey"u8;
internal static readonly @string x509Malformedˢ3 = "x509: malformed issuerUniqueID"u8;
internal static readonly @string x509Malformedˢ4 = "x509: malformed subjectUniqueID"u8;
internal static readonly @string x509MalformedExtensionsˢ = "x509: malformed extensions"u8;
internal static readonly @string x509MalformedExtensionˢ4 = "x509: malformed extension"u8;
internal static readonly @string x509MalformedSignatureˢ2 = "x509: malformed signature"u8;

internal static (ж<Certificate>, error) parseCertificate(slice<byte> der) {
    var cert = Ꮡ(new Certificate(nil));
    ref var input = ref heap<cryptobyte.String>(out var Ꮡinput);
    input = ((cryptobyte.String)der);
    // we read the SEQUENCE including length and tag bytes so that
    // we can populate Certificate.Raw, before unwrapping the
    // SEQUENCE so it can be operated on
    if (!input.ReadASN1Element(Ꮡinput, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedCertificateˢ));
    }
    cert.Value.Raw = input;
    if (!input.ReadASN1(Ꮡinput, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedCertificateˢ));
    }
    ref var tbs = ref heap<cryptobyte.String>(out var Ꮡtbs);
    // do the same trick again as above to extract the raw
    // bytes for Certificate.RawTBSCertificate
    if (!input.ReadASN1Element(Ꮡtbs, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedTbsˢ));
    }
    cert.Value.RawTBSCertificate = tbs;
    if (!tbs.ReadASN1(Ꮡtbs, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedTbsˢ));
    }
    if (!tbs.ReadOptionalASN1Integer(cert.of(Certificate.ᏑVersion), ((cryptobyte_asn1.Tag)0).Constructed().ContextSpecific(), (nint)(0))) {
        return (default!, errors.New(x509MalformedVersionˢ));
    }
    if ((~cert).Version < 0) {
        return (default!, errors.New(x509MalformedVersionˢ));
    }
    // for backwards compat reasons Version is one-indexed,
    // rather than zero-indexed as defined in 5280
    cert.Value.Version++;
    if ((~cert).Version > 3) {
        return (default!, errors.New(x509InvalidVersionˢ));
    }
    var serial = @new<bigꓸInt>();
    if (!tbs.ReadASN1Integer(serial.OrTypedNil())) {
        return (default!, errors.New(x509MalformedSerialˢ));
    }
    if (serial.Sign() == -1) {
        if (x509negativeserial.Value() != "1"u8){
            return (default!, errors.New(x509NegativeSerialNumberˢ));
        } else {
            x509negativeserial.IncNonDefault();
        }
    }
    cert.Value.SerialNumber = serial;
    ref var sigAISeq = ref heap<cryptobyte.String>(out var ᏑsigAISeq);
    if (!tbs.ReadASN1(ᏑsigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedSignatureˢ));
    }
    // Before parsing the inner algorithm identifier, extract
    // the outer algorithm identifier and make sure that they
    // match.
    ref var outerSigAISeq = ref heap<cryptobyte.String>(out var ᏑouterSigAISeq);
    if (!input.ReadASN1(ᏑouterSigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedAlgorithmˢ));
    }
    if (!bytes.Equal(outerSigAISeq, sigAISeq)) {
        return (default!, errors.New(x509InnerAndOuterˢ));
    }
    var (sigAI, err) = parseAI(sigAISeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.Value.SignatureAlgorithm = getSignatureAlgorithmFromAI(sigAI);
    ref var issuerSeq = ref heap<cryptobyte.String>(out var ᏑissuerSeq);
    if (!tbs.ReadASN1Element(ᏑissuerSeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedIssuerˢ));
    }
    cert.Value.RawIssuer = issuerSeq;
    (var issuerRDNs, err) = parseName(issuerSeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.of(Certificate.ᏑIssuer).FillFromRDNSequence(issuerRDNs);
    ref var validity = ref heap<cryptobyte.String>(out var Ꮡvalidity);
    if (!tbs.ReadASN1(Ꮡvalidity, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedValidityˢ));
    }
    (cert.Value.NotBefore, cert.Value.NotAfter, err) = parseValidity(validity);
    if (err != default!) {
        return (default!, err);
    }
    ref var subjectSeq = ref heap<cryptobyte.String>(out var ᏑsubjectSeq);
    if (!tbs.ReadASN1Element(ᏑsubjectSeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedIssuerˢ));
    }
    cert.Value.RawSubject = subjectSeq;
    (var subjectRDNs, err) = parseName(subjectSeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.of(Certificate.ᏑSubject).FillFromRDNSequence(subjectRDNs);
    ref var spki = ref heap<cryptobyte.String>(out var Ꮡspki);
    if (!tbs.ReadASN1Element(Ꮡspki, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedSpkiˢ));
    }
    cert.Value.RawSubjectPublicKeyInfo = spki;
    if (!spki.ReadASN1(Ꮡspki, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedSpkiˢ));
    }
    ref var pkAISeq = ref heap<cryptobyte.String>(out var ᏑpkAISeq);
    if (!spki.ReadASN1(ᏑpkAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedPublicKeyˢ));
    }
    ref var pkAI = ref heap<pkix.AlgorithmIdentifier>(out var ᏑpkAI);
    (pkAI, err) = parseAI(pkAISeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.Value.PublicKeyAlgorithm = getPublicKeyAlgorithmFromOID(pkAI.Algorithm);
    ref var spk = ref heap(new asn1.BitString(), out var Ꮡspk);
    if (!spki.ReadASN1BitString(Ꮡspk)) {
        return (default!, errors.New(x509Malformedˢ2));
    }
    if ((~cert).PublicKeyAlgorithm != UnknownPublicKeyAlgorithm) {
        var ᴛ1 = new publicKeyInfo(
            Algorithm: pkAI,
            PublicKey: spk
        );
        (cert.Value.PublicKey, err) = parsePublicKey(ref ᴛ1);
        if (err != default!) {
            return (default!, err);
        }
    }
    if ((~cert).Version > 1) {
        if (!tbs.SkipOptionalASN1(((cryptobyte_asn1.Tag)1).ContextSpecific())) {
            return (default!, errors.New(x509Malformedˢ3));
        }
        if (!tbs.SkipOptionalASN1(((cryptobyte_asn1.Tag)2).ContextSpecific())) {
            return (default!, errors.New(x509Malformedˢ4));
        }
        if ((~cert).Version == 3) {
            ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
            ref var present = ref heap(new bool(), out var Ꮡpresent);
            if (!tbs.ReadOptionalASN1(Ꮡextensions, Ꮡpresent, ((cryptobyte_asn1.Tag)3).Constructed().ContextSpecific())) {
                return (default!, errors.New(x509MalformedExtensionsˢ));
            }
            if (present) {
                var seenExts = new map<@string, bool>();
                if (!extensions.ReadASN1(Ꮡextensions, cryptobyte_asn1.SEQUENCE)) {
                    return (default!, errors.New(x509MalformedExtensionsˢ));
                }
                while (!extensions.Empty()) {
                    ref var extension = ref heap<cryptobyte.String>(out var Ꮡextension);
                    if (!extensions.ReadASN1(Ꮡextension, cryptobyte_asn1.SEQUENCE)) {
                        return (default!, errors.New(x509MalformedExtensionˢ4));
                    }
                    var (ext, errΔ1) = parseExtension(extension);
                    if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                    @string oidStr = ext.Id.String();
                    if (seenExts[oidStr]) {
                        return (default!, fmt.Errorf("x509: certificate contains duplicate extension with OID %q"u8, oidStr));
                    }
                    seenExts[oidStr] = true;
                    cert.Value.Extensions = append((~cert).Extensions, ext);
                }
                err = processExtensions(cert);
                if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    ref var signature = ref heap(new asn1.BitString(), out var Ꮡsignature);
    if (!input.ReadASN1BitString(Ꮡsignature)) {
        return (default!, errors.New(x509MalformedSignatureˢ2));
    }
    cert.Value.Signature = signature.RightAlign();
    return (cert, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509TrailingDataˢ = "x509: trailing data"u8;

// ParseCertificate parses a single certificate from the given ASN.1 DER data.
//
// Before Go 1.23, ParseCertificate accepted certificates with negative serial
// numbers. This behavior can be restored by including "x509negativeserial=1" in
// the GODEBUG environment variable.
public static (ж<Certificate>, error) ParseCertificate(slice<byte> der) {
    var (cert, err) = parseCertificate(der);
    if (err != default!) {
        return (default!, err);
    }
    if (builtin.len(der) != builtin.len((~cert).Raw)) {
        return (default!, errors.New(x509TrailingDataˢ));
    }
    return (cert, err);
}

// ParseCertificates parses one or more certificates from the given ASN.1 DER
// data. The certificates must be concatenated with no intermediate padding.
public static (slice<ж<Certificate>>, error) ParseCertificates(slice<byte> der) {
    slice<ж<Certificate>> certs = default!;
    while (builtin.len(der) > 0) {
        var (cert, err) = parseCertificate(der);
        if (err != default!) {
            return (default!, err);
        }
        certs = append(certs, cert);
        der = der.slice(builtin.len((~cert).Raw));
    }
    return (certs, default!);
}

// The X.509 standards confusingly 1-indexed the version names, but 0-indexed
// the actual encoded version, so the version for X.509v2 is 1.
internal static UntypedInt x509v2Version => 1;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509MalformedCrlˢ = "x509: malformed crl"u8;
internal static readonly @string x509MalformedTbsCrlˢ = "x509: malformed tbs crl"u8;
internal static readonly @string x509UnsupportedCrlˢ = "x509: unsupported crl version"u8;
internal static readonly @string x509MalformedCrlNumberˢ = "x509: malformed crl number"u8;

// ParseRevocationList parses a X509 v2 [Certificate] Revocation List from the given
// ASN.1 DER data.
public static (ж<RevocationList>, error) ParseRevocationList(slice<byte> der) {
    var rl = Ꮡ(new RevocationList(nil));
    ref var input = ref heap<cryptobyte.String>(out var Ꮡinput);
    input = ((cryptobyte.String)der);
    // we read the SEQUENCE including length and tag bytes so that
    // we can populate RevocationList.Raw, before unwrapping the
    // SEQUENCE so it can be operated on
    if (!input.ReadASN1Element(Ꮡinput, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedCrlˢ));
    }
    rl.Value.Raw = input;
    if (!input.ReadASN1(Ꮡinput, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedCrlˢ));
    }
    ref var tbs = ref heap<cryptobyte.String>(out var Ꮡtbs);
    // do the same trick again as above to extract the raw
    // bytes for Certificate.RawTBSCertificate
    if (!input.ReadASN1Element(Ꮡtbs, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedTbsCrlˢ));
    }
    rl.Value.RawTBSRevocationList = tbs;
    if (!tbs.ReadASN1(Ꮡtbs, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedTbsCrlˢ));
    }
    ref var version = ref heap(new nint(), out var Ꮡversion);
    if (!tbs.PeekASN1Tag(cryptobyte_asn1.INTEGER)) {
        return (default!, errors.New(x509UnsupportedCrlˢ));
    }
    if (!tbs.ReadASN1Integer(Ꮡversion)) {
        return (default!, errors.New(x509MalformedCrlˢ));
    }
    if (version != x509v2Version) {
        return (default!, fmt.Errorf("x509: unsupported crl version: %d"u8, version));
    }
    ref var sigAISeq = ref heap<cryptobyte.String>(out var ᏑsigAISeq);
    if (!tbs.ReadASN1(ᏑsigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedSignatureˢ));
    }
    // Before parsing the inner algorithm identifier, extract
    // the outer algorithm identifier and make sure that they
    // match.
    ref var outerSigAISeq = ref heap<cryptobyte.String>(out var ᏑouterSigAISeq);
    if (!input.ReadASN1(ᏑouterSigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedAlgorithmˢ));
    }
    if (!bytes.Equal(outerSigAISeq, sigAISeq)) {
        return (default!, errors.New(x509InnerAndOuterˢ));
    }
    var (sigAI, err) = parseAI(sigAISeq);
    if (err != default!) {
        return (default!, err);
    }
    rl.Value.SignatureAlgorithm = getSignatureAlgorithmFromAI(sigAI);
    ref var signature = ref heap(new asn1.BitString(), out var Ꮡsignature);
    if (!input.ReadASN1BitString(Ꮡsignature)) {
        return (default!, errors.New(x509MalformedSignatureˢ2));
    }
    rl.Value.Signature = signature.RightAlign();
    ref var issuerSeq = ref heap<cryptobyte.String>(out var ᏑissuerSeq);
    if (!tbs.ReadASN1Element(ᏑissuerSeq, cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New(x509MalformedIssuerˢ));
    }
    rl.Value.RawIssuer = issuerSeq;
    (var issuerRDNs, err) = parseName(issuerSeq);
    if (err != default!) {
        return (default!, err);
    }
    rl.of(RevocationList.ᏑIssuer).FillFromRDNSequence(issuerRDNs);
    (rl.Value.ThisUpdate, err) = parseTime(Ꮡtbs);
    if (err != default!) {
        return (default!, err);
    }
    if (tbs.PeekASN1Tag(cryptobyte_asn1.GeneralizedTime) || tbs.PeekASN1Tag(cryptobyte_asn1.UTCTime)) {
        (rl.Value.NextUpdate, err) = parseTime(Ꮡtbs);
        if (err != default!) {
            return (default!, err);
        }
    }
    if (tbs.PeekASN1Tag(cryptobyte_asn1.SEQUENCE)) {
        ref var revokedSeq = ref heap<cryptobyte.String>(out var ᏑrevokedSeq);
        if (!tbs.ReadASN1(ᏑrevokedSeq, cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New(x509MalformedCrlˢ));
        }
        while (!revokedSeq.Empty()) {
            ref var rce = ref heap<RevocationListEntry>(out var Ꮡrce);
            rce = new RevocationListEntry(nil);
            ref var certSeq = ref heap<cryptobyte.String>(out var ᏑcertSeq);
            if (!revokedSeq.ReadASN1Element(ᏑcertSeq, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New(x509MalformedCrlˢ));
            }
            rce.Raw = certSeq;
            if (!certSeq.ReadASN1(ᏑcertSeq, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New(x509MalformedCrlˢ));
            }
            rce.SerialNumber = @new<bigꓸInt>();
            if (!certSeq.ReadASN1Integer(rce.SerialNumber.OrTypedNil())) {
                return (default!, errors.New(x509MalformedSerialˢ));
            }
            (rce.RevocationTime, err) = parseTime(ᏑcertSeq);
            if (err != default!) {
                return (default!, err);
            }
            ref var extensionsΔ1 = ref heap<cryptobyte.String>(out var ᏑextensionsΔ1);
            ref var presentΔ1 = ref heap(new bool(), out var ᏑpresentΔ1);
            if (!certSeq.ReadOptionalASN1(ᏑextensionsΔ1, ᏑpresentΔ1, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New(x509MalformedExtensionsˢ));
            }
            if (presentΔ1) {
                while (!extensionsΔ1.Empty()) {
                    ref var extension = ref heap<cryptobyte.String>(out var Ꮡextension);
                    if (!extensionsΔ1.ReadASN1(Ꮡextension, cryptobyte_asn1.SEQUENCE)) {
                        return (default!, errors.New(x509MalformedExtensionˢ4));
                    }
                    var (ext, errΔ1) = parseExtension(extension);
                    if (errΔ1 != default!) {
                        return (default!, errΔ1);
                    }
                    if (ext.Id.Equal(oidExtensionReasonCode)) {
                        var val = ((cryptobyte.String)ext.Value);
                        if (!val.ReadASN1Enum(Ꮡrce.of(RevocationListEntry.ᏑReasonCode))) {
                            return (default!, fmt.Errorf("x509: malformed reasonCode extension"u8));
                        }
                    }
                    rce.Extensions = append(rce.Extensions, ext);
                }
            }
            rl.Value.RevokedCertificateEntries = append((~rl).RevokedCertificateEntries, rce);
            var rcDeprecated = new pkix.RevokedCertificate(
                SerialNumber: rce.SerialNumber,
                RevocationTime: rce.RevocationTime,
                Extensions: rce.Extensions
            );
            rl.Value.RevokedCertificates = append((~rl).RevokedCertificates, rcDeprecated);
        }
    }
    ref var extensions = ref heap<cryptobyte.String>(out var Ꮡextensions);
    ref var present = ref heap(new bool(), out var Ꮡpresent);
    if (!tbs.ReadOptionalASN1(Ꮡextensions, Ꮡpresent, ((cryptobyte_asn1.Tag)0).Constructed().ContextSpecific())) {
        return (default!, errors.New(x509MalformedExtensionsˢ));
    }
    if (present) {
        if (!extensions.ReadASN1(Ꮡextensions, cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New(x509MalformedExtensionsˢ));
        }
        while (!extensions.Empty()) {
            ref var extension = ref heap<cryptobyte.String>(out var Ꮡextension);
            if (!extensions.ReadASN1(Ꮡextension, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New(x509MalformedExtensionˢ4));
            }
            var (ext, errΔ2) = parseExtension(extension);
            if (errΔ2 != default!) {
                return (default!, errΔ2);
            }
            if (ext.Id.Equal(oidExtensionAuthorityKeyId)){
                (rl.Value.AuthorityKeyId, errΔ2) = parseAuthorityKeyIdentifier(ext);
                if (errΔ2 != default!) {
                    return (default!, errΔ2);
                }
            } else 
            if (ext.Id.Equal(oidExtensionCRLNumber)) {
                var value = ((cryptobyte.String)ext.Value);
                rl.Value.Number = @new<bigꓸInt>();
                if (!value.ReadASN1Integer((~rl).Number.OrTypedNil())) {
                    return (default!, errors.New(x509MalformedCrlNumberˢ));
                }
            }
            rl.Value.Extensions = append((~rl).Extensions, ext);
        }
    }
    return (rl, default!);
}

// domainNameValid is an alloc-less version of the checks that
// domainToReverseLabels does.
internal static bool domainNameValid(@string s, bool constraint) {
    // TODO(#75835): This function omits a number of checks which we
    // really should be doing to enforce that domain names are valid names per
    // RFC 1034. We previously enabled these checks, but this broke a
    // significant number of certificates we previously considered valid, and we
    // happily create via CreateCertificate (et al). We should enable these
    // checks, but will need to gate them behind a GODEBUG.
    //
    // I have left the checks we previously enabled, noted with "TODO(#75835)" so
    // that we can easily re-enable them once we unbreak everyone.
    // TODO(#75835): this should only be true for constraints.
    if (builtin.len(s) == 0) {
        return true;
    }
    // Do not allow trailing period (FQDN format is not allowed in SANs or
    // constraints).
    if (s[builtin.len(s) - 1] == (rune)'.') {
        return false;
    }
    // TODO(#75835): domains must have at least one label, cannot have
    // a leading empty label, and cannot be longer than 253 characters.
    // if len(s) == 0 || (!constraint && s[0] == '.') || len(s) > 253 {
    // 	return false
    // }
    nint lastDot = -1;
    if (constraint && s[0] == (rune)'.') {
        s = s[1..];
    }
    for (nint i = 0; i <= builtin.len(s); i++) {
        if (i < builtin.len(s) && (s[i] < 33 || s[i] > 126)) {
            // Invalid character.
            return false;
        }
        if (i == builtin.len(s) || s[i] == (rune)'.') {
            nint labelLen = i;
            if (lastDot >= 0) {
                labelLen -= lastDot + 1;
            }
            if (labelLen == 0) {
                return false;
            }
            // TODO(#75835): labels cannot be longer than 63 characters.
            // if labelLen > 63 {
            // 	return false
            // }
            lastDot = i;
        }
    }
    return true;
}

} // end x509_package
