// modulesLock_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).


package main

import (
	"os"
	"strings"
	"testing"
)

// TestModulesLockRoundTripAndClash pins the lock file's shape and its refusal: entries survive a
// write/read round trip sorted and LF-terminated, a matching version passes, and a different version
// of a locked module is refused naming the module, both versions and the lock file.
func TestModulesLockRoundTripAndClash(t *testing.T) {
	root := t.TempDir()

	locked := map[string]moduleLockEntry{
		"example.test/b": {path: "example.test/b", version: "v2.0.0", sum: "", converter: "rev1"},
		"example.test/a": {path: "example.test/a", version: "v1.0.0", sum: "h1:abc=", converter: "rev1"},
	}

	if err := writeModulesLock(root, locked); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(modulesLockPath(root))

	if err != nil {
		t.Fatal(err)
	}

	if strings.Contains(string(data), "\r") {
		t.Errorf("lock is not LF-terminated")
	}

	body := strings.Join(nonCommentLines(string(data)), "\n")

	if want := "example.test/a v1.0.0 h1:abc= rev1\nexample.test/b v2.0.0 - rev1"; body != want {
		t.Errorf("lock body =\n%s\nwant\n%s", body, want)
	}

	read, err := readModulesLock(root)

	if err != nil {
		t.Fatal(err)
	}

	if len(read) != 2 || read["example.test/b"].sum != "" || read["example.test/a"].sum != "h1:abc=" {
		t.Errorf("round trip = %+v", read)
	}

	same := map[string]moduleLockEntry{"example.test/a": {path: "example.test/a", version: "v1.0.0"}}

	if err := checkModulesLock(root, read, same); err != nil {
		t.Errorf("the locked version was refused: %v", err)
	}

	other := map[string]moduleLockEntry{"example.test/a": {path: "example.test/a", version: "v1.1.0"}}
	err = checkModulesLock(root, read, other)

	if err == nil {
		t.Fatalf("a different version of a locked module was not refused")
	}

	for _, want := range []string{"example.test/a", "locked at v1.0.0", "needs v1.1.0", modulesLockFileName} {
		if !strings.Contains(err.Error(), want) {
			t.Errorf("refusal does not name %q:\n%v", want, err)
		}
	}
}
