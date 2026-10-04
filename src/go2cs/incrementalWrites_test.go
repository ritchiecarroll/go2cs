// incrementalWrites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards incremental .cs writes: re-converting an UNCHANGED package must leave every emitted .cs untouched, so its
// timestamp holds and MSBuild does not recompile the project. Every other output the converter writes (project files,
// the solution, readmes, embeds, the platform merge) already skips an identical write through needToWriteFile; the
// converted .cs went through writeOutputFile, which rewrote it unconditionally.

package main

import (
	"go/build"
	"io/fs"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
	"time"
)

// incrementalFixtureStamp is the instant every emitted .cs is set to between the two conversions. A FIXED old instant,
// not "the first run's mtime": a rewrite within the filesystem's timestamp resolution could otherwise read unchanged.
var incrementalFixtureStamp = time.Date(2001, 1, 1, 0, 0, 0, 0, time.UTC)

func incrementalFixture(t *testing.T) (pkgDir, outDir string, options Options) {
	t.Helper()

	root := t.TempDir()
	pkgDir = filepath.Join(root, "incr")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/incr\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "a.go"), `package incr

type Point struct{ X, Y int }

func (p Point) Sum() int { return p.X + p.Y }
`)
	writeModuleFile(t, filepath.Join(pkgDir, "b.go"), `package incr

func Scale(p Point, k int) Point { return Point{p.X * k, p.Y * k} }
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options = Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	return pkgDir, filepath.Join(root, "out"), options
}

// stampEmittedSources sets every .cs under dir to incrementalFixtureStamp and returns their paths.
func stampEmittedSources(t *testing.T, dir string) []string {
	t.Helper()

	var sources []string

	err := filepath.WalkDir(dir, func(path string, entry fs.DirEntry, err error) error {
		if err != nil || entry.IsDir() || !strings.HasSuffix(path, ".cs") {
			return err
		}

		sources = append(sources, path)
		return os.Chtimes(path, incrementalFixtureStamp, incrementalFixtureStamp)
	})

	if err != nil {
		t.Fatalf("stamping the emitted sources: %v", err)
	}

	if len(sources) < 3 {
		t.Fatalf("the fixture emitted %d .cs files; expected a.cs, b.cs and package_info.cs at least: %v", len(sources), sources)
	}

	return sources
}

// rewrittenSources lists the sources whose timestamp moved off incrementalFixtureStamp.
func rewrittenSources(t *testing.T, sources []string) []string {
	t.Helper()

	var moved []string

	for _, path := range sources {
		info, err := os.Stat(path)

		if err != nil {
			t.Fatalf("stat %s: %v", path, err)
		}

		if !info.ModTime().Equal(incrementalFixtureStamp) {
			moved = append(moved, filepath.Base(path))
		}
	}

	return moved
}

func TestUnchangedReconversionLeavesEverySourceUntouched(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("first conversion: %v", err)
	}

	sources := stampEmittedSources(t, outDir)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("second conversion: %v", err)
	}

	if moved := rewrittenSources(t, sources); len(moved) > 0 {
		t.Errorf("an unchanged re-conversion rewrote %d of %d .cs files (their timestamps moved, so MSBuild recompiles): %v", len(moved), len(sources), moved)
	}
}
