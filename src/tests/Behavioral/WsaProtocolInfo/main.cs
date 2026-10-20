namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        ref var data = ref heap(new syscall.WSAData(), out var Ꮡdata);
        {
            var errΔ1 = syscall.WSAStartup((uint32)0x202, Ꮡdata); if (errΔ1 != default!) {
                fmt.Println("FATAL WSAStartup", errΔ1);
                return;
            }
        }
        defer(() => syscall.WSACleanup(), ref ᒐ);

        fmt.Println("-- WSAStartup --");
        fmt.Println("version negotiated:", data.Version == 0x202);
        fmt.Println("high version:", data.HighVersion == 0x202);
        fmt.Println("description printable:", printableASCII(data.Description[..]));

        fmt.Println("-- WSAEnumProtocols --");

        ref var protos = ref heap<array<int32>>(out var Ꮡprotos);
        protos = new int32[]{syscall.IPPROTO_TCP, 0}.array();

        ref var buf = ref heap(new array<syscall.WSAProtocolInfo>(32, () => new()), out var Ꮡbuf);
        ref var length = ref heap<uint32>(out var Ꮡlength);
        length = (uint32)/* unsafe.Sizeof(buf) */ (uintptr)20096;

        var (n, err) = syscall.WSAEnumProtocols(Ꮡprotos.at<int32>(0), Ꮡbuf.at<syscall.WSAProtocolInfo>(0), Ꮡlength);
        if (err != default!) {
            fmt.Println("FATAL WSAEnumProtocols", err);
            return;
        }

        fmt.Println("entry count in range:", n >= 1 && n <= (int32)len(buf));

        var (chainOK, familyOK, typeOK, protoOK, nameOK) = (true, true, true, true, true);
        for (var i = (int32)0; i < n; i++) {
            var p = buf[i].ΔClone();
            if (p.ProtocolChain.ChainLen < 0 || p.ProtocolChain.ChainLen > syscall.MAX_PROTOCOL_CHAIN) {
                chainOK = false;
            }
            if (p.AddressFamily != syscall.AF_INET && p.AddressFamily != syscall.AF_INET6) {
                familyOK = false;
            }
            if (p.SocketType != syscall.SOCK_STREAM) {
                typeOK = false;
            }
            if (p.Protocol != syscall.IPPROTO_TCP) {
                protoOK = false;
            }
            if (!printableUTF16(p.ProtocolName[..])) {
                nameOK = false;
            }
        }

        fmt.Println("chain length in range:", chainOK);
        fmt.Println("address family is inet:", familyOK);
        fmt.Println("socket type is stream:", typeOK);
        fmt.Println("protocol is tcp:", protoOK);
        fmt.Println("protocol name printable:", nameOK);

        var ifs = true;
        for (var i = (int32)0; i < n; i++) {
            if ((buf[i].ServiceFlags1 & (uint32)syscall.XP1_IFS_HANDLES) == 0) {
                ifs = false;
            }
        }
        fmt.Println("every entry carries XP1_IFS_HANDLES:", ifs);

        fmt.Println("-- required size --");

        var record = (uint32)/* unsafe.Sizeof(buf[0]) */ (uintptr)628;
        ref var @required = ref heap<uint32>(out var Ꮡrequired);
        @required = (uint32)0;

        (var m, err) = syscall.WSAEnumProtocols(Ꮡprotos.at<int32>(0), Ꮡbuf.at<syscall.WSAProtocolInfo>(0), Ꮡrequired);
        fmt.Println("undersized buffer refused:", m == -1 && err != default!);
        fmt.Println("required size is whole records:", @required >= record && @required % record == 0);
        fmt.Println("required size matches entry count:", @required == (uint32)n * record);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool printableASCII(slice<byte> b) {
    if (len(b) == 0 || b[0] == 0) {
        return false;
    }
    foreach (var (_, c) in b) {
        if (c == 0) {
            return true;
        }
        if (c < 0x20 || c > 0x7e) {
            return false;
        }
    }
    return false;
}

internal static bool printableUTF16(slice<uint16> s) {
    if (len(s) == 0 || s[0] == 0) {
        return false;
    }
    foreach (var (_, r) in s) {
        if (r == 0) {
            return true;
        }
        if (r < 0x20 || r > 0x7e) {
            return false;
        }
    }
    return false;
}

} // end main_package
