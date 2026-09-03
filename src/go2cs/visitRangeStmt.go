// visitRangeStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// rangeVarRootIdent returns the identifier at the ROOT of a field/element access path
// (`test.x.n` → `test`), or nil when the path is not rooted in a plain identifier.
//
// Addressability in Go flows from the root variable through every VALUE hop, so a write to — or an
// address taken of — any part of that path needs the root itself to be addressable. C# models the
// same rule (CS1654/CS1655 name "fields of" the iteration variable), which is why the range-var
// triggers below must test the path ROOT rather than the immediate operand.
//
// The walk STOPS at the first hop through a POINTER: past a pointer the emitted access goes through
// the heap box (`ж<T>.Value`), which is an independent mutable lvalue, so the iteration variable's
// read-only-ness no longer reaches it and no copy is warranted. Indexing likewise only continues
// through an ARRAY (whose storage is in-place inside the struct); a slice or map index reaches a
// separate backing store and is therefore its own addressability root.
func (v *Visitor) rangeVarRootIdent(expr ast.Expr) *ast.Ident {
	for {
		switch e := expr.(type) {
		case *ast.Ident:
			return e
		case *ast.ParenExpr:
			expr = e.X
		case *ast.SelectorExpr:
			if v.exprHopIsPointer(e.X) {
				return nil
			}

			expr = e.X
		case *ast.IndexExpr:
			operandType := types.Unalias(v.info.TypeOf(e.X))

			if operandType == nil {
				return nil
			}

			if _, isArray := operandType.Underlying().(*types.Array); !isArray {
				return nil
			}

			expr = e.X
		default:
			return nil
		}
	}
}

// exprHopIsPointer reports whether an access path hop is through a pointer — see rangeVarRootIdent.
func (v *Visitor) exprHopIsPointer(expr ast.Expr) bool {
	exprType := types.Unalias(v.info.TypeOf(expr))

	if exprType == nil {
		return false
	}

	_, isPtr := exprType.Underlying().(*types.Pointer)

	return isPtr
}

// rangeVarNeedsMutableCopy reports whether the range key/value identifier must be copied into a
// mutable local in the loop body. Three triggers, each rooted at the range var via
// rangeVarRootIdent: (1) the var — or a field/element PATH rooted at it — is written (via `=`,
// `+=`, `-=`, `++`, …); Go lets a range variable and its members be written (it is a per-iteration
// copy), but a C# `foreach` iteration variable is read-only and so are its fields (CS1656/CS1654);
// (2) a POINTER-RECEIVER method is called on the var or on a value path rooted at it — the emitted
// `[GoRecv]` form takes `this ref T`, and neither a foreach iteration variable nor its fields can
// bind ref (CS1657/CS1655 — dnsmessage's GoString, and the table-driven `test.x.String()` idiom
// that dominates the standard library's own tests); (3) the address of the var or of such a path is
// taken (`&test.x`), which likewise requires an addressable root. Such a var must be iterated
// through a temp and copied into a mutable local. A `:=` redeclaration shadows into a new object,
// so it is not counted (the object identity check excludes it).
func (v *Visitor) rangeVarNeedsMutableCopy(expr ast.Expr, body *ast.BlockStmt) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok || ident.Name == "_" {
		return false
	}

	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		switch s := n.(type) {
		case *ast.AssignStmt:
			if s.Tok == token.DEFINE {
				return true
			}

			// A write to the range var itself, or through a field/element PATH rooted at it
			// (`dt.dll, _ = getString(…)`, debug/pe's importedSymbols; `test.x.n = 99`),
			// modifies the iteration variable or its members (CS1656/CS1654) — the
			// mutable-copy escape applies to both.
			for _, lhs := range s.Lhs {
				if root := v.rangeVarRootIdent(lhs); root != nil && v.info.ObjectOf(root) == obj {
					found = true
					return false
				}
			}
		case *ast.IncDecStmt:
			if root := v.rangeVarRootIdent(s.X); root != nil && v.info.ObjectOf(root) == obj {
				found = true
				return false
			}
		case *ast.UnaryExpr:
			// Taking the address of the range var or of a value path rooted at it (`&test.x`)
			// requires an addressable root (CS1655 on a foreach var's fields).
			if s.Op == token.AND {
				if root := v.rangeVarRootIdent(s.X); root != nil && v.info.ObjectOf(root) == obj {
					found = true
					return false
				}
			}
		case *ast.SelectorExpr:
			// A pointer-receiver method selected on the (value-typed) range var — directly
			// (`q.GoString()`) or on a value path rooted at it (`test.x.String()`, the
			// table-driven test idiom) — binds the `[GoRecv]` `this ref` extension, which needs
			// a mutable lvalue (CS1657/CS1655 on a foreach var and its fields). A receiver that
			// is itself a POINTER dereferences through its box instead and stays legal, so it
			// is excluded.
			root := v.rangeVarRootIdent(s.X)

			if root == nil || v.info.ObjectOf(root) != obj {
				return true
			}

			sel, ok := v.info.Selections[s]

			if !ok || sel.Kind() != types.MethodVal {
				return true
			}

			if sig, ok := sel.Obj().Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, recvIsPtr := sig.Recv().Type().(*types.Pointer); recvIsPtr {
					if _, recvExprIsPtr := v.info.TypeOf(s.X).(*types.Pointer); !recvExprIsPtr {
						found = true
						return false
					}
				}
			}
		}

		return true
	})

	return found
}

func (v *Visitor) visitRangeStmt(rangeStmt *ast.RangeStmt, target LabeledStmtContext) {
	v.outputBuilder.WriteString(v.newline)

	// Everything the range EXPRESSION emits must land after this newline and before the `foreach`
	// header, which is written further below — record the splice point now.
	hoistPos := v.outputBuilder.Len()
	var ptrDeref string

	// A func literal inside the range expression — the table-driven test idiom
	// `for _, test := range []struct{ f func() }{{f: func(){ … bg … }}}` — emits its
	// captured-variable snapshot declarations (`var bgʗ1 = bg;`), which are STATEMENTS and cannot
	// appear in the composite-literal element position they would otherwise be written into
	// (context's TestAllocs/TestCause: CS1003/CS1026/CS1002 ×195, a whole-file syntax cascade).
	// Convert the range expression into a hoist buffer so those decls are spliced onto their own
	// lines before the loop, exactly as visitForStmt does for a for-condition. Save/restore guards
	// nesting (a range inside an expression that already established a sink).
	savedHoist := v.hoistedDecls
	hoistBuf := &strings.Builder{}
	v.hoistedDecls = hoistBuf
	rangeExpr := v.convExpr(rangeStmt.X, nil)
	v.hoistedDecls = savedHoist

	if hoistBuf.Len() > 0 {
		// The buffer carries a leading newline+indent per decl and a trailing newline (convFuncLit
		// trims the trailing indent); the newline written above already opened the line, so drop the
		// buffer's leading one and splice the rest in ahead of anything the expression emitted.
		v.spliceOutput(hoistPos, strings.TrimPrefix(hoistBuf.String(), v.newline))
	}

	// Resolve a type ALIAS (*types.Alias, Go 1.22+ — fiat's `type p224UntypedFieldElement =
	// [4]uint64`) to the type it aliases: the arm dispatch below type-switches on the concrete
	// type, and an unhandled alias fell through every arm — silently emitting the ENTIRE loop
	// as a C# comment. Applied again after the pointer unwrap (the pointee may itself alias).
	rangeType := types.Unalias(v.getExprType(rangeStmt.X))

	// Ranging THROUGH a pointer shares the pointee — Go copies nothing there — so the array-value
	// snapshot below must not fire for it. Recorded here because the unwrap immediately makes
	// `isArray` true for a `*[N]T` operand, and ptrDeref cannot stand in for the test (it is cleared
	// again for a pointer PARAMETER, which the implicit-deref local has already dereferenced).
	rangedThroughPointer := false

	if ptrType, ok := rangeType.(*types.Pointer); ok {
		rangedThroughPointer = true
		rangeType = types.Unalias(ptrType.Elem())
		ptrDeref = ".Value"

		// A pointer parameter is implicitly dereferenced to a value local in the body
		// (`ref var p = ref Ꮡp.Value`), so `p` already denotes the pointed-to value — ranging over
		// it must not add a second `.Value` (which would be applied to the value type, CS1061).
		if ident, ok := rangeStmt.X.(*ast.Ident); ok && v.identIsParameter(ident) {
			ptrDeref = ""
		}

		// When the range expression is a pointer-typed TYPE CONVERSION it renders as a C# cast
		// `(ж<…>)(uintptr)(x)` (nistec's `range (*[43*32*2*4][8]byte)(*p256PrecomputedPtr)`). A cast
		// binds LOWER than member access, so the later `%s%s` = rangeExpr+ptrDeref would parse
		// `(ж<…>)(x).Value` as `(ж<…>)((x).Value)` — the `.Value` deref lands on the operand, not the
		// cast RESULT (CS1579/CS8130). Parenthesize so the deref applies to the whole cast.
		if ptrDeref != "" {
			if callExpr, ok := rangeStmt.X.(*ast.CallExpr); ok {
				// The call is a TYPE CONVERSION when its Fun is a type expression (`(*[N][8]byte)(x)`,
				// including the unsafe.Pointer-based pointer conversions isTypeConversion excludes) —
				// its C# rendering is a cast, which binds lower than the `.Value` deref appended below.
				if tv, ok := v.info.Types[callExpr.Fun]; ok && tv.IsType() {
					rangeExpr = fmt.Sprintf("(%s)", rangeExpr)
				}
			}
		}
	}

	// Get the underlying type if it's a named type. Remember the named-ness: a NAMED func
	// type renders as a C# DELEGATE whose range() adaptation needs the method group
	// (`range(seq.Invoke)` — see the yield-func emission below).
	rangeIsNamedType := false

	if named, ok := rangeType.(*types.Named); ok {
		rangeIsNamedType = true
		rangeType = named.Underlying()
	}

	// Get the constraint type if it's a type parameter
	if typeParam, ok := rangeType.(*types.TypeParam); ok {
		rangeType = v.getConstraintType(typeParam)
	}

	var isSlice, isArray, isMap, isChan, isStr, untypedStr, isInt, untypedInt bool
	var intBasic *types.Basic
	yieldFunc := -1

	// Slice type is expected to be most common, so this is checked first
	_, isSlice = rangeType.(*types.Slice)

	if !isSlice {
		_, isArray = rangeType.(*types.Array)
	}

	if !isSlice && !isArray {
		_, isMap = rangeType.(*types.Map)

		if !isMap {
			_, isChan = rangeType.(*types.Chan)
		}

		if !isMap && !isChan {
			yieldFunc = isYieldFunc(rangeType)
		}

		if !isMap && !isChan && yieldFunc == -1 {
			if basicType, ok := rangeType.(*types.Basic); ok {
				kind := basicType.Kind()

				// Go's range-over-integer (Go 1.22) accepts ANY integer type, not just `int`
				// — `for range atyp.Len` over a uintptr is legal, and internal/abi's
				// buildArrayCloneSeq (unique/clone.go) is the corpus site that proved it.
				// Recognizing only types.Int made every other integer kind fall through to the
				// "unexpected 'ast.RangeStmt' expression" arm below, which emits the whole
				// statement as a COMMENT — the loop silently vanished from the conversion.
				isInt = basicType.Info()&types.IsInteger != 0
				untypedInt = kind == types.UntypedInt

				if isInt {
					intBasic = basicType
				} else {
					isStr = kind == types.String || kind == types.UntypedString
					untypedStr = kind == types.UntypedString
				}
			}
		}
	}

	if !isSlice && !isArray && !isMap && !isChan && yieldFunc == -1 && !isInt && !isStr {
		rangeVal := v.getPrintedNode(rangeStmt)
		v.showWarning("@visitRangeStmt - unexpected 'ast.RangeStmt' expression %s", rangeVal)
		v.writeOutput("/* %s */", rangeVal)
		return
	}

	var valExpr, valType, keyExpr, keyType string
	var assignVars bool

	// key/value in a slice or array: index/value
	// key/value in a map: key/value
	// key/value in a string: index/rune
	// channel: element only
	// int: value only
	// func(func() bool): yield only
	// func(func(V) bool): yield with value
	// func(func(K, V) bool): yield with key/value

	context := DefaultBlockStmtContext()
	context.format.useNewLine = false

	if rangeStmt.Key != nil {
		keyExpr = v.convExpr(rangeStmt.Key, nil)
		assignVars = rangeStmt.Tok == token.ASSIGN
	}

	if keyExpr == "" {
		keyExpr = "_"
	}

	if rangeStmt.Value != nil {
		valExpr = v.convExpr(rangeStmt.Value, nil)
	}

	if valExpr == "" && !(isChan || isInt || yieldFunc == 0 || yieldFunc == 1) {
		valExpr = "_"
	}

	// A heap-boxed range variable (its address is taken, `for i := range s { p := &i }`) in a
	// slice/array/map range must be boxed PER ITERATION (Go 1.22 per-iteration variable semantics:
	// a stored `&i` must point to a DISTINCT box each pass). Its heap decl is therefore deferred
	// into the loop body and the foreach iterates a temp var that is copied into the fresh box —
	// rather than declaring the box once before the loop (which would also clash with the foreach's
	// own re-declaration of the same name → CS0136). Captured here, emitted in the DEFINE branch.
	var keyHeapDecl, valHeapDecl string
	deferRangeVarBox := !isStr && !isChan && !isInt && yieldFunc <= -1

	// If defining new variables, perform escape analysis on the key and value expressions
	if !assignVars {
		var wroteHeapTypeDecl bool

		if keyExpr != "_" {
			// Get ident for key expression and check for heap allocation
			if ident, ok := rangeStmt.Key.(*ast.Ident); ok {
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					if deferRangeVarBox {
						keyHeapDecl = heapTypeDecl
					} else {
						v.writeOutput(heapTypeDecl)
						v.outputBuilder.WriteString(v.newline)
						wroteHeapTypeDecl = true
					}
				} else if !v.options.preferVarDecl {
					keyType = v.getCSharpTypeName(v.getExprType(rangeStmt.Key)) + " "
				}
			}
		}

		if len(valExpr) > 0 && valExpr != "_" {
			// Get ident for value expression and check for heap allocation
			if ident, ok := rangeStmt.Value.(*ast.Ident); ok {
				heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(heapTypeDecl) > 0 {
					if deferRangeVarBox {
						valHeapDecl = heapTypeDecl
					} else {
						v.writeOutput(heapTypeDecl)
						v.outputBuilder.WriteString(v.newline)
						wroteHeapTypeDecl = true
					}
				} else if !v.options.preferVarDecl {
					valType = v.getCSharpTypeName(v.getExprType(rangeStmt.Value)) + " "
				}
			}
		}

		if wroteHeapTypeDecl {
			v.outputBuilder.WriteString(v.newline)
		}
	}

	var varInit string

	if v.options.preferVarDecl && !(keyExpr == "_" && (len(valExpr) == 0 || valExpr == "_")) {
		varInit = "var "
	}

	// A slice/array/map range VALUE of ARRAY type — and a map range KEY of ARRAY type — is a
	// fresh per-iteration COPY in Go, but the emitted foreach tuple element is a struct over a
	// shared T[] backing store, so writes through the variable would reach the container's
	// elements (and later container writes would show through the variable). Route such a
	// variable through the same iterate-a-temp mechanism as a reassigned var, appending the
	// strongly-typed `.Clone()` when it is copied into the variable. A channel range is
	// excluded: the SEND site clones (see visitSendStmt), and a buffered element is received
	// exactly once, so the received struct copy is already unaliased.
	arrayCloneArm := isSlice || isArray || isMap
	keyIsArrayValue := arrayCloneArm && rangeStmt.Key != nil && keyExpr != "_" && typeNeedsValueClone(v.getExprType(rangeStmt.Key))
	valIsArrayValue := arrayCloneArm && rangeStmt.Value != nil && len(valExpr) > 0 && valExpr != "_" && typeNeedsValueClone(v.getExprType(rangeStmt.Value))

	keyCloneSuffix, valCloneSuffix := "", ""

	if keyIsArrayValue {
		keyCloneSuffix = valueCloneSuffix(v.getExprType(rangeStmt.Key))
	}

	if valIsArrayValue {
		valCloneSuffix = valueCloneSuffix(v.getExprType(rangeStmt.Value))
	}

	// Go evaluates the range EXPRESSION once before the loop, so ranging an array VALUE iterates a
	// COPY: a write to the container inside the body is invisible to every later iteration. The
	// emitted `array<T>` (and the generated named-array wrapper) is a struct over a shared T[]
	// backing, so the plain operand aliases the container and the loop reads the writes — the same
	// aliasing every other Go by-value array transfer answers with a copy, and the range expression
	// is simply the copy site nobody had emitted yet.
	//
	// The copy is RangeSnapshotMethod, not the `.Clone()` the other transfer sites take: a snapshot
	// cannot outlive its loop, which makes it Go's inline stack copy — zero mallocs, zero TotalAlloc
	// — and golib's counter mirrors runtime.MemStats.Mallocs, so charging it would disagree with Go
	// wherever an allocation assertion surrounds such a loop. See appendRangeSnapshot.
	//
	// Scoped exactly as gc's own rule is (cmd/compile/internal/walk/order.go's rangeStmt): the copy
	// is taken only when a VALUE iteration variable is present and not blank — with at most one
	// iteration variable and a constant length, Go does not evaluate the range expression at all, so
	// `for i := range a` reads the LIVE array through the index and must stay uncopied. Ranging a
	// POINTER to an array shares the pointee in Go too, and a slice's copy is its header, which the
	// struct assignment already is. exprReadsValueNeedingClone narrows the rest: only a read out of
	// EXISTING storage (ident, selector, index, deref) can alias — a composite literal, call result
	// or conversion is freshly constructed and reachable by no other name.
	if isArray && !rangedThroughPointer && rangeStmt.Value != nil && len(valExpr) > 0 && valExpr != "_" && v.exprReadsValueNeedingClone(rangeStmt.X) {
		rangeExpr = appendRangeSnapshot(rangeExpr)
	}

	// A newly-DEFINED range var that is reassigned in the body — or that receives a pointer-
	// receiver ([GoRecv] `this ref`) method call, or holds an ARRAY value (the by-value copy
	// above) — must become a mutable local: a C# foreach iteration variable is read-only
	// (CS1656) and cannot bind ref (CS1657). The string and slice/array/map emissions iterate
	// a temp and declare the var from it in the body (`foreach (var (_, rᴛ1) in s) { var r =
	// rᴛ1; … }`).
	keyNeedsCopy := !assignVars && (v.rangeVarNeedsMutableCopy(rangeStmt.Key, rangeStmt.Body) || keyIsArrayValue)
	valNeedsCopy := !assignVars && (v.rangeVarNeedsMutableCopy(rangeStmt.Value, rangeStmt.Body) || valIsArrayValue)

	// A blank iteration variable in a SCALAR foreach position (chan / int / single-value yield
	// ranges) must not be emitted as `_`: there C# declares a genuine read-only variable NAMED
	// `_` — not a discard — which shadows the discard idiom for the whole body, so a Go blank
	// assignment inside (`for range b.N { _ = Size(x) }`, encoding/binary's BenchmarkSize) becomes
	// an illegal write to the iteration variable (CS1656). Tuple positions keep `_` (a
	// deconstruction `_` IS a C# discard); scalar positions get a marked temp instead.
	scalarKeyExpr := keyExpr

	if keyExpr == "_" && (isChan || isInt || yieldFunc == 0 || yieldFunc == 1) {
		scalarKeyExpr = v.getTempVarName("_")
	}

	if isStr {
		if untypedStr {
			rangeExpr = fmt.Sprintf("(@string)%s", rangeExpr)
		}

		if assignVars || keyNeedsCopy || valNeedsCopy {
			// `assignVars` copies into pre-existing vars (`r = rᴛ1;`); a reassigned DEFINE var is
			// declared from the temp (`var r = rᴛ1;`).
			var innerPrefix, tempKeyExpr, tempValExpr string

			if keyExpr == "_" || (!assignVars && !keyNeedsCopy) {
				tempKeyExpr = keyExpr
			} else {
				tempKeyExpr = v.getTempVarName("i")
				decl := ""

				if !v.options.preferVarDecl {
					keyType = v.getCSharpTypeName(v.getExprType(rangeStmt.Key)) + " "
				} else if !assignVars {
					decl = "var "
				}

				innerPrefix += fmt.Sprintf("%s%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), decl, keyExpr, tempKeyExpr)
			}

			if valExpr == "_" || (!assignVars && !valNeedsCopy) {
				tempValExpr = valExpr
			} else {
				tempValExpr = v.getTempVarName("r")
				decl := ""

				if !v.options.preferVarDecl {
					valType = v.getCSharpTypeName(v.getExprType(rangeStmt.Value)) + " "
				} else if !assignVars {
					decl = "var "
				}

				innerPrefix += fmt.Sprintf("%s%s%s%s = %s;", v.newline, v.indent(v.indentLevel+1), decl, valExpr, tempValExpr)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr, ptrDeref)
		}
	} else if isChan {
		if v.options.preferVarDecl {
			keyType = "var "
		}

		v.writeOutput("foreach (%s%s in %s%s)", keyType, scalarKeyExpr, rangeExpr, ptrDeref)
	} else if isInt {
		// A non-`int` integer width binds golib's GENERIC range<T>, since Go gives the iteration
		// variable the range operand's OWN type. T is named EXPLICITLY rather than inferred: Go's
		// `rune` and C#'s `int` are the same CLR type, so inference could not tell
		// `for i := range r` (yields rune) from `for i := range 3` (yields Go's int), and a NAMED
		// integer type (a converted `[GoType("num:…")]` struct) satisfies no generic-math interface
		// at all, so its operand is also cast down to the underlying width. `int` itself keeps the
		// bare `range(expr)` form — the overwhelmingly common case, emitted byte-identically.
		rangeTypeArg := ""

		if untypedInt {
			rangeExpr = fmt.Sprintf("@int(%s%s)", rangeExpr, ptrDeref)
			ptrDeref = ""
		} else if intBasic != nil && intBasic.Kind() != types.Int {
			csIntType := v.getCSharpTypeName(types.Default(intBasic))
			rangeTypeArg = fmt.Sprintf("<%s>", csIntType)

			if rangeIsNamedType {
				rangeExpr = fmt.Sprintf("(%s)(%s%s)", csIntType, rangeExpr, ptrDeref)
				ptrDeref = ""
			}
		}

		if v.options.preferVarDecl {
			keyType = "var "
		}

		v.writeOutput("foreach (%s%s in range%s(%s%s))", keyType, scalarKeyExpr, rangeTypeArg, rangeExpr, ptrDeref)
	} else if yieldFunc > -1 {
		// A NAMED func type renders as a C# DELEGATE; golib's range() overloads take
		// Action<Func<…>>, and a distinct delegate type has no conversion — but its method
		// GROUP does: `range(seq.Invoke)`.
		invokeSuffix := ""
		rangeTypeArgs := ""

		if rangeIsNamedType {
			// The method-group conversion binds golib's Action<Func<…>> overloads, but C#
			// cannot infer T from a method group's PARAMETERS — spell the type arguments out
			// from the yield signature: `range<nint>(countdown(5).Invoke)`.
			invokeSuffix = ".Invoke"

			if sig, ok := rangeType.(*types.Signature); ok && sig.Params().Len() == 1 {
				if yieldSig, ok := sig.Params().At(0).Type().Underlying().(*types.Signature); ok && yieldSig.Params().Len() > 0 {
					var elems []string

					for i := range yieldSig.Params().Len() {
						elems = append(elems, v.getCSharpTypeName(yieldSig.Params().At(i).Type()))
					}

					rangeTypeArgs = fmt.Sprintf("<%s>", strings.Join(elems, ", "))
				}
			}
		}

		if yieldFunc == 0 {
			if v.options.preferVarDecl {
				keyType = "var "
			}

			v.writeOutput("foreach (object %s in range%s(%s%s%s))", scalarKeyExpr, rangeTypeArgs, rangeExpr, ptrDeref, invokeSuffix)
		} else if yieldFunc == 1 {
			if v.options.preferVarDecl {
				keyType = "var "
			}

			v.writeOutput("foreach (%s%s in range%s(%s%s%s))", keyType, scalarKeyExpr, rangeTypeArgs, rangeExpr, ptrDeref, invokeSuffix)
		} else {
			v.writeOutput("foreach (%s(%s%s, %s%s) in range%s(%s%s%s))", varInit, keyType, keyExpr, valType, valExpr, rangeTypeArgs, rangeExpr, ptrDeref, invokeSuffix)
		}
	} else {
		// Handle slice, array, and map types
		if assignVars {
			var innerPrefix, tempKeyExpr, tempValExpr string

			if keyExpr == "_" {
				tempKeyExpr = "_"
			} else {
				var keyName string

				if isMap {
					keyName = "k"
				} else {
					keyName = "i"
				}

				tempKeyExpr = v.getTempVarName(keyName)

				if !v.options.preferVarDecl {
					keyType = v.getCSharpTypeName(v.getExprType(rangeStmt.Key)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s%s;", v.newline, v.indent(v.indentLevel+1), keyExpr, tempKeyExpr, keyCloneSuffix)
			}

			if valExpr == "_" {
				tempValExpr = "_"
			} else {
				tempValExpr = v.getTempVarName("v")

				if !v.options.preferVarDecl {
					valType = v.getCSharpTypeName(v.getExprType(rangeStmt.Value)) + " "
				}

				innerPrefix += fmt.Sprintf("%s%s%s = %s%s;", v.newline, v.indent(v.indentLevel+1), valExpr, tempValExpr, valCloneSuffix)
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else if keyHeapDecl != "" || valHeapDecl != "" {
			// A heap-boxed range var: iterate a temp and, per iteration, allocate a fresh box and
			// copy the temp into it (Go 1.22 per-iteration semantics — a stored `&i` must point to a
			// distinct box each pass). A non-heap-boxed companion var declares directly as before.
			var innerPrefix, kExpr, vExpr string
			bodyIndent := v.indent(v.indentLevel + 1)

			if keyExpr == "_" {
				kExpr = "_"
			} else if keyHeapDecl != "" {
				name := "i"
				if isMap {
					name = "k"
				}
				kExpr = v.getTempVarName(name)
				innerPrefix += fmt.Sprintf("%s%s%s%s%s = %s%s;", v.newline, bodyIndent, keyHeapDecl, v.newline+bodyIndent, keyExpr, kExpr, keyCloneSuffix)
			} else if keyNeedsCopy {
				// The OTHER variable's heap decl routed the loop into this branch; a non-boxed
				// key that still needs a mutable/cloned copy takes the same iterate-a-temp
				// escape the dedicated branch below applies.
				name := "i"
				if isMap {
					name = "k"
				}
				kExpr = v.getTempVarName(name)
				decl := "var "

				if !v.options.preferVarDecl {
					keyType = v.getCSharpTypeName(v.getExprType(rangeStmt.Key)) + " "
					decl = ""
				}

				innerPrefix += fmt.Sprintf("%s%s%s%s = %s%s;", v.newline, bodyIndent, decl, keyExpr, kExpr, keyCloneSuffix)
			} else {
				kExpr = keyExpr
			}

			if valExpr == "_" || valExpr == "" {
				vExpr = "_"
			} else if valHeapDecl != "" {
				vExpr = v.getTempVarName("v")
				innerPrefix += fmt.Sprintf("%s%s%s%s%s = %s%s;", v.newline, bodyIndent, valHeapDecl, v.newline+bodyIndent, valExpr, vExpr, valCloneSuffix)
			} else if valNeedsCopy {
				// Same escape as the key arm above, for the value variable.
				vExpr = v.getTempVarName("v")
				decl := "var "

				if !v.options.preferVarDecl {
					valType = v.getCSharpTypeName(v.getExprType(rangeStmt.Value)) + " "
					decl = ""
				}

				innerPrefix += fmt.Sprintf("%s%s%s%s = %s%s;", v.newline, bodyIndent, decl, valExpr, vExpr, valCloneSuffix)
			} else {
				vExpr = valExpr
			}

			v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, kExpr, valType, vExpr, rangeExpr, ptrDeref)

			if innerPrefix != "" {
				context.innerPrefix = innerPrefix + v.newline
			}
		} else {
			// A newly-DEFINED slice/array/map range var reassigned in the body must become a
			// mutable local: a C# foreach iteration variable is read-only (CS1656 — strconv
			// Atoi's `ch -= '0'`). Iterate a temp and declare the var from it in the body
			// (`foreach (var (_, vᴛ1) in s) { var ch = vᴛ1; … }`) — mirrors the string arm.

			if keyNeedsCopy || valNeedsCopy {
				var innerPrefix, tempKeyExpr, tempValExpr string
				bodyIndent := v.indent(v.indentLevel + 1)

				if keyExpr == "_" || !keyNeedsCopy {
					tempKeyExpr = keyExpr
				} else {
					name := "i"

					if isMap {
						name = "k"
					}

					tempKeyExpr = v.getTempVarName(name)
					decl := "var "

					if !v.options.preferVarDecl {
						keyType = v.getCSharpTypeName(v.getExprType(rangeStmt.Key)) + " "
						decl = ""
					}

					innerPrefix += fmt.Sprintf("%s%s%s%s = %s%s;", v.newline, bodyIndent, decl, keyExpr, tempKeyExpr, keyCloneSuffix)
				}

				if valExpr == "_" || valExpr == "" || !valNeedsCopy {
					tempValExpr = valExpr
				} else {
					tempValExpr = v.getTempVarName("v")
					decl := "var "

					if !v.options.preferVarDecl {
						valType = v.getCSharpTypeName(v.getExprType(rangeStmt.Value)) + " "
						decl = ""
					}

					innerPrefix += fmt.Sprintf("%s%s%s%s = %s%s;", v.newline, bodyIndent, decl, valExpr, tempValExpr, valCloneSuffix)
				}

				v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, tempKeyExpr, valType, tempValExpr, rangeExpr, ptrDeref)

				if innerPrefix != "" {
					context.innerPrefix = innerPrefix + v.newline
				}
			} else {
				v.writeOutput("foreach (%s(%s%s, %s%s) in %s%s)", varInit, keyType, keyExpr, valType, valExpr, rangeExpr, ptrDeref)
			}
		}
	}

	// Option to use for loop instead of foreach
	//  else {
	// 	if keyExpr == "_" && !assignVars {
	// 		v.writeOutput("foreach (var %s in %s)", valExpr, rangeExpr)
	// 	} else {
	// 		if assignVars {
	// 			if keyExpr == "_" {
	// 				keyExpr = v.getTempVarName("i")
	// 				v.writeOutput("for (var %s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)
	// 			} else {
	// 				v.writeOutput("for (%s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)
	// 			}

	// 			if valExpr != "_" {
	// 				context.innerPrefix = fmt.Sprintf("%s%s%s = %s[%s];%s", v.newline, v.indent(v.indentLevel+1), valExpr, rangeExpr, keyExpr, v.newline)
	// 			}
	// 		} else {
	// 			v.writeOutput("for (var %s = 0; %s < len(%s); %s++)", keyExpr, keyExpr, rangeExpr, keyExpr)

	// 			if valExpr != "_" {
	// 				context.innerPrefix = fmt.Sprintf("%s%svar %s = %s[%s];%s", v.newline, v.indent(v.indentLevel+1), valExpr, rangeExpr, keyExpr, v.newline)
	// 			}
	// 		}
	// 	}
	// }

	// A labeled range loop carries the same `continue_<label>`/`break_<label>` targets a labeled
	// `for` does: a Go `continue L`/`break L` from a nested loop emits `goto continue_L`/`goto break_L`,
	// so the labels must exist — `continue_L:` at the end of the body, `break_L:` after the loop
	// (otherwise CS0159 "no such label"). Mirrors visitForStmt.
	if len(target.label) > 0 {
		context.innerSuffix = fmt.Sprintf("%s%s:;", v.newline, getContinueLabelName(target.label))
		context.outerSuffix = fmt.Sprintf("%s%s:;", v.newline, getBreakLabelName(target.label))
	}

	// The wrapped-continue label slot — a `do { … } while (false)` switch-break wrapper in the
	// body retargets a bare C# `continue`, so such a continue emits `goto` to a label at the end
	// of the foreach body instead; resolved to the label or dropped after the body emits
	// (see prepareLoopContinueTarget, mirrors visitForStmt).
	continueTarget, continueLabelMarker := v.prepareLoopContinueTarget()
	context.innerSuffix += continueLabelMarker

	// Range variables are per-iteration natively (foreach), so no copy-backs — but the entry
	// keeps an unlabeled `continue` in the body bound to the innermost loop's copy-backs (an
	// enclosing transformed for loop's must not leak through; see forClausePerIterVars).
	v.loopCopyBackStack = append(v.loopCopyBackStack, nil)
	v.continueTargetStack = append(v.continueTargetStack, continueTarget)
	v.visitBlockStmt(rangeStmt.Body, context)
	v.continueTargetStack = v.continueTargetStack[:len(v.continueTargetStack)-1]
	v.loopCopyBackStack = v.loopCopyBackStack[:len(v.loopCopyBackStack)-1]
	v.finalizeLoopContinueTarget(continueTarget, continueLabelMarker)
}

func isYieldFunc(t types.Type) int {
	// First check if it's a function type. A NAMED func type — a defined `type Seq
	// func(yield func(V) bool)` or a generic instantiation like iter.Seq[E] — carries the
	// signature as its UNDERLYING (checking the bare *types.Signature missed every named
	// range-over-func, which then bound golib's numeric range overloads — CS1503/CS8130).
	sig, ok := t.Underlying().(*types.Signature)

	if !ok {
		return -1
	}

	// Check if it has exactly one parameter
	params := sig.Params()

	if params.Len() != 1 {
		return -1
	}

	// Get the parameter type which should be a function
	paramType := params.At(0).Type()
	funcType, ok := paramType.(*types.Signature)

	if !ok {
		return -1
	}

	// Check if return type is bool
	results := funcType.Results()

	if results.Len() != 1 || !types.Identical(results.At(0).Type(), types.Typ[types.Bool]) {
		return -1
	}

	// Now check the parameters of the inner function
	innerParams := funcType.Params()

	if innerParams.Len() < 3 {
		return innerParams.Len()
	}

	return -1
}
