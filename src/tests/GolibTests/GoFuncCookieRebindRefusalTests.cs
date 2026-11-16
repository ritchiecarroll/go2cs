using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// W18 (G, the sqlite .db demo on win-x64): a func word read through a delegate type its func cannot be rebound to
/// panics, and the refusal must say WHICH two types met. It printed <c>Type.Name</c>, so both sides read
/// <c>Func`6</c> and the one thing that differed, a struct argument lifted twice (lib's TLARGE_INTEGER, libc's
/// largeInteger), was invisible. Both delegate types are now printed in full, each generic argument by its full name.
/// </summary>
[TestClass]
public class GoFuncCookieRebindRefusalTests
{
    // Two distinct structs of one layout: Go would call them one type, C# does not, as with a struct lifted twice.
    public struct LiftA
    {
        public uint Lo;
        public int Hi;
    }

    public struct LiftB
    {
        public uint Lo;
        public int Hi;
    }

    private static long SumA(LiftA x) => x.Lo + (long)x.Hi * 10;

    [TestMethod]
    public void ARefusedRebindNamesBothDelegateTypesInFull()
    {
        nuint word = GoFuncCookie.Of(new Func<LiftA, long>(SumA));

        PanicException refused = Assert.ThrowsException<PanicException>(() => GoFuncCookie.Func<Func<LiftB, long>>(word));

        StringAssert.Contains(refused.Message, "holds a System.Func<GolibTests.GoFuncCookieRebindRefusalTests+LiftA, System.Int64>",
            "the delegate the word holds, generic arguments by full name");
        StringAssert.Contains(refused.Message, "cannot be read as System.Func<GolibTests.GoFuncCookieRebindRefusalTests+LiftB, System.Int64>",
            "the delegate type it was read through");
        Assert.IsFalse(refused.Message.Contains('`'), "no arity-only name survives: " + refused.Message);
    }
}
