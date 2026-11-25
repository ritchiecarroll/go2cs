// methodHolder_test.go - Gbtc
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

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// convertMethodHolderFixture converts a single package made of the given Go files and returns every
// emitted `.cs` file by name, with line endings normalized. A `_test.go` file is written beside the
// others and is not converted: the conversion is the production one.
func convertMethodHolderFixture(t *testing.T, files map[string]string) map[string]string {
	t.Helper()

	return convertMethodHolderModule(t, "example.com/app", files)
}

// convertMethodHolderModule is convertMethodHolderFixture for a module of the given path. The path
// decides the namespace of the package: `example.com/app` is converted into `go.example.com`, and
// `go2cs/app`, the shape of a behavioral test's module, into the root namespace `go`.
func convertMethodHolderModule(t *testing.T, modulePath string, files map[string]string) map[string]string {
	t.Helper()

	return convertMethodHolderTree(t, modulePath, files, nil)
}

// convertMethodHolderTree is convertMethodHolderModule with the module's other packages: dependencies
// holds their files by path below the module root (`lib/lib.go`). They are written and loaded, never
// converted, as when a package is converted on its own.
func convertMethodHolderTree(t *testing.T, modulePath string, files, dependencies map[string]string) map[string]string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module "+modulePath+"\n\ngo 1.23\n")

	for name, source := range dependencies {
		writeModuleFile(t, filepath.Join(pkgDir, filepath.FromSlash(name)), source)
	}

	for name, source := range files {
		writeModuleFile(t, filepath.Join(pkgDir, name), source)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("conversion failed: %v", err)
	}

	emitted := map[string]string{"package_info.cs": strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "package_info.cs")), "\r", "")}

	for name := range files {
		if strings.HasSuffix(name, "_test.go") {
			continue
		}

		csName := strings.TrimSuffix(name, ".go") + ".cs"
		emitted[csName] = strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, csName)), "\r", "")
	}

	return emitted
}

// requireRecordedForwarder fails the test unless the conversion just run recorded, for the holder, a forwarder whose
// declaration holds text (recordMethodForwarder; the -tests flow writes them with the variant's metadata).
func requireRecordedForwarder(t *testing.T, holder string, text string) {
	t.Helper()

	var recorded []string

	for _, forwarder := range packageMethodForwarders {
		if forwarder.holder == holder {
			if strings.Contains(forwarder.line, text) {
				return
			}

			recorded = append(recorded, forwarder.line)
		}
	}

	t.Errorf("no forwarder of holder %s holds %q; the holder's forwarders:\n%s", holder, text, strings.Join(recorded, "\n"))
}

// requireEmitted fails the test for each wanted text the emitted file does not hold, and for each
// refused text it does.
func requireEmitted(t *testing.T, name string, emitted string, wanted []string, refused []string) {
	t.Helper()

	for _, text := range wanted {
		if !strings.Contains(emitted, text) {
			t.Errorf("%s does not hold %q\n%s", name, text, emitted)
		}
	}

	for _, text := range refused {
		if strings.Contains(emitted, text) {
			t.Errorf("%s holds %q\n%s", name, text, emitted)
		}
	}
}

const methodHolderSource = `package main

import "fmt"

const Limit = 7

var Count = 3

type Kind int

func Scale(n int) int { return n * 10 }

type Box struct{ n int }

func (b Box) Limit() int { return b.n + Limit }

func (b *Box) Count() int {
	Count++
	return b.n + Count
}

func (b Box) Kind() Kind { return Kind(b.n) }

func (b Box) Scale() int {
	f := Scale
	return Scale(b.n) + f(1)
}

func (b Box) Plain() int { return b.n + other }

const other = 1

func main() {
	b := Box{n: 1}
	limit := Box.Limit
	count := (*Box).Count
	fmt.Println(b.Limit(), b.Count(), b.Kind(), b.Scale(), b.Plain(), limit(b), count(&b), Limit, Count, Scale(1))
}
`

// A method with a holder takes the suffix form (GlyphMethodSuffix): its body `<Name>⁔` stands in the package class
// beside its type, its receiver the first parameter, and the file never switches class. The holder is declared once
// in package_info.cs with the type it holds, and package_info.cs carries the holder's Go-named forwarders to the bodies.
func TestMethodsTakeTheSuffixFormWithAForwarderInTheirHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"partial class main_package {\n",
		"\ninternal static nint Limit⁔(/*this*/ Box b) { return b.n + (nint)Limit; }\n",
		"\ninternal static nint Count⁔(/*this*/ ref Box b) {\n",
		"\ninternal static UntypedInt other => 1;",
		"} // end main_package\n",
	}, []string{
		"Δ",
		"partial class mainᴛBox",
		"using static main_package;",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"// <MethodHolders>\n",
		"[GoMethodHost(typeof(main_package.Box)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛBox;\n",
		"// </MethodHolders>\n",
		"// <MethodForwarders>\npartial class mainᴛBox {\n",
		"    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Limit(this Box b) => main_package.Limit⁔(b);\n",
		"    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static nint Count(this ref Box b) => main_package.Count⁔(ref b);\n",
		"    [MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough] public static Kind Kind(this Box b) => main_package.Kind⁔(b);\n",
		"}\n// </MethodForwarders>\n",
	}, nil)
}

// A constant, a variable and a function named like a method are read bare in the method's body: the body stands in
// the package class, where the method's own name is the suffixed one, so nothing hides them. No declaration takes a
// rename because a method shares its name, and the file takes no alias of its own package class.
func TestABodyReadsThePackageNamesItsMethodsShareBare(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"{ return b.n + (nint)Limit; }",
		"\n    Count++;",
		"return b.n + Count;",
		"var f = Scale;",
		"return Scale(b.n) + f(1);",
		"{ return b.n + (nint)other; }",
		"public static UntypedInt Limit => 7;",
		"public static nint Count = 3;",
		"public static nint Scale(nint n)",
		"internal static Kind Kind⁔(/*this*/ Box b) { return (Kind)b.n; }",
	}, []string{
		"using main = ",
		"main.Limit",
		"main.Count",
		"main.Scale",
		"main.other",
		"(nint)main_package.Limit",
	})
}

// A method expression of a value receiver binds the method's body, so the function is the method's own; a pointer
// receiver's body takes `ref T`, which the expression's ж<T> delegate cannot bind, so it names RecvGenerator's twin
// of the forwarder in the holder (methodExpressionBody).
func TestMethodExpressionBindsTheBodyOrTheHoldersTwin(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"((Func<Box, nint>)(main_package.Limit⁔))",
		"((Func<ж<Box>, nint>)(mainᴛBox.Count))",
	}, []string{
		"mainᴛBox.Limit",
	})
}

// No file takes an alias of its own package class or a `using static` of it: every body stands in the package class.
// The package is a library: package main keeps such methods in its package class.
func TestNoFileTakesASelfAliasOrAUsingStaticOfItsPackage(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{
		"main.go": `package app

import "fmt"

type Box struct{ n int }

func (b Box) Size() int { return b.n + unit }

func Run() { fmt.Println(Box{1}.Size(), double(2)) }
`,
		"plain.go": `package app

const unit = 1

func double(n int) int { return n * 2 }
`,
	})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{"internal static nint Size⁔(/*this*/ Box b) { return b.n + (nint)unit; }"}, []string{"using app = ", "using static app_package;", "partial class appᴛ"})
	requireEmitted(t, "plain.cs", emitted["plain.cs"], nil, []string{"using app = ", "using static app_package;", "partial class appᴛ"})
}

// A local or parameter named like the package (`main`) and a method named like it (odd's `main`) change nothing in a
// body: it names the package's constant bare, as the package class reads it, with no alias to shadow.
func TestABodyNamesItsPackagesConstantBareBesideALocalNamedLikeThePackage(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": `package main

import "fmt"

const Limit = 7

type meter struct{ main int }

func (m meter) Limit() int {
	main := m.main + 1
	return main + Limit
}

func (m meter) Scale(main int) int { return main + Limit }

func (m meter) Count() int { return Limit + m.main }

type odd struct{}

func (o odd) main() int { return 1 }

func (o odd) Limit() int { return Limit + o.main() }

func main() {
	m := meter{main: 2}
	fmt.Println(m.Limit(), m.Scale(3), m.Count(), odd{}.Limit())
}
`})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"return main + (nint)Limit;",
		"internal static nint Scale⁔(/*this*/ meter m, nint main) { return main + (nint)Limit; }",
		"internal static nint Count⁔(/*this*/ meter m) { return (nint)Limit + m.main; }",
		"internal static nint main⁔(/*this*/ odd o) { return 1; }",
		"internal static nint Limit⁔(/*this*/ odd o) { return (nint)Limit + o.main(); }",
	}, []string{
		"main.Limit",
		"main_package.Limit",
	})
}

// A package that declares a type of its own name takes no alias, and its method's body names the package's constant
// bare beside the type.
func TestPackageWithATypeOfItsOwnNameTakesNoSelfAlias(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	pkgDir := filepath.Join(root, "tone")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/tone\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "tone.go"), `package tone

type tone int

const Base = 100

func (t tone) Base() int { return Base * 2 }

func Play(n int) int { return tone(n).Base() }
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("conversion failed: %v", err)
	}

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "tone.cs")), "\r", "")
	packageInfo := strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "package_info.cs")), "\r", "")

	requireEmitted(t, "tone.cs", emitted, []string{
		"internal static nint Base⁔(/*this*/ tone t) { return Base * 2; }",
	}, []string{"using tone = ", "partial class toneᴛtone"})

	requireEmitted(t, "package_info.cs", packageInfo, []string{
		"partial class toneᴛtone {\n",
		"internal static nint Base(this tone t) => tone_package.Base⁔(t);\n",
	}, nil)
}

const narrowedRulesSource = `package main

import (
	"fmt"
	"math/bits"
	"sort"
)

type words []uint64

func (w words) Len() int           { return len(w) }
func (w words) Less(i, j int) bool { return w[i] < w[j] }
func (w words) Swap(i, j int)      { w[i], w[j] = w[j], w[i] }

func (w words) cap() int  { return cap(w) + 1 }
func (w words) bits() int { return bits.Len64(w[0]) }
func (w words) Sort()     { Sort(w) }
func (w words) Run() int  { return len(w) }

type other struct{ n int }

func (o other) size(w words) int { return cap(w) + bits.Len64(uint64(o.n)) }

func Sort(data sort.Interface) { sort.Sort(data) }

func Run() int { return 4 }

func main() {
	w := words{3, 1, 2}
	w.Sort()
	run := Run
	var boxed any = Run
	_, isFunc := boxed.(func() int)
	fmt.Println(w, w.cap(), cap(w), w.bits(), bits.Len64(w[0]), other{n: 5}.size(w), run(), isFunc, w.Run())
}
`

// The four rules below existed package-wide while every method stood in the package class, and held only inside
// the holder that declared the method while its body stood there. A body stands in the package class under its
// suffixed name, so none of them holds anywhere.

// A built-in named like a method is read bare in the method's own body.
func TestABodyReadsABuiltinOfItsMethodsNameBare(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{
		"internal static nint cap⁔(/*this*/ words w) { return cap(w) + 1; }",
		"internal static nint size(this other o, words w) { return cap(w) + ",
		"w.cap(), cap(w), ",
	}, []string{
		"builtin.cap(",
	})
}

// An import named like a method is read through its alias in the method's own body.
func TestABodyReadsAnImportOfItsMethodsNameThroughItsAlias(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{
		"internal static nint bits⁔(/*this*/ words w) { return bits.Len64(w[0]); }",
		"cap(w) + bits.Len64((uint64)o.n); }",
		"w.bits(), bits.Len64(w[0]), ",
	}, []string{
		"bits_package.Len64(",
	})
}

// A method a test file declares stands in a holder of its own, so it changes no production reference:
// the production file keeps the import's alias and carries no comment about the test.
func TestATestFilesMethodLeavesTheProductionImportAliasAlone(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{
		"main.go": `package main

import (
	"fmt"
	"math/bits"
)

type key struct{ b []byte }

func (k key) size() uint64 {
	hi, lo := bits.Mul64(uint64(len(k.b)), 8)
	return hi ^ lo
}

func mix(a, b uint64) uint64 {
	hi, lo := bits.Mul64(a, b)
	return hi ^ lo
}

func main() { fmt.Println(mix(3, 4), key{}.size()) }
`,
		"key_test.go": `package main

func (k key) bits() int { return len(k.b) * 8 }
`,
	})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{"= bits.Mul64(a, b);"}, []string{"bits_package.Mul64", "Fully qualified"})

	if got := strings.Count(emitted, "= bits.Mul64("); got != 2 {
		t.Errorf("main.cs reads bits through its alias %d times, want 2\n%s", got, emitted)
	}
}

// A function named like a method is the only member of its name in the package class, so its value
// is a plain method group: no delegate type is named for a local, and none is cast to for a box.
func TestFunctionNamedLikeAMethodIsAPlainMethodGroup(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{"var run = Run;", "any boxed = Run;"}, []string{"(Func<nint>)(Run)", "Func<nint> run"})
}

// A call of a function whose argument's type declares a method of the function's name binds the
// function: the method is no member of the class the call names, so the argument takes no cast.
func TestCallOfAFunctionNamedLikeItsArgumentsMethodTakesNoCast(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{"internal static void Sort⁔(/*this*/ words w) { Sort(w); }"}, []string{"(sort.Interface)(w)", "main.Sort("})
}

// A type named like a METHOD. While the body stood in the method's holder, two forms read the name as an EXPRESSION
// first, where the holder's method group bound: a box accessor (`Tag.ᏑValue`, CS0119) and a type-switch label with
// no binding (`case Tag:`, CS8917). The body stands in the package class, where the method is the suffixed `Tag⁔`,
// so both forms read the type there as everywhere else.
const typeNamedLikeMethodSource = `package main

import "fmt"

type Tag struct {
	id    int
	Value int
}

type Chest struct{ tag Tag }

func (c *Chest) Tag() Tag { return c.tag }

func (c *Chest) Forms(v any) string {
	ptr := &Tag{id: 3}
	valueAddr := &ptr.Value
	*valueAddr += 4

	kind := "other"

	switch v.(type) {
	case Tag:
		kind = "tag"
	case *Tag, []Tag:
		kind = "tags"
	}

	return fmt.Sprint(ptr.Value, kind, len([]Tag{*ptr}))
}

func outside(v any) string {
	ptr := &Tag{id: 3}
	valueAddr := &ptr.Value
	*valueAddr += 4

	switch v.(type) {
	case Tag:
		return "tag"
	}

	return fmt.Sprint(ptr.Value)
}

func main() {
	c := &Chest{}
	fmt.Println(c.Forms(Tag{}), c.Forms(&Tag{}), c.Forms(1), outside(Tag{}), outside(1), c.Tag().id)
}
`

func TestATypeNamedLikeAMethodIsReadAsATypeInTheMethodsBody(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": typeNamedLikeMethodSource})["main.cs"]

	body, outside, found := strings.Cut(emitted, "internal static @string outside(any v) {")

	if !found {
		t.Fatalf("main.cs does not declare outside after the bodies:\n%s", emitted)
	}

	requireEmitted(t, "the bodies of Chest's methods", body, []string{
		"internal static Tag Tag⁔(/*this*/ ref Chest c) { return c.tag; }",
		"var ptr = Ꮡ(new Tag(id: 3));",
		"var valueAddr = ptr.of(Tag.ᏑValue);",
		"case Tag: {",
		"case ж<Tag> _:",
		"slice<Tag>(",
	}, []string{
		"main_package.Tag",
		"case Tag _:",
	})

	requireEmitted(t, "the package class", outside, []string{
		"var valueAddr = ptr.of(Tag.ᏑValue);",
		"case Tag: {",
	}, []string{
		"main_package.Tag",
		"case Tag _:",
	})

	if strings.Contains(emitted, "partial class mainᴛChest") {
		t.Errorf("main.cs switches into the holder of Chest:\n%s", emitted)
	}
}

const holderEndsWithItsMethodSource = `package main

import "fmt"

type Box struct{ n int }

func Limit() int { return 7 }

func (b Box) Limit() int { return b.n + Limit() }

var Use = func() int { return Limit() }

type words []string

func (w words) len() int { return len(w) }

var Count = func(w words) int { return len(w) }

func main() {
	fmt.Println(Box{1}.Limit(), Use(), words{"a"}.len(), Count(words{"a", "b"}))
}
`

// A method's name hides nothing in the package class, where its body stands as the suffixed name: the bodies and the
// function literals of the package-level variables that follow them name the package's function and the built-in
// bare alike.
func TestNoBodyOrDeclarationAfterItQualifiesANameItsMethodShares(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": holderEndsWithItsMethodSource})
	main := emitted["main.cs"]

	requireEmitted(t, "main.cs", main, []string{
		"internal static nint Limit⁔(/*this*/ Box b) { return b.n + Limit(); }",
		"internal static nint len⁔(/*this*/ words w) { return len(w); }",
	}, []string{
		"main_package.Limit()",
		"main.Limit()",
		"builtin.len(",
	})
}

const holderHiddenTypeSource = `package app

import (
	"fmt"
	"sync/atomic"
)

type sync struct {
	label string
	hits  int32
}

func (s *sync) bump(c *atomic.Int32) { s.hits = c.Add(1) }

func (s sync) again() sync { return sync{label: s.label, hits: s.hits + 1} }

func (s sync) all() []sync { return []sync{s, sync(s)} }

func (s sync) same(other any) bool {
	o, ok := other.(sync)
	return ok && o == s
}

func fresh() sync { return sync{label: "fresh"} }

func each(all []sync) int { return len(all) }

func Run() {
	var c atomic.Int32
	s := fresh()
	s.bump(&c)
	fmt.Println(s.again().hits, each(s.all()), s.same(s), c.Load())
}
`

// A type of the package named like a child namespace of the package's own namespace (`go.sync`, in view
// here through the import of sync/atomic, for a package in the root namespace) is hidden by that
// namespace outside the package class, in a type position as in any other: a holder is not nested in the
// package class, and the namespace's own members are read before package_info.cs's `using static`. A
// method's forwarder repeats its body's signature in the holder, so the method's signature and body name
// the type through the package class (holderHiddenTypes); the package's other functions keep the bare name.
// A package in any other namespace (TestASignatureKeepsATypeNoNamespaceHidesBare) has no such child.
func TestASignatureNamesATypeANamespaceHidesThroughThePackageClass(t *testing.T) {
	emitted := convertMethodHolderModule(t, "go2cs/app", map[string]string{"main.go": holderHiddenTypeSource})
	main, packageInfo := emitted["main.cs"], emitted["package_info.cs"]

	if !strings.HasPrefix(main, "namespace go;") {
		t.Fatalf("main.cs is not in the root namespace, where `go.sync` is a child of the package's own\n%s", main)
	}

	bodies, outside, found := strings.Cut(main, "internal static sync fresh()")

	if !found {
		t.Fatalf("main.cs does not declare fresh after the bodies\n%s", main)
	}

	requireEmitted(t, "the bodies of sync's methods", bodies, []string{
		"bump⁔(/*this*/ ref app_package.sync s, ",
		"app_package.sync again⁔(/*this*/ app_package.sync s)",
		"new app_package.sync(label: s.label",
		"slice<app_package.sync> all⁔(/*this*/ app_package.sync s)",
		"other._<app_package.sync>(",
	}, []string{
		" sync s",
		"<sync>",
		"new sync(",
		"(sync)",
	})

	requireEmitted(t, "the package class", "internal static sync fresh()"+outside, []string{
		"internal static sync fresh()",
		"new sync(label: \"fresh\")",
		"each(slice<sync> all)",
	}, []string{
		"app_package.sync",
	})

	requireEmitted(t, "package_info.cs", packageInfo, []string{
		"partial class appᴛsync {\n",
		"bump(this ref app_package.sync s, ",
		"app_package.sync again(this app_package.sync s) => app_package.again⁔(s);",
	}, []string{
		" sync s",
	})
}

func TestASignatureKeepsATypeNoNamespaceHidesBare(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": holderHiddenTypeSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"namespace go.example.com;",
		"bump⁔(/*this*/ ref sync s, ",
		"internal static sync again⁔(/*this*/ sync s)",
	}, []string{
		"app_package.sync",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"bump(this ref sync s, ",
		"internal static sync again(this sync s) => app_package.again⁔(s);",
	}, []string{
		"ref app_package.sync s",
		"app_package.sync again(",
	})
}

const usingAliasLibSource = `package lib

// Token shares its name with Reader's method. Until methods moved to their holders that renamed it,
// and the consumer below spelled it through the rename's own two-hop record.
type Token any

type Reader struct{}

func (Reader) Token() Token { return "tok" }

// Plain is the same declaration with no method of its name.
type Plain any

func Make() Plain { return 7 }
`

const usingAliasWrapSource = `package wrap

import "example.com/app/lib"

type Token = lib.Token
type Plain = lib.Plain

func Read() Token { return lib.Reader{}.Token() }

func Make() Plain { return lib.Make() }
`

const usingAliasMainSource = `package main

import (
	"fmt"

	"example.com/app/wrap"
)

func main() {
	var t wrap.Token = wrap.Read()
	var p wrap.Plain = wrap.Make()

	fmt.Println(t, p)
}
`

// A defined type over the empty interface (`type Token any`) is emitted by its package as a
// `global using`, never as a member of the package class. A file that reaches it only through another
// package's alias, without importing the type's own package, has no alias key loaded for it, and the
// Go name (`lib.Token`) names nothing in C# (CS0426, the behavioral project RenamedTypeAliasReexport).
// It is spelled as what it stands for. Before methods moved to their holders this held only for a type
// renamed for a method of its name; it holds for every such type.
func TestTypeItsPackageEmitsAsAUsingIsSpelledByItsTargetWhereNoAliasIsLoaded(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"main.go": usingAliasMainSource},
		map[string]string{"lib/lib.go": usingAliasLibSource, "wrap/wrap.go": usingAliasWrapSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"any t = wrap.Read();",
		"any p = wrap.Make();",
	}, []string{
		"lib.Token",
		"lib.Plain",
	})
}

// golib names its friend assemblies in InternalsVisibleTo by assembly name, and a standard-library assembly is
// go.<import path> (assemblyIdentity.go). An internal golib type of namespace `go` is golib's inside a method holder
// of runtime, whose assembly is a friend, and not of fmt, whose assembly is not.
func TestGolibFriendTypesFollowTheAssemblyName(t *testing.T) {
	const friendType = "Waiter"

	if !golibFriendNamespaceTypes[friendType] {
		t.Fatalf("golib no longer declares the internal type %s in namespace go; pick another from golibNamespaceNames.go", friendType)
	}

	if !golibNamespaceType(friendType, "runtime") {
		t.Errorf("runtime's assembly go.runtime is golib's friend, so %s is golib's there", friendType)
	}

	if golibNamespaceType(friendType, "fmt") {
		t.Errorf("fmt's assembly go.fmt is not golib's friend, so %s is not golib's there", friendType)
	}
}

const promotedForwarderHolderSource = `package main

import "fmt"

type Driver struct{ name string }

type Connector interface {
	Driver() Driver
	Name() string
}

type base struct{}

func (base) Driver() Driver { return Driver{name: "base"} }
func (base) Name() string   { return "base" }

type (
	counting struct {
		Connector
		hits int
	}
	after struct{ n int }
)

func (c *counting) Name() string {
	c.hits++
	return "counting"
}

func main() {
	var k Connector = &counting{Connector: base{}}
	fmt.Println(k.Name(), k.Driver().name, after{n: 1}.n)
}
`

// A promoted forwarder the converter writes for a struct (a method its pointer gets only through an
// embedded interface) is one of the struct's methods, so it takes the suffix form as the methods Go declares
// on it do: its body `Driver⁔` in the package class right after the struct, and its Go-named forwarder in the
// struct's holder. The Go name in the package class would share a class with the type of its name: CS0102,
// `Driver` beside `Driver`. No file switches into the holder.
func TestPromotedForwarderTakesTheSuffixFormInItsStructsHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": promotedForwarderHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"\n// Go method set entry for the promoted 'Connector.Driver()'",
		"internal static Driver Driver⁔(/*this*/ counting recvᴛ) => recvᴛ.Connector.Driver();\n\npartial struct after {",
		"\ninternal static @string Name⁔(/*this*/ ref counting c)",
	}, []string{
		"Δ",
		"partial class mainᴛ",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"[GoMethodHost(typeof(main_package.counting)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class mainᴛcounting;\n",
		"internal static Driver Driver(this counting recvᴛ) => main_package.Driver⁔(recvᴛ);\n",
		"internal static @string Name(this ref counting c) => main_package.Name⁔(ref c);\n",
	}, nil)
}

const dotImportHolderLibSource = `package lib

const size = 64

var shared = 1

type pair struct{ a, b int }

func Exported() int { return size + shared + pair{1, 2}.a }
`

const dotImportHolderMainSource = `package prog

import (
	"fmt"

	. "example.com/app/lib"
)

const size = 32

var shared = 2

var alone = 3

type Set struct{ n int }

type pair struct{ x int }

type single struct{ y int }

func (s Set) Bits() int { return s.n + size + shared + alone }

func (p pair) Sum(q single) int { return p.x + q.y + pair{x: 1}.x }

func plain() int { return size + shared + alone }

func Run() {
	fmt.Println(Set{n: 1}.Bits(), plain(), Exported(), pair{}.Sum(single{}))
}
`

// A dot import (`. "runtime"`) is written `using static runtime_package;`, which in C# brings in every name
// of the class the assembly can see, unexported ones included, where Go brings in none of them. In the
// package class the class's own member wins, so a method's body reads every name bare. Outside the package
// class two using-statics supply a type both packages declare with equal standing (net/http's dumpConn,
// CS0104), so a type such a signature names is written through its own package's class, which the forwarder
// in the holder repeats. The package is a library: package main keeps these methods in its package class.
func TestABodyReadsAnOwnNameADotImportedPackageAlsoDeclaresBare(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"main.go": dotImportHolderMainSource},
		map[string]string{"lib/lib.go": dotImportHolderLibSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"using static go.example.com.app.lib_package;\n",
		"internal static nint Bits⁔(/*this*/ Set s) { return s.n + (nint)size + shared + alone; }",
		"return (nint)size + shared + alone;",
		"Sum⁔(/*this*/ prog_package.pair p, single q) { return p.x + q.y + new prog_package.pair(x: 1).x; }",
	}, []string{
		"prog.alone",
		"prog.size",
		"lib_package.size",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"Sum(this prog_package.pair p, single q) => prog_package.Sum⁔(p, q);",
	}, nil)
}

const packageClassMethodsLibSource = `package lib

func Visible() int { return hidden() }

func hidden() int { return 2 }

func Area() int { return 3 }
`

const packageClassMethodsMainSource = `package main

import (
	"fmt"
	"sort"

	. "example.com/app/lib"
)

type plain struct{ n int }

func (p plain) Size() int { return p.n }

func (p *plain) Grow(by int) { p.n += by }

func Limit() int { return 7 }

type withFunc struct{}

func (withFunc) Limit() int { return Limit() }

type withBuiltin []int

func (w withBuiltin) len() int {
	size := plain.Size
	return len(w) + size(plain{0})
}

type withImport []int

func (w withImport) sort() { sort.Ints(w) }

type withGolib struct{}

func (withGolib) zero() int { return 0 }

type withDot struct{}

func (withDot) hidden() int { return 1 }

type withDotExported struct{}

func (withDotExported) Area() int { return Area() }

func apply(Size int) int {
	size := plain.Size
	return size(plain{Size})
}

func pick() (Size func(plain) int) {
	Size = plain.Size
	return
}

func main() {
	size := plain.Size
	Size := plain.Size
	boxed := (*plain).Size
	p := plain{1}
	fmt.Println(Size(p), boxed(&p), apply(1), pick()(p), withDotExported{}.Area())
	p.Grow(2)
	w := withImport{2, 1}
	w.sort()
	fmt.Println(size(p), withFunc{}.Limit(), withBuiltin{1}.len(), withGolib{}.zero(), withDot{}.hidden(), Visible(), w)
}
`

// Package main keeps a type's methods in its package class, beside its functions, unless one of them is named
// like something a file of the package reads bare, which the method would hide there from the whole package: a
// package-level name, a name of the universe, an import's name, a name a dot-imported package declares (an
// unexported one included), or a member of golib's builtin class. Such a type keeps its holder, as does a struct
// whose forwarder for an embedded interface's method is named so (TestPromotedForwarderTakesTheSuffixFormInItsStructsHolder):
// its methods take the suffix form, a body in the package class and a forwarder in the holder.
// A method expression of a type without a holder names the method through the package class, under the delegate
// cast that picks the receiver's overload: bare, it would bind a local, parameter or result of its name first
// (apply, pick, main), and nothing from a holder (withBuiltin's len). withDotExported keeps its holder because
// its method is named like a function the dot import brings, which it calls: in the package class it would call
// itself.
func TestMethodsOfPackageMainStayInItsPackageClassUnlessTheyHideAName(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"main.go": packageClassMethodsMainSource},
		map[string]string{"lib/lib.go": packageClassMethodsLibSource})

	main, packageInfo := emitted["main.cs"], emitted["package_info.cs"]

	requireEmitted(t, "main.cs", main, []string{
		"internal static nint Size(this plain p)",
		"internal static void Grow(this ref plain p, nint by)",
		"((Func<plain, nint>)(main_package.Size))",
		"=> main_package.Size(panicwrapRecv(p0, ",
		"internal static nint Limit⁔(/*this*/ withFunc _) { return Limit(); }",
		"internal static nint len⁔(/*this*/ withBuiltin w) {",
		"internal static void sort⁔(/*this*/ withImport w) { sort.Ints(w); }",
		"internal static nint zero⁔(/*this*/ withGolib _) { return 0; }",
		"internal static nint hidden⁔(/*this*/ withDot _) { return 1; }",
		"internal static nint Area⁔(/*this*/ withDotExported _) { return Area(); }",
	}, []string{
		"partial class mainᴛ",
		"mainᴛplain.",
		"Size⁔",
		"((Func<plain, nint>)(Size))",
		"=> Size(",
	})

	for _, kept := range []string{"withFunc", "withBuiltin", "withImport", "withGolib", "withDot", "withDotExported"} {
		requireEmitted(t, "package_info.cs", packageInfo, []string{"partial class mainᴛ" + kept + ";", "partial class mainᴛ" + kept + " {\n"}, nil)
	}

	requireEmitted(t, "package_info.cs", packageInfo, nil, []string{"mainᴛplain"})

	// A library keeps the holder of the same type: its classes are a consumer's surface.
	library := convertMethodHolderFixture(t, map[string]string{"main.go": `package app

type plain struct{ n int }

func (p plain) Size() int { return p.n }
`})

	requireEmitted(t, "the library's main.cs", library["main.cs"], []string{"internal static nint Size⁔(/*this*/ plain p) { return p.n; }"}, nil)
	requireEmitted(t, "the library's package_info.cs", library["package_info.cs"], []string{"partial class appᴛplain;", "partial class appᴛplain {\n"}, nil)
}

// A file that ends in a method closes its package class by name: the method's body stands in the package class, and
// no file opens a method holder.
func TestFileThatEndsInAMethodClosesItsPackageClass(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{
		"box.go": `package app

type Box struct{ n int }

func Make() Box { return Box{1} }

func (b Box) Size() int { return b.n }
`,
		"make.go": `package app

func (b Box) Twice() int { return b.n * 2 }

func Use() int { return Make().Twice() }
`,
	})

	box, use := emitted["box.cs"], emitted["make.cs"]

	if !strings.HasSuffix(box, "internal static nint Size⁔(/*this*/ Box b) { return b.n; }\n\n} // end app_package\n") {
		t.Errorf("box.cs does not close its package class by name after the body of Size\n%s", box)
	}

	requireEmitted(t, "box.cs", box, nil, []string{"partial class appᴛBox", "// end appᴛBox"})
	requireEmitted(t, "make.cs", use, []string{"internal static nint Twice⁔(/*this*/ Box b) { return b.n * 2; }"}, []string{"partial class appᴛBox"})

	if !strings.HasSuffix(use, "\n} // end app_package\n") {
		t.Errorf("make.cs does not close its package class by name\n%s", use)
	}
}

const mainTestsProductionSource = `package main

import "fmt"

type plain struct{ n int }

func (p plain) Size() int { return p.n }

type opts struct{ on bool }

func (o *opts) verbose() bool { return o.on }

func main() { fmt.Println(plain{1}.Size(), (&opts{true}).verbose()) }
`

const mainTestsTestSource = `package main

import "testing"

var verbose = true

func TestMethods(t *testing.T) {
	size := plain.Size
	on := (*opts).verbose

	if size(plain{2}) != 2 || on(&opts{verbose}) != verbose {
		t.Fatal("method expressions")
	}
}
`

// A program's test file names a method of a type without a holder through the production package class, rooted,
// since the test file emits into a class of its own; and a type whose method a test file's name would hide
// (verbose) keeps its holder, decided alike by the program converted on its own, which loads no test file, and by
// its -tests variant, which names that holder.
func TestPackageMainDecidesItsHoldersWithItsTestFilesNames(t *testing.T) {
	isolateConverterPathGlobals(t)

	program := convertMethodHolderFixture(t, map[string]string{"main.go": mainTestsProductionSource, "main_test.go": mainTestsTestSource})

	requireEmitted(t, "the program's package_info.cs", program["package_info.cs"], []string{"partial class mainᴛopts;"}, []string{"mainᴛplain"})

	test := convertMainTestsVariant(t, mainTestsProductionSource, mainTestsTestSource)

	requireEmitted(t, "main_test.cs", test, []string{"main_package.Size))", "mainᴛopts.verbose"}, []string{"((Func<plain, nint>)(Size))"})
}

// convertMainTestsVariant runs the real -tests wiring (convertTestVariants) over a program with one in-package test
// file and returns the test file's emission, LF-normalized.
func convertMainTestsVariant(t *testing.T, productionSource, testSource string) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":           "module example/maintests\n\ngo 1.23\n",
		"app/main.go":      productionSource,
		"app/main_test.go": testSource,
	})

	inputPath := filepath.Join(dir, "app")

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	internal, external := findTestVariants(loaded, production)

	if production == nil || internal == nil || external != nil {
		t.Fatal("the fixture must load a program and its internal test variant alone")
	}

	outputPath := t.TempDir()

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		testPackagePath:     production.PkgPath,
		testPackageName:     production.Name,
	}

	if _, err = convertTestVariants(selectTestProjectModel(internal, external), production, internal, external, selectCompileExcludedTestFiles(internal, external),
		inputPath, outputPath, "go", hashset.NewHashSet(supportedTestCapabilities()), options); err != nil {
		t.Fatalf("convertTestVariants: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(outputPath, "main_test.cs")), "\r\n", "\n")
}

// A type a test file declares a method on keeps its holder in the program converted on its own, which never sees
// that method, as in its -tests variant, where the method joins the type's own and hides the package function of
// its name (reset): otherwise the test file names a holder the program never emitted.
func TestPackageMainKeepsTheHolderOfATypeItsTestFileAddsAMethodTo(t *testing.T) {
	isolateConverterPathGlobals(t)

	const production = `package main

import "fmt"

type plain int

func (p plain) Size() int { return int(p) }

func reset() {}

func main() {
	reset()
	fmt.Println(plain(1).Size())
}
`

	const test = `package main

import "testing"

func (p plain) reset() {}

func TestSize(t *testing.T) {
	size := plain.Size
	plain(0).reset()

	if size(plain(2)) != 2 {
		t.Fatal("method expression")
	}
}
`

	program := convertMethodHolderFixture(t, map[string]string{"main.go": production, "main_test.go": test})

	requireEmitted(t, "the program's package_info.cs", program["package_info.cs"], []string{"partial class mainᴛplain;"}, nil)
	requireEmitted(t, "the program's main.cs", program["main.cs"], []string{"internal static nint Size⁔(/*this*/ plain p) { return (nint)p; }"}, []string{"partial class mainᴛplain"})
	requireEmitted(t, "the program's package_info.cs", program["package_info.cs"], []string{"partial class mainᴛplain {\n"}, nil)
	requireEmitted(t, "main_test.cs", convertMainTestsVariant(t, production, test), []string{"global::go.mainᴛplain.Size))"}, nil)
}

const foreignConstraintHolderLibSource = `package lib

type Text interface{ ~[]byte | ~string }
`

const foreignConstraintHolderSource = `package iterators

import "example.com/app/lib"

type Iterator[T lib.Text] struct{ data T }

func (it *Iterator[T]) SetText(data T) { it.data = data }

type Window[K comparable, T lib.Text] struct {
	key  K
	data T
}

func (w Window[K, T]) Len() int { return len(w.data) }
`

// A generic type's holder is named by the type's identifier alone, whatever its type parameters' constraints
// are. A constraint from a package whose import path has a slash (example.com/app/lib) used to reach the
// holder's name through the type's string form, where the last-slash strip left the constraint's own name and
// the closing bracket: `mainᴛText>` for every such type, two types sharing one broken holder (CS1022), the
// emission of github.com/clipperhouse/uax29/v2's iterators package. The fixture is a library, as that package
// is: package main keeps its methods in its package class unless a name clashes, so it writes no holder here.
func TestGenericHolderIsNamedByTheTypeWhateverItsConstraint(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"iterators.go": foreignConstraintHolderSource},
		map[string]string{"lib/lib.go": foreignConstraintHolderLibSource})

	requireEmitted(t, "iterators.cs", emitted["iterators.cs"], []string{
		"internal static void SetText⁔<T>(/*this*/ ref Iterator<T> it, T data)\n",
		"internal static nint Len⁔<K, T>(/*this*/ Window<K, T> w)\n",
	}, []string{
		"Text>",
		"partial class iteratorsᴛ",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"[GoMethodHost(typeof(iterators_package.Iterator<>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class iteratorsᴛIterator;\n",
		"[GoMethodHost(typeof(iterators_package.Window<,>)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class iteratorsᴛWindow;\n",
		"partial class iteratorsᴛIterator {\n",
		"partial class iteratorsᴛWindow {\n",
		"public static void SetText<T>(this ref Iterator<T> it, T data) where T : /* lib.Text */ IByteSeq<T, byte>, new() => iterators_package.SetText⁔<T>(ref it, data);\n",
	}, []string{
		"Text>",
	})
}

const forwarderNamespaceScopeSource = `package buf

import (
	"io"
	"unsafe"
)

type Buffer struct{ n int }

type Type int

func (b Buffer) Grow(t Type) Buffer { return Buffer{n: b.n + int(t)} }

func (b *Buffer) Add(xs ...any) int {
	b.n += len(xs)
	return b.n
}

func (b Buffer) Many(ts ...Type) int { return len(ts) + b.n }

func (b *Buffer) At(p unsafe.Pointer) unsafe.Pointer { return unsafe.Add(p, b.n) }

type slice struct{ lo, hi int }

func (b Buffer) Span(data []byte) slice { return slice{hi: len(data) + b.n} }

func (b Buffer) WriteTo(w io.Writer) (int64, error) { return 0, nil }
`

// A forwarder repeats its body's spelling (`this Buffer b`), which in the package class finds the package's nested
// type first. package_info.cs reads it at namespace scope, where the project's `using System;` makes `Buffer` and
// `Type` System's as much as the package's (CS0104) among the compilation unit's directives; so the file repeats its
// `using static` right after the namespace declaration, as each converted file declares it, where a declaration's
// own usings are searched first. The aliases the forwarders name follow it: a variadic parameter's `ꓸꓸꓸT`, whose
// referent resolves there as in the body's file. unsafe.Pointer is spelled through a file alias package_info.cs
// does not declare, so a forwarder names its package class, as an assembly attribute does (`unsafe_package.Pointer`).
// The receiver keeps the bare spelling the generators read a method set by. An imported package's alias is repeated
// there too: inside the namespace a qualifier such as `io` finds the namespace `go.io` first (io/fs makes one), so
// `io.Writer` reads the package class only through an alias the declaration itself carries, as each converted file's
// does.
func TestAForwarderNamesItsPackagesTypesAndVariadicAliasesForNamespaceScope(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"buf.go": forwarderNamespaceScopeSource})

	requireEmitted(t, "buf.cs", emitted["buf.cs"], []string{
		"internal static Buffer Grow⁔(/*this*/ Buffer b, Type t)",
		"using ꓸꓸꓸany = variadic<any>;",
	}, nil)

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"namespace go.example.com.app;\n\nusing static go.example.com.app.buf_package;\nusing io = go.io_package;\nusing ꓸꓸꓸType = variadic<buf_package.Type>;\nusing ꓸꓸꓸany = variadic<any>;\n\n",
		"public static (int64, error) WriteTo(this Buffer b, io.Writer w) => buf_package.WriteTo⁔(b, w);\n",
		"public static Buffer Grow(this Buffer b, Type t) => buf_package.Grow⁔(b, t);\n",
		"public static nint Add(this ref Buffer b, params ꓸꓸꓸany xsʗp) => buf_package.Add⁔(ref b, xsʗp);\n",
		"public static nint Many(this Buffer b, params ꓸꓸꓸType tsʗp) => buf_package.Many⁔(b, tsʗp);\n",
		"public static unsafe_package.Pointer At(this ref Buffer b, unsafe_package.Pointer p) => buf_package.At⁔(ref b, p);\n",
		"public static Δslice Span(this Buffer b, slice<byte> data) => buf_package.Span⁔(b, data);\n",
	}, []string{
		"(this buf_package.Buffer b",
		"@unsafe.",
	})
}

const handOwnedHolderSource = `package app

type gptr uintptr

func (g gptr) ptr() int { return int(g) }

func (g gptr) set(v int) gptr { return gptr(v) }

type kept struct{ n int }

func (k kept) N() int { return k.n }

func Use() int { return gptr(1).ptr() + kept{}.N() }
`

// A method whose C# a hand-owned file supplies (manualConversionFuncs) is emitted as a placeholder; its hand-written
// extension method stands in the holder, `partial class appᴛgptr`, which only package_info.cs declares static. So the
// holder is declared as for a converted method, even where every method of the type is hand-owned, and no forwarder
// is written for a body that does not exist.
func TestAHandOwnedMethodKeepsItsHoldersDeclaration(t *testing.T) {
	displacedFixture(t, "example.com/app", "gptr.ptr", "gptr.set")

	emitted := convertMethodHolderFixture(t, map[string]string{"app.go": handOwnedHolderSource})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"[GoMethodHost(typeof(app_package.gptr)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class appᴛgptr;\n",
		"[GoMethodHost(typeof(app_package.kept)), EditorBrowsable(EditorBrowsableState.Never)] public static partial class appᴛkept;\n",
		"internal static nint N(this kept k) => app_package.N⁔(k);\n",
	}, []string{
		"ptr⁔",
		"set⁔",
	})
}

// Under a `go.go` namespace (a go/* package), a `go.`-rooted directive inside the namespace declaration finds `go.go`
// first, so the namespace-level copies of package_info.cs's compilation-unit directives are rooted at global::, as a
// converted file's `using static` is (globalQualifyRooted).
func TestForwarderDirectivesUnderAShadowedRootTakeGlobal(t *testing.T) {
	savedNamespace := packageNamespace
	t.Cleanup(func() { packageNamespace = savedNamespace })

	lines := []string{
		"using iter = go.iter_package;",
		"using go;",
		"using static go.go.token_package;",
		"",
		"namespace go.go;",
		"",
		"[GoPackage(\"token\")]",
	}

	for _, test := range []struct {
		namespace string
		want      []string
	}{
		{"go.go", []string{"using static global::go.go.token_package;", "using iter = global::go.iter_package;", "using ꓸꓸꓸжFile = variadic<ж<token_package.File>>;"}},
		{"go.example", []string{"using static go.go.token_package;", "using iter = go.iter_package;", "using ꓸꓸꓸжFile = variadic<ж<token_package.File>>;"}},
	} {
		packageNamespace = test.namespace
		got := insertForwarderAliasDirectives(append([]string(nil), lines...), []string{"using ꓸꓸꓸжFile = variadic<ж<token_package.File>>;"}, true)
		want := append(append(append([]string(nil), lines[:5]...), ""), append(test.want, lines[5:]...)...)

		if strings.Join(got, "\n") != strings.Join(want, "\n") {
			t.Errorf("under namespace %s the directive block is\n%s\nwant\n%s", test.namespace, strings.Join(got, "\n"), strings.Join(want, "\n"))
		}
	}
}
