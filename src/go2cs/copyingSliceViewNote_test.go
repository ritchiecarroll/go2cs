// copyingSliceViewNote_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards copyingSliceViewNote.go: a slice of a pointer-to-array cast lowers to a COPYING span, so a
// function that writes through it loses the write, and the converter says so at conversion time,
// naming both lines. A function that only reads the view gets no note. The emission is unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAWriteThroughACopyingSliceViewIsNoted(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/sliceview\n\ngo 1.23\n")

	// Line numbers below are asserted: keep the layout.
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "unsafe"

var words [4]uint32

// writer stores through the view, as runtime's itabInit does.
func writer() {
	view := (*[16]byte)(unsafe.Pointer(&words[0]))[:8:8]
	view[3] = 7
}

// clearer clears a reslice of the view, as runtime's makeheapobjbv does.
func clearer() {
	view := (*[16]byte)(unsafe.Pointer(&words[0]))[:8:8]
	clear(view[:4])
}

// reader only reads the view: the copy is exact.
func reader() byte {
	view := (*[16]byte)(unsafe.Pointer(&words[0]))[:8:8]
	return view[3]
}

func main() {
	writer()
	clearer()
	println(reader())
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	stderr := captureStderr(t, func() {
		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule: %v", err)
		}
	})

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "sliceview", "main.cs"))

	// Control: all three functions really take the copying branch, or the test proves nothing.
	if got := strings.Count(mainCs, "new ReadOnlySpan<byte>("); got != 3 {
		t.Fatalf("control: the fixture must emit the copying branch three times, emitted %d:\n%s", got, mainCs)
	}

	var notes []string

	for _, line := range strings.Split(stderr, "\n") {
		if strings.Contains(line, "COPIES the memory it views") {
			notes = append(notes, line)
		}
	}

	for _, want := range []string{"at line 9 COPIES the memory it views, and it is written through at line 10", "at line 15 COPIES the memory it views, and it is written through at line 16"} {
		found := false

		for _, note := range notes {
			found = found || strings.Contains(note, want)
		}

		if !found {
			t.Errorf("no conversion-time note %q; stderr held:\n%s", want, stderr)
		}
	}

	if len(notes) != 2 {
		t.Errorf("exactly the two writers must be noted, and the reader not; %d notes:\n%s", len(notes), strings.Join(notes, "\n"))
	}
}
