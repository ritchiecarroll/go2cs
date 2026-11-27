// foreignUnexportedPromotionRefusal.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The refusal BY NAME of a conversion to an interface whose UNEXPORTED method the value satisfies only through a
// CONCRETE method promoted from another package, where the converted adapter would call the wrong method or return a
// silent zero, checked before any file is written (GO2CS2016).
//
// Go qualifies an unexported method name by its package. `type U struct{ lib.T }` with `func (U) m()` declares two
// methods m: lib.T's (lib.m), promoted, and U's own (main.m). An interface of lib's that declares m is satisfied by
// lib.m, so `lib.CallM(u)` calls lib.m. go2cs-gen's adapter maps an interface member by its bare name, so the same
// call reaches main.m: the converted program prints main.m where Go prints lib.m. Without an own m the adapter cannot
// call lib.T's m, which is internal to lib's assembly, and stubs it: a no-op for a method with no results, and a zero
// value for a method that returns one.
//
// So the gate refuses exactly two cases: (a) the source type has a method of that bare name of its own (the wrong
// method), and (b) the unexported method returns a value (the stub's silent zero). A method with no results and no
// own-named rival converts as before, because its stub is a no-op: that is the sealed-marker idiom every generated
// gRPC server uses (`type server struct{ pb.UnimplementedGreeterServer }`, whose mustEmbedUnimplementedGreeterServer()
// is promoted from pb and does nothing). What that leaves silent is a void marker whose Go body has an effect
// (docs/KnownIssues.md names it).
//
// Promotion through an embedded INTERFACE field is not refused: the adapter forwards to the field's interface member,
// which is public in C# and dispatches to the dynamic value's own method, the method Go calls
// (crypto/internal/rand's reader embeds drbg.DefaultReader and is converted to it, which must keep converting). The
// exception is a path through a field the converting package cannot reach, an unexported field of another package
// (`type T struct{ impl }` in lib, converted from main): the forward would not compile, so that is refused too.
// Promotion from a type of the SAME package is not refused either: both sides are in one assembly, and Go's name
// qualification agrees with the bare name there.
//
// The gate examines the conversions the source writes: assignment (a redeclared variable of := included), a typed var,
// a call argument, a return value, each of them from a multi-value call too, a composite literal element, a channel
// send, an explicit conversion, an equality comparison, a switch case against its tag, a map index or delete key, a
// range assignment, and a type argument checked against its constraint. A type assertion asks at run time and records
// no adapter, so it is outside the gate (docs/KnownIssues.md names that case).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"sort"

	"golang.org/x/tools/go/packages"
)

// foreignUnexportedPromotionSite is one refused conversion.
type foreignUnexportedPromotionSite struct {
	position token.Position
	text     string
}

// foreignUnexportedPromotionRefusal is the error refuseForeignUnexportedPromotions returns: one canonical diagnostic per
// refused site.
type foreignUnexportedPromotionRefusal struct {
	pkgPath string
	sites   []foreignUnexportedPromotionSite
}

func (refusal *foreignUnexportedPromotionRefusal) Error() string {
	return fmt.Sprintf("package %s converts a value to an interface whose unexported method it satisfies only through a method promoted from another package (%d site(s)); see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticForeignUnexportedPromotionRefused)
}

// report prints each refused site as a canonical diagnostic.
func (refusal *foreignUnexportedPromotionRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticForeignUnexportedPromotionRefused, site.text)
	}
}

// foreignUnexportedPromotionKey identifies one converted value: a multi-value call yields one value per result, all at
// the call's position.
type foreignUnexportedPromotionKey struct {
	pos   token.Pos
	index int
}

// refuseForeignUnexportedPromotions finds every refused conversion in pkg. It returns nil when there is none.
func refuseForeignUnexportedPromotions(pkg *packages.Package) *foreignUnexportedPromotionRefusal {
	if pkg.TypesInfo == nil || pkg.Types == nil {
		return nil
	}

	info := pkg.TypesInfo
	seen := map[foreignUnexportedPromotionKey]bool{}
	var sites []foreignUnexportedPromotionSite

	// checkType refuses converting a value of type source, at pos, to target.
	checkType := func(pos token.Pos, index int, source, target types.Type) {
		key := foreignUnexportedPromotionKey{pos, index}

		if source == nil || target == nil || seen[key] {
			return
		}

		if _, isTypeParam := target.(*types.TypeParam); isTypeParam {
			// A value passed for a type parameter is not converted to an interface; its type argument is checked
			// against the constraint at the instantiation below.
			return
		}

		if text := foreignUnexportedPromotionText(source, target, pkg.Types, "is converted to"); text != "" {
			seen[key] = true
			sites = append(sites, foreignUnexportedPromotionSite{position: pkg.Fset.Position(pos), text: text})
		}
	}

	// values is the type of each value expr yields: one per result for a multi-value call, none for nil or a type.
	values := func(expr ast.Expr) []types.Type {
		tv, ok := info.Types[expr]

		if !ok || tv.Type == nil || tv.IsNil() || tv.IsType() {
			return nil
		}

		if tuple, ok := tv.Type.(*types.Tuple); ok {
			result := make([]types.Type, tuple.Len())

			for i := range result {
				result[i] = tuple.At(i).Type()
			}

			return result
		}

		return []types.Type{tv.Type}
	}

	check := func(expr ast.Expr, target types.Type) {
		if expr == nil {
			return
		}

		if sources := values(expr); len(sources) == 1 {
			checkType(expr.Pos(), 0, sources[0], target)
		}
	}

	// checkList pairs the values of exprs with targets, either one expression per target or one multi-value call
	// for all of them. A nil target is not a conversion.
	checkList := func(exprs []ast.Expr, targets []types.Type) {
		if len(exprs) == 1 && len(targets) > 1 {
			if sources := values(exprs[0]); len(sources) == len(targets) {
				for i, source := range sources {
					checkType(exprs[0].Pos(), i, source, targets[i])
				}
			}

			return
		}

		if len(exprs) == len(targets) {
			for i, expr := range exprs {
				check(expr, targets[i])
			}
		}
	}

	for _, file := range pkg.Syntax {
		var funcResults []*types.Tuple

		var visit func(node ast.Node) bool

		visit = func(node ast.Node) bool {
			switch n := node.(type) {
			case *ast.FuncDecl:
				if n.Body == nil {
					return false
				}

				if obj, ok := info.Defs[n.Name].(*types.Func); ok {
					funcResults = append(funcResults, obj.Signature().Results())
					ast.Inspect(n.Body, visit)
					funcResults = funcResults[:len(funcResults)-1]
				}

				return false
			case *ast.FuncLit:
				if signature, ok := info.TypeOf(n).(*types.Signature); ok {
					funcResults = append(funcResults, signature.Results())
					ast.Inspect(n.Body, visit)
					funcResults = funcResults[:len(funcResults)-1]
				}

				return false
			case *ast.ReturnStmt:
				if len(funcResults) == 0 {
					return true
				}

				if results := funcResults[len(funcResults)-1]; results != nil {
					checkList(n.Results, tupleTypes(results))
				}
			case *ast.AssignStmt:
				if n.Tok == token.ASSIGN || n.Tok == token.DEFINE {
					targets := make([]types.Type, len(n.Lhs))

					for i, lhs := range n.Lhs {
						// A variable := declares takes the value's own type; one it redeclares is assigned to.
						if ident, ok := lhs.(*ast.Ident); ok && (ident.Name == "_" || info.Defs[ident] != nil) {
							continue
						}

						targets[i] = info.TypeOf(lhs)
					}

					checkList(n.Rhs, targets)
				}
			case *ast.ValueSpec:
				if n.Type != nil && len(n.Values) > 0 {
					target := info.TypeOf(n.Type)
					targets := make([]types.Type, len(n.Names))

					for i := range targets {
						targets[i] = target
					}

					checkList(n.Values, targets)
				}
			case *ast.CallExpr:
				checkCallArguments(info, n, check, checkList)
			case *ast.CompositeLit:
				checkCompositeElements(info, n, check)
			case *ast.SendStmt:
				if channel, ok := underlyingOf(info.TypeOf(n.Chan)).(*types.Chan); ok {
					check(n.Value, channel.Elem())
				}
			case *ast.BinaryExpr:
				if n.Op == token.EQL || n.Op == token.NEQ {
					check(n.X, info.TypeOf(n.Y))
					check(n.Y, info.TypeOf(n.X))
				}
			case *ast.SwitchStmt:
				if n.Tag != nil && n.Body != nil {
					tagType := info.TypeOf(n.Tag)

					for _, clause := range n.Body.List {
						if caseClause, ok := clause.(*ast.CaseClause); ok {
							for _, expr := range caseClause.List {
								check(expr, tagType)
								check(n.Tag, info.TypeOf(expr))
							}
						}
					}
				}
			case *ast.IndexExpr:
				if mapType, ok := underlyingOf(info.TypeOf(n.X)).(*types.Map); ok {
					check(n.Index, mapType.Key())
				}
			case *ast.RangeStmt:
				if n.Tok == token.ASSIGN {
					key, value := rangeValueTypes(info.TypeOf(n.X))

					for _, pair := range []struct {
						expr   ast.Expr
						source types.Type
					}{{n.Key, key}, {n.Value, value}} {
						if pair.expr != nil {
							checkType(pair.expr.Pos(), 0, pair.source, info.TypeOf(pair.expr))
						}
					}
				}
			}

			return true
		}

		ast.Inspect(file, visit)
	}

	// A type argument meets its constraint by the same bare-name binding the adapter uses.
	for ident, instance := range info.Instances {
		var typeParams *types.TypeParamList

		switch obj := info.Uses[ident].(type) {
		case *types.Func:
			typeParams = obj.Signature().TypeParams()
		case *types.TypeName:
			if named, ok := types.Unalias(obj.Type()).(*types.Named); ok {
				typeParams = named.TypeParams()
			}
		}

		if typeParams == nil || instance.TypeArgs == nil {
			continue
		}

		for i := 0; i < typeParams.Len() && i < instance.TypeArgs.Len(); i++ {
			key := foreignUnexportedPromotionKey{ident.Pos(), i}

			if seen[key] {
				continue
			}

			if text := foreignUnexportedPromotionText(instance.TypeArgs.At(i), typeParams.At(i).Constraint(), pkg.Types, "is a type argument for a type parameter constrained by"); text != "" {
				seen[key] = true
				sites = append(sites, foreignUnexportedPromotionSite{position: pkg.Fset.Position(ident.Pos()), text: text})
			}
		}
	}

	if len(sites) == 0 {
		return nil
	}

	sort.SliceStable(sites, func(i, j int) bool {
		if sites[i].position.Filename != sites[j].position.Filename {
			return sites[i].position.Filename < sites[j].position.Filename
		}

		if sites[i].position.Offset != sites[j].position.Offset {
			return sites[i].position.Offset < sites[j].position.Offset
		}

		return sites[i].text < sites[j].text
	})

	return &foreignUnexportedPromotionRefusal{pkgPath: pkg.PkgPath, sites: sites}
}

// tupleTypes is the type of each member of tuple.
func tupleTypes(tuple *types.Tuple) []types.Type {
	result := make([]types.Type, tuple.Len())

	for i := range result {
		result[i] = tuple.At(i).Type()
	}

	return result
}

// rangeValueTypes is the type of the key and of the value a range over a value of type t yields, nil where it yields
// none.
func rangeValueTypes(t types.Type) (key, value types.Type) {
	switch u := underlyingOf(t).(type) {
	case *types.Basic:
		if u.Info()&types.IsString != 0 {
			return types.Typ[types.Int], types.Typ[types.Rune]
		}

		return u, nil
	case *types.Slice:
		return types.Typ[types.Int], u.Elem()
	case *types.Array:
		return types.Typ[types.Int], u.Elem()
	case *types.Pointer:
		if array, ok := underlyingOf(u.Elem()).(*types.Array); ok {
			return types.Typ[types.Int], array.Elem()
		}
	case *types.Map:
		return u.Key(), u.Elem()
	case *types.Chan:
		return u.Elem(), nil
	case *types.Signature:
		if u.Params().Len() == 1 {
			if yield, ok := underlyingOf(u.Params().At(0).Type()).(*types.Signature); ok {
				params := yield.Params()

				if params.Len() > 0 {
					key = params.At(0).Type()
				}

				if params.Len() > 1 {
					value = params.At(1).Type()
				}
			}
		}
	}

	return key, value
}

// checkCallArguments checks each argument of a call against its parameter (a multi-value call's results included),
// and an explicit conversion's operand against the converted-to type.
func checkCallArguments(info *types.Info, call *ast.CallExpr, check func(ast.Expr, types.Type), checkList func([]ast.Expr, []types.Type)) {
	funType, ok := info.Types[call.Fun]

	if !ok {
		return
	}

	if funType.IsType() {
		if len(call.Args) == 1 {
			check(call.Args[0], funType.Type)
		}

		return
	}

	// A builtin's recorded signature is the call site's own, so delete(m, k) and append(s, v) read like any call.
	signature, ok := underlyingOf(funType.Type).(*types.Signature)

	if !ok {
		return
	}

	params := signature.Params()
	count := len(call.Args)

	if count == 1 {
		if tuple, ok := info.TypeOf(call.Args[0]).(*types.Tuple); ok {
			count = tuple.Len()
		}
	}

	targets := make([]types.Type, count)

	for i := range targets {
		switch {
		case signature.Variadic() && i >= params.Len()-1:
			if call.Ellipsis.IsValid() {
				continue
			}

			if slice, ok := underlyingOf(params.At(params.Len() - 1).Type()).(*types.Slice); ok {
				targets[i] = slice.Elem()
			}
		case i < params.Len():
			targets[i] = params.At(i).Type()
		}
	}

	checkList(call.Args, targets)
}

// checkCompositeElements checks each element of a composite literal against its field, element or key type.
func checkCompositeElements(info *types.Info, literal *ast.CompositeLit, check func(ast.Expr, types.Type)) {
	literalType := info.TypeOf(literal)

	if literalType == nil {
		return
	}

	if pointer, ok := underlyingOf(literalType).(*types.Pointer); ok {
		literalType = pointer.Elem()
	}

	switch t := underlyingOf(literalType).(type) {
	case *types.Struct:
		for i, element := range literal.Elts {
			if keyValue, ok := element.(*ast.KeyValueExpr); ok {
				if key, ok := keyValue.Key.(*ast.Ident); ok {
					for f := 0; f < t.NumFields(); f++ {
						if t.Field(f).Name() == key.Name {
							check(keyValue.Value, t.Field(f).Type())
						}
					}
				}
			} else if i < t.NumFields() {
				check(element, t.Field(i).Type())
			}
		}
	case *types.Slice:
		checkElements(literal.Elts, nil, t.Elem(), check)
	case *types.Array:
		checkElements(literal.Elts, nil, t.Elem(), check)
	case *types.Map:
		checkElements(literal.Elts, t.Key(), t.Elem(), check)
	}
}

func checkElements(elements []ast.Expr, key, elem types.Type, check func(ast.Expr, types.Type)) {
	for _, element := range elements {
		if keyValue, ok := element.(*ast.KeyValueExpr); ok {
			if key != nil {
				check(keyValue.Key, key)
			}

			check(keyValue.Value, elem)
		} else {
			check(element, elem)
		}
	}
}

// foreignUnexportedPromotionText is the diagnostic text when using a value of type source as target is refused, or ""
// when it is not. target is an interface with an unexported method that source satisfies only through a CONCRETE method
// promoted from a package other than source's own, and either source has a method of that bare name of its own or the
// method returns a value. how says what the use is: "is converted to", or a type argument's.
func foreignUnexportedPromotionText(source, target types.Type, current *types.Package, how string) string {
	targetInterface, ok := underlyingOf(target).(*types.Interface)

	if !ok || types.IsInterface(source) {
		return ""
	}

	if _, isTypeParam := source.(*types.TypeParam); isTypeParam {
		return ""
	}

	ownPackage := current

	if named := namedOrPointerNamed(source); named != nil && named.Obj().Pkg() != nil {
		ownPackage = named.Obj().Pkg()
	}

	for i := 0; i < targetInterface.NumMethods(); i++ {
		method := targetInterface.Method(i)

		if method.Exported() || method.Pkg() == nil {
			continue
		}

		object, index, _ := types.LookupFieldOrMethod(source, false, method.Pkg(), method.Name())
		promoted, ok := object.(*types.Func)

		if !ok || len(index) < 2 || promoted.Pkg() == nil {
			continue
		}

		receiver := promoted.Signature().Recv()

		if receiver == nil {
			continue
		}

		sourceName := types.TypeString(source, types.RelativeTo(current))
		receiverName := types.TypeString(receiver.Type(), types.RelativeTo(current))

		lead := fmt.Sprintf("%s %s %s, whose unexported method %s it satisfies only through %s's %s, promoted from package %s",
			sourceName, how, types.TypeString(target, types.RelativeTo(current)), method.Name(), receiverName, method.Name(),
			promoted.Pkg().Path())

		if types.IsInterface(receiver.Type()) {
			// Promoted through an embedded interface field: the adapter forwards through the field path, which is
			// right only where it can reach every field on it.
			if field := unreachableEmbeddedField(source, index[:len(index)-1], current); field != nil {
				return fmt.Sprintf("%s. The adapter reaches that method through the embedded field %s, which is unexported in package %s and so cannot be reached from %s. go2cs refuses this conversion",
					lead, field.Name(), field.Pkg().Path(), current.Path())
			}

			continue
		}

		if promoted.Pkg() == ownPackage {
			// A concrete method from the source's own package: one assembly, where the bare name agrees with Go's.
			continue
		}

		// A rival: a method of the same bare name in source's own package, which the adapter would bind instead.
		rival, _, _ := types.LookupFieldOrMethod(source, true, ownPackage, method.Name())
		rivalFunc, hasRival := rival.(*types.Func)
		hasRival = hasRival && rivalFunc.Pkg() == ownPackage

		returnsValue := promoted.Signature().Results().Len() > 0

		if !hasRival && !returnsValue {
			// A void method with no rival: the adapter's stub is a no-op, the sealed-marker idiom.
			continue
		}

		if hasRival {
			return fmt.Sprintf("%s. %s declares a method %s of its own, which Go keeps apart from %s's; go2cs maps an interface method by its bare name, so the converted call would reach %s's %s where Go calls %s's. go2cs refuses this conversion",
				lead, sourceName, method.Name(), receiverName, sourceName, method.Name(), receiverName)
		}

		return fmt.Sprintf("%s. That method is internal to package %s, so the converted adapter cannot call it and would return a zero value where Go returns its result. go2cs refuses this conversion",
			lead, promoted.Pkg().Path())
	}

	return ""
}

// unreachableEmbeddedField is the first field on the embedding path index from source that is unexported and declared in
// a package other than current, or nil when the converting package can reach every field on it.
func unreachableEmbeddedField(source types.Type, index []int, current *types.Package) *types.Var {
	t := source

	for _, i := range index {
		if pointer, ok := underlyingOf(t).(*types.Pointer); ok {
			t = pointer.Elem()
		}

		structType, ok := underlyingOf(t).(*types.Struct)

		if !ok || i >= structType.NumFields() {
			return nil
		}

		field := structType.Field(i)

		if !field.Exported() && field.Pkg() != current {
			return field
		}

		t = field.Type()
	}

	return nil
}

// namedOrPointerNamed is t's named type, or the named type t points to, or nil.
func namedOrPointerNamed(t types.Type) *types.Named {
	t = types.Unalias(t)

	if pointer, ok := t.(*types.Pointer); ok {
		t = types.Unalias(pointer.Elem())
	}

	named, _ := t.(*types.Named)

	return named
}
