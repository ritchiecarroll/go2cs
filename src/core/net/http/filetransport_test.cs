// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using os = os_package;
using filepath = global::go.path.filepath_package;
using testing = testing_package;
using fstest = global::go.testing.fstest_package;
using fs = global::go.io.fs_package;
using global::go.path;
using global::go.testing;
using static global::go.net.http_package;

partial class http_internal_test_package {

internal static Action<@string, error> checker(ж<testing.T> Ꮡt) {
    return (@string call, error err) => {
        if (err == default!) {
            return;
        }
        Ꮡt.Fatalf("%s: %v"u8, call, err);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooTxtˢ = "foo.txt"u8;
internal static readonly @string writeFileˢ = "WriteFile"u8;
internal static readonly @string fileˢ = "file"u8;
internal static readonly object barˢ = (@string)"Bar"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string badURLᶜ = "file://../no-exist.txt"u8;

public static void TestFileTransport(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var check = checker(Ꮡt);
        @string dname = Ꮡt.TempDir();
        @string fname = filepath.Join(dname, fooTxtˢ);
        var err = os.WriteFile(fname, slice<byte>("Bar"u8), 420);
        check(writeFileˢ, err);
        defer(os.Remove, fname, ref ᒐ);
        var tr = Ꮡ(new Transport(nil));
        tr.RegisterProtocol(fileˢ, NewFileTransport(((global::go.net.http_package.Dir)dname)));
        var c = Ꮡ(new Client(Transport: new global::go.net.http_package.TransportжRoundTripper(tr)));
        var fooURLs = new @string[]{"file:///foo.txt"u8, "file://../foo.txt"u8}.slice();
        foreach (var (_, urlstr) in fooURLs) {
            var (resΔ1, errΔ1) = c.Get(urlstr);
            check("Get "u8 + urlstr, errΔ1);
            if ((~resΔ1).StatusCode != 200) {
                Ꮡt.Errorf("for %s, StatusCode = %d, want 200"u8, urlstr, (~resΔ1).StatusCode);
            }
            if ((~resΔ1).ContentLength != -1) {
                Ꮡt.Errorf("for %s, ContentLength = %d, want -1"u8, urlstr, (~resΔ1).ContentLength);
            }
            if ((~resΔ1).Body == default!) {
                Ꮡt.Fatalf("for %s, nil Body"u8, urlstr);
            }
            (var slurp, errΔ1) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader((~resΔ1).Body));
            (~resΔ1).Body.Close();
            check("ReadAll "u8 + urlstr, errΔ1);
            if (((sstring)slurp) != "Bar"u8) {
                Ꮡt.Errorf("for %s, got content %q, want %q"u8, urlstr, ((@string)slurp), barˢ);
            }
        }
        @string badURL = badURLᶜ;
        (var res, err) = c.Get(badURL);
        check("Get " + badURL, err);
        if ((~res).StatusCode != 404) {
            Ꮡt.Errorf("for %s, StatusCode = %d, want 404"u8, badURL, (~res).StatusCode);
        }
        (~res).Body.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string badURLᶜ1 = "file://../no-exist.txt"u8;

public static void TestFileTransportFS(ж<testing.T> Ꮡt) {
    var check = checker(Ꮡt);
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["index.html"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("index.html says hello"u8)))
    });
    var tr = Ꮡ(new Transport(nil));
    tr.RegisterProtocol(fileˢ, NewFileTransportFS(fsys));
    var c = Ꮡ(new Client(Transport: new global::go.net.http_package.TransportжRoundTripper(tr)));
    foreach (var (fname, mfile) in fsys) {
        @string urlstr = "file:///"u8 + fname;
        var (resΔ1, errΔ1) = c.Get(urlstr);
        check("Get "u8 + urlstr, errΔ1);
        if ((~resΔ1).StatusCode != 200) {
            Ꮡt.Errorf("for %s, StatusCode = %d, want 200"u8, urlstr, (~resΔ1).StatusCode);
        }
        if ((~resΔ1).ContentLength != -1) {
            Ꮡt.Errorf("for %s, ContentLength = %d, want -1"u8, urlstr, (~resΔ1).ContentLength);
        }
        if ((~resΔ1).Body == default!) {
            Ꮡt.Fatalf("for %s, nil Body"u8, urlstr);
        }
        (var slurp, errΔ1) = io.ReadAll(new http_test_package.io_ReadCloserᴠReader((~resΔ1).Body));
        (~resΔ1).Body.Close();
        check("ReadAll "u8 + urlstr, errΔ1);
        if (((sstring)slurp) != ((sstring)(~mfile).Data)) {
            Ꮡt.Errorf("for %s, got content %q, want %q"u8, urlstr, ((@string)slurp), barˢ);
        }
    }
    @string badURL = badURLᶜ1;
    var (res, err) = c.Get(badURL);
    check("Get " + badURL, err);
    if ((~res).StatusCode != 404) {
        Ꮡt.Errorf("for %s, StatusCode = %d, want 404"u8, badURL, (~res).StatusCode);
    }
    (~res).Body.Close();
}

} // end http_internal_test_package
