using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the two <c>IGoReflectSlice.ReflectOverNativeMemory</c> members. go.lib 1.26.10.3 published the member with
/// three arguments. The slices that keep their root (DESIGN-interior-pointer-root 4.5) needed a fourth, and adding it
/// to the published member, as an optional parameter, removed that member from the surface: the binary-compat guard
/// read CP0002 and CP0006 on it. So the published member stays, and the rooted one is a second member with a default
/// body.
/// </summary>
/// <remarks>
/// The guard holds the signatures. These arms hold what each member answers, and that an implementer written against
/// the published surface alone still compiles and answers the rooted call as it always answered.
/// </remarks>
[TestClass]
public class ReflectOverNativeMemoryMemberTests
{
    private const int N = 4;

    [TestMethod]
    public void ThePublishedMemberAndTheRootedMemberAnswerASliceOverTheSameBlock()
    {
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(N * sizeof(ulong));

        try
        {
            unsafe
            {
                for (int i = 0; i < N; i++)
                    ((ulong*)addr)[i] = 0x5000UL + (ulong)i;
            }

            IGoReflectSlice face = (IGoReflectSlice)(object)default(global::go.slice<ulong>);

            global::go.slice<ulong> published = (global::go.slice<ulong>)face.ReflectOverNativeMemory(addr, 2, N);
            global::go.slice<ulong> rooted = (global::go.slice<ulong>)face.ReflectOverNativeMemory(addr, 2, N, root: null);

            Assert.AreEqual((nint)2, published.Length, "published member: len");
            Assert.AreEqual((nint)N, published.Capacity, "published member: cap");
            Assert.AreEqual(0x5001UL, published[1], "published member: reads the block");

            Assert.AreEqual((nint)2, rooted.Length, "rooted member: len");
            Assert.AreEqual((nint)N, rooted.Capacity, "rooted member: cap");
            Assert.AreEqual(0x5001UL, rooted[1], "rooted member: reads the block");

            // Both are views of the one block: a write through one is read through the other.
            published[0] = 0x77UL;
            Assert.AreEqual(0x77UL, rooted[0], "the two members answer the same storage");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    // An implementer as one compiled against go.lib 1.26.10.3 would be: it has the published member and not the rooted
    // one. That it compiles is the first assertion. The rooted call reaches it through the interface's default body.
    private sealed class PublishedSurfaceOnly : IGoReflectSlice
    {
        public int PublishedCalls;

        public object ReflectOverNativeMemory(nuint address, nint len, nint cap)
        {
            PublishedCalls++;
            return (address, len, cap);
        }

        public object ReflectGrow(nint extra) => throw new NotSupportedException();

        public global::go.slice<byte>? ReflectByteView() => throw new NotSupportedException();

        public object ReflectAliasAsArray(nint length) => throw new NotSupportedException();

        public object? ReflectByteSliceAs(global::go.slice<byte> bytes) => throw new NotSupportedException();

        public (object? backing, nint low, nint len, nint cap) ReflectDescribe() => throw new NotSupportedException();

        public object ReflectElementZero() => throw new NotSupportedException();

        public object? ReflectRebase(Array backing, nint low, nint high, nint max) => throw new NotSupportedException();
    }

    [TestMethod]
    public void AnImplementerWithOnlyThePublishedMemberAnswersTheRootedCallThroughIt()
    {
        PublishedSurfaceOnly implementer = new();
        IGoReflectSlice face = implementer;

        object answer = face.ReflectOverNativeMemory(0x1000, 3, 5, root: new object());

        Assert.AreEqual(1, implementer.PublishedCalls, "the default body forwards to the published member, once");
        Assert.AreEqual(((nuint)0x1000, (nint)3, (nint)5), ((nuint, nint, nint))answer, "with the same three arguments");
    }
}
