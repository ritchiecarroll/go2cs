using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

[TestClass]
public class GoAsyncIOTests
{
    // GoAsyncIO is the rendezvous between a package that SUBMITS asynchronous OS operations and one
    // that WAITS for them, where the reference graph forbids the callback from calling back
    // (docs/phase4/DESIGN-netpoll-managed-poller.md §4.4). Its corpus consumers are the netpoll
    // hand-owns — internal/poll registers a readiness sink per descriptor; syscall's overlapped
    // wrappers own the per-descriptor association and the per-operation records; and
    // internal/syscall/windows reads one property back, the operation's native address.
    //
    // Two of its requirements are NOT satisfied by the obvious implementation and were measured
    // rather than reasoned: create-EXACTLY-once (a bare ConcurrentDictionary.GetOrAdd runs its
    // factory on several threads and keeps one result, which for these consumers means native
    // allocations with no owner and, worse, a socket associated with a completion port twice), and
    // sink REPLACEMENT (the kernel reissues descriptor numbers after a close, so a registration that
    // refused to overwrite would wake a retired waiter). Both get a test below, and the first is a
    // real contention test rather than an assertion about intent.

    // Descriptors are per-test so parallel test execution cannot collide on the shared tables.
    private static int s_next = 0x5000;

    private static nuint nextDescriptor() => (nuint)Interlocked.Add(ref s_next, 1);

    private sealed class disposableState : IDisposable
    {
        internal int Disposals;

        public void Dispose() => Interlocked.Increment(ref Disposals);
    }

    private sealed class addressedState : IGoAsyncOperation
    {
        public nuint NativeAddress { get; init; }

        // §4.7 widened this interface with the submit side. This double models a HARVEST-only
        // participant, so both members throw: that is the honest shape, and RearmOperation's own
        // guard test relies on a state object that genuinely cannot submit.
        public nuint RearmForSubmit() => throw new NotSupportedException("harvest-only test double");

        public nuint StageBytes(int byteCount) => throw new NotSupportedException("harvest-only test double");
    }

    [TestMethod]
    public void SignalReachesTheRegisteredSink()
    {
        nuint descriptor = nextDescriptor();
        List<nint> modes = new();

        GoAsyncIO.SetReadiness(descriptor, mode => { lock (modes) modes.Add(mode); });

        Assert.IsTrue(GoAsyncIO.Signal(descriptor, 'r'));
        Assert.IsTrue(GoAsyncIO.Signal(descriptor, 'w'));

        CollectionAssert.AreEqual(new List<nint> { 'r', 'w' }, modes);

        GoAsyncIO.RemoveDescriptor(descriptor);
    }

    [TestMethod]
    public void SignalForAnUnregisteredDescriptorIsASilentNoOp()
    {
        // Not politeness: Signal runs on an IO completion callback, where an escaping exception ends
        // the process, and a completion racing its own descriptor's close is a race the poller
        // contract permits.
        Assert.IsFalse(GoAsyncIO.Signal(nextDescriptor(), 'r'));
    }

    [TestMethod]
    public void ReadinessSinkIsReplaceableAndRemovable()
    {
        nuint descriptor = nextDescriptor();
        int first = 0, second = 0;

        GoAsyncIO.SetReadiness(descriptor, _ => first++);
        GoAsyncIO.Signal(descriptor, 'r');

        // REPLACEMENT is the load-bearing case: the kernel reissues descriptor numbers, so the
        // second registration must displace the first rather than being refused or queued behind it.
        GoAsyncIO.SetReadiness(descriptor, _ => second++);
        GoAsyncIO.Signal(descriptor, 'r');

        Assert.AreEqual(1, first);
        Assert.AreEqual(1, second);

        GoAsyncIO.SetReadiness(descriptor, null);
        Assert.IsFalse(GoAsyncIO.Signal(descriptor, 'r'));
        Assert.AreEqual(1, second);
    }

    [TestMethod]
    public void HasReadinessFollowsTheSinkFromOpenToClose()
    {
        // syscall's readFile/writeFile take the completion seam only for a descriptor the poller owns,
        // and this is how they ask. It must follow the SINK, not the descriptor's other state: a
        // disassociated descriptor is still the poller's until pollClose retires it.
        nuint descriptor = nextDescriptor();

        Assert.IsFalse(GoAsyncIO.HasReadiness(descriptor), "a descriptor nobody registered");

        GoAsyncIO.SetReadiness(descriptor, _ => { });
        Assert.IsTrue(GoAsyncIO.HasReadiness(descriptor), "registered by pollOpen");

        GoAsyncIO.MarkUnassociated(descriptor);
        Assert.IsTrue(GoAsyncIO.HasReadiness(descriptor), "disassociated, still the poller's");

        GoAsyncIO.RemoveDescriptor(descriptor);
        Assert.IsFalse(GoAsyncIO.HasReadiness(descriptor), "retired by pollClose");
    }

    [TestMethod]
    public void DescriptorStateIsCreatedExactlyOnceUnderContention()
    {
        // THE MEASURED REQUIREMENT. A bare GetOrAdd builds one object per racing thread and keeps
        // one; here that object is a completion-port association, and associating one socket twice
        // is a kernel error. This asserts the FACTORY ran once, not merely that one result was
        // handed back.
        nuint descriptor = nextDescriptor();
        int built = 0;
        object[] results = new object[32];

        using Barrier gate = new(results.Length);

        Parallel.For(0, results.Length, i => {
            gate.SignalAndWait();
            results[i] = GoAsyncIO.GetOrCreateDescriptorState(descriptor, () => {
                Interlocked.Increment(ref built);
                return new object();
            });
        });

        Assert.AreEqual(1, built);

        foreach (object result in results)
            Assert.AreSame(results[0], result);

        GoAsyncIO.RemoveDescriptor(descriptor);
    }

    [TestMethod]
    public void OperationStateIsCreatedExactlyOnceUnderContention()
    {
        // Same requirement one level down, and the one the prototype actually caught: a contention
        // run built TEN operation records where the contract wants one, each owning a preallocated
        // overlapped and native staging buffers — nine native leaks with no owner.
        object key = new();
        int built = 0;
        object[] results = new object[32];

        using Barrier gate = new(results.Length);

        Parallel.For(0, results.Length, i => {
            gate.SignalAndWait();
            results[i] = GoAsyncIO.GetOrCreateOperationState(key, () => {
                Interlocked.Increment(ref built);
                return new object();
            });
        });

        Assert.AreEqual(1, built);

        foreach (object result in results)
            Assert.AreSame(results[0], result);

        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void OperationStateIsKeyedByThePointersOwnEquality()
    {
        // The corpus keys operations by a ж<T> field reference, whose equality is (resolved source
        // pointer, field identity) rather than object identity — that is what makes three separate
        // mints of `&o.o` at three call sites resolve to ONE record. The table must therefore use the
        // KEY's equality, never ReferenceEquals.
        ж<pairOfInts> box = new StandardBox<pairOfInts>(new pairOfInts());
        object first = GoAsyncIO.GetOrCreateOperationState(box.of(pairOfInts.Ꮡsecond), () => new object());
        object again = GoAsyncIO.GetOrCreateOperationState(box.of(pairOfInts.Ꮡsecond), () => new object());

        Assert.AreSame(first, again);

        // A DIFFERENT field of the same source is a different operation.
        object other = GoAsyncIO.GetOrCreateOperationState(box.of(pairOfInts.Ꮡfirst), () => new object());

        Assert.AreNotSame(first, other);

        GoAsyncIO.RemoveOperationState(box.of(pairOfInts.Ꮡsecond));
        GoAsyncIO.RemoveOperationState(box.of(pairOfInts.Ꮡfirst));
    }

    // A stand-in for the corpus's `operation` struct: two fields, so a field reference to each has
    // the same SOURCE and a different field identity.
    private struct pairOfInts
    {
        internal nint first;
        internal nint second;

        internal static ref nint Ꮡfirst(ref pairOfInts instance) => ref instance.first;
        internal static ref nint Ꮡsecond(ref pairOfInts instance) => ref instance.second;
    }

    [TestMethod]
    public void OperationAddressIsReportedOnlyByAnAddressedState()
    {
        object addressed = new();
        object plain = new();

        GoAsyncIO.GetOrCreateOperationState(addressed, () => new addressedState { NativeAddress = 0xDEAD });
        GoAsyncIO.GetOrCreateOperationState(plain, () => new object());

        // This one property is the ENTIRE contract between the package that issues an operation and
        // the package that harvests its result.
        Assert.IsTrue(GoAsyncIO.TryGetOperationAddress(addressed, out nuint address));
        Assert.AreEqual((nuint)0xDEAD, address);

        Assert.IsFalse(GoAsyncIO.TryGetOperationAddress(plain, out _));
        Assert.IsFalse(GoAsyncIO.TryGetOperationAddress(new object(), out _));

        GoAsyncIO.RemoveOperationState(addressed);
        GoAsyncIO.RemoveOperationState(plain);
    }

    [TestMethod]
    public void RemoveDescriptorRetiresBothTheSinkAndTheState()
    {
        nuint descriptor = nextDescriptor();
        disposableState state = new();
        int signals = 0;

        GoAsyncIO.SetReadiness(descriptor, _ => signals++);
        GoAsyncIO.GetOrCreateDescriptorState(descriptor, () => state);

        Assert.IsTrue(GoAsyncIO.TryGetDescriptorState(descriptor, out object? found));
        Assert.AreSame(state, found);

        // The waiter calls this from pollClose, BEFORE the descriptor itself is closed, so the
        // submitting package can still legally unregister from whatever it associated the descriptor
        // with. Disposal of the state is part of that contract.
        GoAsyncIO.RemoveDescriptor(descriptor);

        Assert.AreEqual(1, state.Disposals);
        Assert.IsFalse(GoAsyncIO.TryGetDescriptorState(descriptor, out _));
        Assert.IsFalse(GoAsyncIO.Signal(descriptor, 'r'));
        Assert.AreEqual(0, signals);
    }

    [TestMethod]
    public void RemoveDescriptorDoesNotBuildStateItIsAboutToDiscard()
    {
        // A descriptor that never submitted has no state to retire, and retiring it must not
        // MATERIALIZE one — every pollable FD reaches pollClose, including the ones that only ever
        // listened.
        nuint descriptor = nextDescriptor();

        GoAsyncIO.RemoveDescriptor(descriptor);

        Assert.IsFalse(GoAsyncIO.TryGetDescriptorState(descriptor, out _));
    }

    // ---- the SUBMIT side (netpoll design §4.7, RATIFIED 2026-08-23) ------------------------------
    //
    // The harvest primitives above needed one property, an address, because the operation already
    // existed. A submit may be an operation's FIRST touch, so the seam gained a registered factory
    // plus two interface members. These guard the properties that make it safe to have TWO packages
    // sharing one record store: exactly one record per waiter, staging that belongs to the record,
    // and loud failure when the contract is used out of order.
    //
    // §4.7 ⟨OQ-D⟩ ruled these tests land BEFORE the wrappers that consume them, because the S2b
    // prototype's own leak (10 records where the contract wants 1, each owning native resources)
    // was found this way and the failure mode here is identical.

    private sealed class FakeOperation : IGoAsyncOperation
    {
        internal int RearmCount;
        internal int LastStageRequest = -1;
        internal static int Created;

        public nuint NativeAddress => 0x5000;

        public nuint RearmForSubmit()
        {
            RearmCount++;
            return 0x6000;
        }

        public nuint StageBytes(int byteCount)
        {
            LastStageRequest = byteCount;
            return 0x7000;
        }
    }

    private static void useFakeFactory()
    {
        // NOT SetOperationFactory: this test assembly references core/syscall, whose module
        // initializer claims the factory at ASSEMBLY LOAD, so the public registration always loses
        // here no matter how the tests are ordered. The internal swap is the seam's test hook (golib
        // already grants GolibTests its internals); it also clears the record store, which is what
        // makes each test below independent of the ones before it.
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_fakeFactory);
    }

    private static readonly Func<nuint, object, nint, object> s_fakeFactory = (_, _, _) => {
        FakeOperation.Created++;
        return new FakeOperation();
    };

    [TestMethod]
    public void RearmOperation_CreatesOnceThenRearmsTheSameRecord()
    {
        useFakeFactory();
        object key = new();
        int before = FakeOperation.Created;

        nuint first = GoAsyncIO.RearmOperation(7, key, 'w');
        nuint second = GoAsyncIO.RearmOperation(7, key, 'w');

        Assert.AreEqual((nuint)0x6000, first, "rearm must report the native control block");
        Assert.AreEqual((nuint)0x6000, second);
        Assert.AreEqual(before + 1, FakeOperation.Created, "a second submit on one waiter must REUSE its record");

        Assert.IsTrue(GoAsyncIO.TryGetOperationState(key, out object? state));
        Assert.AreEqual(2, ((FakeOperation)state!).RearmCount, "each submit re-arms");

        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void RearmOperation_UnderContention_CreatesExactlyOneRecord()
    {
        // The S2b leak, pinned: a bare GetOrAdd runs its factory more than once under contention and
        // every discarded record here owns native resources with no owner to free them.
        useFakeFactory();
        object key = new();
        int before = FakeOperation.Created;

        System.Threading.Tasks.Parallel.For(0, 32, _ => GoAsyncIO.RearmOperation(9, key, 'r'));

        Assert.AreEqual(before + 1, FakeOperation.Created, "exactly one record per waiter, under contention");
        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void StageOperationBuffer_ReachesTheRecordAndCarriesTheByteCount()
    {
        useFakeFactory();
        object key = new();
        GoAsyncIO.RearmOperation(3, key, 'w');

        nuint staged = GoAsyncIO.StageOperationBuffer(key, 64);

        Assert.AreEqual((nuint)0x7000, staged);
        Assert.IsTrue(GoAsyncIO.TryGetOperationState(key, out object? state));
        Assert.AreEqual(64, ((FakeOperation)state!).LastStageRequest, "the byte count crosses the seam verbatim");

        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void StageOperationBuffer_WithoutARecord_ThrowsRatherThanReturningZero()
    {
        // Staging against an operation that was never re-armed is a contract violation, and the seam
        // must say so: a 0 address would be handed to the OS as a buffer pointer.
        useFakeFactory();

        Assert.ThrowsException<InvalidOperationException>(() => GoAsyncIO.StageOperationBuffer(new object(), 16));
    }

    [TestMethod]
    public void SetOperationFactory_RejectsADifferentSecondFactory()
    {
        // Two factories would mean two owners for in-flight operations; the seam refuses rather than
        // silently changing which package owns them. The SAME delegate re-registers freely.
        useFakeFactory();

        // The public entry still accepts a repeat registration of the SAME delegate...
        GoAsyncIO.SetOperationFactory(s_fakeFactory);

        // ...and still refuses a different one, which is the property that matters.
        Assert.ThrowsException<InvalidOperationException>(
            () => GoAsyncIO.SetOperationFactory((_, _, _) => new FakeOperation()));
    }

    [TestMethod]
    public void RearmOperation_RejectsStateThatDoesNotSupportSubmission()
    {
        // A record from a package that only ever harvested does not implement the submit side; the
        // seam must not silently answer 0.
        object key = new();
        GoAsyncIO.GetOrCreateOperationState(key, () => new object());

        Assert.ThrowsException<InvalidOperationException>(() => GoAsyncIO.RearmOperation(1, key, 'w'));
        GoAsyncIO.RemoveOperationState(key);
    }

    // ---- the DECODE seam (netpoll design §4.8) ----------------------------------------------
    //
    // These gate the two primitives the receive direction needs. The leak/duplicate modes here are
    // the same ones S2b found for the submit side, which is why they are tested before any wrapper
    // consumes them.

    [TestMethod]
    public void SetOperationCompletion_RunsOnceAtCompletion_WithTheTransferCount()
    {
        object key = new();
        nint seen = -1;
        int runs = 0;

        GoAsyncIO.SetOperationCompletion(key, n => { seen = n; runs++; });

        Assert.IsTrue(GoAsyncIO.CompleteOperation(key, 42), "pending work should report that it ran");
        Assert.AreEqual(1, runs);
        Assert.AreEqual((nint)42, seen, "the completion must receive the transfer count, not a default");
    }

    [TestMethod]
    public void CompleteOperation_RunsAtMostOnce()
    {
        // execIO can genuinely harvest the SAME operation twice -- its cancellation path harvests
        // what its normal path would have -- so decoding twice is a real risk, not a theoretical
        // one. The delegate is removed BEFORE it is invoked, which makes once-only structural.
        object key = new();
        int runs = 0;

        GoAsyncIO.SetOperationCompletion(key, _ => runs++);

        Assert.IsTrue(GoAsyncIO.CompleteOperation(key, 8));
        Assert.IsFalse(GoAsyncIO.CompleteOperation(key, 8), "a second harvest must find nothing pending");
        Assert.AreEqual(1, runs);
    }

    [TestMethod]
    public void CompleteOperation_WithNothingPending_IsNotAnError()
    {
        // The COMMON case: every send and every TCP read completes with no decode owed. The harvest
        // calls this unconditionally, so it must be cheap and silent rather than exceptional.
        Assert.IsFalse(GoAsyncIO.CompleteOperation(new object(), 0));
    }

    [TestMethod]
    public void RemoveOperationState_DropsPendingCompletionWork()
    {
        // An operation torn down before it completes owes nothing. Leaving the delegate behind would
        // leak what it captured AND let a later operation reusing the key inherit a decode meant for
        // the dead one -- which on the real seam would write a stale address into a live socket.
        object key = new();
        int runs = 0;

        GoAsyncIO.GetOrCreateOperationState(key, () => new object());
        GoAsyncIO.SetOperationCompletion(key, _ => runs++);
        GoAsyncIO.RemoveOperationState(key);

        Assert.IsFalse(GoAsyncIO.CompleteOperation(key, 1), "torn-down operations owe nothing");
        Assert.AreEqual(0, runs);
    }

    [TestMethod]
    public void SetOperationCompletion_SupersedesAnUnconsumedRegistration()
    {
        // Each SUBMISSION registers its own decode against a record that outlives many submissions.
        // A new submission's decode must win, or a re-armed operation would decode into the previous
        // submission's destination.
        object key = new();
        int first = 0, second = 0;

        GoAsyncIO.SetOperationCompletion(key, _ => first++);
        GoAsyncIO.SetOperationCompletion(key, _ => second++);

        Assert.IsTrue(GoAsyncIO.CompleteOperation(key, 3));
        Assert.AreEqual(0, first, "the superseded decode must not run");
        Assert.AreEqual(1, second);
    }

    [TestMethod]
    public void CompletionSeam_RejectsNullArguments()
    {
        Assert.ThrowsException<ArgumentNullException>(() => GoAsyncIO.SetOperationCompletion(null!, _ => { }));
        Assert.ThrowsException<ArgumentNullException>(() => GoAsyncIO.SetOperationCompletion(new object(), null!));
        Assert.ThrowsException<ArgumentNullException>(() => GoAsyncIO.CompleteOperation(null!, 0));
    }

    [TestMethod]
    public void CompletionsAreIndependentPerOperation()
    {
        // Two FDs in flight at once is the normal state of a server; completing one must not consume
        // the other's decode.
        object a = new(), b = new();
        int ran = 0;

        GoAsyncIO.SetOperationCompletion(a, _ => ran += 1);
        GoAsyncIO.SetOperationCompletion(b, _ => ran += 10);

        GoAsyncIO.CompleteOperation(a, 0);
        Assert.AreEqual(1, ran, "completing a must not run b's work");

        GoAsyncIO.CompleteOperation(b, 0);
        Assert.AreEqual(11, ran);
    }

    // ---- Pooled operations (Go 1.26) ---------------------------------------------------------
    //
    // Go 1.26's internal/poll takes every overlapped operation from a process-wide sync.Pool
    // (fd_windows.go's operationPool) instead of one read and one write operation per FD, so ONE key
    // reaches RearmOperation for several descriptors and for both modes. A record is bound to the
    // descriptor and mode it was created for (its completion-port association and the mode its
    // completion signals), so a key reused for another pair must get a NEW record, and the old one
    // must be retired. Through Go 1.24 the key never changed binding, which is why the table was keyed
    // by the operation alone.

    private sealed class BoundOperation(nuint descriptor, nint mode) : IGoAsyncOperation, IDisposable
    {
        internal readonly nuint Descriptor = descriptor;
        internal readonly nint Mode = mode;
        internal int Disposals;

        public nuint NativeAddress => 0x5000;

        public nuint RearmForSubmit() => 0x6000;

        public nuint StageBytes(int byteCount) => 0x7000;

        public void Dispose() => Disposals++;
    }

    private static readonly Func<nuint, object, nint, object> s_boundFactory =
        (descriptor, _, mode) => new BoundOperation(descriptor, mode);

    private static BoundOperation boundStateOf(object key)
    {
        Assert.IsTrue(GoAsyncIO.TryGetOperationState(key, out object? state));
        return (BoundOperation)state!;
    }

    [TestMethod]
    public void RearmOperation_APooledKeyReusedForAnotherDescriptor_GetsANewRecord()
    {
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_boundFactory);
        object key = new();

        GoAsyncIO.RearmOperation(11, key, 'r');
        BoundOperation first = boundStateOf(key);

        GoAsyncIO.RearmOperation(12, key, 'r');
        BoundOperation second = boundStateOf(key);

        Assert.AreNotSame(first, second, "a pooled operation submitted on another descriptor must not reuse the first descriptor's record");
        Assert.AreEqual((nuint)12, second.Descriptor);
        Assert.AreEqual(1, first.Disposals, "the retired record is disposed");

        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void RearmOperation_APooledKeyReusedForTheOtherMode_GetsANewRecord()
    {
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_boundFactory);
        object key = new();

        GoAsyncIO.RearmOperation(13, key, 'r');
        BoundOperation read = boundStateOf(key);

        GoAsyncIO.RearmOperation(13, key, 'w');
        BoundOperation write = boundStateOf(key);

        Assert.AreNotSame(read, write, "a write must not signal through a record bound to 'r'");
        Assert.AreEqual((nint)'w', write.Mode);
        Assert.AreEqual(1, read.Disposals);

        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void RearmOperation_Rebinding_DropsTheRetiredRecordsPendingCompletion()
    {
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_boundFactory);
        object key = new();

        GoAsyncIO.RearmOperation(14, key, 'r');
        GoAsyncIO.SetOperationCompletion(key, _ => Assert.Fail("a decode registered for the retired record ran"));

        GoAsyncIO.RearmOperation(15, key, 'r');

        Assert.IsFalse(GoAsyncIO.CompleteOperation(key, 0));
        GoAsyncIO.RemoveOperationState(key);
    }

    [TestMethod]
    public void RemoveOperationState_WithAState_RetiresOnlyThatState()
    {
        // A descriptor's teardown removes the records IT created. After a re-bind the key names
        // another descriptor's live record, which a removal by key alone would retire.
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_boundFactory);
        object key = new();

        GoAsyncIO.RearmOperation(16, key, 'r');
        BoundOperation first = boundStateOf(key);

        GoAsyncIO.RearmOperation(17, key, 'r');
        BoundOperation second = boundStateOf(key);

        Assert.IsFalse(GoAsyncIO.RemoveOperationState(key, first), "the retired record no longer owns the key");
        Assert.AreSame(second, boundStateOf(key));

        Assert.IsTrue(GoAsyncIO.RemoveOperationState(key, second));
        Assert.IsFalse(GoAsyncIO.TryGetOperationState(key, out _));
    }

    // ---- Disassociated descriptors -----------------------------------------------------------
    //
    // net's dup() and os's File.Fd() take a socket off the completion port (internal/poll's
    // DisassociateIOCP), after which Go's waitIO waits on each operation's own event. The submit seam
    // binds a socket to a port LAZILY, at its first overlapped submit, so the poller's hand-own MARKS
    // the descriptor instead, and a record built before the mark must not serve a submit after it: its
    // overlapped would carry no event and complete to a port nobody waits on.

    private sealed class AssociationRecord(bool unassociated) : IGoAsyncOperation, IDisposable
    {
        internal readonly bool Unassociated = unassociated;
        internal int Disposals;

        public nuint NativeAddress => 0x5000;

        public nuint RearmForSubmit() => 0x6000;

        public nuint StageBytes(int byteCount) => 0x7000;

        public void Dispose() => Disposals++;
    }

    // The factory reads the mark exactly as the seam's does, at creation.
    private static readonly Func<nuint, object, nint, object> s_associationFactory =
        (descriptor, _, _) => new AssociationRecord(GoAsyncIO.IsUnassociated(descriptor));

    private static AssociationRecord associationStateOf(object key)
    {
        Assert.IsTrue(GoAsyncIO.TryGetOperationState(key, out object? state));
        return (AssociationRecord)state!;
    }

    [TestMethod]
    public void MarkUnassociated_RetiresTheRecordBuiltBeforeIt()
    {
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_associationFactory);
        nuint descriptor = nextDescriptor();
        object key = new();

        GoAsyncIO.RearmOperation(descriptor, key, 'r');
        AssociationRecord bound = associationStateOf(key);
        Assert.IsFalse(bound.Unassociated);

        GoAsyncIO.MarkUnassociated(descriptor);
        GoAsyncIO.RearmOperation(descriptor, key, 'r');
        AssociationRecord unbound = associationStateOf(key);

        Assert.AreNotSame(bound, unbound, "a submit after the mark must not reuse the record built for the port");
        Assert.IsTrue(unbound.Unassociated, "the new record is built under the mark");
        Assert.AreEqual(1, bound.Disposals, "the retired record is disposed");

        GoAsyncIO.RemoveOperationState(key);
        GoAsyncIO.RemoveDescriptor(descriptor);
    }

    [TestMethod]
    public void MarkUnassociated_KeepsTheRecordBuiltUnderIt()
    {
        GoAsyncIO.ReplaceOperationFactoryForTesting(s_associationFactory);
        nuint descriptor = nextDescriptor();
        object key = new();

        GoAsyncIO.MarkUnassociated(descriptor);
        GoAsyncIO.RearmOperation(descriptor, key, 'w');
        AssociationRecord first = associationStateOf(key);

        GoAsyncIO.RearmOperation(descriptor, key, 'w');

        Assert.AreSame(first, associationStateOf(key), "the mark does not churn records built under it");
        Assert.IsTrue(first.Unassociated);
        Assert.AreEqual(0, first.Disposals);

        GoAsyncIO.RemoveOperationState(key);
        GoAsyncIO.RemoveDescriptor(descriptor);
    }

    [TestMethod]
    public void MarkUnassociated_IsClearedWhenTheDescriptorIsRetiredOrRegisteredAgain()
    {
        // The kernel reissues descriptor numbers, so a mark must not outlive the socket it was made for.
        nuint retired = nextDescriptor();
        GoAsyncIO.MarkUnassociated(retired);
        Assert.IsTrue(GoAsyncIO.IsUnassociated(retired));

        GoAsyncIO.RemoveDescriptor(retired);
        Assert.IsFalse(GoAsyncIO.IsUnassociated(retired), "RemoveDescriptor retires the mark");

        nuint reused = nextDescriptor();
        GoAsyncIO.MarkUnassociated(reused);
        GoAsyncIO.SetReadiness(reused, null);
        Assert.IsTrue(GoAsyncIO.IsUnassociated(reused), "unregistering a sink is not a new socket");

        GoAsyncIO.SetReadiness(reused, _ => { });
        Assert.IsFalse(GoAsyncIO.IsUnassociated(reused), "a new registration is a new socket");

        GoAsyncIO.RemoveDescriptor(reused);
    }
}
