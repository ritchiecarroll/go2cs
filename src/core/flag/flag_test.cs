// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using static flag_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δos = os_package;
using exec = go.os.exec_package;
using Δregexp = regexp_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using flag = flag_package;
using go.os;
using static go.flag_internal_test_package;

partial class flag_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string falseˢ => ref falseˢᵇ;
private static readonly @string falseˢᵇ = "false"u8;
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;

internal static @string boolString(@string s) {
    if (s == "0"u8) {
        return falseˢ;
    }
    return trueˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testBoolˢ => ref testBoolˢᵇ;
private static readonly @string testBoolˢᵇ = "test_bool"u8;
internal static ref readonly @string boolValueˢ => ref boolValueˢᵇ;
private static readonly @string boolValueˢᵇ = "bool value"u8;
internal static ref readonly @string testIntˢ => ref testIntˢᵇ;
private static readonly @string testIntˢᵇ = "test_int"u8;
internal static ref readonly @string intValueˢ => ref intValueˢᵇ;
private static readonly @string intValueˢᵇ = "int value"u8;
internal static ref readonly @string testInt64ˢ => ref testInt64ˢᵇ;
private static readonly @string testInt64ˢᵇ = "test_int64"u8;
internal static ref readonly @string int64Valueˢ => ref int64Valueˢᵇ;
private static readonly @string int64Valueˢᵇ = "int64 value"u8;
internal static ref readonly @string testUintˢ => ref testUintˢᵇ;
private static readonly @string testUintˢᵇ = "test_uint"u8;
internal static ref readonly @string uintValueˢ => ref uintValueˢᵇ;
private static readonly @string uintValueˢᵇ = "uint value"u8;
internal static ref readonly @string testUint64ˢ => ref testUint64ˢᵇ;
private static readonly @string testUint64ˢᵇ = "test_uint64"u8;
internal static ref readonly @string uint64Valueˢ => ref uint64Valueˢᵇ;
private static readonly @string uint64Valueˢᵇ = "uint64 value"u8;
internal static ref readonly @string testStringˢ => ref testStringˢᵇ;
private static readonly @string testStringˢᵇ = "test_string"u8;
internal static ref readonly @string stringValueˢ => ref stringValueˢᵇ;
private static readonly @string stringValueˢᵇ = "string value"u8;
internal static ref readonly @string testFloat64ˢ => ref testFloat64ˢᵇ;
private static readonly @string testFloat64ˢᵇ = "test_float64"u8;
internal static ref readonly @string float64Valueˢ => ref float64Valueˢᵇ;
private static readonly @string float64Valueˢᵇ = "float64 value"u8;
internal static ref readonly @string testDurationˢ => ref testDurationˢᵇ;
private static readonly @string testDurationˢᵇ = "test_duration"u8;
internal static ref readonly @string timeDurationValueˢ => ref timeDurationValueˢᵇ;
private static readonly @string timeDurationValueˢᵇ = "time.Duration value"u8;
internal static ref readonly @string testFuncˢ => ref testFuncˢᵇ;
private static readonly @string testFuncˢᵇ = "test_func"u8;
internal static ref readonly @string funcValueˢ => ref funcValueˢᵇ;
private static readonly @string funcValueˢᵇ = "func value"u8;
internal static ref readonly @string testBoolfuncˢ => ref testBoolfuncˢᵇ;
private static readonly @string testBoolfuncˢᵇ = "test_boolfunc"u8;
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = "func"u8;
internal static readonly object visitBadValueˢ = (@string)"Visit: bad value"u8;
internal static readonly object forˢ = (@string)"for"u8;
internal static readonly object visitAllMissesSomeFlagsˢ = (@string)"VisitAll misses some flags"u8;
internal static readonly object visitFailsAfterSetˢ = (@string)"Visit fails after set"u8;

public static void TestEverything(ж<testing.T> t) {
    flag_internal_test_package.ResetForTesting(default);
    Bool(testBoolˢ, false, boolValueˢ);
    Int(testIntˢ, 0, intValueˢ);
    Int64(testInt64ˢ, 0, int64Valueˢ);
    Uint(testUintˢ, 0, uintValueˢ);
    Uint64(testUint64ˢ, 0, uint64Valueˢ);
    String(testStringˢ, "0", stringValueˢ);
    Float64(testFloat64ˢ, 0D, float64Valueˢ);
    Duration(testDurationˢ, 0, timeDurationValueˢ);
    Func(testFuncˢ, funcValueˢ, (@string _) => default);
    BoolFunc(testBoolfuncˢ, funcˢ, (@string _) => default);

    ref var m = ref heap<map<@string, ж<flag.Flag>>>(new map<@string, ж<flag.Flag>>(), out var Ꮡm);
    @string desired = "0";
    var visitor = (ж<flag.Flag> f) => {
        if (len(f.Name) > 5 && f.Name[0..5] == "test_") {
            Ꮡm.ValueSlot[f.Name] = f;
            var ok = false;
            switch (ᐧ) {
            case {} when (~f).Value.String() == desired: {
                ok = true;
                break;
            }
            case {} when f.Name == "test_bool"u8 && (~f).Value.String() == boolString(desired): {
                ok = true;
                break;
            }
            case {} when f.Name == "test_duration"u8 && (~f).Value.String() == desired + "s"u8: {
                ok = true;
                break;
            }
            case {} when f.Name == "test_func"u8 && (~f).Value.String() == ""u8: {
                ok = true;
                break;
            }
            case {} when f.Name == "test_boolfunc"u8 && (~f).Value.String() == ""u8: {
                ok = true;
                break;
            }}
            if (!ok) {
                t.Error(visitBadValueˢ, (~f).Value.String(), forˢ, f.Name);
            }
        }
    };
    VisitAll(visitor);
    if (len(m) != 10) {
        t.Error(visitAllMissesSomeFlagsˢ);
        foreach (var (k, v) in m) {
            t.Log(k, v.Value);
        }
    }
    m = new map<@string, ж<flag.Flag>>();
    Visit(visitor);
    if (len(m) != 0) {
        t.Errorf("Visit sees unset flags");
        foreach (var (k, v) in m) {
            t.Log(k, v.Value);
        }
    }
    // Now set all flags
    Set(testBoolˢ, trueˢ);
    Set(testIntˢ, "1");
    Set(testInt64ˢ, "1");
    Set(testUintˢ, "1");
    Set(testUint64ˢ, "1");
    Set(testStringˢ, "1");
    Set(testFloat64ˢ, "1");
    Set(testDurationˢ, "1s");
    Set(testFuncˢ, "1");
    Set(testBoolfuncˢ, ""u8);
    desired = "1";
    Visit(visitor);
    if (len(m) != 10) {
        t.Error(visitFailsAfterSetˢ);
        foreach (var (k, v) in m) {
            t.Log(k, v.Value);
        }
    }
    // Now test they're visited in sort order.
    ref var flagNames = ref heap<slice<@string>>(out var ᏑflagNames);
    Visit((ж<flag.Flag> f) => {
        ᏑflagNames.ValueSlot = append(ᏑflagNames.ValueSlot, f.Name);
    });
    if (!slices.IsSorted<slice<@string>, @string>(flagNames)) {
        t.Errorf("flag names not sorted: %v", flagNames);
    }
}

public static void TestGet(ж<testing.T> t) {
    flag_internal_test_package.ResetForTesting(default);
    Bool(testBoolˢ, true, boolValueˢ);
    Int(testIntˢ, 1, intValueˢ);
    Int64(testInt64ˢ, 2, int64Valueˢ);
    Uint(testUintˢ, 3, uintValueˢ);
    Uint64(testUint64ˢ, 4, uint64Valueˢ);
    String(testStringˢ, "5", stringValueˢ);
    Float64(testFloat64ˢ, 6D, float64Valueˢ);
    Duration(testDurationˢ, 7, timeDurationValueˢ);

    var visitor = (ж<flag.Flag> f) => {
        if (len(f.Name) > 5 && f.Name[0..5] == "test_") {
            var (g, ok) = (~f).Value._<Getter>(ᐧ);
            if (!ok) {
                t.Errorf("Visit: value does not satisfy Getter: %T", (~f).Value);
                return;
            }
            var exprᴛ1 = f.Name;
            switch (ᐧ) {
            case {} when exprᴛ1 == "test_bool"u8: {
                ok = AreEqual(g.Get(), true);
                break;
            }
            case {} when exprᴛ1 == "test_int"u8: {
                ok = AreEqual(g.Get(), (nint)1);
                break;
            }
            case {} when exprᴛ1 == "test_int64"u8: {
                ok = AreEqual(g.Get(), (int64)2);
                break;
            }
            case {} when exprᴛ1 == "test_uint"u8: {
                ok = AreEqual(g.Get(), (nuint)3);
                break;
            }
            case {} when exprᴛ1 == "test_uint64"u8: {
                ok = AreEqual(g.Get(), (uint64)4);
                break;
            }
            case {} when exprᴛ1 == "test_string"u8: {
                ok = AreEqual(g.Get(), (@string)("5"));
                break;
            }
            case {} when exprᴛ1 == "test_float64"u8: {
                ok = AreEqual(g.Get(), (float64)6D);
                break;
            }
            case {} when exprᴛ1 == "test_duration"u8: {
                ok = AreEqual(g.Get(), ((time.Duration)7));
                break;
            }}
            if (!ok) {
                t.Errorf("Visit: bad value %T(%v) for %s", g.Get(), g.Get(), f.Name);
            }
        }
    };
    VisitAll(visitor);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseDidNotFailForˢ = (@string)"parse did not fail for unknown flag"u8;
internal static readonly object didNotCallUsageForˢ = (@string)"did not call Usage for unknown flag"u8;

public static void TestUsage(ж<testing.T> t) {
    var called = false;
    flag_internal_test_package.ResetForTesting(() => {
        called = true;
    });
    if (CommandLine.Parse(slice<@string>(["-x"])) == nil) {
        t.Error(parseDidNotFailForˢ);
    }
    if (!called) {
        t.Error(didNotCallUsageForˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fParseTrueBeforeParseˢ = (@string)"f.Parse() = true before Parse"u8;
internal static ref readonly @string boolˢ => ref boolˢᵇ;
private static readonly @string boolˢᵇ = "bool"u8;
internal static ref readonly @string bool2ˢ => ref bool2ˢᵇ;
private static readonly @string bool2ˢᵇ = "bool2"u8;
internal static ref readonly @string bool2Valueˢ => ref bool2Valueˢᵇ;
private static readonly @string bool2Valueˢᵇ = "bool2 value"u8;
internal static ref readonly @string intˢ => ref intˢᵇ;
private static readonly @string intˢᵇ = "int"u8;
internal static ref readonly @string int64ˢ => ref int64ˢᵇ;
private static readonly @string int64ˢᵇ = "int64"u8;
internal static ref readonly @string uintˢ => ref uintˢᵇ;
private static readonly @string uintˢᵇ = "uint"u8;
internal static ref readonly @string uint64ˢ => ref uint64ˢᵇ;
private static readonly @string uint64ˢᵇ = "uint64"u8;
internal static ref readonly @string stringˢ => ref stringˢᵇ;
private static readonly @string stringˢᵇ = "string"u8;
internal static ref readonly @string float64ˢ => ref float64ˢᵇ;
private static readonly @string float64ˢᵇ = "float64"u8;
internal static ref readonly @string durationˢ => ref durationˢᵇ;
private static readonly @string durationˢᵇ = "duration"u8;
internal static ref readonly @string oneExtraArgumentˢ => ref oneExtraArgumentˢᵇ;
private static readonly @string oneExtraArgumentˢᵇ = "one-extra-argument"u8;
internal static readonly object fParseFalseAfterParseˢ = (@string)"f.Parse() = false after Parse"u8;
internal static readonly object boolFlagShouldBeTrueIsˢ = (@string)"bool flag should be true, is "u8;
internal static readonly object bool2FlagShouldBeTrueIsˢ = (@string)"bool2 flag should be true, is "u8;
internal static readonly object intFlagShouldBe22Isˢ = (@string)"int flag should be 22, is "u8;
internal static readonly object int64FlagShouldBe0x23Isˢ = (@string)"int64 flag should be 0x23, is "u8;
internal static readonly object uintFlagShouldBe24Isˢ = (@string)"uint flag should be 24, is "u8;
internal static readonly object uint64FlagShouldBe25Isˢ = (@string)"uint64 flag should be 25, is "u8;
internal static readonly object stringFlagShouldBeHelloˢ = (@string)"string flag should be `hello`, is "u8;
internal static readonly object float64FlagShouldBeˢ = (@string)"float64 flag should be 2718e28, is "u8;
internal static readonly object durationFlagShouldBe2mIsˢ = (@string)"duration flag should be 2m, is "u8;
internal static readonly object expectedOneArgumentGotˢ = (@string)"expected one argument, got"u8;

internal static void testParse(ж<flag.FlagSet> Ꮡf, ж<testing.T> t) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.Parsed()) {
        t.Error(fParseTrueBeforeParseˢ);
    }
    var boolFlag = f.Bool(boolˢ, false, boolValueˢ);
    var bool2Flag = f.Bool(bool2ˢ, false, bool2Valueˢ);
    var intFlag = f.Int(intˢ, 0, intValueˢ);
    var int64Flag = f.Int64(int64ˢ, 0, int64Valueˢ);
    var uintFlag = f.Uint(uintˢ, 0, uintValueˢ);
    var uint64Flag = f.Uint64(uint64ˢ, 0, uint64Valueˢ);
    var stringFlag = f.String(stringˢ, "0", stringValueˢ);
    var float64Flag = f.Float64(float64ˢ, 0D, float64Valueˢ);
    var durationFlag = f.Duration(durationˢ, (time.Duration)(5000000000L), timeDurationValueˢ);
    @string extra = oneExtraArgumentˢ;
    var args = slice<@string>([
        "-bool",
        "-bool2=true",
        "--int", "22",
        "--int64", "0x23",
        "-uint", "24",
        "--uint64", "25",
        "-string", "hello",
        "-float64", "2718e28",
        "-duration", "2m",
        extra
    ]);
    {
        var err = Ꮡf.Parse(args); if (err != nil) {
            t.Fatal(err);
        }
    }
    if (!f.Parsed()) {
        t.Error(fParseFalseAfterParseˢ);
    }
    if (boolFlag.Value != true) {
        t.Error(boolFlagShouldBeTrueIsˢ, boolFlag.Value);
    }
    if (bool2Flag.Value != true) {
        t.Error(bool2FlagShouldBeTrueIsˢ, bool2Flag.Value);
    }
    if (intFlag.Value != 22) {
        t.Error(intFlagShouldBe22Isˢ, intFlag.Value);
    }
    if (int64Flag.Value != 0x23) {
        t.Error(int64FlagShouldBe0x23Isˢ, int64Flag.Value);
    }
    if (uintFlag.Value != 24) {
        t.Error(uintFlagShouldBe24Isˢ, uintFlag.Value);
    }
    if (uint64Flag.Value != 25) {
        t.Error(uint64FlagShouldBe25Isˢ, uint64Flag.Value);
    }
    if (stringFlag.Value != "hello"u8) {
        t.Error(stringFlagShouldBeHelloˢ, stringFlag.Value);
    }
    if (float64Flag.Value != 2718e28D) {
        t.Error(float64FlagShouldBeˢ, float64Flag.Value);
    }
    if (durationFlag.Value != (time.Duration)(120000000000L)) {
        t.Error(durationFlagShouldBe2mIsˢ, durationFlag.Value);
    }
    if (len(f.Args()) != 1) {
        t.Error(expectedOneArgumentGotˢ, len(f.Args()));
    } else if (f.Args()[0] != extra) {
        t.Errorf("expected argument %q got %q", extra, f.Args()[0]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badParseˢ = (@string)"bad parse"u8;

public static void TestParse(ж<testing.T> t) {
    flag_internal_test_package.ResetForTesting(() => {
        t.Error(badParseˢ);
    });
    testParse(CommandLine, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;

public static void TestFlagSetParse(ж<testing.T> t) {
    testParse(NewFlagSet(testˢ, ContinueOnError), t);
}

// Declare a user-defined flag type.
partial struct flagVar /*[]@string*/;

internal static @string ΔString(this ref flagVar f) {
    return fmt.Sprint((slice<@string>)(f));
}

internal static error ΔSet(this ref flagVar f, @string value) {
    f = append(f, value);
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string usageˢ => ref usageˢᵇ;
private static readonly @string usageˢᵇ = "usage"u8;
internal static readonly object expected3ArgsGotˢ = (@string)"expected 3 args; got "u8;

public static void TestUserDefined(ж<testing.T> t) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var v = ref heap<flagVar>(out var Ꮡv);
    flags.Var(new flag_test_package.flagVarжValue(Ꮡv), "v", usageˢ);
    {
        var err = Ꮡflags.Parse(slice<@string>(["-v", "1", "-v", "2", "-v=3"])); if (err != nil) {
            t.Error(err);
        }
    }
    if (len(v) != 3) {
        t.Fatal(expected3ArgsGotˢ, len(v));
    }
    @string expect = "[1 2 3]";
    if (v.ΔString() != expect) {
        t.Errorf("expected value %q got %q", expect, v.ΔString());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error; got none"u8;
internal static ref readonly @string testErrorˢ => ref testErrorˢᵇ;
private static readonly @string testErrorˢᵇ = "test error"u8;

public static void TestUserDefinedFunc(ж<testing.T> t) {
    var flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var ss = ref heap<slice<@string>>(out var Ꮡss);
    flags.Func("v", usageˢ, (@string s) => {
        Ꮡss.ValueSlot = append(Ꮡss.ValueSlot, s);
        return default;
    });
    {
        var err = flags.Parse(slice<@string>(["-v", "1", "-v", "2", "-v=3"])); if (err != nil) {
            t.Error(err);
        }
    }
    if (len(ss) != 3) {
        t.Fatal(expected3ArgsGotˢ, len(ss));
    }
    @string expect = "[1 2 3]";
    {
        @string got = fmt.Sprint(ss); if (got != expect) {
            t.Errorf("expected value %q got %q", expect, got);
        }
    }
    // test usage
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Parse(slice<@string>(["-h"]));
    {
        @string usage = buf.String(); if (!strings.Contains(usage, usageˢ)) {
            t.Errorf("usage string not included: %q", usage);
        }
    }
    // test Func error
    flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    flags.Func("v", usageˢ, (@string s) => fmt.Errorf("test error"u8));
    // flag not set, so no error
    {
        var err = flags.Parse(default); if (err != nil) {
            t.Error(err);
        }
    }
    // flag set, expect error
    {
        var err = flags.Parse(slice<@string>(["-v", "1"])); if (err == nil) {
            t.Error(expectedErrorGotNoneˢ);
        } else {
            @string errMsg = err.Error(); if (!strings.Contains(errMsg, testErrorˢ)) {
                t.Errorf(@"error should contain ""test error""; got %q", errMsg);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string helpᶜ => ref helpᶜᵇ;
private static readonly @string helpᶜᵇ = "HELP"u8;

public static void TestUserDefinedForCommandLine(ж<testing.T> t) {
    @string help = helpᶜ;
    @string result = default;
    flag_internal_test_package.ResetForTesting(() => {
        result = help;
    });
    Usage();
    if (result != help) {
        t.Fatalf("got %q; expected %q", result, help);
    }
}

// Declare a user-defined boolean flag type.
partial struct boolFlagVar {
    internal nint count;
}

internal static @string ΔString(this ref boolFlagVar b) {
    return fmt.Sprintf("%d"u8, b.count);
}

internal static error ΔSet(this ref boolFlagVar b, @string value) {
    if (value == "true"u8) {
        b.count++;
    }
    return default;
}

internal static bool IsBoolFlag(this ref boolFlagVar b) {
    return b.count < 4;
}

public static void TestUserDefinedBool(ж<testing.T> t) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var b = ref heap(new boolFlagVar(), out var Ꮡb);
    error err = default;
    flags.Var(new flag_test_package.boolFlagVarжValue(Ꮡb), "b", usageˢ);
    {
        err = Ꮡflags.Parse(slice<@string>(["-b", "-b", "-b", "-b=true", "-b=false", "-b", "barg", "-b"])); if (err != nil) {
            if (b.count < 4) {
                t.Error(err);
            }
        }
    }

    if (b.count != 4) {
        t.Errorf("want: %d; got: %d", (nint)4, b.count);
    }

    if (err == nil) {
        t.Error(expectedErrorGotNoneˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bXBValueXˢ => ref bXBValueXˢᵇ;
private static readonly @string bXBValueXˢᵇ = "  -b\tX\n  -b value\n    \tX\n"u8;

public static void TestUserDefinedBoolUsage(ж<testing.T> t) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.bytes_BufferжWriter(Ꮡbuf));
    ref var b = ref heap(new boolFlagVar(), out var Ꮡb);
    flags.Var(new flag_test_package.boolFlagVarжValue(Ꮡb), "b", "X");
    b.count = 0;
    // b.IsBoolFlag() will return true and usage will look boolean.
    Ꮡflags.PrintDefaults();
    @string got = Ꮡbuf.String();
    @string want = "  -b\tX\n";
    if (got != want) {
        t.Errorf("false: want %q; got %q", want, got);
    }
    b.count = 4;
    // b.IsBoolFlag() will return false and usage will look non-boolean.
    Ꮡflags.PrintDefaults();
    got = Ꮡbuf.String();
    want = bXBValueXˢ;
    if (got != want) {
        t.Errorf("false: want %q; got %q", want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownˢ => ref unknownˢᵇ;
private static readonly @string unknownˢᵇ = "-unknown"u8;

public static void TestSetOutput(ж<testing.T> t) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Init(testˢ, ContinueOnError);
    Ꮡflags.Parse(slice<@string>(["-unknown"]));
    {
        @string @out = buf.String(); if (!strings.Contains(@out, unknownˢ)) {
            t.Logf("expected output mentioning unknown; got %q", @out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string beforeˢ => ref beforeˢᵇ;
private static readonly @string beforeˢᵇ = "before"u8;
internal static ref readonly @string afterˢ => ref afterˢᵇ;
private static readonly @string afterˢᵇ = "after"u8;

// This tests that one can reset the flags. This still works but not well, and is
// superseded by FlagSet.
public static void TestChangingArgs(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        flag_internal_test_package.ResetForTesting(() => {
            t.Fatal(badParseˢ);
        });
        var oldArgs = Δos.Args;
        var oldArgsʗ1 = oldArgs;
        defer(() => {
            Δos.Args = oldArgsʗ1;
        }, ref ᒐ);
        Δos.Args = slice<@string>(["cmd", "-before", "subcmd", "-after", "args"]);
        var before = Bool(beforeˢ, false, ""u8);
        {
            var err = CommandLine.Parse(Δos.Args[1..]); if (err != nil) {
                t.Fatal(err);
            }
        }
        @string cmd = Arg(0);
        Δos.Args = Args();
        var after = Bool(afterˢ, false, ""u8);
        Parse();
        var args = Args();

        if (!before.Value || cmd != "subcmd"u8 || !after.Value || len(args) != 1 || args[0] != "args") {
            t.Fatalf("expected true subcmd true [args] got %v %v %v %v", before.Value, cmd, after.Value, args);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helpTestˢ => ref helpTestˢᵇ;
private static readonly @string helpTestˢᵇ = "help test"u8;
internal static ref readonly @string flagˢ => ref flagˢᵇ;
private static readonly @string flagˢᵇ = "flag"u8;
internal static ref readonly @string regularFlagˢ => ref regularFlagˢᵇ;
private static readonly @string regularFlagˢᵇ = "regular flag"u8;
internal static readonly object expectedNoErrorGotˢ = (@string)"expected no error; got "u8;
internal static readonly object flagWasNotSetByFlagˢ = (@string)"flag was not set by -flag"u8;
internal static readonly object helpCalledForRegularFlagˢ = (@string)"help called for regular flag"u8;
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static readonly object expectedErrHelpGotˢ = (@string)"expected ErrHelp; got "u8;
internal static readonly object helpWasNotCalledˢ = (@string)"help was not called"u8;
internal static ref readonly @string helpˢ => ref helpˢᵇ;
private static readonly @string helpˢᵇ = "help"u8;
internal static ref readonly @string helpFlagˢ => ref helpFlagˢᵇ;
private static readonly @string helpFlagˢᵇ = "help flag"u8;
internal static readonly object expectedNoErrorForˢ = (@string)"expected no error for defined -help; got "u8;
internal static readonly object helpWasCalledShouldNotˢ = (@string)"help was called; should not have been for defined help flag"u8;

// Test that -help invokes the usage message and returns ErrHelp.
public static void TestHelp(ж<testing.T> t) {
    bool helpCalled = false;
    var fs = NewFlagSet(helpTestˢ, ContinueOnError);
    fs.Usage = () => {
        helpCalled = true;
    };
    ref var flag = ref heap(new bool(), out var Ꮡflag);
    fs.BoolVar(Ꮡflag, flagˢ, false, regularFlagˢ);
    // Regular flag invocation should work
    var err = fs.Parse(slice<@string>(["-flag=true"]));
    if (err != nil) {
        t.Fatal(expectedNoErrorGotˢ, err);
    }
    if (!flag) {
        t.Error(flagWasNotSetByFlagˢ);
    }
    if (helpCalled) {
        t.Error(helpCalledForRegularFlagˢ);
        helpCalled = false; // reset for next test
    }
    // Help flag should work as expected.
    err = fs.Parse(slice<@string>(["-help"]));
    if (err == nil) {
        t.Fatal(errorExpectedˢ);
    }
    if (!AreEqual(err, ErrHelp)) {
        t.Fatal(expectedErrHelpGotˢ, err);
    }
    if (!helpCalled) {
        t.Fatal(helpWasNotCalledˢ);
    }
    // If we define a help flag, that should override.
    ref var help = ref heap(new bool(), out var Ꮡhelp);
    fs.BoolVar(Ꮡhelp, helpˢ, false, helpFlagˢ);
    helpCalled = false;
    err = fs.Parse(slice<@string>(["-help"]));
    if (err != nil) {
        t.Fatal(expectedNoErrorForˢ, err);
    }
    if (helpCalled) {
        t.Fatal(helpWasCalledShouldNotˢ);
    }
}

// zeroPanicker is a flag.Value whose String method panics if its dontPanic
// field is false.
partial struct zeroPanicker {
    internal bool dontPanic;
    internal @string v;
}

internal static error ΔSet(this ref zeroPanicker f, @string s) {
    f.v = s;
    return default;
}

internal static @string ΔString(this ref zeroPanicker f) {
    if (!f.dontPanic) {
        throw panic("panic!");
    }
    return f.v;
}

internal static ref readonly @string defaultOutput => ref defaultOutputᵇ;
private static readonly @string defaultOutputᵇ = """
  -A	for bootstrapping, allow 'any' type
  -Alongflagname
    	disable bounds checking
  -C	a boolean defaulting to true (default true)
  -D path
    	set relative path for local imports
  -E string
    	issue 23543 (default "0")
  -F number
    	a non-zero number (default 2.7)
  -G float
    	a float that defaults to zero
  -M string
    	a multiline
    	help
    	string
  -N int
    	a non-zero int (default 27)
  -O	a flag
    	multiline help string (default true)
  -V list
    	a list of strings (default [a b])
  -Z int
    	an int that defaults to zero
  -ZP0 value
    	a flag whose String method panics when it is zero
  -ZP1 value
    	a flag whose String method panics when it is zero
  -maxT timeout
    	set timeout for dial

panic calling String method on zero flag_test.zeroPanicker for flag ZP0: panic!
panic calling String method on zero flag_test.zeroPanicker for flag ZP1: panic!

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string printDefaultsTestˢ => ref printDefaultsTestˢᵇ;
private static readonly @string printDefaultsTestˢᵇ = "print defaults test"u8;
internal static ref readonly @string forBootstrappingAllowAnyˢ => ref forBootstrappingAllowAnyˢᵇ;
private static readonly @string forBootstrappingAllowAnyˢᵇ = "for bootstrapping, allow 'any' type"u8;
internal static ref readonly @string alongflagnameˢ => ref alongflagnameˢᵇ;
private static readonly @string alongflagnameˢᵇ = "Alongflagname"u8;
internal static ref readonly @string disableBoundsCheckingˢ => ref disableBoundsCheckingˢᵇ;
private static readonly @string disableBoundsCheckingˢᵇ = "disable bounds checking"u8;
internal static ref readonly @string aBooleanDefaultingToTrueˢ => ref aBooleanDefaultingToTrueˢᵇ;
private static readonly @string aBooleanDefaultingToTrueˢᵇ = "a boolean defaulting to true"u8;
internal static ref readonly @string setRelativePathForLocalˢ => ref setRelativePathForLocalˢᵇ;
private static readonly @string setRelativePathForLocalˢᵇ = "set relative `path` for local imports"u8;
internal static ref readonly @string issue23543ˢ => ref issue23543ˢᵇ;
private static readonly @string issue23543ˢᵇ = "issue 23543"u8;
internal static ref readonly @string aNonZeroNumberˢ => ref aNonZeroNumberˢᵇ;
private static readonly @string aNonZeroNumberˢᵇ = "a non-zero `number`"u8;
internal static ref readonly @string aFloatThatDefaultsToZeroˢ => ref aFloatThatDefaultsToZeroˢᵇ;
private static readonly @string aFloatThatDefaultsToZeroˢᵇ = "a float that defaults to zero"u8;
internal static ref readonly @string aMultilineHelpStringˢ => ref aMultilineHelpStringˢᵇ;
private static readonly @string aMultilineHelpStringˢᵇ = "a multiline\nhelp\nstring"u8;
internal static ref readonly @string aNonZeroIntˢ => ref aNonZeroIntˢᵇ;
private static readonly @string aNonZeroIntˢᵇ = "a non-zero int"u8;
internal static ref readonly @string aFlagMultilineHelpStringˢ => ref aFlagMultilineHelpStringˢᵇ;
private static readonly @string aFlagMultilineHelpStringˢᵇ = "a flag\nmultiline help string"u8;
internal static ref readonly @string aListOfStringsˢ => ref aListOfStringsˢᵇ;
private static readonly @string aListOfStringsˢᵇ = "a `list` of strings"u8;
internal static ref readonly @string anIntThatDefaultsToZeroˢ => ref anIntThatDefaultsToZeroˢᵇ;
private static readonly @string anIntThatDefaultsToZeroˢᵇ = "an int that defaults to zero"u8;
internal static ref readonly @string zp0ˢ => ref zp0ˢᵇ;
private static readonly @string zp0ˢᵇ = "ZP0"u8;
internal static ref readonly @string aFlagWhoseStringMethodˢ => ref aFlagWhoseStringMethodˢᵇ;
private static readonly @string aFlagWhoseStringMethodˢᵇ = "a flag whose String method panics when it is zero"u8;
internal static ref readonly @string zp1ˢ => ref zp1ˢᵇ;
private static readonly @string zp1ˢᵇ = "ZP1"u8;
internal static ref readonly @string maxTˢ => ref maxTˢᵇ;
private static readonly @string maxTˢᵇ = "maxT"u8;
internal static ref readonly @string setTimeoutForDialˢ => ref setTimeoutForDialˢᵇ;
private static readonly @string setTimeoutForDialˢᵇ = "set `timeout` for dial"u8;

public static void TestPrintDefaults(ж<testing.T> t) {
    var fs = NewFlagSet(printDefaultsTestˢ, ContinueOnError);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fs.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    fs.Bool("A", false, forBootstrappingAllowAnyˢ);
    fs.Bool(alongflagnameˢ, false, disableBoundsCheckingˢ);
    fs.Bool("C", true, aBooleanDefaultingToTrueˢ);
    fs.String("D", ""u8, setRelativePathForLocalˢ);
    fs.String("E", "0", issue23543ˢ);
    fs.Float64("F", 2.7D, aNonZeroNumberˢ);
    fs.Float64("G", 0D, aFloatThatDefaultsToZeroˢ);
    fs.String("M", ""u8, aMultilineHelpStringˢ);
    fs.Int("N", 27, aNonZeroIntˢ);
    fs.Bool("O", true, aFlagMultilineHelpStringˢ);
    fs.Var(new flag_test_package.flagVarжValue(Ꮡ(new flagVar(new @string[]{"a", "b"}.slice()))), "V", aListOfStringsˢ);
    fs.Int("Z", 0, anIntThatDefaultsToZeroˢ);
    fs.Var(new flag_test_package.zeroPanickerжValue(Ꮡ(new zeroPanicker(true, ""))), zp0ˢ, aFlagWhoseStringMethodˢ);
    fs.Var(new flag_test_package.zeroPanickerжValue(Ꮡ(new zeroPanicker(true, "something"))), zp1ˢ, aFlagWhoseStringMethodˢ);
    fs.Duration(maxTˢ, 0, setTimeoutForDialˢ);
    fs.PrintDefaults();
    @string got = buf.String();
    if (got != defaultOutput) {
        t.Errorf("got:\n%q\nwant:\n%q", got, defaultOutput);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedSuccessSettingˢ = (@string)"unexpected success setting Int"u8;
internal static readonly object unexpectedSuccessSettingˢ2 = (@string)"unexpected success setting Uint"u8;

// Issue 19230: validate range of Int and Uint flag values.
public static void TestIntFlagOverflow(ж<testing.T> t) {
    if (strconv.IntSize != 32) {
        return;
    }
    flag_internal_test_package.ResetForTesting(default);
    Int("i", 0, ""u8);
    Uint("u", 0, ""u8);
    {
        var err = Set("i", "2147483648"); if (err == nil) {
            t.Error(unexpectedSuccessSettingˢ);
        }
    }
    {
        var err = Set("u", "4294967296"); if (err == nil) {
            t.Error(unexpectedSuccessSettingˢ2);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "flag provided but not defined: -i\nUsage of app:\n"u8;

// Issue 20998: Usage should respect CommandLine.output.
public static void TestUsageOutput(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        flag_internal_test_package.ResetForTesting(flag_internal_test_package.DefaultUsage);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        CommandLine.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
        defer((slice<@string> old) => {
            Δos.Args = old;
        }, Δos.Args, ref ᒐ);
        Δos.Args = slice<@string>(["app", "-i=1", "-unknown"]);
        Parse();
        @string want = wantᶜ;
        {
            @string got = buf.String(); if (got != want) {
                t.Errorf("output = %q; want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string flagSetˢ => ref flagSetˢᵇ;
private static readonly @string flagSetˢᵇ = "flag set"u8;
internal static ref readonly @string gopherˢ => ref gopherˢᵇ;
private static readonly @string gopherˢᵇ = "gopher"u8;

public static void TestGetters(ж<testing.T> t) {
    @string expectedName = flagSetˢ;
    flagꓸErrorHandling expectedErrorHandling = ContinueOnError;
    var expectedOutput = ((Δio.Writer)new Δos.FileжWriter(Δos.Stderr));
    var fs = NewFlagSet(expectedName, expectedErrorHandling);

    if (fs.Name() != expectedName) {
        t.Errorf("unexpected name: got %s, expected %s", fs.Name(), expectedName);
    }
    if (fs.ErrorHandling() != expectedErrorHandling) {
        t.Errorf("unexpected ErrorHandling: got %d, expected %d", fs.ErrorHandling(), expectedErrorHandling);
    }
    if (!AreEqual(fs.Output(), expectedOutput)) {
        t.Errorf("unexpected output: got %#v, expected %#v", fs.Output(), expectedOutput);
    }

    expectedName = gopherˢ;
    expectedErrorHandling = ExitOnError;
    expectedOutput = new Δos.FileжWriter(Δos.Stdout);
    fs.Init(expectedName, expectedErrorHandling);
    fs.SetOutput(expectedOutput);

    if (fs.Name() != expectedName) {
        t.Errorf("unexpected name: got %s, expected %s", fs.Name(), expectedName);
    }
    if (fs.ErrorHandling() != expectedErrorHandling) {
        t.Errorf("unexpected ErrorHandling: got %d, expected %d", fs.ErrorHandling(), expectedErrorHandling);
    }
    if (!AreEqual(fs.Output(), expectedOutput)) {
        t.Errorf("unexpected output: got %v, expected %v", fs.Output(), expectedOutput);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string parseErrorTestˢ => ref parseErrorTestˢᵇ;
private static readonly @string parseErrorTestˢᵇ = "parse error test"u8;
internal static ref readonly @string invalidˢ => ref invalidˢᵇ;
private static readonly @string invalidˢᵇ = "invalid"u8;
internal static ref readonly @string parseErrorˢ => ref parseErrorˢᵇ;
private static readonly @string parseErrorˢᵇ = "parse error"u8;

public static void TestParseError(ж<testing.T> t) {
    foreach (var (_, typ) in slice<@string>(["bool", "int", "int64", "uint", "uint64", "float64", "duration"])) {
        var fs = NewFlagSet(parseErrorTestˢ, ContinueOnError);
        fs.SetOutput(Δio.Discard);
        _ = fs.Bool(boolˢ, false, ""u8);
        _ = fs.Int(intˢ, 0, ""u8);
        _ = fs.Int64(int64ˢ, 0, ""u8);
        _ = fs.Uint(uintˢ, 0, ""u8);
        _ = fs.Uint64(uint64ˢ, 0, ""u8);
        _ = fs.Float64(float64ˢ, 0D, ""u8);
        _ = fs.Duration(durationˢ, 0, ""u8);
        // Strings cannot give errors.
        var args = slice<@string>(["-"u8 + typ + "=x"u8]);
        var err = fs.Parse(args); // x is not a valid setting for any flag.
        if (err == nil) {
            t.Errorf("Parse(%q)=%v; expected parse error", args, err);
            continue;
        }
        if (!strings.Contains(err.Error(), invalidˢ) || !strings.Contains(err.Error(), parseErrorˢ)) {
            t.Errorf("Parse(%q)=%v; expected parse error", args, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string valueOutOfRangeˢ => ref valueOutOfRangeˢᵇ;
private static readonly @string valueOutOfRangeˢᵇ = "value out of range"u8;

public static void TestRangeError(ж<testing.T> t) {
    var bad = slice<@string>([
        "-int=123456789012345678901",
        "-int64=123456789012345678901",
        "-uint=123456789012345678901",
        "-uint64=123456789012345678901",
        "-float64=1e1000"
    ]);
    foreach (var (_, arg) in bad) {
        var fs = NewFlagSet(parseErrorTestˢ, ContinueOnError);
        fs.SetOutput(Δio.Discard);
        _ = fs.Int(intˢ, 0, ""u8);
        _ = fs.Int64(int64ˢ, 0, ""u8);
        _ = fs.Uint(uintˢ, 0, ""u8);
        _ = fs.Uint64(uint64ˢ, 0, ""u8);
        _ = fs.Float64(float64ˢ, 0D, ""u8);
        // Strings cannot give errors, and bools and durations do not return strconv.NumError.
        var err = fs.Parse(slice<@string>([arg]));
        if (err == nil) {
            t.Errorf("Parse(%q)=%v; expected range error", arg, err);
            continue;
        }
        if (!strings.Contains(err.Error(), invalidˢ) || !strings.Contains(err.Error(), valueOutOfRangeˢ)) {
            t.Errorf("Parse(%q)=%v; expected range error", arg, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goChildFlagˢ => ref goChildFlagˢᵇ;
private static readonly @string goChildFlagˢᵇ = "GO_CHILD_FLAG"u8;
internal static ref readonly @string goChildFlagHandleˢ => ref goChildFlagHandleˢᵇ;
private static readonly @string goChildFlagHandleˢᵇ = "GO_CHILD_FLAG_HANDLE"u8;
internal static ref readonly @string testRunTestExitCodeˢ => ref testRunTestExitCodeˢᵇ;
private static readonly @string testRunTestExitCodeˢᵇ = "-test.run=^TestExitCode$"u8;

internal partial struct TestExitCode_tests /*dyn*/ {
    internal @string flag;
    internal @string flagHandle;
    internal nint expectExit;
}

public static void TestExitCode(ж<testing.T> t) {
    testenv.MustHaveExec(new flag_test_package.testing_TжTB(t));

    nint magic = 123;
    if (Δos.Getenv(goChildFlagˢ) != ""u8) {
        var fs = NewFlagSet(testˢ, ExitOnError);
        if (Δos.Getenv(goChildFlagHandleˢ) != ""u8) {
            ref var b = ref heap(new bool(), out var Ꮡb);
            fs.BoolVar(Ꮡb, Δos.Getenv(goChildFlagHandleˢ), false, ""u8);
        }
        fs.Parse(slice<@string>([Δos.Getenv(goChildFlagˢ)]));
        Δos.Exit(magic);
    }

    var tests = slice<TestExitCode_tests>([
        new(
            flag: "-h",
            expectExit: 0
        ),
        new(
            flag: "-help",
            expectExit: 0
        ),
        new(
            flag: "-undefined",
            expectExit: 2
        ),
        new(
            flag: "-h",
            flagHandle: "h",
            expectExit: magic
        ),
        new(
            flag: "-help",
            flagHandle: "help",
            expectExit: magic
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        var test = vᴛ1;

        var cmd = exec.Command(testenv.Executable(new flag_test_package.testing_TжTB(t)), testRunTestExitCodeˢ);
        cmd.Env = append(
            Δos.Environ(),
            "GO_CHILD_FLAG="u8 + test.flag,
            "GO_CHILD_FLAG_HANDLE=" + test.flagHandle);
        cmd.Run();
        nint got = cmd.ProcessState.ExitCode();
        // ExitCode is either 0 or 1 on Plan 9.
        if (Δruntime.GOOS == "plan9"u8 && test.expectExit != 0) {
            test.expectExit = 1;
        }
        if (got != test.expectExit) {
            t.Errorf("unexpected exit code for test case %+v \n: got %d, expect %d",
                test, got, test.expectExit);
        }
    }
}

internal static void mustPanic(ж<testing.T> t, @string testName, @string expected, Action f) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                t.Errorf("%s\n: expected panic(%q), but did not panic", testName, expected);
                break;
            }
            case @string msg: {
                {
                    var (ok, _) = Δregexp.MatchString(expected, msg); if (!ok) {
                        t.Errorf("%s\n: expected panic(%q), but got panic(%q)", testName, expected, msg);
                    }
                }
                break;
            }
            default: {
                var msg = switchᴛ1;
                t.Errorf("%s\n: expected panic(%q), but got panic(%T%v)", testName, expected, msg, msg);
                break;
            }}
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestInvalidFlags_tests /*dyn*/ {
    internal @string flag;
    internal @string errorMsg;
}

public static void TestInvalidFlags(ж<testing.T> t) {
    var tests = slice<TestInvalidFlags_tests>([
        new(
            flag: "-foo",
            errorMsg: "flag \"-foo\" begins with -"
        ),
        new(
            flag: "foo=bar",
            errorMsg: "flag \"foo=bar\" contains ="
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestInvalidFlags_tests(), out var Ꮡtest);
        test = vᴛ1;

        @string testName = fmt.Sprintf("FlagSet.Var(&v, %q, \"\")"u8, test.flag);

        var fs = NewFlagSet(""u8, ContinueOnError);
        var buf = Ꮡ(new strings.Builder());
        fs.SetOutput(new flag_test_package.strings_BuilderжWriter(buf));

        var fsʗ1 = fs;
        var testʗ1 = test;
        mustPanic(t, testName, test.errorMsg, () => {
            ref var v = ref heap<flagVar>(out var Ꮡv);
            fsʗ1.Var(new flag_test_package.flagVarжValue(Ꮡv), testʗ1.flag, ""u8);
        });
        {
            @string msg = test.errorMsg + "\n"u8; if (msg != buf.String()) {
                t.Errorf("%s\n: unexpected output: expected %q, bug got %q", testName, msg, buf.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

internal partial struct TestRedefinedFlags_tests /*dyn*/ {
    internal @string flagSetName;
    internal @string errorMsg;
}

public static void TestRedefinedFlags(ж<testing.T> t) {
    var tests = slice<TestRedefinedFlags_tests>([
        new(
            flagSetName: "",
            errorMsg: "flag redefined: foo"
        ),
        new(
            flagSetName: "fs",
            errorMsg: "fs flag redefined: foo"
        )
    ]);

    foreach (var (_, test) in tests) {
        @string testName = fmt.Sprintf("flag redefined in FlagSet(%q)"u8, test.flagSetName);

        var fs = NewFlagSet(test.flagSetName, ContinueOnError);
        var buf = Ꮡ(new strings.Builder());
        fs.SetOutput(new flag_test_package.strings_BuilderжWriter(buf));

        ref var v = ref heap<flagVar>(out var Ꮡv);
        fs.Var(new flag_test_package.flagVarжValue(Ꮡv), fooˢ, ""u8);

        var fsʗ1 = fs;
        mustPanic(t, testName, test.errorMsg, () => {
            fsʗ1.Var(new flag_test_package.flagVarжValue(Ꮡv), fooˢ, ""u8);
        });
        {
            @string msg = test.errorMsg + "\n"u8; if (msg != buf.String()) {
                t.Errorf("%s\n: unexpected output: expected %q, bug got %q", testName, msg, buf.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trueˢ2 => ref trueˢ2ᵇ;
private static readonly @string trueˢ2ᵇ = "[true]"u8;
internal static readonly object gotErrNilWantErrNilˢ = (@string)"got err == nil; want err != nil"u8;

public static void TestUserDefinedBoolFunc(ж<testing.T> t) {
    var flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var ss = ref heap<slice<@string>>(out var Ꮡss);
    flags.BoolFunc("v", usageˢ, (@string s) => {
        Ꮡss.ValueSlot = append(Ꮡss.ValueSlot, s);
        return default;
    });
    {
        var err = flags.Parse(slice<@string>(["-v", "", "-v", "1", "-v=2"])); if (err != nil) {
            t.Error(err);
        }
    }
    if (len(ss) != 1) {
        t.Fatalf("got %d args; want 1 arg", len(ss));
    }
    @string want = trueˢ2;
    {
        @string got = fmt.Sprint(ss); if (got != want) {
            t.Errorf("got %q; want %q", got, want);
        }
    }
    // test usage
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Parse(slice<@string>(["-h"]));
    {
        @string usage = buf.String(); if (!strings.Contains(usage, usageˢ)) {
            t.Errorf("usage string not included: %q", usage);
        }
    }
    // test BoolFunc error
    flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    flags.BoolFunc("v", usageˢ, (@string s) => fmt.Errorf("test error"u8));
    // flag not set, so no error
    {
        var err = flags.Parse(default); if (err != nil) {
            t.Error(err);
        }
    }
    // flag set, expect error
    {
        var err = flags.Parse(slice<@string>(["-v", ""])); if (err == nil) {
            t.Error(gotErrNilWantErrNilˢ);
        } else {
            @string errMsg = err.Error(); if (!strings.Contains(errMsg, testErrorˢ)) {
                t.Errorf(@"got %q; error should contain ""test error""", errMsg);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string myFlagˢ => ref myFlagˢᵇ;
private static readonly @string myFlagˢᵇ = "myFlag"u8;
internal static ref readonly @string valueˢ => ref valueˢᵇ;
private static readonly @string valueˢᵇ = "value"u8;
internal static ref readonly @string defineAfterSetˢ => ref defineAfterSetˢᵇ;
private static readonly @string defineAfterSetˢᵇ = "DefineAfterSet"u8;
internal static ref readonly @string flagMyFlagSetAtFlagTestˢ => ref flagMyFlagSetAtFlagTestˢᵇ;
private static readonly @string flagMyFlagSetAtFlagTestˢᵇ = "flag myFlag set at .*/flag_test.go:.* before being defined"u8;
internal static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default"u8;

public static void TestDefineAfterSet(ж<testing.T> t) {
    var flags = NewFlagSet(testˢ, ContinueOnError);
    // Set by itself doesn't panic.
    flags.Set(myFlagˢ, valueˢ);

    // Define-after-set panics.
    var flagsʗ1 = flags;
    mustPanic(t, defineAfterSetˢ, flagMyFlagSetAtFlagTestˢ, () => {
        _ = flagsʗ1.String(myFlagˢ, defaultˢ, usageˢ);
    });
}

} // end flag_test_package
