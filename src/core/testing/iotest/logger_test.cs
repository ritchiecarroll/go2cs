// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using log = log_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.testing.iotest_package;

partial class iotest_internal_test_package {

internal partial struct errWriter {
    internal error err;
}

internal static (nint, error) Write(this errWriter w, slice<byte> _) {
    return (0, w.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeˢ => ref writeˢᵇ;
private static readonly @string writeˢᵇ = "write:"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "Hello, World!"u8;

public static void TestWriteLogger(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var olw = log.Writer();
        nint olf = log.Flags();
        @string olp = log.Prefix();

        // Revert the original log settings before we exit.
        var olwʗ1 = olw;
        defer(() => {
            log.SetFlags(olf);
            log.SetPrefix(olp);
            log.SetOutput(olwʗ1);
        }, ref ᒐ);

        var lOut = @new<strings.Builder>();
        log.SetPrefix("lw: ");
        log.SetOutput(new iotest_test_package.strings_BuilderжWriter(lOut));
        log.SetFlags(0);

        var lw = @new<strings.Builder>();
        var wl = NewWriteLogger(writeˢ, new iotest_test_package.strings_BuilderжWriter(lw));
        {
            var (_, err) = wl.Write(slice<byte>("Hello, World!"u8)); if (err != nil) {
                t.Fatalf("Unexpectedly failed to write: %v", err);
            }
        }

        {
            @string g = lw.String();
            @string w = helloWorldˢ; if (g != w) {
                t.Errorf("WriteLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }
        @string wantLogWithHex = fmt.Sprintf("lw: write: %x\n"u8, helloWorldˢ);
        {
            @string g = lOut.String();
            @string w = wantLogWithHex; if (g != w) {
                t.Errorf("WriteLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeErrorˢ => ref writeErrorˢᵇ;
private static readonly @string writeErrorˢᵇ = "Write Error!"u8;

public static void TestWriteLogger_errorOnWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var olw = log.Writer();
        nint olf = log.Flags();
        @string olp = log.Prefix();

        // Revert the original log settings before we exit.
        var olwʗ1 = olw;
        defer(() => {
            log.SetFlags(olf);
            log.SetPrefix(olp);
            log.SetOutput(olwʗ1);
        }, ref ᒐ);

        var lOut = @new<strings.Builder>();
        log.SetPrefix("lw: ");
        log.SetOutput(new iotest_test_package.strings_BuilderжWriter(lOut));
        log.SetFlags(0);

        var lw = new errWriter(err: errors.New(writeErrorˢ));
        var wl = NewWriteLogger(writeˢ, lw);
        {
            var (_, err) = wl.Write(slice<byte>("Hello, World!"u8)); if (err == nil) {
                t.Fatalf("Unexpectedly succeeded to write: %v", err);
            }
        }

        @string wantLogWithHex = fmt.Sprintf("lw: write: %x: %v\n"u8, (@string)""u8, writeErrorˢ);
        {
            @string g = lOut.String();
            @string w = wantLogWithHex; if (g != w) {
                t.Errorf("WriteLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readˢ => ref readˢᵇ;
private static readonly @string readˢᵇ = "read:"u8;

public static void TestReadLogger(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var olw = log.Writer();
        nint olf = log.Flags();
        @string olp = log.Prefix();

        // Revert the original log settings before we exit.
        var olwʗ1 = olw;
        defer(() => {
            log.SetFlags(olf);
            log.SetPrefix(olp);
            log.SetOutput(olwʗ1);
        }, ref ᒐ);

        var lOut = @new<strings.Builder>();
        log.SetPrefix("lr: ");
        log.SetOutput(new iotest_test_package.strings_BuilderжWriter(lOut));
        log.SetFlags(0);

        var data = slice<byte>("Hello, World!"u8);
        var p = new slice<byte>(len(data));
        var lr = bytes.NewReader(data);
        var rl = NewReadLogger(readˢ, new iotest_test_package.bytes_ReaderжReader(lr));

        var (n, err) = rl.Read(p);
        if (err != nil) {
            t.Fatalf("Unexpectedly failed to read: %v", err);
        }

        {
            var (g, w) = (p.slice(0, n), data); if (!bytes.Equal(g, w)) {
                t.Errorf("ReadLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }

        @string wantLogWithHex = fmt.Sprintf("lr: read: %x\n"u8, helloWorldˢ);
        {
            @string g = lOut.String();
            @string w = wantLogWithHex; if (g != w) {
                t.Errorf("ReadLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ioFailureˢ => ref ioFailureˢᵇ;
private static readonly @string ioFailureˢᵇ = "io failure"u8;
internal static ref readonly @string readˢ2 => ref readˢ2ᵇ;
private static readonly @string readˢ2ᵇ = "read"u8;

public static void TestReadLogger_errorOnRead(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var olw = log.Writer();
        nint olf = log.Flags();
        @string olp = log.Prefix();

        // Revert the original log settings before we exit.
        var olwʗ1 = olw;
        defer(() => {
            log.SetFlags(olf);
            log.SetPrefix(olp);
            log.SetOutput(olwʗ1);
        }, ref ᒐ);

        var lOut = @new<strings.Builder>();
        log.SetPrefix("lr: ");
        log.SetOutput(new iotest_test_package.strings_BuilderжWriter(lOut));
        log.SetFlags(0);

        var data = slice<byte>("Hello, World!"u8);
        var p = new slice<byte>(len(data));

        var lr = ErrReader(errors.New(ioFailureˢ));
        var rl = NewReadLogger(readˢ2, lr);
        var (n, err) = rl.Read(p);
        if (err == nil) {
            t.Fatalf("Unexpectedly succeeded to read: %v", err);
        }

        @string wantLogWithHex = fmt.Sprintf("lr: read %x: io failure\n"u8, p.slice(0, n));
        {
            @string g = lOut.String();
            @string w = wantLogWithHex; if (g != w) {
                t.Errorf("ReadLogger mismatch\n\tgot:  %q\n\twant: %q", g, w);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end iotest_internal_test_package
