// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;

partial class testing_test_package {

// The line numbering of this file is important for TestTBHelper.
internal static partial void notHelper(ж<testing.T> t, @string msg) {
    t.Error(msg);
}

internal static partial void helper(ж<testing.T> t, @string msg) {
    t.Helper();
    t.Error(msg);
}

internal static partial void notHelperCallingHelper(ж<testing.T> t, @string msg) {
    helper(t, msg);
}

internal static partial void helperCallingHelper(ж<testing.T> t, @string msg) {
    t.Helper();
    helper(t, msg);
}

internal static partial void genericHelper<G>(ж<testing.T> t, @string msg) {
    t.Helper();
    t.Error(msg);
}

internal static Action<ж<testing.T>, @string> genericIntHelper = genericHelper<nint>;

internal static void testTestHelper(ж<testing.T> t) {
    testHelper(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string subˢ => ref subˢᵇ;
private static readonly @string subˢᵇ = "sub"u8;
private static ref readonly @string genericFloat64ˢ => ref genericFloat64ˢᵇ;
private static readonly @string genericFloat64ˢᵇ = "GenericFloat64"u8;
private static ref readonly @string genericIntˢ => ref genericIntˢᵇ;
private static readonly @string genericIntˢᵇ = "GenericInt"u8;

internal static partial void testHelper(ж<testing.T> t) {
    // Check combinations of directly and indirectly
    // calling helper functions.
    notHelper(t, "0");
    helper(t, "1");
    notHelperCallingHelper(t, "2");
    helperCallingHelper(t, "3");

    // Check a function literal closing over t that uses Helper.
    void fn(@string msg) {
        t.Helper();
        t.Error(msg);
    }
    fn("4");

    t.Run(subˢ, (ж<testing.T> tΔ1) => {
        helper(tΔ1, "5");
        notHelperCallingHelper(tΔ1, "6");
        // Check that calling Helper from inside a subtest entry function
        // works as if it were in an ordinary function call.
        tΔ1.Helper();
        tΔ1.Error((@string)"7");
    });

    // Check that right caller is reported for func passed to Cleanup when
    // multiple cleanup functions have been registered.
    t.Cleanup(() => {
        t.Helper();
        t.Error((@string)"10");
    });
    t.Cleanup(() => {
        t.Helper();
        t.Error((@string)"9");
    });

    // Check that helper-ness propagates up through subtests
    // to helpers above. See https://golang.org/issue/44887.
    helperSubCallingHelper(t, "11");

    // Check that helper-ness propagates up through panic/recover.
    // See https://golang.org/issue/31154.
    recoverHelper(t, "12");

    genericHelper<float64>(t, genericFloat64ˢ);
    genericIntHelper(t, genericIntˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string parallelˢ => ref parallelˢᵇ;
private static readonly @string parallelˢᵇ = "parallel"u8;

internal static partial void parallelTestHelper(ж<testing.T> t) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 5; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            notHelperCallingHelper(t, parallelˢ);
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string sub2ˢ => ref sub2ˢᵇ;
private static readonly @string sub2ˢᵇ = "sub2"u8;

internal static void helperSubCallingHelper(ж<testing.T> t, @string msg) {
    t.Helper();
    t.Run(sub2ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Helper();
        tΔ1.Fatal(msg);
    });
}

internal static void recoverHelper(ж<testing.T> t, @string msg) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        defer(() => {
            t.Helper();
            {
                var err = recover(); if (err != nil) {
                    t.Errorf("recover %s", err);
                }
            }
        }, ref ᒐ);
        doPanic(t, msg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void doPanic(ж<testing.T> t, @string msg) {
    t.Helper();
    throw panic(msg);
}

} // end testing_test_package
