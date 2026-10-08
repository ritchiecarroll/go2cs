// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package godebugs provides a table of known GODEBUG settings,
// for use by a variety of other packages, including internal/godebug,
// runtime, runtime/metrics, and cmd/go/internal/load.
namespace go.@internal;

partial class godebugs_package {

// An Info describes a single known GODEBUG setting.
partial struct Info {
    public @string Name; // name of the setting ("panicnil")
    public @string Package; // package that uses the setting ("runtime")
    public nint Changed;   // minor version when default changed, if any; 21 means Go 1.21
    public @string Old; // value that restores behavior prior to Changed
    public bool Opaque;   // setting does not export information to runtime/metrics using [internal/godebug.Setting.IncNonDefault]
}

// bug #66217: remove Opaque
//{Name: "multipartfiles", Package: "mime/multipart"},
// All is the table of known settings, sorted by Name.
//
// Note: After adding entries to this table, run 'go generate runtime/metrics'
// to update the runtime/metrics doc comment.
// (Otherwise the runtime/metrics test will fail.)
//
// Note: After adding entries to this table, update the list in doc/godebug.md as well.
// (Otherwise the test in this package will fail.)
public static ж<slice<Info>> ᏑAll = new StandardBox<slice<Info>>(new Info[]{
    new(Name: "allowmultiplevcs"u8, Package: "cmd/go"u8),
    new(Name: "asynctimerchan"u8, Package: "time"u8, Changed: 23, Old: "1"u8),
    new(Name: "dataindependenttiming"u8, Package: "crypto/subtle"u8, Opaque: true),
    new(Name: "execerrdot"u8, Package: "os/exec"u8),
    new(Name: "fips140"u8, Package: "crypto/fips140"u8, Opaque: true),
    new(Name: "gocachehash"u8, Package: "cmd/go"u8),
    new(Name: "gocachetest"u8, Package: "cmd/go"u8),
    new(Name: "gocacheverify"u8, Package: "cmd/go"u8),
    new(Name: "gotestjsonbuildtext"u8, Package: "cmd/go"u8, Changed: 24, Old: "1"u8),
    new(Name: "gotypesalias"u8, Package: "go/types"u8, Changed: 23, Old: "0"u8),
    new(Name: "http2client"u8, Package: "net/http"u8),
    new(Name: "http2debug"u8, Package: "net/http"u8, Opaque: true),
    new(Name: "http2server"u8, Package: "net/http"u8),
    new(Name: "httpcookiemaxnum"u8, Package: "net/http"u8, Changed: 24, Old: "0"u8),
    new(Name: "httplaxcontentlength"u8, Package: "net/http"u8, Changed: 22, Old: "1"u8),
    new(Name: "httpmuxgo121"u8, Package: "net/http"u8, Changed: 22, Old: "1"u8),
    new(Name: "httpservecontentkeepheaders"u8, Package: "net/http"u8, Changed: 23, Old: "1"u8),
    new(Name: "installgoroot"u8, Package: "go/build"u8),
    new(Name: "jstmpllitinterp"u8, Package: "html/template"u8, Opaque: true),
    new(Name: "multipartmaxheaders"u8, Package: "mime/multipart"u8),
    new(Name: "multipartmaxparts"u8, Package: "mime/multipart"u8),
    new(Name: "multipathtcp"u8, Package: "net"u8, Changed: 24, Old: "0"u8),
    new(Name: "netdns"u8, Package: "net"u8, Opaque: true),
    new(Name: "netedns0"u8, Package: "net"u8, Changed: 19, Old: "0"u8),
    new(Name: "panicnil"u8, Package: "runtime"u8, Changed: 21, Old: "1"u8),
    new(Name: "randautoseed"u8, Package: "math/rand"u8),
    new(Name: "randseednop"u8, Package: "math/rand"u8, Changed: 24, Old: "0"u8),
    new(Name: "rsa1024min"u8, Package: "crypto/rsa"u8, Changed: 24, Old: "0"u8),
    new(Name: "tarinsecurepath"u8, Package: "archive/tar"u8),
    new(Name: "tls10server"u8, Package: "crypto/tls"u8, Changed: 22, Old: "1"u8),
    new(Name: "tls3des"u8, Package: "crypto/tls"u8, Changed: 23, Old: "1"u8),
    new(Name: "tlsmaxrsasize"u8, Package: "crypto/tls"u8),
    new(Name: "tlsmlkem"u8, Package: "crypto/tls"u8, Changed: 24, Old: "0"u8, Opaque: true),
    new(Name: "tlsrsakex"u8, Package: "crypto/tls"u8, Changed: 22, Old: "1"u8),
    new(Name: "tlsunsafeekm"u8, Package: "crypto/tls"u8, Changed: 22, Old: "1"u8),
    new(Name: "urlmaxqueryparams"u8, Package: "net/url"u8, Changed: 24, Old: "0"u8),
    new(Name: "winreadlinkvolume"u8, Package: "os"u8, Changed: 23, Old: "0"u8),
    new(Name: "winsymlink"u8, Package: "os"u8, Changed: 23, Old: "0"u8),
    new(Name: "x509keypairleaf"u8, Package: "crypto/tls"u8, Changed: 23, Old: "0"u8),
    new(Name: "x509negativeserial"u8, Package: "crypto/x509"u8, Changed: 23, Old: "1"u8),
    new(Name: "x509rsacrt"u8, Package: "crypto/x509"u8, Changed: 24, Old: "0"u8),
    new(Name: "x509usefallbackroots"u8, Package: "crypto/x509"u8),
    new(Name: "x509usepolicies"u8, Package: "crypto/x509"u8, Changed: 24, Old: "0"u8),
    new(Name: "zipinsecurepath"u8, Package: "archive/zip"u8)
}.slice());
public static ref slice<Info> All => ref ᏑAll.ValueSlot;

// Lookup returns the Info with the given name.
public static ж<Info> Lookup(@string name) {
    // binary search, avoiding import of sort.
    nint lo = 0;
    nint hi = len(All);
    while (lo < hi) {
        nint m = (nint)(((nuint)(lo + hi) >> (int)(1)));
        @string mid = All[m].Name;
        if (name == mid) {
            return Ꮡ(All, m);
        }
        if (name < mid){
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return default!;
}

} // end godebugs_package
