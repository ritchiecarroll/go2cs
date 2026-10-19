// GoSizeClasses.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;

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
/// toolchain's, and proves that taking the smallest class at least as large as the request is exactly Go's
/// <c>SizeToSizeClass8</c>/<c>SizeToSizeClass128</c> lookup for every small size.
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

    /// <summary>Go's <c>MinSizeForMallocHeader</c>: <c>goarch.PtrSize * goarch.PtrBits</c>, 8 * 64 on 64-bit.</summary>
    public const nuint MinSizeForMallocHeader = 512;

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
}

/// <summary>
/// The two facts Go's growslice reads from an element type, read once per closed <typeparamref name="T"/>:
/// its Go size and whether it holds pointers (Go's <c>noscan</c> is the inverse).
/// </summary>
internal static class GoGrowthFacts<T>
{
    /// <summary>The Go (amd64) size of <typeparamref name="T"/>, or 0 when it is not derivable.</summary>
    internal static readonly nuint Size = GoReflect.TryGoSizeOf(typeof(T), null, out nuint size) ? size : 0;

    /// <summary>Whether <typeparamref name="T"/> holds no pointer; an unknown pointer prefix is not noscan.</summary>
    internal static readonly bool NoScan = GoReflect.GoPtrBytesOf(typeof(T)) == 0;

    /// <summary>
    /// The capacity Go's growslice gives <paramref name="newCap"/> elements: the bytes
    /// <see cref="GoSizeClasses.RoundUpSize"/> provides for them, in whole elements. An element whose size is
    /// not derivable keeps <paramref name="newCap"/>, as does a byte count past what the address space holds,
    /// where Go panics and the caller's own ceiling answers first.
    /// </summary>
    internal static nint RoundUpCapacity(nint newCap)
    {
        nuint size = Size;

        if (size == 0 || newCap <= 0 || (nuint)newCap > nuint.MaxValue / size)
            return newCap;

        nuint capacity = GoSizeClasses.RoundUpSize((nuint)newCap * size, NoScan) / size;

        return capacity > (nuint)nint.MaxValue ? newCap : (nint)capacity;
    }
}
