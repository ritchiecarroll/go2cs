using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The go2cs-gen named-pointer wrapper's OUTBOUND conversions (Templates/InheritedType/
// PointerTypeTemplate.cs), driven through a REAL generated wrapper: syscall.Pointer (`type Pointer
// *struct{}`, windows flavour), the type AddrinfoW.Addr is declared as.
//
// WHAT WAS BROKEN. syscall/windows' GetAddrInfoW hand-own fills AddrinfoW.Addr with a native box over
// a ManagedPointerTokens ORDER TOKEN -- a deliberate carrier, whose address IS the token -- and the
// consumer (net's lookup_windows.cs, lookupIP's getaddr and lookupPort) reads it back with
// `(uintptr)(~result).Addr`. The template converted with `fixed (void* ptr = &value.Value)`, a
// DEREFERENCE, which golib's arm-2a refusal (40c9b3316a) correctly rejects for a token. Every
// Windows name resolution then panicked `dereference of a managed pointer with no address
// (*EmptyStruct over 0x...)`: crypto/x509's TestHybridPool, the runtime row's test host, and the
// LookupServicePort behavioral project. ж<T>'s own operator returns a native address unread; the
// template now defers to it.
[TestClass]
public class NamedPointerTokenCarrierTests
{
    // Built exactly as zsyscall_windows_addrinfo_impl.cs's tokenPointer builds it, over the same
    // pointee. ⚠ The pointee MUST be reference-bearing: arm 2a refuses a token only when the box it
    // names holds references (RawSockaddrInet4 carries its Addr as an array<byte>), and a first cut
    // of this fixture over a plain ж<nint> passed at the unfixed base -- it never reached the defect.
    private static (syscall_package.Pointer carrier, nuint token, ж<syscall_package.RawSockaddrInet4> box) TokenCarrier()
    {
        ж<syscall_package.RawSockaddrInet4> box = new StandardBox<syscall_package.RawSockaddrInet4>(new syscall_package.RawSockaddrInet4());
        box.Value.Port = 443;
        nuint token = box.PointerOrderToken;

        ManagedPointerTokens.Register(token, box);

        return (new syscall_package.Pointer((ж<EmptyStruct>)(uintptr)token), token, box);
    }

    [TestMethod]
    public void ATokenCarrierConvertsToItsTokenUnread()
    {
        var (carrier, token, _) = TokenCarrier();

        Assert.AreEqual(token, ((uintptr)carrier).Value,
            "a named pointer over a token carrier must hand the token back unread -- the conversion " +
            "is not a dereference, and arm 2a refuses a dereference of a token");
    }

    [TestMethod]
    public unsafe void ATokenCarrierConvertsToAVoidPointerUnread()
    {
        var (carrier, token, _) = TokenCarrier();

        Assert.AreEqual(token, (nuint)(void*)carrier,
            "the void* conversion is the same outbound door as uintptr and must answer the same");
    }

    [TestMethod]
    public void TheConsumersCastResolvesTheTokenBackToItsBox()
    {
        // lookup_windows.cs's own shape: `(*RawSockaddrInet4)(unsafe.Pointer(result.Addr))`.
        var (carrier, _, box) = TokenCarrier();

        var resolved = (ж<syscall_package.RawSockaddrInet4>)(uintptr)carrier;

        Assert.AreSame(box, resolved, "the token resolves back to the very box the carrier was built over");
        Assert.AreEqual((uint16)443, resolved.Value.Port);
    }

    [TestMethod]
    public void ANilNamedPointerConvertsToZero()
    {
        syscall_package.Pointer nilPointer = nil;

        Assert.AreEqual((nuint)0, ((uintptr)nilPointer).Value, "Go: uintptr(unsafe.Pointer(nil)) == 0");
    }

    [TestMethod]
    public void AGenuinelyNativeAddressRoundTripsUnchanged()
    {
        // The control: an ordinary native address, which both the old and the new operator answer.
        nint memory = Marshal.AllocHGlobal(16);

        try
        {
            var native = new syscall_package.Pointer((ж<EmptyStruct>)(uintptr)(nuint)memory);

            Assert.AreEqual((nuint)memory, ((uintptr)native).Value);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    // ---- the INBOUND conversions: an address converted INTO the named pointer ----
    //
    // The template read `*(T*)value` into a fresh managed box, so the result was a COPY of the
    // pointee at some other address: the address it was made from was gone, and a token was
    // dereferenced as if it were memory. Both now defer to ж<T>'s own operator, which aliases.

    [TestMethod]
    public void AnInboundNativeAddressAliasesTheAddress()
    {
        nint memory = Marshal.AllocHGlobal(16);

        try
        {
            syscall_package.Pointer native = (uintptr)(nuint)memory;

            Assert.AreEqual((nuint)memory, ((uintptr)native).Value,
                "Go: uintptr(unsafe.Pointer(Pointer(unsafe.Pointer(p)))) == p -- a copy answers the copy's address");
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    [TestMethod]
    public unsafe void AnInboundVoidPointerAliasesTheAddress()
    {
        nint memory = Marshal.AllocHGlobal(16);

        try
        {
            syscall_package.Pointer native = (void*)memory;

            Assert.AreEqual((nuint)memory, (nuint)(void*)native, "the void* conversion is the same inbound door as uintptr");
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    [TestMethod]
    public void AnInboundTokenIsCarriedUnread()
    {
        // The carrier TokenCarrier builds by hand, built through the wrapper's own operator.
        var (_, token, box) = TokenCarrier();

        syscall_package.Pointer carrier = (uintptr)token;

        Assert.AreEqual(token, ((uintptr)carrier).Value, "a token converted in must come back out as the same token");
        Assert.AreSame(box, (ж<syscall_package.RawSockaddrInet4>)(uintptr)carrier);
    }
}
