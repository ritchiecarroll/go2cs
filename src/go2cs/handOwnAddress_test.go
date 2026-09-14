// handOwnAddress_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// THE CLASS THIS GUARDS, and it cost a commit on 2026-09-14.
//
// A hand-own's C# address is a function of its package: the namespace follows the import path and
// the package class follows the Go package name. A RELOCATION moves the file's PATH and leaves both
// behind, so the file lands in the right directory declaring the wrong address. All three
// relocations in the H5 hop did exactly that (c8d50e014f): internal/concurrent/hashtriemap.cs moved
// to internal/sync still saying `partial class concurrent_package`, internal/weak/pointer.cs moved
// to weak/ still saying `namespace go.@internal`, and crypto/internal/alias/alias_impl.cs moved
// under fips140/ still saying `namespace go.crypto.@internal`.
//
// ⚠ WHY NO BUILD CAN FIND THIS, WHICH IS THE WHOLE REASON THE GUARD EXISTS. go2cs-gen reads the
// package address OUT OF THE FILE ITSELF -- `ImplementGenerator.cs` takes the namespace from the
// file's own namespace syntax and the package class from the file's own first class, and never
// checks either against the package being compiled -- then `TemplateBase`'s header emits
// `namespace {PackageNamespace}; public static partial class {PackageName}_package`. So a wrong
// address is not caught, it is MIRRORED: the generator emits a complete, internally consistent
// companion at the same wrong address. Nothing collides, nothing is missing, every part agrees, the
// build is clean, and the members are simply not where any consumer binds. A half-move produces
// ZERO build errors, measured independently by two lanes.
//
// It also silently re-arms the defect the hand-own existed to displace: the auto conversion at the
// RIGHT address keeps shipping while the hand-own sits inert at the wrong one.
//
// ⚠ HOP-INDEPENDENT BY CONSTRUCTION. It compares a file against its own directory and never against
// a Go release, a GOROOT, a converted corpus or a built tree -- pure text, like the ValueClone stamp
// guard it sits beside. Measured green on master's corpus (146 marked files) and on the 1.24.13
// version branch (145) with identical residual buckets, and RED on the pre-cure tree naming exactly
// the three real defects. That last reading is the positive control that matters: a planted mismatch
// proves the scanner runs, but a HISTORICAL one proves it catches the thing it was built for.
//
// ⚠ WHAT THE AUTHORITY IS, AND THE TWO SPLITS THAT ARE NOT DEFECTS. The recon that preceded this
// guard measured both, because a rule written from expectation would have gone red on master for
// legitimate reasons:
//
//   - A directory legitimately holds THREE package classes: `<pkg>_package` plus the external and
//     internal test packages `<pkg>_test_package` and `<pkg>_internal_test_package`. So the class
//     authority is the single NON-TEST class, and the test variants are EXCLUDED rather than
//     tolerated -- 39 directories would otherwise read as split.
//   - src/core/testing legitimately holds `go.testing_runtime` beside `go` (the test-host machinery
//     lives in the same directory as the Go package). So the namespace assertion is MEMBERSHIP in
//     the set the directory declares, not equality with one value. Membership is still sharp enough
//     to have caught two of the three real defects.

var (
	handOwnAddressMarkerRe = regexp.MustCompile(`(?m)^[\t ]*\[module:[\t ]*(?:go\.)?GoManualConversion\]`)

	// ⚠ EVERY ANCHOR CARRIES \r?. The corpus is CRLF, and Go's `$` under (?m) matches before the
	// \n -- which on a CRLF file is AFTER the \r, so a bare `$` matches NOTHING here. This cost a
	// red post-condition over a correct tree on 2026-09-14, and it is the shape that fails SILENTLY
	// when the expected count is zero.
	handOwnNamespaceRe = regexp.MustCompile(`(?m)^namespace[\t ]+([^;{\r\n]+?)[\t ]*;[\t ]*\r?$`)
	handOwnClassRe     = regexp.MustCompile(`(?m)^[\t ]*(?:public[\t ]+|internal[\t ]+)?(?:static[\t ]+)?partial[\t ]+(?:class|struct)[\t ]+([A-Za-z_@][A-Za-z0-9_@]*_package)\b`)
)

// isTestPackageClass reports whether a package class is one of the test-package variants. Both
// `<pkg>_test_package` (Go's external test package) and `<pkg>_internal_test_package` end in the
// same suffix, so one test covers both.
func isTestPackageClass(name string) bool { return strings.HasSuffix(name, "_test_package") }

// addressFinding is one hand-own whose declared address disagrees with its directory.
type addressFinding struct {
	File  string
	Axis  string // "namespace" or "class"
	Got   string
	Want  string
}

func (f addressFinding) String() string {
	return fmt.Sprintf("%s: %s is %q, but its package declares %s", f.File, f.Axis, f.Got, f.Want)
}

// csAddress is one file's declared address. An empty field means the file does not declare that
// axis, which is a SKIP and never a finding: a file of free functions declares no package class,
// and a scanner that cannot read a declaration has not found a missing one.
type csAddress struct {
	Namespace string
	Class     string
}

func readCsAddress(path string) (csAddress, error) {
	raw, err := os.ReadFile(path)
	if err != nil {
		return csAddress{}, err
	}
	// Bytes, deliberately: one corpus file (golib/sslice.cs) is ISO-8859 rather than UTF-8, and a
	// reader that decodes would have to decide what to do about it. Matching over bytes does not
	// care, and this guard has no business having an opinion about that file's encoding.
	text := string(raw)
	var addr csAddress
	if m := handOwnNamespaceRe.FindStringSubmatch(text); m != nil {
		addr.Namespace = m[1]
	}
	if m := handOwnClassRe.FindStringSubmatch(text); m != nil {
		addr.Class = m[1]
	}
	return addr, nil
}

// scanHandOwnAddresses returns every hand-own whose declared address disagrees with its directory,
// plus the population counts the vacuity arm reads.
func scanHandOwnAddresses(root string) (findings []addressFinding, marked, compared int, err error) {
	// One pass to group by directory, because the authority for a file is its SIBLINGS.
	byDir := map[string][]string{}
	walkErr := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			switch info.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}
		// `<name>.cs.auto` does not end in ".cs", so demoted auto siblings are excluded here rather
		// than needing a rule of their own.
		if strings.HasSuffix(path, ".cs") {
			byDir[filepath.Dir(path)] = append(byDir[filepath.Dir(path)], path)
		}
		return nil
	})
	if walkErr != nil {
		return nil, 0, 0, walkErr
	}

	dirs := make([]string, 0, len(byDir))
	for d := range byDir {
		dirs = append(dirs, d)
	}
	sort.Strings(dirs)

	for _, dir := range dirs {
		paths := byDir[dir]
		sort.Strings(paths)

		addrs := make(map[string]csAddress, len(paths))
		raws := make(map[string][]byte, len(paths))
		for _, p := range paths {
			raw, readErr := os.ReadFile(p)
			if readErr != nil {
				return nil, 0, 0, readErr
			}
			raws[p] = raw
			addr, addrErr := readCsAddress(p)
			if addrErr != nil {
				return nil, 0, 0, addrErr
			}
			addrs[p] = addr
		}

		for _, p := range paths {
			if !handOwnAddressMarkerRe.Match(raws[p]) {
				continue
			}
			marked++
			rel, relErr := filepath.Rel(root, p)
			if relErr != nil {
				rel = p
			}
			rel = filepath.ToSlash(rel)
			mine := addrs[p]

			// The authority: the siblings' declared addresses.
			nsSet := map[string]bool{}
			clsSet := map[string]bool{}
			for _, q := range paths {
				if q == p {
					continue
				}
				if a := addrs[q]; true {
					if a.Namespace != "" {
						nsSet[a.Namespace] = true
					}
					if a.Class != "" && !isTestPackageClass(a.Class) {
						clsSet[a.Class] = true
					}
				}
			}
			if len(nsSet) == 0 && len(clsSet) == 0 {
				continue // no sibling declares an address: nothing to compare against
			}
			compared++

			// NAMESPACE -- membership, because src/core/testing legitimately declares two.
			if mine.Namespace != "" && len(nsSet) > 0 && !nsSet[mine.Namespace] {
				want := make([]string, 0, len(nsSet))
				for n := range nsSet {
					want = append(want, n)
				}
				sort.Strings(want)
				findings = append(findings, addressFinding{
					File: rel, Axis: "namespace", Got: mine.Namespace,
					Want: strings.Join(want, " or "),
				})
			}

			// CLASS -- equality, but only where the directory has exactly ONE non-test class to be
			// equal to. A hand-own that is itself a test-package file is not held to it.
			if mine.Class != "" && !isTestPackageClass(mine.Class) && len(clsSet) == 1 {
				var want string
				for c := range clsSet {
					want = c
				}
				if mine.Class != want {
					findings = append(findings, addressFinding{
						File: rel, Axis: "package class", Got: mine.Class, Want: want,
					})
				}
			}
		}
	}

	sort.Slice(findings, func(i, j int) bool { return findings[i].String() < findings[j].String() })
	return findings, marked, compared, nil
}

// TestHandOwnAddressMatchesItsPackage is the guard.
//
// ⚠ IT READS FILES OUTSIDE THIS MODULE (src\core), and cmd/go DROPS out-of-module files from the
// test input hash -- so a cached PASS would survive a reintroduction. Run the converter suite with
// -count=1, which every gate in this repo already does.
func TestHandOwnAddressMatchesItsPackage(t *testing.T) {
	root := repoRootFromPackageDir(t)
	core := filepath.Join(root, "src", "core")
	if _, err := os.Stat(core); err != nil {
		t.Fatalf("converted corpus not found at %s: %v", core, err)
	}

	findings, marked, compared, err := scanHandOwnAddresses(core)
	if err != nil {
		t.Fatalf("scanning %s: %v", core, err)
	}

	// A zero that could not have been anything else is not a measurement -- the lesson the
	// neighbouring ValueClone guard paid for, not repeated here.
	if marked == 0 || compared == 0 {
		t.Fatalf("VACUOUS: found %d hand-owned file(s) and compared %d against a sibling; "+
			"the guard cannot fail in this state -- check the [module: GoManualConversion] spelling "+
			"and that src/core is populated", marked, compared)
	}
	t.Logf("hand-owned files %d, compared against a sibling %d", marked, compared)

	if len(findings) > 0 {
		var b strings.Builder
		for _, f := range findings {
			b.WriteString("\n  " + f.String())
		}
		t.Fatalf("%d hand-own(s) declare an address their package does not use. go2cs-gen reads the "+
			"address out of the file and MIRRORS it, so this produces no build error at all -- the "+
			"members land at the wrong address and every consumer binds to the auto conversion "+
			"instead:%s", len(findings), b.String())
	}
}

// TestHandOwnAddressScannerFiresAndAdmits is the positive control, in BOTH directions: a planted
// mismatch on each axis must be reported, and a planted CORRECT hand-own must not be. An admit-only
// control cannot fail on a dead scanner, and a fires-only control cannot fail on one that refuses
// everything.
//
// The mismatches planted are the REAL historical ones from c8d50e014f rather than invented shapes,
// so the control also pins the `@`-escaped namespace spelling the findings depend on.
func TestHandOwnAddressScannerFiresAndAdmits(t *testing.T) {
	root := t.TempDir()

	write := func(dir, name, body string) {
		t.Helper()
		full := filepath.Join(root, dir)
		if err := os.MkdirAll(full, 0o755); err != nil {
			t.Fatalf("planting %s: %v", dir, err)
		}
		// CRLF on purpose: the corpus is CRLF and a control written LF-only would pass over
		// anchors that cannot match the real tree.
		body = strings.ReplaceAll(body, "\n", "\r\n")
		if err := os.WriteFile(filepath.Join(full, name), []byte(body), 0o600); err != nil {
			t.Fatalf("planting %s/%s: %v", dir, name, err)
		}
	}

	const marker = "[module: go.GoManualConversion]\n"

	// BAD 1 -- the namespace defect: the hand-own kept its old package's namespace.
	write("weak", "package_info.cs", "namespace go;\npublic static partial class weak_package\n{\n}\n")
	write("weak", "pointer.cs", marker+"namespace go.@internal;\npartial class weak_package {\n}\n")

	// BAD 2 -- the class defect: the hand-own kept its old package's class.
	write("internal/sync", "package_info.cs", "namespace go.@internal;\npublic static partial class sync_package\n{\n}\n")
	write("internal/sync", "hashtriemap.cs", marker+"namespace go.@internal;\npartial class concurrent_package {\n}\n")

	// GOOD -- same shape, spelled as its package does.
	write("bytes", "package_info.cs", "namespace go;\npublic static partial class bytes_package\n{\n}\n")
	write("bytes", "buffer_impl.cs", marker+"namespace go;\npartial class bytes_package {\n}\n")

	// GOOD -- the test-package variants must NOT be read as a competing authority.
	write("subtle", "package_info.cs", "namespace go.crypto;\npublic static partial class subtle_package\n{\n}\n")
	write("subtle", "package_test_info.cs", "namespace go.crypto;\npublic static partial class subtle_test_package\n{\n}\n")
	write("subtle", "constant_time.cs", marker+"namespace go.crypto;\npartial class subtle_package {\n}\n")

	// UNMARKED -- a converted file with the same defect must be IGNORED; the converter owns its own
	// emission and this guard is about frozen hand-owns only.
	write("elsewhere", "package_info.cs", "namespace go;\npublic static partial class elsewhere_package\n{\n}\n")
	write("elsewhere", "plain.cs", "namespace go.@internal;\npartial class wrong_package {\n}\n")

	findings, marked, compared, err := scanHandOwnAddresses(root)
	if err != nil {
		t.Fatalf("scanning the planted tree: %v", err)
	}
	if marked != 4 {
		t.Fatalf("expected 4 MARKED files (the unmarked plant must be skipped), got %d", marked)
	}
	if compared != 4 {
		t.Fatalf("expected all 4 marked plants to have a sibling authority, got %d", compared)
	}
	if len(findings) != 2 {
		t.Fatalf("expected exactly 2 findings (one per axis), got %d: %v", len(findings), findings)
	}

	byFile := map[string]addressFinding{}
	for _, f := range findings {
		byFile[f.File] = f
	}

	ns, ok := byFile["weak/pointer.cs"]
	if !ok {
		t.Fatalf("the namespace defect was not reported; got %v", findings)
	}
	if ns.Axis != "namespace" || ns.Got != "go.@internal" || ns.Want != "go" {
		t.Errorf("the namespace finding does not name both spellings: %+v", ns)
	}

	cls, ok := byFile["internal/sync/hashtriemap.cs"]
	if !ok {
		t.Fatalf("the class defect was not reported; got %v", findings)
	}
	if cls.Axis != "package class" || cls.Got != "concurrent_package" || cls.Want != "sync_package" {
		t.Errorf("the class finding does not name both spellings: %+v", cls)
	}
}

// TestHandOwnAddressScannerRefusesAnEmptyTree pins the vacuity arm itself. A guard whose population
// can silently reach zero reports success over a tree it never read -- the failure mode the
// ValueClone stamp guard went vacuous by, one directory over, on this same hop.
func TestHandOwnAddressScannerRefusesAnEmptyTree(t *testing.T) {
	_, marked, compared, err := scanHandOwnAddresses(t.TempDir())
	if err != nil {
		t.Fatalf("scanning an empty tree: %v", err)
	}
	if marked != 0 || compared != 0 {
		t.Fatalf("an empty tree must yield a zero population, got marked=%d compared=%d", marked, compared)
	}
}
