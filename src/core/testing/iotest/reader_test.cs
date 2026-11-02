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

public static void TestOneByteReader_nonEmptyReader(ж<testing.T> t) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);

    var obr = OneByteReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default;
    var (n, err) = obr.Read(b);
    if (err != nil || n != 0) {
        t.Errorf("Empty buffer read returned n=%d err=%v", n, err);
    }

    b = new slice<byte>(3);
    // Read from obr until EOF.
    var got = @new<strings.Builder>();
    for (nint i = 0; ; i++) {
        (n, err) = obr.Read(b);
        if (err != nil) {
            break;
        }
        {
            nint g = n;
            nint w = 1; if (g != w) {
                t.Errorf("Iteration #%d read %d bytes, want %d", i, g, w);
            }
        }
        got.Write(b.slice(0, n));
    }
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            t.Errorf("Unexpected error after reading all bytes\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            t.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q", g, w);
        }
    }
}

public static void TestOneByteReader_emptyReader(ж<testing.T> t) {
    var r = @new<bytes.Buffer>();

    var obr = OneByteReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default;
    {
        var (nΔ1, errΔ1) = obr.Read(b); if (errΔ1 != nil || nΔ1 != 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ1, errΔ1);
        }
    }

    b = new slice<byte>(5);
    var (n, err) = obr.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
}

public static void TestHalfReader_nonEmptyReader(ж<testing.T> t) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    // empty read buffer
    var hr = HalfReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default;
    var (n, err) = hr.Read(b);
    if (err != nil || n != 0) {
        t.Errorf("Empty buffer read returned n=%d err=%v", n, err);
    }
    // non empty read buffer
    b = new slice<byte>(2);
    var got = @new<strings.Builder>();
    for (nint i = 0; ; i++) {
        (n, err) = hr.Read(b);
        if (err != nil) {
            break;
        }
        {
            nint g = n;
            nint w = 1; if (g != w) {
                t.Errorf("Iteration #%d read %d bytes, want %d", i, g, w);
            }
        }
        got.Write(b.slice(0, n));
    }
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            t.Errorf("Unexpected error after reading all bytes\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            t.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q", g, w);
        }
    }
}

public static void TestHalfReader_emptyReader(ж<testing.T> t) {
    var r = @new<bytes.Buffer>();

    var hr = HalfReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default;
    {
        var (nΔ1, errΔ1) = hr.Read(b); if (errΔ1 != nil || nΔ1 != 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ1, errΔ1);
        }
    }

    b = new slice<byte>(5);
    var (n, err) = hr.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
}

public static void TestTimeOutReader_nonEmptyReader(ж<testing.T> t) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);
    // empty read buffer
    var tor = TimeoutReader(new iotest_test_package.bytes_BufferжReader(buf));
    slice<byte> b = default;
    var (n, err) = tor.Read(b);
    if (err != nil || n != 0) {
        t.Errorf("Empty buffer read returned n=%d err=%v", n, err);
    }
    // Second call should timeout
    (n, err) = tor.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
    // non empty read buffer
    var tor2 = TimeoutReader(new iotest_test_package.bytes_BufferжReader(buf));
    b = new slice<byte>(3);
    {
        var (nΔ1, errΔ1) = tor2.Read(b); if (errΔ1 != nil || nΔ1 == 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ1, errΔ1);
        }
    }
    // Second call should timeout
    (n, err) = tor2.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
}

public static void TestTimeOutReader_emptyReader(ж<testing.T> t) {
    var r = @new<bytes.Buffer>();
    // empty read buffer
    var tor = TimeoutReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default;
    {
        var (nΔ1, errΔ1) = tor.Read(b); if (errΔ1 != nil || nΔ1 != 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ1, errΔ1);
        }
    }
    // Second call should timeout
    var (n, err) = tor.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
    // non empty read buffer
    var tor2 = TimeoutReader(new iotest_test_package.bytes_BufferжReader(r));
    b = new slice<byte>(5);
    {
        var (nΔ2, errΔ2) = tor2.Read(b); if (!AreEqual(errΔ2, io.EOF) || nΔ2 != 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ2, errΔ2);
        }
    }
    // Second call should timeout
    (n, err) = tor2.Read(b);
    {
        var (g, w) = (err, ErrTimeout); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
}

public static void TestDataErrReader_nonEmptyReader(ж<testing.T> t) {
    @string msg = helloWorldˢ;
    var buf = @new<bytes.Buffer>();
    buf.WriteString(msg);

    var der = DataErrReader(new iotest_test_package.bytes_BufferжReader(buf));

    var b = new slice<byte>(3);
    var got = @new<strings.Builder>();
    nint n = default;
    error err = default;
    for (;;) {
        (n, err) = der.Read(b);
        got.Write(b.slice(0, n));
        if (err != nil) {
            break;
        }
    }
    if (!AreEqual(err, io.EOF) || n == 0) {
        t.Errorf("Last Read returned n=%d err=%v", n, err);
    }
    {
        @string g = got.String();
        @string w = helloWorldˢ; if (g != w) {
            t.Errorf("Read mismatch\n\tGot:  %q\n\tWant: %q", g, w);
        }
    }
}

public static void TestDataErrReader_emptyReader(ж<testing.T> t) {
    var r = @new<bytes.Buffer>();

    var der = DataErrReader(new iotest_test_package.bytes_BufferжReader(r));
    slice<byte> b = default;
    {
        var (nΔ1, errΔ1) = der.Read(b); if (!AreEqual(errΔ1, io.EOF) || nΔ1 != 0) {
            t.Errorf("Empty buffer read returned n=%d err=%v", nΔ1, errΔ1);
        }
    }

    b = new slice<byte>(5);
    var (n, err) = der.Read(b);
    {
        var (g, w) = (err, io.EOF); if (!AreEqual(g, w)) {
            t.Errorf("Error mismatch\n\tGot:  %v\n\tWant: %v", g, w);
        }
    }
    {
        nint g = n;
        nint w = 0; if (g != w) {
            t.Errorf("Unexpectedly read %d bytes, wanted %d", g, w);
        }
    }
}

internal partial struct TestErrReader_cases /*dyn*/ {
    internal @string name;
    internal error err;
}

public static void TestErrReader(ж<testing.T> t) {
    var cases = slice<TestErrReader_cases>([
        new("nil error", default),
        new("non-nil error", errors.New(ioFailureˢ)),
        new("io.EOF", io.EOF)
    ]);

    foreach (var (_, vᴛ1) in cases) {
        ref var tt = ref heap(new TestErrReader_cases(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (n, err) = ErrReader(ttʗ1.err).Read(default);
            if (!AreEqual(err, ttʗ1.err)) {
                tΔ1.Fatalf("Error mismatch\nGot:  %v\nWant: %v", err, ttʗ1.err);
            }
            if (n != 0) {
                tΔ1.Fatalf("Byte count mismatch: got %d want 0", n);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string msgᶜ => ref msgᶜᵇ;
private static readonly @string msgᶜᵇ = "Now is the time for all good gophers."u8;

public static void TestStringsReader(ж<testing.T> t) {
    @string msg = msgᶜ;

    var r = strings.NewReader(msg);
    {
        var err = TestReader(new iotest_test_package.strings_ReaderжReader(r), slice<byte>(msg)); if (err != nil) {
            t.Fatal(err);
        }
    }
}

} // end iotest_internal_test_package
