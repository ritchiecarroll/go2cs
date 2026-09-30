using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// OPTION 3 of the hash-token sizing (COORD ruling 2026-09-30): a ж token names ONE live allocation.
// ж tokens were minted from the CLR identity hash (AllocationBase), which collides between live objects
// (the first collision among live ж<object> boxes came after 4622 boxes). A reference-bearing box
// REGISTERS its token on every uintptr conversion and ManagedPointerTokens.Register keeps the LAST
// writer, so `(ж<T>)(uintptr)p` of the FIRST box's number handed back the SECOND box: a silent wrong
// object, since both answer Resolve's verify.
[TestClass]
public class PointerTokenUniquenessTests
{
    [TestMethod]
    public void APointerRoundTripReturnsItsOwnBoxAmongCollidingIdentityHashes()
    {
        Dictionary<int, ж<object>> byHash = new();
        List<ж<object>> live = new();
        ж<object>? first = null, second = null;

        for (int made = 0; second is null && made < 2_000_000; made++)
        {
            ref object value = ref heap((object)new object(), out ж<object> box);
            _ = value;
            live.Add(box);

            if (byHash.TryGetValue(RuntimeHelpers.GetHashCode(box), out ж<object>? prior))
            {
                first = prior;
                second = box;
            }
            else
            {
                byHash[RuntimeHelpers.GetHashCode(box)] = box;
            }
        }

        Assert.IsNotNull(second, "no two live boxes shared an identity hash in 2M; the arm measured nothing");

        uintptr firstNumber = first!;
        uintptr secondNumber = second;
        ж<object> back = (ж<object>)firstNumber;

        Assert.AreNotEqual((ulong)(nuint)firstNumber, (ulong)(nuint)secondNumber, "two live allocations must have two numbers");
        Assert.IsTrue(ReferenceEquals(back, first), "the first box's number resolved to another live box");

        System.GC.KeepAlive(live);
    }
}
