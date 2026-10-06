// markerComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the rule that a Go comment is never a member marker (docs/PLAN-marker-comment-parity.md, 6):
// testdata/markercomments/main.go carries Go comments spelled exactly like the markers go2cs-gen reads,
// and its conversion with -comments must carry each one re-spelled (carriedComment) beside the markers the
// converter writes for the real facts. testdata/markercomments/main.cs is that conversion, committed, and
// GenTests' MemberRecordGeneratorTests reads it, so the generator side is tested against the converter's
// own output: this test fails when the two part.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestGoCommentsAreNeverMarkers(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	source, err := os.ReadFile(filepath.Join("testdata", "markercomments", "main.go"))

	if err != nil {
		t.Fatalf("read fixture: %v", err)
	}

	committed, err := os.ReadFile(filepath.Join("testdata", "markercomments", "main.cs"))

	if err != nil {
		t.Fatalf("read committed conversion: %v", err)
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/mkc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), string(source))

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
		includeComments:     true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "mkc", "main.cs")), "\r\n", "\n")

	// The markers the converter writes, one per real fact, each where the reader takes it.
	for _, want := range []string{
		embedMarker + " public Reader Reader;",
		embedMarker + " internal nint @int;",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in:\n%s", want, mainCs)
		}
	}

	if got := strings.Count(mainCs, embedMarker); got != 2 {
		t.Errorf("got %d %s, want 2 (the two real embeds; every Go comment re-spelled):\n%s", got, embedMarker, mainCs)
	}

	// Every Go comment carried, re-spelled.
	if got, want := strings.Count(mainCs, carriedComment(embedMarker)), strings.Count(string(source), embedMarker); got != want {
		t.Errorf("got %d carried %s, want %d (one per Go comment):\n%s", got, carriedComment(embedMarker), want, mainCs)
	}

	if want := strings.ReplaceAll(string(committed), "\r\n", "\n"); mainCs != want {
		t.Errorf("testdata/markercomments/main.cs is not this converter's output; regenerate it (GenTests reads it). Got:\n%s", mainCs)
	}
}
