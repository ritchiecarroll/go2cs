using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// GetGoMethodSetCandidates walks every scanned extension method once per (element, pointer) key. For a VALUE key
// it must also exclude pointer-receiver methods, which costs custom-attribute reads (IsPointerSetByRefReceiver).
// Under Native AOT each read decodes reflection metadata, so the order of the two filters is the cost: the receiver
// must match before the attribute test runs, or every method of every package pays it for every value key.
[TestClass]
public class MethodSetCandidateFilterTests
{
    [TestMethod]
    public void AValueMethodSetTestsThePointerRuleOnlyOnMethodsOfItsOwnReceiver()
    {
        TypeExtensions.ResetExtensionScanForTest();
        int scanned = TypeExtensions.GetExtensionMethods().Length;
        int tested = 0;
        List<MethodInfo> candidates;

        TypeExtensions.PointerSetTestProbeForTest = _ => tested++;

        try
        {
            candidates = TypeExtensions.GetGoMethodSetCandidates(typeof(sort_package.IntSlice), false);
        }
        finally
        {
            TypeExtensions.PointerSetTestProbeForTest = null;
        }

        // Control: the key resolved sort.IntSlice's value methods (Len, Less, Swap, Search, Sort) out of a real scan.
        Assert.IsTrue(scanned > 1000 && candidates.Count >= 5, $"control: {scanned} scanned, {candidates.Count} candidates");

        Assert.IsTrue(tested < 50,
            $"the pointer-receiver attribute test ran on {tested} of {scanned} scanned methods for one value key; " +
            "it must run only on methods whose receiver is sort.IntSlice");
    }
}
