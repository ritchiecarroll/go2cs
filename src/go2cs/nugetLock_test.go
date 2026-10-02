// nugetLock_test.go - Gbtc
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

func TestNuGetLockRoundTrips(t *testing.T) {
	outRoot := t.TempDir()
	entries := map[string]nugetLockEntry{
		"github.com/b/b": {module: "github.com/b/b", version: "v2.0.0", nugetID: "b", status: "community", layer: "/path with space/mine.txt", packageVersion: "-", contentHash: "-"},
		"github.com/a/a": {module: "github.com/a/a", version: "v1.0.0", nugetID: "a", status: "canonical", layer: "https://nugetgo.net/v1/mappings.txt", packageVersion: "-", contentHash: "-"},
	}

	if err := writeNuGetLock(outRoot, entries); err != nil {
		t.Fatal(err)
	}

	read, err := readNuGetLock(outRoot)

	if err != nil {
		t.Fatal(err)
	}

	if len(read) != 2 || read["github.com/a/a"] != entries["github.com/a/a"] || read["github.com/b/b"] != entries["github.com/b/b"] {
		t.Errorf("round trip %+v; want %+v", read, entries)
	}

	data, _ := os.ReadFile(nugetLockPath(outRoot))

	if strings.Index(string(data), "github.com/a/a") > strings.Index(string(data), "github.com/b/b") {
		t.Errorf("lock rows are not sorted by module:\n%s", data)
	}
}

func TestNuGetLockMissingReadsAsEmpty(t *testing.T) {
	read, err := readNuGetLock(t.TempDir())

	if err != nil || len(read) != 0 {
		t.Errorf("a missing lock read as %+v (err %v); want empty", read, err)
	}
}

func TestNuGetLockRefusesAMalformedRowByLine(t *testing.T) {
	outRoot := t.TempDir()
	body := "# go2cs.nuget.lock\n" + strings.Join([]string{"github.com/a/a", "v1.0.0", "a", "canonical", "registry", "-"}, "\t") + "\n"

	if err := os.WriteFile(nugetLockPath(outRoot), []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	_, err := readNuGetLock(outRoot)

	if err == nil || !strings.Contains(err.Error(), nugetLockFileName+":2") || !strings.Contains(err.Error(), "7") {
		t.Errorf("a six-field lock row was accepted, or the refusal does not name the file, line and 7 fields (err %v)", err)
	}
}
