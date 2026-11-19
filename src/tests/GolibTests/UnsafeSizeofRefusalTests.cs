using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// unsafe.Sizeof<T> answers through GoReflect.TryGoSizeOf. Where that declined, it fell back to Marshal.SizeOf<T>(), which
// is a MARSHALLING size and not Go's: dnsmessage's SVCParam {Key uint16; Value []byte} marshals as 48 where Go says 32.
// So the fallback answered a plausible wrong number when it ran, and because it is generic, ILC compiled a Marshal struct
// thunk for every T that reaches it. When T holds an array-backed value field (slice, string, array), that thunk is a
// "will always throw" stub in every Native AOT image that reaches the T, although the line never runs for any T in the
// corpus (each one answers through TryGoSizeOf).
//
// A size TryGoSizeOf cannot derive now refuses by name, as the abi descriptor's Size() does (AbiUnknownArrayLengthTests).
// Red first, at 0c1d295ea8: "unsafe.Sizeof answered 16 (Marshal.SizeOf<TooLarge>() is 16) ... instead of refusing".
[TestClass]
public class UnsafeSizeofRefusalTests
{
    // A struct past the address space: [1<<62]int64 is 2^65 bytes in Go's layout, so TryGoSizeOf declines it (Go answers
    // such a type with a panic in reflect.StructOf and with a compile error in source). No converted struct has this
    // shape; it stands in for the routes TryGoSizeOf declines (an overflow, a length that is not carried, the depth
    // bound). Marshal sizes it anyway, from the managed field's reference, which is the plausible wrong number.
    private struct TooLarge
    {
        [GoArrayDims(1L << 62)] public array<long> A;
    }

    // The shape that does reach the line in the corpus: dnsmessage's SVCParam.
    private struct LikeSvcParam
    {
        public ushort Key;
        public slice<byte> Value;
    }

    [TestMethod]
    public void SizeofOfAnUnknownLayoutRefusesByName()
    {
        Assert.IsFalse(GoReflect.TryGoSizeOf(typeof(TooLarge), null, out _), "precondition: TryGoSizeOf declines the type");

        PanicException? panic = null;
        uintptr answered = 0;
        try { answered = @unsafe.Sizeof(new TooLarge()); }
        catch (PanicException e) { panic = e; }

        if (panic is null)
            Assert.Fail($"unsafe.Sizeof answered {answered} (Marshal.SizeOf<TooLarge>() is {System.Runtime.InteropServices.Marshal.SizeOf<TooLarge>()}) for a size TryGoSizeOf could not derive, instead of refusing");

        StringAssert.StartsWith(panic.Message, "runtime error: unsafe.Sizeof: the Go size of ", "the refusal's prefix");
        StringAssert.Contains(panic.Message, "TryGoSizeOf declined it", "the refusal does not say why");
        StringAssert.Contains(panic.Message, nameof(TooLarge), "the refusal does not name the type");
    }

    [TestMethod]
    public void SizeofOfADerivableLayoutAnswersGoSize()
    {
        Assert.AreEqual((uintptr)32, @unsafe.Sizeof(new LikeSvcParam()), "SVCParam's shape: Go's size is 32");
        Assert.AreEqual((uintptr)16, @unsafe.Sizeof((@string)"go"), "string");
        Assert.AreEqual((uintptr)24, @unsafe.Sizeof(new slice<byte>()), "slice");
    }
}
