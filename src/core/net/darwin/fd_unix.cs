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
    return (ret, default!);
}

internal static error init(this ж<netFD> Ꮡfd) {
    ref var fd = ref Ꮡfd.DerefOrNull();

    return Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true);
}

[GoRecv] internal static @string name(this ref netFD fd) {
    @string ls = default!;
    @string rs = default!;
    if (fd.laddr != default!) {
        ls = fd.laddr.String();
    }
    if (fd.raddr != default!) {
        rs = fd.raddr.String();
    }
    return fd.net + ":"u8 + ls + "->"u8 + rs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string connectˢ = "connect"u8;
internal static readonly @string getsockoptˢ = "getsockopt"u8;

internal static (syscall.Sockaddr rsa, error ret) connect(this ж<netFD> Ꮡfd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) {
    syscall.Sockaddr rsa = default!;
    error ret = default!;
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
            else if (AreEqual(exprᴛ1, default!) || AreEqual(exprᴛ1, syscall.EISCONN)) { matchᴛ1 = true;
                var selᴛ11 = ctx.Done();
                switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
                case 0 when selᴛ11.ꟷᐳ(out _): {
                    (rsa, ret) = (default!, mapErr(ctx.Err())); goto ᒐdone;
                }
                default: {
                    break;
                }}
                {
                    var errΔ2 = Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true); if (errΔ2 != default!) {
                        (rsa, ret) = (default!, errΔ2); goto ᒐdone;
                    }
                }
                runtime.KeepAlive(Ꮡfd.OrTypedNil());
                (rsa, ret) = (default!, default!); goto ᒐdone;
            }
            else if (AreEqual(exprᴛ1, syscall.EINVAL)) { matchᴛ1 = true;
                if (runtime.GOOS == "solaris"u8 || runtime.GOOS == "illumos"u8) {
                    // On Solaris and illumos we can see EINVAL if the socket has
                    // already been accepted and closed by the server.  Treat this
                    // as a successful connection--writes to the socket will see
                    // EOF.  For details and a test case in C see
                    // https://golang.org/issue/6828.
                    (rsa, ret) = (default!, default!); goto ᒐdone;
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1) { /* default: */
                (rsa, ret) = (default!, os.NewSyscallError(connectˢ, err)); goto ᒐdone;
            }
        }

        {
            var err = Ꮡfd.of(netFD.Ꮡpfd).Init(fd.net, true); if (err != default!) {
                (rsa, ret) = (default!, err); goto ᒐdone;
            }
        }
        {
            var (deadline, hasDeadline) = ctx.Deadline(); if (hasDeadline) {
                Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline(deadline);
                defer(Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline, noDeadline, ref ᒐ);
            }
        }
        // Start the "interrupter" goroutine, if this context might be canceled.
        //
        // The interrupter goroutine waits for the context to be done and
        // interrupts the dial (by altering the fd's write deadline, which
        // wakes up waitWrite).
        var ctxDone = ctx.Done();
        if (ctxDone != default!) {
            // Wait for the interrupter goroutine to exit before returning
            // from connect.
            var done = new channel<EmptyStruct>(0);
            var interruptRes = new channel<error>(0);
            var doneʗ1 = done;
            var interruptResʗ1 = interruptRes;
            defer(() => {
                builtin.close(doneʗ1);
                {
                    var ctxErr = ᐸꟷ(interruptResʗ1); if (ctxErr != default! && ret == default!) {
                        // The interrupter goroutine called SetWriteDeadline,
                        // but the connect code below had returned from
                        // waitWrite already and did a successful connect (ret
                        // == nil). Because we've now poisoned the connection
                        // by making it unwritable, don't return a successful
                        // dial. This was issue 16523.
                        ret = mapErr(ctxErr);
                        Ꮡfd.Close(); // prevent a leak
                    }
                }
            }, ref ᒐ);
            var ctxDoneʗ1 = ctxDone;
            var doneʗ2 = done;
            var interruptResʗ2 = interruptRes;
            goǃ(() => {
                var selᴛ12 = ctxDoneʗ1;
                var selᴛ13 = doneʗ2;
                switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
                case 0 when selᴛ12.ꟷᐳ(out _): {
                    Ꮡfd.of(netFD.Ꮡpfd).SetWriteDeadline(aLongTimeAgo);
                    testHookCanceledDial();
                    interruptResʗ2.ᐸꟷ(ctx.Err());
                    break;
                }
                case 1 when selᴛ13.ꟷᐳ(out _): {
                    interruptResʗ2.ᐸꟷ(default!);
                    break;
                }}
            });
        }
        // Force the runtime's poller to immediately give up
        // waiting for writability, unblocking waitWrite
        // below.
        while (ᐧ) {
            // Performing multiple connect system calls on a
            // non-blocking socket under Unix variants does not
            // necessarily result in earlier errors being
            // returned. Instead, once runtime-integrated network
            // poller tells us that the socket is ready, get the
            // SO_ERROR socket option to see if the connection
            // succeeded or failed. See issue 7474 for further
            // details.
            {
                var errΔ1 = fd.pfd.WaitWrite(); if (errΔ1 != default!) {
                    var selᴛ14 = ctxDone;
                    switch (trySelect(ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
                    case 0 when selᴛ14.ꟷᐳ(out _): {
                        (rsa, ret) = (default!, mapErr(ctx.Err())); goto ᒐdone;
                    }
                    default: {
                        break;
                    }}
                    (rsa, ret) = (default!, errΔ1); goto ᒐdone;
                }
            }
            var (nerr, err) = getsockoptIntFunc(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_ERROR);
            if (err != default!) {
                (rsa, ret) = (default!, os.NewSyscallError(getsockoptˢ, err)); goto ᒐdone;
            }
            {
                var errΔ1 = ((syscall.Errno)(uintptr)nerr);
                var exprᴛ2 = errΔ1;
                if (exprᴛ2 == syscall.EINPROGRESS || exprᴛ2 == syscall.EALREADY || exprᴛ2 == syscall.EINTR) {
                }
                else if (exprᴛ2 == syscall.EISCONN) {
                    (rsa, ret) = (default!, default!); goto ᒐdone;
                }
                else if (exprᴛ2 == ((syscall.Errno)0)) {
                    {
                        var (rsaΔ2, errΔ3) = syscall.Getpeername(fd.pfd.Sysfd); if (errΔ3 == default!) {
                            // The runtime poller can wake us up spuriously;
                            // see issues 14548 and 19289. Check that we are
                            // really connected; if not, wait again.
                            (rsa, ret) = (rsaΔ2, default!); goto ᒐdone;
                        }
                    }
                }
                else { /* default: */
                    (rsa, ret) = (default!, os.NewSyscallError(connectˢ, errΔ1)); goto ᒐdone;
                }
            }

            runtime.KeepAlive(Ꮡfd.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (rsa, ret);
}

internal static (ж<netFD> netfd, error err) accept(this ж<netFD> Ꮡfd) {
    ж<netFD> netfd = default!;
    error err = default!;

    ref var fd = ref Ꮡfd.DerefOrNull();
    (var d, var rsa, var errcall, err) = Ꮡfd.of(netFD.Ꮡpfd).Accept();
    if (err != default!) {
        if (errcall != ""u8) {
            err = wrapSyscallError(errcall, err);
        }
        return (default!, err);
    }
    {
        (netfd, err) = newFD(d, fd.family, fd.sotype, fd.net); if (err != default!) {
            poll.CloseFunc(d);
            return (default!, err);
        }
    }
    {
        err = netfd.init(); if (err != default!) {
            netfd.Close();
            return (default!, err);
        }
    }
    var (lsa, _) = syscall.Getsockname((~netfd).pfd.Sysfd);
    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (netfd, default!);
}

// Defined in os package.
[global::System.Diagnostics.StackTraceHidden] internal static ж<os.File> newUnixFile(nint fd, @string name) {
    return os.net_newUnixFile(fd, name);
}

internal static (ж<os.File> f, error err) dup(this ж<netFD> Ꮡfd) {
    error err = default!;

    ref var fd = ref Ꮡfd.DerefOrNull();
    (var ns, var call, err) = Ꮡfd.of(netFD.Ꮡpfd).Dup();
    if (err != default!) {
        if (call != ""u8) {
            err = os.NewSyscallError(call, err);
        }
        return (default!, err);
    }
    return (newUnixFile(ns, fd.name()), default!);
}

} // end net_package
