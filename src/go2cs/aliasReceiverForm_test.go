// aliasReceiverForm_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the receiver FORM of a method whose receiver type is written through an alias
// (`type AttrNode = Node`, then `func (n *AttrNode) Mode()`). Such a method is the method of the type
// the alias denotes, so it takes the form that method takes under the type's own name: the box as its
// receiver (`this ж<T> Ꮡn`) when the body observes the receiver as a pointer, `this ref T n` otherwise.
// The selection of box-receiver methods once kept a method only when the receiver's element was a
// `*types.Named`; an alias is a `*types.Alias` there, so the method was dropped before its body was
// read and was always taken by reference. Its nil guard then compiled against the struct's own
// comparison with nil and read the struct through a nil receiver (docs/phase4/DESIGN-alias-receiver-form.md).
//
// The fixture is the behavioral project's own source, so the two cannot drift apart.

package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// aliasReceiverFixture names a behavioral source whose receivers are written through aliases, and how
// to rewrite it with each alias replaced by what it denotes.
type aliasReceiverFixture struct {
	project   string            // the behavioral project that holds main.go
	decls     *regexp.Regexp    // the alias declarations, removed for the named spelling
	declCount int               // how many declarations decls must find
	goNames   *strings.Replacer // each alias to what it denotes, in the Go source
	csNames   *strings.Replacer // each alias to what it denotes, in the emission
}

var (
	// Aliases of the receiver's ELEMENT type: `type AttrNode = Node`, `func (n *AttrNode) M()`.
	aliasReceiverForms = aliasReceiverFixture{
		project:   "AliasReceiverForms",
		decls:     regexp.MustCompile(`(?m)^(?:\t(?:AttrNode|Chain)\s+= .*|type ListAlias = List)\n`),
		declCount: 3,
		goNames:   strings.NewReplacer("AttrNode", "Node", "Chain", "Node", "ListAlias", "List"),
		csNames:   strings.NewReplacer("AttrNode", "Node", "Chain", "Node", "ListAlias", "List"),
	}

	// Aliases of the POINTER type: `type NodePtr = *Node`, `func (p NodePtr) M()`, `var q NodePtr`.
	pointerAliasForms = aliasReceiverFixture{
		project:   "PointerAliasForms",
		decls:     regexp.MustCompile(`(?m)^(?:\t(?:NodePtr|ChainPtr)\s+= .*|type ListPtr = \*List)\n`),
		declCount: 3,
		goNames:   strings.NewReplacer("(NodePtr).", "(*Node).", "NodePtr", "*Node", "ChainPtr", "*Node", "ListPtr", "*List"),
		csNames:   strings.NewReplacer("NodePtr", "ж<Node>", "ChainPtr", "ж<Node>", "ListPtr", "ж<List>"),
	}
)

// convertAliasReceiverFixture converts the behavioral source, as written (alias-typed receivers) or with
// every alias replaced by the name it denotes, and returns the emitted main.cs.
func convertAliasReceiverFixture(t *testing.T, fixture aliasReceiverFixture, named bool) string {
	t.Helper()

	data, err := os.ReadFile(filepath.Join("..", "tests", "Behavioral", fixture.project, "main.go"))

	if err != nil {
		t.Fatalf("read the fixture: %v", err)
	}

	// A checkout that converts line endings hands the fixture over with CRLF; the declaration patterns
	// are written against LF.
	source := strings.ReplaceAll(string(data), "\r\n", "\n")

	if named {
		if got := len(fixture.decls.FindAllString(source, -1)); got != fixture.declCount {
			t.Fatalf("the fixture declares %d aliases this test knows, want %d", got, fixture.declCount)
		}

		source = fixture.goNames.Replace(fixture.decls.ReplaceAllString(source, ""))
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/recvform\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), source)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "recvform", "main.cs"))
}

// TestAliasReceiverTakesTheNamedForm pins the two shapes the defect showed on: the nil guard and the
// address of a receiver field, each on a receiver written through an alias.
func TestAliasReceiverTakesTheNamedForm(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertAliasReceiverFixture(t, aliasReceiverForms, false)

	for _, want := range []string{
		// The nil guard compares the BOX, and the value alias binds without a dereference.
		"Mode(this ж<AttrNode> Ꮡn, nint def)",
		"ref var n = ref Ꮡn.DerefOrNull();",
		"if (Ꮡn == nil) {",
		// An alias of an alias, and an alias of a named slice.
		"IsNil(this ж<Chain> Ꮡn) { return Ꮡn == nil; }",
		"Len(this ж<ListAlias> Ꮡl)",
		// The address of a receiver field goes through the receiver's box.
		"KindPtr(this ж<AttrNode> Ꮡn) { return Ꮡn.Ꮡkind; }",
		// A body that only reads fields keeps the by-reference form, as it does under the type's own name.
		"Announce(this ref AttrNode n)",
		// A value receiver is not a pointer receiver, whatever its spelling.
		"Kind(this AttrNode n)",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("the emission lacks %q", want)
		}
	}

	for _, refuse := range []string{
		"Mode(this ref AttrNode n",
		"IsNil(this ref Chain n",
		"Len(this ref ListAlias l",
		"if (n == nil) {",
	} {
		if strings.Contains(emitted, refuse) {
			t.Errorf("the emission holds %q: an alias-typed receiver that its body observes was taken by reference", refuse)
		}
	}
}

// TestAliasReceiverEmitsAsNamedReceiver converts the fixture twice, with its receivers written through
// aliases and with each alias replaced by the name it denotes, and requires ONE emission once the alias
// names are mapped. A receiver-type read that forgets to resolve an alias fails it on whichever shape
// that read decides, without this test having to name the shape.
func TestAliasReceiverEmitsAsNamedReceiver(t *testing.T) {
	isolateConverterPathGlobals(t)

	requireAliasEmissionMatchesNamed(t, aliasReceiverForms)
}

// requireAliasEmissionMatchesNamed converts the fixture as written and with each alias replaced by what
// it denotes, and fails on every line the two emissions differ in once the alias names are mapped.
func requireAliasEmissionMatchesNamed(t *testing.T, fixture aliasReceiverFixture) {
	t.Helper()

	normalise := func(emitted string) []string {
		var lines []string

		for _, line := range strings.Split(strings.ReplaceAll(emitted, "\r\n", "\n"), "\n") {
			if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "global using ") {
				continue
			}

			lines = append(lines, line)
		}

		return lines
	}

	alias := normalise(fixture.csNames.Replace(convertAliasReceiverFixture(t, fixture, false)))
	named := normalise(convertAliasReceiverFixture(t, fixture, true))

	if len(named) < 100 {
		t.Fatalf("the named-receiver emission has %d lines, want the whole program", len(named))
	}

	differing := 0

	for index := 0; index < len(alias) || index < len(named); index++ {
		var left, right string

		if index < len(alias) {
			left = alias[index]
		}

		if index < len(named) {
			right = named[index]
		}

		if left != right {
			differing++

			if differing <= 12 {
				t.Errorf("line %d differs:\n  alias receivers: %s\n  named receivers: %s", index+1, left, right)
			}
		}
	}

	if differing > 12 {
		t.Errorf("%d lines differ in all", differing)
	}
}

// TestPointerAliasEmitsAsPointer is the same requirement for a pointer type written through an alias
// (`type NodePtr = *Node`): as a receiver (`func (p NodePtr) M()`, which Go accepts as the pointer
// method `(*Node).M`) and as the type of a parameter, a local, a field, an element, a map value and a
// channel element. The converter once asked "is this a pointer?" of the type as written, and an alias
// answered no. The alias keeps its name wherever Go writes it in a type position.
func TestPointerAliasEmitsAsPointer(t *testing.T) {
	isolateConverterPathGlobals(t)

	requireAliasEmissionMatchesNamed(t, pointerAliasForms)

	emitted := convertAliasReceiverFixture(t, pointerAliasForms, false)

	for _, want := range []string{
		// The box form, under the alias's own name.
		"Mode(this NodePtr Ꮡp, nint def)",
		"IsNil(this ChainPtr Ꮡp) { return Ꮡp == nil; }",
		// By reference there is no alias spelling: `ref` names the element.
		"SetKind(this ref Node p, nint kind) { p.kind = kind; }",
		"Push(this ref List l, nint v)",
		// A field, a parameter and an element keep the alias name.
		"internal NodePtr attrs;",
		"viaParam(NodePtr Ꮡq)",
		"slice<NodePtr>",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("the emission lacks %q", want)
		}
	}
}

// pointerTypeReadsThatKeepTheSpelling are the functions that read a pointer type AS WRITTEN on
// purpose: the type-name renderers, which print an alias of a pointer type by its own name.
var pointerTypeReadsThatKeepTheSpelling = map[string]bool{
	"getAliasQualifiedTypeName": true,
	"getFullyQualifiedTypeName": true,
}

// unresolvedPointerTypeReads lists, as "function:line", every type assertion `X.(*types.Pointer)` in
// the source whose X is not resolved first: not `types.Unalias(...)`, not `....Underlying()`, and not
// a local name its function assigns only from such expressions.
func unresolvedPointerTypeReads(t *testing.T, fileName string, source any) []string {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, fileName, source, 0)

	if err != nil {
		t.Fatalf("parse %s: %v", fileName, err)
	}

	resolved := func(expr ast.Expr) bool {
		call, ok := expr.(*ast.CallExpr)

		if !ok {
			return false
		}

		selector, ok := call.Fun.(*ast.SelectorExpr)

		if !ok {
			return false
		}

		if selector.Sel.Name == "Underlying" {
			return true
		}

		pkg, ok := selector.X.(*ast.Ident)

		return ok && pkg.Name == "types" && selector.Sel.Name == "Unalias"
	}

	resolvedName := func(expr ast.Expr, function *ast.FuncDecl) bool {
		ident, ok := expr.(*ast.Ident)

		if !ok {
			return false
		}

		assigned, all := 0, true

		ast.Inspect(function, func(node ast.Node) bool {
			assign, ok := node.(*ast.AssignStmt)

			if !ok {
				return true
			}

			for i, lhs := range assign.Lhs {
				if name, ok := lhs.(*ast.Ident); ok && name.Name == ident.Name {
					assigned++

					if len(assign.Rhs) != len(assign.Lhs) || !resolved(assign.Rhs[i]) {
						all = false
					}
				}
			}

			return true
		})

		return assigned > 0 && all
	}

	var unresolved []string

	for _, decl := range file.Decls {
		function, ok := decl.(*ast.FuncDecl)

		if !ok || function.Body == nil || pointerTypeReadsThatKeepTheSpelling[function.Name.Name] {
			continue
		}

		ast.Inspect(function, func(node ast.Node) bool {
			assert, ok := node.(*ast.TypeAssertExpr)

			if !ok || assert.Type == nil || types.ExprString(assert.Type) != "*types.Pointer" {
				return true
			}

			if !resolved(assert.X) && !resolvedName(assert.X, function) {
				unresolved = append(unresolved, fmt.Sprintf("%s:%d", function.Name.Name, fset.Position(assert.Pos()).Line))
			}

			return true
		})
	}

	return unresolved
}

// TestPointerTypeReadsResolveAliases holds the rule the two fixtures above cannot hold shape by shape:
// a read that decides whether a type IS a pointer resolves an alias first, because `type NodePtr = *Node`
// is a `*types.Alias` to go/types and a bare `.(*types.Pointer)` answers no for it. 151 such reads were
// bare when this test was written; a new one fails here instead of on the first program that spells a
// pointer through an alias.
func TestPointerTypeReadsResolveAliases(t *testing.T) {
	// The detector first: one bare read among the resolved forms, and the renderers' exemption.
	sample := `package main

import "go/types"

func decide(t types.Type, v *types.Var) bool {
	_, bare := t.(*types.Pointer)
	_, viaAlias := types.Unalias(t).(*types.Pointer)
	_, viaUnderlying := v.Type().Underlying().(*types.Pointer)
	under := types.Unalias(t)
	_, viaName := under.(*types.Pointer)

	return bare || viaAlias || viaUnderlying || viaName
}

func getAliasQualifiedTypeName(t types.Type) bool {
	_, spelled := t.(*types.Pointer)

	return spelled
}
`

	if got := unresolvedPointerTypeReads(t, "sample.go", sample); len(got) != 1 || got[0] != "decide:6" {
		t.Fatalf("the detector reads %v in its sample, want [decide:6]", got)
	}

	files, err := filepath.Glob("*.go")

	if err != nil {
		t.Fatal(err)
	}

	scanned := 0

	for _, fileName := range files {
		if strings.HasSuffix(fileName, "_test.go") {
			continue
		}

		scanned++

		for _, site := range unresolvedPointerTypeReads(t, fileName, nil) {
			t.Errorf("%s: %s asserts `.(*types.Pointer)` on a type that may be an alias of a pointer type; "+
				"resolve it first (`types.Unalias(x).(*types.Pointer)`)", fileName, site)
		}
	}

	if scanned < 100 {
		t.Fatalf("scanned %d converter sources, want the whole converter", scanned)
	}
}
