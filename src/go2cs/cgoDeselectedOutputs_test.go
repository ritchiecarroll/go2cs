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

	"golang.org/x/tools/go/packages"
)

const cgoStubSourceHeader = "// Copyright 2011 The Go Authors. All rights reserved.\n// Use of this source code is governed by a BSD-style\n// license that can be found in the LICENSE file.\n\n// This file holds stub versions of the cgo functions.\n\n//go:build unix && !cgo\n\n"

// The first build of net's cgo-on test host (19.6): the seeded root holds the corpus's linux/cgo_stub.cs, the cgo-on
// selection swaps cgo_stub.go for cgo_unix.go, and both compile. Within COORD's bounds (inbox H 20261124T231602Z), the
// cure removes the stale file wherever the layout put it, only when it carries go2cs's emission header for that source,
// keeps every file the cgo-on load selected, skips a file that is not there, refuses a hand-owned file and a file
// without the header by name with nothing removed, and in a dry run lists the set without removing it.
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

	// The emission: the source's leading comments as the converter copies them (blank lines dropped, CRLF).
	emitted := strings.ReplaceAll(strings.ReplaceAll(cgoStubSourceHeader, "\n\n", "\n"), "\n", "\r\n") + "namespace go;\r\n"

	source := t.TempDir()
	stubGo := filepath.Join(source, "cgo_stub.go")
	write(t, stubGo, cgoStubSourceHeader+"package net\n")
	write(t, filepath.Join(source, "dial.go"), "package net\n")

	cgoOff := []string{stubGo, filepath.Join(source, "dial.go")}
	cgoOn := []string{filepath.Join(source, "cgo_unix.go"), filepath.Join(source, "dial.go"), filepath.Join(source, "cgo_unix_cgo.go")}

	t.Run("in a key folder", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, emitted)
		write(t, filepath.Join(out, "linux", "dial.cs"), "namespace go;\n")
		write(t, filepath.Join(out, "dial.cs"), "namespace go;\n")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

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
		write(t, stub, emitted)

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false); err != nil || len(removed) != 1 || exists(stub) {
			t.Fatalf("want the flat stub removed, got %v (err %v)", removed, err)
		}
	})

	t.Run("dry run lists and keeps", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, emitted)

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, true); err != nil || len(removed) != 1 || !exists(stub) {
			t.Fatalf("a dry run lists the stub and keeps it, got %v (err %v, kept %v)", removed, err, exists(stub))
		}
	})

	t.Run("nothing to remove", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false); err != nil || len(removed) != 0 {
			t.Fatalf("an empty output removes nothing, got %v (err %v)", removed, err)
		}
	})

	t.Run("hand-owned is refused", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, strings.TrimSuffix(emitted, "namespace go;\r\n")+"using go;\r\n\r\n[module: GoManualConversion]\r\n\r\nnamespace go;\r\n")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err == nil || !strings.Contains(err.Error(), "hand-owned") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the hand-own and the file kept, got %v (err %v, kept %v)", removed, err, exists(stub))
		}
	})

	t.Run("only this target's folders: darwin's copy stays", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		linux := filepath.Join(out, "linux", "cgo_stub.cs")
		darwin := filepath.Join(out, "darwin", "cgo_stub.cs")
		write(t, linux, emitted)
		write(t, darwin, emitted)

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err != nil || len(removed) != 1 || exists(linux) || !exists(darwin) {
			t.Fatalf("want linux's copy removed and darwin's kept, got %v (err %v, linux there %v, darwin there %v)", removed, err, exists(linux), exists(darwin))
		}
	})

	t.Run("every folder the target resolves to: key folder and flat", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		key := filepath.Join(out, "linux", "cgo_stub.cs")
		flat := filepath.Join(out, "cgo_stub.cs")
		write(t, key, emitted)
		write(t, flat, emitted)

		if removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false); err != nil || len(removed) != 2 || exists(key) || exists(flat) {
			t.Fatalf("want both copies removed, got %v (err %v)", removed, err)
		}
	})

	t.Run("an _impl.cs companion is refused", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, emitted)
		write(t, filepath.Join(out, "linux", "cgo_stub_impl.cs"), "// hand-owned")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err == nil || !strings.Contains(err.Error(), "companion") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the companion and the file kept, got %v (err %v)", removed, err)
		}
	})

	t.Run("A3: a companion in another folder of the target's set is refused", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, emitted)
		write(t, filepath.Join(out, "cgo_stub_impl.cs"), "// hand-owned")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err == nil || !strings.Contains(err.Error(), "companion") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the flat companion and the file kept, got %v (err %v)", removed, err)
		}
	})

	t.Run("a file in a repository's corpus is refused", func(t *testing.T) {
		repository := t.TempDir()

		if err := os.WriteFile(filepath.Join(repository, ".git"), []byte("gitdir: elsewhere"), 0o644); err != nil {
			t.Fatal(err)
		}

		out := filepath.Join(repository, "src", "core", "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, emitted)

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err == nil || !strings.Contains(err.Error(), "repository's corpus") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the corpus and the file kept, got %v (err %v)", removed, err)
		}
	})

	t.Run("a file without the emission header is refused", func(t *testing.T) {
		out := filepath.Join(t.TempDir(), "net")
		stub := filepath.Join(out, "linux", "cgo_stub.cs")
		write(t, stub, "// Someone else's file of the same name.\r\nnamespace go;\r\n")

		removed, err := cgoDeselectedOutputs(out, "linux/amd64", cgoOff, cgoOn, false)

		if err == nil || !strings.Contains(err.Error(), "emission header") || len(removed) != 0 || !exists(stub) {
			t.Fatalf("want a refusal naming the header and the file kept, got %v (err %v, kept %v)", removed, err, exists(stub))
		}
	})
}

// COORD's first bound: the removal runs only on the -cgo -tests path. A plain -cgo conversion, a -tests conversion
// without -cgo and the offline seam return before any load (here, a load of a directory with no Go files would fail).
func TestTheCgoRemovalRunsOnlyOnTheCgoTestsPath(t *testing.T) {
	empty := t.TempDir()
	pkg := &packages.Package{PkgPath: "example.com/none", GoFiles: []string{filepath.Join(empty, "none.go")}}

	for _, options := range []Options{
		{parseCgoTargets: true, targetPlatform: "linux/amd64"},
		{convertTests: true, targetPlatform: "linux/amd64"},
	} {
		if err := removeCgoDeselectedOutputs(pkg, filepath.Join(empty, "out"), options); err != nil {
			t.Errorf("cgo %v, tests %v: the removal ran (%v); it belongs to -cgo -tests only", options.parseCgoTargets, options.convertTests, err)
		}
	}

	if err := removeCgoDeselectedOutputs(pkg, filepath.Join(empty, "out"), Options{parseCgoTargets: true, convertTests: true, targetPlatform: "linux/amd64"}); err == nil {
		t.Error("the control: on the -cgo -tests path the removal loads the directory, and a directory with no Go files fails that load")
	}
}
