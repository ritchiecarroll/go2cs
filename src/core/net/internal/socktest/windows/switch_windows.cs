// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.@internal;

using syscall = syscall_package;

partial class socktest_package {

partial struct ΔSockets /*map[syscallꓸHandle, Status]*/;

internal static ж<Status> sockso(this ж<Switch> Ꮡsw, syscallꓸHandle s) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var sw = ref Ꮡsw.DerefOrNull();

        Ꮡsw.of(Switch.Ꮡsmu).RLock();
        ᒐd1 = true;
        ref var so = ref heap<Status>(out var Ꮡso);
        (so, var ok) = sw.sotab[s, ꟷ];
        if (!ok) {
            return default!;
        }
        return Ꮡso;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡsw.of(Switch.Ꮡsmu).RUnlock(); ᒐ.Run(); }
}

// addLocked returns a new Status without locking.
// sw.smu must be held before call.
internal static ж<Status> addLocked(this ж<Switch> Ꮡsw, syscallꓸHandle s, nint family, nint sotype, nint proto) {
    ref var sw = ref Ꮡsw.DerefOrNull();

    Ꮡsw.of(Switch.Ꮡonce).Do(Ꮡsw.init);
    ref var so = ref heap<Status>(out var Ꮡso);
    so = new Status(Cookie: cookie(family, sotype, proto));
    sw.sotab[s] = so;
    return Ꮡso;
}

} // end socktest_package
