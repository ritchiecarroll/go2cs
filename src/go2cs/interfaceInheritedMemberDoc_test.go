// interfaceInheritedMemberDoc_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards seat #43 (class 2 of the comment-parity residue): the doc of an interface member that the C# interface
// INHERITS rather than declares is still written inside the interface body, at the member's Go position. Two shapes
// lost it: a declared method a structural base covers (net.Conn's Read/Write/Close, inherited from
// io.ReadWriteCloser, so the member loop skipped them, docs included), and Go's own embed (hash.Hash's `io.Writer`,
// moved to the C# base list with its doc dropped).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestInterfaceInheritedMemberDocStaysInTheBody(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/iimd\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "io"

// Conn is a generic stream-oriented network connection.
type Conn interface {
	// Read reads data from the connection.
	// Read can be made to time out.
	Read(b []byte) (n int, err error)

	// Write writes data to the connection.
	Write(b []byte) (n int, err error)

	// Close closes the connection.
	Close() error

	// LocalAddr returns the local network address, if known.
	LocalAddr() string
}

// Hash is the common interface implemented by all hash functions.
type Hash interface {
	// Write (via the embedded io.Writer interface) adds more data to the running hash.
	// It never returns an error.
	io.Writer

	// Sum appends the current hash to b and returns the resulting slice.
	Sum(b []byte) []byte
}

func main() {
	var c Conn
	var h Hash
	_, _ = c, h
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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "iimd", "main.cs")), "\r\n", "\n")

	// body returns the lines between the interface's declaration and its closing brace.
	body := func(name string) []string {
		lines := strings.Split(mainCs, "\n")

		for i, line := range lines {
			if !strings.Contains(line, "partial interface "+name) {
				continue
			}

			for j := i; j < len(lines); j++ {
				if strings.TrimSpace(lines[j]) == "{" {
					for k := j + 1; k < len(lines); k++ {
						if strings.TrimSpace(lines[k]) == "}" {
							return lines[j+1 : k]
						}
					}
				}
			}
		}

		t.Fatalf("control: no complete body for interface %q:\n%s", name, mainCs)
		return nil
	}

	// Each interface's member docs, in Go order. Each must appear once, inside the body, after the one before it.
	for _, c := range []struct {
		iface string
		docs  []string
	}{
		{"Conn", []string{
			"// Read reads data from the connection.",
			"// Read can be made to time out.",
			"// Write writes data to the connection.",
			"// Close closes the connection.",
			"// LocalAddr returns the local network address, if known.",
		}},
		{"Hash", []string{
			"// Write (via the embedded io.Writer interface) adds more data to the running hash.",
			"// It never returns an error.",
			"// Sum appends the current hash to b and returns the resulting slice.",
		}},
	} {
		lines := body(c.iface)
		at := -1

		for _, doc := range c.docs {
			found := -1

			for i, line := range lines {
				if strings.TrimSpace(line) == doc {
					if found >= 0 {
						t.Errorf("%s: %q appears more than once in the body:\n%s", c.iface, doc, strings.Join(lines, "\n"))
					}

					found = i
				}
			}

			switch {
			case found < 0:
				t.Errorf("%s: the member doc %q is not in the interface body:\n%s", c.iface, doc, strings.Join(lines, "\n"))
			case found < at:
				t.Errorf("%s: the member doc %q is out of Go order:\n%s", c.iface, doc, strings.Join(lines, "\n"))
			default:
				at = found
			}
		}
	}

	// Controls: the inheritance itself is unchanged — Conn takes io.ReadWriteCloser, Hash takes io.Writer, and
	// neither re-declares an inherited member.
	for _, want := range []string{"io.ReadWriteCloser", "io.Writer"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: the base %q is missing:\n%s", want, mainCs)
		}
	}

	for _, member := range []string{" Read(", " Write(", " Close("} {
		for _, line := range body("Conn") {
			if strings.Contains(line, member) && !strings.HasPrefix(strings.TrimSpace(line), "//") {
				t.Errorf("control: Conn re-declares the inherited member %q:\n  %s", member, line)
			}
		}
	}
}
