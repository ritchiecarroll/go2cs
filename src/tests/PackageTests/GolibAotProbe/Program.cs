// Seat #7 A/B probe. Each scenario reaches a site the seat changed, in the shape the converter emits:
// defer/recover and a re-panic (GoFrame.ReRaisedByTheDeferredDelegate), a recovered bounds panic
// (RuntimePanicCheck.RaisedInRuntimePackage), goroutine launches (Goroutine.CreatorFrame), and
// GoArrayZero.New<T, TDims> (GoArrayDims). Output goes to stdout only; the arms are compared byte for byte.
// Seat #18 added 9-12: the syscall marshal's shapes, which must run under Native AOT (see the comment above RtoParams).
using System;
using go;
using static go.builtin;

public static class Program
{
    private sealed class ᴀ4 : IGoArrayDims { public static nint[] Dims { get; } = [4]; }
    private sealed class ᴀ0 : IGoArrayDims { public static nint[] Dims { get; } = []; }

    private static string Show(object e) => e is null ? "<nil>" : e is error err ? (string)err.Error() : e.ToString();

    private static void RecoverDirect()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("1 recovered: " + Show(e)); }, ref ᒐ);
            throw panic("boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void RecoverBounds()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("2 recovered: " + Show(e)); }, ref ᒐ);
            slice<nint> s = new nint[] { 1, 2 }.slice();
            nint i = 5;
            Console.WriteLine(s[i]);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void PanicTwice()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => throw panic("second"), ref ᒐ);
            throw panic("first");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void RecoverRePanic()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("3 recovered: " + Show(e)); }, ref ᒐ);
            PanicTwice();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void Handle()
    {
        var calledByDefer = GoFrame.TakeDeferredCall();
        var e = recover(calledByDefer);
        Console.WriteLine("4 recovered: " + Show(e));
    }

    private static void RecoverThroughHandler()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(Handle, ref ᒐ, recovers: true);
            throw panic("handled");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void Goroutines()
    {
        var ch = new channel<nint>(0);
        goǃ(() => ch.ᐸꟷ((nint)42));
        Console.WriteLine("5 received: " + ᐸꟷ(ch));

        var done = new channel<@string>(0);
        goǃ(() =>
        {
            GoFrame ᒐ = default;
            try
            {
                defer(() => { var e = recover(); done.ᐸꟷ((@string)("6 goroutine recovered: " + Show(e))); }, ref ᒐ);
                throw panic("in goroutine");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Console.WriteLine((string)ᐸꟷ(done));
    }

    private static string Zero<T, Tᴀ>() where Tᴀ : IGoArrayDims
    {
        ж<T> p = GoArrayZero.New<T, Tᴀ>();
        return p.ValueSlot is array<nint> a ? $"array len {a.Length}: [{a[0]} {a[3]}]" : $"{p.ValueSlot}";
    }

    // Seat #18 (1.26.10.2): NativeStructMarshal classified a struct's scalar fields with MakeGenericMethod, which a
    // Native AOT image has no code for (a converted TLS program exited 2 in syscall.WSAIoctl). Two shapes reach it.

    // net's `(*byte)(unsafe.Pointer(&params))` before WSAIoctl: a reference-free struct viewed as bytes and handed
    // to a syscall as a uintptr. The conversion asks whether the struct has a native image, which classifies its
    // uint16 and uint8 fields.
    public struct RtoParams
    {
        public ushort Rtt;
        public byte MaxSynRetransmissions;
    }

    // A reference-bearing struct (an array field is a managed reference) passed by pointer through the syscall
    // marshal: copied to a Go-layout native buffer, changed there by the "kernel", and decoded back.
    public struct Sockaddr
    {
        public byte Len;
        public byte Family;
        public ushort Port;
        public array<byte> Addr = new(4);

        public Sockaddr() { }
    }

    // SetEntriesInAcl's `**ACL`: the callee writes a pointer word that points outside every copy, which comes back as
    // a native box over that address, built for an element type known only at run time. The IGoTypeOpsSource member is
    // the one go2cs-gen emits for a converted struct whose package spells a pointer to it (StructTypeTemplate).
    public struct Acl : IGoTypeOpsSource
    {
        IGoTypeOps IGoTypeOpsSource.TypeOps => GoTypeOps<Acl>.Instance;

        public byte AclRevision;
        public byte Sbz1;
        public ushort AclSize;
        public ushort AceCount;
        public ushort Sbz2;
    }

    private static nuint deepCallee(nuint written, ReadOnlySpan<nuint> args)
    {
        unsafe { *(nuint*)args[0] = written; }
        return 0;
    }

    private static unsafe void OutPointerWord()
    {
        heap<ж<Acl>>(nil, out ж<ж<Acl>> newAcl);
        Acl* written = (Acl*)System.Runtime.InteropServices.NativeMemory.AllocZeroed((nuint)sizeof(Acl));

        try
        {
            nuint[] args = [((uintptr)newAcl).Value];
            NativeStructMarshal.CallDeep(args, (nuint)written, &deepCallee);
            written->AceCount = 4;
            Console.WriteLine($"12 OUT word: at the written address {((uintptr)newAcl.Value).Value == (nuint)written}, reads AceCount {newAcl.Value.Value.AceCount}");
        }
        finally
        {
            System.Runtime.InteropServices.NativeMemory.Free(written);
        }
    }

    private static void ReinterpretToUintptr()
    {
        ж<RtoParams> p = Ꮡ(new RtoParams { Rtt = 3, MaxSynRetransmissions = 2 });
        uintptr u = (uintptr)p.Reinterpret<RtoParams, byte>();
        Console.WriteLine("9 reinterpret to uintptr: " + (u != (uintptr)0));
    }

    private static unsafe void MarshalRoundTrip()
    {
        ж<Sockaddr> s = Ꮡ(new Sockaddr { Len = 16, Family = 2, Port = 80 });
        s.ValueSlot.Addr[0] = 127;
        s.ValueSlot.Addr[3] = 1;

        var (r1, _, errno) = NativeStructMarshal.Call((uintptr)0, (uintptr)s, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0, (uintptr)0,
            (num, a1, a2, a3, a4, a5, a6) =>
            {
                byte* raw = (byte*)(nuint)a1;
                Console.WriteLine($"10 native copy: len {raw[0]} family {raw[1]} port {*(ushort*)(raw + 2)} addr {raw[4]}.{raw[5]}.{raw[6]}.{raw[7]}");
                *(ushort*)(raw + 2) = 443;
                raw[4] = 10;
                return ((uintptr)7, (uintptr)0, (uintptr)0);
            });

        Sockaddr back = s.ValueSlot;
        Console.WriteLine($"11 decoded back: r1 {(nuint)r1} errno {(nuint)errno} port {back.Port} addr {back.Addr[0]}.{back.Addr[1]}.{back.Addr[2]}.{back.Addr[3]}");
    }

    // No argument runs every scenario. `marshal`, `roundtrip` or `outword` runs that one seat #18 scenario alone, so
    // each site it fixed reads red or green by itself.
    public static int Main(string[] args)
    {
        switch (args.Length > 0 ? args[0] : "")
        {
            case "marshal":
                ReinterpretToUintptr();
                return 0;
            case "roundtrip":
                MarshalRoundTrip();
                return 0;
            case "outword":
                OutPointerWord();
                return 0;
        }

        RecoverDirect();
        RecoverBounds();
        RecoverRePanic();
        RecoverThroughHandler();
        Goroutines();
        Console.WriteLine("7 new(nint): " + Zero<nint, ᴀ0>());
        Console.WriteLine("8 new([4]nint): " + Zero<array<nint>, ᴀ4>());
        ReinterpretToUintptr();
        MarshalRoundTrip();
        OutPointerWord();
        Console.WriteLine("done");
        return 0;
    }
}
