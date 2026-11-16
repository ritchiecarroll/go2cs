using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// B2's value decode (COORD ruling 2026-11-15): <see cref="ж{T}.GoValue"/>, Go's <c>*p</c> read as a VALUE, reads a
/// native box over a Go struct whose only references are arrays at Go's own field offsets, into a fresh managed value.
/// </summary>
/// <remarks>
/// <para>
/// sqlite's crypt_blowfish reads <c>*(*TBF_ctx)(unsafe.Pointer(&amp;[1042]uint32{...}))</c>. <c>TBF_ctx</c> is kept
/// out of inline arrays, so its arrays are <c>array&lt;T&gt;</c> references and its CLR layout is not its Go layout:
/// the whole-value read through <see cref="ж{T}.Value"/> is refused by name (the layout floor, or B0'). A Go value read
/// is a COPY, so decoding the bytes into a fresh value is Go's answer, and nothing can alias the copy, so a decode
/// never writes back. Writes keep <see cref="ж{T}.Value"/> and its refusals.
/// </para>
/// <para>
/// The struct below is TBF_ctx as go2cs emits it: a constructor that builds each array at its Go length (the generated
/// parameterless constructor running the field initializers), and the <c>[GoArrayDims(0, 1042)]</c> stamp B2's G1 puts
/// on a field whose zero length hides the inner one. Its Go layout is then KNOWN: FPS at 0 (size 0), Fs at 0, FP at 0,
/// FS at 72, size 4168.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NativeBoxValueDecodeTests
{
    private const int Words = 1042;

    public struct CtxFs
    {
        public array<uint32> FP;
        public array<array<uint32>> FS;

        public CtxFs()
        {
            FP = new(18);
            FS = new(4, () => new array<uint32>(256));
        }
    }

    public struct Ctx
    {
        [GoArrayDims(0, 1042)] public array<array<uint32>> FPS;
        public CtxFs Fs;

        public Ctx()
        {
            FPS = new(0, () => new array<uint32>(1042));
            Fs = new();
        }
    }

    // Arrays and a string: the string has no image in memory, so the decode refuses the struct by naming that field.
    public struct Labeled
    {
        public array<uint32> Words;
        public @string Label;

        public Labeled()
        {
            Words = new(4);
            Label = "";
        }
    }

    private static PanicException? Attempt(Action read)
    {
        try
        {
            read();
            return null;
        }
        catch (PanicException panic)
        {
            return panic;
        }
    }

    // word i holds i * 7 + 1, so every Go offset reads a value no other offset holds.
    private static array<uint32> Pattern()
    {
        array<uint32> words = new(Words);

        for (int i = 0; i < Words; i++)
            words[i] = (uint32)(i * 7 + 1);

        return words;
    }

    private static void AssertIsThePattern(Ctx value, string path)
    {
        Assert.AreEqual(0, (int)value.FPS.Length, $"{path}: FPS is [0][1042]");
        Assert.AreEqual(18, (int)value.Fs.FP.Length, $"{path}: FP is [18]");
        Assert.AreEqual(4, (int)value.Fs.FS.Length, $"{path}: FS is [4][256]");

        for (int i = 0; i < 18; i++)
            Assert.AreEqual((uint)(i * 7 + 1), (uint)value.Fs.FP[i], $"{path}: Fs.FP[{i}] reads word {i}");

        for (int k = 0; k < 4; k++)
        {
            Assert.AreEqual(256, (int)value.Fs.FS[k].Length, $"{path}: FS[{k}] is [256]");

            for (int j = 0; j < 256; j++)
            {
                int word = 18 + k * 256 + j;
                Assert.AreEqual((uint)(word * 7 + 1), (uint)value.Fs.FS[k][j], $"{path}: Fs.FS[{k}][{j}] reads word {word}");
            }
        }
    }

    [TestMethod]
    public void TheStampMakesTheGoLayoutKnownAndProvesItIsNotTheClrLayout()
    {
        Assert.IsNotNull(GoReflect.GoFieldOffsets(typeof(Ctx)), "G1's stamp carries the 1042 the zero instance cannot measure");
        Assert.AreEqual(4168, (int)GoReflect.GoSizeOf(typeof(Ctx)), "TBF_ctx's Go size");
        Assert.IsNotNull(GoReflect.ClrLayoutMismatch(typeof(Ctx)), "its arrays are references where Go stores bytes");
    }

    [TestMethod]
    public void AValueReadThroughAReinterpretDecodesAtGoOffsets()
    {
        array<uint32> words = Pattern();
        ж<Ctx> view = builtin.Ꮡ(words).Reinterpret<array<uint32>, Ctx>();

        Assert.IsInstanceOfType(view, typeof(NativeBox<Ctx>), "precondition: the address route");

        AssertIsThePattern(view.GoValue(), "decode");

        PanicException? whole = Attempt(() => _ = view.Value);

        Assert.IsNotNull(whole, "the whole-value ref read is still refused: only the value read decodes");
    }

    [TestMethod]
    public void TheDecodedValueIsACopy()
    {
        array<uint32> words = Pattern();
        ж<Ctx> view = builtin.Ꮡ(words).Reinterpret<array<uint32>, Ctx>();

        Ctx value = view.GoValue();
        value.Fs.FP[0] = 0xdead;
        value.Fs.FS[1][1] = 0xbeef;

        Assert.AreEqual(1u, (uint)words[0], "a write to the copy reached the source");
        Assert.AreEqual((uint)((18 + 257) * 7 + 1), (uint)words[18 + 257], "a write to the copy reached the source");
        AssertIsThePattern(view.GoValue(), "a second read");
    }

    [TestMethod]
    public void CMemoryDecodesAsWell()
    {
        int bytes = Words * 4;
        nint block = Marshal.AllocHGlobal(bytes);

        try
        {
            for (int i = 0; i < Words; i++)
                Marshal.WriteInt32(block, i * 4, i * 7 + 1);

            ж<Ctx> view = (ж<Ctx>)(uintptr)(nuint)block;

            AssertIsThePattern(view.GoValue(), "C memory");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AStructWithAStringIsRefusedByNamingTheField()
    {
        array<uint32> words = Pattern();
        ж<Labeled> view = builtin.Ꮡ(words).Reinterpret<array<uint32>, Labeled>();

        PanicException? panic = Attempt(() => _ = view.GoValue());

        Assert.IsNotNull(panic, "a string has no image in memory, so the struct cannot be decoded");
        StringAssert.Contains(panic.Message ?? "", "cannot read Labeled as a value", "the decode's refusal, naming the type");
        StringAssert.Contains(panic.Message ?? "", "field Label is a managed reference", "and the field");
    }

    [TestMethod]
    public void ANilPointerPanicsAsANilDereference()
    {
        ж<Ctx> nil = go.ж<Ctx>.NilBox;

        PanicException? panic = Attempt(() => _ = nil.GoValue());

        Assert.IsNotNull(panic);
        StringAssert.Contains(panic.Message ?? "", "nil pointer dereference");
    }

    [TestMethod]
    public void AManagedBoxReadsItsValue()
    {
        Ctx ctx = new();
        ctx.Fs.FP[3] = 42;
        ж<Ctx> box = builtin.Ꮡ(ctx);

        Ctx value = box.GoValue();

        Assert.AreEqual(42u, (uint)value.Fs.FP[3], "a box over a managed T is ~p");
    }
}
