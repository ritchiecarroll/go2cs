// FieldOrderGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// golib reads a Go struct's fields in DECLARATION order at four sites: SliceHeaderBox and HeaderSliceBox (a slice
// header's pointer, len and cap) and GoLibcCall (a libc trampoline's arguments, and its result block's first word).
// They sorted by FieldInfo.MetadataToken, and Native AOT gives a member no token (reading one throws
// InvalidOperationException), so they take GetFields' order. .NET does not document that order. Measured 2026-10-06 on
// eight struct shapes (a ref+nint+nint header, mixed byte/ref/long/int/string/nint/bool/short, blittable, a libc args
// block, a struct split across two partial declarations, generics over a value and a reference type): GetFields answers
// in declaration order under the JIT and under Native AOT alike, and equals token order under the JIT. Field OFFSETS
// are not an order: a struct holding a reference gets auto layout.
//
// THIS GUARD IS THE PART THAT LASTS: under the JIT, where tokens exist, GetFields order equals token order for every
// value type of every converted assembly this host loads. A runtime that ever reorders GetFields fails here, not in a
// program. Its control plants a reordering (GoFieldMetadata.ReorderedForTest) and requires the guard to name it.
[TestClass]
public class FieldOrderGuardTests
{
    private struct OrderProbe { internal long a; internal object? b; internal int c; }

    // A notInHeapSlice-shaped header: [0] the array box, [1] len, [2] cap. HeaderSliceBox serves it.
    private struct HeaderInOrderProbe { internal ж<int>? array; internal nint len, cap; }

    // The same shape, with its own type so no static the box cached for another arm can answer for it.
    private struct HeaderReorderedProbe { internal ж<int>? array; internal nint len, cap; }

    // Every assembly this host loads that IS golib or references it (every converted package does), transitively.
    private static List<Assembly> ConvertedAssemblies()
    {
        Dictionary<string, Assembly> seen = new(StringComparer.Ordinal);
        Queue<Assembly> work = new([typeof(FieldOrderGuardTests).Assembly]);

        while (work.Count > 0)
        {
            foreach (AssemblyName reference in work.Dequeue().GetReferencedAssemblies())
            {
                if (seen.ContainsKey(reference.Name!))
                    continue;

                Assembly loaded;

                try
                {
                    loaded = Assembly.Load(reference);
                }
                catch (Exception)
                {
                    continue;
                }

                bool converted = loaded == typeof(GoFieldMetadata).Assembly ||
                                 loaded.GetReferencedAssemblies().Any(name => name.Name == typeof(GoFieldMetadata).Assembly.GetName().Name);

                if (!converted)
                    continue;

                seen[reference.Name!] = loaded;
                work.Enqueue(loaded);
            }
        }

        return [.. seen.Values.OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)];
    }

    private static IEnumerable<Type> ValueTypes(Assembly assembly)
    {
        Type?[] types;

        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types;
        }

        return types.Where(type => type is { IsValueType: true, IsEnum: false, IsPrimitive: false })!;
    }

    // The value types with an order to keep: two or more instance fields. One field has no order, and a type that
    // declares none (the compiler's explicit-size <PrivateImplementationDetails> blobs) is not a Go struct at all --
    // GoFieldMetadata.InstanceFields rightly refuses such a type as one whose metadata is gone.
    private static bool HasAnOrder(Type type) =>
        type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length >= 2;

    // The types whose fields golib's order (GoFieldMetadata.InstanceFields: GetFields' order) does NOT give in
    // metadata-token order, each with both orders spelled out.
    private static List<string> Misordered(IEnumerable<Type> types)
    {
        List<string> found = [];

        foreach (Type type in types.Where(HasAnOrder))
        {
            FieldInfo[] fields = GoFieldMetadata.InstanceFields(type);
            FieldInfo[] byToken = [.. fields.OrderBy(field => field.MetadataToken)];

            if (!fields.SequenceEqual(byToken))
                found.Add($"{type.FullName}: [{string.Join(", ", fields.Select(field => field.Name))}] vs tokens [{string.Join(", ", byToken.Select(field => field.Name))}]");
        }

        return found;
    }

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void EveryConvertedValueTypeReadsItsFieldsInTokenOrder()
    {
        List<Assembly> assemblies = ConvertedAssemblies();
        List<Type> types = [.. assemblies.SelectMany(ValueTypes)];
        List<Type> ordered = [.. types.Where(HasAnOrder)];

        // The population, so a zero is never a reading that measured nothing: golib plus the converted packages this
        // host references, and the value types among them that have an order to keep.
        TestContext?.WriteLine($"field-order guard: {assemblies.Count} converted assemblies, {types.Count} value types, {ordered.Count} with two or more fields ({ordered.Sum(type => GoFieldMetadata.InstanceFields(type).Length)} fields)");
        Assert.IsTrue(assemblies.Count >= 10, $"the guard read only {assemblies.Count} converted assemblies");
        Assert.IsTrue(ordered.Count >= 500, $"the guard read only {ordered.Count} value types with an order to keep");

        List<string> misordered = Misordered(types);
        Assert.AreEqual(0, misordered.Count, "GetFields no longer answers in declaration (token) order:\n  " + string.Join("\n  ", misordered.Take(20)));
    }

    [TestMethod]
    public void ControlAPlantedReorderingIsNamed()
    {
        Assert.AreEqual(0, Misordered([typeof(OrderProbe)]).Count, "unplanted, the probe reads in token order");

        GoFieldMetadata.ReorderedForTest = typeof(OrderProbe);

        try
        {
            List<string> misordered = Misordered([typeof(OrderProbe)]);
            Assert.AreEqual(1, misordered.Count, "a reordered GetFields must fail the guard");
            StringAssert.Contains(misordered[0], nameof(OrderProbe));
        }
        finally
        {
            GoFieldMetadata.ReorderedForTest = null;
        }
    }

    // The sites READ golib's order, not the token's: under a planted reordering the header box sees [cap, len, array],
    // whose first field is not an array box, and declines the pair. A site that still sorted by token would put the
    // fields back and serve it.
    [TestMethod]
    public void TheHeaderBoxTakesGolibsOrderNotTheTokens()
    {
        Assert.IsTrue(HeaderSliceBox<HeaderInOrderProbe, slice<int>>.Applies, "control: a header in declaration order is served");

        GoFieldMetadata.ReorderedForTest = typeof(HeaderReorderedProbe);

        try
        {
            Assert.IsFalse(HeaderSliceBox<HeaderReorderedProbe, slice<int>>.Applies,
                "the header box re-sorted the fields it was given (by metadata token, which Native AOT does not have)");
        }
        finally
        {
            GoFieldMetadata.ReorderedForTest = null;
        }
    }
}
