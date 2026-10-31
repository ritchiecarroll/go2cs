// staticReadAccessor_test.go - Gbtc
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

// Seat #53, part 1 (docs/phase4/DESIGN-jit-inlining-tiering-publish.md s.2): the JIT refuses to inline any method that
// copies a struct-typed static out of its field (`ldsfld` of a value class). Every struct-typed `static readonly` the
// converter writes is therefore read through a same-named `ref readonly` accessor over a private backing field (marker
// ᵇ), which the JIT inlines as an address. The accessor carries the declared name, so no use site moves.
const staticReadAccessorFixture = `package sra

import "fmt"

// Version is the package's version.
const Version = "1.0" // the release

type curveID string

const p224 curveID = "P-224"

// Two129 is beyond int64 and uint64.
const Two129 = 1 << 129

func FormatBool(b bool) string {
	if b {
		return "true"
	}
	return "false"
}

func atoi() string {
	const fnAtoi = "Atoi"
	return fnAtoi
}

func bigLocal(x float64) bool {
	const huge = 1 << 80
	return x > huge
}

func boxed() { fmt.Println("boxed only") }

func use() string { return Version + string(p224) }
`

func TestStructStaticsReadThroughSameNamedRefReadonlyAccessor(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/sra\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "sra.go"), staticReadAccessorFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		includeComments:     true,
		hoistLiterals:       true, // the standard library's mode, so the hoisted-literal and hoisted-const writers run
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	pkgDir := filepath.Join(options.go2csPath, "src", "example.com", "sra")
	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(pkgDir, "sra.cs")), "\r\n", "\n")

	// Each writer: the accessor under the declared name, then the private backing field with the value.
	for _, want := range []string{
		// hoisted @string literals (hoistedLiteralOperations.go, writeHoistedLiteralDecls)
		"private static ref readonly @string trueˢ => ref trueˢᵇ;\nprivate static readonly @string trueˢᵇ = \"true\"u8;",
		"private static ref readonly @string falseˢ => ref falseˢᵇ;\nprivate static readonly @string falseˢᵇ = \"false\"u8;",
		// a hoisted function-local string const (arm C)
		"private static ref readonly @string fnAtoiᶜ => ref fnAtoiᶜᵇ;\nprivate static readonly @string fnAtoiᶜᵇ = \"Atoi\"u8;",
		// a package-level string const keeps its access, its doc above the accessor and its trailing comment on the value
		"public static ref readonly @string Version => ref Versionᵇ;\nprivate static readonly @string Versionᵇ = \"1.0\"u8; // the release",
		// a named string type
		"internal static ref readonly curveID p224 => ref p224ᵇ;\nprivate static readonly curveID p224ᵇ = \"P-224\"u8;",
		// a package-level big constant
		"public static ref readonly GoBigConst Two129 => ref Two129ᵇ;\nprivate static readonly GoBigConst Two129ᵇ = /* 1 << 129 */",
		// a hoisted function-local big constant
		"private static ref readonly GoBigConst hugeᶜ => ref hugeᶜᵇ;\nprivate static readonly GoBigConst hugeᶜᵇ = GoBigConst.Parse(\"1208925819614629174706176\");",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the accessor and its backing field:\n%s\n-- in --\n%s", want, emitted)
		}
	}

	// The doc comment stays on the declared (public) name.
	if !strings.Contains(emitted, "Version is the package's version.") ||
		strings.Index(emitted, "Version is the package's version.") > strings.Index(emitted, "public static ref readonly @string Version") {
		t.Errorf("Version's doc comment must precede its accessor:\n%s", emitted)
	}

	// No use site moves: every body reads the declared name. (That no use names a backing field is the count below: a
	// body extract runs on to the next declaration block, so it is no place to look for the marker.)
	for decl, want := range map[string]string{
		"public static @string FormatBool(": "return trueˢ;",
		"internal static @string atoi(":     "@string fnAtoi = fnAtoiᶜ;",
		"internal static @string use(":      "Version",
		"internal static bool bigLocal(":    "hugeᶜ;",
	} {
		if body := liftFunctionBody(t, emitted, decl); !strings.Contains(body, want) {
			t.Errorf("%s: want %q:\n%s", decl, want, body)
		}
	}

	// The backing field is named exactly twice per accessor: in the accessor and in its own declaration.
	accessors := strings.Count(emitted, " static ref readonly ")
	backingNames := strings.Count(emitted, StaticBackingFieldMarker)

	if accessors < 7 || backingNames != 2*accessors {
		t.Errorf("want each backing field named twice (accessor and declaration): %d accessors, %d marker occurrences:\n%s", accessors, backingNames, emitted)
	}

	// A pre-boxed `object` field is a reference type: the refusal is about value-class loads, so it is untouched.
	if !strings.Contains(emitted, "private static readonly object ") || strings.Contains(emitted, "ref readonly object") {
		t.Errorf("the pre-boxed any-slot literal keeps its plain object field:\n%s", emitted)
	}
}

// The scope switch (structStaticReadAccessorsApply) decides whether a PLAIN-mode package, a user package without
// -hoist-literals, takes the accessor for its string constants. This follows the switch either way: a plain-mode
// package-level string const takes the accessor exactly when the switch says it applies with the hoist mode off.
func TestPlainModeStringConstFollowsTheAccessorScopeSwitch(t *testing.T) {
	isolateConverterPathGlobals(t)

	saved := packageHoistsLiterals
	packageHoistsLiterals = false
	applies := structStaticReadAccessorsApply()
	packageHoistsLiterals = saved

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/srp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "srp.go"), "package srp\n\nconst Version = \"1.0\"\n\nfunc use() string { return Version }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "srp", "srp.cs")), "\r\n", "\n")
	accessor := "public static ref readonly @string Version => ref Versionᵇ;\nprivate static readonly @string Versionᵇ = \"1.0\"u8;"
	field := "public static readonly @string Version = \"1.0\"u8;"

	if applies && !strings.Contains(emitted, accessor) {
		t.Errorf("the switch applies the accessor to a plain-mode package, but its string const has none:\n%s", emitted)
	}

	if !applies && !strings.Contains(emitted, field) {
		t.Errorf("the switch limits the accessor to the hoist mode, but a plain-mode string const is not one plain field:\n%s", emitted)
	}
}
