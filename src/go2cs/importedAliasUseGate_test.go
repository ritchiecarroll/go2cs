// importedAliasUseGate_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// An imported alias PARSED from a dependency's package_info.cs declares its `global using` only once an
// emitted reference has resolved through it, as a derived one does (seat #41). The parsed set is the
// dependency's set FOR THE TARGET GOOS, so emitting it whole made package_info.cs carry every alias the
// target's flavor of the dependency publishes: FieldAccessReinterpret's committed (windows) file declared
// syscallꓸHandle and syscallꓸSockaddr, which the program never names, and the same file built for linux
// failed CS0426 (no ΔHandle in linux's syscall) and CS0305 (linux's ΔSockaddr is generic). Gated on use,
// the emission depends only on what the Go source references, which compiles on every target.
func TestParsedImportedAliasDeclaresOnlyWhenReferenced(t *testing.T) {
	isolateConverterPathGlobals(t)
	fileName := seedAliasCollisionTestInfo(t, t.TempDir())

	resetPackageState(&packages.Package{})
	packageName = "ecdh"
	packageNamespace = "go.crypto"

	// The shape of syscall on windows: a type the consumer references, and two Δ-renamed ones it
	// never names.
	applyExportedTypeAliases([][2]string{
		{"RawSockaddrInet4", "RawSockaddrInet4"},
		{"Handle", ShadowVarMarker + "Handle"},
		{"Sockaddr", ShadowVarMarker + "Sockaddr"},
	}, PackageInfo{PackageName: "syscall", RootPackageName: "syscall", SourceDir: "syscall"})

	if got := getAliasedTypeName("syscall.RawSockaddrInet4"); got != "syscall"+TypeAliasDot+"RawSockaddrInet4" {
		t.Fatalf("a referenced foreign type must render through its alias, got %q", got)
	}

	writePackageInfoFile(fileName, false)

	declarations := strings.Join(emittedImportedAliases(t, fileName), "\n")

	if !strings.Contains(declarations, "syscall"+TypeAliasDot+"RawSockaddrInet4 =") {
		t.Errorf("the referenced alias must be declared:\n%s", declarations)
	}

	for _, unused := range []string{"Handle", "Sockaddr"} {
		if strings.Contains(declarations, "syscall"+TypeAliasDot+unused+" =") {
			t.Errorf("an alias the package never references must not be declared (syscall.%s):\n%s", unused, declarations)
		}
	}
}
