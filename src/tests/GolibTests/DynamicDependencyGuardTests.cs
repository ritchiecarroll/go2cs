// DynamicDependencyGuardTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A DynamicDependency that names its target by STRING (type name and assembly) is checked by nothing at compile time.
// When the name does not resolve, the trimmer warns IL2036 in every trimmed publish of a converted program and keeps
// nothing, so the suppression that relies on the dependency rests on nothing either. The trimmer reads the type name
// as a documentation-comment ID, not a reflection name: a nested type is Outer.Nested, never Outer+Nested, which
// Type.GetType accepts and the trimmer does not. This resolves every string-named dependency golib declares, as built,
// the way the trimmer does: the type by its documentation ID in the named assembly, then the member by name.
[TestClass]
public class DynamicDependencyGuardTests
{
    private const BindingFlags Declared = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    [TestMethod]
    public void EveryStringNamedDynamicDependencyInGolibResolves()
    {
        List<string> unresolved = [];
        int checkedCount = 0;

        foreach (Type type in typeof(GoTypeRegistry).Assembly.GetTypes())
        {
            IEnumerable<MemberInfo> members = type.GetMethods(Declared).Cast<MemberInfo>()
                .Concat(type.GetConstructors(Declared))
                .Concat(type.GetFields(Declared))
                .Append(type);

            foreach (MemberInfo member in members)
            {
                foreach (DynamicDependencyAttribute dependency in member.GetCustomAttributes<DynamicDependencyAttribute>(false))
                {
                    if (dependency.TypeName is null)
                        continue;

                    checkedCount++;
                    string site = $"{type.FullName}.{member.Name}: DynamicDependency(\"{dependency.MemberSignature}\", \"{dependency.TypeName}\", \"{dependency.AssemblyName}\")";
                    Type? target = ByDocumentationId(dependency.TypeName, dependency.AssemblyName);

                    if (target is null)
                    {
                        unresolved.Add($"{site}: the type does not resolve");
                        continue;
                    }

                    if (dependency.MemberSignature is null)
                        continue;

                    // The member signature is a name, optionally followed by a parameter list.
                    string name = dependency.MemberSignature.Split('(')[0];

                    if (target.GetMember(name, Declared).Length == 0)
                        unresolved.Add($"{site}: {target.FullName} declares no member {name}");
                }
            }
        }

        Assert.IsTrue(checkedCount >= 3, $"only {checkedCount} string-named DynamicDependency attribute(s) were found in golib, where it declares at least 3, so the guard did not see them");
        Assert.AreEqual(0, unresolved.Count, $"{unresolved.Count} of {checkedCount} string-named DynamicDependency target(s) do not resolve:\n  " + string.Join("\n  ", unresolved));
    }

    // The type whose documentation-comment ID (namespace and enclosing types joined by '.', generic arity as `n) is
    // the given name, in the named assembly; null when the assembly does not load or no type has that ID.
    private static Type? ByDocumentationId(string typeName, string? assemblyName)
    {
        Assembly assembly;

        try
        {
            assembly = assemblyName is null ? typeof(GoTypeRegistry).Assembly : Assembly.Load(assemblyName);
        }
        catch (Exception)
        {
            return null;
        }

        return assembly.GetTypes().FirstOrDefault(type => type.FullName?.Replace('+', '.') == typeName);
    }
}
