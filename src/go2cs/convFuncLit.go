// convFuncLit.go - Gbtc
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

// numericBasicLit returns the (optionally sign-prefixed) INT or FLOAT basic literal behind
// expr, if any — the numeric cousin of isStringBasicLit, serving the func-lit inference scans
// (an untyped numeric constant arm emits bare, so it infers the literal's natural C# type
// instead of the declared result's).
//
// Both signs are stripped. Go writes an explicitly POSITIVE literal wherever it pairs with a
// negative one, which is exactly the comparator shape these scans exist for — crypto/tls's
// `isBetter` returns `-1` and `+1` — and `+1` emits identically to `1`, so treating it as
// anything other than a numeric literal only blinds the caller to half of its own arm set.
func numericBasicLit(expr ast.Expr) (*ast.BasicLit, bool) {
	if unary, ok := expr.(*ast.UnaryExpr); ok && (unary.Op == token.SUB || unary.Op == token.ADD) {
		expr = unary.X
	}

	lit, ok := expr.(*ast.BasicLit)

	if !ok || (lit.Kind != token.INT && lit.Kind != token.FLOAT) {
		return nil, false
	}

	return lit, true
}

// funcLitReturnArmTypes scans a function literal's OWN single-value return arms (a nested
// literal's returns belong to it), reporting the distinct arm types, whether any such arm exists
// at all, and whether EVERY one of them is the untyped nil.
//
// Extracted so both consumers can share it: the interface-result arm in convFuncLit, whose
// distinct-arm-types case it was written for, and the nilable-result arm beside it. Keeping one
// scan is what makes those two answer the same question the same way.
func (v *Visitor) funcLitReturnArmTypes(funcLit *ast.FuncLit) (armTypes []types.Type, hasSingleReturn bool, allArmsUntypedNil bool) {
	allArmsUntypedNil = true

	ast.Inspect(funcLit.Body, func(n ast.Node) bool {
		if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
			return false // a nested literal's returns belong to it
		}

		if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == 1 {
			hasSingleReturn = true

			if basic, isBasic := v.getType(ret.Results[0], false).(*types.Basic); !isBasic || basic.Kind() != types.UntypedNil {
				allArmsUntypedNil = false
			}

			if retType := v.getType(ret.Results[0], false); retType != nil {
				known := false

				for _, t := range armTypes {
					if types.Identical(t, retType) {
						known = true
						break
					}
				}

				if !known {
					armTypes = append(armTypes, retType)
				}
			}
		}

		return true
	})

	return armTypes, hasSingleReturn, allArmsUntypedNil
}

// funcLitAllReturnArmsAreUntypedNil reports whether the literal returns a single value and EVERY
// one of its own return arms is the untyped nil — the shape whose arms all render `default!`, so
// none contributes a natural type and C# cannot infer the delegate (CS8917).
func (v *Visitor) funcLitAllReturnArmsAreUntypedNil(funcLit *ast.FuncLit) bool {
	_, hasSingleReturn, allArmsUntypedNil := v.funcLitReturnArmTypes(funcLit)

	return hasSingleReturn && allArmsUntypedNil
}

// funcLitReturnsUntypedNamedConst reports whether any of the literal's OWN top-level return arms
// returns a bare reference to a named untyped numeric constant — OR a constant operator expression
// containing one that no literal fold rescues. Both shapes emit with a golib `Untyped*` wrapper
// (see isUntypedNamedConstRef) and defeat C# lambda return-type inference in natural-inference
// position (see the single-result numeric arm in convFuncLit): bytes TestMap's `invalidRune :=
// func(r rune) rune { return utf8.MaxRune + 1 }` renders the arm `utf8.MaxRune + 1`, whose C#
// operator result keeps the wrapper type, so the inferred delegate was `Func<int, UntypedInt>`
// (CS1503 at the invariant Map call) exactly like the bare-reference case. An arm the constant
// folds rewrite to a plain literal (overflowingConstLiteral / floatContextConstLiteral) emits
// concretely typed and is excluded, as are literal-only arms (`return 'a'`, `return -1`).
func (v *Visitor) funcLitReturnsUntypedNamedConst(funcLit *ast.FuncLit) bool {
	found := false

	ast.Inspect(funcLit.Body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
			return false // a nested literal's returns belong to it
		}

		if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == 1 && v.returnArmKeepsUntypedWrapper(ret.Results[0]) {
			found = true
			return false
		}

		return true
	})

	return found
}

// funcLitNumericArmsMisinfer reports whether EVERY one of the literal's OWN top-level single-result
// return arms is a numeric basic literal whose natural C# type differs from the declared basic
// result — the arm set on which C# infers a delegate the literal's Go signature does not describe
// (see the arm in convFuncLit that consumes it).
//
// The gate is narrow because it is keyed to what the converter actually EMITS, not to the literal's
// Go-side natural type — measured against real output, twice, after two wider cuts each proved to
// over-apply:
//
//   - An INT literal at a declared INTEGER width emits BARE (`return 9;` under an `int64` result),
//     so it is naturally C# `int`. That is the misinferring case, and the only one.
//   - The same INT literal at a declared FLOATING result does NOT emit bare — it takes the declared
//     width's suffix (`func() float64 { return 3 }` renders `3D`), so inference already lands on
//     `double`.
//   - A FLOAT literal likewise carries the declared width (`func() float32 { return 0.5 }` renders
//     `0.5F`, not `0.5`).
//
// So only a declared integer type OTHER than int32 can be misinferred; `int32`/`rune` IS the bare
// literal's own C# type. The `FuncLitUntypedConstReturn` guard carries all four shapes side by side
// so the split stays pinned to the emission rather than to this comment.
//
// Every other arm reports false, which is what keeps the predicate churn-free: an arm it cannot
// classify (any expression that is not an INT literal) is ASSUMED to carry the declared type, so a
// mixed arm set keeps its present emission. A bare `return` against named results does the same — it
// emits the declared-typed result variables. A literal with no single-result return arm at all
// reports false rather than vacuously true.
func (v *Visitor) funcLitNumericArmsMisinfer(funcLit *ast.FuncLit, declared *types.Basic) bool {
	hasArm := false
	allMisinfer := true

	ast.Inspect(funcLit.Body, func(n ast.Node) bool {
		if !allMisinfer {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
			return false // a nested literal's returns belong to it
		}

		ret, ok := n.(*ast.ReturnStmt)

		if !ok {
			return true
		}

		if len(ret.Results) == 0 {
			// A bare `return` against named results emits the declared-typed names.
			allMisinfer = false
			return false
		}

		if len(ret.Results) != 1 {
			return true
		}

		hasArm = true
		lit, isNumeric := numericBasicLit(ret.Results[0])

		// Anything but an INT literal at a declared INTEGER type other than int32 already infers
		// correctly — see the emission evidence in the doc comment.
		if !isNumeric || lit.Kind != token.INT || declared.Info()&types.IsInteger == 0 || declared.Kind() == types.Int32 {
			allMisinfer = false
			return false
		}

		return true
	})

	return hasArm && allMisinfer
}

// returnArmKeepsUntypedWrapper reports whether a single-result return arm's emission keeps a golib
// `Untyped*` wrapper type: a bare named untyped-const reference, or a CONSTANT paren/unary/binary
// operator expression containing one — unless a constant fold (overflowingConstLiteral /
// floatContextConstLiteral) rewrites the whole arm to a plain literal, which emits concretely typed.
func (v *Visitor) returnArmKeepsUntypedWrapper(expr ast.Expr) bool {
	if v.isUntypedNamedConstRef(expr) {
		return true
	}

	switch expr.(type) {
	case *ast.ParenExpr, *ast.UnaryExpr, *ast.BinaryExpr:
	default:
		return false
	}

	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return false
	}

	if v.overflowingConstLiteral(expr) != "" || v.floatContextConstLiteral(expr) != "" {
		return false
	}

	return v.constExprContainsUntypedNamedConstRef(expr)
}

// constExprContainsUntypedNamedConstRef reports whether a named untyped numeric constant reference
// appears as a leaf of the paren/unary/binary operator tree — the operand shape that renders as a
// golib `Untyped*` wrapper and makes the whole operator result wrapper-typed.
func (v *Visitor) constExprContainsUntypedNamedConstRef(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X)
	case *ast.UnaryExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X)
	case *ast.BinaryExpr:
		return v.constExprContainsUntypedNamedConstRef(e.X) || v.constExprContainsUntypedNamedConstRef(e.Y)
	default:
		return v.isUntypedNamedConstRef(expr)
	}
}

// localFunctionDefine reports whether assignStmt is the `name := func(…) {…}` shape that can be
// emitted as a C# LOCAL FUNCTION (`<result> name(<params>) { … }`) instead of a lambda bound to a
// variable, returning the declared identifier and the literal when it is.
//
// Why it matters: a C# lambda that captures anything allocates a display class to hold the captured
// variables AND a delegate object bound to it, on EVERY evaluation of the lambda expression — 88
// bytes for the two-word case, charged per call of the enclosing function. Go allocates neither when
// escape analysis proves the closure does not outlive the frame, which is why `time`'s
// `TestUnmarshalTextAllocations` asserts zero: `parseRFC3339`'s `parseUint := func(…)` captures `ok`
// and was costing exactly that on every parse. A C# local function that is only ever CALLED captures
// through a by-ref STRUCT closure the compiler passes as a hidden `ref` parameter — no heap object,
// same shared-storage semantics as the display class.
//
// The gate is the "only ever called" proof, and it is what keeps the by-ref struct closure available:
// Roslyn falls back to a heap display class the moment a local function is converted to a delegate,
// so a variable that is stored, returned, compared, or passed as a value must stay a lambda (and a
// local function has no VALUE form to give those uses anyway). The declaring occurrence is exempt;
// every other reference must be the callee of a call. That also subsumes reassignment (`f = …` is a
// non-call use) and address-taking, so the emitted name can never need to be a first-class value.
//
// A literal that uses defer or recover was excluded while its body was emitted inside a
// `func((defer, recover) => …)` execution context, whose GoFunc object, display class and per-defer
// delegates dominated the cost this rule removes — converting the outer binding alone would have
// churned goldens for no measurable win. The ref-struct frame (docs/phase4/DESIGN-closure-emission.md
// §4) made that shape free, so the exclusion is gone: such a literal is a local function whose body
// declares its own `GoFrame`, which is an ordinary local of one.
func (v *Visitor) localFunctionDefine(assignStmt *ast.AssignStmt, format FormattingContext) (*ast.Ident, *ast.FuncLit, bool) {
	// A local function is a DECLARATION statement: it is only legal in a statement list, never in
	// the init/post clause of a `for`, or an `if`/`switch` init, which is what !useNewLine marks.
	if !format.useNewLine || assignStmt.Tok != token.DEFINE || len(assignStmt.Lhs) != 1 || len(assignStmt.Rhs) != 1 {
		return nil, nil, false
	}

	funcLit, ok := assignStmt.Rhs[0].(*ast.FuncLit)

	if !ok {
		return nil, nil, false
	}

	ident, ok := assignStmt.Lhs[0].(*ast.Ident)

	if !ok || isDiscardedVar(ident.Name) {
		return nil, nil, false
	}

	// A mixed `f, err := …` re-use records the name in Uses, not Defs; only a genuine new
	// declaration binds the literal to a fresh object this statement owns.
	obj, ok := v.info.Defs[ident].(*types.Var)

	if !ok || obj == nil {
		return nil, nil, false
	}

	if v.currentFuncDecl == nil || !v.objectOnlyCalled(obj, v.currentFuncDecl) {
		return nil, nil, false
	}

	return ident, funcLit, true
}

// objectOnlyCalled reports whether every reference to obj within root — other than its declaring
// occurrence — is the callee identifier of a call expression. See localFunctionDefine for what the
// proof buys; the walk covers nested function literals because root is the whole declaration, so a
// closure that captures the name and uses it as a value is caught.
func (v *Visitor) objectOnlyCalled(obj types.Object, root ast.Node) bool {
	if obj == nil || root == nil {
		return false
	}

	callees := HashSet[*ast.Ident]{}

	ast.Inspect(root, func(node ast.Node) bool {
		if callExpr, ok := node.(*ast.CallExpr); ok {
			if ident, ok := callExpr.Fun.(*ast.Ident); ok {
				callees.Add(ident)
			}
		}

		return true
	})

	onlyCalled := true

	ast.Inspect(root, func(node ast.Node) bool {
		if !onlyCalled {
			return false
		}

		ident, ok := node.(*ast.Ident)

		if !ok || v.info.Uses[ident] != obj {
			return true
		}

		if !callees.Contains(ident) {
			onlyCalled = false
		}

		return true
	})

	return onlyCalled
}

func (v *Visitor) convFuncLit(funcLit *ast.FuncLit, context LambdaContext) string {
	if v.currentFuncSignature == nil {
		v.currentFuncSignature = v.info.Types[funcLit].Type.(*types.Signature)
		v.funcSignatureIsLiteralSeed = true
	}

	// A literal that defers or recovers gets a FRAME OF ITS OWN, and a frame's tail emits the
	// defer→finally lowered calls (deferFinallyLowering.go). Those calls belong to the ENCLOSING
	// function, so the literal's frame must not re-emit them — the flag is an ordinary local a
	// lambda can capture, so a leaked emission COMPILES and runs the call twice, once at the
	// literal's exit and once at the function's. The enclosing plan cannot rule the case out on its
	// own: funcBodyDeferRecover deliberately stops at a FuncLit, so a literal that only RECOVERS
	// leaves the enclosing hasRecover false while still earning a frame. Cleared for the literal's
	// whole emission and restored afterwards; a literal's own defers are never lowered, because
	// all-or-nothing already refuses any function whose defers are not its body's direct children.
	savedLoweredDefers, savedLoweredDeferIndex := v.loweredDefers, v.loweredDeferIndex
	v.loweredDefers, v.loweredDeferIndex = nil, nil

	defer func() {
		v.loweredDefers, v.loweredDeferIndex = savedLoweredDefers, savedLoweredDeferIndex
	}()

	litSig, _ := v.info.TypeOf(funcLit).(*types.Signature)

	// visitReturnStmt derives a bare `return`'s emitted RESULTS from the return signature. A nested
	// literal must return against ITS OWN results, not the enclosing function's — a bare `return` inside
	// a VOID closure otherwise gets the OUTER function's named results (`forEachGRace(func(gp1 *g) { …
	// return … })` inside a func returning named `(n, ok)` emitted `return (n, ok);` → CS8030). This is a
	// SEPARATE field from currentFuncSignature, which must stay the ENCLOSING function's signature so the
	// receiver/parameter detection (isBoxedPointerLocal, varIsDerefdPointerParam) still resolves a
	// CAPTURED pointer param (an outer parameter) correctly. Save/restore around the body.
	savedReturnSignature := v.currentReturnSignature

	if litSig != nil {
		v.currentReturnSignature = litSig
	}

	// Does THIS literal need its own func() execution context, and additionally the
	// named-return-defer handling (named results that deferred code, including recover, mutates)?
	// A deferred-call target is excluded — its defer/recover belong to the enclosing function.
	var litHasDefer, litHasRecover, litNamedDefer bool
	var litNamedNames []string

	if litSig != nil {
		litHasDefer, litHasRecover = v.funcBodyDeferRecover(funcLit.Body)

		if context.deferCall {
			// A DEFERRED literal's `recover()` recovers the ENCLOSING function — that is the whole
			// point of `defer func(){ recover() }()` — so it gets no recover scope of its own. A
			// `defer` written INSIDE it is a different matter: Go scopes that to the literal, and
			// dropping both together registered it into the enclosing function's scope instead, to
			// run at the wrong time. The frame form is what makes the split expressible, since the
			// literal can now carry its own frame while recover() keeps resolving statically to the
			// enclosing panic slot. (The shape occurs nowhere in the converted corpus, so this
			// closes a latent hole rather than fixing a live defect.)
			litHasRecover = false
		}

		litNamedDefer, litNamedNames = v.detectNamedReturnDefer(litSig, litHasDefer, litHasRecover)
	}

	// Does THIS literal's defer/recover scope emit as a GoFrame? A literal is a lambda (or a local
	// function), and a ref struct is a perfectly ordinary local of one — it is only CAPTURE that is
	// forbidden, and an inline body captures nothing. So the same frame the enclosing function gets
	// works here verbatim, which is also what lifts §3.1's local-function exclusion.
	useLitFrame := v.litGoFrameEligible(litSig, litHasDefer, litHasRecover, funcLit)

	// A function literal with NAMED results needs their declarations at the top of its block —
	// Go zero-initializes them and a bare `return` returns them. iter.Pull's
	// `next = func() (v1 V, ok1 bool) { …; return }` emitted `return (v1, ok1);` with nothing
	// declared (CS0103 — the wave-1 iter errors). The litNamedDefer path composes its own decls;
	// this covers plain and defer-without-mutation literals.
	litHasNamedResults := false

	if litSig != nil && funcLit.Type.Results != nil {
		for _, field := range funcLit.Type.Results.List {
			for _, name := range field.Names {
				if !isDiscardedVar(name.Name) {
					litHasNamedResults = true
				}
			}
		}
	}

	// A function literal's body is converted with ITS OWN namedReturnDeferMode, not the enclosing
	// function's (which must not leak in — otherwise the closure's `return expr` would be rewritten
	// against the OUTER named results). Save/restore around the body conversion.
	savedNamedReturnDeferMode := v.namedReturnDeferMode
	savedNamedReturnNames := v.namedReturnNames
	v.namedReturnDeferMode = litNamedDefer
	v.namedReturnNames = litNamedNames

	// A func literal BODY is function scope even when the literal sits in a PACKAGE-LEVEL var
	// initializer (`var Support = sync.OnceValue(func() bool { var size uint32; … })` —
	// internal/syscall/windows): visitFuncDecl never ran, so inFunction was false and the
	// literal's locals emitted as package fields (`internal static uint32 size;` inside the
	// lambda — a CS1002 syntax cascade gating os/fmt). Save/restore around the body.
	savedInFunction := v.inFunction
	v.inFunction = true

	// The literal's OWN parameters and locals can shadow golib's `heap` intrinsic even when the
	// enclosing declaration's do not — and a literal in a PACKAGE-LEVEL initializer has no enclosing
	// declaration at all, so the flag would otherwise carry whatever function was visited last. Rebase
	// on the package answer in that case, then OR in this literal's own declarations; restored below.
	savedHeapIntrinsicShadowed := v.heapIntrinsicShadowed

	if !savedInFunction {
		v.heapIntrinsicShadowed = v.packageDeclaresHeapIntrinsicIdent()
	}

	v.heapIntrinsicShadowed = v.heapIntrinsicShadowed || v.declaresHeapIntrinsicIdent(funcLit)

	// `inFunction` says the BODY is function scope; it does NOT say there is an enclosing function
	// DECLARATION. currentFuncName and currentFuncPrefix are owned by visitFuncDecl (allocated
	// together there), so for a literal in a package-level initializer they hold whatever the
	// PREVIOUS function declaration in the file left behind — or nothing at all, when none has been
	// visited yet. Every type-lift site keys on `lifted && v.inFunction` and then writes the
	// declaration into currentFuncPrefix, so a type lifted here — fmt scan_test.go's
	// `struct{ io.Reader }`, returned from a func literal inside the package-level `readers` — was
	// named after an UNRELATED preceding function and written into that function's ALREADY-FLUSHED
	// buffer, i.e. silently dropped: `new Scan_type(…)` with no declaration anywhere (CS1729, plus
	// CS0103/CS0034 in the ImplementGenerator's wrapper for the phantom type). With NO preceding
	// function declaration the buffer is nil outright and the converter PANICS (nil receiver in
	// strings.Builder.copyCheck, recovered as "visit file error" — the whole FILE is skipped).
	// ONE root; which of the two symptoms appears depends only on declaration order in the file.
	//
	// Give the literal its own sink and a name seed from the declaration being initialized, then
	// flush at package scope — where a lifted type belongs anyway, and exactly where the sibling
	// package-level lift (`readersᴛ1`) already goes. The flush lands before the var's own field
	// because a package-level initializer is converted to a STRING first and written afterwards.
	savedFuncName := v.currentFuncName
	savedFuncPrefix := v.currentFuncPrefix

	var packageLevelLifts *strings.Builder

	if !savedInFunction {
		packageLevelLifts = &strings.Builder{}
		v.currentFuncName = v.packageInitLiftName
		v.currentFuncPrefix = packageLevelLifts
	}

	// A literal's body is NOT inside the ENCLOSING function's GoFrame, whatever that function emits
	// as: a `defer` written here belongs to this literal, and a ref struct cannot be captured by a
	// lambda in any case. It gets a frame of its OWN when it defers or recovers, and that frame is a
	// nested one — so its names take a depth suffix that keeps them out of the enclosing declaration
	// space (CS0136). Both the body conversion and the composition of `inner` below name the frame,
	// so the depth is held across both and restored after — the same save/restore, and for the same
	// reason, as the named-result mode above (see visitorState's inGoFrame).
	savedInGoFrame := v.inGoFrame
	savedGoFrameNamedExit := v.goFrameNamedExit
	savedOpenGoFrames := v.openGoFrames
	v.inGoFrame = useLitFrame
	v.goFrameNamedExit = false

	if useLitFrame {
		v.openGoFrames++
	}

	defer func() {
		v.namedReturnDeferMode = savedNamedReturnDeferMode
		v.namedReturnNames = savedNamedReturnNames
		v.currentReturnSignature = savedReturnSignature
		v.inFunction = savedInFunction
		v.heapIntrinsicShadowed = savedHeapIntrinsicShadowed
		v.inGoFrame = savedInGoFrame
		v.goFrameNamedExit = savedGoFrameNamedExit
		v.openGoFrames = savedOpenGoFrames
		v.currentFuncName = savedFuncName
		v.currentFuncPrefix = savedFuncPrefix

		if packageLevelLifts != nil && packageLevelLifts.Len() > 0 {
			v.outputBuilder.WriteString(v.newline)
			v.outputBuilder.WriteString(packageLevelLifts.String())
		}
	}()

	if v.lambdaCapture == nil {
		v.lambdaCapture = newLambdaCapture()
		v.capturedVarCount = make(map[string]int)
	}

	v.enterLambdaConversion(funcLit)
	defer v.exitLambdaConversion()

	// Create a map of parameters to avoid capturing them
	paramNames := make(map[string]bool)

	if funcLit.Type.Params != nil {
		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// A NAMED RESULT is declared in the function literal's OWN scope — Go zero-initializes it and a bare
	// `return` returns it (litHasNamedResults below emits its declaration inside the lambda), so a
	// reference to it in the body is the result, NEVER an outer-scope capture. text/template's readFileFS
	// returns `func(file string) (name string, b []byte, err error)`; `b` was mis-hoisted as a capture
	// (`var bʗ1 = b;` — CS0103, `b` is undefined in the enclosing func). Exclude named results from the
	// capture set exactly as parameters are.
	if funcLit.Type.Results != nil {
		for _, field := range funcLit.Type.Results.List {
			for _, name := range field.Names {
				paramNames[name.Name] = true
			}
		}
	}

	// Filter out any captures that are actually parameters
	if captures, exists := v.lambdaCapture.stmtCaptures[funcLit]; exists {
		for ident := range captures {
			if paramNames[ident.Name] {
				delete(captures, ident)
			}
		}

		// If no captures remain, remove the empty map
		if len(captures) == 0 {
			delete(v.lambdaCapture.stmtCaptures, funcLit)
		}
	}

	v.prepareStmtCaptures(funcLit)

	result := strings.Builder{}

	if decls := v.generateCaptureDeclarations(); decls != "" {
		switch {
		case context.deferredDecls != nil:
			// go/defer/return thread an explicit builder for their own hoisting.
			context.deferredDecls.WriteString(strings.TrimRight(decls, " "))
		case v.hoistedDecls != nil:
			// The enclosing statement (assignment RHS, composite-literal element, call argument)
			// hoists these decls to a valid position before the statement.
			v.hoistedDecls.WriteString(strings.TrimRight(decls, " "))
		default:
			result.WriteString(decls)
		}
	}

	// The literal's own VALUE parameters on which its body calls a capture-mode (direct-ж)
	// method need the same ENTRY-TIME heap box as a declaration's parameters (see
	// paramNeedsHeapBox): the signature takes the incoming value under the `ʗp` name and the
	// prologue injected below re-declares the Go name as the boxed ref alias.
	boxedParamIdents := v.funcLitHeapBoxParamIdents(funcLit)
	var boxedParamNames HashSet[string]

	if len(boxedParamIdents) > 0 {
		boxedParamNames = HashSet[string]{}

		for _, ident := range boxedParamIdents {
			boxedParamNames.Add(v.getIdentName(ident))
		}
	}

	// A literal passed to a generic call whose type argument resolved to a CONSTRAINT PROXY must
	// declare each proxied parameter AT the proxy type: C# applies no user-defined conversion at a
	// parameter declaration, so the natural box is CS1678 + CS1661 against the delegate. Same
	// incoming-name-plus-prologue shape as the boxed params above — the parameter arrives under a
	// synthesized name and the prologue below re-declares the Go name at its natural type, leaving
	// the whole body unchanged. Keyed by the RENDERED name, exactly as funcLitHeapBoxParamNames
	// beside it: a literal's signature is generated from SYNTHESIZED vars (see getSignature) that
	// already carry the shadow-renamed name, so keying on the Go name silently misses every
	// renamed parameter — and net/http renames the common one, its `run(t, func(t *testing.T, mode
	// testMode))` inner `t` shadowing the outer (`tΔ1`). That miss is not inert: the prologue keys
	// off the same map, so it fired while the signature did not, emitting the natural type in the
	// declaration AND a same-named local beside it.
	proxyParamNames := map[string]string{}

	if len(context.proxyParamTypes) > 0 && funcLit.Type.Params != nil {
		index := 0

		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				if proxyType, ok := context.proxyParamTypes[index]; ok && name.Name != "" && name.Name != "_" {
					proxyParamNames[v.getIdentName(name)] = proxyType
				}

				index++
			}
		}
	}

	var parameterSignature string

	// For C#, lambda return type is inferred and not explicitly declared. The transient
	// boxed-param name set is scoped to exactly this call: the literal's signature is
	// generated from SYNTHESIZED vars (see getSignature) that carry the rendered names but
	// never match identEscapesHeap, so generateParametersSignature reads the set to emit a
	// boxed param under its incoming `ʗp` name. Cleared before the body conversion so a
	// NESTED literal's signature cannot inherit it. The proxy-param map is scoped identically
	// and for the same reason.
	v.funcLitHeapBoxParamNames = boxedParamNames

	if len(proxyParamNames) > 0 {
		v.funcLitProxyParamTypes = proxyParamNames
	}

	_, parameterSignature = v.convFuncType(funcLit.Type)
	v.funcLitHeapBoxParamNames = nil
	v.funcLitProxyParamTypes = nil

	blockStatementContext := DefaultBlockStmtContext()
	blockStatementContext.format.useNewLine = false

	// In namedReturnDefer mode the literal's body sits inside an extra block + func() wrapper, so
	// indent it one level deeper. A frame-form literal nests the same way — its body is the `try`
	// block inside the lambda's own block.
	if litNamedDefer || useLitFrame {
		v.indentLevel++
	}

	// The literal's own capture declarations were flushed above, into the ENCLOSING statement's hoist
	// buffer where they belong. That buffer is NOT a valid position for anything the BODY hoists: a
	// statement inside the body opens its own buffer, and a nested literal whose captures reference a
	// binding declared inside THIS body would otherwise be declared outside it. time's
	// BenchmarkStaggeredTickerLatency nests three levels of `b.Run(…, func(b *testing.B){…})`, and the
	// innermost `go func(…)`'s captures of `stats` — a slice `make`d in the middle literal — landed in
	// the OUTER literal's `b.Run(…)` ExprStmt buffer: `var statsʗ1 = stats;` two blocks above the
	// declaration of `stats` (CS0103 ×3). Detach for the body so a nested hoist can only reach a
	// position inside it, and restore afterward so the enclosing statement keeps its own sink.
	savedHoist := v.hoistedDecls
	v.hoistedDecls = nil

	// The pending KeepAlive list is FRAME-scoped the same way: a box the ENCLOSING statement has
	// already named (`consumeWith(noescape(unsafe.Pointer(&i)), func() { … })` names Ꮡi before
	// its arguments — this literal among them — are converted) must not be drained by the first
	// statement inside this body, where it would hold a box of the OUTER frame from the wrong one
	// and leave the enclosing statement with nothing. Convert the body against an empty list,
	// require it drained by the body's own statements, and restore the enclosing statement's.
	savedPendingKeepAlive := v.pendingSyscallKeepAlive
	v.pendingSyscallKeepAlive = nil

	v.pushBlock()
	v.visitBlockStmt(funcLit.Body, blockStatementContext)
	body := v.popBlockAppend(false)

	v.assertNoPendingKeepAlive("func literal")
	v.pendingSyscallKeepAlive = savedPendingKeepAlive

	v.hoistedDecls = savedHoist

	if litNamedDefer || useLitFrame {
		v.indentLevel--
	}

	// The prologues below (boxed value params, the variadic slice binding, plain named-result
	// declarations) are composed AFTER the body visit restored the indent, so under a frame they
	// need the extra level the frame's 	ry block added — same correction visitFuncDecl applies to
	// its own block prefix (reindentGoFrameBlock).
	bodyIndent := v.indentLevel + 1

	if useLitFrame {
		bodyIndent++
	}

	// A boxed VALUE parameter (see funcLitHeapBoxParamIdents) arrives under the `ʗp` name; the
	// literal's first statements re-declare the Go name as the boxed ref alias — the exact
	// parameter-preamble form of a declaration's boxed param (see paramNeedsHeapBox) — so body
	// uses hit the boxed storage and convSelectorExpr routes the capture-mode call through
	// `Ꮡ<name>` (CS1929 on the raw value without it; a call-site Ꮡ(value) copy-box would
	// compile but silently drop the callee's writes). Injected BEFORE the return-collapse
	// below, which also keeps the body a block. Unlike the variadic prologue, an IIFE is NOT
	// excluded: iifeParamName emits the `ʗp` name for a boxed param, so the rebinding is
	// required there too. When both prologues apply, the variadic injection below prepends its
	// line above these, matching visitFuncDecl's preamble order.
	if len(boxedParamIdents) > 0 {
		trimmedBody := strings.TrimSpace(body)

		if strings.HasPrefix(trimmedBody, "{") {
			prologue := strings.Builder{}

			for _, ident := range boxedParamIdents {
				renderedName := v.getIdentName(ident)
				incomingName := getHeapBoxLitParamName(renderedName)

				// An ARRAY param (direct, aliased, or named — see visitFuncDecl's parameter
				// preamble) folds its Go by-value clone into the box init.
				if typeNeedsValueClone(v.getIdentType(ident)) {
					incomingName += valueCloneSuffix(v.getIdentType(ident))
				}

				if v.options.preferVarDecl {
					prologue.WriteString(fmt.Sprintf("%s%sref var %s = ref %s(%s, out var %s%s);", v.newline, v.indent(bodyIndent), getSanitizedIdentifier(renderedName), v.heapIntrinsicName(), incomingName, AddressPrefix, renderedName))
				} else {
					csTypeName := v.getCSharpTypeName(v.getIdentType(ident))
					prologue.WriteString(fmt.Sprintf("%s%sref %s %s = ref %s(%s, out %s<%s> %s%s);", v.newline, v.indent(bodyIndent), csTypeName, getSanitizedIdentifier(renderedName), v.heapIntrinsicName(), incomingName, PointerPrefix, csTypeName, AddressPrefix, renderedName))
				}
			}

			body = "{" + prologue.String() + strings.TrimPrefix(trimmedBody, "{")
		}
	}

	// A CONSTRAINT-PROXY parameter (see constraintProxyLitParamTypes) arrives under its synthesized
	// name AT the proxy type, because C# applies no user-defined conversion at a parameter
	// declaration. The literal's first statements re-declare the Go name at its natural type, which
	// IS a position C# performs the conversion — the proxy's own `implicit operator ж<T>(proxy)`.
	// Everything after this line is the body exactly as it would have been emitted, so no member
	// access, capture, or nested literal inside it renders differently; only the boundary moved.
	if len(proxyParamNames) > 0 && funcLit.Type.Params != nil {
		trimmedBody := strings.TrimSpace(body)

		if strings.HasPrefix(trimmedBody, "{") {
			prologue := strings.Builder{}

			// Declaration order, so the emitted prologue matches the signature it mirrors.
			for _, field := range funcLit.Type.Params.List {
				for _, name := range field.Names {
					renderedName := v.getIdentName(name)

					if _, ok := proxyParamNames[renderedName]; !ok {
						continue
					}

					incomingName := getConstraintProxyLitParamName(renderedName)

					if v.options.preferVarDecl {
						prologue.WriteString(fmt.Sprintf("%s%svar %s = (%s)%s;", v.newline, v.indent(bodyIndent), getSanitizedIdentifier(renderedName), v.getCSharpTypeName(v.getIdentType(name)), incomingName))
					} else {
						prologue.WriteString(fmt.Sprintf("%s%s%s %s = %s;", v.newline, v.indent(bodyIndent), v.getCSharpTypeName(v.getIdentType(name)), getSanitizedIdentifier(renderedName), incomingName))
					}
				}
			}

			body = "{" + prologue.String() + strings.TrimPrefix(trimmedBody, "{")
		}
	}

	// A plain (non-boxed) ARRAY-typed literal parameter is a per-call COPY in Go; mirror
	// visitFuncDecl's parameter-preamble clone (`a = a.Clone();`) so writes through the
	// parameter cannot reach the caller's backing store. A heap-boxed array param already
	// folds its clone into the box init above.
	if funcLit.Type.Params != nil {
		prologue := strings.Builder{}

		for _, field := range funcLit.Type.Params.List {
			for _, name := range field.Names {
				if name.Name == "_" || boxedParamNames.Contains(v.getIdentName(name)) {
					continue
				}

				if !typeNeedsValueClone(v.getIdentType(name)) {
					continue
				}

				renderedName := getSanitizedIdentifier(v.getIdentName(name))
				prologue.WriteString(fmt.Sprintf("%s%s%s = %s%s;", v.newline, v.indent(v.indentLevel+1), renderedName, renderedName, valueCloneSuffix(v.getIdentType(name))))
			}
		}

		if prologue.Len() > 0 {
			trimmedBody := strings.TrimSpace(body)

			if strings.HasPrefix(trimmedBody, "{") {
				body = "{" + prologue.String() + strings.TrimPrefix(trimmedBody, "{")
			}
		}
	}

	// A variadic parameter arrives as a C# `params` array named `<name>ʗp` (see getVariadicParamName);
	// the Go body references the bare `<name>` as a slice<T>. A top-level func gets a
	// `var <name> = <name>ʗp.slice();` prologue (visitFuncDecl); a function LITERAL emitted no such
	// prologue, so a closure that references its variadic param was undefined (CS0103 — internal/dag's
	// `errorf := func(format string, a ...any) { … fmt.Sprintf(format, a...) }` spread bare `a`).
	// Inject the same prologue at the top of the literal's block. Doing it BEFORE the return-collapse
	// below also keeps the body a block (the param is a statement-scoped local), which is correct.
	if litSig != nil && litSig.Variadic() && litSig.Params().Len() > 0 && !context.isIIFE {
		// (An IIFE is excluded: it emits param NAMES only — the raw `a`, not `<name>ʗp` — via
		// iifeParamNames, with the delegate cast supplying the `params` type, so there is no
		// `<name>ʗp` array to .slice() and no rename to undo.)
		// body may still carry leading whitespace here (it is TrimSpace'd only in the collapse/else
		// arms below) — trim before probing for the opening brace.
		trimmedBody := strings.TrimSpace(body)

		param := litSig.Params().At(litSig.Params().Len() - 1)

		// An unnamed (`func(...int) { … }`) or blank (`func(_ ...int) { … }`) variadic parameter is
		// unreferenceable by Go law, so the prologue this block exists to inject would declare a dead
		// local — with an EMPTY name for the unnamed case, and under a name the literal's signature
		// never declared. See variadicParamIsUnreferenceable.
		if strings.HasPrefix(trimmedBody, "{") && !variadicParamIsUnreferenceable(param) {
			var prologue string
			useSSlice := v.ssliceEligible[param]
			sliceMethod := "slice"
			sliceType := "slice"

			if useSSlice {
				sliceMethod = "sslice"
				sliceType = "sslice"
			}

			if v.options.preferVarDecl {
				prologue = fmt.Sprintf("%s%svar %s = %s.%s();", v.newline, v.indent(bodyIndent), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
			} else {
				prologue = fmt.Sprintf("%s%s%s<%s> %s = %s.%s();", v.newline, v.indent(bodyIndent), sliceType, v.getCSharpTypeName(param.Type().(*types.Slice).Elem()), getSanitizedIdentifier(param.Name()), getVariadicParamName(param), sliceMethod)
			}

			body = "{" + prologue + strings.TrimPrefix(trimmedBody, "{")
		}
	}

	// An IIFE keeps a block body (it may need a func() wrapper and reads more like the Go
	// source); other single-return literals collapse to an expression-bodied lambda. A
	// namedReturnDefer literal always keeps a block (it returns its named results after the
	// func() wrapper).
	if v.firstStatementIsReturn && !context.isIIFE && !litNamedDefer && !litHasNamedResults && !useLitFrame {
		// The position-map sentinels this block carries are invisible in the emitted text but NOT
		// to this inspection, which reads the block as a string: an un-stripped sentinel makes the
		// prefix test below see `{` plus a sentinel rather than `{`, and every single-return
		// literal in the corpus (110 files, measured) refuses to collapse and emits a block body.
		// A collapsed body has no emitted line of its own — it lands on the enclosing statement's
		// line, which already carries that statement's sentinel — so the collapse DROPS them
		// rather than relocating them, and the block-body path below keeps the original text.
		collapsible := stripPositionSentinels(body)

		// Find return statement in string and remove it
		returnIndex := strings.Index(collapsible, "return ")

		// The visited block can carry HOISTED statements ahead of the return —
		// visitReturnStmt's tuple-conversion arm writes `var (ᴛ1, ᴛ2) = call;` before
		// `return (ᴛ1, ᴛ2);` (net lookup.go's `DoChan(key, func() (any, error) {
		// return testHookLookupIP(…) })`). Chopping at "return " dropped the call and
		// left the bare markers (CS0103 ×2). Collapse only when nothing but the block's
		// opening brace precedes the return; otherwise keep the block body.
		if returnIndex != -1 {
			if prefix := strings.TrimSpace(collapsible[:returnIndex]); prefix != "{" && prefix != "" {
				returnIndex = -1
				body = strings.TrimSpace(body)
			}
		}

		if returnIndex != -1 {
			body = collapsible[returnIndex+7:]

			// Remove the BLOCK's closing brace — always the last non-whitespace rune of the
			// visited block; the statement's `;` always separates it from the expression. The
			// old TrimSuffix+LastIndex pair cut at the last `}` ANYWHERE, truncating a return
			// expression containing its own `}` — `return []Value{ValueOf(yield(in[0]))}`
			// emitted `new ΔValue[]{ValueOf(yield(@in[0]))` with `}.slice()` chopped
			// (reflect/iter.go MakeFunc literals, CS1513 x2).
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, "}")
			body = strings.TrimSpace(body)
			body = strings.TrimSuffix(body, ";")
		}
	} else {
		body = strings.TrimSpace(body)
	}

	// Declare plain named results at the top of the literal's block (the litNamedDefer arm
	// below declares its own, outside the func() wrapper).
	if litHasNamedResults && !litNamedDefer && strings.HasPrefix(body, "{") {
		// Liveness applies only OUTSIDE a frame: a literal frame's named exit reads the locals
		// from generated code, exactly as a function's does.
		livenessBody := funcLit.Body

		if useLitFrame {
			livenessBody = nil
		}

		body = "{" + v.namedReturnDeclLines(litSig, bodyIndent, false, livenessBody) + strings.TrimPrefix(body, "{")
	}

	// Build the lambda body (what follows `=>`). A function literal that uses defer/recover gets
	// its own `func((defer, recover) => …)` execution context (so its deferred code runs and
	// recovers when invoked); when it also has named results that deferred code mutates, the
	// named results are declared outside that wrapper and returned after it. A deferred-call
	// target is the exception (its defer/recover belong to the already-wrapped enclosing function).
	var inner string

	switch {
	case useLitFrame:
		// The frame form, in the literal's own block. A value-returning literal's catch arm ends
		// with Go's zero results for the same reason a value-returning function's does (see
		// visitFuncDecl): a recovered panic returns them, and an unrecovered one never gets there.
		catchReturn := ""

		if litSig != nil && litSig.Results() != nil && litSig.Results().Len() > 0 && !litNamedDefer {
			catchReturn = "return default!;"
		}

		namedDecls := ""
		exitAndClose := fmt.Sprintf("%s%s}", v.newline, v.indent(v.indentLevel))

		if litNamedDefer {
			// §4.4 in a literal: results declared before the try, mutated by the deferred calls,
			// read back after the finally through the label the body's exits jump to.
			// nil body: these are the litNamedDefer declarations the deferred calls mutate and
			// the post-finally read consumes — never liveness-eligible.
			namedDecls = v.namedReturnDeclLines(litSig, v.indentLevel+1, true, nil)

			returnNames := v.namedReturnBoxReadNames(litSig, litNamedNames)
			returnExpr := strings.Join(returnNames, ", ")

			if len(returnNames) > 1 {
				returnExpr = "(" + returnExpr + ")"
			}

			exitLabel := ""

			if v.goFrameNamedExit {
				exitLabel = v.goFrameExitLabel() + ": "
			}

			if aliases := v.namedResultBoxAliasLines(litSig, v.indentLevel+2); aliases != "" && strings.HasPrefix(body, "{") {
				body = "{" + aliases + strings.TrimPrefix(body, "{")
			}

			exitAndClose = fmt.Sprintf("%s%s%sreturn %s;%s%s}", v.newline, v.indent(v.indentLevel+1), exitLabel, returnExpr, v.newline, v.indent(v.indentLevel))
		}

		inner = fmt.Sprintf("{%s%s%sGoFrame %s = default;%s%stry %s%s%s",
			namedDecls,
			v.newline, v.indent(v.indentLevel+1), v.goFrameName(),
			v.newline, v.indent(v.indentLevel+1), body,
			v.goFrameTail(v.indentLevel, catchReturn), exitAndClose)
	default:
		inner = body
	}

	if context.localFuncName != "" {
		// LOCAL FUNCTION emission (see LambdaContext.localFuncName): the whole statement, result
		// type included, so visitAssignStmt writes it verbatim. The result type is generated by
		// the same helper visitFuncDecl uses, so a local function reads exactly like a declared
		// one. A collapsed single-return body arrives as an EXPRESSION and takes the
		// expression-bodied form (which needs its own terminating `;`); a block body does not.
		result.WriteString(v.litNoInliningPrefix(funcLit) + v.generateResultSignature(litSig) + " " + context.localFuncName + "(" + parameterSignature + ") ")

		if strings.HasPrefix(inner, "{") {
			result.WriteString(inner)
		} else {
			result.WriteString("=> " + inner + ";")
		}
	} else if context.isIIFE {
		// Immediately-invoked function literal: emit `paramNames => BODY` (names only — the
		// delegate-cast in convCallExpr supplies the types). The transient boxed-param set is
		// scoped to the name rendering so a boxed param emits its incoming `ʗp` name here too.
		v.funcLitHeapBoxParamNames = boxedParamNames
		iifeParams := v.iifeParamNames(litSig)
		v.funcLitHeapBoxParamNames = nil

		result.WriteString(v.litNoInliningPrefix(funcLit) + iifeParams + " => " + inner)
	} else {
		// A literal with a single unsafe.Pointer result can mix return arms of DIFFERENT C#
		// types — reflect deepEqual's ptrval returns `(uintptr)v.pointer()` on one arm and the
		// raw `v.ptr` on the other — which defeats C# lambda return-type inference (CS8917).
		// State the return type explicitly (`@unsafe.Pointer (ΔValue v) => …`); each arm then
		// converts implicitly through the golib operators.
		returnTypePrefix := ""

		if results := litSig.Results(); context.genericResultInferenceTarget && results != nil && results.Len() > 0 {
			// The callee is generic and infers a type argument FROM this literal's return type
			// (see CallExprContext.genericResultInferredFuncArgs). C# derives that from the arms'
			// natural types, not from the Go result go/types resolved, so the declared type is
			// stated explicitly here — `sync.OnceValue(any () => { …; throw panic("x"); })`,
			// `OnceValues((any, any) () => …)`, `OnceValue(nint () => 42)`. That fixes the type
			// argument to exactly Go's, which is right for every arm shape, so no arm inspection
			// is needed (unlike the natural-inference cases below).
			if results.Len() == 1 {
				returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
			} else {
				returnTypePrefix = v.generateResultSignature(litSig) + " "
			}
		} else if results := litSig.Results(); context.untypedInterfaceTarget && results != nil && results.Len() > 1 {
			// The MULTI-result twin of the `any`-slot rule below, which was scoped to single
			// results for want of a demonstrated consumer. html/template's escape_test supplies
			// one: `FuncMap{"pred": func(a ...any) (any, error) {…}}` renders its arms as C#
			// tuples whose elements are `default!` on the error arm and `(i - 1, default!)` on
			// the success arm — neither carries a natural type, so NO arm can fix the delegate
			// and inference fails outright (CS8917, then CS1662/CS8716 on each return). Stating
			// the declared Go result tuple explicitly is the same remedy the single-result arm
			// and the generic-inference arm above already apply, through the same helper.
			returnTypePrefix = v.generateResultSignature(litSig) + " "
		} else if results := litSig.Results(); results != nil && results.Len() == 1 {
			if context.untypedInterfaceTarget {
				// A literal converted into a real `any` parameter slot is NATURAL-typed by C# —
				// there is no delegate target, so the inferred return type comes from the arms'
				// literal types (`return 0` → C# int = Go int32) rather than the DECLARED Go
				// result. The natural type becomes the value's runtime dynamic type, which
				// reflection then classifies: `func(x int) int` and `func(x int) int32` collapsed
				// to one Func<nint, int>, so quick.CheckEqual saw equal types where Go's differ
				// (TestFailure #3). State the declared Go result type explicitly. (Multi-result
				// literals in `any` slots keep natural tuple typing — no demonstrated consumer.)
				returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
			} else if basic, ok := results.At(0).Type().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
				returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
			} else if declaredIsIface, isEmpty := isInterface(results.At(0).Type()); declaredIsIface && !isEmpty {
				// An INTERFACE-returning literal whose arms return DISTINCT concrete types —
				// net ipsock.go's `inetaddr := func(ip IPAddr) Addr` returns TCPAddrжΔAddr /
				// UDPAddrжΔAddr / IPAddrжΔAddr adapter classes — has no best common type
				// either (CS8917); each arm converts implicitly once the return type is
				// explicit. Single-typed literals keep the inferred form (zero churn).
				//
				// The OPPOSITE end of the same inference failure: EVERY arm is untyped `nil`
				// (`client := func(*TCPConn) error { <-serverDone; return nil }`, net_test).
				// Each renders `default!`, which carries no natural type at all, so NO arm
				// contributes and the delegate is again uninferable (CS8917). This is the
				// single-result twin of the multi-result `!hasFullyTypedArm` rule below, and it
				// is drift-free by construction: an all-`default!` arm set never had an inferable
				// natural type to begin with. Assignment position only - an argument/return
				// literal is target-typed by its delegate, where an explicit return type could
				// only add an identity-match constraint against the target's own result type.
				armTypes, hasSingleReturn, allArmsUntypedNil := v.funcLitReturnArmTypes(funcLit)

				if len(armTypes) > 1 || (context.isAssignment && hasSingleReturn && allArmsUntypedNil) {
					returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
				}
			} else if context.isAssignment && v.funcLitAllReturnArmsAreUntypedNil(funcLit) {
				// The all-arms-untyped-nil rule from the interface arm above, for every OTHER
				// nilable result kind. It was written for `client := func(*TCPConn) error { …;
				// return nil }` and lived inside that arm, so a slice / map / pointer / channel /
				// func result never reached it — reflect's `g := func(in []Value) []Value { …;
				// return nil }` (TestCallGC) emitted `var g = (slice<Value> @in) => default!;`,
				// CS8917.
				//
				// The reasoning is identical and kind-independent: every arm renders `default!`,
				// which carries no natural type, so NO arm contributes one and the delegate is
				// uninferable. Stating the declared result type target-types each `default!` in
				// place. No extra kind test is needed — a literal whose every arm is the untyped
				// nil necessarily has a nilable result by Go's own rules.
				//
				// Assignment position only, for the reason the sibling arms give: an
				// argument/return literal is target-typed by its delegate, where an explicit
				// return type could only add an identity-match constraint against that target.
				returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
			} else if context.isAssignment {
				// A STRING-returning literal in natural-inference position (`pick := func(v any)
				// string {…}` → `var pick = …`) can mix return arms of DIFFERENT C# types even
				// though every arm is a Go string: a string literal is a `"…"u8` ReadOnlySpan<byte>,
				// a literal+var concat binds golib's `operator +(@string, @string)` (so it is
				// @string regardless of u8 suppression), and a call into a hand-written stub can
				// return C# `string` (the baseline fmt.Sprintf does). @string↔string convert
				// implicitly BOTH ways, so no unique best common type exists and the delegate is
				// not inferable (CS8917). State the return type explicitly (`var pick = @string
				// (any v) => …`); each arm then converts to @string in place. Gated to the basic
				// string kind (a named string type would need its own conversions — see
				// lambdaConstReturnCastType's named-type rationale) and to assignment position:
				// an argument/return/composite-element literal is target-typed by its delegate
				// (no inference to fail), where an explicit return type could only add an
				// identity-match constraint against stub delegate types.
				if basic, ok := types.Unalias(results.At(0).Type()).(*types.Basic); ok {
					if basic.Kind() == types.String {
						returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
					} else if basic.Info()&types.IsNumeric != 0 && v.funcLitReturnsUntypedNamedConst(funcLit) {
						// A NUMERIC-result literal in natural-inference position with a NAMED
						// untyped-constant return arm — `maxRune := func(rune) rune { return
						// unicode.MaxRune }` (strings TestMap): the const ref renders as a golib
						// `Untyped*` wrapper reference, whose implicit conversions run BOTH ways
						// with every numeric type. An all-const arm set therefore infers the
						// wrapper delegate (`Func<rune, UntypedInt>` — rejected at the invariant-
						// delegate use site, CS1503), and a mixed const/typed arm set has no
						// unique best common type at all (CS8917). Stating the declared return
						// type explicitly (`var maxRune = rune (rune _) => …`) converts each arm
						// in place. Gated to a BASIC numeric result (a named type would need a
						// second user conversion the wrapper cannot chain — see
						// lambdaConstReturnCastType's named-type rationale); a literal-only arm
						// set is the separate arm below.
						returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
					} else if basic.Info()&types.IsNumeric != 0 && v.funcLitNumericArmsMisinfer(funcLit, basic) {
						// A NUMERIC-result literal whose arms are ALL numeric literals of a
						// natural C# type the DECLARED result does not share. The arm above used
						// to record literal-only arm sets as "no churn — those render at concrete
						// C# types already"; they do, but at the LITERAL's type, not the declared
						// one, and the two differ whenever the declared Go type is not the
						// literal's natural width. crypto/tls's TestCipherSuites has the shape:
						// `isBetter := func(a, b uint16) int { …; return -1; …; return +1 }`
						// renders every arm as C# `int` where Go's `int` is `nint`, so the
						// inferred delegate is `Func<ushort, ushort, int>` — accepted everywhere
						// the variable is CALLED (int→nint converts), rejected the moment it is
						// passed as a delegate VALUE, delegate types being invariant:
						// `slices.IsSortedFunc(prefOrder, isBetter)`, CS1503, one of the package's
						// four build errors. Stating the declared type (`var isBetter = nint
						// (uint16 a, uint16 b) => …`) converts each arm in place.
						//
						// Scope. Assignment position only, for the same reason as every arm above
						// (elsewhere the literal is target-typed by its delegate). A literal
						// bound to a name that is ONLY ever called never reaches here at all —
						// localFunctionDefine already emits it as a local function carrying an
						// explicit result type — so this arm can only fire where the delegate
						// type is genuinely observable. And a single arm of a type the declared
						// result already matches suppresses it, which keeps the pervasive
						// `func(…) int32 { return 0 }` / `func(…) float64 { return 1.5 }` shapes
						// and every mixed arm set on their present emission (C# picks the wider
						// type there, which is the declared one). The mixed-arm case where the
						// declared type is NARROWER than C# would pick — `func(…) uint16` with
						// one `0` arm and one `ushort` arm, where ushort widens to int — is not
						// covered and has no measured instance; it needs the natural type of an
						// arbitrary arm, which this predicate deliberately does not attempt.
						returnTypePrefix = convertToCSTypeName(v.getAliasQualifiedTypeName(results.At(0).Type(), false)) + " "
					}
				}
			}
		} else if results := litSig.Results(); results != nil && results.Len() > 1 && context.isAssignment {
			// A MULTI-result literal where EVERY return arm carries a typeless element —
			// `return (default!, err)` on the error arms and `return (b, default!)` on the
			// success arm (macho file.go's sectionData, func(s *Section) ([]byte, error)):
			// a tuple literal with any untyped element has no natural type, so NO arm
			// contributes to inference (CS8917 + CS8130/CS8716 cascade). State the tuple
			// return type explicitly; nil elements then take the target element type.
			// NAMED results are included: crypto/x509 parseNameConstraintsExtension's
			// `getValues := func(subtrees) (dnsNames []string, ips []*net.IPNet, emails,
			// uriDomains []string, err error)` returns `nil,nil,nil,nil,err` on every error
			// arm and `…, nil` on the success arm — no fully-typed arm, CS8917. A bare `return`
			// (len 0) never matches results.Len(), so it neither sets hasReturn nor a false
			// hasFullyTypedArm; a named literal that DOES have a fully-typed explicit arm keeps
			// inferred typing (no return-type prefix, no churn).
			//
			// A basic-STRING literal element is ALSO inference-defeating — worse than typeless,
			// it is WRONGLY typed: inside a tuple the literal emits as a bare C# string (u8
			// spans cannot be tuple elements — see visitReturnStmt), and @string↔string convert
			// implicitly both ways, so an arm like `return dur, coverageSnapshot, ""` infers a
			// C# `string` element where the Go result is @string (internal/fuzz fuzzOnce: the
			// destructured errMsg then has no `!=` against a u8 literal, CS0019 — the tuple-
			// element sibling of the single-string-result CS8917 arm above). Gated on the
			// literal's presence AND a declared basic-string element, so a literal whose string
			// elements are all variables keeps inferred typing (no churn).
			//
			// An untyped NUMERIC constant literal element is the same shape when the declared
			// result element is a differently-SIZED basic type: the literal emits bare, so the
			// arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT
			// literal C# `double` — where the Go result is e.g. int64 (net/http ServeContent's
			// `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` inferred
			// `Func<(int, error errSeeker)>`, rejected at the serveContent call: delegate types
			// are invariant, CS1662/CS0029/CS1503; the explicit tuple type also drops the
			// leaked `errSeeker` element name). A declared element the literal's natural type
			// already matches (int32 for INT, float64 for FLOAT) infers correctly, and Go `int`
			// (C# nint) is deliberately exempt — the `return 0, err` shape against (int, error)
			// results is pervasive and green today (int→nint converts at every use site), so
			// marking it would churn stdlib-wide for no observed defect (the same reasoning
			// keeps lambdaConstReturnCastType away from signed single results).
			hasReturn := false
			hasFullyTypedArm := false

			// Per-result-position tracking for the Go-`int` (C# nint) MIXED-arm conflict below: at a
			// declared-`int` position, an INT LITERAL arm (`0`) is naturally C# `int` while a
			// non-literal Go-`int` arm (`i + 1`) is C# `nint`. When both occur — and the other tuple
			// positions are typeless (`default!`) on the non-literal arms — the ONLY arm with a natural
			// tuple type is a literal one, so C# infers the delegate's first element as `int` and the
			// `nint` arm then fails to convert to it (CS0029/CS1662; the var's inferred delegate is
			// also `int`-first, rejected at the invariant use site, CS0407 — bufio ExampleScanner_*'s
			// `onComma := func(...) (advance int, ...) { …; return 0, data, ErrFinalToken; …; return
			// i+1, …; }`). The plain `types.Int` exemption below intentionally lets the literal count as
			// matching (the pervasive all-literal `return 0, err` shape is green and must not churn), so
			// this narrower conflict is detected separately and forces the explicit return type.
			posHasIntLiteral := make([]bool, results.Len())
			posHasNintExpr := make([]bool, results.Len())

			ast.Inspect(funcLit.Body, func(n ast.Node) bool {
				if _, isLit := n.(*ast.FuncLit); isLit && n != funcLit.Body {
					return false // a nested literal's returns belong to it
				}

				if ret, ok := n.(*ast.ReturnStmt); ok && len(ret.Results) == results.Len() {
					hasReturn = true
					fullyTyped := true

					for i, res := range ret.Results {
						if tv, ok := v.info.Types[res]; ok {
							if basic, isBasic := tv.Type.(*types.Basic); isBasic && basic.Kind() == types.UntypedNil {
								fullyTyped = false
								break
							}
						}

						if isStringBasicLit(res) {
							if declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic); ok && declared.Kind() == types.String {
								fullyTyped = false
								break
							}
						}

						if lit, isNumeric := numericBasicLit(res); isNumeric {
							if declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic); ok && declared.Info()&types.IsNumeric != 0 {
								naturalKind := types.Int32

								if lit.Kind == token.FLOAT {
									naturalKind = types.Float64
								}

								if declared.Kind() != naturalKind && declared.Kind() != types.Int {
									fullyTyped = false
									break
								}
							}
						}
					}

					if fullyTyped {
						hasFullyTypedArm = true
					}

					// Record int-literal vs nint-expression occupancy per declared-`int` position
					// (independent of the fullyTyped break above, which is why it runs in its own loop).
					for i, res := range ret.Results {
						declared, ok := types.Unalias(results.At(i).Type()).(*types.Basic)

						if !ok || declared.Kind() != types.Int {
							continue
						}

						if lit, isNumeric := numericBasicLit(res); isNumeric && lit.Kind == token.INT {
							posHasIntLiteral[i] = true
						} else if resType := v.getType(res, false); resType != nil {
							if resBasic, ok := types.Unalias(resType).(*types.Basic); ok && resBasic.Kind() == types.Int {
								posHasNintExpr[i] = true
							}
						}
					}
				}

				return true
			})

			mixedIntConflict := false

			for i := range posHasIntLiteral {
				if posHasIntLiteral[i] && posHasNintExpr[i] {
					mixedIntConflict = true
					break
				}
			}

			if hasReturn && (!hasFullyTypedArm || mixedIntConflict) {
				returnTypePrefix = v.generateResultSignature(litSig) + " "
			}
		}

		result.WriteString(v.litNoInliningPrefix(funcLit) + returnTypePrefix + "(" + parameterSignature + ") => " + inner)
	}

	return result.String()
}

// funcLitHeapBoxParamIdents returns the literal's own VALUE parameters that need an
// entry-time heap box, in declaration order — the function-literal analogue of
// paramNeedsHeapBox, which serves declaration parameters via currentFuncDecl (a literal's
// params never enter its walk, and package-level initializer literals have no declaration at
// all). A literal param qualifies exactly like a declaration param: marked escaping by
// markCaptureModeBoxedParams AND re-verified against the declaring ident, so a param that
// leaked into identEscapesHeap via a mixed `t, y := …` define keeps its historical unboxed
// emission — or routed to SHARED storage by the capture analysis (written after a NESTED
// closure captured it — see processPotentialCapture's varShareFacts arm), whose renders
// reference the box inside every capturing lambda, so the literal's prologue must declare it
// (the declaration-param cousins are database/sql beginDC's `ctx` / go/types nify's `x, y`,
// CS0103). A variadic parameter is excluded (its `ʗp` rename/prologue is the variadic slice
// convention, and its unnamed []T type carries no methods).
func (v *Visitor) funcLitHeapBoxParamIdents(funcLit *ast.FuncLit) []*ast.Ident {
	if funcLit.Type.Params == nil {
		return nil
	}

	var boxed []*ast.Ident

	for _, field := range funcLit.Type.Params.List {
		if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
			continue
		}

		for _, ident := range field.Names {
			if isDiscardedVar(ident.Name) {
				continue
			}

			obj := v.info.ObjectOf(ident)

			if obj == nil {
				continue
			}

			if _, isPointer := obj.Type().(*types.Pointer); isPointer {
				continue
			}

			if v.identHasHeapBox(obj, obj.Type()) && v.paramBoxReasonHolds(ident, obj, funcLit.Body) {
				boxed = append(boxed, ident)
			}
		}
	}

	return boxed
}
