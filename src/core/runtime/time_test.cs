// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using testenv = @internal.testenv_package;
using exec = global::go.os.exec_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using @internal;
using encoding;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object faketimeNotSupportedOnˢ = (@string)"faketime not supported on windows"u8;
internal static ref readonly @string testfaketimeˢ => ref testfaketimeˢᵇ;
private static readonly @string testfaketimeˢᵇ = "testfaketime"u8;
internal static ref readonly @string tagsFaketimeˢ => ref tagsFaketimeˢᵇ;
private static readonly @string tagsFaketimeˢᵇ = "-tags=faketime"u8;

public static void TestFakeTime(ж<testing.T> t) {
    if (Δruntime.GOOS == "windows"u8) {
        t.Skip(faketimeNotSupportedOnˢ);
    }

    // Faketime is advanced in checkdead. External linking brings in cgo,
    // causing checkdead not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(t), deadlockBuildTypes);

    t.Parallel();

    var (exe, err) = buildTestProg(t, testfaketimeˢ, tagsFaketimeˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    var cmd = exec.Command(exe);
    cmd.Stdout = new runtime_test_package.bytes_BufferжWriter(Ꮡstdout);
    cmd.Stderr = new runtime_test_package.bytes_BufferжWriter(Ꮡstderr);

    err = testenv.CleanCmdEnv(cmd).Run();
    if (err != nil) {
        t.Fatalf("exit status: %v\n%s", err, Ꮡstderr.String());
    }

    t.Logf("raw stdout: %q", Ꮡstdout.String());
    t.Logf("raw stderr: %q", Ꮡstderr.String());

    var (f1, err1) = parseFakeTime(stdout.Bytes());
    if (err1 != nil) {
        t.Fatal(err1);
    }
    var (f2, err2) = parseFakeTime(stderr.Bytes());
    if (err2 != nil) {
        t.Fatal(err2);
    }

    UntypedInt time0 = 1257894000000000000;
    var got = slice<slice<fakeTimeFrame>>([f1, f2]);
    slice<slice<fakeTimeFrame>> want = slice<slice<fakeTimeFrame>>([slice<fakeTimeFrame>([
        new(time0 + 1, "line 2\n"),
        new(time0 + 1, "line 3\n"),
        new(/* time0 + 1e9 */ 1257894001000000000UL, "line 5\n"),
        new(/* time0 + 1e9 */ 1257894001000000000UL, "2009-11-10T23:00:01Z")]), slice<fakeTimeFrame>([
        new(time0, "line 1\n"),
        new(time0 + 2, "line 4\n")])
    ]);
    if (!reflect.DeepEqual(want, got)) {
        t.Fatalf("want %v, got %v", want, got);
    }
}

partial struct fakeTimeFrame {
    internal uint64 time;
    internal @string data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string truncatedHeaderˢ => ref truncatedHeaderˢᵇ;
private static readonly @string truncatedHeaderˢᵇ = "truncated header"u8;
internal static ref readonly @string badMagicˢ => ref badMagicˢᵇ;
private static readonly @string badMagicˢᵇ = "bad magic"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string magicᶜ => ref magicᶜᵇ;
private static readonly @string magicᶜᵇ = "\x00\x00PB"u8;

internal static (slice<fakeTimeFrame>, error) parseFakeTime(slice<byte> x) {
    var framesᴷ = new stackAppend<fakeTimeFrame>(moves: true);

    slice<fakeTimeFrame> frames = default;
    while (len(x) != 0) {
        if (len(x) < 4 + 8 + 4) {
            return (default, errors.New(truncatedHeaderˢ));
        }
        @string magic = magicᶜ;
        if (((sstring)(x[..(int)(len(magic))])) != magic) {
            return (default, errors.New(badMagicˢ));
        }
        x = x[(int)(len(magic))..];
        var time = binary.BigEndian.Uint64(x);
        x = x[8..];
        var dlen = binary.BigEndian.Uint32(x);
        x = x[4..];
        @string data = (@string)(x.slice(0, (nint)(dlen)));
        x = x.slice((nint)(dlen));
        frames = append(framesᴷ, frames, new fakeTimeFrame(time, data));
    }
    frames = move2heap(framesᴷ, frames);
    return (frames, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string timeTimerˢ => ref timeTimerˢᵇ;
private static readonly @string timeTimerˢᵇ = "time.Timer"u8;
internal static ref readonly @string timeTickerˢ => ref timeTickerˢᵇ;
private static readonly @string timeTickerˢᵇ = "time.Ticker"u8;

public static void TestTimeTimerType(ж<testing.T> t) {
    // runtime.timeTimer (exported for testing as TimeTimer)
    // must have time.Timer and time.Ticker as a prefix
    // (meaning those two must have the same layout).
    var runtimeTimeTimer = reflect.TypeOf(new TimeTimer());

    var runtimeTimeTimerʗ1 = runtimeTimeTimer;
    void check(@string name, reflectꓸType typ) {
        nint n1 = runtimeTimeTimerʗ1.NumField();
        nint n2 = typ.NumField();
        if (n1 != n2 + 1) {
            t.Errorf("runtime.TimeTimer has %d fields, want %d (%s has %d fields)", n1, n2 + 1, name, n2);
            return;
        }
        for (nint i = 0; i < n2; i++) {
            var f1 = runtimeTimeTimerʗ1.Field(i);
            var f2 = typ.Field(i);
            var t1 = f1.Type;
            var t2 = f2.Type;
            if (!AreEqual(t1, t2) && !(t1.Kind() == reflect.ΔUnsafePointer && t2.Kind() == reflect.Chan)) {
                t.Errorf("runtime.Timer field %s %v incompatible with %s field %s %v", f1.Name, t1, name, f2.Name, t2);
            }
            if (f1.Offset != f2.Offset) {
                t.Errorf("runtime.Timer field %s offset %d incompatible with %s field %s offset %d", f1.Name, f1.Offset, name, f2.Name, f2.Offset);
            }
        }
    }

    check(timeTimerˢ, reflect.TypeOf(new time.Timer()));
    check(timeTickerˢ, reflect.TypeOf(new time.Ticker()));
}

} // end runtime_test_package
