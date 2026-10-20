// ж.FieldRefBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    /// <summary>The native base address when this box IS a native root (a <see cref="NativeBox{T}"/>), else 0.</summary>
    nuint NativeRootAddress => 0;

    /// <summary>
    /// A field reference's address at its GO offset when it sits ONE field hop off a native root, else 0
    /// -- the address the native field-view door views (<see cref="builtin.NativeFieldArrayPointer{T}"/>).
    /// </summary>
    nuint NativeGoFieldAddress => 0;
}

/// <summary>
/// A pointer INTO an allocation, an element or field view, whose own token already carries its Go offset
/// within that allocation. A field view taken through one builds on that token rather than on the
/// allocation's base (option 3's base alone made &amp;s[0].f and &amp;s[1].f, and &amp;o.X.f and &amp;o.Y.f, one number).
/// </summary>
internal interface IInteriorPointer
{
    nuint PointerOrderToken { get; }
}

/// <summary>
/// A field view whose order token is base plus Go offsets all the way down, asked of a view's SOURCE (held as
/// <c>object</c>) by the promoted-field identity arm, which compares order tokens only when both are exact.
/// </summary>
internal interface IExactOrderToken
{
    bool OrderTokenIsExact { get; }
}

/// <summary>
/// A field view asked, as <c>object</c>, for the box it is the FIRST field of: its source when the field sits
/// at Go offset 0 in it, else <c>null</c>. Go's prefix-downcast idiom reads exactly that relation —
/// <c>(*entry[T])(unsafe.Pointer(&amp;e.node))</c> is <c>e</c> — and
/// <see cref="PointerExtensions.Reinterpret{T,TDst}"/> walks it to hand the parent back.
/// </summary>
internal interface IFirstFieldView
{
    object? FirstFieldParent { get; }
}

public sealed class FieldRefBox<T> : ж<T>, INativeRooted, IInteriorPointer, IExactOrderToken, IFirstFieldView, IReinterpretView
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
        m_pointerOrderToken = unchecked(SourceBase(m_source) + GoFieldDisplacement(m_source, m_token));

    // Where this field's displacement is counted from: an element or field view's own token, which
    // already carries its Go offset within the allocation, else the allocation's base. Option 3 gave
    // every view the allocation's base, so &s[0].f and &s[1].f, and &o.X.f and &o.Y.f, shared a number.
    private static nuint SourceBase(object source) =>
        source is IInteriorPointer interior && interior.PointerOrderToken is var token and not 0 ? token :
        AllocationBase(ManagedPointerTokens.AllocationIdOf(source));

    internal override ulong AllocationId => ManagedPointerTokens.AllocationIdOf(m_source);

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
        !TargetsSharedZeroSizeSlot && m_source is INativeRooted { IsNativeRooted: true } ? (nuint)Unsafe.AsPointer(ref ValueSlot) : 0;

    // A Go ZERO-SIZE field's ref target is not its storage: a readonly zero-size field laid out at Go's
    // offset answers golib's shared per-type slot (GoZeroSizeSlot, A17), so the TARGET's address names no
    // field at all. Identity stays (source, field): NativeSlotAddress answers 0 above, and StorageKind None
    // below makes ж -> uintptr hand out the order token (source base + Go field offset). &x.f != &y.f for
    // distinct x and y, as in Go (C2's Z2 field-stays-distinct rule).
    //
    // The test is the TARGET, not the TYPE. This kind also carries golib's aliasing REINTERPRET VIEW
    // (`(*SID)(unsafe.Pointer(&b[0]))` -- Windows' variable-length SID over a byte buffer, Go's opaque
    // zero-size struct), whose target IS the buffer's storage and whose uintptr is that address. Keying
    // on the zero-size TYPE tokenised the view, and the syscall door refused CopySid at argument 1 --
    // os/user's TestLookupGroup family and internal/syscall/windows' TestRunAtLowIntegrity, regressed at
    // the TRAIN I union. Only a ref that lands ON the shared slot is a zero-size FIELD. It costs no
    // instance state: GoZeroSizeFacts<T>.IsZeroSize is a static readonly per T, so every other type
    // folds the whole test away at JIT time.
    private bool TargetsSharedZeroSizeSlot =>
        GoZeroSizeFacts<T>.IsZeroSize && Unsafe.AreSame(ref ValueSlot, ref GoZeroSizeSlot<T>.Ref);

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
        if (other is not FieldRefBox<T> fr)
            return false;

        // The same Go address reached through DIFFERENT sources: converted code's flat promoted `&o.X` is one view over
        // `o`, the nested `&o.Inner.X` (and reflect's FieldByName("X").Addr(), which builds the nested chain) a view over
        // `o`'s view of Inner. Go says they are one pointer.
        if (!SameSource(m_source, fr.m_source))
            return SameGoAddress(fr);

        if (m_token.Equals(fr.m_token))
            return true;

        // The same field reached by the OTHER construction path: converted code's `&s.A` carries go2cs-gen's
        // generated `ᏑA` accessor, reflect's `Field(i).Addr()` carries GoReflect's `goref_A`. Over the same
        // source they are one Go pointer, so compare by the Go field NAME each token spells -- the key
        // PointerOrderToken already resolves both spellings by, which is why their uintptrs always agreed
        // while == answered false and a map keyed by one missed the other (box-equality census, 2026-09-29).
        // "Same declaring type" holds by construction: both tokens are resolved against the one source's pointee.
        // (A PROMOTED field reached two ways has two SOURCES, a flat view over the outer box and a nested chain;
        // the different-sources arm above, SameGoAddress, answers it.)
        if (FieldNameOf(m_token) is { } name && name == FieldNameOf(fr.m_token))
            return true;

        // Two DIFFERENT fields of one source sit at one Go offset only when one is zero-size; Go's comparison then may
        // answer either way, and the address it compares answers equal (SameGoAddress).
        return SameGoAddress(fr);
    }

    /// <inheritdoc/>
    // Consistent with Equals on both arms: a native-rooted reference hashes by the address it compares
    // by (NativeBox hashes the same number), every other one by its source identity.
    public override int GetHashCode() =>
        NativeSlotAddress is var address and not 0 ? address.GetHashCode() : SourceIdentityHash(m_source);

    /// <inheritdoc/>
    bool INativeRooted.IsNativeRooted => m_source is INativeRooted { IsNativeRooted: true };

    // The field at its GO offset off a NATIVE root, one hop -- never NativeSlotAddress, which is the CLR
    // slot: a struct carrying a managed reference is laid out automatically, and darwin's converted
    // RawSockaddrInet4 keeps Port at CLR offset 0 where libc wrote it at 2. The offset is the struct's Go
    // layout (GoReflect.GoFieldOffsets, the same pass the order token's displacement reads); a layout that
    // cannot be known answers 0, so the caller keeps its own route and nothing is viewed at a guess.
    //
    // The order matters for cost: the native-root test comes FIRST, so a managed root -- every root the door
    // declines, runtime's cheaprand among them -- pays one interface test and nothing else. A native root
    // pays one memoized lookup per call (GoFieldOffsetOf), never the name read and slice that resolve it.
    nuint INativeRooted.NativeGoFieldAddress =>
        m_source is INativeRooted { NativeRootAddress: var root and not 0 } &&
        GoFieldOffsetOf(m_source, m_token) is var offset and >= 0
            ? root + (nuint)offset
            : 0;

    // An ARRAY field of a struct that lives in NATIVE memory -- `&buf.PathBuffer[0]` over
    // `buf := (*MountPointReparseBuffer)(unsafe.Pointer(&b[0]))`, whose box is a NativeBox over the pinned
    // bytes -- is a native window at the field's GO offset, with the field's declared length. Without it the
    // element door (arrayView) read Value, the struct at the CLR's layout, and took the bytes under the field
    // as an array<E> HEADER: a zeroed buffer read as a null array of length 0 (os createMountPoint's
    // "index out of range [0] with length 0"), a filled one as a fabricated reference. A managed root, a
    // field that is not an array, and a layout that cannot be known answer null, so arrayView keeps its
    // route; a reference-bearing element answers null from the shadow kind, which keeps it too.
    internal override IArray? TryGetNativeArrayView(Type elementType) =>
        NativeArrayWindow() is { } window ? window.TryGetNativeArrayView(elementType) : null;

    // The box ж.of hands out for a field of a struct in NATIVE memory, null for any other root: the native window
    // above for an array field whose Go length is known, else a NativeBox at the field's Go address. So every hop
    // of a native chain is itself native and addresses Go's layout, never the CLR slot.
    internal ж<T>? NativeFieldBox() =>
        ((INativeRooted)this).NativeGoFieldAddress is var address and not 0
            ? NativeArrayWindow() ?? new NativeBox<T>(address)
            : null;

    private ж<T>? NativeArrayWindow()
    {
        if (NativeArrayField.Element is not { } element || ((INativeRooted)this).NativeGoFieldAddress is not (var address and not 0))
            return null;

        nint length = GoArrayLengthOf(m_source, m_token);

        // The window is X's registered door (NativeArrayOpeners), compiled only for an [N]X field some compilation
        // declares, never a generic method closed by reflection (Native AOT has no code for one over a value type).
        // T is array<X>, so the window IS a ж<T>.
        return length >= 0 ? (ж<T>)NativeArrayOpeners.OpenArray(element, address, length) : null;
    }

    // X for an array field type T = array<X>, else null: a pure type query, decided once per T.
    private static class NativeArrayField
    {
        internal static readonly Type? Element =
            typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(array<>) ? typeof(T).GetGenericArguments()[0] : null;
    }

    // A directly declared array field's Go length (its [N]), read from the struct's Go fields: -1 when the
    // field is promoted, not an array, or not one-dimensional, so the caller keeps its route.
    private static nint GoArrayLengthOf(object source, Delegate fieldId)
    {
        if (PointeeTypeOf(source) is not { } structType || FieldNameOf(fieldId) is not { } fieldName)
            return -1;

        return s_goArrayLengths.GetOrAdd((structType, fieldName), static key =>
        {
            foreach (GoReflect.GoFieldInfo field in GoReflect.GoFields(key.Item1))
            {
                if (field.Name == key.Item2)
                    return field.Path.Length == 1 && field.ArrayDims is { Length: 1 } dims ? dims[0] : -1;
            }

            return -1;
        });
    }

    private static readonly ConcurrentDictionary<(Type, string), nint> s_goArrayLengths = new();

    // A field's Go offset per accessor, memoized beside the displacement above and on the same terms: an
    // entry records the source type it was resolved for, so it is exact on a match and re-resolved on a
    // mismatch. Resolving reads the field's name off the delegate and slices it, which allocates; the door
    // asks on every call. -1 is "no Go layout", and it is cached too.
    internal static nint GoFieldOffsetOf(object source, Delegate fieldId)
    {
        Type sourceType = source.GetType();

        if (s_goOffsetsByAccessor.TryGetValue(fieldId, out GoOffset? cached) && ReferenceEquals(cached.SourceType, sourceType))
            return cached.Value;

        nint offset = IsReinterpretAccessor(fieldId) ? 0 :
            PointeeTypeOf(source) is { } structType && FieldNameOf(fieldId) is { } fieldName
                ? s_goFieldDisplacements.GetOrAdd((structType, fieldName), static key => ResolveGoFieldOffset(key.Item1, key.Item2))
                : -1;

        s_goOffsetsByAccessor.AddOrUpdate(fieldId, new GoOffset(sourceType, offset));

        return offset;
    }

    private sealed record GoOffset(Type SourceType, nint Value);

    private static readonly ConditionalWeakTable<Delegate, GoOffset> s_goOffsetsByAccessor = new();

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
    //
    // THE SECOND EXCEPTION, a REINTERPRET of a first field (COORD, G design OK 2026-10-17). Go's
    // `(*byte)(unsafe.Pointer(&rdb.header))` names rdb's own address through its first field, and os_windows_test.go's
    // createDirLink hands it to DeviceIoControl with a length that runs PAST the header into `detail [16384]byte`. That
    // root is reference-bearing here (detail is an array<byte>), so the bytes after the header's raw address are the
    // CLR's image of the struct, not Go's, and windows refused the buffer (ERROR_INVALID_REPARSE_DATA, os TestRootMulti*).
    // So a reinterpret view that widens at Go offset 0 to a MANAGED root whose CLR layout is provably not Go's
    // (GoReflect.ClrLayoutMismatch) yet has a deep native image answers None: ж -> uintptr hands out its order token,
    // which is its root's own number (the reinterpret hop and the first field both sit at Go offset 0, so
    // uintptr(&rdb.header) == uintptr(&rdb) as in Go), and the windows funnel's deep form copies the whole root at Go's
    // layout and decodes it back (NativeStructMarshal.WidestAtSameAddress).
    // Managed reads through the view are untouched; only the number handed out changes. Kept narrow on purpose: a DIRECT
    // field pointer keeps its raw address (an overlapped word is written by the kernel after the call returns, so it must
    // stay the caller's memory, never a copy freed at return), as does a root with no native image (the Sysfd repair) and
    // a reference-free root, whose CLR layout is Go's.
    public override PointerStorage StorageKind =>
        RuntimeHelpers.IsReferenceOrContainsReferences<T>() || TargetsSharedZeroSizeSlot || CrossesAsItsRoot ? PointerStorage.None :
        PinnableStorage is null ? PointerStorage.Unpinnable : PointerStorage.Pinnable;

    // The second exception above. The accessor test comes first, so every other field view pays one delegate read.
    private bool CrossesAsItsRoot =>
        IsReinterpretAccessor(m_token) &&
        NativeStructMarshal.WidestAtSameAddress(this) is var root && !ReferenceEquals(root, this) &&
        root is not INativeRooted { IsNativeRooted: true } &&
        PointeeTypeOf(root) is { } rootType && GoReflect.ClrLayoutMismatch(rootType) is not null;

    /// <inheritdoc/>
    // Recurse: the source of a nested field ref is a per-call intermediate box, never an
    // allocation of its own. The chain is finite — each `of` wraps a strictly outer pointer.
    public override object ReferentObject =>
        m_source is INilPointer parent ? parent.ReferentObject : m_source;

    // ---- the promoted-field identity arm (COORD 2026-10-12) ----

    // Two managed-rooted field views of ONE root allocation at ONE Go offset are one Go pointer: the same allocation
    // (AllocationId resolves an of() chain to its root) and the same order token (allocation base plus the Go offset,
    // composed down the chain). T is the same by construction (Equals(ж<T>)), and a pointer of another type at that
    // offset is another Go type, never compared equal. Both tokens must be EXACT -- every hop's offset resolved from the
    // Go layout -- since an unresolved hop falls to a hash displacement that could agree by accident. A native-rooted view
    // keeps its address comparison above. Two zero-size fields at one offset compare equal here, which Go allows: "Pointers
    // to distinct zero-size variables may or may not be equal" (The Go Programming Language Specification, Comparison
    // operators).
    private bool SameGoAddress(FieldRefBox<T> other) =>
        NativeSlotAddress == 0 && other.NativeSlotAddress == 0 &&
        OrderTokenIsExact && other.OrderTokenIsExact &&
        AllocationId == other.AllocationId &&
        PointerOrderToken == other.PointerOrderToken;

    // Whether this view's order token is base plus Go offsets all the way down: its own field's offset resolved, and its
    // source's token exact in turn (a root box's base is; an element view's index times the Go stride is).
    bool IExactOrderToken.OrderTokenIsExact => OrderTokenIsExact;

    // The source this view is the first field of: the field's Go offset in its source is 0. An offset the Go
    // layout cannot resolve (-1) is not 0, so an unknown layout never claims a parent.
    object? IFirstFieldView.FirstFieldParent => GoFieldOffsetOf(m_source, m_token) == 0 ? m_source : null;

    private bool OrderTokenIsExact =>
        GoFieldOffsetOf(m_source, m_token) >= 0 && (m_source is not IExactOrderToken source || source.OrderTokenIsExact);

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
        if (IsReinterpretAccessor(fieldId))
            return 0;

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

    // The field's Go offset in structType: a field of its own, else a field PROMOTED through VALUE embeds, by Go's rule --
    // the shallowest depth that has the name wins, and it must hold exactly one (an ambiguous name selects nothing). A
    // promotion through an embedded POINTER is not followed: it names storage in another allocation, and the converter
    // roots those views at the pointer. Without the promoted arm, converted code's flat `&o.X` (`Ꮡo.of(Outer.ᏑX)`) had no
    // offset and fell to a hash displacement, so its order token could not agree with the nested `&o.Inner.X`'s.
    private static nint ResolveGoFieldOffset(Type structType, string fieldName)
    {
        List<(Type type, nint baseOffset)> level = [(structType, 0)];

        for (int depth = 0; depth < 16 && level.Count > 0; depth++)
        {
            List<(Type type, nint baseOffset)> next = [];
            nint found = -1;
            int matches = 0;

            foreach ((Type type, nint baseOffset) in level)
            {
                if (GoReflect.GoFieldOffsets(type) is not { } offsets)
                    continue;

                GoReflect.GoFieldInfo[] fields = GoReflect.GoFields(type);

                for (int i = 0; i < fields.Length && i < offsets.Length; i++)
                {
                    if (fields[i].Name == fieldName)
                    {
                        found = baseOffset + offsets[i];
                        matches++;
                    }
                    else if (fields[i].Embedded && fields[i].Type.IsValueType)
                    {
                        next.Add((fields[i].Type, baseOffset + offsets[i]));
                    }
                }
            }

            if (matches > 0)
                return matches == 1 ? found : -1;

            level = next;
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

    internal static string? FieldNameOf(Delegate fieldId)
    {
        string name = fieldId.Method.Name;

        if (name.StartsWith(GoReflect.FieldAccessorPrefix, StringComparison.Ordinal))
            return name[GoReflect.FieldAccessorPrefix.Length..];

        return name.Length > 1 && name[0] == PointerFieldAccessorPrefix ? name[1..] : null;
    }

    // A REINTERPRET view -- Reinterpret's aliasing kind, whose accessor is ж<TSource>.ReinterpretRef<T> -- is its source's
    // own storage read as another type: `(*U)(unsafe.Pointer(p))` is p's address, so the hop is Go offset 0 whatever the
    // two types are. Counting it so gives the view its source's order token (one Go address, one number) and lets a
    // reinterpret of a first field widen to its struct (IFirstFieldView). Every other view resolves its field by name.
    // The box this view re-reads as T when it is a REINTERPRET view, else null. Reinterpret re-views it rather than this
    // view, so a chain of pointer conversions stays one view over one box (PointerExtensions.Reinterpret).
    internal object? ReinterpretRoot => IsReinterpretAccessor(m_token) ? m_source : null;

    object? IReinterpretView.ReinterpretRoot => ReinterpretRoot;

    // ReinterpretUnaliased's refusing accessor is a reinterpret view too: same address, same root.
    private static bool IsReinterpretAccessor(Delegate fieldId) =>
        fieldId.Method is { Name: nameof(ж<byte>.ReinterpretRef) or nameof(ж<byte>.RootReinterpretRef) or nameof(ж<byte>.UnaliasedRef), DeclaringType: { IsGenericType: true } owner } &&
        owner.GetGenericTypeDefinition() == typeof(ж<>);
}
