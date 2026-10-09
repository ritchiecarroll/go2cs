// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// These tests are too simple.
using bytes = bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.log_package;

partial class log_internal_test_package {

public static readonly @string Rdate = @"[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]"u8;
public static readonly @string Rtime = @"[0-9][0-9]:[0-9][0-9]:[0-9][0-9]"u8;
public static readonly @string Rmicroseconds = @"\.[0-9][0-9][0-9][0-9][0-9][0-9]"u8;
public static readonly @string Rline = @"(63|65):"u8; // must update if the calls to l.Printf / l.Print below move
public static readonly @string Rlongfile = ".*/[A-Za-z0-9_\\-]+\\.go:(63|65):";
public static readonly @string Rshortfile = "[A-Za-z0-9_\\-]+\\.go:(63|65):";

internal partial struct tester {
    internal nint flag;
    internal @string prefix;
    internal @string pattern; // regexp that log output must match; we add ^ and expected_text$ always
}

// individual pieces:
// microsec implies time
// shortfile overrides longfile
// everything at once:
internal static slice<tester> tests = new tester[]{
    new(0, ""u8, ""u8),
    new(0, "XXX"u8, "XXX"u8),
    new(Ldate, ""u8, Rdate + " "),
    new(Ltime, ""u8, Rtime + " "),
    new((nint)((nint)Ltime | (nint)Lmsgprefix), "XXX"u8, Rtime + " XXX"),
    new((nint)((nint)Ltime | (nint)Lmicroseconds), ""u8, Rtime + Rmicroseconds + " "),
    new(Lmicroseconds, ""u8, Rtime + Rmicroseconds + " "),
    new(Llongfile, ""u8, Rlongfile + " "),
    new(Lshortfile, ""u8, Rshortfile + " "),
    new((nint)((nint)Llongfile | (nint)Lshortfile), ""u8, Rshortfile + " "),
    new((nint)((nint)(UntypedInt)((UntypedInt)(Ldate | Ltime) | Lmicroseconds) | (nint)Llongfile), "XXX"u8, "XXX" + Rdate + " " + Rtime + Rmicroseconds + " " + Rlongfile + " "),
    new((nint)((nint)(UntypedInt)((UntypedInt)(Ldate | Ltime) | Lmicroseconds) | (nint)Lshortfile), "XXX"u8, "XXX" + Rdate + " " + Rtime + Rmicroseconds + " " + Rshortfile + " "),
    new((nint)((nint)(UntypedInt)((UntypedInt)((UntypedInt)(Ldate | Ltime) | Lmicroseconds) | Llongfile) | (nint)Lmsgprefix), "XXX"u8, Rdate + " " + Rtime + Rmicroseconds + " " + Rlongfile + " XXX"),
    new((nint)((nint)(UntypedInt)((UntypedInt)((UntypedInt)(Ldate | Ltime) | Lmicroseconds) | Lshortfile) | (nint)Lmsgprefix), "XXX"u8, Rdate + " " + Rtime + Rmicroseconds + " " + Rshortfile + " XXX")
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloˢ = (@string)"hello"u8;
internal static readonly object worldˢ = (@string)"world"u8;
internal static readonly object patternDidNotCompileˢ = (@string)"pattern did not compile:"u8;

// Test using Println("hello", 23, "world") or using Printf("hello %d world", 23)
internal static void testPrint(ж<testing.T> Ꮡt, nint flag, @string prefix, @string pattern, bool useFormat) {
    var buf = @new<strings.Builder>();
    SetOutput(new log_test_package.strings_BuilderжWriter(buf));
    SetFlags(flag);
    SetPrefix(prefix);
    if (useFormat){
        Printf("hello %d world"u8, (nint)(23));
    } else {
        Println(helloˢ, (nint)(23), worldˢ);
    }
    @string line = buf.String();
    line = line.slice(0, len(line) - 1);
    pattern = "^"u8 + pattern + "hello 23 world$"u8;
    var (matched, err) = Δregexp.MatchString(pattern, line);
    if (err != default!) {
        Ꮡt.Fatal(patternDidNotCompileˢ, err);
    }
    if (!matched) {
        Ꮡt.Errorf("log output should match %q is %q"u8, pattern, line);
    }
    SetOutput(new os.FileжWriter(os.Stderr));
}

public static void TestDefault(ж<testing.T> Ꮡt) {
    {
        var got = Default(); if (got != std) {
            Ꮡt.Errorf("Default [%p] should be std [%p]"u8, got.OrTypedNil(), std.OrTypedNil());
        }
    }
}

public static void TestAll(ж<testing.T> Ꮡt) {
    foreach (var (_, testcase) in tests) {
        testPrint(Ꮡt, testcase.flag, testcase.prefix, testcase.pattern, false);
        testPrint(Ꮡt, testcase.flag, testcase.prefix, testcase.pattern, true);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testStringᶜ = "test"u8;

public static void TestOutput(ж<testing.T> Ꮡt) {
    @string testString = testStringᶜ;
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var l = New(new log_test_package.strings_BuilderжWriter(Ꮡb), ""u8, 0);
    l.Println(testString);
    {
        @string expect = testString + "\n"; if (b.String() != expect) {
            Ꮡt.Errorf("log output should match %q is %q"u8, expect, b.String());
        }
    }
}

public static void TestNonNewLogger(ж<testing.T> Ꮡt) {
    ref var l = ref heap(new global::go.log_package.Logger(), out var Ꮡl);
    Ꮡl.SetOutput(new log_test_package.bytes_BufferжWriter(@new<bytes.Buffer>())); // minimal work to initialize a Logger
    Ꮡl.Print(helloˢ);
}

public static partial void TestOutputRace(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var l = New(new log_test_package.bytes_BufferжWriter(Ꮡb), ""u8, 0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(100);
    for (nint i = 0; i < 100; i++) {
        var lʗ1 = l;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                lʗ1.SetFlags(0);
                lʗ1.Output(0, ""u8);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "Test:"u8;
internal static readonly @string realityˢ = "Reality:"u8;
internal static readonly object messageDidNotMatchˢ = (@string)"message did not match pattern"u8;

public static void TestFlagAndPrefixSetting(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var l = New(new log_test_package.bytes_BufferжWriter(Ꮡb), testˢ, LstdFlags);
    nint f = l.Flags();
    if (f != LstdFlags) {
        Ꮡt.Errorf("Flags 1: expected %x got %x"u8, (nint)(LstdFlags), f);
    }
    l.SetFlags((nint)(f | (nint)Lmicroseconds));
    f = l.Flags();
    if (f != (nint)((nint)LstdFlags | (nint)Lmicroseconds)) {
        Ꮡt.Errorf("Flags 2: expected %x got %x"u8, (nint)((nint)LstdFlags | (nint)Lmicroseconds), f);
    }
    @string p = l.Prefix();
    if (p != "Test:"u8) {
        Ꮡt.Errorf(@"Prefix: expected ""Test:"" got %q"u8, p);
    }
    l.SetPrefix(realityˢ);
    p = l.Prefix();
    if (p != "Reality:"u8) {
        Ꮡt.Errorf(@"Prefix: expected ""Reality:"" got %q"u8, p);
    }
    // Verify a log message looks right, with our prefix and microseconds present.
    l.Print(helloˢ);
    @string pattern = "^Reality:" + Rdate + " " + Rtime + Rmicroseconds + " hello\n";
    var (matched, err) = Δregexp.Match(pattern, b.Bytes());
    if (err != default!) {
        Ꮡt.Fatalf("pattern %q did not compile: %s"u8, pattern, err);
    }
    if (!matched) {
        Ꮡt.Error(messageDidNotMatchˢ);
    }
    // Ensure that a newline is added only if the buffer lacks a newline suffix.
    b.Reset();
    l.SetFlags(0);
    l.SetPrefix("\n"u8);
    l.Output(0, ""u8);
    {
        @string got = Ꮡb.String(); if (got != "\n"u8) {
            Ꮡt.Errorf("message mismatch:\ngot  %q\nwant %q"u8, got, (@string)"\n"u8);
        }
    }
}

public static void TestUTCFlag(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var l = New(new log_test_package.strings_BuilderжWriter(Ꮡb), testˢ, LstdFlags);
    l.SetFlags((nint)((nint)(UntypedInt)(Ldate | Ltime) | (nint)LUTC));
    // Verify a log message looks right in the right time zone. Quantize to the second only.
    var now = time.Now().UTC();
    l.Print(helloˢ);
    @string want = fmt.Sprintf("Test:%d/%.2d/%.2d %.2d:%.2d:%.2d hello\n"u8,
        now.Year(), now.Month(), now.Day(), now.Hour(), now.Minute(), now.Second());
    @string got = b.String();
    if (got == want) {
        return;
    }
    // It's possible we crossed a second boundary between getting now and logging,
    // so add a second and try again. This should very nearly always work.
    now = now.Add(time.ΔSecond);
    want = fmt.Sprintf("Test:%d/%.2d/%.2d %.2d:%.2d:%.2d hello\n"u8,
        now.Year(), now.Month(), now.Day(), now.Hour(), now.Minute(), now.Second());
    if (got == want) {
        return;
    }
    Ꮡt.Errorf("got %q; want %q"u8, got, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string headerˢ = "Header:"u8;
internal static readonly object nonEmptyˢ = (@string)"non-empty"u8;
internal static readonly @string headerˢ2 = "Header"u8;

public static void TestEmptyPrintCreatesLine(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var l = New(new log_test_package.strings_BuilderжWriter(Ꮡb), headerˢ, LstdFlags);
    l.Print();
    l.Println(nonEmptyˢ);
    @string output = b.String();
    {
        nint n = strings.Count(output, headerˢ2); if (n != 2) {
            Ꮡt.Errorf("expected 2 headers, got %d"u8, n);
        }
    }
    {
        nint n = strings.Count(output, "\n"u8); if (n != 2) {
            Ꮡt.Errorf("expected 2 lines, got %d"u8, n);
        }
    }
}

public static void TestDiscard(ж<testing.T> Ꮡt) {
    var l = New(Δio.Discard, ""u8, 0);
    @string s = strings.Repeat("a"u8, 102400);
    var lʗ1 = l;
    var c = testing.AllocsPerRun(100, [MethodImpl(MethodImplOptions.NoInlining)] () => {
        lʗ1.Printf("%s"u8, s);
    });
    // One allocation for slice passed to Printf,
    // but none for formatting of long string.
    if (c > 1D) {
        Ꮡt.Errorf("got %v allocs, want at most 1"u8, c);
    }
}

public static void BenchmarkItoa(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var dst = ref heap<slice<byte>>(out var Ꮡdst);
    dst = new slice<byte>(0, 64);
    for (nint i = 0; i < b.N; i++) {
        dst = dst[0..0];
        itoa(ref dst, 2015, 4); // year
        itoa(ref dst, 1, 2); // month
        itoa(ref dst, 30, 2); // day
        itoa(ref dst, 12, 2); // hour
        itoa(ref dst, 56, 2); // minute
        itoa(ref dst, 0, 2); // second
        itoa(ref dst, 987654, 6); // microsecond
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testStringᶜ1 = "test"u8;

public static void BenchmarkPrintln(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string testString = testStringᶜ1;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var l = New(new log_test_package.bytes_BufferжWriter(Ꮡbuf), ""u8, LstdFlags);
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        l.Println(testString);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string testStringᶜ2 = "test"u8;

public static void BenchmarkPrintlnNoFlags(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string testString = testStringᶜ2;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var l = New(new log_test_package.bytes_BufferжWriter(Ꮡbuf), ""u8, 0);
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        l.Println(testString);
    }
}

// discard is identical to io.Discard,
// but copied here to avoid the io.Discard optimization in Logger.
internal partial struct discard {
}

internal static (nint, error) Write(this discard _, slice<byte> p) {
    return (len(p), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string prefixˢ = "prefix: "u8;
internal static readonly @string helloWorldˢ = "hello, world!"u8;

public static partial void BenchmarkConcurrent(ж<testing.B> Ꮡb) {
    var l = New(new discard(nil), prefixˢ, (nint)((nint)(UntypedInt)((UntypedInt)((UntypedInt)(Ldate | Ltime) | Lmicroseconds) | Llongfile) | (nint)Lmsgprefix));
    ref var group = ref heap(new Δsync.WaitGroup(), out var Ꮡgroup);
    for (nint i = Δruntime.NumCPU(); i > 0; i--) {
        Ꮡgroup.Add(1);
        var lʗ1 = l;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                for (nint iΔ1 = 0; iΔ1 < Ꮡb.Value.N; iΔ1++) {
                    lʗ1.Output(0, helloWorldˢ);
                }
                defer(Ꮡgroup.Done, ref ᒐ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡgroup.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fizzbuzzˢ = (@string)"fizzbuzz"u8;

public static void BenchmarkDiscard(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var l = New(Δio.Discard, ""u8, (nint)((nint)LstdFlags | (nint)Lshortfile));
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        l.Printf("processing %d objects from bucket %q"u8, (nint)(1234), fizzbuzzˢ);
    }
}

} // end log_internal_test_package
