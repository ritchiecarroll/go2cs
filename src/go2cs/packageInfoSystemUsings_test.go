// packageInfoSystemUsings_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"go/types"
	"strings"
	"testing"
)

// The name list is read from the .NET reference assemblies (glyphSystemNames.go). These members pin that it holds the
// three namespaces' types, an attribute under both of its names, and nothing from elsewhere.
func TestSystemNameListHoldsTheThreeNamespacesTypes(t *testing.T) {
	for _, name := range []string{
		"Process", "Debug", "StackTrace", "StackTraceHidden", "DebuggerStepThrough", // System.Diagnostics
		"Unsafe", "MethodImplOptions", "MethodImplAttribute", "MethodImpl", // System.Runtime.CompilerServices
		"EditorBrowsableState", "EditorBrowsableAttribute", "EditorBrowsable", "Component", // System.ComponentModel
	} {
		if !glyphSystemTypeNames[name] {
			t.Errorf("glyphSystemTypeNames does not hold %s", name)
		}
	}

	for _, name := range []string{"Reader", "Span", "String", "Stopwatch2", "Attribute"} {
		if glyphSystemTypeNames[name] {
			t.Errorf("glyphSystemTypeNames holds %s, which none of the three namespaces declares", name)
		}
	}

	if got := len(glyphSystemTypeNames); got != 543 {
		t.Errorf("glyphSystemTypeNames holds %d names, its provenance comment says 543", got)
	}
}

const systemUsingsHolderSource = `package app

type Box struct{ n int }

func (b Box) Get() int { return b.n }

func (b *Box) Bump() { b.n++ }
`

const systemUsingsClashSource = `package app

type MethodImpl struct{ n int }

func (m MethodImpl) Get() int { return m.n }

func (m *MethodImpl) Bump() { m.n++ }

type StackTrace int

func Make() MethodImpl { return MethodImpl{n: 1} }
`

// A package's package_info.cs spells its holders' and forwarders' System attributes through using directives, after
// the template's own; a package that declares a type the usings would make ambiguous (CS0104) keeps the global::
// spelling for the whole file, carries none of the usings, and says so on stderr.
func TestAPackageDeclaringASystemTypeNameKeepsTheGlobalSpelling(t *testing.T) {
	sibling := convertMethodHolderFixture(t, map[string]string{"app.go": systemUsingsHolderSource})["package_info.cs"]

	requireEmitted(t, "package_info.cs", sibling, []string{
		"using go;\nusing static go.example.com.app_package;\nusing System.ComponentModel;\nusing System.Diagnostics;\nusing System.Runtime.CompilerServices;\n\n",
		"[GoMethodHost(typeof(app_package.Box)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class appᴛBox;\n",
		"    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Get(this Box b) => app_package.Get⁔(b);\n",
	}, []string{"global::System.ComponentModel", "global::System.Diagnostics", "global::System.Runtime.CompilerServices"})

	var clash string

	stderr := captureStderr(t, func() {
		clash = convertMethodHolderFixture(t, map[string]string{"app.go": systemUsingsClashSource})["package_info.cs"]
	})

	requireEmitted(t, "package_info.cs", clash, []string{
		"using go;\nusing static go.example.com.app_package;\n\n",
		"[GoMethodHost(typeof(app_package.MethodImpl)), " + methodHolderAttributes + "] public static partial class appᴛMethodImpl;\n",
		"    [" + methodForwarderAttributes + "] public static nint Get(this MethodImpl m) => app_package.Get⁔(m);\n",
	}, []string{"using System.", methodHolderAttributesShort, methodForwarderAttributesShort})

	if want := "package_info.cs of example.com/app keeps the global:: spelling of its System attributes: the package declares MethodImpl, StackTrace\n"; !strings.Contains(stderr, want) {
		t.Errorf("stderr does not hold %q:\n%s", want, stderr)
	}
}

// The check counts a name the attribute arguments spell in an expression even where it is no type, and a method of
// that name, whose forwarder the holder's attributes would find before any using.
func TestSystemUsingClashesCountTheAttributeArgumentsEnumsAsAnyMember(t *testing.T) {
	pkg := loadTypesFixture(t, `package app

const MethodImplOptions = 1

type Debug struct{}

func (Debug) EditorBrowsableState() int { return 0 }

func Process() {}

var Unsafe = 2
`)

	if got, want := strings.Join(systemUsingClashes(pkg), ", "), "Debug, Debug.EditorBrowsableState, MethodImplOptions"; got != want {
		t.Errorf("systemUsingClashes = %q, want %q (a function or variable named like a System type is no clash)", got, want)
	}
}

// A merging write finds an earlier write's lines in either spelling: the pass converts them both ways, adds or drops
// the usings with them, and is idempotent.
func TestMergingWriteRespellsSystemAttributesInBothDirections(t *testing.T) {
	saved := packageInfoSystemClashes
	t.Cleanup(func() { packageInfoSystemClashes = saved })

	shortForm := []string{
		"using go;",
		"using static go.app_package;",
		"using System.ComponentModel;",
		"using System.Diagnostics;",
		"using System.Runtime.CompilerServices;",
		"",
		"namespace go;",
		"// <MethodHolders>",
		"[GoMethodHost(typeof(app_package.Box)), " + methodHolderAttributesShort + "] public static partial class appᴛBox;",
		"// </MethodHolders>",
		"// <MethodForwarders>",
		"partial class appᴛBox {",
		"    [" + methodForwarderAttributesShort + "] public static nint Get(this Box b) => app_package.Get⁔(b);",
		"}",
		"// </MethodForwarders>",
	}

	globalForm := []string{
		"using go;",
		"using static go.app_package;",
		"",
		"namespace go;",
		"// <MethodHolders>",
		"[GoMethodHost(typeof(app_package.Box)), " + methodHolderAttributes + "] public static partial class appᴛBox;",
		"// </MethodHolders>",
		"// <MethodForwarders>",
		"partial class appᴛBox {",
		"    [" + methodForwarderAttributes + "] public static nint Get(this Box b) => app_package.Get⁔(b);",
		"}",
		"// </MethodForwarders>",
	}

	respell := func(clashes []string, lines []string) string {
		packageInfoSystemClashes = clashes
		return strings.Join(applyPackageInfoSystemUsings(append([]string(nil), lines...)), "\n")
	}

	captureStderr(t, func() {
		if got, want := respell([]string{"MethodImpl"}, shortForm), strings.Join(globalForm, "\n"); got != want {
			t.Errorf("short to global::\n got:\n%s\nwant:\n%s", got, want)
		}

		if got, want := respell([]string{"MethodImpl"}, globalForm), strings.Join(globalForm, "\n"); got != want {
			t.Errorf("global:: kept\n got:\n%s\nwant:\n%s", got, want)
		}
	})

	if got, want := respell(nil, globalForm), strings.Join(shortForm, "\n"); got != want {
		t.Errorf("global:: to short\n got:\n%s\nwant:\n%s", got, want)
	}

	if got, want := respell(nil, shortForm), strings.Join(shortForm, "\n"); got != want {
		t.Errorf("short kept\n got:\n%s\nwant:\n%s", got, want)
	}
}

// A forwarder an earlier write spelled short is the same forwarder as this write's global:: rendering of it, so a
// merging write keeps one of them.
func TestMergingWriteKeepsOneForwarderWhateverItsEarlierSpelling(t *testing.T) {
	savedHolders, savedForwarders := packageMethodHolders, packageMethodForwarders
	t.Cleanup(func() { packageMethodHolders, packageMethodForwarders = savedHolders, savedForwarders })

	line := "public static nint Get(this Box b) => app_package.Get⁔(b);"
	packageMethodHolders = map[string]string{"appᴛBox": "app_package.Box"}
	packageMethodForwarders = []methodForwarder{{holder: "appᴛBox", key: "a", line: "[" + methodForwarderAttributes + "] " + line}}

	merged := applyMethodForwarderSection([]string{
		"namespace go;",
		"// <MethodForwarders>",
		"partial class appᴛBox {",
		"    [" + methodForwarderAttributesShort + "] " + line,
		"}",
		"// </MethodForwarders>",
	}, true)

	if got := strings.Count(strings.Join(merged, "\n"), line); got != 1 {
		t.Errorf("merged section holds the forwarder %d times:\n%s", got, strings.Join(merged, "\n"))
	}
}

// loadTypesFixture type-checks one Go file as package example.com/app.
func loadTypesFixture(t *testing.T, source string) *types.Package {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "app.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{}).Check("example.com/app", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	return pkg
}
