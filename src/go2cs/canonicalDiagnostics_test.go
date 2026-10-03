// canonicalDiagnostics_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The converter's own failures on its INPUT -- a package that does not fully type-check, an `import
// "C"` source it refuses -- print in MSBuild's canonical form, `file(line,col): error GO2CSnnnn: text`,
// so CI logs and editor problem matchers pick them up. The human-readable summary line each site
// printed before stays: the behavioral runner and check-no-regression key on its wording.

package main

import (
	"bytes"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

func TestCanonicalDiagnosticFormat(t *testing.T) {
	cases := []struct {
		position, code, text, want string
	}{
		{"/src/te/te.go:3:16", "GO2CS1003", "cannot use 1", "/src/te/te.go(3,16): error GO2CS1003: cannot use 1"},
		{"/src/te/te.go:3", "GO2CS1002", "expected ';'", "/src/te/te.go(3): error GO2CS1002: expected ';'"},
		{`C:\src\te\te.go:3:16`, "GO2CS1003", "cannot use 1", `C:\src\te\te.go(3,16): error GO2CS1003: cannot use 1`},
		{"/src/te/te.go", "GO2CS1001", "no package", "/src/te/te.go: error GO2CS1001: no package"},
		{"", "GO2CS1001", "no required module provides package x", "go2cs: error GO2CS1001: no required module provides package x"},
		{"-", "GO2CS1000", "load failed", "go2cs: error GO2CS1000: load failed"},
		{"/src/te/te.go:3:16", "GO2CS1001", "first line\n\tsecond line\r\n", "/src/te/te.go(3,16): error GO2CS1001: first line second line"},
	}

	for _, c := range cases {
		if got := canonicalDiagnostic(c.position, c.code, c.text); got != c.want {
			t.Errorf("canonicalDiagnostic(%q, %q, %q)\n  got  %q\n  want %q", c.position, c.code, c.text, got, c.want)
		}
	}
}

func TestLoadErrorCodes(t *testing.T) {
	for kind, want := range map[packages.ErrorKind]string{
		packages.UnknownError: "GO2CS1000",
		packages.ListError:    "GO2CS1001",
		packages.ParseError:   "GO2CS1002",
		packages.TypeError:    "GO2CS1003",
	} {
		if got := loadErrorCode(kind); got != want {
			t.Errorf("loadErrorCode(%v) = %q, want %q", kind, got, want)
		}
	}
}

// runConverterOnFixture converts the one package in dir through the real command line and returns its
// stderr and exit error.
func runConverterOnFixture(t *testing.T, dir string, env ...string) (string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	var stderr bytes.Buffer
	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, dir, t.TempDir())
	cmd.Env = append(append(os.Environ(), "GOWORK=off", "GOFLAGS="), env...)
	cmd.Stdout, cmd.Stderr = &bytes.Buffer{}, &stderr
	err = cmd.Run()

	return stderr.String(), err
}

// requireStderrLine fails unless stderr holds a line that is exactly prefix-in-dir + suffix, where the
// directory may be spelled as given or with its symbolic links resolved (a temp root behind a link).
func requireStderrLine(t *testing.T, stderr, dir, file, suffix string) {
	t.Helper()

	wants := []string{filepath.Join(dir, file) + suffix}

	if resolved, err := filepath.EvalSymlinks(dir); err == nil && resolved != dir {
		wants = append(wants, filepath.Join(resolved, file)+suffix)
	}

	for _, line := range strings.Split(stderr, "\n") {
		for _, want := range wants {
			if strings.TrimRight(line, "\r") == want {
				return
			}
		}
	}

	t.Errorf("stderr has no line %q\nstderr:\n%s", wants[0], stderr)
}

func TestTypeCheckFailurePrintsCanonicalDiagnostics(t *testing.T) {
	dir := t.TempDir()

	writeCgoFixture(t, dir, "go.mod", "module tefixture\n\ngo 1.24\n")
	writeCgoFixture(t, dir, "te.go", "package te\n\nvar S string = 1\nvar N int = \"n\"\n\nfunc F() int {\n\treturn 2\n}\n")

	stderr, err := runConverterOnFixture(t, dir, "CGO_ENABLED=0")

	if err != nil {
		t.Fatalf("a package that does not fully type-check still converts best-effort, got %v\nstderr:\n%s", err, stderr)
	}

	if !strings.Contains(stderr, "WARNING: tefixture did not fully type-check") {
		t.Errorf("the human-readable summary line is gone\nstderr:\n%s", stderr)
	}

	requireStderrLine(t, stderr, dir, "te.go", "(3,16): error GO2CS1003: cannot use 1 (untyped int constant) as string value in variable declaration")
	requireStderrLine(t, stderr, dir, "te.go", `(4,13): error GO2CS1003: cannot use "n" (untyped string constant) as int value in variable declaration`)
}

func TestSelectedCgoRefusalPrintsCanonicalDiagnostic(t *testing.T) {
	if _, err := exec.LookPath("gcc"); err != nil {
		if _, err := exec.LookPath("cc"); err != nil {
			t.Skip("no C compiler: a real cgo load is not possible here")
		}
	}

	dir := t.TempDir()

	writeCgoFixture(t, dir, "go.mod", "module cgofixture\n\ngo 1.24\n")
	writeCgoFixture(t, dir, "p.go", "package p\n")
	writeCgoFixture(t, dir, "cgo_here.go", cgoImportSource)

	stderr, err := runConverterOnFixture(t, dir, "CGO_ENABLED=1")

	if err == nil {
		t.Fatalf("a selected cgo source must be refused with a non-zero exit\nstderr:\n%s", stderr)
	}

	if !strings.Contains(stderr, "Refusing to convert: package cgofixture selects cgo source cgo_here.go") {
		t.Errorf("the human-readable refusal line is gone\nstderr:\n%s", stderr)
	}

	requireStderrLine(t, stderr, dir, "cgo_here.go", "(6,8): error GO2CS2001: package cgofixture selects this cgo source (import \"C\") for this build, and cgo has no C# conversion yet; convert with CGO_ENABLED=0 or exclude the file from the build")
}
