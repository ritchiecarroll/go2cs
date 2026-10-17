using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The (A) follow-up: a HAND-OWNED file declares its embeds as the converter does, `/*embed*/` fields in Go order, so the
// zero-size layout arc can place them. runtime.mutex (`struct{ lockRankStruct; key uintptr }`, lock ranking off) is the
// live case: its zero-size embed and its key both sit at Go's offset 0 and the struct is Go's 8 bytes. While the
// hand-owned runtime2.cs spelled the embed as a `partial ref` property, the generator's one-byte backing field followed
// key in sequential layout and the CLR size was 16.
[TestClass]
public class HandOwnedEmbedLayoutTests
{
    [TestMethod]
    public void RuntimeMutexIsLaidOutAtGosSizeAndOffsets()
    {
        (int size, string? mismatch) = runtime_package.GoMutexClrLayout;

        Assert.AreEqual(8, size, "runtime.mutex is not Go's 8 bytes");
        Assert.IsNull(mismatch, $"runtime.mutex's CLR layout is not its Go layout: {mismatch}");
    }
}
