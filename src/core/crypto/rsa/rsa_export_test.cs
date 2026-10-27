// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using io = io_package;
using static go.crypto.rsa_package;

partial class rsa_internal_test_package {

public static readonly ж<Func<slice<byte>, io.Reader, error>> ᏑNonZeroRandomBytes = new StaticBox<Func<slice<byte>, io.Reader, error>>(nonZeroRandomBytes);
public static ref Func<slice<byte>, io.Reader, error> NonZeroRandomBytes => ref ((StaticBox<Func<slice<byte>, io.Reader, error>>)ᏑNonZeroRandomBytes).ValueSlot;

} // end rsa_internal_test_package
