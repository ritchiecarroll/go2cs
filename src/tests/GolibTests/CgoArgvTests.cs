using System;
using System.Runtime.InteropServices;
using System.Text;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = go.cgotestꓸC_package;

namespace GolibTests;

// cgo Phase 3, .5 cgo item 2, the argv shape (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.12.3; H's verdict (A), the
// word mirror of the backing array): `C.f(&argv[0])` hands C a pointer to the first of a Go array's C pointers, and C
// may read or fill the whole array. go2cs's Go array holds BOXES, not words, so the whole backing array crosses as a
// mirror of words, pinned for the call, and is copied back word by word. Each element crosses as a pointer member does:
// nil is 0, a C pointer its address, a Go pointer cgo's own panic (B-10). Run against the probe library.
//
// Before the cure the typed door refused the element by name ("an element of a Go array of C pointers") and the void
// door refused it as a Go pointer to Go pointers. The void door's rows are the cgo-e2e's (argvvoid): a void parameter
// takes go2cs's unsafe.Pointer, which this golden's fixture does not declare.
[TestClass]
[DoNotParallelize]
public unsafe class CgoArgvTests
{
    private static T OnAGoroutine<T>(Func<T> call) => CgoScalarWrapperTests.OnAGoroutine(call);

    private static string RefusalOf(Action call)
    {
        string? message = OnAGoroutine(() =>
        {
            try
            {
                call();
            }
            catch (PanicException refused)
            {
                return refused.Message;
            }

            return null;
        });

        Assert.IsNotNull(message, "the call was not refused");
        return message;
    }

    // C strings in one native block, freed with it: each word is a C pointer, so the array holds no Go pointer.
    private sealed class CStrings : IDisposable
    {
        private readonly byte* m_block;
        private int m_used;

        public CStrings() => m_block = (byte*)NativeMemory.AllocZeroed(256);

        public ж<C.@char> Add(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text + "\0");
            bytes.CopyTo(new Span<byte>(m_block + m_used, bytes.Length));
            nuint address = (nuint)(m_block + m_used);
            m_used += bytes.Length;
            return new NativeBox<C.@char>(address);
        }

        public void Dispose() => NativeMemory.Free(m_block);
    }

    private static string TextAt(ж<C.@char> word) =>
        Marshal.PtrToStringAnsi((nint)((NativeBox<C.@char>)word).NativeAddress) ?? "(null)";

    // The typed door: argv = {"a", "bb", nil} reads as count 2, length 3.
    [TestMethod]
    public void AGoArrayOfCPointersCrossesWhole()
    {
        CgoProbe.Require();

        using CStrings strings = new();
        array<ж<C.@char>> argv = new(3);
        argv[0] = strings.Add("a");
        argv[1] = strings.Add("bb");

        int32 r = OnAGoroutine(() => (int32)C.go2cs_probe_argv_count(new ElemRefBox<ж<C.@char>>(argv, 0)));

        Assert.AreEqual(203, r, "C read every word of the array, up to the NULL");
    }

    // C fills the first two words with its own strings; the third, which C left alone, keeps its identity.
    [TestMethod]
    public void CFillsAGoArrayOfCPointers()
    {
        CgoProbe.Require();

        using CStrings strings = new();
        array<ж<C.@char>> argv = new(3);
        ж<C.@char> kept = strings.Add("z");
        argv[2] = kept;

        int32 r = OnAGoroutine(() => (int32)C.go2cs_probe_argv_fill(new ElemRefBox<ж<C.@char>>(argv, 0), 2));

        Assert.AreEqual(2, r);
        Assert.AreEqual("c0", TextAt(argv[0]));
        Assert.AreEqual("c11", TextAt(argv[1]));
        Assert.IsTrue(ReferenceEquals(kept, argv[2]), "a word C left alone is the element it was");
    }

    // A word that is a pointer into Go memory: cgo's own panic, before the call (B-10).
    [TestMethod]
    public void AGoPointerAmongTheWordsPanics()
    {
        CgoProbe.Require();

        using CStrings strings = new();
        array<ж<C.@char>> argv = new(3);
        argv[0] = strings.Add("a");
        argv[1] = new StandardBox<C.@char>((sbyte)'g');

        string message = RefusalOf(() => C.go2cs_probe_argv_count(new ElemRefBox<ж<C.@char>>(argv, 0)));

        StringAssert.Contains(message, "argument of cgo function has Go pointer to unpinned Go pointer");
    }

    // H's point 2: C stores &argv[2] into argv[0], a pointer into the very Go memory it was handed (the mirror is one of
    // the call's pins). Go would keep it; go2cs refuses by name, as it does any word inside a pin. Not a false positive:
    // C must not store a Go pointer into Go memory that go2cs holds as boxes.
    [TestMethod]
    public void CStoringAnAddressIntoTheArrayItselfIsRefused()
    {
        CgoProbe.Require();

        array<ж<C.@char>> argv = new(3);

        string message = RefusalOf(() => C.go2cs_probe_argv_self(new ElemRefBox<ж<C.@char>>(argv, 0)));

        StringAssert.Contains(message, "C.go2cs_probe_argv_self");
        StringAssert.Contains(message, "returned an address inside the Go memory it was handed");
    }

    // The near-miss: an argv in C memory crosses by its own address, uncopied, as before.
    [TestMethod]
    public void AnArgvInCMemoryCrossesByItsAddress()
    {
        CgoProbe.Require();

        using CStrings strings = new();
        nint* words = (nint*)NativeMemory.AllocZeroed(3, (nuint)sizeof(nint));

        try
        {
            words[0] = (nint)((NativeBox<C.@char>)strings.Add("abc")).NativeAddress;
            int32 r = OnAGoroutine(() => (int32)C.go2cs_probe_argv_count(new NativeBox<ж<C.@char>>((nuint)words)));

            Assert.AreEqual(103, r);
        }
        finally
        {
            NativeMemory.Free(words);
        }
    }
}
