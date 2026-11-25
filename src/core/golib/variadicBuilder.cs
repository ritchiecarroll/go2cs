// variadicBuilder.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>The collection builder of <see cref="variadic{T}"/>: a literal argument list.</summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class VariadicBuilder
{
    // The compiler hands the arguments as a read-only span over storage of its choosing: a stack buffer, or
    // read-only static data for an all-constant primitive list. The pack views it without copying and is
    // marked as compiler storage, so nothing writes it (see variadic<T>.IsCompilerStorage).
    public static variadic<T> Create<T>(ReadOnlySpan<T> items) =>
        new(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(items), items.Length), compilerStorage: true);
}
