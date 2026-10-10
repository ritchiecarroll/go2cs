// TrimStage3c2bTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using sort = go.sort_package;
using time = go.time_package;

namespace GolibTests;

// Trim stage 3c-2b (docs/PLAN-golib-full-trim.md, section 9): reflect's type-only sites keyed by a POINTEE or a FIELD type
// (NewPointerBox, CanonicalNilPointer) reach a type's operations (GoTypeOps<T>) through the type's own generated face
// (IGoTypeOpsSource, go2cs-gen, on a type whose pointer its compilation spells) or the builtins table, at FIXED depth --
// and only what none of those names (an interface, a func type, a pointer to a pointer, golib's containers) falls back to
// closing the operations over the type at run time. Under the JIT the fallback answers the same instance, so FromSource
// pins the route.
[TestClass]
public class TrimStage3c2bTests
{
    private interface ISomeInterface { }

    [TestMethod]
    public void AGeneratedStructsOpsComeFromItsOwnFace()
    {
        // package time spells ж<Timer>, so Timer carries the face (go2cs-gen's TypeOpsScope).
        Assert.AreSame(GoTypeOps<time.Timer>.Instance, GoTypeOps.FromSource(typeof(time.Timer)), "a converted struct names its own ops");
        Assert.IsNull(GoTypeOps.FromSource(typeof(ж<time.Timer>)), "a pointer to it is a reference type: the fallback's, not a face's");
        Assert.AreSame(GoTypeOps<ж<time.Timer>>.Instance, GoTypeOps.Of(typeof(ж<time.Timer>)));
    }

    // The face goes only where the type's own compilation spells a pointer to it. Package time writes Go's *Time receivers
    // as `ref` and never spells ж<Time>, and package sort never points to IntSlice: both fall back, which under the JIT
    // answers the same operations and under Native AOT is trim stage 3d's boundary.
    [TestMethod]
    public void ATypeItsPackageNeverPointsToFallsBack()
    {
        Assert.IsNull(GoTypeOps.FromSource(typeof(time.Time)));
        Assert.IsNull(GoTypeOps.FromSource(typeof(sort.IntSlice)));
        Assert.AreSame(GoTypeOps<time.Time>.Instance, GoTypeOps.Of(typeof(time.Time)));
        Assert.AreSame(GoTypeOps<sort.IntSlice>.Instance, GoTypeOps.Of(typeof(sort.IntSlice)));
    }

    // THE HAZARD PIN: a face on a generic container names the container's ops, those make its box live, and the box builds a
    // container of the container, whose face names the next level -- under Native AOT's default partial trim ILC expanded
    // without bound and died (System.OverflowException, measured 2026-10-09: the stage-3 table's rows D and E; row G, which
    // removed only these faces, compiles). A container's ops come from the escape registry (3c-2b(iii)) or the fallback.
    [TestMethod]
    public void GolibsContainersCarryNoFace()
    {
        foreach (Type container in new[] { typeof(array<>), typeof(slice<>), typeof(map<,>), typeof(channel<>) })
            Assert.IsFalse(typeof(IGoTypeOpsSource).IsAssignableFrom(container), $"{container.Name} must not carry the operations face");

        Assert.IsNull(GoTypeOps.FromSource(typeof(slice<int>)));
        Assert.IsNull(GoTypeOps.FromSource(typeof(array<byte>)));
        Assert.IsNull(GoTypeOps.FromSource(typeof(map<@string, int>)));
        Assert.IsNull(GoTypeOps.FromSource(typeof(channel<int>)));

        // Under the JIT the fallback answers them all the same.
        Assert.AreSame(GoTypeOps<slice<int>>.Instance, GoTypeOps.Of(typeof(slice<int>)));
        Assert.AreSame(GoTypeOps<channel<int>>.Instance, GoTypeOps.Of(typeof(channel<int>)));
    }

    [TestMethod]
    public void TheBuiltinsAreNamedByTheTable()
    {
        Assert.AreSame(GoTypeOps<int>.Instance, GoTypeOps.Of(typeof(int)));
        Assert.AreSame(GoTypeOps<ж<int>>.Instance, GoTypeOps.Of(typeof(ж<int>)));
        Assert.AreSame(GoTypeOps<@string>.Instance, GoTypeOps.Of(typeof(@string)));
        Assert.AreSame(GoTypeOps<error>.Instance, GoTypeOps.Of(typeof(error)));

        // A builtin has no face of its own: the table, not a source, answers it.
        Assert.IsNull(GoTypeOps.FromSource(typeof(int)));
    }

    [TestMethod]
    public void OnlyWhatNothingNamesFallsBack()
    {
        // An interface, and a pointer (ж<T> carries no face: a reference type's operations are the fallback's, which Native AOT
        // loads -- measured -- and a box's own face would name ж<ж<T>>, without bound).
        Assert.IsNull(GoTypeOps.FromSource(typeof(ISomeInterface)));
        Assert.IsNull(GoTypeOps.FromSource(typeof(ж<ж<time.Time>>)));

        // Under the JIT the fallback answers them all the same.
        Assert.AreSame(GoTypeOps<ISomeInterface>.Instance, GoTypeOps.Of(typeof(ISomeInterface)));
        Assert.AreSame(GoTypeOps<ж<ж<time.Time>>>.Instance, GoTypeOps.Of(typeof(ж<ж<time.Time>>)));
    }

    [TestMethod]
    public void TheOperationsBuildWhatTheGenericHelpersBuilt()
    {
        IGoTypeOps ops = GoTypeOps.Of(typeof(sort.IntSlice));

        ж<sort.IntSlice> box = (ж<sort.IntSlice>)ops.NewBox(null);
        Assert.IsFalse(box.IsNilPointer, "a fresh box");
        Assert.AreEqual((nint)0, box.Value.Length, "holding the zero value");

        ж<int> holding = (ж<int>)GoTypeOps.Of(typeof(int)).NewBox(7);
        Assert.AreEqual(7, holding.Value);

        Assert.AreSame(global::go.ж<sort.IntSlice>.NilBox, ops.NilBox(), "the canonical typed nil");
        Assert.AreSame(global::go.ж<sort.IntSlice>.NilBox, GoReflect.CanonicalNilPointer(typeof(ж<sort.IntSlice>)), "and reflect's route to it");

        object dimsNil = GoTypeOps.Of(typeof(array<int>)).NilBoxOfDims([4]);
        Assert.IsInstanceOfType(dimsNil, typeof(ж<array<int>>));
        Assert.IsTrue(((ж<array<int>>)dimsNil).IsNilPointer);

        object newBox = GoReflect.NewPointerBox(typeof(time.Timer), null);
        Assert.IsInstanceOfType(newBox, typeof(ж<time.Timer>), "reflect.New's box, through the struct's own face");
    }
}
