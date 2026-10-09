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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using net;
using static go.mime.multipart_package;

partial class multipart_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myfileˢ = "myfile"u8;
internal static readonly @string myFileTxtˢ = "my-file.txt"u8;
internal static readonly @string valˢ = "val"u8;
internal static readonly object stringUnexpectedEmptyˢ = (@string)"String: unexpected empty result"u8;
internal static readonly object stringUnexpectedNewlineˢ = (@string)"String: unexpected newline"u8;

public static void TestWriter(ж<testing.T> Ꮡt) {
    var fileContents = slice<byte>("my file contents"u8);
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var w = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡb));
    {
        var (partΔ1, errΔ1) = w.CreateFormFile(myfileˢ, myFileTxtˢ);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("CreateFormFile: %v"u8, errΔ1);
        }
        partΔ1.Write(fileContents);
        errΔ1 = w.WriteField(keyˢ, valˢ);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("WriteField: %v"u8, errΔ1);
        }
        partΔ1.Write(slice<byte>("val"u8));
        errΔ1 = w.Close();
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("Close: %v"u8, errΔ1);
        }
        @string s = Ꮡb.String();
        if (len(s) == 0) {
            Ꮡt.Fatal(stringUnexpectedEmptyˢ);
        }
        if (s[0] == (rune)'\r' || s[0] == (rune)'\n') {
            Ꮡt.Fatal(stringUnexpectedNewlineˢ);
        }
    }
    var r = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡb), w.Boundary());
    var (part, err) = r.NextPart();
    if (err != default!) {
        Ꮡt.Fatalf("part 1: %v"u8, err);
    }
    {
        @string g = part.FormName();
        @string e = myfileˢ; if (g != e) {
            Ꮡt.Errorf("part 1: want form name %q, got %q"u8, e, g);
        }
    }
    (var slurp, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
    if (err != default!) {
        Ꮡt.Fatalf("part 1: ReadAll: %v"u8, err);
    }
    {
        @string e = ((@string)fileContents);
        @string g = ((@string)slurp); if (e != g) {
            Ꮡt.Errorf("part 1: want contents %q, got %q"u8, e, g);
        }
    }
    (part, err) = r.NextPart();
    if (err != default!) {
        Ꮡt.Fatalf("part 2: %v"u8, err);
    }
    {
        @string g = part.FormName();
        @string e = keyˢ; if (g != e) {
            Ꮡt.Errorf("part 2: want form name %q, got %q"u8, e, g);
        }
    }
    (slurp, err) = io.ReadAll(new global::go.mime.multipart_package.PartжReader(part));
    if (err != default!) {
        Ꮡt.Fatalf("part 2: ReadAll: %v"u8, err);
    }
    {
        @string e = valˢ;
        @string g = ((@string)slurp); if (e != g) {
            Ꮡt.Errorf("part 2: want contents %q, got %q"u8, e, g);
        }
    }
    (part, err) = r.NextPart();
    if (part != nil || err == default!) {
        Ꮡt.Fatalf("expected end of parts; got %v, %v"u8, part.OrTypedNil(), err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object multipartFormDataˢ = (@string)"multipart/form-data"u8;

internal partial struct TestWriterSetBoundary_tests /*dyn*/ {
    internal @string b;
    internal bool ok;
}

public static void TestWriterSetBoundary(ж<testing.T> Ꮡt) {
    var tests = new TestWriterSetBoundary_tests[]{
        new("abc"u8, true),
        new(""u8, false),
        new("ungültig"u8, false),
        new("!"u8, false),
        new(strings.Repeat("x"u8, 70), true),
        new(strings.Repeat("x"u8, 71), false),
        new("bad!ascii!"u8, false),
        new("my-separator"u8, true),
        new("with space"u8, true),
        new("badspace "u8, false),
        new("(boundary)"u8, true)
    }.slice();
    foreach (var (i, tt) in tests) {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡb));
        var err = w.SetBoundary(tt.b);
        var got = err == default!;
        if (got != tt.ok){
            Ꮡt.Errorf("%d. boundary %q = %v (%v); want %v"u8, i, tt.b, got, err, tt.ok);
        } else 
        if (tt.ok) {
            @string gotΔ1 = w.Boundary();
            if (gotΔ1 != tt.b) {
                Ꮡt.Errorf("boundary = %q; want %q"u8, gotΔ1, tt.b);
            }
            @string ct = w.FormDataContentType();
            var (mt, @params, errΔ1) = mime.ParseMediaType(ct);
            if (errΔ1 != default!){
                Ꮡt.Errorf("could not parse Content-Type %q: %v"u8, ct, errΔ1);
            } else 
            if (mt != "multipart/form-data"u8){
                Ꮡt.Errorf("unexpected media type %q; want %q"u8, mt, multipartFormDataˢ);
            } else 
            {
                @string bΔ1 = @params[boundaryˢ]; if (bΔ1 != tt.b) {
                    Ꮡt.Errorf("unexpected boundary parameter %q; want %q"u8, bΔ1, tt.b);
                }
            }
            w.Close();
            @string wantSub = "\r\n--"u8 + tt.b + "--\r\n"u8;
            {
                @string gotΔ2 = b.String(); if (!strings.Contains(gotΔ2, wantSub)) {
                    Ꮡt.Errorf("expected %q in output. got: %q"u8, wantSub, gotΔ2);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

public static partial void TestWriterBoundaryGoroutines(ж<testing.T> Ꮡt) {
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
internal static readonly @string mimeboundaryˢ = "MIMEBOUNDARY"u8;
internal static readonly @string mimeboundaryA2B5B7B6C4M3ˢ = "--MIMEBOUNDARY\r\nA: 2\r\nB: 5\r\nB: 7\r\nB: 6\r\nC: 4\r\nM: 3\r\nZ: 1\r\n\r\nfoo\r\n--MIMEBOUNDARY--\r\n"u8;

public static void TestSortedHeader(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var w = NewWriter(new multipart_test_package.strings_BuilderжWriter(Ꮡbuf));
    {
        var errΔ1 = w.SetBoundary(mimeboundaryˢ); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Error setting mime boundary: %v"u8, errΔ1);
        }
    }
    var header = new textproto.MIMEHeader(new map<@string, slice<@string>>{
        ["A"u8] = new @string[]{"2"u8}.slice(),
        ["B"u8] = new @string[]{"5"u8, "7"u8, "6"u8}.slice(),
        ["C"u8] = new @string[]{"4"u8}.slice(),
        ["M"u8] = new @string[]{"3"u8}.slice(),
        ["Z"u8] = new @string[]{"1"u8}.slice()
    });
    var (part, err) = w.CreatePart(header);
    if (err != default!) {
        Ꮡt.Fatalf("Unable to create part: %v"u8, err);
    }
    part.Write(slice<byte>("foo"u8));
    w.Close();
    @string want = mimeboundaryA2B5B7B6C4M3ˢ;
    if (want != buf.String()) {
        Ꮡt.Fatalf("\n got: %q\nwant: %q\n"u8, buf.String(), want);
    }
}

} // end multipart_internal_test_package
