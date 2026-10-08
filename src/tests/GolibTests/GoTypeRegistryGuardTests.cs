// GoTypeRegistryGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// golib suppresses its reflection warnings on the strength of the trimming registry (GoTypeRegistry;
// docs/PLAN-golib-full-trim.md, stage 2): every Go type and every interface adapter carries a DynamicDependency that
// keeps the members golib reads. A suppression justified by a registry must fail LOUDLY when the registry misses one,
// so this walks every converted assembly this host loads, as built, and names each Go type of a package class that its
// package's module initializer does not register, and each adapter whose constructor does not register it.
[TestClass]
public class GoTypeRegistryGuardTests
{
    private const BindingFlags Declared = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    // Every assembly this host loads that references golib (every converted package does), transitively.
    private static List<Assembly> ConvertedAssemblies()
    {
        string golib = typeof(GoTypeRegistry).Assembly.GetName().Name!;
        Dictionary<string, Assembly> seen = new(StringComparer.Ordinal);
        Queue<Assembly> work = new([typeof(GoTypeRegistryGuardTests).Assembly]);

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

                if (loaded.GetReferencedAssemblies().All(name => name.Name != golib))
                    continue;

                seen[reference.Name!] = loaded;
                work.Enqueue(loaded);
            }
        }

        return [.. seen.Values.OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)];
    }

    private static Type[] TypesOf(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return [.. ex.Types.Where(type => type is not null)!];
        }
    }

    private static Type Definition(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

    // The types a member's DynamicDependency attributes name.
    private static IEnumerable<Type> DependenciesOf(MemberInfo member) =>
        member.GetCustomAttributes<DynamicDependencyAttribute>().Where(dependency => dependency.Type is not null).Select(dependency => Definition(dependency.Type!));

    [TestMethod]
    public void EveryGoTypeOfEveryConvertedPackageIsRegistered()
    {
        List<string> missing = [];
        int packages = 0, goTypes = 0;

        foreach (Assembly assembly in ConvertedAssemblies())
        {
            foreach (Type packageClass in TypesOf(assembly).Where(type => type.DeclaringType is null && type.Name.EndsWith("_package", StringComparison.Ordinal)))
            {
                HashSet<Type> registered = packageClass.GetMethods(Declared)
                    .Where(method => method.IsDefined(typeof(ModuleInitializerAttribute), false))
                    .SelectMany(DependenciesOf)
                    .ToHashSet();

                Type[] marked = packageClass.GetNestedTypes(Declared).Where(type => type.IsDefined(typeof(GoTypeAttribute), false)).ToArray();

                if (marked.Length > 0)
                    packages++;

                goTypes += marked.Length;
                missing.AddRange(marked.Where(type => !registered.Contains(Definition(type))).Select(type => $"{assembly.GetName().Name}: {type.FullName}"));
            }
        }

        Assert.IsTrue(goTypes > 500, $"the walk must see the corpus's Go types: {goTypes} in {packages} package classes");
        Assert.AreEqual(0, missing.Count, $"{missing.Count} of {goTypes} Go types are NOT registered for trimming:\n" + string.Join("\n", missing.Take(40)));
    }

    [TestMethod]
    public void EveryInterfaceAdapterRegistersItself()
    {
        List<string> missing = [];
        int adapters = 0;

        foreach (Assembly assembly in ConvertedAssemblies())
        {
            foreach (Type type in TypesOf(assembly).Where(type => type is { IsClass: true, IsAbstract: false } &&
                                                                  (typeof(IжAdapter).IsAssignableFrom(type) || typeof(IValueAdapter).IsAssignableFrom(type))))
            {
                // A generated adapter carries [GeneratedCode]; a hand-written one (golib's own, a hand-owned file's)
                // is not the generator's to register and is listed nowhere here.
                if (!type.IsDefined(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute), false))
                    continue;

                adapters++;

                if (!type.GetConstructors(Declared).Any(ctor => DependenciesOf(ctor).Contains(Definition(type))))
                    missing.Add($"{assembly.GetName().Name}: {type.FullName}");
            }
        }

        Assert.IsTrue(adapters > 0, "the walk must see generated interface adapters");
        Assert.AreEqual(0, missing.Count, $"{missing.Count} of {adapters} adapters do NOT register themselves for trimming:\n" + string.Join("\n", missing.Take(40)));
    }

    [TestMethod]
    public void GolibRegistersItsOwnGoTypes()
    {
        MethodInfo? register = typeof(GoTypeRegistry).GetMethod("RegisterGolibTypes", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.IsNotNull(register, "golib's own registration");
        Assert.IsTrue(register!.IsDefined(typeof(ModuleInitializerAttribute), false), "it is a module initializer, so the trimmer keeps it");

        HashSet<Type> registered = DependenciesOf(register).ToHashSet();

        foreach (Type type in new[] { typeof(@string), typeof(uintptr), typeof(slice<>), typeof(map<,>), typeof(array<>), typeof(channel<>), typeof(ж<>) })
            Assert.IsTrue(registered.Contains(type), $"golib registers {type.Name}");
    }
}
