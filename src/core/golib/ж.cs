// ж.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

[assembly:InternalsVisibleTo("unsafe")]
[assembly:InternalsVisibleTo("GolibTests")]
[assembly:InternalsVisibleTo("runtime")] // runtime.Gosched delegates to golib's GoschedBackoff escalation

namespace go;

/// <summary>
/// Represents a heap allocated reference to an instance of type <typeparamref name="T"/> — the
/// managed form of a Go pointer (<c>*T</c>).
/// </summary>
/// <remarks>
/// <para>
/// THE ABSTRACT BASE of the per-kind pointer model (B1, <c>docs/phase4/DESIGN-zh-box-b1.md</c>
/// §3, ratified 2026-08-26). Every DECLARED pointer position in converted code is typed
/// <c>ж&lt;T&gt;</c> and never changes; every INSTANCE is one of exactly four kinds, each a
/// subclass carrying only its own storage:
/// </para>
/// <list type="bullet">
/// <item><see cref="StandardBox{T}"/> — a heap box that IS the storage it names (UNSEALED:
/// <c>@unsafe.Pointer</c> derives from its <c>uintptr</c> instantiation, per P-F5).</item>
/// <item><see cref="FieldRefBox{T}"/> — a pointer into a field of another allocation.</item>
/// <item><see cref="ElemRefBox{T}"/> — a pointer to an element of managed collection
/// storage.</item>
/// <item><see cref="NativeBox{T}"/> — an alias of a native address (and the §4 source-retention
/// carrier).</item>
/// </list>
/// <para>
/// The base carries the TWO fields every kind owns — the structural nil mark
/// (<see cref="m_isNull"/>, set only by kind constructors under the amendment-7 contract: the
/// standard nil ctor, and a zero-address native mint) and the pin
/// (<see cref="m_pin"/> — every kind's storage can be pinned on address-take, which is what keeps
/// <see cref="EnsureStableAddress"/>/<see cref="IsPinnedAt"/> base-resident over one virtual
/// <see cref="PinnableStorage"/>). Everything else dispatches per-kind through the abstract
/// accessors, measured ≤ the pre-split branch chains on both runtimes (the banked §1/§2 benches).
/// A missed construction site is a COMPILE error (CS0144 on the abstract base), never a
/// wrong-kind box.
/// </para>
/// </remarks>
/// <summary>
/// What a box's pointer value IS — the three-way answer the pointer-to-scalar operators need,
/// which a pinnability question cannot give them.
/// </summary>
/// <remarks>
/// Two answers were conflated before this existed: "no pinnable storage" was read as "no address",
/// which is true of a standard box over a reference-bearing pointee and of the header kinds, and
/// FALSE of a field or element reference whose root merely happens to be reference-bearing. The
/// middle answer below is that class, and it is the one the merged Q44 arm turned into a token.
/// </remarks>
public enum PointerStorage
{
    /// <summary>
    /// No storage whose address means anything: the value is an order token, never an address.
    /// A standard box over a reference-bearing pointee, and the header kinds, whose value is
    /// materialized rather than resident.
    /// </summary>
    None,

    /// <summary>
    /// A real machine address that CANNOT be held still — an interior reference into an
    /// allocation with no pinnable slot. The address is correct the moment it is taken and may
    /// move afterwards; that is the standing pin-unheld hole, older than this enum and not
    /// closed by it, and it is still strictly better than handing the kernel a non-address.
    /// </summary>
    Unpinnable,

    /// <summary>
    /// A real machine address that can be pinned, and is, before it is handed out.
    /// </summary>
    Pinnable
}

public abstract partial class ж<T> : IPointer<T>, IEquatable<ж<T>>, INilPointer, IUntypedSlotAccess
{
    // The ONE storage fact every kind shares: whether this box IS the nil pointer. STRUCTURAL —
    // set only at construction, by the kind ctor contracts (see the class remarks); the
    // value-peeking refinement for a standard box lives in StandardBox.IsNull.
    private protected readonly bool m_isNull;

    // A pin this box OWNS, kept alive for the box's lifetime and freed when the box is collected
    // (the PinnedBuffer finalizer releases the GCHandle). Serves every kind: a standard box's
    // slot, a field/element reference's canonical backing, and a reinterpret-derived native
    // box's source storage are all "an address of managed storage that must hold still".
    private protected PinnedBuffer? m_pin;

    private protected ж(bool isNull = false) => m_isNull = isNull;

    /// <summary>
    /// Gets a reference to the value this pointer names, panicking on a nil dereference
    /// (Go's <c>*p</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">Cannot get reference to value, source is not a valid array or slice pointer.</exception>
    public abstract ref T Value { get; }

    /// <summary>
    /// Gets a reference to the value slot WITHOUT the nil-pointer-dereference check that
    /// <see cref="Value"/> performs — identical to <see cref="Value"/> except it never throws.
    /// </summary>
    /// <remarks>
    /// Used only where this box is a real heap allocation (created via <c>Ꮡ</c> / <c>heap</c>)
    /// AND its value is a <em>reference</em> type that may legitimately be null — there
    /// <c>.Value</c> would be a read of the held value, not a dereference of this box, and must
    /// not panic (Go: <c>*(&amp;p)</c> where <c>p</c> is a nil <c>*T</c>/slice/map yields the nil
    /// value). Returns the <em>real</em> slot — reads and writes both persist. A genuine
    /// nil-pointer dereference (<c>~Ꮡp</c>) still routes through the strict <see cref="Value"/>.
    /// </remarks>
    public abstract ref T ValueSlot { get; }

    /// <inheritdoc/>
    // The dereference-guard nil question. For every non-standard kind this is exactly the
    // structural mark (their storage resolves without a null check); StandardBox refines it with
    // the value peek its own storage doctrine requires.
    public virtual bool IsNull => m_isNull;

    /// <inheritdoc/>
    // The IDENTITY nil question: whether this box IS the nil pointer (structural, never
    // value-peeking). One non-virtual read — DerefOrNull's fast path (V5's one-field fix).
    public bool IsNilPointer => m_isNull;

    // The pre-split three-term predicate (`no fieldRef && no elemRef && m_isNull`) reproduced by
    // the kind ctor contracts: FieldRefBox/ElemRefBox never set the mark, StandardBox sets it
    // from its nil ctor, NativeBox sets it for the zero address — so the base mark IS the
    // predicate on every constructible instance (amendment 7).
    internal bool IsNilStandardPointer => m_isNull;

    /// <summary>Gets a flag indicating whether this pointer aliases a NATIVE address.</summary>
    public bool IsNative => NativeAddress != 0;

    /// <summary>
    /// Gets the native address this pointer aliases, or 0 for every managed-storage kind.
    /// </summary>
    public virtual nuint NativeAddress => 0;

    // ---- the atomic pointer-word boundary (NativeBox overrides; see its doc) ----

    /// <summary>Atomically reads the pointer-sized word a NATIVE-backed box aliases (acquire semantics).</summary>
    public virtual nuint ReadPointerWord() => throw NonNativeWordAccess();

    /// <summary>Atomically exchanges the pointer-sized word a NATIVE-backed box aliases, returning the previous word.</summary>
    public virtual nuint ExchangePointerWord(nuint value) => throw NonNativeWordAccess();

    /// <summary>Atomically compare-and-swaps the pointer-sized word a NATIVE-backed box aliases.</summary>
    public virtual bool CompareExchangePointerWord(nuint old, nuint @new) => throw NonNativeWordAccess();

    // Callers branch on IsNative before the word ops (the documented contract); reaching a base
    // body is a caller defect, kept loud exactly as the old wrong-kind arms were.
    private static InvalidOperationException NonNativeWordAccess() =>
        new("Pointer-word access is only meaningful for a NATIVE-backed pointer; callers branch on IsNative.");

    // ---- element machinery (ElemRefBox overrides; base defaults are the no-element answers) ----

    // The raw (collection, index) pair for an element reference that kept its ORIGINAL
    // collection, used by the bounds-check extension; null for the fast arm and every other kind.
    internal virtual (IArray, int)? ArrayRef => null;

    // Real managed element storage behind this pointer, when it exists and deref-equivalence
    // holds (see ElemRefBox).
    internal virtual bool TryGetElementStorage(out T[]? backing, out nint index)
    {
        backing = null;
        index = 0;
        return false;
    }

    // The length-element aliasing window this pointer's referent starts — what makes
    // `unsafe.Slice(&s[i], n)` alias the original backing store (crypto/subtle's xorBytes).
    internal virtual bool TryGetElementWindow(int length, out slice<T> window)
    {
        window = default;
        return false;
    }

    // The pinned-reinterpret arm of the Reinterpret fallback (element storage only).
    internal virtual ж<TDst>? TryPinnedReinterpret<TDst>() => null;

    // ---- minting (the of()/at() surface — unchanged signatures, kind ctors behind them) ----

    /// <summary>
    /// Gets a pointer to a field of the struct this pointer references (Go's <c>&amp;p.field</c>).
    /// </summary>
    public ж<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        return new FieldRefBox<TElem>(this, fieldRefFunc);
    }

    /// <summary>
    /// Gets a pointer to a field of the struct this pointer references, via a typed accessor
    /// (wrapped per-call; equality compares the ORIGINAL accessor — see FieldRefBox).
    /// </summary>
    public ж<TElem> of<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        return new FieldRefBox<TElem>(this, FieldRefWrappers<TElem>.For(fieldRefFunc), fieldRefFunc);
    }

    private static class FieldRefWrappers<TElem>
    {
        private static readonly ConditionalWeakTable<FieldRefFunc<T, TElem>, FieldRefFunc<TElem>> s_wrappers = new();

        public static FieldRefFunc<TElem> For(FieldRefFunc<T, TElem> fieldRefFunc)
        {
            return s_wrappers.GetValue(fieldRefFunc, Wrap);
        }

        private static FieldRefFunc<TElem> Wrap(FieldRefFunc<T, TElem> fieldRefFunc)
        {
            return getFieldRef;

            ref TElem getFieldRef(object structPtr)
            {
                ж<T> typedPtr = (ж<T>)structPtr;

                return ref fieldRefFunc(ref typedPtr.Value);
            }
        }
    }

    /// <summary>
    /// Gets a pointer to a field that is itself accessed through a pointer-yielding accessor.
    /// </summary>
    public ж<TElem> of<TElem>(FieldPtrFunc<T, TElem> fieldPtrFunc)
    {
        return fieldPtrFunc(ref Value);
    }

    // ---- the array-backing publish (see arrayView/publishArrayBacking below) ----

    // Per-INSTANTIATION constant: can a T value carry a LAZILY-materialized array backing that has
    // to be published into this box's storage before an element pointer is taken?
    //
    // ⚠ A reflection-BUILT constrained delegate (GetMethod + MakeGenericMethod(typeof(T)) +
    // CreateDelegate, in a static initializer) stood in for this guard until 2026-08-10, and that
    // shape is FATAL under Native AOT: the value-type generic instantiation is reachable only
    // through reflection, ILC emits no native code for it, and the first ж<> type-init of any
    // AOT-published program threw NotSupportedException (all 13 perf-suite binaries died before
    // main — d5c0c9c10). NEVER reintroduce it. The pure TYPE queries below are a different thing
    // entirely: they build no code, so ILC answers them from the type system.
    private static readonly bool s_publishArrayBacking = computePublishArrayBacking();

    private static bool computePublishArrayBacking()
    {
        // Only a VALUE type can lose a lazily-materialized backing to a copy — a class wrapper's
        // backing is shared by reference, so a "copy" of it is the same object.
        if (!typeof(T).IsValueType || !typeof(IArray).IsAssignableFrom(typeof(T)))
            return false;

        // golib's own array<T>/slice<T> — and every named-slice wrapper, which holds a slice<T> in
        // a NON-nullable field — keep their backing in a field that is assigned at construction.
        // Nothing about them is lazy, so there is nothing to publish and the box-touch-copy-back
        // would rewrite identical bytes.
        //
        // Excluding them is a CORRECTNESS-of-cost matter, not just tidiness: `slice<T>.Source` is
        // DEFINED to hand back a DETACHED COPY (AllocationCounter.CopyOf over the window — see
        // slice.cs and the NilType.cs note), and a windowed `array<T>`'s implicit T[] conversion
        // copies too. The unconditional box-touch-copy-back this guard replaces therefore
        // allocated and threw away a FULL COPY OF THE BACKING on every element take through a
        // ж<slice<T>> — measured at ~6x the array cost for a 64-element slice, and unnoticed
        // because it is invisible in every gate but a benchmark.
        if (typeof(ISlice).IsAssignableFrom(typeof(T)))
            return false;

        return !(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(array<>));

        // What remains is exactly the shape that IS lazy: a go2cs-gen named fixed-size array
        // wrapper (`Value => m_value ??= new array<E>(N)`, TypeClass "Array") and the array-VIEW
        // wrapper over one (`type pallocBits pageBits`). Both hand back the RAW backing from
        // Source once materialized, which is what makes it usable as the identity token below.
        // A future value-type IArray that is neither lazy nor excluded here would simply take the
        // publish path and stay correct — the conservative direction.
    }

    // The array backing this box has PUBLISHED into its own storage, or null while it has
    // published none. Written ONLY under `lock (this)`; read with acquire semantics on the fast
    // path. It serves two purposes at once: `null` is the once-only gate (every thread serializes
    // until the first publish lands), and a DIFFERENT backing is the reassignment detector (if
    // `*p = someOtherArray` installs a fresh still-lazy wrapper, the next element take publishes
    // again rather than trusting a stale "ready" flag).
    private object? m_publishedArrayBacking;

    /// <summary>
    /// Gets a view of the array or slice this pointer references, with any lazily-materialized
    /// backing already published into this box's own storage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A go2cs-gen named fixed-size array wrapper allocates its backing on FIRST TOUCH
    /// (<c>Value => m_value ??= new array&lt;E&gt;(N)</c>), and golib can only reach that getter
    /// by BOXING the wrapper — <see cref="ж{T}"/> is deliberately unconstrained in
    /// <typeparamref name="T"/>. So the backing materializes on a private copy and has to be
    /// copied back, or the real storage stays virgin and every write through the returned element
    /// pointer is silently dropped (the pallocBits lesson at the box-element seam, 47ddd5a50).
    /// </para>
    /// <para>
    /// That box-touch-copy-back is a read-modify-write of shared mutable state, and until
    /// 2026-08-30 it ran with NO synchronization: two threads reaching a still-lazy wrapper each
    /// allocated their own backing, and the second copy-back silently discarded the first — along
    /// with every element already written into it. The element pointers already handed out kept
    /// naming the orphan, so their writes landed where nothing would ever read them again
    /// (measured: <c>crypto/internal/boring/bcache</c>'s concurrent section lost entries in ~28%
    /// of runs). The same unsynchronized copy-back could also be observed HALF DONE — the wrapper
    /// is several words wide — surfacing as a spurious IndexOutOfRangeException out of the bounds
    /// check below.
    /// </para>
    /// <para>
    /// The publish is therefore gated per BOX, which is the only durable unit here: the by-value
    /// copy cannot be, and constraining <typeparamref name="T"/> is not available. The fast path
    /// stays lock-free — one acquire read, one type test, one reference compare — so an
    /// already-published box pays no lock, and the slow path runs at most once per box (twice
    /// only if the pointed-to value is REASSIGNED to a different array).
    /// </para>
    /// </remarks>
    private IArray<Telem> arrayView<Telem>()
    {
        if (!s_publishArrayBacking)
        {
            // Nothing behind this T is lazy (see computePublishArrayBacking), so the view IS the
            // storage's view and no publish — and no Source probe — is owed. A non-IArray T is
            // simply the wrong receiver, and reports the same error it always did.
            return Value as IArray<Telem> ?? throw notAnArrayOrSlice();
        }

        // FAST PATH — a backing has already been published, and the view just boxed shares it, so
        // the copy-back would rewrite identical bytes. Lock-free by construction: while
        // m_publishedArrayBacking is still null EVERY thread takes the lock, which is exactly the
        // cold-start window the race lives in.
        // ACQUIRE: pairs with the release write in publishArrayBacking, so observing a published
        // backing also means observing the copy-back that installed it.
        object? published = Volatile.Read(ref m_publishedArrayBacking);

        if (published is not null && Value is IArray<Telem> view && ReferenceEquals(view.Source, published))
            return view;

        return publishArrayBacking<Telem>();
    }

    // The at-most-once publish. Kept out of line so arrayView stays small enough to inline.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private IArray<Telem> publishArrayBacking<Telem>()
    {
        // Resolve the storage reference BEFORE taking the lock: for a field or element reference
        // that walk runs through the parent box, and the lock guards the publish, not the walk.
        ref T value = ref Value;

        lock (this)
        {
            if (value is not IArray<Telem> view)
                throw notAnArrayOrSlice();

            // Materializes the lazy backing on the boxed copy, and hands back the RAW backing
            // reference — the identity token the fast path compares against. (Only the lazy
            // wrappers reach here; everything whose Source is defined to copy was excluded by
            // computePublishArrayBacking, so this neither allocates nor detaches.)
            object? backing = view.Source;

            if (!ReferenceEquals(backing, m_publishedArrayBacking))
            {
                // Copy the whole wrapper — a struct over a SHARED backing reference — back over
                // the real storage, which lands that reference where every later reader looks.
                value = (T)(object)view;

                // RELEASE: the copy-back above must be visible to any thread that later observes
                // this write on the lock-free fast path.
                Volatile.Write(ref m_publishedArrayBacking, backing);
            }

            // The view we just published, not a fresh copy of it — the element box then names the
            // published backing by construction rather than by a re-read that could race.
            return view;
        }
    }

    private static InvalidOperationException notAnArrayOrSlice() =>
        new("Cannot get pointer to element at index, type is not an array or slice.");

    /// <summary>
    /// Gets a pointer to the element at <paramref name="index"/> of the array or slice this
    /// pointer references (Go's <c>&amp;p[i]</c> through a pointer-to-collection).
    /// </summary>
    public ж<Telem> at<Telem>(nint index)
    {
        IArray<Telem> array = arrayView<Telem>();

        if (!array.IndexIsValid(index))
            throw new IndexOutOfRangeException("Index is out of range for array or slice.");

        return new ElemRefBox<Telem>(array, (int)index);
    }

    public ж<TElem> at<TElem>(FieldRefFunc<T, array<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<array<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<T, slice<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<slice<TElem>> fieldRefFunc, nint index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, array<TElem>> fieldPtrFunc, nint index) => of(fieldPtrFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, slice<TElem>> fieldPtrFunc, nint index) => of(fieldPtrFunc).at<TElem>(index);

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.PrintPointer();
    }

    // ---- identity (abstract per-kind; the doctrine lives on each kind's override) ----

    /// <inheritdoc/>
    /// <remarks>
    /// Per-kind, with one rule shared by all: Go pointer comparison is by IDENTITY — the same
    /// storage location — never by the pointed-to value. Virtual beyond the kinds for one derived
    /// class and one reason: <c>unsafe.Pointer</c>'s VALUE is the address, so two of them over
    /// one address are ONE Go pointer; overriding here makes <c>==</c>, <c>Equals(object)</c> and
    /// a map-key lookup all answer through the one rule.
    /// </remarks>
    public abstract bool Equals(ж<T>? other);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ж<T> other && Equals(other);
    }

    /// <inheritdoc/>
    public abstract override int GetHashCode();

    /// <inheritdoc/>
    /// <remarks>
    /// A stable, order-consistent address token (see <see cref="INilPointer.PointerOrderToken"/>):
    /// nil → 0; a native alias → its real address; an element reference → canonical storage
    /// identity + absolute index; a field reference → allocation base + Go field offset; a heap
    /// box → its own allocation base. Equal pointers always produce equal tokens.
    /// </remarks>
    public abstract nuint PointerOrderToken { get; }

    // An allocation's token base: the identity hash lifted clear of the low 32 bits, so every
    // base is 8-aligned and the whole low half is available to carry a within-allocation
    // displacement.
    private protected static nuint AllocationBase(int identityHash)
    {
        return unchecked((nuint)((ulong)(uint)identityHash << 32));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The object whose lifetime is the referenced Go allocation: an element reference → the
    /// canonical backing storage; a field reference → its source allocation, recursively; a heap
    /// box (and a native alias, which names no managed allocation) → the box itself.
    /// </remarks>
    public virtual object ReferentObject => this;

    // ---- pinning (base-resident over the one virtual storage answer) ----

    /// <inheritdoc/>
    /// <remarks>
    /// The managed storage whose address IS this pointer's meaning, when such storage exists:
    /// a standard box's pinnable slot, a field reference's container allocation (recursively),
    /// an element reference's canonical backing. Null when nothing pinnable exists (a managed-T
    /// standard box, a native alias).
    /// </remarks>
    public virtual object? PinnableStorage => null;

    /// <summary>
    /// Whether this box's pointer VALUE is a machine address at all, and if so whether that
    /// address can be held still — the question the pointer-to-scalar operators actually ask.
    /// </summary>
    /// <remarks>
    /// DELIBERATELY ABSTRACT, and that is the repair. Both operators used to ask
    /// <see cref="PinnableStorage"/> — "can this be held still?" — and read the answer as "is
    /// there an address here?". Those are different questions and the difference is a whole class:
    /// a field or element reference rooted in a REFERENCE-BEARING allocation answers null to the
    /// first (its root has no pinnable slot, and the answer recurses) while naming a perfectly real
    /// interior address. Under the merged Q44 arm every such pointer became an order token, and the
    /// kernel refused it: WSAEFAULT on every Windows TCP dial, through
    /// `жfd.of(netФD.жpfd).of(poll.FD.жSysfd).Reinterpret&lt;ΔHandle, byte&gt;()` at the
    /// SO_UPDATE_CONNECT_CONTEXT that ends netFD.connect — a reference-free pointee whose address
    /// was correct before the merge. A silent one rode with it in the Windows `os` layer.
    ///
    /// Abstract rather than virtual so a NEW box kind cannot inherit an answer to the wrong
    /// question the way the header boxes did: it must state its own, or the assembly does not
    /// compile. That is the difference between a rule and a reminder.
    /// </remarks>
    public abstract PointerStorage StorageKind { get; }

    // Hold the storage still for as long as this box lives — the address-take contract: whatever
    // receives the address may still be using it after the statement that produced it returns.
    private void EnsureStableAddress()
    {
        if (m_pin is not null)
            return;

        if (PinnableStorage is { } storage)
            m_pin = PinnedBuffer.PinOnly(storage);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Validate-on-read for the provenance record: alive is already established by the caller
    /// (the weak entry resolved); "still pinned THERE" is re-derived from the same computation
    /// that registered it. No pin, no claim — and a box whose storage moved or was re-pinned
    /// elsewhere answers false, which fails MISS-wards by design.
    /// </remarks>
    public unsafe bool IsPinnedAt(nuint address)
    {
        PinnedBuffer? pin = m_pin;

        if (pin is null || NativeAddress != 0)
            return false;

        // A fixed-array buffer's provenance entry records the pinned DATA address
        // (pinnedArrayData) — a different allocation than the value slot — so the pin answers
        // for its own storage first. Without this arm those entries register but never resolve,
        // and the keystone tether is blind to exactly the buffer arguments (pipe2's `*[2]int32`,
        // readlinkat's `*[N]byte`) whose mid-syscall unpinning the record exists to prevent.
        // Pin-only holds are zero-length by construction and never take this arm.
        if (pin.Length > 0 && pin.PinnedTarget is not null && (nuint)pin.Pointer == address)
            return true;

        fixed (void* ptr = &this.ValueSlot)
            return (nuint)ptr == address;
    }

    // Returns a stable native pointer to the first element of this box's Go fixed-array data,
    // pinning the array's backing for the box's lifetime (idempotent; a concurrent first touch
    // can at worst allocate one extra handle that the finalizer frees).
    private unsafe void* pinnedArrayData(IArray arr)
    {
        m_pin ??= new PinnedBuffer(arr.Source, arr.Length);
        return m_pin.Pointer;
    }

    // ---- untyped slot access (the bare-unsafe.Pointer store/load-through seam — I5) ----
    //
    // One body serves all four kinds because ValueSlot already dispatches per kind. Explicit
    // implementations: the interface is the internal recovery seam for unsafe.Pointer's retained
    // referent, never a surface converted code calls.

    bool IUntypedSlotAccess.TryStoreThrough(object? value)
    {
        // A nil pointer cannot be stored through (Go faults; the caller owns the loud form).
        if (IsNilPointer)
            return false;

        switch (value)
        {
            case T typed:
                ValueSlot = typed;
                return true;

            // Storing the nil pointer form: a reference-typed slot (a *T location holding a
            // pointer/map/func/…) takes null; a value-typed slot refuses, and the caller's
            // candidate ladder supplies the value-form nil (e.g. a zero uintptr) instead.
            case null when !typeof(T).IsValueType:
                ValueSlot = default!;
                return true;

            default:
                return false;
        }
    }

    bool IUntypedSlotAccess.TryLoadThrough(out object? value)
    {
        if (IsNilPointer)
        {
            value = null;
            return false;
        }

        value = ValueSlot;
        return true;
    }

    // ---- the dereference operator and equality operators ----

    /// <summary>
    /// Dereferences the pointer (Go's <c>*p</c>), panicking on nil.
    /// </summary>
    public static T operator ~(ж<T> value)
    {
        if (value.IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();

        return value.ValueSlot;
    }

    static T IPointer<T>.operator ~(IPointer<T> value)
    {
        if (value is INilPointer nilable ? nilable.IsNilPointer : value.IsNull)
            throw RuntimeErrorPanic.NilPointerDereference();

        return value is ж<T> box ? box.ValueSlot : value.Value;
    }

    public static bool operator ==(ж<T>? value1, ж<T>? value2)
    {
        return value1 is null ? value2 is null || value2.m_isNull : value1.Equals(value2);
    }

    public static bool operator !=(ж<T>? value1, ж<T>? value2)
    {
        return !(value1 == value2);
    }

    public static bool operator ==(ж<T>? value, NilType _)
    {
        return value is null || value.m_isNull;
    }

    public static bool operator !=(ж<T>? value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, ж<T>? value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, ж<T>? value)
    {
        return value != nil;
    }

    /// <summary>
    /// The canonical typed nil instance for this pointer type — what a Go nil <c>*T</c> is when
    /// its dynamic type must survive (interface packing, canonical-nil marshalling).
    /// </summary>
    public static ж<T> NilBox { get; } = new StandardBox<T>(nil);

    public static implicit operator ж<T>(NilType _)
    {
        // The canonical instance, not a fresh box — see NilBox.
        return NilBox;
    }

    // The reinterpreting ref accessor for PointerExtensions.Reinterpret, as a static method
    // rather than a lambda so that two reinterprets of one box compare EQUAL: field-ref equality
    // compares the source object and the field identity delegate, and Delegate.Equals compares
    // method + target — equal across call sites for the same static method. Go requires
    // `(*U)(unsafe.Pointer(p)) == (*U)(unsafe.Pointer(p))`.
    internal static ref TDst ReinterpretRef<TDst>(object source)
    {
        return ref Unsafe.As<T, TDst>(ref ((ж<T>)source).ValueSlot);
    }

    // ---- the address conversions (the uintptr/void* seam; kind tests are virtual reads) ----

    // EXPLICIT by design: reinterpreting a raw address as a pointer is the runtime-unsafe
    // reinterpret seam — never something to happen silently. The result ALIASES the address —
    // it must never box a COPY of the pointed-at value (a copy silently discards the address:
    // pointer arithmetic then walks the GC heap, and handing the pointer back to a native API
    // frees GC memory — STATUS_HEAP_CORRUPTION). Aliasing keeps `uintptr(unsafe.Pointer(p))`
    // an exact round-trip, as Go requires.
    public static unsafe explicit operator ж<T>(uintptr value)
    {
        // A pointer to MANAGED storage has no address to have been converted from: what a
        // reflect projection handed out was an order token (see ManagedPointerTokens). Recover
        // the box that token named, so the result aliases the very storage the reflect Value
        // did — instead of a native box over a number that is not an address.
        if (ManagedPointerTokens.Resolve((nuint)value.Value) is ж<T> aliased)
            return aliased;

        return new NativeBox<T>((nuint)value.Value);
    }

    public static unsafe implicit operator uintptr(ж<T> value)
    {
        // A native-backed pointer round-trips to the EXACT address it aliases — it is not
        // managed storage, so there is nothing to pin and no copy to take.
        if (value is not null && value.NativeAddress != 0)
            return (uintptr)value.NativeAddress;

        // A NIL pointer's address is 0, matching Go (`uintptr(unsafe.Pointer(nil)) == 0`) — and
        // the syscall wrappers legitimately pass nil pointers whose numeric address is simply 0.
        // The value-peeking IsNull is KEPT deliberately: this is the address model, and a
        // reference-typed pointee has no address to report.
        if (value is null || value.IsNull)
            return default;

        // A pointer to a Go fixed array (`unsafe.Pointer(&arr)`): the native address must reference the
        // array's DATA (element 0), pinned so a syscall can fill it in and the managed reads afterward
        // observe the result — not the transient address of the `array<T>` struct wrapper. Slices keep
        // header semantics (`&s` is the slice header in Go), so they fall through to the value-slot path.
        //
        // REGISTERED like every other pinned conversion (the os/exec heap-corruption arc,
        // 2026-08-26): this path returned the data address WITHOUT the RegisterPinned record the
        // ratified provenance design requires of the pin moment — so the reverse conversion read
        // these addresses as "genuinely native", and the keystone tether (which re-roots a
        // syscall argument's box by resolving its address) could not see fixed-array BUFFER
        // arguments at all. The pin still lives on the box, the box still dies at JIT retirement,
        // and a blocking read(2) into such a buffer then lands the kernel's write on recycled
        // heap — HeapVerify caught exactly that shape (a range-smashed victim under pipe-buffer
        // load) once the other corridors were closed. Recording the address is what makes the
        // tether's Resolve, and the provenance record itself, honest for case 1.
        if (value.Value is IArray arr && arr is not ISlice)
        {
            uintptr dataAddr = (uintptr)value.pinnedArrayData(arr);
            ManagedPointerTokens.RegisterPinned((nuint)dataAddr.Value, value);
            return dataAddr;
        }

        // A REFERENCE-BEARING pointee has no pinnable slot (StandardBox keeps it in m_val, a field of
        // this box object), so there is no storage to hold still and `fixed` would hand out the
        // address of a movable heap object — the number SUB-Q42's witness measured going stale. Its
        // stable, resolvable number is the box's own order token, the same value reflect projects for
        // `%p` (value_impl.cs) and MintOpaque registers: registered here, `(ж<T>)(uintptr)` recovers
        // this very box through ManagedPointerTokens.Resolve's order-token arm (ж.cs:612–622,
        // ж.PointerTokens.cs:327) for exactly as long as something else keeps the box alive — the
        // record's own weak-lifetime rule. docs/phase4/DESIGN-managed-pointer-token.md (Q44).
        if (value.StorageKind is PointerStorage.None)
        {
            nuint token = value.PointerOrderToken;
            ManagedPointerTokens.Register(token, value);
            return (uintptr)token;
        }

        // Hold the storage still BEFORE reading its address: `fixed` pins only for its own
        // statement, and the address outlives that statement by definition.
        value.EnsureStableAddress();

        fixed (void* ptr = &value.Value)
        {
            // The PROVENANCE record (DESIGN-pointer-provenance.md, RATIFIED): the pin is the one
            // guarantee the resolve-side validate-on-read leans on.
            //
            // AND IT IS REACHED ON THE UNPINNABLE PATH TOO, deliberately. EnsureStableAddress
            // pins only when PinnableStorage is non-null, so for PointerStorage.Unpinnable this
            // records "the box was at this address" about an address nothing is holding. That is
            // harmless rather than sloppy: Resolve validates on READ (alive AND still pinned
            // there), so a stale entry answers MISS, which is the same answer the caller got
            // before this record existed. It is also exactly what the code did before the token
            // arm was added — removing it here would be a SECOND undeclared change inside a
            // repair, and the pin-unheld hole it hints at is its own arc with its own guard.
            ManagedPointerTokens.RegisterPinned((nuint)ptr, value);
            return (uintptr)ptr;
        }
    }

    public static unsafe implicit operator ж<T>(void* value)
    {
        // The same resolve as the uintptr operator: a token this family handed out through
        // `operator void*` (the Q44 reference-bearing arm) comes back as its box, never as a
        // native box over the token — the in-operator was the one door the token arm left
        // asymmetric (found 2026-09-05 while rooting the PointerCastSliceRange row).
        return (ж<T>)(uintptr)(nuint)value;
    }

    public static unsafe implicit operator void*(ж<T> value)
    {
        if (value is not null && value.NativeAddress != 0)
            return (void*)value.NativeAddress;

        if (value is null || value.IsNull)
            return null;

        // A pointer to a Go fixed array resolves to the pinned address of the array data — see the
        // uintptr operator above for the full rationale, including why the address is REGISTERED
        // (the provenance record and the keystone tether must see buffer addresses too).
        if (value.Value is IArray arr && arr is not ISlice)
        {
            void* dataAddr = value.pinnedArrayData(arr);
            ManagedPointerTokens.RegisterPinned((nuint)dataAddr, value);
            return dataAddr;
        }

        // The same reference-bearing arm as the uintptr operator above (Q44): the token, not a
        // field address, is what a native call could later hand back to `(ж<T>)(void*)`.
        if (value.StorageKind is PointerStorage.None)
        {
            nuint token = value.PointerOrderToken;
            ManagedPointerTokens.Register(token, value);
            return (void*)token;
        }

        value.EnsureStableAddress();

        fixed (T* ptr = &value.Value)
        {
            // Reached on the UNPINNABLE path too, for the reason spelled at the uintptr twin:
            // the entry is stale by construction there, Resolve validates on read and answers
            // MISS, and dropping it would be a second undeclared change inside a repair.
            ManagedPointerTokens.RegisterPinned((nuint)ptr, value);
            return ptr;
        }
    }
}
