// noInliningPartialUsing_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Visual-parity seat B of the next converter train (owner, 2026-11-04, seen in the Tour's Channels lesson): a file
// imports System.Runtime.CompilerServices only when it writes the [MethodImpl(MethodImplOptions.NoInlining)] text.
// Since 42ce0e34f8 a bodied function that needs the no-inline mark is a partial method's implementing part, and the
// mark rides go2cs-gen's generated declaring part, so the visible file names no attribute; registering the using for
// it anyway left an unused `using System.Runtime.CompilerServices;` in every such file.
//
// carrier.go's only no-inline function is a partial carrier: its file loses the using. literal.go holds a func
// literal that executes a go, which cannot be partial and writes the attribute itself: its file keeps the using.

func convertNoInliningUsingFixture(t *testing.T) (carrier, literal string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/noinliningusing\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "carrier.go"), `package main

func say(s string, done chan bool) {
	println(s)
	done <- true
}

func main() {
	done := make(chan bool)
	go say("hello", done)
	<-done
	launcher(done)()
	<-done
}
`)

	writeModuleFile(t, filepath.Join(appDir, "literal.go"), `package main

func launcher(done chan bool) func() {
	return func() { go say("world", done) }
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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	out := filepath.Join(options.go2csPath, "src", "example.com", "noinliningusing")

	return readGenerated(t, filepath.Join(out, "carrier.cs")), readGenerated(t, filepath.Join(out, "literal.cs"))
}

func TestCompilerServicesUsingOnlyWhereTheAttributeIsWritten(t *testing.T) {
	isolateConverterPathGlobals(t)
	carrier, literal := convertNoInliningUsingFixture(t)

	const (
		compilerServices = "using System.Runtime.CompilerServices;"
		attribute        = "[MethodImpl(MethodImplOptions.NoInlining)]"
	)

	// Controls: main is a partial carrier (it executes a go), and the literal writes the attribute itself.
	if !strings.Contains(carrier, "static partial void Main(") || strings.Contains(carrier, attribute) {
		t.Fatalf("control: carrier.cs must declare main as a partial carrier with no attribute text, or this test proves nothing:\n%s", carrier)
	}

	if !strings.Contains(literal, attribute) {
		t.Fatalf("control: literal.cs must write the attribute on its func literal, or this test proves nothing:\n%s", literal)
	}

	if strings.Contains(carrier, compilerServices) {
		t.Errorf("carrier.cs writes no MethodImpl attribute and still imports System.Runtime.CompilerServices:\n%s", carrier)
	}

	if !strings.Contains(literal, compilerServices) {
		t.Errorf("literal.cs writes the MethodImpl attribute and must import System.Runtime.CompilerServices:\n%s", literal)
	}
}
