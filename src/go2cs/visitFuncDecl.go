// visitFuncDecl.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
	"unicode"
	"unicode/utf8"
)

const FunctionPrefixMarker = ">>MARKER:FUNC_%s_PREFIX<<"
const FunctionAccessMarker = ">>MARKER:FUNC_%s_ACCESS<<"
const FunctionUnsafeMarker = ">>MARKER:FUNC_%s_UNSAFE<<"
const FunctionPartialMarker = ">>MARKER:FUNC_%s_PARTIAL<<"
const FunctionAttributeMarker = ">>MARKER:FUNC_%s_RECEIVER<<"
const FunctionParametersMarker = ">>MARKER:FUNC_%s_PARAMETERS<<"
const FunctionExecContextMarker = ">>MARKER:FUNC_%s_EXEC_CONTEXT<<"
const FunctionBlockPrefixMarker = ">>MARKER:FUNC_%s_BLOCK_PREFIX<<"

// hasDuplicateBlankParams reports whether a parameter list has two or more blank (`_`) or unnamed
// parameters. Go permits repeated blank params, but C# forbids duplicate parameter names (CS0100), so
// such a list needs synthetic placeholder names. A LONE blank/unnamed param stays `_` (valid C# and
// visually closer to the Go source).
func hasDuplicateBlankParams(parameters *types.Tuple) bool {
	if parameters == nil {
		return false
	}

	count := 0

	for i := 0; i < parameters.Len(); i++ {
		if name := parameters.At(i).Name(); name == "" || name == "_" {
			count++

			if count >= 2 {
				return true
			}
		}
	}

	return false
}

// bodyUsesBlankDiscard reports whether the function's body contains a `_ = …` discard
// (single or tuple position). A LONE blank param normally keeps the literal `_` name
// (visually Go-like), but a parameter named `_` HIJACKS the body's C# discards —
// encoding/binary's bounds-check hints (`_ = b[7]`) assigned a byte to the blank
// littleEndian receiver (CS0029 ×12), so such functions synthesize blank names instead.
func bodyUsesBlankDiscard(funcDecl *ast.FuncDecl) bool {
	if funcDecl == nil || funcDecl.Body == nil {
		return false
	}

	found := false

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		if assign, ok := node.(*ast.AssignStmt); ok {
			for _, lhs := range assign.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok && ident.Name == "_" {
					found = true
					return false
				}
			}
		}

		return !found
	})

	return found
}

// variadicElementType returns the C# element type name for a variadic parameter, the ellipsis alias
// identifier that stands for `Span<element>` in a signature, and whether the parameter must instead be
// emitted inline as `Span<T>`. Both the alias and the inline form sit at namespace scope, so a
// SAME-PACKAGE named element type must be qualified with the package class — a bare nested name like
// `statDep` does not resolve there (CS0246). Only the alias NAME is constrained to a plain identifier
// though; its REFERENT may be qualified freely, so the readable form survives qualification by keying
// the identifier off the element's GO-facing name (`ꓸꓸꓸShape` = `Span<main_package.Shape>`,
// `ꓸꓸꓸunsafeꓸPointer` = `Span<unsafe_package.Pointer>`). A POINTER transliterates through go2cs's own
// `ж` notation (`ꓸꓸꓸжbox`); only a type parameter or a constructed element with no such rendering
// (`map<@string, any>`, `slice<byte>`, `Action<…>`) has nothing to key on and stays inline.
func (v *Visitor) variadicElementType(elem types.Type) (typeName string, aliasName string, inline bool) {
	typeName, identBody, inline := v.variadicElementParts(elem)

	if inline {
		return typeName, "", true
	}

	return typeName, EllipsisOperator + identBody, false
}

// variadicElementParts is variadicElementType's recursive core, returning the alias identifier BODY
// (no ellipsis prefix) so a pointer element can compose its pointee's.
func (v *Visitor) variadicElementParts(elem types.Type) (typeName string, identBody string, inline bool) {
	typeName = v.getCSharpTypeName(elem)

	// A type parameter is not in scope at namespace scope, so it can be neither an alias referent nor
	// an alias name — `First<T>(params Span<T> valsʗp)` has no readable form.
	if _, isTypeParam := elem.(*types.TypeParam); isTypeParam {
		return typeName, "", true
	}

	// A POINTER is the one CONSTRUCTED form with an identifier-safe rendering: go2cs already writes
	// `*T` as `ж<T>`, so the identifier transliterates to `жT` and still reads like the Go source
	// (`bs ...*box` → `params ꓸꓸꓸжbox bsʗp`). The pointee resolves through this same routine because
	// it needs the identical namespace-scope qualification — the alias referent must say
	// `ж<main_package.box>` where the INLINE form can say bare `ж<box>`, since only the inline form
	// sits inside the package class. A pointee with no alias form of its own (a type parameter, a
	// constructed pointee such as `*[]byte`) takes the whole element inline with it.
	if pointer, ok := elem.(*types.Pointer); ok {
		if typeName != fmt.Sprintf("%s<%s>", PointerPrefix, v.getCSharpTypeName(pointer.Elem())) {
			// getCSharpTypeName rendered this pointer some other way (an erased pointer-core type
			// parameter, a lifted type) — do not guess at its structure.
			return typeName, "", true
		}

		pointeeType, pointeeIdent, pointeeInline := v.variadicElementParts(pointer.Elem())

		if pointeeInline {
			return typeName, "", true
		}

		return fmt.Sprintf("%s<%s>", PointerPrefix, pointeeType), PointerPrefix + pointeeIdent, false
	}

	// Every other constructed type carries '<'/'>', which a using-alias identifier cannot contain and
	// for which there is no established transliteration (`Action<ж<options>>`, `map<@string, any>`,
	// `slice<byte>`).
	if strings.ContainsAny(typeName, "<>") {
		return typeName, "", true
	}

	// Derived BEFORE the referent is qualified/rewritten below — the identifier tracks the Go source,
	// the referent tracks what C# can resolve, and the two deliberately diverge.
	aliasIdent := v.variadicAliasIdent(elem, typeName)
	selfQualified := false

	if named, ok := elem.(*types.Named); ok {
		// A methodless named func type has already been rendered AS its base delegate
		// (`Action<…>`/`Func<…>`) by getCSharpTypeName — it is not a package-class member, so the
		// `<pkg>_package.` qualifier below would mangle it (`main_package.Action`, CS0426).
		if _, isCollapsed := methodlessNamedFuncSignature(elem); !isCollapsed {
			if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg && !strings.Contains(typeName, ".") {
				// packageScopeClassName (W3b), not a bare packageName+PackageSuffix concatenation:
				// Go sees ONE package for obj.Pkg() == v.pkg regardless of which file declares elem,
				// but under the whitebox-reference test model that one Go package emits into TWO C#
				// classes — a TEST-file-declared type (export_test.go's `AddrRange`) lives in the
				// bridge class, not the production one, and unconditionally qualifying to production
				// names a class that never declares it (CS0426). packageScopeClassName already makes
				// exactly this same-package-two-classes distinction for bare-identifier qualification
				// elsewhere; this is the identical question for a variadic alias's referent.
				typeName = fmt.Sprintf("%s.%s", v.packageScopeClassName(obj), typeName)
				selfQualified = true
			}
		}
	}

	// C# resolves a using-alias REFERENT with the compilation unit's own using directives NOT in
	// effect, so the referent may only name what resolves without them: a bare name (a golib type in
	// `namespace go`, or a GLOBAL-using alias from package_info.cs — `osꓸSignal`/`CorpusEntry` DO
	// carry over, which is why those two already ship as aliases), or a namespace-qualified class.
	// A cross-package SHORT form (`@unsafe.Pointer`, `ast.Expr`) leads with a FILE-LOCAL alias, so it
	// must be rewritten to that alias's own target — which is using-independent by construction,
	// being what the `using <alias> = <target>;` line itself resolves. Left as-is it fails CS0246
	// here, and go2cs-gen (which copies the using into its generated file) cannot resolve the symbol
	// either and falls back to unescaped text, `Span<unsafe.Pointer>`, whose bare keyword cascades
	// to CS8956.
	if !selfQualified {
		if head, member, qualified := strings.Cut(typeName, "."); qualified {
			switch {
			case v.importAliasTargets[head] != "":
				typeName = v.importAliasTargets[head] + "." + member
			case strings.HasSuffix(head, PackageSuffix):
				// Already a package CLASS (`io_package.Writer`), not an alias — a member of the
				// root `go` namespace, so it resolves from any converted file's namespace.
			default:
				// An alias this file has not bound yet — visitFile synthesizes canonical aliases for
				// inference-only foreign references AFTER the declarations are visited, so the target
				// is genuinely unknown here. Degrade to the inline form, which never needs one.
				return typeName, "", true
			}
		}
	}

	return typeName, aliasIdent, false
}

// variadicAliasIdent renders the body of a variadic element's ellipsis alias identifier so the
// signature reads like the Go source it came from: a qualifier is KEPT but joined with TypeAliasDot
// (`...unsafe.Pointer` → `params ꓸꓸꓸunsafeꓸPointer`), the same `pkgꓸType` convention package_info.cs
// already uses for its global usings — which is why os/signal's long-standing `ꓸꓸꓸosꓸSignal` and
// text/template's `ꓸꓸꓸreflectꓸValue` come out byte-identical through this path.
//
// The GO names are preferred over the emitted C# ones: they need no '@' keyword-escape stripping ('@'
// is legal only at identifier START, so `ꓸꓸꓸ@string` is a lex error, CS1002/CS0116), they carry no
// `_package` class suffix, and they undo a Δ collision-rename so go/types' `...Type` reads `ꓸꓸꓸType`
// rather than `ꓸꓸꓸΔType`. Any Go identifier is a legal C# one, and the ellipsis
// prefix means even a C# keyword (`...event`) cannot collide. Types with no such name — a basic type
// (`unsafe.Pointer`), a universe type (`error`), a lifted anonymous struct (internal/fuzz's
// `CorpusEntry`) — fall back to transliterating the emitted C# name.
func (v *Visitor) variadicAliasIdent(elem types.Type, typeName string) string {
	if named, ok := elem.(*types.Named); ok {
		if obj := named.Obj(); obj != nil && obj.Pkg() != nil {
			if obj.Pkg() == v.pkg {
				return obj.Name()
			}

			return obj.Pkg().Name() + TypeAliasDot + obj.Name()
		}
	}

	head, member, qualified := strings.Cut(typeName, ".")

	if !qualified {
		return strings.TrimPrefix(typeName, "@")
	}

	return strings.TrimSuffix(strings.TrimPrefix(head, "@"), PackageSuffix) + TypeAliasDot + member
}

// variadicParamType renders a variadic parameter's C# type, registering the file-local
// `using ꓸꓸꓸT = Span<…>;` alias whenever one applies. The alias is what keeps a variadic signature
// reading like its Go original (`shapes ...Shape` → `params ꓸꓸꓸShape shapesʗp`); it already backs
// os/signal's `Notify(…, params ꓸꓸꓸosꓸSignal sigʗp)` and internal/fuzz's `ꓸꓸꓸCorpusEntry`.
func (v *Visitor) variadicParamType(elem types.Type) string {
	typeName, aliasName, inline := v.variadicElementType(elem)
	spanType := fmt.Sprintf("Span<%s>", typeName)

	if inline {
		return spanType
	}

	// Two element types transliterating to ONE identifier in a single file would bind it twice
	// (CS1537). Keeping the qualifier makes that rare — a same-package `Shape` and an imported
	// `pkg.Shape` land on distinct `ꓸꓸꓸShape`/`ꓸꓸꓸpkgꓸShape` — but same-NAMED packages still collide
	// (`math/rand` and `crypto/rand` both yield `randꓸRand`). The first claim wins; the loser falls
	// back to the inline form, which is always correct if less readable.
	requiredUsing := fmt.Sprintf("%s = %s", aliasName, spanType)
	aliasPrefix := aliasName + " = "

	for _, existing := range v.requiredUsings.Keys() {
		if existing != requiredUsing && strings.HasPrefix(existing, aliasPrefix) {
			return spanType
		}
	}

	v.addRequiredUsing(requiredUsing)

	return aliasName
}

// callsSkipCountedRuntimeCaller reports whether body calls runtime.Caller or runtime.Callers —
// both take a skip argument the converted implementation satisfies by counting Go-source frames
// on a System.Diagnostics.StackTrace (runtime/managed_impl.cs's captureCallers). That machinery's
// own two frames are hand-marked [MethodImpl(NoInlining)] already (its header names the exact
// risk), but nothing protects the CALLER's side of the chain: a thin one-line forwarder — the
// shape flag.Set's call to its own unexported set is — is exactly what a fully-tiered/Release JIT
// inlines, which silently shifts the frame count by one and the skip-counted call resolves the
// wrong frame (or none: ok=false, the "?:0" shape). Tier-0 (no inlining at all) never triggers
// this, which is why it surfaced only under the tiering A/B's Release+TieredCompilation=0 sweep —
// see docs/phase4/MAILBOX.md, i9, 2026-08-30. FuncForPC itself takes a *pc*, not a skip count, so
// it is not part of this family; nothing else in the runtime frame-walking surface takes a literal
// skip argument the way Caller/Callers do.
func callsSkipCountedRuntimeCaller(info *types.Info, body *ast.BlockStmt) bool {
	if body == nil || info == nil {
		return false
	}

	found := false
	ast.Inspect(body, func(node ast.Node) bool {
		if found {
			return false
		}
		sel, ok := node.(*ast.SelectorExpr)
		if !ok {
			return true
		}
		used, ok := info.Uses[sel.Sel].(*types.Func)
		if !ok || used.Pkg() == nil || used.Pkg().Path() != "runtime" {
			return true
		}
		switch used.Name() {
		case "Caller", "Callers":
			found = true
			return false
		}
		return true
	})
	return found
}

// noInliningPrefix returns the [MethodImpl(NoInlining)] attribute text (with its trailing space,
// ready to prepend to whatever else functionAttributeMarker resolves to) when fnObj is a member
// of v.needsNoInlining — the package-wide closure computeNoInliningClosure computed before any
// file in this package was visited, covering both direct runtime.Caller/Callers callers and the
// thin-forwarder chains that reach them (see callerInliningAnalysis.go; a per-function direct-call
// check alone is NOT sufficient — flag.Set needs the attribute despite never mentioning
// runtime.Caller itself, confirmed by hand-testing before this closure existed). Composes with —
// never replaces — [GoInit]/[GoRecv]: a receiver method that also needs this needs both
// attributes, not one or the other. Registers the attribute's namespace the same way
// visitStructType's InteropServices marker does, only when the file actually emits one.
func (v *Visitor) noInliningPrefix(fnObj types.Object) string {
	if fnObj != nil && v.needsNoInlining[fnObj] {
		v.addRequiredUsing("System.Runtime.CompilerServices")
		return "[MethodImpl(MethodImplOptions.NoInlining)] "
	}
	return ""
}

// litNoInliningPrefix is noInliningPrefix's func-literal counterpart: a closure has no
// types.Object, so it cannot live in v.needsNoInlining and is instead judged directly against its
// own body (literalCallsSkipCountedRuntimeCaller) and, for a closure that itself thinly forwards
// to an already-marked package function, against the same forwarder shape thinForwarderTarget
// already recognizes for declarations — both in callerInliningAnalysis.go. C#'s lambda-attribute
// grammar places the attribute list before everything else, including an explicit return type, so
// every convFuncLit emission site prepends this ahead of its own return-type prefix, not after it.
func (v *Visitor) litNoInliningPrefix(funcLit *ast.FuncLit) string {
	needsIt := literalCallsSkipCountedRuntimeCaller(v.info, funcLit)

	if !needsIt {
		if target := thinForwarderTarget(v.info, v.pkg, funcLit.Body); target != nil {
			needsIt = v.needsNoInlining[target]
		}
	}

	if needsIt {
		v.addRequiredUsing("System.Runtime.CompilerServices")
		return "[MethodImpl(MethodImplOptions.NoInlining)] "
	}

	return ""
}

// funcPlaceholderFormat is the ONE definition of the line the converter writes where a
// manualConversionFuncs registration displaces a func body. It is a WITNESS two other places read,
// which is why it lives here beside its emission rather than being spelled three times: the
// registry's source-side guard (manualConversionDestination_test.go) uses it to prove every
// registration displaces something, and layout L3's merge uses it to learn which TARGETS a
// displacement actually happened on — which is what routes a scope-restricted hand-own to the
// folders it belongs in (platformHandOwn.go).
const funcPlaceholderFormat = "// go2cs generated this placeholder — func %s is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])"

// funcPlaceholderLead is the format's fixed prefix, for the readers that scan for the line rather
// than render it. Kept as its own constant rather than derived, and pinned to the format by
// TestFuncPlaceholderLeadMatchesTheFormat so the two cannot drift apart silently.
const funcPlaceholderLead = "// go2cs generated this placeholder — func "

func (v *Visitor) visitFuncDecl(funcDecl *ast.FuncDecl) {
	// A declaration owned by a manual conversion (see manualTypeOperations.go) emits only a
	// marker comment — the package's *_impl.cs supplies the implementation.
	//
	// The comment sink still has to be served, because this return skips BOTH places a converted
	// declaration drains it: the writeDoc below, which flushes the free-floating comments standing
	// AHEAD of the declaration, and the body visit, which flushes the ones INSIDE it. Unserved,
	// neither set is dropped — both are misplaced, since the drain is positional and the next
	// declaration's own writeDoc takes everything positioned before it. So the preceding comments
	// are written here exactly as they would be above a converted declaration, and the ones inside
	// the displaced span are retired: they document a body this file does not contain.
	if v.isManualFuncDecl(funcDecl) {
		v.outputBuilder.WriteString(v.newline)
		v.writeDoc(nil, funcDecl.Pos())
		v.writeOutput(funcPlaceholderFormat, funcDecl.Name.Name)
		v.outputBuilder.WriteString(v.newline)
		v.discardStandAloneComments(funcDecl.Pos(), funcDecl.End())
		return
	}

	v.inFunction = true
	v.capturedVarCount = nil
	v.tempVarCount = nil
	v.useUnsafeFunc = false
	v.loopCopyBackStack = nil
	v.continueTargetStack = nil

	// Does anything in this declaration — or the package it lives in — name an identifier `heap`?
	// If so, every heap-box emission below spells golib's intrinsic `builtin.heap` (see
	// heapIntrinsicName); otherwise the bare `heap` the corpus reads everywhere.
	v.heapIntrinsicShadowed = v.packageDeclaresHeapIntrinsicIdent() || v.declaresHeapIntrinsicIdent(funcDecl)

	// Plan which repeated `string(x)` sstring conversions to lift to a single function-scope temp
	// (loop-invariant / repeated-conversion hoisting — see planSStringHoists). Runs after tempVarCount
	// is reset so the temp names are function-scoped, and before the body is emitted so visitBlockStmt
	// and convCallExpr can consult the plan.
	v.planSStringHoists(funcDecl)

	goFunctionName := funcDecl.Name.Name
	csFunctionName := getSanitizedFunctionName(goFunctionName)

	// A Go function named `_` (blank) is a compile-time-only construct — it is never callable, and
	// a package may declare several. Emitting it literally as a method `_` makes a `_ = expr`
	// discard in its body bind to the method group (CS1656). Give it a unique generated name so
	// `_` remains a discard inside (and multiple blank funcs don't collide).
	if goFunctionName == "_" && funcDecl.Recv == nil {
		csFunctionName = getGlobalTempVarName("_")
	}

	// A `-tests` variant registered THIS test-file method declarator for a Δ-rename — its name
	// collides with a pinned production element or a dot-imported function (B2/B9, see
	// performNameCollisionAnalysis); reference sites follow via convIdent's isMethod arm.
	if testMethodRenames[v.info.ObjectOf(funcDecl.Name)] {
		csFunctionName = ShadowVarMarker + csFunctionName
	}

	v.currentFuncDecl = funcDecl
	v.currentFuncName = csFunctionName
	v.currentFuncPrefix = &strings.Builder{}

	// Record the Go name of every function literal this declaration encloses (the funcLits half
	// of the file's GoPositionMap record) — from the AST, before any conversion of the body, so
	// the counter is a source-order fact (see positionMapOperations.go).
	v.collectFuncLitNames(funcDecl)

	// Tier C: the hoisted string-literal fields this function OWNS (its body holds their first
	// package-wide use) lead the prefix, so they land immediately above the doc comment — ahead of
	// any anonymous type this function's body lifts into the same builder.
	v.writeHoistedLiteralDecls(funcDecl)

	// The //go:cgo_unsafe_args block lift (cgoUnsafeArgsLift.go): a lifted declaration's synthesized
	// block struct joins the prefix here, beneath the hoisted literals, exactly where a function-local
	// anonymous struct would lift; the construction line and the argument rewrite follow below.
	v.currentCgoLift = nil
	v.beginCgoUnsafeArgsLift(funcDecl)

	v.varNames = make(map[*types.Var]string)

	currentFuncType := v.info.ObjectOf(funcDecl.Name).(*types.Func)

	if currentFuncType == nil {
		panic("@visitFuncDecl - Failed to find function \"" + goFunctionName + "\" in the type info")
	}

	signature := currentFuncType.Signature()
	v.currentFuncSignature = signature
	v.currentReturnSignature = signature
	// A real DECLARATION's signature: whatever a package-level literal seeded earlier in this file is
	// superseded, so the parameter-detection predicates apply again.
	v.funcSignatureIsLiteralSeed = false

	// A pointer-core type parameter (`[P *T]`) of a plain function is ERASED — dropped from the
	// emitted generic parameter list, rendered as `ж<T>`, and classified as a pointer everywhere.
	// Every renderer/classifier consults this identity set (see collectErasedTypeParams), so the
	// analyses below (nil-safe params, deref aliases) already see the erased pointers.
	v.erasedTypeParams = collectErasedTypeParams(signature)

	// A generic capture-mode method (e.g. atomic.Pointer[T]) is emitted with its heap box
	// AS the receiver (`this ж<T> Ꮡx`) so the receiver's type parameter stays in scope for
	// the field-ref form `Ꮡx.of(Type.ᏑField)`. See packageDirectBoxReceiverMethods.
	directBoxReceiver := packageDirectBoxReceiverMethods != nil && packageDirectBoxReceiverMethods[currentFuncType]

	// Analyze function variables for reassignments and redeclarations (variable shadows).
	v.performVariableAnalysis(funcDecl, signature)

	// Record the function-local untyped consts whose declaration tightens to a single
	// concrete basic type (the declaration and every wrapper-keyed cast site consult it).
	v.performUntypedConstAnalysis(funcDecl)

	// Scope defer/recover to THIS function's own body: a `defer`/`recover` inside a nested
	// function literal (an IIFE or closure) belongs to that literal, not to this function, so it
	// must not force a func() execution context here. (performVariableAnalysis sets hasDefer/
	// hasRecover by walking everything, including nested literals.)
	v.hasDefer, v.hasRecover = v.funcBodyDeferRecover(funcDecl.Body)

	// A function with named return values that also uses defer/recover needs the named
	// returns declared outside the func() wrapper and returned after it (see the field doc on
	// namedReturnDeferMode). Determine that here, before the body is emitted, so visitReturnStmt
	// can route returns through the named result params.
	v.namedReturnDeferMode = false
	v.namedReturnNames = nil

	if funcDecl.Body != nil {
		v.namedReturnDeferMode, v.namedReturnNames = v.detectNamedReturnDefer(signature, v.hasDefer, v.hasRecover)
	}

	// Does this function's defer/recover scope emit as a GoFrame (an inline body in
	// try/catch/finally) rather than as the func((defer, recover) => …) execution context? Decided
	// HERE, before the body is visited, because the body's own `defer` statements register into the
	// frame by name and visitDeferStmt reads this to know that (see goFrameOperations.go).
	useGoFrame := v.goFrameEligible(funcDecl, signature)
	v.inGoFrame = useGoFrame
	v.goFrameNamedExit = false
	v.openGoFrames = 0

	// Which of this function's defers are emitted into the frame's finally rather than registered
	// (capability 4). Decided HERE for the same reason the frame form is: visitDeferStmt reads the
	// plan to know whether to emit a registration or a reached-flag, and both the flag declarations
	// and the finally's calls are composed from it after the body has rendered.
	v.loweredDefers = nil
	v.loweredDeferIndex = nil
	v.entryAliasBoxPaths = nil

	if useGoFrame {
		v.planDeferFinallyLowering(funcDecl)
	}

	if useGoFrame {
		v.openGoFrames = 1
	}

	// Collect parameter names from the function declaration
	if v.paramNames == nil {
		v.paramNames = HashSet[string]{}
	} else {
		v.paramNames.Clear()
	}

	// Collect the parameter OBJECTS too, so identIsParameter can distinguish a real parameter from
	// a local that merely SHADOWS a parameter's name (`func f(t *T){ { var t *T; … } }`).
	v.paramObjects = map[types.Object]bool{}

	for _, param := range funcDecl.Type.Params.List {
		for _, name := range param.Names {
			v.paramNames.Add(name.Name)

			if obj := v.info.Defs[name]; obj != nil {
				v.paramObjects[obj] = true
			}
		}
	}

	// A parameter/result's own anonymous-struct or -interface type is externally significant
	// across function scopes (see liftAtCallBoundary's doc comment) — set for both loops below.
	v.liftAtCallBoundary = true

	// Loop through function results to check if any are structs
	if funcDecl.Type.Results != nil {
		for index, field := range funcDecl.Type.Results.List {
			var fieldName string

			if field.Names == nil {
				fieldName = fmt.Sprintf("R%d", index)
			} else {
				fieldName = field.Names[0].Name
			}

			// Check if the return type is a struct or pointer to a struct
			if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
				v.indentLevel++
				v.visitStructType(structType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}

			// Check if the return type is an anonymous interface
			if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
				v.indentLevel++
				v.visitInterfaceType(interfaceType, exprType, fieldName, field.Comment, true, nil)
				v.indentLevel--
			}
		}
	}

	// Loop through function parameters to check if any are structs
	if funcDecl.Type.Params != nil {
		for _, field := range funcDecl.Type.Params.List {
			for _, name := range field.Names {
				// Check if the parameter type is a struct or pointer to a struct
				if structType, exprType := v.extractStructType(field.Type); structType != nil && !v.liftedTypeExists(structType) {
					v.indentLevel++
					v.visitStructType(structType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(field.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, name.Name, field.Comment, true, nil)
					v.indentLevel--
				}
			}
		}
	}

	v.liftAtCallBoundary = false

	functionPrefixMarker := fmt.Sprintf(FunctionPrefixMarker, goFunctionName)
	functionAccessMarker := fmt.Sprintf(FunctionAccessMarker, goFunctionName)
	functionUnsafeMarker := fmt.Sprintf(FunctionUnsafeMarker, goFunctionName)
	functionPartialMarker := fmt.Sprintf(FunctionPartialMarker, goFunctionName)
	functionAttributeMarker := fmt.Sprintf(FunctionAttributeMarker, goFunctionName)
	functionParametersMarker := fmt.Sprintf(FunctionParametersMarker, goFunctionName)
	functionExecContextMarker := fmt.Sprintf(FunctionExecContextMarker, goFunctionName)
	functionBlockPrefixMarker := fmt.Sprintf(FunctionBlockPrefixMarker, goFunctionName)

	v.outputBuilder.WriteString(v.newline)
	v.outputBuilder.WriteString(functionPrefixMarker)
	v.writePositionSentinel(funcDecl.Pos())
	v.writeDoc(funcDecl.Doc, funcDecl.Pos())

	functionAccess := packageFuncAccess(goFunctionName, funcDecl.Recv == nil)

	// A test-file-declared EXPORTED free function whose signature references an unexported same-
	// package type is downgraded to `internal`: production emits that type internal and is converted
	// independently of (and before) the test files, so a public helper over it is CS0050/CS0051 (Go's
	// `func NewDecimal(uint64) *decimal` in strconv's internal_test.go). Internal is both correct and
	// sufficient regardless of test project model (see signatureReferencesUnexportedProductionType).
	// Methods (Recv != nil) get the identical whole-signature check applied separately, further down
	// (testMethodAccessDowngrade, W3a) — this block only ever fires for a free function, so it stops
	// at that gate rather than duplicating the method path's own gating here.
	if functionAccess == "public" && funcDecl.Recv == nil && v.isTestFileDecl(funcDecl.Pos()) &&
		v.signatureReferencesUnexportedProductionType(signature, v.pkg) {
		functionAccess = "internal"
	}

	isModuleInitializer := false

	if funcDecl.Recv == nil {
		// Handle Go "main" function as a special case, in C# this should be capitalized "Main"
		if csFunctionName == "main" {
			csFunctionName = "Main"
		} else if csFunctionName == "init" {
			isModuleInitializer = true

			// C# module initializer functions should have internal scope
			functionAccess = "internal"

			packageLock.Lock()

			if initFuncCounter > 0 {
				csFunctionName = fmt.Sprintf("init%s%d", ShadowVarMarker, initFuncCounter)
			}

			initFuncCounter++

			packageLock.Unlock()
		}
	}

	blockContext := DefaultBlockStmtContext()
	blockContext.innerPrefix = functionBlockPrefixMarker
	typeParams, constraints := v.getGenericDefinition(currentFuncType.Type())

	resultSignature := v.generateResultSignature(signature)

	// B′-S0 arm (a) — the R3 ruling (2026-09-02): a ref-return primary declares `ref T` where the
	// Go signature says `*T`. The selection's bare-return precondition guarantees exactly one
	// result of the receiver's own pointer type, so the rewrite is total here; the body's
	// `return v` sites take `return ref v;` in visitReturnStmt, and RecvGenerator's twin restores
	// the ж surface (returning its OWN box — Go's receiver pointer) for every existing consumer.
	v.currentRefReturnPrimary = packageRefReturnPrimaryMethods != nil && packageRefReturnPrimaryMethods[currentFuncType.Origin()]

	if v.currentRefReturnPrimary {
		if pointer, isPointer := types.Unalias(signature.Results().At(0).Type()).(*types.Pointer); isPointer {
			resultSignature = "ref " + v.getCSharpTypeName(pointer.Elem())
		}
	}

	v.writeOutput("%s%s static%s%s %s %s%s(%s)%s%s", functionAttributeMarker, functionAccessMarker, functionUnsafeMarker, functionPartialMarker, resultSignature, csFunctionName, typeParams, functionParametersMarker, constraints, functionExecContextMarker)

	// The CONVERTED body text (for detecting whether a pointer parameter's deref VALUE alias is
	// actually referenced — a param used only through its box gets no alias; see below). Captured
	// from the output builder across the body visit; the signature written above is excluded.
	bodyText := ""

	if funcDecl.Body != nil {
		blockContext.format.useNewLine = len(constraints) > 0

		// In namedReturnDeferMode the func() wrapper is nested inside an extra block body, so
		// the lambda body sits one level deeper. Bump the indent across the body visit so the
		// statements (and the closing `}` that the `);` attaches to) align under `func(…`. The
		// frame form nests the same way: its body is the `try` block inside the method's own block.
		bodyInBlockForm := v.namedReturnDeferMode || useGoFrame

		if bodyInBlockForm {
			v.indentLevel++
		}

		bodyStart := v.outputBuilder.Len()
		v.visitBlockStmt(funcDecl.Body, blockContext)
		v.assertNoPendingKeepAlive("func " + funcDecl.Name.Name)
		bodyText = v.outputBuilder.String()[bodyStart:]

		if bodyInBlockForm {
			v.indentLevel--
		}
	}

	signatureOnly := funcDecl.Body == nil

	parameterSignature, receiverAccess := v.generateParametersSignature(signature, true)
	blockPrefix := ""
	// In namedReturnDeferMode this holds the named-return declarations, emitted outside the frame's
	// `try` (see the frame assembly below).
	namedReturnDeclsStr := ""

	// If receiver access is not public, update function access to match
	if len(receiverAccess) > 0 && receiverAccess != "public" {
		functionAccess = receiverAccess
	}

	functionAccess = v.testMethodAccessDowngrade(functionAccess, funcDecl, signature)

	if !signatureOnly {
		resultParameters := &strings.Builder{}
		arrayClones := &strings.Builder{}
		implicitPointers := &strings.Builder{}
		paramHeapBoxes := &strings.Builder{}

		// In namedReturnDeferMode the named-return declarations are emitted OUTSIDE the func()
		// wrapper (so defers/recover mutate them by closure); collect them separately. Otherwise
		// they go into the block prefix inside the wrapper as before.
		namedReturnDecls := &strings.Builder{}

		// In-wrapper `ref var name = ref Ꮡname.Value;` re-aliases for heap-box-backed named
		// results in namedReturnDeferMode (see below) — joined into the block prefix with the
		// parameter deref aliases.
		namedResultAliases := &strings.Builder{}

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			resultParams := signature.Results()
			paramIndex := 0

			resultDeclTarget := resultParameters

			if v.namedReturnDeferMode {
				resultDeclTarget = namedReturnDecls
			}

			for _, field := range funcDecl.Type.Results.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous parameter (no name)
					paramIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							// In namedReturnDeferMode a BLANK result is still a real slot: return
							// statements write it and the post-wrapper return reads it back, so it
							// must be declared out here alongside the named ones (its generated
							// name comes from namedResultName). Outside that mode the slot never
							// exists — the wrapper returns the tuple directly.
							if v.namedReturnDeferMode {
								blankParam := resultParams.At(paramIndex)

								resultDeclTarget.WriteString(v.newline)
								v.writeString(resultDeclTarget, "%s%s %s = %s;", v.indent(v.indentLevel+1), v.getCSharpTypeName(blankParam.Type()), v.namedResultName(blankParam), v.zeroValueInitializer(blankParam.Type()))
							}

							paramIndex++
							continue
						}

						param := resultParams.At(paramIndex)
						paramName := getSanitizedIdentifier(v.getIdentName(ident))

						// A named result the body never touches needs no local at all — the name
						// still reads off the C# tuple return type. Checked BEFORE the newline is
						// reserved, so the prologue does not keep a blank line where the
						// declaration was. A nil body switches liveness OFF for the two lowerings
						// whose generated code reads the locals (see namedResultNeedsDeclaration).
						livenessBody := funcDecl.Body

						if v.namedReturnDeferMode || useGoFrame {
							livenessBody = nil
						}

						if !v.namedResultNeedsDeclaration(livenessBody, param) {
							paramIndex++
							continue
						}

						resultDeclTarget.WriteString(v.newline)

						// A heap-box-backed named result (routed to shared storage by the
						// capture/escape analyses — see identHasHeapBox) must declare the box
						// its render sites reference (`Ꮡerr`); the plain form leaves it
						// undeclared (CS0103 — internal/poll SendFile's deferred
						// TestHookDidSendFile reading `Ꮡerr.ValueSlot`).
						if v.identHasHeapBox(param, param.Type()) {
							if v.namedReturnDeferMode {
								// The decls sit OUTSIDE the func() wrapper, whose lambda cannot
								// capture a ref local (CS8175): create only the box here; the
								// wrapper re-aliases the value name inside (namedResultAliases),
								// and the final post-defer return reads through the box (see the
								// namedReturnDeferMode close-out below).
								v.writeString(resultDeclTarget, "%sheap<%s>(out var %s%s);", v.indent(v.indentLevel+1), v.getCSharpTypeName(param.Type()), AddressPrefix, v.boxBaseName(ident))
								v.writeString(namedResultAliases, "%s%sref var %s = ref %s%s%s;", v.newline, v.indent(v.indentLevel+1), paramName, AddressPrefix, v.boxBaseName(ident), namedResultBoxAccessor(param.Type()))
							} else {
								v.writeString(resultDeclTarget, "%s%s", v.indent(v.indentLevel+1), v.convertToHeapTypeDecl(ident, true))
							}

							paramIndex++
							continue
						}

						// A result whose Go zero value is not all-bits-zero — a fixed-size array,
						// a promoted-embed struct, a struct carrying a fixed array at any depth —
						// must construct rather than take `default!` (see zeroValueInitializer).
						v.writeString(resultDeclTarget, "%s%s %s = %s;", v.indent(v.indentLevel+1), v.getCSharpTypeName(param.Type()), paramName, v.zeroValueInitializer(param.Type()))

						paramIndex++
					}
				}
			}
		}

		parameters := getParameters(signature, true)

		// A pointer param whose deref VALUE alias is skipped (dead — see below) still needs the
		// signature rebuilt so it emits as the box `Ꮡ<name>` the body references — but with no alias
		// there is nothing in implicitPointers to trigger that rebuild. This flag forces it.
		skippedDeadPointerAlias := false

		// A ж-box ref-LOWERED pointer param (stage A2, DESIGN-zh-box-reduction §3.4) emits as
		// `ref T <name>` and needs no deref alias at all — the parameter IS the alias — but, like
		// the dead-alias case, nothing lands in implicitPointers to trigger the rebuild.
		hasRefLoweredParams := false

		for i := 0; i < parameters.Len(); i++ {
			param := parameters.At(i)

			// For any array parameters, Go copies the array by value
			// The BODY-side name of a parameter may be shadow-renamed by the analysis (a param
			// sharing an imported package name the function uses — math/big's `rand *rand.Rand`
			// → randΔ1): every alias/clone declared here must use the ANALYZED name or the
			// renamed body uses reference a name that was never declared (CS0103 ×3). The box
			// (`Ꮡrand`) keeps the RAW name, as everywhere else.
			analyzedName := param.Name()

			if renamed, ok := v.varNames[param]; ok && renamed != "" {
				analyzedName = renamed
			}

			// Direct, aliased, AND named array types all clone: a NAMED array's generated wrapper
			// is a struct over the same shared backing, and its strongly-typed Clone() returns the
			// wrapper (an array-typed VALUE RECEIVER — necessarily a named type — is a per-call
			// copy in Go and clones here too). A struct carrying array fields clones the same way
			// (typeNeedsValueClone), through its generated ΔClone().
			//
			// A BLANK or unnamed parameter is skipped: it is emitted with a synthetic name and can
			// never be referenced, so there is nothing for the copy to protect — and the clone line
			// would be written against the EMPTY analyzed name (` = .ΔClone();`, CS1525 ×2 in
			// log/slog's benchmark `Handle(disabledHandler, context.Context, slog.Record)`, whose
			// every parameter is blank). The existing array-typed arm had the same latent hole; no
			// blank array parameter happened to exist in the corpus.
			if analyzedName != "" && analyzedName != "_" && typeNeedsValueClone(param.Type()) {
				// A heap-boxed array param folds its by-value clone into the box init below
				// (the plain reassignment would reference the pre-rename name — CS0103).
				if !v.paramNeedsHeapBox(param) {
					v.writeString(arrayClones, "%s%s%s = %s%s;", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), getSanitizedIdentifier(analyzedName), valueCloneSuffix(param.Type()))
				}
			}

			// All pointers in Go can be implicitly dereferenced, so setup a "local ref" instance to each.
			// paramPointerType also classifies an ERASED pointer-core type parameter (`p P` under
			// `[P *T]` renders as `ж<T> Ꮡp` — see pointerCoreConstraint), so it takes the same
			// deref-alias and box conventions as a plain pointer parameter.
			if pointerType, ok := v.paramPointerType(param.Type()); ok {
				if i == 0 && funcDecl.Recv != nil && !directBoxReceiver {
					// Skip receiver parameter (direct-ж receivers get the deref below, so
					// the box parameter `Ꮡx` resolves to the value `x` in the body).
					continue
				}

				// A Phase-A ref-lowered parameter (ж-box A2): no box exists and no deref alias is
				// emitted — the `ref T <name>` parameter itself is the value alias every body use
				// binds. The signature rebuild below emits the lowered form.
				if v.paramIsRefLowered(param) {
					hasRefLoweredParams = true
					continue
				}

				// An unnamed (`func(*T)`) or blank (`_`) pointer parameter is never referenced in the
				// body, so it gets no deref alias; it is emitted in the signature with a synthetic name
				// and no box (`Ꮡ`) convention. Emitting the deref would produce `ref var  = ref Ꮡ.Value;`.
				if param.Name() == "" || param.Name() == "_" {
					continue
				}

				// A NAMED pointer param whose deref'd VALUE alias is never referenced in the body —
				// every use goes through the box `Ꮡp` (`unsafe.Pointer(p)`, `p == nil`, or passing p
				// as a pointer) — gets no alias either. The alias `ref var p = ref Ꮡp.Value` would be
				// a dead local that DEREFERENCES the box, so a nil argument NREs at function entry even
				// though Go never touches the pointee (syscall's `writeFile(…, overlapped *Overlapped)`
				// called with a nil overlapped, used only as `unsafe.Pointer(overlapped)`). Skipping an
				// unreferenced alias is behavior-preserving and removes the spurious nil deref. The
				// scan errs toward KEEPING the alias — a coincidental match in a field selector,
				// string, or comment — so a genuinely live value alias is never dropped (no CS0103).
				if !bodyReferencesIdentAsValue(bodyText, getSanitizedIdentifier(analyzedName)) {
					// The param still needs the box rename (`Ꮡ<name>`) the body's uses reference —
					// force the signature rebuild even if this leaves implicitPointers empty.
					skippedDeadPointerAlias = true
					continue
				}

				// Every direct-ж pointer entry alias — RECEIVER and PARAMETER alike — is
				// nil-DEFERRING, unconditionally. Go's nil rule does not distinguish the two: a nil
				// `*T` may be passed to a function exactly as it may be the receiver of a method, the
				// body RUNS, and the nil-pointer panic happens only where the body dereferences the
				// pointee. The eager `ref var p = ref Ꮡp.Value;` instead deref'd at ENTRY, so every
				// nil-tolerant body panicked before its guard could run — whether the guard is spelled
				// INLINE (`if b == nil`), DELEGATED through the box (`f.checkValid("chdir")`, which is
				// what asks `f == nil`), or delegated to a CALLEE that takes the pointer and never
				// derefs it (`concurrent.newIndirectNode(nil)`, the whole of `net`'s package-init
				// chain).
				//
				// DerefOrNull binds a NULL ref for a nil box, which is legal to hold and faults on
				// USE, so the panic is deferred to Go's own point rather than discarded: the first
				// field read/write or whole-struct copy raises Go's message, AFTER any side effect the
				// body performed first. That is what makes this unconditional and analysis-FREE — the
				// accessor is faithful whether or not the body guards. The two accessors it replaces
				// were each faithful only under a proof: `.Value` needed "this pointer is never nil"
				// (unprovable — it was simply the default), and the nil-SAFE accessor needed "the body
				// provably guards" (a body analysis whose silent `default(T)` slot was a wrong answer
				// wherever the analysis was wrong). Non-nil pointers are unaffected: DerefOrNull routes
				// them to the same real slot.
				//
				// It subsumes the third accessor this site used to select, too: a REFERENCE-TYPE
				// POINTEE (`*error`, `*[]T`, `*map`, `**T`, `*func`, `*chan`) took `.ValueSlot`,
				// because establishing its entry alias reads the HELD value rather than dereferencing
				// the box (in Go `*(&err)` of a nil `error` yields nil, no panic) and `.Value`'s
				// value-peeking IsNull check fired spuriously on it — tabwriter's `handlePanic(err
				// *error)`. DerefOrNull's own non-nil path IS `.ValueSlot` (its nil test,
				// IsNilStandardPointer, is STRUCTURAL and never peeks at the held value), so it
				// answers that case identically while also surviving a nil BOX, which `.ValueSlot`
				// cannot: reached through a null reference it throws at the alias line. The two
				// selections were mutually exclusive under the retired analysis, which ran FIRST and
				// so kept nil-ability ahead of pointee-kind; type-selecting `.ValueSlot` ahead of an
				// unconditional nil policy would invert that order and hand 9 corpus aliases across 8
				// files (`internal/weak`'s `ptr`, runtime mbitmap's `header`, dwarf's `fixups`, …) a
				// NEW entry-time fault on exactly the nil arguments the base tolerated — the very
				// defect this unification exists to close. `.ValueSlot` remains type-selected
				// everywhere else it belongs (named-result boxes, box-of-pointer LOCALS, `heap()`, the
				// reflection bridge's field paths); it is only at this site — a pointer's ENTRY alias,
				// where nothing can know whether the body dereferences — that the nil-policy accessor
				// is the honest one. See *The THREE deref accessors of ж<T>*.
				derefAccessor := NilDeferringDerefAccessor

				// Record the alias's own source expression. The alias is declared INSIDE the
				// frame's try, so anything emitted into the frame's FINALLY — a defer→finally
				// lowered call — cannot name it and must go through the box instead. Captured
				// here rather than reconstructed later so the two spellings cannot drift.
				if v.entryAliasBoxPaths == nil {
					v.entryAliasBoxPaths = map[string]string{}
				}

				v.entryAliasBoxPaths[getSanitizedIdentifier(analyzedName)] = fmt.Sprintf("%s%s.%s", AddressPrefix, param.Name(), derefAccessor)

				if v.options.preferVarDecl {
					v.writeString(implicitPointers, "%s%sref var %s = ref %s%s.%s;", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), AddressPrefix, param.Name(), derefAccessor)
				} else {
					v.writeString(implicitPointers, "%s%sref %s %s = ref %s%s.%s;", v.newline, v.indent(v.indentLevel+1), convertToCSTypeName(pointerType.Elem().String()), getSanitizedIdentifier(analyzedName), AddressPrefix, param.Name(), derefAccessor)
				}
			}

			// A value parameter marked for an entry-time heap box (a capture-mode/direct-ж
			// method is called on it — see paramNeedsHeapBox): the incoming value arrives as
			// `<name>ʗp` (renamed in the signature) and is boxed here, so body uses read and
			// write the boxed storage — the same storage the callee mutates through the
			// receiver pointer — and convSelectorExpr routes the call through `Ꮡ<name>`. The
			// box keeps the ANALYZED (rendered) name, matching an escaping value local's
			// convention (see convertToHeapTypeDecl / boxBaseName). An ARRAY param folds its
			// Go by-value clone into the box init (its plain clone line above is skipped).
			if v.paramNeedsHeapBox(param) {
				incomingName := getHeapBoxParamName(param)

				if typeNeedsValueClone(param.Type()) {
					incomingName += valueCloneSuffix(param.Type())
				}

				if v.options.preferVarDecl {
					v.writeString(paramHeapBoxes, "%s%sref var %s = ref %s(%s, out var %s%s);", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(analyzedName), v.heapIntrinsicName(), incomingName, AddressPrefix, analyzedName)
				} else {
					csTypeName := v.getCSharpTypeName(param.Type())
					v.writeString(paramHeapBoxes, "%s%sref %s %s = ref %s(%s, out %s<%s> %s%s);", v.newline, v.indent(v.indentLevel+1), csTypeName, getSanitizedIdentifier(analyzedName), v.heapIntrinsicName(), incomingName, PointerPrefix, csTypeName, AddressPrefix, analyzedName)
				}
			}

			// Check if parameter is variadic, in this case parameter is a C# params array that needs to be converted to a Go slice<T>
			if i == parameters.Len()-1 && signature.Variadic() {
				// An unnamed or blank variadic parameter is unreferenceable by Go law, so its
				// unpacked local is dead — and emitting it is broken, not merely redundant.
				if variadicParamIsUnreferenceable(param) {
					continue
				}

				useSSlice := v.ssliceEligible[param]
				sliceMethod := "slice"
				sliceType := "slice"

				if useSSlice {
					sliceMethod = "sslice"
					sliceType = "sslice"
				}

				if v.options.preferVarDecl {
					v.writeString(resultParameters, "%s%svar %s = %s.%s();", v.newline, v.indent(v.indentLevel+1), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
				} else {
					v.writeString(resultParameters, "%s%s%s<%s> %s = %s.%s();", v.newline, v.indent(v.indentLevel+1), sliceType, v.getCSharpTypeName(param.Type().(*types.Slice).Elem()), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
				}

			}
		}

		if namedReturnDecls.Len() > 0 {
			namedReturnDeclsStr = namedReturnDecls.String()
		}

		if resultParameters.Len() > 0 {
			resultParameters.WriteString(v.newline)
			blockPrefix += resultParameters.String()
		}

		if arrayClones.Len() > 0 {
			if blockPrefix == "" {
				arrayClones.WriteString(v.newline)
			}

			blockPrefix += arrayClones.String()
		}

		if implicitPointers.Len() > 0 {
			if blockPrefix == "" {
				implicitPointers.WriteString(v.newline)
			}

			blockPrefix += implicitPointers.String()
		}

		// Entry-time heap boxes for capture-mode value params follow the pointer derefs; the
		// signature rebuild below renames each boxed param to its incoming `ʗp` form.
		if paramHeapBoxes.Len() > 0 {
			if blockPrefix == "" {
				paramHeapBoxes.WriteString(v.newline)
			}

			blockPrefix += paramHeapBoxes.String()
		}

		// The //go:cgo_unsafe_args block: constructed at entry from the parameters, after the boxes and
		// aliases it reads through, and pinned for the libcCall the body makes (cgoUnsafeArgsLift.go).
		if v.currentCgoLift != nil {
			liftBlock := v.newline + v.indent(v.indentLevel+1) + v.cgoUnsafeArgsBlockPrologue(v.currentCgoLift)

			if blockPrefix == "" {
				liftBlock += v.newline
			}

			blockPrefix += liftBlock
		}

		// In-wrapper value aliases for heap-box-backed named results (namedReturnDeferMode):
		// the box was created outside the wrapper; body statements keep referencing the plain
		// name through this ref alias, exactly like a deref'd pointer parameter.
		if namedResultAliases.Len() > 0 {
			if blockPrefix == "" {
				namedResultAliases.WriteString(v.newline)
			}

			blockPrefix += namedResultAliases.String()
		}

		if implicitPointers.Len() > 0 || paramHeapBoxes.Len() > 0 || skippedDeadPointerAlias || hasRefLoweredParams {
			updatedSignature := strings.Builder{}
			dupBlankParams := hasDuplicateBlankParams(parameters) || bodyUsesBlankDiscard(funcDecl)

			for i := 0; i < parameters.Len(); i++ {
				param := parameters.At(i)

				if i == 0 && funcDecl.Recv != nil {
					updatedSignature.WriteString("this ")

					// Get receiver parameter type
					recvTypeName := v.getRefParameterTypeName(param.Type())

					// Method accessibility is the more restrictive of the receiver type and the method's
					// own (Go) name: an unexported method on an exported type stays package-private
					// (internal) -- otherwise a public method returning that method's own unexported
					// types is CS0050 (inconsistent accessibility). A PUBLICIZED unexported receiver
					// type is emitted `public`, so its exported methods count as public here.
					if (getAccess(recvTypeName) == "public" || receiverTypeIsPublicized(param.Type())) && getAccess(goFunctionName) == "public" {
						functionAccess = "public"
					} else {
						functionAccess = "internal"
					}

					// This rebuild path recomputes functionAccess from scratch (see the identical
					// comment above it), so it needs the same W3a downgrade the normal path gets —
					// otherwise a test method whose signature happens to need pointer/heap-box
					// rebuilding would silently lose the downgrade the normal path already applied.
					functionAccess = v.testMethodAccessDowngrade(functionAccess, funcDecl, signature)

					if directBoxReceiver {
						// Direct-ж: emit the box itself (`ж<Box<T>> Ꮡb`) as the receiver. The
						// deref `ref var b = ref Ꮡb.Value;` is emitted above so the body's value
						// references still read as `b`, while `&b.field` uses the box `Ꮡb`.
						updatedSignature.WriteString(v.getCSharpTypeName(param.Type()))
						updatedSignature.WriteRune(' ')
						updatedSignature.WriteString(AddressPrefix + param.Name())
					} else {
						updatedSignature.WriteString(recvTypeName)
						updatedSignature.WriteRune(' ')

						recvParamName := param.Name()

						// A BLANK receiver keeps the literal `_` only when the body has no
						// `_ = …` discard — a parameter named `_` hijacks C# discards
						// (encoding/binary's bounds-check hints, CS0029 ×12).
						if recvParamName == "" || recvParamName == "_" {
							if dupBlankParams {
								recvParamName = fmt.Sprintf("_Δp%d", i)
							} else {
								recvParamName = "_"
							}
						} else if v.paramNeedsHeapBox(param) {
							// A heap-boxed VALUE receiver arrives under the `ʗp` name; the
							// preamble re-declares the analyzed name as the boxed ref alias
							// (see markAddressTakenBoxedReceiver). The receiver's C# TYPE is
							// unchanged, so the extension method's public surface — the
							// value-receiver form Go's method set requires — is preserved;
							// the box is an implementation detail of the body.
							updatedSignature.WriteString(getHeapBoxParamName(param))
							continue
						}

						updatedSignature.WriteString(getSanitizedIdentifier(recvParamName))
					}

					continue
				}

				if i > 0 {
					updatedSignature.WriteString(", ")
				}

				if i == parameters.Len()-1 && signature.Variadic() {
					updatedSignature.WriteString("params ")

					// If parameter is a slice, convert it to a Span
					if sliceType, ok := param.Type().(*types.Slice); ok {
						updatedSignature.WriteString(v.variadicParamType(sliceType.Elem()))
					} else {
						updatedSignature.WriteString("object[]")
					}

					// Variadic parameters are passed as C# param arrays, so we use a temporary
					// parameter name that will be later converted to a Go slice<T>
					updatedSignature.WriteRune(' ')
					updatedSignature.WriteString(getVariadicParamName(param))
				} else {
					// A Phase-A ref-lowered pointer parameter (ж-box A2) — `ref T <name>`, the
					// §3.4 signature: the parameter is the callee-side alias, under the ANALYZED
					// value name so every body use binds (shadow renames included). No box name
					// exists; the classifier guarantees no body use needs one (D1/D1′/D2 only).
					if v.paramIsRefLowered(param) {
						loweredPointerType, _ := v.paramPointerType(param.Type())
						loweredParamName := param.Name()

						if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
							loweredParamName = adjusted
						}

						updatedSignature.WriteString("ref ")
						updatedSignature.WriteString(v.getCSharpTypeName(loweredPointerType.Elem()))
						updatedSignature.WriteRune(' ')
						updatedSignature.WriteString(getSanitizedIdentifier(loweredParamName))
						continue
					}

					// The fixed-size-array dimension carrier, as in generateParametersSignature —
					// and this path is the one a `*[N]T` parameter always takes, because HAVING a
					// pointer parameter is what triggers the rebuild. So the pointee dims of
					// net/rpc's every reply argument could only ever be stamped here.
					updatedSignature.WriteString(emitGoArrayDimsAttribute(param.Type()))

					updatedSignature.WriteString(v.getCSharpTypeName(param.Type()))
					updatedSignature.WriteRune(' ')

					if _, ok := v.paramPointerType(param.Type()); ok {
						// An unnamed or blank (`_`) pointer param is never referenced (no deref alias
						// above), so emit a plain name without the box `Ꮡ` convention — synthesized
						// unique only when blanks would collide (else a lone `_` is kept).
						if param.Name() == "" || param.Name() == "_" {
							if dupBlankParams {
								updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
							} else {
								updatedSignature.WriteString("_")
							}
						} else {
							updatedSignature.WriteString(AddressPrefix)
							updatedSignature.WriteString(param.Name())
						}
					} else if param.Name() == "" || param.Name() == "_" {
						// Unnamed or blank (`_`) non-pointer param — keep a lone `_`, but synthesize a
						// unique placeholder when blanks would collide (Go allows repeated blank params;
						// C# forbids duplicate parameter names — CS0100).
						if dupBlankParams {
							updatedSignature.WriteString(fmt.Sprintf("_Δp%d", i))
						} else {
							updatedSignature.WriteString("_")
						}
					} else if v.paramNeedsHeapBox(param) {
						// A heap-boxed value parameter arrives under the `ʗp` name; the parameter
						// preamble re-declares the analyzed name as the boxed ref alias.
						updatedSignature.WriteString(getHeapBoxParamName(param))
					} else {
						// A shadow-renamed value parameter must emit its renamed name so the declaration
						// matches its usages (see generateParametersSignature) — this rebuilt-signature
						// path is taken for a function that ALSO has a pointer param (crypto/rsa's
						// `EncryptOAEP(hash hash.Hash, …, *PublicKey)`, where `hash` shadows the `hash`
						// package → `hashΔ1`), so it bypassed the generateParametersSignature lookup and
						// left the decl raw (`hash.Hash hash`) while its uses were `hashΔ1` (CS0103).
						paramName := param.Name()

						if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
							paramName = adjusted
						}

						updatedSignature.WriteString(getSanitizedIdentifier(paramName))
					}
				}
			}

			parameterSignature = updatedSignature.String()
		}
	}

	// Replace function markers
	v.replaceMarker(functionAccessMarker, functionAccess)

	if v.useUnsafeFunc {
		v.replaceMarker(functionUnsafeMarker, " unsafe")
		usesUnsafeCode = true
	} else {
		v.replaceMarker(functionUnsafeMarker, "")
	}

	// A bodyless func carrying `//go:linkname localName otherPkg.func` PULLS another package's
	// (often unexported) function by symbol — golang.org/x/sys/windows's LazyDLL/LazyProc reach
	// syscall.loadlibrary/loadsystemlibrary/getprocaddress this way. Route it: emit a forwarder body
	// that calls the target, instead of a throwing partial stub. Detected here so the `partial`
	// modifier is dropped (it now has a body); the body is written at the signatureOnly branch below.
	linknameAlias, linknameFunc, hasLinknameForward := "", "", false

	// The mirror image: a bodyless func under a one-arg `//go:linkname <thisFunc>` handle whose body
	// ANOTHER package PUSHES in (runtime/mgc.go pushes unique.runtime_registerUniqueMapCleanup). It
	// resolves to a forwarder to the pushing definition, or — when the pushed body is runtime
	// machinery the managed model cannot run — to a stub that panics naming the pair.
	linknamePanic := ""

	if funcDecl.Body == nil {
		if linknameAlias, linknameFunc, hasLinknameForward = v.funcLinknameForward(funcDecl); !hasLinknameForward {
			linknameAlias, linknameFunc, linknamePanic, hasLinknameForward = v.funcLinknamePush(funcDecl)
		}
	}

	// A nil body means the Go function is implemented externally (assembly or cgo):
	// emit a `partial` declaration. Its implementation is supplied either by a
	// hand-written companion (e.g. sync/atomic's doc_impl.cs) or, when none exists, by
	// the PartialStubGenerator (go2cs-gen), which emits a throwing default so the code
	// still compiles.
	if funcDecl.Body == nil && !hasLinknameForward {
		v.replaceMarker(functionPartialMarker, " partial")
	} else {
		v.replaceMarker(functionPartialMarker, "")
	}

	v.replaceMarker(functionParametersMarker, parameterSignature)

	if isModuleInitializer {
		// The `runtime` package's own init functions are Go's runtime SELF-BOOTSTRAP (arena
		// sizing checks, GC/proc setup): in real Go they run only after the assembly bootstrap
		// (osinit/schedinit) has populated globals like physPageSize. Converted code has no such
		// bootstrap — .NET is the runtime — so running them as module initializers executes
		// self-checks against zero-valued stub globals (arena's `% physPageSize` divides by
		// zero at assembly load, before Main). The faithful conversion of the Go runtime
		// bootstrap is to not run it: emit them as plain (never-called) methods.
		if v.pkg.Path() == "runtime" {
			v.replaceMarker(functionAttributeMarker, "/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ ")
		} else {
			v.replaceMarker(functionAttributeMarker, v.noInliningPrefix(v.info.ObjectOf(funcDecl.Name))+"[GoInit] ")
		}
	} else if strings.HasPrefix(parameterSignature, "this ref ") {
		v.replaceMarker(functionAttributeMarker, v.noInliningPrefix(v.info.ObjectOf(funcDecl.Name))+"[GoRecv] ")
	} else {
		v.replaceMarker(functionAttributeMarker, v.noInliningPrefix(v.info.ObjectOf(funcDecl.Name)))
	}

	var funcExecutionContext string

	if useGoFrame {
		// The frame form has no wrapper to assemble: the body is the method's own `try` block, so
		// all that goes here is the frame declaration and the `try` the body's `{` attaches to.
		// Named results are declared ahead of it for the same reason they were declared ahead of
		// the wrapper — deferred code mutates them and the exit reads them back afterwards.
		funcExecutionContext = v.goFrameHead(v.indentLevel, namedReturnDeclsStr)
	} else {
		funcExecutionContext = ""
	}

	v.replaceMarker(functionExecContextMarker, funcExecutionContext)
	if useGoFrame {
		blockPrefix = v.reindentGoFrameBlock(blockPrefix)
	}

	v.replaceMarker(functionBlockPrefixMarker, blockPrefix)

	if v.currentFuncPrefix.Len() > 0 {
		v.currentFuncPrefix.WriteString(v.newline)
	}

	v.replaceMarker(functionPrefixMarker, v.currentFuncPrefix.String())

	if useGoFrame {
		// Close the frame: the catch that parks a panic where recover() can read it, the finally
		// that drains the defers on every exit path, and the method's own closing brace.
		//
		// The catch arm of a value-returning function ends with Go's zero results. A panic a
		// deferred call RECOVERED leaves the function returning those (Go's rule), and one no
		// deferred call recovered never reaches the return at all — Run() re-throws it from the
		// finally, which overrides the pending return. It is also what keeps the method's endpoint
		// unreachable, so a value-returning function needs nothing after the try statement.
		catchReturn := ""

		if signature.Results().Len() > 0 && !v.namedReturnDeferMode {
			catchReturn = "return default!;"
		}

		savedIndent := v.indentLevel
		v.indentLevel = 0

		if v.namedReturnDeferMode {
			// The named-result exit (§4.4). Go assigns the result params, runs the deferred calls,
			// and only then returns — which a `finally` cannot do to a value a `return` has already
			// evaluated. So the results were declared before the try, every `return` inside it left
			// through a goto (which runs the finally exactly as a return would), and the read
			// happens out here. A heap-box-backed result's value alias lives inside the try, so the
			// read goes through the box (`Ꮡerr.ValueSlot`).
			returnNames := v.namedReturnBoxReadNames(signature, v.namedReturnNames)
			returnExpr := strings.Join(returnNames, ", ")

			if len(returnNames) > 1 {
				returnExpr = "(" + returnExpr + ")"
			}

			// The label exists only if something jumps to it: a body that simply falls off the end
			// reaches this return anyway, and an unreferenced label is a warning worth not emitting.
			exitLabel := ""

			if v.goFrameNamedExit {
				exitLabel = v.goFrameExitLabel() + ": "
			}

			v.writeOutputLn("%s%s%s%sreturn %s;%s%s}", v.goFrameTail(savedIndent, catchReturn), v.newline, v.indent(savedIndent+1), exitLabel, returnExpr, v.newline, v.indent(savedIndent))
		} else {
			v.writeOutputLn("%s%s%s}", v.goFrameTail(savedIndent, catchReturn), v.newline, v.indent(savedIndent))
		}

		v.indentLevel = savedIndent
	} else if signatureOnly {
		if linknamePanic != "" {
			// Cross-package //go:linkname push the managed model cannot honor — announce the pair.
			v.writeLinknamePanicStub(linknamePanic)
		} else if hasLinknameForward {
			// Cross-package //go:linkname pull or push — emit a forwarder body calling the target.
			v.writeLinknameForwarder(signature, linknameAlias, linknameFunc)
		} else {
			// Bodyless (assembly/cgo) function: emit a `partial` declaration; the body is
			// supplied by a hand-written companion or the PartialStubGenerator.
			v.writeOutputLn(";")
		}
	} else {
		v.outputBuilder.WriteString(v.newline)
	}

	v.inFunction = false
	v.inGoFrame = false
	v.openGoFrames = 0
}

// identIsParameter checks if the given identifier is a parameter in the current function.
func (v *Visitor) identIsParameter(ident *ast.Ident) bool {
	if v.paramNames == nil || !v.paramNames.Contains(ident.Name) {
		return false
	}

	// The name matches a parameter, but a local can SHADOW a parameter of the same name. Only the
	// actual parameter object gets the deref-aliased box treatment (`Ꮡp`); a shadowing local that is
	// already a pointer (`ж<T> tΔ2`) must keep its plain form, not get a spurious `&` (CS0103 on the
	// undefined `ᏑtΔ2`). Verify the resolved object is genuinely a parameter; fall back to the name
	// match when it cannot be resolved.
	if obj := v.info.ObjectOf(ident); obj != nil && v.paramObjects != nil {
		return v.paramObjects[obj]
	}

	return true
}

// isDerefdPointerParamIdent reports whether ident resolves to a non-blank pointer (`*T`) PARAMETER
// — one that is emitted as a deref alias `ref var p = ref Ꮡp.Value` over its box `Ꮡp`. A pointer
// LOCAL (which already holds the box directly) and an unsafe.Pointer param are excluded. Used both
// to drive the box (`Ꮡp`) form in `==`/`!=` comparisons and to gate the nil-safe deref accessor.
func (v *Visitor) isDerefdPointerParamIdent(ident *ast.Ident) bool {
	if ident == nil || ident.Name == "" || ident.Name == "_" || !v.identIsParameter(ident) {
		return false
	}

	identType := v.getIdentType(ident)

	if identType == nil {
		return false
	}

	if _, isPtr := identType.Underlying().(*types.Pointer); isPtr {
		return true
	}

	// An ERASED pointer-core type parameter (`p P` under `[P *T]`) is a deref-aliased pointer
	// parameter too — its box drives `==`/`!=` comparisons and the nil-safe accessor gate the
	// same way a plain `*T` parameter's does.
	if typeParam, ok := types.Unalias(identType).(*types.TypeParam); ok {
		_, erased := v.typeParamErased(typeParam)
		return erased
	}

	return false
}

// bodyReferencesIdentAsValue reports whether name appears in the CONVERTED body text as a
// STANDALONE identifier — not as a substring of a larger identifier and, decisively, not as the
// suffix of its own box form `Ꮡname` (the address marker Ꮡ is a Unicode LETTER, so a preceding
// letter excludes the box occurrence). Used to decide whether a pointer parameter's deref VALUE
// alias (`ref var name = ref Ꮡparam.Value`) is live: every box/`unsafe.Pointer`/`== nil`/pass-as-
// pointer use renders through `Ꮡparam`, so a param touched only through its box leaves name absent
// and its alias is a dead local. A real value use always emits name as an identifier, so it always
// matches — the boundary test only ever ADDS spurious matches (a field selector `x.name`, a string,
// a comment), which keep the alias, so a genuinely live alias is never dropped.
//
// ⚠ Keeping a DEAD alias is not free, which is why the one spurious match that is both systematic
// and namable is excluded: a C# NAMED-ARGUMENT LABEL (see isNamedArgumentLabel). The alias
// dereferences the box, so a dead one turns a legitimately nil argument into an entry-time
// NilPointerDereference the Go never performs.
func bodyReferencesIdentAsValue(bodyText, name string) bool {
	if name == "" {
		return false
	}

	for offset := 0; ; {
		index := strings.Index(bodyText[offset:], name)

		if index < 0 {
			return false
		}

		start := offset + index
		end := start + len(name)

		beforeOK := start == 0

		if !beforeOK {
			r, _ := utf8.DecodeLastRuneInString(bodyText[:start])
			beforeOK = !isIdentifierRune(r)
		}

		afterOK := end == len(bodyText)

		if !afterOK {
			r, _ := utf8.DecodeRuneInString(bodyText[end:])
			afterOK = !isIdentifierRune(r)
		}

		if beforeOK && afterOK && !isNamedArgumentLabel(bodyText, start, end) {
			return true
		}

		offset = start + 1
	}
}

// isNamedArgumentLabel reports whether the whole-word match at [start, end) is the LABEL half of a
// C# named argument (`new T(field: value)`) rather than a reference to the value.
//
// The converter renders a Go composite literal's field keys as named arguments to the fieldwise
// constructor go2cs-gen generates, so a pointer PARAMETER whose name matches one of the literal's
// FIELDS matches the whole-word scan on the label alone — and Go's own idiom is to name the field
// after the value it is initialized from. internal/concurrent's
// `newIndirectNode(parent *indirect) { return &indirect{node: …, parent: parent} }` never
// dereferences parent (the C# passes the box, `parent: Ꮡparent`), yet the label kept its value
// alias alive; `ref var parent = ref Ꮡparent.Value` then dereferenced the box at ENTRY, and the
// root node — created as `newIndirectNode(nil)`, a legitimately nil parent — threw
// NilPointerDereference inside `NewHashTrieMap`, taking down `unique`'s package initializer,
// `net/netip`'s, and every dependent (encoding/gob's TestNetIP is where it surfaced).
//
// A label is the identifier immediately followed by ONE `:` — never `::`, the namespace qualifier —
// whose preceding non-space character opens or continues an argument list. Every other colon form
// the converter emits is excluded by that: a `case X:` arm and a goto label are preceded by a
// keyword or a statement boundary, an interpolated format specifier (`{x:F2}`) by a brace, and a
// conditional's `cond ? a : b` spaces its colon. A parameter genuinely used elsewhere still matches
// there — this only discards the label occurrence, never the scan.
func isNamedArgumentLabel(bodyText string, start, end int) bool {
	if end >= len(bodyText) || bodyText[end] != ':' {
		return false
	}

	if end+1 < len(bodyText) && bodyText[end+1] == ':' {
		return false
	}

	for i := start - 1; i >= 0; i-- {
		switch bodyText[i] {
		case ' ', '\t', '\r', '\n':
			continue
		case '(', ',':
			return true
		default:
			return false
		}
	}

	return false
}

// isIdentifierRune reports whether r can appear within a C# identifier — a Unicode letter (which
// includes the go2cs marker glyphs Ꮡ/ж/Δ/ᴛ), a Unicode digit, or underscore.
func isIdentifierRune(r rune) bool {
	return r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r)
}

// isDerefdPointerReceiverIdent reports whether ident is the current method's POINTER (`*T`)
// RECEIVER — which, like a deref'd pointer parameter, is emitted as a value alias
// `ref var r = ref Ꮡr.Value` over its box `Ꮡr`. Go's `r == nil` on such a receiver is a POINTER
// comparison (`func (f *File) checkValid() { if f == nil … }`), so it must compare the box
// `Ꮡr == nil`, not the deref'd struct value `r == nil` (which binds the generated
// `T.operator==(T, NilType)` — a null-embed-box NRE for a promoted-embed struct). The receiver is
// deliberately NOT a "parameter" in identIsParameter's model (paramNames excludes Recv), so it needs
// its own recognizer; scoped to the `==`/`!=` operand handling in convBinaryExpr (unlike a pointer
// PARAMETER it is not folded into nilSafePtrParamNames, so the receiver's deref-alias form is
// unchanged — only the comparison switches to the box). Object identity via identResolvesToReceiver,
// so a local shadowing the receiver name keeps its own render.
func (v *Visitor) isDerefdPointerReceiverIdent(ident *ast.Ident) bool {
	if ident == nil || ident.Name == "" || ident.Name == "_" {
		return false
	}

	isPtrRecv, recvName := v.isPointerReceiver()

	return isPtrRecv && v.identResolvesToReceiver(ident, recvName)
}

func getParameters(signature *types.Signature, addRecv bool) *types.Tuple {
	var parameters *types.Tuple

	if addRecv && signature.Recv() != nil {
		// Concatenate receiver parameter with the rest of the parameters
		parameterVars := make([]*types.Var, 0, 1+signature.Params().Len())
		parameterVars = append(parameterVars, signature.Recv())

		for i := 0; i < signature.Params().Len(); i++ {
			parameterVars = append(parameterVars, signature.Params().At(i))
		}

		parameters = types.NewTuple(parameterVars...)
	} else {
		parameters = signature.Params()
	}

	return parameters
}

func (v *Visitor) generateParametersSignature(signature *types.Signature, addRecv bool) (string, string) {
	parameters := getParameters(signature, addRecv)

	if parameters == nil {
		return "", ""
	}

	result := strings.Builder{}
	var receiverAccess string
	dupBlankParams := hasDuplicateBlankParams(parameters) || bodyUsesBlankDiscard(v.currentFuncDecl)

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i == 0 && addRecv && signature.Recv() != nil {
			result.WriteString("this ")

			// Get receiver parameter type
			recvTypeName := v.getRefParameterTypeName(param.Type())

			// Update function access to match receiver type. A PUBLICIZED unexported receiver
			// type is emitted `public`, so it does not restrict the method's access.
			receiverAccess = getAccess(recvTypeName)

			if receiverAccess != "public" && receiverTypeIsPublicized(param.Type()) {
				receiverAccess = "public"
			}

			result.WriteString(v.getRefParameterTypeName(param.Type()))
			result.WriteRune(' ')

			paramName := param.Name()

			if paramName == "" || paramName == "_" {
				if dupBlankParams {
					paramName = fmt.Sprintf("_Δp%d", i)
				} else {
					paramName = "_"
				}
			} else if v.paramNeedsHeapBox(param) {
				// A heap-boxed VALUE receiver arrives under the `ʗp` name, exactly like a
				// heap-boxed value parameter below; the entry-time preamble re-declares the
				// analyzed name as the boxed ref alias. In practice the box also makes
				// visitFuncDecl rebuild this signature (paramHeapBoxes is non-empty), which
				// applies the same rename — keeping both paths in agreement is what makes a
				// future non-rebuilt path safe. getHeapBoxParamName already sanitizes.
				result.WriteString(getHeapBoxParamName(param))
				continue
			}

			result.WriteString(getSanitizedIdentifier(paramName))
			continue
		}

		if i > 0 {
			result.WriteString(", ")
		}

		if i == parameters.Len()-1 && signature.Variadic() {
			result.WriteString("params ")

			// If parameter is a slice, convert it to a Span
			if sliceType, ok := param.Type().(*types.Slice); ok {
				result.WriteString(v.variadicParamType(sliceType.Elem()))
			} else {
				result.WriteString("object[]")
			}

			// Variadic parameters are passed as C# param arrays, so we use a temporary
			// parameter name that will be later converted to a Go slice<T>
			result.WriteRune(' ')
			result.WriteString(getVariadicParamName(param))
		} else {
			paramTypeName := v.getCSharpTypeName(param.Type())

			// A fixed-size-array parameter carries its Go DIMENSION as an attribute: `array<T>`
			// has no const generic to hold it and a type-only position has no value or field
			// initializer to recover it from, so without this the reflection bridge answers a
			// dims-less array for reflect.TypeOf(f).In(i) — testing/quick then generates a
			// ZERO-length array for a `[32]byte` parameter. See emitGoArrayDimsAttribute.
			// One emission point serves all three signature builders (declarations/methods here,
			// func literals and func types through convFuncType, interface methods through
			// visitInterfaceType), so a Go func type's parameter dims are stated wherever its
			// C# shape is written.
			// A func-literal parameter at a CONSTRAINT-PROXY delegate position is DECLARED at the
			// proxy under a synthesized incoming name; the literal's body prologue re-declares the
			// Go name at this natural type (see constraintProxyLitParamTypes). Handled here rather
			// than inside getCSharpTypeName because only the DECLARATION moves — every other
			// rendering of the same Go type, the body's own uses included, stays as it was.
			if proxyType, ok := v.funcLitProxyParamTypes[param.Name()]; ok {
				result.WriteString(proxyType)
				result.WriteRune(' ')
				result.WriteString(getConstraintProxyLitParamName(param.Name()))
				continue
			}

			result.WriteString(emitGoArrayDimsAttribute(param.Type()))

			// A FUNC-LITERAL parameter typed as a `string | []byte`-union TYPE PARAMETER
			// renders as the type parameter itself (`(T part) => ...`): the enclosing
			// method's type parameter is in scope inside a lambda, and every union-typed
			// argument renders T-typed too (a sub-slice IS the type parameter — golib's
			// self-referential IByteSeq<TSelf, T> Range indexer, see convSliceExpr), so the
			// inferred delegate binds exactly and the emission matches the Go. (A historical
			// IByteSeq<byte> widening here worked around interface-typed sub-slice arguments -
			// obsolete once the indexer returned TSelf, and it hid the type identity:
			// user-flagged.)
			result.WriteString(paramTypeName)
			result.WriteRune(' ')

			paramName := param.Name()

			// Keep a lone `_`, but synthesize a unique placeholder when blanks would collide
			// (Go allows repeated blank params; C# forbids duplicate parameter names — CS0100).
			if paramName == "" || paramName == "_" {
				if dupBlankParams {
					paramName = fmt.Sprintf("_Δp%d", i)
				} else {
					paramName = "_"
				}
			} else if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
				// A parameter whose name was shadow-renamed by the variable analysis — because it
				// shadows an imported package, a called builtin, a function, or an outer-scope var —
				// must emit the RENAMED name so it matches every usage, which convIdent renders from
				// v.varNames. crypto/rsa's `func emsaPSSEncode(…, hash hash.Hash)` param `hash` shadows
				// the `hash` package (`using hash = hash_package;`); the declaration kept the raw
				// `hash` while its uses rendered `hashΔ1`, so every use was CS0103 (40 sites in
				// crypto/rsa, 27 in testing/quick's `rand`). Mirrors iifeParamName's lookup.
				paramName = adjusted
			}

			// A heap-boxed value parameter arrives under the `ʗp` name; the parameter preamble
			// re-declares the analyzed name as the boxed ref alias (see paramNeedsHeapBox). This
			// path serves a function with NO pointer params (otherwise the rebuilt-signature
			// path above applies the same rename). A FUNCTION LITERAL's boxed params flow in
			// through the transient name set instead — its signature is generated from
			// SYNTHESIZED vars (see getSignature) that never match the identEscapesHeap entries.
			if v.paramNeedsHeapBox(param) || v.funcLitHeapBoxParamNames.Contains(param.Name()) {
				result.WriteString(getHeapBoxParamName(param))
			} else {
				result.WriteString(getSanitizedIdentifier(paramName))
			}
		}
	}

	return result.String(), receiverAccess
}

func (v *Visitor) generateResultSignature(signature *types.Signature) string {
	results := signature.Results()

	if results == nil {
		return "void"
	}

	result := strings.Builder{}

	if results.Len() == 1 {
		param := results.At(0)

		result.WriteString(v.getCSharpTypeName(param.Type()))

		if param.Name() != "" {
			result.WriteString(" /*")
			result.WriteString(param.Name())
			result.WriteString("*/")
		}

		return result.String()
	}

	result.WriteRune('(')

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result.WriteString(", ")
		}

		param := results.At(i)

		result.WriteString(v.getCSharpTypeName(param.Type()))

		// A BLANK Go result name (`func match(x, y Value) (_, _ Value)`, go/constant) must NOT
		// become a C# tuple element name — two `_` elements collide (CS8127). Emit the type
		// only; C# permits a mixed named/unnamed tuple, so real names are still kept.
		if param.Name() != "" && param.Name() != "_" {
			result.WriteRune(' ')
			result.WriteString(getSanitizedIdentifier(param.Name()))
		}
	}

	result.WriteRune(')')

	return result.String()
}

func getVariadicParamName(param *types.Var) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(param.Name()), CapturedVarMarker)
}

// variadicParamIsUnreferenceable reports whether a variadic parameter's unpacked `slice<T>` local
// must NOT be emitted. A variadic parameter that is UNNAMED (`func cmdPipeTest(...string)` — Go
// permits omitting the name entirely) or BLANK (`func f(_ ...int)`) cannot be referenced from the
// body under Go's own rules, so the local is dead by construction. Emitting it anyway is not merely
// redundant, it is broken, in two distinct ways:
//
//   - UNNAMED: the local inherits the absent Go name, so the declaration comes out with an EMPTY
//     identifier — `var  = ʗp.slice();`, which the C# parser reads as an assignment to a nonexistent
//     `var` (CS0103 ×3 in os/exec's converted test sources, the wall that held that package in front
//     of the TestMain flag bridge). Inside a FUNCTION LITERAL it is worse still: the literal's
//     signature builder normalizes the absent name to `_` and emits `params ꓸꓸꓸnint _ʗp`, while the
//     prologue keeps rendering `ʗp` from the raw name — an empty declared name AND a signature
//     mismatch on the same line.
//   - BLANK: `var _ = _ʗp.slice();` compiles, but it declares a REAL local named `_` (a plain
//     `var _ = e;` declaration is a variable, not a discard), which then hijacks every `_ = …`
//     discard the body writes — the same CS0029 class bodyUsesBlankDiscard exists to prevent for a
//     blank PARAMETER name.
//
// Skipping is the same ruling, for the same reason, that an unnamed/blank POINTER parameter's deref
// alias already takes (see the `param.Name() == "" || param.Name() == "_"` skip in visitFuncDecl's
// implicit-pointer loop, which exists because the deref would emit `ref var  = ref Ꮡ.Value;`). The
// signature is untouched either way: the `params` array keeps its own `ʗp` name and simply goes
// unread, exactly as the Go parameter does.
func variadicParamIsUnreferenceable(param *types.Var) bool {
	name := param.Name()
	return name == "" || name == "_"
}

// getHeapBoxParamName returns the incoming-parameter name for a heap-boxed value parameter
// (see paramNeedsHeapBox) — the same `ʗp` rename convention as a variadic parameter: both
// re-declare the Go parameter name in the function prologue. A parameter is never both
// (a variadic parameter's unnamed []T type carries no capture-mode methods).
func getHeapBoxParamName(param *types.Var) string {
	return getVariadicParamName(param)
}

// getHeapBoxLitParamName renders the incoming `ʗp` parameter name for a heap-boxed value
// parameter of a FUNCTION LITERAL from its rendered body name (getHeapBoxParamName serves
// declaration parameters via their *types.Var; a literal's signature is generated from
// SYNTHESIZED vars — see getSignature — that already carry the rendered name).
func getHeapBoxLitParamName(renderedName string) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(renderedName), CapturedVarMarker)
}

// getConstraintProxyLitParamName renders the incoming name for a FUNCTION-LITERAL parameter
// declared at a CONSTRAINT-PROXY type (see constraintProxyLitParamTypes). The Go name is re-declared
// from it in the literal's body prologue, so the incoming name must not collide with it — the
// shadow marker keeps them distinct while leaving the Go name readable in the emitted signature's
// vicinity, matching the `ʗp` convention the heap-box parameter beside it uses for the same reason.
func getConstraintProxyLitParamName(renderedName string) string {
	return fmt.Sprintf("%s%sp", getSanitizedIdentifier(renderedName), ShadowVarMarker)
}

// paramNeedsHeapBox reports whether the value parameter — or a method's value RECEIVER, parameter 0
// in getParameters' model — needs an entry-time heap box: its own
// address is taken (`&r` / `&r.field` / `&r[i]` — image/draw's `DrawMask(…, r image.Rectangle, …)`
// calling `clip(dst, &r, …)`), or the body calls a capture-mode (direct-ж) method on it, whose
// only emitted receiver form is the box `ж<T>` — go/format's `cfg printer.Config` +
// `cfg.Fprint(…)`, CS1929 ×2 without it.
// The signature takes the incoming value under the `ʗp` name and the parameter preamble
// declares `ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);` — ENTRY-TIME boxing, never a
// call-site Ꮡ(value) copy-box: the copy form compiles but silently drops the callee's writes
// through the receiver pointer for the rest of the body (Go auto-addresses the same storage).
// The identHasHeapBox gate keeps the box decision in lockstep with the isHeapBoxedExpr
// routing in convSelectorExpr, but is NOT sufficient by itself: a parameter can land in
// identEscapesHeap outside markCaptureModeBoxedParams — a mixed `data, pc, line := …` define
// re-uses the param object, so the define walker escape-analyzes it (debug/gosym's slice,
// whose `pc` is stored into a composite literal). Those params keep their historical unboxed
// emission; the box fires only for the capture-mode trigger, re-verified here against the
// declaring ident, or for a param the capture analysis routed to SHARED storage: one WRITTEN
// after a closure captured it (see processPotentialCapture's varShareFacts arm) is referenced
// through its box inside every capturing lambda (`Ꮡctx.ValueSlot`), so the prologue must
// declare that box — database/sql beginDC's `ctx` (redeclared by a body-top-level
// `ctx, cancel := …` after withLock's closure captured it) and go/types nify's `x, y`
// (swapped after the trace defer captured them) rendered a box that was never declared
// (CS0103). The box-ref check rides the declaring-ident lookups below so a box-ref'd value
// RECEIVER (never `ʗp`-renamed by the signature paths) can never take the param form.
func (v *Visitor) paramNeedsHeapBox(param *types.Var) bool {
	if param == nil || param.Name() == "" || param.Name() == "_" {
		return false
	}

	if _, isPointer := param.Type().(*types.Pointer); isPointer {
		return false
	}

	if !v.identHasHeapBox(param, param.Type()) {
		return false
	}

	funcDecl := v.currentFuncDecl

	if funcDecl == nil || funcDecl.Body == nil {
		return false
	}

	// The method's VALUE RECEIVER is parameter 0 (getParameters concatenates it), but it is
	// declared on Recv — the params walk below never finds it. Its box reason set is narrower
	// than a parameter's (see recvBoxReasonHolds), so it takes its own gate.
	if funcDecl.Recv != nil {
		for _, field := range funcDecl.Recv.List {
			for _, ident := range field.Names {
				if v.info.ObjectOf(ident) == param {
					return v.recvBoxReasonHolds(param, funcDecl.Body)
				}
			}
		}
	}

	if funcDecl.Type.Params == nil {
		return false
	}

	for _, field := range funcDecl.Type.Params.List {
		for _, ident := range field.Names {
			if v.info.ObjectOf(ident) == param {
				return v.paramBoxReasonHolds(ident, param, funcDecl.Body)
			}
		}
	}

	// A FUNCTION LITERAL's own value parameter (whose prologue/rename convFuncLit emits — see
	// funcLitHeapBoxParamIdents): the analysis phase reaches here when a NESTED closure inside
	// the literal references the param, and the box-ref arm of processPotentialCapture must see
	// the same verdict emission uses. Find the declaring literal within the current declaration
	// and re-verify against ITS body.
	needsBox := false

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		funcLit, ok := n.(*ast.FuncLit)

		if !ok || funcLit.Type.Params == nil {
			return true
		}

		for _, field := range funcLit.Type.Params.List {
			if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
				continue
			}

			for _, ident := range field.Names {
				if v.info.ObjectOf(ident) == param {
					needsBox = v.paramBoxReasonHolds(ident, param, funcLit.Body)
					return false
				}
			}
		}

		return true
	})

	return needsBox
}

func (v *Visitor) getTempVarName(varPrefix string) string {
	if v.tempVarCount == nil {
		v.tempVarCount = make(map[string]int)
	}

	count := v.tempVarCount[varPrefix]
	count++
	v.tempVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, TempVarMarker, count)
}

// linknameForwardTargets is the whitelist of cross-package //go:linkname PULL targets the converter
// emits a forwarder body for — the specific NATIVE functions hand-implemented in the converted
// standard library (syscall's Windows DLL loaders, reached by golang.org/x/sys/windows's LazyDLL /
// LazyProc). A linkname target is INDISTINGUISHABLE at conversion time from any other bodyless
// assembly/intrinsic Go function — syscall.loadlibrary and runtime.reflectcall are both bodyless asm
// in Go — so forwarding is gated on this explicit set: only these have a real C# implementation to
// call. Every other linkname pull (a method-receiver PUSH like reflect's badlinkname.go, a
// same-package pull, or an unimplemented intrinsic like runtime.reflectcall) stays a bodyless stub,
// the pre-forwarder behavior. Extend this set when a new native linkname target gains a hand-written
// C# implementation.
var linknameForwardTargets = map[string]bool{
	"syscall.loadlibrary":       true,
	"syscall.loadsystemlibrary": true,
	"syscall.getprocaddress":    true,
	// runtime's finalizer-queue drain, pulled by sync's and runtime's own tests
	// (`//go:linkname runtime_blockUntilEmptyFinalizerQueue runtime.blockUntilEmptyFinalizerQueue`).
	// The converted runtime carries a real managed body for it — the finalizer bridge in
	// mfinal.cs waits on GC.WaitForPendingFinalizers — so the pull has something to call.
	"runtime.blockUntilEmptyFinalizerQueue": true,
	// time's embedded-zoneinfo registration, pulled by time/tzdata's `init()`
	// (`//go:linkname registerLoadFromEmbeddedTZData time.registerLoadFromEmbeddedTZData`). The
	// FIRST entry whose implementation is ORDINARY CONVERTED Go rather than a hand-written native
	// or golib body: `time/zoneinfo_read.go` declares it with a real body, and `time` authorizes
	// the pull with the matching one-arg handle, so packageFuncAccess emits it public and the
	// forwarder is an ordinary cross-assembly call. Without it, tzdata's `init()` — which a blank
	// `import _ "time/tzdata"` now genuinely runs — threw out of a module initializer and took the
	// whole program down before `main` (time's own tzdata_test.go is the first consumer). The pull
	// adds a `time` project reference to time/tzdata, which is acyclic: time imports no subpackage
	// of itself.
	"time.registerLoadFromEmbeddedTZData": true,
	// runtime's fcntl, pulled by internal/syscall/unix's fcntl_unix.go
	// (`//go:linkname fcntl runtime.fcntl` over a bodyless `func fcntl(fd, cmd, arg int32) (int32,
	// int32)`), and authorized by the matching one-arg handle in runtime/linkname_unix.go. Like the
	// tzdata entry above, the implementation is ORDINARY CONVERTED Go rather than a hand-written
	// native body — runtime/os_linux.go's fcntl is three lines over
	// internal/runtime/syscall.Syscall6, which is real on Linux now that the keystone P/Invoke
	// exists — so the forwarder is an ordinary cross-assembly call to something that genuinely
	// works.
	//
	// This is the first entry the LINUX flavor needed, and it sits on the very first file any
	// program opens: os.NewFile calls unix.Fcntl(fd, F_GETFL, 0) to learn whether a descriptor is
	// non-blocking, and os.init()'s initStdin runs that for stdin before main. Without the row the
	// stub threw from os_package's type initializer, so `fmt.Println` could not run at all. The
	// Windows corpus never surfaced it: fcntl_unix.go is `//go:build unix`, so the declaration does
	// not exist there.
	//
	// The pull adds a `runtime` project reference to internal/syscall/unix, which is acyclic —
	// runtime does not import internal/syscall/unix, directly or transitively.
	"runtime.fcntl": true,
	// textproto's size-limited MIME header reader, pulled by mime/multipart's readmimeheader.go
	// (`//go:linkname readMIMEHeader net/textproto.readMIMEHeader` over a bodyless declaration) and
	// authorized by the matching one-arg handle in net/textproto/reader.go:506, whose own comment
	// says it "is called by the mime/multipart package". Like the tzdata and fcntl entries the
	// implementation is ORDINARY CONVERTED Go — reader.cs carries the full body, the same one
	// `ReadMIMEHeader` itself delegates to with MaxInt64 limits — so the forwarder is an ordinary
	// cross-assembly call to something that genuinely works. The pull needs no new project
	// reference: multipart already imports net/textproto for the `*textproto.Reader` parameter.
	//
	// What the stub was costing: EVERY part-reading path in mime/multipart bottoms out here
	// (populateHeaders -> newPart -> nextPart, and ReadForm through it), so the throw reached 41 of
	// the package's 52 verdicts as infrastructure-error and four more as parent-test shadows —
	// the package measured 7 of 52 with this single symbol as the whole differential.
	"net/textproto.readMIMEHeader": true,
	// go/types' cgo-mode switch, pulled by go/internal/srcimporter's srcimporter.go
	// (`//go:linkname setUsesCgo go/types.srcimporter_setUsesCgo` over a bodyless declaration) and
	// authorized by the matching one-arg handle in go/types/api.go, whose own comment says
	// "Linkname for use from srcimporter." The implementation is ORDINARY CONVERTED Go — one field
	// assignment (`conf.go115UsesCgo = true`) — so the forwarder is an ordinary cross-assembly
	// call. The pull needs no new project reference: srcimporter already imports go/types for the
	// `*types.Config` parameter itself.
	//
	// What the stub was costing on Linux (the first flavor whose exec seam lets srcimporter run at
	// all): Importer.ImportFrom calls setUsesCgo unconditionally before type-checking, so every
	// import bottomed out in the announcing stub — three of the package's seven verdicts, and
	// go/types' TestStdFixed through the same path.
	"go/types.srcimporter_setUsesCgo": true,
}

// linknameForwardBuiltins is the whitelist of cross-package //go:linkname PULL targets whose
// implementation is a golib BUILTIN — a compiler intrinsic Go defines in the runtime and links
// into another package by symbol, for which golib carries the real C# implementation. The map
// value is the golib builtin's C# name; it is in scope UNQUALIFIED via `using static go.builtin`,
// so the forwarder emits a bare `<builtin>(args)` call (an empty package alias signals this). The
// canonical case is maps.clone — Go implements it as runtime.mapclone (`//go:linkname mapclone
// maps.clone`) and the maps package pulls it as a bodyless `func clone(m any) any`; golib's
// builtin.mapclone returns a shallow, independent clone of the boxed map. Extend this set when a
// new linkname intrinsic gains a golib builtin implementation.
var linknameForwardBuiltins = map[string]string{
	"maps.clone": "mapclone",
}

// funcLinknameForward recognizes a bodyless function carrying a `//go:linkname <thisFunc>
// <pkgpath>.<targetFunc>` directive whose target is a hand-implemented native function
// (linknameForwardTargets). It returns the C# alias for the target package (the last path segment,
// the `using <name> = <name>_package;` alias the importing file emits) and the target function name,
// so the converter can emit a forwarder call to it instead of a throwing stub.
func (v *Visitor) funcLinknameForward(funcDecl *ast.FuncDecl) (alias string, targetFunc string, ok bool) {
	if funcDecl.Doc == nil || funcDecl.Name == nil {
		return "", "", false
	}

	for _, comment := range funcDecl.Doc.List {
		fields := strings.Fields(comment.Text)

		// //go:linkname <local> <pkgpath>.<func>
		if len(fields) != 3 || fields[0] != "//go:linkname" || fields[1] != funcDecl.Name.Name {
			continue
		}

		target := fields[2]

		// A linkname target implemented as a golib BUILTIN — in scope unqualified via
		// `using static go.builtin`, so the forwarder emits a bare `<builtin>(args)` call. The
		// empty alias is the sentinel writeLinknameForwarder reads to omit the package qualifier
		// (maps.clone → mapclone, Go's runtime.mapclone shallow-clone intrinsic).
		if builtin, isBuiltin := linknameForwardBuiltins[target]; isBuiltin {
			return "", builtin, true
		}

		if !linknameForwardTargets[target] {
			return "", "", false
		}

		dot := strings.LastIndex(target, ".")

		return v.linknameTargetAlias(target[:dot]), getSanitizedFunctionName(target[dot+1:]), true
	}

	return "", "", false
}

// linknameTargetAlias returns the qualifier a linkname forwarder must spell to reach pkgPath's
// package class from THIS file, and queues pkgPath for a project reference.
//
// The C# using-alias is whatever THIS file actually emitted for the target package — never the bare
// last path segment. When the package name collides with a namespace segment the collision analysis
// renames the alias (sync's oncefunc_test.go emits `using Δruntime = runtime_package;` because the
// bare `runtime` would bind the `go.runtime` NAMESPACE), and a forwarder spelled `runtime.<fn>` is
// then CS0234. An explicit `import r "runtime"` is recorded in importPathAliases; otherwise the
// canonical alias (which importQualifier has already renamed if needed) is what visitImportSpec
// emitted.
//
// A linkname edge can be the ONLY edge to the target package — `time/tzdata` imports errors, syscall
// and unsafe, never `time`, yet its `init()` calls `time.registerLoadFromEmbeddedTZData`. Queue the
// path so the PROJECT reference is emitted; for a package the file already imports this is a no-op.
// (The var-pull arm does the same — see visitValueSpec's varLinknamePull site.)
func (v *Visitor) linknameTargetAlias(pkgPath string) string {
	v.importQueue.Add(pkgPath)

	if emitted, ok := v.importPathAliases[pkgPath]; ok && emitted != "" {
		return emitted
	}

	// No IMPORT SPEC means no `using` alias in this file, so the call must be fully qualified —
	// the same form the var pull's forwarding property uses, which resolves inside `namespace go;`
	// with no alias at all. A bare `time.` here would bind the `go.time` CHILD NAMESPACE (tzdata's
	// own), not the package class: CS0234.
	if !v.canonicalAliasImported.Contains(pkgPath) {
		return globalQualifyRooted(RootNamespace + "." + convertImportPathToNamespace(pkgPath, PackageSuffix))
	}

	alias, _ := packageUsingAlias(pkgPath)

	return getSanitizedImport(importQualifier(alias))
}

// funcLinknamePush recognizes the CONSUMING side of a `//go:linkname` PUSH: a bodyless function
// under a one-arg `//go:linkname <thisFunc>` handle whose body another package supplies by naming
// it in a two-arg directive (`//go:linkname unique_runtime_registerUniqueMapCleanup
// unique.runtime_registerUniqueMapCleanup` in runtime/mgc.go). The handle is Go's authorization for
// the push and is required here, exactly as the pull arm requires the remote's handle.
//
// The push's disposition comes from linknamePushTargets, keyed by this declaration's own
// fully-qualified name — the converter cannot read the pushing package's directives while
// converting the consumer, and a bodyless one-arg-handle func is otherwise indistinguishable from
// an ordinary assembly stub. A LINKED entry returns the alias and name of the pushing definition,
// which packageFuncAccess has emitted public on the other side. An UNHONORABLE entry returns the
// recorded reason, and the caller emits a panicking stub naming both halves of the pair.
func (v *Visitor) funcLinknamePush(funcDecl *ast.FuncDecl) (alias string, targetFunc string, reason string, ok bool) {
	if funcDecl.Name == nil {
		return "", "", "", false
	}

	push, isPushed := linknamePushTargets[currentPackagePath+"."+funcDecl.Name.Name]

	if !isPushed {
		return "", "", "", false
	}

	// The declaration's own syntax must match the consumer shape the registry row records.
	if !linknamePushDeclMatches(funcDecl, push.bareDecl) {
		return "", "", "", false
	}

	if push.reason != "" {
		return "", "", fmt.Sprintf("//go:linkname push %s -> %s.%s is not honored: %s", push.source, currentPackagePath, funcDecl.Name.Name, push.reason), true
	}

	dot := strings.LastIndex(push.source, ".")

	return v.linknameTargetAlias(push.source[:dot]), getSanitizedFunctionName(push.source[dot+1:]), "", true
}

// linknamePushDeclMatches reports whether a bodyless declaration's own syntax is the consumer shape
// its registry row records — Go's standard library pushes into TWO shapes and they are distinguished
// only by what the consumer writes above itself:
//
//   - the HANDLE shape (bareDecl false): a one-arg `//go:linkname <thisFunc>` directive, Go 1.23's
//     way of opening a symbol to a push (unique.runtime_registerUniqueMapCleanup);
//   - the BARE shape (bareDecl true): no `//go:linkname` directive at all, just a bodyless func and
//     usually a prose comment saying where the body lives (`func runtime_envs() []string // in
//     package runtime`). It predates the handle convention and is what syscall and os still use.
//
// Requiring each row to match its recorded shape is what keeps this fail-closed: a mis-keyed row
// cannot quietly forward some unrelated bodyless declaration that happens to share the name, and a
// consumer carrying a two-arg directive — which is a PULL, a different mechanism entirely — is
// rejected by both arms rather than being mistaken for an unadorned bare declaration.
func linknamePushDeclMatches(funcDecl *ast.FuncDecl, bareDecl bool) bool {
	hasHandle, hasDirective := false, false

	if funcDecl.Doc != nil {
		for _, comment := range funcDecl.Doc.List {
			fields := strings.Fields(comment.Text)

			if len(fields) == 0 || fields[0] != "//go:linkname" {
				continue
			}

			hasDirective = true

			// //go:linkname <thisFunc> — the one-arg handle, i.e. this declaration is open to a push.
			if len(fields) == 2 && fields[1] == funcDecl.Name.Name {
				hasHandle = true
			}
		}
	}

	if bareDecl {
		return !hasDirective
	}

	return hasHandle
}

// writeLinknamePanicStub emits the body of a `//go:linkname` push whose pushed body the managed
// model cannot honor: a `throw panic(...)` naming the pair and the reason. It is deliberately a Go
// PANIC rather than the PartialStubGenerator's NotImplementedException — the declaration is not an
// unimplemented assembly stub, it is a real Go contract this conversion has decided it cannot keep,
// and the message has to say which linkname pair and why so the first caller to hit it lands on the
// hand-own the row actually needs.
func (v *Visitor) writeLinknamePanicStub(reason string) {
	savedIndent := v.indentLevel
	v.indentLevel = 0

	v.writeOutputLn(" {%s%sthrow panic(\"go2cs: %s\");%s%s}", v.newline, v.indent(savedIndent+1), reason, v.newline, v.indent(savedIndent))

	v.indentLevel = savedIndent
}

// linknameForwardArgName returns the C# identifier a parameter was emitted under in the forwarder's
// signature, so the forwarder can pass it through to the target. It mirrors the naming decisions of
// generateParametersSignature (blank/duplicate-blank synthesis, variable-analysis shadow renames,
// heap-box aliasing, variadic naming) so the argument matches the declared parameter exactly.
func (v *Visitor) linknameForwardArgName(param *types.Var, i int, dupBlank bool, variadic bool) string {
	if variadic {
		return getVariadicParamName(param)
	}

	paramName := param.Name()

	if paramName == "" || paramName == "_" {
		if dupBlank {
			return fmt.Sprintf("_%sp%d", ShadowVarMarker, i)
		}

		return "_"
	}

	if adjusted, ok := v.varNames[param]; ok && adjusted != "" {
		paramName = adjusted
	}

	if v.paramNeedsHeapBox(param) || v.funcLitHeapBoxParamNames.Contains(param.Name()) {
		return getHeapBoxParamName(param)
	}

	return getSanitizedIdentifier(paramName)
}

// writeLinknameForwarder emits the body of a cross-package //go:linkname forwarder: a call to
// `<alias>.<targetFunc>(args)` with the local parameters passed through and the results returned.
// Because the target and local signatures are linkname-compatible (structurally identical Go
// types), any nominal C# type difference is between `num:uintptr`-kind types on both sides, so a
// mismatch is bridged through `uintptr` — an integer/uintptr parameter is passed `(uintptr)p`, and
// an integer/uintptr result is returned `(LocalType)(uintptr)r`. Non-integer params/results
// (pointers, slices, strings) are the same golib type on both sides and pass through directly.
func (v *Visitor) writeLinknameForwarder(signature *types.Signature, alias string, targetFunc string) {
	params := signature.Params()
	dupBlank := hasDuplicateBlankParams(params) || bodyUsesBlankDiscard(v.currentFuncDecl)
	args := make([]string, params.Len())

	for i := 0; i < params.Len(); i++ {
		param := params.At(i)
		name := v.linknameForwardArgName(param, i, dupBlank, signature.Variadic() && i == params.Len()-1)

		if v.isUintptrBridgeable(param.Type()) {
			name = "(uintptr)" + name
		}

		args[i] = name
	}

	// An empty alias signals a golib-builtin target (in scope unqualified via `using static
	// go.builtin`); any other alias is the target package's using-alias (`syscall.loadlibrary`).
	call := fmt.Sprintf("%s(%s)", targetFunc, strings.Join(args, ", "))

	if alias != "" {
		call = fmt.Sprintf("%s.%s(%s)", alias, targetFunc, strings.Join(args, ", "))
	}

	results := signature.Results()

	savedIndent := v.indentLevel
	v.indentLevel = 0

	bodyIndent := v.indent(savedIndent + 1)
	closeIndent := v.indent(savedIndent)

	var body strings.Builder
	body.WriteString(" {")
	body.WriteString(v.newline)

	switch results.Len() {
	case 0:
		body.WriteString(fmt.Sprintf("%s%s;", bodyIndent, call))
	case 1:
		body.WriteString(fmt.Sprintf("%sreturn %s;", bodyIndent, v.bridgeLinknameResult(call, results.At(0).Type())))
	default:
		names := make([]string, results.Len())
		bridged := make([]string, results.Len())

		for i := 0; i < results.Len(); i++ {
			names[i] = fmt.Sprintf("%s%d", TempVarMarker, i+1)
			bridged[i] = v.bridgeLinknameResult(names[i], results.At(i).Type())
		}

		body.WriteString(fmt.Sprintf("%svar (%s) = %s;", bodyIndent, strings.Join(names, ", "), call))
		body.WriteString(v.newline)
		body.WriteString(fmt.Sprintf("%sreturn (%s);", bodyIndent, strings.Join(bridged, ", ")))
	}

	body.WriteString(v.newline)
	body.WriteString(closeIndent)
	body.WriteString("}")

	v.writeOutputLn(body.String())
	v.indentLevel = savedIndent
}

// bridgeLinknameResult wraps a target-call result expression so it matches the local result type:
// an integer/uintptr result is cast through `uintptr` (`(LocalType)(uintptr)expr`), covering the
// nominal difference between two `num:uintptr` types across the linkname; any other type is the
// same golib type on both sides and passes through unchanged.
func (v *Visitor) bridgeLinknameResult(expr string, localType types.Type) string {
	if v.isUintptrBridgeable(localType) {
		return fmt.Sprintf("(%s)(uintptr)%s", v.getCSharpTypeName(localType), expr)
	}

	return expr
}

// isUintptrBridgeable reports whether t is an integer/uintptr-kind type — one that converts to and
// from `uintptr`, so a linkname forwarder can bridge a nominal mismatch between two such types.
func (v *Visitor) isUintptrBridgeable(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, ok := t.Underlying().(*types.Basic)

	// UINTPTR-KIND only, not every integer. The bridge exists because a linkname pair can name
	// the same uintptr-shaped value under two different NOMINAL types (x/sys/windows's `Handle`
	// / `Errno` vs syscall's `uintptr`), which C# will not convert implicitly. A SIZED integer
	// (int32/int64/…) is the same C# type on both sides and needs no bridge — casting it through
	// uintptr instead narrows it on 32-bit and does not even bind (`(uintptr)timeout` handed to
	// an `int64` parameter, sync's runtime.blockUntilEmptyFinalizerQueue pull, CS1503).
	return ok && basic.Kind() == types.Uintptr
}
