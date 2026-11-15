// ж.HeldPin.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using go.golib;

namespace go;

// The pin-extent index's backfill reads a box's own pin through this, when a cgo package switches the index on after
// pins were already taken (runtime/PinExtents.cs, docs/phase4/DESIGN-cgo-pin-extents.md D4). Its own file, so the box's
// main file keeps every line the docs cite.
public abstract partial class ж<T> : IHeldPin
{
    PinnedBuffer? IHeldPin.HeldPin => m_pin;
}
