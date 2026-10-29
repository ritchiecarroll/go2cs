// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using goos = go.@internal.goos_package;
using go.@internal;

partial class x509_package {

// Possible certificate files; stop after finding one.
internal static slice<@string> certFiles = slice<@string>([
    "/etc/ssl/certs/ca-certificates.crt"u8,                // Debian/Ubuntu/Gentoo etc.
    "/etc/pki/tls/certs/ca-bundle.crt"u8,                  // Fedora/RHEL 6
    "/etc/ssl/ca-bundle.pem"u8,                            // OpenSUSE
    "/etc/pki/tls/cacert.pem"u8,                           // OpenELEC
    "/etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem"u8, // CentOS/RHEL 7
    "/etc/ssl/cert.pem"u8                                  // Alpine Linux
]);

// Possible directories with certificate files; all will be read.
internal static slice<@string> certDirectories = slice<@string>([
    "/etc/ssl/certs"u8,    // SLES10/SLES11, https://golang.org/issue/12139
    "/etc/pki/tls/certs"u8 // Fedora/RHEL
]);

[GoInit] internal static void initΔ1() {
    if (goos.IsAndroid == 1) {
        certDirectories = append(certDirectories,
            "/system/etc/security/cacerts"u8, // Android system roots
            "/data/misc/keychain/certs-added" // User trusted CA folder
        );
    }
}

} // end x509_package
