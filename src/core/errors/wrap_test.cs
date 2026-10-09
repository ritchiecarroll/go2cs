// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using reflect = reflect_package;
using testing = testing_package;
using io;

partial class errors_test_package {

internal partial struct TestIs_testCases /*dyn*/ {
    internal error err;
    internal error target;
    internal bool match;
}

public static void TestIs(ж<testing.T> Ꮡt) {
    var err1 = errors.New("1"u8);
    var erra = new wrapped("wrap 2"u8, err1);
    var errb = new wrapped("wrap 3"u8, erra);
    var err3 = errors.New("3"u8);
    var err1ʗ1 = err1;
    var err3ʗ1 = err3;
    var poser = Ꮡ(new poser("either 1 or 3"u8, (error err) => AreEqual(err, err1ʗ1) || AreEqual(err, err3ʗ1)
    ));
    var testCases = new TestIs_testCases[]{
        new(default!, default!, true),
        new(default!, err1, false),
        new(err1, default!, false),
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
        new(new errorUncomparable(nil), new errorUncomparable(nil), true),
        new(new errorUncomparable(nil), new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), false),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), new errorUncomparable(nil), true),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), false),
        new(new errorUncomparable(nil), err1, false),
        new(new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), err1, false),
        new(new multiErr(new error[]{}.slice()), err1, false),
        new(new multiErr(new error[]{err1, err3}.slice()), err1, true),
        new(new multiErr(new error[]{err3, err1}.slice()), err1, true),
        new(new multiErr(new error[]{err1, err3}.slice()), errors.New("x"u8), false),
        new(new multiErr(new error[]{err3, errb}.slice()), errb, true),
        new(new multiErr(new error[]{err3, errb}.slice()), erra, true),
        new(new multiErr(new error[]{err3, errb}.slice()), err1, true),
        new(new multiErr(new error[]{errb, err3}.slice()), err1, true),
        new(new multiErr(new error[]{new poserжerror(poser)}.slice()), err1, true),
        new(new multiErr(new error[]{new poserжerror(poser)}.slice()), err3, true),
        new(new multiErr(new error[]{default!}.slice()), default!, false)
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestIs_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(""u8, (ж<testing.T> tΔ1) => {
            {
                var got = errors.Is(tcʗ1.err, tcʗ1.target); if (got != tcʗ1.match) {
                    tΔ1.Errorf("Is(%v, %v) = %v, want %v"u8, tcʗ1.err, tcʗ1.target, got, tcʗ1.match);
                }
            }
        });
    }
}

partial struct poser {
    internal @string msg;
    internal Func<error, bool> f;
}

internal static ж<fs.PathError> poserPathErr = Ꮡ(new fs.PathError(Op: "poser"u8));

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
        x.Value = new errorT("poser"u8);
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
private static readonly @string errˢ = "err"u8;

internal partial interface TestAs_timeout /*dyn*/ {
    bool Timeout();
}

internal partial struct TestAs_testCases /*dyn*/ {
    internal error err;
    internal any target;
    internal bool match;
    internal any want; // value of target on match
}

public static void TestAs(ж<testing.T> Ꮡt) {
    ref var errT = ref heap(new errorT(), out var ᏑerrT);
    ref var errP = ref heap<ж<fs.PathError>>(out var ᏑerrP);
    ref var timeout = ref heap<TestAs_timeout>(out var Ꮡtimeout);
    ref var p = ref heap<ж<poser>>(out var Ꮡp);
    var (_, errF) = os.Open(nonExistingˢ);
    var poserErr = Ꮡ(new poser("oh no"u8, default!));
    var testCases = new TestAs_testCases[]{new(
        default!,
        ᏑerrP,
        false,
        default!
    ), new(
        new wrapped("pitied the fool"u8, new errorT("T"u8)),
        ᏑerrT,
        true,
        new errorT("T"u8)
    ), new(
        errF,
        ᏑerrP,
        true,
        errF
    ), new(
        new errorT(nil),
        ᏑerrP,
        false,
        default!
    ), new(
        new wrapped("wrapped"u8, default!),
        ᏑerrT,
        false,
        default!
    ), new(
        new poserжerror(Ꮡ(new poser("error"u8, default!))),
        ᏑerrT,
        true,
        new errorT("poser"u8)
    ), new(
        new poserжerror(Ꮡ(new poser("path"u8, default!))),
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
        default!
    ), new(
        errF,
        Ꮡtimeout,
        true,
        errF
    ), new(
        new wrapped("path error"u8, errF),
        Ꮡtimeout,
        true,
        errF
    ), new(
        new multiErr(new error[]{}.slice()),
        ᏑerrT,
        false,
        default!
    ), new(
        new multiErr(new error[]{errors.New("a"u8), new errorT("T"u8)}.slice()),
        ᏑerrT,
        true,
        new errorT("T"u8)
    ), new(
        new multiErr(new error[]{new errorT("T"u8), errors.New("a"u8)}.slice()),
        ᏑerrT,
        true,
        new errorT("T"u8)
    ), new(
        new multiErr(new error[]{new errorT("a"u8), new errorT("b"u8)}.slice()),
        ᏑerrT,
        true,
        new errorT("a"u8)
    ), new(
        new multiErr(new error[]{new multiErr(new error[]{errors.New("a"u8), new errorT("a"u8)}.slice()), new errorT("b"u8)}.slice()),
        ᏑerrT,
        true,
        new errorT("a"u8)
    ), new(
        new multiErr(new error[]{new wrapped("path error"u8, errF)}.slice()),
        Ꮡtimeout,
        true,
        errF
    ), new(
        new multiErr(new error[]{default!}.slice()),
        ᏑerrT,
        false,
        default!
    )
    }.slice();
    foreach (var (i, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestAs_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        @string name = fmt.Sprintf("%d:As(Errorf(..., %v), %v)"u8, i, tc.err, tc.target);
        // Clear the target pointer, in case it was set in a previous test.
        ref var rtarget = ref heap<reflectꓸValue>(out var Ꮡrtarget);
        rtarget = reflect.ValueOf(tc.target);
        rtarget.Elem().Set(reflect.Zero(reflect.TypeOf(tc.target).Elem()));
        var rtargetʗ1 = rtarget;
        var tcʗ1 = tc;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var match = errors.As(tcʗ1.err, tcʗ1.target);
            if (match != tcʗ1.match) {
                tΔ1.Fatalf("match: got %v; want %v"u8, match, tcʗ1.match);
            }
            if (!match) {
                return;
            }
            {
                var got = rtargetʗ1.Elem().Interface(); if (!AreEqual(got, tcʗ1.want)) {
                    tΔ1.Fatalf("got %#v, want %#v"u8, got, tcʗ1.want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errorˢ = "error"u8;

public static void TestAsValidation(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new @string(), out var Ꮡs);
    var testCases = new any[]{
        default!,
        ((ж<nint>)nil),
        (@string)"error"u8,
        Ꮡs
    }.slice();
    var err = errors.New(errorˢ);
    foreach (var (_, tc) in testCases) {
        var errʗ1 = err;
        var tcʗ1 = tc;
        Ꮡt.Run(fmt.Sprintf("%T(%v)"u8, tc, tc), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    recover();
                }, ref ᒐ);
                if (errors.As(errʗ1, tcʗ1)) {
                    tΔ1.Errorf("As(err, %T(%v)) = true, want false"u8, tcʗ1, tcʗ1);
                    return;
                }
                tΔ1.Errorf("As(err, %T(%v)) did not panic"u8, tcʗ1, tcʗ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object isFailedˢ = (@string)"Is failed"u8;

public static void BenchmarkIs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err1 = errors.New("1"u8);
    var err2 = new multiErr(new error[]{new multiErr(new error[]{new multiErr(new error[]{err1, new errorT("a"u8)}.slice()), new errorT("b"u8)}.slice())}.slice());
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

    var err = new multiErr(new error[]{new multiErr(new error[]{new multiErr(new error[]{errors.New("a"u8), new errorT("a"u8)}.slice()), new errorT("b"u8)}.slice())}.slice());
    for (nint i = 0; i < b.N; i++) {
        ref var target = ref heap(new errorT(), out var Ꮡtarget);
        if (!errors.As(err, Ꮡtarget)) {
            Ꮡb.Fatal(asFailedˢ);
        }
    }
}

internal partial struct TestUnwrap_testCases /*dyn*/ {
    internal error err;
    internal error want;
}

public static void TestUnwrap(ж<testing.T> Ꮡt) {
    var err1 = errors.New("1"u8);
    var erra = new wrapped("wrap 2"u8, err1);
    var testCases = new TestUnwrap_testCases[]{
        new(default!, default!),
        new(new wrapped("wrapped"u8, default!), default!),
        new(err1, default!),
        new(erra, err1),
        new(new wrapped("wrap 3"u8, erra), erra)
    }.slice();
    foreach (var (_, tc) in testCases) {
        {
            var got = errors.Unwrap(tc.err); if (!AreEqual(got, tc.want)) {
                Ꮡt.Errorf("Unwrap(%v) = %v, want %v"u8, tc.err, got, tc.want);
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
private static readonly @string multiErrorˢ = "multiError"u8;

internal static @string Error(this multiErr m) {
    return multiErrorˢ;
}

internal static slice<error> Unwrap(this multiErr m) {
    return ((slice<error>)m);
}

partial struct errorUncomparable {
    internal slice<@string> f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string uncomparableErrorˢ = "uncomparable error"u8;

internal static @string Error(this errorUncomparable _) {
    return uncomparableErrorˢ;
}

internal static bool Is(this errorUncomparable _Δp0, error target) {
    var (_, ok) = target._<errorUncomparable>(ᐧ);
    return ok;
}

} // end errors_test_package
