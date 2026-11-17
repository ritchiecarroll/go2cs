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
// neither line.
func TestSelfAliasIsDeclaredOnlyWhereAHolderUsesIt(t *testing.T) {
	emitted := convertMethodHolderFixture(t, map[string]string{
		"main.go": `package main

import "fmt"

type Box struct{ n int }

func (b Box) Size() int { return b.n + unit }

func main() { fmt.Println(Box{1}.Size(), double(2)) }
`,
		"plain.go": `package main

const unit = 1

func double(n int) int { return n * 2 }
`,
	})

	requireEmitted(t, "main.cs", emitted["main.cs"], []string{"using static main_package;\n"}, []string{"using main = "})
	requireEmitted(t, "plain.cs", emitted["plain.cs"], nil, []string{"using main = ", "using static main_package;", "partial class mainᴛ"})
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

const holderHiddenTypeSource = `package main

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

func main() {
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

	holderStart := strings.Index(main, "partial class mainᴛsync {")
	holderEnd := strings.LastIndex(main, "} partial class main_package {")

	if holderStart == -1 || holderEnd == -1 || holderEnd < holderStart {
		t.Fatalf("main.cs has no holder of sync followed by the package class\n%s", main)
	}

	holder, outside := main[holderStart:holderEnd], main[:holderStart]+main[holderEnd:]

	requireEmitted(t, "the holder of sync", holder, []string{
		"bump(this ref main_package.sync s, ",
		"main_package.sync again(this main_package.sync s)",
		"new main_package.sync(label: s.label",
		"slice<main_package.sync> all(this main_package.sync s)",
		"other._<main_package.sync>(",
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
		"main_package.sync",
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
		"main_package.sync",
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
