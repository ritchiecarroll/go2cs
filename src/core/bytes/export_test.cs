// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.bytes_package;

partial class bytes_internal_test_package {

// Export func for testing
public static readonly ж<Func<slice<byte>, byte, nint>> ᏑIndexBytePortable = new StaticBox<Func<slice<byte>, byte, nint>>(indexBytePortable);
public static ref Func<slice<byte>, byte, nint> IndexBytePortable => ref ((StaticBox<Func<slice<byte>, byte, nint>>)ᏑIndexBytePortable).ValueSlot;

} // end bytes_internal_test_package
