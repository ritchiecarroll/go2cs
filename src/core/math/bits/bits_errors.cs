// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !compiler_bootstrap
namespace go.math;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class bits_package {

//go:linkname overflowError runtime.overflowError
internal static error overflowError { [global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields, "go.runtime_package", "runtime")] get => global::go.LinknameVar<error>.Get("go.runtime_package, runtime", "overflowError"); set => global::go.LinknameVar<error>.Set("go.runtime_package, runtime", "overflowError", value); }

//go:linkname divideError runtime.divideError
internal static error divideError { [global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields, "go.runtime_package", "runtime")] get => global::go.LinknameVar<error>.Get("go.runtime_package, runtime", "divideError"); set => global::go.LinknameVar<error>.Set("go.runtime_package, runtime", "divideError", value); }

} // end bits_package
