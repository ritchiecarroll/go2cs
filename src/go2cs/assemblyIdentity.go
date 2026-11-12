// assemblyIdentity.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"strings"
)

// A converted standard-library LIBRARY assembly is named after its NuGet package ID: package go.fmt ships go.fmt.dll,
// not fmt.dll, so no converted assembly takes a simple name like net.dll or log.dll into a consumer's bin folder
// (OQ4, docs/phase4/DESIGN-assembly-identity.md). The package ID itself never changes; only the assembly comes to
// carry it. A converted module's library keeps its full dotted import path, and an executable keeps the last
// import-path segment, as `go build` names it.

// stdLibAssemblyName is the assembly name of the standard-library package whose dotted import path is dotted.
func stdLibAssemblyName(dotted string) string {
	return "go." + dotted
}

// testAssemblyName is a test project's assembly name: the production assembly's, plus ".tests". It must follow the
// production name, because the production project grants InternalsVisibleTo $(AssemblyName).tests.
func testAssemblyName(projectFile, projectName string, options Options) string {
	if isStdLibProjectEmission(projectFile, options) {
		return stdLibAssemblyName(projectName) + ".tests"
	}

	return projectName + ".tests"
}

// linknameAssemblyName is the assembly a late-bound //go:linkname pull names for pkgPath, in its type name and its
// DynamicDependency: the standard library's go.<path> for a GOROOT package (decided as importOperations decides
// IsStdLib), else the dotted import path.
func linknameAssemblyName(pkgPath string) string {
	dotted := strings.ReplaceAll(pkgPath, "/", ".")

	if pkg, err := build.Default.Import(pkgPath, "", build.FindOnly); err == nil && pkg.Goroot {
		return stdLibAssemblyName(dotted)
	}

	return dotted
}
