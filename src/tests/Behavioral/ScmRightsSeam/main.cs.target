namespace go;

using fmt = fmt_package;
using syscall = syscall_package;

partial class main_package {

internal static void fatal(@string what, error err) {
    if (err != nil) {
        fmt.Println(what, "failed:", err);
        throw panic(what);
    }
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        @string secret = "scm-rights-payload";

        array<nint> pipeFds = new(2);
        fatal("pipe2", syscall.Pipe2(pipeFds[..], 0));
        nint pipeRead = pipeFds[0];
        nint pipeWrite = pipeFds[1];
        defer(syscall.Close, pipeRead, ref ᒐ);

        var (n, err) = syscall.Write(pipeWrite, slice<byte>(secret));
        fatal("write(pipe)", err);
        fatal("close(pipeWrite)", syscall.Close(pipeWrite));
        fmt.Println("bytes staged in the pipe:", n == len(secret));

        (var pair, err) = syscall.Socketpair(syscall.AF_UNIX, syscall.SOCK_STREAM, 0);
        fatal("socketpair", err);
        nint sender = pair[0];
        nint receiver = pair[1];
        defer(syscall.Close, sender, ref ᒐ);
        defer(syscall.Close, receiver, ref ᒐ);

        var rights = syscall.UnixRights(pipeRead);
        fmt.Println("control image is non-empty:", len(rights) > 0);

        (var sent, err) = syscall.SendmsgN(sender, new byte[]{(rune)'x'}.slice(), rights, default, 0);
        fatal("sendmsg", err);
        fmt.Println("payload bytes sent:", sent == 1);

        var payload = new slice<byte>(8);
        var oob = new slice<byte>(syscall.CmsgSpace(4));
        (var rn, var oobn, _, _, err) = syscall.Recvmsg(receiver, payload, oob, 0);
        fatal("recvmsg", err);
        fmt.Println("payload byte received:", rn == 1 && payload[0] == (rune)'x');
        fmt.Println("control bytes received:", oobn == len(oob));

        (var scms, err) = syscall.ParseSocketControlMessage(oob.slice(0, oobn));
        fatal("parse control", err);
        fmt.Println("control messages:", len(scms));

        (var fds, err) = syscall.ParseUnixRights(Ꮡ(scms, 0));
        fatal("parse rights", err);
        fmt.Println("descriptors received:", len(fds));

        var got = new slice<byte>(len(secret));
        (rn, err) = syscall.Read(fds[0], got);
        fatal("read(received fd)", err);
        syscall.Close(fds[0]);
        fmt.Println("received descriptor reads the staged bytes:", rn == len(secret) && ((sstring)(got.slice(0, rn))) == secret);

        array<nint> pipe2Fds = new(2);
        fatal("pipe2 (phase 2)", syscall.Pipe2(pipe2Fds[..], 0));
        nint pipe2Read = pipe2Fds[0];
        nint pipe2Write = pipe2Fds[1];
        defer(syscall.Close, pipe2Read, ref ᒐ);

        (var n2, err) = syscall.Write(pipe2Write, slice<byte>(secret));
        fatal("write(pipe 2)", err);
        fatal("close(pipe2Write)", syscall.Close(pipe2Write));
        fmt.Println("bytes staged in the second pipe:", n2 == len(secret));

        (var pair2, err) = syscall.Socketpair(syscall.AF_UNIX, syscall.SOCK_STREAM, 0);
        fatal("socketpair (phase 2)", err);
        nint sender2 = pair2[0];
        nint receiver2 = pair2[1];
        defer(syscall.Close, sender2, ref ᒐ);
        defer(syscall.Close, receiver2, ref ᒐ);

        var rights2 = syscall.UnixRights(pipe2Read);
        (var sent2, err) = syscall.SendmsgN(sender2, default, rights2, default, 0);
        fatal("sendmsg (control only)", err);
        fmt.Println("control-only send reports no payload bytes:", sent2 == 0);

        var payload2 = new slice<byte>(8);
        var oob2 = new slice<byte>(syscall.CmsgSpace(4));
        (_, var oobn2, _, _, err) = syscall.Recvmsg(receiver2, payload2, oob2, 0);
        fatal("recvmsg (phase 2)", err);
        (var scms2, err) = syscall.ParseSocketControlMessage(oob2.slice(0, oobn2));
        fatal("parse control (phase 2)", err);
        (var fds2, err) = syscall.ParseUnixRights(Ꮡ(scms2, 0));
        fatal("parse rights (phase 2)", err);

        var got2 = new slice<byte>(len(secret));
        (var rn2, err) = syscall.Read(fds2[0], got2);
        fatal("read(received fd, phase 2)", err);
        syscall.Close(fds2[0]);
        fmt.Println("control-only descriptor reads the staged bytes:", rn2 == len(secret) && ((sstring)(got2.slice(0, rn2))) == secret);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
