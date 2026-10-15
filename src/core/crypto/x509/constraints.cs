// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using fmt = fmt_package;
using net = net_package;
using netip = go.net.netip_package;
using url = go.net.url_package;
using slices = slices_package;
using strings = strings_package;
using go.net;

partial class x509_package {

// This file contains the data structures and functions necessary for
// efficiently checking X.509 name constraints. The method for constraint
// checking implemented in this file is based on a technique originally
// described by davidben@google.com.
//
// The basic concept is based on the fact that constraints describe possibly
// overlapping subtrees that we need to match against. If sorted in lexicographic
// order, and then pruned, removing any subtrees that overlap with preceding
// subtrees, a simple binary search can be used to find the nearest matching
// prefix. This reduces the complexity of name constraint checking from
// quadratic to log linear complexity.
//
// A close reading of RFC 5280 may suggest that constraints could also be
// implemented as a trie (or radix tree), which would present the possibility of
// doing construction and matching in linear time, but the memory cost of
// implementing them is actually quite high, and in the worst case (where each
// node has a high number of children) can be abused to require a program to use
// significant amounts of memory. The log linear approach taken here is
// extremely cheap in terms of memory because we directly alias the already
// parsed constraints, thus avoiding the need to do significant additional
// allocations.
//
// The basic data structure is nameConstraintsSet, which implements the sorting,
// pruning, and querying of the prefix sets.
//
// In order to check IP, DNS, URI, and email constraints, we need to use two
// different techniques, one for IP addresses, which is quite simple, and one
// for DNS names, which additionally compose the portions of URIs and emails we
// care about (technically we also need some special logic for email addresses
// as well for when constraints comprise of full email addresses) which is
// slightly more complex.
//
// IP addresses use two nameConstraintsSets, one for IPv4 addresses and one for
// IPv6 addresses, with no additional logic.
//
// DNS names require some extra logic in order to handle the distinctions
// between permitted and excluded subtrees, as well as for wildcards, and the
// semantics of leading period constraints (i.e. '.example.com'). This logic is
// implemented in the dnsConstraints type.
//
// Email addresses also require some additional logic, which does not make use
// of nameConstraintsSet, to handle constraints which define full email
// addresses (i.e. 'test@example.com'). For bare domain constraints, we use the
// dnsConstraints type described above, querying the domain portion of the email
// address. For full email addresses, we also hold a map of email addresses with
// the domain portion of the email lowercased, since it is case insensitive. When
// looking up an email address in the constraint set, we first check the full
// email address map, and if we don't find anything, we check the domain portion
// of the email address against the dnsConstraints.
partial struct nameConstraintsSet<T, V>
    where V : /* net.IP | string */ IAdditionOperators<V, V, V>, IEqualityOperators<V, V, bool>, IComparisonOperators<V, V, bool>, new()
{
    internal slice<T> set;
}

// sortAndPrune sorts the constraints using the provided comparison function, and then
// prunes any constraints that are subsets of preceding constraints using the
// provided subset function.
internal static void sortAndPrune<T, V>(this ref nameConstraintsSet<T, V> nc, Func<T, T, nint> cmp, Func<T, T, bool> subset)
    where V : /* net.IP | string */ IAdditionOperators<V, V, V>, IEqualityOperators<V, V, bool>, IComparisonOperators<V, V, bool>, new()
{
    if (builtin.len(nc.set) < 2) {
        return;
    }

    slices.SortFunc(nc.set, cmp);

    if (builtin.len(nc.set) < 2) {
        return;
    }
    nint writeIndex = 1;
    for (nint readIndex = 1; readIndex < builtin.len(nc.set); readIndex++) {
        if (!subset(nc.set[writeIndex - 1], nc.set[readIndex])) {
            nc.set[writeIndex] = nc.set[readIndex];
            writeIndex++;
        }
    }
    nc.set = nc.set.slice(0, writeIndex);
}

// search does a binary search over the constraints set for the provided value
// s, using the provided comparison function cmp to find the lower bound, and
// the match function to determine if the found constraint is a prefix of s. If
// a matching constraint is found, it is returned along with true. If no
// matching constraint is found, the zero value of T and false are returned.
internal static (T lowerBound, bool exactMatch) search<T, V>(this ref nameConstraintsSet<T, V> nc, V s, Func<T, V, nint> cmp, Func<T, V, bool> match)
    where V : /* net.IP | string */ IAdditionOperators<V, V, V>, IEqualityOperators<V, V, bool>, IComparisonOperators<V, V, bool>, new()
{
    T lowerBound = GoZero<T>();

    if (builtin.len(nc.set) == 0) {
        return (lowerBound, false);
    }
    // Look for the lower bound of s in the set.
    var (i, found) = slices.BinarySearchFunc(nc.set, s, cmp);
    // If we found an exact match, return it
    if (found) {
        return (nc.set[i], true);
    }

    if (i < 0) {
        return (lowerBound, false);
    }

    T constraint = GoZero<T>();
    if (i == 0){
        constraint = nc.set[0];
    } else {
        constraint = nc.set[i - 1];
    }
    if (match(constraint, s)) {
        return (constraint, true);
    }
    return (lowerBound, false);
}

internal static bool ipNetworkSubset(ж<net.IPNet> Ꮡa, ж<net.IPNet> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (!Ꮡa.Contains(b.IP)) {
        return false;
    }
    var broadcast = new net.IP(builtin.len(b.IP));
    foreach (var (i, _) in b.IP) {
        broadcast[i] = (byte)(b.IP[i] | (((byte)(~b.Mask[i]))));
    }
    return Ꮡa.Contains(broadcast);
}

internal static nint ipNetworkCompare(ж<net.IPNet> Ꮡa, ж<net.IPNet> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    nint i = bytes.Compare(a.IP, b.IP);
    if (i != 0) {
        return i;
    }
    return bytes.Compare(a.Mask, b.Mask);
}

internal static nint ipBinarySearch(ж<net.IPNet> Ꮡconstraint, net.IP target) {
    ref var constraint = ref Ꮡconstraint.DerefOrNull();

    return bytes.Compare(constraint.IP, target);
}

internal static bool ipMatch(ж<net.IPNet> Ꮡconstraint, net.IP target) {
    return Ꮡconstraint.Contains(target);
}

partial struct ipConstraints {
// NOTE: we could store IP network prefixes as a pre-processed byte slice
// (i.e. by masking the IP) and doing the byte prefix checking using faster
// techniques, but this would require allocating new byte slices, which is
// likely significantly more expensive than just operating on the
// pre-allocated *net.IPNet and net.IP objects directly.
    internal ж<nameConstraintsSet<ж<net.IPNet>, net.IP>> ipv4;
    internal ж<nameConstraintsSet<ж<net.IPNet>, net.IP>> ipv6;
}

internal partial interface newIPNetConstraints_R0 /*dyn*/ {
    (ж<net.IPNet>, bool) query(net.IP _);
}

internal static newIPNetConstraints_R0 newIPNetConstraints(slice<ж<net.IPNet>> l) {
    if (builtin.len(l) == 0) {
        return default!;
    }
    slice<ж<net.IPNet>> ipv4 = default!;
    slice<ж<net.IPNet>> ipv6 = default!;
    foreach (var (_, n) in l) {
        if (builtin.len((~n).IP) == net.IPv4len){
            ipv4 = append(ipv4, n);
        } else {
            ipv6 = append(ipv6, n);
        }
    }
    ж<nameConstraintsSet<ж<net.IPNet>, net.IP>> v4c = default!;
    ж<nameConstraintsSet<ж<net.IPNet>, net.IP>> v6c = default!;
    if (builtin.len(ipv4) > 0) {
        v4c = Ꮡ(new nameConstraintsSet<ж<net.IPNet>, net.IP>(
            set: ipv4
        ));
        v4c.sortAndPrune(ipNetworkCompare, ipNetworkSubset);
    }
    if (builtin.len(ipv6) > 0) {
        v6c = Ꮡ(new nameConstraintsSet<ж<net.IPNet>, net.IP>(
            set: ipv6
        ));
        v6c.sortAndPrune(ipNetworkCompare, ipNetworkSubset);
    }
    return new ipConstraintsжnewIPNetConstraints_R0(Ꮡ(new ipConstraints(ipv4: v4c, ipv6: v6c)));
}

internal static (ж<net.IPNet>, bool) query(this ref ipConstraints ipc, net.IP ip) {
    ж<nameConstraintsSet<ж<net.IPNet>, net.IP>> c = default!;
    if (builtin.len(ip) == net.IPv4len){
        c = ipc.ipv4;
    } else {
        c = ipc.ipv6;
    }
    if (c == nil) {
        return (default!, false);
    }
    return c.search(ip, ipBinarySearch, ipMatch);
}

// dnsHasSuffix case-insensitively checks if DNS name b is a label suffix of DNS
// name a, meaning that example.com is not considered a suffix of
// testexample.com, but is a suffix of test.example.com.
//
// dnsHasSuffix supports the URI "leading period" constraint semantics, which
// while not explicitly defined for dNSNames in RFC 5280, are widely supported
// (see errata 5997). In particular, a constraint of ".example.com" is
// considered to only match subdomains of example.com, but not example.com
// itself.
//
// a and b must both be non-empty strings representing (mostly) valid DNS names.
internal static bool dnsHasSuffix(@string a, @string b) {
    nint lenA = builtin.len(a);
    nint lenB = builtin.len(b);
    if (lenA > lenB) {
        return false;
    }
    nint i = lenA - 1;
    nint offset = lenA - lenB;
    for (; i >= 0; i--) {
        var (ar, br) = (a[i], b[i - (offset)]);
        if (ar == br) {
            continue;
        }
        if (br < ar) {
            (ar, br) = (br, ar);
        }
        if ((rune)'A' <= ar && ar <= (rune)'Z' && br == (byte)(ar + (rune)'a' - (rune)'A')) {
            continue;
        }
        return false;
    }

    if (a[0] != (rune)'.' && lenB > lenA && b[lenB - lenA - 1] != (rune)'.') {
        return false;
    }

    return true;
}

// dnsCompareTable contains the ASCII alphabet mapped from a characters index in
// the table to its lowercased form.
internal static array<byte> dnsCompareTable = new(256);

[GoInit] internal static void init() {
    // NOTE: we don't actually need the
    // full alphabet, but calculating offsets would be more expensive than just
    // having redundant characters.
    for (nint i = 0; i < 256; i++) {
        var c = (byte)i;
        if ((rune)'A' <= c && c <= (rune)'Z') {
            // Lowercase uppercase characters A-Z.
            c += (byte)((rune)'a' - (rune)'A');
        }
        dnsCompareTable[i] = c;
    }
    // Set the period character to 0 so that we get the right sorting behavior.
    //
    // In particular, we need the period character to sort before the only
    // other valid DNS name character which isn't a-z or 0-9, the hyphen,
    // otherwise a name with a dash would be incorrectly sorted into the middle
    // of another tree.
    //
    // For example, imagine a certificate with the constraints "a.com", "a.a.com", and
    // "a-a.com". These would sort as "a.com", "a-a.com", "a.a.com", which would break
    // the pruning step since we wouldn't see that "a.a.com" is a subset of "a.com".
    // Sorting the period before the hyphen ensures that "a.a.com" sorts before "a-a.com".
    dnsCompareTable[(rune)'.'] = 0;
}

// dnsCompare is a case-insensitive reversed implementation of strings.Compare
// that operates from the end to the start of the strings. This is more
// efficient that allocating reversed version of a and b and using
// strings.Compare directly (even though it is highly optimized).
//
// NOTE: this function treats the period character ('.') as sorting above every
// other character, which is necessary for us to properly sort names into their
// correct order. This is further discussed in the init function above.
internal static nint dnsCompare(@string a, @string b) {
    nint idxA = builtin.len(a) - 1;
    nint idxB = builtin.len(b) - 1;

    while (idxA >= 0 && idxB >= 0) {
        var byteA = dnsCompareTable[a[idxA]];
        var byteB = dnsCompareTable[b[idxB]];
        if (byteA == byteB) {
            idxA--;
            idxB--;
            continue;
        }
        nint retΔ1 = 1;
        if (byteA < byteB) {
            retΔ1 = -1;
        }
        return retΔ1;
    }

    nint ret = 0;
    if (idxA < idxB){
        ret = -1;
    } else 
    if (idxB < idxA) {
        ret = 1;
    }
    return ret;
}

partial struct dnsConstraints {
    // all lets us short circuit the query logic if we see a zero length
    // constraint which permits or excludes everything.
    internal bool all;
    // permitted indicates if these constraints are for permitted or excluded
    // names.
    internal bool permitted;
    internal ж<nameConstraintsSet<@string, @string>> constraints;
    // parentConstraints contains a subset of constraints which are used for
    // wildcard SAN queries, which are constructed by removing the first label
    // from the constraints in constraints. parentConstraints is only populated
    // if permitted is false.
    internal map<@string, @string> parentConstraints;
}

internal partial interface newDNSConstraints_R0 /*dyn*/ {
    (@string, bool) query(@string _);
}

internal static newDNSConstraints_R0 newDNSConstraints(slice<@string> l, bool permitted) {
    if (builtin.len(l) == 0) {
        return default!;
    }
    foreach (var (_, n) in l) {
        if (builtin.len(n) == 0) {
            return new dnsConstraintsжnewDNSConstraints_R0(Ꮡ(new dnsConstraints(all: true)));
        }
    }
    var constraints = slices.Clone<slice<@string>, @string>(l);

    var nc = Ꮡ(new dnsConstraints(
        constraints: Ꮡ(new nameConstraintsSet<@string, @string>(
            set: constraints
        )),
        permitted: permitted
    ));

    (~nc).constraints.sortAndPrune(dnsCompare, dnsHasSuffix);

    if (!permitted) {
        var parentConstraints = new map<@string, @string>{};
        foreach (var (_, vᴛ1) in (~(~nc).constraints).set) {
            var name = vᴛ1;

            name = strings.ToLower(name);
            @string trimmedName = trimFirstLabel(name);
            if (trimmedName == ""u8) {
                continue;
            }
            parentConstraints[trimmedName] = name;
        }
        if (builtin.len(parentConstraints) > 0) {
            nc.Value.parentConstraints = parentConstraints;
        }
    }

    return new dnsConstraintsжnewDNSConstraints_R0(nc);
}

internal static (@string, bool) query(this ref dnsConstraints dnc, @string s) {
    if (dnc.all) {
        return ("", true);
    }

    var (constraint, match) = dnc.constraints.search(s, dnsCompare, dnsHasSuffix);
    if (match) {
        return (constraint, true);
    }

    if (!dnc.permitted && builtin.len(s) > 0 && s[0] == (rune)'*') {
        s = strings.ToLower(s);
        @string trimmed = trimFirstLabel(s);
        {
            var (constraintΔ1, found) = dnc.parentConstraints[trimmed, ꟷ]; if (found) {
                return (constraintΔ1, true);
            }
        }
    }
    return ("", false);
}


partial struct emailConstraints {
    internal newDNSConstraints_R0 dnsConstraints;
    // fullEmails is map of rfc2821Mailboxs that are fully specified in the
    // constraints, which we need to check for separately since they don't
    // follow the same matching rules as the domain-based constraints. The
    // domain portion of the rfc2821Mailbox has been lowercased, since the
    // domain portion is case insensitive. When checking the map for an email,
    // the domain portion of the query should also be lowercased.
    internal map<rfc2821Mailbox, EmptyStruct> fullEmails;
}

internal partial interface newEmailConstraints_R0 /*dyn*/ {
    (@string, bool) query(rfc2821Mailbox _);
}

internal static newEmailConstraints_R0 newEmailConstraints(slice<@string> l, bool permitted) {
    if (builtin.len(l) == 0) {
        return default!;
    }
    var exactMap = new map<rfc2821Mailbox, EmptyStruct>{};
    slice<@string> domains = default!;
    foreach (var (_, c) in l) {
        if (!strings.ContainsRune(c, (rune)'@')) {
            domains = append(domains, c);
            continue;
        }
        var (parsed, ok) = parseRFC2821Mailbox(c);
        if (!ok) {
            // We've already parsed these addresses in parseCertificate, and
            // treat failures as a hard failure for parsing. The only way we can
            // get a parse failure here is if the caller has mutated the
            // certificate since parsing.
            continue;
        }
        parsed.domain = strings.ToLower(parsed.domain);
        exactMap[parsed] = new EmptyStruct();
    }
    var ec = Ꮡ(new emailConstraints(
        fullEmails: exactMap
    ));
    if (builtin.len(domains) > 0) {
        ec.Value.dnsConstraints = newDNSConstraints(domains, permitted);
    }
    return new emailConstraintsжnewEmailConstraints_R0(ec);
}

internal static (@string, bool) query(this ref emailConstraints ec, rfc2821Mailbox s) {
    if (builtin.len(ec.fullEmails) > 0) {
        {
            var (_, ok) = ec.fullEmails[s, ꟷ]; if (ok) {
                return (fmt.Sprintf("%s@%s"u8, s.local, s.domain), true);
            }
        }
    }
    if (ec.dnsConstraints == default!) {
        return ("", false);
    }
    var (constraint, found) = ec.dnsConstraints.query(s.domain);
    return (constraint, found);
}

partial interface constraints_permitted /*dyn*/ {
    (T, bool) query(V _Δp0);
}


partial struct constraints<T, V> {
    internal @string constraintType;
    internal constraints_permitted permitted;
    internal constraints_permitted excluded;
}

internal static error checkConstraints<T, V, P>(constraints<T, V> c, V s, P p)
    where P : /* string | net.IP | crypto/x509.parsedURI | crypto/x509.rfc2821Mailbox */ new()
{
    if (c.permitted != default!) {
        {
            var (_, found) = c.permitted.query(s); if (!found) {
                return fmt.Errorf("%s %q is not permitted by any constraint"u8, c.constraintType, p);
            }
        }
    }
    if (c.excluded != default!) {
        {
            var (constraint, found) = c.excluded.query(s); if (found) {
                return fmt.Errorf("%s %q is excluded by constraint %q"u8, c.constraintType, p, constraint);
            }
        }
    }
    return default!;
}

partial struct chainConstraints {
    internal constraints<ж<net.IPNet>, net.IP> ip;
    internal constraints<@string, @string> dns;
    internal constraints<@string, @string> uri;
    internal constraints<@string, rfc2821Mailbox> email;
    internal nint index;
    internal ж<chainConstraints> next;
}

internal static error check(this ref chainConstraints cc, slice<@string> dns, slice<parsedURI> uris, slice<rfc2821Mailbox> emails, slice<net.IP> ips) {
    foreach (var (_, ip) in ips) {
        {
            var err = checkConstraints(cc.ip, ip, ip); if (err != default!) {
                return err;
            }
        }
    }
    foreach (var (_, d) in dns) {
        if (!domainNameValid(d, false)) {
            return fmt.Errorf("x509: cannot parse dnsName %q"u8, d);
        }
        {
            var err = checkConstraints(cc.dns, d, d); if (err != default!) {
                return err;
            }
        }
    }
    foreach (var (_, u) in uris) {
        if (!domainNameValid(u.domain, false)) {
            return fmt.Errorf("x509: internal error: URI SAN %q failed to parse"u8, u);
        }
        {
            var err = checkConstraints(cc.uri, u.domain, u); if (err != default!) {
                return err;
            }
        }
    }
    foreach (var (_, e) in emails) {
        if (!domainNameValid(e.domain, false)) {
            return fmt.Errorf("x509: cannot parse rfc822Name %q"u8, e);
        }
        {
            var err = checkConstraints(cc.email, e, e); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

internal static error checkChainConstraints(slice<ж<Certificate>> chain) {
    ж<chainConstraints> currentConstraints = default!;
    ж<chainConstraints> last = default!;
    foreach (var (iᴛ1, c) in chain) {
        ref var i = ref heap(new nint(), out var Ꮡi);
        i = iᴛ1;

        if (!c.hasNameConstraints()) {
            continue;
        }
        var cc = Ꮡ(new chainConstraints(
            ip: new constraints<ж<net.IPNet>, net.IP>("IP address"u8, newIPNetConstraints((~c).PermittedIPRanges), newIPNetConstraints((~c).ExcludedIPRanges)),
            dns: new constraints<@string, @string>("DNS name"u8, newDNSConstraints((~c).PermittedDNSDomains, true), newDNSConstraints((~c).ExcludedDNSDomains, false)),
            uri: new constraints<@string, @string>("URI"u8, newDNSConstraints((~c).PermittedURIDomains, true), newDNSConstraints((~c).ExcludedURIDomains, false)),
            email: new constraints<@string, rfc2821Mailbox>("email address"u8, newEmailConstraints((~c).PermittedEmailAddresses, true), newEmailConstraints((~c).ExcludedEmailAddresses, false)),
            index: i
        ));
        if (currentConstraints == nil){
            currentConstraints = cc;
            last = cc;
        } else 
        if (last != nil) {
            last.Value.next = cc;
            last = cc;
        }
    }
    if (currentConstraints == nil) {
        return default!;
    }

    foreach (var (i, c) in chain) {
        if (!c.hasSANExtension()) {
            continue;
        }
        if (i >= (~currentConstraints).index) {
            while ((~currentConstraints).index <= i) {
                if ((~currentConstraints).next == nil) {
                    return default!;
                }
                currentConstraints = currentConstraints.Value.next;
            }
        }

        var (uris, err) = parseURIs((~c).URIs);
        if (err != default!) {
            return err;
        }
        (var emails, err) = parseMailboxes((~c).EmailAddresses);
        if (err != default!) {
            return err;
        }

        for (var n = currentConstraints; n != nil; n = n.Value.next) {
            {
                var errΔ1 = n.check((~c).DNSNames, uris, emails, (~c).IPAddresses); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
    }

    return default!;
}

partial struct parsedURI {
    internal ж<url.URL> uri;
    internal @string domain;
}

internal static @string String(this parsedURI u) {
    return u.uri.String();
}

internal static (slice<parsedURI>, error) parseURIs(slice<ж<url.URL>> uris) {
    var parsed = new slice<parsedURI>(0, builtin.len(uris));
    foreach (var (_, uri) in uris) {
        @string host = strings.ToLower((~uri).Host);
        if (builtin.len(host) == 0) {
            return (default!, fmt.Errorf("URI with empty host (%q) cannot be matched against constraints"u8, uri.String()));
        }
        if (strings.Contains(host, ":"u8) && !strings.HasSuffix(host, "]"u8)) {
            error err = default!;
            (host, _, err) = net.SplitHostPort((~uri).Host);
            if (err != default!) {
                return (default!, fmt.Errorf("cannot parse URI host %q: %v"u8, (~uri).Host, err));
            }
        }

        // netip.ParseAddr will reject the URI IPv6 literal form "[...]", so we
        // check if _either_ the string parses as an IP, or if it is enclosed in
        // square brackets.
        {
            var (_, err) = netip.ParseAddr(host); if (err == default! || (strings.HasPrefix(host, "["u8) && strings.HasSuffix(host, "]"u8))) {
                return (default!, fmt.Errorf("URI with IP (%q) cannot be matched against constraints"u8, uri.String()));
            }
        }

        parsed = append(parsed, new parsedURI(uri, host));
    }
    return (parsed, default!);
}

internal static (slice<rfc2821Mailbox>, error) parseMailboxes(slice<@string> emails) {
    var parsed = new slice<rfc2821Mailbox>(0, builtin.len(emails));
    foreach (var (_, email) in emails) {
        var (mailbox, ok) = parseRFC2821Mailbox(email);
        if (!ok) {
            return (default!, fmt.Errorf("cannot parse rfc822Name %q"u8, email));
        }
        mailbox.domain = strings.ToLower(mailbox.domain);
        parsed = append(parsed, mailbox);
    }
    return (parsed, default!);
}

internal static @string trimFirstLabel(@string dnsName) {
    nint firstDotInd = strings.IndexByte(dnsName, (rune)'.');
    if (firstDotInd < 0) {
        // Constraint is a single label, we cannot trim it.
        return ""u8;
    }
    return dnsName.slice(firstDotInd);
}

} // end x509_package
