// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cpu = go.@internal.cpu_package;
using sys = go.@internal.runtime.sys_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using go.@internal.runtime;
using static go.crypto.subtle_package;

partial class subtle_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cpuDoesNotSupportDitˢ = (@string)"CPU does not support DIT"u8;
internal static readonly object ditNotEnabledWithinˢ = (@string)"dit not enabled within WithDataIndependentTiming closure"u8;
internal static readonly object ditNotEnabledWithinˢ2 = (@string)"dit not enabled within nested WithDataIndependentTiming closure"u8;
internal static readonly object ditNotEnabledAfterReturnˢ = (@string)"dit not enabled after return from nested WithDataIndependentTiming closure"u8;
internal static readonly object ditNotUnsetAfterˢ = (@string)"dit not unset after returning from WithDataIndependentTiming closure"u8;

public static void TestWithDataIndependentTiming(ж<testing.T> Ꮡt) {
    if (!cpu.ARM64.HasDIT) {
        Ꮡt.Skip(cpuDoesNotSupportDitˢ);
    }

    var ditAlreadyEnabled = sys.DITEnabled();

    WithDataIndependentTiming(() => {
        if (!sys.DITEnabled()) {
            Ꮡt.Fatal(ditNotEnabledWithinˢ);
        }

        WithDataIndependentTiming(() => {
            if (!sys.DITEnabled()) {
                Ꮡt.Fatal(ditNotEnabledWithinˢ2);
            }
        });

        if (!sys.DITEnabled()) {
            Ꮡt.Fatal(ditNotEnabledAfterReturnˢ);
        }
    });

    if (!ditAlreadyEnabled && sys.DITEnabled()) {
        Ꮡt.Fatal(ditNotUnsetAfterˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didnTPanicˢ = (@string)"didn't panic"u8;
internal static readonly object ditStillEnabledAfterˢ = (@string)"DIT still enabled after panic inside of WithDataIndependentTiming closure"u8;

public static void TestDITPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!cpu.ARM64.HasDIT) {
            Ꮡt.Skip(cpuDoesNotSupportDitˢ);
        }

        var ditAlreadyEnabled = sys.DITEnabled();

        defer(() => {
            var e = recover();
            if (e == default!) {
                Ꮡt.Fatal(didnTPanicˢ);
            }
            if (!ditAlreadyEnabled && sys.DITEnabled()) {
                Ꮡt.Error(ditStillEnabledAfterˢ);
            }
        }, ref ᒐ);

        WithDataIndependentTiming(() => {
            if (!sys.DITEnabled()) {
                Ꮡt.Fatal(ditNotEnabledWithinˢ);
            }

            throw panic("bad");
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object ditNotEnabledInNewˢ = (@string)"DIT not enabled in new goroutine"u8;
internal static readonly object ditUnsetAfterReturningˢ = (@string)"dit unset after returning from goroutine started in WithDataIndependentTiming closure"u8;

public static void TestDITGoroutineInheritance(ж<testing.T> Ꮡt) {
    if (!cpu.ARM64.HasDIT) {
        Ꮡt.Skip(cpuDoesNotSupportDitˢ);
    }

    var ditAlreadyEnabled = sys.DITEnabled();

    WithDataIndependentTiming([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        goǃ(() => {
            if (!sys.DITEnabled()) {
                Ꮡt.Error(ditNotEnabledInNewˢ);
            }
            close(doneʗ1);
        });
        ᐸꟷ(done);
        if (!ditAlreadyEnabled && !sys.DITEnabled()) {
            Ꮡt.Fatal(ditUnsetAfterReturningˢ);
        }
    });
}

} // end subtle_internal_test_package
