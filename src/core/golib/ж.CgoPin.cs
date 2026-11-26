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

public abstract partial class ж<T> : IManagedPinHolder, ICgoWord
{
    PinnedBuffer? IManagedPinHolder.ManagedPin => m_pin;

    // cgo .5 item 2 (GoCgoWordMirror): this pointer as one word of a Go array of C pointers handed to C whole, and a
    // native box of this pointee type for a C address C wrote into such an array.
    nuint ICgoWord.CgoWord() => GoCgoArgs.PointerMember(this);

    object ICgoWord.CgoNativeSibling(nuint address) => new NativeBox<T>(address);
}
