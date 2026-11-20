// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;
using static unix_package;

partial class unix_package {

partial struct mremapMmapper {
    /*embed*/ internal mmapper mmapper;
    internal Func<uintptr, uintptr, uintptr, nint, uintptr, (uintptr xaddr, error err)> mremap;
}

internal static ж<mremapMmapper> mapper;
internal static void initᴛmapper() { mapper = Ꮡ(new mremapMmapper(
    mmapper: new mmapper(
        active: new map<ж<byte>, slice<byte>>(),
        mmap: mmap,
        munmap: munmap
    ),
    mremap: mremap
)); }

} partial class unixᴛmremapMmapper {

internal static (slice<byte> data, error err) Mremap(this ж<mremapMmapper> Ꮡm, slice<byte> oldData, nint newLength, nint flags) {
    slice<byte> data = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        if (newLength <= 0 || len(oldData) == 0 || len(oldData) != cap(oldData) || (flags & (nint)mremapFixed) != 0) {
            (data, err) = (default, EINVAL); goto ᒐdone;
        }

        var pOld = Ꮡ(oldData, cap(oldData) - 1);
        Ꮡm.of(mremapMmapper.Ꮡmmapper).of(mmapper.ᏑMutex).Lock();
        defer(Ꮡm.of(mremapMmapper.Ꮡmmapper).of(mmapper.ᏑMutex).Unlock, ref ᒐ);
        var bOld = m.active[pOld];
        if (bOld == nil || Ꮡelem(bOld, 0) != Ꮡelem(oldData, 0)) {
            (data, err) = (default, EINVAL); goto ᒐdone;
        }
        var (newAddr, errno) = m.mremap((uintptr)Ꮡ(bOld, 0), (uintptr)len(bOld), (uintptr)newLength, flags, 0);
        if (errno != nil) {
            (data, err) = (default, errno); goto ᒐdone;
        }
        var bNew = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)newAddr), newLength);
        var pNew = Ꮡ(bNew, cap(bNew) - 1);
        if ((flags & (nint)mremapDontunmap) == 0) {
            delete(m.active, pOld);
        }
        m.active[pNew] = bNew;
        (data, err) = (bNew, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (data, err);
}

} partial class unix_package {

public static (slice<byte> data, error err) Mremap(slice<byte> oldData, nint newLength, nint flags) {
    return mapper.Mremap(oldData, newLength, flags);
}

public static (@unsafe.Pointer ret, error err) MremapPtr(@unsafe.Pointer oldAddr, uintptr oldSize, @unsafe.Pointer newAddr, uintptr newSize, nint flags) {
    error err = default;

    (var xaddr, err) = mapper.mremap((uintptr)oldAddr, oldSize, newSize, flags, (uintptr)newAddr);
    return ((@unsafe.Pointer)xaddr, err);
}

} // end unix_package
