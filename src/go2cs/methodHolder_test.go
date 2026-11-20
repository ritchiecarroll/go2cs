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

// A method is a member of its receiver type's holder, a class beside the package class that the file
// switches into in place, and the holder is declared once in package_info.cs with the type it holds.
func TestMethodsAreEmittedInTheirReceiverTypesHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"using static main_package;\n",
		"partial class main_package {\n",
		"\n} partial class mainᴛBox {\n\npublic static nint Limit(this Box b)",
		"\n} partial class main_package {\n\ninternal static UntypedInt other => 1;",
		"} // end main_package\n",
	}, []string{
		"Δ",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"// <MethodHolders>\n",
		"[GoMethodHost(typeof(main_package.Box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛBox;\n",
		"// </MethodHolders>\n",
	}, nil)
}

// A constant, a variable and a function named like a method of the holder are written through the
// file's alias of its own package class; a name no method of the holder shares stays bare; and no
// declaration takes a rename because a method shares its name.
func TestHolderQualifiesThePackageNamesItsMethodsShare(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"using main = main_package;\n",
		"{ return b.n + (nint)main.Limit; }",
		"main.Count++;",
		"return b.n + main.Count;",
		"var f = main.Scale;",
		"return main.Scale(b.n) + f(1);",
		"{ return b.n + (nint)other; }",
		"public static UntypedInt Limit => 7;",
		"public static nint Count = 3;",
		"public static nint Scale(nint n)",
		"public static Kind Kind(this Box b) { return (Kind)b.n; }",
	}, []string{
		"main.other",
		"main_package.Limit",
	})
}

// A method expression names the method's static form in the holder.
func TestMethodExpressionNamesTheHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": methodHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"((Func<Box, nint>)(mainᴛBox.Limit))",
		"((Func<ж<Box>, nint>)(mainᴛBox.Count))",
	}, nil)
}

// A file whose methods share no package name takes no alias line, and a file without a method takes
// neither line. The package is a library: package main keeps such methods in its package class.
func TestSelfAliasIsDeclaredOnlyWhereAHolderUsesIt(t *testing.T) {
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

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{"using static app_package;\n"}, []string{"using app = "})
	requireEmitted(t, "plain.cs", emitted["plain.cs"], nil, []string{"using app = ", "using static app_package;", "partial class appᴛ"})
}

// Where the alias's name means something else, the package class is written out: for a function that
// declares a variable of that name, and for a holder that declares a method of that name. A function
// named like its package (Go's main) does not take the name from the alias.
func TestHolderFallsBackToThePackageClassWhereTheAliasIsShadowed(t *testing.T) {
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
		"return main + (nint)main_package.Limit;",
		"internal static nint Scale(this meter m, nint main) { return main + (nint)main_package.Limit; }",
		"internal static nint Count(this meter m) { return (nint)main.Limit + m.main; }",
		"internal static nint Limit(this odd o) { return (nint)main_package.Limit + o.main(); }",
	}, nil)
}

// A package that declares a type of its own name takes no alias: the alias would take the type's name
// in every holder of the file.
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

	requireEmitted(t, "tone.cs", emitted, []string{
		"} partial class toneᴛtone {",
		"{ return tone_package.Base * 2; }",
	}, []string{"using tone = "})
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

// The four rules below existed package-wide while every method stood in the package class. Each now
// holds only inside the holder that declares the method, and two of them hold nowhere.

// A built-in is written through `builtin` only inside the holder that declares a method of its name.
func TestBuiltinIsQualifiedOnlyInTheHolderThatDeclaresItsName(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{
		"internal static nint cap(this words w) { return builtin.cap(w) + 1; }",
		"internal static nint size(this other o, words w) { return cap(w) + ",
		"w.cap(), cap(w), ",
	}, nil)

	if got := strings.Count(emitted, "builtin.cap("); got != 1 {
		t.Errorf("main.cs qualifies cap through builtin %d times, want 1: only in the holder of words\n%s", got, emitted)
	}
}

// An import is written through its package class only inside the holder that declares a method of
// its name; every other class of the package keeps the alias.
func TestImportIsQualifiedOnlyInTheHolderThatDeclaresItsName(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": narrowedRulesSource})["main.cs"]

	requireEmitted(t, "main.cs", emitted, []string{
		"internal static nint bits(this words w) { return math.bits_package.Len64(w[0]); }",
		"cap(w) + bits.Len64((uint64)o.n); }",
		"w.bits(), bits.Len64(w[0]), ",
	}, nil)

	if got := strings.Count(emitted, "bits_package.Len64("); got != 1 {
		t.Errorf("main.cs qualifies bits through its package class %d times, want 1: only in the holder of words\n%s", got, emitted)
	}
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

	requireEmitted(t, "main.cs", emitted, []string{"internal static void Sort(this words w) { main.Sort(w); }"}, []string{"(sort.Interface)(w)"})
}

// A type named like a METHOD of the holder a body stands in. A C# type position reads only types,
// so the type keeps its bare name in every declaration, literal, conversion and type argument. Two
// forms read the name as an EXPRESSION first, where the holder's method group binds: a box
// accessor (`Tag.ᏑValue`, CS0119) and a type-switch label with no binding (`case Tag:`, CS8917).
// The accessor names the package class, and the label takes the discard form, which is a type
// position. Outside the holder both forms stay as they were.
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

func TestTypeNamedLikeAMethodOfItsHolderIsReadAsAType(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": typeNamedLikeMethodSource})["main.cs"]

	inside, outside, found := strings.Cut(emitted, "} partial class main_package {")

	if !found {
		t.Fatalf("main.cs does not return to the package class after the holder:\n%s", emitted)
	}

	requireEmitted(t, "the holder of Chest", inside, []string{
		"public static Tag Tag(this ref Chest c) { return c.tag; }",
		"var ptr = Ꮡ(new Tag(id: 3));",
		"var valueAddr = ptr.of(main_package.Tag.ᏑValue);",
		"case Tag _: {",
		"case ж<Tag> _:",
		"slice<Tag>(",
	}, []string{
		"ptr.of(Tag.ᏑValue)",
		"case Tag: {",
	})

	requireEmitted(t, "the package class", outside, []string{
		"var valueAddr = ptr.of(Tag.ᏑValue);",
		"case Tag: {",
	}, []string{
		"main_package.Tag",
		"case Tag _:",
	})
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

// A package-level variable that follows a method in file order stands in the package class, where a
// method's name hides nothing: its function literal names the package's function and the built-in
// bare. Only the bodies of the two methods are qualified.
func TestDeclarationAfterAMethodIsNotReadAsInsideItsHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": holderEndsWithItsMethodSource})
	main := emitted["main.cs"]

	requireEmitted(t, "main.cs", main, []string{
		"return b.n + main.Limit();",
		"return builtin.len(w);",
	}, []string{
		"main_package.Limit()",
	})

	if got := strings.Count(main, "main.Limit()"); got != 1 {
		t.Errorf("main.cs qualifies Limit() %d times, want 1 (inside the holder of Box only)\n%s", got, main)
	}

	if got := strings.Count(main, "builtin.len("); got != 1 {
		t.Errorf("main.cs qualifies len( %d times, want 1 (inside the holder of words only)\n%s", got, main)
	}
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
// namespace inside a method holder, in a type position as in any other: a holder is not nested in the
// package class, and the namespace's own members are read before the file's `using static`. The holder
// names the type through the package class; the package class, where the nested type is read first,
// keeps the bare name. A package in any other namespace (TestHolderKeepsATypeNoNamespaceHidesBare) has
// no such child, and its holder keeps the bare name too.
func TestHolderNamesATypeANamespaceHidesThroughThePackageClass(t *testing.T) {
	emitted := convertMethodHolderModule(t, "go2cs/app", map[string]string{"main.go": holderHiddenTypeSource})
	main := emitted["main.cs"]

	if !strings.HasPrefix(main, "namespace go;") {
		t.Fatalf("main.cs is not in the root namespace, where `go.sync` is a child of the package's own\n%s", main)
	}

	holderStart := strings.Index(main, "partial class appᴛsync {")
	holderEnd := strings.LastIndex(main, "} partial class app_package {")

	if holderStart == -1 || holderEnd == -1 || holderEnd < holderStart {
		t.Fatalf("main.cs has no holder of sync followed by the package class\n%s", main)
	}

	holder, outside := main[holderStart:holderEnd], main[:holderStart]+main[holderEnd:]

	requireEmitted(t, "the holder of sync", holder, []string{
		"bump(this ref app_package.sync s, ",
		"app_package.sync again(this app_package.sync s)",
		"new app_package.sync(label: s.label",
		"slice<app_package.sync> all(this app_package.sync s)",
		"other._<app_package.sync>(",
	}, []string{
		" sync s",
		"<sync>",
		"new sync(",
		"(sync)",
	})

	requireEmitted(t, "the package class", outside, []string{
		"partial struct sync {",
		"internal static sync fresh()",
		"new sync(label: \"fresh\")",
		"each(slice<sync> all)",
	}, []string{
		"app_package.sync",
	})
}

func TestHolderKeepsATypeNoNamespaceHidesBare(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": holderHiddenTypeSource})
	main := emitted["main.cs"]

	requireEmitted(t, "main.cs", main, []string{
		"namespace go.example.com;",
		"bump(this ref sync s, ",
		"internal static sync again(this sync s)",
	}, []string{
		"app_package.sync",
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
// embedded interface) is one of the struct's methods, so it is a member of the struct's holder, beside the
// methods Go declares on it. In the package class it shared a class with the type of its name: CS0102,
// `Driver` beside `Driver`. The next type of a grouped declaration is back in the package class.
func TestPromotedForwarderIsEmittedInItsStructsHolder(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{"main.go": promotedForwarderHolderSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"\n} partial class mainᴛcounting {\n\n// Go method set entry for the promoted 'Connector.Driver()'",
		"internal static Driver Driver(this counting recvᴛ) => recvᴛ.Connector.Driver();\n\n} partial class main_package {\n\npartial struct after {",
		"\n} partial class mainᴛcounting {\n\ninternal static @string Name(this ref counting c)",
	}, []string{
		"Δ",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"[GoMethodHost(typeof(main_package.counting)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcounting;\n",
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
// package class the class's own member wins; a method holder is not nested in the package class, so there
// the two using-statics supply a name both packages declare with equal standing (CS0229, runtime's
// hashSize in hash_test.go; for a type, net/http's dumpConn, CS0104). The holder writes such a name through
// its own package's class, which is the binding Go gives it. A name only the package declares, and every
// name outside a holder, stays bare. The package is a library: package main keeps these methods in its
// package class, where its own members win.
func TestHolderQualifiesAnOwnNameADotImportedPackageAlsoDeclares(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"main.go": dotImportHolderMainSource},
		map[string]string{"lib/lib.go": dotImportHolderLibSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"using static go.example.com.app.lib_package;\n",
		"return s.n + (nint)prog.size + prog.shared + alone;",
		"return (nint)size + shared + alone;",
		"Sum(this prog_package.pair p, single q) { return p.x + q.y + new prog_package.pair(x: 1).x; }",
	}, []string{
		"prog.alone",
		"lib_package.size",
	})
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
// whose forwarder for an embedded interface's method is named so (TestPromotedForwarderIsEmittedInItsStructsHolder).
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
		"partial class mainᴛwithFunc {",
		"partial class mainᴛwithBuiltin {",
		"partial class mainᴛwithImport {",
		"partial class mainᴛwithGolib {",
		"partial class mainᴛwithDot {",
		"partial class mainᴛwithDotExported {",
	}, []string{
		"partial class mainᴛplain",
		"mainᴛplain.",
		"((Func<plain, nint>)(Size))",
		"=> Size(",
	})

	for _, kept := range []string{"withFunc", "withBuiltin", "withImport", "withGolib", "withDot", "withDotExported"} {
		requireEmitted(t, "package_info.cs", packageInfo, []string{"partial class mainᴛ" + kept + ";"}, nil)
	}

	requireEmitted(t, "package_info.cs", packageInfo, nil, []string{"mainᴛplain"})

	// A library keeps the holder of the same type: its classes are a consumer's surface.
	library := convertMethodHolderFixture(t, map[string]string{"main.go": `package app

type plain struct{ n int }

func (p plain) Size() int { return p.n }
`})

	requireEmitted(t, "the library's main.cs", library["main.cs"], []string{"partial class appᴛplain {"}, nil)
	requireEmitted(t, "the library's package_info.cs", library["package_info.cs"], []string{"partial class appᴛplain;"}, nil)
}

// A file closes its own class with the class's name, and a method holder its last method left open with a bare
// brace, as a holder closes wherever the next declaration switches class.
func TestFileThatEndsInAHolderClosesItBare(t *testing.T) {
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

	if !strings.HasSuffix(box, "public static nint Size(this Box b) { return b.n; }\n\n}\n") {
		t.Errorf("box.cs does not close the holder of Box with a bare brace\n%s", box)
	}

	requireEmitted(t, "box.cs", box, nil, []string{"// end appᴛBox", "// end app_package"})

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
	requireEmitted(t, "the program's main.cs", program["main.cs"], []string{"partial class mainᴛplain {"}, nil)
	requireEmitted(t, "main_test.cs", convertMainTestsVariant(t, production, test), []string{"global::go.mainᴛplain.Size))"}, nil)
}

const foreignConstraintHolderLibSource = `package lib

type Text interface{ ~[]byte | ~string }
`

const foreignConstraintHolderMainSource = `package main

import (
	"fmt"

	"example.com/app/lib"
)

type Iterator[T lib.Text] struct{ data T }

func (it *Iterator[T]) SetText(data T) { it.data = data }

type Window[K comparable, T lib.Text] struct {
	key  K
	data T
}

func (w Window[K, T]) Len() int { return len(w.data) }

func main() {
	it := &Iterator[string]{}
	it.SetText("go")
	fmt.Println(len(it.data), Window[int, []byte]{data: []byte("abc")}.Len())
}
`

// A generic type's holder is named by the type's identifier alone, whatever its type parameters' constraints
// are. A constraint from a package whose import path has a slash (example.com/app/lib) used to reach the
// holder's name through the type's string form, where the last-slash strip left the constraint's own name and
// the closing bracket: `mainᴛText>` for every such type, two types sharing one broken holder (CS1022), the
// emission of github.com/clipperhouse/uax29/v2's iterators package.
func TestGenericHolderIsNamedByTheTypeWhateverItsConstraint(t *testing.T) {
	emitted := convertMethodHolderTree(t, "example.com/app",
		map[string]string{"main.go": foreignConstraintHolderMainSource},
		map[string]string{"lib/lib.go": foreignConstraintHolderLibSource})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{
		"\n} partial class mainᴛIterator {\n",
		"\n} partial class mainᴛWindow {\n",
	}, []string{
		"Text>",
	})

	requireEmitted(t, "package_info.cs", emitted["package_info.cs"], []string{
		"[GoMethodHost(typeof(main_package.Iterator<>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛIterator;\n",
		"[GoMethodHost(typeof(main_package.Window<,>)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛWindow;\n",
	}, []string{
		"Text>",
	})
}
