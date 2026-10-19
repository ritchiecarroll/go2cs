// golibGrowSlice_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strconv"
	"strings"
	"testing"
)

// golib's slice growth is Go's runtime.growslice: nextslicecap, then roundupsize over the allocator's size
// classes (src/core/golib/GoSizeClasses.cs). Its numbers are Go's, so two guards hold them to the pinned
// toolchain rather than to a transcription:
//
//   - TestGolibSizeClassesMatchTheToolchain reads GOROOT/src/internal/runtime/gc and fails when golib's
//     class table or any constant roundupsize reads (MaxSmallSize, PageShift, MallocHeaderSize,
//     MinSizeForMallocHeader) differs. It also proves the one liberty golib takes, a binary search for the
//     smallest class at least as large as the request, equal to Go's SizeToSizeClass8 / SizeToSizeClass128
//     lookup at every small size.
//   - TestGoGrowSliceExpectationsMatchTheToolchain re-runs the probe behind GolibTests' GoGrowSliceTests
//     (src/tests/GolibTests/testdata/growslice/main.go, optimizations off so every append is heap growth)
//     and fails when its committed expected.txt is not what the toolchain prints now.
//
// POSITIVE CONTROL: verified red-first when these guards landed. Changing one class in golib's table
// (896 -> 900) failed the first guard naming that class; a stale expected.txt line failed the second.

var (
	goArrayLiteral = regexp.MustCompile(`(?m)^var (\w+) = \[[^\]]*\]uint(?:8|16)\{([^}]*)\}`)
	goConstLine    = regexp.MustCompile(`(?m)^\s*(\w+)\s*=\s*(.+?)\s*(?://.*)?$`)
	csConstLine    = regexp.MustCompile(`public const \w+ (\w+) = ([^;]+);`)
	csClassTable   = regexp.MustCompile(`(?s)ReadOnlySpan<ushort> SizeClassToSize =>\s*\[(.*?)\];`)
)

func parseIntList(t *testing.T, what, list string) []int {
	t.Helper()
	var out []int

	for _, field := range strings.Split(list, ",") {
		field = strings.TrimSpace(field)

		if field == "" {
			continue
		}

		n, err := strconv.Atoi(field)

		if err != nil {
			t.Fatalf("%s: %q is not an integer: %v", what, field, err)
		}

		out = append(out, n)
	}

	return out
}

func gorootForGuards() string {
	if goroot := build.Default.GOROOT; goroot != "" {
		return goroot
	}

	return runtime.GOROOT()
}

func TestGolibSizeClassesMatchTheToolchain(t *testing.T) {
	root := repoRootFromPackageDir(t)
	gcDir := filepath.Join(gorootForGuards(), "src", "internal", "runtime", "gc")

	sizeclasses, err := os.ReadFile(filepath.Join(gcDir, "sizeclasses.go"))

	if err != nil {
		t.Fatalf("reading the toolchain's size classes: %v", err)
	}

	malloc, err := os.ReadFile(filepath.Join(gcDir, "malloc.go"))

	if err != nil {
		t.Fatalf("reading the toolchain's malloc constants: %v", err)
	}

	golib, err := os.ReadFile(filepath.Join(root, "src", "core", "golib", "GoSizeClasses.cs"))

	if err != nil {
		t.Fatalf("reading golib's size classes: %v", err)
	}

	goArrays := map[string][]int{}

	for _, m := range goArrayLiteral.FindAllStringSubmatch(string(sizeclasses), -1) {
		goArrays[m[1]] = parseIntList(t, "sizeclasses.go "+m[1], m[2])
	}

	goConsts := map[string]string{}

	for _, src := range [][]byte{sizeclasses, malloc} {
		for _, m := range goConstLine.FindAllStringSubmatch(string(src), -1) {
			goConsts[m[1]] = m[2]
		}
	}

	goInt := func(name string) int {
		n, err := strconv.Atoi(goConsts[name])

		if err != nil {
			t.Fatalf("the toolchain's %s is %q, not an integer literal", name, goConsts[name])
		}

		return n
	}

	// MinSizeForMallocHeader is an expression in the toolchain; golib states its 64-bit value.
	if got := goConsts["MinSizeForMallocHeader"]; got != "goarch.PtrSize * goarch.PtrBits" {
		t.Fatalf("the toolchain's MinSizeForMallocHeader is now %q; restate golib's MinSizeForMallocHeader from it", got)
	}

	want := map[string]int{
		"MaxSmallSize":           goInt("MaxSmallSize"),
		"PageShift":              goInt("PageShift"),
		"MallocHeaderSize":       goInt("MallocHeaderSize"),
		"MinSizeForMallocHeader": 8 * 64,
	}

	csConsts := map[string]string{}

	for _, m := range csConstLine.FindAllStringSubmatch(string(golib), -1) {
		csConsts[m[1]] = strings.TrimSpace(m[2])
	}

	for name, value := range want {
		if got := csConsts[name]; got != strconv.Itoa(value) {
			t.Errorf("golib's GoSizeClasses.%s is %q, the toolchain's is %d", name, got, value)
		}
	}

	table := csClassTable.FindStringSubmatch(string(golib))

	if table == nil {
		t.Fatalf("GoSizeClasses.cs no longer declares its SizeClassToSize table in the form this guard reads")
	}

	golibClasses := parseIntList(t, "GoSizeClasses.SizeClassToSize", table[1])
	goClasses := goArrays["SizeClassToSize"]

	if len(goClasses) == 0 {
		t.Fatalf("sizeclasses.go no longer declares SizeClassToSize in the form this guard reads")
	}

	if len(golibClasses) != len(goClasses) {
		t.Fatalf("golib has %d size classes, the toolchain %d", len(golibClasses), len(goClasses))
	}

	for i := range goClasses {
		if golibClasses[i] != goClasses[i] {
			t.Errorf("size class %d: golib %d, the toolchain %d", i, golibClasses[i], goClasses[i])
		}
	}

	// golib takes the smallest class at least as large as the request; Go reads two lookup tables. They
	// must agree at every small size.
	to8, to128 := goArrays["SizeToSizeClass8"], goArrays["SizeToSizeClass128"]
	smallSizeMax, smallSizeDiv, largeSizeDiv := goInt("SmallSizeMax"), goInt("SmallSizeDiv"), goInt("LargeSizeDiv")

	if len(to8) == 0 || len(to128) == 0 {
		t.Fatalf("sizeclasses.go no longer declares SizeToSizeClass8/SizeToSizeClass128 in the form this guard reads")
	}

	for size := 1; size <= goInt("MaxSmallSize"); size++ {
		var class int

		if size <= smallSizeMax-8 {
			class = to8[(size+smallSizeDiv-1)/smallSizeDiv]
		} else {
			class = to128[(size-smallSizeMax+largeSizeDiv-1)/largeSizeDiv]
		}

		smallest := 0

		for _, c := range goClasses[1:] {
			if c >= size {
				smallest = c
				break
			}
		}

		if goClasses[class] != smallest {
			t.Fatalf("size %d: Go's lookup gives class size %d, the smallest class at least as large is %d", size, goClasses[class], smallest)
		}
	}
}

func TestGoGrowSliceExpectationsMatchTheToolchain(t *testing.T) {
	root := repoRootFromPackageDir(t)
	dir := filepath.Join(root, "src", "tests", "GolibTests", "testdata", "growslice")

	expected, err := os.ReadFile(filepath.Join(dir, "expected.txt"))

	if err != nil {
		t.Fatalf("reading expected.txt: %v", err)
	}

	run := exec.Command(filepath.Join(gorootForGuards(), "bin", "go"), "run", "-gcflags=-N -l", "main.go")
	run.Dir = dir
	run.Env = append(os.Environ(), "GO111MODULE=off", "CGO_ENABLED=0", "GOTOOLCHAIN=local")
	var stdout, stderr bytes.Buffer
	run.Stdout, run.Stderr = &stdout, &stderr

	if err := run.Run(); err != nil {
		t.Fatalf("running the growslice probe: %v\n%s", err, stderr.String())
	}

	normalize := func(b []byte) string { return strings.TrimSpace(strings.ReplaceAll(string(b), "\r\n", "\n")) }

	if got, want := normalize(stdout.Bytes()), normalize(expected); got != want {
		gotLines, wantLines := strings.Split(got, "\n"), strings.Split(want, "\n")

		for i := 0; i < len(gotLines) || i < len(wantLines); i++ {
			var g, w string

			if i < len(gotLines) {
				g = gotLines[i]
			}

			if i < len(wantLines) {
				w = wantLines[i]
			}

			if g != w {
				t.Errorf("expected.txt line %d differs from the toolchain's output:\n  committed: %s\n  toolchain: %s", i+1, w, g)
			}
		}

		t.Fatalf("regenerate it: cd %s && go run -gcflags='-N -l' main.go > expected.txt", filepath.ToSlash(dir))
	}
}
