// zsyscall_windows_wsa_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the ws2_32 surface go2cs cannot convert literally. The bulk of it
// is the OVERLAPPED (asynchronous) half -- the SUBMIT SEAM of the managed netpoller arc. Design and
// rulings: docs/phase4/DESIGN-netpoll-managed-poller.md §4.3/§4.4/§4.5 (§9 RULED 2026-08-13; the
// accept handover RULED 2026-08-14).
//
// TWO SYNCHRONOUS MEMBERS LIVE HERE TOO, and the file is the ws2_32 family rather than the async one
// because of them. LoadConnectEx (below) is the dial path's extension-pointer lookup. WSAStartup and
// WSAEnumProtocols are the INITIALISATION pair, added 2026-08-16 -- see "The INITIALISATION pair" at
// the foot of this file for why they are here and not in zsyscall_windows_impl.cs, which is the same
// struct-passing class over kernel32.
//
// WHAT THE POLLER ALONE COULD NOT DO. internal/poll's ten runtime_poll* contracts are hand-owned in
// internal/poll/windows/runtime_netpoll_impl.cs, and they make a completion WAKE a blocked goroutine.
// That is half an arc. The other half is that the overlapped submissions execIO issues must actually
// reach the kernel and complete into memory the managed side can read -- and today they cannot, for
// three separate reasons that this file answers one at a time.
//
// (1) THE NATIVE-LAYOUT WALL, the established class with new members. syscall.WSABuf is
// `{uint32 Len; ж<byte> Buf}` -- a MANAGED REFERENCE where native WSABUF wants a raw CHAR* -- so the
// address of a WSABuf is not a native WSABUF. Same class as Timezoneinformation / win32finddata1 /
// RawSockaddrInet4, same remedy: a blittable [StructLayout(Sequential)] mirror with a field-for-field
// copy at the boundary. AcceptEx's output buffer is the class's DECODE side: net hands it a
// slice<RawSockaddrAny> reinterpreted as bytes, whose managed layout cannot hold the native sockaddr
// block at all, so the mirror there is a NATIVE STAGING buffer transcribed back at parse time.
//
// (2) THE LIFETIME WALL, which is what ASYNC adds to that class. The established mirror is "a LOCAL
// at the call site, trivially stable for exactly that long" (syscall_windows_impl.cs). An overlapped
// operation breaks that premise twice over. The kernel retains the OVERLAPPED and the buffer pointers
// until COMPLETION -- unbounded -- and `&o.o` is an interior field address inside a reference-bearing
// container, which golib's address model explicitly CANNOT hold still: EnsureStableAddress pins
// PinnableStorage, which for a struct-field reference recurses to the container's storage, and the
// ж<operation> box's slot is null because `operation` holds managed references (ж<FD>, WSABuf.Buf,
// slice<WSABuf>, ж<RawSockaddrAny>). So the address the generated wrapper would hand the kernel is
// transient by construction -- the pipe-EOF defect (ж.cs's war story) with an unbounded window.
// Second, the OVERLAPPED is the operation's kernel-side IDENTITY: execIO names the same `&o.o` at
// THREE call sites -- submit, CancelIoEx, WSAGetOverlappedResult -- and cancellation matches BY
// ADDRESS, so any scheme minting a fresh native copy per call breaks cancellation outright.
//
// THE REMEDY IS A PER-OPERATION RECORD (OverlappedOp below) owning native-lifetime state, keyed by
// the ж<Overlapped> the waiter names the operation by. That key works, and it was verified rather
// than assumed: ж<T>.Equals for a struct-field reference compares (resolved source pointer, field
// identity token) and GetHashCode agrees with it, so all three of execIO's `Ꮡo.of(operation.Ꮡo)`
// mints resolve to ONE record -- and so do the mints of two SEPARATE FD.Read calls, because
// SameSource resolves an `of()` chain recursively rather than by object reference. ж.cs says this
// property exists to serve "the address-keyed runtime semaphores in the hand-owned
// sync/internal-poll implementations", so this arc reuses a guarantee the corpus already depends on.
// The one bound worth carrying: equality is SOURCE-POINTER identity, so a future call site that
// re-BOXED the operation (a standard heap box rather than `Ꮡfd.of(FD.Ꮡrop)`) would silently mint a
// second record.
//
// (3) THE REFERENCE GRAPH, which is why the wake-up goes through golib. internal/poll REFERENCES
// syscall, so the completion callback here cannot call back into the poller. golib is the one
// assembly both sides see, and GoAsyncIO is the descriptor-keyed rendezvous: the poller registers a
// readiness sink for its descriptor at pollOpen, and the callback below signals it knowing only the
// descriptor and the mode -- which is all a completion callback CAN know. MODE is known from the
// WRAPPER, not from the operation: WSARecv/AcceptEx are always the read operation and
// WSASend/ConnectEx always the write one, because each FD has exactly one rop and one wop for its
// lifetime.
//
// WHERE THE CLR ASSOCIATION IS MADE, and why it is LAZY. Binding happens at the FIRST SUBMIT, not at
// pollOpen (design §4.2 as amended at S2). Go's poller REJECTS completions it does not own --
// pollOperationFromOverlappedEntry compares the completion key against the pollDesc in the operation
// and drops a mismatch (go.dev/issue/58870) -- while ThreadPoolBoundHandle has no equivalent: its
// callback resolves state FROM the NativeOverlapped it allocated, so a FOREIGN overlapped arriving at
// that port is MISREAD rather than ignored. internal/poll has a live producer of one (FD.WSAIoctl
// passes the CALLER's syscall.Overlapped, which being all-scalar in a standard box really does pin
// and really does reach the kernel). Binding lazily removes the hazard structurally: a socket that
// only ever sees foreign overlapped IO is never associated at all.
//
// THE OVERLAPPED IS FREED AT THE NEXT SUBMIT, NOT IN THE CALLBACK, and that is deliberate. execIO
// harvests with WSAGetOverlappedResult AFTER its wait returns, and that call reads Internal /
// InternalHigh out of the very control block the kernel wrote -- so the block must outlive the
// completion callback. Freeing at the next Rearm (and at teardown) also covers the skipSyncNotif
// path, where a synchronously-successful submit posts NO completion packet and no callback ever runs.
//
// WHAT IS NOT HERE. This list is now down to WSARecvMsg and WSASendMsg, and it has shrunk one
// member at a time by the board's do-it-when-a-suite-reaches-it ruling: WSARecvFrom joined
// 2026-08-23 (UdpLoopbackRoundTrip's read), TransmitFile 2026-08-28 (net's sendfile family), and
// WSASendto 2026-09-06 -- the last of those by a DEPARTURE from that ruling, stated at its own
// declaration below, because no suite CAN reach it on Windows and the acceptance had to be a guard
// written for it. The internal/syscall/windows WSASendtoInet4/6 are hand-owned in that package,
// where their linkname declarations live. WSAGetOverlappedResult lives in
// internal/syscall/windows and has its own companion file there; it needs exactly ONE property from
// this one -- the operation's native address -- which it reads through GoAsyncIO.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using GoAsyncIO = go.golib.GoAsyncIO;
using IGoAsyncOperation = go.golib.IGoAsyncOperation;
using Goroutine = go.golib.Goroutine;
using NativeMemory = System.Runtime.InteropServices.NativeMemory;

// Hand-owned (no zsyscall_windows_wsa_impl.go exists, so a reconvert never regenerates this file).
// The declarations it replaces are registered in the converter's manualConversionFuncs, which is what
// turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// Native mirrors, NativeOverlapped* and the staging buffers are all pointer work. Declared rather
// than inherited -- see zsyscall_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // ---- Modes -----------------------------------------------------------------------------------
    //
    // internal/poll names a mode with the RUNE ('r' / 'w'); the readiness sink speaks the same
    // vocabulary, since the poller is the only consumer.
    private const int wsaModeRead = 'r';
    private const int wsaModeWrite = 'w';

    // ---- Native mirrors --------------------------------------------------------------------------

    // WSABUF exactly as Windows lays it out: a length and a RAW pointer. The converted WSABuf holds
    // that pointer as a golib ж<byte>, which is a managed reference -- the whole of defect (1).
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWSABuf
    {
        public uint32 Len;
        public byte* Buf;
    }

    // ---- The per-descriptor association ----------------------------------------------------------

    // A SafeHandle that does NOT own its handle. ThreadPoolBoundHandle.BindHandle needs a SafeHandle,
    // but the socket belongs to internal/poll from open to close -- an owning wrapper would let the
    // finalizer close a descriptor the Go program still holds.
    private sealed class UnownedHandle : SafeHandle
    {
        internal UnownedHandle(nint value) : base(nint.Zero, ownsHandle: false) => SetHandle(value);

        public override bool IsInvalid => handle == nint.Zero || handle == -1;

        protected override bool ReleaseHandle() => true;
    }

    // Everything one socket needs to run overlapped IO through the CLR's completion port, created
    // EXACTLY once per descriptor (GoAsyncIO's Lazy-backed slot -- a bare GetOrAdd would let two
    // threads both bind, and binding one socket twice is a kernel error) and retired by the poller's
    // pollClose, which runs BEFORE the socket is closed precisely so this can unregister first.
    private sealed class SocketBinding : IDisposable
    {
        internal readonly nuint Descriptor;
        internal readonly ThreadPoolBoundHandle Bound;

        private readonly UnownedHandle m_handle;
        private readonly List<OverlappedOp> m_operations = new();
        private bool m_disposed;

        internal SocketBinding(nuint descriptor)
        {
            Descriptor = descriptor;
            m_handle = new UnownedHandle(unchecked((nint)descriptor));
            Bound = ThreadPoolBoundHandle.BindHandle(m_handle);
        }

        internal void Track(OverlappedOp operation)
        {
            lock (m_operations)
                m_operations.Add(operation);
        }

        public void Dispose()
        {
            List<OverlappedOp> operations;

            lock (m_operations)
            {
                if (m_disposed)
                    return;

                m_disposed = true;
                operations = new List<OverlappedOp>(m_operations);
                m_operations.Clear();
            }

            foreach (OverlappedOp operation in operations)
            {
                GoAsyncIO.RemoveOperationState(operation.Key);
                operation.Dispose();
            }

            Bound.Dispose();
            m_handle.Dispose();
        }
    }

    private static SocketBinding bindingFor(nuint descriptor) =>
        (SocketBinding)GoAsyncIO.GetOrCreateDescriptorState(descriptor, () => new SocketBinding(descriptor));

    // ---- The per-operation record ----------------------------------------------------------------

    // One record per (FD, mode) for the FD's lifetime, holding every piece of state the kernel is
    // allowed to see: the NativeOverlapped the operation is identified by, the native WSABUF array or
    // accept staging block, and -- crucially -- the ж<byte> BOXES whose pins hold the caller's own
    // buffers still. Holding the address would not be enough: the pin is a GCHandle owned by that one
    // box and released with it, so a later InitBuf dropping the last reference to a box the kernel is
    // still writing through would unpin memory mid-flight.
    private sealed unsafe class OverlappedOp : IGoAsyncOperation, IDisposable
    {
        internal readonly SocketBinding Binding;
        internal readonly nint Mode;

        // The key this record is filed under in golib's operation table. Kept so that retiring the
        // DESCRIPTOR retires the operation entries too -- the key is a ж<Overlapped> field reference,
        // which holds its source ж<FD> box alive, so leaving it behind would pin the whole FD graph
        // of every socket the program ever opened.
        internal readonly object Key;

        private readonly PreAllocatedOverlapped m_preallocated;
        private NativeOverlapped* m_native;

        // Native staging: the WSABUF array for send/recv, the AcceptEx output block for accept.
        private void* m_staging;
        private nuint m_stagingLength;

        // Boxes whose GCHandles pin the caller's buffers for the flight; replaced at each submit.
        private readonly List<object> m_pins = new();

        private bool m_disposed;

        internal OverlappedOp(SocketBinding binding, nint mode, object key)
        {
            Binding = binding;
            Mode = mode;
            Key = key;
            m_preallocated = new PreAllocatedOverlapped(completed, this, null);
        }

        // The address the kernel knows this operation by -- and the ENTIRE contract
        // internal/syscall/windows' WSAGetOverlappedResult needs from this package.
        public nuint NativeAddress => (nuint)m_native;

        // Frees the previous control block (see the file header on why not in the callback) and
        // allocates the one this submit will use. A PreAllocatedOverlapped may have only one
        // allocation outstanding, so free-then-allocate is required, not merely tidy.
        internal NativeOverlapped* Rearm()
        {
            releaseOverlapped();
            m_native = Binding.Bound.AllocateNativeOverlapped(m_preallocated);

            m_pins.Clear();

            return m_native;
        }

        internal void Pin(object box) => m_pins.Add(box);

        // ---- IGoAsyncOperation's SUBMIT side (netpoll design §4.7) -------------------------------
        // The same Rearm and Staging this package calls in-process, exposed through golib's neutral
        // interface so a submit from ANOTHER package uses ONE record store rather than a parallel
        // one. Nothing about Winsock crosses the seam: an address out, a byte count in.
        nuint golib.IGoAsyncOperation.RearmForSubmit() => (nuint)Rearm();

        nuint golib.IGoAsyncOperation.StageBytes(int byteCount) =>
            (nuint)Staging(byteCount <= 0 ? 1 : (nuint)byteCount);

        // Native staging of at least `length` bytes, reused across submits when it already fits.
        internal void* Staging(nuint length)
        {
            if (m_staging is not null && m_stagingLength >= length)
                return m_staging;

            if (m_staging is not null)
                NativeMemory.Free(m_staging);

            m_staging = NativeMemory.AllocZeroed(length);
            m_stagingLength = length;

            return m_staging;
        }

        internal nuint StagingLength => m_stagingLength;

        private void releaseOverlapped()
        {
            if (m_native is null)
                return;

            Binding.Bound.FreeNativeOverlapped(m_native);
            m_native = null;
        }

        public void Dispose()
        {
            if (m_disposed)
                return;

            m_disposed = true;

            releaseOverlapped();
            m_preallocated.Dispose();

            if (m_staging is not null)
            {
                NativeMemory.Free(m_staging);
                m_staging = null;
                m_stagingLength = 0;
            }

            m_pins.Clear();
        }
    }

    // The completion callback, run on a CLR IO thread-pool thread. It does exactly one thing: name
    // the descriptor and the mode to whoever registered a readiness sink for that descriptor. It
    // deliberately does NOT interpret errorCode -- that is a Win32 code, and execIO branches on WSA
    // codes (WSAECONNRESET where Win32 says ERROR_NETNAME_DELETED), so the harvest asks the OS
    // instead. It deliberately does NOT free the overlapped either (see the file header).
    private static unsafe void completed(uint32 errorCode, uint32 numBytes, NativeOverlapped* native)
    {
        if (ThreadPoolBoundHandle.GetNativeOverlappedState(native) is OverlappedOp operation)
            GoAsyncIO.Signal(operation.Binding.Descriptor, operation.Mode);
    }

    // Resolves (creating exactly once) the record for one operation. `Ꮡoverlapped` is the key: see
    // the file header on why it resolves across execIO's three call sites and across separate calls
    // on one FD.
    // The factory golib calls when a submit from ANOTHER package is the first touch of an operation
    // (netpoll design §4.7). Registered once, from this package, because only this package can build
    // the record -- it needs the socket's completion-port binding. operationFor now goes through the
    // SAME delegate, deliberately: two code paths that must agree about record identity are two code
    // paths that eventually will not.
    private static readonly Func<nuint, object, nint, object> s_operationFactory = (descriptor, key, mode) => {
        SocketBinding binding = bindingFor(descriptor);
        OverlappedOp created = new(binding, mode, (ж<Overlapped>)key);

        binding.Track(created);

        return created;
    };

    // Registered at ASSEMBLY LOAD, not on first use, and the difference is load-bearing: a program
    // that only ever sends DATAGRAMS never calls anything in this file, so a lazy registration
    // leaves internal/syscall/windows' submit with no factory and it fails with "no operation
    // factory registered" (measured -- the UDP guard's first run on this seam). A module
    // initializer is what the corpus already uses to run Go's package init(), so this needs no new
    // mechanism; registration is idempotent and touches nothing else.
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterAsyncOperationFactory() => GoAsyncIO.SetOperationFactory(s_operationFactory);

    private static OverlappedOp operationFor(ΔHandle handle, ж<Overlapped> Ꮡoverlapped, nint mode)
    {
        nuint descriptor = (nuint)(uintptr)handle;

        return (OverlappedOp)GoAsyncIO.GetOrCreateOperationState(Ꮡoverlapped,
            () => s_operationFactory(descriptor, Ꮡoverlapped, mode));
    }

    // ---- WSARecv / WSASend -----------------------------------------------------------------------

    // Copies the caller's WSABuf descriptors into the record's native WSABUF array, pinning each
    // buffer through the ж<byte> box the caller already holds (o.buf.Buf is `Ꮡ(buf, 0)`, an
    // element reference whose (void*) resolves through PinnableStorage to the slice's backing array
    // and pins it for the box's lifetime). Returns the native array's address.
    private static unsafe NativeWSABuf* stageBuffers(OverlappedOp operation, ж<WSABuf> Ꮡbufs, uint32 bufcnt) {
        nuint bytes = (nuint)bufcnt * (nuint)sizeof(NativeWSABuf);
        NativeWSABuf* native = (NativeWSABuf*)operation.Staging(bytes == 0 ? (nuint)sizeof(NativeWSABuf) : bytes);

        writeWSABufs(Ꮡbufs, bufcnt, native, operation);
        return native;
    }

    // The transcription half of stageBuffers, split out so the SYNCHRONOUS arm of WSASendto can
    // reuse it over a `stackalloc` array. `operation` is the record that must outlive the flight and
    // is NULL for that arm alone: a synchronous call cannot return before the kernel is done with
    // these pointers, so the caller keeping `Ꮡbufs` reachable across the call keeps every `ж<byte>`
    // it names -- and therefore every pin those boxes hold -- alive for exactly as long as they are
    // read. That is the same lifetime argument the sockaddr mirror makes for its stack buffers, and
    // it is why the record is not needed there; every OVERLAPPED caller passes one.
    private static unsafe void writeWSABufs(ж<WSABuf> Ꮡbufs, uint32 bufcnt, NativeWSABuf* native, OverlappedOp? operation) {
        for (uint32 i = 0; i < bufcnt; i++) {
            // Index 0 is the common case and the only one a struct-FIELD reference (`&o.buf`) can
            // answer; a multi-buffer submit always names a slice element (`&o.bufs[0]`), which
            // unsafe.Add steps through the backing array.
            ж<WSABuf> Ꮡbuf = i == 0 ? Ꮡbufs : unsafe_package.Add(Ꮡbufs, (uintptr)i);
            ref WSABuf buf = ref Ꮡbuf.Value;

            native[i].Len = buf.Len;

            if (buf.Buf == nil) {
                native[i].Buf = null;
            } else {
                native[i].Buf = (byte*)(void*)buf.Buf;
                operation?.Pin(buf.Buf);
            }
        }
    }

    public static unsafe error /*err*/ WSARecv(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡrecvd, ж<uint32> Ꮡflags, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeRead);
        NativeOverlapped* native = operation.Rearm();
        NativeWSABuf* buffers = stageBuffers(operation, Ꮡbufs, bufcnt);

        // Out-parameters marshal through call-local natives and copy back: they are written only
        // during the SYNCHRONOUS portion (the async result is harvested from the overlapped), so the
        // mirror-is-a-local doctrine holds for them even though it cannot hold for the overlapped.
        // `flags` is IN/OUT -- WSARecv reads it -- so it is seeded from the caller's value.
        uint32 recvd = 0;
        uint32 flags = Ꮡflags == nil ? 0 : Ꮡflags.Value;

        var (r1, _, e1) = Syscall9(procWSARecv.Addr(), 7, (uintptr)s, (uintptr)(void*)buffers, (uintptr)bufcnt, (uintptr)(void*)(&recvd), (uintptr)(void*)(&flags), (uintptr)(void*)native, 0, 0, 0);

        if (Ꮡrecvd != nil) {
            Ꮡrecvd.Value = recvd;
        }

        if (Ꮡflags != nil) {
            Ꮡflags.Value = flags;
        }

        return r1 == socket_error ? errnoErr(e1) : default!;
    }

    public static unsafe error /*err*/ WSASend(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent, uint32 flags, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeWrite);
        NativeOverlapped* native = operation.Rearm();
        NativeWSABuf* buffers = stageBuffers(operation, Ꮡbufs, bufcnt);

        uint32 sent = 0;

        var (r1, _, e1) = Syscall9(procWSASend.Addr(), 7, (uintptr)s, (uintptr)(void*)buffers, (uintptr)bufcnt, (uintptr)(void*)(&sent), (uintptr)flags, (uintptr)(void*)native, 0, 0, 0);

        if (Ꮡsent != nil) {
            Ꮡsent.Value = sent;
        }

        return r1 == socket_error ? errnoErr(e1) : default!;
    }

    // ---- AcceptEx and the accept HANDOVER --------------------------------------------------------

    // What AcceptEx staged, for GetAcceptExSockaddrs to parse. Parked in a GOROUTINE-keyed slot,
    // because GetAcceptExSockaddrs carries no handle and no overlapped -- see the ruling quoted on
    // the field below.
    private sealed class AcceptStaging
    {
        internal OverlappedOp Operation = null!;
        internal nuint Buffer;
        internal uint32 ReceiveLength;
        internal uint32 LocalLength;
        internal uint32 RemoteLength;
    }

    // ⚠ A COUPLING BETWEEN TWO WRAPPERS GO KEEPS INDEPENDENT, ruled deliberately (coordinator,
    // 2026-08-14; design §4.5) and documented here because nothing in the type system sees it.
    //
    // GetAcceptExSockaddrs' signature is (buf, rxdatalen, laddrlen, raddrlen, *lrsa, *lrsalen, *rrsa,
    // *rrsalen): no handle, no overlapped, no identity of any kind. Go needs none, because `buf`
    // really is the caller's [2]RawSockaddrAny and the kernel wrote native bytes into it. Under go2cs
    // neither half survives -- net passes `Ꮡ(rawsa, 0).Reinterpret<RawSockaddrAny, byte>()`, which is
    // an UNPINNED native-address box over a managed array whose layout is not the native one, so it
    // is unusable as data AND unusable as a table key (its identity is physical, so two evaluations
    // are equal only if the GC did not move the array in between).
    //
    // So AcceptEx parks its native staging block here and GetAcceptExSockaddrs consumes it. THE
    // PREMISE is goroutine affinity: the corpus's only caller is net.netFD.accept, which runs
    // FD.Accept and then GetAcceptExSockaddrs on ONE goroutine, with no other accept interleaved.
    // The key is the GOROUTINE, not a [ThreadStatic]: under the dedicated-thread executor those
    // coincide today, but keying on golib's scheduler-arc identity states the premise instead of
    // relying on the coincidence, and survives an executor that stops making it true. (Cross-arc
    // dependency, deliberately: the scheduler arc's Goroutine registry is what makes this handoff
    // sound.)
    //
    // SINGLE OCCUPANCY, CONSUME EXACTLY ONCE, FAIL BY NAME. A sequence Go permits but the corpus
    // never issues must be loud, never misread: parking over a DIFFERENT operation's park throws, and
    // consuming an empty slot throws. Re-parking the SAME operation is a replace, not a fault --
    // FD.Accept legitimately re-submits `fd.rop` after a WSAECONNRESET without an intervening parse,
    // and the park's content is identical anyway (same record, same staging block).
    //
    // Keys are held WEAKLY. A goroutine that accepted and then died without parsing -- an accept that
    // FAILED, since the success path always parses -- would otherwise leave its entry behind forever,
    // and "one goroutine per accept" is a shape real servers write.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, AcceptStaging> acceptHandoff = new();

    // The goroutine running this call, or the thread when there is none (a host that runs Go code
    // without entering a goroutine scope). Both are stable for the accept-then-parse sequence.
    private static object acceptHandoffKey() => (object?)Goroutine.Current ?? Thread.CurrentThread;

    public static unsafe error /*err*/ AcceptEx(ΔHandle ls, ΔHandle @as, ж<byte> Ꮡbuf, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<uint32> Ꮡrecvd, ж<Overlapped> Ꮡoverlapped) {
        // The listening socket owns the overlapped and is the descriptor the completion arrives on;
        // `as` is merely the socket the kernel fills in. Accept is internal/poll's READ operation.
        OverlappedOp operation = operationFor(ls, Ꮡoverlapped, wsaModeRead);
        NativeOverlapped* native = operation.Rearm();

        // The caller's `buf` cannot be used (see the coupling note above); the kernel writes into the
        // record's own native block, which GetAcceptExSockaddrs then parses.
        nuint length = (nuint)rxdatalen + laddrlen + raddrlen;
        void* staging = operation.Staging(length);

        object handoffKey = acceptHandoffKey();

        // The slot is goroutine-PRIVATE by construction, so the read-then-write below needs no lock:
        // only this goroutine can park into it or consume it.
        if (acceptHandoff.TryGetValue(handoffKey, out AcceptStaging? parked) && !ReferenceEquals(parked.Operation, operation)) {
            throw new InvalidOperationException(
                "syscall: AcceptEx staged an accept while another accept on this goroutine was still awaiting GetAcceptExSockaddrs");
        }

        acceptHandoff.AddOrUpdate(handoffKey, new AcceptStaging {
            Operation = operation,
            Buffer = (nuint)staging,
            ReceiveLength = rxdatalen,
            LocalLength = laddrlen,
            RemoteLength = raddrlen
        });

        uint32 recvd = 0;

        var (r1, _, e1) = Syscall9(procAcceptEx.Addr(), 8, (uintptr)ls, (uintptr)@as, (uintptr)staging, (uintptr)rxdatalen, (uintptr)laddrlen, (uintptr)raddrlen, (uintptr)(void*)(&recvd), (uintptr)(void*)native, 0);

        if (Ꮡrecvd != nil) {
            Ꮡrecvd.Value = recvd;
        }

        return r1 == 0 ? errnoErr(e1) : default!;
    }

    // Transcribes one native sockaddr into the MANAGED RawSockaddrAny image that
    // RawSockaddrAny.Sockaddr (syscall_windows_impl.cs) reads -- the exact inverse of the flattening
    // that method performs, and the reason the two are a pair. The Go declaration's own mapping:
    // Family at 0, Addr.Data covering bytes 2..15, Pad covering 16..115.
    private static unsafe ж<RawSockaddrAny> toRawSockaddrAny(byte* native, nint available) {
        var Ꮡany = @new<RawSockaddrAny>();
        fillRawSockaddrAny(Ꮡany, native, available);
        return Ꮡany;
    }

    // The transcription itself, against a CALLER-SUPPLIED box. WSARecvFrom fills the box internal/poll
    // already allocated and will read back from, so it cannot use the allocating shim above; accept
    // mints its own. One body either way, so the two paths cannot drift.
    private static unsafe void fillRawSockaddrAny(ж<RawSockaddrAny> Ꮡany, byte* native, nint available) {
        ref RawSockaddrAny any = ref Ꮡany.Value;

        if (available >= 2) {
            any.Addr.Family = *(uint16*)native;
        }

        for (nint i = 0; i < 14 && 2 + i < available; i++) {
            any.Addr.Data[i] = unchecked((int8)native[2 + i]);
        }

        for (nint i = 0; i < 100 && 16 + i < available; i++) {
            any.Pad[i] = unchecked((int8)native[16 + i]);
        }
    }

    public static unsafe void GetAcceptExSockaddrs(ж<byte> Ꮡbuf, uint32 rxdatalen, uint32 laddrlen, uint32 raddrlen, ж<ж<RawSockaddrAny>> Ꮡlrsa, ж<int32> Ꮡlrsalen, ж<ж<RawSockaddrAny>> Ꮡrrsa, ж<int32> Ꮡrrsalen) {
        object handoffKey = acceptHandoffKey();

        if (!acceptHandoff.TryGetValue(handoffKey, out AcceptStaging? staged)) {
            throw new InvalidOperationException(
                "syscall: GetAcceptExSockaddrs called with no AcceptEx staging parked on this goroutine");
        }

        acceptHandoff.Remove(handoffKey);

        byte* buffer = (byte*)staged.Buffer;
        nuint stagedLength = (nuint)staged.ReceiveLength + staged.LocalLength + staged.RemoteLength;

        byte* localAddr = null;
        int32 localLen = 0;
        byte* remoteAddr = null;
        int32 remoteLen = 0;

        Syscall9(procGetAcceptExSockaddrs.Addr(), 8, (uintptr)(void*)buffer, (uintptr)staged.ReceiveLength, (uintptr)staged.LocalLength, (uintptr)staged.RemoteLength, (uintptr)(void*)(&localAddr), (uintptr)(void*)(&localLen), (uintptr)(void*)(&remoteAddr), (uintptr)(void*)(&remoteLen), 0);

        // The kernel's pointers land inside the staging block; bound every read by what is actually
        // there rather than by the 116 bytes a RawSockaddrAny could hold.
        nint localRoom = localAddr is null ? 0 : (nint)(stagedLength - (nuint)(localAddr - buffer));
        nint remoteRoom = remoteAddr is null ? 0 : (nint)(stagedLength - (nuint)(remoteAddr - buffer));

        if (Ꮡlrsa != nil) {
            // ValueSlot, not Value: T here is itself a POINTER (ж<ж<RawSockaddrAny>>), so the
            // caller's variable legitimately holds null on entry -- and Value's nil-pointer check
            // VALUE-PEEKS, so it would panic on the very write that fills it in. ValueSlot is the
            // documented form for exactly this shape (golib ж.cs).
            Ꮡlrsa.ValueSlot = localAddr is null ? default! : toRawSockaddrAny(localAddr, localRoom);
        }

        if (Ꮡrrsa != nil) {
            Ꮡrrsa.ValueSlot = remoteAddr is null ? default! : toRawSockaddrAny(remoteAddr, remoteRoom);
        }

        if (Ꮡlrsalen != nil) {
            Ꮡlrsalen.Value = localLen;
        }

        if (Ꮡrrsalen != nil) {
            Ꮡrrsalen.Value = remoteLen;
        }
    }

    // ---- CancelIoEx ------------------------------------------------------------------------------

    // execIO cancels BY ADDRESS, so the cancel must name the record's one true native control block
    // -- never a fresh copy, which the kernel has never seen.
    //
    // TWO CALLERS, and they need different answers. fd_windows.cs:212 cancels ONE operation (the
    // deadline/close path); fd_windows.cs:403 cancels ALL IO on a handle with a NIL overlapped (the
    // pipe/file close path), which passes straight through. A non-nil overlapped with no record
    // answers ERROR_NOT_FOUND -- which execIO already tolerates as "the IO already completed" --
    // rather than falling back to 0 and cancelling everything on the socket.
    public static unsafe error /*err*/ CancelIoEx(ΔHandle s, ж<Overlapped> Ꮡo) {
        uintptr address = 0;

        if (Ꮡo != nil) {
            if (!GoAsyncIO.TryGetOperationAddress(Ꮡo, out nuint native)) {
                return ((error)ERROR_NOT_FOUND);
            }

            address = native;
        }

        var (r1, _, e1) = Syscall(procCancelIoEx.Addr(), 2, (uintptr)s, address, 0);

        return r1 == 0 ? errnoErr(e1) : default!;
    }

    // ---- LoadConnectEx ---------------------------------------------------------------------------

    // NOT part of the overlapped family, and hand-owned for a reason this arc MEASURED rather than
    // predicted: the design recorded the extension-pointer lookup as "synchronous and already
    // working". It is not. The converted body is defective at BOTH ends of one WSAIoctl call.
    //
    //   IN: `ᏑWSAID_CONNECTEX.Reinterpret<GUID, byte>()`. syscall.GUID holds `Data4 [8]byte` as a
    //   golib array<byte> MANAGED REFERENCE, so the struct is reference-bearing: Reinterpret cannot
    //   alias it as bytes, falls back to a raw-address box with NO pin, and the 16 bytes the kernel
    //   reads are a CLR auto-layout image with an object reference in them -- never WSAID_CONNECTEX.
    //   Windows answers WSAEINVAL, which surfaces through the hand-owned ConnectEx as
    //   "failed to find ConnectEx: An invalid argument was supplied" -- the exact shape crypto/tls's
    //   census banked nine times, one per loopback dial, each dying in ~2 ms.
    //
    //   OUT: `ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡaddr).Reinterpret<uintptr, byte>()` is an interior
    //   field address inside a struct that holds an `error` -- reference-bearing, so unpinnable, so
    //   transient. Even a successful call would write the function pointer to an address the GC is
    //   free to have moved.
    //
    // The remedy is the package's established one: a blittable native image for the GUID, a native
    // uintptr for the result, both stack locals live for exactly the call, and the answer assigned to
    // the managed field as an ordinary managed write. The GUID VALUE is still the converted
    // declaration's -- read out of ᏑWSAID_CONNECTEX field by field -- so there is one definition of
    // it and this file does not restate a constant.
    public static unsafe error LoadConnectEx() {
        ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡonce).Do(() => {
            var (s, err) = Socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

            if (err != default!) {
                connectExFunc.err = err;
                return;
            }

            try {
                // The 16-byte native GUID: {Data1 uint32}{Data2 uint16}{Data3 uint16}{Data4 [8]byte},
                // which is the on-the-wire layout Windows compares against.
                byte* guid = stackalloc byte[16];
                ref GUID managed = ref ᏑWSAID_CONNECTEX.Value;

                *(uint32*)guid = managed.Data1;
                *(uint16*)(guid + 4) = managed.Data2;
                *(uint16*)(guid + 6) = managed.Data3;

                for (nint i = 0; i < 8; i++) {
                    guid[8 + i] = managed.Data4[i];
                }

                uintptr address = 0;
                uint32 bytes = 0;

                var (r1, _, e1) = Syscall9(procWSAIoctl.Addr(), 9, (uintptr)s, (uintptr)SIO_GET_EXTENSION_FUNCTION_POINTER, (uintptr)(void*)guid, (uintptr)16, (uintptr)(void*)(&address), (uintptr)(uint32)sizeof(uintptr), (uintptr)(void*)(&bytes), 0, 0);

                if (r1 == socket_error) {
                    connectExFunc.err = errnoErr(e1);
                    return;
                }

                connectExFunc.addr = address;
            } finally {
                // Go closes the probe socket with CloseHandle (not closesocket); kept verbatim.
                CloseHandle(s);
            }
        });

        return connectExFunc.err;
    }

    // ---- The record's native overlapped, for this package's other hand-owns ----------------------

    // ConnectEx lives in syscall_windows_impl.cs (it was hand-owned by the sockaddr lane before this
    // arc existed) and needs the same record machinery as the wrappers above -- INCLUDING the pin
    // retention, which is why this takes the boxes rather than leaving the caller to KeepAlive them.
    //
    // A `GC.KeepAlive` after the call is the right closure for a SYNCHRONOUS wrapper, and it is
    // exactly wrong here: ConnectEx's lpOverlapped may not be NULL, so the submit is asynchronous by
    // construction and the kernel's use of lpSendBuffer (and of lpdwBytesSent, which the
    // documentation ties to lpSendBuffer's presence) begins where a KeepAlive would end. The pin has
    // to live for the FLIGHT, which is precisely what m_pins is -- the file header's own reason:
    // "Holding the address would not be enough: the pin is a GCHandle owned by that one box and
    // released with it."
    //
    // The retention is taken AFTER Rearm and cannot be reordered: Rearm() clears the previous
    // submit's pins, so a box pinned before it would be dropped on the spot. Folding both into one
    // call is what makes that ordering unstateable at the call site rather than merely documented.
    internal static unsafe uintptr rearmOverlapped(ΔHandle handle, ж<Overlapped> Ꮡoverlapped, nint mode, INilPointer? pinFirst, INilPointer? pinSecond) {
        OverlappedOp operation = operationFor(handle, Ꮡoverlapped, mode);
        uintptr native = (uintptr)(void*)operation.Rearm();

        retainForFlight(operation, pinFirst);
        retainForFlight(operation, pinSecond);

        return native;
    }

    // A nil pointer names no managed storage and took no pin -- ж.cs's uintptr operator answers 0
    // for it before EnsureStableAddress is ever reached -- so there is nothing to hold still, and a
    // native-backed box's address is not managed storage either. Both are retained anyway rather
    // than tested for: holding an object that owns no pin costs one list slot until the next Rearm,
    // and a test for "is this one of the two harmless kinds" is a predicate that can drift away from
    // the operator's. Only the C# null and the structural nil are skipped.
    private static void retainForFlight(OverlappedOp operation, INilPointer? box) {
        if (box is not null && !box.IsNilPointer) {
            operation.Pin(box);
        }
    }

    // ---- The INITIALISATION pair -----------------------------------------------------------------
    //
    // WHY HERE. These two are the plain native-layout wall of zsyscall_windows_impl.cs -- no
    // overlapped, no lifetime beyond the call, no identity -- so on the class alone they would live
    // there. They are here instead because that file is the kernel32 side of the wall
    // (GetTimeZoneInformation, FindFirstFileW, Process32FirstW) and this one is ws2_32's, and the
    // family is worth keeping whole: a reader chasing a Winsock defect should find every Winsock
    // hand-own in one file. LoadConnectEx already set that precedent. The coupling is real in one
    // more direction too -- the enumeration below computes the answer this file's own skipSyncNotif
    // reasoning turns on (see "THE OVERLAPPED IS FREED AT THE NEXT SUBMIT" in the header).
    //
    // Both files are windows-only and sit in the same per-GOOS folder, so the helpers they share
    // (copyNativeName in zsyscall_windows_impl.cs) are routed to the same platform set by layout L3.

    // WSADESCRIPTION_LEN / WSASYS_STATUS_LEN / WSAPROTOCOL_LEN / MAX_PROTOCOL_CHAIN as the native
    // layouts use them. syscall's own are UntypedInt properties, not compile-time constants, and a
    // `fixed` buffer needs one -- the same reason zsyscall_windows_impl.cs restates maxPath.
    private const int wsaDescriptionLen = 256;
    private const int wsaSysStatusLen = 128;
    private const int wsaProtocolLen = 255;
    private const int maxProtocolChain = 7;

    // WSADATA exactly as Windows lays it out for _WIN64: 408 bytes with both character buffers
    // INLINE. The converted WSAData holds them as two `array<byte>` MANAGED REFERENCES and the vendor
    // pointer as a ж<byte>, so it is roughly 40 bytes with every field at the wrong offset -- the
    // kernel wrote 408 bytes over it and left ASCII where the two array references belong, which is
    // why reading Description faulted in slice<byte>..ctor.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWSAData
    {
        public uint16 Version;
        public uint16 HighVersion;
        public uint16 MaxSockets;
        public uint16 MaxUdpDg;
        public byte* VendorInfo;
        public fixed byte Description[wsaDescriptionLen + 1];
        public fixed byte SystemStatus[wsaSysStatusLen + 1];
    }

    // WSAStartup is the native transcription of the generated wrapper. Return values follow the Go
    // original exactly: WSAStartup reports its failure through the RETURN VALUE rather than
    // WSAGetLastError, so a non-zero r0 becomes that Errno directly and no last-error is consulted.
    public static unsafe error /*sockerr*/ WSAStartup(uint32 verreq, ж<WSAData> Ꮡdata) {
        NativeWSAData native = default;

        var (r0, _, _) = Syscall(procWSAStartup.Addr(), 2, (uintptr)verreq, (uintptr)(void*)(&native), 0);

        if (r0 != 0) {
            // `data` is left untouched on failure, as it is in Go, because the kernel did not write it.
            return ((Errno)r0);
        }

        if (Ꮡdata != nil) {
            ref WSAData data = ref Ꮡdata.Value;

            data.Version = native.Version;
            data.HighVersion = native.HighVersion;
            data.MaxSockets = native.MaxSockets;
            data.MaxUdpDg = native.MaxUdpDg;

            // lpVendorInfo is documented RESERVED and every supported Windows leaves it NULL, which
            // is the nil a Go caller reads. It is reported as nil rather than wrapped: a raw native
            // address has no managed referent to name, and fabricating one is the type-safety break
            // ж.PointerExtensions.cs forbids. If a provider ever set it, this is where that shows.
            data.VendorInfo = nil;

            // Copied WHOLE, NULs included, for the reason copyNativeName gives: Go reads these as a
            // NUL-terminated run and Windows pads the remainder.
            copyNativeBytes(native.Description, ref data.Description, wsaDescriptionLen + 1);
            copyNativeBytes(native.SystemStatus, ref data.SystemStatus, wsaSysStatusLen + 1);
        }

        return default!;
    }

    // GUID exactly as Windows lays it out: 16 bytes with Data4 inline. LoadConnectEx above builds the
    // same image by hand into a stackalloc because it needs only one, in one direction; this is the
    // same 16 bytes as a type, because WSAPROTOCOL_INFOW embeds it.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeGuid
    {
        public uint32 Data1;
        public uint16 Data2;
        public uint16 Data3;
        public fixed byte Data4[8];
    }

    // WSAPROTOCOLCHAIN: 32 bytes, ChainEntries[7] inline.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWSAProtocolChain
    {
        public int32 ChainLen;
        public fixed uint32 ChainEntries[maxProtocolChain];
    }

    // WSAPROTOCOL_INFOW exactly as Windows lays it out: 628 bytes, ending in szProtocol[256] inline
    // 512 bytes past the fields the catalog's contract pins. Field names and order follow the
    // converted WSAProtocolInfo, which is the struct the copy below fills.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeWSAProtocolInfoW
    {
        public uint32 ServiceFlags1;
        public uint32 ServiceFlags2;
        public uint32 ServiceFlags3;
        public uint32 ServiceFlags4;
        public uint32 ProviderFlags;
        public NativeGuid ProviderId;
        public uint32 CatalogEntryId;
        public NativeWSAProtocolChain ProtocolChain;
        public int32 Version;
        public int32 AddressFamily;
        public int32 MaxSockAddr;
        public int32 MinSockAddr;
        public int32 SocketType;
        public int32 Protocol;
        public int32 ProtocolMaxOffset;
        public int32 NetworkByteOrder;
        public int32 SecurityScheme;
        public uint32 MessageSize;
        public uint32 ProviderReserved;
        public fixed uint16 ProtocolName[wsaProtocolLen + 1];
    }

    // WSAEnumProtocols is the native transcription of the generated wrapper. THREE things distinguish
    // it from the other members of this class, and all three come from one fact: this call is sized
    // by a BYTE COUNT the caller computed, over an array it also hands over.
    //
    //   (1) The kernel writes into a NATIVE block this function owns, never into the managed array.
    //       `*bufferLength` is Go's `unsafe.Sizeof(buf)`, which the converter answers with Go's --
    //       i.e. the native -- size, so it is already the number Windows expects; what it is not is
    //       the size of the managed array, which is roughly a fifth of it. The block is sized from
    //       that same byte count, so the two agree by construction.
    //
    //   (2) SIZE IS AN INPUT AND AN OUTPUT. On WSAENOBUFS Windows rewrites `*bufferLength` with the
    //       size the catalog needs, expressed in NATIVE strides -- which the Go caller then compares
    //       against its own unsafe.Sizeof arithmetic. It is passed straight back out unaltered: it is
    //       native-side arithmetic on both sides of the boundary, and converting it would be the
    //       defect, not the fix.
    //
    //   (3) The count Windows returns is a count of NATIVE records, so the copy-back walks the
    //       staging block at the native stride and steps the caller's pointer with unsafe.Add, which
    //       resolves through the backing array (the same mechanism stageBuffers relies on).
    public static unsafe (int32 n, error err) WSAEnumProtocols(ж<int32> Ꮡprotocols, ж<WSAProtocolInfo> ᏑprotocolBuffer, ж<uint32> ᏑbufferLength) {
        nuint stride = (nuint)sizeof(NativeWSAProtocolInfoW);
        uint32 length = ᏑbufferLength == nil ? 0 : ᏑbufferLength.Value;
        nuint capacity = ᏑprotocolBuffer == nil ? 0 : (nuint)length / stride;

        // A non-NULL buffer stays non-NULL even when the caller declared it empty: that is the
        // required-size query, and Windows distinguishes the two pointers.
        void* staging = ᏑprotocolBuffer == nil ? null : NativeMemory.AllocZeroed(length == 0 ? 1 : (nuint)length);

        try {
            uint32 inout = length;
            int32 n;
            Errno e1;

            // `protocols` is the caller's NUL-terminated int32 list, reached through a golib pointer
            // box whose uintptr conversion can only hand back a TRANSIENT address. Pinning it here
            // keeps it fixed for the whole call -- `Value` on an element box resolves to a ref INTO
            // the caller's backing array, so `fixed` pins that array and the whole list stays
            // contiguous behind the pointer (the same pattern findFirstFile1 uses for its name).
            // A nil list is legal and means "every protocol".
            if (Ꮡprotocols == nil) {
                (n, e1) = enumProtocols(null, staging, &inout);
            } else {
                fixed (int32* protocols = &Ꮡprotocols.Value) {
                    (n, e1) = enumProtocols(protocols, staging, &inout);
                }
            }

            if (ᏑbufferLength != nil) {
                ᏑbufferLength.Value = inout;
            }

            if (n == -1) {
                // The buffer is left untouched on failure, as it is in Go, because the kernel wrote
                // nothing into it -- WSAENOBUFS reports only the size it would have needed.
                return (n, errnoErr(e1));
            }

            nuint written = n < 0 ? 0 : (nuint)n;

            // Windows cannot have written more records than the declared byte count holds, but the
            // clamp states that rather than trusting it: `written` is the loop bound over a block
            // this function sized.
            if (written > capacity) {
                written = capacity;
            }

            for (nuint i = 0; i < written; i++) {
                copyNativeProtocolInfo(
                    (NativeWSAProtocolInfoW*)((byte*)staging + i * stride),
                    i == 0 ? ᏑprotocolBuffer : unsafe_package.Add(ᏑprotocolBuffer, (uintptr)i));
            }

            return (n, default!);
        } finally {
            NativeMemory.Free(staging);
        }
    }

    // The call itself, split out so the pinned and unpinned forms of the protocol list share one
    // spelling of it. Returns the raw result and the Errno, which only the caller knows how to read.
    private static unsafe (int32, Errno) enumProtocols(int32* protocols, void* buffer, uint32* length) {
        var (r0, _, e1) = Syscall(procWSAEnumProtocolsW.Addr(), 3, (uintptr)(void*)protocols, (uintptr)buffer, (uintptr)(void*)length);

        return ((int32)r0, e1);
    }

    // Copies one native record into the converted WSAProtocolInfo. Every field is carried, not only
    // the ones the corpus reads today: the Go declaration exposes all of them, and a field left
    // behind is the quiet-wrong-answer shape this whole class takes.
    private static unsafe void copyNativeProtocolInfo(NativeWSAProtocolInfoW* native, ж<WSAProtocolInfo> Ꮡinfo) {
        ref WSAProtocolInfo info = ref Ꮡinfo.Value;

        info.ServiceFlags1 = native->ServiceFlags1;
        info.ServiceFlags2 = native->ServiceFlags2;
        info.ServiceFlags3 = native->ServiceFlags3;
        info.ServiceFlags4 = native->ServiceFlags4;
        info.ProviderFlags = native->ProviderFlags;

        info.ProviderId.Data1 = native->ProviderId.Data1;
        info.ProviderId.Data2 = native->ProviderId.Data2;
        info.ProviderId.Data3 = native->ProviderId.Data3;
        copyNativeBytes(native->ProviderId.Data4, ref info.ProviderId.Data4, 8);

        info.CatalogEntryId = native->CatalogEntryId;

        info.ProtocolChain.ChainLen = native->ProtocolChain.ChainLen;
        copyNativeWords(native->ProtocolChain.ChainEntries, ref info.ProtocolChain.ChainEntries, maxProtocolChain);

        info.Version = native->Version;
        info.AddressFamily = native->AddressFamily;
        info.MaxSockAddr = native->MaxSockAddr;
        info.MinSockAddr = native->MinSockAddr;
        info.SocketType = native->SocketType;
        info.Protocol = native->Protocol;
        info.ProtocolMaxOffset = native->ProtocolMaxOffset;
        info.NetworkByteOrder = native->NetworkByteOrder;
        info.SecurityScheme = native->SecurityScheme;
        info.MessageSize = native->MessageSize;
        info.ProviderReserved = native->ProviderReserved;

        // Copied WHOLE, NULs included: Go reads it as UTF16ToString(p.ProtocolName[:]), which stops
        // at the first NUL, and one WSAProtocolInfo is reused across a whole enumeration.
        copyNativeName(native->ProtocolName, ref info.ProtocolName, wsaProtocolLen + 1);
    }

    // copyNativeName's byte and uint32 siblings (that one lives in zsyscall_windows_impl.cs and is
    // uint16). Each (re)allocates a destination that is not already the right length, so a struct
    // that reached here as `default` -- its field initializer never having run, which is what a
    // nested struct FIELD like GUID.Data4 always is -- is filled rather than dereferenced through a
    // null backing.
    private static unsafe void copyNativeBytes(byte* source, ref array<byte> destination, nint length) {
        if (destination.Length != length) {
            destination = new array<byte>(length);
        }

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }
    }

    private static unsafe void copyNativeWords(uint32* source, ref array<uint32> destination, nint length) {
        if (destination.Length != length) {
            destination = new array<uint32>(length);
        }

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }
    }

    // ---- WSARecvFrom: the DATAGRAM receive, and the decode side of the class ----------------------

    // Go's RawSockaddrAny is 2 + 14 + 100 bytes flat; internal/poll hard-codes that 116 as the
    // buffer length it declares to the kernel, and RawSockaddrAny.Sockaddr flattens back to it.
    private const int32 rawSockaddrAnyLen = 116;

    // Go: WSARecvFrom(s, bufs, bufcnt, recvd, flags, from, fromlen, overlapped, croutine).
    //
    // WHY HAND-OWNED. The generated body hands the kernel `(uintptr)Ꮡfrom` -- the pinned address of a
    // MANAGED RawSockaddrAny -- and internal/poll tells it that buffer is 116 bytes
    // (`o.rsan = unsafe.Sizeof(*o.rsa)`). The managed struct is FORTY bytes and contains references:
    // `Addr.Data` and `Pad` are `array<int8>` object references where Go has inline octets. So the
    // kernel's write overflows the managed object by 76 bytes into the GC heap, and the bytes that do
    // land inside it land ON reference fields. Measured, not inferred: RawSockaddrAny reports
    // containsReferences=true and Unsafe.SizeOf 40 against Go's 128.
    //
    // THE REMEDY IS THE ACCEPT PAIR, ONE FUNCTION OVER (netpoll design §4.8, RATIFIED). AcceptEx
    // already solves the identical problem: the kernel writes into the RECORD's native staging and
    // `toRawSockaddrAny` transcribes that native image into a managed RawSockaddrAny field for field,
    // so `RawSockaddrAny.Sockaddr` -- which flattens the managed struct back to its 116-byte native
    // image -- has a faithful thing to read. Those two are documented as "a pair; neither is
    // meaningful alone"; this is the third member.
    //
    // WHY AT HARVEST. An overlapped receive fills its buffers AFTER this call returns, so there is no
    // moment inside the wrapper at which a decode could run. golib's completion seam carries the
    // transcription to where `internal/syscall/windows.WSAGetOverlappedResult` observes the
    // completion -- the one call every asynchronous exit of `execIO` funnels through. A SYNCHRONOUS
    // completion never reaches that harvest, so it is transcribed inline at the end.
    public static unsafe error /*err*/ WSARecvFrom(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡrecvd, ж<uint32> Ꮡflags, ж<RawSockaddrAny> Ꮡfrom, ж<int32> Ꮡfromlen, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeRead);
        NativeOverlapped* native = operation.Rearm();

        // ONE staging block carved into three regions, because Staging() owns a SINGLE allocation and
        // a later call with a larger size reallocates -- which would dangle every pointer carved from
        // the earlier one. Carving is the caller's business by design: the seam owns the allocation,
        // never the layout.
        //
        // ⚠ ORDER IS LOAD-BEARING. This oversized request runs BEFORE stageBuffers, whose own
        // Staging(bufBytes) call is smaller and therefore returns this same block untouched
        // (Staging reuses when m_stagingLength >= length). Reversing the two would reallocate under
        // the address and length pointers.
        nuint bufBytes = (nuint)(bufcnt == 0 ? 1 : bufcnt) * (nuint)sizeof(NativeWSABuf);
        nuint addrOffset = bufBytes;
        nuint lenOffset = addrOffset + (nuint)nativeSockaddrLen;
        byte* block = (byte*)operation.Staging(lenOffset + (nuint)sizeof(int32));

        NativeWSABuf* buffers = stageBuffers(operation, Ꮡbufs, bufcnt);
        byte* addr = block + addrOffset;
        int32* addrlen = (int32*)(block + lenOffset);

        *addrlen = nativeSockaddrLen;

        uint32 recvd = 0;
        uint32 flags = Ꮡflags == nil ? 0 : Ꮡflags.Value;

        // The transcription this receive owes on completion. The staged addresses are captured as
        // INTEGERS, not pointers: a C# closure cannot capture a pointer local, and the staging
        // outlives the operation by construction, so an address is the honest thing to carry.
        nuint addrAddress = (nuint)addr;
        nuint lenAddress = (nuint)addrlen;
        ж<RawSockaddrAny> Ꮡdst = Ꮡfrom;
        ж<int32> Ꮡdstlen = Ꮡfromlen;

        golib.GoAsyncIO.SetOperationCompletion(Ꮡoverlapped, _ => {
            int32 wrote = *(int32*)lenAddress;

            if (Ꮡdst != nil) {
                fillRawSockaddrAny(Ꮡdst, (byte*)addrAddress, wrote < 0 ? 0 : wrote);
            }
            if (Ꮡdstlen != nil) {
                // internal/poll measures this against Go's 116-byte RawSockaddrAny, so report what
                // the kernel wrote, clamped to what a Go caller can mean by it.
                Ꮡdstlen.Value = wrote > rawSockaddrAnyLen ? rawSockaddrAnyLen : wrote;
            }
        });

        var (r1, _, e1) = Syscall9(procWSARecvFrom.Addr(), 9, (uintptr)s, (uintptr)(void*)buffers, (uintptr)bufcnt,
                                   (uintptr)(void*)(&recvd), (uintptr)(void*)(&flags), (uintptr)(void*)addr,
                                   (uintptr)(void*)addrlen, (uintptr)native, (uintptr)Ꮡcroutine);

        if (Ꮡrecvd != nil) {
            Ꮡrecvd.Value = recvd;
        }

        if (Ꮡflags != nil) {
            Ꮡflags.Value = flags;
        }

        if (r1 == socket_error) {
            error err = errnoErr(e1);

            // ERROR_IO_PENDING is the normal overlapped path: the harvest runs the transcription. Any
            // OTHER error means no completion will arrive, so the operation owes nothing -- drop the
            // pending work rather than leave it for the next submit on this waiter to inherit.
            if (!AreEqual(err, ERROR_IO_PENDING)) {
                golib.GoAsyncIO.CompleteOperation(Ꮡoverlapped, 0);
            }
            return err;
        }

        // Completed SYNCHRONOUSLY: the data is already staged, and an FD carrying skipSyncNotif never
        // harvests, so transcribe now. CompleteOperation removes before it runs, so a later harvest
        // of the same operation cannot decode twice.
        golib.GoAsyncIO.CompleteOperation(Ꮡoverlapped, (nint)recvd);
        return default!;
    }

    // ---- WSASendto: the DATAGRAM SEND, and WSARecvFrom's twin with the staging carved the other way

    // Go: WSASendto(s, bufs, bufcnt, sent, flags, to, overlapped, croutine).
    //
    // WHY HAND-OWNED -- FOUR defects in one statement, where the struct-passing census sees two.
    // docs/phase4/DESIGN-windows-udp-send.md sizes them; the generated body
    // (syscall_windows.cs, before its placeholder) hands the kernel:
    //
    //  1. `(uintptr)Ꮡbufs` -- a MANAGED WSABuf, whose `Buf` is a `ж<byte>` reference where native
    //     WSABUF wants a raw CHAR*. The file header's defect (1), same as WSARecv/WSASend.
    //  2. `(uintptr)rsa` -- whatever `to.sockaddr()` returned, which is the address of a managed
    //     `sa.raw`. That method's own body says so: "It is NOT a native image... which is why every
    //     in-package caller that actually reaches the kernel builds one with writeNativeSockaddr
    //     instead of consuming this." This was the caller that contradicted it.
    //  3. `(uintptr)Ꮡsent` and 4. `(uintptr)Ꮡoverlapped` -- interior field addresses inside
    //     internal/poll's reference-bearing `operation`, which golib's address model cannot hold
    //     still, and the OVERLAPPED is additionally the operation's kernel-side IDENTITY. The file
    //     header's defect (2), and the reason this member lives here rather than beside the sockaddr
    //     mirror: three of its four defects are the async family this file owns.
    //
    // NOTHING NEW IS BUILT. Defects 1, 3 and 4 fall to WSASend's record/rearm/stageBuffers plus
    // WSARecvFrom's carve; defect 2 falls to `writeNativeSockaddr`, which is PRIVATE to `syscall`
    // and reachable because this body is too -- so the public GoWriteNativeSockaddrInet4/6 seam that
    // the Linux and internal/syscall/windows halves consume is not involved, and needs no widening.
    // The shape set it must cover is CLOSED, not merely sufficient: Go declares `Sockaddr` with an
    // unexported method ("lowercase; only we can define Sockaddrs"), so no package outside `syscall`
    // can implement it, the windows sources define exactly three `sockaddr()` methods, and
    // package_info.cs independently records exactly three ΔSockaddr implementations.
    //
    // NO SUITE CAN REACH THIS ON WINDOWS, which is why the acceptance is a behavioral guard
    // (WsaSendtoRoundTrip) rather than a roster row. The sole consumer is internal/poll.FD.WriteTo,
    // whose net-side callers are IPConn and UnixConn -- and net's own testableNetwork returns false
    // for unix/unixgram on windows outright and requires Getuid()==0 for ip/ip4/ip6. UDPConn never
    // arrives either: UDPConn.writeTo switches on fd.family into writeToInet4/6, the pair hand-owned
    // in internal/syscall/windows.
    //
    // THE SYNCHRONOUS ARM, and why it is here at all. `operationFor` keys a record off the
    // OVERLAPPED, so a NIL one has no key; every corpus caller passes `&o.o`, but Go's contract
    // accepts nil and a socket created outside internal/poll is bound to no completion port, which
    // is exactly what a guard driving this function directly must do. That arm stages into
    // `stackalloc`, and the ⟨OQ-G⟩ ruling that forbids a stack buffer for `lpTo` on the OVERLAPPED
    // path does not reach it: that ruling turns on Winsock's silence about when it captures `lpTo`,
    // which can only matter if the call returns before the send completes. A synchronous send cannot,
    // so the established rule applies unchanged -- "a LOCAL at the call site, trivially stable for
    // exactly that long" (syscall_windows_impl.cs).
    public static unsafe error /*err*/ WSASendto(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent, uint32 flags, ΔSockaddr to, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
        // SEEDED from the caller's own slot rather than from zero, so the unconditional write-back
        // below is a no-op on any path where the kernel did not write. Go hands the kernel the
        // caller's pointer directly, so a failed call leaves that slot exactly as it was; starting
        // at zero would deposit a zero there instead. (WSASend above starts at zero and is left
        // alone: its slot is meaningless on failure to its one caller, and changing a sibling's
        // behaviour is not this cut's business.)
        uint32 sent = Ꮡsent != nil ? Ꮡsent.Value : 0;
        int32 addrlen = 0;
        uintptr r1;
        Errno e1;

        if (Ꮡoverlapped == nil) {
            // Synchronous: everything the kernel reads is consumed before this returns.
            NativeWSABuf* stackBufs = stackalloc NativeWSABuf[(int)(bufcnt == 0 ? 1 : bufcnt)];
            byte* stackAddr = stackalloc byte[nativeSockaddrLen];

            writeWSABufs(Ꮡbufs, bufcnt, stackBufs, null);

            // `to` is legitimately nil (Go's own `if to != nil` guard), and then lpTo/iTolen are
            // both zero -- so the pointer starts null and only the encode gives it a value.
            byte* toPtr = null;

            if (to != default!) {
                error encodeErr;
                (addrlen, encodeErr) = writeNativeSockaddr(to, stackAddr);

                if (encodeErr != default!) {
                    return encodeErr;
                }

                toPtr = stackAddr;
            }

            (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, (uintptr)s, (uintptr)(void*)stackBufs, (uintptr)bufcnt,
                                   (uintptr)(void*)(&sent), (uintptr)flags,
                                   (uintptr)(void*)toPtr, (uintptr)addrlen,
                                   0, (uintptr)Ꮡcroutine);

            // The pins live on the `ж<byte>` boxes `Ꮡbufs` names, so keeping the descriptor array
            // reachable keeps every buffer pinned for exactly the span the kernel reads them.
            System.GC.KeepAlive(Ꮡbufs);
        } else {
            OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeWrite);
            NativeOverlapped* native = operation.Rearm();

            // ONE staging block carved into two regions, and ⚠ THE ORDER IS LOAD-BEARING for the
            // reason WSARecvFrom states: this oversized request must run BEFORE stageBuffers, whose
            // own smaller Staging() call then returns this same block untouched. Reversing the two
            // would reallocate under the address pointer.
            nuint bufBytes = (nuint)(bufcnt == 0 ? 1 : bufcnt) * (nuint)sizeof(NativeWSABuf);
            byte* block = (byte*)operation.Staging(bufBytes + (nuint)nativeSockaddrLen);

            NativeWSABuf* buffers = stageBuffers(operation, Ꮡbufs, bufcnt);
            byte* addr = block + bufBytes;
            byte* toPtr = null;

            if (to != default!) {
                error encodeErr;
                (addrlen, encodeErr) = writeNativeSockaddr(to, addr);

                if (encodeErr != default!) {
                    // No submit was issued, so the operation owes nothing -- drop the pending work
                    // rather than leave it for the next submit on this waiter to inherit.
                    golib.GoAsyncIO.CompleteOperation(Ꮡoverlapped, 0);
                    return encodeErr;
                }

                toPtr = addr;
            }

            (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, (uintptr)s, (uintptr)(void*)buffers, (uintptr)bufcnt,
                                   (uintptr)(void*)(&sent), (uintptr)flags,
                                   (uintptr)(void*)toPtr, (uintptr)addrlen,
                                   (uintptr)native, (uintptr)Ꮡcroutine);
        }

        if (Ꮡsent != nil) {
            Ꮡsent.Value = sent;
        }

        if (r1 != socket_error) {
            return default!;
        }

        // The generated body's mapping, kept verbatim: a socket_error with NO errno is EINVAL, not
        // success. WSASendTo reports failure through the return value and the error through
        // WSAGetLastError, and a lost errno must not read as "sent".
        return e1 != 0 ? errnoErr(e1) : EINVAL;
    }

    // ---- TransmitFile: the SENDFILE submit, and the one member that reads the OVERLAPPED ----------

    // Native TRANSMIT_FILE_BUFFERS. The converted `TransmitFileBuffers` is all-scalar
    // (uintptr/uint32 ×2), so unlike WSABuf it is not a LAYOUT defect -- but it is still a MANAGED
    // struct whose address the kernel would have to keep for the flight, which is the same lifetime
    // wall the file header states for the overlapped. Mirrored into the record's staging for the
    // same reason, and it costs four field copies.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeTransmitFileBuffers
    {
        public nuint Head;
        public uint32 HeadLength;
        public nuint Tail;
        public uint32 TailLength;
    }

    // Go: TransmitFile(s, handle, bytesToWrite, bytsPerSend, overlapped, transmitFileBuf, flags).
    //
    // WHY IT JOINS THE SEAM NOW, by the rule this file's header states: a suite REACHED it. net's
    // own sendfile family does -- TestSendfileParts and TestSendfileSeeked copy through
    // `io.CopyN(conn, f, n)`, whose `*io.LimitedReader` is not an `io.WriterTo`, so `io.Copy` takes
    // the `ReaderFrom` branch into net.sendFile -> internal/poll.SendFile -> here. (TestSendfile,
    // one name earlier, passes `*os.File` DIRECTLY, and `os.File.WriteTo` wins the `WriterTo`
    // branch ahead of `ReaderFrom` -- go.dev/issue/67042, which the test's own comment cites -- so
    // it never reaches TransmitFile at all. That is why the family split three-to-two and why the
    // stop looked like a sendfile bug that TestSendfile disproved.)
    //
    // WHAT THE GENERATED BODY DID. It handed the kernel `(uintptr)Ꮡoverlapped` -- the pinned-for-one-
    // statement interior address of the managed `internal/poll.operation`, which holds references, so
    // golib's address model cannot hold it still -- and, decisively, it created NO operation record.
    // With no record there is no completion-port binding for the socket and no readiness sink, so the
    // kernel's completion had nowhere to arrive: `execIO` fell through to `pd.wait('w')` and
    // `runtime_pollWait` blocked on `Monitor.Wait` FOREVER, with the peer blocked in `Read` waiting
    // for bytes that were queued but never reported. A deterministic two-sided deadlock -- which is
    // exactly the shape it was measured as (time-independent: a 60m deadline released nothing a 30m
    // one had not), and it beheaded the whole alphabetical tail of net's suite from
    // `TestSendfileParts` on.
    //
    // THE ONE THING THIS MEMBER NEEDS THAT THE SOCKET SENDS DO NOT: the FILE OFFSET travels in the
    // OVERLAPPED. `internal/poll.SendFile` writes `o.o.Offset` / `o.o.OffsetHigh` before every submit
    // and re-seeks between chunks; WSASend/WSARecv ignore those fields, so `Rearm()` -- which hands
    // back a FRESH, zeroed control block each submit -- has never had to carry anything across. Here
    // it must: without the copy every chunk would transmit from offset 0, which is not a hang but a
    // silent wrong answer (TestSendfileSeeked's whole subject).
    public static unsafe error /*err*/ TransmitFile(ΔHandle s, ΔHandle handle, uint32 bytesToWrite, uint32 bytsPerSend, ж<Overlapped> Ꮡoverlapped, ж<TransmitFileBuffers> ᏑtransmitFileBuf, uint32 flags) {
        OverlappedOp operation = operationFor(s, Ꮡoverlapped, wsaModeWrite);
        NativeOverlapped* native = operation.Rearm();

        // The file offset the caller published in its own OVERLAPPED, carried onto the control block
        // the kernel will actually read. `unchecked` because NativeOverlapped spells both halves
        // `int` while Go (and Windows) spell them DWORD.
        if (Ꮡoverlapped != nil) {
            native->OffsetLow = unchecked((int)Ꮡoverlapped.Value.Offset);
            native->OffsetHigh = unchecked((int)Ꮡoverlapped.Value.OffsetHigh);
        }

        // The head/tail buffers are OPTIONAL and the corpus's only caller passes nil; mirrored
        // anyway rather than passed through, so a caller that does use them is not a latent
        // lifetime defect waiting to be discovered by a suite. Head and Tail are already `uintptr`
        // -- raw addresses the caller owns -- so the mirror is a straight copy with nothing to pin.
        NativeTransmitFileBuffers* buffers = null;

        if (ᏑtransmitFileBuf != nil) {
            buffers = (NativeTransmitFileBuffers*)operation.Staging((nuint)sizeof(NativeTransmitFileBuffers));

            ref TransmitFileBuffers managed = ref ᏑtransmitFileBuf.Value;

            buffers->Head = (nuint)managed.Head.Value;
            buffers->HeadLength = managed.HeadLength;
            buffers->Tail = (nuint)managed.Tail.Value;
            buffers->TailLength = managed.TailLength;
        }

        // TransmitFile answers BOOL, not SOCKET_ERROR -- the generated wrapper's own convention.
        var (r1, _, e1) = Syscall9(procTransmitFile.Addr(), 7, (uintptr)s, (uintptr)handle, (uintptr)bytesToWrite,
                                   (uintptr)bytsPerSend, (uintptr)native, (uintptr)(void*)buffers, (uintptr)flags, 0, 0);

        return r1 == 0 ? errnoErr(e1) : default!;
    }

}
