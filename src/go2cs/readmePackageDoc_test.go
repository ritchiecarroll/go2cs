// readmePackageDoc_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"testing"
)

// writeDocFixture writes a package directory shaped like os/exec/internal/fdtest: the package doc lives in a
// unix-only file, the windows file carries none, and a test file carries a doc of its own that must never count.
func writeDocFixture(t *testing.T) string {
	t.Helper()

	dir := t.TempDir()
	files := map[string]string{
		"exists_unix.go":    "//go:build unix || wasm\n\n// Package fdtest provides test helpers.\npackage fdtest\n",
		"exists_windows.go": "//go:build windows\n\npackage fdtest\n",
		"exists_plan9.go":   "//go:build plan9\n\n// Package fdtest is documented differently on plan9.\npackage fdtest\n",
		"exists_test.go":    "// Package fdtest_test is a test file's doc.\npackage fdtest\n",
		"gen.go":            "//go:build ignore\n\n// Command gen is not this package.\npackage main\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0o644); err != nil {
			t.Fatalf("write %s: %v", name, err)
		}
	}

	return dir
}

func parseDocFixture(t *testing.T, dir string, names ...string) []*ast.File {
	t.Helper()

	fset := token.NewFileSet()
	files := make([]*ast.File, 0, len(names))

	for _, name := range names {
		file, err := parser.ParseFile(fset, filepath.Join(dir, name), nil, parser.ParseComments)

		if err != nil {
			t.Fatalf("parse %s: %v", name, err)
		}

		files = append(files, file)
	}

	return files
}

// A package's README carries its package doc, and one README serves every platform: it is packed flat from the
// project directory. When the doc lives in a file the converting target excludes (os/exec/internal/fdtest's is in
// exists_unix.go), the windows emission had none and the merged README depended on which flavour the merge kept.
// The fallback is the doc pkg.go.dev shows -- the first of its build contexts whose files carry one -- so every
// target emits the same README.
func TestPackageDocFallsBackToTheDocPkgGoDevShows(t *testing.T) {
	dir := writeDocFixture(t)

	windowsSelected := parseDocFixture(t, dir, "exists_windows.go")

	if got := extractPackageDoc(windowsSelected); got != "" {
		t.Fatalf("fixture wrong: the windows selection should carry no doc of its own, got %q", got)
	}

	if got, want := packageDocFor(windowsSelected, dir, "fdtest"), "Package fdtest provides test helpers."; got != want {
		t.Errorf("windows: the README doc is %q, want pkg.go.dev's %q", got, want)
	}

	// A target whose own files carry the doc keeps it, so linux and darwin emit what they always did.
	unixSelected := parseDocFixture(t, dir, "exists_unix.go")

	if got, want := packageDocFor(unixSelected, dir, "fdtest"), "Package fdtest provides test helpers."; got != want {
		t.Errorf("linux: the README doc is %q, want its own %q", got, want)
	}
}

// The fallback reads only what pkg.go.dev would document: non-test files of this package, selected by one of its
// build contexts. A package no context documents stays undocumented.
func TestPackageDocFallbackIgnoresTestFilesAndOtherPackages(t *testing.T) {
	dir := t.TempDir()
	files := map[string]string{
		"a_windows.go": "//go:build windows\n\npackage quiet\n",
		"a_test.go":    "// Package quiet is documented only by its test.\npackage quiet\n",
		"gen.go":       "//go:build ignore\n\n// Command gen documents another package.\npackage main\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0o644); err != nil {
			t.Fatalf("write %s: %v", name, err)
		}
	}

	if got := packageDocFor(parseDocFixture(t, dir, "a_windows.go"), dir, "quiet"); got != "" {
		t.Errorf("a test file's or another package's doc became the README doc: %q", got)
	}
}
