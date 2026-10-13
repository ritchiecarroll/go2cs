// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using fs = go.io.fs_package;
using net = net_package;
using os = os_package;
using testing = testing_package;
using time = time_package;
using go.io;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errNotPollableˢ = "ErrNotPollable"u8;

public static void TestReadError(ж<testing.T> Ꮡt) {
    Ꮡt.Run(errNotPollableˢ, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var (f, err) = badStateFile();
            if (err != default!) {
                tΔ1.Skip(err);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);

            // Give scheduler a chance to have two separated
            // goroutines: an event poller and an event waiter.
            time.Sleep(100 * time.Millisecond);

            array<byte> b = new(1);
            (_, err) = f.Read(b[..]);
            {
                var perr = parseReadError(err, isBadStateFileError); if (perr != default!) {
                    tΔ1.Fatal(perr);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

internal static error parseReadError(error nestedErr, Func<error, (@string, bool)> verify) {
    var err = nestedErr;
    {
        var (nerr, ok) = err._<ж<net.OpError>>(ᐧ); if (ok) {
            err = nerr.Value.Err;
        }
    }
    {
        var (nerr, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
            err = nerr.Value.Err;
        }
    }
    {
        var (nerr, ok) = err._<ж<os.SyscallError>>(ᐧ); if (ok) {
            err = nerr.Value.Err;
        }
    }
    {
        var (s, ok) = verify(err); if (!ok) {
            return fmt.Errorf("got %v; want %s"u8, nestedErr, s);
        }
    }
    return default!;
}

} // end poll_test_package
