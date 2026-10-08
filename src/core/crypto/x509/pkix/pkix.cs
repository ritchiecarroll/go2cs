// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pkix contains shared, low level structures used for ASN.1 parsing
// and serialization of X.509 certificates, CRL and OCSP.
namespace go.crypto.x509;

using asn1 = encoding.asn1_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using time = time_package;
using encoding;
using math;

partial class pkix_package {

// AlgorithmIdentifier represents the ASN.1 structure of the same name. See RFC
// 5280, section 4.1.1.2.
partial struct AlgorithmIdentifier {
    public asn1.ObjectIdentifier Algorithm;
    public asn1.RawValue Parameters; /*`asn1:"optional"`*/
}

partial struct RDNSequence /*[]RelativeDistinguishedNameSET*/;

internal static map<@string, @string> attributeTypeNames = new map<@string, @string>{
    ["2.5.4.6"u8] = "C"u8,
    ["2.5.4.10"u8] = "O"u8,
    ["2.5.4.11"u8] = "OU"u8,
    ["2.5.4.3"u8] = "CN"u8,
    ["2.5.4.5"u8] = "SERIALNUMBER"u8,
    ["2.5.4.7"u8] = "L"u8,
    ["2.5.4.8"u8] = "ST"u8,
    ["2.5.4.9"u8] = "STREET"u8,
    ["2.5.4.17"u8] = "POSTALCODE"u8
};

// String returns a string representation of the sequence r,
// roughly following the RFC 2253 Distinguished Names syntax.
public static @string String(this RDNSequence r) {
    @string s = ""u8;
    for (nint i = 0; i < len(r); i++) {
        var rdn = r[len(r) - 1 - i];
        if (i > 0) {
            s += ","u8;
        }
        foreach (var (j, tv) in rdn) {
            if (j > 0) {
                s += "+"u8;
            }
            @string oidString = tv.Type.String();
            var (typeName, ok) = attributeTypeNames[oidString, ꟷ];
            if (!ok) {
                var (derBytes, err) = asn1.Marshal(tv.Value);
                if (err == default!) {
                    s += oidString + "=#"u8 + hex.EncodeToString(derBytes);
                    continue; // No value escaping necessary.
                }
                typeName = oidString;
            }
            @string valueString = fmt.Sprint(tv.Value);
            var escaped = new slice<rune>(0, len(valueString));
            foreach (var (k, c) in valueString) {
                var escape = false;
                switch (c) {
                case (rune)',' or (rune)'+' or (rune)'"' or (rune)'\\' or (rune)'<' or (rune)'>' or (rune)';': {
                    escape = true;
                    break;
                }
                case (rune)' ': {
                    escape = k == 0 || k == len(valueString) - 1;
                    break;
                }
                case (rune)'#': {
                    escape = k == 0;
                    break;
                }}

                if (escape){
                    escaped = append(escaped, (rune)((rune)'\\'), c);
                } else {
                    escaped = append(escaped, c);
                }
            }
            s += typeName + "="u8 + ((@string)escaped);
        }
    }
    return s;
}

partial struct RelativeDistinguishedNameSET /*[]AttributeTypeAndValue*/;

// AttributeTypeAndValue mirrors the ASN.1 structure of the same name in
// RFC 5280, Section 4.1.2.4.
partial struct AttributeTypeAndValue {
    public asn1.ObjectIdentifier Type;
    public any Value;
}

// AttributeTypeAndValueSET represents a set of ASN.1 sequences of
// [AttributeTypeAndValue] sequences from RFC 2986 (PKCS #10).
partial struct AttributeTypeAndValueSET {
    public asn1.ObjectIdentifier Type;
    public slice<slice<AttributeTypeAndValue>> Value; /*`asn1:"set"`*/
}

// Extension represents the ASN.1 structure of the same name. See RFC
// 5280, section 4.2.
partial struct Extension {
    public asn1.ObjectIdentifier Id;
    public bool Critical; /*`asn1:"optional"`*/
    public slice<byte> Value;
}

// Name represents an X.509 distinguished name. This only includes the common
// elements of a DN. Note that Name is only an approximation of the X.509
// structure. If an accurate representation is needed, asn1.Unmarshal the raw
// subject or issuer as an [RDNSequence].
partial struct Name {
    public slice<@string> Country, Organization, OrganizationalUnit;
    public slice<@string> Locality, Province;
    public slice<@string> StreetAddress, PostalCode;
    public @string SerialNumber, CommonName;
    // Names contains all parsed attributes. When parsing distinguished names,
    // this can be used to extract non-standard attributes that are not parsed
    // by this package. When marshaling to RDNSequences, the Names field is
    // ignored, see ExtraNames.
    public slice<AttributeTypeAndValue> Names;
    // ExtraNames contains attributes to be copied, raw, into any marshaled
    // distinguished names. Values override any attributes with the same OID.
    // The ExtraNames field is not populated when parsing, see Names.
    public slice<AttributeTypeAndValue> ExtraNames;
}

// FillFromRDNSequence populates n from the provided [RDNSequence].
// Multi-entry RDNs are flattened, all entries are added to the
// relevant n fields, and the grouping is not preserved.
public static void FillFromRDNSequence(this ref Name n, ж<RDNSequence> Ꮡrdns) {
    ref var rdns = ref Ꮡrdns.DerefOrNull();

    foreach (var (_, rdn) in rdns) {
        if (len(rdn) == 0) {
            continue;
        }
        foreach (var (_, atv) in rdn) {
            n.Names = append(n.Names, atv);
            var (value, ok) = atv.Value._<@string>(ᐧ);
            if (!ok) {
                continue;
            }
            var t = atv.Type;
            if (len(t) == 4 && t[0] == 2 && t[1] == 5 && t[2] == 4) {
                switch (t[3]) {
                case 3: {
                    n.CommonName = value;
                    break;
                }
                case 5: {
                    n.SerialNumber = value;
                    break;
                }
                case 6: {
                    n.Country = append(n.Country, value);
                    break;
                }
                case 7: {
                    n.Locality = append(n.Locality, value);
                    break;
                }
                case 8: {
                    n.Province = append(n.Province, value);
                    break;
                }
                case 9: {
                    n.StreetAddress = append(n.StreetAddress, value);
                    break;
                }
                case 10: {
                    n.Organization = append(n.Organization, value);
                    break;
                }
                case 11: {
                    n.OrganizationalUnit = append(n.OrganizationalUnit, value);
                    break;
                }
                case 17: {
                    n.PostalCode = append(n.PostalCode, value);
                    break;
                }}

            }
        }
    }
}

internal static slice<nint> oidCountry = new nint[]{2, 5, 4, 6}.slice();
internal static slice<nint> oidOrganization = new nint[]{2, 5, 4, 10}.slice();
internal static slice<nint> oidOrganizationalUnit = new nint[]{2, 5, 4, 11}.slice();
internal static slice<nint> oidCommonName = new nint[]{2, 5, 4, 3}.slice();
internal static slice<nint> oidSerialNumber = new nint[]{2, 5, 4, 5}.slice();
internal static slice<nint> oidLocality = new nint[]{2, 5, 4, 7}.slice();
internal static slice<nint> oidProvince = new nint[]{2, 5, 4, 8}.slice();
internal static slice<nint> oidStreetAddress = new nint[]{2, 5, 4, 9}.slice();
internal static slice<nint> oidPostalCode = new nint[]{2, 5, 4, 17}.slice();

// appendRDNs appends a relativeDistinguishedNameSET to the given RDNSequence
// and returns the new value. The relativeDistinguishedNameSET contains an
// attributeTypeAndValue for each of the given values. See RFC 5280, A.1, and
// search for AttributeTypeAndValue.
internal static RDNSequence appendRDNs(this Name n, RDNSequence @in, slice<@string> values, asn1.ObjectIdentifier oid) {
    if (len(values) == 0 || oidInAttributeTypeAndValue(oid, n.ExtraNames)) {
        return @in;
    }
    var s = new slice<AttributeTypeAndValue>(len(values));
    foreach (var (i, value) in values) {
        s[i].Type = oid;
        s[i].Value = value;
    }
    return append(@in, (RelativeDistinguishedNameSET)(s));
}

// ToRDNSequence converts n into a single [RDNSequence]. The following
// attributes are encoded as multi-value RDNs:
//
//   - Country
//   - Organization
//   - OrganizationalUnit
//   - Locality
//   - Province
//   - StreetAddress
//   - PostalCode
//
// Each ExtraNames entry is encoded as an individual RDN.
public static RDNSequence /*ret*/ ToRDNSequence(this Name n) {
    RDNSequence ret = default!;

    ret = n.appendRDNs(ret, n.Country, oidCountry);
    ret = n.appendRDNs(ret, n.Province, oidProvince);
    ret = n.appendRDNs(ret, n.Locality, oidLocality);
    ret = n.appendRDNs(ret, n.StreetAddress, oidStreetAddress);
    ret = n.appendRDNs(ret, n.PostalCode, oidPostalCode);
    ret = n.appendRDNs(ret, n.Organization, oidOrganization);
    ret = n.appendRDNs(ret, n.OrganizationalUnit, oidOrganizationalUnit);
    if (len(n.CommonName) > 0) {
        ret = n.appendRDNs(ret, new @string[]{n.CommonName}.slice(), oidCommonName);
    }
    if (len(n.SerialNumber) > 0) {
        ret = n.appendRDNs(ret, new @string[]{n.SerialNumber}.slice(), oidSerialNumber);
    }
    foreach (var (_, atv) in n.ExtraNames) {
        ret = append(ret, (RelativeDistinguishedNameSET)(new AttributeTypeAndValue[]{atv}.slice()));
    }
    return ret;
}

// String returns the string form of n, roughly following
// the RFC 2253 Distinguished Names syntax.
public static @string String(this Name n) {
    RDNSequence rdns = default!;
    // If there are no ExtraNames, surface the parsed value (all entries in
    // Names) instead.
    if (n.ExtraNames == default!) {
        foreach (var (_, atv) in n.Names) {
            var t = atv.Type;
            if (len(t) == 4 && t[0] == 2 && t[1] == 5 && t[2] == 4) {
                switch (t[3]) {
                case 3 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 17: {
                    continue;
                    break;
                }}

            }
            // These attributes were already parsed into named fields.
            // Place non-standard parsed values at the beginning of the sequence
            // so they will be at the end of the string. See Issue 39924.
            rdns = append(rdns, (RelativeDistinguishedNameSET)(new AttributeTypeAndValue[]{atv}.slice()));
        }
    }
    rdns = appendꓸꓸꓸ(rdns, n.ToRDNSequence());
    return rdns.String();
}

// oidInAttributeTypeAndValue reports whether a type with the given OID exists
// in atv.
internal static bool oidInAttributeTypeAndValue(asn1.ObjectIdentifier oid, slice<AttributeTypeAndValue> atv) {
    foreach (var (_, a) in atv) {
        if (a.Type.Equal(oid)) {
            return true;
        }
    }
    return false;
}

// CertificateList represents the ASN.1 structure of the same name. See RFC
// 5280, section 5.1. Use Certificate.CheckCRLSignature to verify the
// signature.
//
// Deprecated: x509.RevocationList should be used instead.
partial struct CertificateList {
    public TBSCertificateList TBSCertList;
    public AlgorithmIdentifier SignatureAlgorithm;
    public asn1.BitString SignatureValue;
}

// HasExpired reports whether certList should have been updated by now.
public static bool HasExpired(this ref CertificateList certList, time.Time now) {
    return !now.Before(certList.TBSCertList.NextUpdate);
}

// TBSCertificateList represents the ASN.1 structure of the same name. See RFC
// 5280, section 5.1.
//
// Deprecated: x509.RevocationList should be used instead.
partial struct TBSCertificateList {
    public asn1.RawContent Raw;
    public nint Version; /*`asn1:"optional,default:0"`*/
    public AlgorithmIdentifier Signature;
    public RDNSequence Issuer;
    public time.Time ThisUpdate;
    public time.Time NextUpdate; /*`asn1:"optional"`*/
    public slice<RevokedCertificate> RevokedCertificates; /*`asn1:"optional"`*/
    public slice<Extension> Extensions; /*`asn1:"tag:0,optional,explicit"`*/
}

// RevokedCertificate represents the ASN.1 structure of the same name. See RFC
// 5280, section 5.1.
partial struct RevokedCertificate {
    public ж<bigꓸInt> SerialNumber;
    public time.Time RevocationTime;
    public slice<Extension> Extensions; /*`asn1:"optional"`*/
}

} // end pkix_package
