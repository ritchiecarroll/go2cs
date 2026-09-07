// sleep_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// syncTimer is Go 1.23's `*(*unsafe.Pointer)(unsafe.Pointer(&c))` (time/sleep.go): the channel
// handed to the runtime's timer as a raw pointer, so a synchronous timer channel can be told apart
// from an asynchronous one under GODEBUG=asynctimerchan. The converter drops the auto form
// (manualConversionFuncs["time"] in go2cs/manualTypeOperations.go) and leaves a placeholder at the
// site, and the body below answers nil — for this reason:
//
// Its ONLY reader in this corpus is the hand-owned time.newTimer (time_impl.cs), which by its own
// line never reads that argument (`_ = cp;` — the channel comes from `arg`, the sync bit is
// recomputed from the GODEBUG setting), so the converted body's `(uintptr)Ꮡc` was a DEAD
// reference-bearing address take, and the corpus's dominant one: 176 of the 283 the Q44 census read
// in os (one per time.NewTimer/After/Ticker) and the single one in syscall. Under the managed
// pointer token (docs/phase4/DESIGN-managed-pointer-token.md) every such take would register a weak
// token nobody resolves; COORD's ruling on the item-2 reading (2026-09-05) displaced it instead.
// nil is what Go's own syncTimer returns under asynctimerchan=1, and it is what newTimer ignores
// either way. If a reader of newTimer's `cp` ever appears, this body is withdrawn and the registry
// carries the timers — the design's stated fallback. tick.cs (hand-owned) never calls syncTimer;
// the Go line survives there only as a comment.

using go;

[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;

partial class time_package
{
    internal static @unsafe.Pointer syncTimer(channel<Time> cʗp)
    {
        _ = cʗp;
        return nil;
    }
}
