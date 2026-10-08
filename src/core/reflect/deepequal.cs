// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Deep equality test via reflection
namespace go;

using bytealg = @internal.bytealg_package;
using @unsafe = unsafe_package;
using @internal;

partial class reflect_package {

// During deepValueEqual, must keep track of checks that are
// in progress. The comparison algorithm assumes that all
// checks in progress are true when it reencounters them.
// Visited comparisons are stored in a map indexed by visit.
partial struct visit {
    internal @unsafe.Pointer a1;
    internal @unsafe.Pointer a2;
    internal ΔType typ;
}

// go2cs generated this placeholder — func deepValueEqual is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func DeepEqual is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end reflect_package
