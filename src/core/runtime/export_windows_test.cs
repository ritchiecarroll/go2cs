// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export guts for testing.
namespace go;

using windows = @internal.runtime.syscall.windows_package;
using @unsafe = unsafe_package;
using @internal.runtime.syscall;
using static global::go.runtime_package;

partial class runtime_internal_test_package {

public static readonly ж<Action> ᏑOsYield = new StaticBox<Action>(default(Action));
public static ref Action OsYield => ref ((StaticBox<Action>)ᏑOsYield).ValueSlot;
internal static void initᴛOsYield() { OsYield = osyield; }
public static ж<uint32> TimeBeginPeriodRetValue;
internal static void initᴛTimeBeginPeriodRetValue() { TimeBeginPeriodRetValue = ᏑtimeBeginPeriodRetValue; }

// go2cs generated this placeholder — func NumberOfProcessors is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static uintptr GetCallerFp() {
    return getcallerfp();
}

} // end runtime_internal_test_package
