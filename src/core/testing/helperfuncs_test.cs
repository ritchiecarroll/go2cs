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
internal static void notHelper(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Error(msg);
}

internal static void helper(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Helper();
    Ꮡt.Error(msg);
}

internal static void notHelperCallingHelper(ж<testing.T> Ꮡt, @string msg) {
    helper(Ꮡt, msg);
}

internal static void helperCallingHelper(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Helper();
    helper(Ꮡt, msg);
}

internal static void genericHelper<G>(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Helper();
    Ꮡt.Error(msg);
}

internal static Action<ж<testing.T>, @string> genericIntHelper = genericHelper<nint>;

internal static void testTestHelper(ж<testing.T> Ꮡt) {
    testHelper(Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string subˢ = "sub"u8;
private static readonly @string genericFloat64ˢ = "GenericFloat64"u8;
private static readonly @string genericIntˢ = "GenericInt"u8;

internal static void testHelper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Check combinations of directly and indirectly
    // calling helper functions.
    notHelper(Ꮡt, "0"u8);
    helper(Ꮡt, "1"u8);
    notHelperCallingHelper(Ꮡt, "2"u8);
    helperCallingHelper(Ꮡt, "3"u8);
    // Check a function literal closing over t that uses Helper.
    void fn(@string msg) {
        Ꮡt.Helper();
        Ꮡt.Error(msg);
    }
    fn("4"u8);
    Ꮡt.Run(subˢ, (ж<testing.T> tΔ1) => {
        helper(tΔ1, "5"u8);
        notHelperCallingHelper(tΔ1, "6"u8);
        // Check that calling Helper from inside a subtest entry function
        // works as if it were in an ordinary function call.
        tΔ1.Helper();
        tΔ1.Error((@string)"7"u8);
    });
    // Check that right caller is reported for func passed to Cleanup when
    // multiple cleanup functions have been registered.
    Ꮡt.Cleanup(() => {
        Ꮡt.Helper();
        Ꮡt.Error((@string)"10"u8);
    });
    Ꮡt.Cleanup(() => {
        Ꮡt.Helper();
        Ꮡt.Error((@string)"9"u8);
    });
    // Check that helper-ness propagates up through subtests
    // to helpers above. See https://golang.org/issue/44887.
    helperSubCallingHelper(Ꮡt, "11"u8);
    // Check that helper-ness propagates up through panic/recover.
    // See https://golang.org/issue/31154.
    recoverHelper(Ꮡt, "12"u8);
    genericHelper<float64>(Ꮡt, genericFloat64ˢ);
    genericIntHelper(Ꮡt, genericIntˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string parallelˢ = "parallel"u8;

internal static partial void parallelTestHelper(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 5; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            notHelperCallingHelper(Ꮡt, parallelˢ);
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sub2ˢ = "sub2"u8;

internal static void helperSubCallingHelper(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Helper();
    Ꮡt.Run(sub2ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Helper();
        tΔ1.Fatal(msg);
    });
}

internal static void recoverHelper(ж<testing.T> Ꮡt, @string msg) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        defer(() => {
            Ꮡt.Helper();
            {
                var err = recover(); if (err != default!) {
                    Ꮡt.Errorf("recover %s"u8, err);
                }
            }
        }, ref ᒐ);
        doPanic(Ꮡt, msg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void doPanic(ж<testing.T> Ꮡt, @string msg) {
    Ꮡt.Helper();
    throw panic(msg);
}

} // end testing_test_package
