using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// GoZeroSizeFacts&lt;T&gt;.IsZeroSize's pre-gate (CertainlyHasStorage) may only say "has storage" where the reflective
/// Classify agrees: pre-gate-true implies Classify-false, over every value type this test process loads.
/// </summary>
/// <remarks>
/// <para>
/// The pre-gate is what lets Native AOT fold the zero-size test in every slice&lt;T&gt; element read (item 7, cause 1,
/// G's option A). A pre-gate that claimed storage for a type Go sizes at zero would route that type's slices onto
/// the allocating path and hand out real elements where Go hands out zerobase. That is a silent semantic change,
/// so the guard asks the REAL generic property, closed over each type by reflection, against Classify.
/// </para>
/// <para>
/// The plant: the same checker over a deliberately wrong pre-gate (one that also claims storage for every
/// reference-free struct) must name a violation, and the violation must be a zero-size type. Without that, a
/// checker that compared nothing would pass.
/// </para>
/// </remarks>
[TestClass]
public class ZeroSizePreGateTests
{
    // A zero-size struct of the test's own, so the population is never without one.
    private struct Nothing;

    private struct NothingTwice
    {
        public Nothing A;
        public Nothing B;
    }

    private static readonly MethodInfo s_classify = typeof(GoZeroSizeFacts).GetMethod("Classify", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static bool Classify(Type type) => (bool)s_classify.Invoke(null, [type])!;

    // The real pre-gate: GoZeroSizeFacts<type>.CertainlyHasStorage, the property every read site inlines.
    private static bool RealPreGate(Type type) =>
        (bool)typeof(GoZeroSizeFacts<>).MakeGenericType(type).GetProperty("CertainlyHasStorage", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;

    // Every closed, non-byref-like value type in the assemblies this process has loaded, with golib's and the test
    // assembly's own forced in.
    private static List<Type> Population()
    {
        _ = typeof(GoZeroSizeFacts).Assembly;
        _ = typeof(ZeroSizePreGateTests).Assembly;

        List<Type> types = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] defined;

            try
            {
                defined = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException partial)
            {
                defined = partial.Types.Where(type => type is not null).ToArray()!;
            }

            types.AddRange(defined.Where(type => type is { IsValueType: true, ContainsGenericParameters: false, IsByRefLike: false, IsPointer: false }));
        }

        return types;
    }

    // The checker: every type the pre-gate says has storage must classify as not zero-size. Answers the violations,
    // how many types the pre-gate decided, and how many Classify refused (and so were not compared).
    private static (List<Type> violations, int decided, int refused) Check(IEnumerable<Type> population, Func<Type, bool> preGate)
    {
        List<Type> violations = [];
        int decided = 0, refused = 0;

        foreach (Type type in population)
        {
            bool certainlyHasStorage;

            try
            {
                certainlyHasStorage = preGate(type);
            }
            catch (Exception exception) when (exception is ArgumentException or TypeLoadException or TargetInvocationException or NotSupportedException)
            {
                continue;
            }

            if (!certainlyHasStorage)
                continue;

            decided++;

            bool zeroSize;

            try
            {
                zeroSize = Classify(type);
            }
            catch (TargetInvocationException)
            {
                refused++;
                continue;
            }

            if (zeroSize)
                violations.Add(type);
        }

        return (violations, decided, refused);
    }

    [TestMethod]
    public void ThePreGateClaimsStorageOnlyWhereClassifyAgrees()
    {
        List<Type> population = Population();
        (List<Type> violations, int decided, int refused) = Check(population, RealPreGate);

        Assert.AreEqual(0, violations.Count, "pre-gate-true but zero-size: " + string.Join(", ", violations.Select(type => type.FullName)));
        Assert.IsTrue(decided > 1000, $"the pre-gate decided only {decided} of {population.Count} value types; the population is too thin to mean anything");
        Assert.IsTrue(refused < decided / 10, $"Classify refused {refused} of the {decided} decided types; too many went uncompared");
    }

    [TestMethod]
    public void ThePreGateLeavesEveryZeroSizeStructToClassify()
    {
        Assert.IsFalse(RealPreGate(typeof(Nothing)), "an empty struct must reach Classify");
        Assert.IsFalse(RealPreGate(typeof(NothingTwice)), "a struct of zero-size fields must reach Classify");
        Assert.IsFalse(RealPreGate(typeof(EmptyStruct)), "golib's EmptyStruct must reach Classify");
        Assert.IsTrue(GoZeroSizeFacts<Nothing>.IsZeroSize, "and the property still answers zero-size for it");
        Assert.IsTrue(GoZeroSizeFacts<NothingTwice>.IsZeroSize);
        Assert.IsFalse(GoZeroSizeFacts<nuint>.IsZeroSize, "nuint is decided by the pre-gate");
        Assert.IsFalse(GoZeroSizeFacts<string>.IsZeroSize, "a reference is decided by the pre-gate");
    }

    [TestMethod]
    public void AWrongPreGateIsCaught()
    {
        // The plant: also claim storage for every reference-free struct. That is wrong for every zero-size type.
        Func<Type, bool> wrong = type => RealPreGate(type) || type is { IsValueType: true, IsPrimitive: false, IsEnum: false };

        (List<Type> violations, _, _) = Check(Population(), wrong);

        Assert.IsTrue(violations.Contains(typeof(Nothing)), "the checker must name the test's own empty struct under a wrong pre-gate");
        Assert.IsTrue(violations.All(Classify), "every violation it names is a zero-size type");
    }
}
