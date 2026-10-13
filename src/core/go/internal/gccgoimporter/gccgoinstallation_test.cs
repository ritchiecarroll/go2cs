// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using types = global::go.go.types_package;
using testing = testing_package;
using global::go.go;
using io = io_package;
using static global::go.go.@internal.gccgoimporter_package;

partial class gccgoimporter_internal_test_package {

// importablePackages is a list of packages that we verify that we can
// import. This should be all standard library packages in all relevant
// versions of gccgo. Note that since gccgo follows a different release
// cycle, and since different systems have different versions installed,
// we can't use the last-two-versions rule of the gc toolchain.
internal static array<@string> importablePackages = new @string[]{
    "archive/tar"u8,
    "archive/zip"u8,
    "bufio"u8,
    "bytes"u8,
    "compress/bzip2"u8,
    "compress/flate"u8,
    "compress/gzip"u8,
    "compress/lzw"u8,
    "compress/zlib"u8,
    "container/heap"u8,
    "container/list"u8,
    "container/ring"u8,
    "crypto/aes"u8,
    "crypto/cipher"u8,
    "crypto/des"u8,
    "crypto/dsa"u8,
    "crypto/ecdsa"u8,
    "crypto/elliptic"u8,
    "crypto"u8,
    "crypto/hmac"u8,
    "crypto/md5"u8,
    "crypto/rand"u8,
    "crypto/rc4"u8,
    "crypto/rsa"u8,
    "crypto/sha1"u8,
    "crypto/sha256"u8,
    "crypto/sha512"u8,
    "crypto/subtle"u8,
    "crypto/tls"u8,
    "crypto/x509"u8,
    "crypto/x509/pkix"u8,
    "database/sql/driver"u8,
    "database/sql"u8,
    "debug/dwarf"u8,
    "debug/elf"u8,
    "debug/gosym"u8,
    "debug/macho"u8,
    "debug/pe"u8,
    "encoding/ascii85"u8,
    "encoding/asn1"u8,
    "encoding/base32"u8,
    "encoding/base64"u8,
    "encoding/binary"u8,
    "encoding/csv"u8,
    "encoding/gob"u8,
    // "encoding", // Added in GCC 4.9.
    "encoding/hex"u8,
    "encoding/json"u8,
    "encoding/pem"u8,
    "encoding/xml"u8,
    "errors"u8,
    "expvar"u8,
    "flag"u8,
    "fmt"u8,
    "go/ast"u8,
    "go/build"u8,
    "go/doc"u8,
    // "go/format", // Added in GCC 4.8.
    "go/parser"u8,
    "go/printer"u8,
    "go/scanner"u8,
    "go/token"u8,
    "hash/adler32"u8,
    "hash/crc32"u8,
    "hash/crc64"u8,
    "hash/fnv"u8,
    "hash"u8,
    "html"u8,
    "html/template"u8,
    "image/color"u8,
    // "image/color/palette", // Added in GCC 4.9.
    "image/draw"u8,
    "image/gif"u8,
    "image"u8,
    "image/jpeg"u8,
    "image/png"u8,
    "index/suffixarray"u8,
    "io"u8,
    "io/ioutil"u8,
    "log"u8,
    "log/syslog"u8,
    "math/big"u8,
    "math/cmplx"u8,
    "math"u8,
    "math/rand"u8,
    "mime"u8,
    "mime/multipart"u8,
    "net"u8,
    "net/http/cgi"u8,
    // "net/http/cookiejar", // Added in GCC 4.8.
    "net/http/fcgi"u8,
    "net/http"u8,
    "net/http/httptest"u8,
    "net/http/httputil"u8,
    "net/http/pprof"u8,
    "net/mail"u8,
    "net/rpc"u8,
    "net/rpc/jsonrpc"u8,
    "net/smtp"u8,
    "net/textproto"u8,
    "net/url"u8,
    "os/exec"u8,
    "os"u8,
    "os/signal"u8,
    "os/user"u8,
    "path/filepath"u8,
    "path"u8,
    "reflect"u8,
    "regexp"u8,
    "regexp/syntax"u8,
    "runtime/debug"u8,
    "runtime"u8,
    "runtime/pprof"u8,
    "sort"u8,
    "strconv"u8,
    "strings"u8,
    "sync/atomic"u8,
    "sync"u8,
    "syscall"u8,
    "testing"u8,
    "testing/iotest"u8,
    "testing/quick"u8,
    "text/scanner"u8,
    "text/tabwriter"u8,
    "text/template"u8,
    "text/template/parse"u8,
    "time"u8,
    "unicode"u8,
    "unicode/utf16"u8,
    "unicode/utf8"u8
}.array();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisTestNeedsGccgoˢ = (@string)"This test needs gccgo"u8;

public static void TestInstallationImporter(ж<testing.T> Ꮡt) {
    // This test relies on gccgo being around.
    @string gpath = gccgoPath();
    if (gpath == ""u8) {
        Ꮡt.Skip(thisTestNeedsGccgoˢ);
    }

    global::go.go.@internal.gccgoimporter_package.GccgoInstallation inst = default!;
    var err = inst.InitFromDriver(gpath);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var imp = inst.GetImporter(default!, default!);

    // Ensure we don't regress the number of packages we can parse. First import
    // all packages into the same map and then each individually.
    var pkgMap = new map<@string, ж<types.Package>>();
    foreach (var (_, pkg) in importablePackages.ΔRangeSnapshot()) {
        (_, err) = imp(pkgMap, pkg, "."u8, default!);
        if (err != default!) {
            Ꮡt.Error(err);
        }
    }

    foreach (var (_, pkg) in importablePackages.ΔRangeSnapshot()) {
        (_, err) = imp(new map<@string, ж<types.Package>>(), pkg, "."u8, default!);
        if (err != default!) {
            Ꮡt.Error(err);
        }
    }

    // Test for certain specific entities in the imported data.
    foreach (var (_, vᴛ1) in new importerTest[]{
        new(pkgpath: "io"u8, name: "Reader"u8, want: "type Reader interface{Read(p []byte) (n int, err error)}"u8),
        new(pkgpath: "io"u8, name: "ReadWriter"u8, want: "type ReadWriter interface{Reader; Writer}"u8),
        new(pkgpath: "math"u8, name: "Pi"u8, want: "const Pi untyped float"u8),
        new(pkgpath: "math"u8, name: "Sin"u8, want: "func Sin(x float64) float64"u8),
        new(pkgpath: "sort"u8, name: "Search"u8, want: "func Search(n int, f func(int) bool) int"u8),
        new(pkgpath: "unsafe"u8, name: "Pointer"u8, want: "type Pointer"u8)
    }.array()) {
        ref var test = ref heap(new importerTest(), out var Ꮡtest);
        test = vᴛ1;

        runImporterTest(Ꮡt, imp, default!, Ꮡtest);
    }
}

} // end gccgoimporter_internal_test_package
