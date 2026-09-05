// convCallExpr.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"path/filepath"
	"strconv"
	"strings"
)

// csharpKeywordCastTypes are the C# keyword primitive type names for which `(T)-value` is
// unambiguously a cast. Every other cast target the converter emits is either a using-ALIAS
// (int64=long, uint64=ulong, rune=int, …) or a `[GoType]` NAMED type (level, Class) — for those,
// `(T)-1` parses as `T MINUS 1` (CS0075 "to cast a negative value … enclose in parentheses" /
// CS0119 "T is a type, not valid in the given context"), so the operand must be parenthesized.
var csharpKeywordCastTypes = NewHashSet([]string{
	"int", "uint", "long", "ulong", "short", "ushort", "byte", "sbyte",
	"nint", "nuint", "float", "double", "decimal", "bool", "char",
})

// castOperandNeedsParens reports whether a cast operand must be wrapped in parentheses — it leads
// with a unary `+`/`-` AND the cast target is not a C# keyword type (see csharpKeywordCastTypes), so
// `(T)-value` would otherwise mis-parse as a subtraction rather than a cast (x/text/unicode/bidi's
// `level(-1)`, archive/tar's `int64(-1)`).
func castOperandNeedsParens(typeName, expr string) bool {
	if !strings.HasPrefix(expr, "-") && !strings.HasPrefix(expr, "+") {
		return false
	}

	return !csharpKeywordCastTypes.Contains(typeName)
}

// isNarrowIntegerKind reports whether the basic kind is a sub-`int`-width integer (int8/uint8/
// int16/uint16). C# promotes arithmetic on these to `int`, whereas Go evaluates them at their own
// width (with overflow wrapping), so a narrow-typed result needs an explicit cast back.
func isNarrowIntegerKind(kind types.BasicKind) bool {
	switch kind {
	case types.Int8, types.Uint8, types.Int16, types.Uint16:
		return true
	}

	return false
}

// identIsUniverseBuiltin reports whether an identifier in call position resolves to a Go universe
// built-in rather than to a same-named declaration that SHADOWS it. Go permits shadowing a built-in
// at ANY scope — `make := func(z *Int) *Int {…}` as a function-local in math/big's own tests, a
// parameter named `len`, a package-level `var new …` — after which `make(x)` is an ordinary call to
// that declaration, NOT the built-in. The converter's built-in arms are keyed on the identifier's
// NAME, so without this check a shadowed call is emitted with built-in semantics (`make(test.z)` →
// `new nint()`, CS1503/CS1929). go/types has already resolved the name for us: a genuine built-in
// is the universe *types.Builtin object, anything else is a shadowing declaration and must fall
// through to the ordinary call path.
//
// This is the opposite direction from packageBuiltinShadows, which handles a package method whose
// name collides with a built-in the call still MEANS (there the call is a real built-in and gets
// QUALIFIED as `builtin.<name>`); here the call is not a built-in at all.
func (v *Visitor) identIsUniverseBuiltin(ident *ast.Ident) bool {
	_, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin)

	return isBuiltin
}

// callFunIsUniverseBuiltin reports whether a call's callee is an unshadowed universe built-in —
// identIsUniverseBuiltin for a call whose callee has not already been narrowed to an identifier.
func (v *Visitor) callFunIsUniverseBuiltin(callExpr *ast.CallExpr) bool {
	ident, ok := callExpr.Fun.(*ast.Ident)

	return ok && v.identIsUniverseBuiltin(ident)
}

// callFunIsUniversePrint reports whether a call is to the universe built-in `print` or `println`,
// whose variadic parameter the argument classifier treats as `interface{}`.
//
// The name is read from the AST, never from the CONVERTED callee text. Converting callExpr.Fun just
// to ask "is this spelled print?" re-walked the callee's ENTIRE subtree, and that test sits inside
// the per-parameter loop — so a call with p parameters converted its callee p+1 times. On a CHAINED
// method call each link's callee IS the rest of the chain, which compounds: the cost is (p+1)^N in
// the chain length N. go.mongodb.org/mongo-driver/bson/bsoncodec registers its default codecs as a
// 42-link `rb.RegisterTypeEncoder(…).RegisterTypeEncoder(…)…` fluent chain over a 2-parameter
// method, so its conversion needed 3^42 callee walks and never finished (issue #33). The callee is
// converted once, later, in Phase 7 — this predicate is O(1) and answers the same question, because
// an unshadowed universe built-in is a bare identifier whose name IS the built-in's name.
func (v *Visitor) callFunIsUniversePrint(callExpr *ast.CallExpr) bool {
	ident, ok := callExpr.Fun.(*ast.Ident)

	return ok && (ident.Name == "print" || ident.Name == "println") && v.identIsUniverseBuiltin(ident)
}

// convCallExpr converts any Go call expression to C#. Every function call, method call, built-in
// call and type conversion in the corpus routes through here, which is why it is the largest
// function in the converter.
//
// It reads as one long body, but it is a PIPELINE — each phase narrows what the next has to
// handle, and the `// ---- Phase N ----` banners below mark the boundaries:
//
//	1a  shapes intercepted whole (IIFE, identity pointer reinterpret)
//	1b  type conversions — `T(x)` is a call in Go's grammar but a conversion in meaning
//	1c  constructor calls
//	2   build the argument-conversion context
//	3   classify each argument against the callee signature   <- the bulk
//	4   Go universe built-ins (append/len/cap/make/copy/panic/recover/print/…)
//	5   function-literal and lambda arguments
//	6   conversions whose source is a string or slice
//	7   targeted fixes: name shadowing, min/max, unsafe constants, atomic managed pointers
//	8   generic instantiation type arguments
//	9   render the call
//
// Phases 1a-1c and 4 RETURN directly; the rest fall through and contribute to the final rendering.
// Splitting this along those seams is planned work — the banners exist so that starts from a map.
func (v *Visitor) convCallExpr(callExpr *ast.CallExpr, context LambdaContext) string {
	// The //go:cgo_unsafe_args block lift (cgoUnsafeArgsLift.go): the ONE `unsafe.Pointer(&first)` the
	// current declaration's lift consumes renders as the synthesized block's pinned box. Intercepted
	// ahead of every arm because the conversion reaches two of them -- the type-conversion arm for a
	// boxed operand and the `new @unsafe.Pointer(...)` constructor path for a plain one -- and either
	// would otherwise render the consumed address-of on its own (a copy box, `Ꮡ(kq)`).
	if lift := v.currentCgoLift; lift != nil && callExpr == lift.conversion {
		return cgoUnsafeArgsBlockPointer(lift)
	}

	// A call into the syscall funnel (Syscall/Syscall6/…/SyscallN) — see
	// syscallKeepAliveAnalysis.go for why this reproduces Go's own uintptrkeepalive contract
	// rather than every other call's ordinary argument rendering. Intercepted before anything
	// else so a pointer-derived argument's box is captured before the general path would convert
	// it straight to a transient uintptr with nothing left to keep alive.
	if syscallFunnelCall(v.info, callExpr) {
		// A DEFERRED or SPAWNED funnel call (callArgs non-nil) does NOT take the interception:
		// it renders through the general path below, exactly like any other callee, so the
		// temp-parameter form fills its eager-argument slots. Intercepting it filled nothing —
		// convSyscallFunnelCall takes no LambdaContext and cannot reach callArgs, so the
		// arguments rendered inside the THUNK BODY while every slot stayed empty:
		// `defer((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => syscall.Syscall(SYS_CLOSE, (uintptr)fds[i], 0, 0), , , , ,
		// ref ᒐ)` — CS0839 ×4, and a SEMANTIC defect underneath it, since `fds[i]` would then be
		// read at unwind rather than at the defer statement (measured 2026-09-02 on
		// runtime/memmove_linux_amd64_test.go:44 and on a scratch reproducer whose loop mutates
		// the slice after the defer).
		//
		// Falling through rather than threading the context into convSyscallFunnelCall is the
		// smaller and the more honest fix: the general path is where a deferred argument's
		// defer-specific treatment already lives (convExprList's untyped-constant default-type
		// cast, castArgToType, the ref-lowered box carve-out), and reimplementing that beside the
		// funnel's own rendering would be a second copy of it, drifting on its own schedule. One
		// rule, one shape.
		//
		// What the interception exists for — the uintptrkeepalive contract — is preserved by
		// REJECTING the one shape the general path cannot carry (below), not by re-deriving it.
		if context.callArgs == nil {
			return v.convSyscallFunnelCall(callExpr)
		}

		v.rejectDeferredSyscallKeepAlive(callExpr)
	}

	// The pin-lifetime class's MANAGED-callee member (Q49): an argument that is a bridged
	// unsafe.Pointer-returning call over a frame-minted box — `memhash32(noescape(unsafe.Pointer(
	// &i)), seed)` — renders through the general path unchanged, and the box is named for a
	// KeepAlive drained after the statement, so it outlives the call the way Go's liveness keeps
	// it. See bridgedWrapperKeepAliveBoxes for the shape, the measurement and the census.
	if boxes := v.bridgedWrapperKeepAliveBoxes(callExpr); len(boxes) > 0 {
		if context.callArgs != nil {
			v.rejectDeferredBridgedWrapper(callExpr, boxes)
		}

		v.pendingSyscallKeepAlive = append(v.pendingSyscallKeepAlive, boxes...)
	}

	// ---- Phase 1a: shapes intercepted before the general call path ----
	//
	// Each of these is a Go idiom whose faithful rendering the general path below gets wrong, so
	// it is recognized and emitted directly. They are narrow by design: every one is gated on an
	// exact shape so no ordinary call can fall into them.

	// Immediately-invoked, no-argument function literal (IIFE): `func(){ … }()`. A bare C#
	// lambda cannot be invoked directly (CS0149), and the literal may use defer/recover that
	// must be scoped to itself. Emit it as a `func((defer, recover) => body)` execution-context
	// call, which both wraps (its own defer/recover scope) and runs immediately — so no trailing
	// call `()` is appended. (Argument-taking IIFEs are handled by the normal call path.)
	// The callee is UNPARENTHESIZED first. Go's own idiomatic spelling of an IIFE carries the parens
	// — `(func(){ … })()` — and go/parser puts an *ast.ParenExpr in Fun for it, so a direct assertion
	// declined the interception: the general call path then rendered a bare C# lambda through
	// convParenExpr and appended `(args)`, which cannot be invoked (CS0149, "Method name expected").
	// net/http's clientserver_test.go writes it that way. Both spellings are the same Go program and
	// must convert alike.
	if funcLit, ok := ast.Unparen(callExpr.Fun).(*ast.FuncLit); ok && !context.deferOrGoCall {
		// A C# lambda cannot be invoked directly, so an IIFE is cast to a delegate and then
		// called: `((Action)(() => …))()`, `((Func<nint>)(() => …))()`, `((Action<nint>)(n =>
		// …))(7)`, etc. The literal's body picks up its own `func((defer, recover) => …)`
		// execution context only when it actually uses defer/recover (see convFuncLit). Variadic
		// literals fall through to the normal path (delegate type would need a params array).
		if sig, ok := v.info.TypeOf(funcLit).(*types.Signature); ok && !sig.Variadic() {
			iifeContext := DefaultLambdaContext()
			iifeContext.isIIFE = true

			lambda := v.convFuncLit(funcLit, iifeContext)
			args := v.convExprList(callExpr.Args, callExpr.Lparen, DefaultCallExprContext())

			return fmt.Sprintf("((%s)(%s))(%s)", v.iifeDelegateType(sig), lambda, args)
		}
	}

	// A `(*T)(…)` conversion whose source is an IDENTITY reinterpret of a same-typed pointer —
	// `(*Builder)(abi.NoEscape(unsafe.Pointer(b)))` with b already `*Builder` — is Go's
	// escape-analysis idiom for `b.addr = b` (strings.Builder copyCheck; the type's own TODO says to
	// revert it to exactly that once escape analysis improves). Every other rendering of it — whether
	// the conversion renderer or, because `(*T)(unsafe.Pointer)` mis-classifies as a non-conversion,
	// the regular-call path — round-trips through uintptr, which golib's
	// `(ж<T>)(uintptr) => new ж<T>(*(T*)value)` resolves by DEREFERENCING-and-COPYING: the result box
	// is not reference-equal to the source, so the type's copy-by-value self-check (`b.addr != b`)
	// false-panics at runtime. Intercept ONLY this exact identity shape (leaving all other pointer
	// conversions on their existing path) and emit the source pointer's BOX form directly. The
	// isPointer context renders a deref-aliased pointer param/receiver as `Ꮡb`, not its value alias
	// `b` (a `Builder` value cannot assign to the `ж<Builder>` addr field).
	if len(callExpr.Args) == 1 {
		if identSrc := v.pointerReinterpretIdentitySource(callExpr, callExpr.Args[0]); identSrc != nil {
			identContext := DefaultIdentContext()
			identContext.isPointer = true
			return v.convExpr(identSrc, []ExprContext{identContext})
		}
	}

	funcType := v.getType(callExpr.Fun, false)
	// ---- Phase 1b: type conversions ----
	//
	// T(x) is a CALL in Go's grammar but a conversion in meaning, so it forks off here before
	// any of the call machinery below runs.

	// A conversion whose target is an ANONYMOUS INTERFACE type literal -- `interface{}(x)`, or
	// `interface{ Foo() int }(x)`. Go writes it as a call; C# has no such form, and the target has
	// no types.Object for isTypeConversion's peel to find, so without this arm it fell through to
	// the ordinary call machinery and emitted the LIFTED interface's name as a callee --
	// `main_typeᴛ1(t)`, which is CS1955 (a type is not invocable). The same shape the
	// IndexListExpr arm below was added for.
	//
	// Routed through convertToInterfaceType -- the ASSIGNMENT path's own helper -- rather than
	// through a rendered target name, for two reasons the plain-name path cannot serve: an
	// anonymous interface must be LIFTED to a named C# type and the name path emits the raw
	// `interface{…}` signature instead (the same trap convStarExpr documents for `(*struct{…})`
	// targets), and a Go conversion to an interface IS an assignment in meaning, so sharing the
	// helper is what makes the conversion and assignment forms agree by construction rather than
	// by two renderings that happen to match.
	if ifaceLit := interfaceTypeLiteralTarget(callExpr.Fun); ifaceLit != nil && len(callExpr.Args) == 1 {
		ifaceType := v.info.TypeOf(ifaceLit)
		argType := v.info.TypeOf(callExpr.Args[0])

		if ifaceType != nil && argType != nil {
			// The CAST is load-bearing, and the measurement that says so is worth the line: the
			// FIRST cut of this arm routed through convertToInterfaceType alone, emitted
			// `var c = t;`, and COMPILED, RAN and PRINTED THE RIGHT NUMBERS while losing Go's
			// static type -- `c` was the concrete `T`, not `interface{ Foo() int }`, and it even
			// called the right method, which is why the loss is quiet. That is the false green
			// this arm's guard exists to refuse.
			//
			// The CAST is not optional the way it is at an assignment: a Go conversion has no
			// declared slot to supply the static type, so `x := interface{ Foo() int }(t)` must
			// name the (lifted) interface or C# infers the CONCRETE type and the Go type is lost
			// -- it compiles and even calls the right method, which is why the loss is quiet.
			// convertToInterfaceType still runs first: it records the witness the lift needs.
			converted := v.convertToInterfaceType(ifaceType, argType, v.convExpr(callExpr.Args[0], nil))

			return fmt.Sprintf("(%s)(%s)", v.getCSharpTypeName(ifaceType), converted)
		}
	}

	// Check if the call is a type conversion
	if ok, targetTypeName := v.isTypeConversion(callExpr); ok {
		arg := callExpr.Args[0]

		// A nil→POINTER conversion — `(*T)(nil)` or `NamedPtr(nil)` — renders the nil in
		// POINTER context (golib `nil`), so the target's implicit NilType conversion yields its
		// CANONICAL typed nil instance (ж<T>.NilBox / the named wrapper's NilInstance). The boxed
		// value then carries its Go type: `any((*T)(nil)) != nil`, `%T` prints `*T`, and the
		// pervasive descriptor idiom `reflect.TypeOf((*T)(nil)).Elem()` resolves — where the old
		// `default!` rendering erased the type to a bare null reference. Two sites converting the
		// same type reference-compare equal (one shared instance), which object-typed `==`
		// comparisons of boxed typed nils rely on.
		if tv, ok := v.info.Types[arg]; ok && tv.IsNil() {
			if _, isPtr := v.info.TypeOf(callExpr).Underlying().(*types.Pointer); isPtr {
				// A pointer-to-TYPE-LITERAL target — `(*[]byte)(nil)`, `(*struct{ r7 int })(nil)`
				// (gob's bootstrapType table). Render it through convStarExpr rather than the
				// resolved target name: an ANONYMOUS struct/interface element must be LIFTED to a
				// named C# type, and convStarExpr is the site that performs that lift (the plain
				// name path emits an unresolvable raw `struct{…}` signature instead).
				if starExpr := typeLiteralPointerTarget(callExpr.Fun); starExpr != nil {
					pointerCS := v.convStarExpr(starExpr, DefaultStarExprContext())

					// `(*[N]E)(nil)` — the array LENGTH is part of the Go type and `array<E>`
					// cannot hold it, so it rides the value (arrayDimsNilCargo.go).
					if nilArray := v.nilArrayPtrValue(v.info.TypeOf(callExpr), pointerCS); nilArray != "" {
						return nilArray
					}

					return fmt.Sprintf("((%s)nil)", pointerCS)
				}

				targetCS := convertToCSTypeName(targetTypeName)

				if aliased, ok := v.foreignAliasedTypeName(v.info.TypeOf(callExpr)); ok {
					targetCS = aliased
				}

				// The same cargo through the named-target path — an ALIAS for `*[N]E` resolves to
				// a name here rather than to a StarExpr, and carries the dimension identically.
				if nilArray := v.nilArrayPtrValue(v.info.TypeOf(callExpr), targetCS); nilArray != "" {
					return nilArray
				}

				return fmt.Sprintf("((%s)nil)", targetCS)
			}

			// A nil→DIRECTIONAL-CHANNEL conversion — `(chan<- string)(nil)` — is the zero value
			// of the directional type, and the plain cast of `default!` erased the direction the
			// conversion exists to apply, so `reflect.TypeOf((chan<- string)(nil))` read the
			// bidirectional type (TestAll #12, TestChanOfDir's checkSameType rows). Rendered as
			// the directional nil factory, the same emission every other zero-value site uses.
			// A conversion of nil to a BIDIRECTIONAL channel returns "" here and keeps its
			// existing path, byte for byte. Note this is the CONSTRUCTION half of the narrowing
			// r39d excluded, not the priced half: a cast of nil has a syntactic hook and mints a
			// fresh value; the 89 assignment/argument/return positions the exclusion counted
			// copy a LIVE channel and remain unstamped (see chanDirectionCargo.go).
			if nilChan := v.chanDirNilValue(v.info.TypeOf(callExpr)); nilChan != "" {
				return nilChan
			}
		}

		// A compile-time FLOAT constant CONVERSION whose operand references a named untyped-float
		// const — `float64(100000 * Pi)` / `float32(...)` — FOLDS to a single-rounded literal at the
		// TARGET width, the conversion-operand counterpart of the package-level const fold in
		// visitValueSpec. The runtime form `(float64)(100000D * Pi)` rounds a SECOND time
		// (314159.2653589793, −1 ULP), whereas Go folds `100000*Pi` in arbitrary precision and rounds
		// ONCE to 314159.26535897935. Restricted to a BASIC float64/float32 target (a named float type
		// keeps its [GoType]-wrapper conversion path below), and short-circuits so the operand is NOT
		// separately converted — re-introducing the double round. (Bare refs / pure-literal / int /
		// complex operands are rejected inside the helper.)
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok &&
			(targetBasic.Kind() == types.Float64 || targetBasic.Kind() == types.Float32) {
			if folded := v.foldedNamedFloatConstLiteral(arg, v.getCSharpTypeName(targetBasic)); folded != "" {
				return folded
			}
		}

		// A conversion to a NAMED float type whose constant operand references a named UNTYPED
		// constant needs an explicit hop through the target's underlying basic type. go/types gives
		// the conversion operand the target type, so the general named-numeric path below sees an
		// identity conversion and emits `(Named)(Untyped*)`; C# cannot chain Untyped* -> basic ->
		// Named in one cast (CS0030). A computed float constant is folded first so Go's exact
		// arbitrary-precision evaluation is rounded only once at the target width.
		if targetNamed, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if targetBasic, ok := targetNamed.Underlying().(*types.Basic); ok &&
				targetBasic.Info()&types.IsFloat != 0 && v.containsUntypedNamedConstRef(arg) {
				underlyingCS := v.getCSharpTypeName(targetBasic)
				targetCS := convertToCSTypeName(targetTypeName)
				if aliased, ok := v.foreignAliasedTypeName(v.info.TypeOf(callExpr)); ok {
					targetCS = aliased
				}

				if folded := v.foldedNamedFloatConstLiteral(arg, underlyingCS); folded != "" {
					return fmt.Sprintf("((%s)(%s))", targetCS, folded)
				}

				return fmt.Sprintf("((%s)(%s)(%s))", targetCS, underlyingCS, v.convExpr(arg, nil))
			}
		}

		// A `string(x)` conversion the hoisting pre-pass elected to lift to a single function-scope
		// `sstring` temp (see planSStringHoists) emits just the temp NAME at every use, instead of
		// re-materializing `((sstring)x)` here. suppressSStringHoist is set only while the hoisted
		// decl's OWN initializer is being rendered, so that one keeps emitting the real view.
		if tempName, ok := v.sstringHoistedConvExprs[callExpr]; ok && !v.suppressSStringHoist {
			return tempName
		}

		// `(*Base)(p)` reinterpreting a pointer to a DEFINED type as a pointer to its underlying
		// named type — `(*atomic.Uint32)(c)` where `c` is `*counter` and `type counter atomic.Uint32`
		// (runtime/mprof goroutineProfileStateHolder). C# has no `ж<counter> → ж<atomic.Uint32>`
		// conversion (distinct generic instantiations); the inherited [GoType] wrapper only provides a
		// VALUE conversion (counter → atomic.Uint32). Box that converted value so the pointer-receiver
		// methods (Load/Store/…) resolve: `Ꮡ((atomic.Uint32)(c))`. Done before checkForImplicitConversion
		// so it does not record a spurious counter→ж<atomic.Uint32> indirect conversion. (The address is
		// of a copy — the atomic intrinsics behind it are asm stubs, so this is about compilable C#.)
		if resultPtr, ok := v.info.TypeOf(callExpr).(*types.Pointer); ok {
			if argPtr, ok := v.info.TypeOf(arg).(*types.Pointer); ok {
				resultElem := resultPtr.Elem()
				argElem := argPtr.Elem()

				baseNamed, okBaseNamed := resultElem.(*types.Named)
				defNamed, okDefNamed := argElem.(*types.Named)

				// A pointer reinterpret `(*Base)(p)` between two types with an IDENTICAL underlying, boxed
				// as a COPY of the [GoType] VALUE conversion. `ж<Base>` and `ж<Def>` are distinct generic
				// instantiations with no C# conversion between them, and the atomic intrinsics behind these
				// are asm stubs, so this is about producing compilable C#, not write-through. Two shapes:
				//   - Named ↔ Named: `(*pinnerBits)(*gcBits)` (type pinnerBits gcBits).
				//   - Named → its underlying BASIC: `(*uint64)(*lfstack)` — an atomic op on a named numeric
				//     `type lfstack uint64` (also sweepClass/profAtomic/sysMemStat), where the address of
				//     the named field flows to `atomic.Load64((*uint64)(head))`. `ж<lfstack> → ж<uint64>`
				//     has no conversion (CS0030); box a copy of the value conversion `(uint64)(head)`.
				namedToNamed := okBaseNamed && okDefNamed && baseNamed != defNamed &&
					types.Identical(baseNamed.Underlying(), defNamed.Underlying())

				_, resultIsBasic := resultElem.(*types.Basic)
				namedToBasic := resultIsBasic && okDefNamed && types.Identical(resultElem, defNamed.Underlying())

				// The third direction — BASIC → Named over that basic: `(*stringReader)(&str)`
				// where `type stringReader string` (fmt's Sscan family, CS0030 x3). Same
				// value-convert-and-re-box; writes through the box hit the copy, which is
				// faithful for this pattern (the source string is never re-read).
				_, argIsBasic := argElem.(*types.Basic)
				basicToNamed := okBaseNamed && argIsBasic && types.Identical(argElem, baseNamed.Underlying())

				// `(*[4]uint64)(&s.s)` / `(*fiatScalarNonMontgomeryDomainFieldElement)(&s.s)` — pointer
				// reinterprets of an array-backed DEFINED type (crypto/internal/edwards25519 scalar.go):
				// to its unnamed underlying array, or to a SIBLING defined type written over the SAME
				// unnamed array. `ж<Def>` has no conversion to `ж<array<E>>`/`ж<Sibling>` (distinct
				// generic instantiations — CS0030), and the sibling VALUE conversion the namedToNamed
				// route emits does not exist either (each [GoType("[N]elem")] wrapper converts only
				// to array<E>, and C# never chains two user-defined conversions). Unlike the copy-boxed
				// atomic shapes below, these fiat sites WRITE through the reinterpreted pointer
				// (fiatScalarFromBytes parses the scalar INTO &s.s on a virgin receiver), so the route
				// must share element storage: the wrapper's `Value` property invoked THROUGH THE REF
				// (ж<T>.Value is ref-returning) materializes the lazy array<E> backing on the ORIGINAL
				// storage and returns an array<E> struct sharing its T[]; boxing that (Ꮡ) yields a
				// pointer whose element reads AND writes flow through. Whole-value writes (`*p = q`)
				// rebind only the boxed copy — acceptable, matching the documented array<T> model.
				// The written-RHS gate keeps chain-defined types (`type pallocBits pageBits`, whose
				// wrappers DO convert to each other) on the namedToNamed route below, byte-identical.
				// types.Unalias: the target may be an ALIAS to the unnamed array — fiat's
				// `(*p224UntypedFieldElement)(&x)` where `type p224UntypedFieldElement =
				// [4]uint64` renders as `ж<array<ulong>>` via its global using, so the same
				// storage-sharing route applies (CS0030 ×20, all four fiat curves).
				_, resultIsArray := types.Unalias(resultElem).(*types.Array)
				namedToArray := resultIsArray && okDefNamed &&
					types.Identical(types.Unalias(resultElem), defNamed.Underlying()) &&
					writtenRHSIsUnnamedArray(defNamed)

				namedSiblingArrays := namedToNamed &&
					writtenRHSIsUnnamedArray(defNamed) && writtenRHSIsUnnamedArray(baseNamed)

				if namedToArray || namedSiblingArrays {
					argExpr := v.convExpr(arg, nil)

					if v.needsParentheses(arg) {
						argExpr = fmt.Sprintf("(%s)", argExpr)
					}

					// A deref-aliased pointer param/receiver renders as the wrapper VALUE — a
					// ref-local alias of the real storage, so its `Value` property already runs in
					// place. A genuine box (field box `Ꮡs.of(…)`, heap box `Ꮡss`) derefs through the
					// ref-returning ж<T>.Value first — NOT `~` (operator ~ returns a COPY, which would
					// materialize a virgin backing on the temp and orphan every write — the
					// SetCanonicalBytes-on-a-fresh-Scalar shape).
					if !v.exprIsDerefAliasedPointer(arg) {
						argExpr = fmt.Sprintf("%s.Value", argExpr)
					}

					if namedToArray {
						return fmt.Sprintf("%s(%s.Value)", AddressPrefix, argExpr)
					}

					baseName := convertToCSTypeName(v.getAliasQualifiedTypeName(resultElem, false))

					return fmt.Sprintf("%s((%s)(%s.Value))", AddressPrefix, baseName, argExpr)
				}

				if namedToNamed || namedToBasic || basicToNamed {
					// Go's `(*U)(p)` is a pointer REINTERPRET: the derived pointer names p's OWN
					// storage, so a write through it is visible through p. The value-convert-and-
					// re-box emission below cannot be that — it converts the POINTEE and boxes the
					// RESULT, so the derived pointer addresses a copy and every write through it is
					// silently discarded. Correct only where nothing writes, which is not a property
					// of the conversion and so cannot be its default: gob's
					// `fmt.Sscanf(…, (*int)(g))` (`g *Gobber`, `type Gobber int`) is the shape that
					// proved it — the scan landed in the throwaway box, `g` never changed, and the
					// decoder returned 0 for 23. Route through golib's ALIASING reinterpret, which
					// is where the "can the managed model express this alias?" decision already
					// lives (PointerExtensions.Reinterpret / ReinterpretAliasesStorage) and which is
					// already this file's answer for the same conversion in a deref or raw-address
					// context. It reports false for the shapes that must keep the re-box — an
					// IDENTITY conversion, and an ARRAY pointee, whose lazily-materialized backing
					// store a storage reinterpret bypasses (the chain-defined `type pallocBits
					// pageBits` pair the written-RHS gate above deliberately leaves here) — so those
					// fall through unchanged.
					if emission, ok := v.reinterpretManagedEmission(callExpr, arg); ok {
						return emission
					}

					baseName := convertToCSTypeName(v.getAliasQualifiedTypeName(resultElem, false))
					var argExpr string

					if unary, ok := arg.(*ast.UnaryExpr); ok && unary.Op == token.AND && basicToNamed {
						// `(*stringReader)(&str)` — the address-of collapses with the value
						// deref: the conversion operates on `str` directly, then re-boxes.
						// (Restricted to the new basicToNamed arm so the long-guarded
						// namedToNamed/namedToBasic emissions stay byte-identical.)
						argExpr = v.convExpr(unary.X, nil)
					} else {
						argExpr = v.convExpr(arg, nil)

						// The [GoType] VALUE conversion `(Base)(Def)` operates on the underlying VALUE. A
						// deref-aliased pointer param/receiver already renders as that value (`Δp`/`head`), so
						// it casts directly. But a genuine pointer expression — a call result (`newMarkBits(…)`
						// returning `*gcBits`), a local box, a pointer field — renders as the box `ж<Def>`,
						// which has no conversion to the Base VALUE (CS0030, runtime/pinner
						// `(*pinnerBits)(newMarkBits(…))`). Dereference the box first so the value conversion
						// binds. Both forms then box a COPY (`Ꮡ`) — the shared-underlying value has no
						// write-through to lose (the atomic intrinsics are asm stubs).
						if !v.exprIsDerefAliasedPointer(arg) {
							argExpr = fmt.Sprintf("%s%s", PointerDerefOp, argExpr)
						}
					}

					return fmt.Sprintf("%s((%s)(%s))", AddressPrefix, baseName, argExpr)
				}
			}
		}

		// Go's two slice-to-array conversions are DIFFERENT conversions, and each gets its own
		// golib entry — both panic Go-style on a short slice:
		//
		//   - the 1.20 VALUE form `[4]byte(slice)` yields a COPY (netip AddrFromSlice, CS1955 ×5)
		//     → array<T>(slice<T>, nint);
		//   - the 1.17 POINTER form `(*[4]byte)(slice)` yields a pointer INTO the slice — "the
		//     slice and array share their underlying array" (Go spec) → array<T>.Alias, an
		//     ALIASING window. It boxed the copy ctor until 2026-07-31, which silently discarded
		//     every write through the pointer: image/png's cbTCA8 row loop writes each pixel
		//     through `d := (*[4]byte)(dst)`, so a non-opaque RGBA source encoded as an all-zero
		//     image (TestWriteRGBA).
		//
		// A NAMED-over-array target falls through unchanged (none in the corpus).
		if _, argIsSlice := v.getType(arg, false).Underlying().(*types.Slice); argIsSlice {
			if resultArr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Array); ok {
				elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(resultArr.Elem(), false))
				return fmt.Sprintf("new array<%s>(%s, %d)", elemName, v.convExpr(arg, nil), resultArr.Len())
			}

			if resultPtr, ok := v.info.TypeOf(callExpr).(*types.Pointer); ok {
				if resultArr, ok := types.Unalias(resultPtr.Elem()).(*types.Array); ok {
					elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(resultArr.Elem(), false))
					return fmt.Sprintf("%s(array<%s>.Alias(%s, %d))", AddressPrefix, elemName, v.convExpr(arg, nil), resultArr.Len())
				}
			}
		}

		targetTypeName = convertToCSTypeName(targetTypeName)

		// A `string(x)` conversion the escape pass marked emits the stack-only `sstring` — a zero-copy
		// view over x's bytes — instead of the heap `@string` copy. Two sources feed this: an eligible
		// `s := string(x)` local (visitAssignStmt sets the transient flag around its one RHS conversion),
		// and an unnamed `string(x) == "…"` comparison operand (keyed per-CallExpr in sstringConvExprs).
		// Done after the Go-name → C#-name mapping above, where the target is the `@string` C# name.
		if targetTypeName == "@string" && (v.emitStringConvAsSString || v.sstringConvExprs[callExpr]) {
			targetTypeName = "sstring"
			v.emitStringConvAsSString = false
		}

		// A conversion TARGET that is a foreign RENAMED type routes through the recorded
		// alias (`(syscallꓸHandle)fd`, not the nonexistent `(Δsyscall.Handle)fd` -
		// CS0426, internal/poll DupCloseOnExec).
		if aliased, ok := v.foreignAliasedTypeName(v.info.TypeOf(callExpr)); ok {
			targetTypeName = aliased
		}

		// PEEPHOLE — `uintptr(unsafe.Pointer(x))`, Go's syscall idiom, materialized a DEAD object.
		// Mark the inner conversion so its `new @unsafe.Pointer(…)` wrapper is never built; the
		// operand then converts to uintptr directly, which is the same value by the same operator.
		v.markDeadUnsafePointerBox(callExpr, arg)

		// A CONVERSION is transparent to the capture-copy hoist. Its operand can be a capturing
		// func literal — `HandlerFunc(func(rw, req){ … conn … })` handed to `go Serve(ls, …)`
		// (net/http serve_test) — whose snapshot declarations (`var connʗ1 = conn;`) are
		// STATEMENTS and cannot stand in the argument list the conversion renders into. Every
		// other position threads the enclosing statement's sink to convFuncLit; this one rendered
		// its operand with NO contexts at all, so the wrapper made the literal invisible to the
		// hoist and the decls landed inline (CS1003 + CS1026 + CS1002 + CS1513, seven sites across
		// net/http's client_test and serve_test — the recorded `CS1002 ';' expected` wall). Adopt
		// the ambient target exactly as the `&composite` and composite-literal arms do in convExpr.
		//
		// convFuncLit consults context.deferredDecls FIRST and v.hoistedDecls SECOND, so this is
		// needed only where a statement supplies the explicit builder and no ambient one — the
		// `go`, `defer` and `return` forms. Gated on a non-nil sink, which leaves every other
		// conversion rendering with the same nil contexts it has always had.
		var convContexts []ExprContext

		if context.deferredDecls != nil {
			convLambdaContext := DefaultLambdaContext()
			convLambdaContext.deferredDecls = context.deferredDecls
			convContexts = []ExprContext{convLambdaContext}
		}

		expr := v.checkForImplicitConversion(funcType, arg, targetTypeName, convContexts)

		// A conversion whose TARGET is a non-empty INTERFACE and whose SOURCE is a POINTER —
		// `image.Image(dst)` with `dst *image.RGBA` (image/draw) — is Go's ordinary
		// pointer-to-interface conversion in explicit clothing: route it through the same
		// machinery as implicit interface casts (the exported/local ж-adapter), re-rendering
		// the argument in its BOX form (the adapter wraps the box). The constructor fallback
		// instantiated the interface (`new image.Image(dst)`, CS0144 ×2). Interface and value
		// sources keep their existing plain-cast/partial-impl routes (no churn).
		if callTargetType := v.info.TypeOf(callExpr); callTargetType != nil {
			if targetIsIface, isEmpty := isInterface(callTargetType); targetIsIface && !isEmpty {
				if argPtrType, srcIsPtr := v.getType(arg, false).(*types.Pointer); srcIsPtr {
					identContext := DefaultIdentContext()
					identContext.isPointer = true

					// The result is CAST to the interface: the adapter implements its members
					// EXPLICITLY, so a chained member access on the conversion result
					// (`CrossPkgLib.Labeled(sp).Label()`) cannot bind on the adapter class
					// itself (CS1929) — the cast is an implicit reference conversion that
					// exposes the interface view.
					return fmt.Sprintf("((%s)%s)", targetTypeName, v.convertToInterfaceType(callTargetType, argPtrType, v.convExpr(arg, []ExprContext{identContext})))
				}

				// The VALUE mirror, for a named non-interface VALUE source wherever it is
				// declared. Two shapes, one route:
				//
				//   - a FOREIGN source — `crypto.SignerOpts(sigHash)` with `sigHash crypto.Hash`
				//     (crypto/tls, CS0030 ×4). The plain cast cannot bind: a foreign value type
				//     implements its interfaces via extension methods (never structurally), and
				//     this assembly cannot partial it. convertToInterfaceType records + references
				//     the LOCAL value adapter (the both-foreign arm; syscall.Signal→os.Signal
				//     precedent) or no-ops into the plain spelling when the defining assembly
				//     already implements the pair.
				//   - a LOCAL source — `crypto.Signer(private)` with `type PrivateKey []byte`
				//     (crypto/ed25519, CS0030 ×2) and `pinUnexpMeth(EmbedWithUnexpMeth{})`
				//     (internal/reflectlite). A local type CAN be partial'd to declare the
				//     interface, and that is exactly why it must route here: the partial is
				//     go2cs-gen's, minted from an `[assembly: GoImplement<T, Iface>]` record, and
				//     a plain cast records NOTHING. So the cast had nothing to bind to whenever no
				//     other site recorded the pair — which is every pair the speculative recorder
				//     declines: an interface in ANOTHER assembly (recordSamePackageImplements only
				//     pairs two locals) and an UNEXPORTED local interface (its exported gate,
				//     load-bearing because a record is a cross-assembly contract).
				//
				// Framed by SYNTAX rather than by locality, this is one rule: Go's `Iface(x)` and
				// `var i Iface = x` are the same conversion, and the assignment form has always
				// routed through convertToInterfaceType (visitValueSpec). A conversion's emission
				// must not depend on which of Go's two spellings the source used. For a local
				// non-func value source the route is record-only — it returns the expression
				// unchanged — so the corpus text does not move; a local named FUNC source is the
				// one shape whose emission does, and correctly: a C# delegate cannot be a partial
				// struct, so the generator emits the `ᴠ` value adapter the arm now references.
				//
				// The outer interface cast stays load-bearing exactly like the pointer arm: `var
				// signOpts = …` must type as the INTERFACE, not the adapter class — each tls site
				// reassigns signOpts to a different adapter two lines later (CS0029 hazard).
				//
				// An INTERFACE source keeps its plain cast: interface→interface is the separate
				// recordableInterface class, whose adapter-wrapping emission is a different arc.
				if argType := v.getType(arg, false); argType != nil {
					if named, ok := types.Unalias(argType).(*types.Named); ok && !types.IsInterface(named) {
						if pkg := named.Obj().Pkg(); pkg != nil {
							return fmt.Sprintf("((%s)%s)", targetTypeName, v.convertToInterfaceType(callTargetType, argType, v.convExpr(arg, nil)))
						}
					}
				}
			}
		}

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency. Although C# allows circular dependencies, NuGet does not. If the
		// target happens to not be an unsafe pointer, the cast is still safe since all pointer
		// types support this cast operation.
		//
		// The same routing is required for a pointer-type conversion `(*T)(p)` whose SOURCE is a raw
		// address — an unsafe.Pointer or uintptr — even when the deref path did not set isPointerCast.
		// `unsafe.Pointer` is the golib `Pointer : ж<uintptr>`, so `(ж<T>)p` needs the two user-defined
		// conversions Pointer→uintptr→ж<T>, which C# will not chain in a single cast (CS0030). Routing
		// through uintptr — `(ж<T>)(uintptr)(p)` — reads the T at p's address via golib's
		// `implicit operator ж<T>(uintptr) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => val`), which
		// IS Go's `*(*T)(p)` semantics for native memory. Two shapes miss isPointerCast and need this: the
		// bare-argument form `atomicwb((*unsafe.Pointer)(ptr), …)` and the extra-paren deref
		// `*((*unsafe.Pointer)(k))` (convStarExpr's CallExpr branch sees a ParenExpr, not the CallExpr).
		// The pointer-to-NAMED-type value conversion `(*atomic.Uint32)(counterPtr)` is handled and returned
		// above (its arg is a *types.Pointer, not a raw address), so it never reaches here.
		//
		// NOTE isPointerCast means only "this conversion is the operand of a deref" — it does NOT imply the
		// SOURCE is an address, and this bridge is only ever correct for one that is. The IDENTITY reinterpret
		// `*(*T)(p)` with p already `*T` is the shape that made the difference visible: its source is a managed
		// box (or, for a deref-aliased parameter, that box's VALUE alias), for which the `(uintptr)` leg has no
		// conversion at all (CS0030). It is intercepted upstream by pointerReinterpretIdentitySource and never
		// reaches here. Non-identity typed-pointer sources whose element types differ but share an underlying
		// (a tag-differing struct, a named/unnamed array or struct pair) are NOT all covered by the re-box
		// routes above and still land here; see ConversionStrategies-Reference.
		if context.isPointerCast || v.isRawAddressPointerConversion(callExpr, arg) {
			// A genuine reinterpret whose SOURCE is a managed Go pointer — `(*rtype)(unsafe.
			// Pointer(t))` with t `*abi.Type` — aliases that box instead of round-tripping through
			// its numeric address, which would neither keep the pointee alive nor survive the
			// collector moving it. Placed HERE, at the address route it replaces, rather than with
			// the identity elision upstream: the re-box routes above (a named/unnamed array or
			// struct pair, a defined type over another package's type) already render their own
			// conversions correctly, and intercepting earlier would divert those too — including
			// named ARRAY wrappers, whose lazily-materialized backing store a storage reinterpret
			// silently bypasses. See pointerReinterpretManagedSource.
			if emission, ok := v.reinterpretManagedEmission(callExpr, arg); ok {
				return emission
			}

			// The array-target sibling: `(*[N]T)(unsafe.Pointer(p))` over a `*T` aliases the
			// storage p is an element of instead of punning an `array<T>` out of its data.
			if emission, ok := v.arrayPointerAliasEmission(callExpr, arg); ok {
				return emission
			}

			return fmt.Sprintf("(%s)(uintptr)(%s)", targetTypeName, expr)
		}

		// A NAMED-over-pointer target from a RAW-ADDRESS source — `syscall.Pointer(unsafe.
		// Pointer(x))`, where `type Pointer *struct{}` emits as a ж<EmptyStruct>-wrapping
		// class — needs the same uintptr reinterpret hop as the `(*T)(p)` form above, then
		// the named type's own operator from its underlying box:
		// `((Pointer)(ж<EmptyStruct>)(uintptr)(x))`. The direct cast chained two
		// user-defined conversions (CS0030 ×6, internal/poll's WSAMsg.Name assignments).
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if ptrUnder, ok := named.Underlying().(*types.Pointer); ok {
				if argType := v.info.TypeOf(arg); argType != nil {
					if basic, ok := argType.Underlying().(*types.Basic); ok && (basic.Kind() == types.UnsafePointer || basic.Kind() == types.Uintptr) {
						// The OPAQUE-pointer MINT: when the target's pointee is the empty struct
						// and the referent is statically in hand, preserve it — the numeric chain
						// below is where crypto/x509's pvExtraPolicyPara lost its box.
						if emission, ok := v.opaquePointerMintEmission(callExpr, arg, targetTypeName); ok {
							return emission
						}

						elemCS := convertToCSTypeName(v.getAliasQualifiedTypeName(ptrUnder.Elem(), false))
						return fmt.Sprintf("((%s)(%s<%s>)(uintptr)(%s))", targetTypeName, PointerPrefix, elemCS, expr)
					}
				}
			}
		}

		// unsafe.Pointer(ptr) where ptr is a Go pointer (`*T`, emitted as the managed box `ж<T>`).
		// A managed box has no conversion to the numeric Pointer (`ж<uintptr>`), so a plain cast is
		// CS0030 — e.g. `unsafe.Pointer(&u.value)` → `(@unsafe.Pointer)(Ꮡu.of(…))`. Mint through
		// the referent-RETAINING helper instead: `@unsafe.Pointer.FromBox(box)` carries the same
		// transient numeric address the old `FromRef(ref (box).Value)` form produced AND the box
		// itself, so the bare-unsafe.Pointer atomic primitives (StorepNoWB/Loadp — the I5 ruling:
		// FromRef flattened the alias to a number in a fresh box and the store never landed) can
		// recover the referent and reach the very slot the pointer names. (`uintptr`/
		// `unsafe.Pointer` args are not boxes and keep the implicit-cast path below; only a genuine
		// pointer arg needs this.) The numeric address is still not GC-stable — the same caveat as
		// every unsafe.Pointer-as-uintptr use; the RETAINED box is what survives a move.
		if targetTypeName == "@unsafe.Pointer" {
			// A NAMED-numeric arg whose underlying is uintptr — `unsafe.Pointer(v)` where v is
			// `type gclinkptr uintptr` (runtime malloc/mcache/stack: the allocator's span-address
			// arithmetic type). The [GoType] wrapper only converts named↔underlying, and golib's
			// Pointer only converts from uintptr, so the direct cast needs a two-op chain C# will
			// not build (CS0030); hop through the underlying — `((@unsafe.Pointer)(uintptr)v)`.
			// (Go permits exactly uintptr-underlying types here, so the gate matches the language.)
			if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok {
				if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Kind() == types.Uintptr {
					if v.needsParentheses(arg) {
						return fmt.Sprintf("((@unsafe.Pointer)(uintptr)(%s))", expr)
					}

					return fmt.Sprintf("((@unsafe.Pointer)(uintptr)%s)", expr)
				}

				// A NAMED-over-POINTER arg — `unsafe.Pointer(result.Addr)` where Addr is
				// syscall.Pointer (`type Pointer *struct{}`, net lookup_windows CS0030 ×2):
				// the named wrapper's uintptr bridge provides the first leg, golib Pointer
				// converts from uintptr — same two-leg hop as the named-numeric arm above.
				if _, isPtrUnder := argNamed.Underlying().(*types.Pointer); isPtrUnder {
					return fmt.Sprintf("((@unsafe.Pointer)(uintptr)(%s))", expr)
				}
			}

			if _, isPtr := v.info.TypeOf(arg).(*types.Pointer); isPtr {
				// A pointer PARAMETER carries its BOX (`Ꮡp`) alongside the value alias, and golib's
				// `implicit operator uintptr(ж<T>)` already yields exactly the address Go wants: 0 for
				// a nil box, the aliased address for a reinterpreted native pointer, the pinned
				// storage otherwise. Build the Pointer from the box so `unsafe.Pointer(p)` never
				// DEREFERENCES p — Go's `unsafe.Pointer(nil)` is the 0 address, and a nil out-pointer
				// is idiomatic in the syscall wrappers (`DuplicateHandle(…, nil, …,
				// DUPLICATE_CLOSE_SOURCE)` closes a handle without returning one). The FromRef form
				// instead read the value alias, whose entry-time `ref var p = ref Ꮡp.Value` threw a
				// nil-pointer panic before the call — the hang/crash behind os/exec child creation.
				// Rendering the box also removes the bare VALUE reference from the body, so an
				// otherwise-unused alias is then skipped as dead (visitFuncDecl's
				// bodyReferencesIdentAsValue) and the entry deref disappears with it. Pointer params
				// whose pointee is a basic or struct type already emitted this box form.
				if v.exprIsDerefdPointerParam(arg) {
					identContext := DefaultIdentContext()
					identContext.isPointer = true

					return v.unsafePointerBoxEmission(callExpr, arg, v.convExpr(arg, []ExprContext{identContext}))
				}

				// A deref-aliased pointer RECEIVER renders as the pointed-to VALUE alias
				// (`ref var pc0 = ref Ꮡpc0.Value`) and has NO box to address — a value-ref receiver is
				// `this ref T r` — so its address must come from the alias itself. This is the one
				// mint that cannot retain (FromRef sees only the ref); a recorded residual, loud at
				// the primitives if ever reached.
				if v.exprIsDerefAliasedPointer(arg) {
					return fmt.Sprintf("@unsafe.Pointer.FromRef(ref %s)", expr)
				}

				return fmt.Sprintf("@unsafe.Pointer.FromBox(%s)", expr)
			}
		}

		if targetTypeName == "@string" {
			// Check if it is a generic type parameter - Go will have already
			// validated constraint, so we can just cast to string directly
			if tp, ok := v.getType(arg, false).(*types.TypeParam); ok {
				// A `string | []byte` UNION-constrained value takes golib's ToGoString
				// EXTENSION rather than the @string constructor (bytealg's Rabin-Karp
				// compares `string(s[i:j]) == string(sep)`). The constructor can only accept
				// the sequence as an IByteSeq<byte> — C# has no generic constructor — which
				// boxes the caller's struct on every conversion; the extension takes the
				// concrete type as its own type parameter, so the argument passes unboxed.
				// See the matching `[]byte(s)` case below, and golib ByteSeqExtensions.
				if typeParamIsStringByteUnion(tp) {
					return fmt.Sprintf("%s.ToGoString()", expr)
				}

				return fmt.Sprintf("new %s(%s)", targetTypeName, expr)
			}

			// A NAMED-slice arg converting to string — `string(buf)` where `type
			// appendSliceWriter []byte` (strings replace.go) — hops through the written
			// underlying: the [GoType] wrapper converts only to slice<byte>, and
			// slice<byte>→@string is a SECOND user conversion C# won't chain (CS0030).
			//
			// When the ELEMENT is itself a defined type (`string(b)` over `[]myByte`, named
			// slice or not) the hop is not enough either — `slice<myByte>` has no conversion
			// to @string at all — and the elements are projected back to their underlying
			// basic first. See stringSliceConversions.go for both ends of this family.
			if argType := v.info.TypeOf(arg); argType != nil {
				if sliceType, ok := argType.Underlying().(*types.Slice); ok && isByteOrRuneSlice(sliceType) {
					sliceExpr := expr
					_, argIsNamed := types.Unalias(argType).(*types.Named)

					if argIsNamed {
						sliceExpr = fmt.Sprintf("(%s)%s", v.getCSharpTypeName(sliceType), expr)
					}

					if projected, ok := v.byteSliceToStringConversion(sliceType, sliceExpr); ok {
						return projected
					}

					// A plain element over an UNNAMED slice needs no interception —
					// `slice<byte>`→@string is a single conversion — so it keeps falling
					// through to the general cast below, byte-identically.
					if argIsNamed {
						return fmt.Sprintf("((@string)%s)", sliceExpr)
					}
				}
			}
		}

		// A conversion whose TARGET is an integer-constrained TYPE PARAMETER — `Int(x)` in
		// rand.N[Int intType] (the banked E(100) family; also reflect rangeNum's `T(0)`/`T(v)`)
		// — cannot be a C# cast (CS0030). Route through golib's runtime-typed ConvertToType<T>.
		// An argument that is ITSELF a type parameter first drops to uint64 (no overload binds
		// a bare type parameter).
		if targetTP, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.TypeParam); ok && typeParamIsInteger(targetTP) {
			if argTP, ok := types.Unalias(v.info.TypeOf(arg)).(*types.TypeParam); ok && typeParamIsInteger(argTP) {
				return fmt.Sprintf("ConvertToType<%s>(ConvertToUInt64<%s>(%s))", targetTypeName, v.getCSharpTypeName(argTP), expr)
			}

			return fmt.Sprintf("ConvertToType<%s>(%s)", targetTypeName, expr)
		}

		// The MIRROR: a conversion FROM an integer-constrained type parameter TO a basic integer
		// — `uint64(n)` in rand.N[Int] (CS0030). Drop through golib's ConvertToUInt64 (sign-/
		// zero-extension through the instantiated type — Go's exact widening), then a plain
		// numeric cast when the target is not uint64 itself.
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok && targetBasic.Info()&types.IsInteger != 0 {
			if argTP, ok := types.Unalias(v.info.TypeOf(arg)).(*types.TypeParam); ok && typeParamIsInteger(argTP) {
				inner := fmt.Sprintf("ConvertToUInt64<%s>(%s)", v.getCSharpTypeName(argTP), expr)

				if targetBasic.Kind() == types.Uint64 {
					return inner
				}

				return fmt.Sprintf("(%s)%s", targetTypeName, inner)
			}
		}

		// A conversion to a NAMED FUNC type — `metricReader(read)` where `type metricReader
		// func() uint64` (runtime metrics.go) — targets a C# DELEGATE declaration
		// (`internal delegate uint64 metricReader();`). Distinct delegate types have no cast
		// conversion (CS0030 from `Func<ulong>`); C# converts via DELEGATE CREATION:
		// `new metricReader(read)` (accepts a compatible delegate or method group).
		if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				// `T(nil)` for a func type is the TYPED NIL delegate, and delegate CREATION cannot
				// express it: `new HandlerFunc(default!)` asks for a method group or a delegate value
				// (CS0149) and gives `default` no target type (CS8716) — net/http's server_test.go
				// writes exactly `HandlerFunc(nil)`. A cast is the whole conversion here, since the
				// operand carries no delegate to wrap.
				if argIsUntypedNil(arg, v.info) {
					return fmt.Sprintf("default(%s)!", targetTypeName)
				}

				// A function LITERAL operand (`NamedType(func(...) {...})`) is provably never nil
				// — it constructs a fresh closure at the conversion site — so the direct `new
				// %s(%s)` delegate-copy constructor is both correct and the cheaper emission; the
				// nil-safety wrapper below exists for operands that CAN be nil, not this one.
				if _, isFuncLit := ast.Unparen(arg).(*ast.FuncLit); isFuncLit {
					return fmt.Sprintf("new %s(%s)", targetTypeName, expr)
				}

				// A bare reference to a package-level FUNCTION (`reader(readTen)`, MethodExpression)
				// is provably never nil too — a Go func declaration cannot be unset, only a
				// VARIABLE or PARAMETER holding a func value can be nil. Same fast direct path.
				if ident, isIdent := ast.Unparen(arg).(*ast.Ident); isIdent {
					if _, isFuncObj := v.info.ObjectOf(ident).(*types.Func); isFuncObj {
						return fmt.Sprintf("new %s(%s)", targetTypeName, expr)
					}
				}

				// Any OTHER operand is not a nil LITERAL, but it can still be nil at RUNTIME — a
				// `handler func(...)` parameter reaching `HandlerFunc(handler)` (net/http's own
				// HandleFunc) is exactly this. `new %s(%s)` is the .NET delegate-copy
				// CONSTRUCTOR: eager, and it throws for a null source before Go's own nil check
				// downstream ever runs — Go's conversion itself never panics. Route through the
				// nil-safe golib helper so a null source yields null, matching Go, and only a
				// non-null source pays for construction.
				sourceTypeName := v.getCSharpTypeName(v.info.TypeOf(arg))

				return fmt.Sprintf("NilSafeDelegateConversion<%s, %s>(%s)", targetTypeName, sourceTypeName, expr)
			}
		}

		// A conversion of a string LITERAL to a named type whose underlying is `string`
		// (e.g. `errorString("makeslice: len out of range")`, `type errorString string`): the
		// literal renders as a `u8` ReadOnlySpan<byte>, which has no conversion to the named type
		// (CS0030). Route it through `@string` — which has an implicit conversion FROM the u8 span
		// and TO which the named type converts — `((errorString)(@string)"…"u8)`.
		if basicLit, ok := arg.(*ast.BasicLit); ok && basicLit.Kind == token.STRING && targetTypeName != "@string" {
			if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
				if basic, ok := named.Underlying().(*types.Basic); ok && basic.Kind() == types.String {
					return fmt.Sprintf("((%s)(@string)%s)", targetTypeName, expr)
				}
			}
		}

		// The BYTE/RUNE-slice sibling of the block above: a STRING converting to a NAMED type whose
		// underlying is `[]byte`/`[]rune` — `htmlSig("<!DOCTYPE HTML")`, `type htmlSig []byte`
		// (net/http sniff.go's signature table, CS0030 ×17). Neither a `u8` span nor an `@string`
		// reaches the wrapper in ONE hop (its [GoType] operator takes exactly its underlying slice,
		// and C# chains at most one user-defined conversion). Materialize the underlying slice the
		// way the plain `[]byte("…")` conversion does and let the wrapper's own operator apply:
		// `((htmlSig)slice<byte>((@string)"…"u8))`.
		//
		// Any string-typed operand qualifies, not just a literal — a `string` VARIABLE and a DEFINED
		// string are the same two-hop problem, and rendered as the bare cast they used to fall
		// through to they were CS0030 exactly as the literal was. The operand's own `@string` step,
		// and a DEFINED element type (`type S []myByte`, which no conversion reaches from
		// `slice<byte>` at all), both live in stringSliceConversions.go.
		if targetTypeName != "@string" {
			if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
				if sliceType, ok := named.Underlying().(*types.Slice); ok && isByteOrRuneSlice(sliceType) {
					if argType := v.info.TypeOf(arg); argType != nil && isStringTyped(argType) {
						return fmt.Sprintf("((%s)%s)", targetTypeName, v.stringToByteSliceConversion(sliceType, arg, expr))
					}
				}
			}
		}

		// A conversion to a MANUALLY-converted type (see manualTypeOperations.go) from an
		// unsafe.Pointer — `guintptr(unsafe.Pointer(newg))` (runtime proc.go). The manual type
		// stores the managed referent DIRECTLY, so the numeric cast chain
		// `(Δguintptr)(uintptr)new @unsafe.Pointer(newg)` would lose it; unwrap the
		// unsafe.Pointer conversion and construct from its operand — `new Δguintptr(newg)` —
		// the referent-carrying box expression. (Every current call site passes a pointer LOCAL
		// or an explicit box; a deref-aliased param operand would need its box form and will
		// surface as a loud compile error, not a silent number.) Non-pointer args (e.g. a zero
		// literal) fall through to the named-numeric route against the manual type's operators.
		if named, ok := v.info.TypeOf(callExpr).(*types.Named); ok {
			if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg && v.isManualType(obj.Name()) {
				if argCall, ok := arg.(*ast.CallExpr); ok && v.callExprIsTypeConversion(argCall) && len(argCall.Args) == 1 {
					if argBasic, ok := v.info.TypeOf(argCall).(*types.Basic); ok && argBasic.Kind() == types.UnsafePointer {
						return fmt.Sprintf("new %s(%s)", targetTypeName, v.convExpr(argCall.Args[0], nil))
					}
				}
			}
		}

		// A conversion to a NAMED numeric type (`type arenaIdx uint`, `type traceArg uint64`) whose
		// arg is not already exactly its underlying basic — `arenaIdx(1 << b)` (int arg),
		// `traceArg(procs)` (int32 arg). The `[GoType]` conversion operator only converts between the
		// named type and its EXACT underlying (`nuint` / `uint64`), so a plain `(arenaIdx)(intExpr)`
		// is CS0030. Coerce through the underlying first — `((arenaIdx)(nuint)(1 << b))` — a numeric
		// C# cast that is exactly Go's conversion semantics. Skipped when the arg is already the
		// A POINTER reinterpret from a named-slice pointer to its underlying-slice pointer —
		// net fd_windows.go's `fd.pfd.Writev((*[][]byte)(buf))` with `buf *Buffers` (`type
		// Buffers [][]byte`): ж<Buffers> and ж<slice<slice<byte>>> are unrelated
		// instantiations (CS0030). Project a ж-view over the wrapper's backing field —
		// `Ꮡbuf.of(Buffers.Ꮡm_value)` — TRUE aliasing: header writes through the view
		// (Writev's consume reslicing) land on the original wrapper.
		if targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer); ok {
			if _, targetElemIsNamed := types.Unalias(targetPtr.Elem()).(*types.Named); !targetElemIsNamed {
				if targetSlice, ok := targetPtr.Elem().Underlying().(*types.Slice); ok {
					if argPtr, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); ok {
						if argNamed, ok := types.Unalias(argPtr.Elem()).(*types.Named); ok {
							if argUnderSlice, ok := argNamed.Underlying().(*types.Slice); ok && types.Identical(argUnderSlice, targetSlice) {
								ptrExpr := expr

								if identArg, ok := arg.(*ast.Ident); ok {
									ptrContext := DefaultIdentContext()
									ptrContext.isPointer = true
									ptrExpr = v.convIdent(identArg, ptrContext)
								}

								namedCS := convertToCSTypeName(v.getAliasQualifiedTypeName(argNamed, false))
								return fmt.Sprintf("%s.of(%s.Ꮡm_value)", ptrExpr, namedCS)
							}
						}
					}
				}
			}
		}

		// The MIRROR reinterpret: an underlying-slice pointer to a NAMED-slice pointer —
		// log/slog/internal/buffer's `(*Buffer)(&b)` with `type Buffer []byte` and a local
		// `b []byte`. A named-wrapper box CONTAINS the underlying (the forward arm above projects
		// it out via `.of(Named.Ꮡm_value)`), but a bare-slice box does NOT contain a wrapper, so
		// the reverse cannot project. A bare `(ж<Buffer>)(Ꮡ(b))` cast is CS0030 (unrelated ж
		// instantiations), so this went through golib's STORAGE reinterpret instead:
		// `Ꮡb.Reinterpret<slice<byte>, Buffer>()` re-views the SAME slot as the wrapper — the
		// wrapper is a single-field struct over the slice header, which is exactly the layout
		// correspondence ReinterpretAliasesStorage recognizes, so the managed alias arm engages
		// and every write through the derived pointer lands on the addressed slice.
		//
		// It previously CONSTRUCTED a wrapper box over a COPY (`Ꮡ(new Buffer(b))`), on the stated
		// assumption that "the reinterpret is used through the returned pointer". That assumption is
		// false for the corpus's most important instance of the shape: log/slog's `commonHandler.
		// withAttrs` takes `(*buffer.Buffer)(&h2.preformattedAttrs)` precisely so the pre-formatted
		// attribute bytes it appends land in h2's own field. Against a copy they landed nowhere —
		// `WithAttrs` silently dropped every attribute, and the handler still advanced groupPrefix /
		// nOpenGroups, so the JSON it emitted afterward was unbalanced. It is the root behind four
		// testing/slogtest rows (WithAttrs, multi-With, empty-group-record, resolve-WithAttrs).
		if targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer); ok {
			if targetNamed, ok := types.Unalias(targetPtr.Elem()).(*types.Named); ok {
				if _, targetIsSlice := targetNamed.Underlying().(*types.Slice); targetIsSlice {
					if argPtr, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); ok {
						if _, argElemIsNamed := types.Unalias(argPtr.Elem()).(*types.Named); !argElemIsNamed {
							if argSlice, ok := argPtr.Elem().Underlying().(*types.Slice); ok && types.Identical(argSlice, targetNamed.Underlying()) {
								// The same emission the raw-address route uses for a managed-pointer
								// source (see pointerReinterpretManagedSource); it renders the arg in
								// its BOX form, which is what an `&x` / pointer-parameter source needs.
								if emission, ok := v.reinterpretManagedEmission(callExpr, arg); ok {
									return emission
								}
							}
						}
					}
				}
			}
		}

		// A conversion to a pointer-to-NAMED-ARRAY whose source is a FRESH allocation —
		// reflect's `(*MyBytesArray0)(new([0]byte))` (all_test.go:4501). It emitted the bare
		// `(ж<MyBytesArray0>)Ꮡ(new array<byte>(0))` cast, which is CS0030: ж<> is not variant, so
		// ж<array<byte>> and ж<MyBytesArray0> are unrelated instantiations.
		//
		// CONSTRUCT the wrapper and take ITS address, which is exactly what the sibling
		// composite-literal spelling `&MyBytesArray{1,2,3,4}` already emits one line below in the
		// same reflect table. Restricted to a fresh allocation ON PURPOSE, because for a fresh one
		// nothing else can reference the storage, so constructing over it is indistinguishable
		// from aliasing it.
		//
		// The `(*Named)(&existing)` spelling deliberately KEEPS its CS0030 rather than taking this
		// emission. That was measured, not assumed: a named-ARRAY wrapper's generated field is
		// `array<T>?` — a Nullable, so it is BOTH larger than the `array<T>` it wraps and a
		// different shape — and golib's alias gate refuses it on the size test alone. (The named
		// SLICE wrapper's field is a bare `slice<T>`, identical in size and layout, which is why
		// the reinterpret arm above is correct there and banked.) Widening that arm to arrays
		// therefore compiles but hands back a raw-address box whose deref fabricates a managed
		// reference: measured as an AccessViolationException inside `array<byte>.get_Item` on the
		// FIRST indexed read. So for `&existing` there is no correct emission available yet, and a
		// loud CS0030 is the honest answer — constructing there would silently write through a
		// copy for whole-value assignment, which is the log/slog `WithAttrs` bug this file's
		// history already paid for.
		if targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer); ok {
			if targetNamed, ok := types.Unalias(targetPtr.Elem()).(*types.Named); ok {
				if targetArr, ok := targetNamed.Underlying().(*types.Array); ok {
					if argCall, isCall := arg.(*ast.CallExpr); isCall && len(argCall.Args) == 1 {
						if newIdent, isIdent := argCall.Fun.(*ast.Ident); isIdent && newIdent.Name == "new" && v.identIsUniverseBuiltin(newIdent) {
							// Go only permits the conversion when the pointee underlyings are
							// identical, but assert it rather than inherit it: this arm mints a
							// wrapper over a zero value, so a mismatch would construct silently.
							if argPtr, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); ok && types.Identical(argPtr.Elem().Underlying(), targetArr) {
								elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(targetArr.Elem(), false))
								namedCS := convertToCSTypeName(v.getAliasQualifiedTypeName(targetNamed, false))

								return fmt.Sprintf("Ꮡ(new %s(new array<%s>(%s)))", namedCS, elemName, csNintLiteral(targetArr.Len()))
							}
						}
					}
				}
			}
		}

		// A conversion between two NAMED SLICE types sharing an identical underlying — tar's
		// `sparseElem(s[i*24:])` where s is sparseArray (both `[]byte`): the named-slice
		// slicing wrapper-return makes the arg the NAMED wrapper, and a direct
		// `(sparseElem)(sparseArray)` cast chains two user-defined operators (CS0030). Hop
		// through the shared underlying slice: `((sparseElem)(slice<byte>)(…))`.
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if sliceUnder, ok := named.Underlying().(*types.Slice); ok {
				if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok && argNamed != named {
					if _, argIsSlice := argNamed.Underlying().(*types.Slice); argIsSlice && types.Identical(argNamed.Underlying(), named.Underlying()) {
						// Same EXCEPTION the map/array twin of this rule carries further down: one
						// of the two was WRITTEN directly over the other (`type shuffledList List`),
						// so its wrapper's single conversion operator targets that NAMED type rather
						// than the shared `slice<E>` — and the hop's first leg becomes the very
						// two-operator chain this rule exists to prevent. The plain cast binds.
						if !writtenRHSIsNamedType(argNamed, named) && !writtenRHSIsNamedType(named, argNamed) {
							underlyingCS := convertToCSTypeName(v.getAliasQualifiedTypeName(sliceUnder, false))
							return fmt.Sprintf("((%s)(%s)(%s))", targetTypeName, underlyingCS, expr)
						}
					}
				}
			}
		}

		// underlying basic (the existing cast already binds → no churn). types.Unalias: os's
		// `type FileMode = fs.FileMode` arrives as a *types.Alias, and the bare assertion
		// skipped the hop (direct nint cast into the foreign wrapper, CS0030 removeall_noat).
		if named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
				argType := v.info.TypeOf(arg)

				// An IDENTITY conversion to the same named numeric type — `arenaIdx(x)` where x is
				// already `arenaIdx` — is a Go no-op. This happens for an untyped-constant shift that
				// adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types
				// already types as arenaIdx, so convExpr(arg) has emitted `((arenaIdx)((nuint)1 << b))`),
				// and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expr in another
				// `((arenaIdx)…)` cast just doubles it; return the converted arg as-is.
				if argType != nil && types.Identical(argType, named) {
					// EXCEPT a plain CONSTANT arg: go/types types the constant AS the target
					// (identity), but convExpr rendered the bare literal (`Word(1)` → `1`),
					// which under a binary operator resolves as int and degrades the whole
					// expression (math/big's `mask := Word(1)<<s - 1`, CS0029 ×3). Re-impose
					// the named cast — the := declaration patch in visitAssignStmt covered
					// only the direct-RHS position.
					if tv, ok := v.info.Types[callExpr]; ok && tv.Value != nil {
						namedCS := convertToCSTypeName(v.getAliasQualifiedTypeName(named, false))

						if !strings.HasPrefix(expr, "(("+namedCS+")") && !strings.HasPrefix(expr, "("+namedCS+")") && !strings.HasPrefix(expr, "new ") {
							// TWO independent questions have to be asked of a cast's operand, and
							// this arm asked only the second. castOperandNeedsParens answers the
							// cast-vs-subtraction PARSE AMBIGUITY (`(Named)-1` reads as a
							// subtraction unless the operand is wrapped) — a leading-sign text
							// test. PRECEDENCE is separate: a C# cast binds tighter than EVERY
							// binary operator, so a constant operand that renders as a top-level
							// binary expression has the cast claim its LEFT OPERAND ALONE.
							//
							// Go folds `rf(3 / 2)` in arbitrary precision — untyped integer
							// division, so 1 — then converts. `((rf)3 / 2)` casts 3 to rf FIRST
							// and divides in the target's own arithmetic: 1.5. The defect is
							// therefore VALUE-CHANGING and silent wherever the first leg happens
							// to compile (every named int/float type, whose [GoType] wrapper
							// supplies the operator), and only becomes a hard error where it does
							// not: a named COMPLEX type has no float→named-complex conversion, so
							// `renamedComplex64(3 + 4i)` → `((renamedComplex64)3F + 4F.i())` is
							// CS0030 (fmt's own tests, four sites). Same root, two symptoms.
							//
							// Keyed on the AST rather than the rendered text: the operand's own
							// emission may be a call, a literal or a folded constant, and only the
							// written expression says whether a binary operator is left exposed. A
							// ParenExpr operand already renders wrapped, so the direct type test
							// is sufficient. UNARY operands are deliberately NOT included — a cast
							// and a unary operator share precedence and associate right, so
							// `(T)~0` already means `(T)(~0)`; their only hazard is the sign
							// ambiguity castOperandNeedsParens covers.
							_, operandIsBinary := arg.(*ast.BinaryExpr)

							if operandIsBinary || castOperandNeedsParens(namedCS, expr) {
								return fmt.Sprintf("((%s)(%s))", namedCS, expr)
							}

							return fmt.Sprintf("((%s)%s)", namedCS, expr)
						}
					}

					return expr
				}

				// A NAMED numeric arg needs the underlying hop even when its underlying is
				// IDENTICAL to the target's — `hex(work.full)` where full is `type lfstack uint64`
				// and hex is uint64 (runtime mgc.go): the direct `(Δhex)full` is still a two-op
				// user-defined chain (lfstack→uint64, uint64→Δhex) that C# won't build (CS0030).
				// The identical-underlying skip below is right only for a BASIC arg, where the
				// exact [GoType] operator binds directly.
				argIsDistinctNamedNumeric := false

				if argNamed, ok := types.Unalias(argType).(*types.Named); ok && argType != nil {
					if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 {
						argIsDistinctNamedNumeric = true
					}

					// EXCEPTION to the hop: the conversion target IS the arg's WRITTEN base named
					// type — `syscall.Handle(k)` where `type Key syscall.Handle` (registry value.go).
					// The arg's [GoType] wrapper declares the one-step operator to exactly that type
					// (Key → ΔHandle), so the direct cast binds; the underlying hop's first leg
					// `(uintptr)k` is itself the illegal two-op chain (Key→ΔHandle→uintptr, CS0030).
					// Gated to a CROSS-PACKAGE base: only there does the [GoType] emission keep the
					// NAMED base (`[GoType("syscall_package.ΔHandle")]`); a same-package chain
					// resolves to the basic underlying (`[GoType("num:uintptr")]` — no named-base
					// operator exists) and the underlying hop already binds one-op-per-leg.
					if rhs, okRHS := packageTypeSpecRHS[argNamed.Obj()]; okRHS && rhs != nil {
						if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed == named &&
							named.Obj().Pkg() != argNamed.Obj().Pkg() {
							return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
						}
					}

					// The FORWARD mirror: the conversion TARGET's written base IS the arg's named
					// type (`Key(handle)` / `reading(celsius)` where `type reading lib.Celsius`) —
					// the target's wrapper declares the one-step operator FROM exactly that type;
					// the hop's second leg `(reading)(double)…` has no operator (CS0030).
					if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil {
						if rhsNamed, ok := types.Unalias(rhs).(*types.Named); ok && rhsNamed == argNamed &&
							named.Obj().Pkg() != argNamed.Obj().Pkg() {
							return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
						}
					}
				}

				if argType == nil || argIsDistinctNamedNumeric || !types.Identical(argType.Underlying(), basic) {
					underlyingCS := v.getCSharpTypeName(basic)
					inner := expr

					// When the arg is ITSELF a named numeric whose underlying differs from the target's
					// underlying — `hex(off)` where `off` is `abi.NameOff` (int32) and `hex` is uint64 —
					// the intermediate `(uint64)NameOff` cast is itself CS0030 (the [GoType] wrapper only
					// converts NameOff↔int32). Unwrap the arg through ITS underlying first, so the chain
					// is named→argUnderlying (operator) → targetUnderlying (numeric) → target (operator):
					// `((hex)(uint64)(int32)off)`. (types.Unalias resolves the runtime's aliased offsets.)
					if argNamed, ok := types.Unalias(argType).(*types.Named); ok {
						if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 && !types.Identical(argBasic, basic) {
							if v.needsParentheses(arg) {
								inner = fmt.Sprintf("(%s)(%s)", v.getCSharpTypeName(argBasic), expr)
							} else {
								inner = fmt.Sprintf("(%s)%s", v.getCSharpTypeName(argBasic), expr)
							}
						}
					}

					if v.needsParentheses(arg) {
						return fmt.Sprintf("((%s)(%s)(%s))", targetTypeName, underlyingCS, inner)
					}

					return fmt.Sprintf("((%s)(%s)%s)", targetTypeName, underlyingCS, inner)
				}
			}
		}

		// The MIRROR of the branch above: a conversion FROM a NAMED numeric type TO a DIFFERENT basic
		// numeric type — `uint64(t.Str)` where `Str` is `abi.NameOff` (named int32), `int(idx)` where
		// `idx` is a `num:nuint` named type. The named type's [GoType] wrapper only converts between it
		// and its EXACT underlying (`NameOff ↔ int32`), so a plain `(ulong)NameOff` / `(nint)idx` is
		// CS0030. Route through the underlying first — `((ulong)(int)t.Str)` / `((nint)(nuint)idx)` — the
		// named→basic [GoType] operator followed by a numeric C# cast (exactly Go's conversion semantics).
		// Skipped when the target basic IS the named type's underlying (the exact operator binds → no churn)
		// and when the arg is not a named numeric (a plain basic→basic cast already works).
		if targetBasic, ok := v.info.TypeOf(callExpr).(*types.Basic); ok && targetBasic.Info()&types.IsNumeric != 0 {
			// types.Unalias resolves a Go type alias (`type nameOff = abi.NameOff`) to the named type it
			// aliases — in Go 1.23 `TypeOf` returns a `*types.Alias`, so a bare `.(*types.Named)` would
			// miss the runtime's aliased `abi` offset types (`nameOff`/`typeOff`/`textOff`).
			if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok {
				if argBasic, ok := argNamed.Underlying().(*types.Basic); ok && argBasic.Info()&types.IsNumeric != 0 {
					if !types.Identical(argBasic, targetBasic) {
						underlyingCS := v.getCSharpTypeName(argBasic)

						// No outer parentheses: the conversion target is a BASIC C# type, whose result
						// can never be the receiver of a postfix `.`/`[]`/invocation (basic types expose
						// no Go-callable members and the converter emits none), so C# cast precedence —
						// higher than every binary operator — already binds correctly in any surrounding
						// context. `(ulong)(int)t.Str` parses as `(ulong)((int)(t.Str))` (postfix `.`
						// binds before the cast). See the basic-target note at the general return below.
						if v.needsParentheses(arg) {
							return fmt.Sprintf("(%s)(%s)(%s)", targetTypeName, underlyingCS, expr)
						}

						return fmt.Sprintf("(%s)(%s)%s", targetTypeName, underlyingCS, expr)
					}
				}
			}
		}

		// A conversion to a BASIC C# type omits the outer parentheses around the whole cast: the
		// result of `(uint64)x` / `(nint)y` can never be the receiver of a postfix `.`/`[]`/invocation
		// (Go basic types expose no callable members and the converter emits none on them), and the C#
		// cast operator outranks every binary operator, so `(uint64)x` binds correctly unparenthesized
		// in any surrounding context (`f((uint64)a)`, `return (uint64)a;`, `(uint64)a << n`). This keeps
		// the common `uint64(a)` → `(uint64)a` close to the Go source. A NAMED-type target keeps the
		// outer parens — its result CAN be member-accessed (`Named(x).Method()`), which is parent-
		// context-dependent and not decidable here, so the defensive wrap is retained.
		//
		// `string` is the exception among basic types: its C# representation is the golib `@string`
		// STRUCT, which IS member-accessible — it exposes methods, an indexer, and is the receiver of
		// the variadic-string spread `string(r)...` → `((@string)(rune)r).ꓸꓸꓸ`. Dropping the outer wrap
		// there reparses `.ꓸꓸꓸ`/`[]`/`.Method()` against the cast's INNER operand (`(@string)(rune)r.ꓸꓸꓸ`
		// binds `.ꓸꓸꓸ` to `r`, CS1061). So treat a `string` target like a named type and keep the wrap.
		basicTarget, _ := v.info.TypeOf(callExpr).(*types.Basic)
		targetIsBasic := basicTarget != nil && basicTarget.Kind() != types.String

		// A conversion to STRING whose arg is an UNTYPED constant REFERENCE
		// (`string(utf8.RuneError)`, time format.cs) renders the arg as its cross-package
		// `static readonly` Untyped* wrapper, from which @string has no conversion (CS0030).
		// Hop through the constant's DEFAULT Go type first (`((@string)(rune)runeError)`) —
		// exactly Go's conversion semantics. A plain literal is already a C# constant and
		// keeps its direct form (no churn).
		if basicTarget != nil && basicTarget.Kind() == types.String {
			if tvArg, ok := v.info.Types[arg]; ok && tvArg.Value != nil {
				if argBasic, ok := tvArg.Type.(*types.Basic); ok && argBasic.Info()&types.IsUntyped != 0 && !v.isCSharpConstantExpr(arg) {
					expr = fmt.Sprintf("(%s)%s", v.getCSharpTypeName(types.Default(argBasic).(*types.Basic)), expr)
				}
			} else if argType := v.info.TypeOf(arg); argType != nil {
				// A NAMED integer type (`type Delim rune`, encoding/json's `string(d)`) renders as a
				// [GoType] wrapper struct with no direct @string conversion (CS0030). Go's string(intType)
				// is a code-point → UTF-8 conversion; hop through the underlying integer so the wrapper's
				// implicit operator yields it first, then @string converts the code point.
				if named, ok := types.Unalias(argType).(*types.Named); ok {
					if u, ok := named.Underlying().(*types.Basic); ok && u.Info()&types.IsInteger != 0 {
						expr = fmt.Sprintf("(%s)%s", v.getCSharpTypeName(u), expr)
					}
				}
			}
		}

		// A conversion to the EMPTY INTERFACE whose operand is an UNTYPED constant — `any(7)`,
		// `interface{}(0)` — hops through the constant's DEFAULT Go type first, exactly like the
		// `string(...)` hop above and the implicit interface-slot boxing every other position applies
		// (see untypedConstBoxCast). Without it `((any)7)` boxes a System.Int32 where Go boxes `int`,
		// so the paired `any(7).(int)` assertion (`._<nint>()`) fails and `==` against a real `int`
		// reports unequal. A NON-empty interface target routes through the adapter machinery above.
		if isEmptyInterfaceTarget(v.info.TypeOf(callExpr)) {
			expr = v.applyUntypedConstBoxCast(arg, expr)

			// The POINTER twin of the same hop — `any(p)` boxes the pointer, so it carries its Go
			// type across even when nil (see typedNilInterfaceBoxing.go).
			expr = v.boxPointerIntoEmptyInterface(v.info.TypeOf(callExpr), arg, expr)
		}

		// A conversion between two DIFFERENT NAMED types that share a COMPOSITE underlying (net/mail
		// textproto.MIMEHeader(h) where Header and MIMEHeader both wrap map[string][]string): each is a
		// [GoType] wrapper with implicit conversions only to/from its OWN underlying, and C# will not
		// chain Header->map->MIMEHeader in one cast (CS0030). Hop through the shared underlying so each
		// wrapper implicit operator applies: (MIMEHeader)(map<@string, slice<@string>>)h.
		if targetNamed, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok {
			if convArgType := v.info.TypeOf(arg); convArgType != nil {
				if argNamed, ok := types.Unalias(convArgType).(*types.Named); ok && !types.Identical(targetNamed, argNamed) && types.Identical(targetNamed.Underlying(), argNamed.Underlying()) {
					// EXCEPTION, the composite twin of the named-numeric one above: one of the two
					// was WRITTEN directly over the other — `type shuffledFS MapFS`, where MapFS is
					// itself `map[string]*MapFile` (testing/fstest's own suite). go/types resolves
					// both underlyings to the raw map, so the shared-underlying test passes, but the
					// wrapper does NOT convert to that map: visitIdent/visitTypeSpec keep the NAMED
					// base for any non-basic underlying (`[GoType("…fstest_package.MapFS")]`), so
					// `shuffledFS` declares exactly one operator and it targets `MapFS`. The hop's
					// first leg is then the very two-operator chain the hop exists to prevent
					// (shuffledFS→MapFS→map, CS0030 at testfs_test.cs:62), while the plain cast the
					// fall-through emits binds in one step. Unlike the numeric exception this needs
					// NO cross-package gate — the named base is kept for a composite underlying
					// whether that base is same-package or not.
					if !writtenRHSIsNamedType(argNamed, targetNamed) && !writtenRHSIsNamedType(targetNamed, argNamed) {
						// Map/slice/array underlyings have a nameable C# cast target (map<K,V>, slice<T>,
						// array<T>). A STRUCT underlying is anonymous (struct{...} — not a valid cast term,
						// CS1525) AND is the reverse *underlying->*named REINTERPRET case (NamedPointerReinterpret),
						// so it is excluded.
						switch targetNamed.Underlying().(type) {
						case *types.Map, *types.Slice, *types.Array:
							expr = fmt.Sprintf("(%s)%s", convertToCSTypeName(v.getAliasQualifiedTypeName(targetNamed.Underlying(), false)), expr)
						}
					}
				}
			}
		}

		// A widened constant operand already carries this exact narrowing cast in its own emission
		// (widenedConstExprCastType) — `int32(1<<31 - 1)` converts an operand that rendered as
		// `(int32)(2147483648L - 1)`. Re-casting would only double it, so return it as it stands;
		// the same wholeExprIsCastOfType check the assignment path uses for the sibling fold.
		if targetIsBasic && v.widenedConstExprCastType(arg) == targetTypeName &&
			wholeExprIsCastOfType(expr, targetTypeName) {
			return expr
		}

		// A conversion to an EXPORTED, TEST-FILE-DECLARED defined-type-over-STRUCT whose immediate
		// underlying is itself an UNEXPORTED named struct — runtime export_test.go's
		// `type PageCache pageCache`, then `PageCache(pageCache{...})` / `PageCache(pp.allocToCache())`
		// — has no C# conversion operator to cast through: go2cs-gen's W3a wrapper-scaffolding
		// (InheritedTypeTemplate's OmitUnderlyingConversionOperators) deliberately omits the operator
		// pair for a test-file-declared wrapper whose wrapped type is not itself public (a
		// user-defined conversion operator has no legal non-public form, CS0558, so neither modifier
		// is legal — the pair is omitted rather than weakened), leaving the constructor as the one
		// remaining explicit path, since every consumer is a sibling file in the same whitebox test
		// assembly. The plain cast the fallback below emits therefore cannot compile (CS0030): there
		// is no operator for `((PageCache)pageCacheValue)` to bind. Route through the constructor
		// instead — it is ALWAYS emitted regardless of whether the operator pair is, so
		// `new PageCache(pageCacheValue)` compiles whether or not the cast would have.
		//
		// The TEST-FILE-DECLARED condition on the TARGET is load-bearing, not incidental — measured
		// 2026-09-01 via a two-seeded-reconvert-diffed-against-each-other blast-radius check that
		// caught the first (unguarded) version of this fix breaking log/slog's `value.go`:
		// `type timeTime time.Time` is the SAME exported/unexported-struct shape in reverse (the
		// UNEXPORTED type wraps the EXPORTED one, declared in PRODUCTION source), and its existing
		// cast `((time.Time)a)` already compiles fine there via a real conversion operator — W3a's
		// omission is specific to a test-file wrapper, so applying the constructor route unconditionally
		// tried `new time.Time(a)` against `time.Time`'s OWN ordinary constructors, none of which
		// accept a `timeTime` argument (CS1503, resolving to the nearest unrelated overload instead).
		// declaredInTestFile is the existing idiom for exactly this signal (testAliasShadowOperations.go).
		if targetNamed, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named); ok && targetNamed.Obj().Exported() && v.declaredInTestFile(targetNamed.Obj()) {
			if _, targetIsStruct := targetNamed.Underlying().(*types.Struct); targetIsStruct {
				if argNamed, ok := types.Unalias(v.info.TypeOf(arg)).(*types.Named); ok && !argNamed.Obj().Exported() {
					if _, argIsStruct := argNamed.Underlying().(*types.Struct); argIsStruct {
						return fmt.Sprintf("(new %s(%s))", targetTypeName, expr)
					}
				}
			}
		}

		// Determine if we need parentheses around the expression
		if v.needsParentheses(arg) {
			if targetIsBasic {
				return fmt.Sprintf("(%s)(%s)", targetTypeName, expr)
			}

			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
		}

		if targetIsBasic {
			if castOperandNeedsParens(targetTypeName, expr) {
				return fmt.Sprintf("(%s)(%s)", targetTypeName, expr)
			}

			return fmt.Sprintf("(%s)%s", targetTypeName, expr)
		}

		if castOperandNeedsParens(targetTypeName, expr) {
			return fmt.Sprintf("((%s)(%s))", targetTypeName, expr)
		}

		return fmt.Sprintf("((%s)%s)", targetTypeName, expr)
	}
	// ---- Phase 1c: constructor calls ----

	constructType := ""

	if v.isConstructorCall(callExpr) {
		constructType = "new "
	}

	// ---- Phase 2: build the argument-conversion context ----

	// u8 readonly spans cannot be used as arguments to functions that take interface parameters
	callExprContext := DefaultCallExprContext()
	callExprContext.callArgs = context.callArgs
	// Hoist a func-literal argument's capture declarations to the enclosing statement (a
	// `var mʗ1 = m;` statement is invalid inside an argument list). convExprList builds a
	// LambdaContext from this builder for each argument so the func-literal arg's convFuncLit
	// writes its decls here instead of inline.
	callExprContext.deferredDecls = context.deferredDecls

	// A deferred/spawned call whose SOLE argument is a MULTI-VALUE call spreads that call's
	// results across the callee's parameters (`defer f(g())`). Record the arity so the argument
	// renderer expands the single `ᴛ1` marker component-wise; the eager argument stays the whole
	// tuple, evaluated once at the defer/go statement exactly as Go requires.
	if callExprContext.callArgs != nil {
		callExprContext.multiValueSpreadArity = v.multiValueSpreadArity(callExpr)
	}

	// Check if the call is using the spread operator "..."
	if callExpr.Ellipsis.IsValid() {
		callExprContext.hasSpreadOperator = true
	}

	// ---- Phase 3: classify each argument against the callee signature ----
	//
	// The longest phase. For every parameter position it decides what the argument must become:
	// an interface conversion, a ef/box form, a cast, a clone, a lambda re-wrap. The answers
	// are recorded in callExprContext for convExprList to apply.

	var replacementArgs []string
	funcSignature := v.getFunctionSignature(callExpr)

	// ж-box A2 (DESIGN-zh-box-reduction §3.3): the callee's Phase-A ref-lowered parameter
	// positions, resolved once per call. Ordinary sites replace the boxed argument with a `ref`
	// expression; a defer/go site is a BOXED site categorically — its eager arguments keep
	// today's emission and the invoke-time thunk derives each ref (marked for convExprList).
	refLoweredPositions := v.refLoweredCalleePositions(callExpr)

	if funcSignature != nil {
		// Check if any parameters of callExpr.Fun are interface or pointer types
		params := funcSignature.Params()

		for i := range params.Len() {
			var paramType types.Type
			paramHasArg := callExpr.Args != nil && i < len(callExpr.Args)

			// A pointer-to-ARRAY parameter given a bare `nil` keeps the array's length as cargo --
			// the argument's twin of the assignment and result positions. Recorded here because this
			// is the walk holding both the parameter's type and the argument's index; convExprList
			// consumes it. runtime's `sigprocmask(_SIG_SETMASK, &sigset_all, nil)` is the shape.
			if paramHasArg && callExprContext != nil && callExprContext.nilArrayTypes != nil &&
				v.identIsUniverseNilExpr(callExpr.Args[i]) &&
				v.nilArrayPtrValueForTarget(params.At(i).Type()) != "" {
				callExprContext.nilArrayTypes[i] = params.At(i).Type()
			}

			if paramHasArg {
				// Check if the parameter type is an anonymous struct
				if structType, exprType := v.extractStructType(callExpr.Args[i]); structType != nil && !v.liftedTypeExists(structType) {
					// An argument matching a callee's own anonymous-struct parameter shares that
					// parameter's externally-significant type — see liftAtCallBoundary's doc
					// comment (visitorState.go) for why this publishes into the wide dedup
					// registry rather than staying scoped to this call's enclosing function.
					v.liftAtCallBoundary = true
					v.indentLevel++
					v.visitStructType(structType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
					v.liftAtCallBoundary = false
				}

				// Check if the parameter type is an anonymous interface
				if interfaceType, exprType := v.extractInterfaceType(callExpr.Args[i]); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
					v.indentLevel++
					v.visitInterfaceType(interfaceType, exprType, params.At(i).Name(), nil, true, nil)
					v.indentLevel--
				}

				// A deferred/go call routes its args through defer/goǃ, where the callee's parameter
				// type must equal the C# type the arg renders as. In the METHOD-VALUE form
				// (defer(Action<T>, T arg, …)) T unifies from BOTH the method parameter and the
				// argument; in the LAMBDA form (goǃ(ᴛ1 => f(ᴛ1), arg)) the arg's C# type drives ᴛ1's
				// inference and the lambda body then calls f(ᴛ1) — so ᴛ1 must be f's parameter type.
				// An untyped numeric constant (e.g. `0`, or crc32's untyped `Castagnoli` feeding
				// MakeTable(uint32)) otherwise takes convExprList's DEFAULT-Go-type cast, which won't
				// unify with a wider/other concrete parameter (atomic.Uint64.Store(ulong),
				// MakeTable(uint32) — hash/crc32 TestCastagnoliRace, CS1503). Cast it to the parameter
				// type. In the lambda form the override is applied ONLY when the parameter differs from
				// that default type — when they match, the existing default-cast path already yields the
				// right type, so overriding would only churn the golden (`(nint)x`→`(nint)(x)`).
				if context.callArgs != nil {
					deferParamType := params.At(i).Type()
					if basic, ok := deferParamType.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
						if v.isUntypedNumericConstArg(callExpr.Args[i]) {
							applyCast := !context.renderParams

							if !applyCast {
								if defaultType := v.untypedNumericConstArgDefaultType(callExpr.Args[i]); defaultType != nil {
									applyCast = !types.Identical(defaultType, basic)
								}
							}

							if applyCast {
								if callExprContext.castArgToType == nil {
									callExprContext.castArgToType = make(map[int]string)
								}
								callExprContext.castArgToType[i] = convertToCSTypeName(v.getAliasQualifiedTypeName(deferParamType, false))
							}
						}
					}

					// An untyped-nil argument renders as golib `nil` (NilType), which unifies the
					// defer/goǃ type parameter as NilType and breaks the method-group match
					// (CS0123 — exec_windows DuplicateHandle). Cast it to the parameter's C# type:
					// ж<T>/interface/slice/map/channel all carry an implicit NilType conversion,
					// so `(ж<ΔHandle>)(nil)` binds and keeps the visible `nil`. Method-value form
					// only — the lambda form's untyped nil binds through NilType's own conversions.
					if !context.renderParams {
						if tv, ok := v.info.Types[callExpr.Args[i]]; ok && tv.IsNil() {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}
							callExprContext.castArgToType[i] = v.getCSharpTypeName(deferParamType)
						}
					}
				}
			}

			callExprContext.u8StringArgOK[i] = true

			// Handle builtin functions that take `...Type` parameters, treat as `interface{}`
			var ok bool

			if v.callFunIsUniversePrint(callExpr) {
				paramType = types.NewInterfaceType(nil, nil)
			} else if paramType, ok = getParameterType(funcSignature, i); !ok {
				continue
			}

			if paramHasArg {
				argType := v.getType(callExpr.Args[i], false)
				targetType := paramType
				replacementArg := v.checkForDynamicStructs(argType, targetType)

				// A bidirectional channel passed to a DIRECTIONAL parameter narrows the Go type
				// with no construction to hook — the argument arm of the live-copy narrowing (see
				// chanDirNarrowedValue). It rides the per-argument SUFFIX channel rather than
				// castArgToType's prefix cast, because the re-stamp reads off the value
				// (`ch.WithDirection(...)`); the marker below is what convExprList appends.
				if narrowSuffix := v.chanDirNarrowedValue(paramType, argType, DynamicCastArgMarker); narrowSuffix != "" {
					if suffix, found := strings.CutPrefix(narrowSuffix, DynamicCastArgMarker); found {
						if callExprContext.suffixArgWith == nil {
							callExprContext.suffixArgWith = make(map[int]string)
						}

						callExprContext.suffixArgWith[i] = suffix
					}
				}

				// If a replacement argument is found, add it to the replacementArgs slice,
				// creating the slice if it doesn't exist yet
				if len(replacementArg) > 0 {
					if replacementArgs == nil {
						replacementArgs = make([]string, params.Len())
					}

					replacementArgs[i] = replacementArg
				}
			}

			// A GENERIC parameter declared as a SLICE of a type parameter whose constraint is a
			// METHOD-SET interface — `walkList[N Node](v Visitor, list []N)` — instantiated with
			// a POINTER type argument (`walkList(v, n.Names)`, N=*Ident): the emitted constraint
			// is `where N : Node` (see getGenericDefinition), which the `ж<Ident>` box cannot
			// satisfy — the box does not implement the interface, its generated pointer adapter
			// does. Project the slice element-wise through the adapter (`widen<ж<Ident>, Node>(
			// (~n).Names, elemᴛ0 => new IdentжNode(elemᴛ0))`), instantiating N as the interface
			// itself. The projection copies the slice HEADER only (elements alias through the
			// shared box, so method calls mutate the original objects); a callee that reassigns
			// `list[i]` itself would not write back — acceptable for the read/widen shape this
			// targets. convertToInterfaceType supplies the adapter reference AND the GoImplement
			// recording, exactly as at scalar *T→iface call sites.
			if paramHasArg && (replacementArgs == nil || len(replacementArgs[i]) == 0) {
				if declSlice, ok := paramType.Underlying().(*types.Slice); ok {
					if tp, ok := types.Unalias(declSlice.Elem()).(*types.TypeParam); ok {
						if iface, ok := tp.Constraint().Underlying().(*types.Interface); ok && iface.NumMethods() > 0 && iface.IsMethodSet() {
							if instParam := v.instantiatedParamType(callExpr, i); instParam != nil {
								if instSlice, ok := instParam.Underlying().(*types.Slice); ok {
									if ptrElem, ok := instSlice.Elem().(*types.Pointer); ok && types.Implements(ptrElem, iface) {
										elemVar := fmt.Sprintf("elem%s%d", TempVarMarker, i)
										wrapped := v.convertToInterfaceType(tp.Constraint(), ptrElem, elemVar)

										if strings.HasPrefix(wrapped, "new ") {
											if replacementArgs == nil {
												replacementArgs = make([]string, params.Len())
											}

											replacementArgs[i] = fmt.Sprintf("widen<%s, %s>(%s, %s => %s)",
												v.getCSharpTypeName(ptrElem), v.getCSharpTypeName(tp.Constraint()), DynamicCastArgMarker, elemVar, wrapped)
										}
									}
								}
							}
						}
					}
				}
			}

			// A FUNC-typed parameter of a SELF-REFERENTIAL constraint-proxy instantiation renders
			// its delegate over the proxy (`Func<P224PointжnistPoint>` for `newPoint func() P`),
			// so a method-group / func-value argument must be re-wrapped as a lambda — a C#
			// method-group conversion cannot apply the ж↔proxy user-defined conversion (CS0407 on
			// `testEquivalents(t, nistec.NewP224Point, …)`); nil stays bare, exactly as in the
			// composite-literal field case.
			//
			// A FuncLit needs the SAME remedy applied one position further in, and the claim it
			// once carried here — that a literal "already targets the proxy" — was the premise this
			// missed on. A literal renders its OWN parameter list from the Go signature, so a
			// `func(t T, mode int)` argument emits `(ж<Impl> t, nint mode) => …` against a delegate
			// requiring `Action<ImplжConstrained, nint>`: CS1678 + CS1661 per site, and 48 of
			// net/http's 81. It is marked here and carried to convFuncLit, which declares those
			// parameters at the proxy under synthesized names and opens the body with the natural-
			// typed alias — the conversion moved to a position C# performs it, leaving the body
			// itself byte-for-byte what it was. See constraintProxyLitParamTypes.
			if paramHasArg {
				if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
					if instance, ok := v.info.Instances[funIdent]; ok && instance.TypeArgs != nil {
						if _, isLit := callExpr.Args[i].(*ast.FuncLit); !isLit {
							if tv, isNil := v.info.Types[callExpr.Args[i]]; !isNil || !tv.IsNil() {
								if params, ok := v.constraintProxyLambdaParams(funIdent, instance.TypeArgs, i); ok {
									if callExprContext.wrapArgWithLambda == nil {
										callExprContext.wrapArgWithLambda = make(map[int]string)
									}

									callExprContext.wrapArgWithLambda[i] = params
								}
							}
						} else if proxied := v.constraintProxyLitParamTypes(funIdent, instance.TypeArgs, i); proxied != nil {
							if callExprContext.proxyLitParamTypes == nil {
								callExprContext.proxyLitParamTypes = make(map[int]map[int]string)
							}

							callExprContext.proxyLitParamTypes[i] = proxied
						}
					}
				}
			}

			// A CONSTRAINED SLICE TYPE PARAMETER passed where a concrete slice<E> parameter is
			// expected — Go assignability (S ~[]E is assignable to []E; the slices package's
			// rotateRight(s[m:i], …)/pdqsortOrdered(x, …) helper chain) — materializes through
			// the SHARING slice<T>(ISlice<T>) constructor: `new slice<E>(arg)`. A cast cannot
			// apply (the source is interface-constrained; C# forbids user-defined conversions
			// from interfaces), and the constructor shares backing, preserving Go aliasing.
			if paramHasArg && i < len(callExpr.Args) {
				if paramSlice, ok := types.Unalias(paramType).(*types.Slice); ok {
					if argTP, ok := types.Unalias(v.info.TypeOf(callExpr.Args[i])).(*types.TypeParam); ok && typeParamSliceCore(argTP) != nil {
						if callExprContext.wrapArgWithNew == nil {
							callExprContext.wrapArgWithNew = make(map[int]string)
						}

						callExprContext.wrapArgWithNew[i] = fmt.Sprintf("slice<%s>", convertToCSTypeName(v.getAliasQualifiedTypeName(paramSlice.Elem(), false)))
					}
				}
			}

			// An untyped `nil` in a VARIADIC slot must state the element type, or C# binds the
			// call's NORMAL form and the argument DISAPPEARS. `exec(t, db, "INSERT|…", nil)`
			// (database/sql's sql_test.go, one element: Go reads `nil` as a single `any` value,
			// since passing the slice itself would need `nil...`) emitted
			// `exec(…, insertTId10Nameˢ, default!)` against `params ꓸꓸꓸany argsʗp` — and a typeless
			// `default!` converts to the params ARRAY as readily as to its element, so C#'s
			// preference for the normal form over the expanded one binds it as a null `any[]`.
			// The callee then saw `len(args) == 0` and the driver answered
			// `sql: expected 1 arguments, got 0` — a SILENT divergence, not a compile error, which
			// is what makes it worth a cast the other nil positions do not need: a non-variadic
			// parameter has only one form to bind, so `f(nil)` there is already unambiguous.
			// Casting to `paramType` is casting to the ELEMENT type — getParameterType already
			// yields it for the variadic slot — and every trailing argument is checked because the
			// nil need not be the first (`f(a, nil, b)`). A SPREAD call (`f(args...)`) passes the
			// slice whole, so there is no expansion to disambiguate and it is excluded.
			// … and it must be a real INVOCATION. `(func(...int))(nil)` is a CONVERSION whose
			// callee is a TYPE, but isTypeConversion has no *ast.FuncType arm, so a func-type
			// conversion is never classified as one and arrives here on the regular call path
			// with the target signature standing in for a callee. Everything the comment above
			// argues then inverts: a conversion has no params expansion to disambiguate, its
			// single operand is the conversion SOURCE, and `default!` already binds
			// unambiguously to the one delegate target. Casting it to `paramType` cast the nil
			// to the func's FIRST PARAMETER — `(Actionꓸꓸꓸ<nint>)((nint)(default!))`, CS0030.
			// The predicate is variadic AND exactly one declared parameter, which is why
			// `(func(Point, ...Point) int)(nil)` and every non-variadic spelling were unharmed.
			if calleeTV, isCallee := v.info.Types[callExpr.Fun]; !isCallee || !calleeTV.IsType() {
				if funcSignature.Variadic() && i == params.Len()-1 && !callExprContext.hasSpreadOperator {
					for j := i; j < len(callExpr.Args); j++ {
						if !argIsUntypedNil(callExpr.Args[j], v.info) {
							continue
						}

						if callExprContext.castArgToType == nil {
							callExprContext.castArgToType = make(map[int]string)
						}

						callExprContext.castArgToType[j] = convertToCSTypeName(v.getAliasQualifiedTypeName(paramType, false))
					}
				}
			}

			// A SLICE or ARRAY of the variadic ELEMENT type, passed as the SOLE argument of the
			// variadic slot, needs the identical cast for the identical reason — C# binding the
			// NORMAL form where Go means one element — but reached through a conversion rather
			// than through typelessness. `jsValEscaper(a)` with `a []any` against
			// `params ꓸꓸꓸany argsʗp` (html/template js_test's nesting cases) bound the EXPANDED
			// form under C# 13 and binds the NORMAL form under C# 14, which spreads the slice and
			// loses exactly one level of nesting: `[42]` renders as ` 42 `, `[[42,"foo",null]]` as
			// `[42,"foo",null]`. See variadicArgBindsParamsCollection for the conversion chain
			// C# 14 newly admits (`slice<any>` → `any[]` → `Span<any>`) and why a NAMED slice type
			// is not affected. Gated on the tail holding EXACTLY ONE argument, because that is the
			// only arity at which the normal form is applicable at all.
			if funcSignature.Variadic() && i == params.Len()-1 && !callExprContext.hasSpreadOperator && len(callExpr.Args) == params.Len() {
				if variadicArgBindsParamsCollection(v.info.TypeOf(callExpr.Args[i]), paramType) {
					if callExprContext.castArgToType == nil {
						callExprContext.castArgToType = make(map[int]string)
					}

					if _, exists := callExprContext.castArgToType[i]; !exists {
						callExprContext.castArgToType[i] = convertToCSTypeName(v.getAliasQualifiedTypeName(paramType, false))
					}
				}
			}

			// A Go string passed to a generic type-parameter parameter must be cast to
			// golib's `@string` (a struct). Without a target type, a bare string literal
			// converts to a .NET `System.String`, so C# infers the type argument as
			// `string` — which fails the `new()` constraint go2cs adds to type parameters
			// (and mismatches `@string` args). e.g. `First("A", "B")` for `func First[T any](v ...T)`.
			if paramHasArg {
				if _, isTypeParam := paramType.(*types.TypeParam); isTypeParam {
					// A variadic type parameter receives every trailing argument, so flag
					// all of them (the loop only iterates the declared parameters).
					lastArg := i

					if funcSignature.Variadic() && i == params.Len()-1 {
						lastArg = len(callExpr.Args) - 1
					}

					for j := i; j <= lastArg; j++ {
						if v.isStringType(callExpr.Args[j]) {
							callExprContext.useGoStringArg[j] = true
						}
					}
				}
			}

			// A narrow-integer parameter (int8/uint8/int16/uint16) receiving a binary/unary arithmetic
			// argument: Go evaluates `a+b`/`^a` at the operand's narrow width (with overflow wrapping),
			// but C# promotes sub-int integer arithmetic to `int`, so the result needs an explicit cast
			// back to the parameter type — both to compile (the implicit int→narrow conversion is
			// rejected, CS1503) and to preserve Go's wrap semantics. Gated on the argument's Go type
			// already matching the parameter (so Go accepts it without a conversion) and on it being an
			// arithmetic expression (a bare ident/selector is already the narrow type).
			if paramHasArg {
				if paramBasic, ok := paramType.Underlying().(*types.Basic); ok && isNarrowIntegerKind(paramBasic.Kind()) {
					switch callExpr.Args[i].(type) {
					case *ast.BinaryExpr, *ast.UnaryExpr:
						if argType := v.getType(callExpr.Args[i], false); argType != nil && types.Identical(argType, paramType) {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							callExprContext.castArgToType[i] = convertToCSTypeName(v.getAliasQualifiedTypeName(paramType, false))
						}
					}
				}
			}

			// An untyped INTEGER constant passed to a GENERIC parameter whose declared type IS the
			// ELEMENT type parameter of a sibling `~[]E`-constrained type parameter — the
			// `Index[S ~[]E, E comparable](s S, v E)` / `Insert[S ~[]E, E any](s S, i int, v ...E)`
			// shape (slices) — drives C# generic inference from the LITERAL's OWN C# type. Go infers
			// E from S's core type (`[]int` → E = Go `int` → `nint`), but C# has no analogue for
			// `~[]E` core-type inference: `where S : ISlice<E>` does NOT flow S's concrete element to
			// E, so C# infers E SOLELY from the value literal. A bare int literal is C# `int`
			// (System.Int32), so `Index(s, 2)` makes C# infer E=int and `slice<nint>` then fails the
			// `~[]int` constraint (CS0315/CS0411/CS1503). go/types has already resolved the literal to
			// E's instantiation (int→nint, byte, int64→long, …); emit it AT that C# type so inference
			// binds the intended argument. A resolved `int32`/`rune` (already System.Int32) and a
			// value convBasicLit already casts (`(nint)…L`) are left untouched — no cast, no noise (the
			// wholeExprIsCastOfType skip in convExprList drops the latter).
			//
			// The SECOND gate is the same defect reached from the other side, and the sibling lock is
			// blind to it: a FREELY-inferred type parameter that lands in an INVARIANT C# position.
			// C# repairs a mis-inferred instantiation wherever an implicit conversion bridges it —
			// `int`→`nint` is implicit, so a result that IS the bare type parameter (`Max[T](a, b T) T`)
			// converts at the use site and keeps its bare literal, no churn. A CONSTRUCTED type over the
			// parameter has no such conversion, because C# generics are INVARIANT: `Action<int, bool>`
			// is not `Action<nint, bool>`, `slice<int>` is not `slice<nint>`. So when the type parameter
			// reaches a func/slice/array/map/chan/pointer RESULT, the wrong instantiation is
			// unrepairable and must be prevented at the argument. internal/concurrent's own test suite
			// ships the control pair that separates the two exactly:
			// `expectMissing[K, V comparable](t, key K, want V) func(got V, ok bool)` called
			// `expectMissing(t, s, 0)` mis-infers V=int and the returned `Action<int, bool>` then
			// rejects the map's `nint` (CS1503 ×16), while `expectDeleted(…, 15) func(deleted bool)` —
			// same untyped literal, V absent from the result — compiles untouched.
			//
			// A generic NAMED result (`NewOption[T](v T) Option[T]`) is deliberately NOT included: it is
			// invariant too, but the explicit type-argument arm below already pins it (`NewOption<nint>(42)`),
			// and retyping the argument as well would only add redundant noise to a site that compiles.
			// An EXPLICITLY-instantiated call (`NewOption[nint](42)`) needs nothing either — Go wrote the
			// type argument and the converter emits it. A variadic slot flags every trailing element; an
			// existing per-arg cast (append/narrow-int/defer) is not overwritten.
			if paramHasArg {
				if pTP, ok := types.Unalias(paramType).(*types.TypeParam); ok &&
					(typeParamIsSliceElementOfSibling(funcSignature, pTP) ||
						typeParamReachesInvariantResult(funcSignature, pTP)) {
					lastArg := i

					if funcSignature.Variadic() && i == params.Len()-1 {
						lastArg = len(callExpr.Args) - 1
					}

					for j := i; j <= lastArg; j++ {
						if castType := v.genericInferenceArgCastType(callExpr.Args[j]); castType != "" {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							if _, exists := callExprContext.castArgToType[j]; !exists {
								callExprContext.castArgToType[j] = castType
							}
						}
					}
				}
			}

			// A func LITERAL bound to a parameter whose DECLARED signature returns a TYPE
			// PARAMETER — `sync.OnceValue[T any](f func() T)`, `OnceValues[T1, T2 any](f func()
			// (T1, T2))` — puts the lambda in C# type-argument INFERENCE position: C# derives the
			// type argument from the lambda's own return expressions, ignoring the Go result type
			// go/types already resolved. Arms that yield no C# type at all (a `panic`-terminated
			// body, `return nil` → `default!`) infer nothing (CS0411 ×4, sync oncefunc_test), and
			// arms whose NATURAL C# type merely differs from the Go result (`func() int { return
			// 42 }` → `Func<int>`, Go says `Func<nint>`) infer the wrong delegate (CS0029). Mark
			// the argument so convFuncLit states the declared result type explicitly; the lambda
			// then fixes the type argument to exactly what Go declared. Only a RESULT-position
			// type parameter qualifies — one appearing solely in the callee's parameter list
			// (slices.SortFunc's `cmp func(a, b E) int`) is inferred from the lambda's own typed
			// parameters, which are already correct.
			if paramHasArg {
				if _, isFuncLit := callExpr.Args[i].(*ast.FuncLit); isFuncLit {
					if paramSig, ok := types.Unalias(paramType).(*types.Signature); ok && signatureResultsContainTypeParams(paramSig) {
						if callExprContext.genericResultInferredFuncArgs == nil {
							callExprContext.genericResultInferredFuncArgs = make(map[int]bool)
						}

						callExprContext.genericResultInferredFuncArgs[i] = true
					}
				}
			}

			// A TYPE PARAMETER reads as an interface (its underlying is the constraint), which
			// routed a pointer-instantiated generic param into the interface arm and away from
			// the box treatment (`ptr = abi.Escape(ptr)` instantiating T = *T passed the
			// deref'd value alias - internal/weak Make, CS0029). When the instantiation is a
			// pointer, fall through to the pointer arm below.
			// A NAMED FUNC-type parameter receiving a value of a DIFFERENT delegate type
			// wraps in the target delegate's constructor — C# has no implicit conversion
			// between distinct delegate types (mirrors the composite-literal field rule;
			// archive/tar templateV7Plus's stringFormatter args, CS1503). Method groups and
			// func literals convert natively and are left bare.
			if paramHasArg {
				if paramNamed, ok := types.Unalias(paramType).(*types.Named); ok {
					if _, isSig := paramNamed.Underlying().(*types.Signature); isSig {
						if argType := v.getType(callExpr.Args[i], false); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(paramType)) {
							if _, argIsSig := argType.Underlying().(*types.Signature); argIsSig {
								// A GENERIC named delegate param renders unsubstituted type
								// params at the call site — leave it to native conversion.
								if _, isLit := callExpr.Args[i].(*ast.FuncLit); !isLit && !typeContainsTypeParams(paramType) {
									if callExprContext.wrapArgWithNew == nil {
										callExprContext.wrapArgWithNew = make(map[int]string)
									}

									callExprContext.wrapArgWithNew[i] = v.getCSharpTypeName(paramType)
								}
							}
						}
					}
				}

				// The MIRROR: a STRUCTURAL (anonymous) func-type parameter receiving a value that
				// RENDERS as a named delegate — h2's `sc.scheduleHandler(…, handler)` where the
				// parameter is the written `func(ResponseWriter, *Request)` (CS1503). Go converts
				// named→structural implicitly; C# needs the same delegate re-wrap, targeting the
				// synthesized structural delegate: `new Action<ResponseWriter, ж<Request>>(handler)`.
				// Two argument shapes render named: a value whose GO type is a named func type
				// (with methods — a METHODLESS named func type already renders as the structural
				// delegate itself and stays bare), and a `:=` local DECLARED from a method group,
				// which the declaration emission types with the matching package named delegate
				// (`HandlerFunc handler = Ꮡsc.Value.handler.ServeHTTP;` — see visitAssignStmt's
				// methodGroupDelegateType) even though go/types keeps it structural. Method
				// groups and func literals themselves convert natively and are excluded by both
				// gates.
				if _, paramIsSig := types.Unalias(paramType).(*types.Signature); paramIsSig && paramHasArg && !typeContainsTypeParams(paramType) {
					if argType := v.getType(callExpr.Args[i], false); argType != nil {
						argRendersNamedDelegate := false

						if argNamed, ok := types.Unalias(argType).(*types.Named); ok {
							if _, argIsSig := argNamed.Underlying().(*types.Signature); argIsSig {
								if _, collapses := methodlessNamedFuncSignature(argNamed); !collapses {
									argRendersNamedDelegate = true
								}
							}
						} else if argSig, ok := types.Unalias(argType).(*types.Signature); ok {
							if argIdent, ok := callExpr.Args[i].(*ast.Ident); ok &&
								v.namedFuncTypeNameForSignature(argSig) != "" && v.identDeclaredFromMethodGroup(argIdent) {
								argRendersNamedDelegate = true
							}
						}

						if argRendersNamedDelegate {
							if callExprContext.wrapArgWithNew == nil {
								callExprContext.wrapArgWithNew = make(map[int]string)
							}

							callExprContext.wrapArgWithNew[i] = v.getCSharpTypeName(paramType)
						}
					}
				}
			}

			// An ERASED pointer-core parameter ([P *T]) reads as an interface too (same
			// type-parameter shape as the instantiated-pointer carve-out above) — route it to
			// the pointer arm so a P-typed argument supplies its box (`clone(p)` inside another
			// erased generic must pass `Ꮡp`, not the deref'd value alias).
			if needsInterfaceCast, isEmpty := isInterface(paramType); needsInterfaceCast && !v.instantiatedParamIsPointer(callExpr, paramType, i) && !signatureErasedParamPointerOk(funcSignature, paramType) {
				callExprContext.u8StringArgOK[i] = false

				// A variadic interface parameter (`...Type` / `...any`) receives EVERY trailing
				// argument (getParameterType already yields the variadic ELEMENT type), so the
				// non-empty *T→iface adapter treatment (`makeSig(S, S, NewSlice(T))` left the
				// ж<Slice> result unwrapped otherwise — go/types builtins CS1503) must fan out to
				// all of them; a spread arg is excluded at consumption (convExprList's spreadArg
				// guard), and a non-variadic parameter degenerates to the single index (lastArg == i),
				// byte-identical to before.
				lastArg := i

				if funcSignature.Variadic() && i == params.Len()-1 {
					lastArg = len(callExpr.Args) - 1
				}

				for j := i; j <= lastArg; j++ {
					if !isEmpty {
						callExprContext.interfaceTypes[j] = paramType
					}

					// An untyped constant boxed into the interface must be cast to Go's DEFAULT TYPE for
					// its kind so its C# box matches Go's boxed dynamic type and a later `.(int)`
					// (`._<nint>()`) assertion, `case int:`, or `==` succeeds — see untypedConstBoxCast.
					// Reuses the per-argument castArgToType plumbing convExprList already applies as
					// `(nint)(value)`. This includes the VARIADIC `...any` slot (the fmt/print/log
					// family): a bare int literal there boxes as System.Int32, which formats the same
					// under %d/%v but is NOT Go's `int` — `testEqual("… n = %d …", n, 0)` compared the
					// two boxes and reported unequal (encoding/base32), so the cast is required there
					// too and the former literal-only fast path is gone.
					// isEmptyInterfaceTarget (not the outer isInterface) gates this to a REAL `any`
					// parameter: a type parameter constrained by `any` also reads as an empty interface
					// here, but its instantiation binds the argument to a concrete type (`T`=int → the
					// nint parameter), where a bare int literal already converts implicitly — unlike
					// the u8-span→@string case, no cast is needed and one would be spurious.
					// A string LITERAL argument takes the tighter `(@string)"…"` rendering instead
					// (the same either/or convCompositeLit and markAnyFieldLits apply to an `any`
					// element/field): the flag-less path leaves a bare C# string, which boxes
					// System.String where Go boxes `string`, so `eq(b.v, "seed")` compared false
					// against a field the composite-literal path had already boxed as @string.
					if isEmptyInterfaceTarget(paramType) && j < len(callExpr.Args) {
						if isStringBasicLit(callExpr.Args[j]) {
							callExprContext.u8StringArgOK[j] = true
							callExprContext.useGoStringArg[j] = true
						} else if castType := v.untypedConstBoxCast(callExpr.Args[j]); castType != "" {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							callExprContext.castArgToType[j] = castType
						}
					}

					// The EMPTY interface (`any`/`interface{}`) needs no wrapping adapter, but a
					// POINTER argument must still render as the pointer VALUE — the box `Ꮡp`, not the
					// deref'd value alias `p` — because Go boxes the *pointer* into the interface.
					// A deref-aliased pointer (a `*T` parameter, or the current method's direct-ж
					// receiver) whose box is dropped loses pointer identity: fmt's
					// `func (p *pp) free() { … ppFree.Put(p) }` put the `pp` VALUE into the sync.Pool,
					// so the next `Get().(*pp)` (rendered `._<ж<pp>>()`) failed the cast and panicked
					// on the 2nd pool round-trip. This mirrors the pointer-parameter branch below: a
					// pointer LOCAL already holds its box directly (`!v.isPointer`/param/direct-ж
					// guard excludes it), and a variadic `...any` fans the treatment across every
					// trailing argument (so it is NOT gated by variadicSlot, unlike the nint cast).
					// Since the empty interface leaves `interfaceTypes` unset, the argument takes the
					// identical convExpr path a `*T`-parameter argument does. Like the untyped-constant
					// box cast above, a variadic `...any` fans the treatment across every trailing
					// argument.
					if isEmpty && j < len(callExpr.Args) {
						// A func LITERAL into a real `any` slot is natural-typed by C# — mark it so
						// convFuncLit states the Go result type explicitly (emptyInterfaceArgs note).
						if isEmptyInterfaceTarget(paramType) {
							if _, isFuncLit := callExpr.Args[j].(*ast.FuncLit); isFuncLit {
								if callExprContext.emptyInterfaceArgs == nil {
									callExprContext.emptyInterfaceArgs = make(map[int]bool)
								}

								callExprContext.emptyInterfaceArgs[j] = true
							}
						}

						if argType := v.getType(callExpr.Args[j], false); argType != nil {
							// The FUNC half of this boundary. A func value crossing into a real
							// `any` parameter carries its Go type, and the delegate representing
							// nil is a bare `null` that carries nothing once boxed — the same
							// argument the pointer arm below makes for its box. Consumed in
							// convExprList, which applies the predicate that exempts method groups
							// and literals (they can never be null, and an extension method cannot
							// be invoked on a method group at all).
							if _, argIsFunc := argType.Underlying().(*types.Signature); argIsFunc {
								callExprContext.anyBoxedFuncArgs[j] = true
							}

							if _, argIsPtr := argType.(*types.Pointer); argIsPtr {
								ident := getIdentifier(callExpr.Args[j])

								if !v.isPointer(ident) || v.identIsParameter(ident) || v.exprIsCurrentDirectBoxReceiver(callExpr.Args[j]) {
									callExprContext.argTypeIsPtr[j] = true
								}

								// The OTHER half of the same boundary: the box that crosses must
								// carry its Go type even when it is nil — see
								// typedNilInterfaceBoxing.go (consumed in convExprList).
								callExprContext.anyBoxedPtrArgs[j] = true
							}
						}
					}
				}
			} else if paramHasArg && refLoweredPositions[i] && callExprContext.callArgs == nil && !(callExprContext.hasSpreadOperator && i == params.Len()-1) {
				// ж-box A2: a Phase-A ref-lowered position takes a `ref` argument (the §3.3
				// emission rows) instead of a box — a TOTAL replacement, so convExprList skips
				// the boxed render entirely. Defer/go sites never reach this arm (callArgs is
				// non-nil there) and keep the boxed carve-out below.
				if replacementArgs == nil {
					replacementArgs = make([]string, params.Len())
				}

				replacementArgs[i] = v.refLoweredArgReplacement(callExpr.Args[i], paramType, callExprContext.deferredDecls)
			} else if paramHasArg && (isPointer(paramType) || signatureErasedParamPointerOk(funcSignature, paramType) || v.instantiatedParamIsPointer(callExpr, paramType, i)) && !(callExprContext.hasSpreadOperator && i == params.Len()-1) {
				// paramHasArg guards Args[i]: a variadic pointer parameter called with no
				// trailing arguments (e.g. `In(r)` for `In(r rune, ...*RangeTable)`) has no
				// arg at the variadic index, so indexing Args[i] would panic.
				//
				// getParameterType returns the variadic *element* type, so isPointer is true
				// for `...*T`. When the call spreads a slice into the variadic (`f(s...)`), the
				// argument is the whole slice, not a single element pointer, so the element
				// address-of treatment (which would emit `Ꮡs`) must be skipped.
				// An `unsafe.Pointer` argument passed to an `unsafe.Pointer` parameter is passed as the
				// `@unsafe.Pointer` struct directly — NOT reduced to its inner `uintptr` via `.Value`.
				// `.Value` (a uintptr) converts implicitly to BOTH the `@unsafe.Pointer` parameter AND any
				// same-named method's `ж<T>` overload (golib has uintptr↔both), so the call goes
				// ambiguous — e.g. `add(p, x)` between the free `add(@unsafe.Pointer,…)` and a
				// `notInHeap.add(ж<…>,…)` extension (CS0121). The struct is an exact match for the
				// parameter, so it disambiguates.
				// ж-box A2, the defer/go boxed carve-out (§3.3): the eager argument keeps the
				// boxed emission below; the temp-param lambda body derives the ref at invoke
				// time (`ᴛN` renders as `ref ᴛN.DerefOrNull()` — see convExprList). visitDeferStmt/
				// visitGoStmt force the lambda form whenever the callee has lowered positions.
				if callExprContext.callArgs != nil && refLoweredPositions[i] {
					if callExprContext.refLoweredTempArgs == nil {
						callExprContext.refLoweredTempArgs = make(map[int]bool)
					}

					callExprContext.refLoweredTempArgs[i] = true
				}

				paramIsUnsafePtr := false

				if basic, ok := paramType.Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
					paramIsUnsafePtr = true
				}

				// A variadic pointer parameter (`...*T`) receives EVERY trailing argument, so the
				// per-argument box treatment below must apply to all of them — this loop only visits
				// declared parameters, and argTypeIsPtr's false default left args after the first as
				// the deref'd value alias (`checkInitialized(Ꮡp, q)`, edwards25519 CS1503). Mirrors
				// the variadic fan-out of the type-parameter @string treatment above; the spread form
				// (`f(s...)`) is already excluded by this branch's guard. A non-variadic parameter
				// degenerates to the single index (lastArg == i), byte-identical to before.
				lastArg := i

				if funcSignature.Variadic() && i == params.Len()-1 {
					lastArg = len(callExpr.Args) - 1
				}

				for j := i; j <= lastArg; j++ {
					argIsUnsafePtr := false

					if argType := v.getType(callExpr.Args[j], false); argType != nil {
						if basic, ok := argType.Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
							argIsUnsafePtr = true
						}
					}

					if paramIsUnsafePtr && argIsUnsafePtr {
						// pass the @unsafe.Pointer struct directly; no argTypeIsPtr / `.Value`
						continue
					}

					ident := getIdentifier(callExpr.Args[j])

					// A deref-aliased pointer (a parameter, or the current method's direct-ж receiver)
					// passed WHOLE as a pointer argument must be emitted as its box `Ꮡc`, not the value
					// alias `c` (a value cannot bind a `ж<T>` parameter → CS1503). A direct-ж receiver
					// is not an `identIsParameter`, so it needs the explicit receiver check — e.g.
					// `func (c *mcache) prepareForSweep(){ … stackcache_clear(c) }` where `c` also takes
					// a field address (making it direct-ж).
					if !v.isPointer(ident) || v.identIsParameter(ident) || v.exprIsCurrentDirectBoxReceiver(callExpr.Args[j]) {
						callExprContext.argTypeIsPtr[j] = true
					}
				}
			}
		}
	}

	callExprContext.replacementArgs = replacementArgs
	// ---- Phase 4: Go universe built-ins ----

	// Every arm below is keyed on a built-in's NAME; a shadowing declaration of that name makes the
	// call an ordinary one, so the whole group is gated on the identifier actually resolving to the
	// universe built-in (see identIsUniverseBuiltin).
	if ident, ok := callExpr.Fun.(*ast.Ident); ok && v.identIsUniverseBuiltin(ident) {
		// `len(p)`/`cap(p)` on a pointer-to-array. A ж<array> argument has no golib overload (the
		// wrapper implements IArray, its BOX does not — CS1503, runtime proc.go's
		// `len(mp.cgoCallers)` where cgoCallers is `*cgoCallers`).
		//
		// Go computes both from the TYPE: each is N, the expression is a CONSTANT, and the operand
		// is not evaluated. Emit the constant. The deref form emitted here previously
		// (`len(p.Value)`) is wrong for a NIL pointer — Go answers N, a deref throws — and reflect's
		// TestValue_Cap discriminates deliberately: it checks `cap(a)` on `a := &[3]int{1,2,3}`,
		// then sets `a = nil` and checks `cap(a)` AGAIN, still expecting 3. A fix that derefs passes
		// the first assertion and dies on the second.
		//
		// The pointee no longer has to be NAMED. It was, so an UNNAMED `*[3]int` fell through to
		// golib's `cap(IArray)` holding a `ж<array<nint>>` — and golib cannot fix that from its
		// side: `array<T>` keeps its length in the VALUE (C# generics cannot encode N), so a nil box
		// has no N to read. Only the converter knows N, from the static type.
		//
		// Go DOES still evaluate operand function calls and channel receives (the spec's
		// "not evaluated" carries that carve-out), so those keep the evaluating form rather than
		// having their side effect silently dropped.
		if (ident.Name == "len" || ident.Name == "cap") && len(callExpr.Args) == 1 {
			if ptr, ok := v.info.TypeOf(callExpr.Args[0]).(*types.Pointer); ok {
				if arr, isArray := ptr.Elem().Underlying().(*types.Array); isArray {
					if !v.exprHasCallOrReceive(callExpr.Args[0]) {
						return fmt.Sprintf("%d", arr.Len())
					}

					return fmt.Sprintf("%s(%s.Value)", ident.Name, v.convExpr(callExpr.Args[0], nil))
				}
			}
		}

		// close(ch) on a NAMED channel type cannot infer `close<T>(in channel<T>)`'s element
		// from the [GoType("chan T")] wrapper (a user-defined conversion is invisible to
		// generic inference, CS0411) — name the element type explicitly so the wrapper's
		// implicit conversion applies at the argument (see namedChanElemTypeArg). A package
		// that ALSO declares a `close` method shadows the using-static builtin (C# member
		// lookup stops at the class's own group — net/http's `(*conn).close`), so the same
		// `builtin.` qualification the general builtin path applies is needed here too.
		if ident.Name == "close" && len(callExpr.Args) == 1 {
			if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); isBuiltin {
				if typeArgs := v.namedChanElemTypeArg(callExpr.Args[0]); typeArgs != "" {
					funcName := ident.Name

					if packageBuiltinShadows[ident.Name] {
						funcName = "builtin." + funcName
					}

					return fmt.Sprintf("%s%s(%s)", funcName, typeArgs, v.convExpr(callExpr.Args[0], nil))
				}
			}
		}

		// `complex(re, im)` picks its golib overload BY ARGUMENT WIDTH — `complex(float32,
		// float32) => complex64`, `complex(float64, float64) => complex128` — and an
		// `UntypedFloat` argument converts implicitly to BOTH. C# then prefers the BETTER
		// CONVERSION TARGET, which is the NARROWER one, so a complex128 the Go checker typed as
		// such was silently constructed at float32 width: `complex(math.MaxFloat32*2,
		// math.MaxFloat32*2)` came out +Inf where Go has 6.8e38, and encoding/gob's TestOverflow
		// then found nothing out of complex64's range to reject. LITERAL arguments already carry
		// the width — the untyped-const analysis records the element type as their context and
		// convBasicLit renders the F/D suffix from it (`complex(1.5D, 2.5D)`) — but a NAMED
		// untyped const (`Δmath.MaxFloat32`), or a constant expression over one, renders as the
		// UntypedFloat symbol and cannot. Pin the untyped arguments of exactly those calls to the
		// element width Go's own typing gives the call.
		//
		// The rule cannot be expressed from golib's side: naming the untyped pair explicitly makes
		// every MIXED call ambiguous, and completing all four pairings does not rescue it either,
		// because UntypedFloat converts implicitly in BOTH directions with float32 and float64 —
		// for an operand that is neither, no candidate is strictly better and the ambiguity simply
		// moves (docs/phase4/BOARD-next-validation-candidates.md, gob root 7).
		if ident.Name == "complex" && len(callExpr.Args) == 2 {
			if elementType := v.complexCallElementType(callExpr); elementType != nil &&
				(v.containsUntypedNamedConstRef(callExpr.Args[0]) || v.containsUntypedNamedConstRef(callExpr.Args[1])) {
				elementCS := v.getCSharpTypeName(elementType)
				args := make([]string, len(callExpr.Args))

				for i, arg := range callExpr.Args {
					args[i] = v.convExpr(arg, nil)

					// Only an UNTYPED argument is unpinned; one that already has a Go type
					// renders at that width and needs nothing (and pinning either operand is
					// enough to decide the overload — float64 has no implicit conversion to
					// float32, and a float32-width call cannot have a float64 operand).
					if argBasic, ok := v.info.TypeOf(arg).(*types.Basic); ok && argBasic.Info()&types.IsUntyped != 0 {
						args[i] = fmt.Sprintf("(%s)(%s)", elementCS, args[i])
					}
				}

				funcName := ident.Name

				if packageBuiltinShadows[ident.Name] {
					funcName = "builtin." + funcName
				}

				return fmt.Sprintf("%s(%s)", funcName, strings.Join(args, ", "))
			}
		}

		// Handle make call as a special case
		if ident.Name == "make" {
			typeExpr := callExpr.Args[0]
			typeParam := v.info.TypeOf(typeExpr)
			typeName := convertToCSTypeName(v.getExpressionTypeName(typeExpr, false))
			remainingArgs := v.convExprList(callExpr.Args[1:], callExpr.Lparen, callExprContext)
			isTypeParam := false

			if typeConstraint, ok := typeParam.(*types.TypeParam); ok {
				typeParam = v.getConstraintType(typeConstraint)
				isTypeParam = typeParam != nil
			}

			if typeParam != nil {
				// Underlying: `make(closeWaiter)` of a NAMED channel type (`type closeWaiter
				// chan struct{}`) takes the same unbuffered default as a plain `make(chan T)` —
				// the wrapper's `(nint size)` constructor forwards to `channel<T>(size)`.
				// Capacity 0 is a REAL unbuffered (rendezvous) channel: golib's channel<T> now
				// models Go's hchan, so `make(chan T)` and `make(chan T, 1)` are distinct
				// (cap 0 vs 1) — the old "1" default conflated them.
				if _, ok := typeParam.Underlying().(*types.Chan); ok && !isTypeParam {
					if len(remainingArgs) == 0 {
						remainingArgs = "0"
					}
				}

				// `make(StrIntMap)` of a DEFINED map type (`type StrIntMap map[string]int`) with NO
				// size argument: the generated wrapper struct has an allocating `(nint size)` ctor
				// but NO parameterless one, so a bare `new StrIntMap()` is default(StrIntMap) — a
				// NIL map (its backing store is null), making `m == nil` TRUE. Go's `make` yields a
				// NON-nil empty map (`m == nil` is false), so default the size to 0 to run the
				// allocating ctor. Scoped to *types.Named defined types: the UNNAMED `map<K,V>`
				// builtin already has an allocating parameterless ctor and is left as `new map<K,V>()`.
				if _, isNamed := typeParam.(*types.Named); isNamed && !isTypeParam {
					if _, ok := typeParam.Underlying().(*types.Map); ok && len(remainingArgs) == 0 {
						remainingArgs = "0"
					}
				}

				// make(T, len[, cap]) for a slice/map/chan — the golib `slice<T>(nint length, nint
				// capacity)` / `map<K,V>(nint capacity)` / `channel<T>(nint capacity)` ctors all take nint.
				// A length/cap/hint whose Go type is an integer with NO implicit C# conversion to nint
				// (`uintptr`/`uint`/`uint32`/`uint64`/`int64` → `nuint`/`uint`/`ulong`/`long`) has no
				// applicable ctor — for a slice, overload resolution falls onto `slice<T>(T[])` (CS1503:
				// `nuint`→`byte[]`, runtime/mbitmap `make([]byte, n/goarch.PtrSize)`); for a map/chan it is
				// a direct `nuint`→`nint` CS1503. Cast such args to nint. A plain `int` (nint) or an untyped
				// constant binds directly and is left alone.
				if !isTypeParam && len(callExpr.Args) > 1 {
					switch typeParam.Underlying().(type) {
					case *types.Slice, *types.Map, *types.Chan:
						remainingArgs = v.makeLenArgs(callExpr.Args[1:])
					}
				}

				// `make([]E, len[, cap])` whose ELEMENT zero value must itself be constructed: golib's
				// slice ctor fills its backing with `default(T)`, which is not usable storage for an
				// unnamed nested array — whose length lives only in the Go type, never in `array<T>` —
				// or for a struct whose own zero value needs construction. `make([][hashSize]int, n)`
				// therefore produced n zero-LENGTH arrays, and the first `grid[i][j] +=` panicked with
				// index-out-of-range (hash/maphash's avalancheTest1; image/draw's Floyd-Steinberg
				// quantError rows and x/text/transform's chain buffers carry the same latent defect).
				// Thread the SAME element factory the fixed-array paths already build
				// (arrayZeroValueArgs/arrayElemFactory, mirroring go2cs-gen's field initializers), which
				// golib's `slice<T>(nint, Func<T>, nint)` fills the backing with. Every other element
				// type keeps the plain length ctor, so only genuinely nested shapes change.
				if !isTypeParam && len(callExpr.Args) > 1 {
					if sliceType, isSlice := typeParam.Underlying().(*types.Slice); isSlice {
						if factory := v.arrayElemFactory(sliceType.Elem()); factory != "" {
							parts := []string{v.makeLenArgs(callExpr.Args[1:2]), "() => " + factory}

							if len(callExpr.Args) > 2 {
								parts = append(parts, v.makeLenArgs(callExpr.Args[2:3]))
							}

							remainingArgs = strings.Join(parts, ", ")

							// A DEFINED slice type (`type SortedMap []KeyValue`) is not `slice<E>`: its
							// go2cs-gen wrapper declares `T(nint length, nint capacity = -1, nint low = 0)`
							// and NO element-factory overload, so the lambda binds to `nint`
							// (CS1660 at internal/fmtsort's `make(SortedMap, 0, n)` — which breaks that
							// package AND, once its regenerated `.cs` is on disk, every banked package
							// downstream of fmt). Build the factory-filled backing as the underlying
							// slice<E> — the same value the unnamed form produces — and hand it to the
							// wrapper's `T(slice<E> value)` ctor, which the generator always emits.
							if _, isNamed := typeParam.(*types.Named); isNamed {
								remainingArgs = fmt.Sprintf("new %s(%s)", v.getCSharpTypeName(sliceType), remainingArgs)
							}
						}
					}
				}

				if isTypeParam {
					return v.withSliceElemDims(fmt.Sprintf("make<%s>(%s)", typeName, remainingArgs), typeParam)
				}

				// `make(chan<- T[, n])` is where a DIRECTIONAL channel value is born, and the one
				// place the converter can still see a direction the managed type cannot hold — so
				// it rides along as the channel constructor's second argument, descriptor cargo the
				// reflection bridge reads back (see chanDirectionCargo.go). A bidirectional or
				// defined channel type stamps nothing and emits byte-identically.
				if cargo := chanCargoExpr(typeParam); cargo != "" {
					// Increment D: the element is a channel or an array, so the whole cargo rides
					// the constructor instead of one direction (chanDirectionCargo.go).
					remainingArgs += ", " + cargo
				} else if dir := chanDirCargoName(typeParam); dir != "" {
					remainingArgs += ", " + dir
				}

				// `make([][3]uint8, n)` is a slice CREATION site: the element array's length is
				// statically known here and, once the made slice is empty, nowhere else.
				if v.options.preferVarDecl {
					return v.withSliceElemDims(fmt.Sprintf("new %s(%s)", typeName, remainingArgs), typeParam)
				}

				return v.withSliceElemDims(fmt.Sprintf("new(%s)", remainingArgs), typeParam)
			}

			v.showWarning("@convCallExpr - unexpected call to 'make' method for type '%s'", typeName)
			return fmt.Sprintf("make\u01C3<%s>(%s)", typeName, remainingArgs)
		}

		// Handle new call as a special case
		if ident.Name == "new" {
			typeExpr := callExpr.Args[0]
			typeName := convertToCSTypeName(v.getExpressionTypeName(typeExpr, false))

			// `new([N]T)` — golib's `@new<T>()` builds the zero value through the parameterless
			// constructor, and `array<T>()` has NO length (the N lives only in the Go type), so
			// the box came back with a length-0 backing: `len(*p)` reported 0 (Go says N) and the
			// first indexed write panicked (flate's `f.bits = new([maxNumLit+maxNumDist]int)`).
			// Construct the SIZED value and box it, threading the element factory for nested /
			// construction-needing elements exactly as the var-declaration paths do
			// (arrayZeroValueArgs). Every other type keeps the zero-value `@new<T>()` form.
			// A NAMED array type (`type row [4]byte`) is excluded: its generated wrapper allocates
			// its own backing lazily from the size it knows, so the zero-value form is already
			// right (same carve-out arrayElemFactory makes).
			newType := v.getExprType(typeExpr)
			_, newTypeIsNamed := types.Unalias(newType).(*types.Named)

			if newType != nil && !newTypeIsNamed {
				if arrayType, isArray := newType.Underlying().(*types.Array); isArray {
					return fmt.Sprintf("%s(new %s(%s))", AddressPrefix, typeName,
						v.arrayZeroValueArgs(strconv.FormatInt(arrayType.Len(), 10), arrayType))
				}
			}

			// `new(chan<- T)` — the same shape one level down: `@new<channel<T>>()` builds the zero
			// value, which for a channel is the NIL one, and nothing in a bare `default` carries the
			// direction the pointee's Go type has. Box the direction-carrying nil instead, which is
			// the position reflectlite's `TypeOf(new(<-chan int)).Elem()` reads (a pointer descriptor
			// hands its pointee's direction down unshifted, exactly as it does array dims).
			if nilChan := v.chanDirNilValue(newType); nilChan != "" {
				return fmt.Sprintf("%s(%s)", AddressPrefix, nilChan)
			}

			return fmt.Sprintf("@new<%s>()", typeName)
		}

		// Handle append: an untyped-constant variadic element (e.g. `append(buf, replacementChar)`)
		// makes C# overload resolution ambiguous — the `append<T>(ISlice, params T[])` overload
		// infers T from the element (the untyped wrapper) while the `slice<T>` overloads infer T
		// from the slice. Cast such elements to the slice's element type, matching Go's implicit
		// conversion and the already-working explicitly-converted element pattern (`uint16(r)`).
		if ident.Name == "append" && len(callExpr.Args) >= 2 && !callExpr.Ellipsis.IsValid() {
			// A Go array VALUE appended as a slice ELEMENT (`append(rows, arr)` with rows of
			// type [][3]int) is copied into the new slot; an existing-storage array element
			// clones so the stored element does not alias the source's backing (see
			// exprReadsValueNeedingClone). A spread `append(dst, src...)` is untouched —
			// its element-wise copy happens inside golib (a documented remaining gap for
			// nested-array elements, alongside copy()).
			for i := 1; i < len(callExpr.Args); i++ {
				if v.exprReadsValueNeedingClone(callExpr.Args[i]) {
					if callExprContext.cloneArrayArg == nil {
						callExprContext.cloneArrayArg = make(map[int]bool)
					}

					callExprContext.cloneArrayArg[i] = true
				}
			}

			if sliceType := v.info.TypeOf(callExpr.Args[0]); sliceType != nil {
				if sliceUnder, ok := sliceType.Underlying().(*types.Slice); ok {
					// A bare `nil` element (untyped nil → `default!`) binds to append's `params`
					// parameter as the WHOLE null array — appending ZERO elements — instead of as a
					// single nil element: `append(b.lines, nil)` on `[][]cell` never grew the slice
					// (tabwriter addLine, which left b.lines empty and every terminateCell indexing
					// `b.lines[len-1]` panicking [-1]). Cast the nil to the element type so it binds as
					// ONE element. The interface and named-composite branches below already do this for
					// their element kinds (both differ from the untyped nil); this covers the remaining
					// nillable element types — unnamed slice/map/pointer/chan/func — for which no other
					// branch fires. A nil element is only ever valid when the element type is nillable,
					// so the cast target always exists.
					for i := 1; i < len(callExpr.Args); i++ {
						if tv, ok := v.info.Types[callExpr.Args[i]]; ok && tv.IsNil() {
							if callExprContext.castArgToType == nil {
								callExprContext.castArgToType = make(map[int]string)
							}

							callExprContext.castArgToType[i] = convertToCSTypeName(v.getAliasQualifiedTypeName(sliceUnder.Elem(), false))
						}
					}

					// Only numeric element types are affected (the wrong-element-type overload
					// selection is a numeric-conversion artifact); skip otherwise.
					if elemBasic, ok := sliceUnder.Elem().Underlying().(*types.Basic); ok && elemBasic.Info()&types.IsNumeric != 0 {
						elemCSType := convertToCSTypeName(v.getAliasQualifiedTypeName(sliceUnder.Elem(), false))

						for i := 1; i < len(callExpr.Args); i++ {
							if v.isUntypedNumericConstArg(callExpr.Args[i]) {
								if callExprContext.castArgToType == nil {
									callExprContext.castArgToType = make(map[int]string)
								}
								callExprContext.castArgToType[i] = elemCSType
							}
						}
					}

					// An INTERFACE element appended from a value of a DIFFERENT type — a pointer
					// rendering as the *T→iface ADAPTER ctor (`new rtypeᴵΔType(Ꮡt)`) or a raw
					// struct value (`new Dog(nil)`) — leaves both append overloads applicable
					// (`append<T>(ISlice, params T[])` infers the concrete/adapter type,
					// `append<T>(slice<T>, params Span<T>)` infers the interface — CS0121 ×3,
					// reflect Method construction). Cast the element to the interface type so the
					// slice<T> overload binds; an already-interface-typed element stays bare. The
					// EMPTY interface (`any`) is affected identically: `append(args[:len:len], c.output)`
					// with `args []any` and `c.output []byte` infers T=slice<byte> on the ISlice
					// overload but T=any on the slice<T> overload (testing flushToParent, CS0121) —
					// cast to `any` so both agree.
					if needsCast, _ := isInterface(sliceUnder.Elem()); needsCast {
						elemCSType := convertToCSTypeName(v.getAliasQualifiedTypeName(sliceUnder.Elem(), false))

						for i := 1; i < len(callExpr.Args); i++ {
							if argType := v.info.TypeOf(callExpr.Args[i]); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(sliceUnder.Elem())) {
								if callExprContext.castArgToType == nil {
									callExprContext.castArgToType = make(map[int]string)
								}
								callExprContext.castArgToType[i] = elemCSType
							}
						}
					}

					// `append` is a BUILT-IN, so its arguments never reach the declared-parameter
					// loop that applies the pointer-into-`any` boundary — but an `[]any` element
					// slot IS that boundary (typedNilInterfaceBoxing.go). Mark the pointer elements
					// here: they cross as the box, carrying their Go type. The `(any)` cast above
					// still applies, around the result.
					if isEmptyInterfaceTarget(sliceUnder.Elem()) {
						for i := 1; i < len(callExpr.Args); i++ {
							// The func sibling of the pointer arm below, at the same built-in slot.
							if argType := v.getType(callExpr.Args[i], false); argType != nil {
								if _, argIsFunc := argType.Underlying().(*types.Signature); argIsFunc {
									callExprContext.anyBoxedFuncArgs[i] = true
								}
							}

							if _, argIsPtr := v.getType(callExpr.Args[i], false).(*types.Pointer); argIsPtr {
								callExprContext.argTypeIsPtr[i] = true
								callExprContext.anyBoxedPtrArgs[i] = true
							}
						}
					}

					// A NAMED-COMPOSITE element type (slice/struct/map — NOT interface or basic, those are
					// handled by the branches above) appended from a value of a DIFFERENT type: crypto/x509/pkix
					// append(rdns, s) where rdns is RDNSequence ([]RelativeDistinguishedNameSET) and s is
					// []AttributeTypeAndValue (the element UNDERLYING). Go implicitly converts s to the element
					// type; C# append otherwise infers the element as slice<ATV>, so the result slice<slice<ATV>>
					// does not bind RDNSequence (CS0029). Cast the differing arg to the named element type.
					if named, isNamed := sliceUnder.Elem().(*types.Named); isNamed {
						switch named.Underlying().(type) {
						case *types.Basic, *types.Interface:
							// numeric / interface element casts are handled above
						default:
							elemCSType := convertToCSTypeName(v.getAliasQualifiedTypeName(sliceUnder.Elem(), false))

							for i := 1; i < len(callExpr.Args); i++ {
								if argType := v.info.TypeOf(callExpr.Args[i]); argType != nil && !types.Identical(types.Unalias(argType), types.Unalias(sliceUnder.Elem())) {
									if callExprContext.castArgToType == nil {
										callExprContext.castArgToType = make(map[int]string)
									}

									callExprContext.castArgToType[i] = elemCSType
								}
							}
						}
					}
				}
			}
		}

		// THE SLICE-SHAPED SPREAD (the Span int32-ceiling arc, ruled post-B2): a slice-typed
		// spread operand passes AS THE SLICE IT IS — binding golib's ISlice<T>-taking append
		// overloads — instead of projecting through `.ꓸꓸꓸ` to a Span at the call boundary, so
		// lengths stay nint end to end and a constrained (generic-body) spread stops needing a
		// span the interface must mint. Strings keep the span route: their spread is a byte
		// projection, not a slice. types.CoreType answers uniformly for unnamed slices, named
		// slice types and `~[]E`-constrained type parameters.
		if ident.Name == "append" && callExpr.Ellipsis.IsValid() && len(callExpr.Args) == 2 {
			if operandType := v.info.TypeOf(callExpr.Args[1]); operandType != nil {
				_, operandIsSlice := operandType.Underlying().(*types.Slice)

				if !operandIsSlice {
					if tp, ok := types.Unalias(operandType).(*types.TypeParam); ok && typeParamSliceCore(tp) != nil {
						operandIsSlice = true
					}
				}

				if operandIsSlice {
					callExprContext.spreadArgAsSlice = true

					// A constrained DESTINATION (`append(s, v...)` over `S ~[]E`) binds the
					// S-generic ISlice overload, whose element type never infers from a
					// constraint surface — emit both type arguments explicitly.
					if dstType := v.info.TypeOf(callExpr.Args[0]); dstType != nil {
						if tp, ok := types.Unalias(dstType).(*types.TypeParam); ok {
							if dstCore := typeParamSliceCore(tp); dstCore != nil {
								callExprContext.appendTypeArgs = fmt.Sprintf("<%s, %s>",
									convertToCSTypeName(v.getAliasQualifiedTypeName(dstType, false)),
									convertToCSTypeName(v.getAliasQualifiedTypeName(dstCore.Elem(), false)))
							}
						}
					}
				}
			}
		}

		// `delete(m, k)` on an `any`-KEYED map is the third built-in whose argument crosses into
		// interface space without passing a declared parameter (see append above and panic below):
		// the key is boxed to look the entry up, so a pointer key must cross as its box carrying
		// its Go type, or it cannot match the entry the composite-literal / index-store path wrote.
		if ident.Name == "delete" && len(callExpr.Args) == 2 {
			if mapArgType := v.getType(callExpr.Args[0], false); mapArgType != nil {
				if mapType, ok := mapArgType.Underlying().(*types.Map); ok {
					if _, keyIsPtr := v.getType(callExpr.Args[1], false).(*types.Pointer); keyIsPtr && isEmptyInterfaceTarget(mapType.Key()) {
						callExprContext.argTypeIsPtr[1] = true
						callExprContext.anyBoxedPtrArgs[1] = true
					}
				}
			}
		}

		// Handle panic call as a special case
		if ident.Name == "panic" {
			// The deferred/spawned argument slot belongs to the ENCLOSING LambdaContext, which
			// the basic-literal context below shadows — read it first (see the substitution
			// at the end of this arm).
			deferCallArgs := context.callArgs
			deferRenderParams := context.renderParams

			context := DefaultBasicLitContext()
			context.u8StringOK = false
			context.spanTargetUnsupported = true

			// `panic`'s Go parameter IS `any`, and this arm renders its argument outside the
			// declared-parameter loop — so the pointer-into-`any` boundary is applied here
			// directly (typedNilInterfaceBoxing.go). A recovered typed nil must still answer
			// `r.(*T)`, which a bare null cannot.
			panicValueType := types.NewInterfaceType(nil, nil)
			contexts := v.emptyInterfacePointerContexts(panicValueType, callExpr.Args[0], []ExprContext{context})
			panicValue := v.boxPointerIntoEmptyInterface(panicValueType, callExpr.Args[0], v.convExpr(callExpr.Args[0], contexts))

			// ...and the UNTYPED-CONSTANT boundary is the same story one step on. `panic`'s
			// parameter is `any`, so an untyped constant argument takes Go's DEFAULT type there —
			// `panic(0xdead)` panics with an `int` — and every ordinary call site already applies
			// that rule inside the declared-parameter loop this arm bypasses. Without it the value
			// boxed as C#'s `int`, which is Go's INT32, so a recovering `r.(int)`, a `case int:` and
			// a `reflect.DeepEqual(recover(), 0xdead)` all saw a type Go never panicked with.
			// encoding/json's TestMarshalPanic and TestUnmarshalPanic compare exactly that.
			//
			// NUMERIC constants only. A `panic("…")` needs no cast here because golib's `panic`
			// normalizes a C# string to `@string` at that single boxing boundary already — and it
			// does so DELIBERATELY, to cover the computed and hand-owned callers a per-site cast
			// cannot reach. Re-stating it here would restate nothing and churn every `panic("…")`
			// in the corpus (measured: 279 files).
			if tv, isConst := v.info.Types[callExpr.Args[0]]; isConst && tv.Value != nil {
				if basic, isBasic := tv.Type.(*types.Basic); isBasic && basic.Info()&types.IsString == 0 {
					panicValue = v.boxUntypedConstAsDefaultType(panicValueType, callExpr.Args[0], panicValue)
				}
			}

			// A DEFERRED or SPAWNED panic (`defer panic(err)` — go/types check_test.go:170) must
			// capture its argument at defer/go time: Go evaluates a deferred call's arguments when
			// the DEFER statement executes, not when the frame unwinds, so a `panic(err)` deferred
			// before `err` is reassigned must report the value `err` held at the defer. `panic` is
			// the one built-in this arm renders as a `throw` rather than as a call, so its argument
			// never reaches convExprList — the single place the temp-parameter (ᴛN) substitution
			// happens and the eager-argument slot is filled. Left alone, the registration emitted an
			// EMPTY argument slot (`defer(ᴛ1 => throw panic(errΔ2), , ref ᒐ)` — CS0839) while the
			// body inlined the ORIGINAL expression, which would additionally have read the variable
			// at unwind time. Perform the substitution here: the boxed value becomes the eager
			// argument, and the thunk throws its parameter. (Capturing the expression in the lambda
			// body instead — dropping the parameter — compiles but is semantically WRONG for exactly
			// the reassignment case above.) Both visitDeferStmt and visitGoStmt force the temp-param
			// form for a built-in callee, so renderParams is set whenever callArgs is.
			if deferCallArgs != nil && len(deferCallArgs) == 1 && deferRenderParams {
				deferCallArgs[0] = panicValue

				return fmt.Sprintf("throw panic(%s1)", TempVarMarker)
			}

			return fmt.Sprintf("throw panic(%s)", panicValue)
		}
	}

	// ---- Phase 5: function-literal and lambda arguments ----

	lambdaContext := DefaultLambdaContext()
	lambdaContext.isCallExpr = true
	lambdaContext.isPointerCast = context.isPointerCast
	lambdaContext.deferredDecls = context.deferredDecls
	// A deferred call target (`defer func(){…}()`) is a function literal whose recover() binds to
	// the enclosing function; pass that through so convFuncLit does not give it its own wrapper.
	lambdaContext.deferCall = context.deferCall

	var typeParamExpr string
	resultType := v.info.TypeOf(callExpr)

	if resultType != nil {
		// A call whose RESULT is a generic instantiation needs the type arguments when the
		// callee IS the type (a conversion/constructor form) or a GENERIC function (whose
		// untyped-const args would infer C# int where Go infers nint — `NewOption<nint>(42)`).
		// A plain NON-generic function returning a generic named type
		// (`func countdown(n int) Seq[int]`) must not have them appended (`countdown<nint>(5)`
		// — CS0308 on a non-generic method).
		funIsType := false
		calleeIsGeneric := false

		if tv, ok := v.info.Types[callExpr.Fun]; ok {
			funIsType = tv.IsType()
		}

		if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
			if funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func); ok {
				if sig, ok := funcObj.Type().(*types.Signature); ok && sig.TypeParams() != nil && sig.TypeParams().Len() > 0 {
					calleeIsGeneric = true
				}
			}
		}

		if named, ok := resultType.(*types.Named); ok && (funIsType || calleeIsGeneric) {
			if named.TypeArgs().Len() > 0 {
				typeArgs := named.TypeArgs()

				// A GENERIC FUNCTION's explicit arguments must come from the CALLEE's resolved
				// instantiation (info.Instances), not the RESULT type's arguments — the lists
				// differ whenever the callee has more type parameters than the result names
				// (reflect's `rangeNum[T, N](num N) iter.Seq[T]` called `rangeNum[int8](v)`: the
				// result Seq[T] carries ONE argument where the method needs TWO — CS0305 ×11).
				// The result's OWN arguments still gate WHETHER to emit (a generic callee
				// returning a plain named type keeps C# inference — no churn); the conversion/
				// constructor form (funIsType) keeps the result's arguments outright.
				var typeParams []string

				if calleeIsGeneric && !funIsType {
					if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
						if instance, ok := v.info.Instances[funIdent]; ok && instance.TypeArgs != nil {
							// Instance-derived positions align with the callee's declared type
							// parameters, so erased (pointer-core) positions leave the emitted
							// list (see renderedTypeArgs) — identical output when nothing erases.
							typeParams = v.renderedTypeArgs(funIdent, instance.TypeArgs)
						}
					}
				}

				if typeParams == nil {
					for i := range typeArgs.Len() {
						typeParams = append(typeParams, v.getCSharpTypeName(typeArgs.At(i)))
					}
				}

				if len(typeParams) > 0 {
					typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
				}
			}
		}

		// A call to a GENERIC FUNCTION whose resolved type arguments involve a TYPE PARAMETER —
		// slices.Sort's helper chain (`Sort[S ~[]E, …]` calling `pdqsortOrdered(x, …)`), pdqsort's
		// recursion — must render its type arguments EXPLICITLY: Go infers them through core
		// types, but C# never infers a type parameter that appears only in constraints (CS0411,
		// 14 sites in the slices/maps wave). go/types already resolved every instantiation
		// (info.Instances); a concrete instantiation still infers fine in C# and stays bare
		// (no churn). A SELF-REFERENTIAL constraint forces the same explicit rendering for the
		// opposite reason: C# WOULD infer, and would infer the box that cannot satisfy the bound
		// (see callNeedsConstraintProxy).
		if len(typeParamExpr) == 0 {
			if funIdent := getCallFunIdent(callExpr.Fun); funIdent != nil {
				if instance, ok := v.info.Instances[funIdent]; ok && instance.TypeArgs != nil &&
					(v.calleeHasConstraintOnlyTypeParam(funIdent) || v.callHasMethodGroupArg(callExpr) ||
						v.calleeTypeParamUnsuppliedByCall(callExpr, funIdent) ||
						v.callNeedsConstraintProxy(funIdent, instance.TypeArgs)) {
					// Erased (pointer-core) callee positions leave the emitted list — `clone[P *T,
					// T any]` emits `clone<ΔSignature>(…)` (see renderedTypeArgs); a list that
					// erases to empty stays bare.
					if typeParams := v.renderedTypeArgs(funIdent, instance.TypeArgs); len(typeParams) > 0 {
						typeParamExpr = fmt.Sprintf("<%s>", strings.Join(typeParams, ", "))
					}
				}
			}
		}

		// In a pointer cast, we need to intermediately cast the target expression to an uintptr.
		// This is required since unsafe.Pointer is in its own library and no implicit cast can
		// be added for it on the pointer class (ж<T>) in the core library without creating a
		// circular dependency.
		if resultType.String() == "unsafe.Pointer" {
			if len(constructType) == 0 {
				// …unless the call IS a bare expression statement, where nothing consumes the
				// result and C# allows only a call, never a cast: `func() { SwapPointer(nil, nil) }`
				// (sync/atomic's nil-deref table) emitted `(uintptr)SwapPointer(nil, nil);` — CS0201.
				// Node identity, not a flag, so a nested call in the same statement whose value IS
				// consumed keeps the prefix (see visitExprStmt).
				if callExpr != v.resultDiscardedExpr {
					constructType = "(uintptr)"
				}
			} else if len(callExpr.Args) == 1 && v.currentFuncSignature != nil {
				// The ref-based extension-function rewrite below only applies to a pointer-receiver
				// METHOD whose argument aliases the receiver; a package-scope initializer (e.g.
				// `var p uintptr = uintptr(unsafe.Pointer(&x))` in cmp_test.go) has no enclosing
				// function, so currentFuncSignature is nil — skip the receiver handling and fall
				// through to the normal `(uintptr)new @unsafe.Pointer(...)` emission.
				// Check if current function is a receiver function
				if v.currentFuncSignature.Recv() != nil {
					// Get the receiver type
					recvType := v.currentFuncSignature.Recv().Type()

					// Check if receiver is a pointer type
					isRecvPointer := false

					if ptrType, ok := recvType.(*types.Pointer); ok {
						recvType = ptrType.Elem()
						isRecvPointer = true
					}

					// Get the unsafe.Pointer call argument type
					argType := v.info.TypeOf(callExpr.Args[0])
					isArgPointer := false

					// Check if the argument is a pointer
					if ptrType, ok := argType.(*types.Pointer); ok {
						argType = ptrType.Elem()
						isArgPointer = true
					}

					// Check if the receiver type is pointer and call argument matches
					if isRecvPointer && isArgPointer && types.Identical(recvType, argType) {
						// Since pointer-based receiver functions are converted to C# as ref-based
						// extension functions, we need to convert the pointer from a reference type
						return fmt.Sprintf("(uintptr)@unsafe.Pointer.FromRef(ref %s)", v.convExpr(callExpr.Args[0], nil))
					}
				}
			}
		}
	}

	funcTypeName := v.getAliasQualifiedTypeName(funcType, true)
	// ---- Phase 6: conversions whose SOURCE is a string or slice ----
	//
	// []byte(s), []rune(s) and their literal forms. These need golib's @string in the middle
	// because C# will not chain two user-defined conversions on its own.

	// A string-source element-decoding conversion — `[]rune("lit")` or `[]byte("lit")` — must cast
	// the literal to golib's `@string` so the existing `@string`→`slice<rune>`/`slice<byte>`
	// conversion applies. A bare string literal is a System.String, which has no such conversion
	// (CS1503/CS1929). A string *variable* is already `@string`, so this only matters for literals;
	// the cast fires only on STRING basic-literal args (see convBasicLit). The flag name predates
	// the `[]byte` case.
	callExprContext.sourceIsRuneArray = funcTypeName == "[]rune" || funcTypeName == "[]byte"

	// A `[]byte(s)` conversion of a `string | []byte` union-constrained value (time
	// format_rfc3339's parseUint ranges `[]byte(s)`): the usual route binds golib's
	// slice<T>(T[])/slice(@string) builtins, neither of which accepts a value of the
	// constrained type parameter — and ADDING a builtin IByteSeq overload makes @string args
	// ambiguous (CS0121, both conversions applicable). Emit golib's ToSlice EXTENSION: it takes
	// the caller's CONCRETE type as its own type parameter, so the argument passes unboxed, and
	// it keeps Go's per-instantiation semantics (a slice source shares its backing, a string
	// source copies). A constructor cannot do this — C# has no generic constructor, so
	// `new slice<byte>(seq)` can only accept the interface, which boxes; and a static factory
	// cannot be named, because `using static go.builtin` shadows `slice` with a method (CS0119).
	if funcTypeName == "[]byte" && len(callExpr.Args) == 1 {
		if tp, ok := types.Unalias(v.getType(callExpr.Args[0], false)).(*types.TypeParam); ok && typeParamIsStringByteUnion(tp) {
			return fmt.Sprintf("%s.ToSlice()", v.convExpr(callExpr.Args[0], nil))
		}
	}

	// A `[]byte("literal")` over a plain-text string LITERAL feeds the `u8` ROM span straight into the
	// slice — `slice<byte>("literal"u8)` — instead of routing through the heap `@string` the general
	// `[]byte`/`[]rune` path emits (`slice<byte>((@string)"literal")`, via sourceIsRuneArray below). The
	// `u8` literal is zero-allocation static ROM; golib's `slice<T>(ReadOnlySpan<T>)` copies it into the
	// slice's backing array — one fewer allocation and a cleaner rendering. Gated to what `convBasicLit`
	// renders as a `u8` span: a high-`\xHH`-byte literal stays the byte-array-backed `@string` (its bytes
	// do not round-trip through `u8`), and a `[]rune` conversion needs `@string`'s rune decoding, so only
	// `[]byte` qualifies. A string VARIABLE is already an `@string` and keeps the general path.
	if funcTypeName == "[]byte" && len(callExpr.Args) == 1 {
		if basicLit, ok := callExpr.Args[0].(*ast.BasicLit); ok && basicLit.Kind == token.STRING {
			if strings.HasPrefix(basicLit.Value, "`") || !stringLiteralNeedsByteArray(basicLit.Value) {
				u8Context := DefaultBasicLitContext()
				u8Context.u8StringOK = true
				return fmt.Sprintf("slice<byte>(%s)", v.convExpr(basicLit, []ExprContext{u8Context}))
			}
		}
	}

	// A `[]byte`/`[]rune` conversion over Go's line-SPLITTING literal idiom — `[]byte("first " +
	// "second")`, crypto/hmac's long-key vectors — renders as a C# `string` concatenation, and C#
	// refuses the two user-defined conversions string → @string → byte[] that golib's
	// `slice<T>(T[])` needs (CS1503 "cannot convert from 'string' to 'byte[]'"). The literal route
	// above sees no BasicLit to take, and the `(@string)` cast convBasicLit applies under
	// sourceIsRuneArray reaches only a TOP-LEVEL literal argument, never one nested in a binary
	// expression. Cast the rendered operand AS A WHOLE: the source's split survives verbatim (C#
	// constant-folds the concatenation itself, so there is no runtime cost to preserving it), and
	// one explicit step to @string leaves a single implicit step to the parameter.
	//
	// Gated to a `+` chain whose every leaf is a PLAINLY-rendered string literal, which is exactly
	// the shape that produces a bare C# string — see isConstantStringConcat for what that excludes
	// and why each exclusion already carries an @string of its own.
	if funcTypeName == "[]byte" || funcTypeName == "[]rune" {
		if len(callExpr.Args) == 1 && v.isConstantStringConcat(callExpr.Args[0]) {
			elementName := "byte"

			if funcTypeName == "[]rune" {
				elementName = "rune"
			}

			return fmt.Sprintf("slice<%s>((@string)(%s))", elementName, v.convExpr(callExpr.Args[0], nil))
		}
	}

	// A string→byte/rune-slice conversion with an UNNAMED slice target in which a DEFINED type
	// sits on one end or the other — `[]byte(v)` over `type strMarshaler string`, `[]Uint8("hello")`
	// over `type Uint8 byte` (both encoding/json's suite). Neither end is reachable by chaining
	// conversions: a defined string needs `[GoType]`→`@string`→`byte[]` (two user-defined hops,
	// CS1503), and `slice<byte>`→`slice<Uint8>` does not exist at all. The named-slice target takes
	// the same route from its own arm on the conversion path; see stringSliceConversions.go.
	//
	// A plain string converting to a plain `[]byte`/`[]rune` is deliberately NOT claimed — golib's
	// `@string` converts straight to `byte[]`/`rune[]`, so the general path already emits the one
	// call that is the whole conversion, and claiming it would rewrite the corpus to no effect.
	if len(callExpr.Args) == 1 {
		if targetSlice, ok := types.Unalias(funcType).(*types.Slice); ok && isByteOrRuneSlice(targetSlice) {
			arg := callExpr.Args[0]

			if argType := v.getType(arg, false); argType != nil && isStringTyped(argType) {
				_, elemDefined := sliceElemIsDefined(targetSlice)
				_, srcDefined := types.Unalias(argType).(*types.Named)

				if elemDefined || srcDefined {
					return v.stringToByteSliceConversion(targetSlice, arg, v.convExpr(arg, []ExprContext{callExprContext}))
				}
			}
		}
	}

	// An explicit slice conversion `[]E(x)` whose SOURCE is a ~[]E-constrained type
	// parameter S is the EXPLICIT twin of the implicit wrapArgWithNew assignability path
	// above (a value of S passed where a concrete []E is expected). The general call path
	// renders `slice<E>(x)`, binding golib's array-only builtin.slice<T>(T[]) — but S is
	// an ISlice<E>, not E[], so that fails to compile (CS1503). Emit the SHARING
	// slice<T>(ISlice<T>) constructor instead, so an explicit conversion aliases the
	// caller's backing exactly as the implicit form does (Go's `[]E(x)` shares storage).
	// Cleanly disjoint from surrounding conversions: named-slice casts (`[]Named(x)`) take
	// the isTypeConversion cast path and never reach here; string/nil sources are not
	// *types.TypeParam; and the string|[]byte union `[]byte(x)` is handled by the block
	// just above (typeParamSliceCore returns nil when a constraint term is not a slice).
	if strings.HasPrefix(funcTypeName, "[]") && len(callExpr.Args) == 1 {
		if tp, ok := types.Unalias(v.getType(callExpr.Args[0], false)).(*types.TypeParam); ok && typeParamSliceCore(tp) != nil {
			if targetSlice, ok := funcType.(*types.Slice); ok {
				elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(targetSlice.Elem(), false))
				return fmt.Sprintf("new slice<%s>(%s)", elemName, v.convExpr(callExpr.Args[0], nil))
			}
		}
	}

	if len(callExpr.Args) == 1 {
		// `(*U)(unsafe.Pointer(p))` mis-classifies as a non-conversion and lands here rather than on
		// the conversion path, so the managed reinterpret is intercepted at BOTH address routes —
		// this one and the conversion path's. Must precede the isPointerCast assignment below, which
		// is what renders the `(ж<U>)(uintptr)` prefix this replaces.
		if emission, ok := v.reinterpretManagedEmission(callExpr, callExpr.Args[0]); ok {
			return emission
		}

		// Same placement rule for the array-target sibling — it replaces the `(ж<array<T>>)(uintptr)`
		// prefix the isPointerCast assignment below renders.
		if emission, ok := v.arrayPointerAliasEmission(callExpr, callExpr.Args[0]); ok {
			return emission
		}

		argTypeName := v.getExpressionTypeName(callExpr.Args[0], true)

		if argTypeName == "unsafe.Pointer" {
			lambdaContext.isPointerCast = true
		}
	}

	funcName := ""

	// A call through a dereferenced function pointer, `(*fp)(args)`. Converting the ParenExpr
	// faithfully yields `(fp.Value)(args)`, which C# parses as a CAST when the argument list is empty
	// (`(fp.Value)()` reads as "cast `()` to type `fp.Value`" → CS1525). Emit the deref WITHOUT the
	// wrapping parens (`fp.Value(args)`) so it is unambiguously an invocation. Restricted to a starred
	// VALUE operand; a starred type (`(*int)(x)`) is a conversion handled earlier.
	if paren, ok := callExpr.Fun.(*ast.ParenExpr); ok {
		if star, ok := paren.X.(*ast.StarExpr); ok {
			if tv, ok := v.info.Types[star.X]; ok && tv.IsValue() {
				funcName = v.convExpr(star, []ExprContext{lambdaContext})
			}
		}
	}

	if funcName == "" {
		// The CALLEE's ident context suppresses the generic-function-VALUE type-argument append —
		// phase 8 below owns a call's type arguments, and emits them only where C# cannot infer.
		// The selector form is gated the same way through LambdaContext.isCallExpr
		// (convSelectorExpr).
		calleeIdentContext := DefaultIdentContext()
		calleeIdentContext.suppressGenericTypeArgs = true

		// A parenthesized TYPE-NAME callee renders UNPARENTHESIZED, so that
		// `(unsafe.Pointer)(x)` and `unsafe.Pointer(x)` — the same Go expression, twice spelled
		// — reach the same emission. Rendering the ParenExpr gave `funcName` a parenthesized
		// form that the `new @unsafe.Pointer(…)` peephole below cannot match (it compares
		// `funcName` against the bare name), leaving the raw-cast route and CS0030.
		//
		// Narrowed to an Ident/SelectorExpr payload ON PURPOSE — exactly the two shapes
		// isConstructorCall accepts, so the two halves of this fix stay in step. A parenthesized
		// STAR type `(*int)(p)` must keep its parens: convParenExpr owns the Pointer → ж<T>
		// uintptr hop for that shape, and unwrapping it loses the hop (measured — it turned two
		// working round-trip reads into CS1503 "cannot convert from Pointer to in ж<nint>").
		callee := callExpr.Fun

		if paren, isParen := callee.(*ast.ParenExpr); isParen {
			if tv, isTyped := v.info.Types[callee]; isTyped && tv.IsType() {
				switch ast.Unparen(paren.X).(type) {
				case *ast.Ident, *ast.SelectorExpr:
					callee = ast.Unparen(paren.X)
				}
			}
		}

		funcName = v.convExpr(callee, []ExprContext{lambdaContext, calleeIdentContext})
	}

	// A VARIADIC func-literal callee renders as `(params ꓸꓸꓸ@string dirsʗp) => …`, which C# can
	// neither invoke directly (CS0149) nor convert to any `Action`/`Func` — cast it to its golib
	// family delegate, the same `((<delegate>)(<lambda>))(<args>)` shape phase 1a already uses for
	// a non-variadic IIFE, so the invocation binds. Skipped for the BARE-callee `defer`/`go` form
	// below, which emits the literal with no argument list at all; `visitDeferStmt`/`visitGoStmt`
	// force the temp-parameter form for exactly this shape, so a variadic literal never lands
	// there. Phase 1a's own `!sig.Variadic()` restriction is what routes an immediately-invoked
	// variadic literal here, and this is the cast it was waiting for.
	if context.renderParams || context.callArgs == nil {
		if sig := v.variadicFuncLitCallee(callExpr); sig != nil {
			funcName = fmt.Sprintf("((%s)(%s))", v.iifeDelegateType(sig), funcName)
		}
	}

	// The same cast, for the one NON-variadic literal callee the defer/go path also renders as an
	// INVOCATION: a MULTI-VALUE spread (`defer func(n int, s string) { … }(g())`) takes the
	// temp-parameter form so the thunk can spread the tuple's components, and a bare lambda literal
	// cannot be invoked (CS0149). Phase 1a's IIFE cast declines every defer/go callee because such a
	// literal is normally handed to the rung AS a delegate with no argument list at all — true for
	// every shape but this one.
	if context.renderParams && context.callArgs != nil && v.multiValueSpreadArity(callExpr) > 1 {
		if funcLit, ok := ast.Unparen(callExpr.Fun).(*ast.FuncLit); ok {
			if sig, ok := v.info.TypeOf(funcLit).(*types.Signature); ok && !sig.Variadic() {
				funcName = fmt.Sprintf("((%s)(%s))", v.iifeDelegateType(sig), funcName)
			}
		}
	}

	// ---- Phase 7a: built-in name shadowing, and min/max argument typing ----

	// A Go built-in call (`clear(s)`, `len(s)`, …) whose name the package ALSO declares as a method
	// shadows the using-static `go.builtin.<name>` (C# member lookup binds the package's own
	// `<name>(this ref T)` extension first → CS1620/CS1503). Qualify it as `builtin.<name>` so it
	// resolves to the golib built-in regardless of the same-class shadow.
	if len(packageBuiltinShadows) > 0 {
		if ident, ok := callExpr.Fun.(*ast.Ident); ok && funcName == ident.Name && packageBuiltinShadows[ident.Name] {
			if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); isBuiltin {
				funcName = "builtin." + funcName
			}
		}
	}

	// Go's min/max builtins type every argument to the call's single result type. An argument that
	// is a NAMED UNTYPED CONSTANT renders as its UntypedInt (BigInteger) static, which the golib
	// min/max `params ReadOnlySpan<T>` overloads reject (CS1503 — params-span element binding does
	// not apply the user-defined implicit conversion): runtime `min(n, maxObletBytes)` (mgcmark.go,
	// n uintptr) and `min(debug.profstackdepth, maxProfStackDepth)` (runtime1.go, int32). Cast such
	// an argument to the call's Go-resolved result type: `min(n, (uintptr)(maxObletBytes))`.
	// Literal and typed arguments are left as-is (no churn — the early return fires only when an
	// untyped-const argument is present).
	if funcName == "min" || funcName == "max" || funcName == "builtin.min" || funcName == "builtin.max" {
		if funIdent, ok := callExpr.Fun.(*ast.Ident); ok {
			if _, isBuiltin := v.info.ObjectOf(funIdent).(*types.Builtin); isBuiltin {
				if callType := v.info.TypeOf(callExpr); callType != nil {
					argIsNamedUntypedConst := func(arg ast.Expr) bool {
						ident := getIdentifier(arg)

						if ident == nil {
							return false
						}

						constObj, ok := v.info.ObjectOf(ident).(*types.Const)

						if !ok {
							return false
						}

						basic, ok := constObj.Type().(*types.Basic)

						return ok && basic.Info()&types.IsUntyped != 0
					}

					needsCast := false

					for _, arg := range callExpr.Args {
						if argIsNamedUntypedConst(arg) {
							needsCast = true
							break
						}
					}

					if needsCast {
						// Once one argument is cast, an untyped LITERAL sibling (`min(big, limit,
						// 500)` — a bare `500` is a C# int) breaks T inference against the cast
						// type, so every constant-valued argument gets the cast; typed variable
						// arguments are left as-is.
						csCallType := convertToCSTypeName(v.getAliasQualifiedTypeName(callType, false))
						args := make([]string, len(callExpr.Args))

						for i, arg := range callExpr.Args {
							args[i] = v.convExpr(arg, nil)

							if _, isLit := arg.(*ast.BasicLit); isLit || argIsNamedUntypedConst(arg) {
								args[i] = fmt.Sprintf("(%s)(%s)", csCallType, args[i])
							}
						}

						return fmt.Sprintf("%s(%s)", funcName, strings.Join(args, ", "))
					}
				}
			}
		}
	}

	// ---- Phase 7b: unsafe.Sizeof / Alignof / Offsetof constant folding ----

	// Go defines `unsafe.Sizeof` / `unsafe.Alignof` / `unsafe.Offsetof` as COMPILE-TIME CONSTANTS
	// computed from the operand's STATIC type — the operand is never evaluated — so the converter
	// FOLDS them to the value go/types already holds, keeping the Go expression as a comment. That
	// is the emission declaration sites have always used (`/* unsafe.Offsetof(cpu.X86.HasAVX) */
	// 66`); it now covers expression sites too, so one Go construct has one behavior.
	//
	// The golib run-time forms below are what expression sites used to emit. They answer with the
	// CLR's MARSHALLED layout, which is a different number whenever golib's field representation
	// differs from Go's (`string` is 16 bytes in Go/amd64; `@string` is a managed struct) — and
	// golib's `Sizeof` rides `Marshal.SizeOf<T>`, which THROWS outright for a non-blittable `T`,
	// i.e. for any converted struct holding a slice, string, interface or `ж<T>` field. `debug/elf`
	// reads its on-disk ELF header offsets through `Offsetof`: those must be Go's numbers.
	//
	// A non-constant result exists only for a VARIABLE-SIZE operand (a type parameter). If go/types
	// has no constant, the run-time form is still emitted and the site is reported rather than lost.
	if len(constructType) == 0 && len(callExpr.Args) == 1 {
		if builtinName := v.unsafeConstBuiltinName(callExpr); len(builtinName) > 0 {
			if folded, ok := v.foldUnsafeConstBuiltin(callExpr); ok {
				return folded
			}

			v.showWarning("Go 'unsafe.%s' did not resolve to a constant - emitting run-time form: %s", builtinName, v.getPrintedNode(callExpr))
		}
	}

	// unsafe.Offsetof / unsafe.Alignof reshape for the golib helpers, which take a System.Type
	// rather than a value. Both are defined by Go against the STATIC type of the operand — an
	// operand Go never evaluates — so the shape is derived from go/types.
	//
	// It was previously derived by splitting the CONVERTED C# text on '.' and reading the pieces
	// as a Go field selector, which corrupts every rendering that is not literally `ident` or
	// `ident.field`: `unsafe.Alignof(uint32(0))` became `(uint32)0.GetType()`, which C# parses as
	// `(uint32)(0.GetType())` — CS0030, crypto/md5's benchmarkSize; a `ж` deref `Ꮡx.Value` read as
	// struct `Ꮡx` with field `Value`; and a two-level `cpu.X86.HasAVX` was rejected outright.
	// `.GetType()` was also wrong on its own terms — it reports the DYNAMIC type of a boxed or
	// interface-typed operand where Go uses the static one, and it evaluates the operand.
	if len(constructType) == 0 && len(callExpr.Args) == 1 &&
		(funcName == "@unsafe.Offsetof" || funcName == "@unsafe.Alignof") {
		arg := callExpr.Args[0]

		if funcName == "@unsafe.Offsetof" {
			v.showWarning("Go code converted to C# using 'unsafe.Offsetof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			if structType, fieldName, ok := v.unsafeFieldOperand(arg); ok {
				// `unsafe.Offsetof(structValue.field)` to
				// `@unsafe.Offsetof(typeof(StructType), "field")`
				return fmt.Sprintf("%s(typeof(%s), \"%s\")", funcName, v.getCSharpTypeName(structType), fieldName)
			}

			v.showWarning("Unexpected 'unsafe.Offsetof' argument format: %s", v.getPrintedNode(arg))
		} else {
			v.showWarning("Go code converted to C# using 'unsafe.Alignof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))

			// `unsafe.Alignof(x)` to `@unsafe.Alignof(typeof(T))`, for EVERY operand shape: Go's
			// `Alignof(s.f)` is the required alignment of the FIELD's own type, which is what
			// golib's `(type, fieldName)` overload resolves to anyway, so one rule covers both.
			if operandType := v.info.TypeOf(arg); operandType != nil {
				return fmt.Sprintf("%s(typeof(%s))", funcName, v.getCSharpTypeName(operandType))
			}

			v.showWarning("Unexpected 'unsafe.Alignof' argument format: %s", v.getPrintedNode(arg))
		}
	}

	if len(constructType) == 0 && len(callExpr.Args) == 1 && funcName == "@unsafe.Sizeof" {
		v.showWarning("Go code converted to C# using 'unsafe.Sizeof' may not produce same value as Go - verify usage: %s", v.getPrintedNode(callExpr))
	}

	// ---- Phase 7c: sync/atomic on managed pointers ----

	// sync/atomic.LoadPointer/StorePointer on a MANAGED pointer field — the lock-free
	// `atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))` / `StorePointer(…,
	// unsafe.Pointer(v))` idiom where `x.field` is a `*T` (a `ж<T>` reference). The literal
	// conversion round-trips the managed reference through a transient `uintptr` address and NREs
	// (x/sys/windows's LazyDLL/LazyProc caches). Emit the golib managed-referent overloads on the
	// field box (`ж<ж<T>>`) instead, so the atomic read/write operates on the reference directly.
	// A LOAD result stays `unsafe.Pointer`-typed to Go, so the caller's nil-compare still wraps it
	// `(uintptr)… == nil`; the `ж<T> → uintptr` operator yields 0 for a nil box (see golib), so the
	// comparison is correct without changing the surrounding emission.
	if addrExpr, storeVal, isLoad, ok := v.managedAtomicPointerIdiom(callExpr); ok {
		box := v.convExpr(addrExpr, nil)

		if isLoad {
			return fmt.Sprintf("%s(%s)", funcName, box)
		}

		return fmt.Sprintf("%s(%s, %s)", funcName, box, v.convExpr(storeVal, nil))
	}

	// ---- Phase 8: generic instantiation type arguments ----

	if len(typeParamExpr) > 0 && !strings.HasSuffix(funcName, typeParamExpr) {
		// A PARTIAL Go instantiation (`Grow[S](nil, size)` — only S written, E inferred through
		// core types) already rendered its explicit arguments into funcName; the RESOLVED full
		// list replaces them (C# needs every constraint-only parameter spelled out — appending
		// would emit `Grow<S><S, E>`).
		funcName = stripTrailingTypeArgs(funcName) + typeParamExpr
	}

	// ---- Phase 9: render the call ----
	//
	// Everything above decided HOW each piece renders; this assembles the final text and then
	// re-walks the arguments purely to record implicit conversions as a side effect.

	// The slice-shaped spread rides on the NAME — `appendꓸꓸꓸ(s, t)` for Go's `append(s, t...)`,
	// the same glyph the operand spread uses — binding golib's ISlice-taking forms with no
	// overload interplay against append's params families (a shared name re-entered the
	// C#14 params/betterness thicket: measured CS0121). The constrained form additionally
	// carries its explicit type arguments (see CallExprContext.appendTypeArgs).
	if callExprContext.spreadArgAsSlice {
		funcName = strings.TrimSuffix(funcName, "append") + "appendꓸꓸꓸ" + callExprContext.appendTypeArgs
	}

	var result string

	if !context.renderParams && context.callArgs != nil {
		// Capture arguments for function literal in a defer context, but do not render
		v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)
		result = fmt.Sprintf("%s%s", constructType, funcName)
	} else {
		expr := v.convExprList(callExpr.Args, callExpr.Lparen, callExprContext)

		if strings.HasSuffix(funcName, "(uintptr)") && strings.HasPrefix(expr, "(uintptr)") {
			// Remove redundant cast to uintptr
			expr = expr[9:]
		}

		// The `unsafe.Pointer(x)` constructor form of the dead-wrapper peephole (see
		// markDeadUnsafePointerBox): the enclosing `uintptr(…)` reads the address straight back
		// out, so the wrapper object is never built and the operand stands alone.
		if constructType == "new " && funcName == "@unsafe.Pointer" && len(callExpr.Args) == 1 {
			result = v.unsafePointerBoxEmission(callExpr, callExpr.Args[0], expr)
		} else {
			result = fmt.Sprintf("%s%s(%s)", constructType, funcName, expr)
		}
	}

	// Record each argument's implicit conversions. Nothing here is emitted — the call text was
	// assembled above — so this walks the RECORDING half only (applyImplicitConversion), never
	// convExpr.
	//
	// It used to call checkForImplicitConversion, i.e. a full second conversion of every argument
	// subtree whose result was discarded, which made every call cost 2× and every NESTED call
	// `f(f(f(…)))` cost 2^depth: depth 22 took 10.6s against a 0.6s floor on this machine. Same
	// class as the issue-#33 callee-path exponential (a chained call re-converting its callee once
	// per parameter), one code path over — see the board's ARGUMENT-path entry. The recording is
	// entirely type-driven, so dropping the traversal is free: it also removes the need to suppress
	// capture-decl hoisting around the loop (the second conversion of a func-literal argument was
	// what wrote its decls into the hoist buffer a second time).
	for _, arg := range callExpr.Args {
		argType := v.getType(arg, false)
		argTypeName := convertToCSTypeName(v.getAliasQualifiedTypeName(argType, false))

		v.applyImplicitConversion(funcType, arg, argTypeName, "")
	}

	return result
}

// identDeclaredFromMethodGroup reports whether ident resolves to a local whose `:=` declaration
// initialized it from a bare function/method reference (a C# method group). Such a declaration is
// emitted with the package named delegate matching its signature when one exists (visitAssignStmt's
// methodGroupDelegateType via namedFuncTypeNameForSignature), so at a call site the local's C# type
// is that NAMED delegate even though go/types reports the unnamed structural signature — the shape
// that needs the delegate re-wrap at a structural func parameter. Scans only the current function's
// body (the `:=` declaration and its uses share the function scope).
func (v *Visitor) identDeclaredFromMethodGroup(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return false
	}

	found := false

	ast.Inspect(v.currentFuncDecl.Body, func(node ast.Node) bool {
		if found {
			return false
		}

		assign, ok := node.(*ast.AssignStmt)

		if !ok || assign.Tok != token.DEFINE || len(assign.Lhs) != len(assign.Rhs) {
			return true
		}

		for i, lhs := range assign.Lhs {
			lhsIdent, ok := lhs.(*ast.Ident)

			if !ok || v.info.Defs[lhsIdent] != obj {
				continue
			}

			found = v.exprIsMethodGroup(assign.Rhs[i])

			return false
		}

		return true
	})

	return found
}

// checkForImplicitConversion converts arg and applies any implicit conversion the call site needs,
// returning the emitted expression. This is the RENDERING caller's entry point (the explicit
// type-conversion branch of convCallExpr, which uses the returned text); a caller that only wants
// the recording side effect calls applyImplicitConversion directly and skips the conversion — see
// the argument loop at the end of convCallExpr.
//
// contexts carries the enclosing statement's capture-copy hoist sink so a CAPTURING func-literal
// operand snapshots to a statement slot instead of inline (see the call site); every other
// conversion passes nil, which is what this always did.
func (v *Visitor) checkForImplicitConversion(funcType types.Type, arg ast.Expr, targetTypeName string, contexts []ExprContext) string {
	return v.applyImplicitConversion(funcType, arg, targetTypeName, v.convExpr(arg, contexts))
}

// applyImplicitConversion records the implicit conversion, if any, between arg's type and the
// call's target type, returning expr wrapped as the target requires.
//
// Every DECISION here is type-driven — funcType, argType, targetTypeName and packageTypeSpecRHS —
// and expr is pure text that flows only to the return value (the two pointer cases wrap it). That
// is what lets the recording run without a conversion at all: a caller that discards the result
// passes "" and pays no traversal. Keep it that way; reading anything out of expr would put a whole
// second walk of the argument subtree back on every call (see the loop's note in convCallExpr).
func (v *Visitor) applyImplicitConversion(funcType types.Type, arg ast.Expr, targetTypeName string, expr string) string {
	argType := v.getType(arg, false)

	// The callee of a call whose operand went INVALID has no recorded type at all (go/types drops
	// invalid operands rather than recording them), so getType returned nil for it — a package that
	// did not fully type-check reaches here with one (issue #33). There is no target type to convert
	// TO, so emit the argument as written; the recorded implicit conversions are an optimization of
	// the emitted form, never a correctness requirement. Without this the nil deref below faulted the
	// whole FILE out of the conversion (the per-file recover in processConversion catches it), losing
	// every other declaration in it over one undefined symbol.
	if funcType == nil {
		return expr
	}

	var targetTypeIsPointer bool

	// Check if function type is a signature, i.e., an anonymous struct
	if sigType, ok := funcType.(*types.Signature); ok && sigType.Params().Len() > 0 {
		funcType = sigType.Params().At(0).Type()
	}

	// Check if function type is a struct or a pointer to a struct
	if ptrType, ok := funcType.(*types.Pointer); ok {
		funcType = ptrType.Elem()
		targetTypeIsPointer = true
	}

	if _, ok := funcType.Underlying().(*types.Struct); ok {
		// Check if argType is a struct or a pointer to a struct
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			if _, ok := argType.Underlying().(*types.Struct); ok {
				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.Value ?? default!)", expr)
				}

				argTypeName := v.getCSharpTypeName(argType)

				// SKIP a conversion the [GoType] wrapper already provides: `type bitStringEncoder
				// BitString` makes the TypeGenerator emit BOTH bitStringEncoder<->BitString
				// implicit operators, so a recorded GoImplicitConv<BitString, bitStringEncoder>
				// (or its reverse) is a DUPLICATE user-defined conversion (CS0557, encoding/asn1).
				// packageTypeSpecRHS marks a defined type's written RHS (the wrapper relationship).
				wrapperConversion := false

				if named, ok := funcType.(*types.Named); ok {
					if rhs, has := packageTypeSpecRHS[named.Obj()]; has && rhs != nil && types.Identical(rhs, argType) {
						wrapperConversion = true
					}
				}

				if named, ok := argType.(*types.Named); ok {
					if rhs, has := packageTypeSpecRHS[named.Obj()]; has && rhs != nil && types.Identical(rhs, funcType) {
						wrapperConversion = true
					}
				}

				// A pairing carrying UNBOUND type parameters cannot be recorded: an assembly
				// attribute cannot name K/V (internal/concurrent's indirect[K,V] GoImplicitConv
				// record emitted GoImplicitConv<Δindirect<K, V>, ж<Δindirect<K, V>>>, CS0246 x4
				// - and one bad record kills the whole generator run).
				// An operand that is an ALIAS TO A PRIMITIVE cannot take part either, for the
				// same "one bad record kills the generator run" reason. `type _C_int = int`
				// emits as `global using _C_int = int` — a name for a BCL primitive, with no
				// wrapper struct and no `.Value`. As the record's HOST it mints a phantom
				// (`partial struct UInt32`, CS1729 on a constructor the primitive lacks); as the
				// record's SOURCE it renders as a type name the operator body dereferences
				// (`new addrinfoErrno((nint)src.Value)` over `@int`, CS0246). Both shapes came
				// from the second darwin census's cgo-flavor leaves, whose C-type mirrors are
				// declared entirely as such aliases. Nothing is lost by declining: a conversion
				// involving a primitive is what C#'s own numeric conversions already express.
				if targetTypeName != argTypeName && !wrapperConversion && !typeContainsTypeParams(argType) && !typeContainsTypeParams(funcType) &&
					!typeIsPrimitiveAlias(argType) && !typeIsPrimitiveAlias(funcType) &&
					v.conversionRecordHasLocalOperand(funcType, argType, pointerBoxConversionRecord(argTypeName, targetTypeName)) {
					// The recorded conversion type names use cross-package import aliases (e.g.
					// `abi.Type`); register them so package_info.cs can emit a resolving `global using`.
					v.recordConversionPackageUsing(argType)
					v.recordConversionPackageUsing(funcType)

					// If both funcType and argType are distinct structs, track implicit conversions
					packageLock.Lock()

					var targetConversionsMap map[string]HashSet[string]

					if targetTypeIsPointer {
						targetConversionsMap = indirectImplicitConversions
					} else {
						targetConversionsMap = implicitConversions
					}

					var conversions HashSet[string]
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions.Add(targetTypeName)
					} else {
						conversions = NewHashSet([]string{targetTypeName})
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()

					v.addImplicitSubStructConversions(argType, targetTypeName, targetTypeIsPointer)
				}
			}
		}
	}

	// Check if the function type is an aliased numeric type
	if ok := isAliasedNumericType(funcType); ok {
		// Check if argType is a pointer type
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem()
		}

		if !types.Identical(funcType, argType) {
			// A conversion between a defined type and its WRITTEN base named type — `Key(handle)`
			// or `Handle(key)` where `type Key syscall.Handle` — is already provided by the
			// [GoType] wrapper itself (TypeGenerator emits the implicit operators both ways);
			// recording it again makes ImplicitConvGenerator emit the identical operator into the
			// same type (CS0557 — internal/syscall/windows/registry's Key, gating os/fmt).
			if named, ok := funcType.(*types.Named); ok {
				if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil && types.Identical(rhs, argType) {
					return expr
				}
			}

			if named, ok := argType.(*types.Named); ok {
				if rhs, okRHS := packageTypeSpecRHS[named.Obj()]; okRHS && rhs != nil && types.Identical(rhs, funcType) {
					return expr
				}
			}

			// Check if the arg type is an aliased numeric type
			if ok := isAliasedNumericType(argType); ok {
				// argType is the type the generated operator CONSTRUCTS in both arms below —
				// directly when the record is `Inverted` (LH is the record's source), and after
				// the local-anchor swap when it is not (LH is the record's target) — so its
				// BACKING PRIMITIVE is what ValueType must name. See numericConversionValueTypeName.
				valueTypeName := numericConversionValueTypeName(argType)

				if targetTypeIsPointer {
					// Dereference target type when casting to pointer types,
					// in C# implicit casting operator requires the target type
					// to be a direct type, not a pointer type
					expr = fmt.Sprintf("(%s?.Value ?? default!)", expr)
				}

				argTypeName := v.getCSharpTypeName(argType)

				// A primitive-alias operand is excluded here for the same reason as in the struct
				// branch below, and this is the arm that reaches it as the record's SOURCE: one
				// LOCAL operand satisfies the locality test on its own, so `addrinfoErrno` (a real
				// wrapper) admitted a record whose other side is `_C_int = int`, and the generated
				// operator dereferenced `.Value` on a BCL primitive (`new addrinfoErrno((nint)
				// src.Value)` over `@int`, CS0246 — net's darwin leaf, second darwin census).
				if targetTypeName != argTypeName && !typeIsPrimitiveAlias(argType) && !typeIsPrimitiveAlias(funcType) &&
					v.conversionRecordHasLocalOperand(funcType, argType, false) {
					// The recorded conversion type names use cross-package import aliases (e.g.
					// `driver.IsolationLevel`); register them so package_info.cs emits a resolving
					// `global using` — the STRUCT-conversion branch above already does this, but the
					// aliased-NUMERIC branch omitted it, so a cross-package named-numeric conversion
					// (database/sql's `driver.IsolationLevel(opts.Isolation)`) left `driver` unresolved
					// in both package_info.cs and the ImplicitConvGenerator .g.cs (CS0246).
					v.recordConversionPackageUsing(argType)
					v.recordConversionPackageUsing(funcType)

					if strings.Contains(argTypeName, ".") || strings.Contains(argTypeName, TypeAliasDot) {
						valueTypeName = fmt.Sprintf("imported:%s", valueTypeName)
						targetTypeName, argTypeName = argTypeName, targetTypeName
					}

					// If both funcType and argType are both aliased numeric types, track value conversions
					packageLock.Lock()

					var targetConversionsMap map[string]map[string]string

					if targetTypeIsPointer {
						targetConversionsMap = indirectNumericConversions
					} else {
						targetConversionsMap = numericConversions
					}

					var conversions map[string]string
					var exists bool

					if conversions, exists = targetConversionsMap[argTypeName]; exists {
						conversions[targetTypeName] = valueTypeName
					} else {
						conversions = make(map[string]string)
						conversions[targetTypeName] = valueTypeName
						targetConversionsMap[argTypeName] = conversions
					}

					packageLock.Unlock()
				}
			}
		}
	}

	return expr
}

// conversionRecordHasLocalOperand reports whether a GoImplicitConv record over this operand pair
// can be REALIZED by ImplicitConvGenerator, which hosts the operator in a `partial struct` inside
// THIS package's class. That needs at least one operand the package actually declares: the
// generator already relocates the host when exactly one side is foreign (its "foreign SOURCE via a
// local alias" / "foreign TARGET via a qualified reference" arms), but with NEITHER side local it
// has nothing to extend and falls back to declaring `partial struct <simple name>` locally — a
// PHANTOM type of that name, whose `.Value` does not exist (CS1061).
//
// os's `syscall.Handle(t)` over a `syscall.Token` (os_windows_test.go's privilege helper) is the
// reached case: both operands live in `syscall`, and the aliased-numeric arm's local-anchor swap
// just picks the other foreign one. Declining costs nothing — the call site emits the explicit
// `(syscallꓸHandle)(uintptr)t` cast chain, which needs no generated operator, and an operator
// between two foreign types could not be hosted in either of their assemblies from here anyway.
//
// The POINTER-BOXING route — `T` → `ж<T>` — is the one shape that needs no local operand, and it
// takes the second arm. `ж<T>` is golib's generic box, which no converted package declares, so the
// generator finds no struct declaration for the target and skips the record before it ever chooses a
// host: it can neither mint a phantom nor extend the closed production assembly. That is the same
// fact recordsRequireProductionMutation already states for the same shape, and
// pointerBoxConversionRecord is now the one definition both read.
//
// Only a WHITEBOX-PRODUCTION operand is readmitted by that arm — a type this conversion genuinely
// declares in Go, whose C# merely lives in the referenced production assembly. A both-FOREIGN pair
// stays declined exactly as before: an operator between two foreign types has no home here whether
// or not one would have been hosted.
func (v *Visitor) conversionRecordHasLocalOperand(funcType, argType types.Type, pointerBoxRecord bool) bool {
	if v.typeDeclaredInConvertedPackage(funcType) || v.typeDeclaredInConvertedPackage(argType) {
		return true
	}

	// The pointer-BOXING route hosts nothing, so the phantom this predicate exists to prevent
	// cannot arise and a whitebox-production operand still counts. See pointerBoxConversionRecord.
	return pointerBoxRecord && (v.whiteboxProductionDeclaration(funcType) || v.whiteboxProductionDeclaration(argType))
}

// pointerBoxConversionRecord reports whether a recorded conversion pair is the shared Go
// pointer-boxing route — a type to its own golib box, `T` → `ж<T>`. It is the ONE definition of that
// shape:
// conversionRecordHasLocalOperand reads it to know the record hosts no operator, and
// recordsRequireProductionMutation reads it to know the record cannot mutate production.
//
// The `global::` root prefix is normalized off BOTH sides, preserving the normalization
// recordsRequireProductionMutation already applied: the two names are composed at different points
// (the source from the argument type, the box from the caller's own rendering of it), so the root
// escape is a property of the spelling rather than of the pair.
func pointerBoxConversionRecord(sourceTypeName, targetTypeName string) bool {
	inner, boxed := strings.CutPrefix(targetTypeName, PointerPrefix+"<")

	if !boxed {
		return false
	}

	trimRoot := func(name string) string { return strings.TrimPrefix(name, "global::") }

	return trimRoot(strings.TrimSuffix(inner, ">")) == trimRoot(sourceTypeName)
}

// pointerBoxRecordEitherOrientation reports the pointer-boxing route without regard to WHICH side of
// a recorded pair holds the box. The record maps disagree about that: a conversion-site record is
// stored argument-first, so the box lands on the target, while invertedImplicitConversions is keyed
// by the INTERFACE parameter type and emits its attribute with the two arguments swapped — so the
// same Go shape reaches the metadata with the box on either side depending on which site recorded it.
//
// Orientation does not change the fact the exemption rests on, because ImplicitConvGenerator refuses
// the record from both directions. Whichever generic argument is the box, one of its two guards
// fires: `ж<T>` is golib's generic CLASS, so it fails the `TypeKind.Struct` test when it lands in the
// source position, and it has no local struct declaration to enumerate members from when it lands in
// the target position. Either way the generator skips the pair before it chooses a host, so it can
// neither mint a phantom nor extend a closed production assembly.
func pointerBoxRecordEitherOrientation(sourceTypeName, targetTypeName string) bool {
	return pointerBoxConversionRecord(sourceTypeName, targetTypeName) ||
		pointerBoxConversionRecord(targetTypeName, sourceTypeName)
}

// whiteboxProductionDeclaration reports whether a named/aliased type is one the converted package
// declares in GO but not in C# — a production declaration merged into the internal `-tests` variant
// by go/packages, whose emitted type lives in the referenced production assembly. It is exactly the
// case typeDeclaredInConvertedPackage subtracts, named separately so a caller that has established
// no operator will be hosted can add it back rather than re-deriving the condition.
func (v *Visitor) whiteboxProductionDeclaration(t types.Type) bool {
	if t == nil || v.pkg == nil {
		return false
	}

	var obj *types.TypeName

	switch declared := t.(type) {
	case *types.Named:
		obj = declared.Obj()
	case *types.Alias:
		obj = declared.Obj()
	default:
		return false
	}

	return obj != nil && obj.Pkg() == v.pkg && v.whiteboxProductionObject(obj)
}

// typeDeclaredInConvertedPackage reports whether a named/aliased type is declared by the package
// currently being converted. An unnamed type (basic, literal struct) has no declaring package and
// answers false.
//
// A WHITEBOX-PRODUCTION declaration answers false too: on the internal `-tests` variant,
// go/packages merges the production files into the test package, so a production type's obj.Pkg()
// IS v.pkg — local in the Go sense — while its C# lives in the CLOSED referenced production
// assembly, which the generator cannot extend with an operator. Counting it local re-created the
// exact phantom the caller exists to prevent: internal/reflectlite's export_test.go converts
// `flag(typ.Kind())`, the pair recorded with the production `flag` as its host, and the generator
// declared `partial struct flag` in the TEST class — a phantom whose `.Value` does not exist
// (CS1061). Declining costs nothing, exactly as for the both-foreign pair: the cast site already
// emits the explicit chain (`(flag)(uintptr)(uint8)typ.Kind()`), which needs no generated
// operator, and production's own operators (its package_info.cs carries `GoImplicitConv<flag,
// abiꓸKind>`) serve any implicit position. whiteboxProductionObject is option-gated, so every
// non-`-tests` conversion is untouched by construction.
func (v *Visitor) typeDeclaredInConvertedPackage(t types.Type) bool {
	if t == nil || v.pkg == nil {
		return false
	}

	var obj *types.TypeName

	switch declared := t.(type) {
	case *types.Named:
		obj = declared.Obj()
	case *types.Alias:
		// An alias to a PRIMITIVE declares no C# type to host an operator on — see
		// typeIsPrimitiveAlias, which both this predicate and the record-emission gate read.
		if typeIsPrimitiveAlias(declared) {
			return false
		}

		obj = declared.Obj()
	default:
		return false
	}

	return obj != nil && obj.Pkg() == v.pkg && !v.whiteboxProductionObject(obj)
}

// numericConversionValueTypeName renders the C# name of the PRIMITIVE that backs a named (or
// aliased) numeric type — the `uint32` in `type WaitStatus uint32` — which is what a
// `[assembly: GoImplicitConv<…>]` record's `ValueType` must carry.
//
// ValueType is not a type ARGUMENT; ImplicitConvGenerator applies it as a CAST to the source
// operand and feeds the result to the constructed type's constructor:
//
//	public static implicit operator WaitStatus(ΔSignal src) => new WaitStatus((ValueType)src.Value);
//
// (ImplicitConvTemplate.ParamList). That constructor takes the backing primitive, so ValueType must
// name the primitive. Naming the constructed NAMED type there instead — which is what the record
// carried until this was rooted — round-trips through the type's own conversion operators,
// `new WaitStatus((WaitStatus)src.Value)`. That compiles only while a standard EXPLICIT conversion
// exists from the SOURCE's primitive to the target's, because a user-defined conversion admits just
// one standard conversion on its input: syscall's `ΔSignal` is backed by `nint` and `WaitStatus` by
// `uint32`, and `uint32`→`nint` is not a standard IMPLICIT conversion (a 32-bit unsigned value does
// not fit a 32-bit native int), so its reverse is not a standard explicit one, the operator is not
// applicable, and the cast is CS0030. Casting to the primitive is a plain numeric conversion and is
// always available. Windows never saw it: `WaitStatus` is a struct there, so the pair is never
// registered at all.
//
// Callers must have established isAliasedNumericType, which is what makes the underlying a
// *types.Basic; the name falls back to the type's own rendering if that ever stops holding, since a
// wrong-but-shaped ValueType is a compile error at the call site rather than silent bad codegen.
func numericConversionValueTypeName(t types.Type) string {
	if basic, ok := t.Underlying().(*types.Basic); ok {
		return convertToCSTypeName(basic.Name())
	}

	return convertToCSTypeName(types.TypeString(t, nil))
}

func isAliasedNumericType(targetType types.Type) bool {
	if aliasedType, ok := targetType.(*types.Alias); ok {
		underlyingType := aliasedType.Underlying()
		return isNumericType(underlyingType)
	} else if namedType, ok := targetType.(*types.Named); ok {
		underlyingType := namedType.Underlying()
		return isNumericType(underlyingType)
	}

	return false
}

// isUntypedNumericConstArg reports whether the argument is an untyped numeric constant — a
// numeric literal, or a named const declared without a type. In C# these render either as a
// bare `int`/`double` literal or a golib `Untyped*` wrapper, neither of which is the slice's
// element type; that is what makes `append`'s overload resolution pick the wrong element type
// (the `ISlice` overload infers T from the element, yielding e.g. `slice<int>`).
func (v *Visitor) isUntypedNumericConstArg(arg ast.Expr) bool {
	switch a := arg.(type) {
	case *ast.BasicLit:
		// Literals are untyped constants; numeric kinds only (avoid string/append-string).
		return a.Kind == token.INT || a.Kind == token.FLOAT || a.Kind == token.CHAR || a.Kind == token.IMAG
	case *ast.Ident:
		if constObj, ok := v.info.Uses[a].(*types.Const); ok {
			// A TIGHTENED local const is declared at its concrete type — element/parameter
			// inference sees that type directly, so the cast machinery does not apply
			// (see performUntypedConstAnalysis).
			if _, tightened := v.tightenedConsts[constObj]; tightened {
				return false
			}

			if basic, ok := constObj.Type().(*types.Basic); ok {
				return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
			}
		}
	case *ast.SelectorExpr:
		// A CROSS-PACKAGE untyped numeric const reached through a qualified selector
		// (`tabwriter.Escape`, whose `const Escape = '\xff'` renders as a golib `UntypedInt`)
		// is an untyped constant just like a bare ident, but arrives here as a SelectorExpr —
		// the const object hangs off the Sel ident. `append([]byte, tabwriter.Escape)` otherwise
		// leaves the two append overloads ambiguous (the ISlice overload infers T from the
		// UntypedInt element while slice<T> infers byte — CS0121 ×6, go/printer). Cast it to the
		// element type exactly as the bare-ident case does.
		if constObj, ok := v.info.Uses[a.Sel].(*types.Const); ok {
			if basic, ok := constObj.Type().(*types.Basic); ok {
				return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
			}
		}
	case *ast.UnaryExpr:
		// A numeric unary operator over an untyped numeric constant (`-1`, `+2`, `^0`) is itself an
		// untyped numeric constant but not a bare BasicLit/Ident (regexp `append(a, -1)` left the two
		// append overloads ambiguous, CS0121). Recurse on the operand.
		switch a.Op {
		case token.SUB, token.ADD, token.XOR:
			return v.isUntypedNumericConstArg(a.X)
		}
	}

	return false
}

// untypedNumericConstArgDefaultType returns the DEFAULT Go basic type an untyped numeric constant
// argument renders as (the type convExprList already casts it to for a defer/go call arg), or nil
// when the arg is not an untyped numeric constant. It mirrors isUntypedNumericConstArg's shape so
// the two stay in lockstep. Used to decide whether the lambda-form defer/go arg needs an explicit
// parameter-type override: when the parameter IS this default type the existing default-cast path
// already yields the right C# type, so no override (and no golden drift) is needed.
func (v *Visitor) untypedNumericConstArgDefaultType(arg ast.Expr) types.Type {
	switch a := arg.(type) {
	case *ast.BasicLit:
		switch a.Kind {
		case token.INT:
			return types.Typ[types.Int]
		case token.FLOAT:
			return types.Typ[types.Float64]
		case token.CHAR:
			return types.Typ[types.Rune]
		case token.IMAG:
			return types.Typ[types.Complex128]
		}
	case *ast.Ident:
		if constObj, ok := v.info.Uses[a].(*types.Const); ok {
			if _, tightened := v.tightenedConsts[constObj]; tightened {
				return nil
			}
			if basic, ok := constObj.Type().(*types.Basic); ok && basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0 {
				return types.Default(basic)
			}
		}
	case *ast.SelectorExpr:
		if constObj, ok := v.info.Uses[a.Sel].(*types.Const); ok {
			if basic, ok := constObj.Type().(*types.Basic); ok && basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0 {
				return types.Default(basic)
			}
		}
	case *ast.UnaryExpr:
		switch a.Op {
		case token.SUB, token.ADD, token.XOR:
			return v.untypedNumericConstArgDefaultType(a.X)
		}
	}

	return nil
}

// untypedIntGenericArgCastType returns the C# type an untyped INTEGER constant argument must be
// cast to when it feeds a bare type-parameter slot of a generic call — or "" when no cast is
// needed. C# infers the type argument from the literal's OWN type, and a bare int literal is
// `int` (System.Int32); go/types has already resolved the literal to the type parameter's
// concrete instantiation, so the cast retypes it to bind the intended argument (`nint` for Go
// `int`, `byte`, `long` for `int64`, `nuint` for `uint`/`uintptr`, …). Returns "" for a
// non-constant, a non-integer resolved type, or a resolved `int32`/`rune` kind — a bare int
// literal already IS System.Int32, so those need no retype (an int literal in an int32 context
// gains no noise). Reuses isUntypedNumericConstArg's constant test — the same one the append and
// narrow-int element casts key off — so a tightened local const (already declared at its concrete
// type) is correctly excluded.
func (v *Visitor) untypedIntGenericArgCastType(arg ast.Expr) string {
	if !v.isUntypedNumericConstArg(arg) {
		return ""
	}

	tv, ok := v.info.Types[arg]

	if !ok || tv.Value == nil {
		return ""
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 || basic.Info()&types.IsUntyped != 0 {
		return ""
	}

	// int32/rune already renders as — and C# infers as — System.Int32 from a bare literal.
	if basic.Kind() == types.Int32 {
		return ""
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false))
}

// genericInferenceArgCastType is untypedIntGenericArgCastType widened to the FOLDED constant
// EXPRESSION. The syntactic untyped-constant test recognizes a literal, an untyped const ident or
// selector, and a unary sign over one — but not `1<<10`, `3+4`, or a parenthesized form, which
// go/types folds to a constant and the converter emits as bare C# arithmetic whose natural type is
// `int` in exactly the same way. `expectMissing(t, k, 1<<10)` is ordinary Go and mis-infers for
// precisely the reason the literal does, so the rule covers its own class rather than half of it.
//
// The delegation order matters: untypedIntGenericArgCastType is consulted FIRST so its deliberate
// exclusions still hold — most importantly the TIGHTENED local const, which is declared at its
// concrete C# type and therefore already infers correctly (see performUntypedConstAnalysis). Only
// the two expression forms it does not classify are examined here, and only when go/types recorded
// a constant VALUE for them; a non-constant expression carries the mapped Go type in its emitted C#
// already and needs no retype. The type test mirrors its sibling exactly, int32/rune included.
func (v *Visitor) genericInferenceArgCastType(arg ast.Expr) string {
	if castType := v.untypedIntGenericArgCastType(arg); castType != "" {
		return castType
	}

	switch arg.(type) {
	case *ast.BinaryExpr, *ast.ParenExpr:
	default:
		return ""
	}

	tv, ok := v.info.Types[arg]

	if !ok || tv.Value == nil {
		return ""
	}

	basic, ok := types.Unalias(tv.Type).(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 || basic.Info()&types.IsUntyped != 0 {
		return ""
	}

	// int32/rune already renders as — and C# infers as — System.Int32 from a bare literal.
	if basic.Kind() == types.Int32 {
		return ""
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false))
}

// typeParamReachesInvariantResult reports whether type parameter tp appears inside a CONSTRUCTED
// result type of sig — a func, slice, array, map, chan or pointer over tp — as opposed to a result
// that IS tp itself. It is the gate for retyping a constant argument at a freely-inferred type
// parameter (see the call-site cast), and the distinction it draws is C#'s variance rule:
//
//   - a result that is the BARE type parameter mis-infers to C# `int`, but the implicit int→nint
//     conversion repairs it at the use site, so nothing is emitted and no golden moves;
//   - a result that CONSTRUCTS a type over the parameter is invariant — `Action<int, bool>` is not
//     `Action<nint, bool>` — so the mis-inference is unrepairable and the argument must carry the
//     type Go resolved.
//
// A generic NAMED result is skipped: invariant likewise, but the explicit type-argument arm already
// pins that instantiation, and adding an argument cast there would be redundant noise.
//
// The structural walk is constraintOperations' typeMentionsTypeParam, which already carries the
// *types.Signature case this gate turns on — a Go func result renders as a C# delegate, so a type
// parameter in `func(V, bool)` reaches a materialized type argument exactly as a slice element does.
// (Its shared sibling typeContainsTypeParams has no Signature case and is left untouched: it answers
// a different question for other callers.) Each call starts a fresh `seen` set, which is what
// terminates the recursion on a self-referential named type.
func typeParamReachesInvariantResult(sig *types.Signature, tp *types.TypeParam) bool {
	results := sig.Results()

	if results == nil {
		return false
	}

	for i := range results.Len() {
		resultType := types.Unalias(results.At(i).Type())

		switch resultType.(type) {
		case *types.TypeParam, *types.Named:
			continue
		}

		if typeMentionsTypeParam(resultType, tp, map[types.Type]bool{}) {
			return true
		}
	}

	return false
}

// typeParamIsSliceElementOfSibling reports whether type parameter tp is the ELEMENT type of some
// OTHER type parameter's `~[]E` slice-core constraint in the same signature — the `S ~[]E, E …`
// shape (slices.Index/Insert/Replace). Go's core-type inference fixes E from S's concrete element,
// but C#'s `where S : ISlice<E>` constraint carries no such flow — C# infers E purely from the
// value argument — so an untyped-int literal in the E slot mis-infers E and violates the S
// constraint unless it is retyped to E's instantiation (see the call-site cast). When tp is NOT
// thus locked — E free (`[T any](v ...T)`), or determined directly by another parameter's type
// (`[P *T, T any](p P, v T)`, where C# infers T from the pointer arg) — C# already infers it
// correctly, so no retype is wanted. Mirrors typeParamSliceCore's constraint walk.
func typeParamIsSliceElementOfSibling(sig *types.Signature, tp *types.TypeParam) bool {
	tparams := sig.TypeParams()

	if tparams == nil {
		return false
	}

	for k := range tparams.Len() {
		sibling := tparams.At(k)

		if sibling == tp {
			continue
		}

		if core := typeParamSliceCore(sibling); core != nil {
			if elem, ok := types.Unalias(core.Elem()).(*types.TypeParam); ok && elem == tp {
				return true
			}
		}
	}

	return false
}

// untypedConstBoxCast reports the C# type an UNTYPED-CONSTANT expression must be cast to before it
// is materialized into an EMPTY-INTERFACE slot — Go's DEFAULT TYPE for the constant's kind — or "" when
// the rendered form already boxes as that type.
//
// Go boxes an untyped constant at its default type: untyped int → `int` (go2cs `nint`, an IntPtr),
// untyped rune → `rune` (`int32`), untyped float → `float64`. Interface equality (golib `AreEqual`)
// and type assertions/switches (`._<nint>()`, `case int:`) compare the boxed DYNAMIC TYPE first, and
// fmt's printArg type-switch dispatches on it, so a value boxed under the wrong CLR type silently
// diverges from Go — it compares unequal, fails its assertion, or formats through the reflection
// fallback. Two renderings need the cast:
//
//   - A bare int LITERAL (or literal-only arithmetic) in the int32 range: convBasicLit renders it as a
//     plain C# integer literal, which is `System.Int32`, not nint. (A larger int constant already
//     renders as `(nint)…L`, correctly boxed; a rune literal renders `(rune)'A'` and a float literal
//     `2.5D`, both already the default CLR type — so those need nothing.)
//   - ANY expression referencing a NAMED untyped constant (`const fsize = 5`), which renders as the
//     golib `UntypedInt`/`UntypedFloat` WRAPPER STRUCT (convertToCSFullTypeName), never a CLR number —
//     `fsize+1` evaluates through the wrapper's operator overloads and boxes the struct. The cast to
//     the default type routes through the wrapper's implicit conversion, so the box carries Go's type.
//     This holds at EVERY magnitude and for every wrapped kind, which is why the int32-range test
//     applies to the literal rendering only.
//
// An untyped STRING constant is the mirror image of the numeric kinds. Go's default type is `string`
// (golib `@string`), and it is the LITERAL rendering that boxes wrong: convBasicLit emits a plain C#
// string (`"seed"`, a System.String) or a `"…"u8` ReadOnlySpan<byte> (a ref struct, which cannot box
// at all — CS0029). A NAMED string constant needs nothing, typed or untyped, because it is emitted as
// an `@string` member — there is no `UntypedString` wrapper struct — and its concatenations evaluate
// through `@string`'s own operators. So the string arm keys off the literal-only SHAPE
// (constExprIsStringLiteralConcat), exactly inverting the `wrapped` test the numeric arms apply.
//
// Keying the kind off info.Types[arg] (rather than the AST shape) reads the type go/types has already
// DEFAULTED for the interface slot, so a literal (`42`), a unary (`-5`), a binary (`1 + 2`), and a
// named untyped const are all classified by the same rule. An untyped COMPLEX constant is deliberately
// excluded: a named one renders as golib `GoBigConst` (a BigInteger parse — visitValueSpec's
// writeUntypedConst path, with its own standing TODO), which is a separate pre-existing gap that a
// `(complex128)` cast would not close.
func (v *Visitor) untypedConstBoxCast(arg ast.Expr) string {
	// A type-conversion CallExpr (`int(x)`) is itself a constant of type int but already renders with
	// its own `(nint)…` cast — skip it so the box is not double-wrapped.
	if _, isCall := arg.(*ast.CallExpr); isCall {
		return ""
	}

	tv, ok := v.info.Types[arg]

	if !ok || tv.Value == nil {
		return ""
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok {
		return ""
	}

	wrapped := v.exprRendersUntypedConstWrapper(arg)

	switch basic.Kind() {
	case types.Int:
		if wrapped {
			return "nint"
		}

		if iv, exact := constant.Int64Val(tv.Value); exact && iv >= math.MinInt32 && iv <= math.MaxInt32 {
			return "nint"
		}
	case types.Int32: // Go's default type for an untyped RUNE constant
		if wrapped {
			return "int32"
		}
	case types.Float64:
		if wrapped {
			return "float64"
		}
	case types.String: // Go's default type for an untyped STRING constant
		if constExprIsStringLiteralConcat(arg) {
			return "@string"
		}
	}

	return ""
}

// constExprIsStringLiteralConcat reports whether expr is built exclusively from STRING literals
// joined by `+` (through parens) — the exact shape convBasicLit renders as a plain C# string or a
// `"…"u8` span rather than a golib `@string`. Every other constant-string leaf already carries
// `@string`: a named constant (typed or untyped alike) is emitted as an `@string` member, and a
// conversion emits concretely typed. The string twin of constExprIsIntLiteralArithmetic.
func constExprIsStringLiteralConcat(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return constExprIsStringLiteralConcat(e.X)
	case *ast.BinaryExpr:
		return e.Op == token.ADD &&
			constExprIsStringLiteralConcat(e.X) && constExprIsStringLiteralConcat(e.Y)
	case *ast.BasicLit:
		return e.Kind == token.STRING
	}

	return false
}

// applyUntypedConstBoxCast wraps an already-rendered untyped-constant expression in the default-type
// cast untypedConstBoxCast reports for it, or returns the rendering unchanged when none applies.
//
// A rendering that ALREADY leads with that cast is left alone. The string-literal positions whose
// BasicLitContext carries castToGoString emit the tighter `(@string)"…"` form during rendering
// (anyBoxedStringLitContext and its siblings), which wholeExprIsCastOfType does not recognize —
// its operand is not parenthesized — so without this test the box would be double-cast. The test is
// sound for the wider `(@string)"a" + "b"` shape too: `@string`'s own `operator+` already yields an
// `@string`.
// A HOISTED string literal (Tier C) is likewise already the right box: an `@string` field needs no
// cast, and a PRE-BOXED `object` field IS the @string box — re-casting it would unbox and re-box a
// fresh one on every evaluation, defeating the hoist at exactly the `any`-slot sites it targets.
func (v *Visitor) applyUntypedConstBoxCast(value ast.Expr, rendered string) string {
	if basicLit, ok := value.(*ast.BasicLit); ok && hoistedLiteralName(basicLit) != "" {
		return rendered
	}

	castType := v.untypedConstBoxCast(value)

	if castType == "" || strings.HasPrefix(rendered, "("+castType+")") {
		return rendered
	}

	return fmt.Sprintf("(%s)(%s)", castType, rendered)
}

// exprRendersUntypedConstWrapper reports whether expr syntactically references — directly or through
// arithmetic — a named Go constant declared WITHOUT an explicit type (`const fsize = 5`), which makes
// the whole expression render as a golib untyped-constant WRAPPER STRUCT rather than a plain C#
// numeric literal. Such a constant is emitted as an `UntypedInt`/`UntypedFloat`-typed C# field or local
// (convertToCSFullTypeName's "untyped …" classification), so `fsize+1` evaluates through the wrapper's
// own operator overloads and yields a wrapper result. That distinguishes it from a bare literal or
// literal-only arithmetic (`42`, `1+2`), which convBasicLit always renders as a plain C# literal
// regardless of AST shape. The constant's OWN declared type is what is tested — not info.Types[expr],
// which reports the post-DEFAULT type (plain `int`) for a literal and a named const alike.
//
// A boxed wrapper struct is not any Go type: fmt's printArg type-switch (print.cs) doesn't recognize
// it and falls back to reflection, formatting `fmt.Sprintf("%d", fsize+1)` as a two-field struct
// (`{6 %!d(bool=false)}`) instead of `6` — the go/token TestIssue57490 failure — and interface
// equality against the same value under its real Go type reports unequal.
func (v *Visitor) exprRendersUntypedConstWrapper(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		ident, ok := n.(*ast.Ident)

		if !ok {
			return true
		}

		constObj, ok := v.info.Uses[ident].(*types.Const)

		if !ok {
			return true
		}

		if basic, ok := constObj.Type().(*types.Basic); ok {
			switch basic.Kind() {
			case types.UntypedInt, types.UntypedRune, types.UntypedFloat:
				found = true
			}
		}

		return true
	})

	return found
}

// boxUntypedConstAsDefaultType wraps an already-rendered value expression in the cast to Go's default
// type for its untyped-constant kind when `target` is the empty interface (see untypedConstBoxCast).
// It is the non-call-argument twin of the castArgToType treatment convCallExpr applies at interface
// call sites — assignment, var-spec, return, channel send, and keyed composite/struct/map positions
// render a value against a known empty-interface slot and route through here so a later `.(int)` /
// `case int:` / `==` observes Go's boxed dynamic type. A non-empty-interface slot, a type-parameter
// slot, or a non-untyped-constant value passes through unchanged. Mirrors the string→@string family's
// per-position boxing (castToGoString), which the empty interface likewise handles outside
// convertToInterfaceType.
func (v *Visitor) boxUntypedConstAsDefaultType(target types.Type, value ast.Expr, rendered string) string {
	if !isEmptyInterfaceTarget(target) {
		return rendered
	}

	return v.applyUntypedConstBoxCast(value, rendered)
}

// recordConversionPackageUsing registers the import alias → C# namespace for any cross-package named
// type referenced (directly, or through a pointer/slice/array/map/channel wrapper or a generic type
// argument) by a recorded implicit conversion. The generated `[assembly: GoImplicitConv<…>]` lines in
// package_info.cs use the alias form (e.g. `abi.Type`), but that file has no file-local `using abi =
// …`; conversionPackageUsings drives a resolving `global using` there.
func (v *Visitor) recordConversionPackageUsing(t types.Type) {
	switch t := t.(type) {
	case *types.Pointer:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Slice:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Array:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Map:
		v.recordConversionPackageUsing(t.Key())
		v.recordConversionPackageUsing(t.Elem())
	case *types.Chan:
		v.recordConversionPackageUsing(t.Elem())
	case *types.Named:
		if obj := t.Obj(); obj != nil {
			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				// Register under the same qualifier the recorded conversion STRINGS carry:
				// a Δ-renamed import (`Δsyscall`, internal/poll) records `Δsyscall.Sockaddr…`
				// attribute type names, so the resolving using must declare that exact alias
				// (`using Δsyscall = go.syscall_package;`) — the plain-name using left the
				// attributes unresolvable (CS0246 ×4). Unrenamed imports are unchanged
				// (importQualifier is the identity for them).
				packageLock.Lock()
				conversionPackageUsings[importQualifier(pkg.Name())] = convertImportPathToNamespace(pkg.Path(), PackageSuffix)
				packageLock.Unlock()
			}
		}

		if typeArgs := t.TypeArgs(); typeArgs != nil {
			for i := 0; i < typeArgs.Len(); i++ {
				v.recordConversionPackageUsing(typeArgs.At(i))
			}
		}
	}
}

// isRawAddressPointerConversion reports whether callExpr is a pointer-type conversion `(*T)(p)` whose
// RESULT is a pointer type and whose SOURCE is a raw address — an unsafe.Pointer or a uintptr. Such a
// conversion reinterprets the raw address as a `*T` (golib `ж<T>`); because `unsafe.Pointer` is the golib
// `Pointer : ж<uintptr>`, a direct `(ж<T>)p` needs two chained user-defined conversions (Pointer→uintptr→
// ж<T>) that C# rejects (CS0030), so the caller routes it through uintptr instead. Excludes the pointer-to-
// named-type value conversion (arg is a *types.Pointer, handled separately) — only a genuine raw-address
// source (Basic UnsafePointer/Uintptr) qualifies.
// makeLenArgs renders the length/capacity/size-hint arguments of a `make(T, len[, cap])` call (slice, map,
// or chan), casting any argument whose Go type is an integer with no implicit C# conversion to nint to nint
// — so it binds the golib `slice<T>(nint,nint)` / `map<K,V>(nint)` / `channel<T>(nint)` constructor rather
// than falling onto `slice<T>(T[])` or failing `nuint`→`nint` (CS1503). A plain int / untyped constant
// binds directly and is left alone (no golden churn).
func (v *Visitor) makeLenArgs(args []ast.Expr) string {
	parts := make([]string, len(args))

	for i, arg := range args {
		argStr := v.convExpr(arg, nil)

		if v.makeLenArgNeedsNintCast(arg) {
			// A NAMED numeric type (`type Hash uint`, crypto's maxHash in `make([]T, maxHash)`) renders
			// as a [GoType] wrapper struct with implicit conversions only to/from its UNDERLYING, so a
			// direct `(nint)(Hash)` has no conversion (CS0030). Route the cast through the underlying
			// numeric so the wrapper's implicit operator applies first: `(nint)(nuint)(maxHash)`.
			if argType := v.info.TypeOf(arg); argType != nil {
				if _, isNamed := types.Unalias(argType).(*types.Named); isNamed {
					underlyingCS := convertToCSTypeName(v.getAliasQualifiedTypeName(argType.Underlying(), false))
					argStr = fmt.Sprintf("(nint)(%s)(%s)", underlyingCS, argStr)
				} else {
					argStr = "(nint)(" + argStr + ")"
				}
			} else {
				argStr = "(nint)(" + argStr + ")"
			}
		}

		parts[i] = argStr
	}

	return strings.Join(parts, ", ")
}

// makeLenArgNeedsNintCast reports whether a make length/cap argument's type is an integer that C# will not
// implicitly convert to nint (so `new slice<T>(arg)` must cast it): true for uintptr/uint/uint32/uint64/
// int64; false for int/int8/int16/int32/uint8/uint16 (which widen to nint) and untyped constants (which
// render as bare literals that bind to nint directly).
func (v *Visitor) makeLenArgNeedsNintCast(arg ast.Expr) bool {
	argType := v.info.TypeOf(arg)

	if argType == nil {
		return false
	}

	basic, ok := argType.Underlying().(*types.Basic)

	if !ok {
		return false
	}

	if basic.Info()&types.IsUntyped != 0 {
		return false
	}

	switch basic.Kind() {
	case types.Int64, types.Uint, types.Uint32, types.Uint64, types.Uintptr:
		return true
	}

	return false
}

func (v *Visitor) isRawAddressPointerConversion(callExpr *ast.CallExpr, arg ast.Expr) bool {
	if _, ok := v.info.TypeOf(callExpr).(*types.Pointer); !ok {
		return false
	}

	argType := v.info.TypeOf(arg)

	if argType == nil {
		return false
	}

	if basic, ok := argType.Underlying().(*types.Basic); ok {
		return basic.Kind() == types.UnsafePointer || basic.Kind() == types.Uintptr
	}

	return false
}

// pointerReinterpretIdentitySource reports the underlying pointer expression when a `(*T)(…)`
// conversion is a semantic IDENTITY — its source, after peeling an optional escape-analysis
// identity wrapper, is a pointer that ALREADY has the same pointer type `*T`, reached either
// DIRECTLY (`(*T)(p)`) or through an `unsafe.Pointer(p)` round trip.
//
// Go's strings.Builder/bytes.Buffer copyCheck writes `b.addr = (*Builder)(abi.NoEscape(unsafe.
// Pointer(b)))`, which the language spec makes exactly `b.addr = b` (see the type's own TODO to
// revert it once escape analysis improves). Emitting the uintptr round-trip instead
// DEREFERENCES-and-COPIES through golib's `(ж<T>)(uintptr) => new ж<T>(*(T*)value)`, producing a
// box that is NOT reference-equal to the source — so the type's copy-by-value self-check
// (`b.addr != b`) false-fires at runtime. Returning p lets the caller emit the box directly and
// preserve managed-pointer identity.
//
// The DIRECT form — `*(*T)(p)` with p already `*T`, the shape reflect/runtime use to re-read a
// pointer at a fixed type — is the same no-op, but its source is a MANAGED BOX (`ж<T>`), never an
// address. Routing it through the raw-address uintptr bridge emitted `(ж<T>)(uintptr)(p)`, and a
// deref-aliased pointer parameter renders as its VALUE alias, so the leg had no conversion at all
// (CS0030 — `Cannot convert type 'Pt' to 'uintptr'`). Returning p emits the box, which the caller's
// deref then reads in place: correct for a value read, an lvalue write, and pointer identity alike.
// Returns nil when the pattern does not apply (a DIFFERENT element type is a genuine reinterpret and
// keeps the round-trip).
func (v *Visitor) pointerReinterpretIdentitySource(callExpr *ast.CallExpr, arg ast.Expr) ast.Expr {
	p, srcPtr, targetPtr := v.pointerConversionSource(callExpr, arg)

	if p == nil {
		return nil
	}

	// p must already be `*T` with the SAME element type as the target — only then is the whole
	// conversion a no-op identity. A DIFFERENT element type is a genuine reinterpret and belongs to
	// pointerReinterpretManagedSource below.
	if !types.Identical(srcPtr.Elem(), targetPtr.Elem()) {
		return nil
	}

	return p
}

// pointerReinterpretManagedSource reports the source pointer expression and the target's C# element
// type name when a `(*U)(…)` conversion is a genuine REINTERPRET — different element types — whose
// source is a Go pointer `*T`, i.e. a MANAGED BOX (`ж<T>`) rather than a raw address.
//
// Such a reinterpret must ALIAS the source box, never round-trip through its numeric address. The
// address route (`(ж<U>)(uintptr)(…)`, below) builds a NATIVE-backed box that holds no reference to
// the source: it neither keeps the pointee alive nor survives the collector moving it, so a derived
// pointer that is RETAINED reads whatever later occupies that memory. reflect caches exactly such a
// pointer for process lifetime (`toRType` → `canonType`), and once the address was recycled
// `TypeOf(x).Kind()` reported Invalid mid-process — silently, corpus-wide. Emitting golib's
// `Reinterpret<U>()` instead aliases the same managed storage, which is Go's own semantics: a
// pointer obtained through `unsafe.Pointer` is a real reference the collector tracks. (A `uintptr`
// source keeps the address route — matching Go's rule that a uintptr is a NUMBER which does not
// keep its referent alive. See docs/phase4/FINDING-managed-box-uintptr-lifetime.md.)
//
// Whether the derived pointer may ALIAS the source's storage or must fall back to the address route
// is decided at RUNTIME by golib, not here: it turns on the C# layout of the two surrogates, which a
// go2cs surrogate does not inherit from its Go type (a Go `[2]byte` is 2 bytes; `array<byte>` is a
// reference to a backing store). The emission is therefore uniform and golib gates it.
//
// A pointer-to-ARRAY target (`(*[N]T)(unsafe.Pointer(p))`) is excluded here rather than at the golib
// gate, because `Reinterpret` can never take the managed arm for one: `array<X>` is an 8-byte struct
// holding a backing-store REFERENCE, so it fails the size gate against any smaller pointee and the
// reference gate against any numeric one — the emission would fall through to exactly the address
// route it replaced. And the address route's TEXT is not inert: the slice-of-pointer-cast fusion in
// convSliceExpr keys on a leading `(ж<…>` (isPointerCast) to render `(*[N]T)(ptr)[:n]` as a
// `slice<T>` over a `ReadOnlySpan<T>` of the pointed-to memory — the only lowering available for an
// array an `array<T>` can neither view (native memory) nor be fabricated from (a scalar's bytes).
// Emitting `Reinterpret` instead silently defeats that match and leaves `(~box).slice(…)` over an
// `array<T>` whose backing reference was punned out of the pointee's data: a fabricated managed
// reference, i.e. an AccessViolation rather than the previous correct slice. Measured on
// internal/syscall/windows/registry.GetStringValue — the read behind time.initLocalFromTZI and
// mime.initMimeWindows — which returned its value before and hard-faults after. So array targets
// keep this route whenever the source pointer's element type DIFFERS from the array's;
// arrayPointerAliasEmission below takes the one case where the managed model has a real window
// (same element type), and everything else here is unchanged.
//
// Returns a nil expression when the pattern does not apply.
func (v *Visitor) pointerReinterpretManagedSource(callExpr *ast.CallExpr, arg ast.Expr) (ast.Expr, string, string) {
	p, srcPtr, targetPtr := v.pointerConversionSource(callExpr, arg)

	if p == nil {
		return nil, "", ""
	}

	if types.Identical(srcPtr.Elem(), targetPtr.Elem()) {
		return nil, "", ""
	}

	// Pointer-to-array target — keep the pre-existing route (see the header comment), UNLESS the
	// SOURCE pointee is ALSO a named type over the identical array shape: two sibling names for one
	// underlying array, Go's `(*pageBits)(b)` where `b *pallocBits` and both are `[N]E` (runtime's
	// mpallocbits.go). That sub-case is exactly what the header comment's exclusion does NOT cover —
	// the array-target failure mode it describes is a numeric-or-other pointee reinterpreting to an
	// ARRAY pointee, which golib's Reinterpret gate always refuses (array<E> is a backing-store
	// REFERENCE, too wide to alias a narrower slot) and would silently fall through to the very
	// address route this function exists to avoid. Here BOTH sides are the SAME array<E>-backed
	// [GoType("Array")] wrapper shape — go2cs-gen's InheritedTypeTemplate gives every such wrapper
	// ONE field (an E[]-backing StrongBox slot), so golib's ReinterpretAliasesStorage sees two
	// single-field structs of identical layout and correctly takes the aliasing path.
	//
	// Emitting the pre-existing route here is not merely suboptimal, it is UNSOUND: the general
	// conversion path renders `(*pageBits)(b)` as a VALUE conversion — a generated `implicit
	// operator pageBits(pallocBits value) => value.view` — over a BY-VALUE parameter, so a
	// still-unmaterialized source's lazy backing materializes on the OPERATOR'S OWN PARAMETER COPY,
	// never on the caller's storage; every write through the derived pointer before something else
	// touches the original is silently lost. Measured with a transcription of this exact shape
	// (ElemAliasProbe arm8): EXPOSED on first touch, fine only once something else has already
	// materialized the source. Routing through Reinterpret (like every other managed pointer
	// reinterpret here) never constructs that copy at all.
	if targetArr, isArray := targetPtr.Elem().Underlying().(*types.Array); isArray {
		srcArr, srcIsArray := srcPtr.Elem().Underlying().(*types.Array)

		if !srcIsArray || !types.Identical(srcArr, targetArr) {
			return nil, "", ""
		}
	}

	return p,
		convertToCSTypeName(v.getAliasQualifiedTypeName(srcPtr.Elem(), false)),
		convertToCSTypeName(v.getAliasQualifiedTypeName(targetPtr.Elem(), false))
}

// reinterpretManagedEmission renders the golib managed reinterpret for a `(*U)(unsafe.Pointer(p))`
// whose source is a Go pointer, reporting false when the shape does not apply. Called at BOTH points
// that would otherwise emit the `(ж<U>)(uintptr)(…)` address route — the conversion path and the
// regular-call path — since `(*U)(unsafe.Pointer(…))` mis-classifies as a non-conversion and reaches
// only the latter. The isPointer context renders a deref-aliased pointer param/receiver as its BOX
// (`Ꮡt`), which is what carries the provenance; see pointerReinterpretManagedSource.
func (v *Visitor) reinterpretManagedEmission(callExpr *ast.CallExpr, arg ast.Expr) (string, bool) {
	src, srcElem, targetElem := v.pointerReinterpretManagedSource(callExpr, arg)

	if src == nil {
		return "", false
	}

	identContext := DefaultIdentContext()
	identContext.isPointer = true

	// `unsafe.Pointer` as the TARGET pointee is the one destination the storage reinterpret can
	// never serve: it is a CLASS, so golib's alias gate refuses it and the fallback deref-copied
	// whatever bytes sat in the source's first reference-sized slot INTO a Pointer reference — a
	// fabricated managed reference. time.syncTimer's `*(*unsafe.Pointer)(unsafe.Pointer(&c))` did
	// that on EVERY NewTimer: junk dispatch on a quiet heap, an AccessViolationException inside
	// Pointer.op_Implicit when the punned bits landed unmapped (measured twice, the 1.23.12 time
	// suite at GODEBUG=asynctimerchan=2). Emit the CARRYING form instead — Go's semantics for the
	// read is "the pointer word at that storage", and the managed model carries a word two ways:
	//   - a uintptr source's word IS the number, so the derived Pointer holds the dereffed value —
	//     exact Go fidelity (runtime/stack.go's `*(*unsafe.Pointer)(&pp)` shape);
	//   - any other managed pointee's word is a REFERENCE, which no Pointer value can hold, so the
	//     derived Pointer carries the source BOX's pin token (`(uintptr)Ꮡx` registers it with
	//     ManagedPointerTokens) — non-nil, stable for the box's lifetime, and provenance-resolvable
	//     back to the storage it names. One corner is knowingly inexact and harmless where the
	//     stdlib uses the shape: a nil channel's word is 0 in Go, while the token of the box
	//     HOLDING that nil channel is non-zero (syncTimer's consumer reads only the nil-bit and
	//     recomputes it from the GODEBUG setting; no stdlib site passes a nil channel here).
	// Both arms wrap in Ꮡ(…) so the expression stays a ж<unsafe.Pointer> for the deref/pointer
	// contexts the four call sites serve. golib's Reinterpret keeps its own refusal for this shape
	// as defense-in-depth for emissions that have not been reconverted yet.
	//
	// The target may sit under MORE than one pointer level — Go reads a func value's code pointer as
	// `**(**unsafe.Pointer)(unsafe.Pointer(&fn))`, because a func value points at a funcval whose
	// first word is that pointer. The rationale above does not care how deep it is: at every level the
	// destination is still `unsafe.Pointer`, which a storage reinterpret still cannot serve. So unwrap
	// the levels and wrap the carrying form in one Ꮡ(…) per level, which the emitted derefs unwind
	// symmetrically. Matching only the single level left the func shape on the fallthrough, where its
	// pointee is a DELEGATE — a reference type, so the alias gate refuses on its very first clause and
	// the address route's deref is a nil dereference. reflect's TestValuePointerAndUnsafePointer builds
	// its case table EAGERLY, so that one element killed the whole test before any subtest ran: seven
	// empty verdicts from one throw.
	if levels, ok := unsafePointerTargetLevels(v.info.TypeOf(callExpr)); ok {
		boxExpr := v.convExpr(src, []ExprContext{identContext})
		carried := fmt.Sprintf("new @unsafe.Pointer((uintptr)%s)", boxExpr)

		if srcPtr, isPtr := types.Unalias(v.info.TypeOf(src)).(*types.Pointer); isPtr {
			if srcBasic, srcIsBasic := types.Unalias(srcPtr.Elem()).(*types.Basic); srcIsBasic && srcBasic.Kind() == types.Uintptr {
				carried = fmt.Sprintf("new @unsafe.Pointer(~%s)", boxExpr)
			}
		}

		for i := 0; i < levels; i++ {
			carried = "Ꮡ(" + carried + ")"
		}

		return carried, true
	}

	// Go's prefix-downcast idiom — allocate the larger record, hand out a pointer to its embedded
	// Type header, cast back — has no managed answer as a REINTERPRET: a ж<abi.Type> holds only an
	// abi.Type, so there is nothing behind it to downcast to, and golib's alias gate rightly refuses
	// (ReinterpretAliasesStorage). The address route it falls back to yields a box that reads as
	// zero, which is why funcLayout died on its own entry gate ("reflect: funcLayout of non-func
	// type <nil>"). But the descriptor CARRIES a System.Type, and for the records whose Go cargo is
	// recoverable from it the answer is to SYNTHESIZE rather than to downcast — so emit the
	// synthesizing accessor instead. See internal/abi/type_impl.cs.
	if emission, ok := v.synthesizedDowncastEmission(callExpr, src, srcElem, identContext); ok {
		return emission, true
	}

	return fmt.Sprintf("%s.Reinterpret<%s, %s>()", v.convExpr(src, []ExprContext{identContext}), srcElem, targetElem), true
}

// The internal/abi record types the managed model can synthesize from a descriptor's carried
// System.Type, keyed by the abi type name and valued by the accessor that does it. Deliberately a
// CURATED list and not a rule about embedding: an entry belongs here only once an accessor actually
// exists and its cargo has been measured, exactly as manualConversionFuncs is curated.
var synthesizedAbiDowncasts = map[string]string{
	"FuncType": "FuncType",
}

// synthesizedDowncastEmission renders a prefix downcast to a synthesizable abi record as that
// record's accessor, reporting false for every other shape. The source is either the abi.Type
// header itself (`t.common()`) or a single-field wrapper over it (`reflect.rtype`, Go's
// `type rtype struct { t abi.Type }`) — the wrapper is unwrapped with the SAME Reinterpret whose
// alias arm golib already takes for that correspondence, so the composition adds no new mechanism.
func (v *Visitor) synthesizedDowncastEmission(callExpr *ast.CallExpr, src ast.Expr, srcElem string, identContext ExprContext) (string, bool) {
	targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer)

	if !ok {
		return "", false
	}

	targetNamed, ok := types.Unalias(targetPtr.Elem()).(*types.Named)

	if !ok || targetNamed.Obj().Pkg() == nil || targetNamed.Obj().Pkg().Path() != "internal/abi" {
		return "", false
	}

	accessor, ok := synthesizedAbiDowncasts[targetNamed.Obj().Name()]

	if !ok {
		return "", false
	}

	// The record embeds the Type header as its first field; that is both what makes it a prefix
	// downcast and where the header's own type name comes from, so it is read rather than assumed.
	targetStruct, ok := targetNamed.Underlying().(*types.Struct)

	if !ok || targetStruct.NumFields() == 0 {
		return "", false
	}

	headerType := targetStruct.Field(0).Type()

	if !isAbiTypeHeader(headerType) {
		return "", false
	}

	srcPtr, ok := types.Unalias(v.info.TypeOf(src)).(*types.Pointer)

	if !ok {
		return "", false
	}

	boxExpr := v.convExpr(src, []ExprContext{identContext})

	// The header itself: the accessor binds directly.
	if isAbiTypeHeader(srcPtr.Elem()) {
		return fmt.Sprintf("%s.%s()", boxExpr, accessor), true
	}

	// A single-field wrapper over the header: unwrap first, then bind.
	if unwrapsToAbiTypeHeader(srcPtr.Elem()) {
		headerElem := convertToCSTypeName(v.getAliasQualifiedTypeName(headerType, false))
		return fmt.Sprintf("%s.Reinterpret<%s, %s>().%s()", boxExpr, srcElem, headerElem, accessor), true
	}

	return "", false
}

// isAbiTypeHeader reports whether t is internal/abi.Type, the header every descriptor record embeds.
func isAbiTypeHeader(t types.Type) bool {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return false
	}

	obj := named.Obj()

	return obj.Pkg() != nil && obj.Pkg().Path() == "internal/abi" && obj.Name() == "Type"
}

// unwrapsToAbiTypeHeader reports whether t is a struct whose ONE field is the abi.Type header —
// Go's `type rtype struct { t abi.Type }`, the correspondence golib's ReinterpretAliasesStorage
// already recognizes as an alias-safe single-field wrapper.
func unwrapsToAbiTypeHeader(t types.Type) bool {
	strct, ok := types.Unalias(t).Underlying().(*types.Struct)

	if !ok || strct.NumFields() != 1 {
		return false
	}

	return isAbiTypeHeader(strct.Field(0).Type())
}

// unsafePointerTargetLevels reports how many pointer levels stand between t and an `unsafe.Pointer`
// pointee, and whether t is such a type at all — 1 for `*unsafe.Pointer`, 2 for `**unsafe.Pointer`,
// and false for everything else. It is the depth-aware form of the single-level test the
// unsafe.Pointer carrying arm used to make; see reinterpretManagedEmission for why depth is not
// something that arm should care about.
func unsafePointerTargetLevels(t types.Type) (int, bool) {
	levels := 0

	for {
		ptr, isPtr := types.Unalias(t).(*types.Pointer)

		if !isPtr {
			return 0, false
		}

		levels++
		elem := types.Unalias(ptr.Elem())

		if basic, isBasic := elem.(*types.Basic); isBasic {
			if basic.Kind() == types.UnsafePointer {
				return levels, true
			}

			return 0, false
		}

		t = elem
	}
}

// arrayPointerAliasEmission renders golib's element-window alias for a `(*[N]T)(unsafe.Pointer(p))`
// whose SOURCE pointer has the SAME element type as the target array — the one array-target shape the
// managed model can express faithfully, and the sibling of the slice form's `array<T>.Alias` (Go 1.17
// `(*[N]T)(s)`). Reported false for every other shape, which keeps the raw-address route described on
// pointerReinterpretManagedSource.
//
// Go's array here is a VIEW of the elements at p — `(*[N]T)(unsafe.Pointer(&s[i]))` addresses s's own
// storage — so a write through it has to land in the caller's buffer. The address route cannot do
// that: it builds a native-backed `ж<array<T>>` whose deref reads an `array<T>` STRUCT (a backing
// reference plus bounds) out of the pointed-at DATA, i.e. a fabricated managed reference; where the
// convSliceExpr fusion catches the `[:n]` form first it instead yields a `slice<T>` COPY of the
// memory, whose writes go nowhere. os's `TestReadStdin` is the witness for the second: its
// `poll.ReadConsole` fake fills internal/poll's buffer through
// `copy((*[10000]uint16)(unsafe.Pointer(buf))[:n:n], s16)`, so every one of its 462 subtests read
// back zeros. Same element type is what makes the window exist — a `T[]` view over differently-typed
// storage has no managed spelling — and golib decides at RUNTIME whether the pointer actually
// addresses managed element storage, falling back to the identical address route when it does not
// (see array<T>.AliasPointer).
//
// A NAMED array target keeps the pre-existing route, matching the slice form (none in the corpus).
func (v *Visitor) arrayPointerAliasEmission(callExpr *ast.CallExpr, arg ast.Expr) (string, bool) {
	p, srcPtr, targetPtr := v.pointerConversionSource(callExpr, arg)

	if p == nil {
		return "", false
	}

	targetArr, ok := types.Unalias(targetPtr.Elem()).(*types.Array)

	if !ok || !types.Identical(srcPtr.Elem(), targetArr.Elem()) {
		return "", false
	}

	identContext := DefaultIdentContext()
	identContext.isPointer = true

	elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(targetArr.Elem(), false))

	return fmt.Sprintf("array<%s>.AliasPointer(%s, %s)", elemName, v.convExpr(p, []ExprContext{identContext}), csNintLiteral(targetArr.Len())), true
}

// csNintLiteral renders a Go array length as a C# `nint` argument. A literal that fits in `int` binds
// to `nint` implicitly and stays bare; a wider one types as `long`, for which no implicit conversion
// to `nint` exists (CS1503), so it takes an explicit cast. This is not a corner case for the
// array-POINTER form — the whole idiom is spelled with a sentinel length that stands for "as many as
// are there", and runtime's `findnull`/`findnullw`/`gostringw` use Go's own maxima
// (`1<<47 - 1` bytes, `1<<46 - 1` uint16s).
func csNintLiteral(value int64) string {
	if value >= math.MinInt32 && value <= math.MaxInt32 {
		return strconv.FormatInt(value, 10)
	}

	// A beyond-int32 CONSTANT conversion to `nint` is CS8778 unless it is unchecked. The value is a
	// Go array length and nint is 64-bit on every platform go2cs targets, so it is exact at runtime.
	return fmt.Sprintf("unchecked((nint)%d)", value)
}

// pointerConversionSource peels a `(*U)(…)` pointer CONVERSION down to its source pointer
// expression, returning that expression with the source and target pointer types. The source is
// reached either DIRECTLY (`(*U)(p)`) or through an `unsafe.Pointer(p)` round trip, in either case
// under an optional escape-analysis identity wrapper. Returns a nil expression when the conversion's
// source is not a Go pointer — a raw address (`unsafe.Pointer`/`uintptr` valued) has no box behind
// it and keeps the address route.
func (v *Visitor) pointerConversionSource(callExpr *ast.CallExpr, arg ast.Expr) (ast.Expr, *types.Pointer, *types.Pointer) {
	targetPtr, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Pointer)
	if !ok {
		return nil, nil, nil
	}

	// callExpr must be the CONVERSION `(*U)(…)` — its Fun DENOTES the pointer type. Without this the
	// direct-source form below matches any one-argument CALL that happens to take and return the same
	// pointer type, and elides it: `advance(a)` → `a`, `Ꮡp.Swap(Ꮡa)` → `Ꮡa`. (The unsafe.Pointer form
	// was implicitly guarded by requiring its ARGUMENT to be a type conversion; the direct form has no
	// such wrapper and must check the call itself.)
	if tv, isType := v.info.Types[callExpr.Fun]; !isType || !tv.IsType() {
		return nil, nil, nil
	}

	// Peel a single escape-analysis identity wrapper (`abi.NoEscape`/local `noescape`).
	inner := arg
	if call, ok := inner.(*ast.CallExpr); ok && len(call.Args) == 1 && v.isNoEscapeIdentityCall(call) {
		inner = call.Args[0]
	}

	// The source pointer, either reached directly or unwrapped from an `unsafe.Pointer(p)`
	// CONVERSION — a call whose Fun DENOTES the unsafe.Pointer type, not merely a function that
	// happens to return unsafe.Pointer (e.g. runtime `mallocgc(…)`), which is a genuine raw-address
	// reinterpret and keeps its path.
	p := inner

	if call, ok := inner.(*ast.CallExpr); ok && len(call.Args) == 1 {
		if tv, isType := v.info.Types[call.Fun]; isType && tv.IsType() {
			if basic, ok := v.info.TypeOf(call).Underlying().(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
				p = call.Args[0]
			}
		}
	}

	srcPtr, ok := types.Unalias(v.info.TypeOf(p)).(*types.Pointer)
	if !ok {
		return nil, nil, nil
	}

	return p, srcPtr, targetPtr
}

// isNoEscapeIdentityCall reports whether call is to an escape-analysis identity helper —
// `abi.NoEscape(…)` (internal/abi) or a package-local `noescape(…)` — each an
// unsafe.Pointer→unsafe.Pointer function that returns its argument unchanged. Matched by name
// AND signature shape so an unrelated one-arg call cannot be mistaken for it.
func (v *Visitor) isNoEscapeIdentityCall(call *ast.CallExpr) bool {
	var name string

	switch fun := call.Fun.(type) {
	case *ast.SelectorExpr:
		name = fun.Sel.Name
	case *ast.Ident:
		name = fun.Name
	default:
		return false
	}

	if name != "NoEscape" && name != "noescape" {
		return false
	}

	sig, ok := v.info.TypeOf(call.Fun).(*types.Signature)
	if !ok || sig.Params().Len() != 1 || sig.Results().Len() != 1 {
		return false
	}

	pIn, ok1 := sig.Params().At(0).Type().Underlying().(*types.Basic)
	pOut, ok2 := sig.Results().At(0).Type().Underlying().(*types.Basic)

	return ok1 && ok2 && pIn.Kind() == types.UnsafePointer && pOut.Kind() == types.UnsafePointer
}

func (v *Visitor) isTypeConversion(callExpr *ast.CallExpr) (bool, string) {
	// Get the object associated with the function being called
	var obj types.Object
	var isPointer bool

	targetExpr := callExpr.Fun

	for targetExpr != nil {
		switch funExpr := targetExpr.(type) {
		case *ast.ParenExpr:
			targetExpr = funExpr.X
			continue
		case *ast.IndexExpr:
			targetExpr = funExpr.X
			continue
		case *ast.IndexListExpr:
			// A conversion to a MULTI-type-parameter generic instantiation — Go 1.23 iter's
			// `Seq2Like[K, V](fn)` shape. The single-param IndexExpr case above already peels;
			// without this arm the two-param form fell through as a plain (non-invocable) call
			// (CS1955 on the emitted `Seq2Like<K, V>(…)`).
			// An ANONYMOUS INTERFACE target is the same family and is handled EARLIER, in
			// Phase 1b: it has no types.Object for this peel to find at all. See the
			// interfaceTypeLiteralTarget arm there, including why its cast is load-bearing.
			targetExpr = funExpr.X
			continue
		case *ast.StarExpr:
			if ident, ok := funExpr.X.(*ast.Ident); ok {
				obj = v.info.ObjectOf(ident)
				isPointer = true
			} else if sel, ok := funExpr.X.(*ast.SelectorExpr); ok {
				// A pointer conversion to a cross-package type: `(*atomic.Uint32)(p)`.
				obj = v.info.ObjectOf(sel.Sel)
				isPointer = true
			} else {
				// A pointer conversion to a TYPE-LITERAL target — `(*[4]uint64)(&s.s)`
				// (edwards25519's fiat reinterprets): a composite type has no types.Object,
				// so resolve the target directly from type info. Claimed ONLY for the fiat
				// reinterpret shape — the argument is a pointer to a defined type written
				// directly over an unnamed array, converting to a pointer to that exact
				// underlying array — so every other pointer-to-type-literal conversion (the
				// pointer-cast slice form `(*[1<<20]Method)(p)[:n:n]`, internal/abi) keeps
				// its pre-existing route byte-identically.
				if len(callExpr.Args) != 1 {
					return false, ""
				}

				elemType := v.info.TypeOf(funExpr.X)
				argType := v.info.TypeOf(callExpr.Args[0])

				if elemType == nil || argType == nil {
					return false, ""
				}

				// An untyped-nil operand converting to a pointer-to-TYPE-LITERAL target —
				// `(*[]byte)(nil)`, `(*struct{ r7 int })(nil)` (gob's bootstrapType table). A
				// composite target has no types.Object, so the Ident/SelectorExpr arms above
				// never fire and the shape fell through to the regular call path, where the
				// cast rendered `default!` and erased the TYPED nil to a bare null reference.
				// Go keeps the type — `reflect.TypeOf((*[]byte)(nil)).Elem()` is `[]uint8` —
				// so gob's package init NRE'd on the null descriptor. Claim it, exactly as the
				// named-target `(*T)(nil)` arm below already does, so the conversion renderer's
				// nil interception emits the canonical typed-nil pointer instance.
				if basic, ok := argType.(*types.Basic); ok && basic.Kind() == types.UntypedNil {
					return true, "*" + v.getAliasQualifiedTypeName(elemType, false)
				}

				if argPtr, ok := argType.(*types.Pointer); ok {
					if named, ok := argPtr.Elem().(*types.Named); ok && writtenRHSIsUnnamedArray(named) &&
						types.Identical(elemType, named.Underlying()) {
						return types.ConvertibleTo(argType, types.NewPointer(elemType)), "*" + v.getAliasQualifiedTypeName(elemType, false)
					}

					// A pointer reinterpret from a NAMED-SLICE pointer to its underlying-slice
					// pointer — `(*[][]byte)(buf)` with `buf *Buffers` (net fd_windows.go) —
					// is claimed so the conversion renderer's of-projection arm emits the
					// backing-field VIEW (`Ꮡbuf.of(Buffers.Ꮡm_value)`); unclaimed, the callee
					// rendered as a bare cast between unrelated ж<> instantiations (CS0030).
					if named, ok := types.Unalias(argPtr.Elem()).(*types.Named); ok {
						if underSlice, ok := named.Underlying().(*types.Slice); ok {
							if elemSlice, ok := elemType.Underlying().(*types.Slice); ok && types.Identical(underSlice, elemSlice) {
								if _, elemIsNamed := types.Unalias(elemType).(*types.Named); !elemIsNamed {
									return true, "*" + v.getAliasQualifiedTypeName(elemType, false)
								}
							}
						}
					}
				}

				// Go 1.17 slice-to-array-POINTER — `(*[32]byte)(x)` with a SLICE argument
				// (edwards25519 fiatScalarFromBytes' input, CS0030): claimed so the
				// slice-to-array conversion arm boxes the golib copy.
				if slc, ok := argType.Underlying().(*types.Slice); ok {
					if arrType, ok := types.Unalias(elemType).(*types.Array); ok && types.Identical(arrType.Elem(), slc.Elem()) {
						return true, "*" + v.getAliasQualifiedTypeName(elemType, false)
					}
				}

				return false, ""
			}
			targetExpr = nil
		case *ast.Ident:
			obj = v.info.ObjectOf(funExpr)
			targetExpr = nil
		case *ast.SelectorExpr:
			obj = v.info.ObjectOf(funExpr.Sel)
			targetExpr = nil
		case *ast.ArrayType, *ast.MapType, *ast.ChanType:
			// A composite type literal used as a conversion target whose argument is a
			// named type with the *same* underlying shape — the `[]CaseRange(special)`
			// pattern, where `special` is `type SpecialCase []CaseRange`. This lowers to
			// a cast through the generated implicit operator: `((slice<CaseRange>)special)`.
			// These type-literal targets have no associated types.Object, so resolve the
			// target type directly from type info.
			//
			// Restricted to identical-underlying conversions so that element-decoding
			// conversions like `[]rune(s)` / `[]byte(s)` (string source) keep their
			// existing argument-rendering path (which casts the source to @string first).
			if len(callExpr.Args) != 1 {
				return false, ""
			}

			targetType := v.info.TypeOf(funExpr)
			argType := v.info.TypeOf(callExpr.Args[0])

			if targetType == nil || argType == nil {
				return false, ""
			}

			// Go 1.20 slice-to-ARRAY value conversion — `[4]byte(slice)` (netip
			// AddrFromSlice, CS1955): claimed so the slice-to-array conversion arm emits
			// the golib copy ctor.
			if arrType, ok := types.Unalias(targetType).(*types.Array); ok {
				if slc, ok := argType.Underlying().(*types.Slice); ok && types.Identical(arrType.Elem(), slc.Elem()) {
					return true, v.getAliasQualifiedTypeName(targetType, false)
				}
			}

			// An untyped-nil operand converting to an unnamed MAP type — `map[string]int(nil)`,
			// the `reflect.TypeOf(map[K]V(nil))` descriptor idiom. UntypedNil's underlying is
			// itself, so the identical-underlying guard below rejected the shape and it fell
			// through to the regular CALL path, which emitted `map<@string, nint>(default!)` —
			// CS1955, since `map<TKey, TValue>` is a type and not a method. The NAMED twin
			// `myMap(nil)` is already claimed further down (ConvertibleTo holds for it) and casts
			// correctly, so only the type-LITERAL spelling ever broke.
			//
			// Slice and BIDIRECTIONAL channel literals are deliberately NOT claimed alongside
			// it, having no defect to fix: `[]byte(nil)` binds golib's real
			// `builtin.slice<T>(T[])` conversion helper — the same one `[]byte("…")` is emitted
			// against — and yields the nil slice, while `(chan T)(nil)` already renders as a
			// cast. Claiming either would rewrite ~25 corpus sites to no effect.
			if _, targetIsMap := targetType.Underlying().(*types.Map); targetIsMap {
				if basic, ok := argType.(*types.Basic); ok && basic.Kind() == types.UntypedNil {
					return true, v.getAliasQualifiedTypeName(targetType, false)
				}
			}

			// An untyped-nil operand converting to a DIRECTIONAL channel literal —
			// `(chan<- string)(nil)`, reflect's TypeOf descriptor idiom for a directional type
			// (TestAll #12, TestChanOfDir). The bidirectional form stays unclaimed per the note
			// above — its cast of `default!` IS the correct nil — but the directional cast
			// erased the direction the conversion exists to apply. Claimed exactly as the map
			// arm is, so the conversion renderer's nil interception (chanDirNilValue) emits the
			// directional nil factory. chanDirCargoName is the gate: non-empty only for a
			// directional, undefined channel type, so every other channel conversion keeps its
			// path byte for byte. Part of the 2026-09-01 r39d amendment — see
			// chanDirectionCargo.go.
			// Increment D: nil-conv joins the DIMS stamp set on day one; `(chan [100]T)(nil)` is the
			// only channel-of-array creation site in the std tree (the D census), so a gate that
			// admitted directions alone would miss the row it exists for.
			if chanDirCargoName(targetType) != "" || chanCargoExpr(targetType) != "" {
				if basic, ok := argType.(*types.Basic); ok && basic.Kind() == types.UntypedNil {
					return true, v.getAliasQualifiedTypeName(targetType, false)
				}
			}

			if !types.Identical(targetType.Underlying(), argType.Underlying()) {
				return false, ""
			}

			return types.ConvertibleTo(argType, targetType), v.getAliasQualifiedTypeName(targetType, false)
		default:
			return false, ""
		}
	}

	if obj == nil {
		return false, ""
	}

	// Check if the function being called is a type name
	resolvedTypeName, ok := obj.(*types.TypeName)

	if !ok {
		return false, ""
	}

	// Get the target type
	targetType := resolvedTypeName.Type()

	// A conversion to a GENERIC INSTANTIATION (`Seq2Like[K, V](fn)`, Go 1.23 iter shapes)
	// resolves through the TypeName to the UNINSTANTIATED generic, against which
	// ConvertibleTo is false and the rendered name drops its type arguments. The
	// instantiated target is the type of the Fun expression itself. Gated to exactly the
	// uninstantiated-generic case: for a POINTER conversion the Fun type is the full `*T`
	// (the `*` is re-applied via isPointer below — overriding would double it, ж<ж<T>>),
	// and a non-generic target needs no override.
	if named, ok := targetType.(*types.Named); ok && !isPointer && named.TypeParams().Len() > 0 && named.TypeArgs() == nil {
		if tv, ok := v.info.Types[callExpr.Fun]; ok && tv.IsType() && tv.Type != nil {
			targetType = tv.Type
		}
	}

	// Type conversions typically have exactly one argument
	if len(callExpr.Args) != 1 {
		return false, ""
	}

	// Get the type of the argument
	argType := v.info.TypeOf(callExpr.Args[0])
	originalArgType := argType

	// Check if the argument is a pointer
	if pointer, ok := argType.(*types.Pointer); ok {
		argType = pointer.Elem()
	}

	typeName := v.getAliasQualifiedTypeName(targetType, false)

	if isPointer {
		typeName = "*" + typeName
	}

	// An untyped-nil operand converting to a POINTER-shaped target — `(*T)(nil)` (isPointer:
	// targetType resolved to the ELEM type) or `NamedPtr(nil)` — is a conversion the renderer
	// must claim: ConvertibleTo reports false for the UntypedNil operand, which mis-routed the
	// star form onto the regular call path, where the cast rendered `default!` and erased the
	// typed nil to a bare null. The conversion renderer's nil interception then emits the
	// canonical typed-nil instance. Other nil-able targets (slice/map/chan/func/interface/
	// unsafe.Pointer) deliberately keep their existing routes.
	if basic, ok := argType.(*types.Basic); ok && basic.Kind() == types.UntypedNil {
		if _, targetIsPtr := targetType.Underlying().(*types.Pointer); targetIsPtr || isPointer {
			return true, typeName
		}
	}

	// Check if the argument type is convertible to the target type. For an INTERFACE target
	// the ORIGINAL (pointer) argument type is probed too: a pointer-to-interface conversion —
	// `image.Image(dst)` with `dst *image.RGBA` (image/draw) — converts through the POINTER's
	// method set (the value type alone does not implement the interface), so the elem-only
	// probe misread the conversion as a constructor call (`new image.Image(dst)`, CS0144 ×2).
	// Gated to interface targets: widening every target reroutes `unsafe.Pointer(ptr)`
	// constructor forms into the conversion arm (13-project churn).
	convertible := types.ConvertibleTo(argType, targetType)

	if !convertible && originalArgType != nil {
		if targetIsIface, _ := isInterface(targetType); targetIsIface {
			convertible = types.ConvertibleTo(originalArgType, targetType)
		}
	}

	return convertible, typeName
}

func (v *Visitor) needsParentheses(expr ast.Expr) bool {
	switch expr.(type) {
	case *ast.Ident:
		return false
	case *ast.CallExpr:
		return false
	case *ast.SelectorExpr:
		return false
	case *ast.BasicLit:
		return false
	case *ast.CompositeLit:
		return false
	case *ast.ParenExpr:
		return false
	case *ast.UnaryExpr:
		// Unary expressions like -x or !x need parentheses
		return true
	case *ast.BinaryExpr:
		// Binary expressions like x + y need parentheses
		return true
	case *ast.IndexExpr:
		// Array/slice indexing like arr[i] doesn't need parentheses
		return false
	default:
		// For any other expression types, err on the side of caution
		return true
	}
}

func (v *Visitor) isConstructorCall(callExpr *ast.CallExpr) bool {
	// Get the object associated with the function being called.
	//
	// ast.Unparen: Go's PARENTHESIZED conversion spelling `(T)(x)` puts an *ast.ParenExpr in
	// Fun, which fell to the default arm below and reported "not a constructor" for what is
	// plainly a type callee. `(unsafe.Pointer)(new(int))` then took a different route from the
	// identical `unsafe.Pointer(new(int))` and emitted an uncompilable cast. The two spellings
	// are one Go program and must convert alike — the same rule this file already applies to the
	// two IIFE spellings. Unwrapping cannot widen the answer beyond that: `(*T)(x)` and
	// `(func())(nil)` unwrap to *ast.StarExpr / *ast.FuncType, which still hit the default arm.
	var obj types.Object

	switch funExpr := ast.Unparen(callExpr.Fun).(type) {
	case *ast.Ident:
		obj = v.info.ObjectOf(funExpr)
	case *ast.SelectorExpr:
		obj = v.info.ObjectOf(funExpr.Sel)
	default:
		return false
	}

	if obj == nil {
		return false
	}

	// Determine if the object is a type name
	switch obj.(type) {
	case *types.TypeName:
		// The function being called is a type (constructor call)
		return true
	case *types.Builtin:
		// Built-in functions like len, cap, etc.
		return false
	case *types.Func, *types.Var:
		// Regular functions or variables of function type
		return false
	default:
		return false
	}
}

func (v *Visitor) addImplicitSubStructConversions(sourceType types.Type, targetTypeName string, indirect bool) {
	if subStructTypes, exists := v.subStructTypes[sourceType]; exists {
		for _, subStructType := range subStructTypes {
			// Check if subStructType is a pointer
			if ptrType, ok := subStructType.(*types.Pointer); ok {
				subStructType = ptrType.Elem()
			}

			subStructTypeName := v.getCSharpTypeName(subStructType)
			sourceTypeName := getRootSubStructName(subStructTypeName)

			if strings.HasSuffix(targetTypeName, ">") {
				targetTypeName = fmt.Sprintf("%s_%s>", targetTypeName[:len(targetTypeName)-1], sourceTypeName)
			} else {
				targetTypeName = fmt.Sprintf("%s_%s", targetTypeName, sourceTypeName)
			}

			// Recursively add implicit conversions for sub-structs
			v.addImplicitSubStructConversions(subStructType, targetTypeName, indirect)

			var targetConversionsMap map[string]HashSet[string]

			if indirect {
				targetConversionsMap = indirectImplicitConversions
			} else {
				targetConversionsMap = implicitConversions
			}

			var conversions HashSet[string]
			var exists bool

			packageLock.Lock()

			if conversions, exists = targetConversionsMap[subStructTypeName]; exists {
				conversions.Add(targetTypeName)
			} else {
				conversions = NewHashSet([]string{targetTypeName})
				targetConversionsMap[subStructTypeName] = conversions
			}

			packageLock.Unlock()
		}
	}
}

// typeContainsTypeParams reports whether t contains any unbound type parameter, walking
// pointers, containers, and generic instantiations' type arguments.
func typeContainsTypeParams(t types.Type) bool {
	switch typ := t.(type) {
	case *types.TypeParam:
		return true
	case *types.Alias:
		return typeContainsTypeParams(types.Unalias(typ))
	case *types.Pointer:
		return typeContainsTypeParams(typ.Elem())
	case *types.Slice:
		return typeContainsTypeParams(typ.Elem())
	case *types.Array:
		return typeContainsTypeParams(typ.Elem())
	case *types.Chan:
		return typeContainsTypeParams(typ.Elem())
	case *types.Map:
		return typeContainsTypeParams(typ.Key()) || typeContainsTypeParams(typ.Elem())
	case *types.Named:
		if args := typ.TypeArgs(); args != nil {
			for i := range args.Len() {
				if typeContainsTypeParams(args.At(i)) {
					return true
				}
			}
		}
	}

	return false
}

// signatureResultsContainTypeParams reports whether any RESULT of sig is (or contains) an unbound
// type parameter. Used to detect a func-literal argument that C# must infer a generic callee's type
// argument FROM — see CallExprContext.genericResultInferredFuncArgs.
func signatureResultsContainTypeParams(sig *types.Signature) bool {
	results := sig.Results()

	if results == nil {
		return false
	}

	for i := range results.Len() {
		if typeContainsTypeParams(results.At(i).Type()) {
			return true
		}
	}

	return false
}

func getRootSubStructName(subStructName string) string {
	// Get text beyond last underscore
	lastUnderscoreIndex := strings.LastIndex(subStructName, "_")

	if lastUnderscoreIndex == -1 {
		return subStructName
	}

	return subStructName[lastUnderscoreIndex+1:]
}

func getShortFileName(fileToken *token.File) string {
	if fileToken == nil {
		return ""
	}

	return filepath.Base(fileToken.Name())
}

func (v *Visitor) getFunctionSignature(callExpr *ast.CallExpr) *types.Signature {
	switch fun := callExpr.Fun.(type) {
	case *ast.Ident:
		// Simple identifiers
		obj := v.info.Uses[fun]
		if obj == nil {
			return nil
		}

		if fn, ok := obj.(*types.Func); ok {
			return fn.Type().(*types.Signature)
		}

		if vr, ok := obj.(*types.Var); ok {
			if sig, ok := vr.Type().(*types.Signature); ok {
				return sig
			}
		}

	case *ast.SelectorExpr:
		// Qualified identifiers and method calls
		sel, ok := v.info.Selections[fun]
		if ok {
			// Method call
			if fn, ok := sel.Obj().(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}

			// A FUNC-typed FIELD callee — `d.fill(d, b)` on flate compressor's
			// method-expression fields: its signature drives the same per-argument
			// treatment (the receiver arg must render as the box for a ж<T> slot,
			// CS1503 ×5). Underlying looks through a NAMED func type.
			if vr, ok := sel.Obj().(*types.Var); ok {
				if sig, ok := vr.Type().Underlying().(*types.Signature); ok {
					return sig
				}
			}

			return nil
		}

		// Package-qualified function
		if _, ok := fun.X.(*ast.Ident); ok {
			obj := v.info.Uses[fun.Sel]
			if obj == nil {
				return nil
			}

			if fn, ok := obj.(*types.Func); ok {
				return fn.Type().(*types.Signature)
			}

			if vr, ok := obj.(*types.Var); ok {
				if sig, ok := vr.Type().(*types.Signature); ok {
					return sig
				}
			}
		}

	case *ast.ParenExpr:
		// Handle parenthesized expressions like (pkg.Func)()
		return v.getFunctionSignature(&ast.CallExpr{Fun: fun.X})

	case *ast.CallExpr:
		// Functions returned by other functions. Underlying() looks through a NAMED func-type
		// result — encoding/json's `valueEncoder(v)` returns `encoderFunc` (a methodless named
		// func type), so a bare `t.(*types.Signature)` failed and the per-argument pointer
		// treatment never fired: `valueEncoder(v)(e, …)` passed the receiver value alias `e`
		// where the `ж<encodeState>` slot wanted the box `Ꮡe` (CS1503).
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.Underlying().(*types.Signature); ok {
				return sig
			}
		}

	case *ast.FuncLit:
		// Anonymous functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexExpr:
		// Generic function instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.IndexListExpr:
		// Multiple type parameter instantiations
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.TypeAssertExpr:
		// Type assertions: (x.(T))()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.StarExpr:
		// Dereferencing a function pointer: (*fnPtr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.UnaryExpr:
		// Unary expressions like (*ptr)()
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.BinaryExpr:
		// Binary expressions that somehow evaluate to functions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.CompositeLit:
		// Composite literals that are callable
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}

	case *ast.ArrayType, *ast.ChanType, *ast.FuncType, *ast.InterfaceType,
		*ast.MapType, *ast.StructType:
		// Type expressions
		if t := v.info.TypeOf(fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Handle type conversion cases that return callable functions
	// This covers cases like (*byte)(unsafe.Pointer(...))
	if callExpr, ok := callExpr.Fun.(*ast.CallExpr); ok {
		if fun, ok := callExpr.Fun.(*ast.ParenExpr); ok {
			if t := v.info.TypeOf(fun.X); t != nil {
				if sig, ok := t.(*types.Signature); ok {
					return sig
				}
			}
		}

		// Handle types directly
		if t := v.info.TypeOf(callExpr.Fun); t != nil {
			if sig, ok := t.(*types.Signature); ok {
				return sig
			}
		}
	}

	// Final general fallback - this should catch most remaining cases
	if t := v.info.TypeOf(callExpr.Fun); t != nil {
		if sig, ok := t.(*types.Signature); ok {
			return sig
		}

		if sig, ok := t.Underlying().(*types.Signature); ok {
			return sig
		}
	}

	resultType := v.info.TypeOf(callExpr)

	if resultType != nil && strings.Contains(resultType.String(), "unsafe.Pointer") {
		pkg := types.NewPackage("unsafe", "unsafe")

		// Only concerned with making the parameter a "pointer like" type
		uintptrType := types.Typ[types.Uintptr]
		params := types.NewTuple(types.NewParam(token.NoPos, pkg, "", types.NewPointer(uintptrType)))

		return types.NewSignatureType(nil, nil, nil, params, nil, false)
	}

	return nil
}

// getCallFunIdent returns the NAME identifier of a called function — the ident itself, a
// selector's Sel, or the peeled base of an explicit instantiation — the key go/types uses
// for info.Instances.
func getCallFunIdent(fun ast.Expr) *ast.Ident {
	switch e := fun.(type) {
	case *ast.Ident:
		return e
	case *ast.SelectorExpr:
		return e.Sel
	case *ast.ParenExpr:
		return getCallFunIdent(e.X)
	case *ast.IndexExpr:
		return getCallFunIdent(e.X)
	case *ast.IndexListExpr:
		return getCallFunIdent(e.X)
	}

	return nil
}

// calleeHasConstraintOnlyTypeParam reports whether the called generic function declares a type
// parameter that appears in NO parameter type — visible only in constraints (`Twice[S ~[]E, E
// Integer](s S)`: E). Go infers such a parameter through core types; C# cannot infer it from
// arguments at ANY call site (concrete included), so those calls need explicit type arguments.
// callHasMethodGroupArg reports whether any argument of the call is a bare FUNCTION reference
// (a method group in C#) rather than a func-typed value or a lambda. C# cannot infer a
// generic method's type arguments through a method-group argument (the group has no single
// type until the target delegate is known), so a generic call carrying one must spell its
// type arguments out — `slices.SortFunc(l, bytes.Compare)` was CS0411 (encoding/asn1). A
// func-typed VARIABLE (a *types.Var) is a real delegate value and infers fine, so it is
// excluded; only a *types.Func (package function or method) counts.
func (v *Visitor) callHasMethodGroupArg(callExpr *ast.CallExpr) bool {
	for _, arg := range callExpr.Args {
		if v.exprIsMethodGroup(arg) {
			return true
		}
	}

	return false
}

// calleeTypeParamUnsuppliedByCall reports whether the CALL leaves a declared type parameter with no
// argument to infer it from. Every non-variadic parameter is always supplied in a well-typed Go
// call, so the only way a position goes unsupplied is a VARIADIC parameter that received nothing:
// `slices.Insert(s, 0)` against `Insert[S ~[]E, E any](s S, i int, v ...E) S` hands C# an empty
// `params Span<E>` and E is inferable from nothing (CS0411, cascading into CS1503 as the wrong
// overload binds). Go infers E through S's core type; C# has no such rule.
//
// calleeHasConstraintOnlyTypeParam cannot see this one — E DOES appear in a parameter type — and
// the predicate is deliberately about the ARGUMENTS rather than about variadics as such, because
// "the parameter positions this call actually supplied" is the property C# inference works from.
// It happens to be able to fire on nothing else, which is what holds the emission footprint to
// exactly this shape: a call that passes at least one variadic value, or forwards a slice with
// `...`, keeps its bare form.
func (v *Visitor) calleeTypeParamUnsuppliedByCall(callExpr *ast.CallExpr, funIdent *ast.Ident) bool {
	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil || sig.TypeParams().Len() == 0 || !sig.Variadic() {
		return false
	}

	variadicIndex := sig.Params().Len() - 1

	// A forwarded slice (`f(xs...)`) supplies the position, as does any value in it.
	if callExpr.Ellipsis != token.NoPos || len(callExpr.Args) > variadicIndex {
		return false
	}

	for i := range sig.TypeParams().Len() {
		tp := sig.TypeParams().At(i)
		supplied := false

		for j := 0; j < variadicIndex; j++ {
			if typeUsesTypeParam(sig.Params().At(j).Type(), tp) {
				supplied = true
				break
			}
		}

		if !supplied {
			return true
		}
	}

	return false
}

func (v *Visitor) calleeHasConstraintOnlyTypeParam(funIdent *ast.Ident) bool {
	funcObj, ok := v.info.ObjectOf(funIdent).(*types.Func)

	if !ok {
		return false
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.TypeParams() == nil {
		return false
	}

	for i := range sig.TypeParams().Len() {
		tp := sig.TypeParams().At(i)
		found := false

		for j := range sig.Params().Len() {
			if typeUsesTypeParam(sig.Params().At(j).Type(), tp) {
				found = true
				break
			}
		}

		if !found {
			return true
		}
	}

	return false
}

// typeUsesTypeParam reports whether t structurally contains the SPECIFIC type parameter tp.
func typeUsesTypeParam(t types.Type, tp *types.TypeParam) bool {
	switch tt := t.(type) {
	case *types.TypeParam:
		return tt == tp
	case *types.Slice:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Array:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Pointer:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Map:
		return typeUsesTypeParam(tt.Key(), tp) || typeUsesTypeParam(tt.Elem(), tp)
	case *types.Chan:
		return typeUsesTypeParam(tt.Elem(), tp)
	case *types.Signature:
		params := tt.Params()
		for i := range params.Len() {
			if typeUsesTypeParam(params.At(i).Type(), tp) {
				return true
			}
		}
		results := tt.Results()
		for i := range results.Len() {
			if typeUsesTypeParam(results.At(i).Type(), tp) {
				return true
			}
		}
	case *types.Named:
		if args := tt.TypeArgs(); args != nil {
			for i := range args.Len() {
				if typeUsesTypeParam(args.At(i), tp) {
					return true
				}
			}
		}
	}

	return false
}

// stripTrailingTypeArgs removes a trailing balanced <...> type-argument group from a rendered
// function name (`Grow<S>` -> `Grow`, `F<slice<E>>` -> `F`); a name without one is unchanged.
func stripTrailingTypeArgs(funcName string) string {
	if !strings.HasSuffix(funcName, ">") {
		return funcName
	}

	depth := 0

	for i := len(funcName) - 1; i >= 0; i-- {
		switch funcName[i] {
		case '>':
			depth++
		case '<':
			depth--
			if depth == 0 {
				return funcName[:i]
			}
		}
	}

	return funcName
}

// instantiatedParamIsPointer reports whether a parameter DECLARED as a type parameter is a
// POINTER at this call's instantiation — `abi.Escape(ptr)` where `Escape[T any](x T) T` and
// T=*T (internal/weak Make): the argument must render as its box (`Ꮡptr`), not the deref'd
// value alias (`ptr`), or the result cannot assign back to the pointer (CS0029 T → ж<T>).
// getFunctionSignature returns the DECLARED generic signature; go/types records the
// INSTANTIATED one as the type of the call's Fun expression.
func (v *Visitor) instantiatedParamIsPointer(callExpr *ast.CallExpr, declaredParam types.Type, i int) bool {
	if _, isTP := types.Unalias(declaredParam).(*types.TypeParam); !isTP {
		return false
	}

	// The Fun expression's recorded type can be the UNINSTANTIATED signature — its param
	// still the bare type parameter — with the instantiation living in info.Instances,
	// keyed by the Fun's identifier (`ptr = abi.Escape(ptr)` instantiating T = *T left the
	// arg as the deref'd value alias, internal/weak Make CS0029).
	funIdent, _ := callExpr.Fun.(*ast.Ident)

	if sel, ok := callExpr.Fun.(*ast.SelectorExpr); ok {
		funIdent = sel.Sel
	} else if idx, ok := callExpr.Fun.(*ast.IndexExpr); ok {
		// Explicitly instantiated form: escape[*int](p) / pkg.Escape[*int](p)
		if id, ok := idx.X.(*ast.Ident); ok {
			funIdent = id
		} else if sel, ok := idx.X.(*ast.SelectorExpr); ok {
			funIdent = sel.Sel
		}
	}

	if funIdent != nil {
		if inst, ok := v.info.Instances[funIdent]; ok {
			if sig, ok := inst.Type.(*types.Signature); ok {
				if paramType, ok := getParameterType(sig, i); ok {
					return isPointer(paramType)
				}
			}
		}
	}

	instSig, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature)

	if !ok {
		return false
	}

	if paramType, ok := getParameterType(instSig, i); ok {
		return isPointer(paramType)
	}

	return false
}

// instantiatedParamType returns the INSTANTIATED type of parameter i at this call site, or nil
// when unavailable. getFunctionSignature returns the DECLARED generic signature (its params are
// still bare type parameters); the instantiation lives in info.Instances keyed by the Fun's
// identifier — the same resolution instantiatedParamIsPointer performs (see its notes), factored
// for callers that need the full type rather than a pointer check.
func (v *Visitor) instantiatedParamType(callExpr *ast.CallExpr, i int) types.Type {
	funIdent, _ := callExpr.Fun.(*ast.Ident)

	if sel, ok := callExpr.Fun.(*ast.SelectorExpr); ok {
		funIdent = sel.Sel
	} else if idx, ok := callExpr.Fun.(*ast.IndexExpr); ok {
		// Explicitly instantiated form: walkList[*Ident](v, list) / pkg.F[*T](x)
		if id, ok := idx.X.(*ast.Ident); ok {
			funIdent = id
		} else if sel, ok := idx.X.(*ast.SelectorExpr); ok {
			funIdent = sel.Sel
		}
	}

	if funIdent != nil {
		if inst, ok := v.info.Instances[funIdent]; ok {
			if sig, ok := inst.Type.(*types.Signature); ok {
				if paramType, ok := getParameterType(sig, i); ok {
					return paramType
				}
			}
		}
	}

	if instSig, ok := v.info.TypeOf(callExpr.Fun).(*types.Signature); ok {
		if paramType, ok := getParameterType(instSig, i); ok {
			return paramType
		}
	}

	return nil
}

// unsafeConstBuiltinName reports which of Go's constant-valued `unsafe` builtins a call names —
// `Sizeof`, `Alignof` or `Offsetof` — or "" for anything else. It resolves through go/types rather
// than through the converted call text, so a renamed `unsafe` import (`import u "unsafe"`) resolves
// identically; go/types models all three as `*types.Builtin` objects owned by package `unsafe`.
func (v *Visitor) unsafeConstBuiltinName(callExpr *ast.CallExpr) string {
	selectorExpr, isSelector := callExpr.Fun.(*ast.SelectorExpr)

	if !isSelector {
		return ""
	}

	builtin, isBuiltin := v.info.Uses[selectorExpr.Sel].(*types.Builtin)

	if !isBuiltin || builtin.Pkg() != types.Unsafe {
		return ""
	}

	switch builtin.Name() {
	case "Sizeof", "Alignof", "Offsetof":
		return builtin.Name()
	}

	return ""
}

// foldUnsafeConstBuiltin renders the constant go/types computed for an `unsafe.Sizeof` /
// `unsafe.Alignof` / `unsafe.Offsetof` call as the emitted literal, with the Go expression kept as
// a comment — the declaration-site convention. The value is GO's: go/types folds these against the
// `types.Sizes` for the loaded target GOARCH, the same layout rules the Go compiler applies, so the
// emitted number matches what the Go program would compute rather than what the CLR would measure.
// Reports false when go/types has no integer constant for the call (a variable-size operand), which
// leaves the caller on the run-time form.
//
// The literal carries the call's own type (`uintptr` — Go fixes the result type of all three). Go's
// constant is TYPED, and a bare C# number is not: `uadd := unsafe.Sizeof(*t)` declares a `uintptr`
// in Go but would infer `int` from `var uadd = 56`, and an `int` VARIABLE has no implicit conversion
// back to `nuint` — `addChecked(ptr, uadd, …)` is then CS1503 (internal/abi's `FuncType.InSlice`).
// Elsewhere the cast is inert: C#'s constant-expression conversion would have bound a bare literal
// to `nuint` anyway, and a cast binds tighter than every binary operator, so no site needs parens.
func (v *Visitor) foldUnsafeConstBuiltin(callExpr *ast.CallExpr) (string, bool) {
	tv, ok := v.info.Types[callExpr]

	if !ok || tv.Value == nil || tv.Value.Kind() != constant.Int {
		return "", false
	}

	value, exact := constant.Uint64Val(tv.Value)

	if !exact {
		return "", false
	}

	csTypeName := v.getCSharpTypeName(tv.Type)

	if len(csTypeName) == 0 {
		csTypeName = "uintptr"
	}

	return fmt.Sprintf("/* %s */ (%s)%d", strings.TrimSpace(v.getPrintedNode(callExpr)), csTypeName, value), true
}

// unsafeFieldOperand resolves an `unsafe.Offsetof` operand — Go requires the form
// `structValue.field`, possibly reached through pointers and embedded fields — to the struct type
// that DECLARES the field, plus the field's emitted C# name. Offsetof is relative to the
// immediately enclosing struct, so a PROMOTED field is walked down its embedding chain and the
// type returned is the one the field is actually declared in. The name is the emitted identifier
// with any keyword escape removed, because reflection sees `@out` as `out`.
func (v *Visitor) unsafeFieldOperand(arg ast.Expr) (types.Type, string, bool) {
	selectorExpr, isSelector := arg.(*ast.SelectorExpr)

	if !isSelector {
		return nil, "", false
	}

	selection := v.info.Selections[selectorExpr]

	if selection == nil || selection.Kind() != types.FieldVal {
		return nil, "", false
	}

	structType := unsafeOperandStructType(selection.Recv())
	index := selection.Index()

	for _, embedded := range index[:len(index)-1] {
		structUnder, isStruct := structType.Underlying().(*types.Struct)

		if !isStruct || embedded >= structUnder.NumFields() {
			return nil, "", false
		}

		structType = unsafeOperandStructType(structUnder.Field(embedded).Type())
	}

	if _, isStruct := structType.Underlying().(*types.Struct); !isStruct {
		return nil, "", false
	}

	return structType, strings.TrimPrefix(getSanitizedIdentifier(selection.Obj().Name()), "@"), true
}

// unsafeOperandStructType strips the implicit dereference a Go field selection performs through a
// pointer, so the reported type is the struct itself.
func unsafeOperandStructType(t types.Type) types.Type {
	if t == nil {
		return nil
	}

	if pointer, isPointer := t.Underlying().(*types.Pointer); isPointer {
		return pointer.Elem()
	}

	return t
}

// managedAtomicPointerIdiom recognizes Go's lock-free managed-pointer-field atomics —
//
//	atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))
//	atomic.StorePointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)), unsafe.Pointer(v))
//
// where `x.field` has a pointer type `*T` and so, in the managed conversion, holds a `ж<T>`
// reference. A managed reference cannot survive the `uintptr` round-trip the literal conversion
// emits (the pinned address is transient and reinterpreting it loses GC identity), so the
// converter routes these to the golib overloads that operate on the field BOX (`ж<ж<T>>`)
// directly. Returns the `&x.field` address expression (which renders as that box), and, for a
// store, the stored value expression `v` (unwrapped from its `unsafe.Pointer(...)` conversion so
// it renders as the plain `ж<T>` the overload takes).
func (v *Visitor) managedAtomicPointerIdiom(callExpr *ast.CallExpr) (addrExpr ast.Expr, storeVal ast.Expr, isLoad bool, ok bool) {
	sel, isSel := callExpr.Fun.(*ast.SelectorExpr)

	if !isSel {
		return nil, nil, false, false
	}

	fn, isFn := v.info.ObjectOf(sel.Sel).(*types.Func)

	if !isFn || fn.Pkg() == nil || fn.Pkg().Path() != "sync/atomic" {
		return nil, nil, false, false
	}

	switch fn.Name() {
	case "LoadPointer":
		isLoad = true
	case "StorePointer":
		isLoad = false
	default:
		return nil, nil, false, false
	}

	// arg[0] must be `(*unsafe.Pointer)(unsafe.Pointer(&x.field))` whose `x.field` is `*T`.
	if len(callExpr.Args) < 1 {
		return nil, nil, false, false
	}

	addrExpr = v.unwrapManagedPtrFieldAddress(callExpr.Args[0])

	if addrExpr == nil {
		return nil, nil, false, false
	}

	if isLoad {
		return addrExpr, nil, true, len(callExpr.Args) == 1
	}

	// StorePointer's value arg is `unsafe.Pointer(v)`; the overload takes the plain `ж<T>` value.
	if len(callExpr.Args) != 2 {
		return nil, nil, false, false
	}

	storeVal = v.unwrapUnsafePointerConversion(callExpr.Args[1])

	if storeVal == nil {
		return nil, nil, false, false
	}

	return addrExpr, storeVal, false, true
}

// unwrapManagedPtrFieldAddress returns the `&Z` expression inside
// `(*unsafe.Pointer)(unsafe.Pointer(&Z))` when `Z` has a pointer type `*T` (a managed `ж<T>`
// field); nil otherwise.
func (v *Visitor) unwrapManagedPtrFieldAddress(arg ast.Expr) ast.Expr {
	// (*unsafe.Pointer)(inner)
	outer, isCall := ast.Unparen(arg).(*ast.CallExpr)

	if !isCall || len(outer.Args) != 1 || !v.isPointerToUnsafePointerType(outer.Fun) {
		return nil
	}

	// unsafe.Pointer(&Z)
	inner := v.unwrapUnsafePointerConversion(outer.Args[0])

	if inner == nil {
		return nil
	}

	// &Z
	unary, isUnary := ast.Unparen(inner).(*ast.UnaryExpr)

	if !isUnary || unary.Op != token.AND {
		return nil
	}

	// Z must have a pointer type — its address `&Z` is then `**T`, i.e. the box of a `ж<T>` field.
	operandType := v.info.TypeOf(unary.X)

	if operandType == nil {
		return nil
	}

	if _, isPointer := operandType.Underlying().(*types.Pointer); !isPointer {
		return nil
	}

	return unary
}

// markDeadUnsafePointerBox records that the `unsafe.Pointer(x)` conversion feeding convExpr's
// enclosing `uintptr(…)` conversion may be emitted WITHOUT its wrapper object.
//
// Go's most common syscall idiom, `uintptr(unsafe.Pointer(x))`, converted to
// `(uintptr)new @unsafe.Pointer(x)` — and that object is provably dead. golib's Pointer is a
// `ж<uintptr>` whose only value-taking constructor takes a `uintptr`, so the operand is ALREADY
// converted by `implicit operator uintptr(…)` before the wrapper exists; the wrapper stores that
// number in its own one-element slot, and the enclosing cast reads it straight back out
// (`uintptr(Pointer) => value.IsNull ? 0 : value.Value`, and the constructor marks the box nil
// exactly when the address is 0 — so the round-trip is the identity). Dropping it emits `(uintptr)x`:
// the same operator on the same operand, one fewer allocation per site. In the zsyscall wrappers,
// where every pointer argument is spelled this way, that is three allocations off a single call.
//
// THE PIN IS NOT AFFECTED — the one semantic that a wrapper elision could plausibly have broken.
// `implicit operator uintptr(ж<T>)` pins the ROOT storage behind the OPERAND box, for that box's
// lifetime (`EnsureStableAddress` / `pinnedArrayData` set `m_pin` on the operand, released when the
// operand is collected). The wrapper owns no pin, holds no reference to the operand and tracks no
// lifetime — its `ж<uintptr>` slot holds the finished address as a number. So the address handed to
// native code, and how long the storage behind it is held still, are identical either way.
//
// Only the conversion's own emission changes, and only where the wrapper was going to be built with
// `new`: the arms that render a raw address by other means — `@unsafe.Pointer.FromRef(ref x)` for a
// deref-aliased pointer receiver, and the `((@unsafe.Pointer)(uintptr)v)` cast hop for a named
// uintptr/pointer operand — never consult the mark and are unchanged. The mark is keyed on the inner
// CallExpr node, so it can only ever apply to the conversion this uintptr cast actually wraps.
func (v *Visitor) markDeadUnsafePointerBox(callExpr *ast.CallExpr, arg ast.Expr) {
	// The enclosing conversion must RENDER a `(uintptr)` cast around the operand — the basic
	// `uintptr(…)` target, and the named-over-uintptr target (`Handle(…)`, security_windows'
	// LocalFree defers), which hops through the underlying for the same reason.
	targetType := v.info.TypeOf(callExpr)

	if targetType == nil {
		return
	}

	basic, isBasic := targetType.Underlying().(*types.Basic)

	if !isBasic || basic.Kind() != types.Uintptr {
		return
	}

	innerCall, isCall := ast.Unparen(arg).(*ast.CallExpr)

	if !isCall || len(innerCall.Args) != 1 || !v.callExprIsTypeConversion(innerCall) || !v.isUnsafePointerType(innerCall.Fun) {
		return
	}

	if v.deadUnsafePointerBoxes == nil {
		v.deadUnsafePointerBoxes = map[*ast.CallExpr]bool{}
	}

	v.deadUnsafePointerBoxes[innerCall] = true
}

// unsafePointerBoxEmission renders an `unsafe.Pointer(x)` conversion whose operand has already been
// converted to operandExpr — as the wrapper construction, or as the bare operand when the wrapper is
// dead (see markDeadUnsafePointerBox). The operand keeps the file's usual parenthesization rule: a
// primary expression stands alone under the enclosing `(uintptr)` cast, while one that binds looser
// than a cast (`unsafe.Pointer(uintptr(p) + off)`) is wrapped so the cast still applies to all of it.
func (v *Visitor) unsafePointerBoxEmission(callExpr *ast.CallExpr, arg ast.Expr, operandExpr string) string {
	if !v.deadUnsafePointerBoxes[callExpr] {
		// A POINTER operand renders as the managed box `ж<T>`, and `new @unsafe.Pointer(box)` binds
		// the implicit `ж<T> → uintptr` conversion into `Pointer(uintptr)` — which PINS the storage (the
		// conversion is the pin moment: EnsureStableAddress stores a GCHandle in the box's own field)
		// and then retains NOTHING, so the box carrying that pin is unreachable garbage the instant the
		// mint returns, and its finalizer frees the pin while the address is still in flight. Go has no
		// equivalent hazard here: its heap does not move, so a live pointer is a stable address.
		//
		// Mint through the RETAINING door instead (`@unsafe.Pointer.FromPinnedBox`, golib unsafe.cs),
		// which takes the same address from the same conversion and keeps the box. Measured 2026-09-04:
		// sixteen concurrent TLS connections over the converted stack died SIGSEGV in five seconds, 3/3,
		// and stopped crashing once the box was held across the call.
		//
		// Only a genuine POINTER operand takes this door: a `uintptr` or `unsafe.Pointer` operand is a
		// number rather than a box, and `Pointer(uintptr)` is the right and only mint for it.
		if _, isPtr := types.Unalias(v.info.TypeOf(arg)).(*types.Pointer); isPtr {
			return fmt.Sprintf("@unsafe.Pointer.FromPinnedBox(%s)", operandExpr)
		}

		return fmt.Sprintf("new @unsafe.Pointer(%s)", operandExpr)
	}

	// An ADDRESS-OF operand is the exception among the shapes needsParentheses rejects: `&x` renders
	// as the primary box form (`Ꮡx`, `Ꮡ(…)`, `Ꮡs.at<T>(i)`), which the cast already binds to whole.
	if unary, isUnary := arg.(*ast.UnaryExpr); isUnary && unary.Op == token.AND {
		return operandExpr
	}

	if v.needsParentheses(arg) {
		return fmt.Sprintf("(%s)", operandExpr)
	}

	return operandExpr
}

// opaquePointerMintEmission renders golib's referent-preserving mint for the conversion
// `T(unsafe.Pointer(p))` where T's underlying type is `*struct{}` — the OPAQUE pointer form
// Windows type definitions use for a "pointer to one of many types" field (syscall's
// `type Pointer *struct{}`) — and p is a Go pointer whose box is statically in hand.
//
// The numeric chain this replaces — `(T)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer(p))` —
// projects p's box to a scalar at the @unsafe.Pointer constructor, and for a pointee CARRYING
// MANAGED REFERENCES that scalar is a transient GC-heap address with no recoverable box behind it:
// golib's uintptr operator pins only reference-free storage (ж.cs, EnsureStableAddress). The
// measured victim is crypto/x509's checkChainSSLServerPolicy, whose
// SSL_EXTRA_CERT_CHAIN_POLICY_PARA — ServerName itself a pointer — crossed into
// CertVerifyCertificateChainPolicy as exactly such an address: an ACCESS_VIOLATION inside
// Syscall6, and the one mint-site standing between crypto/tls and a roster row (BOARD
// "THE CRYPTOAPI CHAIN WALL IS DOWN", 2026-08-18). Guarded by the SystemCertVerify behavioral
// test's policy rows, which drive the same mint from test code — which is why the remedy is this
// emission rather than a hand-own of the one x509 function: a hand-own would leave every OTHER
// author of the same Go shape, converted test suites included, minting the same lost pointer.
//
// golib's ManagedPointerTokens.MintOpaque keeps the numeric route byte for byte for every pointee
// that route already answered exactly (nil → 0, native → its address, reference-free → pinned
// stable storage) and diverges only for the reference-bearing class: the scalar becomes the box's
// own pointer-order token, registered so the consuming boundary wrapper
// (zsyscall_windows_certchain_impl.cs) recovers the referent with Resolve, and the minted box
// holds the referent reachable for its own lifetime — the emitted mint's referent is otherwise
// reachable only through a local the JIT is free to retire before the syscall that consumes it.
//
// Scope: the target's pointee must be the EMPTY struct — `*struct{}` names an opaque pointer BY
// CONSTRUCTION (there is nothing to dereference), so no reader can depend on the scalar being a
// dereferenceable address. Wider named-pointer targets keep their existing routes. A source that
// is not `unsafe.Pointer(p)`-over-a-pointer (a uintptr, a stored unsafe.Pointer variable — the
// referent is already gone there) keeps the numeric chain, as does a pointer the emitter cannot
// render as a box (a deref-aliased receiver).
func (v *Visitor) opaquePointerMintEmission(callExpr *ast.CallExpr, arg ast.Expr, targetTypeName string) (string, bool) {
	named, ok := types.Unalias(v.info.TypeOf(callExpr)).(*types.Named)

	if !ok {
		return "", false
	}

	ptrUnder, ok := named.Underlying().(*types.Pointer)

	if !ok {
		return "", false
	}

	structElem, ok := ptrUnder.Elem().Underlying().(*types.Struct)

	if !ok || !isEmptyStructType(structElem) {
		return "", false
	}

	inner := v.unwrapUnsafePointerConversion(arg)

	if inner == nil {
		return "", false
	}

	if _, isPtr := v.info.TypeOf(inner).(*types.Pointer); !isPtr {
		return "", false
	}

	// A deref-aliased pointer RECEIVER renders as the pointed-to value alias and has no box for
	// the mint to hold — the numeric chain is that shape's pre-existing (and only) route.
	if v.exprIsDerefAliasedPointer(inner) && !v.exprIsDerefdPointerParam(inner) {
		return "", false
	}

	identContext := DefaultIdentContext()
	identContext.isPointer = true

	return fmt.Sprintf("((%s)ManagedPointerTokens.MintOpaque(%s))", targetTypeName, v.convExpr(inner, []ExprContext{identContext})), true
}

// unwrapUnsafePointerConversion returns the argument of an `unsafe.Pointer(x)` conversion, or nil
// when expr is not such a conversion.
func (v *Visitor) unwrapUnsafePointerConversion(expr ast.Expr) ast.Expr {
	call, isCall := ast.Unparen(expr).(*ast.CallExpr)

	if !isCall || len(call.Args) != 1 || !v.isUnsafePointerType(call.Fun) {
		return nil
	}

	return call.Args[0]
}

// isUnsafePointerType reports whether expr denotes the `unsafe.Pointer` type (used as a conversion
// target).
func (v *Visitor) isUnsafePointerType(expr ast.Expr) bool {
	t := v.info.TypeOf(expr)

	if t == nil {
		return false
	}

	basic, isBasic := t.Underlying().(*types.Basic)

	return isBasic && basic.Kind() == types.UnsafePointer
}

// isPointerToUnsafePointerType reports whether expr denotes `*unsafe.Pointer` (the conversion
// target of the idiom's outer cast).
func (v *Visitor) isPointerToUnsafePointerType(expr ast.Expr) bool {
	star, isStar := ast.Unparen(expr).(*ast.StarExpr)

	return isStar && v.isUnsafePointerType(star.X)
}

// typeLiteralPointerTarget returns the StarExpr of a conversion target written as a pointer to a
// composite TYPE LITERAL — `(*[]byte)(…)`, `(*struct{ r7 int })(…)` — and nil for every other
// target shape (including the named forms `(*T)(…)` / `(*pkg.T)(…)`, which have a types.Object and
// resolve through the ordinary target-name path). Used by the typed-nil conversion rendering, whose
// element name must come from convStarExpr so an anonymous struct/interface element is lifted.
func typeLiteralPointerTarget(fun ast.Expr) *ast.StarExpr {
	for {
		parenExpr, ok := fun.(*ast.ParenExpr)

		if !ok {
			break
		}

		fun = parenExpr.X
	}

	starExpr, ok := fun.(*ast.StarExpr)

	if !ok {
		return nil
	}

	switch starExpr.X.(type) {
	case *ast.Ident, *ast.SelectorExpr:
		return nil
	}

	return starExpr
}

// isConstantStringConcat reports whether expr is Go's line-SPLITTING string idiom — a `+` chain of
// plain string LITERALS that go/types folds to one constant value. That is exactly the shape whose
// rendering is a concatenation of bare C# string literals (a `string`, not an `@string`), which the
// `[]byte`/`[]rune` conversion above must cast; parentheses are transparent.
//
// Every leaf must be a literal that renders plainly: a `\xHH` raw-byte literal takes convBasicLit's
// byte-ARRAY route, which already yields an `@string` and carries the whole concatenation with it
// (`"" + ((@string)(new byte[]{0xff, 0x80}))` — the ByteTableStringVar behavioral case), so such a
// chain needs no cast and keeps its emission byte-identical. A non-literal constant leaf (an
// ident/selector naming a string const) emits its declared symbol rather than a bare C# string and
// is likewise left alone.
func (v *Visitor) isConstantStringConcat(expr ast.Expr) bool {
	inner := unparenthesize(expr)

	if _, ok := inner.(*ast.BinaryExpr); !ok {
		return false
	}

	if value := v.info.Types[inner].Value; value == nil || value.Kind() != constant.String {
		return false
	}

	return plainStringLiteralLeaves(inner)
}

// plainStringLiteralLeaves reports whether every leaf of a `+` chain is a string literal that
// convBasicLit renders as a plain C# string literal. See isConstantStringConcat.
func plainStringLiteralLeaves(expr ast.Expr) bool {
	switch node := unparenthesize(expr).(type) {
	case *ast.BinaryExpr:
		return node.Op == token.ADD && plainStringLiteralLeaves(node.X) && plainStringLiteralLeaves(node.Y)
	case *ast.BasicLit:
		return node.Kind == token.STRING &&
			(strings.HasPrefix(node.Value, "`") || !stringLiteralNeedsByteArray(node.Value))
	}

	return false
}

// unparenthesize strips any parenthesization from an expression.
func unparenthesize(expr ast.Expr) ast.Expr {
	for {
		parenExpr, ok := expr.(*ast.ParenExpr)

		if !ok {
			return expr
		}

		expr = parenExpr.X
	}
}

// typeIsPrimitiveAlias reports whether a type is a Go ALIAS whose right-hand side is a primitive —
// `type _C_int = int`, `type _C_gid_t = uint32`. Such an alias emits as `global using _C_int = int`:
// a compile-time name for a BCL primitive, with no wrapper struct, no constructor and no `.Value`.
//
// It is the ONE definition of that shape, read by both places a GoImplicitConv record can go wrong
// over one — conversionRecordHasLocalOperand (as the record's HOST: `partial struct UInt32` on a
// primitive, CS1729) and the record-emission gate (as the record's SOURCE: `src.Value` on a
// primitive-named operand, CS0246). The second darwin census surfaced both shapes at once, because
// the cgo-flavor leaves declare their whole C-type mirror this way.
//
// A non-alias NAMED type over a primitive (`type Errno uintptr`) is deliberately NOT this shape: it
// emits as a real [GoType] wrapper with a constructor and a Value, which is precisely what the
// generator needs.
func typeIsPrimitiveAlias(t types.Type) bool {
	alias, isAlias := t.(*types.Alias)

	if !isAlias {
		return false
	}

	_, primitive := alias.Rhs().Underlying().(*types.Basic)

	return primitive
}

// exprHasCallOrReceive reports whether expr contains a function call or a channel receive.
//
// Go's `len`/`cap` of a pointer-to-array is a constant computed from the TYPE and does NOT evaluate
// its operand — except that function calls and channel receives inside the operand ARE evaluated
// (spec, "Length and capacity"). Folding those away would drop a side effect, so the caller keeps
// the evaluating emission for them and folds only the side-effect-free operands, which is every
// instance the corpus and reflect's tests actually contain.
func (v *Visitor) exprHasCallOrReceive(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		switch node := n.(type) {
		case *ast.CallExpr:
			// A CONVERSION is not a call — `len((*[4]byte)(p))` evaluates nothing. Only a call whose
			// callee is a value (function or builtin) counts.
			if ident, ok := node.Fun.(*ast.Ident); ok {
				if _, isType := v.info.ObjectOf(ident).(*types.TypeName); isType {
					return true
				}
			}

			found = true
			return false
		case *ast.UnaryExpr:
			if node.Op == token.ARROW {
				found = true
				return false
			}
		}

		return true
	})

	return found
}
