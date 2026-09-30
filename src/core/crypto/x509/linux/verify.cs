// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using pkix = go.crypto.x509.pkix_package;
using errors = errors_package;
using fmt = fmt_package;
using iter = iter_package;
using maps = maps_package;
using net = net_package;
using netip = go.net.netip_package;
using url = go.net.url_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using time = time_package;
using utf8 = go.unicode.utf8_package;
using asn1 = encoding.asn1_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using encoding;
using go.crypto.x509;
using go.math;
using go.net;
using go.unicode;

partial class x509_package {

[GoType("num:nint")] partial struct InvalidReason;

public static InvalidReason NotAuthorizedToSign => /* iota */ 0;
public static InvalidReason Expired => 1;
public static InvalidReason CANotAuthorizedForThisName => 2;
public static InvalidReason TooManyIntermediates => 3;
public static InvalidReason IncompatibleUsage => 4;
public static InvalidReason NameMismatch => 5;
public static InvalidReason NameConstraintsWithoutSANs => 6;
public static InvalidReason UnconstrainedName => 7;
public static InvalidReason TooManyConstraints => 8;
public static InvalidReason CANotAuthorizedForExtKeyUsage => 9;
public static InvalidReason NoValidChains => 10;

// CertificateInvalidError results when an odd error occurs. Users of this
// library probably want to handle all these errors uniformly.
[GoType] partial struct CertificateInvalidError {
    public ж<Certificate> Cert;
    public InvalidReason Reason;
    public @string Detail;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509CertificateIsNotˢ = "x509: certificate is not authorized to sign other certificates"u8;
internal static readonly @string x509TooManyIntermediatesˢ = "x509: too many intermediates for path length constraint"u8;
internal static readonly @string x509CertificateSpecifiesˢ = "x509: certificate specifies an incompatible key usage"u8;
internal static readonly @string x509IssuerNameDoesNotˢ = "x509: issuer name does not match subject from issuing certificate"u8;
internal static readonly @string x509IssuerHasNameˢ = "x509: issuer has name constraints but leaf doesn't have a SAN extension"u8;
internal static readonly @string x509NoValidChainsBuiltˢ = "x509: no valid chains built"u8;
internal static readonly @string x509UnknownErrorˢ = "x509: unknown error"u8;

public static @string Error(this CertificateInvalidError e) {
    var exprᴛ1 = e.Reason;
    if (exprᴛ1 == NotAuthorizedToSign) {
        return x509CertificateIsNotˢ;
    }
    if (exprᴛ1 == Expired) {
        return "x509: certificate has expired or is not yet valid: "u8 + e.Detail;
    }
    if (exprᴛ1 == CANotAuthorizedForThisName) {
        return "x509: a root or intermediate certificate is not authorized to sign for this name: "u8 + e.Detail;
    }
    if (exprᴛ1 == CANotAuthorizedForExtKeyUsage) {
        return "x509: a root or intermediate certificate is not authorized for an extended key usage: "u8 + e.Detail;
    }
    if (exprᴛ1 == TooManyIntermediates) {
        return x509TooManyIntermediatesˢ;
    }
    if (exprᴛ1 == IncompatibleUsage) {
        return x509CertificateSpecifiesˢ;
    }
    if (exprᴛ1 == NameMismatch) {
        return x509IssuerNameDoesNotˢ;
    }
    if (exprᴛ1 == NameConstraintsWithoutSANs) {
        return x509IssuerHasNameˢ;
    }
    if (exprᴛ1 == UnconstrainedName) {
        return "x509: issuer has name constraints but leaf contains unknown or unconstrained name: "u8 + e.Detail;
    }
    if (exprᴛ1 == NoValidChains) {
        @string s = x509NoValidChainsBuiltˢ;
        if (e.Detail != ""u8) {
            s = fmt.Sprintf("%s: %s"u8, s, e.Detail);
        }
        return s;
    }

    return x509UnknownErrorˢ;
}

// HostnameError results when the set of authorized names doesn't match the
// requested name.
[GoType] partial struct HostnameError {
    public ж<Certificate> Certificate;
    public @string Host;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509CertificateReliesOnˢ = "x509: certificate relies on legacy Common Name field, use SANs instead"u8;

public static @string Error(this HostnameError h) {
    var c = h.Certificate;
    nint maxNamesIncluded = 100;
    if (!c.hasSANExtension() && matchHostnames((~c).Subject.CommonName, h.Host)) {
        return x509CertificateReliesOnˢ;
    }
    ref var valid = ref heap(new strings.Builder(), out var Ꮡvalid);
    {
        var ip = net.ParseIP(h.Host); if (ip != default!){
            // Trying to validate an IP
            if (builtin.len((~c).IPAddresses) == 0) {
                return "x509: cannot validate certificate for "u8 + h.Host + " because it doesn't contain any IP SANs"u8;
            }
            if (builtin.len((~c).IPAddresses) >= maxNamesIncluded) {
                return fmt.Sprintf("x509: certificate is valid for %d IP SANs, but none matched %s"u8, builtin.len((~c).IPAddresses), h.Host);
            }
            foreach (var (_, san) in (~c).IPAddresses) {
                if (valid.Len() > 0) {
                    Ꮡvalid.WriteString(", "u8);
                }
                Ꮡvalid.WriteString(san.String());
            }
        } else {
            if (builtin.len((~c).DNSNames) >= maxNamesIncluded) {
                return fmt.Sprintf("x509: certificate is valid for %d names, but none matched %s"u8, builtin.len((~c).DNSNames), h.Host);
            }
            Ꮡvalid.WriteString(strings.Join((~c).DNSNames, ", "u8));
        }
    }
    if (valid.Len() == 0) {
        return "x509: certificate is not valid for any names, but wanted to match "u8 + h.Host;
    }
    return "x509: certificate is valid for "u8 + valid.String() + ", not "u8 + h.Host;
}

// UnknownAuthorityError results when the certificate issuer is unknown
[GoType] partial struct UnknownAuthorityError {
    public ж<Certificate> Cert;
    // hintErr contains an error that may be helpful in determining why an
    // authority wasn't found.
    internal error hintErr;
    // hintCert contains a possible authority certificate that was rejected
    // because of the error in hintErr.
    internal ж<Certificate> hintCert;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509CertificateSignedByˢ = "x509: certificate signed by unknown authority"u8;

public static @string Error(this UnknownAuthorityError e) {
    @string s = x509CertificateSignedByˢ;
    if (e.hintErr != default!) {
        @string certName = e.hintCert.Value.Subject.CommonName;
        if (builtin.len(certName) == 0) {
            if (builtin.len((~e.hintCert).Subject.Organization) > 0){
                certName = (~e.hintCert).Subject.Organization[0];
            } else {
                certName = "serial:"u8 + (~e.hintCert).SerialNumber.String();
            }
        }
        s += fmt.Sprintf(" (possibly because of %q while trying to verify candidate authority certificate %q)"u8, e.hintErr, certName);
    }
    return s;
}

// SystemRootsError results when we fail to load the system root certificates.
[GoType] partial struct SystemRootsError {
    public error Err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509FailedToLoadSystemˢ = "x509: failed to load system roots and no roots provided"u8;

public static @string Error(this SystemRootsError se) {
    @string msg = x509FailedToLoadSystemˢ;
    if (se.Err != default!) {
        return msg + "; "u8 + se.Err.Error();
    }
    return msg;
}

public static error Unwrap(this SystemRootsError se) {
    return se.Err;
}

// errNotParsed is returned when a certificate without ASN.1 contents is
// verified. Platform-specific verification needs the ASN.1 contents.
internal static error errNotParsed = errors.New("x509: missing ASN.1 contents; use ParseCertificate"u8);

// VerifyOptions contains parameters for Certificate.Verify.
[GoType] partial struct VerifyOptions {
    // DNSName, if set, is checked against the leaf certificate with
    // Certificate.VerifyHostname or the platform verifier.
    public @string DNSName;
    // Intermediates is an optional pool of certificates that are not trust
    // anchors, but can be used to form a chain from the leaf certificate to a
    // root certificate.
    public ж<CertPool> Intermediates;
    // Roots is the set of trusted root certificates the leaf certificate needs
    // to chain up to. If nil, the system roots or the platform verifier are used.
    public ж<CertPool> Roots;
    // CurrentTime is used to check the validity of all certificates in the
    // chain. If zero, the current time is used.
    public time.Time CurrentTime;
    // KeyUsages specifies which Extended Key Usage values are acceptable. A
    // chain is accepted if it allows any of the listed values. An empty list
    // means ExtKeyUsageServerAuth. To accept any key usage, include ExtKeyUsageAny.
    public slice<ExtKeyUsage> KeyUsages;
    // MaxConstraintComparisions is the maximum number of comparisons to
    // perform when checking a given certificate's name constraints. If
    // zero, a sensible default is used. This limit prevents pathological
    // certificates from consuming excessive amounts of CPU time when
    // validating. It does not apply to the platform verifier.
    public nint MaxConstraintComparisions;
    // CertificatePolicies specifies which certificate policy OIDs are
    // acceptable during policy validation. An empty CertificatePolices
    // field implies any valid policy is acceptable.
    public slice<OID> CertificatePolicies;
// The following policy fields are unexported, because we do not expect
// users to actually need to use them, but are useful for testing the
// policy validation code.

    // inhibitPolicyMapping indicates if policy mapping should be allowed
    // during path validation.
    internal bool inhibitPolicyMapping;
    // requireExplicitPolicy indidicates if explicit policies must be present
    // for each certificate being validated.
    internal bool requireExplicitPolicy;
    // inhibitAnyPolicy indicates if the anyPolicy policy should be
    // processed if present in a certificate being validated.
    internal bool inhibitAnyPolicy;
}

internal static UntypedInt leafCertificate => iota;
internal static UntypedInt intermediateCertificate => 1;
internal static UntypedInt rootCertificate => 2;

// rfc2821Mailbox represents a “mailbox” (which is an email address to most
// people) by breaking it into the “local” (i.e. before the '@') and “domain”
// parts.
[GoType] partial struct rfc2821Mailbox {
    internal @string local, domain;
}

// parseRFC2821Mailbox parses an email address into local and domain parts,
// based on the ABNF for a “Mailbox” from RFC 2821. According to RFC 5280,
// Section 4.2.1.6 that's correct for an rfc822Name from a certificate: “The
// format of an rfc822Name is a "Mailbox" as defined in RFC 2821, Section 4.1.2”.
internal static (rfc2821Mailbox mailbox, bool ok) parseRFC2821Mailbox(@string @in) {
    rfc2821Mailbox mailbox = default!;

    if (builtin.len(@in) == 0) {
        return (mailbox, false);
    }
    var localPartBytes = new slice<byte>(0, builtin.len(@in) / 2);
    if (@in[0] == (rune)'"'){
        // Quoted-string = DQUOTE *qcontent DQUOTE
        // non-whitespace-control = %d1-8 / %d11 / %d12 / %d14-31 / %d127
        // qcontent = qtext / quoted-pair
        // qtext = non-whitespace-control /
        //         %d33 / %d35-91 / %d93-126
        // quoted-pair = ("\" text) / obs-qp
        // text = %d1-9 / %d11 / %d12 / %d14-127 / obs-text
        //
        // (Names beginning with “obs-” are the obsolete syntax from RFC 2822,
        // Section 4. Since it has been 16 years, we no longer accept that.)
        @in = @in[1..];
QuotedString:
        while (ᐧ) {
            if (builtin.len(@in) == 0) {
                return (mailbox, false);
            }
            var c = @in[0];
            @in = @in[1..];
            switch (ᐧ) {
            case {} when c is (rune)'"': {
                goto break_QuotedString;
                break;
            }
            case {} when c is (rune)'\\': {
                if (builtin.len(@in) == 0) {
                    // quoted-pair
                    return (mailbox, false);
                }
                if (@in[0] == 11 || @in[0] == 12 || (1 <= @in[0] && @in[0] <= 9) || (14 <= @in[0] && @in[0] <= 127)){
                    localPartBytes = append(localPartBytes, @in[0]);
                    @in = @in[1..];
                } else {
                    return (mailbox, false);
                }
                break;
            }
            case {} when c == 11 || c == 12 || c == 32 || c == 33 || c == 127 || (1 <= c && c <= 8) || (14 <= c && c <= 31) || (35 <= c && c <= 91) || (93 <= c && c <= 126): {
                localPartBytes = append(localPartBytes, // Space (char 32) is not allowed based on the
 // BNF, but RFC 3696 gives an example that
 // assumes that it is. Several “verified”
 // errata continue to argue about this point.
 // We choose to accept it.
 // qtext
 c);
                break;
            }
            default: {
                return (mailbox, false);
            }}

continue_QuotedString:;
        }
break_QuotedString:;
    } else {
        // Atom ("." Atom)*
NextChar:
        while (builtin.len(@in) > 0) {
            // atext from RFC 2822, Section 3.2.4
            var c = @in[0];
            var matchᴛ1 = false;
            if (c is (rune)'\\') { matchᴛ1 = true;
                @in = @in[1..];
                if (builtin.len(@in) == 0) {
                    // Examples given in RFC 3696 suggest that
                    // escaped characters can appear outside of a
                    // quoted string. Several “verified” errata
                    // continue to argue the point. We choose to
                    // accept it.
                    return (mailbox, false);
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && (((rune)'0' <= c && c <= (rune)'9') || ((rune)'a' <= c && c <= (rune)'z') || ((rune)'A' <= c && c <= (rune)'Z') || c == (rune)'!' || c == (rune)'#' || c == (rune)'$' || c == (rune)'%' || c == (rune)'&' || c == (rune)'\'' || c == (rune)'*' || c == (rune)'+' || c == (rune)'-' || c == (rune)'/' || c == (rune)'=' || c == (rune)'?' || c == (rune)'^' || c == (rune)'_' || c == (rune)'`' || c == (rune)'{' || c == (rune)'|' || c == (rune)'}' || c == (rune)'~' || c == (rune)'.')) {
                localPartBytes = append(localPartBytes, @in[0]);
                @in = @in[1..];
            }
            else if (!matchᴛ1) { /* default: */
                goto break_NextChar;
            }

continue_NextChar:;
        }
break_NextChar:;
        if (builtin.len(localPartBytes) == 0) {
            return (mailbox, false);
        }
        // From RFC 3696, Section 3:
        // “period (".") may also appear, but may not be used to start
        // or end the local part, nor may two or more consecutive
        // periods appear.”
        var twoDots = new byte[]{(rune)'.', (rune)'.'}.slice();
        if (localPartBytes[0] == (rune)'.' || localPartBytes[builtin.len(localPartBytes) - 1] == (rune)'.' || bytes.Contains(localPartBytes, twoDots)) {
            return (mailbox, false);
        }
    }
    if (builtin.len(@in) == 0 || @in[0] != (rune)'@') {
        return (mailbox, false);
    }
    @in = @in[1..];
    // The RFC species a format for domains, but that's known to be
    // violated in practice so we accept that anything after an '@' is the
    // domain part.
    {
        var (_, okΔ1) = domainToReverseLabels(@in); if (!okΔ1) {
            return (mailbox, false);
        }
    }
    mailbox.local = ((@string)localPartBytes);
    mailbox.domain = @in;
    return (mailbox, true);
}

// domainToReverseLabels converts a textual domain name like foo.example.com to
// the list of labels in reverse order, e.g. ["com", "example", "foo"].
internal static (slice<@string> reverseLabels, bool ok) domainToReverseLabels(@string domain) {
    slice<@string> reverseLabels = default!;

    reverseLabels = new slice<@string>(0, strings.Count(domain, "."u8) + 1);
    while (builtin.len(domain) > 0) {
        {
            nint i = strings.LastIndexByte(domain, (rune)'.'); if (i == -1){
                reverseLabels = append(reverseLabels, domain);
                domain = ""u8;
            } else {
                reverseLabels = append(reverseLabels, domain.slice(i + 1));
                domain = domain.slice(0, i);
                if (i == 0) {
                    // domain == ""
                    // domain is prefixed with an empty label, append an empty
                    // string to reverseLabels to indicate this.
                    reverseLabels = append(reverseLabels, ""u8);
                }
            }
        }
    }
    if (builtin.len(reverseLabels) > 0 && builtin.len(reverseLabels[0]) == 0) {
        // An empty label at the end indicates an absolute value.
        return (default!, false);
    }
    foreach (var (_, label) in reverseLabels) {
        if (builtin.len(label) == 0) {
            // Empty labels are otherwise invalid.
            return (default!, false);
        }
        foreach (var (_, c) in label) {
            if (c < 33 || c > 126) {
                // Invalid character.
                return (default!, false);
            }
        }
    }
    return (reverseLabels, true);
}

internal static (bool, error) matchEmailConstraint(rfc2821Mailbox mailbox, @string constraint, bool excluded, map<@string, slice<@string>> reversedDomainsCache, map<@string, slice<@string>> reversedConstraintsCache) {
    // If the constraint contains an @, then it specifies an exact mailbox
    // name.
    if (strings.Contains(constraint, "@"u8)) {
        var (constraintMailbox, ok) = parseRFC2821Mailbox(constraint);
        if (!ok) {
            return (false, fmt.Errorf("x509: internal error: cannot parse constraint %q"u8, constraint));
        }
        return (mailbox.local == constraintMailbox.local && strings.EqualFold(mailbox.domain, constraintMailbox.domain), default!);
    }
    // Otherwise the constraint is like a DNS constraint of the domain part
    // of the mailbox.
    return matchDomainConstraint(mailbox.domain, constraint, excluded, reversedDomainsCache, reversedConstraintsCache);
}

internal static (bool, error) matchURIConstraint(ж<url.URL> Ꮡuri, @string constraint, bool excluded, map<@string, slice<@string>> reversedDomainsCache, map<@string, slice<@string>> reversedConstraintsCache) {
    ref var uri = ref Ꮡuri.DerefOrNull();

    // From RFC 5280, Section 4.2.1.10:
    // “a uniformResourceIdentifier that does not include an authority
    // component with a host name specified as a fully qualified domain
    // name (e.g., if the URI either does not include an authority
    // component or includes an authority component in which the host name
    // is specified as an IP address), then the application MUST reject the
    // certificate.”
    @string host = uri.Host;
    if (builtin.len(host) == 0) {
        return (false, fmt.Errorf("URI with empty host (%q) cannot be matched against constraints"u8, uri.String()));
    }
    if (strings.Contains(host, ":"u8) && !strings.HasSuffix(host, "]"u8)) {
        error err = default!;
        (host, _, err) = net.SplitHostPort(uri.Host);
        if (err != default!) {
            return (false, err);
        }
    }
    // netip.ParseAddr will reject the URI IPv6 literal form "[...]", so we
    // check if _either_ the string parses as an IP, or if it is enclosed in
    // square brackets.
    {
        var (_, err) = netip.ParseAddr(host); if (err == default! || (strings.HasPrefix(host, "["u8) && strings.HasSuffix(host, "]"u8))) {
            return (false, fmt.Errorf("URI with IP (%q) cannot be matched against constraints"u8, uri.String()));
        }
    }
    return matchDomainConstraint(host, constraint, excluded, reversedDomainsCache, reversedConstraintsCache);
}

internal static (bool, error) matchIPConstraint(net.IP ip, ref net.IPNet constraint) {
    if (builtin.len(ip) != builtin.len(constraint.IP)) {
        return (false, default!);
    }
    foreach (var (i, _) in ip) {
        {
            var mask = constraint.Mask[i]; if ((byte)(ip[i] & mask) != (byte)(constraint.IP[i] & mask)) {
                return (false, default!);
            }
        }
    }
    return (true, default!);
}

internal static (bool, error) matchDomainConstraint(@string domain, @string constraint, bool excluded, map<@string, slice<@string>> reversedDomainsCache, map<@string, slice<@string>> reversedConstraintsCache) {
    // The meaning of zero length constraints is not specified, but this
    // code follows NSS and accepts them as matching everything.
    if (builtin.len(constraint) == 0) {
        return (true, default!);
    }
    var (domainLabels, found) = reversedDomainsCache[domain, ꟷ];
    if (!found) {
        bool ok = default!;
        (domainLabels, ok) = domainToReverseLabels(domain);
        if (!ok) {
            return (false, fmt.Errorf("x509: internal error: cannot parse domain %q"u8, domain));
        }
        reversedDomainsCache[domain] = domainLabels;
    }
    var wildcardDomain = false;
    if (builtin.len(domain) > 0 && domain[0] == (rune)'*') {
        wildcardDomain = true;
    }
    // RFC 5280 says that a leading period in a domain name means that at
    // least one label must be prepended, but only for URI and email
    // constraints, not DNS constraints. The code also supports that
    // behaviour for DNS constraints.
    var mustHaveSubdomains = false;
    if (constraint[0] == (rune)'.') {
        mustHaveSubdomains = true;
        constraint = constraint[1..];
    }
    (var constraintLabels, found) = reversedConstraintsCache[constraint, ꟷ];
    if (!found) {
        bool ok = default!;
        (constraintLabels, ok) = domainToReverseLabels(constraint);
        if (!ok) {
            return (false, fmt.Errorf("x509: internal error: cannot parse domain %q"u8, constraint));
        }
        reversedConstraintsCache[constraint] = constraintLabels;
    }
    if (builtin.len(domainLabels) < builtin.len(constraintLabels) || (mustHaveSubdomains && builtin.len(domainLabels) == builtin.len(constraintLabels))) {
        return (false, default!);
    }
    if (excluded && wildcardDomain && builtin.len(domainLabels) > 1 && builtin.len(constraintLabels) > 1) {
        domainLabels = domainLabels.slice(0, builtin.len(domainLabels) - 1);
        constraintLabels = constraintLabels.slice(0, builtin.len(constraintLabels) - 1);
    }
    foreach (var (i, constraintLabel) in constraintLabels) {
        if (!strings.EqualFold(constraintLabel, domainLabels[i])) {
            return (false, default!);
        }
    }
    return (true, default!);
}

// checkNameConstraints checks that c permits a child certificate to claim the
// given name, of type nameType. The argument parsedName contains the parsed
// form of name, suitable for passing to the match function. The total number
// of comparisons is tracked in the given count and should not exceed the given
// limit.
internal static error checkNameConstraints(this ж<Certificate> Ꮡc, ж<nint> Ꮡcount, nint maxConstraintComparisons, @string nameType, @string name, any parsedName, Func<any, any, bool, (bool, error)> match, any permitted, any excluded) {
    ref var count = ref Ꮡcount.DerefOrNull();

    var excludedValue = reflect.ValueOf(excluded);
    count += excludedValue.Len();
    if (count > maxConstraintComparisons) {
        return new CertificateInvalidError(Ꮡc, TooManyConstraints, ""u8);
    }
    for (nint i = 0; i < excludedValue.Len(); i++) {
        var constraint = excludedValue.Index(i).Interface();
        var (matchΔ1, err) = match(parsedName, constraint, true);
        if (err != default!) {
            return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, err.Error());
        }
        if (matchΔ1) {
            return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, fmt.Sprintf("%s %q is excluded by constraint %q"u8, nameType, name, constraint));
        }
    }
    var permittedValue = reflect.ValueOf(permitted);
    count += permittedValue.Len();
    if (count > maxConstraintComparisons) {
        return new CertificateInvalidError(Ꮡc, TooManyConstraints, ""u8);
    }
    var ok = true;
    for (nint i = 0; i < permittedValue.Len(); i++) {
        var constraint = permittedValue.Index(i).Interface();
        error err = default!;
        {
            (ok, err) = match(parsedName, constraint, false); if (err != default!) {
                return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, err.Error());
            }
        }
        if (ok) {
            break;
        }
    }
    if (!ok) {
        return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, fmt.Sprintf("%s %q is not permitted by any constraint"u8, nameType, name));
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509InternalErrorEmptyˢ = "x509: internal error: empty chain when appending CA cert"u8;
internal static readonly @string emailAddressˢ = "email address"u8;
internal static readonly @string dnsNameˢ = "DNS name"u8;
internal static readonly @string uriˢ = "URI"u8;
internal static readonly @string ipAddressˢ = "IP address"u8;

// isValid performs validity checks on c given that it is a candidate to append
// to the chain in currentChain.
internal static error isValid(this ж<Certificate> Ꮡc, nint certType, slice<ж<Certificate>> currentChain, ж<VerifyOptions> Ꮡopts) {
    ref var c = ref Ꮡc.DerefOrNull();
    ref var opts = ref Ꮡopts.DerefOrNull();

    if (builtin.len(c.UnhandledCriticalExtensions) > 0) {
        return new UnhandledCriticalExtension(nil);
    }
    if (builtin.len(currentChain) > 0) {
        var child = currentChain[builtin.len(currentChain) - 1];
        if (!bytes.Equal((~child).RawIssuer, c.RawSubject)) {
            return new CertificateInvalidError(Ꮡc, NameMismatch, ""u8);
        }
    }
    var now = opts.CurrentTime;
    if (now.IsZero()) {
        now = time.Now();
    }
    if (now.Before(c.NotBefore)){
        return new CertificateInvalidError(
            Cert: Ꮡc,
            Reason: Expired,
            Detail: fmt.Sprintf("current time %s is before %s"u8, now.Format(time.RFC3339), c.NotBefore.Format(time.RFC3339))
        );
    } else 
    if (now.After(c.NotAfter)) {
        return new CertificateInvalidError(
            Cert: Ꮡc,
            Reason: Expired,
            Detail: fmt.Sprintf("current time %s is after %s"u8, now.Format(time.RFC3339), c.NotAfter.Format(time.RFC3339))
        );
    }
    nint maxConstraintComparisons = opts.MaxConstraintComparisions;
    if (maxConstraintComparisons == 0) {
        maxConstraintComparisons = 250000;
    }
    ref var comparisonCount = ref heap<nint>(out var ᏑcomparisonCount);
    comparisonCount = 0;
    if (certType == intermediateCertificate || certType == rootCertificate) {
        if (builtin.len(currentChain) == 0) {
            return errors.New(x509InternalErrorEmptyˢ);
        }
    }
    // Each time we do constraint checking, we need to check the constraints in
    // the current certificate against all of the names that preceded it. We
    // reverse these names using domainToReverseLabels, which is a relatively
    // expensive operation. Since we check each name against each constraint,
    // this requires us to do N*C calls to domainToReverseLabels (where N is the
    // total number of names that preceed the certificate, and C is the total
    // number of constraints in the certificate). By caching the results of
    // calling domainToReverseLabels, we can reduce that to N+C calls at the
    // cost of keeping all of the parsed names and constraints in memory until
    // we return from isValid.
    var reversedDomainsCache = new map<@string, slice<@string>>{};
    var reversedConstraintsCache = new map<@string, slice<@string>>{};
    if ((certType == intermediateCertificate || certType == rootCertificate) && c.hasNameConstraints()) {
        var toCheck = new ж<Certificate>[]{}.slice();
        foreach (var (_, cΔ1) in currentChain) {
            if (cΔ1.hasSANExtension()) {
                toCheck = append(toCheck, cΔ1);
            }
        }
        foreach (var (_, sanCert) in toCheck) {
            var reversedConstraintsCacheʗ1 = reversedConstraintsCache;
            var reversedDomainsCacheʗ1 = reversedDomainsCache;
            var err = forEachSAN(sanCert.getSANExtension(), error (nint tag, slice<byte> data) => {
                var exprᴛ1 = tag;
                if (exprᴛ1 == nameTypeEmail) {
                    @string name = ((@string)data);
                    var (mailbox, ok) = parseRFC2821Mailbox(name);
                    if (!ok) {
                        return fmt.Errorf("x509: cannot parse rfc822Name %q"u8, mailbox);
                    }
                    {
                            var reversedConstraintsCacheʗ2 = reversedConstraintsCacheʗ1;
                            var reversedDomainsCacheʗ2 = reversedDomainsCacheʗ1;
                        var errΔ6 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, emailAddressˢ, name, mailbox,
                            (any parsedName, any constraint, bool excluded) => matchEmailConstraint(parsedName._<rfc2821Mailbox>(), constraint._<@string>(), excluded, reversedDomainsCacheʗ2, reversedConstraintsCacheʗ2), Ꮡc.Value.PermittedEmailAddresses, Ꮡc.Value.ExcludedEmailAddresses); if (errΔ6 != default!) {
                            return errΔ6;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeDNS) {
                    @string name = ((@string)data);
                    if (!domainNameValid(name, false)) {
                        return fmt.Errorf("x509: cannot parse dnsName %q"u8, name);
                    }
                    {
                            var reversedConstraintsCacheʗ3 = reversedConstraintsCacheʗ1;
                            var reversedDomainsCacheʗ3 = reversedDomainsCacheʗ1;
                        var errΔ7 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, dnsNameˢ, name, name,
                            (any parsedName, any constraint, bool excluded) => matchDomainConstraint(parsedName._<@string>(), constraint._<@string>(), excluded, reversedDomainsCacheʗ3, reversedConstraintsCacheʗ3), Ꮡc.Value.PermittedDNSDomains, Ꮡc.Value.ExcludedDNSDomains); if (errΔ7 != default!) {
                            return errΔ7;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeURI) {
                    @string name = ((@string)data);
                    var (uri, errΔ8) = url.Parse(name);
                    if (errΔ8 != default!) {
                        return fmt.Errorf("x509: internal error: URI SAN %q failed to parse"u8, name);
                    }
                    {
                            var reversedConstraintsCacheʗ4 = reversedConstraintsCacheʗ1;
                            var reversedDomainsCacheʗ4 = reversedDomainsCacheʗ1;
                        var errΔ9 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, uriˢ, name, uri.OrTypedNil(),
                            (any parsedName, any constraint, bool excluded) => matchURIConstraint(parsedName._<ж<url.URL>>(), constraint._<@string>(), excluded, reversedDomainsCacheʗ4, reversedConstraintsCacheʗ4), Ꮡc.Value.PermittedURIDomains, Ꮡc.Value.ExcludedURIDomains); if (errΔ9 != default!) {
                            return errΔ9;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeIP) {
                    var ip = ((net.IP)data);
                    {
                        nint l = builtin.len(ip); if (l != net.IPv4len && l != net.IPv6len) {
                            return fmt.Errorf("x509: internal error: IP SAN %x failed to parse"u8, data);
                        }
                    }
                    {
                        var errΔ10 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, ipAddressˢ, ip.String(), ip,
                            (any parsedName, any constraint, bool _) => matchIPConstraint(parsedName._<net.IP>(), ref (constraint._<ж<net.IPNet>>()).DerefOrNull()), Ꮡc.Value.PermittedIPRanges, Ꮡc.Value.ExcludedIPRanges); if (errΔ10 != default!) {
                            return errΔ10;
                        }
                    }
                }
                else { /* default: */
                }

                // Unknown SAN types are ignored.
                return default!;
            });
            if (err != default!) {
                return err;
            }
        }
    }
    // KeyUsage status flags are ignored. From Engineering Security, Peter
    // Gutmann: A European government CA marked its signing certificates as
    // being valid for encryption only, but no-one noticed. Another
    // European CA marked its signature keys as not being valid for
    // signatures. A different CA marked its own trusted root certificate
    // as being invalid for certificate signing. Another national CA
    // distributed a certificate to be used to encrypt data for the
    // country’s tax authority that was marked as only being usable for
    // digital signatures but not for encryption. Yet another CA reversed
    // the order of the bit flags in the keyUsage due to confusion over
    // encoding endianness, essentially setting a random keyUsage in
    // certificates that it issued. Another CA created a self-invalidating
    // certificate by adding a certificate policy statement stipulating
    // that the certificate had to be used strictly as specified in the
    // keyUsage, and a keyUsage containing a flag indicating that the RSA
    // encryption key could only be used for Diffie-Hellman key agreement.
    if (certType == intermediateCertificate && (!c.BasicConstraintsValid || !c.IsCA)) {
        return new CertificateInvalidError(Ꮡc, NotAuthorizedToSign, ""u8);
    }
    if (c.BasicConstraintsValid && c.MaxPathLen >= 0) {
        nint numIntermediates = builtin.len(currentChain) - 1;
        if (numIntermediates > c.MaxPathLen) {
            return new CertificateInvalidError(Ꮡc, TooManyIntermediates, ""u8);
        }
    }
    return default!;
}

// Verify attempts to verify c by building one or more chains from c to a
// certificate in opts.Roots, using certificates in opts.Intermediates if
// needed. If successful, it returns one or more chains where the first
// element of the chain is c and the last element is from opts.Roots.
//
// If opts.Roots is nil, the platform verifier might be used, and
// verification details might differ from what is described below. If system
// roots are unavailable the returned error will be of type SystemRootsError.
//
// Name constraints in the intermediates will be applied to all names claimed
// in the chain, not just opts.DNSName. Thus it is invalid for a leaf to claim
// example.com if an intermediate doesn't permit it, even if example.com is not
// the name being validated. Note that DirectoryName constraints are not
// supported.
//
// Name constraint validation follows the rules from RFC 5280, with the
// addition that DNS name constraints may use the leading period format
// defined for emails and URIs. When a constraint has a leading period
// it indicates that at least one additional label must be prepended to
// the constrained name to be considered valid.
//
// Extended Key Usage values are enforced nested down a chain, so an intermediate
// or root that enumerates EKUs prevents a leaf from asserting an EKU not in that
// list. (While this is not specified, it is common practice in order to limit
// the types of certificates a CA can issue.)
//
// Certificates that use SHA1WithRSA and ECDSAWithSHA1 signatures are not supported,
// and will not be used to build chains.
//
// Certificates other than c in the returned chains should not be modified.
//
// WARNING: this function doesn't do any revocation checking.
public static (slice<slice<ж<Certificate>>> chains, error err) Verify(this ж<Certificate> Ꮡc, VerifyOptions optsʗp) {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    ref var opts = ref heap(optsʗp, out var Ꮡopts);
    // Platform-specific verification needs the ASN.1 contents so
    // this makes the behavior consistent across platforms.
    if (builtin.len(c.Raw) == 0) {
        return (default!, errNotParsed);
    }
    for (nint i = 0; i < opts.Intermediates.len(); i++) {
        var (cΔ1, _, errΔ1) = opts.Intermediates.cert(i);
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("crypto/x509: error fetching intermediate: %w"u8, errΔ1));
        }
        if (builtin.len((~cΔ1).Raw) == 0) {
            return (default!, errNotParsed);
        }
    }
    // Use platform verifiers, where available, if Roots is from SystemCertPool.
    if (runtime.GOOS == "windows"u8 || runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8) {
        // Don't use the system verifier if the system pool was replaced with a non-system pool,
        // i.e. if SetFallbackRoots was called with x509usefallbackroots=1.
        var systemPool = systemRootsPool();
        if (opts.Roots == nil && (systemPool == nil || (~systemPool).systemPool)) {
            return c.systemVerify(Ꮡopts);
        }
        if (opts.Roots != nil && (~opts.Roots).systemPool) {
            var (platformChains, errΔ2) = c.systemVerify(Ꮡopts);
            // If the platform verifier succeeded, or there are no additional
            // roots, return the platform verifier result. Otherwise, continue
            // with the Go verifier.
            if (errΔ2 == default! || opts.Roots.len() == 0) {
                return (platformChains, errΔ2);
            }
        }
    }
    if (opts.Roots == nil) {
        opts.Roots = systemRootsPool();
        if (opts.Roots == nil) {
            return (default!, new SystemRootsError(systemRootsErr));
        }
    }
    err = Ꮡc.isValid(leafCertificate, default!, Ꮡopts);
    if (err != default!) {
        return (chains, err);
    }
    if (builtin.len(opts.DNSName) > 0) {
        err = Ꮡc.VerifyHostname(opts.DNSName);
        if (err != default!) {
            return (chains, err);
        }
    }
    slice<slice<ж<Certificate>>> candidateChains = default!;
    if (opts.Roots.contains(Ꮡc)){
        candidateChains = new slice<ж<Certificate>>[]{new ж<Certificate>[]{Ꮡc}.slice()}.slice();
    } else {
        (candidateChains, err) = Ꮡc.buildChains(new ж<Certificate>[]{Ꮡc}.slice(), nil, Ꮡopts);
        if (err != default!) {
            return (default!, err);
        }
    }
    chains = new slice<slice<ж<Certificate>>>(0, builtin.len(candidateChains));
    nint invalidPoliciesChains = default!;
    foreach (var (_, candidate) in candidateChains) {
        if (!policiesValid(candidate, opts)) {
            invalidPoliciesChains++;
            continue;
        }
        chains = append(chains, candidate);
    }
    if (builtin.len(chains) == 0) {
        return (default!, new CertificateInvalidError(Ꮡc, NoValidChains, "all candidate chains have invalid policies"u8));
    }
    foreach (var (_, eku) in opts.KeyUsages) {
        if (eku == ExtKeyUsageAny) {
            // If any key usage is acceptable, no need to check the chain for
            // key usages.
            return (chains, default!);
        }
    }
    if (builtin.len(opts.KeyUsages) == 0) {
        opts.KeyUsages = new ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
    }
    candidateChains = chains;
    chains = chains[..0];
    nint incompatibleKeyUsageChains = default!;
    foreach (var (_, candidate) in candidateChains) {
        if (!checkChainForKeyUsage(candidate, opts.KeyUsages)) {
            incompatibleKeyUsageChains++;
            continue;
        }
        chains = append(chains, candidate);
    }
    if (builtin.len(chains) == 0) {
        slice<@string> details = default!;
        if (incompatibleKeyUsageChains > 0) {
            if (invalidPoliciesChains == 0) {
                return (default!, new CertificateInvalidError(Ꮡc, IncompatibleUsage, ""u8));
            }
            details = append(details, fmt.Sprintf("%d chains with incompatible key usage"u8, incompatibleKeyUsageChains));
        }
        if (invalidPoliciesChains > 0) {
            details = append(details, fmt.Sprintf("%d chains with invalid policies"u8, invalidPoliciesChains));
        }
        err = new CertificateInvalidError(Ꮡc, NoValidChains, strings.Join(details, ", "u8));
        return (default!, err);
    }
    return (chains, default!);
}

internal static slice<ж<Certificate>> appendToFreshChain(slice<ж<Certificate>> chain, ж<Certificate> Ꮡcert) {
    var n = new slice<ж<Certificate>>(builtin.len(chain) + 1);
    copy(n, chain);
    n[builtin.len(chain)] = Ꮡcert;
    return n;
}

[GoType("dyn")] internal partial interface alreadyInChain_pubKeyEqual {
    bool Equal(cryptoꓸPublicKey _);
}

// alreadyInChain checks whether a candidate certificate is present in a chain.
// Rather than doing a direct byte for byte equivalency check, we check if the
// subject, public key, and SAN, if present, are equal. This prevents loops that
// are created by mutual cross-signatures, or other cross-signature bridge
// oddities.
internal static bool alreadyInChain(ref Certificate candidate, slice<ж<Certificate>> chain) {
    ж<pkix.Extension> candidateSAN = default!;
    foreach (var (_, vᴛ1) in candidate.Extensions) {
        ref var ext = ref heap(new pkix.Extension(), out var Ꮡext);
        ext = vᴛ1;

        if (ext.Id.Equal(oidExtensionSubjectAltName)) {
            candidateSAN = Ꮡext;
            break;
        }
    }
    foreach (var (_, cert) in chain) {
        if (!bytes.Equal(candidate.RawSubject, (~cert).RawSubject)) {
            continue;
        }
        // We enforce the canonical encoding of SPKI (by only allowing the
        // correct AI paremeter encodings in parseCertificate), so it's safe to
        // directly compare the raw bytes.
        if (!bytes.Equal(candidate.RawSubjectPublicKeyInfo, (~cert).RawSubjectPublicKeyInfo)) {
            continue;
        }
        ж<pkix.Extension> certSAN = default!;
        foreach (var (_, vᴛ2) in (~cert).Extensions) {
            ref var ext = ref heap(new pkix.Extension(), out var Ꮡext);
            ext = vᴛ2;

            if (ext.Id.Equal(oidExtensionSubjectAltName)) {
                certSAN = Ꮡext;
                break;
            }
        }
        if (candidateSAN == nil && certSAN == nil){
            return true;
        } else 
        if (candidateSAN == nil || certSAN == nil) {
            return false;
        }
        if (bytes.Equal((~candidateSAN).Value, (~certSAN).Value)) {
            return true;
        }
    }
    return false;
}

// maxChainSignatureChecks is the maximum number of CheckSignatureFrom calls
// that an invocation of buildChains will (transitively) make. Most chains are
// less than 15 certificates long, so this leaves space for multiple chains and
// for failed checks due to different intermediates having the same Subject.
internal static UntypedInt maxChainSignatureChecks => 100;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x509SignatureCheckˢ = "x509: signature check attempts limit reached while verifying certificate chain"u8;

internal static (slice<slice<ж<Certificate>>> chains, error err) buildChains(this ж<Certificate> Ꮡc, slice<ж<Certificate>> currentChain, ж<nint> ᏑsigChecks, ж<VerifyOptions> Ꮡopts) {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;

    ref var opts = ref Ꮡopts.DerefOrNull();
    ref var hintErr = ref heap<error>(out var ᏑhintErr);
    ref var hintCert = ref heap<ж<Certificate>>(out var ᏑhintCert);
    var currentChainʗ1 = currentChain;
    void considerCandidate(nint certType, potentialParent candidate) {
        if ((~candidate.cert).PublicKey == default! || alreadyInChain(ref (candidate.cert).DerefOrNull(), currentChainʗ1)) {
            return;
        }
        if (ᏑsigChecks == nil) {
            ᏑsigChecks = @new<nint>();
        }
        ᏑsigChecks.Value++;
        if (ᏑsigChecks.Value > maxChainSignatureChecks) {
            err = errors.New(x509SignatureCheckˢ);
            return;
        }
        {
            var errΔ1 = Ꮡc.Value.CheckSignatureFrom(candidate.cert); if (errΔ1 != default!) {
                if (ᏑhintErr.ValueSlot == default!) {
                    ᏑhintErr.ValueSlot = errΔ1;
                    ᏑhintCert.ValueSlot = candidate.cert;
                }
                return;
            }
        }
        err = candidate.cert.isValid(certType, currentChainʗ1, Ꮡopts);
        if (err != default!) {
            if (ᏑhintErr.ValueSlot == default!) {
                ᏑhintErr.ValueSlot = err;
                ᏑhintCert.ValueSlot = candidate.cert;
            }
            return;
        }
        if (candidate.constraint != default!) {
            {
                var errΔ2 = candidate.constraint(currentChainʗ1); if (errΔ2 != default!) {
                    if (ᏑhintErr.ValueSlot == default!) {
                        ᏑhintErr.ValueSlot = errΔ2;
                        ᏑhintCert.ValueSlot = candidate.cert;
                    }
                    return;
                }
            }
        }
        var exprᴛ1 = certType;
        if (exprᴛ1 == rootCertificate) {
            chains = append(chains, appendToFreshChain(currentChainʗ1, candidate.cert));
        }
        else if (exprᴛ1 == intermediateCertificate) {
            slice<slice<ж<Certificate>>> childChains = default!;
            (childChains, err) = candidate.cert.buildChains(appendToFreshChain(currentChainʗ1, candidate.cert), ᏑsigChecks, Ꮡopts);
            chains = appendꓸꓸꓸ(chains, childChains);
        }

    }
    foreach (var (_, root) in opts.Roots.findPotentialParents(Ꮡc)) {
        considerCandidate(rootCertificate, root);
    }
    foreach (var (_, intermediate) in opts.Intermediates.findPotentialParents(Ꮡc)) {
        considerCandidate(intermediateCertificate, intermediate);
    }
    if (builtin.len(chains) > 0) {
        err = default!;
    }
    if (builtin.len(chains) == 0 && err == default!) {
        err = new UnknownAuthorityError(Ꮡc, hintErr, hintCert);
    }
    return (chains, err);
}

internal static bool validHostnamePattern(@string host) {
    return validHostname(host, true);
}

internal static bool validHostnameInput(@string host) {
    return validHostname(host, false);
}

// validHostname reports whether host is a valid hostname that can be matched or
// matched against according to RFC 6125 2.2, with some leniency to accommodate
// legacy values.
internal static bool validHostname(@string host, bool isPattern) {
    if (!isPattern) {
        host = strings.TrimSuffix(host, "."u8);
    }
    if (builtin.len(host) == 0) {
        return false;
    }
    if (host == "*"u8) {
        // Bare wildcards are not allowed, they are not valid DNS names,
        // nor are they allowed per RFC 6125.
        return false;
    }
    foreach (var (i, part) in strings.Split(host, "."u8)) {
        if (part == ""u8) {
            // Empty label.
            return false;
        }
        if (isPattern && i == 0 && part == "*"u8) {
            // Only allow full left-most wildcards, as those are the only ones
            // we match, and matching literal '*' characters is probably never
            // the expected behavior.
            continue;
        }
        foreach (var (j, c) in part) {
            if ((rune)'a' <= c && c <= (rune)'z') {
                continue;
            }
            if ((rune)'0' <= c && c <= (rune)'9') {
                continue;
            }
            if ((rune)'A' <= c && c <= (rune)'Z') {
                continue;
            }
            if (c == (rune)'-' && j != 0) {
                continue;
            }
            if (c == (rune)'_') {
                // Not a valid character in hostnames, but commonly
                // found in deployments outside the WebPKI.
                continue;
            }
            return false;
        }
    }
    return true;
}

internal static bool matchExactly(@string hostA, @string hostB) {
    if (hostA == ""u8 || hostA == "."u8 || hostB == ""u8 || hostB == "."u8) {
        return false;
    }
    return toLowerCaseASCII(hostA) == toLowerCaseASCII(hostB);
}

internal static bool matchHostnames(@string pattern, @string host) {
    pattern = toLowerCaseASCII(pattern);
    host = toLowerCaseASCII(strings.TrimSuffix(host, "."u8));
    if (builtin.len(pattern) == 0 || builtin.len(host) == 0) {
        return false;
    }
    var patternParts = strings.Split(pattern, "."u8);
    var hostParts = strings.Split(host, "."u8);
    if (builtin.len(patternParts) != builtin.len(hostParts)) {
        return false;
    }
    foreach (var (i, patternPart) in patternParts) {
        if (i == 0 && patternPart == "*"u8) {
            continue;
        }
        if (patternPart != hostParts[i]) {
            return false;
        }
    }
    return true;
}

// toLowerCaseASCII returns a lower-case version of in. See RFC 6125 6.4.1. We use
// an explicitly ASCII function to avoid any sharp corners resulting from
// performing Unicode operations on DNS labels.
internal static @string toLowerCaseASCII(@string @in) {
    // If the string is already lower-case then there's nothing to do.
    var isAlreadyLowerCase = true;
    foreach (var (_, c) in @in) {
        if (c == utf8.RuneError) {
            // If we get a UTF-8 error then there might be
            // upper-case ASCII bytes in the invalid sequence.
            isAlreadyLowerCase = false;
            break;
        }
        if ((rune)'A' <= c && c <= (rune)'Z') {
            isAlreadyLowerCase = false;
            break;
        }
    }
    if (isAlreadyLowerCase) {
        return @in;
    }
    var @out = slice<byte>(@in);
    foreach (var (i, c) in @out) {
        if ((rune)'A' <= c && c <= (rune)'Z') {
            @out[i] += (byte)((rune)'a' - (rune)'A');
        }
    }
    return ((@string)@out);
}

// VerifyHostname returns nil if c is a valid certificate for the named host.
// Otherwise it returns an error describing the mismatch.
//
// IP addresses can be optionally enclosed in square brackets and are checked
// against the IPAddresses field. Other names are checked case insensitively
// against the DNSNames field. If the names are valid hostnames, the certificate
// fields can have a wildcard as the complete left-most label (e.g. *.example.com).
//
// Note that the legacy Common Name field is ignored.
public static error VerifyHostname(this ж<Certificate> Ꮡc, @string h) {
    ref var c = ref Ꮡc.DerefOrNull();

    // IP addresses may be written in [ ].
    @string candidateIP = h;
    if (builtin.len(h) >= 3 && h[0] == (rune)'[' && h[builtin.len(h) - 1] == (rune)']') {
        candidateIP = h.slice(1, builtin.len(h) - 1);
    }
    {
        var ip = net.ParseIP(candidateIP); if (ip != default!) {
            // We only match IP addresses against IP SANs.
            // See RFC 6125, Appendix B.2.
            foreach (var (_, candidate) in c.IPAddresses) {
                if (ip.Equal(candidate)) {
                    return default!;
                }
            }
            return new HostnameError(Ꮡc, candidateIP);
        }
    }
    @string candidateName = toLowerCaseASCII(h); // Save allocations inside the loop.
    var validCandidateName = validHostnameInput(candidateName);
    foreach (var (_, match) in c.DNSNames) {
        // Ideally, we'd only match valid hostnames according to RFC 6125 like
        // browsers (more or less) do, but in practice Go is used in a wider
        // array of contexts and can't even assume DNS resolution. Instead,
        // always allow perfect matches, and only apply wildcard and trailing
        // dot processing to valid hostnames.
        if (validCandidateName && validHostnamePattern(match)){
            if (matchHostnames(match, candidateName)) {
                return default!;
            }
        } else {
            if (matchExactly(match, candidateName)) {
                return default!;
            }
        }
    }
    return new HostnameError(Ꮡc, h);
}

internal static bool checkChainForKeyUsage(slice<ж<Certificate>> chain, slice<ExtKeyUsage> keyUsages) {
    var usages = new slice<ExtKeyUsage>(builtin.len(keyUsages));
    copy(usages, keyUsages);
    if (builtin.len(chain) == 0) {
        return false;
    }
    nint usagesRemaining = builtin.len(usages);
    // We walk down the list and cross out any usages that aren't supported
    // by each certificate. If we cross out all the usages, then the chain
    // is unacceptable.
NextCert:
    for (nint i = builtin.len(chain) - 1; i >= 0; i--) {
        var cert = chain[i];
        if (builtin.len((~cert).ExtKeyUsage) == 0 && builtin.len((~cert).UnknownExtKeyUsage) == 0) {
            // The certificate doesn't have any extended key usage specified.
            continue;
        }
        foreach (var (_, usage) in (~cert).ExtKeyUsage) {
            if (usage == ExtKeyUsageAny) {
                // The certificate is explicitly good for any usage.
                goto continue_NextCert;
            }
        }
        ExtKeyUsage invalidUsage = -1;
NextRequestedUsage:
        foreach (var (iΔ1, requestedUsage) in usages) {
            if (requestedUsage == invalidUsage) {
                continue;
            }
            foreach (var (_, usage) in (~cert).ExtKeyUsage) {
                if (requestedUsage == usage) {
                    goto continue_NextRequestedUsage;
                }
            }
            usages[iΔ1] = invalidUsage;
            usagesRemaining--;
            if (usagesRemaining == 0) {
                return false;
            }
continue_NextRequestedUsage:;
        }
break_NextRequestedUsage:;
continue_NextCert:;
    }
break_NextCert:;
    return true;
}

internal static OID mustNewOIDFromInts(slice<uint64> ints) {
    var (oid, err) = OIDFromInts(ints);
    if (err != default!) {
        throw panic(fmt.Sprintf("OIDFromInts(%v) unexpected error: %v"u8, ints, err));
    }
    return oid;
}

[GoType] partial struct policyGraphNode {
    internal OID validPolicy;
    internal slice<OID> expectedPolicySet;
// we do not implement qualifiers, so we don't track qualifier_set
    internal map<ж<policyGraphNode>, bool> parents;
    internal map<ж<policyGraphNode>, bool> children;
}

internal static ж<policyGraphNode> newPolicyGraphNode(OID valid, slice<ж<policyGraphNode>> parents) {
    var n = Ꮡ(new policyGraphNode(
        validPolicy: valid,
        expectedPolicySet: new OID[]{valid}.slice(),
        children: new map<ж<policyGraphNode>, bool>{},
        parents: new map<ж<policyGraphNode>, bool>{}
    ));
    foreach (var (_, p) in parents) {
        p.Value.children[n] = true;
        n.Value.parents[p] = true;
    }
    return n;
}

[GoType] partial struct policyGraph {
    internal slice<map<@string, ж<policyGraphNode>>> strata;
    // map of OID -> nodes at strata[depth-1] with OID in their expectedPolicySet
    internal map<@string, slice<ж<policyGraphNode>>> parentIndex;
    internal nint depth;
}

internal static OID anyPolicyOID;
internal static void initᴛanyPolicyOID() { anyPolicyOID = mustNewOIDFromInts(new uint64[]{2, 5, 29, 32, 0}.slice()); }

internal static ж<policyGraph> newPolicyGraph() {
    ref var root = ref heap<policyGraphNode>(out var Ꮡroot);
    root = new policyGraphNode(
        validPolicy: anyPolicyOID,
        expectedPolicySet: new OID[]{anyPolicyOID}.slice(),
        children: new map<ж<policyGraphNode>, bool>{},
        parents: new map<ж<policyGraphNode>, bool>{}
    );
    return Ꮡ(new policyGraph(
        depth: 0,
        strata: new map<@string, ж<policyGraphNode>>[]{new map<@string, ж<policyGraphNode>>{[((@string)anyPolicyOID.der)] = Ꮡroot}}.slice()
    ));
}

[GoRecv] internal static void insert(this ref policyGraph pg, ж<policyGraphNode> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    pg.strata[pg.depth].Set(((@string)n.validPolicy.der), Ꮡn);
}

[GoRecv] internal static slice<ж<policyGraphNode>> parentsWithExpected(this ref policyGraph pg, OID expected) {
    if (pg.depth == 0) {
        return default!;
    }
    return pg.parentIndex[tmpstring(expected.der)];
}

[GoRecv] internal static ж<policyGraphNode> parentWithAnyPolicy(this ref policyGraph pg) {
    if (pg.depth == 0) {
        return default!;
    }
    return pg.strata[pg.depth - 1][tmpstring(anyPolicyOID.der)];
}

[GoRecv] internal static iter.Seq<ж<policyGraphNode>> parents(this ref policyGraph pg) {
    if (pg.depth == 0) {
        return default!;
    }
    return maps.Values<map<@string, ж<policyGraphNode>>, @string, ж<policyGraphNode>>(pg.strata[pg.depth - 1]);
}

[GoRecv] internal static map<@string, ж<policyGraphNode>> leaves(this ref policyGraph pg) {
    return pg.strata[pg.depth];
}

[GoRecv] internal static ж<policyGraphNode> leafWithPolicy(this ref policyGraph pg, OID policy) {
    return pg.strata[pg.depth][tmpstring(policy.der)];
}

[GoRecv] internal static void deleteLeaf(this ref policyGraph pg, OID policy) {
    var n = pg.strata[pg.depth][tmpstring(policy.der)];
    if (n == nil) {
        return;
    }
    foreach (var (p, _) in (~n).parents) {
        delete((~p).children, n);
    }
    foreach (var (c, _) in (~n).children) {
        delete((~c).parents, n);
    }
    delete(pg.strata[pg.depth], ((@string)policy.der));
}

[GoRecv] internal static slice<ж<policyGraphNode>> validPolicyNodes(this ref policyGraph pg) {
    slice<ж<policyGraphNode>> validNodes = default!;
    for (nint i = pg.depth; i >= 0; i--) {
        foreach (var (_, n) in pg.strata[i]) {
            if ((~n).validPolicy.Equal(anyPolicyOID)) {
                continue;
            }
            if (builtin.len((~n).parents) == 1) {
                foreach (var (p, _) in (~n).parents) {
                    if ((~p).validPolicy.Equal(anyPolicyOID)) {
                        validNodes = append(validNodes, n);
                    }
                }
            }
        }
    }
    return validNodes;
}

[GoRecv] internal static void prune(this ref policyGraph pg) {
    for (nint i = pg.depth - 1; i > 0; i--) {
        foreach (var (_, n) in pg.strata[i]) {
            if (builtin.len((~n).children) == 0) {
                foreach (var (p, _) in (~n).parents) {
                    delete((~p).children, n);
                }
                delete(pg.strata[i], ((@string)(~n).validPolicy.der));
            }
        }
    }
}

[GoRecv] internal static void incrDepth(this ref policyGraph pg) {
    pg.parentIndex = new map<@string, slice<ж<policyGraphNode>>>{};
    foreach (var (_, n) in pg.strata[pg.depth]) {
        foreach (var (_, e) in (~n).expectedPolicySet) {
            pg.parentIndex[((@string)e.der)] = append(pg.parentIndex[tmpstring(e.der)], n);
        }
    }
    pg.depth++;
    pg.strata = append(pg.strata, new map<@string, ж<policyGraphNode>>{});
}

internal static bool policiesValid(slice<ж<Certificate>> chain, VerifyOptions opts) {
    // The following code implements the policy verification algorithm as
    // specified in RFC 5280 and updated by RFC 9618. In particular the
    // following sections are replaced by RFC 9618:
    //	* 6.1.2 (a)
    //	* 6.1.3 (d)
    //	* 6.1.3 (e)
    //	* 6.1.3 (f)
    //	* 6.1.4 (b)
    //	* 6.1.5 (g)
    if (builtin.len(chain) == 1) {
        return true;
    }
    // n is the length of the chain minus the trust anchor
    nint n = builtin.len(chain) - 1;
    var pg = newPolicyGraph();
    nint inhibitAnyPolicy = default!;
    nint explicitPolicy = default!;
    nint policyMapping = default!;
    if (!opts.inhibitAnyPolicy) {
        inhibitAnyPolicy = n + 1;
    }
    if (!opts.requireExplicitPolicy) {
        explicitPolicy = n + 1;
    }
    if (!opts.inhibitPolicyMapping) {
        policyMapping = n + 1;
    }
    var initialUserPolicySet = new map<@string, bool>{};
    foreach (var (_, p) in opts.CertificatePolicies) {
        initialUserPolicySet[((@string)p.der)] = true;
    }
    // If the user does not pass any policies, we consider
    // that equivalent to passing anyPolicyOID.
    if (builtin.len(initialUserPolicySet) == 0) {
        initialUserPolicySet[((@string)anyPolicyOID.der)] = true;
    }
    for (nint i = n - 1; i >= 0; i--) {
        var cert = chain[i];
        var isSelfSigned = bytes.Equal((~cert).RawIssuer, (~cert).RawSubject);
        // 6.1.3 (e) -- as updated by RFC 9618
        if (builtin.len((~cert).Policies) == 0) {
            pg = default!;
        }
        // 6.1.3 (f) -- as updated by RFC 9618
        if (explicitPolicy == 0 && pg == nil) {
            return false;
        }
        if (pg != nil) {
            pg.incrDepth();
            var policies = new map<@string, bool>{};
            // 6.1.3 (d) (1) -- as updated by RFC 9618
            foreach (var (_, policy) in (~cert).Policies) {
                policies[((@string)policy.der)] = true;
                if (policy.Equal(anyPolicyOID)) {
                    continue;
                }
                // 6.1.3 (d) (1) (i) -- as updated by RFC 9618
                var parents = pg.parentsWithExpected(policy);
                if (builtin.len(parents) == 0) {
                    // 6.1.3 (d) (1) (ii) -- as updated by RFC 9618
                    {
                        var anyParent = pg.parentWithAnyPolicy(); if (anyParent != nil) {
                            parents = new ж<policyGraphNode>[]{anyParent}.slice();
                        }
                    }
                }
                if (builtin.len(parents) > 0) {
                    pg.insert(newPolicyGraphNode(policy, parents));
                }
            }
            // 6.1.3 (d) (2) -- as updated by RFC 9618
            // NOTE: in the check "n-i < n" our i is different from the i in the specification.
            // In the specification chains go from the trust anchor to the leaf, whereas our
            // chains go from the leaf to the trust anchor, so our i's our inverted. Our
            // check here matches the check "i < n" in the specification.
            if (policies[tmpstring(anyPolicyOID.der)] && (inhibitAnyPolicy > 0 || (n - i < n && isSelfSigned))) {
                var missing = new map<@string, slice<ж<policyGraphNode>>>{};
                var leaves = pg.leaves();
                foreach (var p in range<ж<policyGraphNode>>(pg.parents().Invoke)) {
                    foreach (var (_, expected) in (~p).expectedPolicySet) {
                        if (leaves[tmpstring(expected.der)] == nil) {
                            missing[((@string)expected.der)] = append(missing[tmpstring(expected.der)], p);
                        }
                    }
                }
                foreach (var (oidStr, parents) in missing) {
                    pg.insert(newPolicyGraphNode(new OID(der: slice<byte>(oidStr)), parents));
                }
            }
            // 6.1.3 (d) (3) -- as updated by RFC 9618
            pg.prune();
            if (i != 0) {
                // 6.1.4 (b) -- as updated by RFC 9618
                if (builtin.len((~cert).PolicyMappings) > 0) {
                    // collect map of issuer -> []subject
                    var mappings = new map<@string, slice<OID>>{};
                    foreach (var (_, mapping) in (~cert).PolicyMappings) {
                        if (policyMapping > 0){
                            if (mapping.IssuerDomainPolicy.Equal(anyPolicyOID) || mapping.SubjectDomainPolicy.Equal(anyPolicyOID)) {
                                // Invalid mapping
                                return false;
                            }
                            mappings[((@string)mapping.IssuerDomainPolicy.der)] = append(mappings[tmpstring(mapping.IssuerDomainPolicy.der)], mapping.SubjectDomainPolicy);
                        } else {
                            // 6.1.4 (b) (3) (i) -- as updated by RFC 9618
                            pg.deleteLeaf(mapping.IssuerDomainPolicy);
                            // 6.1.4 (b) (3) (ii) -- as updated by RFC 9618
                            pg.prune();
                        }
                    }
                    foreach (var (issuerStr, subjectPolicies) in mappings) {
                        // 6.1.4 (b) (1) -- as updated by RFC 9618
                        {
                            var matching = pg.leafWithPolicy(new OID(der: slice<byte>(issuerStr))); if (matching != nil){
                                matching.Value.expectedPolicySet = subjectPolicies;
                            } else 
                            {
                                var matchingΔ1 = pg.leafWithPolicy(anyPolicyOID); if (matchingΔ1 != nil) {
                                    // 6.1.4 (b) (2) -- as updated by RFC 9618
                                    var nΔ1 = newPolicyGraphNode(new OID(der: slice<byte>(issuerStr)), new ж<policyGraphNode>[]{matchingΔ1}.slice());
                                    nΔ1.Value.expectedPolicySet = subjectPolicies;
                                    pg.insert(nΔ1);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (i != 0) {
            // 6.1.4 (h)
            if (!isSelfSigned) {
                if (explicitPolicy > 0) {
                    explicitPolicy--;
                }
                if (policyMapping > 0) {
                    policyMapping--;
                }
                if (inhibitAnyPolicy > 0) {
                    inhibitAnyPolicy--;
                }
            }
            // 6.1.4 (i)
            if (((~cert).RequireExplicitPolicy > 0 || (~cert).RequireExplicitPolicyZero) && (~cert).RequireExplicitPolicy < explicitPolicy) {
                explicitPolicy = cert.Value.RequireExplicitPolicy;
            }
            if (((~cert).InhibitPolicyMapping > 0 || (~cert).InhibitPolicyMappingZero) && (~cert).InhibitPolicyMapping < policyMapping) {
                policyMapping = cert.Value.InhibitPolicyMapping;
            }
            // 6.1.4 (j)
            if (((~cert).InhibitAnyPolicy > 0 || (~cert).InhibitAnyPolicyZero) && (~cert).InhibitAnyPolicy < inhibitAnyPolicy) {
                inhibitAnyPolicy = cert.Value.InhibitAnyPolicy;
            }
        }
    }
    // 6.1.5 (a)
    if (explicitPolicy > 0) {
        explicitPolicy--;
    }
    // 6.1.5 (b)
    if ((~chain[0]).RequireExplicitPolicyZero) {
        explicitPolicy = 0;
    }
    // 6.1.5 (g) (1) -- as updated by RFC 9618
    slice<ж<policyGraphNode>> validPolicyNodeSet = default!;
    // 6.1.5 (g) (2) -- as updated by RFC 9618
    if (pg != nil) {
        validPolicyNodeSet = pg.validPolicyNodes();
        // 6.1.5 (g) (3) -- as updated by RFC 9618
        {
            var currentAny = pg.leafWithPolicy(anyPolicyOID); if (currentAny != nil) {
                validPolicyNodeSet = append(validPolicyNodeSet, currentAny);
            }
        }
    }
    // 6.1.5 (g) (4) -- as updated by RFC 9618
    var authorityConstrainedPolicySet = new map<@string, bool>{};
    foreach (var (_, nΔ2) in validPolicyNodeSet) {
        authorityConstrainedPolicySet[((@string)(~nΔ2).validPolicy.der)] = true;
    }
    // 6.1.5 (g) (5) -- as updated by RFC 9618
    var userConstrainedPolicySet = maps.Clone<map<@string, bool>, @string, bool>(authorityConstrainedPolicySet);
    // 6.1.5 (g) (6) -- as updated by RFC 9618
    if (builtin.len(initialUserPolicySet) != 1 || !initialUserPolicySet[tmpstring(anyPolicyOID.der)]) {
        // 6.1.5 (g) (6) (i) -- as updated by RFC 9618
        foreach (var (p, _) in userConstrainedPolicySet) {
            if (!initialUserPolicySet[p]) {
                delete(userConstrainedPolicySet, p);
            }
        }
        // 6.1.5 (g) (6) (ii) -- as updated by RFC 9618
        if (authorityConstrainedPolicySet[tmpstring(anyPolicyOID.der)]) {
            foreach (var (policy, _) in initialUserPolicySet) {
                userConstrainedPolicySet[policy] = true;
            }
        }
    }
    if (explicitPolicy == 0 && builtin.len(userConstrainedPolicySet) == 0) {
        return false;
    }
    return true;
}

} // end x509_package
