using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The FUNC COOKIE: the word a Go func value answers when read as a word
/// (<c>uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&amp;f)))</c>), which an OS carries through an lParam and
/// <c>*(*func())(unsafe.Pointer(&amp;lparam))</c> resolves back (runtime's nestedCall / callback pair;
/// ruling 2026-09-28 15:03 (2)). The band arms pin the property the syscall door rests on: a cookie is
/// NOT a managed-pointer order token, so the door passes it, and it still names no user memory, so a
/// native dereference of one is refused rather than read.
/// </summary>
[TestClass]
public class GoFuncCookieTests
{
    private delegate void OtherFuncShape();

    [TestMethod]
    public void ACookieIsStableNonZeroAndInItsOwnBand()
    {
        Action fn = () => { };
        nuint word = GoFuncCookie.Of(fn);

        Assert.AreNotEqual((nuint)0, word);
        Assert.AreEqual(word, GoFuncCookie.Of(fn), "every read of one func answers one word");
        Assert.IsTrue(GoFuncCookie.IsCookie(word));
        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(word), "the syscall door refuses order tokens; a cookie must pass it");
        Assert.IsTrue(ManagedPointerTokens.NamesNoUserMemory(word), "a native dereference of a cookie must be refused, never read");
        Assert.IsFalse(GoFuncCookie.IsCookie(unchecked((nuint)0x8000_8000_0000_1000UL)), "a caller-span token is not a cookie");
        Assert.IsFalse(GoFuncCookie.IsCookie(unchecked((nuint)0xFFFF_8000_0000_1000UL)), "a synthetic PC is not a cookie");
    }

    [TestMethod]
    public void DistinctFuncsGetDistinctCookiesAndNilIsTheZeroWord()
    {
        Action a = () => { };
        Action b = () => Console.Write("");

        Assert.AreNotEqual(GoFuncCookie.Of(a), GoFuncCookie.Of(b));
        Assert.AreEqual((nuint)0, GoFuncCookie.Of(null));
    }

    [TestMethod]
    public void UnsafePointerOfFuncCarriesTheCookieAndNilIsNil()
    {
        Action fn = () => { };

        Assert.AreEqual(GoFuncCookie.Of(fn), (nuint)(uintptr)@unsafe.Pointer.OfFunc(fn));
        Assert.IsTrue(@unsafe.Pointer.OfFunc(null) == nil);
    }

    [TestMethod]
    public void AUintptrSlotHoldingACookieReadsBackAsTheSameFunc()
    {
        int calls = 0;
        Action fn = () => calls++;
        ж<nuint> lparam = new StandardBox<nuint>(GoFuncCookie.Of(fn));

        ж<Action> asFunc = lparam.Reinterpret<nuint, Action>();

        Assert.AreSame(fn, asFunc.Value, "the cookie resolves to the very delegate it was minted for");
        asFunc.Value();
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void AGoUintptrSlotHoldingACookieReadsBackAsTheSameFunc()
    {
        // The EMISSION's slot type: runtime's callback heaps its lparam as go.uintptr, never nuint
        // (`Ꮡlparam.Reinterpret<uintptr, Action>()`). ReferenceEquals, not AreSame: a miss must fail
        // the assert, never format -- i.e. dereference -- the word read as a reference.
        int calls = 0;
        Action fn = () => calls++;
        ж<uintptr> lparam = new StandardBox<uintptr>((uintptr)GoFuncCookie.Of(fn));

        ж<Action> asFunc = lparam.Reinterpret<uintptr, Action>();

        Assert.IsTrue(ReferenceEquals(fn, asFunc.ValueSlot), "a go.uintptr slot holding a cookie resolves to its func");
        asFunc.ValueSlot();
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void AFuncReadThroughAnotherDelegateShapeIsReboundNotLost()
    {
        int calls = 0;
        Action fn = () => calls++;
        ж<nuint> lparam = new StandardBox<nuint>(GoFuncCookie.Of(fn));

        lparam.Reinterpret<nuint, OtherFuncShape>().Value();

        Assert.AreEqual(1, calls, "a named func type over the same signature reaches the same target and method");
    }

    [TestMethod]
    public void AZeroWordReadsAsGosNilFunc()
    {
        ж<nuint> lparam = new StandardBox<nuint>((nuint)0);

        // ValueSlot, the structural read the emission uses (`….ValueSlot()` in runtime's callback):
        // `.Value` is the value-peeking dereference guard, which refuses a box HOLDING nil -- and a
        // nil func is exactly what the zero word must read as. Calling it then panics, as Go's does.
        Assert.IsNull(lparam.Reinterpret<nuint, Action>().ValueSlot);
    }

    [TestMethod]
    public void ACookieWhoseFuncIsGoneIsRefusedByName()
    {
        nuint word = MintForAFuncNobodyHolds();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (GoFuncCookie.Resolve(word) is not null)
            Assert.Inconclusive("the JIT kept the func alive past its frame; the refusal arm cannot be reached on this run");

        ж<nuint> lparam = new StandardBox<nuint>(word);
        PanicException refused = Assert.ThrowsException<PanicException>(() => lparam.Reinterpret<nuint, Action>());
        StringAssert.Contains(refused.Message, "names no live func");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint MintForAFuncNobodyHolds()
    {
        object captured = new();
        Action fn = () => GC.KeepAlive(captured);
        return GoFuncCookie.Of(fn);
    }

    // sqlite-cs S1 (docs/phase4/DESIGN-sqlite-cs-funcptr.md section 3.1): a delegate with no target is a
    // converted top-level Go func, which Go gives one static funcval. Its word is keyed on the method.

    private delegate int Combine(int a, int b);

    private static int Add(int a, int b) => a + b;

    private static int Mul(int a, int b) => a * b;

    private static int Sub(int a, int b) => a - b;

    [TestMethod]
    public void AStaticFunctionAnswersOneWordUnderEveryDelegateType()
    {
        Func<int, int, int> first = Add;
        Func<int, int, int> second = new(Add);
        Combine named = Add;
        nuint word = GoFuncCookie.Of(first);

        Assert.IsFalse(ReferenceEquals(first, second), "the arm needs two delegate instances over one method");
        Assert.IsTrue(GoFuncCookie.IsCookie(word));
        Assert.AreEqual(word, GoFuncCookie.Of(second), "a second delegate instance over the same static function");
        Assert.AreEqual(word, GoFuncCookie.Of(named), "a named delegate type over the same static function");
        Assert.AreNotEqual(word, GoFuncCookie.Of((Func<int, int, int>)Mul), "another function answers another word");
    }

    [TestMethod]
    public void ADelegateBuiltOverAnotherStandsForIt()
    {
        Func<int, int, int> plain = Mul;
        Combine converted = new(plain);
        int captured = 3;
        Func<int, int, int> closure = (a, b) => a + b + captured;
        Combine convertedClosure = new(closure);

        Assert.AreEqual(GoFuncCookie.Of(plain), GoFuncCookie.Of(converted), "a converted static function keeps its word");
        Assert.AreEqual(GoFuncCookie.Of(closure), GoFuncCookie.Of(convertedClosure), "a converted closure keeps its word");
    }

    [TestMethod]
    public void AStaticFunctionsWordRecoversAsEitherTypeAndItsRebindIsCached()
    {
        nuint word = GoFuncCookie.Of((Func<int, int, int>)Add);

        Assert.AreEqual(5, GoFuncCookie.Func<Func<int, int, int>>(word)(2, 3));

        Combine rebound = GoFuncCookie.Func<Combine>(word);
        Assert.AreEqual(6, rebound(2, 4), "a recovery as another delegate type over the same signature is rebound");
        Assert.AreSame(rebound, GoFuncCookie.Func<Combine>(word), "a static function's rebind is cached per delegate type");
        Assert.IsNull(GoFuncCookie.Func<Combine>(0), "word 0 is Go's nil func");
    }

    [TestMethod]
    public void AStaticFunctionsWordOutlivesEveryDelegateOverIt()
    {
        nuint word = MintForAStaticFunctionNobodyHolds();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.AreEqual(7, GoFuncCookie.Func<Func<int, int, int>>(word)(9, 2), "Go's static funcval is never freed");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint MintForAStaticFunctionNobodyHolds() => GoFuncCookie.Of(new Func<int, int, int>(Sub));

    [TestMethod]
    public void ANumberThatIsNoFuncWordPanicsByNameOnBothRoutes()
    {
        nuint notAWord = 0x1234;

        PanicException direct = Assert.ThrowsException<PanicException>(() => GoFuncCookie.Func<Action>(notAWord));
        StringAssert.Contains(direct.Message, "is not a func word");

        ж<nuint> slot = new StandardBox<nuint>(notAWord);
        PanicException viaReinterpret = Assert.ThrowsException<PanicException>(() => slot.Reinterpret<nuint, Action>());
        StringAssert.Contains(viaReinterpret.Message, "is not a func word");
    }

    [TestMethod]
    public void AThirtyTwoBitRuntimeIsRefusedAtTheMint()
    {
        Assert.ThrowsException<PlatformNotSupportedException>(() => GoFuncCookie.RequireWordSize(4));
        GoFuncCookie.RequireWordSize(8);
    }

    [TestMethod]
    public void SixtyFourThreadsMintingStaticFunctionsAgreeOnOneWordEach()
    {
        Func<int, int, int>[] functions = [R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, R13, R14, R15];
        const int threads = 64;
        nuint[,] seen = new nuint[threads, functions.Length];
        using System.Threading.Barrier start = new(threads);
        System.Threading.Thread[] workers = new System.Threading.Thread[threads];

        for (int t = 0; t < threads; t++)
        {
            int thread = t;

            workers[t] = new System.Threading.Thread(() =>
            {
                start.SignalAndWait();

                // Each thread mints in its own order, alternating the two delegate types, each time over
                // a fresh delegate instance.
                for (int k = 0; k < functions.Length; k++)
                {
                    int i = (k * 7 + thread) % functions.Length;
                    Type shape = (thread + k) % 2 == 0 ? typeof(Func<int, int, int>) : typeof(Combine);
                    seen[thread, i] = GoFuncCookie.Of(Delegate.CreateDelegate(shape, functions[i].Method));
                }
            });

            workers[t].Start();
        }

        foreach (System.Threading.Thread worker in workers)
            worker.Join();

        System.Collections.Generic.HashSet<nuint> words = [];

        for (int i = 0; i < functions.Length; i++)
        {
            for (int t = 1; t < threads; t++)
                Assert.AreEqual(seen[0, i], seen[t, i], $"function {i} was split across two words");

            Assert.IsTrue(words.Add(seen[0, i]), $"function {i} shares a word with another function");
        }
    }

    private static int R0(int a, int b) => a + b + 0;
    private static int R1(int a, int b) => a + b + 1;
    private static int R2(int a, int b) => a + b + 2;
    private static int R3(int a, int b) => a + b + 3;
    private static int R4(int a, int b) => a + b + 4;
    private static int R5(int a, int b) => a + b + 5;
    private static int R6(int a, int b) => a + b + 6;
    private static int R7(int a, int b) => a + b + 7;
    private static int R8(int a, int b) => a + b + 8;
    private static int R9(int a, int b) => a + b + 9;
    private static int R10(int a, int b) => a + b + 10;
    private static int R11(int a, int b) => a + b + 11;
    private static int R12(int a, int b) => a + b + 12;
    private static int R13(int a, int b) => a + b + 13;
    private static int R14(int a, int b) => a + b + 14;
    private static int R15(int a, int b) => a + b + 15;
}
