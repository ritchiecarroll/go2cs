// ж.PointerTokens.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

// ---------------------------------------------------------------------------------------------
// POINTER TOKEN RECOVERY — making `unsafe.Pointer` round-trip for a MANAGED pointer.
// ---------------------------------------------------------------------------------------------
//
// A Go pointer to managed storage has no machine address to report, so every projection of one to
// a scalar — `uintptr(unsafe.Pointer(p))`, `reflect.Value.Pointer`, `reflect.Value.UnsafePointer` —
// answers with a stable ORDER TOKEN instead (see INilPointer.PointerOrderToken, whose own remarks
// say plainly that tokens "are order keys, never an identity substitute").
//
// That contract is sufficient for every consumer that only ORDERS or NIL-TESTS the result — fmt's
// `%p`, internal/fmtsort's map-key ordering — and it was written for exactly those. It is NOT
// sufficient for the other direction Go permits: converting the scalar back to a pointer and
// dereferencing it.
//
//     ps := (*bool)(v.FieldByName(name).Addr().UnsafePointer())   // go/types check_test.go:345
//     *ps = true
//
// Emitted, that is `(ж<bool>)(uintptr)(…)` followed by a `.Value` store — and the uintptr operator
// builds a NATIVE-address box over whatever number it was handed. Storing a bool at the numeric
// value of an order token writes to an arbitrary address: an access violation when the page is not
// mapped, and silent heap corruption when it is. It killed `go/types`' converted test host outright
// at the first test that reaches the idiom (TestCheck/blank.go), 542 verdicts behind it.
//
// The information needed to do better is never actually lost — `reflect.Value.Addr` surfaces the
// real aliasing ж box, and only the projection to a scalar discards it. So this table remembers the
// association the projection drops: the token that was handed out, and the box it named. The uintptr
// operator consults it FIRST, and a token that came from here recovers its own box and aliases the
// original storage exactly as Go's pointer would. Anything else keeps the pre-existing native-address
// route, unchanged.
//
// WHY THE TOKEN VALUE IS NOT CHANGED. The obvious alternative — mint handles from a reserved numeric
// range so a token is self-identifying — would also change what `%p` prints and what order pointer-
// keyed maps print in, because those read the very same token. Keeping the value byte-identical and
// carrying the association out of band costs one dictionary probe on a conversion that was already
// building an object, and leaves every existing observable untouched.
//
// COLLISION. A real machine address that numerically equals a live token would resolve to that
// token's box instead of the address. The window is remote in practice — the table only ever holds
// tokens that a reflect projection actually handed out, which is a handful — and the resolution is
// verified against the box's own current token before it is used, so a stale or reused entry can
// never answer. Where it did collide, the effect is a managed access in place of a wild one, i.e. it
// fails safe relative to the behavior this replaces.
//
// LIFETIME. Entries are weak: this table must never be the reason a box stays alive, or every
// pointer fmt ever printed would leak. Dead entries are swept opportunistically as the table grows.
//
// PUBLIC because the only party that mints these tokens is the hand-owned reflection bridge in the
// separate `reflect` assembly (reflect/value_impl.cs), the same way GoReflect is public for it. It is
// a runtime seam, not part of any Go surface.
public static class ManagedPointerTokens
{
    // minted opaque box → the referent box it stands for. What makes a MINTED opaque pointer behave
    // like the Go pointer it converts: holding the scalar-valued box keeps the referent reachable,
    // exactly as holding the real pointer would, so a boundary wrapper that resolves the token
    // mid-call can never lose a race against the collector — the emitted mint's referent is
    // otherwise reachable only through a local the JIT is free to retire before the call it feeds.
    // ConditionalWeakTable so the tie is exactly the minted box's own lifetime: when nothing holds
    // the mint, the referent is again governed by its own reachability alone, and the weak Register
    // entry beside it dies with the referent as designed.
    private static readonly ConditionalWeakTable<object, object> s_mintedReferents = new();

    // derived reinterpret box → the SOURCE box it was reinterpreted from, for the one class the
    // address-keyed table above provably cannot serve: a pointee that carries managed references
    // has no pinnable storage, so no provenance entry can ever validate on read and the scalar is
    // not an address a moment later either. Keyed on the DERIVED BOX rather than on a number, so
    // there is nothing to validate and nothing to go stale; ConditionalWeakTable so the tie is
    // exactly the derived pointer's own lifetime, the same lifetime doctrine as s_mintedReferents.
    // Written only from PointerExtensions.Reinterpret's unpinnable arm — see the rationale there.
    private static readonly ConditionalWeakTable<object, object> s_reinterpretSources = new();

    // token → the box that token named. WeakReference so a remembered pointer is still collectable.
    //
    // CONCURRENT, and read WITHOUT a lock, because Resolve sits on the `uintptr → ж<T>` conversion
    // operator — 875 emitted call sites across the corpus, 54 of them in the syscall wrappers. A
    // global lock there would serialize goroutines through a conversion that is otherwise free.
    private static readonly ConcurrentDictionary<nuint, WeakReference<object>> s_table = new();

    // The overwhelmingly common case is a program that never asks reflect for a pointer's scalar
    // form at all, and it must not pay even a hash: an empty table answers from this one load.
    // Volatile because the writer is a different thread than the reader in the general case.
    //
    // The fast path is exact for the sequence that matters. A round trip PROJECTS a pointer and then
    // CONVERTS the scalar back on ONE thread — `(*bool)(v.Addr().UnsafePointer())` is a single
    // expression — so the registration always happens-before the resolution that depends on it. A
    // reader on some OTHER thread racing the very first registration may still see zero and take the
    // native-address route, which is exactly what it would have done before this table existed: the
    // fast path can lose a race it was never in a position to win.
    private static volatile int s_count;

    // Guards Sweep alone — registration and resolution never take it. Sweeping is rare and its
    // cost is proportional to the table, so one sweeper at a time is the point, not a bottleneck.
    private static readonly object s_sweepLock = new();

    // Sweep dead entries when the table has grown by this much since the last sweep. The table is
    // expected to hold a handful of live entries; the threshold exists so a program that projects
    // many short-lived pointers cannot grow it without bound.
    internal const int SweepThreshold = 256;

    private static int s_sweepAt = SweepThreshold;

    /// <summary>
    /// The number of token registrations the table currently holds, live and not-yet-swept alike.
    /// Read by the registry's growth guard (GolibTests), which measures the bound the sweep policy
    /// claims — a claim about growth is a prediction until a run reads it.
    /// </summary>
    internal static int RegisteredCount => s_count;

    /// <summary>
    /// Remembers that <paramref name="token"/> was handed out as the scalar form of
    /// <paramref name="box"/>, so a conversion back to a pointer can recover it.
    /// </summary>
    /// <remarks>
    /// A zero token is the reserved nil form and is never registered. Re-registering a token simply
    /// refreshes it: two projections of one pointer produce one token, and a token whose box has
    /// been collected is free to be reused by whatever the runtime hands the same identity to next.
    /// </remarks>
    public static void Register(nuint token, object box)
    {
        if (token == 0 || box is null)
            return;

        // Already remembered — return without allocating or writing. This is the steady state, not
        // an edge case: `fmt` projects a pointer through this on every `%p` and on every nil-test of
        // a pointer, map, func or channel it prints, so printing one value in a loop would otherwise
        // allocate a WeakReference and take a bucket lock per iteration to store what is already
        // there.
        if (s_table.TryGetValue(token, out WeakReference<object>? existing) &&
            existing.TryGetTarget(out object? remembered) &&
            ReferenceEquals(remembered, box))
        {
            return;
        }

        s_table[token] = new WeakReference<object>(box);
        s_count = s_table.Count;

        if (s_count >= s_sweepAt)
            Sweep();
    }

    /// <summary>
    /// Remembers that <paramref name="address"/> is the PINNED address of managed storage behind
    /// <paramref name="box"/> -- the provenance record of
    /// docs/phase4/DESIGN-pointer-provenance.md (RATIFIED), registered by the pointer-to-scalar
    /// conversions at the moment they pin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The withdrawn native-array-view SAFETY FLOOR proved (six behavioral counterexamples, three
    /// classes) that no test on a pointee TYPE can decide whether reinterpreting an address is
    /// sound -- the deciding fact is where the ADDRESS CAME FROM, and before this record nothing
    /// carried that. With pins registered, a Resolve MISS becomes the meaningful statement
    /// "this address is not managed storage this process pinned": the exact predicate the floor
    /// needed and could not express, and the arm-selection test <c>unsafe.Slice</c> needs for the
    /// same ambiguity one container over.
    /// </para>
    /// <para>
    /// OVERWRITE-on-register, per the ratified OQ-P2 refinement: the latest pin owns the address,
    /// so same-storage re-pins are benign and an entry left by a DEAD box is displaced the moment
    /// the address is legitimately reused. The ABA residue -- a stale entry whose box is still
    /// alive but no longer pinned there -- is closed at READ instead (see Resolve), so the record
    /// needs no type key and no eager invalidation.
    /// </para>
    /// <para>
    /// Cost, measured before the mechanism was built (gate #1): ~163 bytes per DISTINCT pin and
    /// ~0 per repeat; the heaviest socket row on the roster (crypto/tls, 47.5k pins) holds ~500
    /// RESIDENT entries (~88 KB) under exactly these semantics, flat across the run -- the weak
    /// tie keeps the table at the LIVE population, not the cumulative one.
    /// </para>
    /// </remarks>
    public static void RegisterPinned(nuint address, object box)
    {
        if (address == 0 || box is null)
            return;

        // The same no-allocation steady state Register keeps: a repeated pin of the same storage
        // reports the same address, and 70% of real pins are repeats (gate #1's census).
        if (s_table.TryGetValue(address, out WeakReference<object>? existing) &&
            existing.TryGetTarget(out object? remembered) &&
            ReferenceEquals(remembered, box))
        {
            return;
        }

        s_table[address] = new WeakReference<object>(box);
        s_count = s_table.Count;

        if (s_count >= s_sweepAt)
            Sweep();
    }

    /// <summary>
    /// Converts a Go pointer to the opaque pointer-to-empty-struct form (Go's
    /// <c>type Pointer *struct{}</c>, e.g. <c>syscall.Pointer</c>) that Windows type definitions
    /// use for a "pointer to one of many types" field — preserving the REFERENT when the pointee
    /// has no native address to give.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The converter emits this for the conversion <c>T(unsafe.Pointer(p))</c> where <c>T</c>'s
    /// underlying type is <c>*struct{}</c> and <c>p</c> is a Go pointer. The numeric route that
    /// emission previously took — <c>(T)(ж&lt;EmptyStruct&gt;)(uintptr)(new @unsafe.Pointer(p))</c> —
    /// projects the box to a scalar at the <c>@unsafe.Pointer</c> constructor, and for a pointee
    /// CARRYING MANAGED REFERENCES that scalar is a transient GC-heap address with no recoverable
    /// box behind it (golib's uintptr operator pins only reference-free storage). crypto/x509's
    /// <c>checkChainSSLServerPolicy</c> is the measured victim: its
    /// <c>SSL_EXTRA_CERT_CHAIN_POLICY_PARA</c> — whose ServerName is itself a pointer — crossed
    /// into <c>CertVerifyCertificateChainPolicy</c> as exactly such an address, and crypt32
    /// reading 24 native bytes off it was an ACCESS_VIOLATION or a garbage verdict depending on
    /// the day's heap layout. This is the mint-site problem named in
    /// docs/phase4/BOARD-next-validation-candidates.md.
    /// </para>
    /// <para>
    /// The classes the numeric route already answers exactly keep it byte for byte: a nil pointer
    /// is address 0, a native-backed box aliases its real address, and a reference-free pointee
    /// pins and reports stable storage. Only the reference-bearing class diverges — its scalar
    /// becomes the box's own <see cref="INilPointer.PointerOrderToken"/>, registered here so the
    /// consuming boundary wrapper recovers the box with <see cref="Resolve"/> (the same round trip
    /// the ADDRINFOW hand-own mints for its sockaddr fields — this is the third minter). The
    /// returned box additionally holds the referent reachable for its own lifetime, so the minted
    /// pointer keeps its pointee alive exactly as the Go pointer it stands for would.
    /// </para>
    /// </remarks>
    public static ж<EmptyStruct> MintOpaque<T>(ж<T>? box)
    {
        if (box is null || box.IsNative || !RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return new NativeBox<EmptyStruct>((nuint)(uintptr)box!);

        nuint token = box.PointerOrderToken;

        // A nil-constructed box tokens 0, which is the nil form on the numeric route too.
        if (token == 0)
            return new NativeBox<EmptyStruct>((nuint)0);

        Register(token, box);

        ж<EmptyStruct> minted = new NativeBox<EmptyStruct>(token);

        s_mintedReferents.Add(minted, box);

        return minted;
    }

    /// <summary>
    /// Records that <paramref name="derived"/> is the reinterpretation of <paramref name="source"/>,
    /// for a source whose storage cannot be pinned and whose address therefore names nothing a
    /// boundary wrapper could read.
    /// </summary>
    /// <remarks>
    /// Called only from <c>PointerExtensions.Reinterpret</c>'s unpinnable arm, which owns the
    /// narrowing (reference-bearing source, reference-free destination — the
    /// <c>(*byte)(unsafe.Pointer(&amp;record))</c> boundary idiom, never reflect's prefix downcast).
    /// </remarks>
    public static void RememberReinterpretSource(object derived, object source)
    {
        if (derived is null || source is null)
            return;

        // AddOrUpdate rather than Add: one source box can legitimately be reinterpreted twice, and
        // a repeat must not throw the way Add does on a duplicate key.
        s_reinterpretSources.AddOrUpdate(derived, source);
    }

    /// <summary>
    /// Recovers the box <paramref name="derived"/> was reinterpreted FROM, or <c>null</c> when it
    /// was not produced by the unpinnable reinterpret arm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the seam a hand-owned boundary wrapper uses when it is handed a pointer whose
    /// managed identity the address model has already discarded — the struct-passing class where
    /// the record reaching the wrapper is a <c>*byte</c> rather than a typed pointer, so the
    /// mirror-and-copy remedy has nothing to copy FROM. <c>internal/syscall/windows.NetShareAdd</c>
    /// is the worked instance.
    /// </para>
    /// <para>
    /// A <c>null</c> answer is a real answer and callers must treat it as one: it means the pointer
    /// is either genuinely native or came from a route that keeps its address meaningful, and a
    /// wrapper that cannot proceed without the source must say so loudly rather than read the
    /// scalar as a record.
    /// </para>
    /// </remarks>
    public static object? ReinterpretSource(object derived)
    {
        if (derived is null)
            return null;

        return s_reinterpretSources.TryGetValue(derived, out object? source) ? source : null;
    }

    /// <summary>
    /// Recovers the box <paramref name="token"/> was handed out for, or <c>null</c> when the token
    /// did not come from a reflect pointer projection, or its box has since been collected.
    /// </summary>
    public static object? Resolve(nuint token)
    {
        // The fast path every non-reflect program takes: nothing was ever registered, so no token
        // can resolve and the conversion goes straight to its native-address route.
        if (token == 0 || s_count == 0)
            return null;

        if (!s_table.TryGetValue(token, out WeakReference<object>? weak))
            return null;

        if (!weak.TryGetTarget(out object? box))
        {
            if (s_table.TryRemove(token, out _))
                s_count = s_table.Count;

            return null;
        }

        // Verify the box still answers for this scalar before handing it back -- by ORDER TOKEN
        // for a projection entry, or by CURRENT PINNED ADDRESS for a provenance entry
        // (validate-on-read, the ratified OQ-P2 refinement: alive + still pinned there, else
        // MISS). A stale entry -- dead box, released pin, re-identified token -- must never
        // resolve, because the number may by now be a real native address that never carried it.
        if (CurrentToken(box) == token)
            return box;

        return box is INilPointer pointer && pointer.IsPinnedAt(token) ? box : null;
    }

    // The token the box would report today — the same projection reflect used to mint the entry.
    private static nuint CurrentToken(object box)
    {
        return box switch
        {
            INilPointer p => p.PointerOrderToken,
            IChannel c => c.PointerOrderToken,
            _ => (nuint)(uint)RuntimeHelpers.GetHashCode(box)
        };
    }

    // Drops entries whose box has been collected. One sweeper at a time; concurrent registrations
    // and resolutions continue against the table throughout.
    private static void Sweep()
    {
        if (!Monitor.TryEnter(s_sweepLock))
            return;

        try
        {
            // Re-check under the gate: a thread that queued behind a sweep has nothing left to do.
            if (s_table.Count < s_sweepAt)
                return;

            foreach ((nuint token, WeakReference<object> weak) in s_table)
            {
                if (!weak.TryGetTarget(out _))
                    s_table.TryRemove(token, out _);
            }

            // Re-arm above the surviving population so a table that is legitimately large does not
            // sweep on every registration.
            s_count = s_table.Count;
            s_sweepAt = s_count + SweepThreshold;
        }
        finally
        {
            Monitor.Exit(s_sweepLock);
        }
    }
}
