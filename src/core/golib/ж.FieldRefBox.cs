// ж.FieldRefBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The STRUCT-FIELD reference kind — a pointer into a field of another allocation (Go's
/// <c>&amp;x.field</c>, the <c>of()</c> chains). One of the four kinds of <see cref="ж{T}"/>
/// under the B1 per-kind split (<c>docs/phase4/DESIGN-zh-box-b1.md</c> §3).
/// </summary>
/// <remarks>
/// The identity doctrine is unchanged from the pre-split box, moved here verbatim:
/// <see cref="m_token"/> carries the ORIGINAL accessor delegate when <see cref="m_accessor"/> is
/// a per-call closure wrapper (the typed <c>of(…)</c> overload), because equality must compare
/// the FIELD, not the wrapper instance — <c>&amp;x.f == &amp;x.f</c> is Go pointer identity, and
/// the address-keyed runtime semaphores in the hand-owned sync/internal-poll implementations
/// depend on it. <c>Delegate.Equals</c> compares method + target, so two conversions of the same
/// accessor method group compare equal across call sites.
/// </remarks>
/// <summary>
/// Whether a pointer's storage is NATIVE memory, recursively through an <c>of()</c> chain — the one
/// question equality needs answered across kinds without making <see cref="ж{T}.NativeAddress"/>
/// (which gates the native-word accessors) mean something new. Non-generic so a field reference can ask
/// its source, which it holds as <c>object</c>.
/// </summary>
internal interface INativeRooted
{
    bool IsNativeRooted { get; }
}

public sealed class FieldRefBox<T> : ж<T>, INativeRooted
{
    private readonly object m_source;
    private readonly FieldRefFunc<T> m_accessor;
    private readonly Delegate m_token;

    // The field-view cache's slot on a VIEW (ж.Views.cs): a chain's inner hop
    // (`Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex)`) caches here at the slot's cost rather than the weak
    // table's — 8 B per distinct (box, field) pair, since a view exists only because an of() happened.
    internal ViewEntry? m_views;

    // Create a new reference to a field in a heap allocated struct. fieldIdentity carries the
    // original accessor delegate when fieldRefFunc is a per-call closure wrapper (see the typed
    // `of(...)` overload) so pointer equality compares the FIELD, not the wrapper instance.
    internal FieldRefBox(object source, FieldRefFunc<T> fieldRefFunc, Delegate? fieldIdentity = null)
    {
        m_source = source;
        m_accessor = fieldRefFunc;
        m_token = fieldIdentity ?? fieldRefFunc;

        // The box only. The accessor delegate is the caller's (typically a compiler-cached
        // static), and any per-call closure wrapper is charged where it is minted. Leaf-ctor
        // counting per the B1 split — same charge as before it.
        AllocationCounter.Count();
    }

    /// <inheritdoc/>
    public override ref T Value => ref m_accessor(m_source);

    /// <inheritdoc/>
    public override ref T ValueSlot => ref m_accessor(m_source);

    // A field reference resolves through Value without a null check — the base's IsNull default
    // (m_isNull, always false for this kind) is exactly the pre-split predicate's answer.

    /// <inheritdoc/>
    // The source identity is resolved through an `of()` chain (SameSource), so the documented
    // invariant holds at every depth: equal pointers produce equal tokens. The offset added is
    // the field's within its IMMEDIATE parent, which is what keeps alignment composing correctly
    // down a nested chain.
    public override nuint PointerOrderToken =>
        m_pointerOrderToken != 0 ? m_pointerOrderToken :
        m_pointerOrderToken = unchecked(AllocationBase(SourceIdentityHash(m_source)) + GoFieldDisplacement(m_source, m_token));

    // The token above, computed once per view. It is a pure function of this view's immutable source
    // identity and field, and a view is cached per (box, accessor), so a repeated conversion of the same
    // field (a reference-bearing field answers its token on every ж -> uintptr) reads it back instead of
    // re-resolving the field's name and Go offset, which allocates. A token is never 0 (AllocationBase sets
    // bit 63), so 0 means "not yet computed"; a racing first read computes the same value twice, harmlessly.
    private nuint m_pointerOrderToken;

    /// <summary>
    /// The field slot's REAL address when this reference's root is native memory, else 0.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A field of a struct the program placed in NATIVE memory (<c>persistentalloc</c>, <c>sysAlloc</c>,
    /// an mmap'd block) has a genuine machine address that nothing can move, and Go's own contract for
    /// such a pointer is ADDRESS identity: <c>unsafe.Pointer</c> round-tripped through <c>uintptr</c>
    /// and back is the same pointer. The <c>uintptr</c> conversion already answers that address (the
    /// operator's `fixed` fallback reads the slot, which for a native root IS the native address);
    /// EQUALITY did not, because each kind compared only against its own kind.
    /// </para>
    /// <para>
    /// MEASURED, runtime row 2026-09-22: <c>lfnodeValidate</c> does
    /// <c>lfstackUnpack(lfstackPack(node, ^uintptr(0))) != node</c>. `node` is <c>&amp;n.LFNode</c> over a
    /// persistentalloc'd MyNode (this kind); the unpacked pointer is a <see cref="NativeBox{T}"/> over the
    /// same address. Same address, different kind, so `!=` read TRUE and Go's FATAL
    /// <c>throw("bad lfnode address")</c> ended the test host after 186 of 10,891 results.
    /// </para>
    /// <para>
    /// This is identity ONLY. <see cref="ж{T}.NativeAddress"/> is deliberately NOT overridden: it gates
    /// the native-word accessors (<c>ReadPointerWord</c>) and <c>unsafe.Add</c>'s byte-stepping arm,
    /// which stay this kind's own. A managed-rooted field reference answers 0 and keeps the source+token
    /// comparison below, unchanged.
    /// </para>
    /// </remarks>
    internal unsafe nuint NativeSlotAddress =>
        m_source is INativeRooted { IsNativeRooted: true } ? (nuint)Unsafe.AsPointer(ref ValueSlot) : 0;

    /// <inheritdoc/>
    public override bool Equals(ж<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // A native-rooted field reference IS its address (see NativeSlotAddress): compare by it, against
        // either kind that can name the same machine address.
        if (NativeSlotAddress is var address and not 0)
        {
            if (other is NativeBox<T> native)
                return address == native.NativeAddress;

            if (other is FieldRefBox<T> nativeField && nativeField.NativeSlotAddress is var otherAddress and not 0)
                return address == otherAddress;
        }

        // Pointer into a struct field: same source object and field accessor. The comparison uses
        // the field IDENTITY token — the original accessor delegate — never the stored ref
        // function (comparing per-call wrappers made every distinct `&x.field` box unequal).
        return other is FieldRefBox<T> fr && SameSource(m_source, fr.m_source) && m_token.Equals(fr.m_token);
    }

    /// <inheritdoc/>
    // Consistent with Equals on both arms: a native-rooted reference hashes by the address it compares
    // by (NativeBox hashes the same number), every other one by its source identity.
    public override int GetHashCode() =>
        NativeSlotAddress is var address and not 0 ? address.GetHashCode() : SourceIdentityHash(m_source);

    /// <inheritdoc/>
    bool INativeRooted.IsNativeRooted => m_source is INativeRooted { IsNativeRooted: true };

    /// <inheritdoc/>
    // A field reference's storage is its container's, recursively: `Ꮡo.of(Ꮡin).of(Ꮡv)` hangs off
    // a per-access intermediate box whose own storage is the outer allocation's.
    public override object? PinnableStorage =>
        m_source is INilPointer parent ? parent.PinnableStorage : m_source as Array;

    /// <inheritdoc/>
    // THE REPAIR. A field reference ALWAYS names a real interior address — that is what it is —
    // and only its PINNABILITY depends on the root. PinnableStorage recurses to the parent, so a
    // root with no pinnable slot answers null all the way down; reading that null as "no address"
    // tokenised `жfd.of(жpfd).of(жSysfd)` and the kernel refused it on every Windows TCP dial.
    // The pointee there is reference-FREE and its address was correct before the merge; the root
    // being reference-bearing is a fact about the container, not about whether an address exists.
    //
    // The one exception is the FIELD's own type, mirroring ElemRefBox (ruling 2026-09-22): a field
    // that itself holds managed references answers None, so ж -> uintptr hands out an order token,
    // never its raw address. The CLR lays such a struct out automatically and the address is
    // unpinned, so a native writer given it writes its own layout over object references: os's
    // `unix.Fstatat(parent, n, &fs.sys, ...)` had the kernel write 144 bytes of `struct stat` over
    // a reference-bearing syscall.Stat_t (every Root Stat/Lstat read size 1000 and mode p---------).
    // A token is refused by the kernel (EFAULT) and carried by the linux keystone's marshal. The
    // repair above is untouched: the TCP dial's Sysfd is reference-free and keeps its address.
    public override PointerStorage StorageKind =>
        RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? PointerStorage.None :
        PinnableStorage is null ? PointerStorage.Unpinnable : PointerStorage.Pinnable;

    /// <inheritdoc/>
    // Recurse: the source of a nested field ref is a per-call intermediate box, never an
    // allocation of its own. The chain is finite — each `of` wraps a strictly outer pointer.
    public override object ReferentObject =>
        m_source is INilPointer parent ? parent.ReferentObject : m_source;

    // ---- identity helpers (unchanged bodies, relocated with their sole consumer) ----

    private static bool SameSource(object source1, object source2)
    {
        return ReferenceEquals(source1, source2) ||
               (source1 is INilPointer && source2 is INilPointer && source1.Equals(source2));
    }

    private static int SourceIdentityHash(object source)
    {
        return source is INilPointer parent ? parent.GetHashCode() : RuntimeHelpers.GetHashCode(source);
    }

    // The displacement a field view adds to its source's allocation base, cached by the accessor delegate
    // (the view's identity token). A view is cached per (box, accessor), so every FRESH box pays its
    // view's first token, and resolving it by name costs a Type[] (PointeeTypeOf's generic-argument
    // read) and a sliced name string (FieldNameOf) each time. The answer is a pure function of the
    // source box's TYPE and the accessor, so an entry that records the source type it was resolved for
    // is exact on a match and re-resolves on a mismatch -- no assumption about which boxes an accessor
    // serves. Weak on the delegate: an accessor built per call leaves nothing behind.
    private static nuint GoFieldDisplacement(object source, Delegate fieldId)
    {
        Type sourceType = source.GetType();

        if (s_displacementsByAccessor.TryGetValue(fieldId, out Displacement? cached) && ReferenceEquals(cached.SourceType, sourceType))
            return cached.Value;

        nuint displacement = ResolveGoFieldDisplacement(source, fieldId);
        s_displacementsByAccessor.AddOrUpdate(fieldId, new Displacement(sourceType, displacement));

        return displacement;
    }

    private sealed record Displacement(Type SourceType, nuint Value);

    private static readonly ConditionalWeakTable<Delegate, Displacement> s_displacementsByAccessor = new();

    private static nuint ResolveGoFieldDisplacement(object source, Delegate fieldId)
    {
        Type? structType = PointeeTypeOf(source);
        string? fieldName = FieldNameOf(fieldId);

        if (structType is not null && fieldName is not null &&
            s_goFieldDisplacements.GetOrAdd((structType, fieldName), static key => ResolveGoFieldOffset(key.Item1, key.Item2)) is { } offset && offset >= 0)
        {
            return (nuint)offset;
        }

        return unchecked((nuint)(uint)fieldId.GetHashCode());
    }

    private static readonly ConcurrentDictionary<(Type, string), nint> s_goFieldDisplacements = new();

    private static nint ResolveGoFieldOffset(Type structType, string fieldName)
    {
        if (GoReflect.GoFieldOffsets(structType) is not { } offsets)
            return -1;

        GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(structType);

        for (int i = 0; i < fields.Length && i < offsets.Length; i++)
        {
            if (fields[i].Name == fieldName)
                return offsets[i];
        }

        return -1;
    }

    // The type a source box points AT: `ж<S>` → S — the shared base-chain walk, so it answers for
    // every constructed box including the per-kind subclasses.
    private static Type? PointeeTypeOf(object source)
    {
        return GoReflect.TryBoxPointee(source.GetType(), out Type? pointee) ? pointee : null;
    }

    // The Go field name behind an accessor delegate, in either spelling the two construction paths
    // produce (go2cs-gen's TypeGenerator and GoReflect.buildFieldAccessor) — a contract between
    // siblings, not a guess about arbitrary delegates.
    private const char PointerFieldAccessorPrefix = 'Ꮡ';

    private static string? FieldNameOf(Delegate fieldId)
    {
        string name = fieldId.Method.Name;

        if (name.StartsWith(GoReflect.FieldAccessorPrefix, StringComparison.Ordinal))
            return name[GoReflect.FieldAccessorPrefix.Length..];

        return name.Length > 1 && name[0] == PointerFieldAccessorPrefix ? name[1..] : null;
    }
}
