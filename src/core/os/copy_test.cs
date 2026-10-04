// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using rand = math.rand.rand_package;
using Δnet = net_package;
using Δos = os_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using Δtesting = testing_package;
using nettest = vendor.golang.org.x.net.nettest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using math.rand;
using static go.os_internal_test_package;
using time = time_package;
using vendor.golang.org.x.net;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;

// Exercise sendfile/splice fast paths with a moderately large file.
//
// https://go.dev/issue/70000
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestLargeCopyViaNetwork(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt size = /* 10 * 1024 * 1024 */ 10485760;
        @string dir = Ꮡt.TempDir();
        var (src, err) = Δos.Create(dir + "/src"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var srcʗ1 = src;
        defer(() => srcʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = Δio.CopyN(new Δos.FileжWriter(src), new os_test_package.randReaderжReader(newRandReader()), size); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var (_, errΔ2) = src.Seek(0, 0); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        (var dst, err) = Δos.Create(dir + "/dst"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dstʗ1 = dst;
        defer(() => dstʗ1.Close(), ref ᒐ);
        var (client, server) = createSocketPair(Ꮡt, tcpˢ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        var dstʗ2 = dst;
        var serverʗ1 = server;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                {
                    var (n, errΔ3) = Δio.Copy(new Δos.FileжWriter(dstʗ2), new os_test_package.net_ConnᴠReader(serverʗ1)); if (n != size || errΔ3 != default!) {
                        Ꮡt.Errorf("copy to destination = %v, %v; want %v, nil"u8, n, errΔ3, (nint)(size));
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var clientʗ1 = client;
        var srcʗ2 = src;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var clientʗ2 = clientʗ1;
                defer(() => clientʗ2.Close(), ref ᒐ);
                {
                    var (n, errΔ4) = Δio.Copy(new os_test_package.net_ConnᴠWriter(clientʗ1), new os_test_package.os_FileжReader(srcʗ2)); if (n != size || errΔ4 != default!) {
                        Ꮡt.Errorf("copy from source = %v, %v; want %v, nil"u8, n, errΔ4, (nint)(size));
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();
        {
            var (_, errΔ5) = dst.Seek(0, 0); if (errΔ5 != default!) {
                Ꮡt.Fatal(errΔ5);
            }
        }
        {
            var errΔ6 = compareReaders(new os_test_package.os_FileжReader(dst), Δio.LimitReader(new os_test_package.randReaderжReader(newRandReader()), size)); if (errΔ6 != default!) {
                Ꮡt.Fatal(errΔ6);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dstˢ = "dst"u8;

public static void TestCopyFileToFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt size = /* 1 * 1024 * 1024 */ 1048576;
        @string dir = Ꮡt.TempDir();
        var (src, err) = Δos.Create(dir + "/src"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var srcʗ1 = src;
        defer(() => srcʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = Δio.CopyN(new Δos.FileжWriter(src), new os_test_package.randReaderжReader(newRandReader()), size); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var (_, errΔ2) = src.Seek(0, 0); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        int64 mustSeek(ж<Δos.File> f, int64 offset, nint whence) {
            var (ret, errΔ3) = f.Seek(offset, whence);
            if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
            return ret;
        }
        foreach (var (_, srcStart) in new int64[]{0, 100, size}.slice()) {
            var remaining = (int64)size - srcStart;
            foreach (var (_, dstStart) in new int64[]{0, 200}.slice()) {
                foreach (var (_, limit) in new int64[]{remaining, remaining - 100, size * 2, 0}.slice()) {
                    if (limit < 0) {
                        continue;
                    }
                    @string name = fmt.Sprintf("srcStart=%v/dstStart=%v/limit=%v"u8, srcStart, dstStart, limit);
                    var mustSeekʗ1 = mustSeek;
                    var srcʗ2 = src;
                    Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
                        GoFrame ᒐ = default;
                        try {
                            var (dst, errΔ4) = Δos.CreateTemp(dir, dstˢ);
                            if (errΔ4 != default!) {
                                tΔ1.Fatal(errΔ4);
                            }
                            var dstʗ1 = dst;
                            defer(() => dstʗ1.Close(), ref ᒐ);
                            defer(Δos.Remove, dst.Name(), ref ᒐ);
                            mustSeekʗ1(srcʗ2, srcStart, Δio.SeekStart);
                            {
                                var (_, errΔ5) = Δio.CopyN(new Δos.FileжWriter(dst), new zeroReader(nil), dstStart); if (errΔ5 != default!) {
                                    tΔ1.Fatal(errΔ5);
                                }
                            }
                            int64 copied = default!;
                            if (limit == 0){
                                (copied, errΔ4) = Δio.Copy(new Δos.FileжWriter(dst), new os_test_package.os_FileжReader(srcʗ2));
                            } else {
                                (copied, errΔ4) = Δio.CopyN(new Δos.FileжWriter(dst), new os_test_package.os_FileжReader(srcʗ2), limit);
                            }
                            if (limit > remaining){
                                if (!AreEqual(errΔ4, Δio.EOF)) {
                                    tΔ1.Errorf("Copy: %v; want io.EOF"u8, errΔ4);
                                }
                            } else {
                                if (errΔ4 != default!) {
                                    tΔ1.Errorf("Copy: %v; want nil"u8, errΔ4);
                                }
                            }
                            var wantCopied = remaining;
                            if (limit != 0) {
                                wantCopied = min(limit, wantCopied);
                            }
                            if (copied != wantCopied) {
                                tΔ1.Errorf("copied %v bytes, want %v"u8, copied, wantCopied);
                            }
                            var srcPos = mustSeekʗ1(srcʗ2, 0, Δio.SeekCurrent);
                            var wantSrcPos = srcStart + wantCopied;
                            if (srcPos != wantSrcPos) {
                                tΔ1.Errorf("source position = %v, want %v"u8, srcPos, wantSrcPos);
                            }
                            var dstPos = mustSeekʗ1(dst, 0, Δio.SeekCurrent);
                            var wantDstPos = dstStart + wantCopied;
                            if (dstPos != wantDstPos) {
                                tΔ1.Errorf("destination position = %v, want %v"u8, dstPos, wantDstPos);
                            }
                            mustSeekʗ1(dst, 0, Δio.SeekStart);
                            var rr = newRandReader();
                            Δio.CopyN(Δio.Discard, new os_test_package.randReaderжReader(rr), srcStart);
                            var wantReader = Δio.MultiReader(
                                Δio.LimitReader(new zeroReader(nil), dstStart),
                                Δio.LimitReader(new os_test_package.randReaderжReader(rr), wantCopied));
                            {
                                var errΔ6 = compareReaders(new os_test_package.os_FileжReader(dst), wantReader); if (errΔ6 != default!) {
                                    tΔ1.Fatal(errΔ6);
                                }
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contentsMismatchˢ = "contents mismatch"u8;

internal static error compareReaders(Δio.Reader a, Δio.Reader b) {
    var bufa = new slice<byte>(4096);
    var bufb = new slice<byte>(4096);
    nint off = 0;
    while (ᐧ) {
        var (na, erra) = Δio.ReadFull(a, bufa);
        if (erra != default! && !AreEqual(erra, Δio.EOF) && !AreEqual(erra, Δio.ErrUnexpectedEOF)) {
            return erra;
        }
        var (nb, errb) = Δio.ReadFull(b, bufb);
        if (errb != default! && !AreEqual(errb, Δio.EOF) && !AreEqual(errb, Δio.ErrUnexpectedEOF)) {
            return errb;
        }
        if (!bytes.Equal(bufa.slice(0, na), bufb.slice(0, nb))) {
            return errors.New(contentsMismatchˢ);
        }
        if (erra != default! && errb != default!) {
            break;
        }
        off += len(bufa);
    }
    return default!;
}

[GoType] partial struct zeroReader {
}

internal static (nint, error) Read(this zeroReader r, slice<byte> p) {
    clear(p);
    return (len(p), default!);
}

[GoType] partial struct randReader {
    internal ж<rand.Rand> rand;
}

internal static ж<randReader> newRandReader() {
    return Ꮡ(new randReader(rand.New(new rand.PCGжSource(rand.NewPCG(0, 0)))));
}

[GoRecv] internal static (nint, error) Read(this ref randReader r, slice<byte> p) {
    foreach (var (i, _) in p) {
        p[i] = (byte)((uint32)(r.rand.Uint32() & 0xff));
    }
    return (len(p), default!);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static (Δnet.Conn client, Δnet.Conn server) createSocketPair(ж<Δtesting.T> Ꮡt, @string proto) {
    Δnet.Conn client = default!;
    Δnet.Conn server = default!;

    Ꮡt.Helper();
    if (!nettest.TestableNetwork(proto)) {
        Ꮡt.Skipf("%s does not support %q"u8, Δruntime.GOOS, proto);
    }
    var (ln, err) = nettest.NewLocalListener(proto);
    if (err != default!) {
        Ꮡt.Fatalf("NewLocalListener error: %v"u8, err);
    }
    var lnʗ1 = ln;
    Ꮡt.Cleanup(() => {
        if (lnʗ1 != default!) {
            lnʗ1.Close();
        }
        if (client != default!) {
            client.Close();
        }
        if (server != default!) {
            server.Close();
        }
    });
    var ch = new channel<EmptyStruct>(0);
    var chʗ1 = ch;
    var lnʗ2 = ln;
    goǃ(() => {
        error errΔ1 = default!;
        (server, errΔ1) = lnʗ2.Accept();
        if (errΔ1 != default!) {
            Ꮡt.Errorf("Accept new connection error: %v"u8, errΔ1);
        }
        chʗ1.ᐸꟷ(new EmptyStruct());
    });
    (client, err) = Δnet.Dial(proto, ln.Addr().String());
    ᐸꟷ(ch);
    if (err != default!) {
        Ꮡt.Fatalf("Dial new connection error: %v"u8, err);
    }
    return (client, server);
}

} // end os_test_package
