// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// The current implementation of notes on Darwin is not async-signal-safe,
// so on Darwin the sigqueue code uses different functions to wake up the
// signal_recv thread. This file holds the non-Darwin implementations of
// those functions. These functions will never be called.
//go:build !darwin && !plan9
namespace go;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sigNoteSetupˢ => ref sigNoteSetupˢᵇ;
private static readonly @string sigNoteSetupˢᵇ = "sigNoteSetup"u8;

internal static void sigNoteSetup(ж<note> _) {
    @throw(sigNoteSetupˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sigNoteSleepˢ => ref sigNoteSleepˢᵇ;
private static readonly @string sigNoteSleepˢᵇ = "sigNoteSleep"u8;

internal static void sigNoteSleep(ж<note> _) {
    @throw(sigNoteSleepˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sigNoteWakeupˢ => ref sigNoteWakeupˢᵇ;
private static readonly @string sigNoteWakeupˢᵇ = "sigNoteWakeup"u8;

internal static void sigNoteWakeup(ж<note> _) {
    @throw(sigNoteWakeupˢ);
}

} // end runtime_package
