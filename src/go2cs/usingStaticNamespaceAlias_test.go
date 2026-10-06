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
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

func convertUsingStaticFixture(t *testing.T) (dotted, dottedLater, plain string) {
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

// launch above is a partial method's implementing part, which names no attribute; this literal executes
// a go too and, unable to be partial, writes [MethodImpl(MethodImplOptions.NoInlining)] itself.
func launcher(done chan string) func() {
	return func() { go func() { done <- Unsafe }() }
}

func total(c counter) int { return Marshal(int(c.v)) }
`)

	// POSITIVE, the dot-import BEHIND an ordinary import: the shape of both real sites (go/types'
	// stdlib_test.go and its errors.go), where the import block's first using is an alias, not the
	// using static. dotted.go above keeps the dot-import first, so both spellings are held.
	writeModuleFile(t, filepath.Join(appDir, "dottedlater.go"), `package main

import (
	"strings"
	. "example.com/usingstatic/lib"
)

func launchUpper(done chan string) {
	go func() { done <- strings.ToUpper(Unsafe) }()
}
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

	return readGenerated(t, filepath.Join(out, "dotted.cs")), readGenerated(t, filepath.Join(out, "dottedlater.cs")), readGenerated(t, filepath.Join(out, "main.cs"))
}

func TestAFileWithAUsingStaticImportsNoDotNetNamespace(t *testing.T) {
	dotted, dottedLater, plain := convertUsingStaticFixture(t)

	const (
		compilerServices = "using System.Runtime.CompilerServices;"
		interopServices  = "using System.Runtime.InteropServices;"
		explicitLayout   = "[StructLayout(LayoutKind.Explicit, Size = 4)]"
		fieldOffset      = "[FieldOffset(0)]"
	)

	// Controls: both files demand both namespaces, and the dotted one holds a using static.
	for name, text := range map[string]string{"dotted.cs": dotted, "main.cs": plain} {
		if !fileKeepsAFrame(text) {
			t.Fatalf("control: %s must carry the no-inline mark, or this test proves nothing:\n%s", name, text)
		}

		for _, emitted := range []string{explicitLayout, fieldOffset} {
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

	// The dot-import behind an ordinary import: its using static is not the block's first line.
	if !fileKeepsAFrame(dottedLater) || strings.HasPrefix(strings.TrimSpace(dottedLater[strings.Index(dottedLater, "using "):]), "using static ") {
		t.Fatalf("control: dottedlater.cs must carry the no-inline mark and open its usings with something other than the using static:\n%s", dottedLater)
	}

	if strings.Contains(dottedLater, compilerServices) || !strings.Contains(dottedLater, "using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;") {
		t.Errorf("dottedlater.cs holds a using static behind an ordinary import and must take the aliases, not the namespace:\n%s", dottedLater)
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

// The test-project trigger. A converted test project whose production is a REFERENCED assembly
// carries `global using static <production class>` (and the white-box bridge) in its seeded
// metadata, project-wide. That directive sits at compilation-unit level, OUTSIDE a file's
// file-scoped namespace, so a namespace using INSIDE the namespace does not collide with it: it
// silently WINS, and a bare Go name the namespace also declares binds the .NET type (measured on
// the union: an external reference-model test file bound System.Runtime.CompilerServices.Unsafe,
// not the Go member). So every file of such a test project takes the aliases, the internal file
// (which also holds its own file-level using static) and the external one (which holds none).
func TestEveryFileOfAReferenceModelTestProjectBindsByAlias(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads a module fixture through go/packages")
	}

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":    "module example/aliasvariant\n\ngo 1.23\n",
		"lib.go":    "package aliasvariant\n\nvar Unsafe = \"unsafe\"\n\nfunc Name() string { return Unsafe }\n",
		"inner_test.go": "package aliasvariant\n\n" +
			"func innerProbe(done chan string) {\n\tgo func() { done <- Unsafe }()\n}\n",
		"outer_test.go": "package aliasvariant_test\n\nimport \"example/aliasvariant\"\n\n" +
			"func outerProbe(done chan string) {\n\tgo func() { done <- aliasvariant.Name() }()\n}\n",
	})

	internal, external := loadBothTestVariantsForDir(t, dir)

	if internal == nil || external == nil {
		t.Fatal("both test variants must load")
	}

	outputPath := t.TempDir()
	bridgeName := getSanitizedImport("aliasvariant_internal_test" + PackageSuffix)

	// The white-box reference model's own option set, as convertTestVariants builds it.
	base := Options{
		indentSpaces:           4,
		preferVarDecl:          true,
		useChannelOperators:    true,
		testProductionPath:     "example/aliasvariant",
		testProductionName:     "aliasvariant",
		testMetadataAnchorName: bridgeName,
		testWhiteboxReference:  true,
		testInternalBridgeName: bridgeName,
	}

	testMethodRenames = make(map[types.Object]bool)
	testTypeRenames = make(map[types.Object]bool)
	whiteboxInternalTestObjects = collectWhiteboxInternalTestObjects(internal)
	whiteboxBridgeDeclaredNames = collectWhiteboxBridgeDeclaredNames(internal)
	whiteboxBridgeTypeNames = collectWhiteboxBridgeTypeNames(internal)

	t.Cleanup(func() {
		testMethodRenames = nil
		testTypeRenames = nil
		whiteboxInternalTestObjects = nil
		whiteboxBridgeDeclaredNames = hashset.HashSet[string]{}
		whiteboxBridgeTypeNames = hashset.HashSet[string]{}
	})

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, testVariantOptions(base, testProjectWhiteboxReference, false, bridgeName)); err != nil {
		t.Fatal(err)
	}

	if _, _, err := convertTestVariant(external, testFileEntries(external), outputPath, "go", productionSeed{}, testVariantOptions(base, testProjectWhiteboxReference, true, bridgeName)); err != nil {
		t.Fatal(err)
	}

	const (
		compilerServices = "using System.Runtime.CompilerServices;"
		optionsAlias     = "using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;"
	)

	for _, name := range []string{"inner_test.cs", "outer_test.cs"} {
		cs := readConvertedTestFile(t, outputPath, name)

		// Control: each file executes a go, so its frame is kept and the namespace is demanded.
		if !fileKeepsAFrame(cs) {
			t.Fatalf("control: %s must carry the no-inline mark, or this test proves nothing:\n%s", name, cs)
		}

		if strings.Contains(cs, compilerServices) || !strings.Contains(cs, optionsAlias) {
			t.Errorf("%s is compiled into a test project that imports its production class project-wide, and must take the aliases, not the namespace:\n%s", name, cs)
		}
	}
}

// The plain REFERENCE model (no internal test file, so no bridge and no file-level using static at
// all): the external file's only using static is the seeded `global using static`, so only the
// project-level trigger can reach it. This is the shape COORD's probe measured binding the .NET
// Unsafe on the union.
func TestAnExternalOnlyTestFileBindsByAlias(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads a module fixture through go/packages")
	}

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/aliasexternal\n\ngo 1.23\n",
		"lib.go": "package aliasexternal\n\nvar Unsafe = \"unsafe\"\n\nfunc Name() string { return Unsafe }\n",
		"outer_test.go": "package aliasexternal_test\n\nimport \"example/aliasexternal\"\n\n" +
			"func outerProbe(done chan string) {\n\tgo func() { done <- aliasexternal.Name() }()\n}\n",
	})

	_, external := loadBothTestVariantsForDir(t, dir)

	if external == nil {
		t.Fatal("the external test variant must load")
	}

	outputPath := t.TempDir()

	// The reference model's option set, as processTestConversion builds it for a package whose
	// tests are all external: production referenced, no white-box bridge.
	base := Options{
		indentSpaces:           4,
		preferVarDecl:          true,
		useChannelOperators:    true,
		testProductionPath:     "example/aliasexternal",
		testProductionName:     "aliasexternal",
		testMetadataAnchorName: getSanitizedImport("aliasexternal_test" + PackageSuffix),
	}

	testMethodRenames = make(map[types.Object]bool)
	testTypeRenames = make(map[types.Object]bool)

	t.Cleanup(func() {
		testMethodRenames = nil
		testTypeRenames = nil
	})

	if _, _, err := convertTestVariant(external, testFileEntries(external), outputPath, "go", productionSeed{}, testVariantOptions(base, testProjectReference, true, "")); err != nil {
		t.Fatal(err)
	}

	cs := readConvertedTestFile(t, outputPath, "outer_test.cs")

	if !fileKeepsAFrame(cs) {
		t.Fatalf("control: outer_test.cs executes a go and must keep its frame, or this test proves nothing:\n%s", cs)
	}

	if strings.Contains(cs, "using static ") {
		t.Fatalf("control: outer_test.cs must hold no file-level using static, so only the project-level trigger can reach it:\n%s", cs)
	}

	if strings.Contains(cs, "using System.Runtime.CompilerServices;") || !strings.Contains(cs, "using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;") {
		t.Errorf("outer_test.cs is compiled under its project's global using static of the production class, and must take the aliases, not the namespace:\n%s", cs)
	}
}

// fileKeepsAFrame reports whether a converted file carries the no-inline mark in either form: a func
// literal keeps the [MethodImpl(MethodImplOptions.NoInlining)] attribute itself, and a declared function
// is a partial method's implementing part whose declaring part go2cs-gen writes (keepsOwnFrame). The
// converter demands the attribute's namespace for both.
func fileKeepsAFrame(cs string) bool {
	if strings.Contains(cs, "[MethodImpl(MethodImplOptions.NoInlining)]") {
		return true
	}

	for _, line := range strings.Split(cs, "\n") {
		if keepsOwnFrame(line) {
			return true
		}
	}

	return false
}
