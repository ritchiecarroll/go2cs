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
internal static readonly @string falseˢ = "false"u8;
internal static readonly @string trueˢ = "true"u8;

internal static @string boolString(@string s) {
    if (s == "0"u8) {
        return falseˢ;
    }
    return trueˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testBoolˢ = "test_bool"u8;
internal static readonly @string boolValueˢ = "bool value"u8;
internal static readonly @string testIntˢ = "test_int"u8;
internal static readonly @string intValueˢ = "int value"u8;
internal static readonly @string testInt64ˢ = "test_int64"u8;
internal static readonly @string int64Valueˢ = "int64 value"u8;
internal static readonly @string testUintˢ = "test_uint"u8;
internal static readonly @string uintValueˢ = "uint value"u8;
internal static readonly @string testUint64ˢ = "test_uint64"u8;
internal static readonly @string uint64Valueˢ = "uint64 value"u8;
internal static readonly @string testStringˢ = "test_string"u8;
internal static readonly @string stringValueˢ = "string value"u8;
internal static readonly @string testFloat64ˢ = "test_float64"u8;
internal static readonly @string float64Valueˢ = "float64 value"u8;
internal static readonly @string testDurationˢ = "test_duration"u8;
internal static readonly @string timeDurationValueˢ = "time.Duration value"u8;
internal static readonly @string testFuncˢ = "test_func"u8;
internal static readonly @string funcValueˢ = "func value"u8;
internal static readonly @string testBoolfuncˢ = "test_boolfunc"u8;
internal static readonly @string funcˢ = "func"u8;
internal static readonly object visitBadValueˢ = (@string)"Visit: bad value"u8;
internal static readonly object forˢ = (@string)"for"u8;
internal static readonly object visitAllMissesSomeFlagsˢ = (@string)"VisitAll misses some flags"u8;
internal static readonly object visitFailsAfterSetˢ = (@string)"Visit fails after set"u8;

public static void TestEverything(ж<testing.T> Ꮡt) {
    flag_internal_test_package.ResetForTesting(default!);
    Bool(testBoolˢ, false, boolValueˢ);
    Int(testIntˢ, 0, intValueˢ);
    Int64(testInt64ˢ, 0, int64Valueˢ);
    Uint(testUintˢ, 0, uintValueˢ);
    Uint64(testUint64ˢ, 0, uint64Valueˢ);
    String(testStringˢ, "0"u8, stringValueˢ);
    Float64(testFloat64ˢ, 0D, float64Valueˢ);
    Duration(testDurationˢ, 0, timeDurationValueˢ);
    Func(testFuncˢ, funcValueˢ, (@string _) => default!);
    BoolFunc(testBoolfuncˢ, funcˢ, (@string _) => default!);
    ref var m = ref heap<map<@string, ж<flag.Flag>>>(out var Ꮡm);
    m = new map<@string, ж<flag.Flag>>();
    @string desired = "0"u8;
    var visitor = (ж<flag.Flag> f) => {
        if (len((~f).Name) > 5 && (~f).Name[0..5] == "test_") {
            Ꮡm.ValueSlot[(~f).Name] = f;
            var ok = false;
            switch (ᐧ) {
            case {} when (~f).Value.String() == desired: {
                ok = true;
                break;
            }
            case {} when (~f).Name == "test_bool"u8 && (~f).Value.String() == boolString(desired): {
                ok = true;
                break;
            }
            case {} when (~f).Name == "test_duration"u8 && (~f).Value.String() == desired + "s"u8: {
                ok = true;
                break;
            }
            case {} when (~f).Name == "test_func"u8 && (~f).Value.String() == ""u8: {
                ok = true;
                break;
            }
            case {} when (~f).Name == "test_boolfunc"u8 && (~f).Value.String() == ""u8: {
                ok = true;
                break;
            }}

            if (!ok) {
                Ꮡt.Error(visitBadValueˢ, (~f).Value.String(), forˢ, (~f).Name);
            }
        }
    };
    VisitAll(visitor);
    if (len(m) != 10) {
        Ꮡt.Error(visitAllMissesSomeFlagsˢ);
        foreach (var (k, v) in m) {
            Ꮡt.Log(k, v.Value);
        }
    }
    m = new map<@string, ж<flag.Flag>>();
    Visit(visitor);
    if (len(m) != 0) {
        Ꮡt.Errorf("Visit sees unset flags"u8);
        foreach (var (k, v) in m) {
            Ꮡt.Log(k, v.Value);
        }
    }
    // Now set all flags
    Set(testBoolˢ, trueˢ);
    Set(testIntˢ, "1"u8);
    Set(testInt64ˢ, "1"u8);
    Set(testUintˢ, "1"u8);
    Set(testUint64ˢ, "1"u8);
    Set(testStringˢ, "1"u8);
    Set(testFloat64ˢ, "1"u8);
    Set(testDurationˢ, "1s"u8);
    Set(testFuncˢ, "1"u8);
    Set(testBoolfuncˢ, ""u8);
    desired = "1"u8;
    Visit(visitor);
    if (len(m) != 10) {
        Ꮡt.Error(visitFailsAfterSetˢ);
        foreach (var (k, v) in m) {
            Ꮡt.Log(k, v.Value);
        }
    }
    // Now test they're visited in sort order.
    ref var flagNames = ref heap<slice<@string>>(out var ᏑflagNames);
    Visit((ж<flag.Flag> f) => {
        ᏑflagNames.ValueSlot = append(ᏑflagNames.ValueSlot, (~f).Name);
    });
    if (!slices.IsSorted<slice<@string>, @string>(flagNames)) {
        Ꮡt.Errorf("flag names not sorted: %v"u8, flagNames);
    }
}

public static void TestGet(ж<testing.T> Ꮡt) {
    flag_internal_test_package.ResetForTesting(default!);
    Bool(testBoolˢ, true, boolValueˢ);
    Int(testIntˢ, 1, intValueˢ);
    Int64(testInt64ˢ, 2, int64Valueˢ);
    Uint(testUintˢ, 3, uintValueˢ);
    Uint64(testUint64ˢ, 4, uint64Valueˢ);
    String(testStringˢ, "5"u8, stringValueˢ);
    Float64(testFloat64ˢ, 6D, float64Valueˢ);
    Duration(testDurationˢ, 7, timeDurationValueˢ);
    var visitor = (ж<flag.Flag> f) => {
        if (len((~f).Name) > 5 && (~f).Name[0..5] == "test_") {
            var (g, ok) = (~f).Value._<Getter>(ᐧ);
            if (!ok) {
                Ꮡt.Errorf("Visit: value does not satisfy Getter: %T"u8, (~f).Value);
                return;
            }
            var exprᴛ1 = (~f).Name;
            if (exprᴛ1 == "test_bool"u8) {
                ok = AreEqual(g.Get(), true);
            }
            else if (exprᴛ1 == "test_int"u8) {
                ok = AreEqual(g.Get(), (nint)1);
            }
            else if (exprᴛ1 == "test_int64"u8) {
                ok = AreEqual(g.Get(), (int64)2);
            }
            else if (exprᴛ1 == "test_uint"u8) {
                ok = AreEqual(g.Get(), (nuint)3);
            }
            else if (exprᴛ1 == "test_uint64"u8) {
                ok = AreEqual(g.Get(), (uint64)4);
            }
            else if (exprᴛ1 == "test_string"u8) {
                ok = AreEqual(g.Get(), (@string)("5"));
            }
            else if (exprᴛ1 == "test_float64"u8) {
                ok = AreEqual(g.Get(), (float64)6D);
            }
            else if (exprᴛ1 == "test_duration"u8) {
                ok = AreEqual(g.Get(), ((time.Duration)7));
            }

            if (!ok) {
                Ꮡt.Errorf("Visit: bad value %T(%v) for %s"u8, g.Get(), g.Get(), (~f).Name);
            }
        }
    };
    VisitAll(visitor);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseDidNotFailForˢ = (@string)"parse did not fail for unknown flag"u8;
internal static readonly object didNotCallUsageForˢ = (@string)"did not call Usage for unknown flag"u8;

public static void TestUsage(ж<testing.T> Ꮡt) {
    var called = false;
    flag_internal_test_package.ResetForTesting(() => {
        called = true;
    });
    if (CommandLine.Parse(new @string[]{"-x"u8}.slice()) == default!) {
        Ꮡt.Error(parseDidNotFailForˢ);
    }
    if (!called) {
        Ꮡt.Error(didNotCallUsageForˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fParseTrueBeforeParseˢ = (@string)"f.Parse() = true before Parse"u8;
internal static readonly @string boolˢ = "bool"u8;
internal static readonly @string bool2ˢ = "bool2"u8;
internal static readonly @string bool2Valueˢ = "bool2 value"u8;
internal static readonly @string intˢ = "int"u8;
internal static readonly @string int64ˢ = "int64"u8;
internal static readonly @string uintˢ = "uint"u8;
internal static readonly @string uint64ˢ = "uint64"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string float64ˢ = "float64"u8;
internal static readonly @string durationˢ = "duration"u8;
internal static readonly @string oneExtraArgumentˢ = "one-extra-argument"u8;
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

internal static void testParse(ж<flag.FlagSet> Ꮡf, ж<testing.T> Ꮡt) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.Parsed()) {
        Ꮡt.Error(fParseTrueBeforeParseˢ);
    }
    var boolFlag = f.Bool(boolˢ, false, boolValueˢ);
    var bool2Flag = f.Bool(bool2ˢ, false, bool2Valueˢ);
    var intFlag = f.Int(intˢ, 0, intValueˢ);
    var int64Flag = f.Int64(int64ˢ, 0, int64Valueˢ);
    var uintFlag = f.Uint(uintˢ, 0, uintValueˢ);
    var uint64Flag = f.Uint64(uint64ˢ, 0, uint64Valueˢ);
    var stringFlag = f.String(stringˢ, "0"u8, stringValueˢ);
    var float64Flag = f.Float64(float64ˢ, 0D, float64Valueˢ);
    var durationFlag = f.Duration(durationˢ, (time.Duration)(5000000000L), timeDurationValueˢ);
    @string extra = oneExtraArgumentˢ;
    var args = new @string[]{
        "-bool"u8,
        "-bool2=true"u8,
        "--int"u8, "22"u8,
        "--int64"u8, "0x23"u8,
        "-uint"u8, "24"u8,
        "--uint64"u8, "25"u8,
        "-string"u8, "hello"u8,
        "-float64"u8, "2718e28"u8,
        "-duration"u8, "2m"u8,
        extra
    }.slice();
    {
        var err = Ꮡf.Parse(args); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    if (!f.Parsed()) {
        Ꮡt.Error(fParseFalseAfterParseˢ);
    }
    if (boolFlag.Value != true) {
        Ꮡt.Error(boolFlagShouldBeTrueIsˢ, boolFlag.Value);
    }
    if (bool2Flag.Value != true) {
        Ꮡt.Error(bool2FlagShouldBeTrueIsˢ, bool2Flag.Value);
    }
    if (intFlag.Value != 22) {
        Ꮡt.Error(intFlagShouldBe22Isˢ, intFlag.Value);
    }
    if (int64Flag.Value != 0x23) {
        Ꮡt.Error(int64FlagShouldBe0x23Isˢ, int64Flag.Value);
    }
    if (uintFlag.Value != 24) {
        Ꮡt.Error(uintFlagShouldBe24Isˢ, uintFlag.Value);
    }
    if (uint64Flag.Value != 25) {
        Ꮡt.Error(uint64FlagShouldBe25Isˢ, uint64Flag.Value);
    }
    if (stringFlag.Value != "hello"u8) {
        Ꮡt.Error(stringFlagShouldBeHelloˢ, stringFlag.Value);
    }
    if (float64Flag.Value != 2718e28D) {
        Ꮡt.Error(float64FlagShouldBeˢ, float64Flag.Value);
    }
    if (durationFlag.Value != (time.Duration)(120000000000L)) {
        Ꮡt.Error(durationFlagShouldBe2mIsˢ, durationFlag.Value);
    }
    if (len(f.Args()) != 1){
        Ꮡt.Error(expectedOneArgumentGotˢ, len(f.Args()));
    } else 
    if (f.Args()[0] != extra) {
        Ꮡt.Errorf("expected argument %q got %q"u8, extra, f.Args()[0]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badParseˢ = (@string)"bad parse"u8;

public static void TestParse(ж<testing.T> Ꮡt) {
    flag_internal_test_package.ResetForTesting(() => {
        Ꮡt.Error(badParseˢ);
    });
    testParse(CommandLine, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;

public static void TestFlagSetParse(ж<testing.T> Ꮡt) {
    testParse(NewFlagSet(testˢ, ContinueOnError), Ꮡt);
}

partial struct flagVar /*[]@string*/;

internal static @string ΔString(this ref flagVar f) {
    return fmt.Sprint(((slice<@string>)(f)));
}

internal static error ΔSet(this ref flagVar f, @string value) {
    f = append(f, value);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usageˢ = "usage"u8;
internal static readonly object expected3ArgsGotˢ = (@string)"expected 3 args; got "u8;

public static void TestUserDefined(ж<testing.T> Ꮡt) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var v = ref heap<flagVar>(out var Ꮡv);
    flags.Var(new flag_test_package.flagVarжValue(Ꮡv), "v"u8, usageˢ);
    {
        var err = Ꮡflags.Parse(new @string[]{"-v"u8, "1"u8, "-v"u8, "2"u8, "-v=3"u8}.slice()); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    if (len(v) != 3) {
        Ꮡt.Fatal(expected3ArgsGotˢ, len(v));
    }
    @string expect = "[1 2 3]"u8;
    if (v.ΔString() != expect) {
        Ꮡt.Errorf("expected value %q got %q"u8, expect, v.ΔString());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error; got none"u8;
internal static readonly @string testErrorˢ = "test error"u8;

public static void TestUserDefinedFunc(ж<testing.T> Ꮡt) {
    var flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var ss = ref heap<slice<@string>>(out var Ꮡss);
    flags.Func("v"u8, usageˢ, (@string s) => {
        Ꮡss.ValueSlot = append(Ꮡss.ValueSlot, s);
        return default!;
    });
    {
        var err = flags.Parse(new @string[]{"-v"u8, "1"u8, "-v"u8, "2"u8, "-v=3"u8}.slice()); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    if (len(ss) != 3) {
        Ꮡt.Fatal(expected3ArgsGotˢ, len(ss));
    }
    @string expect = "[1 2 3]"u8;
    {
        @string got = fmt.Sprint(ss); if (got != expect) {
            Ꮡt.Errorf("expected value %q got %q"u8, expect, got);
        }
    }
    // test usage
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Parse(new @string[]{"-h"u8}.slice());
    {
        @string usage = buf.String(); if (!strings.Contains(usage, usageˢ)) {
            Ꮡt.Errorf("usage string not included: %q"u8, usage);
        }
    }
    // test Func error
    flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    flags.Func("v"u8, usageˢ, (@string s) => fmt.Errorf("test error"u8));
    // flag not set, so no error
    {
        var err = flags.Parse(default!); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    // flag set, expect error
    {
        var err = flags.Parse(new @string[]{"-v"u8, "1"u8}.slice()); if (err == default!){
            Ꮡt.Error(expectedErrorGotNoneˢ);
        } else 
        {
            @string errMsg = err.Error(); if (!strings.Contains(errMsg, testErrorˢ)) {
                Ꮡt.Errorf(@"error should contain ""test error""; got %q"u8, errMsg);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string helpᶜ = "HELP"u8;

public static void TestUserDefinedForCommandLine(ж<testing.T> Ꮡt) {
    @string help = helpᶜ;
    @string result = default!;
    flag_internal_test_package.ResetForTesting(() => {
        result = help;
    });
    Usage();
    if (result != help) {
        Ꮡt.Fatalf("got %q; expected %q"u8, result, help);
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
    return default!;
}

internal static bool IsBoolFlag(this ref boolFlagVar b) {
    return b.count < 4;
}

public static void TestUserDefinedBool(ж<testing.T> Ꮡt) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var b = ref heap(new boolFlagVar(), out var Ꮡb);
    error err = default!;
    flags.Var(new flag_test_package.boolFlagVarжValue(Ꮡb), "b"u8, usageˢ);
    {
        err = Ꮡflags.Parse(new @string[]{"-b"u8, "-b"u8, "-b"u8, "-b=true"u8, "-b=false"u8, "-b"u8, "barg"u8, "-b"u8}.slice()); if (err != default!) {
            if (b.count < 4) {
                Ꮡt.Error(err);
            }
        }
    }
    if (b.count != 4) {
        Ꮡt.Errorf("want: %d; got: %d"u8, (nint)(4), b.count);
    }
    if (err == default!) {
        Ꮡt.Error(expectedErrorGotNoneˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bXBValueXˢ = "  -b\tX\n  -b value\n    \tX\n"u8;

public static void TestUserDefinedBoolUsage(ж<testing.T> Ꮡt) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    flags.Init(testˢ, ContinueOnError);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.bytes_BufferжWriter(Ꮡbuf));
    ref var b = ref heap(new boolFlagVar(), out var Ꮡb);
    flags.Var(new flag_test_package.boolFlagVarжValue(Ꮡb), "b"u8, "X"u8);
    b.count = 0;
    // b.IsBoolFlag() will return true and usage will look boolean.
    Ꮡflags.PrintDefaults();
    @string got = Ꮡbuf.String();
    @string want = "  -b\tX\n"u8;
    if (got != want) {
        Ꮡt.Errorf("false: want %q; got %q"u8, want, got);
    }
    b.count = 4;
    // b.IsBoolFlag() will return false and usage will look non-boolean.
    Ꮡflags.PrintDefaults();
    got = Ꮡbuf.String();
    want = bXBValueXˢ;
    if (got != want) {
        Ꮡt.Errorf("false: want %q; got %q"u8, want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownˢ = "-unknown"u8;

public static void TestSetOutput(ж<testing.T> Ꮡt) {
    ref var flags = ref heap(new flag.FlagSet(), out var Ꮡflags);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Init(testˢ, ContinueOnError);
    Ꮡflags.Parse(new @string[]{"-unknown"u8}.slice());
    {
        @string @out = buf.String(); if (!strings.Contains(@out, unknownˢ)) {
            Ꮡt.Logf("expected output mentioning unknown; got %q"u8, @out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beforeˢ = "before"u8;
internal static readonly @string afterˢ = "after"u8;

// This tests that one can reset the flags. This still works but not well, and is
// superseded by FlagSet.
public static void TestChangingArgs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        flag_internal_test_package.ResetForTesting(() => {
            Ꮡt.Fatal(badParseˢ);
        });
        var oldArgs = Δos.Args;
        var oldArgsʗ1 = oldArgs;
        defer(() => {
            Δos.Args = oldArgsʗ1;
        }, ref ᒐ);
        Δos.Args = new @string[]{"cmd"u8, "-before"u8, "subcmd"u8, "-after"u8, "args"u8}.slice();
        var before = Bool(beforeˢ, false, ""u8);
        {
            var err = CommandLine.Parse(Δos.Args[1..]); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        @string cmd = Arg(0);
        Δos.Args = Args();
        var after = Bool(afterˢ, false, ""u8);
        Parse();
        var args = Args();
        if (!before.Value || cmd != "subcmd"u8 || !after.Value || len(args) != 1 || args[0] != "args") {
            Ꮡt.Fatalf("expected true subcmd true [args] got %v %v %v %v"u8, before.Value, cmd, after.Value, args);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helpTestˢ = "help test"u8;
internal static readonly @string flagˢ = "flag"u8;
internal static readonly @string regularFlagˢ = "regular flag"u8;
internal static readonly object expectedNoErrorGotˢ = (@string)"expected no error; got "u8;
internal static readonly object flagWasNotSetByFlagˢ = (@string)"flag was not set by -flag"u8;
internal static readonly object helpCalledForRegularFlagˢ = (@string)"help called for regular flag"u8;
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static readonly object expectedErrHelpGotˢ = (@string)"expected ErrHelp; got "u8;
internal static readonly object helpWasNotCalledˢ = (@string)"help was not called"u8;
internal static readonly @string helpˢ = "help"u8;
internal static readonly @string helpFlagˢ = "help flag"u8;
internal static readonly object expectedNoErrorForˢ = (@string)"expected no error for defined -help; got "u8;
internal static readonly object helpWasCalledShouldNotˢ = (@string)"help was called; should not have been for defined help flag"u8;

// Test that -help invokes the usage message and returns ErrHelp.
public static void TestHelp(ж<testing.T> Ꮡt) {
    bool helpCalled = false;
    var fs = NewFlagSet(helpTestˢ, ContinueOnError);
    fs.Value.Usage = () => {
        helpCalled = true;
    };
    ref var flag = ref heap(new bool(), out var Ꮡflag);
    fs.BoolVar(Ꮡflag, flagˢ, false, regularFlagˢ);
    // Regular flag invocation should work
    var err = fs.Parse(new @string[]{"-flag=true"u8}.slice());
    if (err != default!) {
        Ꮡt.Fatal(expectedNoErrorGotˢ, err);
    }
    if (!flag) {
        Ꮡt.Error(flagWasNotSetByFlagˢ);
    }
    if (helpCalled) {
        Ꮡt.Error(helpCalledForRegularFlagˢ);
        helpCalled = false; // reset for next test
    }
    // Help flag should work as expected.
    err = fs.Parse(new @string[]{"-help"u8}.slice());
    if (err == default!) {
        Ꮡt.Fatal(errorExpectedˢ);
    }
    if (!AreEqual(err, ErrHelp)) {
        Ꮡt.Fatal(expectedErrHelpGotˢ, err);
    }
    if (!helpCalled) {
        Ꮡt.Fatal(helpWasNotCalledˢ);
    }
    // If we define a help flag, that should override.
    ref var help = ref heap(new bool(), out var Ꮡhelp);
    fs.BoolVar(Ꮡhelp, helpˢ, false, helpFlagˢ);
    helpCalled = false;
    err = fs.Parse(new @string[]{"-help"u8}.slice());
    if (err != default!) {
        Ꮡt.Fatal(expectedNoErrorForˢ, err);
    }
    if (helpCalled) {
        Ꮡt.Fatal(helpWasCalledShouldNotˢ);
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
    return default!;
}

internal static @string ΔString(this ref zeroPanicker f) {
    if (!f.dontPanic) {
        throw panic("panic!");
    }
    return f.v;
}

internal static readonly @string defaultOutput = """
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

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string printDefaultsTestˢ = "print defaults test"u8;
internal static readonly @string forBootstrappingAllowAnyˢ = "for bootstrapping, allow 'any' type"u8;
internal static readonly @string alongflagnameˢ = "Alongflagname"u8;
internal static readonly @string disableBoundsCheckingˢ = "disable bounds checking"u8;
internal static readonly @string aBooleanDefaultingToTrueˢ = "a boolean defaulting to true"u8;
internal static readonly @string setRelativePathForLocalˢ = "set relative `path` for local imports"u8;
internal static readonly @string issue23543ˢ = "issue 23543"u8;
internal static readonly @string aNonZeroNumberˢ = "a non-zero `number`"u8;
internal static readonly @string aFloatThatDefaultsToZeroˢ = "a float that defaults to zero"u8;
internal static readonly @string aMultilineHelpStringˢ = "a multiline\nhelp\nstring"u8;
internal static readonly @string aNonZeroIntˢ = "a non-zero int"u8;
internal static readonly @string aFlagMultilineHelpStringˢ = "a flag\nmultiline help string"u8;
internal static readonly @string aListOfStringsˢ = "a `list` of strings"u8;
internal static readonly @string anIntThatDefaultsToZeroˢ = "an int that defaults to zero"u8;
internal static readonly @string zp0ˢ = "ZP0"u8;
internal static readonly @string aFlagWhoseStringMethodˢ = "a flag whose String method panics when it is zero"u8;
internal static readonly @string zp1ˢ = "ZP1"u8;
internal static readonly @string maxTˢ = "maxT"u8;
internal static readonly @string setTimeoutForDialˢ = "set `timeout` for dial"u8;

public static void TestPrintDefaults(ж<testing.T> Ꮡt) {
    var fs = NewFlagSet(printDefaultsTestˢ, ContinueOnError);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fs.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    fs.Bool("A"u8, false, forBootstrappingAllowAnyˢ);
    fs.Bool(alongflagnameˢ, false, disableBoundsCheckingˢ);
    fs.Bool("C"u8, true, aBooleanDefaultingToTrueˢ);
    fs.String("D"u8, ""u8, setRelativePathForLocalˢ);
    fs.String("E"u8, "0"u8, issue23543ˢ);
    fs.Float64("F"u8, 2.7D, aNonZeroNumberˢ);
    fs.Float64("G"u8, 0D, aFloatThatDefaultsToZeroˢ);
    fs.String("M"u8, ""u8, aMultilineHelpStringˢ);
    fs.Int("N"u8, 27, aNonZeroIntˢ);
    fs.Bool("O"u8, true, aFlagMultilineHelpStringˢ);
    fs.Var(new flag_test_package.flagVarжValue(Ꮡ(new flagVar(new @string[]{"a"u8, "b"u8}.slice()))), "V"u8, aListOfStringsˢ);
    fs.Int("Z"u8, 0, anIntThatDefaultsToZeroˢ);
    fs.Var(new flag_test_package.zeroPanickerжValue(Ꮡ(new zeroPanicker(true, ""u8))), zp0ˢ, aFlagWhoseStringMethodˢ);
    fs.Var(new flag_test_package.zeroPanickerжValue(Ꮡ(new zeroPanicker(true, "something"u8))), zp1ˢ, aFlagWhoseStringMethodˢ);
    fs.Duration(maxTˢ, 0, setTimeoutForDialˢ);
    fs.PrintDefaults();
    @string got = buf.String();
    if (got != defaultOutput) {
        Ꮡt.Errorf("got:\n%q\nwant:\n%q"u8, got, defaultOutput);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedSuccessSettingˢ = (@string)"unexpected success setting Int"u8;
internal static readonly object unexpectedSuccessSettingˢ2 = (@string)"unexpected success setting Uint"u8;

// Issue 19230: validate range of Int and Uint flag values.
public static void TestIntFlagOverflow(ж<testing.T> Ꮡt) {
    if (strconv.IntSize != 32) {
        return;
    }
    flag_internal_test_package.ResetForTesting(default!);
    Int("i"u8, 0, ""u8);
    Uint("u"u8, 0, ""u8);
    {
        var err = Set("i"u8, "2147483648"u8); if (err == default!) {
            Ꮡt.Error(unexpectedSuccessSettingˢ);
        }
    }
    {
        var err = Set("u"u8, "4294967296"u8); if (err == default!) {
            Ꮡt.Error(unexpectedSuccessSettingˢ2);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "flag provided but not defined: -i\nUsage of app:\n"u8;

// Issue 20998: Usage should respect CommandLine.output.
public static void TestUsageOutput(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        flag_internal_test_package.ResetForTesting(flag_internal_test_package.DefaultUsage);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        CommandLine.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
        defer((slice<@string> old) => {
            Δos.Args = old;
        }, Δos.Args, ref ᒐ);
        Δos.Args = new @string[]{"app"u8, "-i=1"u8, "-unknown"u8}.slice();
        Parse();
        @string want = wantᶜ;
        {
            @string got = buf.String(); if (got != want) {
                Ꮡt.Errorf("output = %q; want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string flagSetˢ = "flag set"u8;
internal static readonly @string gopherˢ = "gopher"u8;

public static void TestGetters(ж<testing.T> Ꮡt) {
    @string expectedName = flagSetˢ;
    flagꓸErrorHandling expectedErrorHandling = ContinueOnError;
    var expectedOutput = ((Δio.Writer)new Δos.FileжWriter(Δos.Stderr));
    var fs = NewFlagSet(expectedName, expectedErrorHandling);
    if (fs.Name() != expectedName) {
        Ꮡt.Errorf("unexpected name: got %s, expected %s"u8, fs.Name(), expectedName);
    }
    if (fs.ErrorHandling() != expectedErrorHandling) {
        Ꮡt.Errorf("unexpected ErrorHandling: got %d, expected %d"u8, fs.ErrorHandling(), expectedErrorHandling);
    }
    if (!AreEqual(fs.Output(), expectedOutput)) {
        Ꮡt.Errorf("unexpected output: got %#v, expected %#v"u8, fs.Output(), expectedOutput);
    }
    expectedName = gopherˢ;
    expectedErrorHandling = ExitOnError;
    expectedOutput = new Δos.FileжWriter(Δos.Stdout);
    fs.Init(expectedName, expectedErrorHandling);
    fs.SetOutput(expectedOutput);
    if (fs.Name() != expectedName) {
        Ꮡt.Errorf("unexpected name: got %s, expected %s"u8, fs.Name(), expectedName);
    }
    if (fs.ErrorHandling() != expectedErrorHandling) {
        Ꮡt.Errorf("unexpected ErrorHandling: got %d, expected %d"u8, fs.ErrorHandling(), expectedErrorHandling);
    }
    if (!AreEqual(fs.Output(), expectedOutput)) {
        Ꮡt.Errorf("unexpected output: got %v, expected %v"u8, fs.Output(), expectedOutput);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseErrorTestˢ = "parse error test"u8;
internal static readonly @string invalidˢ = "invalid"u8;
internal static readonly @string parseErrorˢ = "parse error"u8;

public static void TestParseError(ж<testing.T> Ꮡt) {
    foreach (var (_, typ) in new @string[]{"bool"u8, "int"u8, "int64"u8, "uint"u8, "uint64"u8, "float64"u8, "duration"u8}.slice()) {
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
        var args = new @string[]{"-"u8 + typ + "=x"u8}.slice();
        var err = fs.Parse(args); // x is not a valid setting for any flag.
        if (err == default!) {
            Ꮡt.Errorf("Parse(%q)=%v; expected parse error"u8, args, err);
            continue;
        }
        if (!strings.Contains(err.Error(), invalidˢ) || !strings.Contains(err.Error(), parseErrorˢ)) {
            Ꮡt.Errorf("Parse(%q)=%v; expected parse error"u8, args, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueOutOfRangeˢ = "value out of range"u8;

public static void TestRangeError(ж<testing.T> Ꮡt) {
    var bad = new @string[]{
        "-int=123456789012345678901"u8,
        "-int64=123456789012345678901"u8,
        "-uint=123456789012345678901"u8,
        "-uint64=123456789012345678901"u8,
        "-float64=1e1000"u8
    }.slice();
    foreach (var (_, arg) in bad) {
        var fs = NewFlagSet(parseErrorTestˢ, ContinueOnError);
        fs.SetOutput(Δio.Discard);
        _ = fs.Int(intˢ, 0, ""u8);
        _ = fs.Int64(int64ˢ, 0, ""u8);
        _ = fs.Uint(uintˢ, 0, ""u8);
        _ = fs.Uint64(uint64ˢ, 0, ""u8);
        _ = fs.Float64(float64ˢ, 0D, ""u8);
        // Strings cannot give errors, and bools and durations do not return strconv.NumError.
        var err = fs.Parse(new @string[]{arg}.slice());
        if (err == default!) {
            Ꮡt.Errorf("Parse(%q)=%v; expected range error"u8, arg, err);
            continue;
        }
        if (!strings.Contains(err.Error(), invalidˢ) || !strings.Contains(err.Error(), valueOutOfRangeˢ)) {
            Ꮡt.Errorf("Parse(%q)=%v; expected range error"u8, arg, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goChildFlagˢ = "GO_CHILD_FLAG"u8;
internal static readonly @string goChildFlagHandleˢ = "GO_CHILD_FLAG_HANDLE"u8;
internal static readonly @string testRunTestExitCodeˢ = "-test.run=^TestExitCode$"u8;

internal partial struct TestExitCode_tests /*dyn*/ {
    internal @string flag;
    internal @string flagHandle;
    internal nint expectExit;
}

public static void TestExitCode(ж<testing.T> Ꮡt) {
    testenv.MustHaveExec(new flag_test_package.testing_TжTB(Ꮡt));
    nint magic = 123;
    if (Δos.Getenv(goChildFlagˢ) != ""u8) {
        var fs = NewFlagSet(testˢ, ExitOnError);
        if (Δos.Getenv(goChildFlagHandleˢ) != ""u8) {
            ref var b = ref heap(new bool(), out var Ꮡb);
            fs.BoolVar(Ꮡb, Δos.Getenv(goChildFlagHandleˢ), false, ""u8);
        }
        fs.Parse(new @string[]{Δos.Getenv(goChildFlagˢ)}.slice());
        Δos.Exit(magic);
    }
    var tests = new TestExitCode_tests[]{
        new(
            flag: "-h"u8,
            expectExit: 0
        ),
        new(
            flag: "-help"u8,
            expectExit: 0
        ),
        new(
            flag: "-undefined"u8,
            expectExit: 2
        ),
        new(
            flag: "-h"u8,
            flagHandle: "h"u8,
            expectExit: magic
        ),
        new(
            flag: "-help"u8,
            flagHandle: "help"u8,
            expectExit: magic
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        var test = vᴛ1;

        var cmd = exec.Command(Δos.Args[0], testRunTestExitCodeˢ);
        cmd.Value.Env = append(
            Δos.Environ(),
            "GO_CHILD_FLAG="u8 + test.flag,
            "GO_CHILD_FLAG_HANDLE=" + test.flagHandle);
        cmd.Run();
        nint got = (~cmd).ProcessState.ExitCode();
        // ExitCode is either 0 or 1 on Plan 9.
        if (Δruntime.GOOS == "plan9"u8 && test.expectExit != 0) {
            test.expectExit = 1;
        }
        if (got != test.expectExit) {
            Ꮡt.Errorf("unexpected exit code for test case %+v \n: got %d, expect %d"u8,
                test, got, test.expectExit);
        }
    }
}

internal static void mustPanic(ж<testing.T> Ꮡt, @string testName, @string expected, Action f) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                Ꮡt.Errorf("%s\n: expected panic(%q), but did not panic"u8, testName, expected);
                break;
            }
            case @string msg: {
                {
                    var (ok, _) = Δregexp.MatchString(expected, msg); if (!ok) {
                        Ꮡt.Errorf("%s\n: expected panic(%q), but got panic(%q)"u8, testName, expected, msg);
                    }
                }
                break;
            }
            default: {
                var msg = switchᴛ1;
                Ꮡt.Errorf("%s\n: expected panic(%q), but got panic(%T%v)"u8, testName, expected, msg, msg);
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

public static void TestInvalidFlags(ж<testing.T> Ꮡt) {
    var tests = new TestInvalidFlags_tests[]{
        new(
            flag: "-foo"u8,
            errorMsg: "flag \"-foo\" begins with -"u8
        ),
        new(
            flag: "foo=bar"u8,
            errorMsg: "flag \"foo=bar\" contains ="u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestInvalidFlags_tests(), out var Ꮡtest);
        test = vᴛ1;

        @string testName = fmt.Sprintf("FlagSet.Var(&v, %q, \"\")"u8, test.flag);
        var fs = NewFlagSet(""u8, ContinueOnError);
        var buf = Ꮡ(new strings.Builder(nil));
        fs.SetOutput(new flag_test_package.strings_BuilderжWriter(buf));
        var fsʗ1 = fs;
        var testʗ1 = test;
        mustPanic(Ꮡt, testName, test.errorMsg, () => {
            ref var v = ref heap<flagVar>(out var Ꮡv);
            fsʗ1.Var(new flag_test_package.flagVarжValue(Ꮡv), testʗ1.flag, ""u8);
        });
        {
            @string msg = test.errorMsg + "\n"u8; if (msg != buf.String()) {
                Ꮡt.Errorf("%s\n: unexpected output: expected %q, bug got %q"u8, testName, msg, buf.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

internal partial struct TestRedefinedFlags_tests /*dyn*/ {
    internal @string flagSetName;
    internal @string errorMsg;
}

public static void TestRedefinedFlags(ж<testing.T> Ꮡt) {
    var tests = new TestRedefinedFlags_tests[]{
        new(
            flagSetName: ""u8,
            errorMsg: "flag redefined: foo"u8
        ),
        new(
            flagSetName: "fs"u8,
            errorMsg: "fs flag redefined: foo"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        @string testName = fmt.Sprintf("flag redefined in FlagSet(%q)"u8, test.flagSetName);
        var fs = NewFlagSet(test.flagSetName, ContinueOnError);
        var buf = Ꮡ(new strings.Builder(nil));
        fs.SetOutput(new flag_test_package.strings_BuilderжWriter(buf));
        ref var v = ref heap<flagVar>(out var Ꮡv);
        fs.Var(new flag_test_package.flagVarжValue(Ꮡv), fooˢ, ""u8);
        var fsʗ1 = fs;
        mustPanic(Ꮡt, testName, test.errorMsg, () => {
            fsʗ1.Var(new flag_test_package.flagVarжValue(Ꮡv), fooˢ, ""u8);
        });
        {
            @string msg = test.errorMsg + "\n"u8; if (msg != buf.String()) {
                Ꮡt.Errorf("%s\n: unexpected output: expected %q, bug got %q"u8, testName, msg, buf.OrTypedNil());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ2 = "[true]"u8;
internal static readonly object gotErrNilWantErrNilˢ = (@string)"got err == nil; want err != nil"u8;

public static void TestUserDefinedBoolFunc(ж<testing.T> Ꮡt) {
    var flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    ref var ss = ref heap<slice<@string>>(out var Ꮡss);
    flags.BoolFunc("v"u8, usageˢ, (@string s) => {
        Ꮡss.ValueSlot = append(Ꮡss.ValueSlot, s);
        return default!;
    });
    {
        var err = flags.Parse(new @string[]{"-v"u8, ""u8, "-v"u8, "1"u8, "-v=2"u8}.slice()); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    if (len(ss) != 1) {
        Ꮡt.Fatalf("got %d args; want 1 arg"u8, len(ss));
    }
    @string want = trueˢ2;
    {
        @string got = fmt.Sprint(ss); if (got != want) {
            Ꮡt.Errorf("got %q; want %q"u8, got, want);
        }
    }
    // test usage
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    flags.SetOutput(new flag_test_package.strings_BuilderжWriter(Ꮡbuf));
    flags.Parse(new @string[]{"-h"u8}.slice());
    {
        @string usage = buf.String(); if (!strings.Contains(usage, usageˢ)) {
            Ꮡt.Errorf("usage string not included: %q"u8, usage);
        }
    }
    // test BoolFunc error
    flags = NewFlagSet(testˢ, ContinueOnError);
    flags.SetOutput(Δio.Discard);
    flags.BoolFunc("v"u8, usageˢ, (@string s) => fmt.Errorf("test error"u8));
    // flag not set, so no error
    {
        var err = flags.Parse(default!); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    // flag set, expect error
    {
        var err = flags.Parse(new @string[]{"-v"u8, ""u8}.slice()); if (err == default!){
            Ꮡt.Error(gotErrNilWantErrNilˢ);
        } else 
        {
            @string errMsg = err.Error(); if (!strings.Contains(errMsg, testErrorˢ)) {
                Ꮡt.Errorf(@"got %q; error should contain ""test error"""u8, errMsg);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string myFlagˢ = "myFlag"u8;
internal static readonly @string valueˢ = "value"u8;
internal static readonly @string defineAfterSetˢ = "DefineAfterSet"u8;
internal static readonly @string flagMyFlagSetAtFlagTestˢ = "flag myFlag set at .*/flag_test.go:.* before being defined"u8;
internal static readonly @string defaultˢ = "default"u8;

public static void TestDefineAfterSet(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var flags = NewFlagSet(testˢ, ContinueOnError);
    // Set by itself doesn't panic.
    flags.Set(myFlagˢ, valueˢ);
    // Define-after-set panics.
    var flagsʗ1 = flags;
    mustPanic(Ꮡt, defineAfterSetˢ, flagMyFlagSetAtFlagTestˢ, () => {
        _ = flagsʗ1.String(myFlagˢ, defaultˢ, usageˢ);
    });
}

} // end flag_test_package
