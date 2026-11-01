// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = go.errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using reflect = reflect_package;
using testing = testing_package;
using go;
using io;

partial class errors_test_package {

internal partial struct TestIs_testCases /*dyn*/ {
    internal error err;
    internal error target;
    internal bool match;
}

public static void TestIs(ж<testing.T> t) {
    var err1 = errors.New("1");
    var erra = new wrapped("wrap 2", err1);
    var errb = new wrapped("wrap 3", erra);

    var err3 = errors.New("3");

    var err1ʗ1 = err1;
    var err3ʗ1 = err3;
    var poser = Ꮡ(new poser("either 1 or 3", (error err) => AreEqual(err, err1ʗ1) || AreEqual(err, err3ʗ1)
    ));

    var testCases = slice<TestIs_testCases>([
        new(default, default, true),
        new(default, err1, false),
        new(err1, default, false),
        new(err1, err1, true),
        new(erra, err1, true),
        new(errb, err1, true),
        new(err1, err3, false),
        new(erra, err3, false),
        new(errb, err3, false),
        new(new poserжerror(poser), err1, true),
        new(new poserжerror(poser), err3, true),
        new(new poserжerror(poser), erra, false),
        new(new poserжerror(poser), errb, false),
        new(new errorUncomparable(), new errorUncomparable(), true),
        new(new errorUncomparable(), new errorUncomparableжerror(Ꮡ(new errorUncomparable())), false),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable())), new errorUncomparable(), true),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable())), new errorUncomparableжerror(Ꮡ(new errorUncomparable())), false),
        new(new errorUncomparable(), err1, false),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable())), err1, false),
        new(new multiErr(new error[]{}.slice()), err1, false),
        new(new multiErr(new error[]{err1, err3}.slice()), err1, true),
        new(new multiErr(new error[]{err3, err1}.slice()), err1, true),
        new(new multiErr(new error[]{err1, err3}.slice()), errors.New("x"), false),
        new(new multiErr(new error[]{err3, errb}.slice()), errb, true),
        new(new multiErr(new error[]{err3, errb}.slice()), erra, true),
        new(new multiErr(new error[]{err3, errb}.slice()), err1, true),
        new(new multiErr(new error[]{errb, err3}.slice()), err1, true),
        new(new multiErr(new error[]{new poserжerror(poser)}.slice()), err1, true),
        new(new multiErr(new error[]{new poserжerror(poser)}.slice()), err3, true),
        new(new multiErr(new error[]{default}.slice()), default, false)
    ]);
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestIs_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(""u8, (ж<testing.T> tΔ1) => {
            {
                var got = errors.Is(tcʗ1.err, tcʗ1.target); if (got != tcʗ1.match) {
                    tΔ1.Errorf("Is(%v, %v) = %v, want %v", tcʗ1.err, tcʗ1.target, got, tcʗ1.match);
                }
            }
        });
    }
}

partial struct poser {
    internal @string msg;
    internal Func<error, bool> f;
}

internal static ж<fs.PathError> poserPathErr = Ꮡ(new fs.PathError(Op: "poser"));

internal static @string Error(this ref poser p) {
    return p.msg;
}

internal static bool Is(this ref poser p, error err) {
    return p.f(err);
}

internal static bool As(this ж<poser> Ꮡp, any err) {
    switch (err.type()) {
    case ж<ж<poser>> x: {
        x.ValueSlot = Ꮡp;
        break;
    }
    case ж<errorT> x: {
        x.Value = new errorT("poser");
        break;
    }
    case ж<ж<fs.PathError>> x: {
        x.ValueSlot = poserPathErr;
        break;
    }
    default: {
        var x = err;
        return false;
    }}
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string errˢ => ref errˢᵇ;
private static readonly @string errˢᵇ = "err"u8;

internal partial interface TestAs_timeout /*dyn*/ {
    bool Timeout();
}

internal partial struct TestAs_testCases /*dyn*/ {
    internal error err;
    internal any target;
    internal bool match;
    internal any want; // value of target on match
}

public static void TestAs(ж<testing.T> t) {
    ref var errT = ref heap(new errorT(), out var ᏑerrT);
    ref var errP = ref heap<ж<fs.PathError>>(out var ᏑerrP);
    ref var timeout = ref heap<TestAs_timeout>(out var Ꮡtimeout);
    ref var p = ref heap<ж<poser>>(out var Ꮡp);
    var (_, errF) = os.Open(nonExistingˢ);
    var poserErr = Ꮡ(new poser("oh no", default));

    var testCases = slice<TestAs_testCases>([new(
        default,
        ᏑerrP,
        false,
        default
    ), new(
        new wrapped("pitied the fool", new errorT("T")),
        ᏑerrT,
        true,
        new errorT("T")
    ), new(
        errF,
        ᏑerrP,
        true,
        errF
    ), new(
        new errorT(),
        ᏑerrP,
        false,
        default
    ), new(
        new wrapped("wrapped", default),
        ᏑerrT,
        false,
        default
    ), new(
        new poserжerror(Ꮡ(new poser("error", default))),
        ᏑerrT,
        true,
        new errorT("poser")
    ), new(
        new poserжerror(Ꮡ(new poser("path", default))),
        ᏑerrP,
        true,
        poserPathErr.OrTypedNil()
    ), new(
        new poserжerror(poserErr),
        Ꮡp,
        true,
        poserErr.OrTypedNil()
    ), new(
        errors.New(errˢ),
        Ꮡtimeout,
        false,
        default
    ), new(
        errF,
        Ꮡtimeout,
        true,
        errF
    ), new(
        new wrapped("path error", errF),
        Ꮡtimeout,
        true,
        errF
    ), new(
        new multiErr(new error[]{}.slice()),
        ᏑerrT,
        false,
        default
    ), new(
        new multiErr(new error[]{errors.New("a"), new errorT("T")}.slice()),
        ᏑerrT,
        true,
        new errorT("T")
    ), new(
        new multiErr(new error[]{new errorT("T"), errors.New("a")}.slice()),
        ᏑerrT,
        true,
        new errorT("T")
    ), new(
        new multiErr(new error[]{new errorT("a"), new errorT("b")}.slice()),
        ᏑerrT,
        true,
        new errorT("a")
    ), new(
        new multiErr(new error[]{new multiErr(new error[]{errors.New("a"), new errorT("a")}.slice()), new errorT("b")}.slice()),
        ᏑerrT,
        true,
        new errorT("a")
    ), new(
        new multiErr(new error[]{new wrapped("path error", errF)}.slice()),
        Ꮡtimeout,
        true,
        errF
    ), new(
        new multiErr(new error[]{default}.slice()),
        ᏑerrT,
        false,
        default
    )
    ]);
    foreach (var (i, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestAs_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        @string name = fmt.Sprintf("%d:As(Errorf(..., %v), %v)"u8, i, tc.err, tc.target);
        // Clear the target pointer, in case it was set in a previous test.
        ref var rtarget = ref heap<reflectꓸValue>(reflect.ValueOf(tc.target), out var Ꮡrtarget);
        rtarget.Elem().Set(reflect.Zero(reflect.TypeOf(tc.target).Elem()));
        var rtargetʗ1 = rtarget;
        var tcʗ1 = tc;
        t.Run(name, (ж<testing.T> tΔ1) => {
            var match = errors.As(tcʗ1.err, tcʗ1.target);
            if (match != tcʗ1.match) {
                tΔ1.Fatalf("match: got %v; want %v", match, tcʗ1.match);
            }
            if (!match) {
                return;
            }
            {
                var got = rtargetʗ1.Elem().Interface(); if (!AreEqual(got, tcʗ1.want)) {
                    tΔ1.Fatalf("got %#v, want %#v", got, tcʗ1.want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string errorˢ2 => ref errorˢ2ᵇ;
private static readonly @string errorˢ2ᵇ = "error"u8;

public static void TestAsValidation(ж<testing.T> t) {
    ref var s = ref heap(new @string(), out var Ꮡs);
    var testCases = slice<any>([
        default,
        (ж<nint>)nil,
        errorˢ2,
        Ꮡs
    ]);
    var err = errors.New(errorˢ2);
    foreach (var (_, tc) in testCases) {
        var errʗ1 = err;
        var tcʗ1 = tc;
        t.Run(fmt.Sprintf("%T(%v)"u8, tc, tc), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    recover();
                }, ref ᒐ);
                if (errors.As(errʗ1, tcʗ1)) {
                    tΔ1.Errorf("As(err, %T(%v)) = true, want false", tcʗ1, tcʗ1);
                    return;
                }
                tΔ1.Errorf("As(err, %T(%v)) did not panic", tcʗ1, tcʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial interface TestAsType_timeout /*dyn*/ :
    error
{
    bool Timeout();
}

public static void TestAsType(ж<testing.T> t) {
    errorT errT = default;
    ж<fs.PathError> errP = default;
    var (_, errF) = os.Open(nonExistingˢ);
    var poserErr = Ꮡ(new poser("oh no", default));

    testAsType(t,
        default,
        errP,
        false);
    testAsType(t,
        new wrapped("pitied the fool", new errorT("T")),
        new errorT("T"),
        true);
    testAsType(t,
        errF,
        errF,
        true);
    testAsType(t,
        errT,
        errP,
        false);
    testAsType(t,
        new wrapped("wrapped", default),
        errT,
        false);
    testAsType(t,
        new poserжerror(Ꮡ(new poser("error", default))),
        new errorT("poser"),
        true);
    testAsType(t,
        new poserжerror(Ꮡ(new poser("path", default))),
        poserPathErr,
        true);
    testAsType(t,
        new poserжerror(poserErr),
        poserErr,
        true);
    testAsType(t,
        errors.New(errˢ),
        (TestAsType_timeout)default,
        false);
    testAsType(t,
        errF,
        errF._<TestAsType_timeout>(),
        true);
    testAsType(t,
        new wrapped("path error", errF),
        errF._<TestAsType_timeout>(),
        true);
    testAsType(t,
        new multiErr(new error[]{}.slice()),
        errT,
        false);
    testAsType(t,
        new multiErr(new error[]{errors.New("a"), new errorT("T")}.slice()),
        new errorT("T"),
        true);
    testAsType(t,
        new multiErr(new error[]{new errorT("T"), errors.New("a")}.slice()),
        new errorT("T"),
        true);
    testAsType(t,
        new multiErr(new error[]{new errorT("a"), new errorT("b")}.slice()),
        new errorT("a"),
        true);
    testAsType(t,
        new multiErr(new error[]{new multiErr(new error[]{errors.New("a"), new errorT("a")}.slice()), new errorT("b")}.slice()),
        new errorT("a"),
        true);
    testAsType(t,
        new multiErr(new error[]{new wrapped("path error", errF)}.slice()),
        errF._<TestAsType_timeout>(),
        true);
    testAsType(t,
        new multiErr(new error[]{default}.slice()),
        errT,
        false);
}

partial interface compError :
    error
{
}

internal static void testAsType<E>(ж<testing.T> t, error err, E want, bool wantOK) {
    t.Helper();
    @string name = fmt.Sprintf("AsType[%T](Errorf(..., %v))"u8, (want).OrTypedNilOf(), err);
    t.Run(name, (ж<testing.T> tΔ1) => {
        var (got, gotOK) = errors.AsType<E>(err);
        if (gotOK != wantOK || !AreEqual(got, want)) {
            tΔ1.Fatalf("got %v, %t; want %v, %t", (got).OrTypedNilOf(), gotOK, (want).OrTypedNilOf(), wantOK);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object isFailedˢ = (@string)"Is failed"u8;

public static void BenchmarkIs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err1 = errors.New("1");
    var err2 = new multiErr(new error[]{new multiErr(new error[]{new multiErr(new error[]{err1, new errorT("a")}.slice()), new errorT("b")}.slice())}.slice());

    for (nint i = 0; i < b.N; i++) {
        if (!errors.Is(err2, err1)) {
            Ꮡb.Fatal(isFailedˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object asFailedˢ = (@string)"As failed"u8;

public static void BenchmarkAs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err = new multiErr(new error[]{new multiErr(new error[]{new multiErr(new error[]{errors.New("a"), new errorT("a")}.slice()), new errorT("b")}.slice())}.slice());
    for (nint i = 0; i < b.N; i++) {
        ref var target = ref heap(new errorT(), out var Ꮡtarget);
        if (!errors.As(err, Ꮡtarget)) {
            Ꮡb.Fatal(asFailedˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object asTypeFailedˢ = (@string)"AsType failed"u8;

public static void BenchmarkAsType(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err = new multiErr(new error[]{new multiErr(new error[]{new multiErr(new error[]{errors.New("a"), new errorT("a")}.slice()), new errorT("b")}.slice())}.slice());
    foreach (var _ᴛ1 in range(b.N)) {
        {
            var (_, ok) = errors.AsType<errorT>(err); if (!ok) {
                Ꮡb.Fatal(asTypeFailedˢ);
            }
        }
    }
}

internal partial struct TestUnwrap_testCases /*dyn*/ {
    internal error err;
    internal error want;
}

public static void TestUnwrap(ж<testing.T> t) {
    var err1 = errors.New("1");
    var erra = new wrapped("wrap 2", err1);

    var testCases = slice<TestUnwrap_testCases>([
        new(default, default),
        new(new wrapped("wrapped", default), default),
        new(err1, default),
        new(erra, err1),
        new(new wrapped("wrap 3", erra), erra)
    ]);
    foreach (var (_, tc) in testCases) {
        {
            var got = errors.Unwrap(tc.err); if (!AreEqual(got, tc.want)) {
                t.Errorf("Unwrap(%v) = %v, want %v", tc.err, got, tc.want);
            }
        }
    }
}

partial struct errorT {
    internal @string s;
}

internal static @string Error(this errorT e) {
    return fmt.Sprintf("errorT(%s)"u8, e.s);
}

partial struct wrapped {
    internal @string msg;
    internal error err;
}

internal static @string Error(this wrapped e) {
    return e.msg;
}

internal static error Unwrap(this wrapped e) {
    return e.err;
}

partial struct multiErr /*[]error*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string multiErrorˢ => ref multiErrorˢᵇ;
private static readonly @string multiErrorˢᵇ = "multiError"u8;

internal static @string Error(this multiErr m) {
    return multiErrorˢ;
}

internal static slice<error> Unwrap(this multiErr m) {
    return (slice<error>)m;
}

partial struct errorUncomparable {
    internal slice<@string> f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string uncomparableErrorˢ => ref uncomparableErrorˢᵇ;
private static readonly @string uncomparableErrorˢᵇ = "uncomparable error"u8;

internal static @string Error(this errorUncomparable _) {
    return uncomparableErrorˢ;
}

internal static bool Is(this errorUncomparable _Δp0, error target) {
    var (_, ok) = target._<errorUncomparable>(ᐧ);
    return ok;
}

} // end errors_test_package
