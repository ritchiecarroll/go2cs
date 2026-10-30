// platformLayoutFold_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Seat #49: a multi-platform merge that folds a package's LAST per-GOOS files flat must leave the project file every
// single-target writer renders for the folded package, with no L3 block. os/signal and time/tzdata: #41's imported-alias
// drop made their three targets' package_info.cs identical, so the merge folded them flat, but it never re-rendered
// their project files. The seed's L3 block survived, inert, beside empty per-GOOS folders. A -tests convert (the
// single-target writer, which reads no per-GOOS sources there) then rendered the file without it, and so would any
// reconvert from a fresh checkout, which carries no empty folders: the emission had two fixed points.

package main

import (
	"os"
	"path/filepath"
	"testing"
)

func TestMergeThatFoldsAPackagesLastPerGoosFilesDropsItsLayoutBlock(t *testing.T) {
	root := t.TempDir()
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	plain := string(csprojTemplate)
	layout := applyPlatformLayoutBlocks(plain, "osx.csproj")

	if layout == plain {
		t.Fatal("fixture is inert: the stock template takes no L3 block")
	}

	write := func(base, rel, content string) {
		t.Helper()
		path := filepath.Join(base, "core", filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	read := func(rel string) (string, bool) {
		data, err := os.ReadFile(filepath.Join(root, "core", filepath.FromSlash(rel)))
		return string(data), err == nil
	}

	// The seeded corpus: both packages are L3, signal through its per-GOOS package_info.cs alone (the targets' copies
	// differed by an imported alias), osx through a source that still varies.
	write(root, "signal/signal.cs", "// signal\r\n")
	write(root, "signal/signal.csproj", layout)
	write(root, "osx/osx.csproj", layout)
	write(root, "osx/shared.cs", "// shared\r\n")

	for _, target := range targets {
		goos := goosOfTarget(target)
		write(root, "signal/"+goos+"/package_info.cs", "// package_info "+goos+"\r\n")
		write(root, "osx/"+goos+"/file.cs", "// file "+goos+"\r\n")
	}

	var emissions []*platformEmission

	for _, target := range targets {
		goos := goosOfTarget(target)
		stage := t.TempDir()
		write(stage, "signal/signal.cs", "// signal\r\n")
		write(stage, "signal/package_info.cs", "// package_info, now the same on every target\r\n")
		write(stage, "osx/shared.cs", "// shared\r\n")
		write(stage, "osx/file.cs", "// file "+goos+"\r\n")

		emissions = append(emissions, &platformEmission{target: target, goos: goos, root: stage, artifacts: map[string]artifactState{
			"signal/signal.cs":       {hash: "signal", emitted: true},
			"signal/package_info.cs": {hash: "info", emitted: true},
			"osx/shared.cs":          {hash: "shared", emitted: true},
			"osx/file.cs":            {hash: "file-" + goos, emitted: true},
		}})
	}

	if err := mergePlatformEmissions(root, targets, emissions); err != nil {
		t.Fatalf("merge: %v", err)
	}

	// Vacuity: the fold happened, and osx stayed per-GOOS.
	if _, ok := read("signal/package_info.cs"); !ok {
		t.Fatal("fixture is inert: signal's package_info.cs was not folded flat")
	}

	if _, ok := read("osx/linux/file.cs"); !ok {
		t.Fatal("fixture is inert: osx's varying source left its per-GOOS folder")
	}

	// The subject: the folded package's project file is the single-target writer's, and its empty folders are gone.
	if got, _ := read("signal/signal.csproj"); got != plain {
		t.Errorf("a package with no per-GOOS sources left keeps the L3 block the single-target writer does not render: two fixed points:\n%s", got)
	}

	for _, target := range targets {
		if _, err := os.Stat(filepath.Join(root, "core", "signal", goosOfTarget(target))); err == nil {
			t.Errorf("signal/%s is left behind empty; no checkout carries it", goosOfTarget(target))
		}
	}

	// The negative side: a package that still carries per-GOOS sources keeps its block byte for byte.
	if got, _ := read("osx/osx.csproj"); got != layout {
		t.Errorf("a package that is still L3 must keep its project file byte-identical:\n%s", got)
	}
}
