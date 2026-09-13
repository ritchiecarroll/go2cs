// iifeOperations.go - Gbtc
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
	"go/types"
	"strings"
)

// funcBodyDeferRecover reports whether a function body (a func declaration or a func literal)
// directly uses defer and/or recover — i.e. a `defer` statement at this function's own level,
// or a `recover()` call that is effective for this function (called directly, or inside one of
// this function's deferred closures). Nested function literals have their OWN defer/recover
// scope and are not counted, except that a deferred closure's recover() recovers this function.
//
// This is the per-function view used to decide whether a function needs the `func((defer,
// recover) => …)` execution context, so an enclosing function is not wrapped merely because a
// nested IIFE/closure uses defer/recover.
func (v *Visitor) funcBodyDeferRecover(body *ast.BlockStmt) (hasDefer bool, hasRecover bool) {
	if body == nil {
		return false, false
	}

	// Declared and assigned in one step: ast.Inspect drives the descent, so this closure never
	// needs to name itself and does not require the two-step form a self-recursive one would.
	walk := func(n ast.Node) bool {
		// Both answers are already known — stop walking.
		if hasDefer && hasRecover {
			return false
		}

		switch node := n.(type) {
		case *ast.FuncLit:
			// A nested function literal owns its own defer/recover scope.
			return false
		case *ast.DeferStmt:
			hasDefer = true

			// A recover() inside the deferred call recovers THIS function.
			if v.exprContainsRecover(node.Call) {
				hasRecover = true
			}

			return false
		case *ast.CallExpr:
			if v.isRecoverCall(node) {
				hasRecover = true
			}
		}

		return true
	}

	ast.Inspect(body, walk)

	return hasDefer, hasRecover
}

// exprContainsRecover reports whether the expression contains a recover() call. It descends
// into function literals (a deferred closure — `defer func(){ recover() }()` — places recover
// inside the literal), which is where recover almost always appears.
func (v *Visitor) exprContainsRecover(expr ast.Node) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if call, ok := n.(*ast.CallExpr); ok && v.isRecoverCall(call) {
			found = true
			return false
		}

		return true
	})

	return found
}

// isRecoverCall reports whether the call is the built-in recover().
func (v *Visitor) isRecoverCall(call *ast.CallExpr) bool {
	ident, ok := call.Fun.(*ast.Ident)

	if !ok || ident.Name != "recover" {
		return false
	}

	_, isBuiltin := v.info.Uses[ident].(*types.Builtin)

	return isBuiltin
}

// iifeParamNames renders an IIFE's parameter list as names only (the delegate-cast supplies the
// types): `()` for none, `name` for one, `(n1, n2, …)` for several. Names follow variable
// analysis (shadow-renames), so the body's references resolve.
func (v *Visitor) iifeParamNames(sig *types.Signature) string {
	params := sig.Params()

	if params.Len() == 0 {
		return "()"
	}

	names := make([]string, params.Len())

	for i := range params.Len() {
		names[i] = v.iifeParamName(params.At(i))
	}

	if params.Len() == 1 {
		return names[0]
	}

	return "(" + strings.Join(names, ", ") + ")"
}

// iifeParamName returns the C# identifier the body uses for a parameter (honoring shadow-renames
// recorded by variable analysis); a blank parameter stays `_`. A heap-boxed value parameter
// (see funcLitHeapBoxParamIdents) arrives under its incoming `ʗp` name — the literal's prologue
// re-declares the Go name as the boxed ref alias — so it emits that name here.
func (v *Visitor) iifeParamName(param *types.Var) string {
	if isDiscardedVar(param.Name()) {
		return "_"
	}

	name := param.Name()

	if adjusted, ok := v.varNames[param]; ok {
		name = adjusted
	}

	if v.funcLitHeapBoxParamNames.Contains(name) {
		return getHeapBoxLitParamName(name)
	}

	return getSanitizedIdentifier(name)
}

// iifeDelegateType builds the C# delegate type for casting an IIFE's lambda so it can be invoked
// directly: `Action`/`Action<…>` for a void literal, `Func<…, TResult>` for a value-returning
// one (a tuple result type for multiple returns). Because getCSharpTypeName routes every anonymous
// (and collapsed methodless named) func type here, this is THE lowering of a Go func type used
// as a value.
//
// A VARIADIC signature lowers to the golib Actionꓸꓸꓸ/Funcꓸꓸꓸ family, whose trailing
// `params Span<T>` carries the variadic tail — the delegate's LAST type argument is the variadic
// ELEMENT type (the signature stores []T; the Span shell lives in the delegate declaration).
// This makes the three previously-incompatible lowerings agree: the named-function convention
// (`params ꓸꓸꓸT argsʗp`) and a variadic func literal convert to it natively (the parameter
// types match by identity), calls through the value pass loose Go-style arguments or an empty
// tail via C# params expansion, and a spread slice binds its `.ꓸꓸꓸ` Span in normal form —
// go/types' `reportf Action<@string, slice<any>>` took a `params ꓸꓸꓸany` literal (CS1661/CS1678)
// and loose-arg calls (CS1503/CS7036) before this.
// variadicFuncLitCallee returns the signature of a call whose callee is a func LITERAL with a
// VARIADIC signature and at least one parameter, and nil for every other call. That one shape has no
// delegate conversion in C#: a non-variadic literal emits explicitly-typed parameters
// (`(@string p, @string q) => …`) which convert to `Action<T1, T2>` and infer fine, while a variadic
// one emits `params ꓸꓸꓸ@string` and converts to nothing — so it can neither be invoked directly
// (CS0149) nor infer the type arguments of golib's arity-N `defer`/`go` overloads (CS0411,
// html/template's examplefiles_test.go:90, the only site in the Go 1.23 tree). Callers pair it with
// iifeDelegateType, which already renders the matching `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` family type.
func (v *Visitor) variadicFuncLitCallee(callExpr *ast.CallExpr) *types.Signature {
	// Unparenthesized, for the same reason convCallExpr's IIFE interception is: Go's idiomatic
	// `(func(…){ … })(…)` puts an *ast.ParenExpr in Fun, and the two spellings are one program.
	funcLit, ok := ast.Unparen(callExpr.Fun).(*ast.FuncLit)

	if !ok {
		return nil
	}

	sig, ok := v.info.TypeOf(funcLit).(*types.Signature)

	if !ok || !sig.Variadic() || sig.Params().Len() == 0 {
		return nil
	}

	return sig
}

func (v *Visitor) iifeDelegateType(sig *types.Signature) string {
	params := sig.Params()
	results := sig.Results()

	variadic := sig.Variadic() && params.Len() > 0

	typeArgs := make([]string, 0, params.Len()+1)

	for i := range params.Len() {
		paramType := params.At(i).Type()

		if variadic && i == params.Len()-1 {
			if sliceType, ok := paramType.Underlying().(*types.Slice); ok {
				paramType = sliceType.Elem()
			} else {
				// Defensive: a variadic signature's last param is always []T; if not, keep
				// the plain Action/Func lowering rather than a mis-shaped family reference.
				variadic = false
			}
		}

		typeArgs = append(typeArgs, v.aliasedElementTypeName(paramType))
	}

	family := ""

	if variadic {
		family = EllipsisOperator
	}

	if results.Len() == 0 {
		if len(typeArgs) == 0 {
			return "Action"
		}

		return "Action" + family + "<" + strings.Join(typeArgs, ", ") + ">"
	}

	var resultType string

	if results.Len() == 1 {
		resultType = v.aliasedElementTypeName(results.At(0).Type())
	} else {
		resultTypes := make([]string, results.Len())

		for i := range results.Len() {
			resultTypes[i] = v.aliasedElementTypeName(results.At(i).Type())
		}

		resultType = "(" + strings.Join(resultTypes, ", ") + ")"
	}

	typeArgs = append(typeArgs, resultType)

	return "Func" + family + "<" + strings.Join(typeArgs, ", ") + ">"
}

// signatureTypeName renders a func type structurally in GO syntax — `func(name type, …)
// results` — with every parameter/result type resolved recursively through getAliasQualifiedTypeName, so a
// cross-package element carries the short import-alias qualification the surrounding file uses
// (`types.Package`, `tls.Conn`), exactly like the map/slice/chan structural renders. The
// t.String() fall-through this replaces embeds slash-qualified import paths
// (`func(imports map[string]*go/types.Package, …)`), and convertToCSFullTypeName's slash
// heuristics then mangle them one of three ways depending on the string's shape: a naive
// whole-string path conversion (`ж<go.types.Package>`, `ж<crypto.tls.Conn>` — no `_package`
// class, and under a `go.go`-nested namespace the leading segment binds the wrong namespace),
// or an unrooted class-suffixed form (`@internal.trace_package.UtilFlags`) — CS0234 across
// go/importer, net/http's TLSNextProto maps, and traceviewer (one root, three variants).
// Same-package and builtin elements render byte-identically to the old path (t.String()'s
// package-path prefix strip), so simple signatures see no churn. A VARIADIC tail renders as
// `...elem` (which t.String()'s `..`-strip used to reduce to the unparseable `.elem`); the
// string-path func-type conversion lowers it to the golib Actionꓸꓸꓸ/Funcꓸꓸꓸ delegate family,
// mirroring iifeDelegateType. Parameter and result NAMES are preserved so a named multi-result
// signature keeps its named C# tuple (see convertToCSResultList).
func (v *Visitor) signatureTypeName(sig *types.Signature, isUnderlying bool) string {
	result := strings.Builder{}
	result.WriteString("func(")

	params := sig.Params()

	for i := range params.Len() {
		if i > 0 {
			result.WriteString(", ")
		}

		param := params.At(i)

		if name := param.Name(); name != "" {
			result.WriteString(name)
			result.WriteByte(' ')
		}

		if sig.Variadic() && i == params.Len()-1 {
			if sliceType, ok := param.Type().Underlying().(*types.Slice); ok {
				result.WriteString("...")
				result.WriteString(v.getAliasQualifiedTypeName(sliceType.Elem(), isUnderlying))
				continue
			}
		}

		result.WriteString(v.getAliasQualifiedTypeName(param.Type(), isUnderlying))
	}

	result.WriteByte(')')

	results := sig.Results()

	if results.Len() == 1 && results.At(0).Name() == "" {
		result.WriteByte(' ')
		result.WriteString(v.getAliasQualifiedTypeName(results.At(0).Type(), isUnderlying))
	} else if results.Len() > 0 {
		result.WriteString(" (")

		for i := range results.Len() {
			if i > 0 {
				result.WriteString(", ")
			}

			resultVar := results.At(i)

			if name := resultVar.Name(); name != "" {
				result.WriteString(name)
				result.WriteByte(' ')
			}

			result.WriteString(v.getAliasQualifiedTypeName(resultVar.Type(), isUnderlying))
		}

		result.WriteByte(')')
	}

	return result.String()
}

// namedResultName returns the C# identifier the body uses for a named result parameter
// (honoring shadow-renames recorded by variable analysis).
// aliasedElementTypeName renders a delegate element type, substituting the file-local
// ꓸ-alias form for a cross-package named type that is Δ-RENAMED inside its own package -
// `syscall.Handle` declares `ΔHandle`, so the raw qualified render (`Δsyscall.Handle`)
// names a type that does not exist (CS0426 x26, internal/poll's hook_windows func-typed
// globals). The imported-type-alias registry already records the foreign rename
// (`global using syscallꓸHandle = go.syscall_package.ΔHandle`); a type without a
// registered alias, a generic instantiation, and every non-named element keep the plain
// getCSharpTypeName render (no churn). Pointer elements recurse so `*syscall.Overlapped`
// becomes `ж<syscallꓸOverlapped>`.
func (v *Visitor) aliasedElementTypeName(t types.Type) string {
	if ptr, ok := types.Unalias(t).(*types.Pointer); ok {
		return fmt.Sprintf("%s<%s>", PointerPrefix, v.aliasedElementTypeName(ptr.Elem()))
	}

	if named, ok := types.Unalias(t).(*types.Named); ok {
		if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg && named.TypeArgs().Len() == 0 {
			plainKey := fmt.Sprintf("%s.%s", getSanitizedIdentifier(pkg.Name()), getCoreSanitizedIdentifier(named.Obj().Name()))

			packageLock.Lock()
			_, exists := importedTypeAliases[plainKey]
			packageLock.Unlock()

			// A foreign type reached ONLY through ANOTHER package's signature has no alias
			// registered: the preload covers packages the current package imports DIRECTLY,
			// but go/types renders go/ast's `FieldFilter` — whose `reflect.Value` parameter
			// type go/types itself never imports — so the rename (reflect declares ΔValue)
			// was missed and the raw `reflect.Value` resolved inside reflect_package
			// (CS0426, plus CS0123 on the mismatched delegate). Load the owning package's
			// exported aliases on demand — the dedup in loadImportedTypeAliases makes this
			// a one-time no-op per package — and re-check; the resolving `global using`
			// then rides the normal package_info emission (assembly references are
			// transitive, so the consumer sees the type through its importer's reference).
			if !exists {
				v.loadImportedTypeAliases(pkg.Path())

				packageLock.Lock()
				_, exists = importedTypeAliases[plainKey]
				packageLock.Unlock()
			}

			if exists {
				return getAliasedTypeName(plainKey)
			}
		}
	}

	return v.getCSharpTypeName(t)
}

func (v *Visitor) namedResultName(param *types.Var) string {
	// A BLANK result gets a generated slot name: `_` is C#'s DISCARD, so declaring it would make
	// every later `_ = expr` in the same scope bind to the declaration, and two blank results
	// would collide outright. The name is interned per result object so the declaration, the
	// return-statement assignment and the post-defer read all agree (see blankResultNames).
	if isDiscardedVar(param.Name()) {
		if name, ok := v.blankResultNames[param]; ok {
			return name
		}

		if v.blankResultNames == nil {
			v.blankResultNames = map[*types.Var]string{}
		}

		name := v.getTempVarName("_")
		v.blankResultNames[param] = name

		return name
	}

	if ident := v.getVarIdent(param); ident != nil {
		return getSanitizedIdentifier(v.getIdentName(ident))
	}

	return getSanitizedIdentifier(param.Name())
}

// detectNamedReturnDefer reports whether a function with the given signature needs the
// named-return-defer handling: it uses defer/recover AND at least one of its results is named.
// When so, it returns the result identifiers in order (used to declare them outside the func()
// wrapper and to return them after it runs, so deferred code — including recover — can mutate
// them).
//
// Go permits MIXING blank and named results (`func parse(s string, flags Flags) (_ *Regexp, err
// error)`); such a signature still needs the handling, because deferred code can mutate `err`.
// A blank slot cannot be named by the body — only a return statement writes it — but it must
// still be declared, so namedResultName mints it a generated name. Rejecting the whole signature
// on the first blank silently dropped the mechanism: regexp/syntax's `parse` recovered ErrLarge
// into `err` and then returned the ZERO tuple from the wrapper, so every over-large expression
// came back as (nil, nil) — reported as a successful parse of a regexp that must not compile.
//
// A result list that is entirely UNNAMED (or entirely blank) keeps the plain wrapper: there is
// nothing deferred code could mutate, and Go likewise returns the zero results after a recover.
func (v *Visitor) detectNamedReturnDefer(sig *types.Signature, hasDefer, hasRecover bool) (bool, []string) {
	if !(hasDefer || hasRecover) || sig == nil || sig.Results() == nil {
		return false, nil
	}

	results := sig.Results()

	if results.Len() == 0 {
		return false, nil
	}

	names := make([]string, 0, results.Len())
	hasNamedResult := false

	for i := range results.Len() {
		param := results.At(i)

		// A truly UNNAMED result (`func f() (int, error)`) cannot be mixed with named ones in
		// Go, so seeing one settles the whole signature.
		if param.Name() == "" {
			return false, nil
		}

		if !isDiscardedVar(param.Name()) {
			hasNamedResult = true
		}

		names = append(names, v.namedResultName(param))
	}

	if !hasNamedResult {
		return false, nil
	}

	return true, names
}

// namedResultHeapBoxIdent returns the declaring ident of a heap-box-backed named result — one
// the capture/escape analyses route to shared storage (see identHasHeapBox), whose render sites
// reference the box (`Ꮡerr`) — or nil for a plain named result. The plain `T name = default!;`
// declaration leaves such a box undeclared (CS0103 — internal/poll SendFile's deferred
// `TestHookDidSendFile(fd, 0, written, err, …)` reads `Ꮡerr.ValueSlot` after `curpos, err :=`
// escape-marked the named result).
func (v *Visitor) namedResultHeapBoxIdent(param *types.Var) *ast.Ident {
	if param == nil || isDiscardedVar(param.Name()) || !v.identHasHeapBox(param, param.Type()) {
		return nil
	}

	return v.getVarIdent(param)
}

// namedResultBoxAccessor returns the box-read accessor for a named result's type: reading the
// box of an inherently heap-allocated value (a `ж<ж<T>>`/`ж<error>`… holding the reference
// itself) is not a dereference, so it takes the nil-check-free `.ValueSlot`; a value-type box
// is a genuine dereference and keeps the strict `.Value` (mirrors isBoxedPointerLocal).
func namedResultBoxAccessor(t types.Type) string {
	if isInherentlyHeapAllocatedType(t) {
		return ".ValueSlot"
	}

	return ".Value"
}

// namedResultBoxAliasLines renders the in-wrapper `ref var name = ref Ꮡname.Value;` re-alias for
// each heap-box-backed named result. In namedReturnDeferMode the box is declared OUTSIDE the
// func() wrapper (so the final post-defer return can read it) and a lambda cannot capture a ref
// local (CS8175), so the wrapper re-derives the value alias inside — the same treatment deref'd
// pointer parameters get (`ref var fd = ref Ꮡfd.Value;`).
func (v *Visitor) namedResultBoxAliasLines(sig *types.Signature, indentLevel int) string {
	results := sig.Results()
	aliases := strings.Builder{}

	for i := range results.Len() {
		param := results.At(i)

		if ident := v.namedResultHeapBoxIdent(param); ident != nil {
			aliases.WriteString(fmt.Sprintf("%s%sref var %s = ref %s%s%s;", v.newline, v.indent(indentLevel),
				getSanitizedIdentifier(v.getIdentName(ident)), AddressPrefix, v.boxBaseName(ident), namedResultBoxAccessor(param.Type())))
		}
	}

	return aliases.String()
}

// namedReturnBoxReadNames maps named-result return names to their box-read form (`Ꮡerr.ValueSlot`)
// for heap-box-backed results — used where the plain value alias is not in scope: the final
// post-wrapper `return <named>;` of namedReturnDeferMode (the alias lives inside the wrapper).
func (v *Visitor) namedReturnBoxReadNames(sig *types.Signature, names []string) []string {
	results := sig.Results()

	if results == nil || results.Len() != len(names) {
		return names
	}

	mapped := make([]string, len(names))
	copy(mapped, names)

	for i := range results.Len() {
		param := results.At(i)

		if ident := v.namedResultHeapBoxIdent(param); ident != nil {
			mapped[i] = AddressPrefix + v.boxBaseName(ident) + namedResultBoxAccessor(param.Type())
		}
	}

	return mapped
}

// namedReturnAssignTargets returns the LHS names for an explicit namedReturnDefer return's
// `(named) = (results);` rewrite. Inside a function LITERAL's conversion a box-ref named result
// is referenced through its box everywhere (see convIdent's box-ref render — the ref alias the
// enclosing-FUNCTION form re-declares in its wrapper does not exist here), so the assignment
// target reads the box too. The enclosing FUNCTION's wrapper body keeps plain targets (its
// in-wrapper alias is in scope; see visitFuncDecl).
func (v *Visitor) namedReturnAssignTargets(signature *types.Signature) []string {
	names := v.namedReturnNames

	if signature == nil || v.lambdaCapture == nil || !v.lambdaCapture.conversionInLambda {
		return names
	}

	results := signature.Results()

	if results == nil || results.Len() != len(names) {
		return names
	}

	mapped := make([]string, len(names))
	copy(mapped, names)

	for i := range results.Len() {
		param := results.At(i)

		if !v.isLambdaBoxRefVar(param) {
			continue
		}

		if ident := v.getVarIdent(param); ident != nil {
			mapped[i] = AddressPrefix + v.boxBaseName(ident) + namedResultBoxAccessor(param.Type())
		}
	}

	return mapped
}

// namedReturnDeclLines renders the `Type name = default!;` declarations for a signature's named
// results, each on its own line at the given indent level. A heap-box-backed result (see
// namedResultHeapBoxIdent) declares its box instead: when the decls sit OUTSIDE a func() wrapper
// (declsOutsideWrapper — the litNamedDefer form), only the box is created here (`heap<T>(out var
// Ꮡname);`) because the wrapper lambda cannot capture the ref-local alias (CS8175); otherwise the
// full boxed-alias form is used, matching an escaping local's declaration.
//
// livenessBody is the literal's own body when a declaration the body never touches may be dropped,
// and nil when it may not — the same opt-out the function path uses (see
// namedResultNeedsDeclaration).
func (v *Visitor) namedReturnDeclLines(sig *types.Signature, indentLevel int, declsOutsideWrapper bool, livenessBody *ast.BlockStmt) string {
	results := sig.Results()
	decls := strings.Builder{}

	for i := range results.Len() {
		param := results.At(i)

		if !v.namedResultNeedsDeclaration(livenessBody, param) {
			continue
		}

		if ident := v.namedResultHeapBoxIdent(param); ident != nil {
			if declsOutsideWrapper {
				decls.WriteString(fmt.Sprintf("%s%sheap<%s>(out var %s%s);", v.newline, v.indent(indentLevel),
					v.getCSharpTypeName(param.Type()), AddressPrefix, v.boxBaseName(ident)))
			} else {
				decls.WriteString(fmt.Sprintf("%s%s%s", v.newline, v.indent(indentLevel), v.convertToHeapTypeDecl(ident, true)))
			}

			continue
		}

		// A result whose Go zero value is not all-bits-zero — a fixed-size array, a promoted-embed
		// struct, a struct carrying a fixed array at any depth — must construct rather than take
		// `default!` (see zeroValueInitializer).
		decls.WriteString(fmt.Sprintf("%s%s%s %s = %s;", v.newline, v.indent(indentLevel), v.getCSharpTypeName(param.Type()), v.namedResultName(param), v.zeroValueInitializer(param.Type())))
	}

	return decls.String()
}
