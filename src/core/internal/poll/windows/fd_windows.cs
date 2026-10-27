// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using race = go.@internal.race_package;
using windows = go.@internal.syscall.windows_package;
using io = io_package;
using Δruntime = runtime_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using Δsyscall = syscall_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @unsafe = unsafe_package;
using go;
using go.@internal;
using go.@internal.syscall;
using go.sync;
using unicode;

partial class poll_package {

internal static error initErr;
internal static ж<uint64> ᏑioSync = new StaticBox<uint64>(default(uint64));
internal static ref uint64 ioSync => ref ᏑioSync.Value;

// This package uses the SetFileCompletionNotificationModes Windows
// API to skip calling GetQueuedCompletionStatus if an IO operation
// completes synchronously. There is a known bug where
// SetFileCompletionNotificationModes crashes on some systems (see
// https://support.microsoft.com/kb/2568167 for details).
internal static bool socketCanUseSetFileCompletionNotificationModes; // determines is SetFileCompletionNotificationModes is present and sockets can safely use it

// checkSetFileCompletionNotificationModes verifies that
// SetFileCompletionNotificationModes Windows API is present
// on the system and is safe to use.
// See https://support.microsoft.com/kb/2568167 for details.
internal static void checkSetFileCompletionNotificationModes() {
    var err = Δsyscall.LoadSetFileCompletionNotificationModes();
    if (err != default!) {
        return;
    }
    ref var protos = ref heap<array<int32>>(out var Ꮡprotos);
    protos = new int32[]{Δsyscall.IPPROTO_TCP, 0}.array();
    ref var buf = ref heap(new array<Δsyscall.WSAProtocolInfo>(32, () => new()), out var Ꮡbuf);
    ref var len = ref heap<uint32>(out var Ꮡlen);
    len = (uint32)/* unsafe.Sizeof(buf) */ (uintptr)20096;
    (var n, err) = Δsyscall.WSAEnumProtocols(Ꮡprotos.at<int32>(0), Ꮡbuf.at<Δsyscall.WSAProtocolInfo>(0), Ꮡlen);
    if (err != default!) {
        return;
    }
    for (var i = (int32)0; i < n; i++) {
        if ((buf[i].ServiceFlags1 & (uint32)Δsyscall.XP1_IFS_HANDLES) == 0) {
            return;
        }
    }
    socketCanUseSetFileCompletionNotificationModes = true;
}

// InitWSA initiates the use of the Winsock DLL by the current process.
// It is called from the net package at init time to avoid
// loading ws2_32.dll when net is not used.
public static Action InitWSA = sync.OnceFunc(() => {
    ref var d = ref heap(new Δsyscall.WSAData(), out var Ꮡd);
    var e = Δsyscall.WSAStartup((uint32)0x202, Ꮡd);
    if (e != default!) {
        initErr = e;
    }
    checkSetFileCompletionNotificationModes();
});

// operation contains superset of data necessary to perform all async IO.
partial struct operation {
    // Used by IOCP interface, it must be first field
    // of the struct, as our code relies on it.
    internal Δsyscall.Overlapped o;
    // fields used by runtime.netpoll
    internal uintptr runtimeCtx;
    internal int32 mode;
}

internal static ж<Δsyscall.Overlapped> overlapped(this ref FD fd, ж<operation> Ꮡo) {
    if (fd.isBlocking) {
        // Don't return the overlapped object if the file handle
        // doesn't use overlapped I/O. It could be used, but
        // that would then use the file pointer stored in the
        // overlapped object rather than the real file pointer.
        return default!;
    }
    return Ꮡo.of(operation.Ꮡo);
}

internal static ж<Δsyscall.WSABuf> newWsaBuf(slice<byte> b) {
    return Ꮡ(new Δsyscall.WSABuf(Buf: @unsafe.SliceData(b), Len: (uint32)len(b)));
}

[GoLazy] partial class lazyᴛwsaBufsPool { internal static ж<sync.Pool> ᏑwsaBufsPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => {
        var buf = new slice<Δsyscall.WSABuf>(0, 16);
        return Ꮡ(buf);
    }
)); }
internal static ref sync.Pool wsaBufsPool => ref ᏑwsaBufsPool.Value;

internal static ж<slice<Δsyscall.WSABuf>> newWSABufs(ref slice<slice<byte>> buf) {
    var bufsPtr = ᏑwsaBufsPool.Get()._<ж<slice<Δsyscall.WSABuf>>>();
    bufsPtr.ValueSlot = (bufsPtr.ValueSlot)[..0];
    foreach (var (_, vᴛ1) in buf) {
        var b = vᴛ1;

        if (len(b) == 0) {
            bufsPtr.ValueSlot = append(bufsPtr.ValueSlot, new Δsyscall.WSABuf(nil));
            continue;
        }
        while (len(b) > maxRW) {
            bufsPtr.ValueSlot = append(bufsPtr.ValueSlot, new Δsyscall.WSABuf(Len: maxRW, Buf: Ꮡ(b, 0)));
            b = b[(int)(maxRW)..];
        }
        if (len(b) > 0) {
            bufsPtr.ValueSlot = append(bufsPtr.ValueSlot, new Δsyscall.WSABuf(Len: (uint32)len(b), Buf: Ꮡ(b, 0)));
        }
    }
    return bufsPtr;
}

internal static void freeWSABufs(ж<slice<Δsyscall.WSABuf>> ᏑbufsPtr) {
    ref var bufsPtr = ref ᏑbufsPtr.DerefOrNull();

    // Clear pointers to buffers so they can be released by garbage collector.
    var bufs = bufsPtr;
    foreach (var i in range(bufs)) {
        bufs[i].Buf = default!;
    }
    // Proper usage of a sync.Pool requires each entry to have approximately
    // the same memory cost. To obtain this property when the stored type
    // contains a variably-sized buffer, we add a hard limit on the maximum buffer
    // to place back in the pool.
    //
    // See https://go.dev/issue/23199
    if (cap(bufsPtr) > 128) {
        bufsPtr = default!;
    }
    ᏑwsaBufsPool.Put(ᏑbufsPtr.OrTypedNil());
}

// wsaMsgPool is a pool of WSAMsg structures that can only hold a single WSABuf.
[GoLazy] partial class lazyᴛwsaMsgPool { internal static ж<sync.Pool> ᏑwsaMsgPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => Ꮡ(new windows.WSAMsg(
            Buffers: Ꮡ(new Δsyscall.WSABuf(nil)),
            BufferCount: 1
        ))
)); }
internal static ref sync.Pool wsaMsgPool => ref ᏑwsaMsgPool.Value;

// newWSAMsg creates a new WSAMsg with the provided parameters.
// Use [freeWSAMsg] to free it.
internal static ж<windows.WSAMsg> newWSAMsg(slice<byte> p, slice<byte> oob, nint flags, ж<wsaRsa> Ꮡrsa) {
    ref var rsa = ref Ꮡrsa.DerefOrNull();

    // The returned object can't be allocated in the stack because it is accessed asynchronously
    // by Windows in between several system calls. If the stack frame is moved while that happens,
    // then Windows may access invalid memory.
    // TODO(qmuntal): investigate using runtime.Pinner keeping this path allocation-free.

    // Use a pool to reuse allocations.
    var msg = ᏑwsaMsgPool.Get()._<ж<windows.WSAMsg>>();
    msg.Value.Buffers.Value.Len = (uint32)len(p);
    msg.Value.Buffers.Value.Buf = @unsafe.SliceData(p);
    if (len(oob) > 0) {
        msg.Value.Control = new Δsyscall.WSABuf(
            Len: (uint32)len(oob),
            Buf: @unsafe.SliceData(oob)
        );
    }
    msg.Value.Flags = (uint32)flags;
    if (Ꮡrsa != nil) {
        msg.Value.Name = Ꮡrsa.of(wsaRsa.Ꮡname);
        msg.Value.Namelen = rsa.namelen;
    }
    return msg;
}

internal static void freeWSAMsg(ж<windows.WSAMsg> Ꮡmsg) {
    ref var msg = ref Ꮡmsg.DerefOrNull();

    // Clear pointers to buffers so they can be released by garbage collector.
    msg.Name = default!;
    msg.Namelen = 0;
    msg.Buffers.Value.Len = 0;
    msg.Buffers.Value.Buf = default!;
    msg.Control.Len = 0;
    msg.Control.Buf = default!;
    ᏑwsaMsgPool.Put(Ꮡmsg.OrTypedNil());
}

// wsaRsa bundles a [syscall.RawSockaddrAny] with its length for efficient caching.
//
// When used by WSARecvFrom, wsaRsa must be on the heap. See
// https://learn.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-wsarecvfrom.
partial struct wsaRsa {
    internal Δsyscall.RawSockaddrAny name;
    internal int32 namelen;
}

internal static ж<sync.Pool> ᏑwsaRsaPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => @new<wsaRsa>()
));
internal static ref sync.Pool wsaRsaPool => ref ᏑwsaRsaPool.Value;

internal static ж<wsaRsa> newWSARsa() {
    var rsa = ᏑwsaRsaPool.Get()._<ж<wsaRsa>>();
    rsa.Value.name = new Δsyscall.RawSockaddrAny(nil);
    rsa.Value.namelen = (int32)/* unsafe.Sizeof(syscall.RawSockaddrAny{}) */ (uintptr)116;
    return rsa;
}

internal static ж<sync.Pool> ᏑoperationPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => @new<operation>()
));
internal static ref sync.Pool operationPool => ref ᏑoperationPool.Value;

// waitIO waits for the IO operation o to complete.
internal static error waitIO(this ж<FD> Ꮡfd, ж<operation> Ꮡo) {
    ref var fd = ref Ꮡfd.DerefOrNull();
    ref var o = ref Ꮡo.DerefOrNull();

    if (fd.isBlocking) {
        throw panic("can't wait on blocking operations");
    }
    if (!Ꮡfd.pollable()) {
        // The overlapped handle is not added to the runtime poller,
        // the only way to wait for the IO to complete is block until
        // the overlapped event is signaled.
        var (_, errΔ1) = Δsyscall.WaitForSingleObject(o.o.HEvent, Δsyscall.INFINITE);
        return errΔ1;
    }
    // Wait for our request to complete.
    var err = fd.pd.wait((nint)o.mode, fd.isFile);
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, default!) || AreEqual(exprᴛ1, ErrNetClosing) || AreEqual(exprᴛ1, ErrFileClosing) || AreEqual(exprᴛ1, ErrDeadlineExceeded)) {
        // No other error is expected.
    }
    else { /* default: */
        throw panic("unexpected runtime.netpoll error: " + err.Error());
    }
    return err;
}

// cancelIO cancels the IO operation o and waits for it to complete.
internal static void cancelIO(this ж<FD> Ꮡfd, ж<operation> Ꮡo) {
    ref var fd = ref Ꮡfd.DerefOrNull();
    ref var o = ref Ꮡo.DerefOrNull();

    if (!Ꮡfd.pollable()) {
        return;
    }
    // Cancel our request.
    var err = Δsyscall.CancelIoEx(fd.Sysfd, Ꮡo.of(operation.Ꮡo));
    // Assuming ERROR_NOT_FOUND is returned, if IO is completed.
    if (err != default! && !AreEqual(err, Δsyscall.ERROR_NOT_FOUND)) {
        // TODO(brainman): maybe do something else, but panic.
        throw panic(err);
    }
    fd.pd.waitCanceled((nint)o.mode);
}

// pin pins ptr for the duration of the IO operation.
// If fd is in blocking mode, pin does nothing.
internal static void pin(this ref FD fd, nint mode, any ptr) {
    if (fd.isBlocking) {
        return;
    }
    if (mode == (rune)'r'){
        fd.readPinner.Pin(ptr);
    } else {
        fd.writePinner.Pin(ptr);
    }
}

// execIO executes a single IO operation o.
// It supports both synchronous and asynchronous IO.
internal static (nint, error) execIO(this ж<FD> Ꮡfd, nint mode, Func<ж<operation>, (uint32, error)> submit) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (mode == (rune)'r'){
            defer(Ꮡfd.of(FD.ᏑreadPinner).Unpin, ref ᒐ);
        } else {
            defer(Ꮡfd.of(FD.ᏑwritePinner).Unpin, ref ᒐ);
        }
        // Notify runtime netpoll about starting IO.
        var err = fd.pd.prepare(mode, fd.isFile);
        if (err != default!) {
            return (0, err);
        }
        var o = ᏑoperationPool.Get()._<ж<operation>>();
        defer(ᏑoperationPool.Put, o.OrTypedNil(), ref ᒐ);
        o.Value = new operation(
            o: new Δsyscall.Overlapped(
                OffsetHigh: (uint32)(fd.offset >> 32),
                Offset: (uint32)fd.offset
            ),
            runtimeCtx: fd.pd.runtimeCtx,
            mode: (int32)mode
        );
        // Start IO.
        if (!fd.isBlocking && !Ꮡfd.pollable()) {
            // If the handle is opened for overlapped IO but we can't
            // use the runtime poller, then we need to use an
            // event to wait for the IO to complete.
            var (h, errΔ1) = windows.CreateEvent(nil, 0, 0, nil);
            if (errΔ1 != default!) {
                // This shouldn't happen when all CreateEvent arguments are zero.
                throw panic(errΔ1);
            }
            // Set the low bit so that the external IOCP doesn't receive the completion packet.
            o.Value.o.HEvent = (syscallꓸHandle)(h | 1);
            defer(Δsyscall.CloseHandle, h, ref ᒐ);
        }
        fd.pin(mode, o.OrTypedNil());
        ref var qty = ref heap<uint32>(out var Ꮡqty);
        (qty, err) = submit(o);
        error waitErr = default!;
        // Blocking operations shouldn't return ERROR_IO_PENDING.
        // Continue without waiting if that happens.
        if (!fd.isBlocking && (AreEqual(err, Δsyscall.ERROR_IO_PENDING) || (err == default! && !fd.skipSyncNotif))) {
            // IO started asynchronously or completed synchronously but
            // a sync notification is required. Wait for it to complete.
            waitErr = Ꮡfd.waitIO(o);
            if (waitErr != default!) {
                // IO interrupted by "close" or "timeout".
                Ꮡfd.cancelIO(o);
                // We issued a cancellation request, but the IO operation may still succeeded
                // before the cancellation request runs.
            }
            if (fd.isFile){
                err = windows.GetOverlappedResult(fd.Sysfd, o.of(operation.Ꮡo), Ꮡqty, false);
            } else {
                ref var flags = ref heap(new uint32(), out var Ꮡflags);
                err = windows.WSAGetOverlappedResult(fd.Sysfd, o.of(operation.Ꮡo), Ꮡqty, false, Ꮡflags);
            }
        }
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, Δsyscall.ERROR_OPERATION_ABORTED)) {
            // ERROR_OPERATION_ABORTED may have been caused by us. In that case,
            // map it to our own error. Don't do more than that, each submitted
            // function may have its own meaning for each error.
            if (waitErr != default!){
                // IO canceled by the poller while waiting for completion.
                err = waitErr;
            } else 
            if (fd.kind == kindPipe && Ꮡfd.closing()) {
                // Close uses CancelIoEx to interrupt concurrent I/O for pipes.
                // If the fd is a pipe and the Write was interrupted by CancelIoEx,
                // we assume it is interrupted by Close.
                err = errClosing(fd.isFile);
            }
        }
        else if (AreEqual(exprᴛ1, windows.ERROR_IO_INCOMPLETE)) {
            // waitIO couldn't wait for the IO to complete.
            if (waitErr != default!) {
                // The wait error will be more informative.
                err = waitErr;
            }
        }
        return ((nint)qty, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// FD is a file descriptor. The net and os packages embed this type in
// a larger type representing a network connection or OS file.
partial struct FD {
    // Lock sysfd and serialize access to Read and Write methods.
    internal fdMutex fdmu;
    // System file descriptor. Immutable until Close.
    public syscallꓸHandle Sysfd;
    // I/O poller.
    internal pollDesc pd;
    // The file offset for the next read or write.
    // Overlapped IO operations don't use the real file pointer,
    // so we need to keep track of the offset ourselves.
    internal int64 offset;
    // For console I/O.
    internal slice<byte> lastbits; // first few bytes of the last incomplete rune in last write
    internal slice<uint16> readuint16; // buffer to hold uint16s obtained with ReadConsole
    internal slice<byte> readbyte; // buffer to hold decoding of readuint16 from utf16 to utf8
    internal nint readbyteOffset;     // readbyte[readOffset:] is yet to be consumed with file.Read
    // Semaphore signaled when file is closed.
    internal uint32 csema;
    internal bool skipSyncNotif;
    // Whether this is a streaming descriptor, as opposed to a
    // packet-based descriptor like a UDP socket.
    public bool IsStream;
    // Whether a zero byte read indicates EOF. This is false for a
    // message based socket connection.
    public bool ZeroReadIsEOF;
    // KeepFileCompletionModes prevents Init from changing the file object's
    // completion notification modes.
    public bool KeepFileCompletionModes;
    // Whether the handle is owned by os.File.
    internal bool isFile;
    // The kind of this file.
    internal fileKind kind;
    // Whether FILE_FLAG_OVERLAPPED was not set when opening the file.
    internal bool isBlocking;
    internal atomic.Bool disassociated;
    // readPinner and writePinner are automatically unpinned
    // before execIO returns.
    internal Δruntime.Pinner readPinner;
    internal Δruntime.Pinner writePinner;
}

// setOffset sets the offset fields of the overlapped object
// to the given offset. The fd read/write lock must be held.
//
// Overlapped IO operations don't update the offset fields
// of the overlapped object nor the file pointer automatically,
// so we do that manually here.
// Note that this is a best effort that only works if the file
// pointer is completely owned by this operation. We could
// call seek to allow other processes or other operations on the
// same file to see the updated offset. That would be inefficient
// and won't work for concurrent operations anyway. If concurrent
// operations are needed, then the caller should serialize them
// using an external mechanism.
internal static void setOffset(this ref FD fd, int64 off) {
    fd.offset = off;
}

// addOffset adds the given offset to the current offset.
internal static void addOffset(this ref FD fd, nint off) {
    fd.setOffset(fd.offset + (int64)off);
}

// pollable should be used instead of fd.pd.pollable(),
// as it is aware of the disassociated state.
internal static bool pollable(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    return fd.pd.pollable() && !Ꮡfd.of(FD.Ꮡdisassociated).Load();
}

// fileKind describes the kind of file.
partial struct fileKind /*num:byte*/;

internal static fileKind kindNet => /* iota */ 0;
internal static fileKind kindFile => 1;
internal static fileKind kindConsole => 2;
internal static fileKind kindPipe => 3;
internal static fileKind kindFileNet => 4;

// Init initializes the FD. The Sysfd field should already be set.
// This can be called multiple times on a single FD.
// The net argument is a network name from the net package (e.g., "tcp"),
// or "file" or "console" or "dir".
// Set pollable to true if fd should be managed by runtime netpoll.
// Pollable must be set to true for overlapped fds.
public static error Init(this ж<FD> Ꮡfd, @string net, bool pollable) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (initErr != default!) {
        return initErr;
    }

    var exprᴛ1 = net;
    if (exprᴛ1 == "file"u8) {
        fd.kind = kindFile;
    }
    else if (exprᴛ1 == "console"u8) {
        fd.kind = kindConsole;
    }
    else if (exprᴛ1 == "pipe"u8) {
        fd.kind = kindPipe;
    }
    else if (exprᴛ1 == "file+net"u8) {
        fd.kind = kindFileNet;
    }
    else { /* default: */
        // We don't actually care about the various network types.
        fd.kind = kindNet;
    }
    fd.isFile = fd.kind != kindNet;
    fd.isBlocking = !pollable;

    if (!pollable) {
        return default!;
    }

    if (fd.KeepFileCompletionModes) {
        // Query the existing skip-success mode so we don't wait for a
        // suppressed completion or skip waiting for an expected one.
        ref var info = ref heap(new windows.FILE_IO_COMPLETION_NOTIFICATION_INFORMATION(), out var Ꮡinfo);
        {
            var errΔ1 = windows.NtQueryInformationFile(fd.Sysfd, Ꮡ(new windows.IO_STATUS_BLOCK(nil)),
                @unsafe.Pointer.FromPinnedBox(Ꮡinfo), (uint32)/* unsafe.Sizeof(info) */ (uintptr)4, windows.FileIoCompletionNotificationInformation); if (errΔ1 != default!) {
                // Without knowing the modes, neither waiting for a completion
                // packet on success nor skipping it is safe. Leave the handle
                // unassociated and use explicit events for pending I/O instead.
                // Inline success needs no wait, and deadlines are unavailable.
                fd.skipSyncNotif = true;
                return default!;
            }
        }
        fd.skipSyncNotif = (info.Flags & (uint32)Δsyscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS) != 0;
    }

    // It is safe to add overlapped handles that also perform I/O
    // outside of the runtime poller. The runtime poller will ignore
    // I/O completion notifications not initiated by us.
    var err = fd.pd.init(Ꮡfd);
    if (err != default!) {
        return err;
    }
    if (!fd.KeepFileCompletionModes && (fd.kind != kindNet || socketCanUseSetFileCompletionNotificationModes)) {
        // Non-socket handles can use SetFileCompletionNotificationModes without problems.
        var errΔ2 = Δsyscall.SetFileCompletionNotificationModes(fd.Sysfd,
            (uint8)((uint8)Δsyscall.FILE_SKIP_SET_EVENT_ON_HANDLE | (uint8)Δsyscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS));
        fd.skipSyncNotif = errΔ2 == default!;
    }
    return default!;
}

// go2cs generated this placeholder — func DisassociateIOCP is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static error destroy(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (fd.Sysfd == Δsyscall.InvalidHandle) {
        return Δsyscall.EINVAL;
    }
    // Poller may want to unregister fd in readiness notification mechanism,
    // so this must be executed before fd.CloseFunc.
    fd.pd.close();
    error err = default!;
    var exprᴛ1 = fd.kind;
    if (exprᴛ1 == kindNet || exprᴛ1 == kindFileNet) {
        // The net package uses the CloseFunc variable for testing.
        err = CloseFunc(fd.Sysfd);
    }
    else { /* default: */
        err = Δsyscall.CloseHandle(fd.Sysfd);
    }
    fd.Sysfd = Δsyscall.InvalidHandle;
    runtime_Semrelease(Ꮡfd.of(FD.Ꮡcsema));
    return err;
}

// Close closes the FD. The underlying file descriptor is closed by
// the destroy method when there are no remaining references.
public static error Close(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    if (!fd.fdmu.increfAndClose()) {
        return errClosing(fd.isFile);
    }

    if (fd.kind == kindPipe) {
        Δsyscall.CancelIoEx(fd.Sysfd, nil);
    }
    // unblock pending reader and writer
    fd.pd.evict();
    var err = Ꮡfd.decref();
    // Wait until the descriptor is closed. If this was the only
    // reference, it is already closed.
    runtime_Semacquire(Ꮡfd.of(FD.Ꮡcsema));
    return err;
}

// Windows ReadFile and WSARecv use DWORD (uint32) parameter to pass buffer length.
// This prevents us reading blocks larger than 4GB.
// See golang.org/issue/26923.
internal static UntypedInt maxRW => /* 1 << 30 */ 1073741824; // 1GB is large enough and keeps subsequent reads aligned

// Read implements io.Reader.
public static (nint, error) Read(this ж<FD> Ꮡfd, slice<byte> buf) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (fd.kind == kindFile){
            {
                var errΔ1 = Ꮡfd.readWriteLock(); if (errΔ1 != default!) {
                    return (0, errΔ1);
                }
            }
            ᒐd1 = true;
        } else {
            {
                var errΔ2 = Ꮡfd.readLock(); if (errΔ2 != default!) {
                    return (0, errΔ2);
                }
            }
            ᒐd2 = true;
        }

        if (len(buf) > 0) {
            fd.pin((rune)'r', Ꮡ(buf, 0));
        }

        if (len(buf) > maxRW) {
            buf = buf[..(int)(maxRW)];
        }

        nint n = default!;
        error err = default!;
        var exprᴛ1 = fd.kind;
        if (exprᴛ1 == kindConsole) {
            (n, err) = fd.readConsole(buf);
        }
        else if (exprᴛ1 == kindFile || exprᴛ1 == kindPipe) {
            var bufʗ1 = buf;
            (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ5 = default!;
                errΔ5 = Δsyscall.ReadFile(Ꮡfd.Value.Sysfd, bufʗ1, Ꮡqty, Ꮡfd.Value.overlapped(o));
                return (Ꮡqty.Value, errΔ5);
            });
            fd.addOffset(n);
            var exprᴛ2 = err;
            if (AreEqual(exprᴛ2, Δsyscall.ERROR_HANDLE_EOF)) {
                err = io.EOF;
            }
            else if (AreEqual(exprᴛ2, Δsyscall.ERROR_BROKEN_PIPE)) {
                // ReadFile only documents ERROR_BROKEN_PIPE for pipes.
                if (fd.kind == kindPipe) {
                    err = io.EOF;
                }
            }
        }
        else if (exprᴛ1 == kindNet) {
            var bufʗ2 = buf;
            (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ6 = default!;
                ref var flags = ref heap(new uint32(), out var Ꮡflags);
                errΔ6 = Δsyscall.WSARecv(Ꮡfd.Value.Sysfd, newWsaBuf(bufʗ2), 1, Ꮡqty, Ꮡflags, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ6);
            });
            if (race.Enabled) {
                race.Acquire(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
            }
        }
        if (len(buf) != 0) {
            err = fd.eofError(n, err);
        }
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd2) Ꮡfd.readUnlock(); if (ᒐd1) Ꮡfd.readWriteUnlock(); ᒐ.Run(); }
}

public static Func<syscallꓸHandle, ж<uint16>, uint32, ж<uint32>, ж<byte>, error> ReadConsole = Δsyscall.ReadConsole;                                                         // changed for testing

// readConsole reads utf16 characters from console File,
// encodes them into utf8 and stores them in buffer b.
// It returns the number of utf8 bytes read and an error, if any.
internal static (nint, error) readConsole(this ref FD fd, slice<byte> b) {
    if (len(b) == 0) {
        return (0, default!);
    }

    if (fd.readuint16 == default!) {
        // Note: syscall.ReadConsole fails for very large buffers.
        // The limit is somewhere around (but not exactly) 16384.
        // Stay well below.
        fd.readuint16 = new slice<uint16>(0, 10000);
        fd.readbyte = new slice<byte>(0, 4 * cap(fd.readuint16));
    }

    while (fd.readbyteOffset >= len(fd.readbyte)) {
        nint n = cap(fd.readuint16) - len(fd.readuint16);
        if (n > len(b)) {
            n = len(b);
        }
        ref var nw = ref heap(new uint32(), out var Ꮡnw);
        var err = ReadConsole(fd.Sysfd, Ꮡ(fd.readuint16.slice(0, len(fd.readuint16) + 1), len(fd.readuint16)), (uint32)n, Ꮡnw, nil);
        if (err != default!) {
            return (0, err);
        }
        var uint16s = fd.readuint16.slice(0, len(fd.readuint16) + (nint)nw);
        fd.readuint16 = fd.readuint16[..0];
        var buf = fd.readbyte[..0];
        for (nint iΔ1 = 0; iΔ1 < len(uint16s); iΔ1++) {
            var r = (rune)uint16s[iΔ1];
            if (utf16.IsSurrogate(r)) {
                if (iΔ1 + 1 == len(uint16s)){
                    if (nw > 0) {
                        // Save half surrogate pair for next time.
                        fd.readuint16 = fd.readuint16[..1];
                        fd.readuint16[0] = (uint16)r;
                        break;
                    }
                    r = utf8.RuneError;
                } else {
                    r = utf16.DecodeRune(r, (rune)uint16s[iΔ1 + 1]);
                    if (r != utf8.RuneError) {
                        iΔ1++;
                    }
                }
            }
            buf = utf8.AppendRune(buf, r);
        }
        fd.readbyte = buf;
        fd.readbyteOffset = 0;
        if (nw == 0) {
            break;
        }
    }

    var src = fd.readbyte.slice(fd.readbyteOffset);
    nint i = default!;
    for (i = 0; i < len(src) && i < len(b); i++) {
        var x = src[i];
        if (x == 0x1A) { // Ctrl-Z
            if (i == 0) {
                fd.readbyteOffset++;
            }
            break;
        }
        b[i] = x;
    }
    fd.readbyteOffset += i;
    return (i, default!);
}

// Pread emulates the Unix pread system call.
public static (nint, error) Pread(this ж<FD> Ꮡfd, slice<byte> buf, int64 off) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (fd.kind == kindPipe) {
            // Pread does not work with pipes
            return (0, Δsyscall.ESPIPE);
        }

        {
            var errΔ1 = Ꮡfd.readWriteLock(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        defer(Ꮡfd.readWriteUnlock, ref ᒐ);

        if (len(buf) > 0) {
            fd.pin((rune)'r', Ꮡ(buf, 0));
        }

        if (len(buf) > maxRW) {
            buf = buf[..(int)(maxRW)];
        }

        if (fd.isBlocking){
            var (curoffset, errΔ2) = Δsyscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
            if (errΔ2 != default!) {
                return (0, errΔ2);
            }
            defer(Δsyscall.Seek, Ꮡfd.Value.Sysfd, curoffset, (nint)(io.SeekStart), ref ᒐ);
            defer(Ꮡfd.setOffset, curoffset, ref ᒐ);
        } else {
            // Overlapped handles don't have the file pointer updated
            // when performing I/O operations, so there is no need to
            // call Seek to reset the file pointer.
            // Also, some overlapped file handles don't support seeking.
            // See https://go.dev/issues/74951.
            var curoffset = fd.offset;
            defer(Ꮡfd.setOffset, curoffset, ref ᒐ);
        }
        fd.setOffset(off);
        var bufʗ1 = buf;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ3 = default!;
            errΔ3 = Δsyscall.ReadFile(Ꮡfd.Value.Sysfd, bufʗ1, Ꮡqty, o.of(operation.Ꮡo));
            return (Ꮡqty.Value, errΔ3);
        });
        if (AreEqual(err, Δsyscall.ERROR_HANDLE_EOF)) {
            err = io.EOF;
        }
        if (len(buf) != 0) {
            err = fd.eofError(n, err);
        }
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ReadFrom wraps the recvfrom network call.
public static (nint, syscallꓸSockaddr, error) ReadFrom(this ж<FD> Ꮡfd, slice<byte> buf) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (len(buf) == 0) {
            return (0, default!, default!);
        }
        if (len(buf) > maxRW) {
            buf = buf[..(int)(maxRW)];
        }
        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, default!, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        fd.pin((rune)'r', Ꮡ(buf, 0));

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var bufʗ1 = buf;
        var rsaʗ1 = rsa;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            ref var flags = ref heap(new uint32(), out var Ꮡflags);
            errΔ2 = Δsyscall.WSARecvFrom(Ꮡfd.Value.Sysfd, newWsaBuf(bufʗ1), 1, Ꮡqty, Ꮡflags, rsaʗ1.of(wsaRsa.Ꮡname), rsaʗ1.of(wsaRsa.Ꮡnamelen), o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        if (err != default!) {
            return (n, default!, err);
        }
        var (sa, _) = rsa.of(wsaRsa.Ꮡname).Sockaddr();
        return (n, sa, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ReadFromInet4 wraps the recvfrom network call for IPv4.
public static (nint, error) ReadFromInet4(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet4> Ꮡsa4) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (len(buf) == 0) {
            return (0, default!);
        }
        if (len(buf) > maxRW) {
            buf = buf[..(int)(maxRW)];
        }
        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        fd.pin((rune)'r', Ꮡ(buf, 0));

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var bufʗ1 = buf;
        var rsaʗ1 = rsa;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            ref var flags = ref heap(new uint32(), out var Ꮡflags);
            errΔ2 = Δsyscall.WSARecvFrom(Ꮡfd.Value.Sysfd, newWsaBuf(bufʗ1), 1, Ꮡqty, Ꮡflags, rsaʗ1.of(wsaRsa.Ꮡname), rsaʗ1.of(wsaRsa.Ꮡnamelen), o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        if (err != default!) {
            return (n, err);
        }
        rawToSockaddrInet4(rsa.of(wsaRsa.Ꮡname), ref (Ꮡsa4).DerefOrNull());
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ReadFromInet6 wraps the recvfrom network call for IPv6.
public static (nint, error) ReadFromInet6(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet6> Ꮡsa6) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (len(buf) == 0) {
            return (0, default!);
        }
        if (len(buf) > maxRW) {
            buf = buf[..(int)(maxRW)];
        }
        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        fd.pin((rune)'r', Ꮡ(buf, 0));

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var bufʗ1 = buf;
        var rsaʗ1 = rsa;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            ref var flags = ref heap(new uint32(), out var Ꮡflags);
            errΔ2 = Δsyscall.WSARecvFrom(Ꮡfd.Value.Sysfd, newWsaBuf(bufʗ1), 1, Ꮡqty, Ꮡflags, rsaʗ1.of(wsaRsa.Ꮡname), rsaʗ1.of(wsaRsa.Ꮡnamelen), o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        if (err != default!) {
            return (n, err);
        }
        rawToSockaddrInet6(rsa.of(wsaRsa.Ꮡname), ref (Ꮡsa6).DerefOrNull());
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Write implements io.Writer.
public static (nint, error) Write(this ж<FD> Ꮡfd, slice<byte> buf) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    bool ᒐd2 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (fd.kind == kindFile){
            {
                var err = Ꮡfd.readWriteLock(); if (err != default!) {
                    return (0, err);
                }
            }
            ᒐd1 = true;
        } else {
            {
                var err = Ꮡfd.writeLock(); if (err != default!) {
                    return (0, err);
                }
            }
            ᒐd2 = true;
        }

        if (len(buf) > 0) {
            fd.pin((rune)'w', Ꮡ(buf, 0));
        }
        nint ntotal = default!;
        while (ᐧ) {
            nint max = len(buf);
            if (max - ntotal > maxRW) {
                max = ntotal + (nint)maxRW;
            }
            var b = buf.slice(ntotal, max);
            nint n = default!;
            error err = default!;
            var exprᴛ1 = fd.kind;
            if (exprᴛ1 == kindConsole) {
                (n, err) = fd.writeConsole(b);
            }
            else if (exprᴛ1 == kindPipe || exprᴛ1 == kindFile) {
                var bʗ1 = b;
                (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                    ref var qty = ref heap(new uint32(), out var Ꮡqty);
                    error errΔ3 = default!;
                    errΔ3 = Δsyscall.WriteFile(Ꮡfd.Value.Sysfd, bʗ1, Ꮡqty, Ꮡfd.Value.overlapped(o));
                    return (Ꮡqty.Value, errΔ3);
                });
                fd.addOffset(n);
            }
            else if (exprᴛ1 == kindNet) {
                if (race.Enabled) {
                    race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
                }
                var bʗ2 = b;
                (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                    ref var qty = ref heap(new uint32(), out var Ꮡqty);
                    error errΔ4 = default!;
                    errΔ4 = Δsyscall.WSASend(Ꮡfd.Value.Sysfd, newWsaBuf(bʗ2), 1, Ꮡqty, 0, o.of(operation.Ꮡo), nil);
                    return (Ꮡqty.Value, errΔ4);
                });
            }
            ntotal += n;
            if (ntotal == len(buf) || err != default!) {
                return (ntotal, err);
            }
            if (n == 0) {
                return (ntotal, io.ErrUnexpectedEOF);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd2) Ꮡfd.writeUnlock(); if (ᒐd1) Ꮡfd.readWriteUnlock(); ᒐ.Run(); }
}

// writeConsole writes len(b) bytes to the console File.
// It returns the number of bytes written and an error, if any.
internal static (nint, error) writeConsole(this ref FD fd, slice<byte> b) {
    var runesᴷ = new stackAppend<rune>();

    nint n = len(b);
    var runes = new slice<rune>(0, 256);
    if (len(fd.lastbits) > 0) {
        b = appendꓸꓸꓸ(fd.lastbits, b);
        fd.lastbits = default!;
    }
    while (len(b) >= utf8.UTFMax || utf8.FullRune(b)) {
        var (r, l) = utf8.DecodeRune(b);
        runes = append(runesᴷ, runes, r);
        b = b.slice(l);
    }
    if (len(b) > 0) {
        fd.lastbits = new slice<byte>(len(b));
        copy(fd.lastbits, b);
    }
    // syscall.WriteConsole seems to fail, if given large buffer.
    // So limit the buffer to 16000 characters. This number was
    // discovered by experimenting with syscall.WriteConsole.
    const nint maxWrite = 16000;
    while (len(runes) > 0) {
        nint m = len(runes);
        if (m > maxWrite) {
            m = maxWrite;
        }
        var chunk = runes.slice(0, m);
        runes = runes.slice(m);
        var uint16s = utf16.Encode(chunk);
        while (len(uint16s) > 0) {
            ref var written = ref heap(new uint32(), out var Ꮡwritten);
            var err = Δsyscall.WriteConsole(fd.Sysfd, Ꮡ(uint16s, 0), (uint32)len(uint16s), Ꮡwritten, nil);
            if (err != default!) {
                return (0, err);
            }
            uint16s = uint16s.slice((nint)(written));
        }
    }
    return (n, default!);
}

// Pwrite emulates the Unix pwrite system call.
public static (nint, error) Pwrite(this ж<FD> Ꮡfd, slice<byte> buf, int64 off) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (fd.kind == kindPipe) {
            // Pwrite does not work with pipes
            return (0, Δsyscall.ESPIPE);
        }

        {
            var err = Ꮡfd.readWriteLock(); if (err != default!) {
                return (0, err);
            }
        }
        defer(Ꮡfd.readWriteUnlock, ref ᒐ);

        if (len(buf) > 0) {
            fd.pin((rune)'w', Ꮡ(buf, 0));
        }

        if (fd.isBlocking){
            var (curoffset, err) = Δsyscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
            if (err != default!) {
                return (0, err);
            }
            defer(Δsyscall.Seek, Ꮡfd.Value.Sysfd, curoffset, (nint)(io.SeekStart), ref ᒐ);
            defer(Ꮡfd.setOffset, curoffset, ref ᒐ);
        } else {
            // Overlapped handles don't have the file pointer updated
            // when performing I/O operations, so there is no need to
            // call Seek to reset the file pointer.
            // Also, some overlapped file handles don't support seeking.
            // See https://go.dev/issues/74951.
            var curoffset = fd.offset;
            defer(Ꮡfd.setOffset, curoffset, ref ᒐ);
        }

        nint ntotal = default!;
        while (ᐧ) {
            nint max = len(buf);
            if (max - ntotal > maxRW) {
                max = ntotal + (nint)maxRW;
            }
            fd.setOffset(off + (int64)ntotal);
            var bufʗ1 = buf;
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = Δsyscall.WriteFile(Ꮡfd.Value.Sysfd, bufʗ1.slice(ntotal, max), Ꮡqty, o.of(operation.Ꮡo));
                return (Ꮡqty.Value, errΔ1);
            });
            if (n > 0) {
                ntotal += n;
            }
            if (ntotal == len(buf) || err != default!) {
                return (ntotal, err);
            }
            if (n == 0) {
                return (ntotal, io.ErrUnexpectedEOF);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Writev emulates the Unix writev system call.
public static (int64, error) Writev(this ж<FD> Ꮡfd, ж<slice<slice<byte>>> Ꮡbuf) {
    GoFrame ᒐ = default;
    try {
        ref var buf = ref Ꮡbuf.DerefOrNull();

        if (len(buf) == 0) {
            return (0, default!);
        }
        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);
        if (race.Enabled) {
            race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(ᏑioSync));
        }
        var bufs = newWSABufs(ref (Ꮡbuf).DerefOrNull());
        defer(freeWSABufs, bufs, ref ᒐ);
        var bufsʗ1 = bufs;
        var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            errΔ2 = Δsyscall.WSASend(Ꮡfd.Value.Sysfd, Ꮡ((bufsʗ1.ValueSlot), 0), (uint32)len(bufsʗ1.ValueSlot), Ꮡqty, 0, o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        TestHookDidWritev(n);
        consume(ref (Ꮡbuf).DerefOrNull(), (int64)n);
        return ((int64)n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// WriteTo wraps the sendto network call.
public static (nint, error) WriteTo(this ж<FD> Ꮡfd, slice<byte> buf, syscallꓸSockaddr sa) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;

        if (len(buf) == 0) {
            // handle zero-byte payload
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = Δsyscall.WSASendto(Ꮡfd.Value.Sysfd, Ꮡ(new Δsyscall.WSABuf(nil)), 1, Ꮡqty, 0, sa, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            return (n, err);
        }

        fd.pin((rune)'w', Ꮡ(buf, 0));

        nint ntotal = 0;
        while (len(buf) > 0) {
            ref var b = ref heap<slice<byte>>(out var Ꮡb);
            b = buf;
            if (len(b) > maxRW) {
                b = b[..(int)(maxRW)];
            }
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = Δsyscall.WSASendto(Ꮡfd.Value.Sysfd, newWsaBuf(Ꮡb.ValueSlot), 1, Ꮡqty, 0, sa, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            ntotal += (nint)n;
            if (err != default!) {
                return (ntotal, err);
            }
            buf = buf.slice(n);
        }
        return (ntotal, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteToInet4 is WriteTo, specialized for syscall.SockaddrInet4.
public static (nint, error) WriteToInet4(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet4> Ꮡsa4) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;

        if (len(buf) == 0) {
            // handle zero-byte payload
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = windows.WSASendtoInet4(Ꮡfd.Value.Sysfd, Ꮡ(new Δsyscall.WSABuf(nil)), 1, Ꮡqty, 0, Ꮡsa4, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            return (n, err);
        }

        fd.pin((rune)'w', Ꮡ(buf, 0));

        nint ntotal = 0;
        while (len(buf) > 0) {
            ref var b = ref heap<slice<byte>>(out var Ꮡb);
            b = buf;
            if (len(b) > maxRW) {
                b = b[..(int)(maxRW)];
            }
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = windows.WSASendtoInet4(Ꮡfd.Value.Sysfd, newWsaBuf(Ꮡb.ValueSlot), 1, Ꮡqty, 0, Ꮡsa4, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            ntotal += (nint)n;
            if (err != default!) {
                return (ntotal, err);
            }
            buf = buf.slice(n);
        }
        return (ntotal, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// WriteToInet6 is WriteTo, specialized for syscall.SockaddrInet6.
public static (nint, error) WriteToInet6(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet6> Ꮡsa6) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;

        if (len(buf) == 0) {
            // handle zero-byte payload
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = windows.WSASendtoInet6(Ꮡfd.Value.Sysfd, Ꮡ(new Δsyscall.WSABuf(nil)), 1, Ꮡqty, 0, Ꮡsa6, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            return (n, err);
        }

        fd.pin((rune)'w', Ꮡ(buf, 0));

        nint ntotal = 0;
        while (len(buf) > 0) {
            ref var b = ref heap<slice<byte>>(out var Ꮡb);
            b = buf;
            if (len(b) > maxRW) {
                b = b[..(int)(maxRW)];
            }
            var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                errΔ1 = windows.WSASendtoInet6(Ꮡfd.Value.Sysfd, newWsaBuf(Ꮡb.ValueSlot), 1, Ꮡqty, 0, Ꮡsa6, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            ntotal += (nint)n;
            if (err != default!) {
                return (ntotal, err);
            }
            buf = buf.slice(n);
        }
        return (ntotal, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// Call ConnectEx. This doesn't need any locking, since it is only
// called when the descriptor is first created. This is here rather
// than in the net package so that it can use fd.wop.
public static error ConnectEx(this ж<FD> Ꮡfd, syscallꓸSockaddr ra) {
    var (_, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => ((uint32)(0), ConnectExFunc(Ꮡfd.Value.Sysfd, ra, nil, 0, nil, o.of(operation.Ꮡo))));
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string acceptexˢ = "acceptex"u8;
internal static readonly @string setsockoptˢ = "setsockopt"u8;

internal static (@string, error) acceptOne(this ж<FD> Ꮡfd, syscallꓸHandle s, slice<Δsyscall.RawSockaddrAny> rawsa) {
    // Submit accept request.
    var rsan = (uint32)/* unsafe.Sizeof(rawsa[0]) */ (uintptr)116;
    var rawsaʗ1 = rawsa;
    var (_, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
        ref var qty = ref heap(new uint32(), out var Ꮡqty);
        error errΔ1 = default!;
        errΔ1 = AcceptFunc(Ꮡfd.Value.Sysfd, s, Ꮡ(rawsaʗ1, 0).Reinterpret<Δsyscall.RawSockaddrAny, byte>(), 0, rsan, rsan, Ꮡqty, o.of(operation.Ꮡo));
        return (Ꮡqty.Value, errΔ1);
    });
    if (err != default!) {
        CloseFunc(s);
        return (acceptexˢ, err);
    }

    // Inherit properties of the listening socket.
    err = Δsyscall.Setsockopt(s, Δsyscall.SOL_SOCKET, Δsyscall.SO_UPDATE_ACCEPT_CONTEXT, Ꮡfd.of(FD.ᏑSysfd).Reinterpret<syscallꓸHandle, byte>(), (int32)/* unsafe.Sizeof(fd.Sysfd) */ (uintptr)8);
    if (err != default!) {
        CloseFunc(s);
        return (setsockoptˢ, err);
    }

    return ("", default!);
}

// Accept handles accepting a socket. The sysSocket parameter is used
// to allocate the net socket.
public static (syscallꓸHandle, slice<Δsyscall.RawSockaddrAny>, uint32, @string, error) Accept(this ж<FD> Ꮡfd, Func<(syscallꓸHandle, error)> sysSocket) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return (Δsyscall.InvalidHandle, default!, 0, "", err);
            }
        }
        ᒐd1 = true;

        array<Δsyscall.RawSockaddrAny> rawsa = new(2, () => new());
        while (ᐧ) {
            var (s, err) = sysSocket();
            if (err != default!) {
                return (Δsyscall.InvalidHandle, default!, 0, "", err);
            }

            (var errcall, err) = Ꮡfd.acceptOne(s, rawsa[..]);
            if (err == default!) {
                return (s, rawsa[..], (uint32)/* unsafe.Sizeof(rawsa[0]) */ (uintptr)116, "", default!);
            }

            // Sometimes we see WSAECONNRESET and ERROR_NETNAME_DELETED is
            // returned here. These happen if connection reset is received
            // before AcceptEx could complete. These errors relate to new
            // connection, not to AcceptEx, so ignore broken connection and
            // try AcceptEx again for more connections.
            var (errno, ok) = err._<Δsyscall.Errno>(ᐧ);
            if (!ok) {
                return (Δsyscall.InvalidHandle, default!, 0, errcall, err);
            }
            var exprᴛ1 = errno;
            if (exprᴛ1 == Δsyscall.ERROR_NETNAME_DELETED || exprᴛ1 == Δsyscall.WSAECONNRESET) {
                // ignore these and try again
            }
            else { /* default: */
                return (Δsyscall.InvalidHandle, default!, 0, errcall, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// Seek wraps syscall.Seek.
public static (int64, error) Seek(this ж<FD> Ꮡfd, int64 offset, nint whence) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        if (fd.kind == kindPipe) {
            return (0, Δsyscall.ESPIPE);
        }
        {
            var errΔ1 = Ꮡfd.readWriteLock(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        ᒐd1 = true;

        if (!fd.isBlocking) {
            // Windows doesn't use the file pointer for overlapped file handles,
            // there is no point on calling syscall.Seek.
            int64 newOffset = default!;
            var exprᴛ1 = whence;
            if (exprᴛ1 == io.SeekStart) {
                newOffset = offset;
            }
            else if (exprᴛ1 == io.SeekCurrent) {
                newOffset = fd.offset + offset;
            }
            else if (exprᴛ1 == io.SeekEnd) {
                ref var size = ref heap(new int64(), out var Ꮡsize);
                {
                    var errΔ3 = windows.GetFileSizeEx(fd.Sysfd, Ꮡsize); if (errΔ3 != default!) {
                        return (0, errΔ3);
                    }
                }
                newOffset = size + offset;
            }
            else { /* default: */
                return (0, windows.ERROR_INVALID_PARAMETER);
            }
            if (newOffset < 0) {
                return (0, windows.ERROR_NEGATIVE_SEEK);
            }
            fd.setOffset(newOffset);
            return (newOffset, default!);
        }
        var (n, err) = Δsyscall.Seek(fd.Sysfd, offset, whence);
        fd.setOffset(n);
        return (n, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readWriteUnlock(); ᒐ.Run(); }
}

// Fchmod updates syscall.ByHandleFileInformation.Fileattributes when needed.
public static error Fchmod(this ж<FD> Ꮡfd, uint32 mode) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;

        ref var d = ref heap(new Δsyscall.ByHandleFileInformation(), out var Ꮡd);
        {
            var err = Δsyscall.GetFileInformationByHandle(fd.Sysfd, Ꮡd); if (err != default!) {
                return err;
            }
        }
        var attrs = d.FileAttributes;
        if ((mode & (uint32)Δsyscall.S_IWRITE) != 0){
            attrs &= unchecked((uint32)~(uint32)(Δsyscall.FILE_ATTRIBUTE_READONLY));
        } else {
            attrs |= (uint32)(Δsyscall.FILE_ATTRIBUTE_READONLY);
        }
        if (attrs == d.FileAttributes) {
            return default!;
        }

        ref var du = ref heap(new windows.FILE_BASIC_INFO(), out var Ꮡdu);
        du.FileAttributes = attrs;
        return windows.SetFileInformationByHandle(fd.Sysfd, windows.FileBasicInfo, @unsafe.Pointer.FromPinnedBox(Ꮡdu), (uint32)/* unsafe.Sizeof(du) */ (uintptr)40);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// Fchdir wraps syscall.Fchdir.
public static error Fchdir(this ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return Δsyscall.Fchdir(fd.Sysfd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// GetFileType wraps syscall.GetFileType.
public static (uint32, error) GetFileType(this ж<FD> Ꮡfd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return (0, err);
            }
        }
        ᒐd1 = true;
        return Δsyscall.GetFileType(fd.Sysfd);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// GetFileInformationByHandle wraps GetFileInformationByHandle.
public static error GetFileInformationByHandle(this ж<FD> Ꮡfd, ж<Δsyscall.ByHandleFileInformation> Ꮡdata) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.incref(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        return Δsyscall.GetFileInformationByHandle(fd.Sysfd, Ꮡdata);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.decref(); ᒐ.Run(); }
}

// RawRead invokes the user-defined function f for a read operation.
public static error RawRead(this ж<FD> Ꮡfd, Func<uintptr, bool> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.readLock(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;
        while (ᐧ) {
            if (f((uintptr)fd.Sysfd)) {
                return default!;
            }

            // Use a zero-byte read as a way to get notified when this
            // socket is readable. h/t https://stackoverflow.com/a/42019668/332798
            var (_, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
                ref var qty = ref heap(new uint32(), out var Ꮡqty);
                error errΔ1 = default!;
                ref var flags = ref heap(new uint32(), out var Ꮡflags);
                if (!Ꮡfd.Value.IsStream) {
                    flags |= (uint32)(windows.MSG_PEEK);
                }
                errΔ1 = Δsyscall.WSARecv(Ꮡfd.Value.Sysfd, Ꮡ(new Δsyscall.WSABuf(nil)), 1, Ꮡqty, Ꮡflags, o.of(operation.Ꮡo), nil);
                return (Ꮡqty.Value, errΔ1);
            });
            if (AreEqual(err, windows.WSAEMSGSIZE)){
                // expected with a 0-byte peek, ignore.
            } else 
            if (err != default!) {
                return err;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.readUnlock(); ᒐ.Run(); }
}

// RawWrite invokes the user-defined function f for a write operation.
public static error RawWrite(this ж<FD> Ꮡfd, Func<uintptr, bool> f) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var err = Ꮡfd.writeLock(); if (err != default!) {
                return err;
            }
        }
        ᒐd1 = true;

        if (f((uintptr)fd.Sysfd)) {
            return default!;
        }

        // TODO(tmm1): find a way to detect socket writability
        return Δsyscall.EWINDOWS;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡfd.writeUnlock(); ᒐ.Run(); }
}

// go2cs generated this placeholder — func sockaddrInet4ToRaw is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sockaddrInet6ToRaw is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func rawToSockaddrInet4 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func rawToSockaddrInet6 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (int32, error) sockaddrToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<Δsyscall.SockaddrInet4> saΔ1: {
        var sz = sockaddrInet4ToRaw(Ꮡrsa, ref (saΔ1).DerefOrNull());
        return (sz, default!);
    }
    case ж<Δsyscall.SockaddrInet6> saΔ1: {
        var sz = sockaddrInet6ToRaw(Ꮡrsa, ref (saΔ1).DerefOrNull());
        return (sz, default!);
    }
    default: {
        var saΔ1 = sa;
        return (0, Δsyscall.EWINDOWS);
    }}
}

// ReadMsg wraps the WSARecvMsg network call.
public static (nint, nint, nint, syscallꓸSockaddr, error) ReadMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, 0, 0, default!, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        if (len(p) > maxRW) {
            p = p[..(int)(maxRW)];
        }

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var msg = newWSAMsg(p, oob, flags, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            errΔ2 = windows.WSARecvMsg(Ꮡfd.Value.Sysfd, msgʗ1, Ꮡqty, o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        syscallꓸSockaddr sa = default!;
        if (err == default!) {
            (sa, err) = (~msg).Name.Sockaddr();
        }
        return (n, (nint)(~msg).Control.Len, (nint)(~msg).Flags, sa, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ReadMsgInet4 is ReadMsg, but specialized to return a syscall.SockaddrInet4.
public static (nint, nint, nint, error) ReadMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet4> Ꮡsa4) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, 0, 0, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        if (len(p) > maxRW) {
            p = p[..(int)(maxRW)];
        }

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var msg = newWSAMsg(p, oob, flags, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            errΔ2 = windows.WSARecvMsg(Ꮡfd.Value.Sysfd, msgʗ1, Ꮡqty, o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        if (err == default!) {
            rawToSockaddrInet4((~msg).Name, ref (Ꮡsa4).DerefOrNull());
        }
        return (n, (nint)(~msg).Control.Len, (nint)(~msg).Flags, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// ReadMsgInet6 is ReadMsg, but specialized to return a syscall.SockaddrInet6.
public static (nint, nint, nint, error) ReadMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet6> Ꮡsa6) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        {
            var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
                return (0, 0, 0, errΔ1);
            }
        }
        defer(Ꮡfd.readUnlock, ref ᒐ);

        if (len(p) > maxRW) {
            p = p[..(int)(maxRW)];
        }

        var rsa = newWSARsa();
        defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
        var msg = newWSAMsg(p, oob, flags, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'r', (ж<operation> o) => {
            ref var qty = ref heap(new uint32(), out var Ꮡqty);
            error errΔ2 = default!;
            errΔ2 = windows.WSARecvMsg(Ꮡfd.Value.Sysfd, msgʗ1, Ꮡqty, o.of(operation.Ꮡo), nil);
            return (Ꮡqty.Value, errΔ2);
        });
        err = fd.eofError(n, err);
        if (err == default!) {
            rawToSockaddrInet6((~msg).Name, ref (Ꮡsa6).DerefOrNull());
        }
        return (n, (nint)(~msg).Control.Len, (nint)(~msg).Flags, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packetIsTooLargeOnly1gbˢ = "packet is too large (only 1GB is allowed)"u8;

// WriteMsg wraps the WSASendMsg network call.
public static (nint, nint, error) WriteMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, syscallꓸSockaddr sa) {
    GoFrame ᒐ = default;
    try {
        if (len(p) > maxRW) {
            return (0, 0, errors.New(packetIsTooLargeOnly1gbˢ));
        }

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                return (0, 0, errΔ1);
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);

        ж<wsaRsa> rsa = default!;
        if (sa != default!) {
            rsa = newWSARsa();
            defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
            error errΔ2 = default!;
            (rsa.Value.namelen, errΔ2) = sockaddrToRaw(rsa.of(wsaRsa.Ꮡname), sa);
            if (errΔ2 != default!) {
                return (0, 0, errΔ2);
            }
        }
        var msg = newWSAMsg(p, oob, 0, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
            uint32 qty = default!;
            error errΔ3 = default!;
            errΔ3 = windows.WSASendMsg(Ꮡfd.Value.Sysfd, msgʗ1, 0, nil, o.of(operation.Ꮡo), nil);
            return (qty, errΔ3);
        });
        return (n, (nint)(~msg).Control.Len, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// WriteMsgInet4 is WriteMsg specialized for syscall.SockaddrInet4.
public static (nint, nint, error) WriteMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet4> Ꮡsa) {
    GoFrame ᒐ = default;
    try {
        if (len(p) > maxRW) {
            return (0, 0, errors.New(packetIsTooLargeOnly1gbˢ));
        }

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                return (0, 0, errΔ1);
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);

        ж<wsaRsa> rsa = default!;
        if (Ꮡsa != nil) {
            rsa = newWSARsa();
            defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
            rsa.Value.namelen = sockaddrInet4ToRaw(rsa.of(wsaRsa.Ꮡname), ref (Ꮡsa).DerefOrNull());
        }
        var msg = newWSAMsg(p, oob, 0, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
            uint32 qty = default!;
            error errΔ2 = default!;
            errΔ2 = windows.WSASendMsg(Ꮡfd.Value.Sysfd, msgʗ1, 0, nil, o.of(operation.Ꮡo), nil);
            return (qty, errΔ2);
        });
        return (n, (nint)(~msg).Control.Len, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// WriteMsgInet6 is WriteMsg specialized for syscall.SockaddrInet6.
public static (nint, nint, error) WriteMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet6> Ꮡsa) {
    GoFrame ᒐ = default;
    try {
        if (len(p) > maxRW) {
            return (0, 0, errors.New(packetIsTooLargeOnly1gbˢ));
        }

        {
            var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
                return (0, 0, errΔ1);
            }
        }
        defer(Ꮡfd.writeUnlock, ref ᒐ);

        ж<wsaRsa> rsa = default!;
        if (Ꮡsa != nil) {
            rsa = newWSARsa();
            defer(ᏑwsaRsaPool.Put, rsa.OrTypedNil(), ref ᒐ);
            rsa.Value.namelen = sockaddrInet6ToRaw(rsa.of(wsaRsa.Ꮡname), ref (Ꮡsa).DerefOrNull());
        }
        var msg = newWSAMsg(p, oob, 0, rsa);
        defer(freeWSAMsg, msg, ref ᒐ);
        var msgʗ1 = msg;
        var (n, err) = Ꮡfd.execIO((rune)'w', (ж<operation> o) => {
            uint32 qty = default!;
            error errΔ2 = default!;
            errΔ2 = windows.WSASendMsg(Ꮡfd.Value.Sysfd, msgʗ1, 0, nil, o.of(operation.Ꮡo), nil);
            return (qty, errΔ2);
        });
        return (n, (nint)(~msg).Control.Len, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getCurrentProcessˢ = "GetCurrentProcess"u8;
internal static readonly @string duplicateHandleˢ = "DuplicateHandle"u8;

public static (nint, @string, error) DupCloseOnExec(nint fd) {
    var (proc, err) = Δsyscall.GetCurrentProcess();
    if (err != default!) {
        return (0, getCurrentProcessˢ, err);
    }

    ref var nfd = ref heap(new syscallꓸHandle(), out var Ꮡnfd);
    const bool inherit = false; // analogous to CLOEXEC
    {
        var errΔ1 = Δsyscall.DuplicateHandle(proc, (syscallꓸHandle)(uintptr)fd, proc, Ꮡnfd, 0, inherit, Δsyscall.DUPLICATE_SAME_ACCESS); if (errΔ1 != default!) {
            return (0, duplicateHandleˢ, errΔ1);
        }
    }
    return ((nint)(uintptr)nfd, "", default!);
}

} // end poll_package
