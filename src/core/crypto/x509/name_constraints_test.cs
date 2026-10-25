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

internal static slice<nameConstraintsTest> nameConstraintsTests = new nameConstraintsTest[]{
    new(
        name: "certificate generation process"u8,
        roots: new slice<constraintsSpec>(1),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "single level of intermediate"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "two levels of intermediates"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice(),
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "matching DNS constraint in root"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "matching DNS constraint in intermediate"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "leading period only matches subdomains"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        ),
        expectedError: "\"example.com\" is not permitted"u8
    ),
    new(
        name: "leading period matches subdomains"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.example.com"u8}.slice()
        )
    ),
    new(
        name: "leading period matches multiple levels of subdomains"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.bar.example.com"u8}.slice()
        )
    ),
    new(
        name: "specifying a permitted list of names does not exclude other name types"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:10.1.1.1"u8}.slice()
        )
    ),
    new(
        name: "specifying a permitted list of names does not exclude other name types"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:10.0.0.0/8"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "intermediates can try to permit other names, which isn't forbidden if the leaf doesn't mention them"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:example.com"u8, "dns:foo.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "intermediates cannot add permitted names that the root doesn't grant them"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:foo.example.com"u8, "dns:foo.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.com"u8}.slice()
        ),
        expectedError: "\"foo.com\" is not permitted"u8
    ),
    new(
        name: "intermediates can further limit their scope if they wish"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:.bar.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.bar.example.com"u8}.slice()
        )
    ),
    new(
        name: "intermediates can further limit their scope and that limitation is effective"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:.bar.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.notbar.example.com"u8}.slice()
        ),
        expectedError: "\"foo.notbar.example.com\" is not permitted"u8
    ),
    new(
        name: "roots can exclude subtrees and that doesn't affect other names"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.com"u8}.slice()
        )
    ),
    new(
        name: "roots exclusions are effective"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.example.com"u8}.slice()
        ),
        expectedError: "\"foo.example.com\" is excluded"u8
    ),
    new(
        name: "intermediates can also exclude names and that doesn't affect other names"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    bad: new @string[]{"dns:.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.com"u8}.slice()
        )
    ),
    new(
        name: "intermediate exclusions are effective"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    bad: new @string[]{"dns:.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.example.com"u8}.slice()
        ),
        expectedError: "\"foo.example.com\" is excluded"u8
    ),
    new(
        name: "having an exclusion doesn't prohibit other types of names"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.com"u8, "ip:10.1.1.1"u8}.slice()
        )
    ),
    new(
        name: "IP-based exclusions are permitted and don't affect unrelated IP addresses"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:10.0.0.0/8"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:192.168.1.1"u8}.slice()
        )
    ),
    new(
        name: "IP-based exclusions are effective"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:10.0.0.0/8"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:10.0.0.1"u8}.slice()
        ),
        expectedError: "\"10.0.0.1\" is excluded"u8
    ),
    new(
        name: "intermediates can further constrain IP ranges"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:0.0.0.0/1"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    bad: new @string[]{"ip:11.0.0.0/8"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:11.0.0.1"u8}.slice()
        ),
        expectedError: "\"11.0.0.1\" is excluded"u8
    ),
    new(
        name: "multiple intermediates with incompatible constraints"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:.foo.com"u8}.slice()
                ),
                new(
                    ok: new @string[]{"dns:.example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.example.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple intermediates with incompatible constraints swapped"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:.example.com"u8}.slice()
                ),
                new(
                    ok: new @string[]{"dns:.foo.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.example.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple roots with incompatible constraints"u8,
        roots: new constraintsSpec[]{
            new(),
            new(
                ok: new @string[]{"dns:foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "multiple roots with incompatible constraints swapped"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8}.slice()
            ),
            new()
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "chain building with multiple intermediates and roots"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8}.slice()
            ),
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            ),
            new()
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(),
                new(
                    ok: new @string[]{"dns:foo.com"u8}.slice()
                )}.slice(),
            new constraintsSpec[]{
                new(),
                new(
                    ok: new @string[]{"dns:foo.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:bar.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL's chain building is not informed by constraints.

    ),
    new(
        name: "chain building fails with no valid path"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8}.slice()
            ),
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(),
                new(
                    ok: new @string[]{"dns:foo.com"u8}.slice()
                )}.slice(),
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:bar.com"u8}.slice()
                ),
                new(
                    ok: new @string[]{"dns:foo.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:bar.com"u8}.slice()
        ),
        expectedError: "\"bar.com\" is not permitted"u8
    ),
    new(
        name: "unknown name types are unconstrained"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"unknown:"u8}.slice()
        )
    ),
    new(
        name: "unknown name types allowed in constrained chain"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8, "dns:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"unknown:"u8}.slice()
        )
    ),
    new(
        name: "CN is ignored in constrained chain"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8, "dns:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{}.slice(),
            cn: "foo.com"u8
        )
    ),
    new(
        name: "IPv6 permitted constraint"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:abcd:1234::"u8}.slice()
        )
    ),
    new(
        name: "IPv6 permitted constraint is effective"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:1234:abcd::"u8}.slice()
        ),
        expectedError: "\"2000:1234:abcd::\" is not permitted"u8
    ),
    new(
        name: "IPv6 permitted constraint does not affect DNS"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:abcd::"u8, "dns:foo.com"u8}.slice()
        )
    ),
    new(
        name: "IPv6 excluded constraint"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:1234::"u8}.slice()
        )
    ),
    new(
        name: "IPv6 excluded constraint is effective"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:abcd::"u8}.slice()
        ),
        expectedError: "\"2000:abcd::\" is excluded"u8
    ),
    new(
        name: "IPv6 constraint does not permit IPv4"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:2000:abcd::/32"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:10.0.0.1"u8}.slice()
        ),
        expectedError: "\"10.0.0.1\" is not permitted"u8
    ),
    new(
        name: "IPv4 constraint does not permit IPv6"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:10.0.0.0/8"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:2000:abcd::"u8}.slice()
        ),
        expectedError: "\"2000:abcd::\" is not permitted"u8
    ),
    new(
        name: "unknown excluded constraint does not affect other names"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"unknown:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "unknown permitted constraint does not affect other names"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"unknown:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "exact email constraint"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:foo@example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        )
    ),
    new(
        name: "exact email constraint is effective"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:foo@example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:bar@example.com"u8}.slice()
        ),
        expectedError: "\"bar@example.com\" is not permitted"u8
    ),
    new(
        name: "email canonicalization"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:foo@example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:\"\\f\\o\\o\"@example.com"u8}.slice()
        ),
        noOpenSSL: true // OpenSSL doesn't canonicalise email addresses before matching

    ),
    new(
        name: "email host constraint"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        )
    ),
    new(
        name: "email subdomain constraint"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@sub.example.com"u8}.slice()
        )
    ),
    new(
        name: "email subdomain constraint does not match parent"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        ),
        expectedError: "\"foo@example.com\" is not permitted"u8
    ),
    new(
        name: "email subdomain constraint matches deeper subdomains"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:.example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@sub.sub.example.com"u8}.slice()
        )
    ),
    new(
        name: "email local part is case-sensitive"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:foo@example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:Foo@example.com"u8}.slice()
        ),
        expectedError: "\"Foo@example.com\" is not permitted"u8
    ),
    new(
        name: "email domain part is case-insensitive"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:foo@EXAMPLE.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        )
    ),
    new(
        name: "DNS domain is case-insensitive"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:EXAMPLE.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "URI constraint covers host"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{
                "uri:http://example.com/bar"u8,
                "uri:http://example.com:8080/"u8,
                "uri:https://example.com/wibble#bar"u8
            }.slice()
        )
    ),
    new(
        name: "URI with IP is rejected"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://1.2.3.4/"u8}.slice()
        ),
        expectedError: "URI with IP"u8
    ),
    new(
        name: "URI with IP and port is rejected"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://1.2.3.4:43/"u8}.slice()
        ),
        expectedError: "URI with IP"u8
    ),
    new(
        name: "URI with IPv6 is rejected"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://[2006:abcd::1]/"u8}.slice()
        ),
        expectedError: "URI with IP"u8
    ),
    new(
        name: "URI with IPv6 and port is rejected"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://[2006:abcd::1]:16/"u8}.slice()
        ),
        expectedError: "URI with IP"u8
    ),
    new(
        name: "URI permitted constraint is effective"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://bar.com/"u8}.slice()
        ),
        expectedError: "\"http://bar.com/\" is not permitted"u8
    ),
    new(
        name: "URI excluded constraint is effective"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"uri:foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://foo.com/"u8}.slice()
        ),
        expectedError: "\"http://foo.com/\" is excluded"u8
    ),
    new(
        name: "URI subdomain constraint"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://www.foo.com/"u8}.slice()
        )
    ),
    new(
        name: "IPv4-mapped-IPv6 exclusion does not affect IPv4"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"ip:::ffff:1.2.3.4/128"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:1.2.3.4"u8}.slice()
        )
    ),
    new(
        name: "URI constraint not matched by URN"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:urn:example"u8}.slice()
        ),
        expectedError: "URI with empty host"u8
    ),
    new(
        name: "IPv6 exclusion does not exclude all IPv4"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"ip:1.2.3.0/24"u8}.slice(),
                bad: new @string[]{"ip:::0/0"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"ip:1.2.3.4"u8}.slice()
        )
    ),
    new(
        name: "empty EKU in CA means any is ok"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8, "other"u8}.slice()
        )
    ),
    new(
        name: "any EKU means any is ok"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"any"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8, "other"u8}.slice()
        )
    ),
    // default.)
    new(
        name: "intermediate with enumerated EKUs"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"email"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8}.slice()
        ),
        expectedError: "incompatible key usage"u8
    ),
    new(
        name: "unknown EKU in leaf"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"email"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"other"u8}.slice()
        ),
        requestedEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageAny}.slice()
    ),
    // certificate doesn't use them.
    new(
        name: "intermediate cannot add EKUs not in root if leaf uses them"u8,
        roots: new constraintsSpec[]{
            new(
                ekus: new @string[]{"serverAuth"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"serverAuth"u8, "email"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8}.slice()
        )
    ),
    new(
        name: "EKUs in root are effective"u8,
        roots: new constraintsSpec[]{
            new(
                ekus: new @string[]{"email"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"serverAuth"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8}.slice()
        ),
        expectedError: "incompatible key usage"u8
    ),
    new(
        name: "netscapeSGC EKU does not permit server/client auth"u8,
        roots: new constraintsSpec[]{
            new()
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"netscapeSGC"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8, "clientAuth"u8}.slice()
        ),
        expectedError: "incompatible key usage"u8
    ),
    new(
        name: "msSGC EKU does not permit server/client auth"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"msSGC"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8, "clientAuth"u8}.slice()
        ),
        expectedError: "incompatible key usage"u8
    ),
    new(
        name: "empty DNS permitted constraint allows anything"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        )
    ),
    new(
        name: "empty DNS excluded constraint rejects everything"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice()
        ),
        expectedError: "\"example.com\" is excluded"u8
    ),
    new(
        name: "empty email permitted constraint allows anything"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        )
    ),
    new(
        name: "empty email excluded constraint rejects everything"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"email:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:foo@example.com"u8}.slice()
        ),
        expectedError: "\"foo@example.com\" is excluded"u8
    ),
    new(
        name: "empty URI permitted constraint allows anything"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:https://example.com/test"u8}.slice()
        )
    ),
    new(
        name: "empty URI excluded constraint rejects everything"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"uri:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:https://example.com/test"u8}.slice()
        ),
        expectedError: "\"https://example.com/test\" is excluded"u8
    ),
    new(
        name: "serverAuth EKU does not permit clientAuth"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"serverAuth"u8}.slice()
        ),
        requestedEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth}.slice(),
        expectedError: "incompatible key usage"u8
    ),
    new(
        name: "msSGC EKU does not permit serverAuth"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"msSGC"u8}.slice()
        ),
        requestedEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice(),
        expectedError: "incompatible key usage"u8
    ),
    new(
        // An invalid DNS SAN should be detected only at validation time so
        // that we can process CA certificates in the wild that have invalid SANs.
        // See https://github.com/golang/go/issues/23995
        name: "invalid SANs are ignored with no constraints"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:this is invalid"u8, "email:this @ is invalid"u8}.slice()
        )
    ),
    new(
        name: "invalid DNS SAN detected with constraints"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"uri:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:this is invalid"u8}.slice()
        ),
        expectedError: "cannot parse dnsName"u8
    ),
    new(
        name: "invalid email SAN detected with constraints"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"uri:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:this @ is invalid"u8}.slice()
        ),
        expectedError: "cannot parse rfc822Name"u8
    ),
    new(
        name: "any requested EKU is sufficient"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            ekus: new @string[]{"email"u8}.slice()
        ),
        requestedEKUs: new global::go.crypto.x509_package.ExtKeyUsage[]{ExtKeyUsageClientAuth, ExtKeyUsageEmailProtection}.slice()
    ),
    new(
        name: "unrequested EKUs not required to be nested"u8,
        roots: new slice<constraintsSpec>(1),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ekus: new @string[]{"serverAuth"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:example.com"u8}.slice(),
            // There's no email EKU in the intermediate. This would be rejected if
            // full nesting was required.
            ekus: new @string[]{"email"u8, "serverAuth"u8}.slice()
        )
    ),
    new(
        name: "empty leaf is accepted in constrained chain"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8, "dns:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{}.slice()
        )
    ),
    new(
        name: "no SANs and non-hostname CN is accepted in constrained chain"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8, "dns:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{}.slice(),
            cn: "foo.bar"u8
        )
    ),
    new(
        name: "constraints don't apply to CN"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:foo.com"u8, "dns:.foo.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:foo.com"u8}.slice(),
            cn: "foo.bar"u8
        )
    ),
    new(
        name: "DNS SAN cannot use leading period form"u8,
        roots: new constraintsSpec[]{new(ok: new @string[]{"dns:example.com"u8}.slice())}.slice(),
        leaf: new leafSpec(sans: new @string[]{"dns:.example.com"u8}.slice()),
        expectedError: "cannot parse dnsName \".example.com\""u8
    ),
    new(
        name: "URI with IPv6 and zone is rejected"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"uri:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"uri:http://[2006:abcd::1%25.example.com]:16/"u8}.slice()
        ),
        expectedError: "URI with IP"u8
    ),
    new(
        name: "intermediate can narrow permitted dns scope"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    ok: new @string[]{"dns:example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:test.com"u8}.slice()
        ),
        expectedError: "\"test.com\" is not permitted"u8
    ),
    new(
        name: "intermediate cannot narrow excluded dns scope"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    bad: new @string[]{"dns:example.com"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:test.com"u8}.slice()
        ),
        expectedError: "\"test.com\" is excluded by constraint \"\""u8
    ),
    new(
        name: "intermediate can narrow excluded dns scope"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new(
                    bad: new @string[]{"dns:"u8}.slice()
                )}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:test.com"u8}.slice()
        ),
        expectedError: "\"test.com\" is excluded by constraint \"\""u8
    ),
    new(
        name: "permitted dns constraint is not a prefix match"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:testexample.com"u8}.slice()
        ),
        expectedError: "\"testexample.com\" is not permitted"u8
    ),
    new(
        name: "subdomain constraint does not allow wildcard"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"dns:a.com"u8, "dns:foo.example.com"u8, "dns:z.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:*.example.com"u8}.slice()
        ),
        expectedError: "\"*.example.com\" is not permitted"u8
    ),
    new(
        name: "excluded dns constraint is not a prefix match"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:testexample.com"u8}.slice()
        )
    ),
    new(
        name: "excluded email constraint, multiple email with matching local portion"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"email:a@example.com"u8, "email:a@test.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:a@example.com"u8}.slice()
        ),
        expectedError: "\"a@example.com\" is excluded by constraint \"a@example.com\""u8
    ),
    new(
        name: "email_case_check"u8,
        roots: new constraintsSpec[]{
            new(
                ok: new @string[]{"email:a@example.com"u8}.slice()
            )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{
            new constraintsSpec[]{
                new()}.slice()
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"email:a@ExAmple.com"u8}.slice()
        )
    ),
    new(
        name: "excluded constraint, empty DNS san"u8,
        roots: new constraintsSpec[]{
            new(
                bad: new @string[]{"dns:example.com"u8}.slice()
            )
        }.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:"u8}.slice()
        )
    ),
    new(
        name: "subdomain exclusion blocks uppercase wildcard"u8,
        roots: new constraintsSpec[]{new(
            bad: new @string[]{"dns:sub.example.com"u8}.slice()
        )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{new constraintsSpec[]{new()}.slice()}.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:*.EXAMPLE.COM"u8}.slice()
        ),
        expectedError: "\"*.EXAMPLE.COM\" is excluded by constraint \"sub.example.com\""u8
    ),
    new(
        name: "uppercase subdomain exclusion blocks lowercase wildcard"u8,
        roots: new constraintsSpec[]{new(
            bad: new @string[]{"dns:SUB.EXAMPLE.COM"u8}.slice()
        )
        }.slice(),
        intermediates: new slice<constraintsSpec>[]{new constraintsSpec[]{new()}.slice()}.slice(),
        leaf: new leafSpec(
            sans: new @string[]{"dns:*.example.com"u8}.slice()
        ),
        expectedError: "\"*.example.com\" is excluded by constraint \"sub.example.com\""u8
    )
}.slice();

internal static (ж<global::go.crypto.x509_package.Certificate>, error) makeConstraintsCACert(constraintsSpec constraints, @string name, ж<ecdsa.PrivateKey> Ꮡkey, ж<global::go.crypto.x509_package.Certificate> Ꮡparent, ж<ecdsa.PrivateKey> ᏑparentKey) {
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
        var errΔ1 = addConstraintsToTemplate(constraints, template); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }

    if (Ꮡparent == nil) {
        Ꮡparent = template; parent = ref Ꮡparent.DerefOrNull();
    }
    var (derBytes, err) = CreateCertificate(rand.Reader, template, Ꮡparent, Ꮡkey.of(ecdsa.PrivateKey.ᏑPublicKey), ᏑparentKey.OrTypedNil());
    if (err != default!) {
        return (default!, err);
    }

    (var caCert, err) = ParseCertificate(derBytes);
    if (err != default!) {
        return (default!, err);
    }

    return (caCert, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dnsˢ = "dns:"u8;
internal static readonly @string invalidipˢ = "invalidip:"u8;
internal static readonly @string emailˢ = "email:"u8;
internal static readonly @string uriˢ = "uri:"u8;
internal static readonly @string unknownˢ = "unknown:"u8;

internal static (ж<global::go.crypto.x509_package.Certificate>, error) makeConstraintsLeafCert(leafSpec leaf, ж<ecdsa.PrivateKey> Ꮡkey, ж<global::go.crypto.x509_package.Certificate> Ꮡparent, ж<ecdsa.PrivateKey> ᏑparentKey) {
    ref var parent = ref Ꮡparent.DerefOrNull();

    array<byte> serialBytes = new(16);
    rand.Read(serialBytes[..]);

    var template = Ꮡ(new Certificate(
        SerialNumber: @new<bigꓸInt>().SetBytes(serialBytes[..]),
        Subject: new pkix.Name(
            OrganizationalUnit: new @string[]{"Leaf"u8}.slice(),
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
            template.Value.DNSNames = append((~template).DNSNames, name[4..]);
            break;
        }
        case {} when strings.HasPrefix(name, "ip:"u8): {
            var ip = net.ParseIP(name[3..]);
            if (ip == default!) {
                return (default!, fmt.Errorf("cannot parse IP %q"u8, name[3..]));
            }
            template.Value.IPAddresses = append((~template).IPAddresses, ip);
            break;
        }
        case {} when strings.HasPrefix(name, invalidipˢ): {
            var (ipBytes, errΔ2) = hex.DecodeString(name[10..]);
            if (errΔ2 != default!) {
                return (default!, fmt.Errorf("cannot parse invalid IP: %s"u8, errΔ2));
            }
            template.Value.IPAddresses = append((~template).IPAddresses, (net.IP)ipBytes);
            break;
        }
        case {} when strings.HasPrefix(name, emailˢ): {
            template.Value.EmailAddresses = append((~template).EmailAddresses, name[6..]);
            break;
        }
        case {} when strings.HasPrefix(name, uriˢ): {
            var (uri, errΔ3) = url.Parse(name[4..]);
            if (errΔ3 != default!) {
                return (default!, fmt.Errorf("cannot parse URI %q: %s"u8, name[4..], errΔ3));
            }
            template.Value.URIs = append((~template).URIs, uri);
            break;
        }
        case {} when strings.HasPrefix(name, unknownˢ): {
            // This is a special case for testing unknown
            // name types. A custom SAN extension is
            // injected into the certificate.
            if (builtin.len(leaf.sans) != 1) {
                throw panic("when using unknown name types, it must be the sole name");
            }

            template.Value.ExtraExtensions = append((~template).ExtraExtensions, new pkix.Extension(
                Id: new nint[]{2, 5, 29, 17}.slice(),
                Value: new byte[]{
                    0x30, // SEQUENCE
                    3, // three bytes
                    9, // undefined GeneralName type 9
                    1,
                    1
                }.slice()
            ));
            break;
        }
        default: {
            return (default!, fmt.Errorf("unknown name type %q"u8, name));
        }}
    }

    error err = default!;
    {
        (template.Value.ExtKeyUsage, template.Value.UnknownExtKeyUsage, err) = parseEKUs(leaf.ekus); if (err != default!) {
            return (default!, err);
        }
    }

    if (Ꮡparent == nil) {
        Ꮡparent = template; parent = ref Ꮡparent.DerefOrNull();
    }

    (var derBytes, err) = CreateCertificate(rand.Reader, template, Ꮡparent, Ꮡkey.of(ecdsa.PrivateKey.ᏑPublicKey), ᏑparentKey.OrTypedNil());
    if (err != default!) {
        return (default!, err);
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

    slice<byte> contents = default!;
    if (!isExcluded){
        contents = appendConstraint(contents, 0 /* tag 0 for permitted */);
    } else {
        contents = appendConstraint(contents, 1 /* tag 1 for excluded */);
    }

    slice<byte> value = default!;
    value = append(value, (byte)0x30 /* SEQUENCE */);
    value = append(value, (byte)builtin.len(contents));
    value = appendꓸꓸꓸ(value, contents);

    return new pkix.Extension(
        Id: new nint[]{2, 5, 29, 30}.slice(),
        Value: value
    );
}

internal static error addConstraintsToTemplate(constraintsSpec constraints, ж<global::go.crypto.x509_package.Certificate> Ꮡtemplate) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emailAddrs, slice<@string> uriDomains, error err) parse(slice<@string> constraintsΔ1) {
        slice<@string> dnsNames = default!;
        slice<ж<net.IPNet>> ips = default!;
        slice<@string> emailAddrs = default!;
        slice<@string> uriDomains = default!;
        error errΔ1 = default!;
        foreach (var (_, constraint) in constraintsΔ1) {
            switch (ᐧ) {
            case {} when strings.HasPrefix(constraint, dnsˢ): {
                dnsNames = append(dnsNames, constraint[4..]);
                break;
            }
            case {} when strings.HasPrefix(constraint, "ip:"u8): {
                var (_, ipNet, errΔ3) = net.ParseCIDR(constraint[3..]);
                if (errΔ3 != default!) {
                    return (default!, default!, default!, default!, errΔ3);
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
                return (default!, default!, default!, default!, fmt.Errorf("unknown constraint %q"u8, constraint));
            }}
        }

        return (dnsNames, ips, emailAddrs, uriDomains, errΔ1);
    }

    bool handleSpecialConstraint(@string constraint, bool isExcluded) {
        switch (ᐧ) {
        case {} when constraint == "unknown:"u8: {
            Ꮡtemplate.Value.ExtraExtensions = append(Ꮡtemplate.Value.ExtraExtensions, customConstraintsExtension(9, /* undefined GeneralName type */ new byte[]{1}.slice(), isExcluded));
            break;
        }
        default: {
            return false;
        }}

        return true;
    }

    if (builtin.len(constraints.ok) == 1 && builtin.len(constraints.bad) == 0) {
        if (handleSpecialConstraint(constraints.ok[0], false)) {
            return default!;
        }
    }

    if (builtin.len(constraints.bad) == 1 && builtin.len(constraints.ok) == 0) {
        if (handleSpecialConstraint(constraints.bad[0], true)) {
            return default!;
        }
    }

    error err = default!;
    (template.PermittedDNSDomains, template.PermittedIPRanges, template.PermittedEmailAddresses, template.PermittedURIDomains, err) = parse(constraints.ok);
    if (err != default!) {
        return err;
    }

    (template.ExcludedDNSDomains, template.ExcludedIPRanges, template.ExcludedEmailAddresses, template.ExcludedURIDomains, err) = parse(constraints.bad);
    if (err != default!) {
        return err;
    }

    {
        (template.ExtKeyUsage, template.UnknownExtKeyUsage, err) = parseEKUs(constraints.ekus); if (err != default!) {
            return err;
        }
    }

    return default!;
}

internal static (slice<global::go.crypto.x509_package.ExtKeyUsage> ekus, slice<asn1.ObjectIdentifier> unknowns, error err) parseEKUs(slice<@string> ekuStrs) {
    slice<global::go.crypto.x509_package.ExtKeyUsage> ekus = default!;
    slice<asn1.ObjectIdentifier> unknowns = default!;
    error err = default!;
    var unknownsᴷ = new stackAppend<asn1.ObjectIdentifier>(moves: true);

    foreach (var (_, s) in ekuStrs) {
        var exprᴛ1 = s;
        if (exprᴛ1 == "serverAuth"u8) {
            ekus = append(ekus, ExtKeyUsageServerAuth);
        }
        else if (exprᴛ1 == "clientAuth"u8) {
            ekus = append(ekus, ExtKeyUsageClientAuth);
        }
        else if (exprᴛ1 == "email"u8) {
            ekus = append(ekus, ExtKeyUsageEmailProtection);
        }
        else if (exprᴛ1 == "netscapeSGC"u8) {
            ekus = append(ekus, ExtKeyUsageNetscapeServerGatedCrypto);
        }
        else if (exprᴛ1 == "msSGC"u8) {
            ekus = append(ekus, ExtKeyUsageMicrosoftServerGatedCrypto);
        }
        else if (exprᴛ1 == "any"u8) {
            ekus = append(ekus, ExtKeyUsageAny);
        }
        else if (exprᴛ1 == "other"u8) {
            unknowns = append(unknownsᴷ, unknowns, new asn1.ObjectIdentifier(new nint[]{2, 4, 1, 2, 3}.slice()));
        }
        else { /* default: */
            return (default!, default!, fmt.Errorf("unknown EKU %q"u8, s));
        }
    }

    unknowns = move2heap(unknownsᴷ, unknowns);
    return (ekus, unknowns, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedlySucceededˢ = (@string)"unexpectedly succeeded against OpenSSL"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

public static void TestConstraintCases(ж<testing.T> Ꮡt) {
    ref var privateKeys = ref heap<sync.Pool>(out var ᏑprivateKeys);
    privateKeys = new sync.Pool(
        New: () => {
            var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
            if (err != default!) {
                throw panic(err);
            }
            return priv.OrTypedNil();
        }
    );

    foreach (var (i, vᴛ1) in nameConstraintsTests) {
        ref var test = ref heap(new nameConstraintsTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var keysᴷ = new stackAppend<ж<ecdsa.PrivateKey>>();
            var rootPool = NewCertPool();
            var rootKey = ᏑprivateKeys.Get()._<ж<ecdsa.PrivateKey>>();
            @string rootName = "Root "u8 + strconv.Itoa(i);

            // keys keeps track of all the private keys used in a given
            // test and puts them back in the privateKeys pool at the end.
            var keys = new ж<ecdsa.PrivateKey>[]{rootKey}.slice();

            // At each level (root, intermediate(s), leaf), parent points to
            // an example parent certificate and parentKey the key for the
            // parent level. Since all certificates at a given level have
            // the same name and public key, any parent certificate is
            // sufficient to get the correct issuer name and authority
            // key ID.
            ж<global::go.crypto.x509_package.Certificate> parent = default!;
            var parentKey = rootKey;

            foreach (var (_, root) in testʗ1.roots) {
                var (rootCert, errΔ1) = makeConstraintsCACert(root, rootName, rootKey, nil, rootKey);
                if (errΔ1 != default!) {
                    tΔ1.Fatalf("failed to create root: %s"u8, errΔ1);
                }

                parent = rootCert;
                rootPool.AddCert(rootCert);
            }

            var intermediatePool = NewCertPool();

            foreach (var (level, intermediates) in testʗ1.intermediates) {
                var levelKey = ᏑprivateKeys.Get()._<ж<ecdsa.PrivateKey>>();
                keys = append(keysᴷ, keys, levelKey);
                @string levelName = "Intermediate level "u8 + strconv.Itoa(level);
                ж<global::go.crypto.x509_package.Certificate> last = default!;

                foreach (var (_, intermediate) in intermediates) {
                    var (caCert, errΔ2) = makeConstraintsCACert(intermediate, levelName, levelKey, parent, parentKey);
                    if (errΔ2 != default!) {
                        tΔ1.Fatalf("failed to create %q: %s"u8, levelName, errΔ2);
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
            if (err != default!) {
                tΔ1.Fatalf("cannot create leaf: %s"u8, err);
            }

            // Skip tests with CommonName set because OpenSSL will try to match it
            // against name constraints, while we ignore it when it's not hostname-looking.
            if (!testʗ1.noOpenSSL && testNameConstraintsAgainstOpenSSL && testʗ1.leaf.cn == ""u8) {
                var (output, errΔ1) = testChainAgainstOpenSSL(tΔ1, leafCert, intermediatePool, rootPool);
                if (errΔ1 == default! && builtin.len(testʗ1.expectedError) > 0) {
                    tΔ1.Error(unexpectedlySucceededˢ);
                    if (debugOpenSSLFailure) {
                        return;
                    }
                }

                if (errΔ1 != default!) {
                    {
                        var (_, ok) = errΔ1._<ж<exec.ExitError>>(ᐧ); if (!ok){
                            tΔ1.Errorf("OpenSSL failed to run: %s"u8, errΔ1);
                        } else 
                        if (builtin.len(testʗ1.expectedError) == 0) {
                            tΔ1.Errorf("OpenSSL unexpectedly failed: %v"u8, output);
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
            if (builtin.len(testʗ1.expectedError) == 0){
                if (err != default!){
                    tΔ1.Errorf("unexpected failure: %s"u8, err);
                } else {
                    logInfo = false;
                }
            } else {
                if (err == default!){
                    tΔ1.Error(unexpectedSuccessˢ);
                } else 
                if (!strings.Contains(err.Error(), testʗ1.expectedError)){
                    tΔ1.Errorf("expected error containing %q, but got: %s"u8, testʗ1.expectedError, err);
                } else {
                    logInfo = false;
                }
            }

            if (logInfo) {
                @string certAsPEM(ж<global::go.crypto.x509_package.Certificate> cert) {
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    pem.Encode(new x509_test_package.bytes_BufferжWriter(Ꮡbuf), Ꮡ(new pem.Block(Type: "CERTIFICATE"u8, Bytes: (~cert).Raw)));
                    return Ꮡbuf.String();
                }
                tΔ1.Errorf("root:\n%s"u8, certAsPEM(rootPool.mustCert(tΔ1, 0)));
                {
                    var intermediates = allCerts(tΔ1, intermediatePool); if (builtin.len(intermediates) > 0) {
                        foreach (var (ii, intermediate) in intermediates) {
                            tΔ1.Errorf("intermediate %d:\n%s"u8, ii, certAsPEM(intermediate));
                        }
                    }
                }
                tΔ1.Errorf("leaf:\n%s"u8, certAsPEM(leafCert));
            }

            foreach (var (_, key) in keys) {
                ᏑprivateKeys.Put(key.OrTypedNil());
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameConstraintsTestˢ = "name_constraints_test"u8;

internal static ж<os.File> writePEMsToTempFile(slice<ж<global::go.crypto.x509_package.Certificate>> certs) {
    var (@file, err) = os.CreateTemp(""u8, nameConstraintsTestˢ);
    if (err != default!) {
        throw panic("cannot create tempfile");
    }

    var pemBlock = Ꮡ(new pem.Block(Type: "CERTIFICATE"u8));
    foreach (var (_, cert) in certs) {
        pemBlock.Value.Bytes = cert.Value.Raw;
        pem.Encode(new os.FileжWriter(@file), pemBlock);
    }

    return @file;
}

internal static (@string, error) testChainAgainstOpenSSL(ж<testing.T> Ꮡt, ж<global::go.crypto.x509_package.Certificate> Ꮡleaf, ж<global::go.crypto.x509_package.CertPool> Ꮡintermediates, ж<global::go.crypto.x509_package.CertPool> Ꮡroots) {
    GoFrame ᒐ = default;
    try {
        ref var leaf = ref Ꮡleaf.DerefOrNull();
        ref var intermediates = ref Ꮡintermediates.DerefOrNull();
        ref var roots = ref Ꮡroots.DerefOrNull();
        var argsᴷ = new stackAppend<@string>();

        var args = new @string[]{"verify"u8, "-no_check_time"u8}.slice();

        var rootsFile = writePEMsToTempFile(allCerts(Ꮡt, Ꮡroots));
        if (debugOpenSSLFailure){
            println((@string)"roots file:"u8, rootsFile.Name());
        } else {
            defer(os.Remove, rootsFile.Name(), ref ᒐ);
        }
        args = append(argsᴷ, args, "-CAfile"u8, rootsFile.Name());

        if (Ꮡintermediates.len() > 0) {
            var intermediatesFile = writePEMsToTempFile(allCerts(Ꮡt, Ꮡintermediates));
            if (debugOpenSSLFailure){
                println((@string)"intermediates file:"u8, intermediatesFile.Name());
            } else {
                defer(os.Remove, intermediatesFile.Name(), ref ᒐ);
            }
            args = append(args, "-untrusted"u8, intermediatesFile.Name());
        }

        var leafFile = writePEMsToTempFile(new ж<global::go.crypto.x509_package.Certificate>[]{Ꮡleaf}.slice());
        if (debugOpenSSLFailure){
            println((@string)"leaf file:"u8, leafFile.Name());
        } else {
            defer(os.Remove, leafFile.Name(), ref ᒐ);
        }
        args = append(args, leafFile.Name());

        var cmd = testenv.Command(new x509_test_package.testing_TжTB(Ꮡt), "openssl"u8, args.ꓸꓸꓸ);
        var (@out, err) = cmd.CombinedOutput();
        return ((@string)@out, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct rfc2821Testsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string localPart, domain;
}
internal static slice<rfc2821Testsᴛ1> rfc2821Tests = new rfc2821Testsᴛ1[]{
    new("foo@example.com"u8, "foo"u8, "example.com"u8),
    new("@example.com"u8, ""u8, ""u8),
    new("\"@example.com"u8, ""u8, ""u8),
    new("\"\"@example.com"u8, ""u8, "example.com"u8),
    new("\"a\"@example.com"u8, "a"u8, "example.com"u8),
    new("\"\\a\"@example.com"u8, "a"u8, "example.com"u8),
    new("a\"@example.com"u8, ""u8, ""u8),
    new("foo..bar@example.com"u8, ""u8, ""u8),
    new(".foo.bar@example.com"u8, ""u8, ""u8),
    new("foo.bar.@example.com"u8, ""u8, ""u8),
    new("|{}?'@example.com"u8, "|{}?'"u8, "example.com"u8),

    // Examples from RFC 3696
    new("Abc\\@def@example.com"u8, "Abc@def"u8, "example.com"u8),
    new("Fred\\ Bloggs@example.com"u8, "Fred Bloggs"u8, "example.com"u8),
    new("Joe.\\\\Blow@example.com"u8, "Joe.\\Blow"u8, "example.com"u8),
    new("\"Abc@def\"@example.com"u8, "Abc@def"u8, "example.com"u8),
    new("\"Fred Bloggs\"@example.com"u8, "Fred Bloggs"u8, "example.com"u8),
    new("customer/department=shipping@example.com"u8, "customer/department=shipping"u8, "example.com"u8),
    new("$A12345@example.com"u8, "$A12345"u8, "example.com"u8),
    new("!def!xyz%abc@example.com"u8, "!def!xyz%abc"u8, "example.com"u8),
    new("_somename@example.com"u8, "_somename"u8, "example.com"u8)
}.slice();

public static void TestRFC2821Parsing(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in rfc2821Tests) {
        var (mailbox, ok) = parseRFC2821Mailbox(test.@in);
        var expectedFailure = builtin.len(test.localPart) == 0 && builtin.len(test.domain) == 0;

        if (ok && expectedFailure) {
            Ꮡt.Errorf("#%d: %q unexpectedly parsed as (%q, %q)"u8, i, test.@in, mailbox.local, mailbox.domain);
            continue;
        }

        if (!ok && !expectedFailure) {
            Ꮡt.Errorf("#%d: unexpected failure for %q"u8, i, test.@in);
            continue;
        }

        if (!ok) {
            continue;
        }

        if (mailbox.local != test.localPart || mailbox.domain != test.domain) {
            Ꮡt.Errorf("#%d: %q parsed as (%q, %q), but wanted (%q, %q)"u8, i, test.@in, mailbox.local, mailbox.domain, test.localPart, test.domain);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedToParseˢ = "failed to parse "u8;
internal static readonly @string constraintˢ = "constraint"u8;
internal static readonly @string cannotBeEncodedAsAnˢ = "cannot be encoded as an IA5String"u8;

internal partial struct TestBadNamesInConstraints_badNames /*dyn*/ {
    internal @string name;
    internal Func<error, bool> matcher;
}

public static void TestBadNamesInConstraints(ж<testing.T> Ꮡt) {
    var constraintParseError = (error errΔ1) => {
        @string str = errΔ1.Error();
        return strings.Contains(str, failedToParseˢ) && strings.Contains(str, constraintˢ);
    };

    var encodingError = (error errΔ2) => strings.Contains(errΔ2.Error(), cannotBeEncodedAsAnˢ);

    // Bad names in constraints should not parse.
    var badNames = new TestBadNamesInConstraints_badNames[]{
        new("dns:foo.com."u8, constraintParseError),
        new("email:abc@foo.com."u8, constraintParseError),
        new("email:foo.com."u8, constraintParseError),
        new("uri:example.com."u8, constraintParseError),
        new("uri:1.2.3.4"u8, constraintParseError),
        new("uri:ffff::1"u8, constraintParseError),
        new("dns:not–hyphen.com"u8, encodingError),
        new("email:foo@not–hyphen.com"u8, encodingError),
        new("uri:not–hyphen.com"u8, encodingError)
    }.slice();

    var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        throw panic(err);
    }

    foreach (var (_, test) in badNames) {
        var (_, errΔ3) = makeConstraintsCACert(new constraintsSpec(
            ok: new @string[]{test.name}.slice()
        ), "TestAbsoluteNamesInConstraints"u8, priv, nil, priv);

        if (errΔ3 == default!){
            Ꮡt.Errorf("bad name %q unexpectedly accepted in name constraint"u8, test.name);
            continue;
        } else {
            if (!test.matcher(errΔ3)) {
                Ꮡt.Errorf("bad name %q triggered unrecognised error: %s"u8, test.name, errΔ3);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotParseˢ = "cannot parse "u8;

public static void TestBadNamesInSANs(ж<testing.T> Ꮡt) {
    // Bad names in URI and IP SANs should not parse. Bad DNS and email SANs
    // will parse and are tested in name constraint tests at the top of this
    // file.
    var badNames = new @string[]{
        "uri:https://example.com./dsf"u8,
        "invalidip:0102"u8,
        "invalidip:0102030405"u8
    }.slice();

    var (priv, err) = ecdsa.GenerateKey(elliptic.P256(), rand.Reader);
    if (err != default!) {
        throw panic(err);
    }

    foreach (var (_, badName) in badNames) {
        var (_, errΔ1) = makeConstraintsLeafCert(new leafSpec(sans: new @string[]{badName}.slice()), priv, nil, priv);

        if (errΔ1 == default!) {
            Ꮡt.Errorf("bad name %q unexpectedly accepted in SAN"u8, badName);
            continue;
        }

        {
            @string str = errΔ1.Error(); if (!strings.Contains(str, cannotParseˢ)) {
                Ꮡt.Errorf("bad name %q triggered unrecognised error: %s"u8, badName, str);
            }
        }
    }
}

} // end x509_internal_test_package
