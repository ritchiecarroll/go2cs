// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished supplement (runtime panic VALUES bridge — no Go counterpart file).
//
// Go's compiler lowers an integer division to a zero check plus a call to runtime.panicdivide(),
// which panics with runtime.divideError — a value whose dynamic type is the unexported
// runtime.errorString, and therefore satisfies runtime.Error. go2cs instead lets the CLR raise
// DivideByZeroException and maps it to a Go panic in golib (RuntimeErrorPanic.TryAsPanic), so an
// IMPLICIT division panic carried only the message text: recover()'s value failed the
// `err.(runtime.Error)` assertion that math/bits' TestDiv32PanicZero makes (Div32 divides without
// an explicit zero guard, unlike Div/Div64 which panic(divideError) themselves).
//
// golib sits UNDER this package and cannot name divideError, so the dependency is inverted: golib
// exposes the hook and the runtime registers its own canonical value here. That keeps the panic
// value identical whether it was raised explicitly by panicdivide() or implicitly by the hardware
// trap — which is exactly Go's own invariant.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files.
[module: GoManualConversion]

namespace go;

using System;
using System.Runtime.CompilerServices;
using go.golib;
using atomic = @internal.runtime.atomic_package;
using @internal.runtime;

public static partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛRegisterRuntimePanicValues()
    {
        // Deferred to first use: divideError and shiftError are statics of this package, and reading
        // them during module initialization would force this type's static constructor to run ahead
        // of the rest of the package's own initialization order.
        RuntimeErrorPanic.IntegerDivideByZeroValue = static () => divideError;
        RuntimeErrorPanic.ShiftErrorValue = static () => shiftError;

        // Go 1.21's panic(nil), gopanic's own first branch verbatim (panic.go): a *PanicNilError unless
        // GODEBUG=panicnil=1, which keeps the nil and counts the non-default event. golib's panic never
        // reaches gopanic, so before this it wrapped the nil itself and runtime's TestPanicNil read
        // recover() = nil where Go reads a *PanicNilError. Deferred to first use, like divideError.
        builtin.NilPanicValue = static () =>
        {
            if (Ꮡdebug.of(debugᴛ1.Ꮡpanicnil).Load() != 1)
                return @new<PanicNilError>();

            panicnil.IncNonDefault();
            return null;
        };
    }
}
