// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export guts for testing.
namespace go;

using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using static global::go.runtime_package;

partial class runtime_internal_test_package {

public static UntypedInt MaxArgs => /* maxArgs */ 42;

public static Action OsYield;
internal static void initᴛOsYield() { OsYield = osyield; }
public static ж<uint32> TimeBeginPeriodRetValue;
internal static void initᴛTimeBeginPeriodRetValue() { TimeBeginPeriodRetValue = ᏑtimeBeginPeriodRetValue; }

// go2cs generated this placeholder — func NumberOfProcessors is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public partial struct ContextStub {
    internal partial ref global::go.runtime_package.context context { get; }
}

public static uintptr GetPC(this ContextStub c) {
    return c.context.ip();
}

// go2cs generated this placeholder — func NewContextStub is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end runtime_internal_test_package
