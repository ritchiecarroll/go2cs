// cgoPseudoPackageStamp_test.go - Gbtc
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

	"github.com/ritchiecarroll/hashset"
)

// A cgo-on package's test host compiles package_test_info.cs and the internal test unit (package_info_internal_test.cs)
// into one assembly, and both were stamped with the C pseudo-package's [GoPackage] declaration: CS0579 on net's
// netꓸC_package at the first build after the bare-C fix (cgo Phase 3, increment 3.4, 19.6, probe 45513467921). While
// cgoPseudoPackageStampElsewhere is set, a file keeps its `using C` alias and drops the stamped declaration, including
// one it already holds; unset, the stamp is written as before.
func TestATestUnitCarriesNoSecondCgoPseudoPackageStamp(t *testing.T) {
	savedNames, savedAccess, savedName, savedNamespace := cgoPseudoPackageNames, cgoPseudoPackageTypeAccess, packageName, packageNamespace
	t.Cleanup(func() {
		cgoPseudoPackageNames, cgoPseudoPackageTypeAccess, packageName, packageNamespace = savedNames, savedAccess, savedName, savedNamespace
		cgoPseudoPackageStampElsewhere = false
	})

	cgoPseudoPackageNames = map[string]string{"_Ctype_uint": "@uint"}
	cgoPseudoPackageTypeAccess = hashset.HashSet[string]{}
	packageName, packageNamespace = "cgot5", "go"

	className := getSanitizedImport(cgoPseudoPackageClass(packageName))
	declaration := "partial class " + className
	usingStatic := "using static go." + getSanitizedImport(packageName+PackageSuffix) + ";"
	lines := []string{"namespace go;", "", usingStatic, "", "partial class cgot5_package", "{", "}"}

	stamped := applyCgoPseudoPackageStamp(lines, false)
	text := strings.Join(stamped, "\n")

	if strings.Count(text, declaration) != 1 || strings.Count(text, "using C = ") != 1 {
		t.Fatalf("the control: the package's own metadata file carries the stamp and the alias once each, got:\n%s", text)
	}

	cgoPseudoPackageStampElsewhere = true

	for name, input := range map[string][]string{"fresh": lines, "already stamped": stamped} {
		unit := strings.Join(applyCgoPseudoPackageStamp(input, true), "\n")

		if strings.Contains(unit, declaration) {
			t.Errorf("%s: the test unit carries the C pseudo-package's declaration, which package_test_info.cs already stamps:\n%s", name, unit)
		}

		if strings.Count(unit, "using C = ") != 1 {
			t.Errorf("%s: the test unit lost its `using C` alias:\n%s", name, unit)
		}
	}
}
