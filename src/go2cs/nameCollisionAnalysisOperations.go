// nameCollisionAnalysisOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"

	"golang.org/x/tools/go/packages"
)

// The `performNameCollisionAnalysis` function analyzes the package for name collisions
// between constants/variables and method names. Resulting collisions are stored in the
// global `nameCollisions` map. This function is called for each package during the
// conversion process to ensure that any potential name collisions are identified and
// handled appropriately. This is important to avoid naming conflicts that could lead
// to runtime errors or unexpected behavior in the generated C# code, which is more
// strict about unique naming of discrete types than Go is in this case.

// emitterSpelledTypeNames are type names the converter's EMITTER spells independent of the Go
// source's own spelling — so a user-declared package-level TYPE of the same name must be
// package-scoped Δ-renamed (nameCollisions), never '@'-escaped or globally `reserved` (both
// would corrupt the emitter's legitimate spellings in every other package). Proven vectors:
// `any` — the `interface{}` rendering (`slice<any>` bound the user struct, CS0029 ×3);
// `rune` — the untyped rune-constant default (`c := 'x'` emits `rune c = 'x';`, CS0030);
// `nint`/`nuint` — the Go int/uint MAPPED spellings and C# native-int contextual keywords
// (`partial struct nint { internal nint d; }` is a CS0523 layout cycle, and '@' cannot fix a
// name-identity problem).
var emitterSpelledTypeNames = map[string]bool{
	"any": true, "rune": true, "nint": true, "nuint": true,
}

func performNameCollisionAnalysis(pkg *packages.Package) {
	// Track names of various declarations
	namedElementNames := make(map[string]bool)
	methodNames := make(map[string]bool)

	// A `-tests` variant universe mixes production files with `_test.go` files, but only the test
	// files are EMITTED — the production .cs on disk were converted from the production-only
	// universe and are recompiled into the test assembly as-is. Production symbol names are
	// therefore IMMUTABLE here: a collision a test file introduces must resolve by Δ-renaming the
	// TEST-side declarator (see the resolution loop and the dot-import scan below). Production
	// conversions have no `_test.go` files in pkg.Syntax, so these maps stay empty and the
	// function behaves exactly as before.
	elementInTestFile := make(map[string]bool)
	methodInProductionFile := make(map[string]bool)
	testMethodObjects := make(map[string][]types.Object)

	// Every package-level FuncDecl (method AND free function), keyed by name — the input to the
	// receiver-vs-first-parameter collision scan below, which needs the full signature, not just
	// the name sets above.
	funcDeclsByName := make(map[string][]packageFuncDecl)

	// Collect all named element names and method names (top-level declarations only)
	for _, file := range pkg.Syntax {
		isTestFile := strings.HasSuffix(strings.ToLower(filepath.Base(pkg.Fset.Position(file.Pos()).Filename)), "_test.go")

		for _, decl := range file.Decls {
			switch node := decl.(type) {
			case *ast.GenDecl:
				// Handle constants and variables at package level (not inside functions)
				if node.Tok == token.CONST || node.Tok == token.VAR {
					for _, spec := range node.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, name := range valueSpec.Names {
								// The blank identifier is a discard, never referenced, and the
								// value-spec visitor gives each blank a unique name — so a `_`
								// const/var must not be treated as colliding with a `func _()`
								// (a common stringer compile-time assertion). Otherwise every
								// `_` is Δ-prefixed to the same `Δ_` and they collide (CS0102).
								if name.Name == "_" {
									continue
								}

								namedElementNames[name.Name] = false

								if isTestFile {
									elementInTestFile[name.Name] = true
								}
							}
						}
					}
				}

				// Handle type declarations (structs, interfaces, type aliases)
				if node.Tok == token.TYPE {
					for _, spec := range node.Specs {
						if typeSpec, ok := spec.(*ast.TypeSpec); ok {
							if !typeSpec.Assign.IsValid() {
								namedElementNames[typeSpec.Name.Name] = true

								if isTestFile {
									elementInTestFile[typeSpec.Name.Name] = true
								}
							}
						}
					}
				}

			case *ast.FuncDecl:
				methodNames[node.Name.Name] = true

				if pkg.TypesInfo != nil {
					if obj := pkg.TypesInfo.Defs[node.Name]; obj != nil {
						if signature, ok := obj.Type().(*types.Signature); ok {
							funcDeclsByName[node.Name.Name] = append(funcDeclsByName[node.Name.Name],
								packageFuncDecl{Object: obj, Signature: signature, InTestFile: isTestFile})
						}
					}
				}

				if isTestFile {
					if pkg.TypesInfo != nil {
						if obj := pkg.TypesInfo.Defs[node.Name]; obj != nil {
							testMethodObjects[node.Name.Name] = append(testMethodObjects[node.Name.Name], obj)
						}
					}
				} else {
					methodInProductionFile[node.Name.Name] = true
				}
			}
		}
	}

	// A package method/function whose name is a Go built-in (`func (b *pageBits) clear()`) shadows
	// the using-static `go.builtin.<name>` for an unqualified free call in C#, so such built-in calls
	// must be emitted qualified (`builtin.<name>(…)`). Record which built-ins this package shadows.
	packageBuiltinShadows = make(map[string]bool)

	for name := range methodNames {
		// `recover` was excluded here for as long as a Go `recover()` emitted as the execution
		// context's lambda PARAMETER: a parameter is in scope wherever recover is legal and
		// correctly shadows a same-named package method, and there was no `builtin.recover` to
		// qualify to anyway. The GoFrame emission retires that parameter — `recover()` is the
		// static built-in now, reading the same thread-local slot the emitted catch parks the panic
		// in — so the shadow is live again and qualification is both possible and required:
		// text/template/parse declares `func (t *Tree) recover(errp *error)`, and inside its own
		// package class that extension method wins over the using-static import (CS7036 on the
		// nullary built-in call). It therefore takes the same treatment as every other shadowed
		// built-in, with no special case at all.
		if goBuiltinNames[name] {
			packageBuiltinShadows[name] = true
		}
	}

	// A package-level TYPE named after a spelling the EMITTER produces on its own (`any`,
	// `rune`, `nint`, `nuint` — see emitterSpelledTypeNames) shadows that spelling inside the
	// package class, breaking the converter's own emissions (`slice<any>` bound the user
	// struct, CS0029 ×3; `internal nint d;` inside `partial struct nint` is a CS0523 cycle).
	// Δ-rename every ident named it in THIS package (nameCollisions is package-scoped),
	// keeping the bare name bound to its emitter meaning. These names must never go in the
	// string-based `reserved` set: that would corrupt the emitter's legitimate spellings in
	// every OTHER package (see the comment on that set).
	for name, isType := range namedElementNames {
		if isType && emitterSpelledTypeNames[name] {
			nameCollisions[name] = true
		}
	}

	// A method/function name can also shadow an IMPORTED PACKAGE's using-alias inside the
	// package class (`func (s *byLiteral) sort(…)` vs `import "sort"` — `sort.Sort(…)`
	// bound the method group, CS0119, compress/flate). Record every method/function name;
	// the package-ident emission qualifies through the _package class when shadowed.
	packageFuncMethodNames = make(map[string]bool)
	packageTestAliasShadows = make(map[string]bool)

	for name := range methodNames {
		packageFuncMethodNames[name] = true
	}

	// A name the package's own `_test.go` half declares lands in the SAME package class when tests
	// are compiled and shadows a production file's using-alias there just as a production
	// declarator would. Fold the sibling half's declarator names into every production analysis so
	// ordinary and -tests conversion emit the same source spelling. Track names contributed ONLY by
	// tests separately so statement emission can explain the otherwise surprising qualification.
	for _, name := range siblingTestFuncMethodNames {
		if !methodNames[name] {
			packageTestAliasShadows[name] = true
		}

		packageFuncMethodNames[name] = true
	}

	// Find collisions (names that appear in both sets)
	for name, isType := range namedElementNames {
		if methodNames[name] {
			// B2 (test-variant coherence): a collision that exists ONLY because a `_test.go`
			// file declared a method over a production-declared element must NOT Δ-rename the
			// element — the production .cs on disk keeps the bare name, so renaming it here
			// splits one assembly into two disagreeing halves (strings' export_test.go method
			// `Replacer` over the production type: CS0102 + CS0246 ΔReplacer). Production names
			// are pinned; the TEST-side declarator is Δ-renamed instead — necessarily a METHOD
			// (Go keeps method names in a separate namespace; any other same-scope reuse is a Go
			// compile error) — and its reference sites follow via convIdent's isMethod arm. When
			// a PRODUCTION method also carries the name, the production universe had the same
			// collision and its emission already Δ-renamed the element, so the normal path below
			// stays consistent with the on-disk .cs.
			if !elementInTestFile[name] && !methodInProductionFile[name] && len(testMethodObjects[name]) > 0 {
				registerTestMethodRenames(testMethodObjects[name])
				continue
			}

			// Found a collision
			nameCollisions[name] = true

			// Add collision avoidance name as a type aliases to package info,
			// this way original name can be referenced as normal when using
			// the name from referenced package. The name will not collide in
			// a remote package because the type will have the package prefix.
			if getAccess(name) == "public" {
				var typePrefix string

				if !isType {
					typePrefix = "const:"
				}

				packageLock.Lock()
				exportedTypeAliases[getCoreSanitizedIdentifier(name)] = fmt.Sprintf("%s%s", typePrefix, getCollisionAvoidanceIdentifier(name))
				packageLock.Unlock()
			}
		}
	}

	// B9 (test-variant coherence, dot-import): a TEST-declared method whose name matches a
	// dot-imported foreign FUNCTION the variant references UNQUALIFIED hijacks every such call
	// site — Go keeps method names and dot-imported function names in separate namespaces, but
	// both land in the package class's member-lookup scope in C#, and the enclosing class's
	// method group always wins over `using static` imports (sort_test.go's dot-imported
	// `Sort(data)` bound example_keys_test.go's `By.Sort` extension: CS1501 ×14). Production
	// names are pinned (the foreign function keeps its bare emission at every call site); the
	// test method declarator is Δ-renamed. Only unqualified references conflict — a qualified
	// `sort.Sort(ps)` resolves through the package alias — so SelectorExpr Sels are excluded
	// from the scan; an unqualified reference to another package's package-level function can
	// only have arrived through a dot-import. The scan covers the WHOLE variant universe:
	// production files' on-disk .cs recompile into the same class, so their dot-imported call
	// sites are hijacked just the same.
	if len(testMethodObjects) > 0 && pkg.TypesInfo != nil {
		unqualifiedForeignFuncRefs := make(map[string]bool)
		selIdents := make(map[*ast.Ident]bool)

		for _, file := range pkg.Syntax {
			ast.Inspect(file, func(n ast.Node) bool {
				switch node := n.(type) {
				case *ast.SelectorExpr:
					// Parents are visited before children, so the Sel is marked before the
					// ident case below can reach it.
					selIdents[node.Sel] = true
				case *ast.Ident:
					if selIdents[node] {
						break
					}

					if fn, ok := pkg.TypesInfo.Uses[node].(*types.Func); ok &&
						fn.Pkg() != nil && fn.Pkg() != pkg.Types && fn.Signature().Recv() == nil {
						unqualifiedForeignFuncRefs[fn.Name()] = true
					}
				}

				return true
			})
		}

		for name, objects := range testMethodObjects {
			if unqualifiedForeignFuncRefs[name] && !methodInProductionFile[name] {
				registerTestMethodRenames(objects)
			}
		}
	}

	resolveReceiverParameterCollisions(funcDeclsByName)
}

// packageFuncDecl is one package-level FuncDecl — method or free function — with the signature
// and test/production origin the receiver-vs-first-parameter collision scan needs.
type packageFuncDecl struct {
	Object     types.Object
	Signature  *types.Signature
	InTestFile bool
}

// resolveReceiverParameterCollisions Δ-renames a `-tests` test-file declarator whose EMITTED C#
// signature is identical to a same-named sibling's because a METHOD's receiver becomes the
// extension method's leading `this` parameter. Go keeps method names and package-scope function
// names in separate namespaces, so `func (z nat) norm() nat` (nat.go) and `func norm(x nat) nat`
// (int_test.go) coexist legally; both emit into the package class as `norm(nat)` — `this` does not
// participate in C# signature identity — so the test variant fails to compile (math/big, CS0111).
//
// Production names are pinned exactly as in B2/B9 above: the production .cs on disk recompile into
// the test assembly unchanged, so only the TEST-side declarator may move. When BOTH sides are
// test-declared the free function is the one renamed, so the choice is deterministic and two
// colliding declarators never both become Δ-prefixed. A collision between two PRODUCTION
// declarators is deliberately left alone: it would equally break the production-only conversion,
// making it a different (and currently hypothetical — the 302-package corpus compiles clean) fix
// than test-variant coherence.
func resolveReceiverParameterCollisions(funcDeclsByName map[string][]packageFuncDecl) {
	for _, decls := range funcDeclsByName {
		if len(decls) < 2 {
			continue
		}

		for i, left := range decls {
			for _, right := range decls[i+1:] {
				if !emittedSignaturesCollide(left.Signature, right.Signature) {
					continue
				}

				// Rename the test-side declarator; with both test-declared, the free function
				// (the one WITHOUT a receiver) is renamed so the outcome does not depend on
				// declaration order.
				switch {
				case left.InTestFile && right.InTestFile:
					if left.Signature.Recv() == nil {
						registerTestMethodRenames([]types.Object{left.Object})
					} else {
						registerTestMethodRenames([]types.Object{right.Object})
					}
				case left.InTestFile:
					registerTestMethodRenames([]types.Object{left.Object})
				case right.InTestFile:
					registerTestMethodRenames([]types.Object{right.Object})
				}
			}
		}
	}
}

// emittedSignaturesCollide reports whether two same-named package-level signatures land on the
// same C# member signature once emitted. Only a method/free-function pair can: a method emits its
// receiver as the leading `this` parameter, so `func (r R) f(a A)` and `func f(r R, a A)` both
// become `f(R, A)`. Two methods differ by receiver type (Go forbids redeclaring one method on one
// type) and two free functions cannot share a package scope at all.
func emittedSignaturesCollide(left, right *types.Signature) bool {
	method, function := left, right

	if method.Recv() == nil {
		method, function = right, left
	}

	// Not a method/free-function pair — either two methods or two free functions.
	if method.Recv() == nil || function.Recv() != nil {
		return false
	}

	// Generic declarations are emitted with their type parameters, which keeps the C# signatures
	// distinct; comparing instantiated parameter types here would be meaningless anyway.
	if method.TypeParams().Len() != 0 || function.TypeParams().Len() != 0 ||
		method.RecvTypeParams().Len() != 0 || function.RecvTypeParams().Len() != 0 {
		return false
	}

	// A variadic tail emits as `params`, which C# still counts as one parameter of the same type,
	// so the arities must agree in the same way — but a variadic/non-variadic mismatch is a
	// genuine difference in the emitted member and must not be treated as a collision.
	if method.Variadic() != function.Variadic() {
		return false
	}

	methodParams, functionParams := method.Params(), function.Params()

	if methodParams.Len()+1 != functionParams.Len() {
		return false
	}

	if !types.Identical(method.Recv().Type(), functionParams.At(0).Type()) {
		return false
	}

	for i := 0; i < methodParams.Len(); i++ {
		if !types.Identical(methodParams.At(i).Type(), functionParams.At(i+1).Type()) {
			return false
		}
	}

	return true
}

// registerTestMethodRenames records `-tests` test-file method declarators that must emit (and be
// referenced) Δ-renamed to keep production symbol names immutable — see testMethodRenames in
// main.go for the session-scoping rationale. Lazy-initialized so direct unit-test drivers of a
// single variant conversion need no session setup.
func registerTestMethodRenames(objects []types.Object) {
	if testMethodRenames == nil {
		testMethodRenames = make(map[types.Object]bool)
	}

	for _, obj := range objects {
		testMethodRenames[obj] = true
	}
}

// heapIntrinsicIdent is the Go identifier that shadows golib's heap-boxing intrinsic. Unlike the
// name collisions above — which are Go declarations colliding with each OTHER through the emitter's
// naming rules — this one is a collision go2cs INVENTS: `heap` is an ordinary, legal Go identifier
// that no Go program is constrained by, and it only becomes a hazard because the converter emits
// address-taken locals through golib's `heap(value, out var Ꮡname)` helper, imported by every
// converted file via `using static go.builtin`.
const heapIntrinsicIdent = "heap"

// heapIntrinsicName is the spelling a heap-box emission must use for golib's boxing intrinsic in
// the CURRENT function. Bare `heap` normally — that is what the whole corpus reads — and the
// fully-qualified `builtin.heap` where a Go declaration of that name is in scope.
//
// A C# local, parameter or containing-class member wins simple-name lookup outright over a member
// imported by `using static`, so a Go body that both declares `heap` and needs a heap box emits
// `ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);` with `heap` bound to the DECLARATION
// — `CS0149: Method name expected`, which reads as an emitter defect at the box site rather than as
// a name collision (internal/trace's `heapDebugString(heap []*batchCursor)`, the whole of that
// package's 92-verdict build wall). Qualifying is the minimal remedy: it renames nothing the Go
// source chose, and it is conditional so the corpus stays byte-identical everywhere the collision
// does not exist.
//
// An IMPORT alias named `heap` (`import "container/heap"`, which the converter renders as
// `using heap = go.container.heap_package;`) is deliberately NOT treated as shadowing: a using-alias
// does not displace a `using static` method group in an invocation, proven by container/heap's own
// banked example_pq_test.cs, which carries both the alias and two heap-box emissions and compiles.
func (v *Visitor) heapIntrinsicName() string {
	if v.heapIntrinsicShadowed {
		return "builtin." + heapIntrinsicIdent
	}

	return heapIntrinsicIdent
}

// declaresHeapIntrinsicIdent reports whether node declares — anywhere inside it, nested function
// literals included — a Go object named `heap` that would win C# simple-name lookup over golib's
// intrinsic. Package names are excluded (see heapIntrinsicName); everything else that Defs records
// is a local, parameter, result, field, constant, type or function whose emitted C# name is exactly
// `heap`.
func (v *Visitor) declaresHeapIntrinsicIdent(node ast.Node) bool {
	if node == nil || v.info == nil || !v.packageMentionsHeapIntrinsicIdent() {
		return false
	}

	shadowed := false

	ast.Inspect(node, func(n ast.Node) bool {
		if shadowed {
			return false
		}

		ident, ok := n.(*ast.Ident)

		if !ok || ident.Name != heapIntrinsicIdent {
			return true
		}

		obj := v.info.Defs[ident]

		if obj == nil {
			return true
		}

		if _, isPackageName := obj.(*types.PkgName); isPackageName {
			return true
		}

		shadowed = true

		return false
	})

	return shadowed
}

// packageMentionsHeapIntrinsicIdent reports whether the package under conversion declares an object
// named `heap` ANYWHERE, at any scope. It is the cheap gate in front of declaresHeapIntrinsicIdent's
// per-declaration AST walk: without it every function in every package pays a second full traversal
// to learn what one pass over the package's own Defs map answers for all of them. Computed once and
// cached; the overwhelmingly common answer is false, and a false answer means no walk happens at all.
func (v *Visitor) packageMentionsHeapIntrinsicIdent() bool {
	if v.heapIdentInPackage == nil {
		mentions := false

		for ident, obj := range v.info.Defs {
			if ident == nil || ident.Name != heapIntrinsicIdent || obj == nil {
				continue
			}

			if _, isPackageName := obj.(*types.PkgName); isPackageName {
				continue
			}

			mentions = true

			break
		}

		v.heapIdentInPackage = &mentions
	}

	return *v.heapIdentInPackage
}

// packageDeclaresHeapIntrinsicIdent reports whether the package under conversion declares a package
// level FUNC named `heap`, which emits as a member of the `<pkg>_package` class and so wins
// simple-name lookup inside every method of that class — a package-wide qualification, not a
// per-function one.
//
// ⚠ FUNC specifically, and the narrowing is measured, not assumed. C#'s invocable-member rule
// (§12.8.4 — a simple name used as the target of an invocation ignores TYPE MEMBERS that are not
// invocable) means a package-level Go `type heap` or `var heap`, both of which emit as non-invocable
// members, are skipped in `heap(…)` and the `using static go.builtin` method group is found anyway.
// The first version of this check tested "any non-PkgName object" and was falsified by its own A/B:
// GlobalCapturedInClosure declares `type heap` at package level, and with the fix REVERTED it still
// compiled — so the broad form was only over-qualifying, changing a golden no defect required. A
// LOCAL or PARAMETER named `heap` is a different rule and does win (see declaresHeapIntrinsicIdent);
// the invocable-member filter applies to type members, not to the local declaration space.
//
// The func shape is therefore the one package-level case that can genuinely collide. Nothing in the
// corpus declares it — GOROOT's only `heap` declarations are internal/trace/batchcursor.go's
// parameters and runtime/time.go's locals, both function-scoped — so it is reasoned from the lookup
// rule rather than reproduced; the NEGATIVE side (a package-level type must NOT trigger) is what
// GlobalCapturedInClosure guards.
func (v *Visitor) packageDeclaresHeapIntrinsicIdent() bool {
	if v.pkg == nil || v.pkg.Scope() == nil {
		return false
	}

	_, isFunc := v.pkg.Scope().Lookup(heapIntrinsicIdent).(*types.Func)

	return isFunc
}
