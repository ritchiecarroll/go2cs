// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows && !plan9
namespace go.log;

using errors = errors_package;
using fmt = fmt_package;
using log = log_package;
using net = net_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using io = io_package;

partial class syslog_package {

partial struct Priority /*num:nint*/;

internal static UntypedInt severityMask => 0x07;

internal static UntypedInt facilityMask => 0xf8;

// Severity.
public static Priority LOG_EMERG => /* iota */ 0;
public static Priority LOG_ALERT => 1;
public static Priority LOG_CRIT => 2;
public static Priority LOG_ERR => 3;
public static Priority LOG_WARNING => 4;
public static Priority LOG_NOTICE => 5;
public static Priority LOG_INFO => 6;
public static Priority LOG_DEBUG => 7;

// Facility.
public static Priority LOG_KERN => /* iota << 3 */ 0;
public static Priority LOG_USER => 8;
public static Priority LOG_MAIL => 16;
public static Priority LOG_DAEMON => 24;
public static Priority LOG_AUTH => 32;
public static Priority LOG_SYSLOG => 40;
public static Priority LOG_LPR => 48;
public static Priority LOG_NEWS => 56;
public static Priority LOG_UUCP => 64;
public static Priority LOG_CRON => 72;
public static Priority LOG_AUTHPRIV => 80;
public static Priority LOG_FTP => 88;
internal static Priority _ᴛ1ʗ => 96; // unused
internal static Priority _ᴛ2ʗ => 104; // unused
internal static Priority _ᴛ3ʗ => 112; // unused
internal static Priority _ᴛ4ʗ => 120; // unused
public static Priority LOG_LOCAL0 => 128;
public static Priority LOG_LOCAL1 => 136;
public static Priority LOG_LOCAL2 => 144;
public static Priority LOG_LOCAL3 => 152;
public static Priority LOG_LOCAL4 => 160;
public static Priority LOG_LOCAL5 => 168;
public static Priority LOG_LOCAL6 => 176;
public static Priority LOG_LOCAL7 => 184;

// A Writer is a connection to a syslog server.
partial struct Writer {
    internal Priority priority;
    internal @string tag;
    internal @string hostname;
    internal @string network;
    internal @string raddr;
    internal sync.Mutex mu; // guards conn
    internal serverConn conn;
}

// This interface and the separate syslog_unix.go file exist for
// Solaris support as implemented by gccgo. On Solaris you cannot
// simply open a TCP connection to the syslog daemon. The gccgo
// sources have a syslog_solaris.go file that implements unixSyslog to
// return a type that satisfies this interface and simply calls the C
// library syslog function.
partial interface serverConn {
    error writeString(Priority p, @string hostname, @string tag, @string s, @string nl);
    error close();
}

partial struct netConn {
    internal bool local;
    internal net.Conn conn;
}

// New establishes a new connection to the system log daemon. Each
// write to the returned writer sends a log message with the given
// priority (a combination of the syslog facility and severity) and
// prefix tag. If tag is empty, the [os.Args][0] is used.
public static (ж<Writer>, error) New(Priority priority, @string tag) {
    return Dial(""u8, ""u8, priority, tag);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string logSyslogInvalidPriorityˢ = "log/syslog: invalid priority"u8;

// Dial establishes a connection to a log daemon by connecting to
// address raddr on the specified network. Each write to the returned
// writer sends a log message with the facility and severity
// (from priority) and tag. If tag is empty, the [os.Args][0] is used.
// If network is empty, Dial will connect to the local syslog server.
// Otherwise, see the documentation for net.Dial for valid values
// of network and raddr.
public static (ж<Writer>, error) Dial(@string network, @string raddr, Priority priority, @string tag) {
    GoFrame ᒐ = default;
    try {
        if (priority < 0 || priority > (Priority)(LOG_LOCAL7 | LOG_DEBUG)) {
            return (default!, errors.New(logSyslogInvalidPriorityˢ));
        }
        if (tag == ""u8) {
            tag = os.Args[0];
        }
        ref var hostname = ref heap<@string>(out var Ꮡhostname);
        (hostname, _) = os.Hostname();
        var w = Ꮡ(new Writer(
            priority: priority,
            tag: tag,
            hostname: hostname,
            network: network,
            raddr: raddr
        ));
        w.of(Writer.Ꮡmu).Lock();
        var wʗ1 = w;
        defer(wʗ1.of(Writer.Ꮡmu).Unlock, ref ᒐ);
        var err = w.connect();
        if (err != default!) {
            return (default!, err);
        }
        return (w, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localhostˢ = "localhost"u8;

// connect makes a connection to the syslog server.
// It must be called with w.mu held.
internal static error /*err*/ connect(this ref Writer w) {
    error err = default!;

    if (w.conn != default!) {
        // ignore err from close, it makes sense to continue anyway
        w.conn.close();
        w.conn = default!;
    }
    if (w.network == ""u8){
        (w.conn, err) = unixSyslog();
        if (w.hostname == ""u8) {
            w.hostname = localhostˢ;
        }
    } else {
        net.Conn c = default!;
        (c, err) = net.Dial(w.network, w.raddr);
        if (err == default!) {
            w.conn = new netConnжserverConn(Ꮡ(new netConn(
                conn: c,
                local: w.network == "unixgram"u8 || w.network == "unix"u8
            )));
            if (w.hostname == ""u8) {
                w.hostname = c.LocalAddr().String();
            }
        }
    }
    return err;
}

// Write sends a log message to the syslog daemon.
public static (nint, error) Write(this ж<Writer> Ꮡw, slice<byte> b) {
    ref var w = ref Ꮡw.DerefOrNull();

    return Ꮡw.writeAndRetry(w.priority, ((@string)b));
}

// Close closes a connection to the syslog daemon.
public static error Close(this ж<Writer> Ꮡw) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        w.mu.Lock();
        ᒐd1 = true;
        if (w.conn != default!) {
            var err = w.conn.close();
            w.conn = default!;
            return err;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// Emerg logs a message with severity [LOG_EMERG], ignoring the severity
// passed to New.
public static error Emerg(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_EMERG, m);
    return err;
}

// Alert logs a message with severity [LOG_ALERT], ignoring the severity
// passed to New.
public static error Alert(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_ALERT, m);
    return err;
}

// Crit logs a message with severity [LOG_CRIT], ignoring the severity
// passed to New.
public static error Crit(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_CRIT, m);
    return err;
}

// Err logs a message with severity [LOG_ERR], ignoring the severity
// passed to New.
public static error Err(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_ERR, m);
    return err;
}

// Warning logs a message with severity [LOG_WARNING], ignoring the
// severity passed to New.
public static error Warning(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_WARNING, m);
    return err;
}

// Notice logs a message with severity [LOG_NOTICE], ignoring the
// severity passed to New.
public static error Notice(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_NOTICE, m);
    return err;
}

// Info logs a message with severity [LOG_INFO], ignoring the severity
// passed to New.
public static error Info(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_INFO, m);
    return err;
}

// Debug logs a message with severity [LOG_DEBUG], ignoring the severity
// passed to New.
public static error Debug(this ж<Writer> Ꮡw, @string m) {
    var (_, err) = Ꮡw.writeAndRetry(LOG_DEBUG, m);
    return err;
}

internal static (nint, error) writeAndRetry(this ж<Writer> Ꮡw, Priority p, @string s) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        Priority pr = (Priority)(((Priority)(w.priority & (nint)facilityMask)) | ((Priority)(p & (nint)severityMask)));
        w.mu.Lock();
        ᒐd1 = true;
        if (w.conn != default!) {
            {
                var (n, err) = w.write(pr, s); if (err == default!) {
                    return (n, default!);
                }
            }
        }
        {
            var err = w.connect(); if (err != default!) {
                return (0, err);
            }
        }
        return w.write(pr, s);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡw.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// write generates and writes a syslog formatted string. The
// format is as follows: <PRI>TIMESTAMP HOSTNAME TAG[PID]: MSG
internal static (nint, error) write(this ref Writer w, Priority p, @string msg) {
    // ensure it ends in a \n
    @string nl = ""u8;
    if (!strings.HasSuffix(msg, "\n"u8)) {
        nl = "\n"u8;
    }
    var err = w.conn.writeString(p, w.hostname, w.tag, msg, nl);
    if (err != default!) {
        return (0, err);
    }
    // Note: return the length of the input, not the number of
    // bytes printed by Fprintf, because this must behave like
    // an io.Writer.
    return (len(msg), default!);
}

internal static error writeString(this ref netConn n, Priority p, @string hostname, @string tag, @string msg, @string nl) {
    if (n.local) {
        // Compared to the network form below, the changes are:
        //	1. Use time.Stamp instead of time.RFC3339.
        //	2. Drop the hostname field from the Fprintf.
        @string timestampΔ1 = time.Now().Format(time.Stamp);
        var (_, errΔ1) = fmt.Fprintf(new net_ConnᴠWriter(n.conn), "<%d>%s %s[%d]: %s%s"u8,
            p, timestampΔ1,
            tag, os.Getpid(), msg, nl);
        return errΔ1;
    }
    @string timestamp = time.Now().Format(time.RFC3339);
    var (_, err) = fmt.Fprintf(new net_ConnᴠWriter(n.conn), "<%d>%s %s %s[%d]: %s%s"u8,
        p, timestamp, hostname,
        tag, os.Getpid(), msg, nl);
    return err;
}

internal static error close(this ref netConn n) {
    return n.conn.Close();
}

// NewLogger creates a [log.Logger] whose output is written to the
// system log service with the specified priority, a combination of
// the syslog facility and severity. The logFlag argument is the flag
// set passed through to [log.New] to create the Logger.
public static (ж<log.Logger>, error) NewLogger(Priority p, nint logFlag) {
    var (s, err) = New(p, ""u8);
    if (err != default!) {
        return (default!, err);
    }
    return (log.New(new WriterжWriter(s), ""u8, logFlag), default!);
}

} // end syslog_package
