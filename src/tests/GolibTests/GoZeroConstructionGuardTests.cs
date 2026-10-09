// GoZeroConstructionGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// Trim stage 3b moved golib's Go zero value from reflection to a generated hook. ZeroFacts asked a converted struct for
// its parameterless constructor and ran it through Activator; it now builds only a struct carrying IGoZeroConstructed,
// which go2cs-gen writes where the constructor builds something (IsNeedy, or a directional channel's stamp), and takes
// `default` for every other one. The two answers differ only for a struct whose constructor builds something and that
// carries no hook: there the old code built a zero value and the new one returns `default`.
//
// THIS GUARD IS THE EQUIVALENCE: for every converted struct this host loads, a constructed value that differs from
// `default` (a byte of it is not zero) carries the hook. A generic definition is closed over nint, or object when its
// constraints refuse nint. Its control plants an unhooked struct whose constructor builds an array and requires the guard
// to name it, and requires the scan to have seen sha3.SHA3, a known needy struct.
[TestClass]
public class GoZeroConstructionGuardTests
{
    private struct UnhookedNeedyProbe
    {
        internal array<byte> a = new(8);

        public UnhookedNeedyProbe() { }
    }

    private struct PlainProbe
    {
        internal nint x;
        internal object? y;

        public PlainProbe() { }
    }

    // Every assembly this host loads that IS golib or references it, transitively (FieldOrderGuardTests' walk).
    private static List<Assembly> ConvertedAssemblies()
    {
        Dictionary<string, Assembly> seen = new(StringComparer.Ordinal);
        Queue<Assembly> work = new([typeof(GoZeroConstructionGuardTests).Assembly]);
        string golib = typeof(IGoZeroConstructed).Assembly.GetName().Name!;

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

                if (loaded.GetName().Name != golib && loaded.GetReferencedAssemblies().All(name => name.Name != golib))
                    continue;

                seen[reference.Name!] = loaded;
                work.Enqueue(loaded);
            }
        }

        return [.. seen.Values.OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)];
    }

    // The converted structs: what ZeroFacts used to construct (a [GoType] value type with a public parameterless
    // constructor), each generic definition closed so it can be built.
    private static IEnumerable<Type> ConvertedStructs(Assembly assembly)
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

        foreach (Type? type in types)
        {
            if (type is not { IsValueType: true, IsEnum: false } || !type.IsDefined(typeof(GoTypeAttribute), false))
                continue;

            Type? closed = type.IsGenericTypeDefinition ? Closed(type) : type;

            if (closed?.GetConstructor(Type.EmptyTypes) is not null)
                yield return closed;
        }
    }

    private static Type? Closed(Type definition)
    {
        foreach (Type argument in new[] { typeof(nint), typeof(object) })
        {
            try
            {
                return definition.MakeGenericType([.. definition.GetGenericArguments().Select(_ => argument)]);
            }
            catch (ArgumentException)
            {
            }
        }

        return null;
    }

    // A constructed value's bytes: all zero exactly when the constructor built nothing `default` lacks.
    private static bool ConstructedIsDefault<T>() where T : struct
    {
        T value = (T)Activator.CreateInstance(typeof(T))!;

        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>()).IndexOfAnyExcept((byte)0) < 0;
    }

    private static readonly MethodInfo s_constructedIsDefault =
        typeof(GoZeroConstructionGuardTests).GetMethod(nameof(ConstructedIsDefault), BindingFlags.Static | BindingFlags.NonPublic)!;

    // The structs whose constructed value is not `default`, split by whether they carry the hook.
    private static (List<Type> hooked, List<string> unhooked) Built(IEnumerable<Type> types)
    {
        List<Type> hooked = [];
        List<string> unhooked = [];

        foreach (Type type in types)
        {
            bool isDefault;

            try
            {
                isDefault = (bool)s_constructedIsDefault.MakeGenericMethod(type).Invoke(null, null)!;
            }
            catch (TargetInvocationException ex)
            {
                unhooked.Add($"{type.FullName}: its constructor threw {ex.InnerException?.GetType().Name}");
                continue;
            }

            if (isDefault)
                continue;

            if (typeof(IGoZeroConstructed).IsAssignableFrom(type))
                hooked.Add(type);
            else
                unhooked.Add(type.FullName!);
        }

        return (hooked, unhooked);
    }

    [TestMethod]
    public void EveryConvertedStructWhoseConstructorBuildsSomethingCarriesTheHook()
    {
        List<Assembly> assemblies = ConvertedAssemblies();
        List<Type> structs = [.. assemblies.SelectMany(ConvertedStructs)];

        (List<Type> hooked, List<string> unhooked) = Built(structs);

        Console.WriteLine($"{assemblies.Count} converted assemblies, {structs.Count} converted structs, {hooked.Count} constructed and hooked");

        Assert.IsTrue(hooked.Contains(typeof(go.crypto.sha3_package.SHA3)), "the scan saw sha3.SHA3, a needy struct, and found it hooked");
        Assert.AreEqual(0, unhooked.Count, "built but unhooked (GoZero would now return default):\n" + string.Join("\n", unhooked));
    }

    [TestMethod]
    public void TheGuardNamesAnUnhookedStructWhoseConstructorBuildsSomething()
    {
        (List<Type> hooked, List<string> unhooked) = Built([typeof(UnhookedNeedyProbe), typeof(PlainProbe)]);

        Assert.AreEqual(0, hooked.Count);
        CollectionAssert.AreEqual(new[] { typeof(UnhookedNeedyProbe).FullName }, unhooked, "only the probe that builds an array");
    }
}
