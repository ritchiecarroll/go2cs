// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Test that Resolver.Dial can be a func returning an in-memory net.Conn
// speaking DNS.
namespace go;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using time = time_package;
using dnsmessage = vendor.golang.org.x.net.dns.dnsmessage_package;
using static go.net_package;
using vendor.golang.org.x.net.dns;

partial class net_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarˢ = "foo.bar."u8;
internal static readonly @string barBazˢ = "bar.baz."u8;
internal static readonly @string lookupIPˢ = "LookupIP"u8;
internal static readonly @string lookupSRVˢ = "LookupSRV"u8;
internal static readonly @string someServiceˢ = "some-service"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fakeDomainᶜ = "something-that-is-a-not-a-real-domain.fake-tld."u8;

public static void TestResolverDialFunc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var r = Ꮡ(new Resolver(
        PreferGo: true,
        Dial: newResolverDialFunc(Ꮡ(new resolverDialHandler(
            StartDial: (@string network, @string address) => {
                Ꮡt.Logf("StartDial(%q, %q) ..."u8, network, address);
                return default!;
            },
            Question: (dnsmessage.Header h, dnsmessageꓸQuestion q) => {
                q = q.ΔClone();
                Ꮡt.Logf("Header: %+v for %q (type=%v, class=%v)"u8, h,
                    q.Name.String(), q.Type, q.Class);
            }, // TODO: add test without HandleA* hooks specified at all, that Go
 // doesn't issue retries; map to something terminal.

            HandleA: (AWriter w, @string name) => {
                w.AddIP(new byte[]{1, 2, 3, 4}.array());
                w.AddIP(new byte[]{5, 6, 7, 8}.array());
                return default!;
            },
            HandleAAAA: (AAAAWriter w, @string name) => {
                w.AddIP(new array<byte>(16){[1] = 1, [15] = 15});
                w.AddIP(new array<byte>(16){[2] = 2, [14] = 14});
                return default!;
            },
            HandleSRV: (SRVWriter w, @string name) => {
                w.AddSRV(1, 2, 80, fooBarˢ);
                w.AddSRV(2, 3, 81, barBazˢ);
                return default!;
            }
        )))
    ));
    var ctx = context.Background();
    @string fakeDomain = fakeDomainᶜ;
    var ctxʗ1 = ctx;
    var rʗ1 = r;
    Ꮡt.Run(lookupIPˢ, (ж<testing.T> tΔ1) => {
        var (ips, err) = rʗ1.LookupIP(ctxʗ1, "ip"u8, fakeDomain);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        {
            var (got, want) = (sortedIPStrings(ips), new @string[]{"0:200::e00"u8, "1.2.3.4"u8, "1::f"u8, "5.6.7.8"u8}.slice()); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                tΔ1.Errorf("LookupIP wrong.\n got: %q\nwant: %q\n"u8, got, want);
            }
        }
    });
    var ctxʗ2 = ctx;
    var rʗ2 = r;
    Ꮡt.Run(lookupSRVˢ, (ж<testing.T> tΔ2) => {
        var (_, got, err) = rʗ2.LookupSRV(ctxʗ2, someServiceˢ, tcpˢ, fakeDomain);
        if (err != default!) {
            tΔ2.Fatal(err);
        }
        var want = new ж<global::go.net_package.SRV>[]{
            Ꮡ(new global::go.net_package.SRV(
                Target: "foo.bar."u8,
                Port: 80,
                Priority: 1,
                Weight: 2)),
            Ꮡ(new global::go.net_package.SRV(
                Target: "bar.baz."u8,
                Port: 81,
                Priority: 2,
                Weight: 3))
        }.slice();
        if (!reflect.DeepEqual(got, want)) {
            tΔ2.Errorf("wrong result. got:"u8);
            foreach (var (_, rΔ1) in got) {
                tΔ2.Logf("  - %+v"u8, rΔ1.OrTypedNil());
            }
        }
    });
}

internal static slice<@string> sortedIPStrings(slice<global::go.net_package.IP> ips) {
    var ret = new slice<@string>(len(ips));
    foreach (var (i, ip) in ips) {
        ret[i] = ip.String();
    }
    slices.Sort<slice<@string>, @string>(ret);
    return ret;
}

internal static Func<context.Context, @string, @string, (global::go.net_package.Conn, error)> newResolverDialFunc(ж<resolverDialHandler> Ꮡh) {
    return (context.Context ctx, @string network, @string address) => {
        var a = Ꮡ(new resolverFuncConn(
            h: Ꮡh,
            network: network,
            address: address,
            ttl: 10
        ));
        // 10 second default if unset
        if (Ꮡh.Value.StartDial != default!) {
            {
                var err = Ꮡh.Value.StartDial(network, address); if (err != default!) {
                    return (default!, err);
                }
            }
        }
        return (new net_internal_test_package.resolverFuncConnжConn(a), default!);
    };
}

[GoType] internal partial struct resolverDialHandler {
    // StartDial, if non-nil, is called when Go first calls Resolver.Dial.
    // Any error returned aborts the dial and is returned unwrapped.
    public Func<@string, @string, error> StartDial;
    public Action<dnsmessage.Header, dnsmessageꓸQuestion> Question;
    // err may be ErrNotExist or ErrRefused; others map to SERVFAIL (RCode2).
    // A nil error means success.
    public Func<AWriter, @string, error> HandleA;
    public Func<AAAAWriter, @string, error> HandleAAAA;
    public Func<SRVWriter, @string, error> HandleSRV;
}

[GoType] public partial struct ResponseWriter {
    internal ж<resolverFuncConn> a;
}

internal static dnsmessage.ResourceHeader header(this ResponseWriter w) {
    var q = w.a.Value.q.ΔClone();
    return new dnsmessage.ResourceHeader(
        Name: q.Name.ΔClone(),
        Type: q.Type,
        Class: q.Class,
        TTL: (~w.a).ttl
    );
}

// SetTTL sets the TTL for subsequent written resources.
// Once a resource has been written, SetTTL calls are no-ops.
// That is, it can only be called at most once, before anything
// else is written.
public static void SetTTL(this ResponseWriter w, uint32 seconds) {
    // ... intention is last one wins and mutates all previously
    // written records too, but that's a little annoying.
    // But it's also annoying if the requirement is it needs to be set
    // last.
    // And it's also annoying if it's possible for users to set
    // different TTLs per Answer.
    if ((~w.a).wrote) {
        return;
    }
    w.a.Value.ttl = seconds;
}

[GoType] public partial struct AWriter {
    public partial ref ResponseWriter ResponseWriter { get; }
}

public static void AddIP(this AWriter w, [GoArrayDims(4)] array<byte> v4) {
    v4 = v4.Clone();

    w.a.Value.wrote = true;
    var err = (~w.a).builder.AResource(w.header(), new dnsmessageꓸAResource(A: v4.Clone()));
    if (err != default!) {
        throw panic(err);
    }
}

[GoType] public partial struct AAAAWriter {
    public partial ref ResponseWriter ResponseWriter { get; }
}

public static void AddIP(this AAAAWriter w, [GoArrayDims(16)] array<byte> v6) {
    v6 = v6.Clone();

    w.a.Value.wrote = true;
    var err = (~w.a).builder.AAAAResource(w.header(), new dnsmessageꓸAAAAResource(AAAA: v6.Clone()));
    if (err != default!) {
        throw panic(err);
    }
}

[GoType] public partial struct SRVWriter {
    public partial ref ResponseWriter ResponseWriter { get; }
}

// AddSRV adds a SRV record. The target name must end in a period and
// be 63 bytes or fewer.
public static error AddSRV(this SRVWriter w, uint16 priority, uint16 weight, uint16 port, @string target) {
    var (targetName, err) = dnsmessage.NewName(target);
    if (err != default!) {
        return err;
    }
    w.a.Value.wrote = true;
    err = (~w.a).builder.SRVResource(w.header(), new dnsmessageꓸSRVResource(
        Priority: priority,
        Weight: weight,
        Port: port,
        Target: targetName.ΔClone()
    ));
    if (err != default!) {
        throw panic(err); // internal fault, not user
    }
    return default!;
}

public static error ErrNotExist = errors.New("name does not exist"u8); // maps to RCode3, NXDOMAIN
public static error ErrRefused = errors.New("refused"u8);   // maps to RCode5, REFUSED

[GoType] [GoValueClone("q")] internal partial struct resolverFuncConn {
    internal ж<resolverDialHandler> h;
    internal @string network;
    internal @string address;
    internal ж<dnsmessage.Builder> builder;
    internal dnsmessageꓸQuestion q;
    internal uint32 ttl;
    internal bool wrote;
    internal bytes.Buffer rbuf;
}

[GoRecv] internal static error Close(this ref resolverFuncConn _) {
    return default!;
}

[GoRecv] internal static global::go.net_package.ΔAddr LocalAddr(this ref resolverFuncConn _) {
    return new someaddr(nil);
}

[GoRecv] internal static global::go.net_package.ΔAddr RemoteAddr(this ref resolverFuncConn _) {
    return new someaddr(nil);
}

[GoRecv] internal static error SetDeadline(this ref resolverFuncConn _, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref resolverFuncConn _, time.Time t) {
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref resolverFuncConn _, time.Time t) {
    return default!;
}

[GoRecv] internal static (nint n, error err) Read(this ref resolverFuncConn a, slice<byte> p) {
    return a.rbuf.Read(p);
}

internal static (nint n, error err) Write(this ж<resolverFuncConn> Ꮡa, slice<byte> packet) {
    nint n = default!;
    error err = default!;

    ref var a = ref Ꮡa.DerefOrNull();
    if (len(packet) < 2) {
        return (0, fmt.Errorf("short write of %d bytes; want 2+"u8, len(packet)));
    }
    nint reqLen = (nint)(((nint)packet[0] << (int)(8)) | (nint)packet[1]);
    var req = packet[2..];
    if (len(req) != reqLen) {
        return (0, fmt.Errorf("packet declared length %d doesn't match body length %d"u8, reqLen, len(req)));
    }
    dnsmessage.Parser parser = default!;
    (var h, err) = parser.Start(req);
    if (err != default!) {
        // TODO: hook
        return (0, err);
    }
    (var q, err) = parser.Question();
    var hadQ = (err == default!);
    if (err == default! && (~a.h).Question != default!) {
        (~a.h).Question(h, q);
    }
    if (err != default! && !AreEqual(err, dnsmessage.ErrSectionDone)) {
        return (0, err);
    }
    var resh = h;
    resh.Response = true;
    resh.Authoritative = true;
    if (hadQ){
        resh.RCode = dnsmessage.RCodeSuccess;
    } else {
        resh.RCode = dnsmessage.RCodeNotImplemented;
    }
    a.rbuf.Grow(514);
    a.rbuf.WriteByte((rune)'X'); // reserved header for beu16 length
    a.rbuf.WriteByte((rune)'Y'); // reserved header for beu16 length
    ref var builder = ref heap<dnsmessage.Builder>(out var Ꮡbuilder);
    builder = dnsmessage.NewBuilder(a.rbuf.Bytes(), resh);
    a.builder = Ꮡbuilder;
    if (hadQ) {
        a.q = q.ΔClone();
        a.builder.StartQuestions();
        var errΔ1 = a.builder.Question(q);
        if (errΔ1 != default!) {
            return (0, fmt.Errorf("Question: %w"u8, errΔ1));
        }
        a.builder.StartAnswers();
        var exprᴛ1 = q.Type;
        if (exprᴛ1 == dnsmessage.TypeA) {
            if ((~a.h).HandleA != default!) {
                resh.RCode = mapRCode((~a.h).HandleA(new AWriter(new ResponseWriter(Ꮡa)), q.Name.String()));
            }
        }
        else if (exprᴛ1 == dnsmessage.TypeAAAA) {
            if ((~a.h).HandleAAAA != default!) {
                resh.RCode = mapRCode((~a.h).HandleAAAA(new AAAAWriter(new ResponseWriter(Ꮡa)), q.Name.String()));
            }
        }
        else if (exprᴛ1 == dnsmessage.TypeSRV) {
            if ((~a.h).HandleSRV != default!) {
                resh.RCode = mapRCode((~a.h).HandleSRV(new SRVWriter(new ResponseWriter(Ꮡa)), q.Name.String()));
            }
        }

    }
    (var tcpRes, err) = builder.Finish();
    if (err != default!) {
        return (0, fmt.Errorf("Finish: %w"u8, err));
    }
    n = len(tcpRes) - 2;
    tcpRes[0] = (byte)((n >> (int)(8)));
    tcpRes[1] = (byte)n;
    a.rbuf.Write(tcpRes[2..]);
    return (len(packet), default!);
}

[GoType] internal partial struct someaddr {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unusedˢ = "unused"u8;

internal static @string Network(this someaddr _) {
    return unusedˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unusedSomeaddrˢ = "unused-someaddr"u8;

internal static @string String(this someaddr _) {
    return unusedSomeaddrˢ;
}

internal static dnsmessage.RCode mapRCode(error err) {
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, default!)) {
        return dnsmessage.RCodeSuccess;
    }
    if (AreEqual(exprᴛ1, ErrNotExist)) {
        return dnsmessage.RCodeNameError;
    }
    if (AreEqual(exprᴛ1, ErrRefused)) {
        return dnsmessage.RCodeRefused;
    }
    { /* default: */
        return dnsmessage.RCodeServerFailure;
    }

}

} // end net_internal_test_package
