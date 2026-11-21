// PointerBoxStaticTableTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards lever E: no static lookup table in golib may be declared on a generic type with a field type closed over
/// that type's own type parameter.
/// </summary>
/// <remarks>
/// <para>
/// A static field of a generic class is new storage per type argument, and when the field's TYPE names the type
/// parameter (a <c>ConditionalWeakTable&lt;T[], ...&gt;</c>, or a table whose key or value is a type nested in the
/// generic class, which carries its parameter too) it is also a new instantiation of the table's whole method set:
/// entry, container, callback and enumerator types, and the collection interfaces over its key-value pair. Native
/// AOT compiles that set, under its own symbol names, for every type argument a program reaches. On the converted
/// TypeScript demo the pointer-box tables were 30% of the image's symbol names with lever G already in. Keyed by
/// reference, one non-generic table holds every type argument's entries without collision (<c>GoBoxTables</c>).
/// </para>
/// <para>
/// Only a JIT start-up or a Native AOT publish shows the cost, so this reads metadata instead, and runs wherever
/// GolibTests runs. It guards static TABLES; a per-type singleton (<c>Zero&lt;T&gt;.Default</c>, a lambda cache)
/// instantiates no collection and is out of scope. A table whose type does not name the parameter shares one
/// method set across instantiations and is out of scope too.
/// </para>
/// </remarks>
[TestClass]
public class PointerBoxStaticTableTests
{
    // Tables left per-type on purpose, each with its reason. An entry must still name a per-type table, or it is
    // stale and must go (the liveness test below), so this list cannot quietly widen what the guard admits.
    private static readonly Dictionary<(string Type, string Field), string> s_admitted = new()
    {
        [("go.builtin+Itab`1", "s_entries")] =
            "the type-assertion cache, one per interface type, on the assertion hot path; moving it changes the " +
            "lookup key and wants its own measurement",
        [("go.map`2", "s_sampledStorage")] =
            "one per map instantiation, keyed by the map's own store; outside lever E's measured scope",
    };

    private static readonly HashSet<Type> s_tableDefinitions =
    [
        typeof(ConditionalWeakTable<,>),
        typeof(Dictionary<,>),
        typeof(ConcurrentDictionary<,>),
        typeof(HashSet<>),
    ];

    [TestMethod]
    public void NoGolibStaticTableIsClosedOverItsDeclaringTypesParameter()
    {
        int genericTypes = 0;
        List<string> offenders = [];

        foreach (Type type in typeof(ж<>).Assembly.GetTypes().Where(type => type.IsGenericTypeDefinition))
        {
            genericTypes++;

            foreach (FieldInfo field in PerTypeTables(type))
            {
                if (!s_admitted.ContainsKey((type.FullName!, field.Name)))
                    offenders.Add($"{type.FullName}.{field.Name}");
            }
        }

        // A reader that finds no generic types cannot find an offender either.
        Assert.IsTrue(genericTypes > 100, $"read only {genericTypes} generic type definitions in golib: the walk is not seeing the assembly");

        Assert.AreEqual(0, offenders.Count,
            $"per-type static tables: {string.Join(", ", offenders)}. Each is a new instantiation of the table's whole " +
            "method set for every type argument a program reaches, which Native AOT compiles under its own names. Key " +
            "one non-generic table by reference instead (GoBoxTables), or admit it here with its reason");
    }

    [TestMethod]
    public void EveryAdmittedTableIsStillAPerTypeTable()
    {
        Assembly golib = typeof(ж<>).Assembly;

        foreach ((string typeName, string fieldName) in s_admitted.Keys)
        {
            Type type = golib.GetType(typeName);

            Assert.IsNotNull(type, $"admitted table's type {typeName} no longer exists: remove its entry");

            Assert.IsTrue(PerTypeTables(type).Any(field => field.Name == fieldName),
                $"{typeName}.{fieldName} is no longer a per-type static table: remove its entry");
        }
    }

    [TestMethod]
    public void TheReaderSeesAPerTypeTableWhereThereIsOne()
    {
        // The positive control: a table keyed by the type's own T, and one whose value is a type nested in the
        // generic class (it carries T without naming it), which is how FieldRefBox<T>'s offset records looked.
        string[] found = PerTypeTables(typeof(Control<>)).Select(field => field.Name).Order().ToArray();

        CollectionAssert.AreEqual(new[] { nameof(Control<int>.s_byNestedValue), nameof(Control<int>.s_byOwnT) }, found,
            "the reader did not report exactly the two per-type tables of the control: it cannot guard anything");
    }

    [TestMethod]
    public void TheReaderLeavesSharedTablesAndSingletonsAlone()
    {
        // The negative control: the same generic class's shared table and per-type singleton are not per-type tables.
        string[] found = PerTypeTables(typeof(Control<>)).Select(field => field.Name).ToArray();

        Assert.IsFalse(found.Contains(nameof(Control<int>.s_shared)), "a table whose type does not name T was reported");
        Assert.IsFalse(found.Contains(nameof(Control<int>.s_singleton)), "a per-type singleton was reported as a table");
    }

    private static class Control<T>
    {
        internal sealed class Nested;

        internal static readonly ConditionalWeakTable<T[], object> s_byOwnT = new();
        internal static readonly ConditionalWeakTable<object, Nested> s_byNestedValue = new();
        internal static readonly ConcurrentDictionary<Type, object> s_shared = new();
        internal static readonly T[] s_singleton = [];
    }

    // The static tables declared on a generic type whose field type names one of the type's own parameters,
    // directly or through a nested type of it.
    private static IEnumerable<FieldInfo> PerTypeTables(Type type)
    {
        const BindingFlags Declared = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        return type.GetFields(Declared).Where(field =>
            !field.IsLiteral &&
            field.FieldType.IsGenericType &&
            s_tableDefinitions.Contains(field.FieldType.GetGenericTypeDefinition()) &&
            Flatten(field.FieldType).Any(argument => argument.IsGenericParameter && argument.DeclaringMethod is null));
    }

    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;

        if (type.HasElementType)
        {
            foreach (Type inner in Flatten(type.GetElementType()!))
                yield return inner;
        }

        if (type.IsGenericType)
        {
            foreach (Type argument in type.GetGenericArguments())
            {
                foreach (Type inner in Flatten(argument))
                    yield return inner;
            }
        }
    }
}
