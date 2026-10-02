// usingStaticNamespaceAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the rule in visitFile.go: a file that reaches Go names BARE through a `using static` never
// imports a whole .NET namespace. A `using static` member and a namespace-imported type are two
// imports at one level, so a Go name the namespace also declares is CS0229 -- measured 2026-10-02 on
// go/types' stdlib_test.go, whose bare `Unsafe` (go/types' package var, through the test's dot-import)
// met System.Runtime.CompilerServices.Unsafe the moment TestStdlib took [MethodImpl(NoInlining)] for
// executing a `go`. Such a file takes alias directives for exactly the types the emission names; a
// file with no `using static` keeps the namespace import and does not move.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertUsingStaticFixture(t *testing.T) (dotted, plain string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/usingstatic\n\ngo 1.23\n")

	// The names two .NET namespaces the converter imports on demand also declare as types.
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

var Unsafe = "unsafe"

func Marshal(n int) int { return n + 1 }
`)

	// POSITIVE -- a dot-import, a function that executes a go (NoInlining), and a struct with a
	// zero-size field (explicit layout): both namespaces are demanded beside a using static.
	writeModuleFile(t, filepath.Join(appDir, "dotted.go"), `package main

import . "example.com/usingstatic/lib"

type noCopy struct{}

type counter struct {
	_ noCopy
	v int32
}

func launch(done chan string) {
	go func() { done <- Unsafe }()
}

func total(c counter) int { return Marshal(int(c.v)) }
`)

	// CONTROL -- the same two demands in a file with no using static.
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type gauge struct {
	_ noCopy
	v int32
}

func main() {
	done := make(chan string)
	go launch(done)
	println(<-done, total(counter{v: 1}), gauge{v: 2}.v)
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

	out := filepath.Join(options.go2csPath, "src", "example.com", "usingstatic")

	return readGenerated(t, filepath.Join(out, "dotted.cs")), readGenerated(t, filepath.Join(out, "main.cs"))
}

func TestAFileWithAUsingStaticImportsNoDotNetNamespace(t *testing.T) {
	dotted, plain := convertUsingStaticFixture(t)

	const (
		compilerServices = "using System.Runtime.CompilerServices;"
		interopServices  = "using System.Runtime.InteropServices;"
		noInlining       = "[MethodImpl(MethodImplOptions.NoInlining)]"
		explicitLayout   = "[StructLayout(LayoutKind.Explicit, Size = 4)]"
		fieldOffset      = "[FieldOffset(0)]"
	)

	// Controls: both files demand both namespaces, and the dotted one holds a using static.
	for name, text := range map[string]string{"dotted.cs": dotted, "main.cs": plain} {
		for _, emitted := range []string{noInlining, explicitLayout, fieldOffset} {
			if !strings.Contains(text, emitted) {
				t.Fatalf("control: %s must emit %s, or this test proves nothing:\n%s", name, emitted, text)
			}
		}
	}

	if !strings.Contains(dotted, "using static ") {
		t.Fatalf("control: dotted.cs must hold a using static:\n%s", dotted)
	}

	if strings.Contains(plain, "using static ") {
		t.Fatalf("control: main.cs must hold no using static:\n%s", plain)
	}

	for _, namespace := range []string{compilerServices, interopServices} {
		if strings.Contains(dotted, namespace) {
			t.Errorf("dotted.cs reaches Go names through a using static and still imports a whole namespace (%s), so a Go name that namespace declares is CS0229", namespace)
		}

		if !strings.Contains(plain, namespace) {
			t.Errorf("main.cs holds no using static and must keep %s unchanged", namespace)
		}
	}

	for _, alias := range []string{
		"using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;",
		"using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;",
		"using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;",
		"using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;",
		"using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;",
	} {
		if !strings.Contains(dotted, alias) {
			t.Errorf("dotted.cs must bind exactly the types its emission names; missing: %s", alias)
		}

		if strings.Contains(plain, alias) {
			t.Errorf("main.cs holds no using static and must not move to aliases: %s", alias)
		}
	}

	// Every type name the alias table can bind is one the emission really names -- a table entry
	// nothing emits would be an unused alias in every such file.
	for namespace, typeNames := range systemNamespaceTypes {
		for _, typeName := range typeNames {
			if used := strings.TrimSuffix(typeName, "Attribute"); !strings.Contains(dotted, "["+used+"(") && !strings.Contains(dotted, used+".") {
				t.Errorf("%s.%s is aliased but nothing in the fixture's emission names it", namespace, typeName)
			}
		}
	}
}
