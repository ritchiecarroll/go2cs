// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export guts for testing.
// Since testing imports os and os imports internal/poll,
// the internal/poll tests can not be in package poll.
namespace go.@internal;

using static go.@internal.poll_package;

partial class poll_internal_test_package {

public static Action<ж<slice<slice<byte>>>, int64> Consume = (ж<slice<slice<byte>>> ᴛ0, int64 ᴛ1) => consume(ref ᴛ0.DerefOrNull(), ᴛ1);

public partial struct XFDMutex {
    internal partial ref global::go.@internal.poll_package.fdMutex fdMutex { get; }
}

public static bool Incref(this ж<XFDMutex> Ꮡmu) {
    return Ꮡmu.of(XFDMutex.ᏑfdMutex).incref();
}

public static bool IncrefAndClose(this ж<XFDMutex> Ꮡmu) {
    return Ꮡmu.of(XFDMutex.ᏑfdMutex).increfAndClose();
}

public static bool Decref(this ж<XFDMutex> Ꮡmu) {
    return Ꮡmu.of(XFDMutex.ᏑfdMutex).decref();
}

public static bool RWLock(this ж<XFDMutex> Ꮡmu, bool read) {
    return Ꮡmu.of(XFDMutex.ᏑfdMutex).rwlock(read);
}

public static bool RWUnlock(this ж<XFDMutex> Ꮡmu, bool read) {
    return Ꮡmu.of(XFDMutex.ᏑfdMutex).rwunlock(read);
}

} // end poll_internal_test_package
