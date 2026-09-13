// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

public static UntypedInt MapBucketCountBits => 3;
public static UntypedInt MapBucketCount => /* 1 << MapBucketCountBits */ 8;
public static UntypedInt MapMaxKeyBytes => 128;
public static UntypedInt MapMaxElemBytes => 128;

} // end main_package
