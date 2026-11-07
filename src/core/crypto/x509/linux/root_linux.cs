// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using goos = go.@internal.goos_package;
using go.@internal;

partial class x509_package {

// Possible certificate files; stop after finding one.
internal static slice<@string> certFiles = [
    "/etc/ssl/certs/ca-certificates.crt",                // Debian/Ubuntu/Gentoo etc.
    "/etc/pki/tls/certs/ca-bundle.crt",                  // Fedora/RHEL 6
    "/etc/ssl/ca-bundle.pem",                            // OpenSUSE
    "/etc/pki/tls/cacert.pem",                           // OpenELEC
    "/etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem", // CentOS/RHEL 7
    "/etc/ssl/cert.pem"                                  // Alpine Linux
];

// Possible directories with certificate files; all will be read.
internal static slice<@string> certDirectories = [
    "/etc/ssl/certs",    // SLES10/SLES11, https://golang.org/issue/12139
    "/etc/pki/tls/certs" // Fedora/RHEL
];

[GoInit] internal static void initΔ1() {
    if (goos.IsAndroid == 1) {
        certDirectories = append(certDirectories,
            (@string)"/system/etc/security/cacerts",   // Android system roots
            (@string)"/data/misc/keychain/certs-added" // User trusted CA folder
        );
    }
}

} // end x509_package
