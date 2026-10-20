// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Malloc small size classes.
//
// See malloc.go for overview.
// See also mksizeclasses.go for how we decide what size classes to use.
namespace go;

using gc = @internal.runtime.gc_package;
using @internal.runtime;

partial class runtime_package {

// Returns size of the memory block that mallocgc will allocate if you ask for the size,
// minus any inline space for metadata.
internal static uintptr /*reqSize*/ roundupsize(uintptr size, bool noscan) {
    uintptr reqSize = default!;

    reqSize = size;
    if (reqSize <= (uintptr)(maxSmallSize - gc.MallocHeaderSize)) {
        // Small object.
        if (!noscan && reqSize > gc.MinSizeForMallocHeader) { // !noscan && !heapBitsInSpan(reqSize)
            reqSize += gc.MallocHeaderSize;
        }
        // (reqSize - size) is either mallocHeaderSize or 0. We need to subtract mallocHeaderSize
        // from the result if we have one, since mallocgc will add it back in.
        if (reqSize <= (uintptr)(gc.SmallSizeMax - 8)) {
            return (uintptr)gc.SizeClassToSize[gc.SizeToSizeClass8[divRoundUp(reqSize, gc.SmallSizeDiv)]] - (reqSize - size);
        }
        return (uintptr)gc.SizeClassToSize[gc.SizeToSizeClass128[divRoundUp(reqSize - (uintptr)gc.SmallSizeMax, gc.LargeSizeDiv)]] - (reqSize - size);
    }
    // Large object. Align reqSize up to the next page. Check for overflow.
    reqSize += pageSize - 1;
    if (reqSize < size) {
        return size;
    }
    return (uintptr)(reqSize & ~(uintptr)(pageSize - 1));
}

} // end runtime_package
