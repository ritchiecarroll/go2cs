// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;
using time = time_package;

partial class net_package {

internal static readonly @string readSyscallName = "read"u8;
internal static readonly @string readFromSyscallName = "recvfrom"u8;
internal static readonly @string readMsgSyscallName = "recvmsg"u8;
internal static readonly @string writeSyscallName = "write"u8;
internal static readonly @string writeToSyscallName = "sendto"u8;
internal static readonly @string writeMsgSyscallName = "sendmsg"u8;

internal static (ж<netFD>, error) newFD(nint sysfd, nint family, nint sotype, @string net) {
    var ret = Ꮡ(new netFD(
        pfd: new poll.FD(
            Sysfd: sysfd,
            IsStream: sotype == syscall.SOCK_STREAM,
            ZeroReadIsEOF: sotype != syscall.SOCK_DGRAM && sotype != syscall.SOCK_RAW
        ),
        family: family,
        sotype: sotype,
        net: net
    ));
    return (ret, default);
}

internal static error init(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    return Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectˢ = "connect"u8;
internal static readonly @string getsockoptˢ = "getsockopt"u8;

internal static (syscall.Sockaddr rsa, error ret) connect(this ж<netFD> Ꮡfd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) {
    syscall.Sockaddr rsa = default;
    error ret = default;
    GoFrame ᒐ = default;
    try {
        ref var fd = ref Ꮡfd.DerefOrNull();

        // Do not need to call fd.writeLock here,
        // because fd is not yet accessible to user,
        // so no concurrent operations are possible.
        {
            var err = connectFunc(fd.pfd.Sysfd, ra);
            var exprᴛ1 = err;
            var matchᴛ1 = false;
            if (AreEqual(exprᴛ1, syscall.EINPROGRESS) || AreEqual(exprᴛ1, syscall.EALREADY) || AreEqual(exprᴛ1, syscall.EINTR)) { matchᴛ1 = true;
            }
            else if (AreEqual(exprᴛ1, default) || AreEqual(exprᴛ1, syscall.EISCONN)) { matchᴛ1 = true;
                var selᴛ11 = ctx.Done();
                switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
                case 0 when selᴛ11.ꟷᐳ(out _): {
                    (rsa, ret) = (default, mapErr(ctx.Err())); goto ᒐdone;
                }
                default: {
                    break;
                }}
                {
                    var errΔ2 = Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true); if (errΔ2 != nil) {
                        (rsa, ret) = (default, errΔ2); goto ᒐdone;
                    }
                }
                runtime.KeepAlive(Ꮡfd.OrTypedNil());
                (rsa, ret) = (default, default); goto ᒐdone;
            }
            else if (AreEqual(exprᴛ1, syscall.EINVAL)) { matchᴛ1 = true;
                // On Solaris and illumos we can see EINVAL if the socket has
                // already been accepted and closed by the server.  Treat this
                // as a successful connection--writes to the socket will see
                // EOF.  For details and a test case in C see
                // https://golang.org/issue/6828.
                if (runtime.GOOS == "solaris"u8 || runtime.GOOS == "illumos"u8) {
                    (rsa, ret) = (default, default); goto ᒐdone;
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                (rsa, ret) = (default, os.NewSyscallError(connectˢ, err)); goto ᒐdone;
            }
        }
        {
            var err = Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true); if (err != nil) {
                (rsa, ret) = (default, err); goto ᒐdone;
            }
        }

        var ctxDone = ctx.Done();
        if (ctxDone != nil) {
            {
                var (deadline, hasDeadline) = ctx.Deadline(); if (hasDeadline) {
                    Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline(deadline);
                    defer(Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline, noDeadline, ref ᒐ);
                }
            }

            // Load the hook function synchronously to prevent a race
            // with test code that restores the old value.
            var testHookCanceledDialΔ1 = testHookCanceledDial;
            var testHookCanceledDialʗ1 = testHookCanceledDialΔ1;
            var stop = context.AfterFunc(ctx, () => {
                // Force the runtime's poller to immediately give up
                // waiting for writability, unblocking waitWrite
                // below.
                _ = Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline(aLongTimeAgo);
                testHookCanceledDialʗ1();
            });
            var stopʗ1 = stop;
            defer(() => {
                if (!stopʗ1() && ret == nil) {
                    // The context.AfterFunc has called or is about to call
                    // SetWriteDeadline, but the connect code below had
                    // returned from waitWrite already and did a successful
                    // connect (ret == nil). Because we've now poisoned the
                    // connection by making it unwritable, don't return a
                    // successful dial. This was issue 16523.
                    ret = mapErr(ctx.Err());
                    // The caller closes fd on error, so there's no need to
                    // wait for the SetWriteDeadline call to return.
                }
            }, ref ᒐ);
        }

        for (;;) {
            // Performing multiple connect system calls on a
            // non-blocking socket under Unix variants does not
            // necessarily result in earlier errors being
            // returned. Instead, once runtime-integrated network
            // poller tells us that the socket is ready, get the
            // SO_ERROR socket option to see if the connection
            // succeeded or failed. See issue 7474 for further
            // details.
            {
                var errΔ1 = fd.pfd.WaitWrite(); if (errΔ1 != nil) {
                    var selᴛ12 = ctxDone;
                    switch (trySelect(ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
                    case 0 when selᴛ12.ꟷᐳ(out _): {
                        (rsa, ret) = (default, mapErr(ctx.Err())); goto ᒐdone;
                    }
                    default: {
                        break;
                    }}
                    (rsa, ret) = (default, errΔ1); goto ᒐdone;
                }
            }
            var (nerr, err) = getsockoptIntFunc(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_ERROR);
            if (err != nil) {
                (rsa, ret) = (default, os.NewSyscallError(getsockoptˢ, err)); goto ᒐdone;
            }
            {
                var errΔ1 = (syscall.Errno)(uintptr)nerr;
                switch (errΔ1) {
                case var ᴛ when ᴛ == syscall.EINPROGRESS || ᴛ == syscall.EALREADY || ᴛ == syscall.EINTR: {
                    break;
                }
                case var ᴛ when ᴛ == syscall.EISCONN: {
                    (rsa, ret) = (default, default); goto ᒐdone;
                }
                case var ᴛ when ᴛ == ((syscall.Errno)0): {
                    // The runtime poller can wake us up spuriously;
                    // see issues 14548 and 19289. Check that we are
                    // really connected; if not, wait again.
                    {
                        var (rsaΔ2, errΔ3) = syscall.Getpeername(fd.pfd.Sysfd); if (errΔ3 == nil) {
                            (rsa, ret) = (rsaΔ2, default); goto ᒐdone;
                        }
                    }
                    break;
                }
                default: {
                    (rsa, ret) = (default, os.NewSyscallError(connectˢ, errΔ1)); goto ᒐdone;
                }}
            }
            runtime.KeepAlive(Ꮡfd.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (rsa, ret);
}

internal static (ж<netFD> netfd, error err) accept(this ж<netFD> Ꮡfd) {
    ж<netFD> netfd = default;
    error err = default;
    ref var fd = ref Ꮡfd.DerefOrNull();

    (var d, var rsa, var errcall, err) = Ꮡfd.of(netFD.Ꮡpfd).Accept();
    if (err != nil) {
        if (errcall != ""u8) {
            err = wrapSyscallError(errcall, err);
        }
        return (default, err);
    }

    {
        (netfd, err) = newFD(d, fd.family, fd.sotype, fd.net); if (err != nil) {
            poll.CloseFunc(d);
            return (default, err);
        }
    }
    {
        err = netfd.init(); if (err != nil) {
            netfd.Close();
            return (default, err);
        }
    }
    var (lsa, _) = syscall.Getsockname(netfd.pfd.Sysfd);
    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (netfd, default);
}

// Defined in os package.
/*linkname*/ internal static partial ж<os.File> newUnixFile(nint fd, @string name) {
    return os.net_newUnixFile(fd, name);
}

internal static (ж<os.File> f, error err) dup(this ж<netFD> Ꮡfd) {
    error err = default;
    ref var fd = ref Ꮡfd.DerefOrNull();

    (var ns, var call, err) = Ꮡfd.of(netFD.Ꮡpfd).Dup();
    if (err != nil) {
        if (call != ""u8) {
            err = os.NewSyscallError(call, err);
        }
        return (default, err);
    }

    return (newUnixFile(ns, fd.name()), default);
}

} // end net_package
