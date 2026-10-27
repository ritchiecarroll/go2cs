// handOwnPackageBoxParity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// A hand-owned file's EXPORTED package-level boxes must include every one its converter twin declares.
//
// An importer decides whether another package's exported var has a box from the OWNER's Go syntax
// (importedGlobalIsBoxed runs the owner's predicate), and then binds `pkg.ᏑV`. When the owner's file is a
// whole-file hand-own, the converter's view of that file is its `.cs.auto` review twin, but the COMPILED
// declaration is the hand-own's. If the twin boxes an exported var the hand-own declares as a plain field, every
// importer's `&pkg.V` names a member that does not exist (CS0117). Measured on windows at the crosspkg seat's
// base: syscall.ForkLock (exec_windows.cs) had a box in its twin and a field in its hand-own, latent only because
// no windows emission took its address (BOARD 2026-10-26, G).
//
// Only exported boxes are compared: an unexported box serves the owner's own addresses, and a hand-own is free to
// implement those another way.

// packageBoxDecl matches a package-level box declaration in either form the emitter has written, capturing the
// var's name.
var packageBoxDecl = regexp.MustCompile(`^\s*(?:public|internal)\s+static\s+(?:readonly\s+)?(?:ж|StaticBox|StandardBox)<.+>\s+Ꮡ([\p{L}_][\p{L}\p{N}_]*)\s*=\s*new\s+(?:StaticBox|StandardBox)<`)

func exportedPackageBoxes(content string) map[string]bool {
	boxes := map[string]bool{}

	for _, line := range strings.Split(strings.ReplaceAll(content, "\r\n", "\n"), "\n") {
		if m := packageBoxDecl.FindStringSubmatch(line); m != nil {
			if first := []rune(m[1])[0]; first >= 'A' && first <= 'Z' {
				boxes[m[1]] = true
			}
		}
	}

	return boxes
}

// scanHandOwnBoxParity returns one finding per exported box a twin declares and its hand-own does not. handOwns
// maps a hand-owned file's path to its content; twins maps the same path to its `.cs.auto` content.
func scanHandOwnBoxParity(handOwns, twins map[string]string) []string {
	var findings []string

	for path, twin := range twins {
		handOwn, ok := handOwns[path]

		if !ok {
			continue
		}

		declared := exportedPackageBoxes(handOwn)

		for name := range exportedPackageBoxes(twin) {
			if !declared[name] {
				findings = append(findings, path+": the converter's twin boxes exported var "+name+
					" (Ꮡ"+name+"), the hand-own does not; an importer's &pkg."+name+" names a member that does not exist (CS0117)")
			}
		}
	}

	sort.Strings(findings)
	return findings
}

func TestHandOwnExportedPackageBoxesMatchTheirTwins(t *testing.T) {
	root := repoRootFromPackageDir(t)
	handOwns := map[string]string{}
	twins := map[string]string{}

	for _, p := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(p, "src/core/") || !strings.HasSuffix(p, ".cs.auto") {
			continue
		}

		principal := strings.TrimSuffix(p, ".auto")
		handOwn, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(principal)))

		if err != nil {
			continue
		}

		marked := false

		for _, line := range strings.Split(strings.ReplaceAll(string(handOwn), "\r\n", "\n"), "\n") {
			if handOwnMarkerLine.MatchString(line) {
				marked = true
				break
			}
		}

		if !marked {
			continue
		}

		twin, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("reading %s: %v", p, err)
		}

		handOwns[principal] = string(handOwn)
		twins[principal] = string(twin)
	}

	if len(twins) == 0 {
		t.Fatal("no hand-owned file with a .cs.auto twin found under src/core: wrong root or broken marker predicate")
	}

	for _, finding := range scanHandOwnBoxParity(handOwns, twins) {
		t.Error(finding)
	}

	t.Logf("%d hand-owned files with a converter twin compared", len(twins))
}

// TestHandOwnBoxParityScannerFires plants each case: a missing box is named, a declared one (in either emitted
// form) is not, and an unexported box the hand-own implements another way is not compared.
func TestHandOwnBoxParityScannerFires(t *testing.T) {
	twin := "public static readonly ж<nint> ᏑCount = new StaticBox<nint>(default(nint));\n" +
		"public static ж<sync.RWMutex> ᏑLock = new StandardBox<sync.RWMutex>(default(sync.RWMutex));\n" +
		"internal static readonly ж<nint> Ꮡhidden = new StaticBox<nint>(default(nint));\n"

	cases := []struct {
		name    string
		handOwn string
		want    []string
	}{
		{"both declared", "public static ж<nint> ᏑCount = new StandardBox<nint>(0);\npublic static readonly ж<sync.RWMutex> ᏑLock = new StaticBox<sync.RWMutex>(default(sync.RWMutex));\n", nil},
		{"a field where the twin boxes", "public static ж<nint> ᏑCount = new StandardBox<nint>(0);\npublic static sync.RWMutex Lock;\n", []string{"Lock"}},
		{"neither declared", "public static nint Count;\npublic static sync.RWMutex Lock;\n", []string{"Count", "Lock"}},
	}

	for _, c := range cases {
		findings := scanHandOwnBoxParity(map[string]string{"src/core/p/p.cs": c.handOwn}, map[string]string{"src/core/p/p.cs": twin})

		if len(findings) != len(c.want) {
			t.Errorf("%s: %d findings, want %d: %v", c.name, len(findings), len(c.want), findings)
			continue
		}

		for i, name := range c.want {
			if !strings.Contains(findings[i], "exported var "+name+" ") {
				t.Errorf("%s: finding %d does not name %s: %s", c.name, i, name, findings[i])
			}
		}
	}
}
