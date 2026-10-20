// GoSizeClasses.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace go;

/// <summary>
/// Go's allocator size classes and <c>roundupsize</c>: the capacity a Go allocation of a given byte size
/// really gets, which is what <c>cap()</c> observes after an append grows a slice.
/// </summary>
/// <remarks>
/// <para>
/// The numbers are Go's own (GOROOT/src/internal/runtime/gc: <c>SizeClassToSize</c>, <c>MaxSmallSize</c>,
/// <c>PageShift</c>, <c>MallocHeaderSize</c>, <c>MinSizeForMallocHeader</c>), the same on every 64-bit
/// target. repoguard's TestGolibSizeClassesMatchTheToolchain fails when this table drifts from the pinned
/// toolchain's, holds <see cref="PtrSize"/> to the converted internal/goarch's, and proves that taking the
/// smallest class at least as large as the request is exactly Go's <c>SizeToSizeClass8</c>/
/// <c>SizeToSizeClass128</c> lookup for every small size.
/// </para>
/// <para>
/// A pointer-bearing object larger than <see cref="MinSizeForMallocHeader"/> carries an 8-byte malloc header
/// inside its size class, so its usable size is the class minus the header (Go's <c>roundupsize</c> with
/// <c>noscan</c> false). A request past the largest small class rounds up to whole 8 KiB pages.
/// </para>
/// </remarks>
public static class GoSizeClasses
{
    private static ReadOnlySpan<ushort> SizeClassToSize =>
    [
        0, 8, 16, 24, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 256, 288, 320, 352,
        384, 416, 448, 480, 512, 576, 640, 704, 768, 896, 1024, 1152, 1280, 1408, 1536, 1792, 2048, 2304,
        2688, 3072, 3200, 3456, 4096, 4864, 5376, 6144, 6528, 6784, 6912, 8192, 9472, 9728, 10240, 10880,
        12288, 13568, 14336, 16384, 18432, 19072, 20480, 21760, 24576, 27264, 28672, 32768,
    ];

    /// <summary>The largest small-object size class, Go's <c>MaxSmallSize</c>.</summary>
    public const nuint MaxSmallSize = 32768;

    /// <summary>Go's <c>PageShift</c>: a large object rounds up to a multiple of 1 &lt;&lt; 13 bytes.</summary>
    public const int PageShift = 13;

    /// <summary>The header a pointer-bearing object past <see cref="MinSizeForMallocHeader"/> carries.</summary>
    public const nuint MallocHeaderSize = 8;

    /// <summary>
    /// Go's <c>goarch.PtrSize</c> on the target the standard library is converted for. That target is amd64
    /// (internal/goarch's converted PtrSize is 8, and GoReflect's Go sizes are amd64 sizes), so the malloc
    /// header threshold below is the 64-bit one; Go's 32-bit targets (386, arm) would give 128.
    /// </summary>
    public const nuint PtrSize = 8;

    /// <summary>Go's <c>goarch.PtrBits</c>.</summary>
    public const nuint PtrBits = PtrSize * 8;

    /// <summary>Go's <c>MinSizeForMallocHeader</c>, the same expression over the same pointer size.</summary>
    public const nuint MinSizeForMallocHeader = PtrSize * PtrBits;

    /// <summary>
    /// Go's <c>roundupsize(size, noscan)</c>: the bytes an allocation of <paramref name="size"/> bytes really
    /// provides, where <paramref name="noscan"/> means the type holds no pointers.
    /// </summary>
    public static nuint RoundUpSize(nuint size, bool noscan)
    {
        nuint reqSize = size;

        if (reqSize <= MaxSmallSize - MallocHeaderSize)
        {
            // Small object. A pointer-bearing one past the header threshold carries the header in its
            // class, and the caller sees the class minus the header, as Go's mallocgc adds it back.
            if (!noscan && reqSize > MinSizeForMallocHeader)
                reqSize += MallocHeaderSize;

            return SmallestClassFor(reqSize) - (reqSize - size);
        }

        // Large object: align up to the next page, keeping the request when that overflows.
        const nuint pageSize = (nuint)1 << PageShift;
        nuint rounded = reqSize + (pageSize - 1);

        return rounded < size ? size : rounded & ~(pageSize - 1);
    }

    // The smallest size class at least as large as size (0 < size <= MaxSmallSize), which repoguard proves
    // equal to Go's SizeToSizeClass8 / SizeToSizeClass128 lookup at every size.
    private static nuint SmallestClassFor(nuint size)
    {
        ReadOnlySpan<ushort> classes = SizeClassToSize;
        int index = classes.BinarySearch((ushort)size);

        return classes[index >= 0 ? index : ~index];
    }

    private static readonly ConcurrentDictionary<Type, byte> s_unroundedElementTypes = new();

    /// <summary>
    /// The element types whose Go size golib could not derive, so whose slices grew to Go's <c>nextslicecap</c>
    /// without the size-class rounding, and whose <c>cap()</c> can then differ from Go's.
    /// </summary>
    /// <remarks>
    /// Empty in a correct program. An unnamed array element (<c>[][3]byte</c>) is sized from the appended value
    /// (see <see cref="GoGrowthFacts{T}.SizedByValue"/>), so it is recorded only when a growth has no shaped
    /// value to read: <c>reflect.Value.Grow</c> on a nil or empty slice whose element length the slice never
    /// recorded. GolibTests' GoGrowSliceTests fails when anything is recorded here.
    /// </remarks>
    public static IReadOnlyCollection<Type> UnroundedElementTypes => [.. s_unroundedElementTypes.Keys];

    internal static void RecordUnroundedElementType(Type type) => s_unroundedElementTypes.TryAdd(type, 0);
}

/// <summary>
/// The two facts Go's growslice reads from an element type, its Go size and whether it holds pointers (Go's
/// <c>noscan</c> is the inverse): read once per closed <typeparamref name="T"/>, or, for an unnamed array, off
/// the value each growth appends.
/// </summary>
internal static class GoGrowthFacts<T>
{
    /// <summary>
    /// Whether <typeparamref name="T"/> is an unnamed array, whose Go size is a property of the VALUE:
    /// <c>array&lt;ulong&gt;</c> is the one C# type for <c>[2]uint64</c> and <c>[3]uint64</c>, so no per-type
    /// cache can answer it, and every growth reads the length off an element it appends.
    /// </summary>
    internal static readonly bool SizedByValue =
        !GoReflect.TryGoSizeOf(typeof(T), null, out _) && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(array<>);

    /// <summary>
    /// The Go (amd64) size of <typeparamref name="T"/>, or 0 when it is sized by value or not derivable; an
    /// underivable type is recorded in <see cref="GoSizeClasses.UnroundedElementTypes"/>.
    /// </summary>
    internal static readonly nuint Size = SizeOf();

    /// <summary>Whether <typeparamref name="T"/> holds no pointer; an unknown pointer prefix is not noscan.</summary>
    internal static readonly bool NoScan = GoReflect.GoPtrBytesOf(typeof(T)) == 0;

    private static nuint SizeOf()
    {
        if (GoReflect.TryGoSizeOf(typeof(T), null, out nuint size))
            return size;

        if (!SizedByValue)
            GoSizeClasses.RecordUnroundedElementType(typeof(T));

        return 0;
    }

    /// <summary>
    /// The array dims of one element value, outermost first, for a <see cref="SizedByValue"/> type: a nested
    /// array (<c>[2][3]T</c>) reads its inner length through its first element, and the walk stops at a zero
    /// length, whose Go size is 0 whatever it nests. Null when the value is not a shaped array (a
    /// <c>default</c> array&lt;E&gt;, which no Go value is).
    /// </summary>
    internal static nint[]? ValueDims(in T value)
    {
        List<nint> dims = [];
        object? element = value;

        while (element is IArray array and not ISlice)
        {
            if (array.Source is null)
                return null;

            dims.Add(array.Length);

            if (array.Length == 0)
                break;

            element = array[0];
        }

        return [.. dims];
    }

    /// <summary>
    /// The capacity Go's growslice gives a slice grown to <paramref name="newLen"/> elements, from
    /// <c>nextslicecap</c>'s <paramref name="newCap"/>: the bytes <see cref="GoSizeClasses.RoundUpSize"/>
    /// provides for them, in whole elements. A <see cref="SizedByValue"/> type reads its size and pointer facts
    /// from <paramref name="valueDims"/> (<see cref="ValueDims"/> of an appended element), and a zero-size one
    /// takes Go's zero-size answer, a capacity of exactly <paramref name="newLen"/>. An element whose size is not
    /// derivable keeps <paramref name="newCap"/> and is recorded in
    /// <see cref="GoSizeClasses.UnroundedElementTypes"/>, as does a byte count past what the address space holds,
    /// where Go panics and the caller's own ceiling answers first.
    /// </summary>
    internal static nint RoundUpCapacity(nint newLen, nint newCap, nint[]? valueDims)
    {
        nuint size = Size;
        bool noScan = NoScan;

        if (SizedByValue)
        {
            if (valueDims is null)
            {
                GoSizeClasses.RecordUnroundedElementType(typeof(T));
                return newCap;
            }

            if (valueDims is [.., 0])
                return newLen;

            if (!GoReflect.TryGoSizeOf(typeof(T), valueDims, out size))
            {
                GoSizeClasses.RecordUnroundedElementType(typeof(T));
                return newCap;
            }

            if (size == 0)
                return newLen;

            noScan = GoReflect.GoPtrBytesOf(typeof(T), valueDims) == 0;
        }

        if (size == 0 || newCap <= 0 || (nuint)newCap > nuint.MaxValue / size)
            return newCap;

        nuint capacity = GoSizeClasses.RoundUpSize((nuint)newCap * size, noScan) / size;

        return capacity > (nuint)nint.MaxValue ? newCap : (nint)capacity;
    }
}
