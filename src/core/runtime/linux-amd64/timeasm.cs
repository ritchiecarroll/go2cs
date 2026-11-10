// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Declarations for operating systems implementing time.now directly in assembly.
//go:build !faketime && (windows || (linux && amd64))
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class runtime_package {

//go:linkname time_now time.now
internal static partial (int64 sec, int32 nsec, int64 mono) time_now();

} // end runtime_package
