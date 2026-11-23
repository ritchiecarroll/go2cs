// oracleVetOffRefusal_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// oracleVetOffRefusal_integration_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// -test-oracle-vet-off, end to end (COORD, 2026-11-22): the option drops vet from the oracle only for a package
// whose own `go test` fails at vet. Run over a package that vet passes, the command line refuses it by name and
// runs no oracle, so the package's comparison never reports a verdict. The refusal comes before the converted
// project is built, so the arm costs a conversion, not a build. The unit arms (oracleTestArgs_test.go) cover
// the argument and the disclosure; this arm covers the refusal through the real driver.
func TestTestsRecurseRefusesOracleVetOffForAPackageVetPasses(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds the converter and converts a module with its tests")
	}

	if _, err := exec.LookPath("dotnet"); err != nil {
		t.Skipf("-test-action all needs the .NET SDK on PATH: %v", err)
	}

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "vetclean")
	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.test/vetclean\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(moduleDir, "vetclean.go"), "package vetclean\n\nfunc One() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(moduleDir, "vetclean_test.go"),
		"package vetclean\n\nimport \"testing\"\n\nfunc TestOne(t *testing.T) {\n\tif One() != 1 {\n\t\tt.Fatal(\"one\")\n\t}\n}\n")

	cmd := exec.Command(driverBinary(t), "-tests", "-test-action", "all", "-test-oracle-vet-off", "-recurse",
		"-go2cspath", treeSrc, moduleDir, filepath.Join(root, "out"))
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")
	raw, err := cmd.CombinedOutput()
	output := string(raw)

	if err == nil {
		t.Errorf("exit 0; want nonzero, since -test-oracle-vet-off is refused for a package vet passes\n%s", output)
	}

	if !strings.Contains(output, "-test-oracle-vet-off: go test's vet checks pass for example.test/vetclean") {
		t.Errorf("the refusal does not name the option and the package\n%s", output)
	}

	if strings.Contains(output, "The oracle runs without vet") || strings.Contains(output, "Validated 1 tests against go test") {
		t.Errorf("the oracle ran without vet for a package vet passes\n%s", output)
	}
}
