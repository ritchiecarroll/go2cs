// GenericVirtualMethodGuardTests.cs - Gbtc
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

/// <summary>
/// No interface that golib's per-type generics implement may declare a GENERIC method outside a closed, named baseline.
/// Under Native AOT a generic virtual method needs one body per constructed implementing type TIMES every type argument
/// that reaches a call, and golib's generics are instantiated once per converted type: IReinterpretRoot.AliasAs&lt;TDst&gt;()
/// on ж&lt;T&gt; took the hop's ILC identical-body folding from 22-28 minutes to over 52 without finishing (P1's bisection,
/// first bad merge 1e7e92da23; removing the one call site published in 28.4). A 25-minute publish is too slow to be a gate;
/// this structural rule is not.
/// </summary>
/// <remarks>
/// The baseline is CLOSED: adding a signature needs a ruling and a measured AOT publish arm. Each entry names why it stays.
/// </remarks>
[TestClass]
public class GenericVirtualMethodGuardTests
{
    private const string PredatesPtrconv =
        "predates ptrconv: every box implements it, and it is inside the measured 22-28 min folding baseline (ruled 2026-10-20)";

    private static readonly Dictionary<string, string> s_baseline = new()
    {
        ["go.IPointer`1: go.ж`1[TElem] at[TElem](IntPtr)"] = PredatesPtrconv,
        ["go.IPointer`1: go.ж`1[TElem] of[TElem](go.FieldPtrFunc`2[T,TElem])"] = PredatesPtrconv,
        ["go.IPointer`1: go.ж`1[TElem] of[TElem](go.FieldRefFunc`1[TElem])"] = PredatesPtrconv,
        ["go.IPointer`1: go.ж`1[TElem] of[TElem](go.FieldRefFunc`2[T,TElem])"] = PredatesPtrconv,
    };

    // Two populations, because a generic virtual method can be declared on an INTERFACE or on a CLASS (a virtual or
    // abstract generic method a derived box overrides, e.g. ж<T>.TryPinnedReinterpret<TDst>): every interface implemented
    // by a generic golib type, and every golib type's OWN declared virtual generic methods. An explicit interface
    // implementation is itself virtual (and final); it is the interface's method, so it is counted once, there.
    private static IEnumerable<string> GenericVirtualMethods()
    {
        const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        Type[] types = typeof(ж<>).Assembly.GetTypes();

        foreach (Type type in types.Where(type => type.IsGenericTypeDefinition && !type.IsInterface))
        {
            foreach (Type contract in type.GetInterfaces())
            {
                Type definition = contract.IsGenericType ? contract.GetGenericTypeDefinition() : contract;

                foreach (MethodInfo method in definition.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.IsGenericMethodDefinition && method.IsVirtual)
                        yield return $"{definition.FullName}: {method}";
                }
            }
        }

        foreach (Type type in types.Where(type => !type.IsInterface))
        {
            Type definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            foreach (MethodInfo method in definition.GetMethods(Declared))
            {
                if (!method.IsGenericMethodDefinition || !method.IsVirtual || method.IsFinal)
                    continue;

                // An override is the declaring base's method again; count the slot where it is introduced.
                MethodInfo introduced = method.GetBaseDefinition();
                Type owner = introduced.DeclaringType is { IsGenericType: true } generic ? generic.GetGenericTypeDefinition() : introduced.DeclaringType!;

                yield return $"{owner.FullName}: {introduced}";
            }
        }
    }

    [TestMethod]
    public void NoInterfaceOfAGolibGenericTypeDeclaresAGenericMethodOutsideTheBaseline()
    {
        string[] found = [.. GenericVirtualMethods().Distinct().OrderBy(text => text, StringComparer.Ordinal)];
        string[] unexpected = [.. found.Where(signature => !s_baseline.ContainsKey(signature))];

        Assert.AreEqual(0, unexpected.Length, "generic virtual methods on golib's per-type generics, outside the baseline:\n" + string.Join("\n", unexpected));
    }

    [TestMethod]
    public void EveryBaselineEntryStillExists()
    {
        HashSet<string> found = [.. GenericVirtualMethods()];
        string[] stale = [.. s_baseline.Keys.Where(signature => !found.Contains(signature))];

        Assert.AreEqual(0, stale.Length, "baseline entries no longer declared (remove them; the set only shrinks without a ruling):\n" + string.Join("\n", stale));
    }
}
