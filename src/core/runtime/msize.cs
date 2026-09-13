// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Malloc small size classes.
//
// See malloc.go for overview.
// See also mksizeclasses.go for how we decide what size classes to use.
namespace go;

partial class runtime_package {

// Returns size of the memory block that mallocgc will allocate if you ask for the size,
// minus any inline space for metadata.
internal static uintptr /*reqSize*/ roundupsize(uintptr size, bool noscan) {
    uintptr reqSize = default!;

    reqSize = size;
    if (reqSize <= (uintptr)(maxSmallSize - mallocHeaderSize)) {
        // Small object.
        if (!noscan && reqSize > minSizeForMallocHeader) {
            // !noscan && !heapBitsInSpan(reqSize)
            reqSize += mallocHeaderSize;
        }
        // (reqSize - size) is either mallocHeaderSize or 0. We need to subtract mallocHeaderSize
        // from the result if we have one, since mallocgc will add it back in.
        if (reqSize <= (uintptr)(smallSizeMax - 8)) {
            return (uintptr)class_to_size[size_to_class8[(nint)(divRoundUp(reqSize, smallSizeDiv))]] - (reqSize - size);
        }
        return (uintptr)class_to_size[size_to_class128[(nint)(divRoundUp(reqSize - (uintptr)smallSizeMax, largeSizeDiv))]] - (reqSize - size);
    }
    // Large object. Align reqSize up to the next page. Check for overflow.
    reqSize += pageSize - 1;
    if (reqSize < size) {
        return size;
    }
    return (uintptr)(reqSize & ~(uintptr)(pageSize - 1));
}

} // end runtime_package
