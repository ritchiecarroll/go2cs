// stackAppend.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// Go's stack backing store for a non-escaping append: the 32-byte buffer, and its used flag, that Go 1.25+ keeps
/// in the frame of a function whose append qualifies.
/// </summary>
/// <remarks>
/// <para>
/// The converter declares one per qualifying variable, at the entry of the innermost function that declares the
/// variable and appends to it (<c>var xᴷ = new stackAppend&lt;T&gt;();</c>), and passes it at that variable's
/// first qualifying append (<c>x = append(xᴷ, x, v);</c>). A site qualifies when Go's own compiler reached the
/// backing-store check there: the converter reads that from <c>-gcflags=-m -d=variablemakehash=vy</c>.
/// </para>
/// <para>
/// Go's rule (cmd/compile/internal/ssagen/ssa.go, the append lowering and <c>getBackingStoreInfo</c>): on the
/// growth path only, when the new length is at most K = 32 / the element's size, the flag is unset and the slice
/// is empty, the slice becomes <c>store[:newLen:K]</c> and the flag is set. The flag lives in the frame, so the
/// buffer is taken at most once per call: a slice reset to nil and refilled grows as growslice does.
/// </para>
/// <para>
/// The struct is passed <c>in</c>, which C# does by reference to the caller's local with no keyword at the call,
/// and the overload sets the flag through <see cref="Unsafe.AsRef{T}(in T)"/>. That is sound only when the
/// argument IS a plain local of the calling method or lambda, never a copy: GO2CS0007 (go2cs-gen) refuses any
/// other argument as a build error. The buffer itself is allocated when it is taken and is not charged to
/// AllocationCounter, because Go's lives on the stack and allocates nothing.
/// </para>
/// </remarks>
public struct stackAppend<T>
{
    // The bytes Go reserves: the default of base.Debug.VariableMakeThreshold (cmd/compile/internal/base/flag.go).
    private const nuint StoreBytes = 32;

    internal bool used;

    // K for a type whose Go size is a property of the type; 0 when there is no buffer (a zero-size element, one
    // over 32 bytes, or a size the converter could not derive).
    private static readonly nint s_capacity = CapacityFor(GoGrowthFacts<T>.Size);

    /// <summary>
    /// K, the elements the buffer holds: 32 / the element's Go size. An unnamed array reads its size off the value
    /// it appends, as growslice does (<see cref="GoGrowthFacts{T}.SizedByValue"/>).
    /// </summary>
    internal static nint Capacity(ReadOnlySpan<T> elems)
    {
        if (!GoGrowthFacts<T>.SizedByValue)
            return s_capacity;

        nint[]? dims = GoGrowthFacts<T>.ValueDims(elems[0]);

        return dims is not null && GoReflect.TryGoSizeOf(typeof(T), dims, out nuint size) ? CapacityFor(size) : 0;
    }

    private static nint CapacityFor(nuint size) =>
        size == 0 || size > StoreBytes ? 0 : (nint)(StoreBytes / size);
}

public static partial class builtin
{
    /// <summary>
    /// The append at a variable's first qualifying site, which may take Go's stack backing store: when the slice
    /// must grow, is empty, the buffer is unused and the new length fits K, the result is a K-capacity slice over a
    /// fresh buffer; otherwise this is <see cref="go.slice{T}.Append(in slice{T}, ReadOnlySpan{T})"/>.
    /// </summary>
    /// <param name="buffer">The function's <see cref="stackAppend{T}"/> local, passed by reference.</param>
    /// <param name="slice">Destination slice.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Never inlined, so that both returns reach the caller through ONE result slot, which the site's next call
    /// overwrites. Inlined, the store-taking return builds its slice in a stack temp of its own: the JIT reports that
    /// temp for the caller's whole frame, and nothing overwrites it, so the first store stays reachable after the slice
    /// outgrows it. Go scans its stack buffer only while a live pointer reaches it, and runtime's
    /// TestGCTestIsReachable reads the difference: the outgrown store's elements answer reachable (gcroot: the
    /// test's frame, the temp, the 4-element store).
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static slice<T> append<T>(in stackAppend<T> buffer, slice<T> slice, params ReadOnlySpan<T> elems)
    {
        nint newLength = slice.Length + elems.Length;

        if (newLength > slice.Capacity && slice.Length == 0 && !buffer.used)
        {
            nint capacity = stackAppend<T>.Capacity(elems);

            if (newLength <= capacity)
            {
                Unsafe.AsRef(in buffer).used = true;

                T[] store = new T[capacity];
                elems.CopyTo(store);

                return new slice<T>(store, 0, newLength, capacity);
            }
        }

        return go.slice<T>.Append(slice, elems);
    }
}
