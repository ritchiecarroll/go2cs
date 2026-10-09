// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using poll = go.@internal.poll_package;
using windows = go.@internal.syscall.windows_package;
using os = os_package;
using sync = go.sync_package;
using Δsyscall = syscall_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go;
using go.@internal;
using go.@internal.syscall;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

partial struct loggedFD {
    public @string Net;
    public ж<poll.FD> FD;
    public error Err;
}

internal static ж<sync.Mutex> ᏑlogMu = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex logMu => ref ᏑlogMu.Value;
internal static map<syscallꓸHandle, ж<loggedFD>> loggedFDs;

internal static void logFD(@string net, ж<poll.FD> Ꮡfd, error err) {
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        ᏑlogMu.Lock();
        defer(ᏑlogMu.Unlock, ref ᒐ);
        loggedFDs[fd.Sysfd] = Ꮡ(new loggedFD(
            Net: net,
            FD: Ꮡfd,
            Err: err
        ));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoInit] internal static void init() {
    loggedFDs = new map<syscallꓸHandle, ж<loggedFD>>();
    poll_internal_test_package.LogInitFD.ValueSlot = logFD;
    poll.InitWSA();
}

internal static (ж<loggedFD> lfd, bool found) findLoggedFD(syscallꓸHandle h) {
    ж<loggedFD> lfd = default!;
    bool found = default!;
    GoFrame ᒐ = default;
    try {
        ᏑlogMu.Lock();
        defer(ᏑlogMu.Unlock, ref ᒐ);
        (lfd, found) = loggedFDs[h, ꟷ];
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (lfd, found);
}

// checkFileIsNotPartOfNetpoll verifies that f is not managed by netpoll.
// It returns error, if check fails.
internal static error checkFileIsNotPartOfNetpoll(ж<os.File> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    var (lfd, found) = findLoggedFD(((syscallꓸHandle)Ꮡf.Fd()));
    if (!found) {
        return fmt.Errorf("%v fd=%v: is not found in the log"u8, f.Name(), Ꮡf.Fd());
    }
    if ((~lfd).FD.IsPartOfNetpoll()) {
        return fmt.Errorf("%v fd=%v: is part of netpoll, but should not be (logged: net=%v err=%v)"u8, f.Name(), Ꮡf.Fd(), (~lfd).Net, (~lfd).Err);
    }
    return default!;
}

public static void TestFileFdsAreInitialised(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (exe, err) = os.Executable();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var f, err) = os.Open(exe);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        err = checkFileIsNotPartOfNetpoll(f);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingˢ = (@string)"Skipping: "u8;

public static void TestSerialFdsAreInitialised(ж<testing.T> Ꮡt) {
    foreach (var (_, name) in new @string[]{"COM1"u8, "COM2"u8, "COM3"u8, "COM4"u8}.slice()) {
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (h, err) = Δsyscall.CreateFile(Δsyscall.StringToUTF16Ptr(name),
                    (uint32)((uint32)Δsyscall.GENERIC_READ | (uint32)Δsyscall.GENERIC_WRITE),
                    0,
                    nil,
                    Δsyscall.OPEN_EXISTING,
                    (uint32)((uint32)Δsyscall.FILE_ATTRIBUTE_NORMAL | (uint32)Δsyscall.FILE_FLAG_OVERLAPPED),
                    0);
                if (err != default!) {
                    {
                        var (errno, ok) = err._<Δsyscall.Errno>(ᐧ); if (ok) {
                            var exprᴛ1 = errno;
                            if (exprᴛ1 == Δsyscall.ERROR_FILE_NOT_FOUND || exprᴛ1 == Δsyscall.ERROR_ACCESS_DENIED) {
                                tΔ1.Log(skippingˢ, err);
                                return;
                            }

                        }
                    }
                    tΔ1.Fatal(err);
                }
                var f = os.NewFile((uintptr)h, name);
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                err = checkFileIsNotPartOfNetpoll(f);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

public static void TestWSASocketConflict(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (s, err) = windows.WSASocket(Δsyscall.AF_INET, Δsyscall.SOCK_STREAM, Δsyscall.IPPROTO_TCP, nil, 0, windows.WSA_FLAG_OVERLAPPED);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ref var fd = ref heap<poll.FD>(out var Ꮡfd);
        fd = new poll.FD(Sysfd: s, IsStream: true, ZeroReadIsEOF: true);
        (_, err) = Ꮡfd.Init(tcpˢ, true);
        if (err != default!) {
            Δsyscall.CloseHandle(s);
            Ꮡt.Fatal(err);
        }
        defer(() => Ꮡfd.Close(), ref ᒐ);
        const uint32 SIO_TCP_INFO = /* syscall.IOC_INOUT | syscall.IOC_VENDOR | 39 */ 3623878695;
        ref var inbuf = ref heap<uint32>(out var Ꮡinbuf);
        inbuf = (uint32)0;
        ref var outbuf = ref heap(new _TCP_INFO_v0(), out var Ꮡoutbuf);
        ref var cbbr = ref heap<uint32>(out var Ꮡcbbr);
        cbbr = (uint32)0;
        ref var ov = ref heap(new Δsyscall.Overlapped(), out var Ꮡov);
        // Create an event so that we can efficiently wait for completion
        // of a requested overlapped I/O operation.
        (ov.HEvent, _) = windows.CreateEvent(nil, 0, 0, nil);
        if (ov.HEvent == 0) {
            Ꮡt.Fatalf("could not create the event!"u8);
        }
        defer(Δsyscall.CloseHandle, ov.HEvent, ref ᒐ);
        {
            err = Ꮡfd.WSAIoctl(
                SIO_TCP_INFO,
                Ꮡinbuf.Reinterpret<uint32, byte>(),
                (uint32)/* unsafe.Sizeof(inbuf) */ (uintptr)4,
                Ꮡoutbuf.Reinterpret<_TCP_INFO_v0, byte>(),
                (uint32)/* unsafe.Sizeof(outbuf) */ (uintptr)88,
                Ꮡcbbr,
                Ꮡov,
                0); if (err != default! && !errors.Is(err, Δsyscall.ERROR_IO_PENDING)) {
                Ꮡt.Fatalf("could not perform the WSAIoctl: %v"u8, err);
            }
        }
        if (err != default! && errors.Is(err, Δsyscall.ERROR_IO_PENDING)) {
            // It is possible that the overlapped I/O operation completed
            // immediately so there is no need to wait for it to complete.
            {
                var (res, errΔ1) = Δsyscall.WaitForSingleObject(ov.HEvent, Δsyscall.INFINITE); if (res != 0) {
                    Ꮡt.Fatalf("waiting for the completion of the overlapped IO failed: %v"u8, errΔ1);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct _TCP_INFO_v0 {
    public uint32 State;
    public uint32 Mss;
    public uint64 ConnectionTimeMs;
    public bool TimestampsEnabled;
    public uint32 RttUs;
    public uint32 MinRttUs;
    public uint32 BytesInFlight;
    public uint32 Cwnd;
    public uint32 SndWnd;
    public uint32 RcvWnd;
    public uint32 RcvBuf;
    public uint64 BytesOut;
    public uint64 BytesIn;
    public uint32 BytesReordered;
    public uint32 BytesRetrans;
    public uint32 FastRetrans;
    public uint32 DupAcksIn;
    public uint32 TimeoutEpisodes;
    public uint8 SynRetrans;
}

} // end poll_test_package
