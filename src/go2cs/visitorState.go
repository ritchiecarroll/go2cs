// visitorState.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// This file owns the Visitor and the per-file state it carries.
//
// A Visitor is created per Go source file and walks that file's AST, appending C# to an output
// builder. Nearly every conv*/visit* file in the converter is a method on this type, so its field
// set is effectively the converter's working memory: what is being emitted, what has been
// discovered about the current function, and what the analysis passes recorded earlier.
//
// Only PER-FILE state lives here. State shared across a whole package — the registries the
// concurrently-visited files publish to each other — lives in packageGlobalState.go, whose
// lifecycle is owned by packageStateOperations.go.

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

type FileEntry struct {
	file             *ast.File
	filePath         string
	identEscapesHeap map[types.Object]bool

	// sstringEligible flags a `s := string(x)` / `var s = string(x)` string LOCAL that the escape
	// pass has proven may be emitted as a stack-only `sstring` (a zero-copy view over x's bytes)
	// instead of the heap `@string` — non-escaping, not returned, used only through safe reads, and
	// with no write to the conversion's source for the lifetime of the view. Keyed by the local's
	// types.Object. Computed per file (no cross-file sharing) in performEscapeAnalysis.
	sstringEligible map[types.Object]bool

	// ssliceEligible flags a variadic parameter whose uses are proven not to let its slice header
	// escape the function frame. Its params Span<T> prologue may therefore bind through the
	// stack-only sslice<T> instead of copying into the heap slice<T>. Keyed by the parameter's
	// types.Object and computed per file in performEscapeAnalysis.
	ssliceEligible map[types.Object]bool

	// sstringConvExprs flags the specific `string(x)` conversion CallExprs that must emit `(sstring)x`
	// (the zero-copy view) rather than `(@string)x` (the heap copy): the RHS of an eligible local
	// (above) and unnamed `string(x)` temporaries consumed within a comparison against a literal
	// (`string(buf) == "…"`), which never outlive the expression so are safe unconditionally. Keyed
	// by the *ast.CallExpr node.
	sstringConvExprs map[*ast.CallExpr]bool

	// manualConversion marks a file whose DESTINATION `.cs` is a hand-owned manual conversion
	// ([module: go.GoManualConversion]). The file is still fully analyzed and visited with the
	// rest of its package — its package-wide state contributions (anonymous-struct lifts,
	// package-var registrations, escape/addressed-global analysis, imports) must match an
	// unseeded conversion exactly, or sibling files emit corrupted — but its EMISSION is
	// redirected to the non-compiled `<name>.cs.auto` review sibling instead of overwriting
	// the hand-owned `<name>.cs`.
	manualConversion bool
	// emissionExcluded marks a `-tests` variant file the Phase-4D file-exclusion ruling drops from
	// emission entirely (an Example/Benchmark-only `_test.go`, selectCompileExcludedTestFiles). It
	// stays in the variant's analysis entry list — pkg.Syntax feeds the shared passes — but no C#
	// is ever written for it, so, like a manualConversion file, it must never CLAIM a hoisted
	// string-literal field: a claim would assign the declaration to a file that renders nothing,
	// leaving every other use of that literal referencing a name that does not exist (strings'
	// `"abc"` in ExampleClone did exactly this once 3-char slugs became hoistable — CS0103 across
	// reader_test/replace_test).
	emissionExcluded bool
}

// CapturedVarInfo tracks information about captured variables
type CapturedVarInfo struct {
	origIdent *ast.Ident // Original identifier
	copyIdent *ast.Ident // Temporary copy identifier
	varType   types.Type // Type of the variable
	used      bool       // Whether the capture has been used
}

// LambdaCapture handles analysis and tracking of captured variables
type LambdaCapture struct {
	capturedVars    map[*ast.Ident]*CapturedVarInfo  // Map of original idents to their capture info
	stmtCaptures    map[ast.Node]map[*ast.Ident]bool // Track which vars are captured by which stmt
	pendingCaptures map[string]*CapturedVarInfo      // Variables that need declarations before lambda

	currentLambdaVars map[string]string // Original var name to capture name tracking within current lambda

	// currentLambdaVarObjs records, per captured NAME, the types.Object of the OUTER variable that was
	// captured. currentLambdaVars maps by name, so a same-named variable DECLARED inside the lambda (an
	// `s := f(s)` self-shadow, where the inner `s` shadows the captured outer `s`) would otherwise be
	// renamed to the capture name too — conflating the two (`var sʗ3 = …(~sʗ3)…`, CS0841). The capture
	// name is applied only when an ident resolves to this exact captured object; a distinct inner binding
	// falls through to its own name.
	currentLambdaVarObjs map[string]types.Object

	// boxRefVars holds heap-boxed local variables whose address is taken inside a lambda. Such a
	// variable must NOT be snapshot-captured (the value copy loses the box, so writes through the
	// captured `&m` are lost — and the copy declaration is invalid in expression position, e.g. a
	// func literal passed as a call argument). Instead the lambda references the box directly: `&m`
	// emits `Ꮡm` (a capturable reference) and value uses emit `Ꮡm.Value` — the ref-local alias
	// `ref var m = ref Ꮡm.Value` itself can't be captured (CS8175). Keyed by the var's types.Object.
	boxRefVars map[types.Object]bool

	// Analysis phase tracking
	analysisInLambda  bool     // Currently analyzing a lambda
	currentLambda     ast.Node // Current lambda being analyzed
	detectingCaptures bool

	// Conversion phase tracking
	conversionInLambda bool     // Currently converting a lambda
	currentConversion  ast.Node // Current node being converted

	// conversionStack saves the conversion-phase fields above (plus the per-lambda var maps) on each
	// enterLambdaConversion so a NESTED lambda restores the ENCLOSING lambda's state on exit rather
	// than clobbering it. Without it, a nested func literal's exit reset conversionInLambda to false
	// (and nil'd the var maps), so every receiver/box reference in the enclosing lambda's body AFTER
	// the nested lambda rendered as the un-boxed ref-local — uncapturable inside a closure (CS8175;
	// database/sql (*Stmt).QueryContext read `s.cg` after the inner releaseConn closure).
	conversionStack []lambdaConversionState
}

// lambdaConversionState snapshots the conversion-phase LambdaCapture fields that
// enter/exitLambdaConversion mutate, so nested lambdas nest correctly (LIFO save/restore).
type lambdaConversionState struct {
	conversionInLambda   bool
	currentConversion    ast.Node
	currentLambdaVars    map[string]string
	currentLambdaVarObjs map[string]types.Object
}

type Visitor struct {
	fset               *token.FileSet
	pkg                *types.Package
	info               *types.Info
	file               *token.File
	outputBuilder      *strings.Builder
	standAloneComments map[token.Pos]string
	sortedCommentPos   []token.Pos
	processedComments  HashSet[token.Pos]
	newline            string
	indentLevel        int
	options            Options
	globalIdentNames   map[*ast.Ident]string // Global identifiers to adjusted names map
	globalScope        map[string]*types.Var // Global variable scope
	needsNoInlining    map[types.Object]bool // package-wide computeNoInliningClosure result — see callerInliningAnalysis.go

	// pendingSyscallKeepAlive names the temps a syscall-funnel call's pointer-derived arguments
	// were routed through (see convSyscallFunnelCall, syscallKeepAliveAnalysis.go) — populated
	// while converting the call expression itself, and drained by the enclosing statement
	// (visitAssignStmt/visitExprStmt) into a GC.KeepAlive call emitted right after it. Reset to
	// nil once drained; a non-empty leftover at the next statement would misattribute a keepalive
	// to the wrong call, so every drain site clears it unconditionally.
	pendingSyscallKeepAlive []string

	// syscallKeepAliveCounter numbers every temp convSyscallFunnelCall ever creates, monotonically,
	// for this Visitor's whole run — never reset alongside pendingSyscallKeepAlive above. Naming
	// each statement's temps from 0 (len of the per-statement slice) collided as CS0128 the moment
	// two sibling funnel-call statements shared one enclosing C# block with no scope of their own
	// between them (syscall/linux/lsf_linux.go's SIOCGIFFLAGS/SIOCSIFFLAGS pair, both direct
	// children of one try block): each independently declared `var ᴋ0 = …;`. A monotonic counter
	// makes every temp name unique for the file regardless of how its statement nests, at the cost
	// of not restarting at 0 per function — free, since the name is synthesized and never read back.
	syscallKeepAliveCounter int

	liftedTypeNames HashSet[string]
	liftedTypeMap   map[types.Type]string
	subStructTypes  map[types.Type][]types.Type

	// inUsingAliasTarget is set only while a type alias's `global using` RHS is being rendered.
	// That RHS resolves at COMPILATION scope — outside `namespace go` and outside the emitted
	// `<pkg>_package` class — so every same-package name in it needs a qualification the code-body
	// renderings deliberately elide (see usingAliasTypeQualifier). Set and cleared around the
	// single render call in visitTypeSpec's alias branch.
	inUsingAliasTarget bool

	// Lifted ANONYMOUS struct types deduplicated by structural signature within a function:
	// structurally identical anonymous structs are ONE Go type, so repeated occurrences must
	// lift to a single C# type or reflect.Type identity splits per occurrence (see
	// visitStructType). Keyed `<funcName>\x00<signature>` → lifted name.
	liftedAnonStructNames map[string]string

	// hoistedDecls, when non-nil, collects func-literal capture declarations that would otherwise
	// be emitted inline (a `var mʗ1 = m;` statement) at the func literal's position — invalid C#
	// when the literal sits in an expression slot (a call argument, an assignment RHS, a composite-
	// literal element). The enclosing statement emitter (visitAssignStmt, …) sets this to a buffer,
	// converts its expressions, then writes the collected decls before the statement. convFuncLit
	// consults it (after context.deferredDecls, which go/defer/return thread explicitly). Save and
	// restore around nested statements so an inner statement's decls don't leak to the outer buffer.
	hoistedDecls *strings.Builder

	// resultDiscardedExpr is the expression of the statement currently being emitted as a bare
	// EXPRESSION STATEMENT — the one syntactic slot where C# admits a call but not a cast. A call
	// returning `unsafe.Pointer` takes a `(uintptr)` construct prefix (convCallExpr's pointer-cast
	// note), which is harmless where a value is consumed but turns `SwapPointer(nil, nil);` into
	// `(uintptr)SwapPointer(nil, nil);` — CS0201. Held by NODE IDENTITY, not by a boolean, so a
	// nested call inside the same statement (whose result IS consumed) keeps its conversion.
	resultDiscardedExpr ast.Expr

	// globalDeclHoist, when non-nil, is the PACKAGE-LEVEL var-initializer spill sink: a
	// multi-value inner call spread at a package-level initializer (`var debug = template.Must(
	// template.New(…).Parse(…))`) has no statement sink, so convExprList emits a hidden static
	// tuple FIELD here and visitValueSpec flushes it before the var's own field (C# static field
	// initializers run in textual order). Only the tuple-spread arm writes to it.
	globalDeclHoist *strings.Builder

	// ImportSpec variables
	currentImportPath     string
	packageImports        *strings.Builder
	importQueue           HashSet[string]
	requiredUsings        HashSet[string]
	typeAliasDeclarations *strings.Builder
	// emittedClassName is the `partial class <name>` this FILE's declarations are emitted into —
	// `<pkg>_package`, or the per-variant override under -tests (visitFile computes it; this is the
	// same expression, recorded so emitters running INSIDE the class body can ask what encloses
	// them). Read by forcingTargetShadowed, which needs to know which class's nested types can
	// occlude a namespace-qualified reference written into that class's body.
	emittedClassName string
	// A cross-package type reference emits a short-alias form (`pkg.Type`, `@unsafe.Pointer`) that
	// resolves only through a file-local alias `using <alias> = <namespace>;`. That alias is emitted
	// when the file imports the package under its canonical (unaliased) name; a file can reference the
	// type WITHOUT such an import — via type INFERENCE (a same-package function returns a foreign type,
	// so the caller need not import it — e.g. `fd := funcdata(...)`, funcdata returns unsafe.Pointer),
	// a BLANK import (`_ "pkg"`, whose C# alias is `_`), or an alias that differs from the canonical
	// name — and then the reference fails to resolve (CS0246). referencedForeignPackages collects the
	// import paths whose types getAliasQualifiedTypeName emits; canonicalAliasImported records the paths whose
	// canonical alias a file import already emitted. visitFile supplies the alias for the difference.
	referencedForeignPackages HashSet[string]
	canonicalAliasImported    HashSet[string]
	// importAliasesEmitted holds the C# alias NAMES a file's real imports already bound (`asn1`,
	// `encoding_asn1`, `time`). visitFile's synthesized canonical-alias `using` is skipped when its
	// alias collides with one of these — a same-named subpackage plus an aliased parent import both
	// resolving to alias `asn1` (cryptobyte's `encoding/asn1` + `.../cryptobyte/asn1`, CS1537).
	importAliasesEmitted HashSet[string]

	// importAliasTargets maps each C# alias name a file's imports bound to the TARGET that using
	// resolves to (`@unsafe` → `unsafe_package`, `ast` → `go.ast_package`). C# resolves a using-alias
	// REFERENT with the compilation unit's own using directives NOT in effect, so a referent may not
	// name another file-local alias — variadicElementType substitutes the recorded target to keep the
	// ellipsis alias legal (`using ꓸꓸꓸPointer = Span<unsafe_package.Pointer>;`, never
	// `Span<@unsafe.Pointer>` which fails CS0246). Recorded at the emit sites rather than re-derived,
	// so the `-tests` package-under-test rebinding (visitImportSpec's isPackageUnderTest) is honored
	// automatically instead of silently diverging.
	importAliasTargets map[string]string

	// importPathAliases maps a Go import PATH to the C# alias THIS FILE bound for it, for the
	// EXPLICITLY-ALIASED imports only. getAliasQualifiedTypeName consults it so a foreign type renders via the
	// file's ACTUAL alias, not the canonical package name: cryptobyte's asn1.go imports
	// `encoding/asn1` under the NON-canonical alias `encoding_asn1` (the vendored
	// `.../cryptobyte/asn1` subpackage claims the canonical `asn1`), so a `*asn1.BitString` type
	// reference must render `encoding_asn1.BitString`, not `asn1.BitString` (which resolves to the
	// subpackage — CS0426). Unaliased / blank / dot / Δ-collision-renamed imports are absent and fall
	// back to importQualifier(pkg.Name()) (the prior behavior), so this only changes explicit-alias
	// renders — no churn elsewhere. types.Type carries no source alias, so this map supplies it.
	importPathAliases map[string]string

	// FuncDecl variables
	inFunction      bool
	currentFuncDecl *ast.FuncDecl
	// currentCgoLift is the //go:cgo_unsafe_args block lift of the declaration being emitted, or nil
	// (cgoUnsafeArgsLift.go): the block-prefix assembly renders its construction line and convCallExpr
	// renders its `unsafe.Pointer(&first)` as the block's pinned box.
	currentCgoLift *cgoUnsafeArgsLift
	// liftAtCallBoundary marks an anonymous-struct/interface lift reached from a function's OWN
	// parameter/result declaration or from a call argument being passed to a known callee's
	// matching parameter — positions whose type is externally significant ACROSS function scopes,
	// unlike an ordinary function-body-local lift. visitStructType's package-wide dedup registry
	// is otherwise populated only for package-level (!inFunction) lifts, so two different
	// functions referencing the identical anonymous-struct shape purely through their own
	// signatures/call sites could never unify (CS1503: runtime's
	// testTracebackArgs2/testTracebackArgs5, the traceback pre-pass sizing census). Toggled around
	// exactly the three call sites that reach a signature or call-argument position
	// (visitFuncDecl's parameter/result loops, convCallExpr's argument classification) — never
	// left set across an intervening visit.
	liftAtCallBoundary bool
	// heapIntrinsicShadowed reports that a Go DECLARATION named `heap` is visible where the
	// current function's heap-box emissions land, so each of them must spell golib's boxing
	// intrinsic as `builtin.heap` rather than the bare `heap` (see heapIntrinsicName). Set per
	// function declaration, and OR-ed with a function literal's own declarations (save/restore in
	// convFuncLit) so a literal nested inside a clean function is still covered by its own params.
	heapIntrinsicShadowed bool
	// heapIdentInPackage caches whether the package under conversion declares `heap` at ALL (see
	// packageMentionsHeapIntrinsicIdent) — the gate that keeps the per-declaration scan from
	// costing every package a second AST traversal to learn a package-wide fact.
	heapIdentInPackage   *bool
	currentFuncSignature *types.Signature
	// funcSignatureIsLiteralSeed records that currentFuncSignature is a func LITERAL's own signature,
	// seeded by convFuncLit because there was no enclosing declaration to take it from — the state a
	// package-level `var` initializer's literal converts in. The seed exists for nil-safety (the
	// receiver/parameter detection dereferences the field unguarded), but it is NOT an enclosing
	// signature: a literal's pointer parameter is the raw box `ж<T>` with no deref alias, where a
	// DECLARATION's is a value alias over one. Every predicate that reads the signature to decide
	// "is this ident a parameter of the function being emitted" must decline while this is set.
	funcSignatureIsLiteralSeed bool
	// currentReturnSignature is the signature whose RESULTS a `return` currently emits against — the
	// enclosing function's, or a nested function literal's own (set with save/restore in convFuncLit).
	// Distinct from currentFuncSignature (which stays the enclosing func for receiver/param detection).
	currentReturnSignature *types.Signature
	currentFuncName        string
	currentFuncPrefix      *strings.Builder
	// packageInitLiftName is the name of the PACKAGE-LEVEL declaration whose initializer is
	// currently being converted — the name seed a type lifted from inside a func literal in that
	// initializer takes, standing in for the enclosing function name it has no access to.
	// currentFuncName/currentFuncPrefix are owned by visitFuncDecl and are STALE at package level
	// (see convFuncLit's package-level lift block).
	packageInitLiftName string
	// promotedInterfaceForwarders collects the [GoRecv] forwarders a DUAL-embed struct owes for
	// methods only its embedded-interface field provides in *T's method set (the pointer-only
	// satisfaction arm in visitStructType); emitted immediately after the struct declaration
	// closes, so the extension surface is complete before go2cs-gen composes the pointer-form
	// adapter the arm's GoImplement record asks for.
	promotedInterfaceForwarders []promotedInterfaceForwarder
	paramNames                  HashSet[string]
	paramObjects                map[types.Object]bool
	// currentRefReturnPrimary marks the function being visited as a B′-S0 arm-(a) ref-return
	// primary (packageRefReturnPrimaryMethods): its declared return is `ref T` and its bare
	// receiver-returns emit `return ref v;` (visitReturnStmt). Set per function declaration in
	// visitFuncDecl, stale outside one — the same ownership convention as currentFuncName.
	currentRefReturnPrimary bool
	// erasedTypeParams holds the current FUNCTION declaration's pointer-core (erased) type
	// parameters, identity-keyed to their pointer types (see collectErasedTypeParams) — the
	// single source every renderer/classifier consults so the erasure flips coherently, and
	// declined shapes (generic named types, receiver type params) never half-erase. Reset per
	// function declaration; func literals inside the declaration inherit it.
	erasedTypeParams map[*types.TypeParam]*types.Pointer
	// currentFuncCompanionNames holds the current FUNCTION declaration's DESCRIPTOR COMPANION type
	// parameters (see descriptorCompanion.go) — identity-keyed to the declaration's OWN type
	// parameters, so a generic named type's list, or a CALLED generic's parameters, are simply never
	// in the map and no renderer can half-apply the threading. Reset per function declaration; func
	// literals inside the declaration inherit it, which is what lets a `reflect.TypeFor[T]` written
	// inside a closure render the companion the enclosing declaration declared.
	currentFuncCompanionNames map[*types.TypeParam]string
	// identAddressTakenCache memoizes per-object `&ident` scans of the current function
	// (see identAddressTaken); lazily initialized, keyed by the *types.Object so entries
	// from prior functions are simply never consulted again.
	identAddressTakenCache map[types.Object]bool
	// captureAnalysisDecl is the declaration whose body performVariableAnalysis is currently
	// walking — the real FuncDecl, or visitValueSpec's SYNTHETIC wrapper for a package-level
	// func-literal initializer. The shared-capture routing's write scan (varShareFacts) reads
	// it: it must match the tree processPotentialCapture is analyzing, which currentFuncDecl
	// does not during synthetic analysis (it still points at the previously visited function).
	captureAnalysisDecl *ast.FuncDecl
	// captureShareFactsCache memoizes varShareFacts per captured variable (reset per
	// performVariableAnalysis; variable objects are function-unique so entries never collide).
	captureShareFactsCache map[types.Object]captureShareFacts
	// funcLitHeapBoxParamNames holds the RENDERED names of the function literal parameters that
	// need an entry-time heap box (see funcLitHeapBoxParamIdents) — set transiently by
	// convFuncLit around exactly the signature-generation calls (convFuncType for a plain
	// literal, iifeParamNames for an IIFE) so the parameter emits under its incoming `ʗp` name,
	// and nil otherwise. A literal's signature is generated from SYNTHESIZED vars (see
	// getSignature) that can never match the identEscapesHeap entries paramNeedsHeapBox keys
	// on, so the box decision travels by name here.
	funcLitHeapBoxParamNames HashSet[string]
	// funcLitProxyParamTypes maps a function literal parameter's Go name to the CONSTRAINT-PROXY
	// C# type it must be DECLARED as — for a literal passed to a generic call whose type argument
	// resolved to a proxy (see constraintProxyLitParamTypes). Set transiently by convFuncLit around
	// exactly the signature-generation call, like funcLitHeapBoxParamNames beside it, and nil
	// otherwise: the parameter then arrives under a synthesized name at the proxy type and the
	// body prologue re-declares the Go name at its natural type. Same incoming-name-plus-prologue
	// shape the heap-box parameters use.
	funcLitProxyParamTypes map[string]string
	// funcLitEntries collects this FILE's function-literal name records — the Go source-line span
	// and the dotted counter suffix Go's compiler names each literal with (`1` for Outer.func1,
	// `1.2` for the second literal nested directly inside it) — appended per function declaration
	// by collectFuncLitNames and emitted as the file's GoPositionMap record's funcLits argument
	// (see finalizePositionMap). Pure AST facts, recorded at declaration-visit time so no emission
	// reordering, collapse, or double conversion of an expression can perturb the counter.
	funcLitEntries []funcLitEntry
	varNames       map[*types.Var]string
	hasDefer       bool
	hasRecover     bool
	// pendingTypeAccess carries an explicit C# access modifier ("public ") for the type
	// declaration currently being emitted — set by visitTypeSpec for an unexported type that
	// must be publicized (used as an exported struct field; see packagePublicizedTypes), and
	// consumed (read and cleared) by the type-kind emitter (visitArrayType/visitStructType/…).
	pendingTypeAccess string
	// manualConversion mirrors the file entry's flag: this file's destination `.cs` is a hand-owned
	// manual conversion, so the visit still feeds package-wide state but the emitted text lands in
	// the non-compiled `.cs.auto` review sibling. Consulted by recordTypeAccessibility, which must
	// not declare the accessibility of types the hand-written file owns.
	manualConversion bool
	// sourceFilePath is the ABSOLUTE-able path of the Go file this visitor is converting, kept so
	// the position map can record the source identity Go itself would have baked for the same build
	// (goSourceIdentity). Mirrors the file entry, which the driver does not hand back to the writer.
	sourceFilePath string
	// namedReturnDeferMode is set when the current function has named return values AND uses
	// defer/recover. Such a function is emitted as a block body that declares the named returns
	// *outside* the `func((defer, recover) => …)` wrapper (so deferred code, including recover,
	// mutates them by closure) and returns them *after* the wrapper runs — matching Go, where a
	// `return` assigns the result params, runs the defers, then returns the (possibly-mutated)
	// result params. namedReturnNames holds those result identifiers in order.
	namedReturnDeferMode bool
	namedReturnNames     []string
	// inGoFrame is set while emitting the body of a function whose defer/recover scope is a
	// GoFrame — the ref-struct frame declared beside an INLINE body inside try/catch/finally
	// (docs/Phase4/DESIGN-closure-emission.md §4) — rather than the `func((defer, recover) => …)`
	// execution-context lambda. It is what visitDeferStmt consults to register into the frame
	// (`deferǃ(f, a, ref ᒐ)`) instead of calling the wrapper's `defer` delegate, so it must be
	// FALSE while a nested function literal's own body is converted: that literal owns its own
	// defer scope, and a ref struct cannot be captured by a lambda in any case. convFuncLit saves
	// and restores it for exactly that reason.
	inGoFrame bool
	// openGoFrames counts the GoFrame scopes currently open around the emission point — the
	// enclosing function's, plus one per nested function literal that carries its own. The frame
	// LOCALS need no numbering: a C# lambda or local function may shadow an enclosing method local,
	// so every frame in every emitted function reads under the same name. The named-result exit
	// LABEL does need it, because labels do NOT shadow — repeating one from the enclosing method
	// inside a lambda is CS0158. Zero when no frame is open.
	openGoFrames int
	// goFrameNamedExit records that the body emitted at least one `goto ᒐdone;` — the named-result
	// form's early exit (§4.4). The label is emitted only when something targets it, so a function
	// whose body simply falls off the end carries no unreferenced label.
	goFrameNamedExit bool
	// loweredDefers holds the current function's defer→finally lowered sites in SOURCE order, and
	// loweredDeferIndex maps each admitted DeferStmt to its position in that slice so
	// visitDeferStmt can find its own entry. Both are empty for every function that does not
	// qualify, which is the overwhelming majority — see deferFinallyLowering.go for the gates.
	loweredDefers     []*loweredDefer
	loweredDeferIndex map[*ast.DeferStmt]int
	// entryAliasBoxPaths maps a parameter's entry deref ALIAS to the box expression it was declared
	// from (`c` → `Ꮡc.DerefOrNull()`). The alias is declared INSIDE the frame's try, so the finally
	// cannot name it and a lowered defer's call is re-rooted on the box instead.
	entryAliasBoxPaths map[string]string
	// blankResultNames interns the generated slot name for each BLANK (`_`) result of a
	// namedReturnDefer signature — Go allows mixing blank and named results
	// (`func parse(…) (_ *Regexp, err error)`), and the blank slot still needs a C# local so
	// returns can write it and the post-defer return can read it back. Keyed by the result's
	// *types.Var so every render site of the same slot agrees on one name.
	blankResultNames map[*types.Var]string
	useUnsafeFunc    bool
	capturedVarCount map[string]int
	tempVarCount     map[string]int

	// BlockStmt variables
	blocks                 Stack[*strings.Builder]
	firstStatementIsReturn bool
	// tupleTempIndex numbers the multi-value-call expansion temp markers monotonically per
	// file (see convExprList's tuple-arg expansion).
	tupleTempIndex int
	// inForPost is set while emitting a for-loop's POST statement. A deref-aliased pointer
	// param/box repointed in the post (`for ; scope != nil; scope = scope.Outer`) expands to a
	// box-repoint PLUS a value re-alias (`Ꮡscope = scope.Outer; scope = ref Ꮡscope…`); the
	// second statement cannot share the single for-post slot, so the re-alias is stashed in
	// forPostReAlias and visitForStmt injects it at the TOP of the loop body instead.
	inForPost      bool
	forPostReAlias string
	// forPerIterVars holds the `for i := …` clause variables currently emitted with Go 1.22+
	// per-iteration semantics (see forClausePerIterVars): the clause drives a renamed carrier
	// and the body re-declares the variable fresh each pass. convertToHeapTypeDecl consults it
	// so the carrier stays a plain value — a boxed variable's fresh box is emitted inside the
	// body per iteration, never hoisted at the declaration site.
	forPerIterVars map[types.Object]bool
	// loopCopyBackStack parallels the enclosing loop nesting during body emission. Each entry
	// holds the per-iteration copy-back statements (`iᴛ1 = i;`) an unlabeled `continue` must
	// emit before transferring to the post clause (nil for range loops and for loops whose
	// per-iteration variables are never written in the body).
	loopCopyBackStack [][]string
	// continueTargetStack parallels the enclosing C# ITERATION-statement nesting during body
	// emission: a loop entry per Go for/range loop, a wrapper entry per `do { … } while (false)`
	// switch-break wrap (visitSwitchStmtCore). When the top is a wrapper, an unlabeled `continue`
	// cannot be emitted bare — C# would bind it to the wrapper — so it emits `goto` to the nearest
	// enclosing LOOP entry's end-of-body label, marking the label used so the loop declares it
	// (see wrappedContinueLoopLabel / prepareLoopContinueTarget).
	continueTargetStack     []*continueTargetEntry
	lastStatementWasReturn  bool
	lastReturnIndentLevel   int
	identEscapesHeap        map[types.Object]bool
	tightenedConsts         map[*types.Const]*types.Basic // Function-local untyped consts declared at their single concrete use type (see performUntypedConstAnalysis)
	sstringEligible         map[types.Object]bool         // String locals emittable as stack-only sstring (see FileEntry.sstringEligible)
	ssliceEligible          map[types.Object]bool         // Variadic params emittable as stack-only sslice (see FileEntry.ssliceEligible)
	sstringConvExprs        map[*ast.CallExpr]bool        // `string(x)` conversions that emit `(sstring)x` (see FileEntry.sstringConvExprs)
	emitStringConvAsSString bool                          // Transient: while emitting an eligible decl's RHS, a string([]byte) conversion emits `(sstring)` not `(@string)`
	sstringHoistedConvExprs map[*ast.CallExpr]string      // Per-func: eligible `string(x)` uses lifted to a shared sstring temp — each emits the temp NAME (see planSStringHoists)
	sstringHoistsByStmt     map[ast.Stmt][]sstringHoist   // Per-func: hoisted sstring temp decls to inject before a top-level body statement (its anchor)
	suppressSStringHoist    bool                          // Transient: while rendering a hoisted temp's OWN initializer, ignore sstringHoistedConvExprs so the real `((sstring)x)` view is emitted
	deadUnsafePointerBoxes  map[*ast.CallExpr]bool        // `unsafe.Pointer(x)` conversions whose wrapper object an enclosing `uintptr(…)` reads straight back — emitted without it (see markDeadUnsafePointerBox)
	identNames              map[*ast.Ident]string         // Local identifiers to adjusted names map
	isReassigned            map[*ast.Ident]bool           // Local identifiers to reassignment status map
	// untypedConstContexts maps an UNTYPED constant subexpression to the resolved type of its
	// enclosing typed constant expression — the context go/types drops when it leaves constant
	// operands untyped (see markUntypedConstContexts). convBasicLit consults it for the F/D
	// float-literal suffix and the postfix `.i()` complex64/complex128 overload choice.
	untypedConstContexts map[ast.Expr]types.Type
	funcLevelDecls       map[string]*types.Var // Function-level local declarations of the current function (for global-shadow qualification)
	// funcScopeVarNames holds the Go name of every variable declared ANYWHERE in the current
	// function — receiver, parameters, results and locals at every nesting depth, including inside
	// func literals. A bare type name spelled by the EMITTER (the `Type.Ꮡfield` box accessor) binds
	// to such a variable rather than to the type wherever one exists, so boxAccessorType qualifies
	// against this set. Repopulated per function by performVariableAnalysis.
	funcScopeVarNames HashSet[string]
	scopeStack        []map[string]*types.Var // Stack of local variable scopes
	lambdaCapture     *LambdaCapture          // Lambda capture tracking
}
