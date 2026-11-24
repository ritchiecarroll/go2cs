// ж.CgoPin.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// cgo Phase 3, increment 3.2, seat 1b (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.12): the pin a box already owns,
// for the cgo door that pins a Go argument for one call (GoCgoArgs.PinVoidPointer). A box minted over managed storage
// can hold a pin (the address-take's pin-only hold, or the pin a reinterpret view took over its root); the door hands C
// an address inside the object that pin holds still, and pins that same object again with a call-scoped `fixed`.

using go.golib;

namespace go;

// The pin a box owns, read without the pointee type (the door sees the box as an object).
internal interface IManagedPinHolder
{
    PinnedBuffer? ManagedPin { get; }
}

public abstract partial class ж<T> : IManagedPinHolder
{
    PinnedBuffer? IManagedPinHolder.ManagedPin => m_pin;
}
