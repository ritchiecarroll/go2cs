// main_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"os"
	"path/filepath"
	"testing"
)

func writePackage(t *testing.T, files map[string]string) string {
	t.Helper()
	dir := t.TempDir()

	for name, body := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	return dir
}

func TestPackageSynopsis(t *testing.T) {
	cases := []struct {
		name  string
		files map[string]string
		want  string
	}{
		{
			name: "the first sentence of the package doc",
			files: map[string]string{
				"doc.go":  "// Package uuid generates and inspects UUIDs.\n//\n// UUIDs are based on RFC 9562.\npackage uuid\n",
				"uuid.go": "package uuid\n\ntype UUID [16]byte\n",
			},
			want: "Package uuid generates and inspects UUIDs.",
		},
		{
			name: "a license header is not the package doc",
			files: map[string]string{
				"hashset.go": "// Copyright 2026 The go2cs Authors.\n\n// Package hashset implements a generic set.\npackage hashset\n",
			},
			want: "Package hashset implements a generic set.",
		},
		{
			name: "a test file's doc is not the package's",
			files: map[string]string{
				"set.go":      "package set\n",
				"set_test.go": "// Package set_test tests the set.\npackage set_test\n",
			},
			want: "",
		},
		{
			name: "a file a build constraint excludes is not read",
			files: map[string]string{
				"jwt.go":    "// Package jwt is a Go implementation of JSON Web Tokens.\npackage jwt\n",
				"ignore.go": "//go:build ignore\n\n// Command gen writes tables.\npackage main\n",
			},
			want: "Package jwt is a Go implementation of JSON Web Tokens.",
		},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			got, err := packageSynopsis(writePackage(t, c.files))

			if err != nil {
				t.Fatalf("error: %v", err)
			}

			if got != c.want {
				t.Fatalf("synopsis %q, want %q", got, c.want)
			}
		})
	}
}

func TestNoGoPackageIsAnError(t *testing.T) {
	if _, err := packageSynopsis(writePackage(t, map[string]string{"README.md": "# not Go\n"})); err == nil {
		t.Fatal("a directory with no Go package read as a synopsis")
	}
}

func TestRunPrintsOneLine(t *testing.T) {
	var out bytes.Buffer

	if err := run([]string{"-dir", writePackage(t, map[string]string{"a.go": "// Package a is small.\npackage a\n"})}, &out); err != nil {
		t.Fatal(err)
	}

	if out.String() != "Package a is small.\n" {
		t.Fatalf("printed %q", out.String())
	}
}
