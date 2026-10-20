// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.unicode;

using static go.unicode.utf16_package;

partial class utf16_internal_test_package {

// Extra names for constants so we can validate them during testing.
public static UntypedInt Surr1 => /* surr1 */ 55296;
public static UntypedInt Surr3 => /* surr3 */ 57344;
public static UntypedInt SurrSelf => /* surrSelf */ 65536;
public static UntypedInt MaxRune => /* maxRune */ 1114111;
public static UntypedInt ReplacementChar => /* replacementChar */ 65533;

} // end utf16_internal_test_package
