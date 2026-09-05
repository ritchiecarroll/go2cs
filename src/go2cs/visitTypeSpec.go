// visitTypeSpec.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"regexp"
	"strings"

	"golang.org/x/tools/go/packages"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)
	identType := v.getIdentType(typeSpec.Name)

	// A defined type whose name COLLIDES with a method name (go/ast's `type Filter func(string)
	// bool` vs `(CommentMap).Filter`) is Δ-prefixed at every USE (convIdent →
	// getSanitizedIdentifier), but getIdentName returns the raw name — so the DECLARATION
	// emitted a bare `delegate … Filter` that both duplicated the method (CS0102) and was
	// unreachable from the ΔFilter uses (CS0246 ×14). Match the declaration to the uses. Manual
	// types already record the Δ form explicitly (below), so this covers the auto-emitted kinds.
	if nameCollisions[typeSpec.Name.Name] {
		name = getSanitizedIdentifier(typeSpec.Name.Name)

		if testTypeRenames != nil {
			if obj := v.info.ObjectOf(typeSpec.Name); obj != nil {
				testTypeRenames[obj] = true
			}
		}
	}

	// A DEFINED type over an INTERFACE (`type Token any`, `type Reader io.Reader`) has EXACTLY the
	// interface's method set and can carry no methods of its own (Go forbids an interface receiver),
	// so it is emitted as a `global using` alias to that interface — the SAME form as a real Go type
	// alias below — never a `[GoType] partial struct` wrapper. A struct wrapper over `any` (= object)
	// admits no implicit conversion FROM a concrete value (C# bars user-defined conversions from
	// object), so every `StartElement → Token` assignment was CS0029 (encoding/xml's `type Token
	// any`, ×16). Restricted to a NAMED-type RHS (Ident/Selector); an inline interface DEFINITION
	// (`type X interface{…}`) is an *ast.InterfaceType and still emits a C# interface via the switch.
	definedOverInterface := false

	if !typeSpec.Assign.IsValid() {
		switch typeSpec.Type.(type) {
		case *ast.Ident, *ast.SelectorExpr:
			if _, isIface := identType.Underlying().(*types.Interface); isIface {
				definedOverInterface = true
			}
		}
	}

	// Handle type alias (or a defined type over an interface — see above)
	if typeSpec.Assign.IsValid() || definedOverInterface {
		// Get types.Type from typeSpec.Type expr
		typeSpecType := v.info.TypeOf(typeSpec.Type)

		if typeSpecType == nil {
			panic(fmt.Sprintf("@visitTypeSpec - Failed to get type for type alias %s", name))
		}

		// Check if the aliased type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(typeSpec.Type); structType != nil && !v.liftedTypeExists(structType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitStructType(structType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}
		}

		// Check if the aliased type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(typeSpec.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitInterfaceType(interfaceType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}
		}

		// A `global using` RHS is the one rendering that lands at COMPILATION scope, so — unlike
		// every code-body rendering — nothing in it may lean on `namespace go` or on the enclosing
		// `<pkg>_package` class being in scope. Both halves of that are switched on here and only
		// here: usingAliasTypeQualifier package-qualifies each same-package name (the target and
		// everything it nests — a lifted anonymous type, a slice element, a map value), and
		// renderCSFullTypeName's rootNested mode roots each remaining nested name.
		v.inUsingAliasTarget = true

		typeName := renderCSFullTypeName(v.getFullyQualifiedTypeName(typeSpecType, false), true)

		v.inUsingAliasTarget = false

		// The empty interface target (`type X any` / `type X = any` / `type X interface{}`) renders
		// as `go.any`, which does not resolve in a using-alias RHS (any is a csproj-level alias, and
		// the safe-name rewrite below deliberately skips `.`-qualified names) — it IS `object`. Emit
		// object directly (encoding/xml's `type Token any`).
		if iface, ok := typeSpecType.Underlying().(*types.Interface); ok && iface.Empty() {
			typeName = "object"
		} else {
			// A `using` alias RHS is resolved WITHOUT reference to other using directives - the
			// csproj-level golib aliases (`uint64`, `float64`, `any`, ...) that resolve everywhere
			// else in the compilation are CS0246 here (fiat: `type p224UntypedFieldElement =
			// [4]uint64` must emit `global using ... = go.array<ulong>;`, not `...<uint64>`). Rewrite
			// those names to their using-safe C# keyword/BCL equivalents for this context only.
			typeName = getUsingAliasSafeTypeName(typeName)

			// The ROOTED namespace of a global-using RHS must use the CANONICAL package qualifier,
			// never the file-local collision-rename: os re-exports `type DirEntry = fs.DirEntry`, but
			// os aliases its `io` import to `Δio` (io is shadowed once io/fs is in the reference
			// closure), and getAliasedTypeName applies that rename even when rooting — emitting
			// `go.Δio.fs_package.DirEntry`, where `Δio` is a file-local `using`, not a namespace under
			// `go` → CS0234 (os's DirEntry/PathError/FileInfo/FileMode re-exports). Un-rename the
			// qualifier right after the root when it is a known import rename (a Δ-renamed TYPE segment
			// is left untouched — only an entry the import-rename map produced is reverted).
			rootPrefix := RootNamespace + "."
			if after, rooted := strings.CutPrefix(typeName, rootPrefix); rooted {
				if seg, rest, found := strings.Cut(after, "."); found {
					if canonical, wasRenamed := strings.CutPrefix(seg, ShadowVarMarker); wasRenamed && packageImportAliasRenames[canonical] == seg {
						typeName = rootPrefix + canonical + "." + rest
					}
				}
			}
		}

		// A function-local type declaration is scoped to its function; the `global using` this
		// branch emits is scoped to the whole COMPILATION. Every OTHER local type-declaration kind
		// already takes the liftLocalTypeDecl rename for that reason — this branch was the one that
		// did not, so two functions declaring `type testFnc any` collided under one alias name:
		// CS1537, whether they sit in one file or in two of the same compilation (archive/tar
		// declares it in writer_test.go's TestWriter AND TestFileWriter, and again in
		// reader_test.go's TestFileReader, with `fileMaker` alongside it — 3 diagnostics, all 97 of
		// the package's verdicts behind them). Take the same lift, and register it so every
		// reference in the function resolves to the lifted name.
		aliasName := name

		if v.inFunction {
			aliasName = v.liftLocalTypeDeclName(name)

			if liftedTypeDeclaredBy(identType, v.info.Defs[typeSpec.Name]) {
				v.liftedTypeMap[identType] = aliasName
			}
		}

		v.typeAliasDeclarations.WriteString(fmt.Sprintf("global using %s = %s;%s", aliasName, typeName, v.newline))

		// THE DESCRIPTOR CARRIER. The `global using` above is a COMPILE-TIME construct: it leaves no
		// metadata, so the Go name this declaration gives the type is gone by the time the reflection
		// bridge sees a value of it — `reflect.TypeFor[Token]().Name()` answers "" where Go answers
		// "Token", and for the non-empty case it answers the TARGET interface's name, which belongs to
		// a different Go type. The alias is not the defect (it is what makes Go's universal
		// assignability to `any` fall out of C# assignment for free, and no C# type carries a name AND
		// keeps that), so the VALUE stays exactly as it is and only the DESCRIPTOR gains an identity:
		// an UNINHABITED interface that nothing implements and no value is ever of, carrying the Go
		// name as [GoLocalName] so golib's existing naming reconstruction answers it with no change of
		// its own. Emitted only for a DEFINED type over an interface, never for a real Go alias — Go
		// reports an alias's TARGET name, so a carrier there would invent a wrong name rather than fix
		// one (`os.DirEntry`/`os.FileInfo` are that case, 19 descriptor positions in the corpus).
		//
		// It carries NO [GoType]: TypeGenerator keys on that attribute, and a carrier is not a Go type
		// anything is emitted for. Measured — with all five go2cs-gen generators live in one
		// compilation, a carrier draws zero generated output.
		if definedOverInterface && !v.inFunction {
			carrierAccess := getAccess(name)

			v.writeOutputLn("// Descriptor carrier for `%s` — uninhabited; see GoDescriptorTypeAttribute.", typeSpec.Name.Name)
			v.writeOutputLn("[GoLocalName(\"%s\")] %s interface %s%s { }", typeSpec.Name.Name, carrierAccess, aliasName, DescriptorCarrierSuffix)
			v.writeOutputLn("")
		}

		// Add exported type aliases to package info. Never for a function-local declaration: it is
		// not part of the package's exported surface whatever its Go name looks like, and the lift
		// above means the name a consumer would import does not exist.
		if !v.inFunction && getAccess(name) == "public" {
			packageLock.Lock()
			exportedTypeAliases[name] = typeName
			packageLock.Unlock()
		}

		return
	}

	// A manually-converted type (see manualTypeOperations.go) emits only a marker comment; the
	// package's *_impl.cs declares the type. Both its plain and collision-renamed forms are
	// recorded (a type-vs-method collision Δ-prefixes the TYPE — guintptr → Δguintptr) so the
	// GoImplicitConv attribute emission can skip conversions referencing either rendering.
	//
	// The comment sink is served here for the same reason visitFuncDecl's placeholder serves it, and
	// in the same two calls: this return skips the writeDoc a converted type-kind emitter would have
	// performed (visitStructType/visitInterfaceType write one; the forward-declaration kinds reach it
	// through the NEXT declaration) and never visits the declaration's own span. Unserved, neither
	// set is dropped — both are misplaced, since the drain is positional and the next declaration's
	// writeDoc takes everything standing before it. The drain is anchored on the SPEC rather than the
	// GenDecl so a grouped `type ( … )` serves each of its specs in turn; a doc comment the parser
	// attached to either node is not in the sink at all (visitFile removed it) and is unaffected.
	if v.isManualType(typeSpec.Name.Name) {
		packageLock.Lock()
		packageManualTypeNames[name] = true
		packageManualTypeNames[getSanitizedIdentifier(typeSpec.Name.Name)] = true
		packageLock.Unlock()

		if !v.inFunction {
			v.outputBuilder.WriteString(v.newline)
		}

		v.writeDoc(nil, typeSpec.Pos())
		v.writeOutput("// go2cs generated this placeholder — type %s is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])", name)
		v.outputBuilder.WriteString(v.newline)
		v.discardStandAloneComments(typeSpec.Pos(), typeSpec.End())
		return
	}

	// An unexported type used as an exported struct field, or in an exported callable's signature,
	// must be emitted as public (CS0050/CS0051/CS0052). Set the access modifier for the type-kind
	// emitter below to consume.
	//
	// The two arms answer DIFFERENT questions and must not be exclusive: publicization decides WHAT
	// the modifier is, testInlineTypeAccess only decides WHERE it is written. Asking the inline arm
	// first made it also decide the value — from the name alone — so a publicized BRIDGE type stayed
	// `internal` and its exported referrer was CS0051 (context's `testingT`, declared in the internal
	// `context_test.go` and taken by the exported `XTestParentFinishesChild` that `x_test.go` calls).
	// Publicization therefore outranks the default, and the inline arm supplies that default.
	if v.isPublicizedType(typeSpec.Name) {
		v.pendingTypeAccess = "public "
	} else if localAccess := v.localTypeAccess(); localAccess != "" {
		// A FUNCTION-LOCAL declaration takes the local-type rule, in EVERY conversion: its Go name
		// carries no export meaning, and the hoisted `<Func>_<name>` identifier the name-based rules
		// actually read begins with the enclosing function's first letter. Deriving a modifier from
		// that splits the types ONE function declares between public and internal and breaks C#'s
		// accessibility consistency — reflect's `type BigP *big` inside TestExported is the
		// production-side witness (localTypeAccess carries the full case).
		v.pendingTypeAccess = localAccess
	} else if v.options.testInlineTypeAccess {
		// Bridge-owned named types carry accessibility inline. Their metadata anchor can be a
		// different test class, where an accessibility-only partial would declare a second type.
		//
		// A test-file-declared type whose UNDERLYING is an unexported PRODUCTION named type
		// (runtime's `type MSpan = mspan` in export_test.go, deliberately capitalized so external
		// test files can name it) is DELIBERATELY NOT downgraded here, unlike the method/value
		// siblings of this rule (testMethodAccessDowngrade, testDeclaredValueAccess): downgrading
		// MSpan cascades, because it is a widely-referenced BRIDGE NAME — every other test-file
		// function/method whose signature mentions MSpan (AllocMSpan, FreeMSpan, MSpanCountAlloc, …)
		// is ALSO exported and currently computed independently of MSpan's own accessibility, so
		// downgrading MSpan alone turns dozens of currently-consistent signatures inconsistent
		// (measured: 32 -> 70 errors). The wrapper's OWN scaffolding referencing the unexported
		// underlying type directly (InheritedTypeTemplate's constructor/.Value/field-box accessors)
		// is a real, distinct residual — tracked, not fixed here; it needs either propagating this
		// downgrade to every consumer of the bridge name (a corpus-wide ripple this task did not
		// scope) or representing a Go type ALIAS as a true C# alias rather than a wrapper struct.
		v.pendingTypeAccess = generatedTypeScope(getSanitizedIdentifier(name)) + " "
	}

	defer func() { v.pendingTypeAccess = "" }()

	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, identType, name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType, identType, name)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType, identType, name)
	case *ast.Ident:
		v.visitIdent(typeSpecType, identType, name, v.inFunction)
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	case *ast.MapType:
		v.visitMapType(typeSpecType, identType, name)
	case *ast.ParenExpr:
		v.outputBuilder.WriteString(v.convParenExpr(typeSpecType, DefaultLambdaContext(), DefaultBasicLitContext()))
	case *ast.SelectorExpr:
		// A DEFINED type over a cross-package named type (`type stdFunction unsafe.Pointer`,
		// `type goroutineProfileStateHolder atomic.Uint32`). Emit an inherited `[GoType]` wrapper of
		// the NAMED type (go2cs-gen's InheritedTypeTemplate wraps a plain type-name definition);
		// writing the bare selector text alone is an orphan type reference (CS1585). Use the named
		// type, not its underlying (which may expose unexported cross-package fields → CS0246).
		if rhsType := v.info.TypeOf(typeSpecType); rhsType != nil {
			csName := convertToCSTypeName(v.getFullyQualifiedTypeName(rhsType, false))

			// The GoType attribute is consumed by the generated `<X>.g.cs`, which has no file-local
			// `using` aliases. unsafe.Pointer (a *types.Basic, a C# keyword) renders via the
			// `@unsafe` alias; rewrite it to the alias-free package class so it resolves there.
			// A Δ collision-renamed leading namespace segment must revert to the canonical
			// qualifier for the same reason (see canonicalizeQualifierRename).
			csName = strings.ReplaceAll(csName, "@unsafe.", "unsafe_package.")
			csName = canonicalizeQualifierRename(csName)

			access := v.consumePendingTypeAccess()

			// A defined type over a FOREIGN named type declared inside a function body — reflect's
			// `type MyBuffer bytes.Buffer` in set_test.go's TestImplicitMapConversion. C# forbids a
			// type declaration in a method body, and this was the ONE local type-declaration kind
			// that did not take the lift: the `[GoType] partial struct` landed inline in the block
			// it was written in, and that single site cost 73 parse diagnostics — the whole file,
			// and the whole package's suite behind it. Take the same hoist the StarExpr arm above
			// and visitIdent/visitStructType/visitInterfaceType already take. The GoType STRING is
			// unaffected: it names the wrapped foreign type, which the generated `<X>.g.cs` resolves
			// identically wherever the declaration itself sits (the corpus already emits exactly this
			// rendering for the package-level form — flag's `[GoType("time_package.Duration")]`).
			name, target, finish := v.liftLocalTypeDecl(name, identType)

			if !v.inFunction {
				target.WriteString(v.newline)
			}

			// Cross-package twin of visitIdent's stamp: a defined type over a struct carrying
			// fixed-size ARRAY fields needs the forwarded `Clone()` (see wrapperValueCloneAttr).
			inlineAttrs := v.recordTypeAccessibility("struct", getSanitizedIdentifier(name), "", access, wrapperValueCloneAttr(rhsType))

			v.writeStringLn(target, "%s[GoType(\"%s\")] %s%spartial struct %s;", v.localNameAttrFor(identType), csName, inlineAttrs, access, getSanitizedIdentifier(name))
			finish()
		} else {
			v.outputBuilder.WriteString(v.convSelectorExpr(typeSpecType, DefaultLambdaContext()))
		}
	case *ast.StarExpr:
		{
			// A defined POINTER type — `type dequeueNil *struct{}` (sync/poolqueue). The bare
			// converted star-type text (`ж<EmptyStruct>`) is not a declaration (CS1585 — sync's
			// wave-1 error); emit the `[GoType("ж<T>")] partial class` forward declaration whose
			// Pointer template go2cs-gen implements (the generator matches a CLASS declaration
			// for ж<-prefixed definitions — a named pointer is reference-like).
			access := v.consumePendingTypeAccess()

			// A pointer type declared inside a function body (`type Rec ***Rec`, gob's
			// codec_test.go) cannot be a method-body statement in C#; hoist it to member level
			// (see liftLocalTypeDecl). The lift is taken BEFORE the pointer text is rendered so a
			// SELF-referential declaration resolves its own name through liftedTypeMap to the
			// lifted name. A package-level declaration is unaffected — target is v.outputBuilder and
			// finish() is a no-op.
			name, target, finish := v.liftLocalTypeDecl(name, identType)

			pointerTypeName := v.convStarExpr(typeSpecType, DefaultStarExprContext())

			if !v.inFunction {
				target.WriteString(v.newline)
			}

			v.recordTypeAccessibility("class", getSanitizedIdentifier(name), "", access, "")
			// A defined POINTER-TO-ARRAY type carries the array's dims on the wrapper as TYPE-level descriptor
			// cargo (increment E3 follow-up 7g): `[GoType("ж<array<byte>>")]` spells the pointee's managed type
			// and nothing of its length, so a nil and a live value of `type P *[0]byte` synthesized two
			// descriptors. The dims are read off the RHS pointer literal (undefined, so nilArrayPtrDims answers);
			// any other pointee stamps nothing (census: production 0 named pointer-to-array types).
			dimsAttr := ""

			if dims := nilArrayPtrDims(v.info.TypeOf(typeSpecType)); len(dims) > 0 {
				dimsAttr = fmt.Sprintf("[GoArrayDims(%s)] ", renderDimsList(dims))
			}

			v.writeStringLn(target, "%s[GoType(\"%s\")] %s%spartial class %s;", v.localNameAttrFor(identType), pointerTypeName, dimsAttr, access, getSanitizedIdentifier(name))
			usesUnsafeCode = true
			finish()
		}
	case *ast.StructType:
		v.visitStructType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	default:
		panic(fmt.Sprintf("@visitTypeSpec - Unexpected TypeSpec type: %#v", v.getPrintedNode(typeSpecType)))
	}
}

// liftLocalTypeDecl prepares a forward-declaration type-kind (array/slice, map, channel) for
// emission when it is declared INSIDE a function body. C# forbids a type declaration in a method
// body, so a `type X []T` / `type X map[K]V` / `type X chan T` written inside a function is renamed
// with the enclosing-function prefix, registered in liftedTypeMap (so every later reference resolves
// to the lifted name), redirected to a member-level builder, and emitted at indent 0. The returned
// finish closure flushes that builder into currentFuncPrefix — which the function's prefix marker
// emits ahead of the method — and restores the indent level. At package scope it is a no-op: the
// returned target is v.outputBuilder and finish() does nothing, so the emitted bytes are unchanged.
// Mirrors the identical in-function hoisting inlined by visitIdent/visitStructType/visitInterfaceType.
func (v *Visitor) liftLocalTypeDecl(name string, identType types.Type) (liftedName string, target *strings.Builder, finish func()) {
	if !v.inFunction {
		return name, v.outputBuilder, func() {}
	}

	target = &strings.Builder{}

	preLiftIndentLevel := v.indentLevel
	v.indentLevel = 0

	name = v.liftLocalTypeDeclName(name)

	if identType != nil {
		v.liftedTypeMap[identType] = name
	}

	return name, target, func() {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		v.indentLevel = preLiftIndentLevel
	}
}

// liftLocalTypeDeclName is the NAMING half of liftLocalTypeDecl: it prefixes a function-local type
// declaration with its enclosing function and runs the result through getUniqueLiftedTypeName's ᴛN
// disambiguation, so two functions declaring the same local type name never claim one C# name.
//
// It is shared with visitTypeSpec's type-ALIAS branch, which needs exactly this uniqueness but not
// the member-level builder redirection — a `global using` is emitted at file scope either way.
func (v *Visitor) liftLocalTypeDeclName(name string) string {
	if !strings.HasPrefix(name, v.currentFuncName+"_") {
		name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
	}

	return v.getUniqueLiftedTypeName(name)
}

// localNameAttrFor renders the `[GoLocalName("<name>")] ` stamp for a lifted FUNCTION-LOCAL named
// type declaration, or "" at package scope — where the C# identifier IS the Go name and the stamp
// would be noise. The attribute is what lets golib's GoTypeName answer Go's OWN name for a lifted
// type: goBareTypeName already prefers goLocalNameOf for ANY type, so the receiving half has been
// in place since the dyn-lift sites (visitStructType/visitInterfaceType) established the pattern —
// the NAMED local-type lift branches simply never joined it. Without the stamp every such type
// reported the lifted `<Func>_<name>` identifier as its Go name, whose first rune is the enclosing
// FUNCTION's: reflect's TestExported read `type p *P` as exported off the 'T' in
// `TestExported_p`, and TestSliceOf's String() printed `[]reflect_test.TestSliceOf_T` where Go
// prints `[]reflect_test.T`. Shared by every lift-emitting branch (visitIdent's wrapper kinds,
// visitTypeSpec's SelectorExpr/StarExpr, visitArrayType, visitChanType, visitMapType), mirroring
// the dyn sites' composition verbatim.
func (v *Visitor) localNameAttrFor(identType types.Type) string {
	if !v.inFunction {
		return ""
	}

	if named, ok := identType.(*types.Named); ok {
		return fmt.Sprintf("[GoLocalName(\"%s\")] ", named.Obj().Name())
	}

	return ""
}

// liftedTypeDeclaredBy reports whether t is the type the given declaration INTRODUCES, rather than
// a type it merely names. It gates registering a lifted name against t in liftedTypeMap, where a
// wrong key renames every reference to an unrelated type: `type X = Header` inside a function binds
// the declaration's object to the *existing* Header named type (and, without materialized aliases,
// `type X = int` binds it to plain int), so keying the lift on it would rewrite every Header — or
// every int — in the file. Only a *types.Named or *types.Alias whose own Obj IS this declaration
// qualifies; anything else registers nothing and simply renders as it does today.
func liftedTypeDeclaredBy(t types.Type, declared types.Object) bool {
	if t == nil || declared == nil {
		return false
	}

	switch typed := t.(type) {
	case *types.Named:
		return typed.Obj() == declared
	case *types.Alias:
		return typed.Obj() == declared
	}

	return false
}

// samePackageTypeQualifier returns the prefix that qualifies a SAME-PACKAGE type name with its
// emitted package CLASS (`<pkg>_package`) for a `global using` alias RHS, which sits outside both
// `namespace go` and that class.
//
// For a package in a NESTED namespace (internal/fuzz → `go.@internal`, io/fs → `go.io`) the class
// alone roots the name one segment too shallow: a bare `fuzz_package/CorpusEntryᴛ1` renders
// `go.fuzz_package.CorpusEntryᴛ1`, but the lifted type lives at
// `go.@internal.fuzz_package.CorpusEntryᴛ1` → CS0234 at the `global using` line and every use
// (internal/fuzz's CorpusEntry, ×60). Prepend the namespace segments between the root and the class
// (`@internal`, `io`), taken from the SAME packageNamespace that emitted the `namespace …;`
// declaration so the two always agree. A top-level package's namespace is exactly RootNamespace,
// leaving no prefix segments, so the name stays `<pkg>_package/…`.
func samePackageTypeQualifier() string {
	nsSegments := strings.TrimPrefix(strings.TrimPrefix(packageNamespace, RootNamespace), ".")
	classQualifier := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

	if nsSegments == "" {
		return classQualifier + "/"
	}

	return nsSegments + "." + classQualifier + "/"
}


// DescriptorCarrierSuffix marks a converter-minted DESCRIPTOR CARRIER - the uninhabited interface
// that holds the Go name of a defined-over-interface type the emission erased to a `using` alias.
// U+1D05 LATIN LETTER SMALL CAPITAL D, chosen to sit in the same small-capital family as the
// existing temp (U+1D1B) and value-adapter (U+1D20) markers.
const DescriptorCarrierSuffix = "\u1D05"

// descriptorCarrierFor returns the fully-qualified C# name of the descriptor carrier for t, or ""
// when t needs none. This is the SAME predicate usingAliasTargetType applies (foreignTypeAliases.go)
// and it is deliberately the same three questions in the same order:
//
//  1. a Go type ALIAS gets NO carrier. Go reports the TARGET's name for one, so a carrier would
//     invent a wrong name rather than restore a lost one. Measured: 30 descriptor positions in the
//     corpus are aliases (os.FileInfo, os.DirEntry, net/http.http2timer).
//  2. a type whose underlying is not an interface gets none - it is emitted as a real named type
//     and already carries its name.
//  3. a DEFINED type over an interface gets one ONLY when the declaration's RHS is a NAMED type.
//     An inline definition (`type X interface{...}`) is a real nested C# interface and needs
//     nothing; that distinction lives only in the RHS syntax, which is why the declaring package's
//     handle is consulted. A package loaded without syntax yields "" rather than a guess.
func (v *Visitor) descriptorCarrierFor(t types.Type) string {
	if t == nil {
		return ""
	}

	named, isNamed := t.(*types.Named)

	if !isNamed {
		return ""
	}

	obj := named.Obj()

	if obj == nil || obj.Pkg() == nil || obj.IsAlias() {
		return ""
	}

	if _, isInterface := obj.Type().Underlying().(*types.Interface); !isInterface {
		return ""
	}

	handle := descriptorCarrierPackage(obj.Pkg())

	if handle == nil {
		return ""
	}

	switch definedTypeSpecRHS(handle, obj.Name()).(type) {
	case *ast.Ident, *ast.SelectorExpr:
	default:
		return ""
	}

	// The carrier's C# name must be the one the DECLARATION used, collision rename included: a
	// defined type whose name is also a method name in its own package is Δ-prefixed at the
	// declaration (encoding/xml declares `type Token any` alongside `(*Decoder).Token()`, so the
	// alias and its carrier are ΔToken/ΔTokenᴅ). Naming the unrenamed form here emits
	// `typeof(Tokenᴅ)` against a declared `ΔTokenᴅ` — CS0246, and caught by the first smoke run.
	// packageHasMethodNamed is the types-level form of the same test performNameCollisionAnalysis
	// applies, and is what the foreign-collision derivation already uses for this exact question.
	aliasName := getCoreSanitizedIdentifier(obj.Name())

	if packageHasMethodNamed(obj.Pkg(), obj.Name()) {
		aliasName = getCollisionAvoidanceIdentifier(obj.Name())
	}

	if obj.Pkg() == v.pkg {
		return aliasName + DescriptorCarrierSuffix
	}

	return fmt.Sprintf("%s.%s_package.%s%s", RootNamespace, convertToCSTypeName(obj.Pkg().Path()), aliasName, DescriptorCarrierSuffix)
}

// descriptorCarrierPackage resolves the loaded handle whose SYNTAX the predicate above reads - the
// package under conversion, or one of its imports.
func descriptorCarrierPackage(pkg *types.Package) *packages.Package {
	if pkg == nil {
		return nil
	}

	if currentPackageSource != nil && currentPackageSource.Types == pkg {
		return currentPackageSource
	}

	packageLock.Lock()
	handle := importedPackages[pkg.Path()]
	packageLock.Unlock()

	return handle
}

// golibAliasSafeNames maps the golib csproj-level `<Using Alias="...">` names to equivalents
// that resolve inside a `using` alias directive's RHS. C# resolves a using directive's target
// without reference to other using directives - aliases are not visible to one another - so a
// rendered `uint64`, valid everywhere else, fails CS0246 inside `global using X = ...;`.
// C# keywords (byte, bool, nint, nuint) and `go.`-qualified golib types (go.@string,
// go.complex64) are already safe and are not mapped.
var golibAliasSafeNames = map[string]string{
	"uint8":      "byte",
	"uint16":     "ushort",
	"uint32":     "uint",
	"uint64":     "ulong",
	"int8":       "sbyte",
	"int16":      "short",
	"int32":      "int",
	"int64":      "long",
	"float32":    "float",
	"float64":    "double",
	"complex128": "System.Numerics.Complex",
	"rune":       "int",
	"any":        "object",
	"GoBigConst": "System.Numerics.BigInteger",
}

// Matches a golib csproj-alias name standing alone as an identifier: at string start or after a
// type-syntax delimiter (`<`, `(`, `,`, space) - deliberately NOT after `.`, so a package-
// qualified user type that happens to share a builtin name is left untouched.
var golibAliasNameExpr = regexp.MustCompile(`(^|[<(, ])(uint8|uint16|uint32|uint64|int8|int16|int32|int64|float32|float64|complex128|rune|any|GoBigConst)\b`)

// getUsingAliasSafeTypeName rewrites golib csproj-alias type names inside a rendered C# type
// name into forms that resolve in a `using` alias RHS. Applied ONLY when emitting
// `global using <name> = <type>;` type-alias declarations - code-body renderings keep the
// Go-visual alias names.
func getUsingAliasSafeTypeName(typeName string) string {
	return golibAliasNameExpr.ReplaceAllStringFunc(typeName, func(match string) string {
		sub := golibAliasNameExpr.FindStringSubmatch(match)
		return sub[1] + golibAliasSafeNames[sub[2]]
	})
}
