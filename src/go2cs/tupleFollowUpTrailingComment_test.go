// tupleFollowUpTrailingComment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards where a Go end-of-line comment lands when a tuple assignment emits follow-up lines. A tuple
// element whose target is a local of another func type is received into a temporary and converted on
// the next line (`release = NilSafeDelegateConversion<…>(releaseᴛ1);`). The comment describes the call,
// so it stays on the deconstruction line; written after the whole emission it trailed the conversion
// instead (database/sql's `grabConn`, "blocks, waiting for the connection").
//
// Control: the same tuple with no comment emits the same two lines.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestTupleFollowUpKeepsTheCommentOnTheCallLine(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tuplecomment\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type releaseFunc func(error)

func grab() (int, releaseFunc, error) { return 1, func(error) {}, nil }

func main() {
	var n int
	var release func(error)
	var err error

	n, release, err = grab() // blocks, waiting for the connection.
	release(err)

	n, release, err = grab()
	release(err)

	println(n)
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
		includeComments:     true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	output := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tuplecomment", "main.cs"))

	const conversion = "release = builtin.NilSafeDelegateConversion<Action<error>, releaseFunc>(releaseᴛ"

	var calls, conversions []string

	for _, line := range strings.Split(output, "\n") {
		line = strings.TrimSpace(line)

		switch {
		case strings.Contains(line, "= grab();"):
			calls = append(calls, line)
		case strings.HasPrefix(line, conversion):
			conversions = append(conversions, line)
		}
	}

	if len(calls) != 2 || len(conversions) != 2 {
		t.Fatalf("expected two tuple calls each followed by its conversion; got calls %q, conversions %q:\n%s", calls, conversions, output)
	}

	if !strings.HasSuffix(calls[0], "= grab(); // blocks, waiting for the connection.") {
		t.Errorf("the Go comment must trail the call it describes:\n got: %s", calls[0])
	}

	for i, line := range conversions {
		if strings.Contains(line, "//") {
			t.Errorf("conversion %d carries a comment that belongs to its call:\n got: %s", i+1, line)
		}
	}

	if strings.Contains(calls[1], "//") {
		t.Errorf("the control call has no Go comment and must emit none:\n got: %s", calls[1])
	}
}
