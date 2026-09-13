// visitValueSpec.go - Gbtc
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
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
	"unicode/utf8"
)

func (v *Visitor) visitValueSpec(valueSpec *ast.ValueSpec, doc *ast.CommentGroup, tok token.Token) {
	v.outputBuilder.WriteString(v.newline)
	v.writeDoc(doc, valueSpec.End())

	if tok == token.VAR {
		// A PACKAGE-LEVEL var whose initializer contains a func LITERAL (`var Support =
		// sync.OnceValue(func() bool { … })` — internal/syscall/windows) needs the same
		// per-function variable analysis a declared function gets — shadow renames,
		// reassignment tracking, capture state — for the literal's body. Wrap the initializers
		// in a synthetic function so the analysis walks them; without it the body's locals
		// emitted against stale/empty analysis state (`nint n += i` — CS1002 cascade).
		if !v.inFunction && len(valueSpec.Values) > 0 {
			hasFuncLit := false

			for _, value := range valueSpec.Values {
				ast.Inspect(value, func(n ast.Node) bool {
					if _, ok := n.(*ast.FuncLit); ok {
						hasFuncLit = true
						return false
					}

					return !hasFuncLit
				})
			}

			if hasFuncLit {
				stmts := make([]ast.Stmt, 0, len(valueSpec.Values))

				for _, value := range valueSpec.Values {
					stmts = append(stmts, &ast.ExprStmt{X: value})
				}

				syntheticDecl := &ast.FuncDecl{
					Name: ast.NewIdent(""),
					Type: &ast.FuncType{Params: &ast.FieldList{}},
					Body: &ast.BlockStmt{List: stmts},
				}

				// performVariableAnalysis writes v.varNames, which visitFuncDecl allocates
				// per REAL function — a global func-literal var declared BEFORE any function
				// in the file hit a nil map (recovered panic that silently DROPPED the rest
				// of the file, csproj references included — CrossPkgUser's CheckFunc), and
				// one declared after inherited the PREVIOUS function's stale rename table.
				// Give the synthetic decl its own fresh table.
				v.varNames = make(map[*types.Var]string)

				v.performVariableAnalysis(syntheticDecl, types.NewSignatureType(nil, nil, nil, nil, nil, false))
				v.performUntypedConstAnalysis(syntheticDecl)
			}
		}

		// A package-level `var a, b = f()` — ONE call initializer whose result tuple Go
		// deconstructs across the names. C# field initializers cannot deconstruct, so the
		// per-name loop below assigned the WHOLE ValueTuple to the first field (CS0029 —
		// edwards25519's `var identity, _ = new(Point).SetBytes(…)`) and left the rest
		// uninitialized. Route through the ValueTuple component-read emission instead.
		// In-function specs keep the existing path (`:=` tuples are visitAssignStmt's).
		//
		// A two-value TYPE ASSERTION (`var y, ok = x.(T)`) deconstructs the identical way — Go's
		// comma-ok form is not exclusive to `:=`, and go/types marks the assertion's own type a
		// 2-tuple in EITHER declaration form (the isTuple check below already accounts for it
		// correctly). The CallExpr-only gate simply never considered the shape: runtime's
		// gc_test.go testAssertVar assigned the whole `(error?, bool)` tuple to a bare `error y`
		// with `ok` left `default!` (CS0029) — same defect this branch exists to prevent, one
		// expression kind short of catching it.
		if !v.inFunction && valueSpec.Type == nil && len(valueSpec.Names) > 1 && len(valueSpec.Values) == 1 {
			_, isCall := valueSpec.Values[0].(*ast.CallExpr)
			_, isTypeAssert := valueSpec.Values[0].(*ast.TypeAssertExpr)

			if isCall || isTypeAssert {
				if tuple, isTuple := v.info.TypeOf(valueSpec.Values[0]).(*types.Tuple); isTuple {
					v.visitPackageTupleVarSpec(valueSpec, tuple)
					return
				}
			}
		}

		// A FUNCTION-LOCAL `var name, offset, abs = t.locabs()` (a grouped var spec is not a
		// `:=`, so visitAssignStmt never sees it) deconstructs the same way — the per-name
		// loop below assigned the WHOLE tuple to the first name and silently DEFAULTED the
		// rest (CS0029; time appendFormat read a zero abs). Emit the C# tuple deconstruction
		// (`var (name, offset, abs) = t.locabs();`), matching the `:=` form. Gated to specs
		// with no heap-escaping name (an escaping name needs its `ref heap<T>` box decl —
		// none in the corpus takes this shape yet). Widened to a two-value TYPE ASSERTION for
		// the identical reason as the package-level branch above — see that comment.
		if v.inFunction && valueSpec.Type == nil && len(valueSpec.Names) > 1 && len(valueSpec.Values) == 1 {
			_, isCall := valueSpec.Values[0].(*ast.CallExpr)
			_, isTypeAssert := valueSpec.Values[0].(*ast.TypeAssertExpr)

			if isCall || isTypeAssert {
				if _, isTuple := v.info.TypeOf(valueSpec.Values[0]).(*types.Tuple); isTuple {
					// The gate is "does this name get a `Ꮡname` BOX", which is identHasHeapBox —
					// NOT the raw identEscapesHeap flag, which the escape analysis blanket-sets for
					// every INHERENTLY heap-allocated local (pointer/slice/map/chan/interface/func:
					// already a reference, so no box is emitted unless its address is really taken).
					// Reading the raw flag rejected every tuple whose results include an interface or
					// a func — `var ctx, cancel = context.WithCancel(ctx)` (net sendfile_test) — and
					// the per-name fallback then assigned the WHOLE ValueTuple to the first name with
					// the rest `default!` (CS0029), the very defect this branch exists to prevent.
					// It is the trap paramAddressTakenNeedsBox documents: a verdict the box gate then
					// refuses leaves identEscapesHeap set with no box behind it.
					allPlain := true

					for _, ident := range valueSpec.Names {
						if obj := v.info.Defs[ident]; obj != nil && v.identHasHeapBox(obj, obj.Type()) {
							allPlain = false
							break
						}
					}

					if allPlain {
						names := make([]string, len(valueSpec.Names))

						for i, ident := range valueSpec.Names {
							names[i] = getSanitizedIdentifier(v.getIdentName(ident))
						}

						v.outputBuilder.WriteString(v.newline)
						v.writeOutput("var (%s) = %s;", strings.Join(names, ", "), v.convExpr(valueSpec.Values[0], nil))
						return
					}
				}
			}
		}

		for i, ident := range valueSpec.Names {
			var isAnyType bool
			var isInterfaceType bool
			var ifaceDeclType types.Type
			// The declared type when it is the EMPTY interface — the slot convInterfaceDeclValue
			// needs to apply the pointer boundary treatment (typedNilInterfaceBoxing.go); the
			// non-empty case is carried by ifaceDeclType and routes through the adapter instead.
			var emptyIfaceDeclType types.Type

			// Check if this is an interface type being assigned a value
			if len(valueSpec.Values) > i {
				// Get the type - either from explicit type or from value's type
				var declType types.Type

				if valueSpec.Type != nil {
					declType = v.info.TypeOf(valueSpec.Type)
				} else {
					declType = v.info.TypeOf(ident)
				}

				if declType != nil {
					// Check if it's an interface type
					if isInterface, isEmpty := isInterface(declType); isInterface {
						isInterfaceType = true

						if isEmpty {
							isAnyType = true
							emptyIfaceDeclType = declType
						} else {
							// Get the concrete type from the RHS
							rhsType := v.info.TypeOf(valueSpec.Values[i])

							// Record the implementation; the render sites below route the RHS
							// through the interface conversion (pointer-adapter wrapping).
							if rhsType != nil {
								v.convertToInterfaceType(declType, rhsType, "")
								ifaceDeclType = declType
							}
						}
					}
				}
			}

			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			// NOTE — a package-level VAR's Δ-rename is NOT registered here, and a block that tried
			// could never fire: performGlobalVariableAnalysis has already renamed the declarator
			// (globalIdentNames), so `goIDName` above is the POST-rename `ΔLock`, against which
			// `nameCollisions` answers false while `csIDName` comes out correct anyway (the
			// sanitizers are idempotent). That is the split that made the emission right and the
			// cross-variant record empty. The registration lives at the rename site instead —
			// variableAnalysisOperations.go's performGlobalVariableAnalysis — so the decision and
			// the record are one statement pair. The CONST arm below keeps its own registration:
			// performGlobalVariableAnalysis skips non-`*types.Var` objects, so a const reaches this
			// visitor with its raw Go name and this is its only rename site.

			if csIDName == "_" {
				if v.inFunction {
					csIDName = v.getTempVarName("_")
				} else {
					csIDName = getGlobalTempVarName("_") + CapturedVarMarker
				}
			}

			// A func literal inside a PACKAGE-LEVEL initializer converts its body with
			// inFunction = true but has no enclosing function DECLARATION, so a type it lifts has
			// no name seed of its own. Record the declaration being initialized as that seed —
			// `var readers = []struct{…}{{…, func(s string) io.Reader { return struct{ io.Reader
			// }{…} }}}` (fmt's scan_test.go) lifts to `readers_type`, alongside the outer anonymous
			// struct's `readersᴛ1`. Package-level value specs do not nest, and the in-function arms
			// never read this, so a plain assignment (no save/restore) is sufficient.
			if !v.inFunction {
				v.packageInitLiftName = csIDName
			}

			context := DefaultBasicLitContext()
			// An EMPTY-interface declared type boxes a string LITERAL through @string
			// (`var v any = "x"` → `(@string)"x"u8`), the same rendering the assignment form takes —
			// a bare C# string boxes System.String where Go boxes `string`, and the cast is what
			// lets the u8 span reach the object slot at all. A NON-EMPTY interface target keeps
			// today's bare form (its value needs an adapter, not an @string).
			context.u8StringOK = !isInterfaceType || isAnyType
			context.castToGoString = isAnyType
			context.spanTargetUnsupported = isInterfaceType

			if len(valueSpec.Values) <= i {
				def := v.info.Defs[ident]

				if def != nil {
					// A package var carrying a two-arg //go:linkname PULL (`//go:linkname overflowError
					// runtime.overflowError`, math/bits) is emitted as a forwarding PROPERTY to the remote
					// symbol — Go's linkname aliases their storage, so reads and writes both hit the remote —
					// not the null field this bodyless var would otherwise become. The remote is public via
					// its definition-side one-arg handle (packageVarAccess), and its package is queued for a
					// project reference; the fully-qualified `go.<pkg>_package.<remote>` resolves inside
					// `namespace go;` without a using. See linknameOperations.
					// An ADDRESS-TAKEN pull keeps its heap-box form below: a forwarding property has no
					// address, so `&zeroVal[0]` (reflect, pulling runtime.zeroVal) would reference a
					// nonexistent `ᏑzeroVal` box (CS0103). Such a pull falls through to the addressed-
					// global emission — the pre-feature behavior, which compiles.
					if ref, pkgPath, ok := varLinknamePull(goIDName, v.options, doc, valueSpec.Doc); ok && !v.inFunction && !v.isAddressedGlobal(ident) {
						if i > 0 {
							v.outputBuilder.WriteString(v.newline)
						}

						v.importQueue.Add(pkgPath)
						csTypeName := convertToCSTypeName(v.getAliasQualifiedTypeName(def.Type(), false))
						v.writeOutput("%s static %s %s { get => %s; set => %s = value; }", getAccess(goIDName), csTypeName, csIDName, ref, ref)
						continue
					}

					// The INVERTED alias (linknameVarAliasTargets): this var is the side that FORWARDS
					// and another package holds the storage. Same emitted shape as the pull arm above and
					// for the same reason — Go's alias makes the two declarations one variable, so reads
					// and writes must both reach the one field — but the directive that pairs them lives
					// in the OTHER package and is invisible here, so the registry supplies it. The
					// storage member is public via packageVarAccess's mirror arm.
					//
					// The pull arm's address-taken guard is INHERITED rather than reasoned about again: a
					// forwarding property has no address, so an addressed global would emit `Ꮡ<name>`
					// against a box this arm never declares (CS0103) — reflect's pull of runtime.zeroVal
					// is the recorded case of that shape. Such a var falls through to the addressed-global
					// field emission below, which compiles and is the pre-feature behavior; it also
					// severs the alias, so if a future row ever lands on an addressed var the storage must
					// move to golib (S3) rather than be forwarded from here.
					if ref, ok := v.varLinknameAliasForward(goIDName); ok && !v.inFunction && !v.isAddressedGlobal(ident) {
						if i > 0 {
							v.outputBuilder.WriteString(v.newline)
						}

						access := v.testDeclaredValueAccess(packageVarAccess(goIDName, v.getIdentType(ident)), ident.Pos(), v.getIdentType(ident))
						csTypeName := convertToCSTypeName(v.getAliasQualifiedTypeName(def.Type(), false))
						v.writeOutput("%s static %s %s { get => %s; set => %s = value; }", access, csTypeName, csIDName, ref, ref)
						continue
					}

					if i > 0 {
						v.outputBuilder.WriteString(v.newline)
					}

					// Check if value spec type is a struct or a pointer to a struct
					valueSpecType := valueSpec.Type

					if subStructType, exprType := v.extractStructType(valueSpecType); subStructType != nil && !v.liftedTypeExists(subStructType) {
						v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					// Check if value spec type is an interface or a pointer to an interface
					if subInterfaceType, exprType := v.extractInterfaceType(valueSpecType); subInterfaceType != nil && !v.liftedTypeExists(subInterfaceType) {
						v.visitInterfaceType(subInterfaceType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					goTypeName := v.getAliasQualifiedTypeName(def.Type(), false)
					csTypeName := convertToCSTypeName(goTypeName)

					// An ANONYMOUS func-typed var — or a methodless NAMED func type, which the
					// converter collapses to (and renders everywhere as) its base delegate
					// (methodlessNamedFuncSignature, its own declaration skipped) — renders through
					// the signature-aware path (Func<…>/Action<…> via iifeDelegateType). The raw
					// getAliasQualifiedTypeName text mangles under convertToCSTypeName: an anonymous
					// `func(string, string) ([]byte, error)` collapses to `(<>byte, error)` (time
					// zoneinfo_read's loadTzinfoFromTzdata, CS1003), and a methodless named func type
					// whose signature carries a slash-bearing cross-package element — go/parser's
					// `var f parseSpecFunction` (`func(*go/ast.CommentGroup, go/token.Token, int)
					// ast.Spec`) — mangles those elements to the nonexistent `go.go.ast.CommentGroup`/
					// `go.go.token.Token` (CS0234), while its matching parameter and assigned-lambda
					// sites render structurally through getCSharpTypeName. getCSharpTypeName routes both forms
					// through iifeDelegateType, whose aliasedElementTypeName keeps each element's
					// `pkg.Type` alias. This precedence matches getCSharpTypeName's own (a func render wins
					// over the foreign-alias route below, whose alias would point at the SKIPPED
					// methodless-func declaration). A NAMED func type WITH methods keeps its delegate name.
					_, isSig := types.Unalias(def.Type()).(*types.Signature)
					_, isMethodlessFunc := methodlessNamedFuncSignature(def.Type())

					if isSig || isMethodlessFunc {
						csTypeName = v.getCSharpTypeName(def.Type())
					} else if aliased, ok := v.foreignAliasedTypeName(def.Type()); ok {
						// A local declared as a foreign RENAMED type routes through the recorded
						// alias (`syscallꓸSockaddr sa`, not the nonexistent `Δsyscall.Sockaddr` —
						// CS0426, internal/poll sockaddrToRaw).
						csTypeName = aliased
					}

					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(goIDName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutput("%s", heapTypeDecl)
						} else {
							if arrayType, ok := valueSpecType.(*ast.ArrayType); ok && arrayType.Len != nil {
								// Handle array type
								var arrayLenValue string
								arrayLenExpr := v.convExpr(arrayType.Len, nil)

								// Check if length expression is in type information
								if tv, ok := v.info.Types[arrayType.Len]; ok {
									// Check if it's a constant
									if tv.Value != nil {
										// constArrayLength, not a bare constant.Int64Val: a legal
										// float- or complex-kind length constant (`const S = 1e6`)
										// panics that call — see visitArrayType.go.
										if length, ok := constArrayLength(tv.Value); ok {
											arrayLenValue = length
										}
									}
								}

								if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
									v.writeOutput("%s %s = new(%s); /* %s */", csTypeName, csIDName, v.arrayZeroValueArgs(arrayLenValue, def.Type()), arrayLenExpr)
								} else {
									v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, v.arrayZeroValueArgs(arrayLenExpr, def.Type()))
								}
							} else if arrayType, ok := types.Unalias(def.Type()).(*types.Array); ok {
								// A type ALIAS to a fixed-size array (`type words = [4]uint64`,
								// *types.Alias in Go 1.22+): the spec's type syntax is an Ident, so
								// the ast.ArrayType check above misses — resolve the length through
								// types.Unalias or the local gets `default!` with a null backing
								// array (NRE on first element write).
								v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, v.arrayZeroValueArgs(strconv.FormatInt(arrayType.Len(), 10), arrayType))
							} else if v.structHasPromotedEmbeds(def.Type()) {
								// A struct with a promoted embed stores it in a readonly `ж<T>`
								// box only the constructors initialize — `default!` leaves the
								// box null and the first promoted-member access NREs, so the
								// zero value must construct through the NilType ctor.
								v.writeOutput("%s %s = new(nil);", csTypeName, csIDName)
							} else if v.structZeroValueNeedsConstruction(def.Type()) {
								// A struct whose default(T) is broken by a fixed-size array
								// field (its `= new(N)` backing, at any nesting depth) must run
								// the generated parameterless constructor so those field
								// initializers (and AppendZeroValueInitializers) execute —
								// `default!` skips them, leaving a null backing (len 0 / NRE on
								// index). Mirrors go2cs-gen's NeedsConstruction.
								v.writeOutput("%s %s = new();", csTypeName, csIDName)
							} else if nilChan := v.chanDirNilValue(def.Type()); nilChan != "" {
								// A DIRECTIONAL channel local (`var left chan<- chan T`): the
								// direction is part of the Go type and `default!` erases it, so
								// the zero value is the directional nil factory — the same arm
								// zeroValueInitializer carries for the named-result prologues.
								// This inline ladder is a DELIBERATE copy of that helper (it
								// resolves array lengths from the AST for the symbolic-length
								// comment), and the chan rung had never joined the copy:
								// reflect's TestChanOf read TypeOf(left) as bidirectional off
								// exactly this gap.
								v.writeOutput("%s %s = %s;", csTypeName, csIDName, nilChan)
							} else {
								v.writeOutput("%s %s = default!;", csTypeName, csIDName)
							}
						}
					} else {
						access := v.testDeclaredValueAccess(packageVarAccess(goIDName, v.getIdentType(ident)), ident.Pos(), v.getIdentType(ident))
						typeLenDeviation += token.Pos(len(access) + 6)

						// A fixed-size array global must be allocated (`new(N)`); the default
						// `array<T>` value is empty, so indexing it NREs (e.g. runtime's stackpool).
						// Locals already get this in the in-function branch above.
						var arrayLenValue string

						if arrayType, ok := valueSpecType.(*ast.ArrayType); ok && arrayType.Len != nil {
							arrayLenValue = v.convExpr(arrayType.Len, nil)

							if tv, ok := v.info.Types[arrayType.Len]; ok && tv.Value != nil {
								// constArrayLength, not a bare constant.Int64Val: see the local
								// branch above and visitArrayType.go.
								if length, ok := constArrayLength(tv.Value); ok {
									arrayLenValue = length
								}
							}
						} else if arrayType, ok := types.Unalias(def.Type()).(*types.Array); ok {
							// Alias-typed global (`var gw words` where `type words = [4]uint64`):
							// same types.Unalias resolution as the local path above.
							arrayLenValue = strconv.FormatInt(arrayType.Len(), 10)
						}

						// A nested/needy element type must be constructed per element — the bare
						// length would leave every element `default(T)` (see arrayZeroValueArgs).
						if len(arrayLenValue) > 0 {
							arrayLenValue = v.arrayZeroValueArgs(arrayLenValue, def.Type())
						}

						// A promoted-embed struct global has the same null-box hazard as the
						// local branch above: the readonly `ж<T>` embed boxes only exist when
						// a constructor runs, so the zero value must be `new(nil)`, not the
						// field's implicit default.
						hasPromotedEmbeds := v.structHasPromotedEmbeds(def.Type())

						// A struct global whose default(T) is broken by a fixed-size array field
						// (at any nesting depth) must likewise construct — `new()` runs the
						// generated parameterless constructor's field initializers. Mirrors the
						// local branch above (structZeroValueNeedsConstruction / go2cs-gen); the
						// promoted-embed case above already covers its own superset, so exclude it.
						needsConstruction := !hasPromotedEmbeds && v.structZeroValueNeedsConstruction(def.Type())

						if v.isAddressedGlobal(ident) {
							// Box an N-sized array, not the empty default, so writes through the
							// pointer (and indexing) hit real storage.
							initExpr := ""

							if len(arrayLenValue) > 0 {
								initExpr = fmt.Sprintf("new %s(%s)", csTypeName, arrayLenValue)
							} else if hasPromotedEmbeds {
								initExpr = fmt.Sprintf("new %s(nil)", csTypeName)
							} else if needsConstruction {
								initExpr = fmt.Sprintf("new %s()", csTypeName)
							}

							v.writeAddressedGlobalDecl(access, csTypeName, csIDName, initExpr, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
						} else if len(arrayLenValue) > 0 {
							v.writeOutput("%s static %s %s = new(%s);", access, csTypeName, csIDName, arrayLenValue)
						} else if hasPromotedEmbeds {
							v.writeOutput("%s static %s %s = new(nil);", access, csTypeName, csIDName)
						} else if needsConstruction {
							v.writeOutput("%s static %s %s = new();", access, csTypeName, csIDName)
						} else if nilChan := v.chanDirNilValue(def.Type()); nilChan != "" {
							// A directional-channel GLOBAL: same rung as the local branch above,
							// for the same reason — the implicit field default erases the
							// direction the type carries.
							v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, nilChan)
						} else {
							v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
						}
					}

					v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation-token.Pos(len(csTypeName)))
				}
				continue
			}

			tv := v.info.Types[valueSpec.Values[i]]

			if tv.Value == nil {
				def := v.info.Defs[ident]

				if def != nil {
					if i > 0 {
						v.outputBuilder.WriteString(v.newline)
					}

					// A package-global var whose type is inferred from an anonymous-struct
					// composite literal: lift the struct with the var name up front so the
					// declaration type resolves to that lifted name (and the composite
					// literal reuses it) instead of emitting raw `struct{…}` text. Mirrors
					// the explicit-type path used for uninitialized vars.
					if !v.inFunction && valueSpec.Type == nil {
						if compositeLit, ok := valueSpec.Values[i].(*ast.CompositeLit); ok {
							// The literal's type is composed, so the struct can sit at any depth inside it: a
							// slice/array ELEMENT, a map VALUE (crypto/internal/hpke's `var SupportedKEMs =
							// map[uint16]struct{…}{…}`), or either of those through a pointer (net's `var
							// ipStringTests = []*struct{…}{…}`). Lifting it up front under the var's name is
							// what lets both the declaration type (getCSharpTypeName → getAliasQualifiedTypeName) and the literal
							// type (convMapType/getExpressionTypeName) resolve through liftedTypeMap to the lifted
							// name; without it the raw Go `struct{…}` syntax lands in the C# type and does not
							// parse. Keyed element literals stay the target-typed `new(…)` ctor form.
							if subStructType, exprType := v.extractStructType(compositeLit.Type); subStructType != nil && !v.liftedTypeExists(subStructType) {
								v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
							}
						} else if callExpr, ok := valueSpec.Values[i].(*ast.CallExpr); ok && len(callExpr.Args) == 1 {
							// A builtin `new` over an anonymous struct — `var reserved =
							// new(struct{ types.Type })` (go/internal/gccgoimporter): lift the struct
							// with the var name up front so the declaration type (`ж<reservedᴛ1>`) and
							// the `@new<…>()` type argument both resolve through liftedTypeMap, instead
							// of the declaration falling to the raw `struct{…}` t.String() mangle and
							// the lift arriving from the call-argument path under builtin new's UNNAMED
							// parameter (an empty lift name — a whole-package syntax cascade). Mirrors
							// the composite-literal lift above.
							if funIdent, isIdent := callExpr.Fun.(*ast.Ident); isIdent && funIdent.Name == "new" {
								if _, isBuiltin := v.info.ObjectOf(funIdent).(*types.Builtin); isBuiltin {
									if subStructType, exprType := v.extractStructType(callExpr.Args[0]); subStructType != nil && !v.liftedTypeExists(subStructType) {
										v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
									}
								}
							}
						}
					}

					// An EXPLICITLY typed spec whose declared type is (or reaches) an anonymous
					// struct/interface literal AND which carries an initializer — crypto/ecdh's
					// documented-interface witnesses, `var _ interface{ Equal(x crypto.PublicKey)
					// bool } = &ecdh.PublicKey{}`. Only the BODYLESS arm above lifted an explicit
					// anonymous type, so the initialized form had nothing to resolve to and the raw
					// Go `interface{…}` text landed in BOTH the declaration type and the value
					// adapter's class name (whose `{`/`}` then break the member declaration —
					// CS1519/CS1002/CS1513, and every following member reads as a namespace-level
					// declaration: CS0106 on each one, a whole-file cascade). Lift under the var
					// name so both sites resolve to the lifted C# type. The adapter name is minted
					// earlier in this iteration (convertToInterfaceType, above) as a DEFERRED
					// marker, so registering the lift here still resolves it at the file-visit
					// barrier. Mirrors the bodyless arm's lift exactly.
					// The lift is named from the GO identifier, not from csIDName. For every
					// ordinary name the two agree (csIDName is that name sanitized, and
					// getUniqueLiftedTypeName re-sanitizes its argument), but a BLANK `_` var's
					// csIDName is a synthesized temp (`_ᴛ1ʗ`) that exists in no Go scope — so
					// getUniqueLiftedTypeName's typeExists check cannot see it and hands the type
					// the field's own name back, giving one class a type and a field both called
					// `_ᴛ1ʗ` (CS0102). Passing `_` finds the blank var among the package's defs and
					// bumps the type to `_ᴛ1`, distinct from the field by construction.
					if valueSpec.Type != nil {
						if subStructType, exprType := v.extractStructType(valueSpec.Type); subStructType != nil && !v.liftedTypeExists(subStructType) {
							v.visitStructType(subStructType, exprType, goIDName, valueSpec.Comment, true, nil)
						}

						if subInterfaceType, exprType := v.extractInterfaceType(valueSpec.Type); subInterfaceType != nil && !v.liftedTypeExists(subInterfaceType) {
							v.visitInterfaceType(subInterfaceType, exprType, goIDName, valueSpec.Comment, true, nil)
						}
					}

					var csTypeName string
					var typeLenDeviation token.Pos

					if v.inFunction {
						// A func-literal initializer (`var f T = func(){ …capture… }`) emits its
						// captured-variable snapshot decls inline; collect them in the hoist buffer
						// and write them on their own line(s) before this declaration.
						hoistBuf := &strings.Builder{}
						savedHoist := v.hoistedDecls
						v.hoistedDecls = hoistBuf
						valExpr := v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, emptyIfaceDeclType, context)
						v.hoistedDecls = savedHoist

						// Render the declared type only AFTER converting the initializer: a
						// composite literal over an anonymous struct (`var sects = []struct{…}{…}`)
						// lifts the struct type during value conversion, so the declaration
						// resolves to the lifted name instead of raw `struct{…}` Go syntax.
						csTypeName = v.getCSharpTypeName(def.Type())
						typeLenDeviation = token.Pos(len(csTypeName) + (len(csIDName) - len(goIDName)))

						// A narrow-integer arithmetic initializer (`var x uint8 = a + b`) needs the
						// same cast back to the declared type as the assignment forms — Go wraps the
						// arithmetic at the operand width, C# promotes it to int (CS0266 / lost wrap).
						if narrowCast := v.narrowArithmeticCastTypeFor(def.Type(), valueSpec.Values[i], valExpr); len(narrowCast) > 0 {
							valExpr = fmt.Sprintf("(%s)(%s)", narrowCast, valExpr)
						}

						// A directional channel var taking a BIDIRECTIONAL value narrows the Go
						// type without constructing anything (`var s <-chan T = ch`), so the
						// direction has to be re-stamped onto the value here — the live-copy
						// position chanDirectionCargo.go's exclusion deferred until reflect
						// measured a consumer. Same shape as the narrow-arithmetic wrap above:
						// a post-conversion wrap keyed on the DECLARED type versus the value's.
						if narrowedChan := v.chanDirNarrowedValue(def.Type(), v.info.TypeOf(valueSpec.Values[i]), valExpr); narrowedChan != "" {
							valExpr = narrowedChan
						}

						if hoistBuf.Len() > 0 {
							// The decls carry their own leading newline + per-line indentation;
							// writeOutput below re-indents the declaration line that follows.
							v.outputBuilder.WriteString(strings.TrimRight(hoistBuf.String(), " \t"))
						}

						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutputLn("%s", heapTypeDecl)
							v.outputBuilder.WriteString(v.newline)
							v.writeOutput("%s = %s;", csIDName, valExpr)
						} else {
							// Following declarations must use explicit type, do not use `v.options.preferVarDecl` for these:
							v.writeOutput("%s %s = %s;", csTypeName, csIDName, valExpr)
						}
					} else {
						csTypeName = v.getCSharpTypeName(def.Type())

						access := v.testDeclaredValueAccess(packageVarAccess(goIDName, v.getIdentType(ident)), ident.Pos(), v.getIdentType(ident))
						typeLenDeviation = token.Pos(len(csTypeName)+(len(csIDName)-len(goIDName))) - token.Pos(len(access)+9)

						// A multi-value inner call spread in the initializer (`var debug =
						// template.Must(template.New(…).Parse(…))`, net/rpc debug.go) spills
						// into a hidden static tuple field via v.globalDeclHoist; flush it
						// BEFORE this var's field so the once-evaluated holder precedes its
						// readers (C# static field initializers run in textual order).
						globalHoist := &strings.Builder{}
						savedGlobalHoist := v.globalDeclHoist
						v.globalDeclHoist = globalHoist
						valExpr := v.convInterfaceDeclValue(valueSpec.Values[i], ifaceDeclType, emptyIfaceDeclType, context)
						v.globalDeclHoist = savedGlobalHoist

						// A package var whose initializer's Go init-order dependencies C#'s
						// static-field-initializer order cannot reproduce (cross-file / same-file
						// forward reference / dependency on another relocated var — see
						// collectMovedInitVars) is emitted BARE here, with the initializer relocated
						// into an adjacent per-file init method that the ordered static ctor
						// (package_init.cs) calls in InitOrder. The method lives in this file so the
						// rendered expression keeps the file's own using aliases. An addressed global
						// relocates too (its box is declared with the default value; the ctor
						// assignment writes through the ref property into the same box), else a moved
						// dependency of an addressed global would still read zero. The multi-value
						// hoist form falls back inline with a warning (no stdlib occurrence).
						ordinal, moved := v.movedInitOrdinal(def)

						if moved && globalHoist.Len() > 0 {
							v.showWarning("package var '%s' needs init-order relocation but has a multi-value hoisted initializer - left inline (init order NOT guaranteed)", goIDName)
							moved = false
						}

						if moved {
							if v.isAddressedGlobal(ident) {
								v.writeAddressedGlobalDecl(access, csTypeName, csIDName, "", isInherentlyHeapAllocatedType(v.getIdentType(ident)))
							} else {
								v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
							}

							methodName := packageInitMethodName(csIDName)
							v.outputBuilder.WriteString(v.newline)
							v.writeOutput("internal static void %s() { %s = %s; }", methodName, csIDName, valExpr)
							recordMovedInitMethod(ordinal, methodName)
						} else {
							if globalHoist.Len() > 0 {
								v.outputBuilder.WriteString(globalHoist.String())
							}

							if v.isAddressedGlobal(ident) {
								v.writeAddressedGlobalDecl(access, csTypeName, csIDName, valExpr, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
							} else {
								v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, valExpr)
							}
						}
					}

					v.writeComment(valueSpec.Comment, valueSpec.Values[i].End()-typeLenDeviation)
				}
				continue
			}

			if i > 0 {
				v.outputBuilder.WriteString(v.newline)
			}

			var csTypeName string

			if isAnyType {
				csTypeName = "any"
			} else if valueSpec.Type != nil {
				// An EXPLICITLY typed spec keeps its DECLARED type — this constant-initializer
				// arm otherwise retypes the var from the VALUE (os's `var Kill Signal =
				// syscall.SIGKILL` emitted syscall's ΔSignal where the os.Signal INTERFACE was
				// declared — CS1503 at every Signal-typed use).
				csTypeName = convertToCSTypeName(v.getAliasQualifiedTypeName(v.info.TypeOf(valueSpec.Type), false))
			} else {
				csTypeName = convertToCSTypeName(v.getAliasQualifiedTypeName(tv.Type, false))
			}

			goValue := tv.Value.ExactString()
			csValue := v.convExpr(valueSpec.Values[i], []ExprContext{context})

			// A declared INTERFACE type over a constant initializer wraps the value in the
			// interface conversion (the constant's named type implements the interface —
			// SIGKILL's syscall.Signal implementing os.Signal).
			if valueSpec.Type != nil {
				if declType := v.info.TypeOf(valueSpec.Type); declType != nil {
					if needsCast, isEmpty := isInterface(declType); needsCast && !isEmpty {
						// A constant initializer folds its own named conversion away
						// (`Errno(errnoERROR_IO_PENDING)` renders as the bare reference), so
						// the value loses the type that implements the interface - re-impose
						// it before the interface conversion (syscall zsyscall_windows,
						// UntypedInt -> error CS0029).
						if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
							if _, isBasic := named.Underlying().(*types.Basic); isBasic {
								namedCS := v.getCSharpTypeName(named)

								// Skip when the render already leads with its own cast
								// (`((errorString)(@string)"..."u8)` needs no second wrap).
								if !strings.HasPrefix(csValue, "(("+namedCS+")") {
									csValue = fmt.Sprintf("((%s)%s)", namedCS, csValue)
								}
							}
						}

						csValue = v.convertToInterfaceType(declType, tv.Type, csValue)
					}

					// An EMPTY-interface declared type (`var x any = 1`) boxes an untyped CONSTANT at
					// Go's default type for its kind — the numeric twin of the non-empty wrap above
					// and the @string boxing family — so a later `x.(int)` matches Go's boxed `int`.
					// A no-op for a non-empty/non-interface declared type and a non-constant value.
					csValue = v.boxUntypedConstAsDefaultType(declType, valueSpec.Values[i], csValue)
				}
			}
			typeLenDeviation := token.Pos(len(csTypeName) + len(csValue) + (len(csIDName) - len(goIDName)) + (len(csValue) - len(goValue)))

			if v.inFunction {
				headTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(headTypeDecl) > 0 {
					v.writeOutput("%s", headTypeDecl)

					if len(csValue) > 0 {
						v.outputBuilder.WriteString(v.newline)
						v.writeOutput("%s = %s;", csIDName, csValue)
					}
				} else {
					if len(csValue) > 0 {
						v.writeOutput("%s %s = %s;", csTypeName, csIDName, csValue)
					} else {
						v.writeOutput("%s %s;", csTypeName, csIDName)
					}
				}
			} else {
				access := v.testDeclaredValueAccess(packageVarAccess(goIDName, v.getIdentType(ident)), ident.Pos(), v.getIdentType(ident))
				typeLenDeviation += token.Pos(len(access) + 4)

				// A Go-CONSTANT-valued initializer is still order-sensitive in C#. Go folds the
				// value at compile time, but the emission KEEPS the source expression for
				// readability — and that expression can reference a named/string/untyped const
				// emitted as a `static readonly` FIELD (`var pipeLabel = string(labelPipe) + "!"`),
				// whose own field initializer C# may run later. So this arm needs the same
				// relocation the non-constant arm above performs; collectMovedInitVars already
				// flags it.
				ordinal, moved := v.movedInitOrdinal(v.info.Defs[ident])

				if moved {
					if v.isAddressedGlobal(ident) {
						v.writeAddressedGlobalDecl(access, csTypeName, csIDName, "", isInherentlyHeapAllocatedType(v.getIdentType(ident)))
					} else {
						v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
					}

					methodName := packageInitMethodName(csIDName)
					v.outputBuilder.WriteString(v.newline)
					v.writeOutput("internal static void %s() { %s = %s; }", methodName, csIDName, csValue)
					recordMovedInitMethod(ordinal, methodName)
				} else if v.isAddressedGlobal(ident) {
					// An addressed package var must be heap-boxed even when its initializer folds to
					// a constant (runtime's `var uint16Eface any = uint16InterfacePtr(0)`, addressed
					// via `efaceOf(&uint16Eface)`): convUnaryExpr emits the box form `Ꮡuint16Eface`,
					// so without boxing here that identifier would not exist (CS0103).
					v.writeAddressedGlobalDecl(access, csTypeName, csIDName, csValue, isInherentlyHeapAllocatedType(v.getIdentType(ident)))
				} else {
					v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, csValue)
				}
			}

			v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation)
		}
	} else if tok == token.CONST {
		for i, ident := range valueSpec.Names {
			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			// The const twin of performGlobalVariableAnalysis's var registration — see the NOTE in
			// the VAR arm above and testTypeRenames's doc comment. A const is not a `*types.Var`,
			// so the global analysis pass never renames it and `goIDName` here is still the raw Go
			// name: this IS the rename site for a const, and the record belongs with it.
			if nameCollisions[goIDName] && testTypeRenames != nil {
				if obj := v.info.ObjectOf(ident); obj != nil {
					testTypeRenames[obj] = true
				}
			}

			if csIDName == "_" {
				if v.inFunction {
					csIDName = v.getTempVarName("_")
				} else {
					csIDName = getGlobalTempVarName("_") + CapturedVarMarker
				}
			}

			c := v.info.ObjectOf(ident).(*types.Const)

			// A function-local untyped const whose every use resolves to ONE concrete basic
			// type is DECLARED at that type (see performUntypedConstAnalysis): the wrapper
			// indirection and the per-use casts disappear, C#'s `const` keyword applies where
			// legal for the type, and the existing typed-const machinery below (native-int
			// demotion, uintptr `static readonly`, the float32 `f` suffix) applies unchanged.
			declType := types.Type(c.Type())

			tightenedType, isTightened := v.tightenedConsts[c]

			if isTightened {
				declType = tightenedType
			}

			goTypeName := v.getAliasQualifiedTypeName(declType, false)
			csTypeName := convertToCSTypeName(goTypeName)
			access := v.testDeclaredValueAccess(getAccess(goIDName), ident.Pos(), declType)
			typeLenDeviation := token.Pos(len(csTypeName) + len(access) + (len(csIDName) - len(goIDName)))

			// Check if the type is a named type (user-defined), not a basic type. Unalias first: a
			// const typed through a type ALIAS to a named type (Go 1.23 renders `type Errno =
			// syscall.Errno` as *types.Alias, not *types.Named) still needs `static readonly` — the
			// aliased type is a [GoType] struct C# cannot declare `const` (golang.org/x/sys/windows's
			// `ERROR_… Errno = …`, CS0283/CS0133). Unalias to a *types.Basic (an alias to a primitive)
			// stays non-named, so those remain plain `const`.
			isNamedType := false

			if _, ok := types.Unalias(c.Type()).(*types.Named); ok {
				isNamedType = true
			} else if csTypeName == "UntypedInt" || csTypeName == "UntypedFloat" || csTypeName == "UntypedComplex" {
				isNamedType = true
			}

			var tokEnd token.Pos
			var srcVal string
			var constVal string

			// Whether a COMPLEX-kind constant's value fits the declaration's complex width (and so
			// rendered as a real complex value rather than falling to the GoBigConst arm below).
			complexRepresentable := false

			if c.Val().Kind() == constant.String && len(valueSpec.Values) >= i+1 {
				if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok && lit.Kind == token.STRING {
					constVal = v.convBasicLit(lit, DefaultBasicLitContext())
				} else if s := constant.StringVal(c.Val()); !utf8.ValidString(s) || stringLiteralNeedsByteArray(c.Val().ExactString()) {
					// A CONCATENATED string const folds to one value here; unlike a single *ast.BasicLit
					// (handled by convBasicLit's byte-array machinery above) it bypassed that path, so a
					// raw-byte table like math/bits' `rev8tab` ("\x00\x80…", built by "" + … concatenation)
					// rendered a UTF-16 string literal whose @string byte view UTF-8-re-encodes each >=0x80
					// byte (`rev8tab[1]` == 0xC2, not 0x80 → Reverse8 wrong). A value that is not valid
					// UTF-8 cannot round-trip through a C# string/u8 literal, so emit its exact bytes; a
					// valid-UTF-8 value keeps the readable getStringLiteral form.
					//
					// Valid UTF-8 is NOT the only way that round-trip fails, which is why convBasicLit's
					// own predicate gates this arm too. C#'s `\x` escape is GREEDY — one to FOUR hex
					// digits — where Go's is exactly two, so a `\xHH` followed by a hex-digit CHARACTER
					// re-parses as a different, longer escape. net/http/fcgi's
					// `"\x0f\x01" + "FCGI_MPXS_CONNS1"` folds to this arm and emitted `\x01F`: U+001F,
					// with the 'F' eaten. Every byte in it is ASCII, so the UTF-8 test alone reported
					// that the value round-trips, and TestGetValues then compared the response against
					// its own silently-wrong constant.
					constVal = byteArrayStringLiteral(s)
				} else {
					var isRawStr bool
					constVal, isRawStr = v.getStringLiteral(c.Val().ExactString())

					// A const of a NAMED string type whose value expression is NOT a bare literal —
					// a CONVERSION (`const opLoad = mapOp("Load")`, sync map_test) or a folded
					// concatenation — never reached convBasicLit above, so it renders as a plain C#
					// string literal. That does not reach the [GoType("@string")] wrapper: `string`
					// → wrapper would need string→@string→wrapper, two user-defined conversions,
					// which C# forbids (CS0029 ×9). The u8 form the BasicLit path emits binds the
					// wrapper's ReadOnlySpan<byte> operator in ONE conversion, so state it here too;
					// a plain @string const is unaffected (string→@string is already single-step)
					// and keeps its current rendering. A RAW (backtick) literal has no u8-suffixable
					// verbatim form, so it takes the explicit @string cast — also one conversion.
					if isNamedType {
						if isRawStr {
							constVal = fmt.Sprintf("((@string)%s)", constVal)
						} else {
							constVal += "u8"
						}
					}
				}
			} else if c.Val().Kind() == constant.Float {
				if basic, ok := declType.(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					// A float-KIND value under an INTEGER declared type — an integral untyped
					// float like `const infinity = 1e6` TIGHTENED to its int use type
					// (go/printer nodeSize) — must emit the integer form: C# has no implicit
					// double→int constant conversion (a `1e6` literal is CS0266 against nint),
					// and the tightening pass guaranteed integral representability.
					constVal = constant.ToInt(c.Val()).ExactString()
				} else {
					// The COMPILED value must be exact — Value.String() shortens to ~6 significant
					// digits, truncating the emitted literal (the exact value survived only in the
					// `/* … */` comment; math cbrt's C/D/E/F/G). Emit the source literal verbatim
					// when it is valid C#, else the shortest round-trip form (see exactFloatConstString).
					var srcExpr ast.Expr

					if len(valueSpec.Values) >= i+1 {
						srcExpr = valueSpec.Values[i]
					}

					constVal = exactFloatConstString(c.Val(), srcExpr, csTypeName == "float32")
				}
			} else if c.Val().Kind() == constant.Complex {
				// Rendered from the two EXACT halves; the whole-text ParseComplex representability
				// test it replaces could never succeed — see exactComplexConstString.
				constVal, complexRepresentable = exactComplexConstString(c.Val(), csTypeName == "complex64")

				if !complexRepresentable {
					constVal = c.Val().ExactString()
				}
			} else {
				constVal = c.Val().ExactString()
			}

			if valueSpec.Type == nil && len(valueSpec.Values) >= i+1 {
				tokEnd = valueSpec.Values[i].End()

				if ident := getIdentifier(valueSpec.Values[i]); ident != nil {
					srcVal = ident.Name
				} else if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok {
					srcVal = lit.Value
				}

				typeLenDeviation += token.Pos(len(constVal) - len(srcVal) - 4)
			} else {
				tokEnd = ident.End()
			}

			constHandled := false

			writeUntypedConst := func(hoistToStatic bool) {
				if i > 0 {
					v.outputBuilder.WriteString(v.newline)
				}

				// A Go constant has no runtime existence — its value lives in the instruction
				// stream — and GoBigConst is the one C# projection with a real per-evaluation cost:
				// BigInteger.Parse allocates its bits array on every run. A FUNCTION-LOCAL int-kind
				// big const therefore hoists the parse to one `static readonly` field above the
				// function (the hoisted-string-literal pattern) and the local copies it — a
				// BigInteger struct copy, which allocates nothing. net/textproto's
				// validHeaderFieldByte paid the parse 14 times per canonicalMIMEHeaderKey call
				// (560 B against Go's 0) on a want-zero AllocsPerRun path, and the local was not
				// even referenced — every use had been folded into the emitted literal (L11).
				// Float/complex OVERFLOW constants keep the per-call parse: their exact string may
				// be a rational ("1/3") whose Parse throws, and a field initializer would turn that
				// per-call throw into a package-class TypeInitializationException.
				hoistedFieldName := ""

				if v.inFunction && hoistToStatic {
					hoistedFieldName = claimHoistedConstFieldName(csIDName)

					if v.currentFuncPrefix.Len() > 0 {
						v.currentFuncPrefix.WriteString(v.newline)
					}

					v.currentFuncPrefix.WriteString("// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)")
					v.currentFuncPrefix.WriteString(v.newline)
					v.currentFuncPrefix.WriteString(fmt.Sprintf("private static readonly GoBigConst %s = GoBigConst.Parse(\"%s\");", hoistedFieldName, constVal))
					v.currentFuncPrefix.WriteString(v.newline)
				}

				if v.inFunction {
					v.writeOutput("GoBigConst %s = /* ", csIDName)
				} else {
					v.writeOutput("%s static readonly GoBigConst %s = /* ", access, csIDName)
				}

				if len(valueSpec.Values) >= i+1 {
					v.outputBuilder.WriteString(v.getPrintedNode(valueSpec.Values[i]))
				}

				v.outputBuilder.WriteString(" */")
				v.writeComment(valueSpec.Comment, tokEnd+token.Pos(len(access)-5))
				v.outputBuilder.WriteString(v.newline)

				if hoistedFieldName != "" {
					v.writeOutput("%s%s;", v.indent(v.indentLevel+1), hoistedFieldName)
				} else {
					v.writeOutput("%sGoBigConst.Parse(\"%s\");", v.indent(v.indentLevel+1), constVal)
				}

				constHandled = true
			}

			if c.Val().Kind() == constant.Int {
				// Use an untyped (BigInteger) const only when the value fits in
				// neither uint64 nor int64. ParseUint alone rejects negatives, which
				// would wrongly promote ordinary negative consts (e.g. -1) to GoBigConst.
				_, errUint := strconv.ParseUint(constVal, 0, 64)
				_, errInt := strconv.ParseInt(constVal, 0, 64)
				if errUint != nil && errInt != nil {
					writeUntypedConst(true)
				}
			}

			if c.Val().Kind() == constant.Float {
				// Check if const float value will exceed float64 limits
				if _, err := strconv.ParseFloat(constVal, 64); err != nil {
					constVal = c.Val().ExactString()
					writeUntypedConst(false)
				}
			}

			if c.Val().Kind() == constant.Complex && !complexRepresentable {
				// A complex constant beyond the declaration's width has NO C# form: GoBigConst is a
				// BigInteger and cannot hold a complex at all, so this emission is knowingly lossy.
				// Warn rather than silently emit something that is not the constant. (Before the
				// representability test was fixed, EVERY complex const took this arm — see
				// exactComplexConstString.)
				v.showWarning("Go complex const exceeds %s range and has no C# representation - verify usage: const %s = %s", csTypeName, goIDName, constVal)
				writeUntypedConst(false)
			}

			if c.Val().Kind() == constant.String {
				if i > 0 {
					v.outputBuilder.WriteString(v.newline)
				}

				// A typed const of a NAMED string type keeps that type: materializing it as
				// @string makes every comparison against a value of the named type ambiguous —
				// the [GoType("@string")] wrapper and @string convert implicitly BOTH ways
				// (CS0034 ×20, net/http pattern.go's `const equivalent relationship =
				// "equivalent"`). The u8-literal initializer binds through the wrapper's
				// ReadOnlySpan<byte> implicit operator (StringSurfaceMembers).
				strTypeName := "@string"

				if isNamedType {
					strTypeName = csTypeName
				}

				if v.inFunction {
					v.writeOutput("%s %s = %s;", strTypeName, csIDName, constVal)
				} else {
					v.writeOutput("%s static readonly %s %s = %s;", access, strTypeName, csIDName, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation-1)
				constHandled = true
			}

			// A native-sized integer constant (nint/nuint, incl. the uintptr alias) whose value
			// does not fit a C# constant of that type — e.g. `uintptr MaxUintptr = ^uintptr(0)`
			// = 0xFFFFFFFFFFFFFFFF, a ulong literal needing a non-constant nuint conversion
			// (CS0133/CS0266) — must be emitted as `static readonly` with an unchecked cast
			// rather than `const`. Small native-int consts (e.g. `const nint iota = 0`) are fine.
			nativeIntConst := false
			uintptrConst := false

			// complex128 is System.Numerics.Complex and complex64 is a golib struct; C# forbids
			// `const` of a library struct entirely (CS0283) — the same gap uintptr has — so a
			// representable complex const is `static readonly`. An UntypedComplex-typed one is
			// already routed there by isNamedType.
			complexConst := complexRepresentable && (csTypeName == "complex128" || csTypeName == "complex64")

			// A NAMED type over uintptr (`type Handle uintptr`) has the same gap as raw uintptr:
			// its generated struct bridges the numeric world only through nuint/UntypedInt
			// (UintptrBridgeOperators), so a beyond-int32 folded constant renders as a ulong
			// literal with no implicit path (CS0266 — syscall InvalidHandle = ^Handle(0)).
			namedUintptrType := false

			if isNamedType {
				if basic, ok := c.Type().Underlying().(*types.Basic); ok {
					// A named type over a WIDE UNSIGNED integer (uintptr / uint / uint64) whose folded
					// constant value exceeds int32 needs the same unchecked cast as a native-int const:
					// `^Class(0)` (bidi) / `^big.Word(0)` (go/constant) fold to the underlying's all-ones
					// value (a `ulong` literal in C#), which has no implicit conversion to the named
					// `[GoType]` wrapper (CS0266). Narrow-unsigned underlyings (byte/uint16) fold to
					// small values that the wrapper's int operator still accepts, so they are excluded.
					switch basic.Kind() {
					case types.Uintptr, types.Uint, types.Uint64:
						namedUintptrType = true
					}
				}
			}

			if c.Val().Kind() == constant.Int && (csTypeName == "nint" || csTypeName == "nuint" || csTypeName == "uintptr" || namedUintptrType) {
				// A C# constant of a native-int type only accepts an int-range literal; a value
				// beyond int32 (e.g. runtime/alg's `uintptr c0 = 33054211828000289`) has no
				// implicit/constant conversion to nint/nuint (CS0133/CS0266), so it must be emitted
				// as `static readonly` with an unchecked cast rather than `const`.
				if _, errInt := strconv.ParseInt(constVal, 0, 32); errInt != nil {
					nativeIntConst = true
				}

				// uintptr is a golib STRUCT (golib/uintptr.cs — distinct from uint), and C# forbids
				// `const` of a user struct entirely: every uintptr const is `static readonly`. An
				// int-range value still initializes via the constant-conversion chain (`= 1`), so
				// the unchecked cast stays reserved for the beyond-int32 case above.
				if csTypeName == "uintptr" {
					uintptrConst = true
				}
			}

			if !constHandled {
				if i > 0 {
					v.outputBuilder.WriteString(v.newline)
				}

				// golib's builtin declares `const nint iota = 0`, so a const initialized by
				// exactly the builtin `iota` references that constant directly when it can
				// express the SAME value at an ACCEPTING emitted type: an UntypedInt wrapper
				// takes the nint implicitly, and a declaration EMITTED at nint (explicit Go
				// `int`, or tightened to it) matches golib's constant type exactly —
				// `const nint stateInit = iota;` (compress/flate's stepState group). The `0`
				// gate keeps LATER group positions folded (`x = iota` at position 1 folds to
				// 1, which golib's constant cannot express), and any other emitted type —
				// named wrappers, non-int widths — keeps the folded `/* iota */ N` form
				// rather than casting golib's nint.
				if constVal == "0" && (csTypeName == "nint" || csTypeName == "UntypedInt") && len(valueSpec.Values) >= i+1 {
					if iotaIdent, ok := valueSpec.Values[i].(*ast.Ident); ok && iotaIdent.Name == "iota" {
						if obj := v.info.Uses[iotaIdent]; obj != nil && obj.Parent() == types.Universe {
							constVal = "iota"
						}
					}
				}

				// A plain integer-literal initializer keeps its Go source formatting when it is
				// also a valid C# literal (hex/binary/`_` separators — preserveGoIntLiteral):
				// `const m5 = 0x1d8e4e27c47d124f` emits the hex directly, which also elides the
				// now-redundant `/* original */` comment below. Folded expressions/iota keep the
				// comment form; the GoBigConst path above keeps the decimal (BigInteger.Parse).
				if c.Val().Kind() == constant.Int && len(valueSpec.Values) >= i+1 {
					if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok && lit.Kind == token.INT {
						constVal = preserveGoIntLiteral(lit.Value, constVal)
					}
				}

				orgExpr := ""

				if len(valueSpec.Values) >= i+1 {
					orgExpr = strings.TrimSpace(v.getPrintedNode(valueSpec.Values[i]))
				}

				if constVal == orgExpr {
					orgExpr = ""
				} else {
					// Try parse both constVal and orgExpr as floating point numbers to see if they are same
					if constNum, err := strconv.ParseFloat(constVal, 64); err == nil {
						if orgNum, err := strconv.ParseFloat(orgExpr, 64); err == nil {
							if constNum == orgNum {
								orgExpr = ""
							}
						}
					}

					if len(orgExpr) > 0 {
						if strings.Contains(orgExpr, "unsafe.Sizeof") {
							v.showWarning("Go const converted to C# using 'unsafe.Sizeof' may not match run-time value - verify usage: const %s = %s", goIDName, orgExpr)
						}

						orgExpr = fmt.Sprintf(" /* %s */", orgExpr)
					}
				}

				var constExpr string
				constValExpr := constVal

				if isNamedType {
					constExpr = "static readonly"

					// Beyond-int32 named-uintptr const: same unchecked cast the native-int
					// consts use — `unchecked((ΔHandle)18446744073709551615)`.
					if nativeIntConst {
						// A named type whose WRITTEN base is a CROSS-PACKAGE named type (registry's
						// `type Key syscall.Handle` — [GoType("syscall_package.ΔHandle")]) has NO
						// numeric bridge of its own; the literal hops through the base, one user
						// conversion per cast: `unchecked((Key)(syscall_package.ΔHandle)2147483648)`.
						baseHop := ""

						if named, ok := types.Unalias(c.Type()).(*types.Named); ok {
							if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil {
								if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed.Obj().Pkg() != named.Obj().Pkg() {
									baseHop = fmt.Sprintf("(%s)", v.getCSharpTypeName(rhsNamed))
								}
							}
						}

						constValExpr = fmt.Sprintf("unchecked((%s)%s%s)", csTypeName, baseHop, constVal)
					}
				} else if nativeIntConst {
					constExpr = "static readonly"
					constValExpr = fmt.Sprintf("unchecked((%s)%s)", csTypeName, constVal)
				} else if uintptrConst || complexConst {
					constExpr = "static readonly"
				} else {
					constExpr = "const"

					// A float32 const initialized from a (double) literal needs an `f` suffix —
					// `const float hashLoad = 6.5` is CS0664 without it. Applied to the emitted value
					// only (constVal is still used above for the doc-comment elision check).
					if c.Val().Kind() == constant.Float && csTypeName == "float32" {
						constValExpr += "f"
					}
				}

				if v.inFunction {
					// C# locals cannot be declared "static readonly"; for named
					// (custom) types "const" is also invalid, so emit a plain local
					// variable. Primitive/string consts can still use "const" locally.
					if isNamedType || nativeIntConst || uintptrConst || complexConst {
						v.writeOutput("%s %s =%s %s;", csTypeName, csIDName, orgExpr, constValExpr)
					} else {
						v.writeOutput("%s %s %s =%s %s;", constExpr, csTypeName, csIDName, orgExpr, constValExpr)
					}
				} else if constExpr == "const" {
					v.writeOutput("%s %s %s %s =%s %s;", access, constExpr, csTypeName, csIDName, orgExpr, constValExpr)
				} else {
					// A Go constant has NO initialization: it is a compile-time value usable from
					// anywhere in the package regardless of declaration order. When C# cannot say
					// `const` — a [GoType] struct (UntypedInt/UntypedFloat/UntypedComplex, a named
					// type, uintptr, complex) is not a legal constant type — a `static readonly`
					// FIELD reintroduces initialization, and C# runs static field initializers in
					// class-TEXTUAL order. A package-level variable declared ahead of the constant
					// then reads it as the type's DEFAULT, silently: compress/flate declares
					// `var fixedHuffmanDecoder huffmanDecoder` before `huffmanNumChunks`, so the
					// struct's `chunks = new(huffmanNumChunks)` field initializer allocated a
					// length-0 table (Go: 512) — `init` filled nothing and every later
					// `chunks[i]` read panicked with "index out of range with length 0"; the same
					// order trap zeroed `maxNumLit` for `fixedLiteralEncoding`'s initializer
					// across files (Compile-item order).
					//
					// A get-only property carries no initialization, so declaration order cannot be
					// observed and the JIT folds the literal at every use — the Go semantics
					// exactly. RESIDUE: the two ALLOCATING const forms stay fields, because a
					// property would rebuild their value on every read — `@string` (the u8-literal
					// hoisting the string-literal arc introduced) and `GoBigConst` (a BigInteger
					// parse). Both remain order-sensitive; neither can serve as an array length.
					v.writeOutput("%s static %s %s =>%s %s;", access, csTypeName, csIDName, orgExpr, constValExpr)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation+1)
			}
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}

// convInterfaceDeclValue renders a var-decl initializer. When the declared type is a non-empty
// interface (ifaceDeclType non-nil), the value routes through the interface conversion: a POINTER
// value renders as the box and wraps in the pointer-interface adapter (`var inc Incrementer = c`
// emits `Incrementer inc = new CounterᴵIncrementer(c)`) — Go's interface value holds the *T.
// An EMPTY-interface declared type (emptyIfaceDeclType non-nil) has no adapter, so a POINTER value
// takes the boundary treatment directly: the box, carrying its Go type even when it is nil (see
// typedNilInterfaceBoxing.go). Neither set renders the plain expression.
func (v *Visitor) convInterfaceDeclValue(value ast.Expr, ifaceDeclType types.Type, emptyIfaceDeclType types.Type, context ExprContext) string {
	if ifaceDeclType == nil {
		contexts := v.emptyInterfacePointerContexts(emptyIfaceDeclType, value, []ExprContext{context})

		// A `var` declaration initialized from an existing array value takes golib's
		// `.Clone()` for independent backing storage (see cloneValueCopy).
		rendered := v.cloneValueCopy(nil, value, v.convExpr(value, contexts))

		return v.boxPointerIntoEmptyInterface(emptyIfaceDeclType, value, rendered)
	}

	rhsType := v.info.TypeOf(value)
	contexts := []ExprContext{context}

	if _, isPtr := rhsType.(*types.Pointer); isPtr {
		identContext := DefaultIdentContext()
		identContext.isPointer = true
		contexts = []ExprContext{identContext, context}
	}

	return v.convertToInterfaceType(ifaceDeclType, rhsType, v.convExpr(value, contexts))
}

// visitPackageTupleVarSpec emits a package-level `var a, b = f()` — a single multi-value call
// initializer with no explicit type. C# static field initializers cannot deconstruct a tuple, so
// each non-blank name reads its ValueTuple component. With exactly ONE non-blank name the call
// stays inline on that field and reads its component (`internal static ж<Point> identity =
// @new<Point>().SetBytes(…).Item1;` — blank names keep the plain path's uninitialized `_ᴛNʗ`
// field emission, and the call still runs exactly once). With two or more non-blank names the
// call is evaluated ONCE into a hidden tuple field and each name reads its component from it —
// C# static field initializers run in textual order within a class part, so the reads follow the
// temp. `.ItemN` binds both unnamed and named result tuples. (Comma-ok package vars —
// `var v, ok = m[k]` — are not calls and keep the existing path; no stdlib occurrence.)
//
// A spec collectMovedInitVars flagged for init-order relocation relocates as ONE unit instead —
// see writeMovedPackageTupleVarSpec.
func (v *Visitor) visitPackageTupleVarSpec(valueSpec *ast.ValueSpec, tuple *types.Tuple) {
	nonBlankCount := 0

	for _, ident := range valueSpec.Names {
		if ident.Name != "_" {
			nonBlankCount++
		}
	}

	// Go's InitOrder yields ONE entry per spec with every name in its Lhs, so
	// collectMovedInitVars flags all of a spec's non-blank names together under one shared
	// ordinal — the first non-blank name answers for the whole spec. Blank names are never
	// flagged (their values are unreadable, so their order is immaterial), which also means an
	// all-blank spec (`var _, _ = f()`) always keeps the inline path below, where the first
	// blank's field initializer carries the call for its side effect.
	ordinal := 0
	moved := false

	for _, ident := range valueSpec.Names {
		if ident.Name == "_" {
			continue
		}

		if def := v.info.Defs[ident]; def != nil {
			ordinal, moved = v.movedInitOrdinal(def)
		}

		break
	}

	context := DefaultBasicLitContext()
	context.u8StringOK = true

	callExpr := v.convExpr(valueSpec.Values[0], []ExprContext{context})

	if moved {
		v.writeMovedPackageTupleVarSpec(valueSpec, tuple, callExpr, nonBlankCount, ordinal)
		return
	}

	componentSource := callExpr
	firstLine := true

	if nonBlankCount > 1 {
		// Hidden once-evaluated tuple holder; the named fields read components from it.
		tempName := getGlobalTempVarName("tuple") + CapturedVarMarker
		componentTypes := make([]string, tuple.Len())

		for i := range tuple.Len() {
			componentTypes[i] = v.getCSharpTypeName(tuple.At(i).Type())
		}

		v.writeOutput("internal static (%s) %s = %s;", strings.Join(componentTypes, ", "), tempName, callExpr)
		componentSource = tempName
		firstLine = false
	}

	for i, ident := range valueSpec.Names {
		goIDName := v.getIdentName(ident)
		csIDName := getSanitizedIdentifier(goIDName)
		isBlank := csIDName == "_"

		if isBlank {
			csIDName = getGlobalTempVarName("_") + CapturedVarMarker
		}

		if !firstLine {
			v.outputBuilder.WriteString(v.newline)
		}

		firstLine = false
		csTypeName := v.getCSharpTypeName(tuple.At(i).Type())
		access := v.testDeclaredValueAccess(getAccess(goIDName), ident.Pos(), tuple.At(i).Type())

		// An ALL-BLANK spec (`var _, _ = f()`) must still evaluate the call once for its side
		// effect: carry it on the first blank's initializer. Otherwise blanks stay uninitialized —
		// the call already ran via the non-blank/temp field.
		if isBlank && !(nonBlankCount == 0 && i == 0) {
			v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
		} else if v.isAddressedGlobal(ident) {
			v.writeAddressedGlobalDecl(access, csTypeName, csIDName, fmt.Sprintf("%s.Item%d", componentSource, i+1), isInherentlyHeapAllocatedType(tuple.At(i).Type()))
		} else {
			v.writeOutput("%s static %s %s = %s.Item%d;", access, csTypeName, csIDName, componentSource, i+1)
		}
	}

	v.writeComment(valueSpec.Comment, valueSpec.End())
}

// writeMovedPackageTupleVarSpec emits a package-level tuple var spec whose initialization
// collectMovedInitVars flagged for relocation into the ordered static ctor (package_init.cs).
// Every name becomes a BARE field — blank names keep their uninitialized `_ᴛNʗ` fields (the call
// now runs in the ctor, so no blank ever carries it), and an addressed global's box is declared
// default-valued with the relocated assignment writing through its ref property into the same
// box, both mirroring the plain moved path — and the call moves into ONE per-file initᴛ method
// registered at the spec's InitOrder ordinal (one method per spec, matching Go's one InitOrder
// entry per spec, so writeOrderedInitCalls needs no new bookkeeping). With a single non-blank
// name the method assigns its component directly (`identity = f(…).Item1;`); with two or more it
// evaluates the call once into a method-local and assigns each non-blank component from it — the
// inline path's hidden static tuple holder is unnecessary here, because the method body itself
// sequences the call before its reads. The local reuses the holder's minted `tupleᴛNʗ` name shape
// so it cannot collide with anything the rendered call expression references.
func (v *Visitor) writeMovedPackageTupleVarSpec(valueSpec *ast.ValueSpec, tuple *types.Tuple, callExpr string, nonBlankCount int, ordinal int) {
	componentSource := callExpr

	if nonBlankCount > 1 {
		componentSource = getGlobalTempVarName("tuple") + CapturedVarMarker
	}

	assignments := make([]string, 0, nonBlankCount)
	methodName := ""
	firstLine := true

	for i, ident := range valueSpec.Names {
		goIDName := v.getIdentName(ident)
		csIDName := getSanitizedIdentifier(goIDName)

		if csIDName == "_" {
			csIDName = getGlobalTempVarName("_") + CapturedVarMarker
		} else {
			if len(methodName) == 0 {
				methodName = packageInitMethodName(csIDName)
			}

			assignments = append(assignments, fmt.Sprintf("%s = %s.Item%d;", csIDName, componentSource, i+1))
		}

		if !firstLine {
			v.outputBuilder.WriteString(v.newline)
		}

		firstLine = false
		csTypeName := v.getCSharpTypeName(tuple.At(i).Type())
		access := v.testDeclaredValueAccess(getAccess(goIDName), ident.Pos(), tuple.At(i).Type())

		if v.isAddressedGlobal(ident) {
			v.writeAddressedGlobalDecl(access, csTypeName, csIDName, "", isInherentlyHeapAllocatedType(tuple.At(i).Type()))
		} else {
			v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
		}
	}

	v.outputBuilder.WriteString(v.newline)

	if nonBlankCount > 1 {
		v.writeOutput("internal static void %s() { var %s = %s; %s }", methodName, componentSource, callExpr, strings.Join(assignments, " "))
	} else {
		v.writeOutput("internal static void %s() { %s }", methodName, assignments[0])
	}

	recordMovedInitMethod(ordinal, methodName)
	v.writeComment(valueSpec.Comment, valueSpec.End())
}

// claimHoistedConstFieldName claims a package-unique `static readonly GoBigConst` field name for a
// hoisted function-local big constant: the Go const's C# name suffixed with HoistedConstMarker,
// plus an ordinal when the base name is already taken (two functions — or two files — declaring
// `const mask = <big>` both land in the one `<pkg>_package` partial class). Deterministic because
// files convert sequentially in sorted order; a `-tests` internal variant starts from the
// production conversion's claim counts (productionHoistedConstOrdinals) because the production
// fields already exist on disk in the same class.
func claimHoistedConstFieldName(csIDName string) string {
	packageLock.Lock()
	defer packageLock.Unlock()

	if packageHoistedConstOrdinals == nil {
		packageHoistedConstOrdinals = make(map[string]int)

		for name, count := range productionHoistedConstOrdinals {
			packageHoistedConstOrdinals[name] = count
		}
	}

	ordinal := packageHoistedConstOrdinals[csIDName]
	packageHoistedConstOrdinals[csIDName] = ordinal + 1

	if ordinal == 0 {
		return csIDName + HoistedConstMarker
	}

	return fmt.Sprintf("%s%s%d", csIDName, HoistedConstMarker, ordinal)
}
