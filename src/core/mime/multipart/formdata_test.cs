// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using textproto = net.textproto_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using net;
using static go.mime.multipart_package;

partial class multipart_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFormˢ = (@string)"ReadForm:"u8;
internal static ref readonly @string textaˢ => ref textaˢᵇ;
private static readonly @string textaˢᵇ = "texta"u8;
internal static ref readonly @string textbˢ => ref textbˢᵇ;
private static readonly @string textbˢᵇ = "textb"u8;
internal static ref readonly @string fileaˢ => ref fileaˢᵇ;
private static readonly @string fileaˢᵇ = "filea"u8;
internal static ref readonly @string fileaTxtˢ => ref fileaTxtˢᵇ;
private static readonly @string fileaTxtˢᵇ = "filea.txt"u8;
internal static readonly object fileIsOsFileShouldNotBeˢ = (@string)"file is *os.File, should not be"u8;
internal static ref readonly @string filebˢ => ref filebˢᵇ;
private static readonly @string filebˢᵇ = "fileb"u8;
internal static ref readonly @string filebTxtˢ => ref filebTxtˢᵇ;
private static readonly @string filebTxtˢᵇ = "fileb.txt"u8;

public static void TestReadForm(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var b = strings.NewReader(strings.ReplaceAll(message, "\n", "\r\n"));
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(25);
        if (err != nil) {
            t.Fatal(readFormˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.RemoveAll(), ref ᒐ);
        {
            @string g = (~f).Value[textaˢ][0];
            @string e = textaValue; if (g != e) {
                t.Errorf("texta value = %q, want %q", g, e);
            }
        }
        {
            @string g = (~f).Value[textbˢ][0];
            @string e = textbValue; if (g != e) {
                t.Errorf("texta value = %q, want %q", g, e);
            }
        }
        var fd = testFile(t, f.File[fileaˢ][0], fileaTxtˢ, fileaContents);
        {
            var (_, ok) = fd._<ж<os.File>>(ᐧ); if (ok) {
                t.Error(fileIsOsFileShouldNotBeˢ);
            }
        }
        fd.Close();
        fd = testFile(t, f.File[filebˢ][0], filebTxtˢ, filebContents);
        {
            var (_, ok) = fd._<ж<os.File>>(ᐧ); if (!ok) {
                t.Errorf("file has unexpected underlying type %T", fd);
            }
        }
        fd.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hiddenfileˢ => ref hiddenfileˢᵇ;
private static readonly @string hiddenfileˢᵇ = "hiddenfile"u8;

public static void TestReadFormWithNamelessFile(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var b = strings.NewReader(strings.ReplaceAll(messageWithFileWithoutName, "\n", "\r\n"));
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(25);
        if (err != nil) {
            t.Fatal(readFormˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.RemoveAll(), ref ᒐ);

        {
            @string g = (~f).Value[hiddenfileˢ][0];
            @string e = filebContents; if (g != e) {
                t.Errorf("hiddenfile value = %q, want %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 58384: Handle ReadForm(math.MaxInt64)
public static void TestReadFormWitFileNameMaxMemoryOverflow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var b = strings.NewReader(strings.ReplaceAll(messageWithFileName, "\n", "\r\n"));
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(math.MaxInt64);
        if (err != nil) {
            t.Fatalf("ReadForm(MaxInt64): %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.RemoveAll(), ref ᒐ);

        var fd = testFile(t, f.File[fileaˢ][0], fileaTxtˢ, fileaContents);
        {
            var (_, ok) = fd._<ж<os.File>>(ᐧ); if (ok) {
                t.Error(fileIsOsFileShouldNotBeˢ);
            }
        }
        fd.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFormMaxInt64Missingˢ = (@string)"ReadForm(MaxInt64): missing form"u8;

// Issue 40430: Handle ReadForm(math.MaxInt64)
public static void TestReadFormMaxMemoryOverflow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var b = strings.NewReader(strings.ReplaceAll(messageWithTextContentType, "\n", "\r\n"));
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(math.MaxInt64);
        if (err != nil) {
            t.Fatalf("ReadForm(MaxInt64): %v", err);
        }
        if (f == nil) {
            t.Fatal(readFormMaxInt64Missingˢ);
        }
        var fʗ1 = f;
        defer(() => fʗ1.RemoveAll(), ref ᒐ);

        {
            @string g = (~f).Value[textaˢ][0];
            @string e = textaValue; if (g != e) {
                t.Errorf("texta value = %q, want %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReadFormWithTextContentType(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // From https://github.com/golang/go/issues/24041
        var b = strings.NewReader(strings.ReplaceAll(messageWithTextContentType, "\n", "\r\n"));
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(25);
        if (err != nil) {
            t.Fatal(readFormˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.RemoveAll(), ref ᒐ);

        {
            @string g = (~f).Value[textaˢ][0];
            @string e = textaValue; if (g != e) {
                t.Errorf("texta value = %q, want %q", g, e);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openingFileˢ = (@string)"opening file:"u8;
internal static readonly object copyingContentsˢ = (@string)"copying contents:"u8;

internal static global::go.mime.multipart_package.File testFile(ж<testing.T> t, ж<global::go.mime.multipart_package.FileHeader> Ꮡfh, @string efn, @string econtent) {
    ref var fh = ref Ꮡfh.DerefOrNull();

    if (fh.Filename != efn) {
        t.Errorf("filename = %q, want %q", fh.Filename, efn);
    }
    if (fh.Size != (int64)len(econtent)) {
        t.Errorf("size = %d, want %d", fh.Size, len(econtent));
    }
    var (f, err) = fh.Open();
    if (err != nil) {
        t.Fatal(openingFileˢ, err);
    }
    var b = @new<strings.Builder>();
    (_, err) = io.Copy(new multipart_test_package.strings_BuilderжWriter(b), f);
    if (err != nil) {
        t.Fatal(copyingContentsˢ, err);
    }
    {
        @string g = b.String(); if (g != econtent) {
            t.Errorf("contents = %q, want %q", g, econtent);
        }
    }
    return f;
}

internal static ref readonly @string fileaContents => ref fileaContentsᵇ;
private static readonly @string fileaContentsᵇ = "This is a test file."u8;
internal static ref readonly @string filebContents => ref filebContentsᵇ;
private static readonly @string filebContentsᵇ = "Another test file."u8;
internal static ref readonly @string textaValue => ref textaValueᵇ;
private static readonly @string textaValueᵇ = "foo"u8;
internal static ref readonly @string textbValue => ref textbValueᵇ;
private static readonly @string textbValueᵇ = "bar"u8;
internal static ref readonly @string boundary => ref boundaryᵇ;
private static readonly @string boundaryᵇ = @"MyBoundary"u8;

internal static ref readonly @string messageWithFileWithoutName => ref messageWithFileWithoutNameᵇ;
private static readonly @string messageWithFileWithoutNameᵇ = "\n--MyBoundary\nContent-Disposition: form-data; name=\"hiddenfile\"; filename=\"\"\nContent-Type: text/plain\n\nAnother test file.\n--MyBoundary--\n";

internal static ref readonly @string messageWithFileName => ref messageWithFileNameᵇ;
private static readonly @string messageWithFileNameᵇ = "\n--MyBoundary\nContent-Disposition: form-data; name=\"filea\"; filename=\"filea.txt\"\nContent-Type: text/plain\n\nThis is a test file.\n--MyBoundary--\n";

internal static ref readonly @string messageWithTextContentType => ref messageWithTextContentTypeᵇ;
private static readonly @string messageWithTextContentTypeᵇ = "\n--MyBoundary\nContent-Disposition: form-data; name=\"texta\"\nContent-Type: text/plain\n\nfoo\n--MyBoundary\n";

internal static ref readonly @string message => ref messageᵇ;
private static readonly @string messageᵇ = "\n--MyBoundary\nContent-Disposition: form-data; name=\"filea\"; filename=\"filea.txt\"\nContent-Type: text/plain\n\nThis is a test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"fileb\"; filename=\"fileb.txt\"\nContent-Type: text/plain\n\nAnother test file.\n--MyBoundary\nContent-Disposition: form-data; name=\"texta\"\n\nfoo\n--MyBoundary\nContent-Disposition: form-data; name=\"textb\"\n\nbar\n--MyBoundary--\n";

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contentDispositionFormˢ => ref contentDispositionFormˢᵇ;
private static readonly @string contentDispositionFormˢᵇ = """

-----------------------------8d345eef0d38dc9
Content-Disposition: form-data; name="version"

171
-----------------------------8d345eef0d38dc9--
"""u8.Raw();

public static void TestReadForm_NoReadAfterEOF(ж<testing.T> t) {
    var maxMemory = (int64)32 << 20;
    @string boundary = @"---------------------------8d345eef0d38dc9";
    @string body = contentDispositionFormˢ;

    var mr = NewReader(new multipart_internal_test_package.failOnReadAfterErrorReaderжReader(Ꮡ(new failOnReadAfterErrorReader(t: t, r: new multipart_test_package.strings_ReaderжReader(strings.NewReader(body))))), boundary);

    var (f, err) = mr.ReadForm(maxMemory);
    if (err != nil) {
        t.Fatal(err);
    }
    t.Logf("Got: %#v", f.OrTypedNil());
}

// failOnReadAfterErrorReader is an io.Reader wrapping r.
// It fails t if any Read is called after a failing Read.
internal partial struct failOnReadAfterErrorReader {
    internal ж<testing.T> t;
    internal io.Reader r;
    internal error sawErr;
}

internal static (nint n, error err) Read(this ref failOnReadAfterErrorReader r, slice<byte> p) {
    nint n = default;
    error err = default;

    if (r.sawErr != nil) {
        r.t.Fatalf("unexpected Read on Reader after previous read saw error %v", r.sawErr);
    }
    (n, err) = r.r.Read(p);
    r.sawErr = err;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static ref readonly @string largetextˢ => ref largetextˢᵇ;
private static readonly @string largetextˢᵇ = "largetext"u8;

// TestReadForm_NonFileMaxMemory asserts that the ReadForm maxMemory limit is applied
// while processing non-file form data as well as file form data.
public static void TestReadForm_NonFileMaxMemory(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    nint n = 10 << 20;
    @string largeTextValue = strings.Repeat("1", n);
    @string message = """
--MyBoundary
Content-Disposition: form-data; name="largetext"


"""u8.Raw() + largeTextValue + """

--MyBoundary--

"""u8.Raw();
    @string testBody = strings.ReplaceAll(message, "\n", "\r\n");
    // Try parsing the form with increasing maxMemory values.
    // Changes in how we account for non-file form data may cause the exact point
    // where we change from rejecting the form as too large to accepting it to vary,
    // but we should see both successes and failures.
    UntypedInt failWhenMaxMemoryLessThan = 128;
    for (var maxMemory = (int64)0; maxMemory < (int64)(failWhenMaxMemoryLessThan * 2); maxMemory += 16) {
        var b = strings.NewReader(testBody);
        var r = NewReader(new multipart_test_package.strings_ReaderжReader(b), boundary);
        var (f, err) = r.ReadForm(maxMemory);
        if (err != nil) {
            continue;
        }
        {
            @string g = (~f).Value[largetextˢ][0]; if (g != largeTextValue) {
                t.Errorf("largetext mismatch: got size: %v, expected size: %v", len(g), len(largeTextValue));
            }
        }
        f.RemoveAll();
        if (maxMemory < failWhenMaxMemoryLessThan) {
            t.Errorf("ReadForm(%v): no error, expect to hit memory limit when maxMemory < %v", maxMemory, (nint)(failWhenMaxMemoryLessThan));
        }
        return;
    }
    t.Errorf("ReadForm(x) failed for x < 1024, expect success");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string formDataNameAˢ => ref formDataNameAˢᵇ;
private static readonly @string formDataNameAˢᵇ = @"form-data; name=""a"""u8;
internal static ref readonly @string xFooˢ => ref xFooˢᵇ;
private static readonly @string xFooˢᵇ = "X-Foo"u8;

internal partial struct TestReadForm_MetadataTooLarge_type /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.mime.multipart_package.Writer>> f;
}

// TestReadForm_MetadataTooLarge verifies that we account for the size of field names,
// MIME headers, and map entry overhead while limiting the memory consumption of parsed forms.
public static void TestReadForm_MetadataTooLarge(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestReadForm_MetadataTooLarge_type>([new(
        name: "large name",
        f: (ж<global::go.mime.multipart_package.Writer> fw) => {
            @string name = strings.Repeat("a", 10 << 20);
            var (w, _) = fw.CreateFormField(name);
            w.Write(slice<byte>("value"u8));
        }
    ), new(
        name: "large MIME header",
        f: (ж<global::go.mime.multipart_package.Writer> fw) => {
            var h = new textproto.MIMEHeader(0);
            h.Set(contentDispositionˢ, formDataNameAˢ);
            h.Set(xFooˢ, strings.Repeat("a", 10 << 20));
            var (w, _) = fw.CreatePart(h);
            w.Write(slice<byte>("value"u8));
        }
    ), new(
        name: "many parts",
        f: (ж<global::go.mime.multipart_package.Writer> fw) => {
            for (nint i = 0; i < 110000; i++) {
                var (w, _) = fw.CreateFormField("f");
                w.Write(slice<byte>("v"u8));
            }
        }
    )
    ])) {
        ref var test = ref heap(new TestReadForm_MetadataTooLarge_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var fw = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
            testʗ1.f(fw);
            {
                var errΔ1 = fw.Close(); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            var fr = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡbuf), fw.Boundary());
            var (_, err) = fr.ReadForm(0);
            if (!AreEqual(err, ErrMessageTooLarge)) {
                tΔ1.Errorf("fr.ReadForm() = %v, want ErrMessageTooLarge", err);
            }
        });
    }
}

// TestReadForm_ManyFiles_Combined tests that a multipart form containing many files only
// results in a single on-disk file.
public static void TestReadForm_ManyFiles_Combined(ж<testing.T> t) {
    const bool distinct = false;
    testReadFormManyFiles(t, distinct);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string multipartfilesDistinctˢ => ref multipartfilesDistinctˢᵇ;
private static readonly @string multipartfilesDistinctˢᵇ = "multipartfiles=distinct"u8;

// TestReadForm_ManyFiles_Distinct tests that setting GODEBUG=multipartfiles=distinct
// results in every file in a multipart form being placed in a distinct on-disk file.
public static void TestReadForm_ManyFiles_Distinct(ж<testing.T> t) {
    t.Setenv(godebugˢ, multipartfilesDistinctˢ);
    const bool distinct = true;
    testReadFormManyFiles(t, distinct);
}

internal static void testReadFormManyFiles(ж<testing.T> t, bool distinct) {
    GoFrame ᒐ = default;
    try {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var fw = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
        const nint numFiles = 10;
        for (nint i = 0; i < numFiles; i++) {
            @string name = fmt.Sprint(i);
            var (w, errΔ1) = fw.CreateFormFile(name, name);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            w.Write(slice<byte>(name));
        }
        {
            var errΔ2 = fw.Close(); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
        }
        var fr = NewReader(new multipart_test_package.bytes_BufferжReader(Ꮡbuf), fw.Boundary());
        fr.tempDir = t.TempDir();
        var (form, err) = fr.ReadForm(0);
        if (err != nil) {
            t.Fatal(err);
        }
        for (nint i = 0; i < numFiles; i++) {
            @string name = fmt.Sprint(i);
            {
                nint gotΔ1 = len(form.File[name]); if (gotΔ1 != 1) {
                    t.Fatalf("form.File[%q] has %v entries, want 1", name, gotΔ1);
                }
            }
            var fh = form.File[name][0];
            var (@file, errΔ3) = fh.Open();
            if (errΔ3 != nil) {
                t.Fatalf("form.File[%q].Open() = %v", name, errΔ3);
            }
            if (distinct) {
                {
                    var (_, ok) = @file._<ж<os.File>>(ᐧ); if (!ok) {
                        t.Fatalf("form.File[%q].Open: %T, want *os.File", name, @file);
                    }
                }
            }
            (var got, errΔ3) = io.ReadAll(@file);
            @file.Close();
            if (((sstring)got) != name || errΔ3 != nil) {
                t.Fatalf("read form.File[%q]: %q, %v; want %q, nil", name, (@string)got, errΔ3, name);
            }
        }
        (var dir, err) = os.Open(fr.tempDir);
        if (err != nil) {
            t.Fatal(err);
        }
        var dirʗ1 = dir;
        defer(() => dirʗ1.Close(), ref ᒐ);
        (var names, err) = dir.Readdirnames(0);
        if (err != nil) {
            t.Fatal(err);
        }
        nint wantNames = 1;
        if (distinct) {
            wantNames = numFiles;
        }
        if (len(names) != wantNames) {
            t.Fatalf("temp dir contains %v files; want 1", len(names));
        }
        {
            var errΔ4 = form.RemoveAll(); if (errΔ4 != nil) {
                t.Fatalf("form.RemoveAll() = %v", errΔ4);
            }
        }
        (names, err) = dir.Readdirnames(0);
        if (err != nil) {
            t.Fatal(err);
        }
        if (len(names) != 0) {
            t.Fatalf("temp dir contains %v files; want 0", len(names));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestReadFormLimits_type /*dyn*/ {
    internal nint values;
    internal nint files;
    internal nint extraKeysPerFile;
    internal error wantErr;
    internal @string godebug;
}

public static void TestReadFormLimits(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestReadFormLimits_type>([
        new(values: 1000),
        new(values: 1001, wantErr: ErrMessageTooLarge),
        new(values: 500, files: 500),
        new(values: 501, files: 500, wantErr: ErrMessageTooLarge),
        new(files: 1000),
        new(files: 1001, wantErr: ErrMessageTooLarge),
        new(files: 1, extraKeysPerFile: 9998), // plus Content-Disposition and Content-Type
        new(files: 1, extraKeysPerFile: 10000, wantErr: ErrMessageTooLarge),
        new(godebug: "multipartmaxparts=100", values: 100),
        new(godebug: "multipartmaxparts=100", values: 101, wantErr: ErrMessageTooLarge),
        new(godebug: "multipartmaxheaders=100", files: 2, extraKeysPerFile: 48),
        new(godebug: "multipartmaxheaders=100", files: 2, extraKeysPerFile: 50, wantErr: ErrMessageTooLarge)
    ])) {
        ref var test = ref heap(new TestReadFormLimits_type(), out var Ꮡtest);
        test = vᴛ1;

        @string name = fmt.Sprintf("values=%v/files=%v/extraKeysPerFile=%v"u8, test.values, test.files, test.extraKeysPerFile);
        if (test.godebug != ""u8) {
            name += fmt.Sprintf("/godebug=%v"u8, test.godebug);
        }
        var testʗ1 = test;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (testʗ1.godebug != ""u8) {
                    tΔ1.Setenv(godebugˢ, testʗ1.godebug);
                }
                ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                var fw = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
                for (nint i = 0; i < testʗ1.values; i++) {
                    var (w, _) = fw.CreateFormField(fmt.Sprintf("field%v"u8, i));
                    fmt.Fprintf(w, "value %v"u8, i);
                }
                for (nint i = 0; i < testʗ1.files; i++) {
                    var h = new textproto.MIMEHeader(0);
                    h.Set(contentDispositionˢ,
                        fmt.Sprintf(@"form-data; name=""file%v""; filename=""file%v"""u8, i, i));
                    h.Set(contentTypeˢ, applicationOctetStreamˢ);
                    for (nint j = 0; j < testʗ1.extraKeysPerFile; j++) {
                        h.Set(fmt.Sprintf("k%v"u8, j), "v");
                    }
                    var (w, _) = fw.CreatePart(h);
                    fmt.Fprintf(w, "value %v"u8, i);
                }
                {
                    var errΔ1 = fw.Close(); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                var fr = NewReader(new multipart_test_package.bytes_ReaderжReader(bytes.NewReader(buf.Bytes())), fw.Boundary());
                var (form, err) = fr.ReadForm((int64)1 << 10);
                if (err == nil) {
                    var formʗ1 = form;
                    defer(() => formʗ1.RemoveAll(), ref ᒐ);
                }
                if (!AreEqual(err, testʗ1.wantErr)) {
                    tΔ1.Errorf("ReadForm = %v, want %v", err, testʗ1.wantErr);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string boundaryˢ => ref boundaryˢᵇ;
private static readonly @string boundaryˢᵇ = "boundary"u8;

internal partial struct TestReadFormEndlessHeaderLine_type /*dyn*/ {
    internal @string name;
    internal @string prefix;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string eolᶜ => ref eolᶜᵇ;
private static readonly @string eolᶜᵇ = "\r\n"u8;

public static void TestReadFormEndlessHeaderLine(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestReadFormEndlessHeaderLine_type>([new(
        name: "name",
        prefix: "X-"
    ), new(
        name: "value",
        prefix: "X-Header: "
    ), new(
        name: "continuation",
        prefix: "X-Header: foo\r\n  "
    )
    ])) {
        ref var test = ref heap(new TestReadFormEndlessHeaderLine_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            @string eol = eolᶜ;
            @string s = @"--boundary" + eol;
            s += @"Content-Disposition: form-data; name=""a""" + eol;
            s += @"Content-Type: text/plain" + eol;
            s += testʗ1.prefix;
            var fr = io.MultiReader(
                new multipart_test_package.strings_ReaderжReader(strings.NewReader(s)),
                (neverendingReader)(rune)'X');
            var r = NewReader(fr, boundaryˢ);
            var (_, err) = r.ReadForm((int64)1 << 20);
            if (!AreEqual(err, ErrMessageTooLarge)) {
                tΔ1.Fatalf("ReadForm(1 << 20): %v, want ErrMessageTooLarge", err);
            }
        });
    }
}

internal partial struct neverendingReader /*num:byte*/;

internal static (nint n, error err) Read(this neverendingReader r, slice<byte> p) {
    foreach (var i in range(p)) {
        p[i] = (byte)r;
    }
    return (len(p), default);
}

internal partial struct BenchmarkReadForm_type /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.mime.multipart_package.Writer>, nint> form;
}

public static void BenchmarkReadForm(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in slice<BenchmarkReadForm_type>([new(
        name: "fields",
        form: (ж<global::go.mime.multipart_package.Writer> fw, nint count) => {
            for (nint i = 0; i < count; i++) {
                var (w, _) = fw.CreateFormField(fmt.Sprintf("field%v"u8, i));
                fmt.Fprintf(w, "value %v"u8, i);
            }
        }
    ), new(
        name: "files",
        form: (ж<global::go.mime.multipart_package.Writer> fw, nint count) => {
            for (nint i = 0; i < count; i++) {
                var (w, _) = fw.CreateFormFile(fmt.Sprintf("field%v"u8, i), fmt.Sprintf("file%v"u8, i));
                fmt.Fprintf(w, "value %v"u8, i);
            }
        }
    )
    ])) {
        ref var test = ref heap(new BenchmarkReadForm_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        b.Run(test.name, (ж<testing.B> bΔ1) => {
            foreach (var (_, maxMemory) in slice<int64>([
                0,
                (int64)1 << 20
            ])) {
                ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                var fw = NewWriter(new multipart_test_package.bytes_BufferжWriter(Ꮡbuf));
                testʗ1.form(fw, 10);
                {
                    var err = fw.Close(); if (err != nil) {
                        bΔ1.Fatal(err);
                    }
                }
                var fwʗ1 = fw;
                bΔ1.Run(fmt.Sprintf("maxMemory=%v"u8, maxMemory), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        var fr = NewReader(new multipart_test_package.bytes_ReaderжReader(bytes.NewReader(Ꮡbuf.Value.Bytes())), fwʗ1.Boundary());
                        var (form, err) = fr.ReadForm(maxMemory);
                        if (err != nil) {
                            bΔ2.Fatal(err);
                        }
                        form.RemoveAll();
                    }
                });
            }
        });
    }
}

} // end multipart_internal_test_package
