// plainStringLiterals_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// plainLiteralFixture holds one site per arm of the plain-literal emission (owner ruling 2026-10-17;
// hoistedLiteralOperations.go). Every literal is distinct, so each arm's assertion names exactly one
// site.
const plainLiteralFixture = "package plit\n\n" +
	"type Token string\n\n" +
	"type Box[T any] struct{ v T }\n\n" +
	"func (b *Box[T]) Set(v T) { b.v = v }\n\n" +
	"func Echo[T any](v T) T { return v }\n\n" +
	"func Pair[K comparable, V any](k K, v V) map[K]V { return map[K]V{k: v} }\n\n" +
	"func take(s string) int { return len(s) }\n\n" +
	"func show(v any) {}\n\n" +
	"func token(t Token) Token { return t }\n\n" +
	"func note(s string) {}\n\n" +
	"// a package-level initializer that reaches a function holding a literal (the encoding/pem shape)\n" +
	"var greeting = fill()\n\n" +
	"func fill() string { return \"reached from an initializer\" }\n\n" +
	"var fromInit string\n\n" +
	"func init() { fromInit = \"assigned inside init\" }\n\n" +
	"func sites(ch chan string, m map[string]int) string {\n" +
	"\ttake(\"argument to a string parameter\")\n" +
	"\tm[\"map index key\"]++\n" +
	"\tch <- \"sent on a channel\"\n" +
	"\tdefined := \"defined local value\"\n" +
	"\tvar declared string = \"declared local value\"\n" +
	"\tdefined = \"assigned local value\"\n" +
	"\tshow(\"passed to an any slot\")\n" +
	"\tshow(\"mixed any and value use\")\n" +
	"\ttake(\"mixed any and value use\")\n" +
	"\tEcho(\"echo through a generic\")\n" +
	"\t_ = Pair(\"generic map key\", 1)\n" +
	"\tvar b Box[string]\n" +
	"\tb.Set(\"generic method value\")\n" +
	"\ttoken(\"named string slot\")\n" +
	"\tdefer note(\"deferred argument value\")\n" +
	"\tgo note(\"goroutine argument value\")\n" +
	"\tnote(`raw line one\nraw line two`)\n" +
	"\tconst fnLocal = \"local const value\"\n" +
	"\ttake(fnLocal)\n" +
	"\ttake(\"OK\")\n" +
	"\t_ = defined + declared + greeting + fromInit\n" +
	"\treturn \"\"\n" +
	"}\n\n" +
	"func word() string { return \"returned word value\" }\n"

func convertPlainLiteralFixture(t *testing.T, hoist bool) string {
	t.Helper()
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), plainLiteralFixture)

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
		hoistLiterals:       hoist,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "plit", "plit.cs")), "\r\n", "\n")
}

// TestPlainLiteralsAtStringSlots: a literal in a value slot typed exactly `string` renders as the bare
// C# literal and claims no field, whatever kept it inline before (an empty literal, a degenerate slug,
// `func init()`, a function a package-level initializer reaches).
func TestPlainLiteralsAtStringSlots(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, false)

	for _, want := range []string{
		`take("argument to a string parameter");`,
		`m["map index key"]++;`,
		`ch.ᐸꟷ("sent on a channel");`,
		`@string defined = "defined local value";`,
		`@string declared = "declared local value";`,
		`defined = "assigned local value";`,
		`take("mixed any and value use");`,
		`@string fnLocal = "local const value";`,
		`take("OK");`,
		`return "";`,
		`return "returned word value";`,
		`return "reached from an initializer";`,
		`fromInit = "assigned inside init";`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the plain literal %q:\n%s", want, emitted)
		}
	}

	for _, unwanted := range []string{
		"static readonly @string argumentToAStringˢ",
		"static readonly @string returnedWordValueˢ",
		"static readonly @string localConstValueᶜ",
		"static readonly @string fnLocalᶜ",
	} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("a plain literal claimed a field (%q):\n%s", unwanted, emitted)
		}
	}
}

// TestPlainLiteralsKeepTheHoistWhereABareStringBindsWrong: every slot where a bare C# string would bind
// the wrong C# type keeps today's form. An `any` slot keeps its pre-boxed field (a mixed literal's field
// becomes the pre-boxed object). An inferred generic call keeps its field, since C# would infer
// T = System.String: `Pair("generic map key", 1)` would make a map<string, nint>, which compiles and
// diverges (G's review, 2026-10-18). So do a generic method on a generic type, a named string slot
// (CS0029), go and defer arguments (the capture list infers its own type arguments) and a multi-line
// raw literal (`.Raw()` strips the checkout's CRs).
func TestPlainLiteralsKeepTheHoistWhereABareStringBindsWrong(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, false)

	for _, want := range []string{
		`private static readonly object passedToAnAnySlotˢ = (@string)"passed to an any slot"u8;`,
		`show(passedToAnAnySlotˢ);`,
		`private static readonly object mixedAnyAndValueUseˢ = (@string)"mixed any and value use"u8;`,
		`show(mixedAnyAndValueUseˢ);`,
		`Echo(echoThroughAGenericˢ);`,
		`Pair(genericMapKeyˢ, `,
		`b.Set(genericMethodValueˢ);`,
		`token(namedStringSlotˢ);`,
		`defer(note, deferredArgumentValueˢ, ref ᒐ);`,
		`goǃ(note, goroutineArgumentValueˢ);`,
		".Raw();",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want today's form %q:\n%s", want, emitted)
		}
	}

	for _, unwanted := range []string{
		`Echo("echo through a generic")`,
		`Pair("generic map key"`,
		`b.Set("generic method value")`,
		`token("named string slot")`,
		`defer(note, "deferred argument value"`,
		`goǃ(note, "goroutine argument value"`,
		`show("passed to an any slot")`,
	} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("a bare C# string reached a slot that binds it wrong (%q):\n%s", unwanted, emitted)
		}
	}
}

// TestHoistLiteralsFlagRestoresTheHoist: -hoist-literals emits exactly the hoisted form, so no literal
// renders plain and the value-slot literals are fields again.
func TestHoistLiteralsFlagRestoresTheHoist(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, true)

	for _, want := range []string{
		`take(argumentToAStringˢ);`,
		`return returnedWordValueˢ;`,
		`private static readonly @string fnLocalᶜ = "local const value"u8;`,
		`private static readonly @string mixedAnyAndValueUseˢ = "mixed any and value use"u8;`,
		`return ""u8;`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("-hoist-literals: want %q:\n%s", want, emitted)
		}
	}

	if strings.Contains(emitted, `take("argument to a string parameter")`) {
		t.Errorf("-hoist-literals emitted a plain literal:\n%s", emitted)
	}
}

// TestStandardLibraryKeepsHoistedLiterals pins the mode KEY. A package whose source is under
// GOROOT/src hoists, GOROOT-vendored packages included, whichever driver asks; the -tests driver runs
// with convertStdLib false (main.go's -stdlib branch is the other arm), so the key is the source
// location, never the flag. -hoist-literals opts any other package in.
func TestStandardLibraryKeepsHoistedLiterals(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")

	for dir, want := range map[string]bool{
		filepath.Join(goRoot, "src", "strings"):                                  true,
		filepath.Join(goRoot, "src", "unicode", "utf8"):                          true,
		filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "net", "idna"): true,
		filepath.Join(goRoot, "src", "cmd", "vendor", "golang.org", "x", "mod"):  true,
		filepath.Join(goRoot, "src"):                                             false, // GOROOT/src itself is no package
		filepath.Join(filepath.Dir(goRoot), "app"):                               false,
		filepath.Join(filepath.Dir(goRoot), "go-other", "src", "strings"):        false,
	} {
		for _, convertStdLib := range []bool{false, true} {
			options := Options{goRoot: goRoot, convertStdLib: convertStdLib}

			if got := literalsHoistedFor(dir, options); got != want {
				t.Errorf("literalsHoistedFor(%q, convertStdLib=%v) = %v, want %v", dir, convertStdLib, got, want)
			}

			options.hoistLiterals = true

			if !literalsHoistedFor(dir, options) {
				t.Errorf("literalsHoistedFor(%q) with -hoist-literals = false, want true", dir)
			}
		}
	}
}

// plainCompositeFixture holds B2's sites: package-level var initializers and composite elements, which
// the hoist never reached (Tier B) and which now read as Go does.
const plainCompositeFixture = "package plit\n\n" +
	"type pair struct {\n\tname string\n\tn    int\n}\n\n" +
	"func Echo[T any](v T) T { return v }\n\n" +
	"var packageGreeting = \"package level value\"\n\n" +
	"var names = []string{\"first slice element\", \"second slice element\"}\n\n" +
	"var counts = map[string]int{\"map literal key\": 1}\n\n" +
	"var labels = map[int]string{1: \"map literal value\"}\n\n" +
	"var keyed = pair{name: \"keyed struct field\", n: 2}\n\n" +
	"var positional = pair{\"positional struct field\", 3}\n\n" +
	"var boxed = []any{\"boxed composite element\"}\n\n" +
	"var echoed = Echo(\"package generic argument\")\n\n" +
	"func local() []string { return []string{\"function composite element\"} }\n\n" +
	"func use() int { return len(packageGreeting) + len(names) + counts[\"\"] + len(labels) + keyed.n + positional.n + len(boxed) + len(echoed) + len(local()) }\n"

// TestPlainLiteralsInPackageVarsAndComposites (B2): a package-level var initializer and a composite's
// element, map key, map value and struct field value typed exactly `string` take the plain literal; an
// `any` element keeps its boxed inline form, and an inferred generic call in an initializer keeps
// today's form by G's rule.
func TestPlainLiteralsInPackageVarsAndComposites(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), plainCompositeFixture)

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

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "plit", "plit.cs")), "\r\n", "\n")

	for _, plain := range []string{
		"package level value", "first slice element", "second slice element", "map literal key",
		"map literal value", "keyed struct field", "positional struct field", "function composite element",
	} {
		if want := `"` + plain + `"`; !strings.Contains(emitted, want) || strings.Contains(emitted, want+"u8") {
			t.Errorf("want the plain literal %s:\n%s", want, emitted)
		}
	}

	// The generic initializer keeps the untyped-constant cast, which fixes T = @string; a bare literal
	// there would infer T = System.String.
	for _, kept := range []string{`(@string)"boxed composite element"u8`, `Echo((@string)"package generic argument")`} {
		if !strings.Contains(emitted, kept) {
			t.Errorf("want today's form %s:\n%s", kept, emitted)
		}
	}

	if strings.Contains(emitted, `Echo("package generic argument")`) {
		t.Errorf("a bare literal reached an inferred generic call:\n%s", emitted)
	}
}

// relocationFixture: a package-level initializer that reaches a function with a local string const
// (LocalStringConstHoist's shape). Hoisted, the const is a field the initializer reads, so the var is
// relocated into package_init.cs; plain, it reads no field and stays a field initializer.
const relocationFixture = "package plit\n\n" +
	"var greeting = makeGreeting(\"world\")\n\n" +
	"func makeGreeting(name string) string {\n\tconst hello = \"hello there \"\n\treturn hello + name\n}\n\n" +
	"func use() string { return greeting }\n"

// TestAStalePackageInitIsRetiredWhenNothingRelocates: a conversion that relocates nothing removes the
// package_init.cs an EARLIER conversion generated at the same path, whose static constructor would call
// an init method this conversion no longer declares (CS0103; measured on LocalStringConstHoist,
// GlobalTupleVarDecl and StringLiteralHoisting when plain literals landed). A hand-written
// package_init.cs is never touched.
func TestAStalePackageInitIsRetiredWhenNothingRelocates(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), relocationFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	convert := func(hoist bool) string {
		t.Helper()

		options := Options{
			goRoot:              goRoot,
			goPath:              build.Default.GOPATH,
			go2csPath:           filepath.Join(root, "out"),
			recurse:             true,
			targetPlatform:      "linux/amd64",
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
			hoistLiterals:       hoist,
		}

		build.Default.GOROOT = options.goRoot
		build.Default.GOPATH = options.goPath

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule(hoist=%v): %v", hoist, err)
		}

		return filepath.Join(options.go2csPath, "src", "example.com", "plit")
	}

	pkgDir := convert(true)
	initPath := filepath.Join(pkgDir, PackageInitFileName)

	if !strings.Contains(readGenerated(t, initPath), "initᴛgreeting();") {
		t.Fatalf("the hoisted conversion must relocate greeting into %s", initPath)
	}

	convert(false)

	if _, err := os.Stat(initPath); !os.IsNotExist(err) {
		t.Errorf("the plain conversion relocates nothing, so %s must be retired (stat err %v)", initPath, err)
	}

	if emitted := readGenerated(t, filepath.Join(pkgDir, "plit.cs")); strings.Contains(emitted, "initᴛgreeting") {
		t.Errorf("the plain conversion must not relocate greeting:\n%s", emitted)
	}

	// CONTROL: a hand-written package_init.cs (no generated header) survives a conversion that
	// relocates nothing.
	handWritten := "// hand-owned\r\nnamespace go.example.com;\r\n"

	if err := os.WriteFile(initPath, []byte(handWritten), 0o644); err != nil {
		t.Fatal(err)
	}

	convert(false)

	if got, err := os.ReadFile(initPath); err != nil || string(got) != handWritten {
		t.Errorf("a hand-written package_init.cs must be left untouched (err %v)", err)
	}
}
