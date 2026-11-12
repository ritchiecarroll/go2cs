// MethodHolderRuntimeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// The readers under test take a converted package's SHAPE from metadata: the class `<pkg>_package` in a
// namespace below `go`, and beside it one class per type, marked [GoMethodHost], for the methods the type
// declares. The fixtures are real classes of that shape, in block-scoped namespaces so the file can declare
// more than one.
namespace go.holderfixtures
{
    public static class shape_package
    {
        public struct Box { public nint n; }

        public struct Pair<K, V> { public K k; public V v; }

        public static nint Area() => 1;
    }

    [GoMethodHost(typeof(shape_package.Box))]
    public static class shapeᴛBox
    {
        public static nint Size(this shape_package.Box b) => b.n;
    }

    // A generic type is named open, as the converter names it.
    [GoMethodHost(typeof(shape_package.Pair<,>))]
    public static class shapeᴛPair
    {
        public static nint Len<K, V>(this shape_package.Pair<K, V> p) => 2;
    }

    // Marked for a type that no class declares: the attribute names no package class.
    public struct Loose { public nint n; }

    [GoMethodHost(typeof(Loose))]
    public static class looseᴛLoose
    {
        public static nint Size(this Loose l) => l.n;
    }
}

namespace go
{
    public static class rootshape_package
    {
        public struct Box { public nint n; }
    }

    [GoMethodHost(typeof(rootshape_package.Box))]
    public static class rootshapeᴛBox
    {
        public static nint Size(this rootshape_package.Box b) => b.n;
    }
}

namespace GolibTests
{
    /// <summary>
    /// Holds golib's readers of a method's class to the method holder: a Go method is declared in the class
    /// <c>&lt;pkg&gt;ᴛ&lt;Type&gt;</c> of its receiver type, marked <c>[GoMethodHost(typeof(T))]</c>, and Go
    /// names it by the type's package.
    /// </summary>
    /// <remarks>
    /// Failing-first record: before <see cref="GoSyntheticPC.GoNameOf"/> read a holder through to its package
    /// class it named a holder's method <c>holderfixtures/shapeᴛBox.Size</c>, a name Go never prints.
    /// </remarks>
    [TestClass]
    public class MethodHolderRuntimeTests
    {
        private static MethodInfo MethodOf(Type type, string name) =>
            type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;

        [TestMethod]
        public void AHolderStandsUnderThePackageClassOfItsReceiverType()
        {
            Assert.AreEqual(typeof(go.holderfixtures.shape_package), GoMethodHostAttribute.PackageClassOf(typeof(go.holderfixtures.shapeᴛBox)));
            Assert.AreEqual(typeof(go.holderfixtures.shape_package), GoMethodHostAttribute.PackageClassOf(typeof(go.holderfixtures.shapeᴛPair)),
                "a generic type's holder names the type open");
            Assert.AreEqual(typeof(rootshape_package), GoMethodHostAttribute.PackageClassOf(typeof(rootshapeᴛBox)));

            Assert.IsTrue(GoMethodHostAttribute.IsHost(typeof(go.holderfixtures.shapeᴛBox)));
            Assert.IsTrue(GoMethodHostAttribute.IsHost(typeof(go.holderfixtures.shapeᴛPair)));
        }

        [TestMethod]
        public void AClassThatIsNoHolderStandsUnderItself()
        {
            Assert.AreEqual(typeof(go.holderfixtures.shape_package), GoMethodHostAttribute.PackageClassOf(typeof(go.holderfixtures.shape_package)),
                "a package class");
            Assert.AreEqual(typeof(go.holderfixtures.looseᴛLoose), GoMethodHostAttribute.PackageClassOf(typeof(go.holderfixtures.looseᴛLoose)),
                "a class marked for a type that no class declares");
            Assert.AreEqual(typeof(MethodHolderRuntimeTests), GoMethodHostAttribute.PackageClassOf(typeof(MethodHolderRuntimeTests)),
                "a class that is not static");

            Assert.IsFalse(GoMethodHostAttribute.IsHost(typeof(go.holderfixtures.shape_package)));
            Assert.IsFalse(GoMethodHostAttribute.IsHost(typeof(go.holderfixtures.looseᴛLoose)));
            Assert.IsFalse(GoMethodHostAttribute.IsHost(typeof(MethodHolderRuntimeTests)));
        }

        [TestMethod]
        public void AMethodOfAHolderIsNamedByItsReceiverTypesPackage()
        {
            Assert.AreEqual("holderfixtures/shape.Size", GoSyntheticPC.GoNameOf(MethodOf(typeof(go.holderfixtures.shapeᴛBox), "Size")));
            Assert.AreEqual("holderfixtures/shape.Len", GoSyntheticPC.GoNameOf(MethodOf(typeof(go.holderfixtures.shapeᴛPair), "Len")));
            Assert.AreEqual("rootshape.Size", GoSyntheticPC.GoNameOf(MethodOf(typeof(rootshapeᴛBox), "Size")), "a package directly under `go`");

            // Control: a function of the package class is named as it always was.
            Assert.AreEqual("holderfixtures/shape.Area", GoSyntheticPC.GoNameOf(MethodOf(typeof(go.holderfixtures.shape_package), "Area")));
        }
    }
}
