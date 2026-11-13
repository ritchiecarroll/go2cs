// ж.NativeDerivation.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

// What a native box DERIVED from another pointer keeps of it (docs/phase4/DESIGN-unsafe-pointer-call-result.md).
//
// A native box is an address. One minted over MANAGED storage holds what keeps that storage where the address says it
// is: a pin, or the box it was reinterpreted from. A box derived from it by an offset was minted from the address
// alone, so the pin could be released under a pointer still in use. A box derived from a pointer that is not C memory
// alone RETAINS that pointer, so the root, its pin and its source live as long as the derived box.
internal static class NativeDerivation
{
    // What a box derived from `root` retains: `root` itself, unless it is a native box over C memory alone. The other
    // native kinds (a window over a native array) retain nothing here, as before.
    internal static object? Kept(object? root) => root switch
    {
        null => null,
        INativeRooted rooted => rooted.IsGenuinelyNativeRoot ? null : root,
        IGoReflectBox { NativeAddress: not 0 } => null,
        _ => root
    };

    // unsafe.Add over an unsafe.Pointer that carries its box: the box the moved pointer carries, a native box AT the
    // new value that retains what the pointer carried. Null where there is nothing to keep (the pointer carried
    // nothing, or C memory alone), and for a token, whose arithmetic keeps its own route.
    internal static ж<byte>? Moved(object? carried, nuint value)
    {
        if (value == 0 || ManagedPointerTokens.IsTaggedToken(value) || Kept(carried) is not { } kept)
            return null;

        return new NativeBox<byte>(value, retainedSource: kept);
    }
}
