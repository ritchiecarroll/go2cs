using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// runtime's traceMap (tracemap.cs), the append-only table the tracer's string and stack tables use.
/// newTraceMapNode carved each node and its bytes from a traceRegionAlloc block that sysAlloc handed
/// back as a raw address, then reinterpreted those bytes as a traceMapNode, a struct that holds
/// references (its children and its data slice). The block's data array has no managed storage, so
/// the first put threw IndexOutOfRangeException (traceregion.cs:88); on the goroutines of the runtime
/// row's TestTraceMapConcurrent that took the host down.
/// </summary>
[TestClass]
public class RuntimeTraceMapTests
{
    [TestMethod]
    public void APutReturnsGoIdsAndASecondPutFindsTheValue()
    {
        string[] values = ["a", "b", "aa", "ab", "ba", "bb"];

        string reading = GoTraceMapProbe(values, 3);

        StringBuilder expected = new();

        for (int round = 0; round < 3; round++)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < values.Length; i++)
                    expected.Append($"{values[i]}={i + 1},{pass == 0};");
            }
        }

        Assert.AreEqual(expected.ToString(), reading);
    }
}
