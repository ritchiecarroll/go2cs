// WaitGroupTwinSelectionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GenTests.TwinBoxTests;

namespace GenTests;

/// <summary>
/// M:N, sync.WaitGroup.Wait's twin (docs/phase4/DESIGN-mn-waitgroup-twin.md, row R5): the SELECTION, as a standing row.
/// A function whose only park is <c>wg.Wait()</c> becomes a twin because Wait has a twin that is not weak, and its twin
/// awaits <c>Waitꞏtwin</c>. The seat showed that once, in the twin report of the behavioral program's M:N build; nothing
/// went red if the generator stopped choosing the member, because the program prints the same either way.
/// </summary>
/// <remarks>
/// The library here is a MODEL of go.sync's two members, under their real names and shapes (an extension class
/// <c>syncᴛWaitGroup</c> over <c>ж&lt;sync_package.WaitGroup&gt;</c>, in a referenced assembly). The real member's shape
/// and its missing weak mark are frozen by GolibTests' StdlibTwinSurfaceTests; this row holds what the generator does
/// with a member of that shape.
/// </remarks>
[TestClass]
public class WaitGroupTwinSelectionTests
{
    private enum Twin
    {
        Present,
        Weak,
        Absent
    }

    private static string SyncModel(Twin twin)
    {
        string member = twin switch
        {
            Twin.Present => "public static global::go.golib.GoTask Waitꞏtwin(this ж<sync_package.WaitGroup> Ꮡwg) => default;",
            Twin.Weak => "[global::go.golib.GoParkWeak] public static global::go.golib.GoTask Waitꞏtwin(this ж<sync_package.WaitGroup> Ꮡwg) => default;",
            _ => ""
        };

        return $$"""
            using System;

            namespace go;

            public static partial class sync_package
            {
                public partial struct WaitGroup
                {
                    public nint v;
                }
            }

            public static partial class syncᴛWaitGroup
            {
                public static void Wait(this ж<sync_package.WaitGroup> Ꮡwg) { }

                {{member}}
            }
            """;
    }

    // The behavioral program's waitAll, as the converter emits it: its only park is wg.Wait().
    private const string WaitAll = """
            internal static void waitAll(ж<sync_package.WaitGroup> wg)
            {
                wg.Wait();
            }

        """;

    [TestMethod]
    public void AFunctionThatParksOnlyInWaitIsTwinnedAndAwaitsTheWaitTwin()
    {
        (string twins, Diagnostic[] errors) = RunWith(SyncModel(Twin.Present), null, WaitAll);

        Assert.AreEqual(0, errors.Length, Errors(errors));

        int start = twins.IndexOf("waitAllꞏtwin(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "waitAll parks only through wg.Wait(), and Wait's twin makes it a twin:\n" + twins);

        int end = twins.IndexOf("#line default", start, StringComparison.Ordinal);
        string twin = end < 0 ? twins.Substring(start) : twins.Substring(start, end - start);
        StringAssert.Contains(twin, "await wg.Waitꞏtwin()", "waitAll's twin awaits the Wait twin");
    }

    // The two controls: the same function is NOT twinned when the member is marked weak, and when it is absent. So the
    // row above reads the member, and reads its mark.
    [TestMethod]
    public void AWeakOrAbsentWaitTwinLeavesTheFunctionPlain_TheControls()
    {
        (string weak, Diagnostic[] weakErrors) = RunWith(SyncModel(Twin.Weak), null, WaitAll);

        Assert.AreEqual(0, weakErrors.Length, Errors(weakErrors));
        Assert.IsFalse(weak.Contains("waitAllꞏtwin(", StringComparison.Ordinal), "a weak Wait twin keeps waitAll plain");

        (string absent, Diagnostic[] absentErrors) = RunWith(SyncModel(Twin.Absent), null, WaitAll);

        Assert.AreEqual(0, absentErrors.Length, Errors(absentErrors));
        Assert.IsFalse(absent.Contains("waitAllꞏtwin(", StringComparison.Ordinal), "with no Wait twin, waitAll cannot park and has no twin");
    }
}
