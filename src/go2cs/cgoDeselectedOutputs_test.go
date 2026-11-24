// cgoDeselectedOutputs_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The first build of net's cgo-on test host (19.6): the seeded root holds the corpus's linux/cgo_stub.cs, the cgo-on
// selection swaps cgo_stub.go for cgo_unix.go, and both compile. The cure removes the stale file wherever the layout put
// it, keeps every file the cgo-on load selected, skips a file that is not there, and refuses a hand-owned one by name
// with nothing removed.
func TestACgoOnConversionRemovesTheCgoOffFileItDeselects(t *testing.T) {
	write := func(t *testing.T, path, body string) {
		t.Helper()

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	exists := func(path string) bool {
		_, err := os.Stat(path)
		return err == nil
	}

	cgoOff := []string{"/goroot/src/net/cgo_stub.go", "/goroot/src/net/dial.go", "/goroot/src/net/netgo_off.go"}
	cgoOn := []string{"/goroot/src/net/cgo_unix.go", "/goroot/src/net/dial.go", "/goroot/src/net/netgo_off.go", "/goroot/src/net/cgo_unix_cgo.go"}

	t.Run("in a key folder", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, "// converted\n")
		write(t, filepath.Join(out, "linux", "dial.cs"), "// converted\n")
		write(t, filepath.Join(out, "dial.cs"), "// converted\n")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn)

		if err != nil || len(removed) != 1 || removed[0] != stub || exists(stub) {
			t.Fatalf("want exactly %s removed, got %v (err %v, still there %v)", stub, removed, err, exists(stub))
		}

		if !exists(filepath.Join(out, "dial.cs")) || !exists(filepath.Join(out, "linux", "dial.cs")) {
			t.Error("a file the cgo-on load selected was removed")
		}
	})

	t.Run("flat", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "cgo_stub.cs")
		write(t, stub, "// converted\n")

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn); err != nil || len(removed) != 1 || exists(stub) {
			t.Fatalf("want the flat stub removed, got %v (err %v)", removed, err)
		}
	})

	t.Run("nothing to remove", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn); err != nil || len(removed) != 0 {
			t.Fatalf("an empty output removes nothing, got %v (err %v)", removed, err)
		}
	})

	t.Run("hand-owned is refused", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, "using go;\n\n[module: GoManualConversion]\n\nnamespace go;\n")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn)

		if err == nil || !strings.Contains(err.Error(), "hand-owned") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the hand-own and the file kept, got %v (err %v, kept %v)", removed, err, exists(stub))
		}
	})
}
