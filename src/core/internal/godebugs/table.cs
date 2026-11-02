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
    public @string Name;    // name of the setting ("panicnil")
    public @string Package; // package that uses the setting ("runtime")
    public nint Changed;    // minor version when default changed, if any; 21 means Go 1.21
    public @string Old;     // value that restores behavior prior to Changed
    public bool Opaque;     // setting does not export information to runtime/metrics using [internal/godebug.Setting.IncNonDefault]
    public bool Immutable;  // setting cannot be changed after program start
}

// All is the table of known settings, sorted by Name.
//
// Note: After adding entries to this table, run 'go generate runtime/metrics'
// to update the runtime/metrics doc comment.
// (Otherwise the runtime/metrics test will fail.)
//
// Note: After adding entries to this table, update the list in doc/godebug.md as well.
// (Otherwise the test in this package will fail.)
[GoLazy] partial class lazyᴛAll { public static readonly ж<slice<Info>> ᏑAll = new StaticBox<slice<Info>>(slice<Info>([
    new(Name: "allowmultiplevcs", Package: "cmd/go"),
    new(Name: "asynctimerchan", Package: "time", Changed: 23, Old: "1"),
    new(Name: "containermaxprocs", Package: "runtime", Changed: 25, Old: "0"),
    new(Name: "cryptocustomrand", Package: "crypto", Changed: 26, Old: "1"),
    new(Name: "dataindependenttiming", Package: "crypto/subtle", Opaque: true),
    new(Name: "decoratemappings", Package: "runtime", Opaque: true, Changed: 25, Old: "0"),
    new(Name: "embedfollowsymlinks", Package: "cmd/go"),
    new(Name: "execerrdot", Package: "os/exec"),
    new(Name: "fips140", Package: "crypto/fips140", Opaque: true, Immutable: true),
    new(Name: "fips140ems", Package: "crypto/tls"),
    new(Name: "gocachehash", Package: "cmd/go"),
    new(Name: "gocachetest", Package: "cmd/go"),
    new(Name: "gocacheverify", Package: "cmd/go"),
    new(Name: "gotestjsonbuildtext", Package: "cmd/go", Changed: 24, Old: "1"),
    new(Name: "gotypesalias", Package: "go/types", Changed: 23, Old: "0"),
    new(Name: "htmlmetacontenturlescape", Package: "html/template"),
    new(Name: "http2client", Package: "net/http"),
    new(Name: "http2debug", Package: "net/http", Opaque: true),
    new(Name: "http2server", Package: "net/http"),
    new(Name: "httpcookiemaxnum", Package: "net/http", Changed: 24, Old: "0"),
    new(Name: "httplaxcontentlength", Package: "net/http", Changed: 22, Old: "1"),
    new(Name: "httpmuxgo121", Package: "net/http", Changed: 22, Old: "1"),
    new(Name: "httpservecontentkeepheaders", Package: "net/http", Changed: 23, Old: "1"),
    new(Name: "httpservecontentmaxranges", Package: "net/http", Changed: 26, Old: "1"),
    new(Name: "installgoroot", Package: "go/build"),
    new(Name: "jstmpllitinterp", Package: "html/template", Opaque: true), // bug #66217: remove Opaque
    //{Name: "multipartfiles", Package: "mime/multipart"},
    new(Name: "multipartmaxheaders", Package: "mime/multipart"),
    new(Name: "multipartmaxparts", Package: "mime/multipart"),
    new(Name: "multipathtcp", Package: "net", Changed: 24, Old: "0"),
    new(Name: "netdns", Package: "net", Opaque: true),
    new(Name: "netedns0", Package: "net", Changed: 19, Old: "0"),
    new(Name: "panicnil", Package: "runtime", Changed: 21, Old: "1"),
    new(Name: "randautoseed", Package: "math/rand"),
    new(Name: "randseednop", Package: "math/rand", Changed: 24, Old: "0"),
    new(Name: "rsa1024min", Package: "crypto/rsa", Changed: 24, Old: "0"),
    new(Name: "tarinsecurepath", Package: "archive/tar"),
    new(Name: "tls10server", Package: "crypto/tls", Changed: 22, Old: "1"),
    new(Name: "tls3des", Package: "crypto/tls", Changed: 23, Old: "1"),
    new(Name: "tlsmaxrsasize", Package: "crypto/tls"),
    new(Name: "tlsmlkem", Package: "crypto/tls", Changed: 24, Old: "0", Opaque: true),
    new(Name: "tlsrsakex", Package: "crypto/tls", Changed: 22, Old: "1"),
    new(Name: "tlssecpmlkem", Package: "crypto/tls", Changed: 26, Old: "0", Opaque: true),
    new(Name: "tlssha1", Package: "crypto/tls", Changed: 25, Old: "1"),
    new(Name: "tlsunsafeekm", Package: "crypto/tls", Changed: 22, Old: "1"),
    new(Name: "updatemaxprocs", Package: "runtime", Changed: 25, Old: "0"),
    new(Name: "urlmaxqueryparams", Package: "net/url", Changed: 24, Old: "0"),
    new(Name: "urlstrictcolons", Package: "net/url", Changed: 26, Old: "0"),
    new(Name: "winreadlinkvolume", Package: "os", Changed: 23, Old: "0"),
    new(Name: "winsymlink", Package: "os", Changed: 23, Old: "0"),
    new(Name: "x509keypairleaf", Package: "crypto/tls", Changed: 23, Old: "0"),
    new(Name: "x509negativeserial", Package: "crypto/x509", Changed: 23, Old: "1"),
    new(Name: "x509rsacrt", Package: "crypto/x509", Changed: 24, Old: "0"),
    new(Name: "x509sha256skid", Package: "crypto/x509", Changed: 25, Old: "0"),
    new(Name: "x509usefallbackroots", Package: "crypto/x509"),
    new(Name: "x509usepolicies", Package: "crypto/x509", Changed: 24, Old: "0"),
    new(Name: "zipinsecurepath", Package: "archive/zip")
])); }
public static ref slice<Info> All => ref ((StaticBox<slice<Info>>)ᏑAll).ValueSlot;

partial struct RemovedInfo {
    public @string Name; // name of the removed GODEBUG setting.
    public nint Removed; // minor version of Go, when the removal happened
}

// Removed contains all GODEBUGs that we have removed.
public static readonly ж<slice<RemovedInfo>> ᏑRemoved = new StaticBox<slice<RemovedInfo>>(slice<RemovedInfo>([
    new(Name: "x509sha1", Removed: 24)
]));
public static ref slice<RemovedInfo> Removed => ref ((StaticBox<slice<RemovedInfo>>)ᏑRemoved).ValueSlot;

// Lookup returns the Info with the given name.
public static ж<Info> Lookup(@string name) {
    // binary search, avoiding import of sort.
    nint lo = 0;
    nint hi = len(All);
    while (lo < hi) {
        nint m = (nint)((nuint)(lo + hi) >> 1);
        @string mid = All[m].Name;
        if (name == mid) {
            return Ꮡ(All, m);
        }
        if (name < mid) {
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return default;
}

} // end godebugs_package
