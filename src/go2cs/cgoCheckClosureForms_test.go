// cgoCheckClosureForms_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"encoding/json"
	"errors"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"testing"
)

// cgoCheckClosureForms are the forms of cmd/cgo's pointer-check closure (cmd/cgo gcc.go, rewriteCall and the check*
// helpers it calls), each matched by the _cgoCheckPointer call or the statement that opens it.
var cgoCheckClosureForms = []struct {
	name    string
	pattern *regexp.Regexp
}{
	// &x: Go checks the pointee's members only, as Phase 2's wrapper does.
	{"address", regexp.MustCompile(`_cgoCheckPointer\(_cgoBase\d+, 0 == 0\)`)},
	// A pointer in a variable: Go scans the whole heap object and refuses any pointer into data or bss.
	{"variable", regexp.MustCompile(`_cgoCheckPointer\(_cgo\d+, nil\)`)},
	// &a[i] and a[:]: element and slice memory, Phase 3's.
	{"index", regexp.MustCompile(`_cgoCheckPointer\(_cgo\d+, \*?_cgoIndex\d+\)`)},
	{"slice", regexp.MustCompile(`_cgoCheckPointer\(_cgoSlice\d+, 0 == 0\)`)},
	// The statements a closure can stand in: deferred, and started in a goroutine.
	{"defer", regexp.MustCompile(`\bdefer func\(\) func\(\)`)},
	{"go", regexp.MustCompile(`\bgo func\(\)[^{\n]*\{ (?:var )?_cgo`)},
}

// cgo Phase 2 increment 5 (COORD's review of its design, M1): the address form of cmd/cgo's check closure checks what
// Phase 2's wrapper checks, and the variable form checks more, so whether Phase 2 refuses the variable form depends
// on how often real cgo code reaches it. This counts each form in the generated trees of every cgo package in GOROOT,
// on a host with a C compiler. It is a reading: it fails only when it cannot read.
func TestCgoCheckClosureFormsAcrossGorootsCgoPackages(t *testing.T) {
	requireCCompiler(t)

	env := append(os.Environ(), "CGO_ENABLED=1", "GOWORK=off", "GOFLAGS=")
	enumerate := exec.Command("go", "list", "-e", "-f", "{{if .CgoFiles}}{{.ImportPath}}{{end}}", "std", "cmd")
	enumerate.Env = env
	listed, err := enumerate.Output()

	if err != nil {
		t.Fatalf("go list std cmd: %v", err)
	}

	packages := strings.Fields(string(listed))

	if len(packages) == 0 {
		t.Fatal("no cgo package in GOROOT")
	}

	compile := exec.Command("go", append([]string{"list", "-e", "-compiled", "-json"}, packages...)...)
	compile.Env = env
	var stderr bytes.Buffer
	compile.Stderr = &stderr
	output, err := compile.Output()

	if err != nil {
		t.Fatalf("go list -compiled: %v\n%s", err, stderr.String())
	}

	totals := map[string]int{}
	decoder := json.NewDecoder(bytes.NewReader(output))

	for {
		var pkg struct {
			ImportPath      string
			Dir             string
			CompiledGoFiles []string
			Error           *struct{ Err string }
		}

		if err := decoder.Decode(&pkg); errors.Is(err, io.EOF) {
			break
		} else if err != nil {
			t.Fatal(err)
		}

		if pkg.Error != nil {
			t.Logf("READING %s: not compiled (%s)", pkg.ImportPath, strings.SplitN(pkg.Error.Err, "\n", 2)[0])
			continue
		}

		counts := map[string]int{}

		for _, path := range pkg.CompiledGoFiles {
			if !filepath.IsAbs(path) {
				path = filepath.Join(pkg.Dir, path)
			}

			content, err := os.ReadFile(path)

			if err != nil {
				t.Fatal(err)
			}

			for _, form := range cgoCheckClosureForms {
				counts[form.name] += len(form.pattern.FindAllIndex(content, -1))
			}
		}

		var read []string

		for _, form := range cgoCheckClosureForms {
			totals[form.name] += counts[form.name]

			if counts[form.name] > 0 {
				read = append(read, form.name+" "+strconv.Itoa(counts[form.name]))
			}
		}

		if len(read) > 0 {
			t.Logf("READING %s: %s", pkg.ImportPath, strings.Join(read, ", "))
		}
	}

	names := make([]string, 0, len(totals))

	for _, form := range cgoCheckClosureForms {
		names = append(names, form.name+" "+strconv.Itoa(totals[form.name]))
	}

	t.Logf("READING totals over %d cgo packages: %s", len(packages), strings.Join(names, ", "))
}
