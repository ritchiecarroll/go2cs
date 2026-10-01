// moduleAncestryEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

var machinePathPattern = regexp.MustCompile(`"(?:[A-Za-z]:[\\/]|/)`)

// TestTestHostCarriesTheLogicalModulePathOnly is the converter half of the module ancestry (M4): a
// third-party host's registry names its MODULE path -- the logical half of the host's
// TryStageModule input -- and carries no machine path; a standard-library host (no module path)
// emits exactly as before.
func TestTestHostCarriesTheLogicalModulePathOnly(t *testing.T) {
	module := t.TempDir()

	if err := writeTestHost(module, "go", "example.test/keys/parse", nil, nil, []string{"parse_test.go"}, nil, nil, "example.test/keys", ""); err != nil {
		t.Fatal(err)
	}

	host := readGenerated(t, filepath.Join(module, "go2cs_test_host.cs"))

	if !strings.Contains(host, "}, new string[]\r\n        {\r\n        }, new string[]\r\n        {\r\n        }, \"example.test/keys\");") {
		t.Errorf("the registry does not end with the module path after the two (empty) positional lists:\n%s", host)
	}

	if match := machinePathPattern.FindString(host); match != "" {
		t.Errorf("the emitted host carries a machine path (%s...):\n%s", match, host)
	}

	stdlib := t.TempDir()

	if err := writeTestHost(stdlib, "go", "cmp", nil, nil, []string{"cmp.go"}, nil, nil, "", ""); err != nil {
		t.Fatal(err)
	}

	if host := readGenerated(t, filepath.Join(stdlib, "go2cs_test_host.cs")); !strings.Contains(host, "            \"cmp.go\",\r\n        });") {
		t.Errorf("a standard-library host no longer ends its registry right after the fixtures:\n%s", host)
	}
}

// TestTestHostModuleEnvironmentIsForModulePackagesOnly: the module root reaches a third-party host
// through the environment; a standard-library package (under $GOROOT/src/go.mod, `module std`) and a
// directory with no go.mod above it get nothing.
func TestTestHostModuleEnvironmentIsForModulePackagesOnly(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot}

	module := t.TempDir()
	writeModuleFile(t, filepath.Join(module, "go.mod"), "module example.test/keys\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(module, "parse", "parse.go"), "package parse\n")

	env := testHostModuleEnv(filepath.Join(module, "parse"), options)

	if len(env) != 1 || env[0] != "GO2CS_MODULE_ROOT="+module {
		t.Errorf("module package env = %v, want [GO2CS_MODULE_ROOT=%s]", env, module)
	}

	if env := testHostModuleEnv(filepath.Join(goRoot, "src", "os"), options); len(env) != 0 {
		t.Errorf("a standard-library package got %v", env)
	}

	bare := t.TempDir()

	if err := os.MkdirAll(filepath.Join(bare, "pkg"), 0o755); err != nil {
		t.Fatal(err)
	}

	if env := testHostModuleEnv(filepath.Join(bare, "pkg"), options); len(env) != 0 {
		t.Errorf("a directory with no go.mod above it got %v", env)
	}
}
