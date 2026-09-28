// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using bytealg = @internal.bytealg_package;
using itoa = @internal.itoa_package;
using slices = slices_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using @internal;
using vendor.golang.org.x.net.dns;

partial class net_package {

// provided by runtime
//
//go:linkname runtime_rand runtime.rand
internal static partial uint64 runtime_rand();

internal static nint randInt() {
    return (nint)(((nuint)runtime_rand() >> (int)(1))); // clear sign bit
}

internal static nint randIntn(nint n) {
    return rem(randInt(), n);
}

// reverseaddr returns the in-addr.arpa. or ip6.arpa. hostname of the IP
// address addr suitable for rDNS (PTR) record lookup or an error if it fails
// to parse the IP address.
internal static (@string arpa, error err) reverseaddr(@string addr) {
    var ip = ParseIP(addr);
    if (ip == default!) {
        return ("", new DNSErrorжerror(Ꮡ(new DNSError(Err: "unrecognized address"u8, Name: addr))));
    }
    if (ip.To4() != default!) {
        return (itoa.Uitoa((nuint)ip[15]) + "." + itoa.Uitoa((nuint)ip[14]) + "." + itoa.Uitoa((nuint)ip[13]) + "." + itoa.Uitoa((nuint)ip[12]) + ".in-addr.arpa.", default!);
    }
    // Must be IPv6
    var buf = new slice<byte>(0, len(ip) * 4 + len("ip6.arpa."));
    // Add it, in reverse, to the buffer
    for (nint i = len(ip) - 1; i >= 0; i--) {
        var v = ip[i];
        buf = append(buf, hexDigit[(byte)(v & 0xF)],
            (byte)((rune)'.'),
            hexDigit[(v >> (int)(4))],
            (byte)((rune)'.'));
    }
    // Append "ip6.arpa." and return (buf already has the final .)
    buf = append(buf, ((@string)"ip6.arpa."u8).ꓸꓸꓸ);
    return (((@string)buf), default!);
}

internal static bool equalASCIIName(dnsmessage.Name x, dnsmessage.Name y) {
    x = x.ΔClone();
    y = y.ΔClone();

    if (x.Length != y.Length) {
        return false;
    }
    for (nint i = 0; i < (nint)x.Length; i++) {
        var a = x.Data[i];
        var b = y.Data[i];
        if ((rune)'A' <= a && a <= (rune)'Z') {
            a += 0x20;
        }
        if ((rune)'A' <= b && b <= (rune)'Z') {
            b += 0x20;
        }
        if (a != b) {
            return false;
        }
    }
    return true;
}

// isDomainName checks if a string is a presentation-format domain name
// (currently restricted to hostname-compatible "preferred name" LDH labels and
// SRV-like "underscore labels"; see golang.org/issue/12421).
//
// isDomainName should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname isDomainName
internal static bool isDomainName(@string s) {
    // The root domain name is valid. See golang.org/issue/45715.
    if (s == "."u8) {
        return true;
    }
    // See RFC 1035, RFC 3696.
    // Presentation format has dots before every label except the first, and the
    // terminal empty label is optional here because we assume fully-qualified
    // (absolute) input. We must therefore reserve space for the first and last
    // labels' length octets in wire format, where they are necessary and the
    // maximum total length is 255.
    // So our _effective_ maximum is 253, but 254 is not rejected if the last
    // character is a dot.
    nint l = len(s);
    if (l == 0 || l > 254 || l == 254 && s[l - 1] != (rune)'.') {
        return false;
    }
    var last = (byte)(rune)'.';
    var nonNumeric = false; // true once we've seen a letter or hyphen
    nint partlen = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        switch (ᐧ) {
        default: {
            return false;
        }
        case {} when (rune)'a' <= c && c <= (rune)'z' || (rune)'A' <= c && c <= (rune)'Z' || c == (rune)'_': {
            nonNumeric = true;
            partlen++;
            break;
        }
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            partlen++;
            break;
        }
        case {} when c is (rune)'-': {
            if (last == (rune)'.') {
                // fine
                // Byte before dash cannot be dot.
                return false;
            }
            partlen++;
            nonNumeric = true;
            break;
        }
        case {} when c is (rune)'.': {
            if (last == (rune)'.' || last == (rune)'-') {
                // Byte before dot cannot be dot, dash.
                return false;
            }
            if (partlen > 63 || partlen == 0) {
                return false;
            }
            partlen = 0;
            break;
        }}

        last = c;
    }
    if (last == (rune)'-' || partlen > 63) {
        return false;
    }
    return nonNumeric;
}

// absDomainName returns an absolute domain name which ends with a
// trailing dot to match pure Go reverse resolver and all other lookup
// routines.
// See golang.org/issue/12189.
// But we don't want to add dots for local names from /etc/hosts.
// It's hard to tell so we settle on the heuristic that names without dots
// (like "localhost" or "myhost") do not get trailing dots, but any other
// names do.
internal static @string absDomainName(@string s) {
    if (bytealg.IndexByteString(s, (rune)'.') != -1 && s[len(s) - 1] != (rune)'.') {
        s += "."u8;
    }
    return s;
}

// An SRV represents a single DNS SRV record.
[GoType] partial struct SRV {
    public @string Target;
    public uint16 Port;
    public uint16 Priority;
    public uint16 Weight;
}

[GoType("[]ж<SRV>")] partial struct byPriorityWeight;

// shuffleByWeight shuffles SRV records by weight using the algorithm
// described in RFC 2782.
internal static void shuffleByWeight(this byPriorityWeight addrs) {
    nint sum = 0;
    foreach (var (_, addr) in addrs) {
        sum += (nint)(~addr).Weight;
    }
    while (sum > 0 && len(addrs) > 1) {
        nint s = 0;
        nint n = randIntn(sum);
        foreach (var (i, _) in addrs) {
            s += (nint)(~addrs[i]).Weight;
            if (s > n) {
                if (i > 0) {
                    (addrs[0], addrs[i]) = (addrs[i], addrs[0]);
                }
                break;
            }
        }
        sum -= (nint)(~addrs[0]).Weight;
        addrs = addrs[1..];
    }
}

// sort reorders SRV records as specified in RFC 2782.
internal static void sort(this byPriorityWeight addrs) {
    slices.SortFunc(addrs, (ж<SRV> a, ж<SRV> b) => {
        {
            nint r = cmp.Compare((~a).Priority, (~b).Priority); if (r != 0) {
                return r;
            }
        }
        return cmp.Compare((~a).Weight, (~b).Weight);
    });
    nint i = 0;
    for (nint j = 1; j < len(addrs); j++) {
        if ((~addrs[i]).Priority != (~addrs[j]).Priority) {
            addrs[(int)(i)..(int)(j)].shuffleByWeight();
            i = j;
        }
    }
    addrs[(int)(i)..].shuffleByWeight();
}

// An MX represents a single DNS MX record.
[GoType] partial struct MX {
    public @string Host;
    public uint16 Pref;
}

[GoType("[]ж<MX>")] partial struct byPref;

// sort reorders MX records as specified in RFC 5321.
internal static void sort(this byPref s) {
    foreach (var (i, _) in s) {
        nint j = randIntn(i + 1);
        (s[i], s[j]) = (s[j], s[i]);
    }
    slices.SortFunc(s, (ж<MX> a, ж<MX> b) => cmp.Compare((~a).Pref, (~b).Pref));
}

// An NS represents a single DNS NS record.
[GoType] partial struct NS {
    public @string Host;
}

} // end net_package
