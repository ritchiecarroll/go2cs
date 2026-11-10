// MapAsMapTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// IMap<K, V>.AsMap (TS readiness C24): a map-cored type parameter assigned to its core type, `return dst` as
// `map[K]V`, emits `dst.AsMap()`. Go's two values name ONE hash table, so the result must SHARE the store: a write
// through either is visible through the other. A copy would read right and drop the write, which is the arm that
// pays for this file. The named-map wrapper's implementation is pinned by the TypeParamCoreAssign behavioral project.
[TestClass]
public class MapAsMapTests
{
    private static map<K, V> Core<M, K, V>(M m) where M : IMap<K, V> where K : notnull => m.AsMap();

    [TestMethod]
    public void AsMapThroughTheConstraintSharesTheStore()
    {
        map<@string, nint> m = new() { ["a"] = 1 };

        map<@string, nint> core = Core<map<@string, nint>, @string, nint>(m);
        core["b"] = 2;
        m["c"] = 3;

        Assert.AreEqual((nint)3, len(m), "a write through the result reaches the original");
        Assert.AreEqual((nint)3, len(core), "and a write through the original reaches the result");
        Assert.AreEqual((nint)2, m["b"]);
    }

    [TestMethod]
    public void AsMapThroughTheBoxedInterfaceSharesTheStore()
    {
        map<nint, nint> m = new() { [1] = 1 };
        IMap<nint, nint> boxed = m;

        boxed.AsMap()[2] = 2;

        Assert.AreEqual((nint)2, len(m));
    }

    [TestMethod]
    public void ANilMapStaysNil()
    {
        map<nint, nint> m = default;

        map<nint, nint> core = Core<map<nint, nint>, nint, nint>(m);

        Assert.IsTrue(core == nil, "Go: a nil M assigned to map[K]V is a nil map");
    }
}
