// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (managed-referent pointer family — runtime2.go).
//
// Go's guintptr/puintptr/muintptr hide a *g / *p / *m inside a uintptr so the Go GC does not
// observe the reference (Go runtime code then re-anchors those objects manually). The CLR has
// the OPPOSITE requirement: a managed reference stored as a number is invisible to the .NET GC,
// so the referent can be collected or moved and the number is garbage. The managed conversion
// therefore stores the ж<T> box DIRECTLY and the numeric form never exists (model precedent:
// core/sync/atomic Pointer<T>). The converter skips the auto forms of these declarations via
// the manualConversionTypes/manualConversionFuncs registry (go2cs/manualTypeOperations.go);
// this module marker also causes go2cs to skip re-converting this file wholesale
// (containsManualConversionMarker, go2cs/directiveOperations.go), and the overlay restores it
// over auto output on every reconversion.
//
// Numeric escapes are deliberate, loud, and diagnostic-only:
//  - converting the integer 0 (Go nil for these types) yields the nil value; any OTHER number
//    panics — a number can never faithfully become a managed reference;
//  - converting TO a number (print/hex diagnostics) yields a stable object-identity hash,
//    which is an opaque token, never an address.
[module: GoManualConversion]

namespace go;

using System.Runtime.CompilerServices;
using System.Threading;

partial class runtime_package {

// A guintptr holds a goroutine pointer; in the original it is a uintptr deliberately hidden
// from Go's GC. The managed form holds the ж<g> box itself — see the module header.
internal partial struct Δguintptr {
    internal ж<g> m_ref;

    public Δguintptr(ж<g> gp) {
        m_ref = gp;
    }

    // Go code assigns and compares the untyped constant 0 (nil) to these types; any other
    // number cannot become a managed reference and panics loudly rather than mis-routing.
    public static implicit operator Δguintptr(nint value) {
        if (value != 0)
            throw panic("guintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static explicit operator Δguintptr(nuint value) {
        if (value != 0)
            throw panic("guintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    // Diagnostic identity for print/hex sites: an opaque stable token, never an address.
    public static explicit operator nuint(Δguintptr value) {
        return value.m_ref is null ? 0u : (nuint)(uint)RuntimeHelpers.GetHashCode(value.m_ref);
    }

    // Direct uintptr bridges: C#'s user-conversion "encompassing" rules count only STANDARD
    // conversions, so nothing chains through nuint's user bridge — `(uintptr)mp` (print
    // diagnostics) and the reverse need their own operators (same loud/token semantics).
    public static explicit operator uintptr(Δguintptr value) {
        return (nuint)value;
    }

    public static explicit operator Δguintptr(uintptr value) {
        if (value != 0)
            throw panic("guintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static implicit operator Δhex(Δguintptr value) {
        return (Δhex)(ulong)(nuint)value;
    }

    public static bool operator ==(Δguintptr left, Δguintptr right) {
        return ReferenceEquals(left.m_ref, right.m_ref);
    }

    public static bool operator !=(Δguintptr left, Δguintptr right) {
        return !(left == right);
    }

    public static bool operator ==(Δguintptr left, nint right) {
        return right == 0 && left.m_ref is null;
    }

    public static bool operator !=(Δguintptr left, nint right) {
        return !(left == right);
    }

    public override bool Equals(object? obj) {
        return obj is Δguintptr other && this == other;
    }

    public override int GetHashCode() {
        return m_ref is null ? 0 : RuntimeHelpers.GetHashCode(m_ref);
    }

    public override string ToString() {
        return ((ulong)(nuint)this).ToString();
    }
}

//go:nosplit
internal static ж<g> ptr(this Δguintptr gp) {
    return gp.m_ref;
}

//go:nosplit
[GoRecv] internal static void set(this ref Δguintptr gp, ж<g> Ꮡg) {
    gp.m_ref = Ꮡg;
}

//go:nosplit
[GoRecv] internal static bool cas(this ref Δguintptr gp, Δguintptr old, Δguintptr @new) {
    return ReferenceEquals(Interlocked.CompareExchange(ref gp.m_ref, @new.m_ref, old.m_ref), old.m_ref);
}

//go:nosplit
internal static Δguintptr guintptr(this ж<g> Ꮡgp) {
    return new Δguintptr(Ꮡgp);
}

// runqempty reports whether pp has no Gs on its local run queue. It never returns true spuriously.
//
// Hand-owned (proc.go, all three GOOS): Go reads runnext as `atomic.Loaduintptr((*uintptr)
// (unsafe.Pointer(&pp.runnext)))` -- a guintptr viewed as the number it hides. The managed guintptr
// holds the ж<g> box itself (the module header), so that view has no referent: golib refuses the
// reinterpret because the pointee carries a reference, the conversion falls to the address route,
// and Loaduintptr's Volatile.Read dereferences an order token -- the host dies (arm-2a). All Go asks
// of the number is whether it is 0, i.e. whether runnext is nil, which here is the managed reference
// being null, read with the same acquire. The loop and the re-read of runqtail are Go's, unchanged.
internal static bool runqempty(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.Value;

    while (true) {
        var head = global::go.@internal.runtime.atomic_package.Load(Ꮡpp.of(Δp.Ꮡrunqhead));
        var tail = global::go.@internal.runtime.atomic_package.Load(Ꮡpp.of(Δp.Ꮡrunqtail));
        var runnextNil = Volatile.Read(ref pp.runnext.m_ref) is null;

        if (tail == global::go.@internal.runtime.atomic_package.Load(Ꮡpp.of(Δp.Ꮡrunqtail)))
            return head == tail && runnextNil;
    }
}

// setGNoWB performs *gp = new without a write barrier.
// For times when it's impractical to use a guintptr.
//
//go:nosplit
//go:nowritebarrier
internal static void setGNoWB(ж<ж<g>> Ꮡgp, ж<g> Ꮡnew) {
    Ꮡgp.Value = Ꮡnew;
}

internal partial struct puintptr {
    internal ж<Δp> m_ref;

    public puintptr(ж<Δp> pp) {
        m_ref = pp;
    }

    public static implicit operator puintptr(nint value) {
        if (value != 0)
            throw panic("puintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static explicit operator puintptr(nuint value) {
        if (value != 0)
            throw panic("puintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static explicit operator nuint(puintptr value) {
        return value.m_ref is null ? 0u : (nuint)(uint)RuntimeHelpers.GetHashCode(value.m_ref);
    }

    // Direct uintptr bridges: C#'s user-conversion "encompassing" rules count only STANDARD
    // conversions, so nothing chains through nuint's user bridge — `(uintptr)mp` (print
    // diagnostics) and the reverse need their own operators (same loud/token semantics).
    public static explicit operator uintptr(puintptr value) {
        return (nuint)value;
    }

    public static explicit operator puintptr(uintptr value) {
        if (value != 0)
            throw panic("puintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static implicit operator Δhex(puintptr value) {
        return (Δhex)(ulong)(nuint)value;
    }

    public static bool operator ==(puintptr left, puintptr right) {
        return ReferenceEquals(left.m_ref, right.m_ref);
    }

    public static bool operator !=(puintptr left, puintptr right) {
        return !(left == right);
    }

    public static bool operator ==(puintptr left, nint right) {
        return right == 0 && left.m_ref is null;
    }

    public static bool operator !=(puintptr left, nint right) {
        return !(left == right);
    }

    public override bool Equals(object? obj) {
        return obj is puintptr other && this == other;
    }

    public override int GetHashCode() {
        return m_ref is null ? 0 : RuntimeHelpers.GetHashCode(m_ref);
    }

    public override string ToString() {
        return ((ulong)(nuint)this).ToString();
    }
}

//go:nosplit
internal static ж<Δp> ptr(this puintptr pp) {
    return pp.m_ref;
}

//go:nosplit
[GoRecv] internal static void set(this ref puintptr pp, ж<Δp> Ꮡp) {
    pp.m_ref = Ꮡp;
}

// muintptr is a *m that is not tracked by the garbage collector.
//
// Because we do free Ms, there are some additional constrains on
// muintptrs:
//
//  1. Never hold an muintptr locally across a safe point.
//
//  2. Any muintptr in the heap must be owned by the M itself so it can
//     ensure it is not in use when the last true *m is released.
internal partial struct muintptr {
    internal ж<m> m_ref;

    public muintptr(ж<m> mp) {
        m_ref = mp;
    }

    public static implicit operator muintptr(nint value) {
        if (value != 0)
            throw panic("muintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static explicit operator muintptr(nuint value) {
        if (value != 0)
            throw panic("muintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static explicit operator nuint(muintptr value) {
        return value.m_ref is null ? 0u : (nuint)(uint)RuntimeHelpers.GetHashCode(value.m_ref);
    }

    // Direct uintptr bridges: C#'s user-conversion "encompassing" rules count only STANDARD
    // conversions, so nothing chains through nuint's user bridge — `(uintptr)mp` (print
    // diagnostics) and the reverse need their own operators (same loud/token semantics).
    public static explicit operator uintptr(muintptr value) {
        return (nuint)value;
    }

    public static explicit operator muintptr(uintptr value) {
        if (value != 0)
            throw panic("muintptr: a non-zero integer cannot carry a managed reference");

        return default;
    }

    public static implicit operator Δhex(muintptr value) {
        return (Δhex)(ulong)(nuint)value;
    }

    public static bool operator ==(muintptr left, muintptr right) {
        return ReferenceEquals(left.m_ref, right.m_ref);
    }

    public static bool operator !=(muintptr left, muintptr right) {
        return !(left == right);
    }

    public static bool operator ==(muintptr left, nint right) {
        return right == 0 && left.m_ref is null;
    }

    public static bool operator !=(muintptr left, nint right) {
        return !(left == right);
    }

    public override bool Equals(object? obj) {
        return obj is muintptr other && this == other;
    }

    public override int GetHashCode() {
        return m_ref is null ? 0 : RuntimeHelpers.GetHashCode(m_ref);
    }

    public override string ToString() {
        return ((ulong)(nuint)this).ToString();
    }
}

//go:nosplit
internal static ж<m> ptr(this muintptr mp) {
    return mp.m_ref;
}

//go:nosplit
[GoRecv] internal static void set(this ref muintptr mp, ж<m> Ꮡm) {
    mp.m_ref = Ꮡm;
}

// setMNoWB performs *mp = new without a write barrier.
// For times when it's impractical to use an muintptr.
//
//go:nosplit
//go:nowritebarrier
internal static void setMNoWB(ж<ж<m>> Ꮡmp, ж<m> Ꮡnew) {
    Ꮡmp.Value = Ꮡnew;
}

} // end runtime_package
