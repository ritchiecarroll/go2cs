// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δos = os_package;
using exec = go.os.exec_package;
using Δregexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.os;
using static go.net_package;
using ꓸꓸꓸstring = Span<@string>;

partial class net_internal_test_package {

internal static (syscall.Errno, bool) toErrno(error err) {
    var (operr, ok) = err._<ж<global::go.net_package.OpError>>(ᐧ);
    if (!ok) {
        return (0, false);
    }
    (var syserr, ok) = (~operr).Err._<ж<Δos.SyscallError>>(ᐧ);
    if (!ok) {
        return (0, false);
    }
    (var errno, ok) = (~syserr).Err._<syscall.Errno>(ᐧ);
    if (!ok) {
        return (0, false);
    }
    return (errno, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gotestDialAddrˢ = "GOTEST_DIAL_ADDR"u8;
internal static readonly @string testRunˢ = "-test.run=TestAcceptIgnoreSomeErrors"u8;
internal static readonly @string abcˢ = "abc"u8;

// TestAcceptIgnoreSomeErrors tests that windows TCPListener.AcceptTCP
// handles broken connections. It verifies that broken connections do
// not affect future connections.
public static partial void TestAcceptIgnoreSomeErrors(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        (@string, error) recv(global::go.net_package.Listener lnΔ1, bool ignoreSomeReadErrors) {
            GoFrame ᒐ = default;
            try {
                var (c, errΔ1) = lnΔ1.Accept();
                if (errΔ1 != default!) {
                    // Display windows errno in error message.
                    var (errnoΔ1, okΔ1) = toErrno(errΔ1);
                    if (!okΔ1) {
                        return ("", errΔ1);
                    }
                    return ("", fmt.Errorf("%v (windows errno=%d)"u8, errΔ1, errnoΔ1));
                }
                var cʗ1 = c;
                defer(() => cʗ1.Close(), ref ᒐ);
                var b = new slice<byte>(100);
                (var n, errΔ1) = c.Read(b);
                if (errΔ1 == default! || AreEqual(errΔ1, Δio.EOF)) {
                    return (((@string)(b.slice(0, n))), default!);
                }
                var (errno, ok) = toErrno(errΔ1);
                if (ok && ignoreSomeReadErrors && (errno == syscall.ERROR_NETNAME_DELETED || errno == syscall.WSAECONNRESET)) {
                    return ("", default!);
                }
                return ("", errΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        error send(@string addr, @string data) {
            GoFrame ᒐ = default;
            try {
                var (c, errΔ2) = Dial(tcpˢ, addr);
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                var cʗ2 = c;
                defer(() => cʗ2.Close(), ref ᒐ);
                var b = slice<byte>(data);
                (var n, errΔ2) = c.Write(b);
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                if (n != len(b)) {
                    return fmt.Errorf(@"Only %d chars of string ""%s"" sent"u8, n, data);
                }
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        {
            @string envaddr = Δos.Getenv(gotestDialAddrˢ); if (envaddr != ""u8) {
                // In child process.
                var (c, errΔ3) = Dial(tcpˢ, envaddr);
                if (errΔ3 != default!) {
                    Ꮡt.Fatal(errΔ3);
                }
                fmt.Printf("sleeping\n"u8);
                time.Sleep(time.ΔMinute); // process will be killed here
                c.Close();
            }
        }
        var (ln, err) = Listen(tcpˢ, "127.0.0.1:0"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var lnʗ1 = ln;
        defer(() => lnʗ1.Close(), ref ᒐ);
        // Start child process that connects to our listener.
        var cmd = exec.Command(Δos.Args[0], testRunˢ);
        cmd.Value.Env = append(Δos.Environ(), "GOTEST_DIAL_ADDR="u8 + ln.Addr().String());
        (var stdout, err) = cmd.StdoutPipe();
        if (err != default!) {
            Ꮡt.Fatalf("cmd.StdoutPipe failed: %v"u8, err);
        }
        err = cmd.Start();
        if (err != default!) {
            Ꮡt.Fatalf("cmd.Start failed: %v\n"u8, err);
        }
        var outReader = bufio.NewReader(stdout);
        while (ᐧ) {
            var (sΔ1, errΔ4) = outReader.ReadString((rune)'\n');
            if (errΔ4 != default!) {
                Ꮡt.Fatalf("reading stdout failed: %v"u8, errΔ4);
            }
            if (sΔ1 == "sleeping\n"u8) {
                break;
            }
        }
        var cmdʗ1 = cmd;
        defer(() => cmdʗ1.Wait(), ref ᒐ); // ignore error - we know it is getting killed
        time.Duration alittle = /* 100 * time.Millisecond */ 100000000;
        time.Sleep(alittle);
        (~cmd).Process.Kill(); // the only way to trigger the errors
        time.Sleep(alittle);
        // Send second connection data (with delay in a separate goroutine).
        var result = new channel<error>(0);
        var lnʗ2 = ln;
        var resultʗ1 = result;
        var sendʗ1 = send;
        goǃ(() => {
            time.Sleep(alittle);
            var errΔ5 = sendʗ1(lnʗ2.Addr().String(), abcˢ);
            if (errΔ5 != default!) {
                resultʗ1.ᐸꟷ(errΔ5);
            }
            resultʗ1.ᐸꟷ(default!);
        });
        var resultʗ2 = result;
        defer(() => {
            var errΔ6 = ᐸꟷ(resultʗ2);
            if (errΔ6 != default!) {
                Ꮡt.Fatalf("send failed: %v"u8, errΔ6);
            }
        }, ref ᒐ);
        // Receive first or second connection.
        (var s, err) = recv(ln, true);
        if (err != default!) {
            Ꮡt.Fatalf("recv failed: %v"u8, err);
        }
        var exprᴛ1 = s;
        if (exprᴛ1 == ""u8) {
        }
        else if (exprᴛ1 == "abc"u8) {
            return;
        }
        else { /* default: */
            Ꮡt.Fatalf(@"""%s"" received from recv, but """" or ""abc"" expected"u8, // First connection data is received, let's get second connection data.
 // First connection is lost forever, but that is ok.
 s);
        }

        // Get second connection data.
        (s, err) = recv(ln, false);
        if (err != default!) {
            Ꮡt.Fatalf("recv failed: %v"u8, err);
        }
        if (s != "abc"u8) {
            Ꮡt.Fatalf(@"""%s"" received from recv, but ""abc"" expected"u8, s);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netcmdˢ = "netcmd"u8;
internal static readonly @string powershellˢ = "powershell"u8;
internal static readonly @string commandˢ = "-Command"u8;

internal static (slice<byte>, error) runCmd(params ꓸꓸꓸstring argsʗp) {
    GoFrame ᒐ = default;
    try {
        var args = argsʗp.slice();

        slice<byte> removeUTF8BOM(slice<byte> b) {
            if (len(b) >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) {
                return b[3..];
            }
            return b;
        }
        var (f, err) = Δos.CreateTemp(""u8, netcmdˢ);
        if (err != default!) {
            return (default!, err);
        }
        f.Close();
        defer(Δos.Remove, f.Name(), ref ᒐ);
        @string cmd = fmt.Sprintf(@"%s | Out-File ""%s"" -encoding UTF8"u8, strings.Join(args, " "u8), f.Name());
        (var @out, err) = exec.Command(powershellˢ, commandˢ, cmd).CombinedOutput();
        if (err != default!) {
            if (len(@out) != 0) {
                return (default!, fmt.Errorf("%s failed: %v: %q"u8, args[0], err, ((@string)removeUTF8BOM(@out))));
            }
            error err2 = default!;
            (@out, err2) = Δos.ReadFile(f.Name());
            if (err2 != default!) {
                return (default!, err2);
            }
            if (len(@out) != 0) {
                return (default!, fmt.Errorf("%s failed: %v: %q"u8, args[0], err, ((@string)removeUTF8BOM(@out))));
            }
            return (default!, fmt.Errorf("%s failed: %v"u8, args[0], err));
        }
        (@out, err) = Δos.ReadFile(f.Name());
        if (err != default!) {
            return (default!, err);
        }
        return (removeUTF8BOM(@out), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string netshˢ = "netsh"u8;
internal static readonly @string helpˢ = "help"u8;

internal static void checkNetsh(ж<testing.T> Ꮡt) {
    if (testenv.Builder() == "windows-arm64-10"u8) {
        // netsh was observed to sometimes hang on this builder.
        // We have not observed failures on windows-arm64-11, so for the
        // moment we are leaving the test enabled elsewhere on the theory
        // that it may have been a platform bug fixed in Windows 11.
        testenv.SkipFlaky(new net_test_package.testing_TжTB(Ꮡt), 52082);
    }
    var (@out, err) = runCmd(netshˢ, helpˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (bytes.Contains(@out, slice<byte>("The following helper DLL cannot be loaded"u8))) {
        Ꮡt.Skipf("powershell failure:\n%s"u8, err);
    }
    if (!bytes.Contains(@out, slice<byte>("The following commands are available:"u8))) {
        Ꮡt.Skipf("powershell does not speak English:\n%s"u8, @out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string interfaceˢ = "interface"u8;
internal static readonly @string showˢ = "show"u8;
internal static readonly @string levelVerboseˢ = "level=verbose"u8;

internal static error netshInterfaceIPShowInterface(@string ipver, map<@string, bool> ifaces) {
    var (@out, err) = runCmd(netshˢ, interfaceˢ, ipver, showˢ, interfaceˢ, levelVerboseˢ);
    if (err != default!) {
        return err;
    }
    // interface information is listed like:
    //
    //Interface Local Area Connection Parameters
    //----------------------------------------------
    //IfLuid                             : ethernet_6
    //IfIndex                            : 11
    //State                              : connected
    //Metric                             : 10
    //...
    @string name = default!;
    var lines = bytes.Split(@out, new byte[]{(rune)'\r', (rune)'\n'}.slice());
    foreach (var (_, line) in lines) {
        if (bytes.HasPrefix(line, slice<byte>("Interface "u8)) && bytes.HasSuffix(line, slice<byte>(" Parameters"u8))) {
            var f = line[(int)(len("Interface "))..];
            f = f.slice(0, len(f) - len(" Parameters"));
            name = ((@string)f);
            continue;
        }
        bool isup = default!;
        var exprᴛ1 = ((sstring)line);
        if (exprᴛ1 == "State                              : connected"u8) {
            isup = true;
        }
        else if (exprᴛ1 == "State                              : disconnected"u8) {
            isup = false;
        }
        else { /* default: */
            continue;
        }

        if (name != ""u8) {
            {
                var (v, ok) = ifaces[name, ꟷ]; if (ok && v != isup) {
                    return fmt.Errorf("%s:%s isup=%v: ipv4 and ipv6 report different interface state"u8, ipver, name, isup);
                }
            }
            ifaces[name] = isup;
            name = ""u8;
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ipv6ˢ = "ipv6"u8;
internal static readonly @string ipv4ˢ = "ipv4"u8;

public static void TestInterfacesWithNetsh(ж<testing.T> Ꮡt) {
    checkNetsh(Ꮡt);
    @string toString(@string name, bool isup) {
        if (isup) {
            return name + ":up"u8;
        }
        return name + ":down"u8;
    }
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var have = new slice<@string>(0);
    foreach (var (_, ifi) in ift) {
        have = append(have, toString(ifi.Name, (global::go.net_package.Flags)(ifi.Flags & FlagUp) != 0));
    }
    slices.Sort<slice<@string>, @string>(have);
    var ifaces = new map<@string, bool>();
    err = netshInterfaceIPShowInterface(ipv6ˢ, ifaces);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = netshInterfaceIPShowInterface(ipv4ˢ, ifaces);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var want = new slice<@string>(0);
    foreach (var (name, isup) in ifaces) {
        want = append(want, toString(name, isup));
    }
    slices.Sort<slice<@string>, @string>(want);
    if (strings.Join(want, "/"u8) != strings.Join(have, "/"u8)) {
        Ꮡt.Fatalf("unexpected interface list %q, want %q"u8, have, want);
    }
}

internal static slice<@string> netshInterfaceIPv4ShowAddress(@string name, slice<byte> netshOutput) {
    // Address information is listed like:
    //
    //Configuration for interface "Local Area Connection"
    //    DHCP enabled:                         Yes
    //    IP Address:                           10.0.0.2
    //    Subnet Prefix:                        10.0.0.0/24 (mask 255.255.255.0)
    //    IP Address:                           10.0.0.3
    //    Subnet Prefix:                        10.0.0.0/24 (mask 255.255.255.0)
    //    Default Gateway:                      10.0.0.254
    //    Gateway Metric:                       0
    //    InterfaceMetric:                      10
    //
    //Configuration for interface "Loopback Pseudo-Interface 1"
    //    DHCP enabled:                         No
    //    IP Address:                           127.0.0.1
    //    Subnet Prefix:                        127.0.0.0/8 (mask 255.0.0.0)
    //    InterfaceMetric:                      50
    //
    var addrs = new slice<@string>(0);
    @string addr = default!;
    @string subnetprefix = default!;
    bool processingOurInterface = default!;
    var lines = bytes.Split(netshOutput, new byte[]{(rune)'\r', (rune)'\n'}.slice());
    foreach (var (_, line) in lines) {
        if (!processingOurInterface) {
            if (!bytes.HasPrefix(line, slice<byte>("Configuration for interface"u8))) {
                continue;
            }
            if (!bytes.Contains(line, slice<byte>(@"""" + name + @""""))) {
                continue;
            }
            processingOurInterface = true;
            continue;
        }
        if (len(line) == 0) {
            break;
        }
        if (bytes.Contains(line, slice<byte>("Subnet Prefix:"u8))) {
            var f = bytes.Split(line, new byte[]{(rune)':'}.slice());
            if (len(f) == 2) {
                f = bytes.Split(f[1], new byte[]{(rune)'('}.slice());
                if (len(f) == 2) {
                    f = bytes.Split(f[0], new byte[]{(rune)'/'}.slice());
                    if (len(f) == 2) {
                        subnetprefix = ((@string)bytes.TrimSpace(f[1]));
                        if (addr != ""u8 && subnetprefix != ""u8) {
                            addrs = append(addrs, addr + "/"u8 + subnetprefix);
                        }
                    }
                }
            }
        }
        addr = ""u8;
        if (bytes.Contains(line, slice<byte>("IP Address:"u8))) {
            var f = bytes.Split(line, new byte[]{(rune)':'}.slice());
            if (len(f) == 2) {
                addr = ((@string)bytes.TrimSpace(f[1]));
            }
        }
    }
    return addrs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dDDDˢ = @":\d+\.\d+\.\d+\.\d+$"u8;

internal static slice<@string> netshInterfaceIPv6ShowAddress(@string name, slice<byte> netshOutput) {
    // Address information is listed like:
    //
    //Address ::1 Parameters
    //---------------------------------------------------------
    //Interface Luid     : Loopback Pseudo-Interface 1
    //Scope Id           : 0.0
    //Valid Lifetime     : infinite
    //Preferred Lifetime : infinite
    //DAD State          : Preferred
    //Address Type       : Other
    //Skip as Source     : false
    //
    //Address XXXX::XXXX:XXXX:XXXX:XXXX%11 Parameters
    //---------------------------------------------------------
    //Interface Luid     : Local Area Connection
    //Scope Id           : 0.11
    //Valid Lifetime     : infinite
    //Preferred Lifetime : infinite
    //DAD State          : Preferred
    //Address Type       : Other
    //Skip as Source     : false
    //
    // TODO: need to test ipv6 netmask too, but netsh does not outputs it
    @string addr = default!;
    var addrs = new slice<@string>(0);
    var lines = bytes.Split(netshOutput, new byte[]{(rune)'\r', (rune)'\n'}.slice());
    foreach (var (_, line) in lines) {
        if (addr != ""u8) {
            if (len(line) == 0) {
                addr = ""u8;
                continue;
            }
            if (((@string)line) != "Interface Luid     : "u8 + name) {
                continue;
            }
            addrs = append(addrs, addr);
            addr = ""u8;
            continue;
        }
        if (!bytes.HasPrefix(line, slice<byte>("Address"u8))) {
            continue;
        }
        if (!bytes.HasSuffix(line, slice<byte>("Parameters"u8))) {
            continue;
        }
        var f = bytes.Split(line, new byte[]{(rune)' '}.slice());
        if (len(f) != 3) {
            continue;
        }
        // remove scope ID if present
        f = bytes.Split(f[1], new byte[]{(rune)'%'}.slice());
        // netsh can create IPv4-embedded IPv6 addresses, like fe80::5efe:192.168.140.1.
        // Convert these to all hexadecimal fe80::5efe:c0a8:8c01 for later string comparisons.
        var ipv4Tail = Δregexp.MustCompile(dDDDˢ);
        if (ipv4Tail.Match(f[0])) {
            f[0] = slice<byte>(ParseIP(((@string)f[0])).String());
        }
        addr = ((@string)bytes.ToLower(bytes.TrimSpace(f[0])));
    }
    return addrs;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addressˢ = "address"u8;

public static void TestInterfaceAddrsWithNetsh(ж<testing.T> Ꮡt) {
    checkNetsh(Ꮡt);
    var (outIPV4, err) = runCmd(netshˢ, interfaceˢ, ipv4ˢ, showˢ, addressˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var outIPV6, err) = runCmd(netshˢ, interfaceˢ, ipv6ˢ, showˢ, addressˢ, levelVerboseˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, vᴛ1) in ift) {
        ref var ifi = ref heap(new global::go.net_package.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        // Skip the interface if it's down.
        if (((global::go.net_package.Flags)(ifi.Flags & FlagUp)) == 0) {
            continue;
        }
        var have = new slice<@string>(0);
        var (addrs, errΔ1) = Ꮡifi.Addrs();
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        foreach (var (_, addr) in addrs) {
            switch (addr.type()) {
            case ж<global::go.net_package.IPNet> addrΔ1: {
                if ((~addrΔ1).IP.To4() != default!) {
                    have = append(have, addrΔ1.String());
                }
                if ((~addrΔ1).IP.To16() != default! && (~addrΔ1).IP.To4() == default!) {
                    // netsh does not output netmask for ipv6, so ignore ipv6 mask
                    have = append(have, (~addrΔ1).IP.String());
                }
                break;
            }
            case ж<global::go.net_package.IPAddr> addrΔ1: {
                if ((~addrΔ1).IP.To4() != default!) {
                    have = append(have, addrΔ1.String());
                }
                if ((~addrΔ1).IP.To16() != default! && (~addrΔ1).IP.To4() == default!) {
                    // netsh does not output netmask for ipv6, so ignore ipv6 mask
                    have = append(have, (~addrΔ1).IP.String());
                }
                break;
            }}
        }
        slices.Sort<slice<@string>, @string>(have);
        var want = netshInterfaceIPv4ShowAddress(ifi.Name, outIPV4);
        var wantIPv6 = netshInterfaceIPv6ShowAddress(ifi.Name, outIPV6);
        want = appendꓸꓸꓸ(want, wantIPv6);
        slices.Sort<slice<@string>, @string>(want);
        if (strings.Join(want, "/"u8) != strings.Join(have, "/"u8)) {
            Ꮡt.Errorf("%s: unexpected addresses list %q, want %q"u8, ifi.Name, have, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getmacˢ = "getmac"u8;
internal static readonly @string termGetmacIsNotˢ = "term 'getmac' is not recognized as the name of a cmdlet"u8;

// check that getmac exists as a powershell command, and that it
// speaks English.
internal static void checkGetmac(ж<testing.T> Ꮡt) {
    var (@out, err) = runCmd(getmacˢ, "/?");
    if (err != default!) {
        if (strings.Contains(err.Error(), termGetmacIsNotˢ)) {
            Ꮡt.Skipf("getmac not available"u8);
        }
        Ꮡt.Fatal(err);
    }
    if (!bytes.Contains(@out, slice<byte>("network adapters on a system"u8))) {
        Ꮡt.Skipf("skipping test on non-English system"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string listˢ = "list"u8;
internal static readonly @string transportNameˢ = "Transport Name"u8;
internal static readonly @string physicalAddressˢ = "Physical Address"u8;
internal static readonly @string connectionNameˢ = "Connection Name"u8;

public static void TestInterfaceHardwareAddrWithGetmac(ж<testing.T> Ꮡt) {
    checkGetmac(Ꮡt);
    var (ift, err) = Interfaces();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var have = new map<@string, @string>();
    foreach (var (_, ifi) in ift) {
        if ((global::go.net_package.Flags)(ifi.Flags & FlagLoopback) != 0) {
            // no MAC address for loopback interfaces
            continue;
        }
        have[ifi.Name] = ifi.HardwareAddr.String();
    }
    (var @out, err) = runCmd(getmacˢ, "/fo", listˢ, "/v");
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // getmac output looks like:
    //
    //Connection Name:  Local Area Connection
    //Network Adapter:  Intel Gigabit Network Connection
    //Physical Address: XX-XX-XX-XX-XX-XX
    //Transport Name:   \Device\Tcpip_{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}
    //
    //Connection Name:  Wireless Network Connection
    //Network Adapter:  Wireles WLAN Card
    //Physical Address: XX-XX-XX-XX-XX-XX
    //Transport Name:   Media disconnected
    //
    //Connection Name:  Bluetooth Network Connection
    //Network Adapter:  Bluetooth Device (Personal Area Network)
    //Physical Address: N/A
    //Transport Name:   Hardware not present
    //
    //Connection Name:  VMware Network Adapter VMnet8
    //Network Adapter:  VMware Virtual Ethernet Adapter for VMnet8
    //Physical Address: Disabled
    //Transport Name:   Disconnected
    //
    var want = new map<@string, @string>();
    ref var group = ref heap<map<@string, @string>>(out var Ꮡgroup);
    group = new map<@string, @string>(); // name / values for single adapter
    @string getValue(@string name) {
        var (value, found) = Ꮡgroup.ValueSlot[name, ꟷ];
        if (!found) {
            Ꮡt.Fatalf("%q has no %q line in it"u8, Ꮡgroup.ValueSlot, name);
        }
        if (value == ""u8) {
            Ꮡt.Fatalf("%q has empty %q value"u8, Ꮡgroup.ValueSlot, name);
        }
        return value;
    }
    var getValueʗ1 = getValue;
    var wantʗ1 = want;
    void processGroup() {
        if (len(Ꮡgroup.ValueSlot) == 0) {
            return;
        }
        @string tname = strings.ToLower(getValueʗ1(transportNameˢ));
        if (tname == "n/a"u8) {
            // skip these
            return;
        }
        @string addr = strings.ToLower(getValueʗ1(physicalAddressˢ));
        if (addr == "disabled"u8 || addr == "n/a"u8) {
            // skip these
            return;
        }
        addr = strings.ReplaceAll(addr, "-"u8, ":"u8);
        @string cname = getValueʗ1(connectionNameˢ);
        wantʗ1[cname] = addr;
        Ꮡgroup.ValueSlot = new map<@string, @string>();
    }
    var lines = bytes.Split(@out, new byte[]{(rune)'\r', (rune)'\n'}.slice());
    foreach (var (_, line) in lines) {
        if (len(line) == 0) {
            processGroup();
            continue;
        }
        nint i = bytes.IndexByte(line, (rune)':');
        if (i == -1) {
            Ꮡt.Fatalf("line %q has no : in it"u8, line);
        }
        group[((@string)(line.slice(0, i)))] = ((@string)bytes.TrimSpace(line.slice(i + 1)));
    }
    processGroup();
    var dups = new map<@string, slice<@string>>();
    foreach (var (name, addr) in want) {
        {
            var (_, ok) = dups[addr, ꟷ]; if (!ok) {
                dups[addr] = new slice<@string>(0);
            }
        }
        dups[addr] = append(dups[addr], name);
    }
nextWant:
    foreach (var (name, wantAddr) in want) {
        {
            var (haveAddr, ok) = have[name, ꟷ]; if (ok) {
                if (haveAddr != wantAddr) {
                    Ꮡt.Errorf("unexpected MAC address for %q - %v, want %v"u8, name, haveAddr, wantAddr);
                }
                continue;
            }
        }
        // We could not find the interface in getmac output by name.
        // But sometimes getmac lists many interface names
        // for the same MAC address. If that is the case here,
        // and we can match at least one of those names,
        // let's ignore the other names.
        {
            var (dupNames, ok) = dups[wantAddr, ꟷ]; if (ok && len(dupNames) > 1) {
                foreach (var (_, dupName) in dupNames) {
                    {
                        var (haveAddr, okΔ1) = have[dupName, ꟷ]; if (okΔ1 && haveAddr == wantAddr) {
                            goto continue_nextWant;
                        }
                    }
                }
            }
        }
        Ꮡt.Errorf("getmac lists %q, but it could not be found among Go interfaces %v"u8, name, have);
continue_nextWant:;
    }
break_nextWant:;
}

} // end net_internal_test_package
