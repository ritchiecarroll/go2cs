// GoAsyncIO.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace go.golib;

/// <summary>
/// The seam between a package that SUBMITS asynchronous OS operations and a package that WAITS for
/// them, for descriptor-based IO where the two cannot reference one another.
/// </summary>
/// <remarks>
/// <para>
/// WHY THIS LIVES IN GOLIB, WHICH IS THE WHOLE REASON IT EXISTS. Go's network poller is one package
/// (<c>internal/poll</c>) and its overlapped submissions are others (<c>syscall</c>,
/// <c>internal/syscall/windows</c>); <c>internal/poll</c> REFERENCES both, so a completion callback
/// raised on the submitting side has no legal way to call back into the waiter. Go closes that loop
/// with pointer arithmetic — its poller reads the enclosing <c>operation</c> back out of the
/// OVERLAPPED it dequeued — which go2cs cannot reproduce, because a <c>ж&lt;T&gt;</c> field
/// reference does not expose the object it was taken from. What remains is a PUSH through the one
/// assembly both sides see, which is this one. See
/// <c>docs/phase4/DESIGN-netpoll-managed-poller.md</c> §4.4.
/// </para>
/// <para>
/// It is deliberately PLATFORM-NEUTRAL and mechanism-neutral: a descriptor, a mode, an address, and
/// two opaque state slots. Naming Windows' completion machinery here would drag one platform's IO
/// model into the runtime library every converted program links. The submitting package owns the
/// mechanism; this type owns only the rendezvous.
/// </para>
/// <para>
/// THE DESCRIPTOR IS THE KEY because it is the one identity both sides independently hold — the
/// waiter receives it when it registers the descriptor, and a submitting wrapper receives it as its
/// own first argument. Nothing else in either package's signature is common to both.
/// </para>
/// </remarks>
public static class GoAsyncIO
{
    // Descriptor -> readiness sink. The sink takes the MODE the completion belongs to; the waiter's
    // own closure supplies everything else it needs.
    private static readonly ConcurrentDictionary<nuint, Action<nint>> s_sinks = new();

    // Descriptor -> the submitting package's per-descriptor state, opaque here.
    private static readonly ConcurrentDictionary<nuint, Lazy<object>> s_descriptorState = new();

    // Operation key -> the submitting package's per-operation state, opaque here. The key is
    // whatever pointer the WAITER names the operation by, which is why it is typed as `object`:
    // golib must not know what a `*Overlapped` is. Equality is the key's own — for a go2cs pointer
    // that is (source box, field identity), which is exactly the property that makes three separate
    // mints of `&o.o` at three call sites resolve to ONE record.
    private static readonly ConcurrentDictionary<object, operationSlot> s_operationState = new();

    // Descriptors the WAITER has taken off the completion mechanism (see MarkUnassociated). A
    // submitting package reads this to decide how to issue an operation, and a record keeps the
    // answer it was built under, so a mark retires every record built before it.
    private static readonly ConcurrentDictionary<nuint, bool> s_unassociated = new();

    // One operation's state, plus the (descriptor, mode) a submit bound it to and whether that
    // descriptor was unassociated when it did. A slot made by GetOrCreateOperationState alone is
    // UNBOUND and matches any binding; a slot made by GetOrBindOperationState matches only its own
    // pair and association state (see that method for why).
    private sealed class operationSlot(Lazy<object> state, bool bound, nuint descriptor, nint mode, bool unassociated = false)
    {
        internal readonly Lazy<object> State = state;

        internal bool Matches(nuint other, nint otherMode) =>
            !bound || (descriptor == other && mode == otherMode && unassociated == IsUnassociated(other));
    }

    // Completion work an operation owes, keyed the same way its state is (netpoll design SS4.8).
    // Separate from s_operationState because it is per-SUBMISSION and transient, while the state is
    // per-operation and long-lived: a record outlives many submissions, each of which may or may not
    // owe a decode.
    private static readonly ConcurrentDictionary<object, Action<nint>> s_operationCompletions = new();

    /// <summary>
    /// Registers (or replaces, or with <c>null</c> removes) the readiness sink for
    /// <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="descriptor">OS descriptor the sink speaks for.</param>
    /// <param name="sink">Called with the mode of each completion; <c>null</c> unregisters.</param>
    /// <remarks>
    /// REPLACEMENT IS REQUIRED, not merely tolerated: the kernel reissues descriptor numbers after a
    /// close, so a registration that refused to overwrite would leave a retired waiter being woken
    /// for a descriptor it no longer owns. Measured while prototyping this seam.
    /// </remarks>
    public static void SetReadiness(nuint descriptor, Action<nint>? sink)
    {
        if (sink is null)
        {
            s_sinks.TryRemove(descriptor, out _);
        }
        else
        {
            // A new registration is a new descriptor that reuses a retired number, so it starts
            // associable whatever the old one was marked.
            s_unassociated.TryRemove(descriptor, out _);
            s_sinks[descriptor] = sink;
        }
    }

    /// <summary>
    /// Signals that an operation on <paramref name="descriptor"/> completed in
    /// <paramref name="mode"/>.
    /// </summary>
    /// <param name="descriptor">OS descriptor the completion belongs to.</param>
    /// <param name="mode">Mode of the completed operation, in the waiter's own vocabulary.</param>
    /// <returns><c>true</c> when a sink was registered and ran.</returns>
    /// <remarks>
    /// A signal for an UNREGISTERED descriptor is a silent no-op by contract, for a reason that is
    /// not politeness: this runs on an IO completion callback, where an escaping exception ends the
    /// process, and a completion racing its own descriptor's close is a race the poller contract
    /// permits.
    /// </remarks>
    public static bool Signal(nuint descriptor, nint mode)
    {
        if (!s_sinks.TryGetValue(descriptor, out Action<nint>? sink))
            return false;

        sink(mode);

        return true;
    }

    /// <summary>
    /// Gets the submitting package's state for <paramref name="descriptor"/>, creating it EXACTLY
    /// once.
    /// </summary>
    /// <param name="descriptor">OS descriptor the state belongs to.</param>
    /// <param name="factory">Builds the state on the one thread that wins the race.</param>
    /// <returns>The one state object for this descriptor.</returns>
    /// <remarks>
    /// EXACTLY-ONCE IS LOAD-BEARING AND <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,
    /// Func{TKey,TValue})"/> DOES NOT PROVIDE IT. Its factory may run on several threads with only
    /// one result kept; a contention test over the bare form built ten operation records where the
    /// contract wants one, and each discarded record owned native allocations with no owner left to
    /// free them. Per-descriptor state has the same requirement for a stronger reason still — on
    /// Windows that object is the completion-port association, and associating one socket twice is a
    /// kernel error. <see cref="Lazy{T}"/> with
    /// <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> is the shape that actually holds.
    /// </remarks>
    public static object GetOrCreateDescriptorState(nuint descriptor, Func<object> factory) =>
        s_descriptorState.GetOrAdd(descriptor, _ => new Lazy<object>(factory, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    /// <summary>
    /// Gets the submitting package's state for <paramref name="descriptor"/> without creating it.
    /// </summary>
    /// <param name="descriptor">OS descriptor to look up.</param>
    /// <param name="state">The state, when one exists.</param>
    /// <returns><c>true</c> when a state object exists for this descriptor.</returns>
    public static bool TryGetDescriptorState(nuint descriptor, out object? state)
    {
        state = s_descriptorState.TryGetValue(descriptor, out Lazy<object>? lazy) ? lazy.Value : null;

        return state is not null;
    }

    /// <summary>
    /// Retires <paramref name="descriptor"/>: removes its readiness sink and its state, disposing
    /// the state when it is disposable.
    /// </summary>
    /// <param name="descriptor">OS descriptor to retire.</param>
    /// <remarks>
    /// Called by the WAITER when it stops owning the descriptor, which on the netpoll path is
    /// <c>pollClose</c> — deliberately BEFORE the descriptor itself is closed, so a submitting
    /// package can unregister from whatever OS machinery it associated the descriptor with while
    /// that association is still legal. Disposal never closes the descriptor; that stays the
    /// waiter's throughout.
    /// </remarks>
    public static void RemoveDescriptor(nuint descriptor)
    {
        s_sinks.TryRemove(descriptor, out _);
        s_unassociated.TryRemove(descriptor, out _);

        if (s_descriptorState.TryRemove(descriptor, out Lazy<object>? lazy) && lazy.IsValueCreated)
            (lazy.Value as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Records that the WAITER has taken <paramref name="descriptor"/> off the completion mechanism,
    /// so no submitting package may associate it with one again.
    /// </summary>
    /// <param name="descriptor">OS descriptor the waiter disassociated.</param>
    /// <remarks>
    /// <para>
    /// This is Go's <c>(*FD).DisassociateIOCP</c> post-condition: the handle belongs to no completion
    /// port, and its later IO waits on the operation's own event (<c>waitIO</c>'s non-pollable arm).
    /// A submitting package that associates descriptors LAZILY, at the first operation, would
    /// otherwise associate a disassociated handle again on its next submit, and route that
    /// operation's completion to a port nobody is waiting on.
    /// </para>
    /// <para>
    /// The mark lives until <see cref="RemoveDescriptor"/> retires the descriptor or a new
    /// <see cref="SetReadiness"/> registers its number again. Every operation record built before
    /// the mark stops matching (see <see cref="GetOrBindOperationState"/>), so the next submit on
    /// that descriptor builds a record under the new state.
    /// </para>
    /// </remarks>
    public static void MarkUnassociated(nuint descriptor) => s_unassociated[descriptor] = true;

    /// <summary>
    /// Reports whether <paramref name="descriptor"/> was marked by <see cref="MarkUnassociated"/>.
    /// </summary>
    /// <param name="descriptor">OS descriptor to look up.</param>
    /// <returns><c>true</c> when the descriptor may not be associated.</returns>
    public static bool IsUnassociated(nuint descriptor) => s_unassociated.ContainsKey(descriptor);

    /// <summary>
    /// Gets the submitting package's state for one operation, creating it EXACTLY once.
    /// </summary>
    /// <param name="key">Pointer the operation is named by, on both sides.</param>
    /// <param name="factory">Builds the state on the one thread that wins the race.</param>
    /// <returns>The one state object for this operation.</returns>
    /// <remarks>See <see cref="GetOrCreateDescriptorState"/> for why this is not a bare
    /// <c>GetOrAdd</c>.</remarks>
    public static object GetOrCreateOperationState(object key, Func<object> factory) =>
        s_operationState.GetOrAdd(key, _ => new operationSlot(new Lazy<object>(factory, LazyThreadSafetyMode.ExecutionAndPublication), false, 0, 0)).State.Value;

    /// <summary>
    /// Gets the state for one operation BOUND to <paramref name="descriptor"/> and
    /// <paramref name="mode"/>, creating it exactly once, and retiring a record the same key holds
    /// for a different binding.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="descriptor">Descriptor this submission is for.</param>
    /// <param name="mode">Submitting package's own mode value.</param>
    /// <param name="factory">Builds the state on the one thread that wins the race.</param>
    /// <returns>The one state object for this operation and binding.</returns>
    /// <remarks>
    /// <para>
    /// A record keeps what it was created for: its OS association belongs to one descriptor, and its
    /// completion signals one mode. Through Go 1.24 that matched the key's life, because each FD
    /// owned exactly one read and one write operation. Go 1.26's internal/poll takes every operation
    /// from a process-wide <c>sync.Pool</c> (<c>fd_windows.go</c>'s <c>operationPool</c>), so one
    /// key is reused across descriptors and across modes, and a record found by key alone would
    /// submit on the wrong association and wake the wrong waiter.
    /// </para>
    /// <para>
    /// So a key reused for a different pair gets a NEW record, and so does a key whose descriptor was
    /// marked by <see cref="MarkUnassociated"/> after its record was built. The old one is removed with any
    /// pending completion work and disposed when disposable. That is safe because a pooled
    /// operation goes back to the pool only after <c>execIO</c> has harvested it: nothing is in
    /// flight on the record being retired.
    /// </para>
    /// </remarks>
    public static object GetOrBindOperationState(object key, nuint descriptor, nint mode, Func<object> factory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        while (true)
        {
            operationSlot slot = s_operationState.GetOrAdd(key, _ =>
                new operationSlot(new Lazy<object>(factory, LazyThreadSafetyMode.ExecutionAndPublication), true, descriptor, mode,
                    IsUnassociated(descriptor)));

            if (slot.Matches(descriptor, mode))
                return slot.State.Value;

            if (s_operationState.TryRemove(new KeyValuePair<object, operationSlot>(key, slot)))
            {
                s_operationCompletions.TryRemove(key, out _);

                if (slot.State.IsValueCreated)
                    (slot.State.Value as IDisposable)?.Dispose();
            }
        }
    }

    /// <summary>
    /// Gets the submitting package's state for one operation without creating it.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="state">The state, when one exists.</param>
    /// <returns><c>true</c> when a state object exists for this operation.</returns>
    public static bool TryGetOperationState(object key, out object? state)
    {
        state = s_operationState.TryGetValue(key, out operationSlot? slot) ? slot.State.Value : null;

        return state is not null;
    }

    /// <summary>
    /// Removes one operation's state.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <returns>The removed state, or <c>null</c> when there was none.</returns>
    public static object? RemoveOperationState(object key)
    {
        // Drop any pending completion work first: the operation is going away, so work it owes will
        // never run, and leaving the delegate behind would both leak whatever it captured and let a
        // LATER operation that reuses this key inherit a decode meant for the dead one.
        s_operationCompletions.TryRemove(key, out _);

        return s_operationState.TryRemove(key, out operationSlot? slot) && slot.State.IsValueCreated ? slot.State.Value : null;
    }

    /// <summary>
    /// Removes one operation's state only while <paramref name="key"/> still names
    /// <paramref name="state"/>.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="state">The state the caller is retiring.</param>
    /// <returns><c>true</c> when that state was removed.</returns>
    /// <remarks>
    /// For an owner retiring records it created, such as a descriptor's teardown: since
    /// <see cref="GetOrBindOperationState"/> can re-bind a key to a NEW record for another
    /// descriptor, removing by key alone could retire that other descriptor's live record.
    /// </remarks>
    public static bool RemoveOperationState(object key, object state)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!s_operationState.TryGetValue(key, out operationSlot? slot) || !slot.State.IsValueCreated ||
            !ReferenceEquals(slot.State.Value, state))
            return false;

        if (!s_operationState.TryRemove(new KeyValuePair<object, operationSlot>(key, slot)))
            return false;

        s_operationCompletions.TryRemove(key, out _);
        return true;
    }

    /// <summary>
    /// Reports the native address of the operation named by <paramref name="key"/>, when its state
    /// exposes one.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="address">The operation's native address.</param>
    /// <returns><c>true</c> when an address is available.</returns>
    /// <remarks>
    /// This is the ENTIRE contract between two SUBMITTING packages that must agree on one
    /// in-flight operation — the one that issues it and the one that harvests its result. Narrowing
    /// it to an address is what keeps the harvest calling the real OS routine against the real
    /// control block, instead of re-deriving the OS's own error mapping from whatever a completion
    /// callback happened to report.
    /// </remarks>
    public static bool TryGetOperationAddress(object key, out nuint address)
    {
        address = TryGetOperationState(key, out object? state) && state is IGoAsyncOperation operation
            ? operation.NativeAddress
            : 0;

        return address != 0;
    }

    // ---- the SUBMIT side (DESIGN-netpoll-managed-poller.md §4.7, RATIFIED 2026-08-23) ------------
    //
    // The harvest needed one property, an address, because the operation already existed. A SUBMIT
    // may be the FIRST touch of an operation, so it needs the record CREATED — and only the package
    // that owns the platform's IO machinery can create one. That package registers its factory here
    // once; this seam holds it as an opaque delegate and never learns what it makes. Everything
    // crossing this boundary is a descriptor, a waiter key, a mode, and a byte count.

    private static Func<nuint, object, nint, object>? s_operationFactory;

    /// <summary>
    /// Registers the factory that creates one operation's state, for use when a submit is the first
    /// touch of that operation.
    /// </summary>
    /// <param name="factory">Receives the descriptor, the waiter key and the mode; returns the state.</param>
    /// <remarks>
    /// Called once by the package that owns the platform's IO machinery. Registering a SECOND,
    /// different factory throws rather than silently changing which package owns in-flight
    /// operations; re-registering the same delegate is idempotent.
    /// </remarks>
    public static void SetOperationFactory(Func<nuint, object, nint, object> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Func<nuint, object, nint, object>? existing = Interlocked.CompareExchange(ref s_operationFactory, factory, null);

        if (existing is not null && !ReferenceEquals(existing, factory))
            throw new InvalidOperationException("GoAsyncIO: an operation factory is already registered");
    }

    /// <summary>
    /// Test-only: installs <paramref name="factory"/> as the operation factory, discarding any
    /// previous registration and every operation record created under it.
    /// </summary>
    /// <param name="factory">Factory to install, or <c>null</c> to leave the seam unregistered.</param>
    /// <remarks>
    /// <para>
    /// The PUBLIC rule is deliberately one-factory-per-process and <see cref="SetOperationFactory"/>
    /// enforces it strictly -- two owners for in-flight operations is the failure this seam exists to
    /// refuse. A TEST process is the one legitimate place that rule cannot hold: GolibTests references
    /// <c>core/syscall</c>, whose module initializer claims the factory at ASSEMBLY LOAD, so a test
    /// wanting a fake cannot win by ordering. Measured, not anticipated -- five tests failed in the
    /// full suite while passing filtered, which is precisely the order-dependence shape.
    /// </para>
    /// <para>
    /// Granting the swap to GolibTests -- which golib already trusts with its internals -- keeps the
    /// public contract strict rather than relaxing it for testability. This is NOT public, nothing in
    /// the corpus calls it, and it clears the record store because records outliving their factory
    /// would be owned by nobody.
    /// </para>
    /// </remarks>
    internal static void ReplaceOperationFactoryForTesting(Func<nuint, object, nint, object>? factory)
    {
        s_operationState.Clear();
        Interlocked.Exchange(ref s_operationFactory, factory);
    }

    /// <summary>
    /// Prepares the operation named by <paramref name="key"/> for a new submission, creating its
    /// state through the registered factory when this is its first touch.
    /// </summary>
    /// <param name="descriptor">Descriptor the operation belongs to.</param>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="mode">Submitting package's own mode value.</param>
    /// <returns>Address of the native control block for this submission.</returns>
    public static nuint RearmOperation(nuint descriptor, object key, nint mode)
    {
        ArgumentNullException.ThrowIfNull(key);

        Func<nuint, object, nint, object> factory = s_operationFactory
            ?? throw new InvalidOperationException("GoAsyncIO: no operation factory registered");

        // GetOrBindOperationState's Lazy(ExecutionAndPublication) is what holds this to ONE record
        // per waiter: a bare GetOrAdd runs its factory more than once under contention, and each
        // discarded record here owns native resources nothing would ever free (measured at S2b). The
        // binding is what keeps a POOLED operation (Go 1.26) from reusing another descriptor's record.
        object state = GetOrBindOperationState(key, descriptor, mode, () => factory(descriptor, key, mode));

        return state is IGoAsyncOperation operation
            ? operation.RearmForSubmit()
            : throw new InvalidOperationException("GoAsyncIO: operation state does not support submission");
    }

    /// <summary>
    /// Reports native memory owned by the operation named by <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="byteCount">Bytes required.</param>
    /// <returns>Address of the staged memory.</returns>
    public static nuint StageOperationBuffer(object key, int byteCount)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        return TryGetOperationState(key, out object? state) && state is IGoAsyncOperation operation
            ? operation.StageBytes(byteCount)
            : throw new InvalidOperationException("GoAsyncIO: no operation state to stage against; rearm first");
    }

    /// <summary>
    /// Work the operation named by <paramref name="key"/> owes when it completes.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="onComplete">Work to run once, given the completed transfer count.</param>
    /// <remarks>
    /// <para>
    /// The DECODE-side counterpart of <see cref="StageOperationBuffer"/> (netpoll design §4.8,
    /// RATIFIED). Staging alone solves the submit direction, where the caller writes and the OS
    /// reads; it cannot solve the receive direction, where the OS writes and the caller reads, and
    /// for an overlapped operation the OS writes AFTER the submitting wrapper has returned. There is
    /// no moment inside that wrapper at which a decode could run, so the decode has to be carried by
    /// the operation and run when it completes.
    /// </para>
    /// <para>
    /// Deliberately NOT a member of <see cref="IGoAsyncOperation"/>. That interface describes what a
    /// record can do with NATIVE resources, and every widening of it is a breaking change for every
    /// implementer — §4.7 widened it by two members and immediately broke a test double. Pending
    /// completion work is per-SUBMISSION transient state rather than a record capability, so it
    /// lives here and no implementer has to learn about it.
    /// </para>
    /// <para>
    /// What the work IS remains entirely the caller's business: this seam stores an opaque delegate
    /// and never inspects it, which is the same neutrality rule that keeps one platform's IO
    /// structures out of the shared runtime.
    /// </para>
    /// </remarks>
    public static void SetOperationCompletion(object key, Action<nint> onComplete)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(onComplete);

        s_operationCompletions[key] = onComplete;
    }

    /// <summary>
    /// Runs whatever the operation named by <paramref name="key"/> owes on completion, if anything.
    /// </summary>
    /// <param name="key">Pointer the operation is named by.</param>
    /// <param name="bytesTransferred">Transfer count the completion reported.</param>
    /// <returns><c>true</c> when work was pending and ran; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Runs AT MOST ONCE per registration, and that is structural rather than a convention: the
    /// delegate is REMOVED before it is invoked, so a harvest that races another harvest for the
    /// same operation — which `execIO` can genuinely do, since its cancellation path harvests the
    /// same operation its normal path does — decodes exactly once. An operation with nothing pending
    /// is the common case (every send, every TCP read) and is not an error.
    /// </remarks>
    public static bool CompleteOperation(object key, nint bytesTransferred)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!s_operationCompletions.TryRemove(key, out Action<nint>? onComplete))
            return false;

        onComplete(bytesTransferred);
        return true;
    }
}

/// <summary>
/// Implemented by a submitting package's per-operation state to expose the operation's native
/// control block to the package that harvests its result.
/// </summary>
public interface IGoAsyncOperation
{
    /// <summary>
    /// Address of the operation's native control block, or 0 when none is currently allocated.
    /// </summary>
    nuint NativeAddress { get; }

    /// <summary>
    /// Prepares this operation for a NEW submission and reports its native control block.
    /// </summary>
    /// <returns>Address of the control block the next submit must hand the OS.</returns>
    /// <remarks>
    /// The submit-side counterpart of <see cref="NativeAddress"/>, which reports an EXISTING
    /// operation's block for a harvest. Splitting them is deliberate: a harvest must never re-arm,
    /// and a submit must never reuse a block the OS may still own.
    /// </remarks>
    nuint RearmForSubmit();

    /// <summary>
    /// Reports native memory whose lifetime is this operation's, for data the OS retains until
    /// completion.
    /// </summary>
    /// <param name="byteCount">Bytes required.</param>
    /// <returns>Address of the staged memory.</returns>
    /// <remarks>
    /// A submitting package cannot stage such memory itself: a stack image is wrong by construction
    /// when the OS keeps the pointer past the call, and only the operation record has the right
    /// lifetime. What the bytes MEAN is the caller's business — this seam owns the allocation, never
    /// the layout, which is what keeps one platform's IO structures out of the shared runtime.
    /// </remarks>
    nuint StageBytes(int byteCount);
}
