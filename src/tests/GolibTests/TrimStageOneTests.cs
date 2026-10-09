// TrimStageOneTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// Trim stage 1 (docs/PLAN-golib-full-trim.md) replaced two by-NAME member lookups a trimmer cannot see with lookups
// it can: a box's Value/ValueSlot getter is closed from the statically named ж<> (GoReflect.BoxGetter), and a
// delegate's Invoke is read in one place (GoReflect.DelegateInvoke). These pin that each answers exactly what the
// by-name lookup answered, so the trim-safe spelling cannot drift from the behaviour it replaced.
[TestClass]
public class TrimStageOneTests
{
    [TestMethod]
    public void BoxGetterIsTheGetterTheNameLookupFound()
    {
        Type box = typeof(ж<int>);

        // The by-name lookups the trim-safe spelling replaced, verbatim.
        Assert.AreEqual(box.GetProperty("ValueSlot")!.GetGetMethod(), GoReflect.BoxGetter(box, valueSlot: true), "ValueSlot on ж<int>");
        Assert.AreEqual(box.GetProperty("Value")!.GetGetMethod(), GoReflect.BoxGetter(box, valueSlot: false), "Value on ж<int>");
    }

    [TestMethod]
    public void BoxGetterDispatchesToAConcreteKindsOverride()
    {
        heap(41, out ж<int> box);
        Type kind = box.GetType();

        Assert.AreNotEqual(typeof(ж<int>), kind, "the heap box is a concrete kind deriving from ж<int>");

        MethodInfo getter = GoReflect.BoxGetter(kind, valueSlot: true)!;

        Assert.AreEqual(typeof(ж<int>), getter.DeclaringType, "closed over the ж<T> the kind derives from");
        Assert.AreEqual(41, (int)getter.Invoke(box, null)!, "a virtual call through the base getter reads the kind's own slot");
    }

    [TestMethod]
    public void BoxGetterAnswersNullForANonBox()
    {
        Assert.IsNull(GoReflect.BoxGetter(typeof(int), valueSlot: true));
        Assert.IsNull(GoReflect.BoxGetter(typeof(string), valueSlot: false));
    }

    [TestMethod]
    public void DelegateInvokeReadsTheSignature()
    {
        MethodInfo invoke = GoReflect.DelegateInvoke(typeof(Func<int, string>))!;

        Assert.AreEqual(typeof(string), invoke.ReturnType);
        Assert.AreEqual(typeof(int), invoke.GetParameters()[0].ParameterType);
        Assert.IsNull(GoReflect.DelegateInvoke(typeof(int)), "a non-delegate type has no Invoke");
    }
}
