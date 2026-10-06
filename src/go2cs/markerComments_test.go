// markerComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the member markers the converter writes (docs/PLAN-marker-comment-parity.md, 5.5, 5.6) and the
// rule that a Go comment is never one (6): testdata/markercomments/main.go carries Go comments spelled
// exactly like the markers go2cs-gen reads, beside real embeds and struct tags, and its conversion with
// -comments must carry each Go comment re-spelled (carriedComment) beside the markers the converter writes
// for the real facts: `/*embed*/`, and each tag in the spelling a comment can hold (structTagComment).
// testdata/markercomments/main.cs is that conversion, committed, and GenTests' MemberRecordGeneratorTests
// reads it, so the generator side is tested against the converter's own output: this test fails when the
// two part.

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

	if got := strings.Count(mainCs, embedMarker); got != 3 {
		t.Errorf("got %d %s, want 3 (the three real embeds; every Go comment re-spelled):\n%s", got, embedMarker, mainCs)
	}

	// Each struct tag at the end of its field's line, never as [GoTag]: backquoted where a comment can hold
	// it, quoted otherwise (a backquote, a `*/`, a control or non-printing character), on every line that
	// declares the field, ahead of a Go comment. An empty tag is Go's untagged field.
	for _, want := range []string{
		"public @string Plain; /*`json:\"plain\"`*/",
		"public nint Grouped, Pair; /*`json:\"g\"`*/",
		"public nint Mixed; /*`json:\"m\"`*/",
		"internal nint mixed; /*`json:\"m\"`*/",
		"public @string Quoted; /*\"a:\\\"`b`\\\"\"*/",
		"public @string Closer; /*\"x:\\\"*\\x2f\\\"\"*/",
		"public @string Tabbed; /*\"t:\\\"\\t\\\"\"*/",
		"public @string Separator; /*\"u:\\\"\\u2028\\\"\"*/",
		"public @string Wide; /*`json:\"ü\"`*/",
		"public @string Spaced; /*`json:\"s\"`*/ /* `json:\"other\"`*/",
		"public nint Empty;\n",
		"public partial ref Inner Inner { get; } /*`json:\"inner\"`*/",
		embedMarker + " public fmt_package.Stringer Stringer; /*`json:\"str\"`*/",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in:\n%s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "GoTag") {
		t.Errorf("converted code still carries [GoTag]:\n%s", mainCs)
	}

	// Array dims (5.4): the dims comment directly before the type on a declaration a go2cs-gen record can
	// key (a func or method parameter, a generic one's included (D2), a field, a named array or
	// pointer-to-array type), and the attribute where none can (a lambda, a local function). A map key's
	// dims are still an attribute, not in this step.
	for _, want := range []string{
		"[GoType(\"[2]array<nint>\")] /*[2][3]*/ partial struct nn;",
		"[GoType(\"ж<array<byte>>\")] /*[4]*/ partial class P;",
		"internal /*[3]*/ ж<array<nint>> p;",
		"[GoMapKeyDims(2)]\n    internal /*[3]*/ map<array<@string>, array<nint>> m;",
		"internal /*[5]*/ slice<ж<array<byte>>> s;",
		"internal static nint hash(/*[32]*/ array<byte> b) {",
		", /*[4][8]*/ array<array<byte>> grid) {",
		"internal static void put(this ref holder h, /*[3]*/ array<nint> v) {",
		"internal static nint noted(/*[4]*/ array<byte> a, nint b) {",
		"internal static T first<T>(/*[2]*/ array<T> a) {",
		"var lambda = ([GoArrayDims(32)] array<byte> x) => {",
		"nint local([GoArrayDims(3)] array<nint> x) {",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in:\n%s", want, mainCs)
		}
	}

	if got := strings.Count(mainCs, "GoArrayDims("); got != 2 {
		t.Errorf("got %d [GoArrayDims(...)], want 2 (the lambda, the local function):\n%s", got, mainCs)
	}

	// Every Go comment carried, re-spelled.
	if got, want := strings.Count(mainCs, carriedComment(embedMarker)), strings.Count(string(source), embedMarker); got != want {
		t.Errorf("got %d carried %s, want %d (one per Go comment):\n%s", got, carriedComment(embedMarker), want, mainCs)
	}

	for _, carried := range []string{"/* `json:\"other\"`*/", "/* `json:\"bare\"`*/", "/* \"json:\\\"q\\\"\"*/", "/* [9]*/", "/* [7]*/", "/* [10]*/"} {
		if !strings.Contains(mainCs, carried) {
			t.Errorf("want the Go comment carried as %s in:\n%s", carried, mainCs)
		}
	}

	if want := strings.ReplaceAll(string(committed), "\r\n", "\n"); mainCs != want {
		t.Errorf("testdata/markercomments/main.cs is not this converter's output; regenerate it (GenTests reads it). Got:\n%s", mainCs)
	}
}
