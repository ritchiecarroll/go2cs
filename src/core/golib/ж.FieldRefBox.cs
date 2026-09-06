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
public sealed class FieldRefBox<T> : ж<T>
{
    private readonly object m_source;
    private readonly FieldRefFunc<T> m_accessor;
    private readonly Delegate m_token;

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
        unchecked(AllocationBase(SourceIdentityHash(m_source)) + GoFieldDisplacement(m_source, m_token));

    /// <inheritdoc/>
    public override bool Equals(ж<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // Pointer into a struct field: same source object and field accessor. The comparison uses
        // the field IDENTITY token — the original accessor delegate — never the stored ref
        // function (comparing per-call wrappers made every distinct `&x.field` box unequal).
        return other is FieldRefBox<T> fr && SameSource(m_source, fr.m_source) && m_token.Equals(fr.m_token);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => SourceIdentityHash(m_source);

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
    public override PointerStorage StorageKind =>
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

    private static nuint GoFieldDisplacement(object source, Delegate fieldId)
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
