// variadicPack.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go;

// A Go variadic parameter's incoming pack: `func f(xs ...T)` converts to `params ꓸꓸꓸT xsʗp` where
// `using ꓸꓸꓸT = variadic<T>;`, and the body opens with `var xs = xsʗp.slice();` (or `.sslice()`).
//
// Go gives the callee ONE of two things, and they behave differently:
//   f(a, b, c)  a FRESH slice holding the arguments; the callee's writes are its own.
//   f(s...)     the CALLER'S SLICE ITSELF: same backing array, same length, same capacity. A callee that
//               writes xs[i], writes through &xs[i], or appends within capacity writes the caller's storage.
// A bare Span<T> pack cannot tell those apart, so the heap prologue used to copy both and lost the second's
// writes (modernc.org/sqlite's (*SQLitePreUpdateData).New passes its pack to row, which fills dest[i]).
// This pack carries the difference instead of guessing it: a spread (slice<T>.ꓸꓸꓸ) carries the caller's
// slice<T> header whole, and `.slice()` returns exactly that header; a literal pack carries the compiler's
// elements and `.slice()` copies them, as before. There is no shared state and nothing to go stale.
//
// No conversion from slice<T> to variadic<T> exists on purpose: Go's `f(x)` with `x []any` passes ONE
// element, and C# picks a params method's NORMAL form whenever its argument converts to the parameter
// type. Without the conversion only `x.ꓸꓸꓸ` spreads, as only `x...` does in Go.
[CollectionBuilder(typeof(golib.VariadicBuilder), nameof(golib.VariadicBuilder.Create))]
public readonly ref struct variadic<T>
{
    // The pack's elements. For a spread: the caller's window (writes reach the caller, as in Go). For a
    // literal pack: the compiler's storage, whose kind is the compiler's choice (see IsCompilerStorage).
    private readonly Span<T> m_elements;

    // The caller's slice, when this pack is a spread; default otherwise.
    private readonly slice<T> m_spread;

    private readonly bool m_isSpread;

    // True when the elements are the storage the compiler built for a literal argument list.
    private readonly bool m_isCompilerStorage;

    internal variadic(slice<T> spread)
    {
        m_spread = spread;
        m_isSpread = true;
        m_isCompilerStorage = false;
        m_elements = spread.ToSpan();
    }

    internal variadic(Span<T> elements, bool compilerStorage = false)
    {
        m_elements = elements;
        m_isSpread = false;
        m_isCompilerStorage = compilerStorage;
        m_spread = default;
    }

    /// <summary>True when this pack is a spread <c>f(s...)</c> and so IS the caller's slice.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)] public bool IsSpread => m_isSpread;

    /// <summary>The caller's slice of a spread pack; the nil slice for a literal pack.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)] public slice<T> Spread => m_spread;

    /// <summary>The pack's element count.</summary>
    public int Length => m_elements.Length;

    /// <summary>
    /// The pack's elements as a span. A write through it is legal only when <see cref="IsCompilerStorage"/> is
    /// false: a literal pack's elements are the compiler's storage. Hidden: golib's own plumbing reads it, and a
    /// C# caller reaches a span through the implicit conversion, which copies compiler storage.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)] public Span<T> Elements => m_elements;

    /// <summary>
    /// True for a LITERAL pack built by the compiler (the collection builder): its elements are storage the
    /// compiler chose for the argument list, which for an all-constant primitive list is the module's
    /// read-only static data, and which nothing obliges it to keep private to one call. Such a pack is copied
    /// before anything may write it: the Span conversion copies it, the heap prologue copies it, and the
    /// converter never emits an element write through its stack view.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)] public bool IsCompilerStorage => m_isCompilerStorage;

    /// <summary>An element by reference. A write through it is legal only when <see cref="IsCompilerStorage"/> is false.</summary>
    public ref T this[int index] => ref m_elements[index];

    /// <summary>An element by reference. A write through it is legal only when <see cref="IsCompilerStorage"/> is false.</summary>
    public ref T this[nint index] => ref m_elements[checked((int)index)];

    public Span<T>.Enumerator GetEnumerator() => m_elements.GetEnumerator();

    // Spread operator: a forwarded pack (`var xs = xsʗp;` then `g(xs.ꓸꓸꓸ)`) hands on itself, so a spread
    // caller's slice reaches the next callee whole, as Go's `g(xs...)` passes the same slice.
    [EditorBrowsable(EditorBrowsableState.Never)] public variadic<T> ꓸꓸꓸ => this;

    // Hand-written callees that still take `params Span<T>` (hand-owned files) keep Go's copy-free reading
    // of a spread but not its header; that is their documented, listed gap. A literal pack is copied here,
    // so such a callee can never write the compiler's storage.
    public static implicit operator Span<T>(variadic<T> pack) =>
        pack.m_isCompilerStorage && pack.m_elements.Length > 0 ? AllocationCounter.CopyOf<T>(pack.m_elements) : pack.m_elements;

    public static implicit operator ReadOnlySpan<T>(variadic<T> pack) => pack.m_elements;

    // A span-shaped spread source (a stack view's `v.ꓸꓸꓸ`, an array's) packs as LITERAL elements over
    // that same storage: the callee reads and writes it in place, and a heap prologue copies it as before.
    public static implicit operator variadic<T>(Span<T> elements) => new(elements);
}

[EditorBrowsable(EditorBrowsableState.Never)] public static class VariadicExtensions
{
    // The HEAP prologue `var xs = xsʗp.slice();`. A spread hands back the caller's slice itself (Go's
    // f(s...)); a literal pack copies into a fresh slice, keeping Go's nil for a zero-argument call.
    public static slice<T> slice<T>(this variadic<T> pack) =>
        pack.IsSpread ? pack.Spread : pack.Elements.slice();

    public static slice<T> slice<T>(this variadic<T> pack, nint low) =>
        pack.IsSpread ? pack.Spread.slice(low) : pack.Elements.slice(low);

    public static slice<T> slice<T>(this variadic<T> pack, nint low, nint high) =>
        pack.IsSpread ? pack.Spread.slice(low, high) : pack.Elements.slice(low, high);

    public static slice<T> slice<T>(this variadic<T> pack, nint low, nint high, nint max) =>
        pack.IsSpread ? pack.Spread.slice(low, high, max) : pack.Elements.slice(low, high, max);

    // The stack-view prologue `var xs = xsʗp.sslice();`: the pack's own elements, no copy. For a spread they
    // are the caller's window, so element writes reach the caller, as in Go.
    public static sslice<T> sslice<T>(this variadic<T> pack, nint low = -1, nint high = -1, nint max = -1) =>
        pack.Elements.sslice(low, high, max);
}
