// cgoSourceIdentity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

var goPositionMapIdentity = regexp.MustCompile(`GoPositionMap\("([^"]*)"`)

// cgoPackageIdentities reads the GoPositionMap identities of the package_info.cs files under output whose path contains part.
func cgoPackageIdentities(t *testing.T, output, part string) []string {
	t.Helper()

	var identities []string

	for path, text := range cgoRecurseWritten(t, output, part, ".cs") {
		if strings.HasSuffix(path, "/package_info.cs") {
			for _, match := range goPositionMapIdentity.FindAllStringSubmatch(text, -1) {
				identities = append(identities, match[1])
			}
		}
	}

	return identities
}

// A tree cmd/cgo rewrote names the import "C" source it came from, as Go reports that file's frames: never the hashed
// build-cache intermediate the converter parsed (cgo against go.* INC 4, increment 1c, COORD's ruling). Go's own
// answer, measured with runtime.Caller(0) in an import "C" file: `<module>/c.go` under -trimpath and the absolute
// source path without it. The identity's `<module>@/` and `<module>@<version>/` spellings are the records' forms of the
// same (goSourceIdentity). cmd/cgo's bindings file has no source and carries no record: one identity per package.
//
// RED at ff59c592bf: every arm recorded `<GOCACHE>/xx/<64 hex>-d`.
func TestACgoRewrittenTreeRecordsItsImportCSource(t *testing.T) {
	requireHostCC(t)

	goCache, err := exec.Command("go", "env", "GOCACHE").Output()

	if err != nil {
		t.Fatal(err)
	}

	buildCache := filepath.ToSlash(strings.TrimSpace(string(goCache)))

	only := func(t *testing.T, identities []string, combined string) string {
		t.Helper()

		if len(identities) != 1 {
			t.Fatalf("want exactly one GoPositionMap record (cptr.go; the bindings file has none), have %d: %v\n%s", len(identities), identities, lastLines(combined, 12))
		}

		if buildCache != "" && strings.Contains(identities[0], buildCache) {
			t.Fatalf("the record names the build cache: %s", identities[0])
		}

		return identities[0]
	}

	t.Run("a main-module cgo package records its source in the -trimpath form", func(t *testing.T) {
		combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr}, "-recurse", "-cgo=true")

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, lastLines(combined, 12))
		}

		if got := only(t, cgoPackageIdentities(t, output, "cptr"), combined); got != "example.com/cgorecurse@/cptr/cptr.go" {
			t.Errorf("want example.com/cgorecurse@/cptr/cptr.go, got %s", got)
		}
	})

	t.Run("-trimpath=false records the import \"C\" source's absolute path", func(t *testing.T) {
		combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr}, "-recurse", "-cgo=true", "-trimpath=false")

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, lastLines(combined, 12))
		}

		got := only(t, cgoPackageIdentities(t, output, "cptr"), combined)

		if !filepath.IsAbs(filepath.FromSlash(got)) || !strings.HasSuffix(got, "/cptr/cptr.go") {
			t.Errorf("want the absolute path of cptr/cptr.go, got %s", got)
		}

		if _, err := os.Stat(filepath.FromSlash(got)); err != nil {
			t.Errorf("the recorded source does not exist: %v", err)
		}
	})

	// -provenance's header (nugetgo-pack converts every flavor with it) named the same build-cache intermediate, by its
	// hashed base name, so it changed with every machine and run. It names the import "C" source; cmd/cgo's types file,
	// which no Go source wrote, is named as cmd/cgo names it.
	t.Run("-provenance names the import \"C\" source and cmd/cgo's types file, never a build-cache name", func(t *testing.T) {
		combined, output, err := convertCgoRecurse(t, map[string]string{"cptr/cptr.go": cgoRecurseCptr}, "-recurse", "-cgo=true", "-provenance")

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, lastLines(combined, 12))
		}

		headers := map[string]string{}
		hashed := regexp.MustCompile(`"[0-9a-f]{64}-[a-z]"`)

		for path, text := range cgoRecurseWritten(t, output, "cptr", ".cs") {
			for _, line := range strings.Split(text, "\n") {
				if strings.HasPrefix(line, "// Converted from Go source: ") {
					headers[path] = strings.TrimSpace(line)

					if hashed.MatchString(line) {
						t.Errorf("%s names a build-cache file: %s", path, line)
					}
				}
			}
		}

		var source, bindings string

		for path, header := range headers {
			switch {
			case strings.HasSuffix(path, "/cptr/cptr.cs"):
				source = header
			case strings.HasSuffix(path, ".cgo.cs"):
				bindings = header
			}
		}

		if want := `// Converted from Go source: "cptr/cptr.go" in module example.com/cgorecurse.`; source != want {
			t.Errorf("cptr.cs: want %s, got %q (headers: %v)", want, source, headers)
		}

		if want := `// Converted from Go source: "_cgo_gotypes.go".`; bindings != want {
			t.Errorf("the bindings file: want %s, got %q (headers: %v)", want, bindings, headers)
		}
	})

	// The go.mod-less release converts from a copy (copyModuleCacheRelease), so this arm also reads the copy's map back
	// to the cache path through the //line-adjusted name.
	t.Run("a module-cache release records <module>@<version>/<file>", func(t *testing.T) {
		cache := t.TempDir()
		release := filepath.Join(cache, "example.com", "cgocache@v1.0.0")
		writeModuleFile(t, filepath.Join(release, "cptr.go"), cgoRecurseCptr)
		writeModuleFile(t, filepath.Join(cache, "cache", "download", "example.com", "cgocache", "@v", "v1.0.0.mod"), "module example.com/cgocache\n")

		treeSrc, err := filepath.Abs("..")

		if err != nil {
			t.Fatal(err)
		}

		output := t.TempDir()
		cmd := exec.Command(driverBinary(t), "-recurse", "-cgo=true", "-go2cspath", treeSrc, release, output)
		cmd.Env = append(os.Environ(), "GOMODCACHE="+cache, "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=", "GOPROXY=off", "GOTOOLCHAIN=local", "TMPDIR="+t.TempDir())
		out, err := cmd.CombinedOutput()
		combined := string(out)

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, lastLines(combined, 12))
		}

		if got := only(t, cgoPackageIdentities(t, output, "cgocache"), combined); got != "example.com/cgocache@v1.0.0/cptr.go" {
			t.Errorf("want example.com/cgocache@v1.0.0/cptr.go, got %s", got)
		}
	})
}
