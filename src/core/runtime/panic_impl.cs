// panic_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.throw and runtime.fatal, hand-owned (the fatal path; the two are displaced through
// manualConversionFuncs, so each flavour's panic.cs carries a placeholder where the converted body
// was). docs/phase4/DESIGN-fatal-path.md.
//
// WHY. The converted `throw` prints Go's `fatal error: <text>` line through golib's `print` and
// then calls `fatalthrow`, whose FIRST statement is `getcallerpc()` -- a bodyless partial the
// generator fills with a throw, because getcallerpc/getcallersp are compiler intrinsics returning a
// caller's PC and SP that the CLR does not expose and that this runtime's synthetic-PC scheme
// deliberately does not mint for arbitrary frames. So every fatal on every flavour printed Go's
// first line and then a .NET exception dump naming getcallerpc, and exited 2 through golib's
// unhandled-exception backstop rather than through Go's own exit. Measured on windows and linux,
// frame for frame identical (DESIGN-fatal-path.md §2); the linux prediction that it would differ
// was FALSIFIED, which is what collapsed the remedy to one shape for all three flavours.
//
// THE REMEDY IS NOT TO IMPLEMENT getcallerpc. It is to stop the fatal path needing it, and the
// corpus census says where to cut: `fatalthrow` has exactly TWO callers, `throw` at panic.cs:1098
// and `fatal` at panic.cs:1118, and `fatalpanic` has exactly ONE, `gopanic` at panic.cs:851, which
// is itself already dead at its own `getcallerpc()` and which no panic in this runtime reaches (a
// panic is a golib PanicException reported by CrashReport). So displacing these TWO leaves
// fatalthrow, fatalpanic, getcallerpc and getcallersp all UNREACHED rather than unimplemented --
// the same shape `write1` already has on linux, and a strictly smaller increment than the record's
// own §3 predicted.
//
// WHAT THE BODIES DO. Nothing but forward: golib's FatalReport owns the report and the exit, and it
// owns them for both of this package's sites, for `sync`'s two shims, and at Go 1.24 for the new
// `internal/sync`'s -- one primitive under all of them rather than four spellings of one rule
// (COORD ruling, mailbox 4e9b115). The traceback beneath the text is this package's own
// fatalTraceback, registered into golib from managed_impl.cs's module initializer.
//
// THE throwType AXIS IS PRESERVED. Go's `throw` is throwTypeRuntime -- the runtime itself is at
// fault, and gotraceback raises the traceback level so system goroutines and runtime frames ARE
// shown -- while `fatal` is throwTypeUser and leaves the level alone. That is the `userFault`
// argument, and it is the only difference between these two bodies, exactly as it is the only
// difference between Go's.
//
// WHAT IS NOT REPRODUCED, stated rather than approximated: Go's fatal header carries
// `gp=0x… m=… mp=0x…`, the runtime-pointer form a throw raises the level to get. We hold no g, m or
// mp addresses that mean anything, and inventing three plausible hex numbers would be fabrication
// in the one artifact an operator reads when things have already gone wrong. The plain
// `goroutine N [status]:` header is printed instead -- the form the panic side already prints and
// the form Go's own consumers match on. DESIGN-fatal-path.md §5a.
//
// Hand-owned: there is no panic_impl.go, so a -stdlib reconvert never regenerates this file.

using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // throw triggers a fatal error that dumps a stack trace and exits.
    //
    // throw should be used for runtime-internal fatal errors where Go itself is at fault, and it is
    // not recoverable by any means: no deferred function runs, and recover() never sees it.
    //
    // Go's own body prints the text on the system stack and then calls fatalthrow(throwTypeRuntime);
    // the text and the traceback are one report here, written by one writer, because splitting them
    // is what let the traceback half fail while the text half succeeded.
    internal static void @throw(@string s)
    {
        FatalReport.Fatal(s, userFault: false);
    }

    // fatal triggers a fatal error that dumps a stack trace and exits.
    //
    // fatal is equivalent to throw, but is used when user code is expected to be at fault for the
    // failure, such as racing map writes -- Go's throwTypeUser.
    internal static void fatal(@string s)
    {
        FatalReport.Fatal(s, userFault: true);
    }
}
