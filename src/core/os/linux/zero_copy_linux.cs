// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using poll = @internal.poll_package;
using Δio = io_package;
using syscall = syscall_package;
using @internal;

partial class os_package {

internal static ж<Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)>> ᏑpollCopyFileRange = new StandardBox<Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)>>(poll.CopyFileRange);
internal static ref Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)> pollCopyFileRange => ref ᏑpollCopyFileRange.ValueSlot;
internal static ж<Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)>> ᏑpollSplice = new StandardBox<Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)>>(poll.Splice);
internal static ref Func<ж<poll.FD>, ж<poll.FD>, int64, (int64, bool, error)> pollSplice => ref ᏑpollSplice.ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sendfileˢ = "sendfile"u8;

internal static (int64 written, bool handled, error err) writeTo(this ж<File> Ꮡf, Δio.Writer w) {
    int64 written = default!;
    bool handled = default!;
    ref var err = ref heap<error>(out var Ꮡerr);

    var (pfd, network) = getPollFDAndNetwork(w);
    // TODO(panjf2000): same as File.spliceToFile.
    if (pfd == nil || !(~pfd).IsStream || !isUnixOrTCP(((@string)network))) {
        return (written, handled, err);
    }
    (var sc, err) = Ꮡf.SyscallConn();
    if (err != default!) {
        return (written, handled, err);
    }
    var pfdʗ1 = pfd;
    var rerr = sc.Read((uintptr fd) => {
        (written, Ꮡerr.ValueSlot, handled) = poll.SendFile(pfdʗ1, (nint)fd, 0);
        return true;
    });
    if (err == default!) {
        err = rerr;
    }
    return (written, handled, wrapSyscallError(sendfileˢ, err));
}

internal static (int64 written, bool handled, error err) readFrom(this ж<File> Ꮡf, Δio.Reader r) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    // Neither copy_file_range(2) nor splice(2) supports destinations opened with
    // O_APPEND, so don't bother to try zero-copy with these system calls.
    //
    // Visit https://man7.org/linux/man-pages/man2/copy_file_range.2.html#ERRORS and
    // https://man7.org/linux/man-pages/man2/splice.2.html#ERRORS for details.
    if (f.appendMode) {
        return (0, false, default!);
    }
    (written, handled, err) = Ꮡf.copyFileRange(r);
    if (handled) {
        return (written, handled, err);
    }
    return Ꮡf.spliceToFile(r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spliceˢ = "splice"u8;

internal static (int64 written, bool handled, error err) spliceToFile(this ж<File> Ꮡf, Δio.Reader r) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;

    int64 remain = default!;
    ж<Δio.LimitedReader> lr = default!;
    {
        (lr, r, remain) = tryLimitedReader(r); if (remain <= 0) {
            return (0, true, default!);
        }
    }
    var (pfd, _) = getPollFDAndNetwork(r);
    // TODO(panjf2000): run some tests to see if we should unlock the non-streams for splice.
    // Streams benefit the most from the splice(2), non-streams are not even supported in old kernels
    // where splice(2) will just return EINVAL; newer kernels support non-streams like UDP, but I really
    // doubt that splice(2) could help non-streams, cuz they usually send small frames respectively
    // and one splice call would result in one frame.
    // splice(2) is suitable for large data but the generation of fragments defeats its edge here.
    // Therefore, don't bother to try splice if the r is not a streaming descriptor.
    if (pfd == nil || !(~pfd).IsStream) {
        return (written, handled, err);
    }
    (written, handled, err) = pollSplice(Ꮡf.of(File.Ꮡpfd), pfd, remain);
    if (lr != nil) {
        lr.Value.N = remain - written;
    }
    return (written, handled, wrapSyscallError(spliceˢ, err));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readFromˢ = "ReadFrom"u8;
internal static readonly @string copyFileRangeˢ = "copy_file_range"u8;

internal static (int64 written, bool handled, error err) copyFileRange(this ж<File> Ꮡf, Δio.Reader r) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;

    int64 remain = default!;
    ж<Δio.LimitedReader> lr = default!;
    {
        (lr, r, remain) = tryLimitedReader(r); if (remain <= 0) {
            return (0, true, default!);
        }
    }
    ж<File> src = default!;
    switch (r.type()) {
    case ж<File> v: {
        src = v;
        break;
    }
    case fileWithoutWriteTo v: {
        src = v.File;
        break;
    }
    default: {
        var v = r;
        return (0, false, default!);
    }}
    if (src.checkValid(readFromˢ) != default!) {
        // Avoid returning the error as we report handled as false,
        // leave further error handling as the responsibility of the caller.
        return (0, false, default!);
    }
    (written, handled, err) = pollCopyFileRange(Ꮡf.of(File.Ꮡpfd), src.of(File.Ꮡpfd), remain);
    if (lr != nil) {
        lr.Value.N -= written;
    }
    return (written, handled, wrapSyscallError(copyFileRangeˢ, err));
}

internal partial interface getPollFDAndNetwork_type /*dyn*/ {
    ж<poll.FD> PollFD();
    poll.String Network();
}

// getPollFDAndNetwork tries to get the poll.FD and network type from the given interface
// by expecting the underlying type of i to be the implementation of syscall.Conn
// that contains a *net.rawConn.
internal static (ж<poll.FD>, poll.String) getPollFDAndNetwork(any i) {
    var (sc, ok) = i._<syscall.Conn>(ᐧ);
    if (!ok) {
        return (default!, (@string)"");
    }
    var (rc, err) = sc.SyscallConn();
    if (err != default!) {
        return (default!, (@string)"");
    }
    (var irc, ok) = rc._<getPollFDAndNetwork_type>(ᐧ);
    if (!ok) {
        return (default!, (@string)"");
    }
    return (irc.PollFD(), irc.Network());
}

internal static bool isUnixOrTCP(@string network) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8 || exprᴛ1 == "unix"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

} // end os_package
