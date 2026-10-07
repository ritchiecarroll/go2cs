namespace go;

using errors = errors_package;
using fmt = fmt_package;
using Δnet = net_package;
using os = os_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tcpˢ = "tcp"u8;

internal static partial (Δnet.Conn client, Δnet.Conn server, Action cleanup, bool ok) pair() {
    Δnet.Conn client = default!;
    Δnet.Conn server = default!;

    var (listener, err) = Δnet.Listen(tcpˢ, "127.0.0.1:0"u8);
    if (err != default!) {
        return (default!, default!, () => {
        }, false);
    }
    var accepts = new channel<Δnet.Conn>(1);
    var acceptsʗ1 = accepts;
    var listenerʗ1 = listener;
    goǃ(() => {
        var (conn, errΔ1) = listenerʗ1.Accept();
        if (errΔ1 != default!) {
            acceptsʗ1.ᐸꟷ(default!);
            return;
        }
        acceptsʗ1.ᐸꟷ(conn);
    });
    (client, err) = Δnet.Dial(tcpˢ, listener.Addr().String());
    if (err != default!) {
        listener.Close();
        return (default!, default!, () => {
        }, false);
    }
    server = ᐸꟷ(accepts);
    if (server == default!) {
        client.Close();
        listener.Close();
        return (default!, default!, () => {
        }, false);
    }
    var listenerʗ2 = listener;
    return (client, server, () => {
        server.Close();
        client.Close();
        listenerʗ2.Close();
    }, true);
}

internal static bool isTimeout(error err) {
    return err != default! && errors.Is(err, os.ErrDeadlineExceeded);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object lifecycleSetupFailedˢ = (@string)"lifecycle: setup failed"u8;

internal static partial void deadlineLifecycle() {
    GoFrame ᒐ = default;
    try {
        var (client, server, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(lifecycleSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        var buf = new slice<byte>(16);
        client.SetReadDeadline(time.Now().Add(300 * time.Millisecond));
        var start = time.Now();
        var (_, err) = client.Read(buf);
        var blockedFor = time.Since(start);
        fmt.Printf("lifecycle: blockedReadTimesOut=%v parked=%v\n"u8, isTimeout(err), blockedFor >= 200 * time.Millisecond);
        start = time.Now();
        var (_, err2) = client.Read(buf);
        var stickyFor = time.Since(start);
        fmt.Printf("lifecycle: timeoutIsSticky=%v immediate=%v\n"u8, isTimeout(err2), stickyFor < 150 * time.Millisecond);
        client.SetReadDeadline(new time.Time(nil));
        var serverʗ1 = server;
        goǃ(() => {
            time.Sleep(50 * time.Millisecond);
            serverʗ1.Write(slice<byte>("clear"u8));
        });
        var (n, err3) = client.Read(buf);
        fmt.Printf("lifecycle: clearedDeadlineWorks=%v bytes=%v\n"u8, err3 == default!, n == 5);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pastSetupFailedˢ = (@string)"past: setup failed"u8;

internal static void pastDeadline() {
    GoFrame ᒐ = default;
    try {
        var (client, _, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(pastSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        client.SetReadDeadline(time.Now().Add(-time.ΔSecond));
        var buf = new slice<byte>(16);
        var start = time.Now();
        var (_, err) = client.Read(buf);
        var elapsed = time.Since(start);
        fmt.Printf("past: pastDeadlineNoBlock=%v immediate=%v\n"u8, isTimeout(err), elapsed < 150 * time.Millisecond);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object replaceSetupFailedˢ = (@string)"replace: setup failed"u8;

internal static partial void replacedWhileBlocked() {
    GoFrame ᒐ = default;
    try {
        var (client, server, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(replaceSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        client.SetReadDeadline(time.Now().Add(300 * time.Millisecond));
        var clientʗ1 = client;
        var serverʗ1 = server;
        goǃ(() => {
            time.Sleep(100 * time.Millisecond);
            clientʗ1.SetReadDeadline(time.Now().Add((time.Duration)(30000000000L)));
            time.Sleep(400 * time.Millisecond);
            serverʗ1.Write(slice<byte>("late"u8));
        });
        var buf = new slice<byte>(16);
        var (n, err) = client.Read(buf);
        fmt.Printf("replace: releasedByData=%v bytes=%v notTimeout=%v\n"u8, err == default!, n == 4, !isTimeout(err));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object modesSetupFailedˢ = (@string)"modes: setup failed"u8;

internal static partial void modeIndependence() {
    GoFrame ᒐ = default;
    try {
        var (client, server, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(modesSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        var serverʗ1 = server;
        goǃ(() => {
            var drain = new slice<byte>(64);
            while (ᐧ) {
                {
                    var (_, err) = serverʗ1.Read(drain); if (err != default!) {
                        return;
                    }
                }
            }
        });
        client.SetReadDeadline(time.Now().Add(-time.ΔSecond));
        var buf = new slice<byte>(16);
        var (_, readErr) = client.Read(buf);
        var (_, writeErr) = client.Write(slice<byte>("still writable"u8));
        fmt.Printf("modes: readExpired=%v writeModeIndependent=%v\n"u8, isTimeout(readErr), writeErr == default!);
        client.SetDeadline(time.Now().Add(-time.ΔSecond));
        var (_, readErr2) = client.Read(buf);
        var (_, writeErr2) = client.Write(slice<byte>("now neither"u8));
        fmt.Printf("modes: combinedModeBoth=%v\n"u8, isTimeout(readErr2) && isTimeout(writeErr2));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object orderSetupFailedˢ = (@string)"order: setup failed"u8;

internal static void closeBeatsTimeout() {
    GoFrame ᒐ = default;
    try {
        var (client, _, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(orderSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        client.SetReadDeadline(time.Now().Add(-time.ΔSecond));
        var buf = new slice<byte>(16);
        client.Read(buf);
        client.Close();
        var (_, err) = client.Read(buf);
        fmt.Printf("order: closeBeatsTimeout=%v notTimeout=%v\n"u8,
            err != default! && errors.Is(err, Δnet.ErrClosed), !isTimeout(err));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object raceSetupFailedˢ = (@string)"race: setup failed"u8;

internal static partial void deadlineVersusData() {
    GoFrame ᒐ = default;
    try {
        var (client, server, cleanup, ok) = pair();
        if (!ok) {
            fmt.Println(raceSetupFailedˢ);
            return;
        }
        var cleanupʗ1 = cleanup;
        defer(cleanupʗ1, ref ᒐ);
        var buf = new slice<byte>(16);
        server.Write(slice<byte>("ready"u8));
        time.Sleep(200 * time.Millisecond);
        client.SetReadDeadline(time.Now().Add(-time.ΔSecond));
        var (n, err) = client.Read(buf);
        fmt.Printf("race: expiredBeatsBufferedData=%v noBytes=%v\n"u8, isTimeout(err), n == 0);
        client.SetReadDeadline(time.Now().Add(2 * time.ΔSecond));
        var (n2, err2) = client.Read(buf);
        fmt.Printf("race: dataInsideDeadline=%v bytes=%v\n"u8, err2 == default!, n2 == 5);
        client.SetReadDeadline(new time.Time(nil));
        var serverʗ1 = server;
        goǃ(() => {
            time.Sleep(50 * time.Millisecond);
            serverʗ1.Write(slice<byte>("after"u8));
        });
        var (n3, err3) = client.Read(buf);
        fmt.Printf("race: noStaleExpiryAfterwards=%v bytes=%v\n"u8, err3 == default!, n3 == 5);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doneˢ = (@string)"done"u8;

internal static void Main() {
    deadlineLifecycle();
    pastDeadline();
    replacedWhileBlocked();
    modeIndependence();
    closeBeatsTimeout();
    deadlineVersusData();
    fmt.Println(doneˢ);
}

} // end main_package
