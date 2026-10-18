// uniformLineEndings_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the emitted line endings (incrementalWrites.go, uniformLineEndings). The converter chooses CRLF wherever it
// writes a line break itself, but text it copies verbatim from Go -- a multi-line raw literal's body, a /* */ block
// comment -- arrived with Go's LF, so a fresh emission held MIXED endings. git normalizes them (the blob never changed),
// yet every checkout gives a .cs CRLF (.gitattributes: *.cs text eol=crlf), the sizes differ, and `git status` -- which
// is CNR's verdict -- reads a size change as a modification: RawStringLineEndings and X509SystemVerify read CHANGED on
// every fresh tree. This drives a real conversion and reads the emitted BYTES, never the helper alone.

package main

import (
	"bytes"
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"testing"
)

// convertLineEndingFixture converts a one-file module holding both verbatim producers and returns main.cs as written.
func convertLineEndingFixture(t *testing.T) []byte {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/lineendings\n\ngo 1.23\n")
	// The block comment sits where Go's doc.go files put theirs (fmt, text/template, runtime/metrics): the package doc.
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "/*\nA block comment the converter copies verbatim,\nacross three lines.\n*/\n"+
		"package main\n\n"+
		"import \"fmt\"\n\n"+
		"var pem = `-----BEGIN TEST-----\nQUJDREVGR0g=\n-----END TEST-----\n`\n\n"+
		"func main() {\n\tfmt.Print(pem)\n}\n")

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

	content, err := os.ReadFile(filepath.Join(options.go2csPath, "src", "example.com", "lineendings", "main.cs"))

	if err != nil {
		t.Fatalf("reading the emitted main.cs: %v", err)
	}

	return content
}

// bareLineFeeds returns the 1-based line numbers whose terminator is a LF with no CR before it.
func bareLineFeeds(content []byte) []int {
	var lines []int
	line := 1

	for i, b := range content {
		if b != '\n' {
			continue
		}

		if i == 0 || content[i-1] != '\r' {
			lines = append(lines, line)
		}

		line++
	}

	return lines
}

func TestEmittedSourceHasUniformLineEndings(t *testing.T) {
	isolateConverterPathGlobals(t)
	content := convertLineEndingFixture(t)

	// The fixture must reach both producers, or a clean reading proves nothing.
	for _, want := range []string{`"""`, "QUJDREVGR0g=", ".Raw()", "across three lines."} {
		if !bytes.Contains(content, []byte(want)) {
			t.Fatalf("the emission does not hold %q, so the fixture no longer reaches the producer it guards:\n%s", want, content)
		}
	}

	if !bytes.Contains(content, []byte("\r\n")) {
		t.Fatalf("the emission holds no CRLF at all; the converter's own line ending changed under this guard:\n%s", content)
	}

	if bare := bareLineFeeds(content); len(bare) > 0 {
		t.Errorf("the emitted main.cs ends %d line(s) with a bare LF (lines %v): a checkout gives a .cs CRLF, so a fresh "+
			"tree's `git status` reads the file as changed although git's normalized content is identical", len(bare), bare)
	}
}
