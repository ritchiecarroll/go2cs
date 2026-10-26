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
/// <para>
/// Go 1.26 also gives the buffer to a variable that escapes, when its one escape is a single statement outside its
/// loops (cmd/compile/internal/slice): its buffer is <c>new stackAppend&lt;T&gt;(moves: true)</c>, the converter emits
/// <c>x = move2heap(xᴷ, x);</c> ahead of that statement, which copies a slice still in the buffer to the heap, and a
/// variable whose capacity is observed takes <c>new stackAppend&lt;T&gt;(moves: true, capacityUsed: true)</c>, whose
/// appends grow inside the buffer a size class at a time.
/// </para>
/// </remarks>
public struct stackAppend<T>
{
    // The bytes Go reserves: the default of base.Debug.VariableMakeThreshold (cmd/compile/internal/base/flag.go).
    private const nuint StoreBytes = 32;

    internal bool used;

    // The buffer a moving variable took, which move2heap moves its slice out of and an append that grows inside it
    // reuses. Only a moving variable keeps it, and only while the slice can still be in it: the local is passed by
    // reference, so the JIT reports it for the whole frame, and a store it still held after the slice outgrew it would
    // keep that store's elements reachable where Go's dead stack buffer does not (runtime's TestGCTestIsReachable).
    internal T[]? store;

    // A variable Go's slice pass moves to the heap (cmd/compile/internal/slice): its buffer keeps the store it took.
    internal readonly bool moves;

    // Go's capUsed: the moving variable's capacity is observed, so every append grows inside the buffer a size class
    // at a time (UseBuf, runtime.growsliceBuf) and move2heap keeps the capacity (PreserveCapacity, runtime.moveSlice).
    internal readonly bool capacityUsed;

    /// <summary>
    /// The buffer of a variable Go's slice pass moves to the heap ahead of its one escape
    /// (<see cref="builtin.move2heap{T}"/>). When its capacity is observed (<c>cap(s)</c>, <c>s = s[i:j]</c>,
    /// <c>s = []T{...}</c>, or <c>s</c> passed to a call), its appends grow inside the buffer as
    /// <c>runtime.growsliceBuf</c> does, and the move keeps its capacity.
    /// </summary>
    /// <param name="moves">Whether Go moves the variable to the heap; it must be true.</param>
    /// <param name="capacityUsed">Whether the variable's capacity is observed.</param>
    public stackAppend(bool moves, bool capacityUsed = false)
    {
        this.moves = moves;
        this.capacityUsed = moves && capacityUsed;
    }

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

    // The dims an unnamed array element is sized by (GoGrowthFacts.ValueDims), off the first of the elements.
    internal static nint[]? Dims(ReadOnlySpan<T> elems) =>
        GoGrowthFacts<T>.SizedByValue && elems.Length > 0 ? GoGrowthFacts<T>.ValueDims(elems[0]) : null;
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
    /// Inlined, this is only the check whether the append may take the buffer, or has a moving buffer to track; any
    /// other append is <see cref="go.slice{T}.Append(in slice{T}, ReadOnlySpan{T})"/>. The take itself is never
    /// inlined, so that both returns reach the caller through ONE result slot, which the site's next call overwrites.
    /// Inlined, the store-taking return builds its slice in a stack temp of its own: the JIT reports that temp for
    /// the caller's whole frame, and nothing overwrites it, so the first store stays reachable after the slice
    /// outgrows it. Go scans its stack buffer only while a live pointer reaches it, and runtime's
    /// TestGCTestIsReachable reads the difference: the outgrown store's elements answer reachable (gcroot: the
    /// test's frame, the temp, the 4-element store).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static slice<T> append<T>(in stackAppend<T> buffer, slice<T> slice, params ReadOnlySpan<T> elems)
    {
        if (buffer.moves || !buffer.used && slice.Length == 0)
            return appendBuffered(in buffer, slice, elems);

        return go.slice<T>.Append(slice, elems);
    }

    // The append that may take the buffer, or that tracks a moving buffer's store; never inlined (see append).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static slice<T> appendBuffered<T>(in stackAppend<T> buffer, slice<T> slice, ReadOnlySpan<T> elems)
    {
        nint newLength = slice.Length + elems.Length;

        if (buffer.capacityUsed)
            return appendInBuffer(ref Unsafe.AsRef(in buffer), slice, elems, newLength);

        if (newLength > slice.Capacity && slice.Length == 0 && !buffer.used)
        {
            nint capacity = stackAppend<T>.Capacity(elems);

            if (newLength <= capacity)
            {
                ref stackAppend<T> taken = ref Unsafe.AsRef(in buffer);
                taken.used = true;

                T[] store = new T[capacity];
                elems.CopyTo(store);

                if (taken.moves)
                    taken.store = store;

                return new slice<T>(store, 0, newLength, capacity);
            }
        }

        slice<T> grown = go.slice<T>.Append(slice, elems);

        // A moving variable's one append site: the slice left the store (a growth, or a slice reset and refilled), so
        // let go of it, as nothing but the slice reaches Go's stack buffer.
        if (buffer.store is not null && !ReferenceEquals(grown.m_array, buffer.store))
            Unsafe.AsRef(in buffer).store = null;

        return grown;
    }

    // runtime.growsliceBuf, at every append of a variable whose capacity is observed: a growth that fits the buffer
    // moves what the slice holds to the buffer's start (when it is not there already) and takes the next size class
    // of the new length, not growslice's doubling; one that does not fit grows as growslice does.
    private static slice<T> appendInBuffer<T>(ref stackAppend<T> buffer, slice<T> slice, ReadOnlySpan<T> elems, nint newLength)
    {
        if (newLength <= slice.Capacity || elems.Length == 0)
            return go.slice<T>.Append(slice, elems);

        nint bufferLength = stackAppend<T>.Capacity(elems);

        // Out of the buffer for good, until the slice is emptied: let go of the store, so nothing but the slice reaches
        // it, as nothing but the slice reaches Go's stack buffer. A later growth that fits takes a fresh one.
        if (newLength > bufferLength)
        {
            buffer.store = null;
            return go.slice<T>.Append(slice, elems);
        }

        T[] store = buffer.store ??= new T[bufferLength];
        nint oldLength = slice.Length;

        // Go: oldPtr != bufPtr && oldLen != 0. Span copies handle the overlap of a window into the buffer itself.
        if (oldLength != 0 && !(ReferenceEquals(slice.m_array, store) && slice.Low == 0))
            slice.ToSpan().CopyTo(store);

        nint newCapacity = GoGrowthFacts<T>.RoundUpCapacity(newLength, newLength, stackAppend<T>.Dims(elems));

        // Zeroed beyond the new length, as Go clears the buffer there; [oldLength:newLength) is written below.
        store.AsSpan((int)newLength, (int)(newCapacity - newLength)).Clear();
        elems.CopyTo(store.AsSpan((int)oldLength));

        return new slice<T>(store, 0, newLength, newCapacity);
    }

    /// <summary>
    /// Go's <c>s = move2heap(s)</c> (cmd/compile/internal/slice): ahead of the one statement where a variable whose
    /// appends used <paramref name="buffer"/> stops being the only reference to its backing store (a return, an
    /// assignment of it, a range over it), a slice still in the buffer is copied to the heap. A slice that grew out
    /// of the buffer is returned as it is.
    /// </summary>
    /// <param name="buffer">The variable's <see cref="stackAppend{T}"/> local, passed by reference.</param>
    /// <param name="slice">The variable's slice.</param>
    /// <returns>The slice, on the heap.</returns>
    /// <remarks>
    /// When the capacity is observed, the copy keeps it (<c>runtime.moveSlice</c>); otherwise it is the size class of
    /// the length (<c>runtime.moveSliceNoCap</c>), and the elements past the length are zero. An empty slice comes back
    /// empty: nil stays nil, and a non-nil one has no capacity, as Go's zerobase result does.
    /// <para>
    /// Never inlined, for the reason <c>append(in stackAppend&lt;T&gt;, ...)</c> is not: inlined, the slice it takes in
    /// and the one it builds would sit in stack temps the JIT reports for the caller's whole frame, keeping the store
    /// reachable after the move.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static slice<T> move2heap<T>(in stackAppend<T> buffer, slice<T> slice)
    {
        T[]? store = buffer.store;

        // The transition is the variable's last use of the buffer: let go of the store either way.
        Unsafe.AsRef(in buffer).store = null;

        if (store is null || !ReferenceEquals(slice.m_array, store))
            return slice;

        nint length = slice.Length;
        nint capacity = buffer.capacityUsed ? slice.Capacity : length;

        if (capacity == 0)
            return new slice<T>(Array.Empty<T>(), 0, 0);

        if (!buffer.capacityUsed)
            capacity = GoGrowthFacts<T>.RoundUpCapacity(length, length, stackAppend<T>.Dims(slice.ToSpan()));

        T[] moved = AllocationCounter.NewArray<T>(capacity);
        GoMemProfile.Charge<T>(moved, capacity, buffer.capacityUsed ? GoAllocEntry.MoveSlice : GoAllocEntry.MoveSliceNoCap);

        // moveSlice copies all of s[:cap(s)]; moveSliceNoCap copies s[:len(s)] and leaves the rest zero.
        store.AsSpan((int)slice.Low, (int)(buffer.capacityUsed ? capacity : length)).CopyTo(moved);

        return new slice<T>(moved, 0, length, capacity);
    }
}
