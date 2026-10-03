// hostLaunchLoadTime_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// hostLaunchChildVariable marks a relaunch of this test binary as the stand-in for a converted test
// host: its LOAD-TIME code (the init below) reads a relative path from the directory it was started
// in, exactly as a converted test package's init() does when the host assembly loads -- before
// TestHost stages its sandbox and changes into it.
const hostLaunchChildVariable = "GO2CS_HOSTLAUNCH_CHILD"

func init() {
	if os.Getenv(hostLaunchChildVariable) != "1" {
		return
	}

	// github.com/golang-jwt/jwt/v5's hmac_example_test.go: os.ReadFile("test/hmacTestKey") at init.
	data, err := os.ReadFile(filepath.Join("keys", "k"))

	if err != nil {
		fmt.Printf("LOAD-TIME READ FAILED: %v", err)
		os.Exit(3)
	}

	fmt.Printf("LOAD-TIME READ: %s", data)
	os.Exit(0)
}

// The host is launched through the pipeline's own primitive and launch-directory rule, from a package
// source directory holding the fixture its load-time code reads (testdata/hostlaunch/fixture/keys/k)
// and an output directory that does not -- the shape the -tests pipeline hands both launch sites.
func TestHostLaunchServesALoadTimeRelativeRead(t *testing.T) {
	source, err := filepath.Abs(filepath.Join("testdata", "hostlaunch", "fixture"))

	if err != nil {
		t.Fatal(err)
	}

	output := t.TempDir()
	stdout, err := runCommandWithTimeoutEnv(2*time.Minute, testHostLaunchDir(source, output), Options{},
		[]string{hostLaunchChildVariable + "=1"}, os.Args[0], "-test.run=^$")

	if err != nil || !strings.Contains(stdout, "LOAD-TIME READ: secret") {
		t.Fatalf("a host's load-time relative read must see the package source directory: err=%v, output:\n%s", err, stdout)
	}
}
