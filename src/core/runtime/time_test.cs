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
internal static readonly @string testfaketimeˢ = "testfaketime"u8;
internal static readonly @string tagsFaketimeˢ = "-tags=faketime"u8;

public static void TestFakeTime(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "windows"u8) {
        Ꮡt.Skip(faketimeNotSupportedOnˢ);
    }
    // Faketime is advanced in checkdead. External linking brings in cgo,
    // causing checkdead not working.
    testenv.MustInternalLink(new runtime_test_package.testing_TжTB(Ꮡt), false);
    Ꮡt.Parallel();
    var (exe, err) = buildTestProg(Ꮡt, testfaketimeˢ, tagsFaketimeˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var stdout = ref heap(new bytes.Buffer(), out var Ꮡstdout);
    ref var stderr = ref heap(new bytes.Buffer(), out var Ꮡstderr);
    var cmd = exec.Command(exe);
    cmd.Value.Stdout = new runtime_test_package.bytes_BufferжWriter(Ꮡstdout);
    cmd.Value.Stderr = new runtime_test_package.bytes_BufferжWriter(Ꮡstderr);
    err = testenv.CleanCmdEnv(cmd).Run();
    if (err != default!) {
        Ꮡt.Fatalf("exit status: %v\n%s"u8, err, Ꮡstderr.String());
    }
    Ꮡt.Logf("raw stdout: %q"u8, Ꮡstdout.String());
    Ꮡt.Logf("raw stderr: %q"u8, Ꮡstderr.String());
    var (f1, err1) = parseFakeTime(stdout.Bytes());
    if (err1 != default!) {
        Ꮡt.Fatal(err1);
    }
    var (f2, err2) = parseFakeTime(stderr.Bytes());
    if (err2 != default!) {
        Ꮡt.Fatal(err2);
    }
    UntypedInt time0 = 1257894000000000000;
    var got = new slice<fakeTimeFrame>[]{f1, f2}.slice();
    slice<slice<fakeTimeFrame>> want = new slice<fakeTimeFrame>[]{new fakeTimeFrame[]{
        new(time0 + 1, "line 2\n"u8),
        new(time0 + 1, "line 3\n"u8),
        new(/* time0 + 1e9 */ 1257894001000000000UL, "line 5\n"u8),
        new(/* time0 + 1e9 */ 1257894001000000000UL, "2009-11-10T23:00:01Z"u8)}.slice(), new fakeTimeFrame[]{
        new(time0, "line 1\n"u8),
        new(time0 + 2, "line 4\n"u8)}.slice()
    }.slice();
    if (!reflect.DeepEqual(want, got)) {
        Ꮡt.Fatalf("want %v, got %v"u8, want, got);
    }
}

[GoType] partial struct fakeTimeFrame {
    internal uint64 time;
    internal @string data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string truncatedHeaderˢ = "truncated header"u8;
internal static readonly @string badMagicˢ = "bad magic"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string magicᶜ = "\x00\x00PB"u8;

internal static (slice<fakeTimeFrame>, error) parseFakeTime(slice<byte> x) {
    slice<fakeTimeFrame> frames = default!;
    while (len(x) != 0) {
        if (len(x) < 4 + 8 + 4) {
            return (default!, errors.New(truncatedHeaderˢ));
        }
        @string magic = magicᶜ;
        if (((sstring)(x[..(int)(len(magic))])) != magic) {
            return (default!, errors.New(badMagicˢ));
        }
        x = x[(int)(len(magic))..];
        var time = binary.BigEndian.Uint64(x);
        x = x[8..];
        var dlen = binary.BigEndian.Uint32(x);
        x = x[4..];
        @string data = ((@string)(x.slice(0, (nint)(dlen))));
        x = x.slice((nint)(dlen));
        frames = append(frames, new fakeTimeFrame(time, data));
    }
    return (frames, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeTimerˢ = "time.Timer"u8;
internal static readonly @string timeTickerˢ = "time.Ticker"u8;

public static void TestTimeTimerType(ж<testing.T> Ꮡt) {
    // runtime.timeTimer (exported for testing as TimeTimer)
    // must have time.Timer and time.Ticker as a prefix
    // (meaning those two must have the same layout).
    var runtimeTimeTimer = reflect.TypeOf(new TimeTimer());
    var runtimeTimeTimerʗ1 = runtimeTimeTimer;
    void check(@string name, reflectꓸType typ) {
        nint n1 = runtimeTimeTimerʗ1.NumField();
        nint n2 = typ.NumField();
        if (n1 != n2 + 1) {
            Ꮡt.Errorf("runtime.TimeTimer has %d fields, want %d (%s has %d fields)"u8, n1, n2 + 1, name, n2);
            return;
        }
        for (nint i = 0; i < n2; i++) {
            var f1 = runtimeTimeTimerʗ1.Field(i);
            var f2 = typ.Field(i);
            var t1 = f1.Type;
            var t2 = f2.Type;
            if (!AreEqual(t1, t2) && !(t1.Kind() == reflect.ΔUnsafePointer && t2.Kind() == reflect.Chan)) {
                Ꮡt.Errorf("runtime.Timer field %s %v incompatible with %s field %s %v"u8, f1.Name, t1, name, f2.Name, t2);
            }
            if (f1.Offset != f2.Offset) {
                Ꮡt.Errorf("runtime.Timer field %s offset %d incompatible with %s field %s offset %d"u8, f1.Name, f1.Offset, name, f2.Name, f2.Offset);
            }
        }
    }
    check(timeTimerˢ, reflect.TypeOf(new time.Timer(nil)));
    check(timeTickerˢ, reflect.TypeOf(new time.Ticker(nil)));
}

} // end runtime_test_package
