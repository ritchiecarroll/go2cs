// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.testing.iotest_package;

partial class iotest_internal_test_package {

public static void TestOneByteReader_nonEmptyReader(ж<testing.T> Ꮡt) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    var obr = OneByteReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default!;
    var (n, err) = obr.Read(b);
    if (err != default! || n != 0) {
        Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, n, err);
    }
    b = new slice<byte>(3);
    // Read from obr until EOF.
    var got = @new<strings.Builder>();
    for (nint i = 0; ᐧ ; i++) {
        (n, err) = obr.Read(b);
        if (err != default!) {
            break;
        }
        {
            nint g = n;
            nint w = 1; if (g != w) {
                Ꮡt.Errorf("Iteration #%d read %d bytes, want %d"u8, i, g, w);
            }
        }
        got.Write(b[..(int)(n)]);
    }
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Unexpected error after reading all bytes\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            Ꮡt.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q"u8, g, w);
        }
    }
}

public static void TestOneByteReader_emptyReader(ж<testing.T> Ꮡt) {
    var r = @new<bytes.Buffer>();
    var obr = OneByteReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default!;
    {
        var (nΔ1, errΔ1) = obr.Read(b); if (errΔ1 != default! || nΔ1 != 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ1, errΔ1);
        }
    }
    b = new slice<byte>(5);
    var (n, err) = obr.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
}

public static void TestHalfReader_nonEmptyReader(ж<testing.T> Ꮡt) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    // empty read buffer
    var hr = HalfReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default!;
    var (n, err) = hr.Read(b);
    if (err != default! || n != 0) {
        Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, n, err);
    }
    // non empty read buffer
    b = new slice<byte>(2);
    var got = @new<strings.Builder>();
    for (nint i = 0; ᐧ ; i++) {
        (n, err) = hr.Read(b);
        if (err != default!) {
            break;
        }
        {
            nint g = n;
            nint w = 1; if (g != w) {
                Ꮡt.Errorf("Iteration #%d read %d bytes, want %d"u8, i, g, w);
            }
        }
        got.Write(b[..(int)(n)]);
    }
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Unexpected error after reading all bytes\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            Ꮡt.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q"u8, g, w);
        }
    }
}

public static void TestHalfReader_emptyReader(ж<testing.T> Ꮡt) {
    var r = @new<bytes.Buffer>();
    var hr = HalfReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default!;
    {
        var (nΔ1, errΔ1) = hr.Read(b); if (errΔ1 != default! || nΔ1 != 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ1, errΔ1);
        }
    }
    b = new slice<byte>(5);
    var (n, err) = hr.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
}

public static void TestTimeOutReader_nonEmptyReader(ж<testing.T> Ꮡt) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    // empty read buffer
    var tor = TimeoutReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default!;
    var (n, err) = tor.Read(b);
    if (err != default! || n != 0) {
        Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, n, err);
    }
    // Second call should timeout
    (n, err) = tor.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
    // non empty read buffer
    var tor2 = TimeoutReader(new iotest_test_package.bytes_BufferжReader(buf));
    b = new slice<byte>(3);
    {
        var (nΔ1, errΔ1) = tor2.Read(b); if (errΔ1 != default! || nΔ1 == 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ1, errΔ1);
        }
    }
    // Second call should timeout
    (n, err) = tor2.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
}

public static void TestTimeOutReader_emptyReader(ж<testing.T> Ꮡt) {
    var r = @new<bytes.Buffer>();
    // empty read buffer
    var tor = TimeoutReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default!;
    {
        var (nΔ1, errΔ1) = tor.Read(b); if (errΔ1 != default! || nΔ1 != 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ1, errΔ1);
        }
    }
    // Second call should timeout
    var (n, err) = tor.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
    // non empty read buffer
    var tor2 = TimeoutReader(new iotest_test_package.bytes_BufferжReader(r));
    b = new slice<byte>(5);
    {
        var (nΔ2, errΔ2) = tor2.Read(b); if (!AreEqual(errΔ2, io.EOF) || nΔ2 != 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ2, errΔ2);
        }
    }
    // Second call should timeout
    (n, err) = tor2.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
}

public static void TestDataErrReader_nonEmptyReader(ж<testing.T> Ꮡt) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    var der = DataErrReader(new iotest_test_package.bytes_BufferжReader(buf));
    var b = new slice<byte>(3);
    var got = @new<strings.Builder>();
    nint n = default!;
    error err = default!;
    while (ᐧ) {
        (n, err) = der.Read(b);
        got.Write(b[..(int)(n)]);
        if (err != default!) {
            break;
        }
    }
    if (!AreEqual(err, io.EOF) || n == 0) {
        Ꮡt.Errorf("Last Read returned n=%d err=%v"u8, n, err);
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            Ꮡt.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q"u8, g, w);
        }
    }
}

public static void TestDataErrReader_emptyReader(ж<testing.T> Ꮡt) {
    var r = @new<bytes.Buffer>();
    var der = DataErrReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default!;
    {
        var (nΔ1, errΔ1) = der.Read(b); if (!AreEqual(errΔ1, io.EOF) || nΔ1 != 0) {
            Ꮡt.Errorf("Empty buffer read returned n=%d err=%v"u8, nΔ1, errΔ1);
        }
    }
    b = new slice<byte>(5);
    var (n, err) = der.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            Ꮡt.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            Ꮡt.Errorf("Unexpectedly read %d bytes, wanted %d"u8, g, w);
        }
    }
}

[GoType("dyn")] internal partial struct TestErrReader_cases {
    internal @string name;
    internal error err;
}

public static void TestErrReader(ж<testing.T> Ꮡt) {
    var cases = new TestErrReader_cases[]{
        new("nil error"u8, default!),
        new("non-nil error"u8, errors.New(ioFailureˢ)),
        new("io.EOF"u8, io.EOF)
    }.slice();
    foreach (var (_, tt) in cases) {
        ref var ttΔ1 = ref heap<TestErrReader_cases>(out var ᏑttΔ1);
        ttΔ1 = tt;
        var ttʗ1 = ttΔ1;
        Ꮡt.Run(ttΔ1.name, (ж<testing.T> tΔ1) => {
            var (n, err) = ErrReader(ttʗ1.err).Read(default!);
            if (!AreEqual(err, ttʗ1.err)) {
                tΔ1.Fatalf("Error mismatch\nGot:  %v\nWant: %v"u8, err, ttʗ1.err);
            }
            if (n != 0) {
                tΔ1.Fatalf("Byte count mismatch: got %d want 0"u8, n);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string msgᶜ = "Now is the time for all good gophers."u8;

public static void TestStringsReader(ж<testing.T> Ꮡt) {
    @string msg = msgᶜ;
    var r = strings.NewReader(msg);
    {
        var err = TestReader(new iotest_test_package.strings_ReaderжReader(r), slice<byte>(msg)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

} // end iotest_internal_test_package
