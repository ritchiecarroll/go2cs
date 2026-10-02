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
	"strings"
	"testing"

	"go2cs/internal/sourcemeta"
)

func fixtureSrc(t *testing.T) string {
	t.Helper()
	src := t.TempDir()
	dir := filepath.Join(src, "example.com", "mod")

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	for name, body := range map[string]string{
		"package_info.cs":        "[assembly: GoImplement<A, go.io_package.Reader>]\n",
		"example.com.mod.csproj": "<Project />",
	} {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	return src
}

// The pack script's two calls: generate the file, then verify it through the CONVERTER'S OWN parser, so a
// packed self-description is one the consumer is known to accept.
func TestGenerateThenVerify(t *testing.T) {
	out := filepath.Join(t.TempDir(), "source-metadata.txt")
	var stdout bytes.Buffer

	err := run([]string{
		"-src", fixtureSrc(t), "-module", "example.com/mod", "-module-version", "v1.6.0", "-go2cs-release", "1.24.13.3",
		"-package", "example.com/mod=example.com.mod", "-require", "example.com/dep@v0.2.0=nugetgo.example.com.dep", "-out", out,
	}, &stdout)

	if err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(out)

	if err != nil {
		t.Fatal(err)
	}

	d, err := sourcemeta.Parse(data)

	if err != nil || d.Module != "example.com/mod" || len(d.Requires) != 1 || d.Requires[0].NuGetID != "nugetgo.example.com.dep" {
		t.Fatalf("generated file parsed as %+v (err %v)", d, err)
	}

	stdout.Reset()

	if err := run([]string{"-verify", out}, &stdout); err != nil {
		t.Fatalf("-verify refused a file it generated: %v", err)
	}

	if !strings.Contains(stdout.String(), "example.com/mod v1.6.0") || !strings.Contains(stdout.String(), "1 package") {
		t.Errorf("-verify summary %q does not name the module, its version and its package count", stdout.String())
	}
}

func TestVerifyRefusesAMalformedFile(t *testing.T) {
	bad := filepath.Join(t.TempDir(), "source-metadata.txt")

	if err := os.WriteFile(bad, []byte("not a self-description\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	if err := run([]string{"-verify", bad}, &bytes.Buffer{}); err == nil || !strings.Contains(err.Error(), "line 1") {
		t.Errorf("-verify accepted a malformed file, or did not say where (err %v)", err)
	}
}

func TestMalformedArgumentsAreRefusedByName(t *testing.T) {
	base := []string{"-src", fixtureSrc(t), "-module", "example.com/mod", "-module-version", "v1.6.0", "-go2cs-release", "1.24.13.3", "-out", filepath.Join(t.TempDir(), "x.txt")}

	for _, c := range []struct {
		extra []string
		want  string
	}{
		{[]string{"-package", "example.com/mod"}, "-package"},
		{[]string{"-package", "example.com/mod=example.com.mod", "-require", "example.com/dep=nugetgo.x"}, "-require"},
		{[]string{"-package", "example.com/mod=example.com.mod", "-require", "example.com/dep@v1.0.0"}, "-require"},
	} {
		if err := run(append(append([]string{}, base...), c.extra...), &bytes.Buffer{}); err == nil || !strings.Contains(err.Error(), c.want) {
			t.Errorf("%v: not refused naming %s (err %v)", c.extra, c.want, err)
		}
	}
}
