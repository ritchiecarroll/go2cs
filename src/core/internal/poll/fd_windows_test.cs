// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using poll = go.@internal.poll_package;
using windows = go.@internal.syscall.windows_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using Δsyscall = syscall_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using fs = go.io.fs_package;
using go.@internal;
using go.@internal.syscall;
using path;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

[GoInit] internal static void init() {
    poll.InitWSA();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

public static void TestWSASocketConflict(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (s, err) = windows.WSASocket(Δsyscall.AF_INET, Δsyscall.SOCK_STREAM, Δsyscall.IPPROTO_TCP, nil, 0, windows.WSA_FLAG_OVERLAPPED);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        ref var fd = ref heap<poll.FD>(out var Ꮡfd);
        fd = new poll.FD(Sysfd: s, IsStream: true, ZeroReadIsEOF: true);
        {
            err = Ꮡfd.Init(tcpˢ, true); if (err != default!) {
                Δsyscall.CloseHandle(s);
                Ꮡt.Fatal(err);
            }
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

internal static ж<poll.FD> newFD(testing.TB t, syscallꓸHandle h, @string kind, bool overlapped) {
    ref var fd = ref heap<poll.FD>(out var Ꮡfd);
    fd = new poll.FD(
        Sysfd: h,
        IsStream: true,
        ZeroReadIsEOF: true
    );
    var err = Ꮡfd.Init(kind, overlapped);
    if (overlapped && err != default!) {
        // Overlapped file handles should not error.
        Ꮡfd.Close();
        t.Fatal(err);
    }
    t.Cleanup(() => {
        Ꮡfd.Close();
    });
    return Ꮡfd;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string pipeˢ = "pipe"u8;

internal static ж<poll.FD> newFile(testing.TB t, @string name, bool overlapped) {
    var (namep, err) = Δsyscall.UTF16PtrFromString(name);
    if (err != default!) {
        t.Fatal(err);
    }
    nint flags = Δsyscall.FILE_ATTRIBUTE_NORMAL;
    if (overlapped) {
        flags |= (nint)(Δsyscall.FILE_FLAG_OVERLAPPED);
    }
    (var h, err) = Δsyscall.CreateFile(namep,
        (uint32)((uint32)Δsyscall.GENERIC_READ | (uint32)Δsyscall.GENERIC_WRITE),
        (uint32)((uint32)Δsyscall.FILE_SHARE_WRITE | (uint32)Δsyscall.FILE_SHARE_READ),
        nil, Δsyscall.OPEN_ALWAYS, (uint32)flags, 0);
    if (err != default!) {
        t.Fatal(err);
    }
    (var typ, err) = Δsyscall.GetFileType(h);
    if (err != default!) {
        Δsyscall.CloseHandle(h);
        t.Fatal(err);
    }
    @string kind = fileˢ;
    if (typ == Δsyscall.FILE_TYPE_PIPE) {
        kind = pipeˢ;
    }
    return newFD(t, h, kind, overlapped);
}

public static void BenchmarkReadOverlapped(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, true);
}

public static void BenchmarkReadSync(ж<testing.B> Ꮡb) {
    benchmarkRead(Ꮡb, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ = "hello world"u8;

internal static void benchmarkRead(ж<testing.B> Ꮡb, bool overlapped) {
    @string name = filepath.Join(Ꮡb.TempDir(), fooˢ);
    @string content = contentᶜ;
    var err = os.WriteFile(name, slice<byte>(content), 420);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var @file = newFile(new poll_test_package.testing_BжTB(Ꮡb), name, overlapped);
    array<byte> buf = new(11); /* len(content) */
    while (Ꮡb.Loop()) {
        var (_, errΔ1) = io.ReadFull(new poll_test_package.poll_FDжReader(@file), buf[..]);
        if (errΔ1 != default!) {
            Ꮡb.Fatal(errΔ1);
        }
        {
            var (_, errΔ2) = @file.Seek(0, io.SeekStart); if (errΔ2 != default!) {
                Ꮡb.Fatal(errΔ2);
            }
        }
    }
}

} // end poll_test_package
