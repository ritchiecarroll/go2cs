// arrayPointerViewDoors_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// WHAT THIS WATCHES (docs/phase4/DESIGN-array-pointer-doors.md 4.6 and 4.7).
//
// Go's `(*A)(p)[lo:hi]`, A an array type and p an unsafe.Pointer, is emitted as one of golib's view doors, which
// ALIAS the memory p names (src/go2cs/arrayPointerViewDoor.go). One operand keeps the emission the doors replaced,
// a span that copies: an operand that holds a `FromRef` mint. That number is an address read under `fixed` for one
// statement, so a window that aliases it would outlive the statement that made the address true. Two arms:
//
//	THE SPAN EMISSIONS  every converted file that still holds the copying span, by file and count, each with the
//	                    reason it kept it. A file no row names is a view that copies and that nobody read.
//	NO FromRef AT A DOOR  no line that calls a view door holds a FromRef mint.
//
// Hand-owned files and test sources are not converted production emission and are not read.

// copyingSpanEmission is the emission the doors replaced: a slice built from a span over a raw C# pointer.
var copyingSpanEmission = regexp.MustCompile(`new slice<[^()]+>\(new ReadOnlySpan<[^()]+>\(\([^()]+\*\)`)

// viewDoorCall is a call of one of the doors.
var viewDoorCall = regexp.MustCompile(`@unsafe\.Pointer\.(?:View<|CopyInto[<(]|CopyFrom<)`)

type copyingSpanRow struct {
	sites  int
	reason string
}

// copyingSpanEmissions is the declared census: converted file under src/core -> its copying spans.
var copyingSpanEmissions = map[string]copyingSpanRow{
	"internal/abi/type.cs": {2, "Methods and ExportedMethods: the operand is addChecked over a FromRef mint of the receiver (the node rule keeps " +
		"that mint: its use is an argument). Both read a method table and never write through the view"},
}

// viewDoorFindings reads one converted file: its copying spans against the table, and its door lines for a
// FromRef mint. spans collects the count per file for the rows-with-no-site check.
func viewDoorFindings(under, text string, spans map[string]int) []string {
	var findings []string
	found := 0

	for number, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		code := line

		if comment := strings.Index(code, "//"); comment >= 0 {
			code = code[:comment]
		}

		found += len(copyingSpanEmission.FindAllStringIndex(code, -1))

		if viewDoorCall.MatchString(code) && strings.Contains(code, "FromRef(") {
			findings = append(findings, fmt.Sprintf("FromRef AT A VIEW DOOR: %s:%d calls a view door over a FromRef mint. The door aliases the address, "+
				"and that address is true for one statement only: the converter keeps the copying span for such an operand "+
				"(arrayPointerViewOperand), so this line was written by hand or the rule was removed", under, number+1))
		}
	}

	if found > 0 {
		spans[under] = found
	}

	row, declared := copyingSpanEmissions[under]

	switch {
	case found > 0 && !declared:
		findings = append(findings, fmt.Sprintf("NEW COPYING SPAN: %s holds %d slice(s) built from a span over a raw pointer and is in no row. A write "+
			"through such a slice never reaches the memory. Read why the converter kept the span there, and add the row with the reason", under, found))
	case declared && found != row.sites:
		findings = append(findings, fmt.Sprintf("copying spans: %s holds %d and its row says %d: read the site that was added or removed and correct the row",
			under, found, row.sites))
	}

	return findings
}

// TestArrayPointerViewDoors is the guard.
func TestArrayPointerViewDoors(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	spans := map[string]int{}
	converted, doorFiles := 0, 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") || strings.HasSuffix(rel, "_test.cs") ||
			strings.HasPrefix(rel, mintGolib) || strings.HasPrefix(rel, mintUnsafe) {
			continue
		}

		raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatal(err)
		}

		text := string(raw)

		if interiorPointerIsHandOwned(rel, text) {
			continue
		}

		converted++

		if viewDoorCall.MatchString(text) {
			doorFiles++
		}

		for _, finding := range viewDoorFindings(strings.TrimPrefix(rel, "src/core/"), text, spans) {
			t.Error(finding)
		}
	}

	if converted < 2000 {
		t.Fatalf("VACUOUS: %d converted files; the guard is measuring nothing", converted)
	}

	// The doors' own liveness: a corpus with no door call would pass the FromRef arm without reading a line.
	if doorFiles == 0 {
		t.Errorf("VACUOUS: no converted file calls a view door, so the FromRef arm read nothing. The corpus is expected to hold door calls " +
			"(runtime's heapdump.cs among them); if the converter no longer emits them, this guard describes a tree that is gone")
	}

	for under := range copyingSpanEmissions {
		if spans[under] == 0 {
			t.Errorf("copying spans: the table names %s and it holds none: remove the row", under)
		}
	}

	t.Logf("READING: %d converted files, %d of them call a view door, %d hold a copying span", converted, doorFiles, len(spans))
}

// TestArrayPointerViewDoorsFires is the control: the same scanner over planted text.
func TestArrayPointerViewDoorsFires(t *testing.T) {
	expect := func(label string, findings []string, want string) {
		t.Helper()

		for _, finding := range findings {
			if strings.Contains(finding, want) {
				return
			}
		}

		t.Errorf("%s: no finding containing %q; got %v", label, want, findings)
	}

	planted := strings.ReplaceAll(`
internal static slice<byte> view(uintptr p, nint n) {
    // new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(p), (int)(n))) in a comment
    var b = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(p), (int)(n)));
    var c = @unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, 64);
    return @unsafe.Pointer.View<byte>(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), 8), 0, n, n, 64);
}
`, "\n", "\r\n")

	spans := map[string]int{}
	findings := viewDoorFindings("runtime/planted.cs", planted, spans)

	expect("a span in a file no row names", findings, "NEW COPYING SPAN: runtime/planted.cs holds 1 slice(s)")
	expect("a door over FromRef", findings, "FromRef AT A VIEW DOOR: runtime/planted.cs:6")

	if len(findings) != 2 {
		t.Errorf("the planted file: %d findings, want 2 (the comment and the clean door line are not findings): %v", len(findings), findings)
	}

	// A listed file that gained a span, and one that holds exactly its row.
	listed := "var a = new slice<Method>(new ReadOnlySpan<Method>((Method*)(uintptr)(x), 1));\n"
	expect("a third span in a listed file", viewDoorFindings("internal/abi/type.cs", strings.Repeat(listed, 3), spans), "internal/abi/type.cs holds 3 and its row says 2")

	if findings := viewDoorFindings("internal/abi/type.cs", strings.Repeat(listed, 2), spans); len(findings) != 0 {
		t.Errorf("a listed file at its row's count was reported: %v", findings)
	}

	// The copy doors are doors too.
	for _, door := range []string{"@unsafe.Pointer.CopyInto<byte>(", "@unsafe.Pointer.CopyInto(", "@unsafe.Pointer.CopyFrom<byte>(dst, "} {
		line := "var n = " + door + "(@unsafe.Pointer)(uintptr)@unsafe.Pointer.FromRef(ref t), 0, 64, src);\n"
		expect(door, viewDoorFindings("runtime/planted.cs", line, map[string]int{}), "FromRef AT A VIEW DOOR")
	}
}
