// assemblyIdentity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"testing"
)

// TestStdLibAssemblyNamesAreTheirPackageIds: a standard-library assembly is go.<dotted path>, its test assembly follows
// it, a converted module's test assembly keeps its dotted name, and a late-bound linkname pull names the standard
// library's assembly the same way and any other package's by its dotted path (OQ4).
func TestStdLibAssemblyNamesAreTheirPackageIds(t *testing.T) {
	if got := stdLibAssemblyName("crypto.tls"); got != "go.crypto.tls" {
		t.Errorf("stdLibAssemblyName(crypto.tls) = %q, want go.crypto.tls", got)
	}

	if got := stdLibAssemblyName("go.ast"); got != "go.go.ast" {
		t.Errorf("stdLibAssemblyName(go.ast) = %q, want go.go.ast (the package ID is go.go.ast)", got)
	}

	go2csPath := filepath.Join(t.TempDir(), "src")
	options := Options{go2csPath: go2csPath}
	coreTests := filepath.Join(go2csPath, "core", "fmt", "fmt.tests.csproj")
	moduleTests := filepath.Join(t.TempDir(), "out", "src", "example.com", "lib", "example.com.lib.tests.csproj")

	if got := testAssemblyName(coreTests, "fmt", options); got != "go.fmt.tests" {
		t.Errorf("a standard-library test assembly = %q, want go.fmt.tests", got)
	}

	if got := testAssemblyName(moduleTests, "example.com.lib", options); got != "example.com.lib.tests" {
		t.Errorf("a converted module's test assembly = %q, want example.com.lib.tests", got)
	}

	if got := linknameAssemblyName("runtime"); got != "go.runtime" {
		t.Errorf("linknameAssemblyName(runtime) = %q, want go.runtime", got)
	}

	if got := linknameAssemblyName("example.com/notstd"); got != "example.com.notstd" {
		t.Errorf("linknameAssemblyName(example.com/notstd) = %q, want example.com.notstd", got)
	}
}
