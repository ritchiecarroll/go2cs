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

public abstract partial class ж<T> : IPointer<T>, IEquatable<ж<T>>, INilPointer, IUntypedSlotAccess, IAllocationIdentity
{
    // The ONE storage fact every kind shares: whether this box IS the nil pointer. STRUCTURAL —
    // set only at construction, by the kind ctor contracts (see the class remarks); the
    // value-peeking refinement for a standard box lives in StandardBox.IsNull.
    private protected readonly bool m_isNull;

    // Option 3b: a heap box's allocation id, minted on its first token read (StandardBox). It is declared
    // HERE, beside m_isNull, because the CLR lays out a base class's fields as one pointer-aligned block:
    // an int in the derived class costs a box 8 bytes, while here it fills m_isNull's padding for 0.
    private protected int m_id;

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

    // The element view of a pointer-to-array that names NATIVE memory (Go's *[N]T over off-heap
    // storage), or null for every other kind — which is every kind but one, so the default is the
    // whole answer for the corpus. Consulted by arrayView BEFORE it touches Value, because such a
    // box HAS no materializable Value: Go's *[N]T points at bare contiguous elements with no
    // header, so reading an array<T> from that address would reinterpret element bytes AS the
    // header and hand back a garbage T[] (Q58; the measured prestub null read this seam exists to
    // prevent).
    //
    // A METHOD returning null, never a field: this base is per-BOX, so instance state added here
    // is a corpus-wide byte cost on every pointer in the corpus. The address and the length live
    // on the one kind that has them.
    internal virtual IArray<Telem>? TryGetNativeArrayView<Telem>() => null;

    // ---- minting (the of()/at() surface — unchanged signatures, kind ctors behind them) ----

    /// <summary>
    /// Gets a pointer to a field of the struct this pointer references (Go's <c>&amp;p.field</c>).
    /// </summary>
    public ж<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc)
    {
        // the cached view: one FieldRefBox per (box, accessor) for the box's life (ж.Views.cs)
        return viewOf(fieldRefFunc);
    }

    /// <summary>
    /// Gets a pointer to a field of the struct this pointer references, via a typed accessor
    /// (equality compares the ORIGINAL accessor — see FieldRefBox). The view is cached per
    /// (box, accessor); the per-call wrapper is resolved only on a cache miss (ж.Views.cs).
    /// </summary>
    public ж<TElem> of<TElem>(FieldRefFunc<T, TElem> fieldRefFunc)
    {
        return viewOf(fieldRefFunc);
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
        // A pointer-to-array over NATIVE memory supplies its own view, and must be asked FIRST:
        // every path below reads Value, which for such a box would read element bytes as an
        // array<T> header. Null for every other kind, so nothing else changes shape here.
        if (TryGetNativeArrayView<Telem>() is { } nativeView)
            return nativeView;

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

        // Go's &p[i] through a pointer-to-array panics with runtime.boundsError, which recover() sees;
        // a raw IndexOutOfRangeException escaped it.
        if (!array.IndexIsValid(index))
            throw RuntimeErrorPanic.IndexOutOfRange(index, array.Length);

        return new ElemRefBox<Telem>(array, (int)index);
    }

    /// <summary>
    /// <see cref="at{Telem}(nint)"/> for an UNSIGNED index, checked at its full value before any narrowing
    /// (goPanicIndexU): the converter emits a uint/uint32/uint64/uintptr index bare onto this overload,
    /// where a <c>(nint)</c> cast read an index at or above 2^63 as negative.
    /// </summary>
    // The int form completes Ꮡ(x, i)'s int/nint/ulong set, so a literal or a small-integer index keeps binding
    // exactly with the ulong overload beside it.
    public ж<Telem> at<Telem>(int index) => at<Telem>((nint)index);

    public ж<Telem> at<Telem>(ulong index)
    {
        IArray<Telem> array = arrayView<Telem>();

        if (index >= (ulong)array.Length)
            throw RuntimeErrorPanic.IndexOutOfRange(index, array.Length);

        return new ElemRefBox<Telem>(array, (int)index);
    }

    public ж<TElem> at<TElem>(FieldRefFunc<T, array<TElem>> fieldRefFunc, int index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<array<TElem>> fieldRefFunc, int index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<T, slice<TElem>> fieldRefFunc, int index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<slice<TElem>> fieldRefFunc, int index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, array<TElem>> fieldPtrFunc, int index) => of(fieldPtrFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, slice<TElem>> fieldPtrFunc, int index) => of(fieldPtrFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<T, array<TElem>> fieldRefFunc, ulong index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<array<TElem>> fieldRefFunc, ulong index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<T, slice<TElem>> fieldRefFunc, ulong index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldRefFunc<slice<TElem>> fieldRefFunc, ulong index) => of(fieldRefFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, array<TElem>> fieldPtrFunc, ulong index) => of(fieldPtrFunc).at<TElem>(index);

    public ж<TElem> at<TElem>(FieldPtrFunc<T, slice<TElem>> fieldPtrFunc, ulong index) => of(fieldPtrFunc).at<TElem>(index);

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

    // An allocation's token base: its UNIQUE allocation id (ManagedPointerTokens.NextIdentity: 29
    // bits, never 0) lifted clear of the low 32 bits, so every base is 8-aligned and the whole low
    // half is available to carry a within-allocation displacement -- and TAGGED non-canonical, so
    // the value announces itself as a token.
    //
    // THE TAG (DESIGN-token-value-tag-refusal.md outcome B). x86-64 requires bits 63..47 of a
    // valid user-mode address to be ALL EQUAL. Forcing bit 63 = 1 and bit 47 = 0 makes every
    // token non-canonical, so no real address can be mistaken for a token and no token for an
    // address -- by construction rather than by table, which is what lets the syscall trampoline
    // refuse one from the VALUE alone (see ManagedPointerTokens.IsTaggedToken, which reads back
    // exactly the two bits this sets -- they are declared there so mint and door cannot drift).
    //
    //     bit 63 | bit 62 | 61..48 id hi | bit 47 | 46..32 id lo | 31..0 displacement
    //        1   |    0   |    14 bits   |    0   |    15 bits   |      32 bits
    //
    // The displacement stays 32 bits, so ElemRefBox's absolute index, FieldRefBox's offset and
    // ManagedPointerTokens.IsTokenArithmetic's `& ~0xFFFFFFFF` are all untouched and the ordering
    // contract is unchanged.
    //
    // THE ID (option 3, ruling 2026-09-30). The base was the CLR identity hash, which is not unique: two
    // LIVE objects share one (the first pair among live boxes came after 4622 of them), and because a
    // reference-bearing box registers its token on every uintptr conversion and the registry keeps the
    // last writer, `(*T)(uintptr)p` of the first box's number resolved to the second -- a silent wrong
    // object. An id names one allocation. Bit 62 is clear by construction (the 14-bit hi mask), so a
    // base never meets ManagedPointerTokens' identity band. Ids wrap after 2^29 mints: only an
    // allocation that stays live across that many token reads can share its id.
    private protected static nuint AllocationBase(ulong id) =>
        unchecked((nuint)(ManagedPointerTokens.TagBit | ((id >> 15 & 0x3FFF) << 48) | ((id & 0x7FFF) << 32)));

    // The allocation this pointer's token names. A heap box is its own allocation (StandardBox keeps
    // the id in m_id); element and field references name their storage's; any other kind asks the table.
    internal virtual ulong AllocationId => ManagedPointerTokens.IdentityOf(this);

    ulong IAllocationIdentity.AllocationId => AllocationId;

    /// <inheritdoc/>
    /// <remarks>
    /// The object whose lifetime is the referenced Go allocation: an element reference → the
    /// canonical backing storage; a field reference → its source allocation, recursively; a heap
    /// box (and a native alias, which names no managed allocation) → the box itself.
    /// </remarks>
    public virtual object ReferentObject => this;

    /// <summary>
    /// Whether this pointer is <see cref="GoZeroBase"/>, the one address Go answers for every zero-byte
    /// allocation. Only a heap box of a zero-size type and an element reference over the shared
    /// zero-size or zero-capacity slot say yes; every other kind names its own storage. When it is true
    /// the pointer's equality, hash, order token, referent and address are all the zerobase's.
    /// </summary>
    internal virtual bool NamesZeroBase => false;

    // The address this box converts to, minted by the same operator every `uintptr(p)` uses (nil -> 0,
    // native -> its address, fixed array -> its pinned data, value slot -> its stable address, all
    // registered) -- exposed non-generically so unsafe.Pointer's box-retaining constructor can hold
    // the box it names beside that number (increment E3 root 4).
    nuint INilPointer.StableAddress() => (nuint)(uintptr)this;

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

    /// <inheritdoc/>
    /// <remarks>
    /// THREE facts, and none of them is redundant.
    ///
    /// <see cref="NativeAddress"/> first, because the two NATIVE kinds answer
    /// <see cref="PointerStorage.None"/> too — "no MANAGED storage to name" — while their
    /// <see cref="PointerOrderToken"/> IS a real machine address (ж.NativeArrayBox.cs:143,
    /// ж.NativeBox.cs:92). Both of the other conjuncts therefore hold for a native-backed box at its
    /// own address, and reflect's pointer projection registers exactly that number
    /// (reflect/value_impl.cs:1287, unsafe.cs:580) — so without this test the refusal below would
    /// take real native addresses with it, which is the failure the enum's own retirement condition
    /// names.
    ///
    /// <see cref="StorageKind"/> second, because it is the fact the forward conversion decided ON:
    /// the token is registered on the <see cref="PointerStorage.None"/> arm and nowhere else, so
    /// this is the same question read back rather than a second rule that can drift from it.
    ///
    /// <see cref="PointerOrderToken"/> third, and it is what keeps the answer at OFFSET 0: a number
    /// that resolved to this box WITHOUT being its token came through the pinned-provenance route
    /// and is a real address (Q44 §10.3 arm 2b), which must keep the answer it has today.
    /// </remarks>
    public bool IsOrderTokenAt(nuint number)
    {
        return NativeAddress == 0 && StorageKind is PointerStorage.None && PointerOrderToken == number;
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

    // Per-INSTANTIATION constant: is T golib's own `array<E>`? A pure TYPE query, exactly like
    // s_publishArrayBacking above and subject to the same rule stated there — it builds no code, so
    // ILC answers it from the type system and nothing reflection-BUILT is reintroduced.
    //
    // It exists for the SAFETY FLOOR in the uintptr operator below (q100;
    // docs/phase4/DESIGN-native-array-view.md §4 as amended). Computed once per instantiation
    // because the operator is on a hot path and the answer cannot change.
    private static readonly bool s_isArrayShaped =
        typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(array<>);

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
        // ONE resolve, its result classified rather than re-queried: the Q44 §10.5 census needs to
        // distinguish "resolved to another pointee type" from "did not resolve", and calling Resolve
        // twice could answer differently across a collection.
        object? resolved = ManagedPointerTokens.Resolve((nuint)value.Value);

        if (resolved is ж<T> aliased)
        {
            if (Q44RegistryCensus.Enabled)
                Q44RegistryCensus.Arm1();

            return aliased;
        }

        // The zerobase's address, read back as a pointer to a zero-size type: a zerobase pointer of
        // that type, exactly what converting one to a number and back is in Go. The box answers the
        // zerobase's identity (GoZeroBase), and a zero-size value has nothing to read or write.
        if (GoZeroSizeFacts<T>.IsZeroSize && GoZeroBase.Is(resolved))
            return new StandardBox<T>(default(T)!);

        // ARM 2 (§10.3): the token named a LIVE box whose pointee type is not T. COUNTED FIRST and
        // unconditionally, so the census still reports every arrival at this arm -- including the 2a
        // subset the line below now diverts. An instrument that stops counting the cases a fix
        // handles cannot show the fix working.
        if (Q44RegistryCensus.Enabled && resolved is not null)
            Q44RegistryCensus.Arm2(typeof(T), resolved,
                                   ManagedPointerTokens.CurrentToken(resolved) == (nuint)value.Value);

        // ARM 2a, CLASSIFIED HERE AND ANSWERED AT THE DEREFERENCE (2026-09-20). The number IS a live
        // box's own ORDER TOKEN and that box's pointee type is not T: a REFERENCE-BEARING pointee
        // registered an order token rather than an address (the forward operator's
        // PointerStorage.None arm below), so NO MEMORY ANSWERS TO THIS NUMBER.
        //
        // ⚠ A REFUSAL STOOD HERE FOR ONE MEASUREMENT AND WAS WITHDRAWN, with the measurement kept so
        // nobody rebuilds it. Refusing the CONVERSION took SEVEN GolibTests red against an empty base
        // -- PointerTokenConversionTests' "loud form" row, four ReinterpretSourceRetentionTests, and
        // one each in RuntimeHashFamilyTests and SliceHeaderReinterpretTests -- because the native box
        // OVER THE TOKEN is load-bearing and deliberate: its address IS the token, which is how
        // PointerExtensions.Reinterpret's unpinnable class (ж.PointerExtensions.cs:185) and the
        // boundary wrappers recover the source. The design had already ruled on exactly this, in as
        // many words at RuntimeHashFamilyTests.cs:182 -- "a dereference is the row-level fault the
        // design chose, never a number". So the conversion is ADMITTED, the fact travels IN THE BOX,
        // and the fault lands where the charter puts it.
        //
        // WHAT THE FLAG BUYS is the failure MODE, which is the whole of the fix: the dereference was
        // an UNCATCHABLE AccessViolation on an unmapped page and is now a caught panic that names
        // itself. Go's own `setField` -- `*(*V)(unsafe.Add(unsafe.Pointer(&in), offset))` at
        // reflect/all_test.go:1409, called at OFFSET 0 over `struct{_, a, _ func()}`
        // (all_test.go:1510) -- is that write, and it ended reflect's test host at TestIsZero with 195
        // tests started and no verdict for any of them.
        //
        // The arithmetic refusal below cannot cover this BY CONSTRUCTION: IsTokenArithmetic requires
        // `allocationBase != number`, and at offset 0 the number IS the base.
        //
        // SCOPED BY IsOrderTokenAt, which tests three facts rather than "resolved to another pointee
        // type": the offset-0 sites over PINNED storage (arm 2b, a REAL address through the
        // provenance route) and the native-backed kinds are NOT flagged and dereference exactly as
        // they do today. A flag drawn one step wider would refuse real reads, and nothing else in the
        // tree would notice -- the rows would still report and the packages would still pass.
        //
        // The model question -- a Go-layout byte offset into CLR-auto-laid-out storage -- is NOT
        // answered here, exactly as the arithmetic twin does not answer it. It stays open, and says
        // so out loud instead of corrupting memory.
        bool aliasesAnOrderToken = resolved is INilPointer tokenBox &&
                                   tokenBox.IsOrderTokenAt((nuint)value.Value);

        // THE INTERIOR-ALIAS STEP (seat (c), M4; GoReflect.InteriorAlias.cs), at offset 0. Taken ONLY
        // where the flag above is set -- a box that today would refuse its first dereference -- so no
        // conversion that works today reaches it. It answers a real alias only where the base's Go
        // layout puts a T-typed node exactly at this offset (`*(*V)(unsafe.Pointer(&s))` over a
        // struct whose first field IS a V); any other shape keeps the flagged carrier unchanged.
        if (aliasesAnOrderToken && resolveInterior(resolved!, 0, fromArm3: false) is { } prefix)
            return prefix;

        // THE REFUSAL. A number inside a LIVE token's own 4 GiB block, that is not that token, is
        // a token somebody did arithmetic on — `unsafe.Add(unsafe.Pointer(&v), offset)` over storage
        // that has no address. Answering a native box over it is not "best effort": the write that
        // follows lands on an unmapped page and takes the process down UNCATCHABLY, which is how
        // reflect's TestIsZero went from 388 verdicts to 167 — not more broken, just no longer alive
        // to report. Refusing by name restores the failure MODE the tree had before the token arm
        // existed (a caught panic the arm recovers from) without pretending the underlying model
        // question is answered: a Go-layout byte offset into CLR-auto-laid-out storage still has no
        // meaning, and now says so out loud instead of corrupting memory.
        if (ManagedPointerTokens.IsTokenArithmetic((nuint)value.Value))
        {
            if (Q44RegistryCensus.Enabled)
                Q44RegistryCensus.Arm3();

            // The same interior-alias step, at the offset the arithmetic added. Before it this line
            // refused unconditionally; it still refuses, by the same name, for every offset that does
            // not land exactly on a T-typed node of the base's Go layout.
            if (ManagedPointerTokens.ResolveArithmeticBase((nuint)value.Value, out nuint offset) is { } arithmeticBase &&
                resolveInterior(arithmeticBase, offset, fromArm3: true) is { } interior)
            {
                return interior;
            }

            throw RuntimeErrorPanic.UnsafePointerArithmeticWithoutAddress();
        }

        // ⚠ CLASSIFIED FROM THE RESOLVE ALREADY PERFORMED, never a second call. This line read
        // `ManagedPointerTokens.Resolve(...) is null` until 2026-09-08, and that made the census
        // NOT OBSERVATION-ONLY: `Resolve` EVICTS — a dead weak entry is TryRemove'd and the count
        // reassigned — so enabling the census mutated the registry at moments the uninstrumented
        // program never would. i9 measured the consequence: the banked `os` row flipped PASS -> FAIL
        // with the env gate as the ONLY variable, twice, in both directions. Reaching this line
        // already means `resolved` was not a `ж<T>` and the arithmetic refusal did not fire, so
        // `resolved is null` IS arm 4 — the same verdict, from a value already in hand, with the
        // census performing exactly the calls the census-off path performs.
        if (Q44RegistryCensus.Enabled && resolved is null)
            Q44RegistryCensus.Arm4();

        // ARM 5 — THE SAFETY FLOOR, and it is the one arm here that refuses a SHAPE rather than a
        // number (q100; docs/phase4/DESIGN-native-array-view.md §4, whose §4 amendment block says
        // why this is PROVENANCE-tested and not type-tested).
        //
        // `array<E>` is a MANAGED struct whose first field is an `E[]` reference, and a native box
        // materializes its value with `Unsafe.AsRef<T>((void*)addr)` — so composing the two
        // REINTERPRETS whatever bytes live at that address AS A MANAGED REFERENCE and dereferences
        // it. Measured in GolibTests against golib directly: zeroed memory reads `Length = 0`, a
        // SILENT WRONG ANSWER, and memory filled with 0xAB reads `Length = -1414812757` — the data
        // bytes themselves. It fabricated a managed reference out of content and returned a number
        // instead of faulting, by luck. That is a type-safety hole, not a wrong result.
        //
        // ⚠ THE DISCRIMINATOR IS THE ADDRESS'S PROVENANCE, NEVER A TEST ON T, and that is why this
        // arm sits HERE rather than at the `array<E>` end. A floor keyed on T alone was ratified and
        // then WITHDRAWN AS SPECIFIED on lane R's measured disproof — 6 of 609 behavioral tests red,
        // because pinned-managed round-trips, pointer-shaped T and container shapes over pinned
        // storage are all Go-legal and all arrive at this same operator; Go's own
        // `*(*[2]uintptr)(p)` over pinned managed storage is one of them. What separates them is
        // whether the address carries a PROVENANCE RECORD, and FIRING at this line means it carries
        // none. That record is what docs/phase4/DESIGN-pointer-provenance.md (RATIFIED, landed)
        // supplies, and this floor was not implementable before it existed.
        //
        // ⚠ REACHING this line does NOT mean that, and the distinction is load-bearing rather than
        // pedantic. THREE things above divert control: arm 1 RETURNS the recovered box, and the two
        // refusals — arm 2a's order-token refusal and the token-arithmetic one — THROW. The census
        // calls themselves are `if (Q44RegistryCensus.Enabled && …)` COUNTERS: they divert nothing,
        // and on the production path, with the census off, they do not execute at all.
        //
        // ⚠ THIS PARAGRAPH SAID "arms 2 and 4 divert nothing" AND IT IS NOW HALF FALSE — corrected
        // here rather than left to read as the design (2026-09-20, the arm 2a refusal). Arm 2 is no
        // longer only a counter: its 2a SUBSET (the number IS a live box's order token, a
        // reference-bearing pointee with no address) now throws before this line, because the write
        // that followed the native box was the uncatchable fault that ended reflect's test host.
        // What still arrives HERE with `resolved` NON-null is arm 2b — an address resolving to a
        // LIVE box of another pointee type through the PINNED-provenance route, a real address — and
        // what lets it through is the `resolved is null` conjunct in the condition below.
        //
        // ⚠ WHICH IS WHY THE CONDITION IS NOT SIMPLIFIED TO `if (s_isArrayShaped)`. That conjunct
        // looks redundant only to a reader who believes reaching implies firing — and the edit it
        // invites IS the type-tested floor that was ratified and then WITHDRAWN AS SPECIFIED at
        // 6 of 609 behavioral tests red. NativeArrayViewFloorTests arm 3 is the regression test that
        // would catch it, a unit-level proxy for that tier rather than a measurement of it. This
        // paragraph replaces a sentence that asserted the opposite (C2's second-lane read; the
        // comment was the one place a maintainer would look before making exactly that edit).
        //
        // It CURES NOTHING, and is not meant to: by the design's §1.5 liveness audit no live path on
        // the roster reaches these sites, so nothing that works today starts failing. What it buys is
        // that the NEXT arrival — and Phase 4 manufactures arrivals — announces itself HERE instead
        // of as a plausible panic several layers away. The netpoll recv cost a full misattribution,
        // through a design, a ratification and four documents, for exactly that reason.
        //
        // ⚠ TWO BOUNDS, stated rather than left to be found.
        //
        // ONE — the WEAK-ENTRY bound: a pinned address's provenance record is a weak entry that dies
        // with its box. A program that stashes a uintptr, drops every reference to the box and
        // converts back later would reach this arm and be refused — but that program is one Go
        // itself declares invalid, a uintptr not keeping its referent alive.
        //
        // TWO — the NO-PROVENANCE bound, which is the one the condition actually draws: this refuses
        // the no-provenance class, NOT "fabricated array views" in general. An address that resolves
        // to a LIVE box of a different pointee type, at an `array<U>` pointee, still falls through to
        // `NativeBox` UNREFUSED. That is by design and consistent with "It CURES NOTHING" above;
        // NativeArrayViewFloorTests arm 3 exercises that shape but asserts `IsNotNull` only, so the
        // bound is a stated property and not an accident of the arms. Widening to cover it would be
        // a different cut with its own measurement, not a tightening of this one.
        //
        // ⚠ THAT BOUND NARROWED ON 2026-09-20 and the sentence above is kept rather than rewritten,
        // because it still describes what THIS condition draws. What changed is upstream: the arm 2a
        // refusal now takes the ORDER-TOKEN half of "resolves to a LIVE box of a different pointee
        // type" before it can arrive. The half that still arrives — and that the sentence above is
        // now about — is arm 2b, the PINNED-provenance half, which is exactly the shape
        // NativeArrayViewFloorTests arm 3 drives: a fixed array pins its DATA address, so the number
        // is the pinned address and never the box's order token.
        //
        // `array.cs`'s AliasPointer needs no change of its own: its documented raw-metal fallback is
        // `return (ж<array<T>>)(uintptr)element!`, which funnels through this operator.
        if (resolved is null && s_isArrayShaped)
            throw RuntimeErrorPanic.NativeArrayViewWithoutElementStorage(typeof(T));

        // The fall-through, carrying arm 2a's verdict IN THE BOX. `aliasesAnOrderToken` is false for
        // every other arrival, so an ordinary native pointer's dereference gains a branch on a
        // readonly field and NOTHING ELSE -- no registry lookup, no resolve, no work the census-off
        // path did not already do. The knowledge was computed once, at the one place that had the
        // resolved box in hand; making the accessor re-derive it would put a token lookup on every
        // native read in the corpus.
        return new NativeBox<T>((nuint)value.Value, aliasesAnOrderToken: aliasesAnOrderToken);
    }

    // The interior-alias step's one call shape, shared by both refusal arms: walk, record the path
    // shape when the census is on, and answer the alias box or null (today's refusal stands).
    private static ж<T>? resolveInterior(object baseBox, nuint offset, bool fromArm3)
    {
        object? alias = GoReflect.ResolveInteriorAlias(baseBox, offset, typeof(T), out string shape);

        if (Q44RegistryCensus.Enabled)
            Q44RegistryCensus.Interior(baseBox, typeof(T), fromArm3, shape, alias is not null);

        return alias as ж<T>;
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

        // Every zerobase pointer is ONE address, the zerobase's own (GoZeroBase): Go's
        // uintptr(unsafe.Pointer(new(struct{}))) is &zerobase for every such allocation.
        if (value.NamesZeroBase)
            return GoZeroBase.Address;

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

        // The zerobase's one address, as in the uintptr operator above.
        if (value.NamesZeroBase)
            return (void*)GoZeroBase.Address;

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
