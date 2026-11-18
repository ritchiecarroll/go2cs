using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using syscall = go.syscall_package;

namespace GolibTests;

// syscall's windows readNativeSockaddr decodes the kernel's sockaddr buffer for Getsockname, Getpeername and
// Accept. An UNNAMED AF_UNIX address, the family alone (addrlen 2), has an empty path, and the decode answered it
// with the empty name. The element-address bounds check (Go's `&name[0]` of an empty array is an index panic) must
// not turn that answer into a panic: net's TestUnixConnLocalWindows asks for exactly this address, an unbound
// client's LocalAddr. The decode is managed code over a buffer, so the arm is host-independent; the file is
// compiled only for the windows flavour (GolibTests.csproj), where the method exists.
[TestClass]
public class WindowsUnnamedUnixSockaddrTests
{
    [TestMethod]
    public unsafe void AnUnnamedUnixAddressDecodesWithoutPanicking()
    {
        MethodInfo decode = typeof(syscall).GetMethod("readNativeSockaddr", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("syscall.readNativeSockaddr was not found; the arm would measure nothing");

        byte* buffer = stackalloc byte[2 + 108];
        *(ushort*)buffer = (ushort)syscall.AF_UNIX;

        object? result = decode.Invoke(null, [Pointer.Box(buffer, typeof(byte*)), 2]);
        (syscall.Sockaddr sa, error err) = ((syscall.Sockaddr, error))result!;

        Assert.IsNull(err, "an unnamed AF_UNIX address is a valid answer, not an error");
        Assert.IsNotNull(sa, "the decode must answer a SockaddrUnix for the AF_UNIX family");
    }
}
