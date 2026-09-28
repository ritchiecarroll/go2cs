// GoShiftNegativeCountTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards golib's signed-count <c>GoShift</c> overloads: Go panics with the runtime error "negative
/// shift amount" (runtime.panicshift) when a signed count is below zero at run time, and the converter
/// widens a signed count to <c>int64</c> so these overloads bind. A count of zero or more takes the
/// unsigned guard's answer, including Go's zero (or sign fill) for a count at or past the width.
/// </summary>
[TestClass]
public class GoShiftNegativeCountTests
{
    // Read through fields so nothing folds at compile time.
    private static int64 s_minusOne = -1;
    private static int64 s_three = 3;
    private static int64 s_seventy = 70;

    [TestMethod]
    public void ANegativeCountPanicsWithGosRuntimeError()
    {
        foreach (Action shift in new Action[]
        {
            () => ((nint)1).Lsh(s_minusOne),
            () => ((int64)1).Rsh(s_minusOne),
            () => ((uint32)1).Lsh(s_minusOne),
            () => ((int8)1).Rsh(s_minusOne),
            () => ((uint64)1).Lsh(s_minusOne),
            () => ((uintptr)1).Rsh(s_minusOne),
        })
        {
            PanicException panic = Assert.ThrowsException<PanicException>(shift);
            StringAssert.Contains(panic.Message, "runtime error: negative shift amount");
        }
    }

    [TestMethod]
    public void TheCompoundTwinsPanicToo()
    {
        nint x = 5;
        uint16 y = 5;

        Assert.ThrowsException<PanicException>(() => x.LshAssign(s_minusOne));
        Assert.ThrowsException<PanicException>(() => y.RshAssign(s_minusOne));
        Assert.AreEqual((nint)5, x, "a panicking shift leaves its target alone");
    }

    [TestMethod]
    public void ANonNegativeCountTakesTheUnsignedGuardsAnswer()
    {
        Assert.AreEqual((nint)8, ((nint)1).Lsh(s_three));
        Assert.AreEqual((int64)(-1), ((int64)(-8)).Rsh(s_seventy), "a signed right shift past the width sign-fills");
        Assert.AreEqual((uint32)0, ((uint32)0xF0).Lsh(s_seventy), "a left shift past the width is 0");
        Assert.AreEqual((int8)(-16), ((int8)(-128)).Rsh(s_three));

        int32 z = 1;
        z.LshAssign(s_three);
        Assert.AreEqual(8, z);
    }
}
