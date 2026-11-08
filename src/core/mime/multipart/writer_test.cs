// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using io = io_package;
using mime = mime_package;
using textproto = net.textproto_package;
using strings = strings_package;
using testing = testing_package;
using net;
using static go.mime.multipart_package;

partial class multipart_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myfileˢ => ref myfileˢᵇ;
private static readonly @string myfileˢᵇ = "myfile"u8;
internal static ref readonly @string myFileTxtˢ => ref myFileTxtˢᵇ;
private static readonly @string myFileTxtˢᵇ = "my-file.txt"u8;
internal static ref readonly @string valˢ => ref valˢᵇ;
private static readonly @string valˢᵇ = "val"u8;
internal static readonly object stringUnexpectedEmptyˢ = (@string)"String: unexpected empty result"u8;
internal static readonly object stringUnexpectedNewlineˢ = (@string)"String: unexpected newline"u8;

public static void TestWriter(ж<testing.T> t) {
    var fileContents = slice<byte>("my file contents"u8);

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var w = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡb));
    {
        var (partΔ1, errΔ1) = w.CreateFormFile(myfileˢ, myFileTxtˢ);
        if (errΔ1 != nil) {
            t.Fatalf("CreateFormFile: %v", errΔ1);
        }
        partΔ1.Write(fileContents);
        errΔ1 = w.WriteField(keyˢ, valˢ);
        if (errΔ1 != nil) {
            t.Fatalf("WriteField: %v", errΔ1);
        }
        partΔ1.Write(slice<byte>("val"u8));
        errΔ1 = w.Close();
        if (errΔ1 != nil) {
            t.Fatalf("Close: %v", errΔ1);
        }
        @string s = Ꮡb.String();
        if (len(s) == 0) {
            t.Fatal(stringUnexpectedEmptyˢ);
        }
        if (s[0] == '\r' || s[0] == '\n') {
            t.Fatal(stringUnexpectedNewlineˢ);
        }
    }

    var r = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡb), w.Boundary());

    var (part, err) = r.NextPart();
    if (err != nil) {
        t.Fatalf("part 1: %v", err);
    }
    {
        @string g = part.FormName();
        @string e = myfileˢ; if (g != e) {
            t.Errorf("part 1: want form name %q, got %q", e, g);
        }
    }
    (var slurp, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
    if (err != nil) {
        t.Fatalf("part 1: ReadAll: %v", err);
    }
    {
        @string e = (@string)fileContents;
        @string g = (@string)slurp; if (e != g) {
            t.Errorf("part 1: want contents %q, got %q", e, g);
        }
    }

    (part, err) = r.NextPart();
    if (err != nil) {
        t.Fatalf("part 2: %v", err);
    }
    {
        @string g = part.FormName();
        @string e = keyˢ; if (g != e) {
            t.Errorf("part 2: want form name %q, got %q", e, g);
        }
    }
    (slurp, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
    if (err != nil) {
        t.Fatalf("part 2: ReadAll: %v", err);
    }
    {
        @string e = valˢ;
        @string g = (@string)slurp; if (e != g) {
            t.Errorf("part 2: want contents %q, got %q", e, g);
        }
    }

    (part, err) = r.NextPart();
    if (part != nil || err == nil) {
        t.Fatalf("expected end of parts; got %v, %v", part.OrTypedNil(), err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object multipartFormDataˢ = (@string)"multipart/form-data"u8;

internal partial struct TestWriterSetBoundary_tests /*dyn*/ {
    internal @string b;
    internal bool ok;
}

public static void TestWriterSetBoundary(ж<testing.T> t) {
    var tests = slice<TestWriterSetBoundary_tests>([
        new("abc", true),
        new("", false),
        new("ungültig", false),
        new("!", false),
        new(strings.Repeat("x", 70), true),
        new(strings.Repeat("x", 71), false),
        new("bad!ascii!", false),
        new("my-separator", true),
        new("with space", true),
        new("badspace ", false),
        new("(boundary)", true)
    ]);
    foreach (var (i, tt) in range(tests)) {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡb));
        var err = w.SetBoundary(tt.b);
        var got = err == nil;
        if (got != tt.ok) {
            t.Errorf("%d. boundary %q = %v (%v); want %v", i, tt.b, got, err, tt.ok);
        } else if (tt.ok) {
            @string gotΔ1 = w.Boundary();
            if (gotΔ1 != tt.b) {
                t.Errorf("boundary = %q; want %q", gotΔ1, tt.b);
            }

            @string ct = w.FormDataContentType();
            var (mt, @params, errΔ1) = mime.ParseMediaType(ct);
            if (errΔ1 != nil) {
                t.Errorf("could not parse Content-Type %q: %v", ct, errΔ1);
            } else if (mt != "multipart/form-data"u8) {
                t.Errorf("unexpected media type %q; want %q", mt, multipartFormDataˢ);
            } else {
                @string bΔ1 = @params[boundaryˢ]; if (bΔ1 != tt.b) {
                    t.Errorf("unexpected boundary parameter %q; want %q", bΔ1, tt.b);
                }
            }

            w.Close();
            @string wantSub = "\r\n--"u8 + tt.b + "--\r\n"u8;
            @string gotΔ2 = b.String(); if (!strings.Contains(gotΔ2, wantSub)) {
                t.Errorf("expected %q in output. got: %q", wantSub, gotΔ2);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

public static partial void TestWriterBoundaryGoroutines(ж<testing.T> t) {
    // Verify there's no data race accessing any lazy boundary if it's used by
    // different goroutines. This was previously broken by
    // https://codereview.appspot.com/95760043/ and reverted in
    // https://codereview.appspot.com/117600043/
    var w = NewWriter(io.Discard);
    var done = new channel<nint>(0);
    var doneʗ1 = done;
    var wʗ1 = w;
    goǃ(() => {
        wʗ1.CreateFormField(fooˢ);
        doneʗ1.ᐸꟷ(1);
    });
    w.Boundary();
    ᐸꟷ(done);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mimeboundaryˢ => ref mimeboundaryˢᵇ;
private static readonly @string mimeboundaryˢᵇ = "MIMEBOUNDARY"u8;
internal static ref readonly @string mimeboundaryA2B5B7B6C4M3ˢ => ref mimeboundaryA2B5B7B6C4M3ˢᵇ;
private static readonly @string mimeboundaryA2B5B7B6C4M3ˢᵇ = "--MIMEBOUNDARY\r\nA: 2\r\nB: 5\r\nB: 7\r\nB: 6\r\nC: 4\r\nM: 3\r\nZ: 1\r\n\r\nfoo\r\n--MIMEBOUNDARY--\r\n"u8;

public static void TestSortedHeader(ж<testing.T> t) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf));
    var errΔ1 = w.SetBoundary(mimeboundaryˢ); if (errΔ1 != nil) {
        t.Fatalf("Error setting mime boundary: %v", errΔ1);
    }

    var header = new textproto.MIMEHeader(new map<@string, slice<@string>>{
        ["A"] = ["2"],
        ["B"] = ["5", "7", "6"],
        ["C"] = ["4"],
        ["M"] = ["3"],
        ["Z"] = ["1"]
    });

    var (part, err) = w.CreatePart(header);
    if (err != nil) {
        t.Fatalf("Unable to create part: %v", err);
    }
    part.Write(slice<byte>("foo"u8));

    w.Close();

    @string want = mimeboundaryA2B5B7B6C4M3ˢ;
    if (want != buf.String()) {
        t.Fatalf("\n got: %q\nwant: %q\n", buf.String(), want);
    }
}

internal partial struct TestFileContentDisposition_tests /*dyn*/ {
    internal @string fieldname;
    internal @string filename;
    internal @string want;
}

public static void TestFileContentDisposition(ж<testing.T> t) {
    var tests = slice<TestFileContentDisposition_tests>([
        new("somefield", "somefile.txt", @"form-data; name=""somefield""; filename=""somefile.txt"""),
        new(@"field""withquotes""", "somefile.txt", @"form-data; name=""field\""withquotes\""""; filename=""somefile.txt"""),
        new(@"somefield", @"somefile""withquotes"".txt", @"form-data; name=""somefield""; filename=""somefile\""withquotes\"".txt"""),
        new(@"somefield\withbackslash", "somefile.txt", @"form-data; name=""somefield\\withbackslash""; filename=""somefile.txt"""),
        new("somefield", @"somefile\withbackslash.txt", @"form-data; name=""somefield""; filename=""somefile\\withbackslash.txt"""),
        new("a\rb\nc", "e\rf\ng", @"form-data; name=""a%0Db%0Ac""; filename=""e%0Df%0Ag""")
    ]);
    foreach (var (i, tt) in range(tests)) {
        @string found = FileContentDisposition(tt.fieldname, tt.filename); if (found != tt.want) {
            t.Errorf(@"%d. found: ""%s""; want: ""%s""", i, found, tt.want);
        }
    }
}

} // end multipart_internal_test_package
