// droppedFiles_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// A file the converter fails on (droppedFiles.go): the run fails at its end, naming the file, where it used to exit 0.

// TestDroppedFileFailsTheRunAndNamesTheFile converts a two-file package with the visit of one file faulting, as a
// visitor panic would. The other file is written, the faulted one is not, and the run's closing check returns the
// GO2CS2008 error naming it, then clears, so the next run starts clean. Red before droppedFiles.go: the recover
// printed a warning and the run had nothing to fail on.
func TestDroppedFileFailsTheRunAndNamesTheFile(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	takeDroppedFiles()
	t.Cleanup(func() { takeDroppedFiles(); visitFileFaultHook = nil })

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/dropped\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "kept.go"), "package lib\n\nfunc Kept() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "faulted.go"), "package lib\n\nfunc Faulted() int { return 2 }\n")

	visitFileFaultHook = func(filePath string) {
		if filepath.Base(filePath) == "faulted.go" {
			panic("planted visitor fault")
		}
	}

	out := filepath.Join(root, "out")
	options := Options{
		goRoot:              build.Default.GOROOT,
		goPath:              build.Default.GOPATH,
		go2csPath:           out,
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	if options.goRoot == "" {
		options.goRoot = runtime.GOROOT()
	}

	build.Default.GOROOT = options.goRoot

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	libDir := filepath.Join(out, "src", "example.com", "dropped", "lib")

	if _, err := os.Stat(filepath.Join(libDir, "kept.cs")); err != nil {
		t.Errorf("the file that converted is written: %v", err)
	}

	if _, err := os.Stat(filepath.Join(libDir, "faulted.cs")); !os.IsNotExist(err) {
		t.Errorf("the faulted file is not written (stat: %v)", err)
	}

	err := droppedFileError()

	if err == nil {
		t.Fatal("a run that dropped a file fails; droppedFileError returned nil")
	}

	message := err.Error()

	for _, want := range []string{diagnosticFileDropped, "1 file", "faulted.go (.cs): planted visitor fault"} {
		if !strings.Contains(message, want) {
			t.Errorf("the error names %q:\n%s", want, message)
		}
	}

	if strings.Contains(message, "kept.go") {
		t.Errorf("the error names only the dropped file:\n%s", message)
	}

	if err := droppedFileError(); err != nil {
		t.Errorf("the record is cleared once read; a second read returned:\n%v", err)
	}
}

// TestDroppedFileReportsEachKind holds the text of the two kinds: a .cs that was not written, and a hand-owned file
// whose `.cs.auto` review sibling was not, whose package-wide records may still be incomplete.
func TestDroppedFileReportsEachKind(t *testing.T) {
	takeDroppedFiles()
	t.Cleanup(func() { takeDroppedFiles() })

	recordDroppedFile(filepath.Join("pkg", "a.go"), "boom", false)
	recordDroppedFile(filepath.Join("pkg", "b.go"), "bang", true)

	message := droppedFileError().Error()

	for _, want := range []string{"2 files", "pkg/a.go (.cs): boom", "pkg/b.go (.cs.auto review sibling): bang"} {
		if !strings.Contains(message, want) {
			t.Errorf("the error names %q:\n%s", want, message)
		}
	}

	if text := (droppedFile{reason: "x", sibling: true}).text(); !strings.Contains(text, "may be incomplete") {
		t.Errorf("a sibling's diagnostic says the package-wide records may be incomplete: %q", text)
	}
}

// TestWarnCannotDowngradeADroppedFile: -warn downgrades only a code whose sites fail loud, and a dropped file does
// not, so naming it is a usage error that says why.
func TestWarnCannotDowngradeADroppedFile(t *testing.T) {
	_, err := parseWarnCodes(diagnosticFileDropped)

	if err == nil || !strings.Contains(err.Error(), "cannot downgrade "+diagnosticFileDropped) || !strings.Contains(err.Error(), "not loud") {
		t.Fatalf("-warn %s is refused with its reason; got %v", diagnosticFileDropped, err)
	}
}

// TestDroppedReviewSiblingFailsTheRun covers the two paths that convert a hand-owned file into its `.cs.auto` review
// sibling: a package with one hand-owned file (the per-file loop), and a fully hand-owned package
// (emitAutoConversionSiblings). A faulted visit there loses the sibling and may lose the package-wide records the
// visit feeds, so it fails the run too, named as a sibling.
func TestDroppedReviewSiblingFailsTheRun(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	takeDroppedFiles()
	t.Cleanup(func() { takeDroppedFiles(); visitFileFaultHook = nil })

	handOwn := func(t *testing.T, outDir string, names ...string) {
		t.Helper()

		for _, name := range names {
			writeModuleFile(t, filepath.Join(outDir, name+".cs"), "// "+name+".cs - hand-owned\n[module: go.GoManualConversion]\n\nnamespace go;\n")
		}
	}

	for _, c := range []struct {
		name     string
		handOwns []string
	}{
		{"one hand-owned file", []string{"b"}},
		{"fully hand-owned package", []string{"a", "b"}},
	} {
		t.Run(c.name, func(t *testing.T) {
			pkgDir, outDir, options := incrementalFixture(t)

			visitFileFaultHook = nil

			if err := processConversion(pkgDir, true, outDir, options); err != nil {
				t.Fatalf("first conversion: %v", err)
			}

			if err := droppedFileError(); err != nil {
				t.Fatalf("the unfaulted conversion drops nothing; got:\n%v", err)
			}

			handOwn(t, outDir, c.handOwns...)

			visitFileFaultHook = func(filePath string) {
				if filepath.Base(filePath) == "b.go" {
					panic("planted sibling fault")
				}
			}

			if err := processConversion(pkgDir, true, outDir, options); err != nil {
				t.Fatalf("faulted conversion: %v", err)
			}

			if _, err := os.Stat(filepath.Join(outDir, "b.cs.auto")); !os.IsNotExist(err) {
				t.Errorf("the faulted file's review sibling is not written (stat: %v)", err)
			}

			err := droppedFileError()

			if err == nil || !strings.Contains(err.Error(), "b.go (.cs.auto review sibling): planted sibling fault") {
				t.Fatalf("the run fails naming b.go's review sibling; got:\n%v", err)
			}
		})
	}
}
