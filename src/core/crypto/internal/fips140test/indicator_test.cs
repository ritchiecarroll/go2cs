// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δfips140 = go.crypto.@internal.fips140_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.crypto.@internal;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object indicatorShouldBeFalseIfˢ = (@string)"indicator should be false if no calls are made"u8;
internal static readonly object indicatorShouldBeTrueIfˢ = (@string)"indicator should be true if RecordApproved is called"u8;
internal static readonly object indicatorShouldBeTrueIfˢ2 = (@string)"indicator should be true if RecordApproved is called multiple times"u8;
internal static readonly object indicatorShouldBeFalseIfˢ2 = (@string)"indicator should be false if RecordNonApproved is called"u8;
internal static readonly object indicatorShouldBeFalseIfˢ3 = (@string)"indicator should be false if both RecordApproved and RecordNonApproved are called"u8;
internal static readonly object indicatorShouldBeFalseIfˢ4 = (@string)"indicator should be false if both RecordNonApproved and RecordApproved are called"u8;
internal static readonly object indicatorShouldBeFalseIfˢ5 = (@string)"indicator should be false if RecordApproved is called in a different goroutine"u8;
internal static readonly object indicatorShouldBeTrueIfˢ3 = (@string)"indicator should be true if RecordNonApproved is called in a different goroutine"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestIndicator(ж<testing.T> Ꮡt) {
    Δfips140.ResetServiceIndicator();
    if (Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeFalseIfˢ);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordApproved();
    if (!Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeTrueIfˢ);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordApproved();
    Δfips140.RecordApproved();
    if (!Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeTrueIfˢ2);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordNonApproved();
    if (Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeFalseIfˢ2);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordApproved();
    Δfips140.RecordNonApproved();
    if (Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeFalseIfˢ3);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordNonApproved();
    Δfips140.RecordApproved();
    if (Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeFalseIfˢ4);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordNonApproved();
    ref var done = ref heap<channel<EmptyStruct>>(out var Ꮡdone);
    done = new channel<EmptyStruct>(0);
    goǃ(() => {
        Δfips140.ResetServiceIndicator();
        Δfips140.RecordApproved();
        close(Ꮡdone.ValueSlot);
    });
    ᐸꟷ(done);
    if (Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeFalseIfˢ5);
    }
    Δfips140.ResetServiceIndicator();
    Δfips140.RecordApproved();
    done = new channel<EmptyStruct>(0);
    goǃ(() => {
        Δfips140.ResetServiceIndicator();
        Δfips140.RecordNonApproved();
        close(Ꮡdone.ValueSlot);
    });
    ᐸꟷ(done);
    if (!Δfips140.ServiceIndicator()) {
        Ꮡt.Error(indicatorShouldBeTrueIfˢ3);
    }
}

} // end fipstest_internal_test_package
