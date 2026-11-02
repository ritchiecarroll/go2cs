// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using pkix = go.crypto.x509.pkix_package;
using asn1 = go.encoding.asn1_package;
using hex = go.encoding.hex_package;
using pem = go.encoding.pem_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using big = go.math.big_package;
using net = net_package;
using url = go.net.url_package;
using os = os_package;
using exec = go.os.exec_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using go.crypto;
using go.crypto.x509;
using go.encoding;
using go.math;
using go.net;
using go.os;
using io = io_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

// testNameConstraintsAgainstOpenSSL can be set to true to run tests
// against the system OpenSSL. This is disabled by default because Go
// cannot depend on having OpenSSL installed at testing time.
internal const bool testNameConstraintsAgainstOpenSSL = false;

// debugOpenSSLFailure can be set to true, when
// testNameConstraintsAgainstOpenSSL is also true, to cause
// intermediate files to be preserved for debugging.
internal const bool debugOpenSSLFailure = false;

internal partial struct nameConstraintsTest {
    internal @string name;
    internal slice<constraintsSpec> roots;
    internal slice<slice<constraintsSpec>> intermediates;
    internal leafSpec leaf;
    internal slice<global::go.crypto.x509_package.ExtKeyUsage> requestedEKUs;
    internal @string expectedError;
    internal bool noOpenSSL;
    internal bool ignoreCN;
}

internal partial struct constraintsSpec {
    internal slice<@string> ok;
    internal slice<@string> bad;
    internal slice<@string> ekus;
}

internal partial struct leafSpec {
    internal slice<@string> sans;
    internal slice<@string> ekus;
    internal @string cn;
}

internal static slice<nameConstraintsTest> nameConstraintsTests = slice<nameConstraintsTest>([
    new(
        name: "certificate generation process",
        roots: new slice<constraintsSpec>(1),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "single level of intermediate",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "two levels of intermediates",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()]),
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "matching DNS constraint in root",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "matching DNS constraint in intermediate",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "leading period only matches subdomains",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        ),
        expectedError: "\"example.com\" is not permitted"
    ),
    new(
        name: "leading period matches subdomains",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.example.com"])
        )
    ),
    new(
        name: "leading period matches multiple levels of subdomains",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.bar.example.com"])
        )
    ),
    new(
        name: "specifying a permitted list of names does not exclude other name types",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:10.1.1.1"])
        )
    ),
    new(
        name: "specifying a permitted list of names does not exclude other name types",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:10.0.0.0/8"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "intermediates can try to permit other names, which isn't forbidden if the leaf doesn't mention them",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:example.com", "dns:foo.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "intermediates cannot add permitted names that the root doesn't grant them",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:foo.example.com", "dns:foo.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.com"])
        ),
        expectedError: "\"foo.com\" is not permitted"
    ),
    new(
        name: "intermediates can further limit their scope if they wish",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:.bar.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.bar.example.com"])
        )
    ),
    new(
        name: "intermediates can further limit their scope and that limitation is effective",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:.bar.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.notbar.example.com"])
        ),
        expectedError: "\"foo.notbar.example.com\" is not permitted"
    ),
    new(
        name: "roots can exclude subtrees and that doesn't affect other names",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.com"])
        )
    ),
    new(
        name: "roots exclusions are effective",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.example.com"])
        ),
        expectedError: "\"foo.example.com\" is excluded"
    ),
    new(
        name: "intermediates can also exclude names and that doesn't affect other names",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    bad: slice<@string>(["dns:.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.com"])
        )
    ),
    new(
        name: "intermediate exclusions are effective",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    bad: slice<@string>(["dns:.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.example.com"])
        ),
        expectedError: "\"foo.example.com\" is excluded"
    ),
    new(
        name: "having an exclusion doesn't prohibit other types of names",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.com", "ip:10.1.1.1"])
        )
    ),
    new(
        name: "IP-based exclusions are permitted and don't affect unrelated IP addresses",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:10.0.0.0/8"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:192.168.1.1"])
        )
    ),
    new(
        name: "IP-based exclusions are effective",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:10.0.0.0/8"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:10.0.0.1"])
        ),
        expectedError: "\"10.0.0.1\" is excluded"
    ),
    new(
        name: "intermediates can further constrain IP ranges",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:0.0.0.0/1"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    bad: slice<@string>(["ip:11.0.0.0/8"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:11.0.0.1"])
        ),
        expectedError: "\"11.0.0.1\" is excluded"
    ),
    new(
        name: "multiple intermediates with incompatible constraints",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:.foo.com"])
                ),
                new(
                    ok: slice<@string>(["dns:.example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.example.com"])
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple intermediates with incompatible constraints swapped",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:.example.com"])
                ),
                new(
                    ok: slice<@string>(["dns:.foo.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.example.com"])
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple roots with incompatible constraints",
        roots: slice<constraintsSpec>([
            new(),
            new(
                ok: slice<@string>(["dns:foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple roots with incompatible constraints swapped",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com"])
            ),
            new()
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "chain building with multiple intermediates and roots",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com"])
            ),
            new(
                ok: slice<@string>(["dns:example.com"])
            ),
            new()
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(),
                new(
                    ok: slice<@string>(["dns:foo.com"])
                )]),
            slice<constraintsSpec>([
                new(),
                new(
                    ok: slice<@string>(["dns:foo.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:bar.com"])
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "chain building fails with no valid path",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com"])
            ),
            new(
                ok: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(),
                new(
                    ok: slice<@string>(["dns:foo.com"])
                )]),
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:bar.com"])
                ),
                new(
                    ok: slice<@string>(["dns:foo.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:bar.com"])
        ),
        expectedError: "\"bar.com\" is not permitted"
    ),
    new(
        name: "unknown name types are unconstrained",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["unknown:"])
        )
    ),
    new(
        name: "unknown name types allowed in constrained chain",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com", "dns:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["unknown:"])
        )
    ),
    new(
        name: "CN is ignored in constrained chain",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com", "dns:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>([]),
            cn: "foo.com"
        )
    ),
    new(
        name: "IPv6 permitted constraint",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:abcd:1234::"])
        )
    ),
    new(
        name: "IPv6 permitted constraint is effective",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:1234:abcd::"])
        ),
        expectedError: "\"2000:1234:abcd::\" is not permitted"
    ),
    new(
        name: "IPv6 permitted constraint does not affect DNS",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:abcd::", "dns:foo.com"])
        )
    ),
    new(
        name: "IPv6 excluded constraint",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:1234::"])
        )
    ),
    new(
        name: "IPv6 excluded constraint is effective",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:abcd::"])
        ),
        expectedError: "\"2000:abcd::\" is excluded"
    ),
    new(
        name: "IPv6 constraint does not permit IPv4",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:2000:abcd::/32"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:10.0.0.1"])
        ),
        expectedError: "\"10.0.0.1\" is not permitted"
    ),
    new(
        name: "IPv4 constraint does not permit IPv6",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:10.0.0.0/8"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:2000:abcd::"])
        ),
        expectedError: "\"2000:abcd::\" is not permitted"
    ),
    new(
        name: "unknown excluded constraint does not affect other names",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["unknown:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "unknown permitted constraint does not affect other names",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["unknown:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "exact email constraint",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:foo@example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        )
    ),
    new(
        name: "exact email constraint is effective",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:foo@example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:bar@example.com"])
        ),
        expectedError: "\"bar@example.com\" is not permitted"
    ),
    new(
        name: "email canonicalization",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:foo@example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:\"\\f\\o\\o\"@example.com"])
        ),
        noOpenSSL: true // OpenSSL doesn't canonicalise email addresses before matching

    ),
    new(
        name: "email host constraint",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        )
    ),
    new(
        name: "email subdomain constraint",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@sub.example.com"])
        )
    ),
    new(
        name: "email subdomain constraint does not match parent",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        ),
        expectedError: "\"foo@example.com\" is not permitted"
    ),
    new(
        name: "email subdomain constraint matches deeper subdomains",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:.example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@sub.sub.example.com"])
        )
    ),
    new(
        name: "email local part is case-sensitive",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:foo@example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:Foo@example.com"])
        ),
        expectedError: "\"Foo@example.com\" is not permitted"
    ),
    new(
        name: "email domain part is case-insensitive",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:foo@EXAMPLE.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        )
    ),
    new(
        name: "DNS domain is case-insensitive",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:EXAMPLE.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "URI constraint covers host",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>([
                "uri:http://example.com/bar",
                "uri:http://example.com:8080/",
                "uri:https://example.com/wibble#bar"
            ])
        )
    ),
    new(
        name: "URI with IP is rejected",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://1.2.3.4/"])
        ),
        expectedError: "URI with IP"
    ),
    new(
        name: "URI with IP and port is rejected",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://1.2.3.4:43/"])
        ),
        expectedError: "URI with IP"
    ),
    new(
        name: "URI with IPv6 is rejected",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://[2006:abcd::1]/"])
        ),
        expectedError: "URI with IP"
    ),
    new(
        name: "URI with IPv6 and port is rejected",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://[2006:abcd::1]:16/"])
        ),
        expectedError: "URI with IP"
    ),
    new(
        name: "URI permitted constraint is effective",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://bar.com/"])
        ),
        expectedError: "\"http://bar.com/\" is not permitted"
    ),
    new(
        name: "URI excluded constraint is effective",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["uri:foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://foo.com/"])
        ),
        expectedError: "\"http://foo.com/\" is excluded"
    ),
    new(
        name: "URI subdomain constraint",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://www.foo.com/"])
        )
    ),
    new(
        name: "IPv4-mapped-IPv6 exclusion does not affect IPv4",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["ip:::ffff:1.2.3.4/128"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:1.2.3.4"])
        )
    ),
    new(
        name: "URI constraint not matched by URN",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:urn:example"])
        ),
        expectedError: "URI with empty host"
    ),
    new(
        name: "IPv6 exclusion does not exclude all IPv4",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["ip:1.2.3.0/24"]),
                bad: slice<@string>(["ip:::0/0"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["ip:1.2.3.4"])
        )
    ),
    new(
        name: "empty EKU in CA means any is ok",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth", "other"])
        )
    ),
    new(
        name: "any EKU means any is ok",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["any"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth", "other"])
        )
    ),
    // default.)
    new(
        name: "intermediate with enumerated EKUs",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["email"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth"])
        ),
        expectedError: "incompatible key usage"
    ),
    new(
        name: "unknown EKU in leaf",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["email"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["other"])
        ),
        requestedEKUs: slice<global::go.crypto.x509_package.ExtKeyUsage>([ExtKeyUsageAny])
    ),
    // certificate doesn't use them.
    new(
        name: "intermediate cannot add EKUs not in root if leaf uses them",
        roots: slice<constraintsSpec>([
            new(
                ekus: slice<@string>(["serverAuth"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["serverAuth", "email"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth"])
        )
    ),
    new(
        name: "EKUs in root are effective",
        roots: slice<constraintsSpec>([
            new(
                ekus: slice<@string>(["email"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["serverAuth"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth"])
        ),
        expectedError: "incompatible key usage"
    ),
    new(
        name: "netscapeSGC EKU does not permit server/client auth",
        roots: slice<constraintsSpec>([
            new()
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["netscapeSGC"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth", "clientAuth"])
        ),
        expectedError: "incompatible key usage"
    ),
    new(
        name: "msSGC EKU does not permit server/client auth",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["msSGC"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth", "clientAuth"])
        ),
        expectedError: "incompatible key usage"
    ),
    new(
        name: "empty DNS permitted constraint allows anything",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        )
    ),
    new(
        name: "empty DNS excluded constraint rejects everything",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"])
        ),
        expectedError: "\"example.com\" is excluded"
    ),
    new(
        name: "empty email permitted constraint allows anything",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        )
    ),
    new(
        name: "empty email excluded constraint rejects everything",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["email:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:foo@example.com"])
        ),
        expectedError: "\"foo@example.com\" is excluded"
    ),
    new(
        name: "empty URI permitted constraint allows anything",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:https://example.com/test"])
        )
    ),
    new(
        name: "empty URI excluded constraint rejects everything",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["uri:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:https://example.com/test"])
        ),
        expectedError: "\"https://example.com/test\" is excluded"
    ),
    new(
        name: "serverAuth EKU does not permit clientAuth",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["serverAuth"])
        ),
        requestedEKUs: slice<global::go.crypto.x509_package.ExtKeyUsage>([ExtKeyUsageClientAuth]),
        expectedError: "incompatible key usage"
    ),
    new(
        name: "msSGC EKU does not permit serverAuth",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["msSGC"])
        ),
        requestedEKUs: slice<global::go.crypto.x509_package.ExtKeyUsage>([ExtKeyUsageServerAuth]),
        expectedError: "incompatible key usage"
    ),
    new(
        // An invalid DNS SAN should be detected only at validation time so
        // that we can process CA certificates in the wild that have invalid SANs.
        // See https://github.com/golang/go/issues/23995
        name: "invalid SANs are ignored with no constraints",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:this is invalid", "email:this @ is invalid"])
        )
    ),
    new(
        name: "invalid DNS SAN detected with constraints",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["uri:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:this is invalid"])
        ),
        expectedError: "cannot parse dnsName"
    ),
    new(
        name: "invalid email SAN detected with constraints",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["uri:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:this @ is invalid"])
        ),
        expectedError: "cannot parse rfc822Name"
    ),
    new(
        name: "any requested EKU is sufficient",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            ekus: slice<@string>(["email"])
        ),
        requestedEKUs: slice<global::go.crypto.x509_package.ExtKeyUsage>([ExtKeyUsageClientAuth, ExtKeyUsageEmailProtection])
    ),
    new(
        name: "unrequested EKUs not required to be nested",
        roots: new slice<constraintsSpec>(1),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ekus: slice<@string>(["serverAuth"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:example.com"]),
            // There's no email EKU in the intermediate. This would be rejected if
            // full nesting was required.
            ekus: slice<@string>(["email", "serverAuth"])
        )
    ),
    new(
        name: "empty leaf is accepted in constrained chain",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com", "dns:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>([])
        )
    ),
    new(
        name: "no SANs and non-hostname CN is accepted in constrained chain",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com", "dns:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>([]),
            cn: "foo.bar"
        )
    ),
    new(
        name: "constraints don't apply to CN",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:foo.com", "dns:.foo.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:foo.com"]),
            cn: "foo.bar"
        )
    ),
    new(
        name: "DNS SAN cannot use leading period form",
        roots: slice<constraintsSpec>([new(ok: slice<@string>(["dns:example.com"]))]),
        leaf: new leafSpec(sans: slice<@string>(["dns:.example.com"])),
        expectedError: "cannot parse dnsName \".example.com\""
    ),
    new(
        name: "URI with IPv6 and zone is rejected",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["uri:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["uri:http://[2006:abcd::1%25.example.com]:16/"])
        ),
        expectedError: "URI with IP"
    ),
    new(
        name: "intermediate can narrow permitted dns scope",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    ok: slice<@string>(["dns:example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:test.com"])
        ),
        expectedError: "\"test.com\" is not permitted"
    ),
    new(
        name: "intermediate cannot narrow excluded dns scope",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    bad: slice<@string>(["dns:example.com"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:test.com"])
        ),
        expectedError: "\"test.com\" is excluded by constraint \"\""
    ),
    new(
        name: "intermediate can narrow excluded dns scope",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new(
                    bad: slice<@string>(["dns:"])
                )])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:test.com"])
        ),
        expectedError: "\"test.com\" is excluded by constraint \"\""
    ),
    new(
        name: "permitted dns constraint is not a prefix match",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:testexample.com"])
        ),
        expectedError: "\"testexample.com\" is not permitted"
    ),
    new(
        name: "subdomain constraint does not allow wildcard",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["dns:a.com", "dns:foo.example.com", "dns:z.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:*.example.com"])
        ),
        expectedError: "\"*.example.com\" is not permitted"
    ),
    new(
        name: "excluded dns constraint is not a prefix match",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:testexample.com"])
        )
    ),
    new(
        name: "excluded email constraint, multiple email with matching local portion",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["email:a@example.com", "email:a@test.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:a@example.com"])
        ),
        expectedError: "\"a@example.com\" is excluded by constraint \"a@example.com\""
    ),
    new(
        name: "email_case_check",
        roots: slice<constraintsSpec>([
            new(
                ok: slice<@string>(["email:a@example.com"])
            )
        ]),
        intermediates: slice<slice<constraintsSpec>>([
            slice<constraintsSpec>([
                new()])
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["email:a@ExAmple.com"])
        )
    ),
    new(
        name: "excluded constraint, empty DNS san",
        roots: slice<constraintsSpec>([
            new(
                bad: slice<@string>(["dns:example.com"])
            )
        ]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:"])
        )
    ),
    new(
        name: "subdomain exclusion blocks uppercase wildcard",
        roots: slice<constraintsSpec>([new(
            bad: slice<@string>(["dns:sub.example.com"])
        )
        ]),
        intermediates: slice<slice<constraintsSpec>>([slice<constraintsSpec>([new()])]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:*.EXAMPLE.COM"])
        ),
        expectedError: "\"*.EXAMPLE.COM\" is excluded by constraint \"sub.example.com\""
    ),
    new(
        name: "uppercase subdomain exclusion blocks lowercase wildcard",
        roots: slice<constraintsSpec>([new(
            bad: slice<@string>(["dns:SUB.EXAMPLE.COM"])
        )
        ]),
        intermediates: slice<slice<constraintsSpec>>([slice<constraintsSpec>([new()])]),
        leaf: new leafSpec(
            sans: slice<@string>(["dns:*.example.com"])
        ),
        expectedError: "\"*.example.com\" is excluded by constraint \"sub.example.com\""
    )
]);

internal static (ж<global::go.crypto.x509_package.Certificate>, error) makeConstraintsCACert(constraintsSpec constraints, @string name, ж<ecdsa.PrivateKey> key, ж<global::go.crypto.x509_package.Certificate> Ꮡparent, ж<ecdsa.PrivateKey> parentKey) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    array<byte> serialBytes = new(16);
    rand.Read(serialBytes[..]);

    var template = Ꮡ(new Certificate(
        SerialNumber: @new<bigꓸInt>().SetBytes(serialBytes[..]),
        Subject: new pkix.Name(
            CommonName: name
        ),
        NotBefore: time.Unix(1000, 0),
        NotAfter: time.Unix(2000, 0),
        KeyUsage: KeyUsageCertSign,
        BasicConstraintsValid: true,
        IsCA: true
    ));

    {
        var errΔ1 = addConstraintsToTemplate(constraints, template); if (errΔ1 != nil) {
            return (default, errΔ1);
        }
    }

    if (Ꮡparent == nil) {
        Ꮡparent = template; parent = ref Ꮡparent.DerefOrNull();
    }
    var (derBytes, err) = CreateCertificate(rand.Reader, template, Ꮡparent, key.ᏑPublicKey, parentKey.OrTypedNil());
    if (err != nil) {
        return (default, err);
    }

    (var caCert, err) = ParseCertificate(derBytes);
    if (err != nil) {
        return (default, err);
    }

    return (caCert, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dnsˢ => ref dnsˢᵇ;
private static readonly @string dnsˢᵇ = "dns:"u8;
internal static ref readonly @string invalidipˢ => ref invalidipˢᵇ;
private static readonly @string invalidipˢᵇ = "invalidip:"u8;
internal static ref readonly @string emailˢ => ref emailˢᵇ;
private static readonly @string emailˢᵇ = "email:"u8;
internal static ref readonly @string uriˢ => ref uriˢᵇ;
private static readonly @string uriˢᵇ = "uri:"u8;
internal static ref readonly @string unknownˢ => ref unknownˢᵇ;
private static readonly @string unknownˢᵇ = "unknown:"u8;

internal static (ж<global::go.crypto.x509_package.Certificate>, error) makeConstraintsLeafCert(leafSpec leaf, ж<ecdsa.PrivateKey> key, ж<global::go.crypto.x509_package.Certificate> Ꮡparent, ж<ecdsa.PrivateKey> parentKey) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    array<byte> serialBytes = new(16);
    rand.Read(serialBytes[..]);

    var template = Ꮡ(new Certificate(
        SerialNumber: @new<bigꓸInt>().SetBytes(serialBytes[..]),
        Subject: new pkix.Name(
            OrganizationalUnit: slice<@string>(["Leaf"]),
            CommonName: leaf.cn
        ),
        NotBefore: time.Unix(1000, 0),
        NotAfter: time.Unix(2000, 0),
        KeyUsage: KeyUsageDigitalSignature,
        BasicConstraintsValid: true,
        IsCA: false
    ));

    foreach (var (_, name) in leaf.sans) {
        switch (ᐧ) {
        case {} when strings.HasPrefix(name, dnsˢ): {
            template.DNSNames = append(template.DNSNames, name[4..]);
            break;
        }
        case {} when strings.HasPrefix(name, "ip:"): {
            var ip = net.ParseIP(name[3..]);
            if (ip == nil) {
                return (default, fmt.Errorf("cannot parse IP %q"u8, name[3..]));
            }
            template.IPAddresses = append(template.IPAddresses, ip);
            break;
        }
        case {} when strings.HasPrefix(name, invalidipˢ): {
            var (ipBytes, errΔ2) = hex.DecodeString(name[10..]);
            if (errΔ2 != nil) {
                return (default, fmt.Errorf("cannot parse invalid IP: %s"u8, errΔ2));
            }
            template.IPAddresses = append(template.IPAddresses, (net.IP)ipBytes);
            break;
        }
        case {} when strings.HasPrefix(name, emailˢ): {
            template.EmailAddresses = append(template.EmailAddresses, name[6..]);
            break;
        }
        case {} when strings.HasPrefix(name, uriˢ): {
            var (uri, errΔ3) = url.Parse(name[4..]);
            if (errΔ3 != nil) {
                return (default, fmt.Errorf("cannot parse URI %q: %s"u8, name[4..], errΔ3));
            }
            template.URIs = append(template.URIs, uri);
            break;
        }
        case {} when strings.HasPrefix(name, unknownˢ): {
            // This is a special case for testing unknown
            // name types. A custom SAN extension is
            // injected into the certificate.
            if (builtin.len(leaf.sans) != 1) {
                throw panic("when using unknown name types, it must be the sole name");
            }

            template.ExtraExtensions = append(template.ExtraExtensions, new pkix.Extension(
                Id: slice<nint>([2, 5, 29, 17]),
                Value: slice<byte>([
                    0x30, // SEQUENCE
                    3,    // three bytes
                    9,    // undefined GeneralName type 9
                    1,
                    1
                ])
            ));
            break;
        }
        default: {
            return (default, fmt.Errorf("unknown name type %q"u8, name));
        }}
    }

    error err = default;
    {
        (template.ExtKeyUsage, template.UnknownExtKeyUsage, err) = parseEKUs(leaf.ekus); if (err != nil) {
            return (default, err);
        }
    }

    if (Ꮡparent == nil) {
        Ꮡparent = template; parent = ref Ꮡparent.DerefOrNull();
    }

    (var derBytes, err) = CreateCertificate(rand.Reader, template, Ꮡparent, key.ᏑPublicKey, parentKey.OrTypedNil());
    if (err != nil) {
        return (default, err);
    }

    return ParseCertificate(derBytes);
}

internal static pkix.Extension customConstraintsExtension(nint typeNum, slice<byte> constraint, bool isExcluded) {
    var constraintʗ1 = constraint;
    slice<byte> appendConstraint(slice<byte> contentsΔ1, uint8 tag) {
        contentsΔ1 = append(contentsΔ1, (byte)((uint8)((uint8)(tag | 32) /* constructed */ | 0x80)) /* context-specific */);
        contentsΔ1 = append(contentsΔ1, (byte)(4 + builtin.len(constraintʗ1)) /* length */);
        contentsΔ1 = append(contentsΔ1, (byte)0x30 /* SEQUENCE */);
        contentsΔ1 = append(contentsΔ1, (byte)(2 + builtin.len(constraintʗ1)) /* length */);
        contentsΔ1 = append(contentsΔ1, (byte)typeNum /* GeneralName type */);
        contentsΔ1 = append(contentsΔ1, (byte)builtin.len(constraintʗ1));
        return appendꓸꓸꓸ(contentsΔ1, constraintʗ1);
    }

    slice<byte> contents = default;
    if (!isExcluded) {
        contents = appendConstraint(contents, 0 /* tag 0 for permitted */);
    } else {
        contents = appendConstraint(contents, 1 /* tag 1 for excluded */);
    }

    slice<byte> value = default;
    value = append(value, (byte)0x30 /* SEQUENCE */);
    value = append(value, (byte)builtin.len(contents));
    value = appendꓸꓸꓸ(value, contents);

    return new pkix.Extension(
        Id: slice<nint>([2, 5, 29, 30]),
        Value: value
    );
}

internal static error addConstraintsToTemplate(constraintsSpec constraints, ж<global::go.crypto.x509_package.Certificate> Ꮡtemplate) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emailAddrs, slice<@string> uriDomains, error err) parse(slice<@string> constraintsΔ1) {
        slice<@string> dnsNames = default;
        slice<ж<net.IPNet>> ips = default;
        slice<@string> emailAddrs = default;
        slice<@string> uriDomains = default;
        error errΔ1 = default;
        foreach (var (_, constraint) in constraintsΔ1) {
            switch (ᐧ) {
            case {} when strings.HasPrefix(constraint, dnsˢ): {
                dnsNames = append(dnsNames, constraint[4..]);
                break;
            }
            case {} when strings.HasPrefix(constraint, "ip:"): {
                var (_, ipNet, errΔ3) = net.ParseCIDR(constraint[3..]);
                if (errΔ3 != nil) {
                    return (default, default, default, default, errΔ3);
                }
                ips = append(ips, ipNet);
                break;
            }
            case {} when strings.HasPrefix(constraint, emailˢ): {
                emailAddrs = append(emailAddrs, constraint[6..]);
                break;
            }
            case {} when strings.HasPrefix(constraint, uriˢ): {
                uriDomains = append(uriDomains, constraint[4..]);
                break;
            }
            default: {
                return (default, default, default, default, fmt.Errorf("unknown constraint %q"u8, constraint));
            }}
        }

        return (dnsNames, ips, emailAddrs, uriDomains, errΔ1);
    }

    bool handleSpecialConstraint(@string constraint, bool isExcluded) {
        switch (ᐧ) {
        case {} when constraint == "unknown:"u8: {
            Ꮡtemplate.Value.ExtraExtensions = append(Ꮡtemplate.Value.ExtraExtensions, customConstraintsExtension(9, /* undefined GeneralName type */ slice<byte>([1]), isExcluded));
            break;
        }
        default: {
            return false;
        }}

        return true;
    }

    if (builtin.len(constraints.ok) == 1 && builtin.len(constraints.bad) == 0) {
        if (handleSpecialConstraint(constraints.ok[0], false)) {
            return default;
        }
    }

    if (builtin.len(constraints.bad) == 1 && builtin.len(constraints.ok) == 0) {
        if (handleSpecialConstraint(constraints.bad[0], true)) {
            return default;
        }
    }

    error err = default;
    (template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains, err) = parse(constraints.ok);
    if (err != nil) {
        return err;
    }

    (template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains, err) = parse(constraints.bad);
    if (err != nil) {
        return err;
    }

    {
        (template.ExtKeyUsage, template.UnknownExtKeyUsage, err) = parseEKUs(constraints.ekus); if (err != nil) {
            return err;
        }
    }

    return default;
}

internal static (slice<global::go.crypto.x509_package.ExtKeyUsage> ekus, slice<asn1.ObjectIdentifier> unknowns, error err) parseEKUs(slice<@string> ekuStrs) {
    slice<global::go.crypto.x509_package.ExtKeyUsage> ekus = default;
    slice<asn1.ObjectIdentifier> unknowns = default;
    error err = default;
    var unknownsᴷ = new stackAppend<asn1.ObjectIdentifier>(moves: true);

    foreach (var (_, s) in ekuStrs) {
        var exprᴛ1 = s;
        switch (ᐧ) {
        case {} when exprᴛ1 == "serverAuth"u8: {
            ekus = append(ekus, ExtKeyUsageServerAuth);
            break;
        }
        case {} when exprᴛ1 == "clientAuth"u8: {
            ekus = append(ekus, ExtKeyUsageClientAuth);
            break;
        }
        case {} when exprᴛ1 == "email"u8: {
            ekus = append(ekus, ExtKeyUsageEmailProtection);
            break;
        }
        case {} when exprᴛ1 == "netscapeSGC"u8: {
            ekus = append(ekus, ExtKeyUsageNetscapeServerGatedCrypto);
            break;
        }
        case {} when exprᴛ1 == "msSGC"u8: {
            ekus = append(ekus, ExtKeyUsageMicrosoftServerGatedCrypto);
            break;
        }
        case {} when exprᴛ1 == "any"u8: {
            ekus = append(ekus, ExtKeyUsageAny);
            break;
        }
        case {} when exprᴛ1 == "other"u8: {
            unknowns = append(unknownsᴷ, unknowns, new asn1.ObjectIdentifier(new nint[]{2, 4, 1, 2, 3}.slice()));
            break;
        }
        default: {
            return (default, default, fmt.Errorf("unknown EKU %q"u8, s));
        }}
    }

    unknowns = move2heap(unknownsᴷ, unknowns);
    return (ekus, unknowns, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedlySucceededˢ = (@string)"unexpectedly succeeded against OpenSSL"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

public static void TestConstraintCases(ж<testing.T> t) {
    ref var privateKeys = ref heap<sync.Pool>(new sync.Pool(
        New: () => {
            var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
            if (err != nil) {
                throw panic(err);
            }
            return priv.OrTypedNil();
        }
    ), out var ᏑprivateKeys);

    foreach (var (i, vᴛ1) in nameConstraintsTests) {
        ref var test = ref heap(new nameConstraintsTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var keysᴷ = new stackAppend<ж<ecdsa.PrivateKey>>();
            var rootPool = NewCertPool();
            var rootKey = ᏑprivateKeys.Get()._<ж<ecdsa.PrivateKey>>();
            @string rootName = "Root "u8 + strconv.Itoa(i);

            // keys keeps track of all the private keys used in a given
            // test and puts them back in the privateKeys pool at the end.
            var keys = slice<ж<ecdsa.PrivateKey>>([rootKey]);

            // At each level (root, intermediate(s), leaf), parent points to
            // an example parent certificate and parentKey the key for the
            // parent level. Since all certificates at a given level have
            // the same name and public key, any parent certificate is
            // sufficient to get the correct issuer name and authority
            // key ID.
            ж<global::go.crypto.x509_package.Certificate> parent = default;
            var parentKey = rootKey;

            foreach (var (_, root) in testʗ1.roots) {
                var (rootCert, errΔ1) = makeConstraintsCACert(root, rootName, rootKey, nil, rootKey);
                if (errΔ1 != nil) {
                    tΔ1.Fatalf("failed to create root: %s", errΔ1);
                }

                parent = rootCert;
                rootPool.AddCert(rootCert);
            }

            var intermediatePool = NewCertPool();

            foreach (var (level, intermediates) in testʗ1.intermediates) {
                var levelKey = ᏑprivateKeys.Get()._<ж<ecdsa.PrivateKey>>();
                keys = append(keysᴷ, keys, levelKey);
                @string levelName = "Intermediate level "u8 + strconv.Itoa(level);
                ж<global::go.crypto.x509_package.Certificate> last = default;

                foreach (var (_, intermediate) in intermediates) {
                    var (caCert, errΔ2) = makeConstraintsCACert(intermediate, levelName, levelKey, parent, parentKey);
                    if (errΔ2 != nil) {
                        tΔ1.Fatalf("failed to create %q: %s", levelName, errΔ2);
                    }

                    last = caCert;
                    intermediatePool.AddCert(caCert);
                }

                parent = last;
                parentKey = levelKey;
            }

            var leafKey = ᏑprivateKeys.Get()._<ж<ecdsa.PrivateKey>>();
            keys = append(keys, leafKey);

            var (leafCert, err) = makeConstraintsLeafCert(testʗ1.leaf, leafKey, parent, parentKey);
            if (err != nil) {
                tΔ1.Fatalf("cannot create leaf: %s", err);
            }

            // Skip tests with CommonName set because OpenSSL will try to match it
            // against name constraints, while we ignore it when it's not hostname-looking.
            if (!testʗ1.noOpenSSL && testNameConstraintsAgainstOpenSSL && testʗ1.leaf.cn == ""u8) {
                var (output, errΔ1) = testChainAgainstOpenSSL(tΔ1, leafCert, intermediatePool, rootPool);
                if (errΔ1 == nil && builtin.len(testʗ1.expectedError) > 0) {
                    tΔ1.Error(unexpectedlySucceededˢ);
                    if (debugOpenSSLFailure) {
                        return;
                    }
                }

                if (errΔ1 != nil) {
                    {
                        var (_, ok) = errΔ1._<ж<exec.ExitError>>(ᐧ); if (!ok) {
                            tΔ1.Errorf("OpenSSL failed to run: %s", errΔ1);
                        } else if (builtin.len(testʗ1.expectedError) == 0) {
                            tΔ1.Errorf("OpenSSL unexpectedly failed: %v", output);
                            if (debugOpenSSLFailure) {
                                return;
                            }
                        }
                    }
                }
            }

            var verifyOpts = new VerifyOptions(
                Roots: rootPool,
                Intermediates: intermediatePool,
                CurrentTime: time.Unix(1500, 0),
                KeyUsages: testʗ1.requestedEKUs
            );
            (_, err) = leafCert.Verify(verifyOpts);

            var logInfo = false;
            if (builtin.len(testʗ1.expectedError) == 0) {
                if (err != nil) {
                    tΔ1.Errorf("unexpected failure: %s", err);
                } else {
                    logInfo = false;
                }
            } else {
                if (err == nil) {
                    tΔ1.Error(unexpectedSuccessˢ);
                } else if (!strings.Contains(err.Error(), testʗ1.expectedError)) {
                    tΔ1.Errorf("expected error containing %q, but got: %s", testʗ1.expectedError, err);
                } else {
                    logInfo = false;
                }
            }

            if (logInfo) {
                @string certAsPEM(ж<global::go.crypto.x509_package.Certificate> cert) {
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    pem.Encode(new x509_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡ(new pem.Block(Type: "CERTIFICATE", Bytes: cert.Raw)));
                    return Ꮡbuf.String();
                }
                tΔ1.Errorf("root:\n%s", certAsPEM(rootPool.mustCert(tΔ1, 0)));
                {
                    var intermediates = allCerts(tΔ1, intermediatePool); if (builtin.len(intermediates) > 0) {
                        foreach (var (ii, intermediate) in intermediates) {
                            tΔ1.Errorf("intermediate %d:\n%s", ii, certAsPEM(intermediate));
                        }
                    }
                }
                tΔ1.Errorf("leaf:\n%s", certAsPEM(leafCert));
            }

            foreach (var (_, key) in keys) {
                ᏑprivateKeys.Put(key.OrTypedNil());
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameConstraintsTestˢ => ref nameConstraintsTestˢᵇ;
private static readonly @string nameConstraintsTestˢᵇ = "name_constraints_test"u8;

internal static ж<os.File> writePEMsToTempFile(slice<ж<global::go.crypto.x509_package.Certificate>> certs) {
    var (@file, err) = os.CreateTemp(""u8, nameConstraintsTestˢ);
    if (err != nil) {
        throw panic("cannot create tempfile");
    }

    var pemBlock = Ꮡ(new pem.Block(Type: "CERTIFICATE"));
    foreach (var (_, cert) in certs) {
        pemBlock.Bytes = cert.Raw;
        pem.Encode(new os.FileжWriter(@file), pemBlock);
    }

    return @file;
}

internal static (@string, error) testChainAgainstOpenSSL(ж<testing.T> t, ж<global::go.crypto.x509_package.Certificate> leaf, ж<global::go.crypto.x509_package.CertPool> intermediates, ж<global::go.crypto.x509_package.CertPool> roots) {
    GoFrame ᒐ = default;
    try {
        var argsᴷ = new stackAppend<@string>();

        var args = slice<@string>(["verify", "-no_check_time"]);

        var rootsFile = writePEMsToTempFile(allCerts(t, roots));
        if (debugOpenSSLFailure) {
            println((@string)"roots file:", rootsFile.Name());
        } else {
            defer(os.Remove, rootsFile.Name(), ref ᒐ);
        }
        args = append(argsᴷ, args, (@string)"-CAfile", rootsFile.Name());

        if (intermediates.len() > 0) {
            var intermediatesFile = writePEMsToTempFile(allCerts(t, intermediates));
            if (debugOpenSSLFailure) {
                println((@string)"intermediates file:", intermediatesFile.Name());
            } else {
                defer(os.Remove, intermediatesFile.Name(), ref ᒐ);
            }
            args = append(args, (@string)"-untrusted", intermediatesFile.Name());
        }

        var leafFile = writePEMsToTempFile(slice<ж<global::go.crypto.x509_package.Certificate>>([leaf]));
        if (debugOpenSSLFailure) {
            println((@string)"leaf file:", leafFile.Name());
        } else {
            defer(os.Remove, leafFile.Name(), ref ᒐ);
        }
        args = append(args, leafFile.Name());

        var cmd = testenv.Command(new x509_test_package.testing_TжTB(t), "openssl"u8, args.ꓸꓸꓸ);
        var (@out, err) = cmd.CombinedOutput();
        return ((@string)@out, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

partial struct rfc2821Testsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string localPart, domain;
}
internal static slice<rfc2821Testsᴛ1> rfc2821Tests = slice<rfc2821Testsᴛ1>([
    new("foo@example.com", "foo", "example.com"),
    new("@example.com", "", ""),
    new("\"@example.com", "", ""),
    new("\"\"@example.com", "", "example.com"),
    new("\"a\"@example.com", "a", "example.com"),
    new("\"\\a\"@example.com", "a", "example.com"),
    new("a\"@example.com", "", ""),
    new("foo..bar@example.com", "", ""),
    new(".foo.bar@example.com", "", ""),
    new("foo.bar.@example.com", "", ""),
    new("|{}?'@example.com", "|{}?'", "example.com"),

    // Examples from RFC 3696
    new("Abc\\@def@example.com", "Abc@def", "example.com"),
    new("Fred\\ Bloggs@example.com", "Fred Bloggs", "example.com"),
    new("Joe.\\\\Blow@example.com", "Joe.\\Blow", "example.com"),
    new("\"Abc@def\"@example.com", "Abc@def", "example.com"),
    new("\"Fred Bloggs\"@example.com", "Fred Bloggs", "example.com"),
    new("customer/department=shipping@example.com", "customer/department=shipping", "example.com"),
    new("$A12345@example.com", "$A12345", "example.com"),
    new("!def!xyz%abc@example.com", "!def!xyz%abc", "example.com"),
    new("_somename@example.com", "_somename", "example.com")
]);

public static void TestRFC2821Parsing(ж<testing.T> t) {
    foreach (var (i, test) in rfc2821Tests) {
        var (mailbox, ok) = parseRFC2821Mailbox(test.@in);
        var expectedFailure = builtin.len(test.localPart) == 0 && builtin.len(test.domain) == 0;

        if (ok && expectedFailure) {
            t.Errorf("#%d: %q unexpectedly parsed as (%q, %q)", i, test.@in, mailbox.local, mailbox.domain);
            continue;
        }

        if (!ok && !expectedFailure) {
            t.Errorf("#%d: unexpected failure for %q", i, test.@in);
            continue;
        }

        if (!ok) {
            continue;
        }

        if (mailbox.local != test.localPart || mailbox.domain != test.domain) {
            t.Errorf("#%d: %q parsed as (%q, %q), but wanted (%q, %q)", i, test.@in, mailbox.local, mailbox.domain, test.localPart, test.domain);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string failedToParseˢ => ref failedToParseˢᵇ;
private static readonly @string failedToParseˢᵇ = "failed to parse "u8;
internal static ref readonly @string constraintˢ => ref constraintˢᵇ;
private static readonly @string constraintˢᵇ = "constraint"u8;
internal static ref readonly @string cannotBeEncodedAsAnˢ => ref cannotBeEncodedAsAnˢᵇ;
private static readonly @string cannotBeEncodedAsAnˢᵇ = "cannot be encoded as an IA5String"u8;

internal partial struct TestBadNamesInConstraints_badNames /*dyn*/ {
    internal @string name;
    internal Func<error, bool> matcher;
}

public static void TestBadNamesInConstraints(ж<testing.T> t) {
    var constraintParseError = (error errΔ1) => {
        @string str = errΔ1.Error();
        return strings.Contains(str, failedToParseˢ) && strings.Contains(str, constraintˢ);
    };

    var encodingError = (error errΔ2) => strings.Contains(errΔ2.Error(), cannotBeEncodedAsAnˢ);

    // Bad names in constraints should not parse.
    var badNames = slice<TestBadNamesInConstraints_badNames>([
        new("dns:foo.com.", constraintParseError),
        new("email:abc@foo.com.", constraintParseError),
        new("email:foo.com.", constraintParseError),
        new("uri:example.com.", constraintParseError),
        new("uri:1.2.3.4", constraintParseError),
        new("uri:ffff::1", constraintParseError),
        new("dns:not–hyphen.com", encodingError),
        new("email:foo@not–hyphen.com", encodingError),
        new("uri:not–hyphen.com", encodingError)
    ]);

    var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != nil) {
        throw panic(err);
    }

    foreach (var (_, test) in badNames) {
        var (_, errΔ3) = makeConstraintsCACert(new constraintsSpec(
            ok: slice<@string>([test.name])
        ), "TestAbsoluteNamesInConstraints", priv, nil, priv);

        if (errΔ3 == nil) {
            t.Errorf("bad name %q unexpectedly accepted in name constraint", test.name);
            continue;
        } else {
            if (!test.matcher(errΔ3)) {
                t.Errorf("bad name %q triggered unrecognised error: %s", test.name, errΔ3);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cannotParseˢ => ref cannotParseˢᵇ;
private static readonly @string cannotParseˢᵇ = "cannot parse "u8;

public static void TestBadNamesInSANs(ж<testing.T> t) {
    // Bad names in URI and IP SANs should not parse. Bad DNS and email SANs
    // will parse and are tested in name constraint tests at the top of this
    // file.
    var badNames = slice<@string>([
        "uri:https://example.com./dsf",
        "invalidip:0102",
        "invalidip:0102030405"
    ]);

    var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != nil) {
        throw panic(err);
    }

    foreach (var (_, badName) in badNames) {
        var (_, errΔ1) = makeConstraintsLeafCert(new leafSpec(sans: slice<@string>([badName])), priv, nil, priv);

        if (errΔ1 == nil) {
            t.Errorf("bad name %q unexpectedly accepted in SAN", badName);
            continue;
        }

        {
            @string str = errΔ1.Error(); if (!strings.Contains(str, cannotParseˢ)) {
                t.Errorf("bad name %q triggered unrecognised error: %s", badName, str);
            }
        }
    }
}

} // end x509_internal_test_package
