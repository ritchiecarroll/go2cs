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
	"import \"sync\"\n\n" +
	"type holder struct{ m sync.Map }\n\n" +
	"type Token string\n\n" +
	"type Box[T any] struct{ v T }\n\n" +
	"func (b *Box[T]) Set(v T) { b.v = v }\n\n" +
	"func (b *Box[T]) Note(v any) {}\n\n" +
	"func Tag[T any](t T, label any) {}\n\n" +
	"type Sink interface{ Put(v any) }\n\n" +
	"type logger struct{}\n\n" +
	"func (l *logger) Log(args ...any) {}\n\n" +
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
	"\tvar l logger\n" +
	"\tl.Log(\"method variadic any\", 1)\n" +
	"\tvar mp sync.Map\n" +
	"\tmp.Store(\"cross package method\", 1)\n" +
	"\tvar h holder\n" +
	"\th.m.Store(\"field receiver method\", 2)\n" +
	"\tvar sink Sink\n" +
	"\tif sink != nil {\n" +
	"\t\tsink.Put(\"interface method any\")\n" +
	"\t}\n" +
	"\tshow(\"\")\n" +
	"\tdefer show(\"deferred any value\")\n" +
	"\tgo show(\"goroutine any value\")\n" +
	"\tf := show\n" +
	"\tf(\"func value any\")\n" +
	"\tTag(1, \"generic function any slot\")\n" +
	"\t_ = []any{\"composite any element\"}\n" +
	"\ttake(\"mixed any and value use\")\n" +
	"\tEcho(\"echo through a generic\")\n" +
	"\t_ = Pair(\"generic map key\", 1)\n" +
	"\tvar b Box[string]\n" +
	"\tb.Set(\"generic method value\")\n" +
	"\tb.Note(\"generic type any slot\")\n" +
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
// the wrong C# type keeps today's form. An inferred generic call keeps its field, since C# would infer
// T = System.String: `Pair("generic map key", 1)` would make a map<string, nint>, which compiles and
// diverges (G's review, 2026-10-18). So do a generic method on a generic type, a named string slot
// (CS0029), go and defer arguments (the capture list infers its own type arguments) and a multi-line
// raw literal (`.Raw()` strips the checkout's CRs).
func TestPlainLiteralsKeepTheHoistWhereABareStringBindsWrong(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, false)

	for _, want := range []string{
		`Echo(echoThroughAGenericˢ);`,
		`Pair(genericMapKeyˢ, `,
		`b.Set(genericMethodValueˢ);`,
		`token(namedStringSlotˢ);`,
		`defer(note, deferredArgumentValueˢ, ref ᒐ);`,
		`goǃ(note, goroutineArgumentValueˢ, GoStack.Bounded);`,
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
	} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("a bare C# string reached a slot that binds it wrong (%q):\n%s", unwanted, emitted)
		}
	}
}

// TestPlainLiteralsAtInterceptableAnySlots: an argument in an `any` slot of an ordinary call of a non-generic
// function or method renders as the bare C# literal and claims no field; go2cs-gen's LiteralInterceptorGenerator
// boxes it as a Go string (owner ruling 2026-10-17, part 2). That covers a function, a pointer-receiver method's
// `...any` element, an interface method and the empty literal; a literal used in both an any slot and a string
// slot is plain in both.
func TestPlainLiteralsAtInterceptableAnySlots(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, false)

	for _, want := range []string{
		`show("passed to an any slot");`,
		`show("mixed any and value use");`,
		`mp.Store("cross package method", (nint)1);`,
		`sink.Put("interface method any");`,
		`show("");`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the bare literal in an interceptable any slot %q:\n%s", want, emitted)
		}
	}

	for _, unwanted := range []string{"passedToAnAnySlotˢ", "mixedAnyAndValueUseˢ", "crossPackageMethodˢ", "interfaceMethodAnyˢ"} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("an intercepted literal claimed a field (%q):\n%s", unwanted, emitted)
		}
	}
}

// TestAnySlotsGoInterceptorsCannotReachKeepTheHoist: an any slot the generator cannot intercept keeps the
// pre-boxed field: a go or defer argument (the capture list), a func value (a delegate Invoke), a generic
// function and a method of a generic type (their interceptor would itself be generic, deferred). A composite
// element is not a call and keeps its inline Go string.
func TestAnySlotsGoInterceptorsCannotReachKeepTheHoist(t *testing.T) {
	emitted := convertPlainLiteralFixture(t, false)

	for _, want := range []string{
		`private static readonly object deferredAnyValueˢ = (@string)"deferred any value"u8;`,
		`private static readonly object goroutineAnyValueˢ = (@string)"goroutine any value"u8;`,
		`f(funcValueAnyˢ);`,
		`Tag<nint>(1, genericFunctionAnySlotˢ);`,
		`b.Note(genericTypeAnySlotˢ);`,
		`l.Log(methodVariadicAnyˢ, (nint)1);`,
		`.Store(fieldReceiverMethodˢ, (nint)2);`,
		`(@string)"composite any element"u8`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want the pre-boxed form %q:\n%s", want, emitted)
		}
	}

	for _, unwanted := range []string{
		`"deferred any value", ref`,
		`goǃ(show, "goroutine any value"`,
		`f("func value any")`,
		`Tag<nint>(1, "generic function any slot")`,
		`b.Note("generic type any slot")`,
		`l.Log("method variadic any"`,
		`Store("field receiver method"`,
	} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("a bare literal reached an any slot no interceptor can reach (%q):\n%s", unwanted, emitted)
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

// deferCaptureFixture: string literals in deferred and spawned calls, whose arguments the lowering
// passes through the registration's generic type parameters. Two-letter literals, because a go/defer
// argument keeps the hoist, and only a literal whose slug is too short for a field renders inline.
const deferCaptureFixture = "package plit\n\n" +
	"func note(first string, rest ...string) {}\n\n" +
	"func any1(v any) {}\n\n" +
	"func sites() {\n" +
	"\tdefer note(\"aa\", \"bb\", \"hh\")\n" +
	"\tdefer any1(\"cc\")\n" +
	"\tgo note(\"dd\", \"ee\", \"ii\")\n" +
	"\tdefer func() { _ = recover() }()\n" +
	"\tdefer panic(\"ff\")\n" +
	"}\n\n" +
	"func direct() { panic(\"gg\") }\n"

// TestADeferredCaptureListCastsEveryStringLiteral: every string literal in a deferred or spawned
// call's capture list takes `(@string)`, so the registration's type parameter is Go's string and not
// System.String (GO2CS0004 A2; measured on DeferVariadicCallee, DeferPanicArg and DeferLambdaParam).
// The fixed slot, the first variadic element and an `any` slot already did; a variadic element PAST
// the parameter array (`hh`, `ii`) and a deferred panic's value (`ff`) did not. CONTROL: a direct panic keeps its bare literal, which golib's panic normalizes.
func TestADeferredCaptureListCastsEveryStringLiteral(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), deferCaptureFixture)

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

	for _, literal := range []string{"aa", "bb", "hh", "cc", "dd", "ee", "ii", "ff"} {
		bare := `, "` + literal + `"`
		if strings.Contains(emitted, bare) || !strings.Contains(emitted, `(@string)"`+literal+`"`) {
			t.Errorf("want `(@string)\"%s\"` in the capture list, never a bare C# string:\n%s", literal, emitted)
		}
	}

	if !strings.Contains(emitted, `throw panic("gg")`) {
		t.Errorf("control: a direct panic keeps its bare literal:\n%s", emitted)
	}
}

// goCaptureFixture: string literals in spawned calls only. `go` lowers through golib's
// goǃ<T…>(Action<T…>, T… args) (builtin.GoroutineLaunchers.cs), the same generic argument list as
// defer, so a bare literal there infers T = System.String. Two-letter literals, for the reason
// deferCaptureFixture gives.
const goCaptureFixture = "package plit\n\n" +
	"func f(s string) {}\n\n" +
	"func g(v any) {}\n\n" +
	"func fv(first string, rest ...string) {}\n\n" +
	"func two(s string, n int) {}\n\n" +
	"func spawn() {\n" +
	"\tgo f(\"xx\")\n" +
	"\tgo g(\"yy\")\n" +
	"\tgo fv(\"aa\", \"bb\", \"cc\")\n" +
	"\tgo two(\"tt\", 1)\n" +
	"\tgo func(s string) {}(\"ll\")\n" +
	"\tgo panic(\"pp\")\n" +
	"}\n"

// TestAGoStatementCaptureListCastsEveryStringLiteral: the capture-list cast keys on the argument
// site both lowerings share (convExprList's callArgs, set by visitGoStmt and visitDeferStmt alike),
// so a go statement takes `(@string)` exactly as a defer does: a fixed `string` slot, an `any` slot,
// variadic elements inside and past the parameter array, a two-argument arity, a function literal
// and a spawned panic, whose value is the thunk's eager argument (convCallExpr.go's panic arm reads
// the same callArgs).
func TestAGoStatementCaptureListCastsEveryStringLiteral(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), goCaptureFixture)

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

	if strings.Count(emitted, "goǃ(") != 6 {
		t.Fatalf("want six goǃ launches:\n%s", emitted)
	}

	for _, literal := range []string{"xx", "yy", "aa", "bb", "cc", "tt", "ll", "pp"} {
		bare := `"` + literal + `"`
		if strings.Count(emitted, bare) != strings.Count(emitted, `(@string)`+bare) || !strings.Contains(emitted, `(@string)`+bare) {
			t.Errorf("want `(@string)\"%s\"` in the go statement's capture list, never a bare C# string:\n%s", literal, emitted)
		}
	}
}
