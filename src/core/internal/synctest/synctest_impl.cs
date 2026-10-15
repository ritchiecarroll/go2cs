// synctest_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written bodies for internal/synctest's five //go:linkname pulls -- Run, Wait, acquire, release
// and inBubble. In Go the runtime pushes them from runtime/synctest.go (synctestRun, synctestWait,
// synctest_acquire, synctest_release, synctest_inBubble); go2cs emits them as bodyless `partial`
// methods, and without a body here the PartialStubGenerator fills each with a throwing stub, so every
// synctest.Run in net/http's suite and in this package's own was an infrastructure error.
//
// The bubble itself is go.golib.SyncTestBubble (Go's synctestGroup): membership, the park/READY
// accounting, bubbled channels and fake time all live in golib and in time's hand-owned companion
// (S1a-S3 of docs/phase4/DESIGN-gopark-goready-synctest.md). This file is S4: it only forwards. golib is
// the only home the bubble can have -- this package may reference nothing but golib and unsafe (the
// no-cycle rule internal/sync keeps), and the channels, the sync primitives and time all have to reach
// the same object.
//
// Go's `any` for a bubble is the golib object itself, as Go's is the *synctestGroup: release and
// inBubble cast it back where Go writes `sg.(*synctestGroup)`. Only Acquire's own result ever reaches
// them (through internal/synctest.Bubble), so the failing assertion is not a path either side takes.

// Aliased rather than imported wholesale, as sync's companions do: this file needs exactly one golib
// type.
using SyncTestBubble = go.golib.SyncTestBubble;
using @unsafe = go.unsafe_package;

// Hand-owned (no synctest_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned linkname companions.
[module: go.GoManualConversion]

namespace go.@internal;

partial class synctest_package
{
    // synctestRun: the asynctimerchan refusal, the nested-Run panic, the bubble's timer loop and the
    // deadlock panic are all Go's, in SyncTestBubble.Run.
    public static partial void Run(Action f) => SyncTestBubble.Run(f);

    // synctestWait: "goroutine is not in a bubble" and "wait already in progress" included.
    public static partial void Wait() => SyncTestBubble.Wait();

    // synctest_acquire: the caller's bubble with one more active count, or nil outside a bubble.
    internal static partial any acquire() => SyncTestBubble.Acquire();

    // synctest_release: sg.(*synctestGroup).decActive().
    internal static partial void release(any _) => ((SyncTestBubble)_).Release();

    // synctest_inBubble: gp.syncGroup = sg.(*synctestGroup) for the duration of f.
    internal static partial void inBubble(any _Δp0, Action _Δp1) => SyncTestBubble.InBubble((SyncTestBubble)_Δp0, _Δp1);

    // Go 1.26 pushes four more from runtime/synctest.go. IsInBubble is `getg().bubble != nil`, which
    // is golib's membership test; the other three are bodied below as stated refusals.
    public static partial bool IsInBubble() => SyncTestBubble.Current is not null;

    // synctest_associate, synctest_disassociate and synctest_isAssociated record a bubble id on the
    // HEAP OBJECT p points to (a _KindSpecialBubble special on its span, keyed by p's offset), so Go
    // answers the same for every pointer to the same address. The managed model has no such record:
    // a pointer to a field is a reference box minted where the address is taken, so two pointers
    // to one field are two boxes, and keying a table on the box would answer CurrentBubble to every Associate and false to IsAssociated, a
    // plausible wrong answer. Each refuses loudly instead. Their one caller in Go is sync.WaitGroup
    // (waitgroup.go: Add, Wait), and the hand-owned sync/waitgroup.cs calls none of them: it keeps
    // 1.26's association on the WaitGroup's own lazily made state, the one identity every pointer to
    // the WaitGroup reaches, as Go's special does on the object.
    internal static partial nint associate(@unsafe.Pointer p) => throw panic(refusal("associate"));

    internal static partial void disassociate(@unsafe.Pointer b) => throw panic(refusal("disassociate"));

    internal static partial bool isAssociated(@unsafe.Pointer p) => throw panic(refusal("isAssociated"));

    private static @string refusal(string name) =>
        $"go2cs: //go:linkname push runtime.synctest_{name} -> internal/synctest.{name} is not honored: " +
        "the managed heap has no per-object bubble record, so a pointer's bubble association cannot be kept";
}
