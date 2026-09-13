// lineEndingSeams_test.go - Gbtc
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
	"runtime"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// These guards pin the READ side of the converter's line-ending contract (F3 in
// docs/PLAN-linux-operation.md). The converter emits CRLF unconditionally and .gitattributes now
// pins the emitted artifact types to eol=crlf, but neither fact governs a tree materialized BEFORE
// the pin, a `-recurse` output root the user manages, or a file some other tool rewrote. Every
// reader that scans for CRLF-shaped content therefore has to tolerate LF, and the failure it
// tolerates is not cosmetic: writePackageInfoFile log.Fatals — killing the whole `go test` binary,
// not just one test — and appendExternalTestPackageClass silently duplicates a class declaration.
//
// Each guard carries a NEGATIVE CONTROL asserting the pre-fix code would have failed, so a future
// edit cannot quietly restore the CRLF-only form and still pass.

// packageInfoMarkers are the five section markers writePackageInfoFile scans for; failing to find
// any one of them is a log.Fatal (packageInfoWriter.go, one per section).
var packageInfoMarkers = []string{
	"ImportedTypeAliases",
	"ExportedTypeAliases",
	"InterfaceImplementations",
	"ImplicitConversions",
	"TypeAccessibility",
}

// findMarkerSection replicates writePackageInfoFile's scan exactly: walk the split lines looking for
// the open marker, then the close marker, and report whether both were located in order.
func findMarkerSection(lines []string, marker string) bool {
	start, end := -1, -1

	for i, line := range lines {
		if strings.Contains(line, "<"+marker+">") {
			start = i
			continue
		}

		if strings.Contains(line, "</"+marker+">") {
			end = i
			break
		}
	}

	return start >= 0 && end >= 0 && start < end
}

// The embedded package_info skeleton is the exact content writePackageInfoFile splits — both when
// seeding a NEW file (the template arm) and, in the same shape, when re-reading a committed one.
// This is the whole failure: an LF copy split on "\r\n" yields ONE line, so no section is ever found.
func TestPackageInfoSectionScanSurvivesLfCheckout(t *testing.T) {
	// The template is a printf format string; the verbs are irrelevant to the marker scan, so it is
	// used raw here rather than rendered — what matters is the line structure. The skeleton carries
	// four of the five sections; <TypeAccessibility> is appended to a package_info.cs by a later
	// conversion, so the fixture adds it to reproduce a SETTLED file, which is what a reconvert
	// actually reads back.
	crlf := normalizeToCRLF(string(packageInfoTemplate)) +
		"\r\n// <TypeAccessibility>\r\n// </TypeAccessibility>\r\n"
	lf := normalizeToLF(crlf)

	if !strings.Contains(crlf, "\r\n") {
		t.Fatal("the embedded package_info template has no CRLF at all; this guard is not testing what it claims")
	}

	if strings.Contains(lf, "\r") {
		t.Fatal("normalizeToLF left a carriage return behind")
	}

	for _, marker := range packageInfoMarkers {
		if !findMarkerSection(splitLines(lf), marker) {
			t.Errorf("splitLines could not locate the <%s> section in an LF package_info.cs — writePackageInfoFile would log.Fatal", marker)
		}

		if !findMarkerSection(splitLines(crlf), marker) {
			t.Errorf("splitLines could not locate the <%s> section in a CRLF package_info.cs", marker)
		}
	}

	// Negative control: the pre-fix split. If this ever finds a section, the fixture stopped
	// exercising the bug and every assertion above is vacuous.
	preFix := strings.Split(lf, "\r\n")

	if len(preFix) != 1 {
		t.Fatalf("splitting LF content on \"\\r\\n\" yielded %d lines, expected 1 — the negative control is not reproducing the defect", len(preFix))
	}

	for _, marker := range packageInfoMarkers {
		if findMarkerSection(preFix, marker) {
			t.Fatalf("the pre-fix \"\\r\\n\" split located <%s> in LF content, so this guard proves nothing", marker)
		}
	}
}

// seedTestPackageInfo writes a minimal package_test_info.cs in the requested line-ending form and
// returns its path. The production `using static` directive is the anchor
// appendExternalTestPackageClass inserts against.
func seedTestPackageInfo(t *testing.T, contents string, crlf bool) string {
	t.Helper()

	if crlf {
		contents = normalizeToCRLF(contents)
	} else {
		contents = normalizeToLF(contents)
	}

	path := filepath.Join(t.TempDir(), "package_test_info.cs")

	if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
		t.Fatalf("failed to seed test package metadata: %v", err)
	}

	return path
}

const testPackageInfoSeed = "using static go.strings_package;\n" +
	"\n" +
	"[assembly: GoPackage(\"strings\")]\n"

// appendExternalTestPackageClass tests `strings.Contains(contents, block)` where block is built with
// CRLF. On an LF file the block is never found, so every run appends another copy — the file grows a
// duplicate `public static partial class` on each conversion, which is a CS0101/CS0111 storm rather
// than a clean failure. Idempotence across two calls is the property that catches it.
func TestAppendExternalTestPackageClassIsIdempotentOnLfCheckout(t *testing.T) {
	path := seedTestPackageInfo(t, testPackageInfoSeed, false)
	external := &packages.Package{Name: "strings_test"}

	for pass := 1; pass <= 2; pass++ {
		if err := appendExternalTestPackageClass(path, "go", "strings", external); err != nil {
			t.Fatalf("pass %d failed: %v", pass, err)
		}
	}

	data, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("failed to read back test package metadata: %v", err)
	}

	contents := string(data)

	if count := strings.Count(contents, "public static partial class"); count != 1 {
		t.Errorf("the [GoPackage] class was emitted %d times after two passes, expected 1 — the CRLF-shaped Contains missed on an LF file", count)
	}

	if count := strings.Count(contents, "using static go.strings_test_package;"); count != 1 {
		t.Errorf("the test using directive was inserted %d times after two passes, expected 1", count)
	}

	// The file the converter leaves behind is its own uniform CRLF, whatever it read.
	if strings.Contains(normalizeToCRLF(contents), "\r\n") && contents != normalizeToCRLF(contents) {
		t.Error("appendExternalTestPackageClass left mixed line endings behind; the rewritten file must be uniformly CRLF")
	}
}

// The Windows byte-stability pin: an already-CRLF file whose content needs no change must not be
// rewritten at all. This is what keeps the normalization inert on the platform the corpus is banked
// from — the `contents == string(data)` early-out has to survive it.
func TestAppendExternalTestPackageClassLeavesSettledCrlfFileUntouched(t *testing.T) {
	path := seedTestPackageInfo(t, testPackageInfoSeed, true)
	external := &packages.Package{Name: "strings_test"}

	if err := appendExternalTestPackageClass(path, "go", "strings", external); err != nil {
		t.Fatalf("first pass failed: %v", err)
	}

	settled, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("failed to read settled metadata: %v", err)
	}

	info, err := os.Stat(path)

	if err != nil {
		t.Fatalf("failed to stat settled metadata: %v", err)
	}

	modified := info.ModTime()

	if err := appendExternalTestPackageClass(path, "go", "strings", external); err != nil {
		t.Fatalf("second pass failed: %v", err)
	}

	again, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("failed to re-read settled metadata: %v", err)
	}

	if string(again) != string(settled) {
		t.Error("a second pass over a settled CRLF file changed its bytes")
	}

	if info, err = os.Stat(path); err == nil && !info.ModTime().Equal(modified) {
		t.Error("a second pass over a settled CRLF file rewrote it; the unchanged-content early-out was lost")
	}
}

func TestNormalizeToCRLFIsInertOnCrlfAndIdempotent(t *testing.T) {
	crlf := "one\r\ntwo\r\nthree\r\n"

	if got := normalizeToCRLF(crlf); got != crlf {
		t.Errorf("normalizeToCRLF changed already-CRLF content: %q", got)
	}

	if got := normalizeToCRLF(normalizeToCRLF("one\ntwo\nthree\n")); got != crlf {
		t.Errorf("normalizeToCRLF is not idempotent: %q", got)
	}

	// A bare LF among CRLFs — the shape a half-converted file has — normalizes uniformly rather
	// than doubling the carriage return.
	if got := normalizeToCRLF("one\r\ntwo\nthree\r\n"); got != crlf {
		t.Errorf("normalizeToCRLF mishandled mixed content: %q", got)
	}

	if got := splitLines("one\r\ntwo\nthree"); len(got) != 3 || got[0] != "one" || got[1] != "two" || got[2] != "three" {
		t.Errorf("splitLines did not split mixed content into three clean lines: %#v", got)
	}
}

// pathReplace's no-match is the silent failure F6 names: the caller keeps the untouched absolute
// source directory and emits it as a <ProjectReference> to a .csproj under GOROOT that does not
// exist. The bool is what lets the two stdlib call sites say so out loud.
func TestPathReplaceReportsWhetherItMatched(t *testing.T) {
	goRootSrc := filepath.Join("/usr/lib/go", "src")
	source := filepath.Join(goRootSrc, "fmt")

	got, ok := pathReplace(source, goRootSrc, "$(go2csPath)core")

	if !ok {
		t.Errorf("pathReplace reported no match rewriting %q under %q", source, goRootSrc)
	}

	if want := filepath.Join("$(go2csPath)core", "fmt"); got != want {
		t.Errorf("pathReplace produced %q, want %q", got, want)
	}

	// The case that used to be silent: a GOROOT spelling that shares no prefix with the source dir.
	// Neither path exists, so the symlink fallback cannot rescue it and the miss must be reported.
	elsewhere := filepath.Join("/opt/go-1.23", "src")
	got, ok = pathReplace(source, elsewhere, "$(go2csPath)core")

	if ok {
		t.Errorf("pathReplace claimed a match rewriting %q under the unrelated root %q", source, elsewhere)
	}

	if got != source {
		t.Errorf("a no-match must return the subject unchanged, got %q", got)
	}
}

// The Windows arm is case-INSENSITIVE by design — the same directory is legitimately spelled several
// ways there (`C:\Program Files\Go` vs a `GOROOT` set in lower case), and the corpus's emitted
// references depend on that match succeeding. Pinned so the F6 rework cannot have narrowed it.
func TestPathReplaceStaysCaseInsensitiveOnWindows(t *testing.T) {
	if runtime.GOOS != "windows" {
		t.Skip("the case-insensitive arm is Windows-only by design")
	}

	source := `C:\Program Files\Go\src\fmt`

	got, ok := pathReplace(source, `c:\program files\go\src`, "$(go2csPath)core")

	if !ok {
		t.Fatalf("pathReplace failed to match %q against a differently-cased GOROOT", source)
	}

	if want := `$(go2csPath)core\fmt`; got != want {
		t.Errorf("pathReplace produced %q, want %q", got, want)
	}
}

// The no-match warning is pinned to once per run: a conversion resolves dozens of stdlib imports and
// every one of them would otherwise repeat the same four-line advice.
func TestGoRootPathReplaceWarningIsOncePerRun(t *testing.T) {
	saved := goRootPathReplaceWarned
	goRootPathReplaceWarned = false
	t.Cleanup(func() { goRootPathReplaceWarned = saved })

	warnGoRootPathReplace("/opt/go/src/fmt", "/usr/lib/go/src")

	if !goRootPathReplaceWarned {
		t.Fatal("the first no-match did not arm the once-per-run latch")
	}

	// Second call must be a no-op; the latch staying set is the observable.
	warnGoRootPathReplace("/opt/go/src/strings", "/usr/lib/go/src")

	if !goRootPathReplaceWarned {
		t.Fatal("the once-per-run latch was cleared by a second warning")
	}
}
