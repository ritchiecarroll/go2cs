// typeNameResolution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns TYPE-NAME RENDERING: given a Go type, produce the string to write into C#.
//
// There are six of these and choosing the wrong one is a silent correctness bug the compiler
// cannot catch, so each name states WHO CONSUMES its output — which is the whole taxonomy:
//
//	getAliasQualifiedTypeName   the workhorse. Go-shaped name (`[]atomic.Int32`, `*Reader`) qualified
//	                            with whatever file-local package alias is in scope. Emitted into a
//	                            converted BODY, where visitFile has supplied the matching `using`.
//	                            Most-called by far, and the base every renderer below builds on.
//	getFullyQualifiedTypeName   the same Go-shaped name in its alias-free form
//	                            (`sync.atomic_package.Int32`). REQUIRED for anything a GENERATOR
//	                            consumes — GoType attribute strings and package_info records live in
//	                            files that carry no `using` directives, so an alias resolves to
//	                            nothing there.
//	getScopeCheckedTypeName     the alias-qualified form when every package it names is actually
//	                            imported by this file, and the fully-qualified form when one is not.
//	                            That check is the difference: it is the only renderer that GUARANTEES
//	                            its output resolves in the file receiving it. Body emission only —
//	                            never generator-consumed strings.
//	getCSharpTypeName           C#-shaped rather than Go-shaped: the Go->C# name mapping applied on
//	                            top of the alias-qualified form (`slice<byte>`, not `[]byte`). Reach
//	                            for it wherever the string lands in a C# TYPE position — a
//	                            declaration, an element type, a generic argument.
//	getExpressionTypeName       convenience over the workhorse: resolve an EXPRESSION to its type,
//	                            then name it.
//	getRefParameterTypeName     the C#-shaped spelling a `ref` parameter position needs — a pointer
//	                            renders as `ref T` rather than as a box.
//
// The rule of thumb: if the string lands in generated C# that a human reads, prefer the alias-
// qualified forms; if it lands in metadata a tool parses, use getFullyQualifiedTypeName.
//
// The alias plumbing (getAliasedTypeName, foreignAliasedTypeName, markDerivedTypeAliasUsed) lives
// here too, since it is what makes the short forms legal.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// getExpressionTypeName resolves expr to its type and names it with getAliasQualifiedTypeName —
// the convenience spelling for the common "I have an expression, I need its type name" caller.
//
// It carries one side effect the bare renderer cannot: a channel whose ELEMENT is an anonymous
// struct or interface must have that element lifted to a named declaration before anything can
// refer to it, so the lift happens here, once, the first time such a channel type is named.
func (v *Visitor) getExpressionTypeName(expr ast.Expr, underlying bool) string {

	if chanType, ok := expr.(*ast.ChanType); ok {
		// Check if the channel value is an anonymous struct
		if structType, exprType := v.extractStructType(chanType.Value); structType != nil && !v.liftedTypeExists(structType) {
			v.indentLevel++
			v.visitStructType(structType, exprType, "channel", nil, true, nil)
			v.indentLevel--
		}

		// Check if the channel value is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(chanType.Value); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			v.indentLevel++
			v.visitInterfaceType(interfaceType, exprType, "channel", nil, true, nil)
			v.indentLevel--
		}
	}

	return v.getAliasQualifiedTypeName(v.getType(expr, underlying), underlying)
}

// collectTypePackages records the import paths of every foreign (non-current-package) named type
// reachable in t — directly, or as a pointer/slice/array/map/chan element, a generic type argument,
// or a func-signature parameter/result — into referencedForeignPackages, plus the pseudo-path
// "unsafe" for an unsafe.Pointer basic. These are the packages whose short-alias type names
// (`pkg.Type`, `@unsafe.Pointer`) the converter emits, so visitFile can supply the matching
// `using <alias> = <namespace>;`. A named type is recorded by its own package only — its underlying
// fields belong to its own declaration and are not emitted inline, so the walk does not recurse into
// Underlying (which also avoids cycles on recursive struct types).
func (v *Visitor) collectTypePackages(t types.Type, seen map[types.Type]bool) {
	if t == nil {
		return
	}

	if seen == nil {
		seen = map[types.Type]bool{}
	}

	if seen[t] {
		return
	}

	seen[t] = true

	switch u := t.(type) {
	case *types.Basic:
		if u.Kind() == types.UnsafePointer {
			v.referencedForeignPackages.Add("unsafe")
		}
	case *types.Named:
		if obj := u.Obj(); obj != nil {
			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				v.referencedForeignPackages.Add(pkg.Path())
			}
		}

		if typeArgs := u.TypeArgs(); typeArgs != nil {
			for i := range typeArgs.Len() {
				v.collectTypePackages(typeArgs.At(i), seen)
			}
		}
	case *types.Pointer:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Slice:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Array:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Map:
		v.collectTypePackages(u.Key(), seen)
		v.collectTypePackages(u.Elem(), seen)
	case *types.Chan:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Signature:
		if params := u.Params(); params != nil {
			for i := range params.Len() {
				v.collectTypePackages(params.At(i).Type(), seen)
			}
		}

		if results := u.Results(); results != nil {
			for i := range results.Len() {
				v.collectTypePackages(results.At(i).Type(), seen)
			}
		}
	}
}

// methodlessNamedFuncSignature reports the underlying *types.Signature when t is a NON-GENERIC
// named func type with NO methods — `type releaseConn func(error)`, `context.CancelFunc`. Go treats
// such a type as freely interconvertible with its underlying `func(...)` (the name is purely
// documentary when there are no methods), but the converter would otherwise emit a distinct C#
// delegate (`ΔreleaseConn`) incompatible with the base `Action<error>` its underlying renders to —
// so a value flowing between the two (database/sql's grabConn returns `releaseConn`, queryDC takes
// `func(error)`) fails (CS1503/CS0029, and the mismatch blocks the ж-receiver overload → CS1929).
// Such a type is rendered AS its base delegate everywhere and its declaration is skipped
// (visitFuncType), making the conversions identity, exactly as Go models them. A named func type
// WITH methods keeps its distinct delegate (its method set is meaningful); a GENERIC one keeps its
// name (it is referenced as `Seq<V>`, and the type parameter must stay in scope).
func methodlessNamedFuncSignature(t types.Type) (*types.Signature, bool) {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return nil, false
	}

	if named.NumMethods() != 0 || named.TypeParams() != nil {
		return nil, false
	}

	sig, ok := named.Underlying().(*types.Signature)

	if !ok {
		return nil, false
	}

	// Don't collapse when the signature references ANY named func type (its own or another) in
	// its params/results. A SELF-referential func type — `type stateFn func(*machine) stateFn`
	// (a Go state machine) — has no finite base-delegate form (`Func<M, Func<M, …>>` is infinite),
	// and a reference to ANOTHER named func type would leave that name undefined after collapse
	// (FirstClassFunctions' `strategy func(score) action`). Keeping such a type as a named
	// delegate is correct and self-consistent; only the leaves of the func-type reference graph
	// (whose signatures name no func types — database/sql's `releaseConn func(error)`, context's
	// `CancelFunc func()`) collapse.
	if signatureReferencesNamedFuncType(sig) {
		return nil, false
	}

	return sig, true
}

// signatureReferencesNamedFuncType reports whether any param/result of sig is (or, through
// pointer/slice/array/map/chan wrappers, contains) a NAMED type whose underlying is a func
// signature — i.e. a named func type. Used to keep self- or mutually-referential func types as
// named delegates (see methodlessNamedFuncSignature). Struct fields are not descended into (a
// struct-typed param does not make the delegate recursive).
func signatureReferencesNamedFuncType(sig *types.Signature) bool {
	var referencesFunc func(t types.Type) bool

	referencesFunc = func(t types.Type) bool {
		switch typ := t.(type) {
		case *types.Named:
			if _, ok := typ.Underlying().(*types.Signature); ok {
				return true
			}
		case *types.Pointer:
			return referencesFunc(typ.Elem())
		case *types.Slice:
			return referencesFunc(typ.Elem())
		case *types.Array:
			return referencesFunc(typ.Elem())
		case *types.Map:
			return referencesFunc(typ.Key()) || referencesFunc(typ.Elem())
		case *types.Chan:
			return referencesFunc(typ.Elem())
		}

		return false
	}

	tuples := []*types.Tuple{sig.Params(), sig.Results()}

	for _, tuple := range tuples {
		if tuple == nil {
			continue
		}

		for i := range tuple.Len() {
			if referencesFunc(tuple.At(i).Type()) {
				return true
			}
		}
	}

	return false
}

// getAliasQualifiedTypeName renders t as the Go-shaped type name to write into a converted body,
// qualifying foreign types with the file-local package alias (`atomic.Int32`, `[]time.Duration`).
// It is the base every other renderer in this file builds on, and the most-called of the six.
//
// "Alias-qualified" is a promise about the FORM, not about scope: the alias is emitted whether or
// not this file imports the package, because collectTypePackages records every foreign package
// touched here and visitFile then supplies the matching `using <alias> = <namespace>;`. A caller
// that cannot rely on that plumbing — because it is emitting into a generated file with no usings —
// wants getFullyQualifiedTypeName; a caller that wants the short form only when it is provably
// legal wants getScopeCheckedTypeName.
//
// isUnderlying asks for a named type's underlying representation instead of its own name.
func (v *Visitor) getAliasQualifiedTypeName(t types.Type, isUnderlying bool) string {
	if t == nil {
		return ""
	}

	// A non-generic methodless named func type renders as its base C# delegate (its underlying
	// signature), matching Go's free named↔underlying func interconversion (see
	// methodlessNamedFuncSignature). Guarded so a pointer/composite wrapper still recurses here.
	if sig, ok := methodlessNamedFuncSignature(t); ok {
		v.collectTypePackages(t, nil)
		return v.getAliasQualifiedTypeName(sig, isUnderlying)
	}

	// Register any foreign package whose type is emitted here so visitFile can supply the file-local
	// `using <alias> = <namespace>;` even when the file did not import the package under its canonical
	// name (inferred type, blank/non-canonical alias import) — see the field comment. Walks composites
	// and generics so an element/argument type (`[]time.Duration`, `map[K]abi.Kind`, unsafe.Pointer
	// inside a slice) registers too, since those are emitted through the string path below without
	// recursing into getAliasQualifiedTypeName.
	v.collectTypePackages(t, nil)

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getAliasQualifiedTypeName(pointer.Elem(), isUnderlying)
	}

	// A type parameter whose constraint type-set is a single non-tilde pointer term (`[P *T]`)
	// is ERASED: the singleton type set makes P definitionally *T at every instantiation, so it
	// renders as the pointer type itself and the whole existing pointer rendering chain applies
	// (its element may itself be another type parameter, which renders by name). Gated on the
	// CURRENT function's identity set (typeParamErased) so a declined declaration's parameter —
	// a generic named type's, a receiver type parameter — keeps its bare name coherently rather
	// than half-erasing. Non-erased type parameters keep the t.String() fallthrough.
	if typeParam, ok := t.(*types.TypeParam); ok {
		if pointer, ok := v.typeParamErased(typeParam); ok {
			return "*" + v.getAliasQualifiedTypeName(pointer.Elem(), isUnderlying)
		}
	}

	// A FOREIGN type ALIAS whose TARGET lives in yet ANOTHER package — `os.FileInfo = fs.FileInfo`
	// (os/types.go, target in io/fs) — is emitted as an assembly-scoped `global using FileInfo =
	// go.io.fs_package.FileInfo;` in ITS OWN package's conversion, NOT as a member of that package's C#
	// class, so a cross-package reference `os_package.FileInfo` does not resolve (CS0426, path/filepath's
	// `os.Lstat` func value). Render the alias's TARGET instead. Gated to a DIFFERENT-package target: an
	// alias to a SAME-package type (`CrossPkgLib.Temperature = Celsius`) already resolves through the
	// existing `ꓸ` global-using alias, so it is left untouched (no churn on that mechanism).
	if alias, ok := t.(*types.Alias); ok {
		if aliasObj := alias.Obj(); aliasObj != nil && aliasObj.Pkg() != nil && aliasObj.Pkg() != v.pkg {
			if targetNamed, ok := types.Unalias(t).(*types.Named); ok {
				if targetObj := targetNamed.Obj(); targetObj != nil && targetObj.Pkg() != nil && targetObj.Pkg() != aliasObj.Pkg() {
					return v.getAliasQualifiedTypeName(targetNamed, isUnderlying)
				}
			}
		}
	}

	// The SAME rule, one assembly boundary further in: under the WHITE-BOX reference model the
	// production sources are not compiled into the test assembly, so a `global using` the PRODUCTION
	// emission declares is not in scope for a converted `_test.go`. Go says these are one package —
	// `export_test.go`'s `var Atime = atime` names `FileInfo` unqualified — but C# says the alias
	// lives in the referenced assembly and nothing here declares it (CS0246, os's export_test). Render
	// the alias's TARGET, exactly as the foreign arm above does for the same alias reached from
	// another package. An alias DECLARED by a `_test.go` file emits its own `global using` into this
	// assembly and is left alone, and the target is taken through types.Unalias so a non-Named target
	// (`type buf = []byte`) resolves too — the whole alias name, not just a cross-package one, is what
	// the assembly boundary swallows.
	if alias, ok := t.(*types.Alias); ok && v.options.testWhiteboxReference {
		if aliasObj := alias.Obj(); aliasObj != nil && aliasObj.Pkg() == v.pkg && !v.declaredInTestFile(aliasObj) {
			if target := types.Unalias(t); target != t {
				return v.getAliasQualifiedTypeName(target, isUnderlying)
			}
		}
	}

	// The MIRROR of that rule for the alias the arm above deliberately leaves alone. An alias a
	// `_test.go` file DECLARES emits its own `global using` into this assembly — but the assembly has
	// TWO variant classes, and the EXTERNAL one reaches the alias by package qualification, because
	// Go says `netip.AddrDetail` (export_test.go is part of package netip during a test build). A
	// `global using` is a member of no class, so that spelling is CS0426 — `AddrDetail` does not exist
	// in `netip_package`, net/netip's last one-line wall.
	//
	// Render it BARE. The internal half's `global using AddrDetail = …` is compilation-scoped and
	// this emission lands in the SAME compilation, so the bare name is exactly what resolves — and it
	// is the same spelling the internal half's own files use, which keeps one Go name to one C# name
	// across both halves. Rendering the TARGET instead would resolve too, but it would spell one
	// alias two different ways depending on which half named it.
	if name, bare := v.testDeclaredAliasSpelledBare(t); bare {
		return name
	}

	if name, ok := v.liftedNameFor(t); ok {
		return name
	}

	// An array/slice type is rendered structurally — the `[N]`/`[]` marker plus the recursively
	// resolved element — rather than via t.String() below (mirrors getFullyQualifiedTypeName). t.String()
	// yields a path-qualified string (`[]*internal/abi.Type`) whose cross-package last-segment
	// slash-strip eats everything before the slash INCLUDING a pointer marker, so the element's
	// `*` is silently dropped (`slice<abi.Type>` instead of `slice<ж<abi.Type>>` — reflect
	// CS1503 ×16, plus SILENTLY WRONG type asserts that compiled). Recursing on the element also
	// resolves a lifted element and a cross-package generic element through their own arms.
	switch composite := t.(type) {
	case *types.Array:
		return fmt.Sprintf("[%d]%s", composite.Len(), v.getAliasQualifiedTypeName(composite.Elem(), isUnderlying))
	case *types.Slice:
		return "[]" + v.getAliasQualifiedTypeName(composite.Elem(), isUnderlying)
	case *types.Chan:
		// Structural like slices/arrays so a LIFTED element resolves - `make(chan dialResult)`
		// where dialResult is a FUNCTION-LOCAL type (net dialParallel) rendered
		// `channel<dialResult>` while every composite used the lifted name (CS0246).
		elem := v.getAliasQualifiedTypeName(composite.Elem(), isUnderlying)

		switch composite.Dir() {
		case types.RecvOnly:
			return "<-chan " + elem
		case types.SendOnly:
			return "chan<- " + elem
		default:
			return "chan " + elem
		}
	case *types.Map:
		// Structural like slices/chans — the t.String() path renders a PACKAGE-LOCAL value
		// type path-qualified (`map[chan<- os.Signal]*os/signal.handler`), whose cross-package
		// slash-strip eats everything before the slash including the map header (os/signal's
		// handlers.m emitted `map<channel/*<-*/<os.Signal>*handler>, >` — CS1003 cascade ×8).
		return fmt.Sprintf("map[%s]%s", v.getAliasQualifiedTypeName(composite.Key(), isUnderlying), v.getAliasQualifiedTypeName(composite.Elem(), isUnderlying))
	case *types.Signature:
		// Structural like the composites above: t.String() embeds slash-qualified import paths
		// for cross-package elements (`func(...*go/types.Package...)`), which the string-path
		// slash heuristics mis-qualify three different ways (go/importer's gccgo importer field,
		// net/http's TLSNextProto maps, traceviewer's MutatorUtilFunc — CS0234). Rendering each
		// element recursively keeps the file's short import-alias form (see signatureTypeName).
		return v.signatureTypeName(composite, isUnderlying)
	}

	// The internal white-box variant contains production and test declarations in one Go package,
	// but emits them into separate C# classes. Qualify a production-declared type through the
	// referenced production class before the ordinary same-package branch can erase its owner.
	if named, ok := t.(*types.Named); ok {
		if bridgeName, isBridge := v.whiteboxBridgeNamedType(named); isBridge {
			return bridgeName
		}
		if productionName, isProduction := v.whiteboxProductionNamedType(named); isProduction {
			return productionName
		}

		// PARKED (docs/phase4/DESIGN-w3a-wrapper-scaffolding.md's own arc, COORD ruling 2026-08-30) —
		// see the identical note on getFullyQualifiedTypeName's twin arm; this is the SAME question
		// for the alias-qualified renderer (what getCSharpTypeName/parameter and return types
		// actually use), which is why the mgcscavenge_test.cs/mpallocbits_test.cs target case routes
		// through HERE, not the other function alone. Not built on, not banked for this arc.
		if obj := named.Obj(); obj.Pkg() == v.pkg && (named.TypeArgs() == nil || named.TypeArgs().Len() == 0) {
			if renamed := getSanitizedIdentifier(obj.Name()); renamed != obj.Name() {
				return renamed
			}
		}
	}

	// A cross-package INSTANTIATED generic (e.g. `internal/runtime/atomic.Pointer[func(string,
	// string)]`) must be rendered structurally — `pkg.Name() + "." + Name[args…]` with each arg
	// recursively named — rather than from t.String(). The string form keeps the full import path,
	// and the slash-strip that would reduce it is skipped whenever the string contains '(' (to
	// protect func types), so a func-type type-argument leaves the full path AND the pkg.Name()
	// alias gets prepended → a doubled `atomic.@internal.runtime.atomic.Pointer` (CS0426).
	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()

		if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
			args := make([]string, typeArgs.Len())

			for i := 0; i < typeArgs.Len(); i++ {
				if proxyName, ok := v.constraintProxyArg(named, i); ok {
					args[i] = proxyName
				} else {
					args[i] = v.getAliasQualifiedTypeName(typeArgs.At(i), false)
				}
			}

			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				return fmt.Sprintf("%s.%s[%s]", importQualifier(pkg.Name()), obj.Name(), strings.Join(args, ", "))
			}

			// A SAME-PACKAGE instantiated generic must ALSO render structurally — each type ARGUMENT
			// recursively named — rather than falling through to the t.String() path. When an argument
			// is itself cross-package, t.String() path-qualifies it (`curve[*repro/sub.Item]`), and the
			// cross-package slash-strip then eats everything before the slash INCLUDING the `curve[`
			// header, dropping the wrapper (crypto/elliptic's `*nistCurve[*nistec.P224Point]` →
			// `ж<nistec.P224Point>>`, a CS1519 cascade). Rendering args via getAliasQualifiedTypeName yields their
			// short, slash-free package-qualified names, so the header survives. getSanitizedIdentifier,
			// not the raw name, for the same reason the non-generic arm above takes it — a same-
			// package generic type can collision-rename too (PARKED, see that arm's note).
			return fmt.Sprintf("%s[%s]", getSanitizedIdentifier(obj.Name()), strings.Join(args, ", "))
		}
	}

	var pkgPrefix string
	var plainPkgPrefix string
	var foreignPathPrefix string

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package
		if pkg != nil && pkg != v.pkg {
			// Prefer THIS FILE's actual import alias for the type's package over the canonical
			// package name — cryptobyte's asn1.go imports `encoding/asn1` as `encoding_asn1`
			// (the vendored `.../cryptobyte/asn1` subpackage took the canonical `asn1`), so a
			// `*asn1.BitString` must render `encoding_asn1.BitString`, not `asn1.BitString` (which
			// resolves to the subpackage — CS0426). Only EXPLICITLY-aliased imports populate the map;
			// unaliased/Δ-renamed imports are absent and keep the importQualifier fallback — no churn.
			aliasQualifier := importQualifier(pkg.Name())

			if fileAlias, ok := v.importPathAliases[pkg.Path()]; ok && fileAlias != "" {
				aliasQualifier = fileAlias
			}

			pkgPrefix = aliasQualifier + "."
			plainPkgPrefix = pkg.Name() + "."
			foreignPathPrefix = pkg.Path() + "."
		}
	}

	// A NON-EMPTY anonymous struct/interface reaching this fallback was lifted by its
	// DECLARING file's visitor (this visitor's liftedTypeMap missed above — a cross-file
	// reference, or a same-file use ABOVE the declaration): resolve through the shared
	// package registry, or defer with a marker resolved after the file-visit barrier.
	// The raw t.String() fall-through below is never valid C# for these (B8: bytes'
	// `range compareTests` from bytes_test.go emitted `struct{a <>byte; …}` — CS1526
	// + ~170-error parser cascade).
	if !isUnderlying {
		if name := v.deferredDynamicTypeName(t); name != "" {
			return name
		}
	}

	typeName := strings.ReplaceAll(t.String(), "..", "")
	packagePathPrefix := v.pkg.Path() + "."

	// Remove the current package's path prefix from the type name. Use ReplaceAll, not a
	// single replace: a composite type (e.g. map[K]V) can name two current-package types, and
	// stripping only the first leaves a self-qualified one (which then also trips the slash
	// handling below for slash-bearing package paths like internal/platform → CS0246).
	typeName = strings.ReplaceAll(typeName, packagePathPrefix, "")

	// A FOREIGN named type's t.String() carries its full import-PATH qualifier; reduce it to the
	// package NAME here, because the generic last-segment slash-strip below assumes the path's
	// last segment IS the package qualifier. When the two differ — a major-version path tail —
	// the strip leaves the version segment behind as a phantom qualifier (`math/rand/v2.Rand` →
	// `v2.Rand`) and the alias prepend below then doubles it into `rand.v2.Rand` (`v2` read as a
	// member of class rand_package — CS0426, sort's test suite). An equal-name path reduces to
	// exactly what the slash-strip produced, so every existing emission is unchanged.
	if len(foreignPathPrefix) > 0 && strings.HasPrefix(typeName, foreignPathPrefix) {
		typeName = plainPkgPrefix + typeName[len(foreignPathPrefix):]
	}

	// The slash-strip below reduces a remaining cross-package import path to its last segment
	// (`internal/platform.Foo` → `platform.Foo`). It must NOT touch a composite type string whose
	// slashes are inside it — e.g. a func type `func(go2cs/x/sub.Record)` would be butchered to
	// `sub.Record)`. Such strings are converted structurally downstream (convertToCSFunc... +
	// convertImportPathToNamespace handle their inner package paths), so skip the strip for them.
	if !strings.HasPrefix(typeName, "func") && !strings.Contains(typeName, "(") {
		slashIndex := strings.LastIndex(typeName, "/")

		if slashIndex != -1 {
			typeName = typeName[slashIndex+1:]
		}
	}

	if len(pkgPrefix) > 0 && !strings.HasPrefix(typeName, pkgPrefix) {
		// A Δ-renamed import alias (importQualifier) diverges from the PLAIN package name that
		// t.String() carries (`sync.Pool` under alias `Δsync`): strip the plain qualifier before
		// prepending the alias, or the name doubles — `Δsync.sync.Pool`, `'sync' does not exist
		// in the type 'sync_package'` (io/syscall CS0426 ×22). When alias == plain name the
		// HasPrefix guard above already short-circuits, so this strip only fires on renames.
		if len(plainPkgPrefix) > 0 && strings.HasPrefix(typeName, plainPkgPrefix) {
			typeName = typeName[len(plainPkgPrefix):]
		}

		return pkgPrefix + typeName
	}

	return typeName
}

// getFullyQualifiedTypeName renders t as the Go-shaped type name in its alias-free form
// (`sync.atomic_package.Int32`), so the string resolves inside `namespace go;` with no `using` in
// scope. That is what generator-consumed metadata needs — GoType attribute strings and
// package_info records land in files the converter emits without any import aliases, where an
// alias-qualified name names nothing.
//
// It mirrors getAliasQualifiedTypeName arm for arm (pointers, erased pointer-core type params,
// methodless named func types, lifted anonymous types, array/slice/chan composites); the two must
// flip together, since a shape one renders and the other does not is a name that exists in only
// half the emission.
func (v *Visitor) getFullyQualifiedTypeName(t types.Type, isUnderlying bool) string {
	if t == nil {
		return ""
	}

	// A `global using` RHS may not name another using alias — C# resolves it with the compilation
	// unit's using directives NOT in effect — so an alias reached while rendering one must be
	// replaced by what it RESOLVES to, at any depth. `type second = first` (with `type first =
	// []Header`) otherwise renders the alias's own name and resolves to nothing.
	if v.inUsingAliasTarget {
		if unaliased := types.Unalias(t); unaliased != t {
			return v.getFullyQualifiedTypeName(unaliased, isUnderlying)
		}
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getFullyQualifiedTypeName(pointer.Elem(), isUnderlying)
	}

	// An ERASED pointer-core type parameter renders as its pointer type here too, keeping this
	// renderer in lockstep with getAliasQualifiedTypeName (the flip-together invariant: a `P` that no longer
	// exists in the emitted generic parameter list must never leak as a bare dangling name).
	if typeParam, ok := t.(*types.TypeParam); ok {
		if pointer, ok := v.typeParamErased(typeParam); ok {
			return "*" + v.getFullyQualifiedTypeName(pointer.Elem(), isUnderlying)
		}
	}

	// A non-generic methodless named func type renders as its base C# delegate (see
	// methodlessNamedFuncSignature / the getAliasQualifiedTypeName twin).
	if sig, ok := methodlessNamedFuncSignature(t); ok {
		return v.getFullyQualifiedTypeName(sig, isUnderlying)
	}

	if name, ok := v.liftedNameFor(t); ok {
		return v.usingAliasTypeQualifier(t) + name
	}

	// An array/slice type is rendered structurally — the `[N]`/`[]` marker plus the recursively
	// resolved element — rather than via t.String() below. t.String() yields a path-qualified string
	// (`[2]internal/runtime/atomic.Pointer[…]`) whose cross-package slash-strip would also strip the
	// leading `[N]` marker, dropping the array wrapper (`atomic.Pointer<…>` instead of
	// `array<atomic.Pointer<…>>`). Recursing on the element also resolves a lifted anonymous
	// struct/interface element (liftedTypeMap is keyed by the element) and a cross-package generic.
	switch composite := t.(type) {
	case *types.Array:
		return fmt.Sprintf("[%d]%s", composite.Len(), v.getFullyQualifiedTypeName(composite.Elem(), isUnderlying))
	case *types.Slice:
		return "[]" + v.getFullyQualifiedTypeName(composite.Elem(), isUnderlying)
	case *types.Chan:
		// Mirrors getAliasQualifiedTypeName's Chan arm (lifted channel elements; net dialParallel).
		elem := v.getFullyQualifiedTypeName(composite.Elem(), isUnderlying)

		switch composite.Dir() {
		case types.RecvOnly:
			return "<-chan " + elem
		case types.SendOnly:
			return "chan<- " + elem
		default:
			return "chan " + elem
		}
	}

	if named, ok := t.(*types.Named); ok {
		if bridgeName, isBridge := v.whiteboxBridgeNamedType(named); isBridge {
			return bridgeName
		}
		if productionName, isProduction := v.whiteboxProductionNamedType(named); isProduction {
			return productionName
		}

		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package. Compare package IDENTITY, not NAME: two DIFFERENT
		// packages can share a Go package name (html/template and text/template are both `package
		// template`), and a name-only check (`pkg.Name() != packageName`) then treats the foreign type
		// as same-package — falling through to the t.String() path whose cross-package slash-strip drops
		// both the path segment and the `_package` class (html/template's `type FuncMap = template.FuncMap`
		// emitted a `global using FuncMap = go.template.FuncMap;`, CS0234, instead of
		// `go.text.template_package.FuncMap`). getAliasQualifiedTypeName and collectCrossPackagePaths already key on
		// identity (`pkg != v.pkg`); align this path with them.
		if pkg != nil && pkg != v.pkg {
			baseName := getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + getSanitizedImport(obj.Name())

			// Append type arguments for an instantiated cross-package generic type (e.g.
			// atomic.Pointer[Config]). The qualified-name form above omits them, whereas the
			// local fall-through path keeps them via t.String(); without this, a boxed value
			// of such a type emits `new sync.atomic_package.Pointer()` (missing <Config>).
			if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
				args := make([]string, typeArgs.Len())

				for i := 0; i < typeArgs.Len(); i++ {
					if proxyName, ok := v.constraintProxyArg(named, i); ok {
						args[i] = proxyName
					} else {
						args[i] = v.getFullyQualifiedTypeName(typeArgs.At(i), isUnderlying)
					}
				}

				return baseName + "[" + strings.Join(args, ", ") + "]"
			}

			return baseName
		}

		// Empty everywhere except inside a `global using` alias RHS, where a same-package name has
		// to carry the package-class qualifier (see usingAliasTypeQualifier).
		qualifier := v.usingAliasTypeQualifier(t)

		// PARKED (docs/phase4/DESIGN-w3a-wrapper-scaffolding.md's own arc, COORD ruling 2026-08-30):
		// obj.Name() below is getSanitizedIdentifier'd, not raw — this is the SAME-VARIANT named-
		// type fall-through (the whitebox bridge/production arms above already handle cross-
		// variant references), reached whenever a `-tests` internal-bridge file references a TYPE
		// another file in the SAME variant declares — runtime's mgcscavenge_test.go/
		// mpallocbits_test.go naming export_test.go's `PallocBits`. The declaration side
		// (visitTypeSpec.go) already Δ-renames a colliding type name via this exact same helper;
		// this arm named the raw Go identifier instead, so a same-variant cross-file reference to a
		// collision-renamed type named the ORIGINAL (undeclared) name — CS0426. Real fix, but does
		// NOT resolve the target case alone (that reference routes through getAliasQualifiedTypeName,
		// not this function — see its own PARKED note), and its measured blast radius (two seeded
		// -stdlib reconverts diffed against each other) is real: ~25 files across go/types, reflect,
		// hash/maphash, net/http and others, unverified for correctness. Not built on, not banked as
		// a fix for this arc — parked here for the rebank wave that re-baselines those 25 anyway.
		if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
			args := make([]string, typeArgs.Len())

			for i := 0; i < typeArgs.Len(); i++ {
				if proxyName, ok := v.constraintProxyArg(named, i); ok {
					args[i] = proxyName
				} else {
					args[i] = v.getFullyQualifiedTypeName(typeArgs.At(i), isUnderlying)
				}
			}

			return qualifier + getSanitizedIdentifier(obj.Name()) + "[" + strings.Join(args, ", ") + "]"
		}

		if qualifier != "" {
			return qualifier + getSanitizedIdentifier(obj.Name())
		}
	}

	// Cross-file/forward reference to a lifted anonymous struct/interface: shared-registry
	// name or a deferred marker, never raw Go type text (see the getAliasQualifiedTypeName twin).
	if !isUnderlying {
		if name := v.deferredDynamicTypeName(t); name != "" {
			return v.usingAliasTypeQualifier(t) + name
		}
	}

	typeName := strings.ReplaceAll(t.String(), "..", "")
	packagePathPrefix := v.pkg.Path() + "."

	// Remove the current package's path prefix from the type name (ReplaceAll so a composite
	// type naming two current-package types doesn't keep a self-qualified one — see getAliasQualifiedTypeName).
	//
	// A `global using` alias RHS SUBSTITUTES the package class for that path rather than dropping
	// it, because the alias resolves where a bare same-package name resolves to nothing. This tail
	// is where the shapes the structural arms above do not decompose land — a map's key/value, a
	// func type's parameters and results — so `type m = map[string]Header` needs it to reach
	// `go.map<go.@string, go.main_package.Header>` (see usingAliasTypeQualifier).
	packagePathReplacement := ""

	if qualifier := v.usingAliasTypeQualifier(t); qualifier != "" {
		packagePathReplacement = strings.TrimSuffix(qualifier, "/") + "."
	}

	typeName = strings.ReplaceAll(typeName, packagePathPrefix, packagePathReplacement)

	// Skip the cross-package last-segment strip for composite/func type strings whose slashes are
	// internal (e.g. `func(go2cs/x/sub.Record)`); they are converted structurally downstream.
	if !strings.HasPrefix(typeName, "func") && !strings.Contains(typeName, "(") {
		slashIndex := strings.LastIndex(typeName, "/")

		if slashIndex != -1 {
			typeName = typeName[slashIndex+1:]
		}
	}

	return typeName
}

// usingAliasTypeQualifier returns the `<ns>.<pkg>_package/` prefix a SAME-PACKAGE type name needs
// while a `global using` alias RHS is being rendered, and "" in every other context.
//
// A using alias resolves at COMPILATION scope — outside `namespace go` and outside the emitted
// `<pkg>_package` class — so a same-package type is `go.<pkg>_package.T` there, never a bare `T`.
// It applies at EVERY depth: to the alias target itself (which is why visitTypeSpec no longer
// computes a prefix of its own) and to every type the target nests — a slice element, a map
// key/value, a channel element, a generic argument (`global using hdrs = go.slice<Header>;` was
// CS0246 on `Header`). A cross-package name is excluded because it already qualifies itself.
//
// The `/` terminator is not decoration: convertToCSFullTypeName treats what precedes it as an
// import PATH, which is what carries `<pkg>_package/T` through to `go.<pkg>_package.T`.
func (v *Visitor) usingAliasTypeQualifier(t types.Type) string {
	if !v.inUsingAliasTarget {
		return ""
	}

	if named, ok := t.(*types.Named); ok {
		if obj := named.Obj(); obj == nil || obj.Pkg() == nil || obj.Pkg() != v.pkg {
			return ""
		}
	}

	return samePackageTypeQualifier()
}

// collectCrossPackagePaths gathers the import paths of every cross-package named type referenced
// (recursively) by t — through pointers, arrays/slices, maps, channels and generic type arguments.
// Used by getScopeCheckedTypeName to decide whether the file-local package aliases for those packages
// are all in scope.
func (v *Visitor) collectCrossPackagePaths(t types.Type, paths HashSet[string]) {
	switch tt := t.(type) {
	case *types.Pointer:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Array:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Slice:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Chan:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Map:
		v.collectCrossPackagePaths(tt.Key(), paths)
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Named:
		if pkg := tt.Obj().Pkg(); pkg != nil && pkg != v.pkg {
			paths.Add(pkg.Path())
		}

		if typeArgs := tt.TypeArgs(); typeArgs != nil {
			for i := 0; i < typeArgs.Len(); i++ {
				v.collectCrossPackagePaths(typeArgs.At(i), paths)
			}
		}
	}
}

// getScopeCheckedTypeName resolves a type name for emission into the CURRENT source file's body,
// preferring the readable file-local package alias (`atomic.Int32`, via getAliasQualifiedTypeName)
// over the fully-qualified form (`sync.atomic_package.Int32`, via getFullyQualifiedTypeName) — but
// ONLY when every cross-package type it references is imported in this file, so the alias is
// guaranteed in scope. That check is the "scope-checked" in the name, and it is what separates this
// renderer from its two siblings: it is the one that guarantees its output resolves where it lands.
//
// When a referenced package is not imported here (e.g. a file indexing an atomic-typed array field
// without ever naming the element type → no `using atomic`), it falls back to the fully-qualified
// form, which resolves inside `namespace go;` without an alias. This keeps the converted C#
// visually close to the Go source while staying compilable. NOT for GoType attribute strings or
// other generator-consumed strings, which live in alias-less generated files and must always use
// getFullyQualifiedTypeName.
//
// Unlike its two siblings this takes no isUnderlying flag. Every caller wants the type AS DECLARED
// — the name a reader of the Go source would recognize; asking for a named type's underlying
// representation here would defeat the whole point of the function.
func (v *Visitor) getScopeCheckedTypeName(t types.Type) string {
	// A foreign renamed type resolves to the recorded imported-type alias (see
	// foreignAliasedTypeName). Doing it at this layer and not in getAliasQualifiedTypeName leaves
	// promoted-member naming, which reads the Go-shaped name, untouched.
	if aliased, ok := v.foreignAliasedTypeName(t); ok {
		return aliased
	}

	paths := HashSet[string]{}
	v.collectCrossPackagePaths(t, paths)

	for _, path := range paths.Keys() {
		if !v.importQueue.Contains(path) {
			return v.getFullyQualifiedTypeName(t, false)
		}
	}

	return v.getAliasQualifiedTypeName(t, false)
}

// markDerivedTypeAliasUsed records that an emitted reference resolved through a DERIVED imported
// type alias, which is what qualifies its `global using` for emission (see derivedTypeAliases). A
// parsed alias is unaffected — those are always emitted, used or not.
func markDerivedTypeAliasUsed(alias string) {
	packageLock.Lock()
	defer packageLock.Unlock()

	if derivedTypeAliases.Contains(alias) {
		usedDerivedTypeAliases.Add(alias)
	}
}

func getAliasedTypeName(typeName string) string {
	packageLock.Lock()
	alias, exists := importedTypeAliases[typeName]
	isConst := constImportedTypeAliases.Contains(typeName)
	packageLock.Unlock()

	if exists {
		if isConst {
			parts := strings.Split(typeName, ".")

			if len(parts) == 1 {
				return alias
			}

			qualifier := strings.Join(parts[:len(parts)-1], ".")

			// A CONST entry keeps the member qualified through the package, so the qualifier must
			// be the file-local using ALIAS — which is Δ-renamed when a same-named child namespace
			// is visible (importAliasOperations.go). The raw package name was carried through
			// unchanged, so a Δ-shadowed import emitted the collision-renamed member against the
			// UNRENAMED qualifier: `time.ΔNanosecond` where the file declares
			// `using Δtime = time_package;` and bare `time` binds the go.time CHILD namespace
			// (CS0234 ×44 in time's own external test files, whose blank import of time/tzdata is
			// what puts that namespace in the assembly). Single-segment only — an already
			// `_package`- or `global::`-qualified spelling is not an import alias and is left alone.
			if len(parts) == 2 {
				qualifier = importQualifier(qualifier)
			}

			return fmt.Sprintf("%s.%s", qualifier, alias)
		}

		// This reference resolves through the alias, so a DERIVED entry has earned its
		// `global using` declaration in this package's package_info.cs (see derivedTypeAliases).
		markDerivedTypeAliasUsed(typeName)

		return strings.ReplaceAll(typeName, ".", TypeAliasDot)
	}

	// The file-local using for this qualifier may be COLLISION-RENAMED (`using Δio =
	// io_package;` — `io` collides with the go.io CHILD NAMESPACE once io/fs is in the
	// reference closure): rewrite the qualifier so the type reference binds the renamed
	// alias (mime's `CharsetReader Func<@string, io.Reader, …>` field, CS0234 ×6).
	if qualifier, rest, found := strings.Cut(typeName, "."); found {
		if renamed, ok := packageImportAliasRenames[qualifier]; ok {
			// No recursion: a foreign type WITH its own rename already hit the alias map
			// above under the raw key; this qualifier rewrite is the final form.
			return renamed + "." + rest
		}
	}

	// A `_package`-QUALIFIED member (a same-package method/func name shadows the import's
	// using alias, so convIdent qualifies the package ident through its static class — the
	// compress/flate CS0119 fallback) must still consult the alias map by the PLAIN package
	// name: time Δ-renames const `Second` (const-vs-method collision with `Time.Second()`),
	// so the composed `time_package.Second` missed the `time.Second` entry and bound the
	// `Second(this Time)` extension method group instead (CS0019 ×2, crypto/tls's
	// `Config.time` method). On a CONST hit, keep the `_package` qualifier — only the
	// member is Δ-renamed inside the package class. Gated to consts: the const entries
	// exist only for collision-renamed members, while the TYPE entries cover every exported
	// type, whose raw `_package`-qualified renders already bind correctly.
	if lastDot := strings.LastIndex(typeName, "."); lastDot > 0 {
		qualifier, member := typeName[:lastDot], typeName[lastDot+1:]
		pkgSegment := qualifier[strings.LastIndex(qualifier, ".")+1:]

		if rootPackageName, isPkgQualified := strings.CutSuffix(pkgSegment, PackageSuffix); isPkgQualified {
			plainKey := rootPackageName + "." + member

			packageLock.Lock()
			alias, exists := importedTypeAliases[plainKey]
			isConst := constImportedTypeAliases.Contains(plainKey)
			packageLock.Unlock()

			if exists && isConst {
				return qualifier + "." + alias
			}
		}
	}

	// A Δ-renamed IMPORT qualifier (gif's `color` import arrives shadow-renamed `Δcolor`)
	// must consult the alias map by the RAW package name — the global-using alias
	// (`colorꓸRGBA = go.image.color_package.ΔRGBA`) resolves namespace-wide regardless of
	// the file-local rename; the composed `Δcolor.RGBA` missed the map and kept the RAW
	// foreign type name, which is Δ-renamed in image/color (CS0426 ×4, image/gif). A type
	// the map does NOT rename (Δcolor.Palette) keeps the renamed-import qualifier.
	if strings.Contains(typeName, ".") {
		parts := strings.SplitN(typeName, ".", 2)

		if raw, wasShadow := strings.CutPrefix(parts[0], ShadowVarMarker); wasShadow {
			if resolved := getAliasedTypeName(raw + "." + parts[1]); resolved != raw+"."+parts[1] {
				return resolved
			}
		}
	}

	return typeName
}

// getRefParameterTypeName renders t as the C#-shaped spelling a `ref` parameter position needs.
// A Go pointer parameter that the converter passes by reference is `ref T`, not the `ж<T>` box the
// ordinary C# rendering would produce — the box is the heap-allocated form, while a `ref` param is
// the caller's own storage.
func (v *Visitor) getRefParameterTypeName(t types.Type) string {
	typeName := v.getAliasQualifiedTypeName(t, false)

	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("ref %s", convertToCSTypeName(typeName[1:]))
	}

	return convertToCSTypeName(typeName)
}

// getCSharpTypeName renders t C#-shaped rather than Go-shaped: the alias-qualified Go name mapped
// through convertToCSTypeName, so `[]byte` arrives as `slice<byte>` and `*T` as `ж<T>`. Reach for
// it wherever the string lands in a C# TYPE position — a declaration, an element type, a generic
// argument — and for its siblings above wherever the string is Go-shaped text or tool-read metadata.
//
// Func types do NOT take the string path; they are built structurally from the signature, for the
// reasons the two guards below give.
func (v *Visitor) getCSharpTypeName(t types.Type) string {
	// Render a func type structurally as an Action/Func delegate. The string-based path mangles
	// func types whose parameter/result types carry slash-bearing package paths (the slash-strip in
	// getAliasQualifiedTypeName chops `func(*math/rand.Rand)` to `*math/rand.Rand)`), and emits Go field order for
	// a named multi-result tuple. iifeDelegateType builds it from the signature using getCSharpTypeName
	// per element (correct qualification; nameless tuple results). Simple func types render
	// identically to the old path, so this is zero-churn for them.
	// An ANONYMOUS func type (t itself is the signature) is expanded here; a NAMED func type WITH
	// methods (or generic / self-referential) keeps its distinct delegate name via the normal path.
	if sig, ok := t.(*types.Signature); ok {
		return v.iifeDelegateType(sig)
	}

	// A methodless named func type collapses to its base delegate (methodlessNamedFuncSignature);
	// render it structurally via iifeDelegateType — the same correct-qualification path the
	// anonymous signature above takes — rather than through convertToCSTypeName(getAliasQualifiedTypeName(t)).
	// getAliasQualifiedTypeName collapses it to its signature and emits the STRING form
	// (`func(map[string]*go/ast.Object) …`), whose string-path package-path conversion mangles a
	// slash-bearing cross-package element to a naive namespace form (`go.ast.Object` — no
	// `_package` class, no file alias): CS0234, and the resulting error-typed delegate then fails
	// the method-group conversion of a func passed to it (go/doc wraps `simpleImporter` for
	// `ast.NewPackage`'s `ast.Importer` param — `new Func<…go.ast.Object…>(simpleImporter)`, CS0123).
	// iifeDelegateType names each element via aliasedElementTypeName, so `ast.Object` keeps its alias.
	if sig, ok := methodlessNamedFuncSignature(t); ok {
		return v.iifeDelegateType(sig)
	}

	if aliased, ok := v.foreignAliasedTypeName(t); ok {
		return aliased
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(t, false))
}

// foreignAliasedTypeName resolves a cross-package type that is RENAMED (or Go-aliased) inside
// its own package — syscall declares `ΔHandle` for its type-vs-method-colliding `Handle` — to
// the recorded imported-type alias (`syscallꓸHandle` = `go.syscall_package.ΔHandle`): the raw
// qualified render (`Δsyscall.Handle`) names a type that does not exist (CS0426 ×21,
// internal/poll's signatures, fields, conversion targets, and local declarations). This lives
// at the C#-NAME layers ONLY (getCSharpTypeName / getScopeCheckedTypeName / conversion targets), never
// in getAliasQualifiedTypeName: the Go-shaped name also feeds promoted-embed MEMBER naming, where the alias
// substitution renamed and rescoped the generated accessors (reflect CS8799 ×3 regression on
// the first cut). A type without a registered alias, and every generic instantiation, keeps
// the plain render (no churn).
func (v *Visitor) foreignAliasedTypeName(t types.Type) (string, bool) {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok || (named.TypeArgs() != nil && named.TypeArgs().Len() > 0) {
		return "", false
	}

	pkg := named.Obj().Pkg()

	if pkg == nil || pkg == v.pkg {
		return "", false
	}

	plainKey := fmt.Sprintf("%s.%s", getSanitizedIdentifier(pkg.Name()), getCoreSanitizedIdentifier(named.Obj().Name()))

	packageLock.Lock()
	_, aliasExists := importedTypeAliases[plainKey]
	packageLock.Unlock()

	if !aliasExists {
		return "", false
	}

	return getAliasedTypeName(plainKey), true
}

// namedFuncTypeNameForSignature returns the C# delegate name of a package-level named func type whose
// underlying signature is identical to sig, or "" if none exists. Go's `:=` on a bare function value
// (`state := lexText`, where `lexText` returns the named func type `stateFn`) infers the variable's
// type as the UNNAMED signature `func(*lexer) stateFn`, not the named `stateFn` — so naming the local
// structurally emits a `Func<…>` delegate that is a DISTINCT C# type from the `stateFn` delegate the
// function group actually produces and that later `state = state(l)` assignments yield (CS0029, no
// implicit conversion between two delegate types). Recovering the named delegate lets the local take
// the single interconvertible delegate type (the classic self-referential state-machine func type).
func (v *Visitor) namedFuncTypeNameForSignature(sig *types.Signature) string {
	if sig == nil || v.pkg == nil {
		return ""
	}

	// A generic signature (type params or receiver) is never a plain named func type match.
	if sig.TypeParams() != nil || sig.RecvTypeParams() != nil || sig.Recv() != nil {
		return ""
	}

	scope := v.pkg.Scope()

	if scope == nil {
		return ""
	}

	for _, name := range scope.Names() {
		obj := scope.Lookup(name)

		typeName, ok := obj.(*types.TypeName)

		if !ok {
			continue
		}

		named, ok := typeName.Type().(*types.Named)

		if !ok || named.TypeParams() != nil {
			continue
		}

		underlyingSig, ok := named.Underlying().(*types.Signature)

		if !ok {
			continue
		}

		if types.Identical(underlyingSig, sig) {
			return getSanitizedIdentifier(named.Obj().Name())
		}
	}

	return ""
}

// exprIsMethodGroup reports whether expr is a bare reference to a function or method value (a C#
// method group), i.e. an identifier or selector whose resolved object is a *types.Func and which is
// not itself the call in a call expression. Such a value has no inferable delegate type under `var`.
//
// An EXPLICIT instantiation of a generic function — `equal[int]`, `cmp.Compare[int]`,
// `Equal[Slice]` — is still a method group: writing the type arguments fixes the group's SHAPE, not
// its C#-inference status. Peeling the index form matters most to callHasMethodGroupArg, where the
// omission was eight of slices' seventeen compile errors (`EqualFunc(s1, s2, equal[int])`,
// `CompareFunc(s1, s2, cmp.Compare[int])`, `CompactFunc(s, equal[int])`, `equalToCmp(equal[int])` —
// every one CS0411): the predicate met an *ast.IndexExpr, which matched neither case, answered "not
// a method group", and the enclosing generic call kept its uninferable bare form. Indexing a
// function VALUE is not legal Go, so peeling can never reclassify an ordinary map/slice index.
func (v *Visitor) exprIsMethodGroup(expr ast.Expr) bool {
	var ident *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		ident = e
	case *ast.SelectorExpr:
		ident = e.Sel
	case *ast.ParenExpr:
		return v.exprIsMethodGroup(e.X)
	case *ast.IndexExpr:
		return v.exprIsMethodGroup(e.X)
	case *ast.IndexListExpr:
		return v.exprIsMethodGroup(e.X)
	default:
		return false
	}

	if ident == nil {
		return false
	}

	_, isFunc := v.info.ObjectOf(ident).(*types.Func)

	return isFunc
}

// discardTargetTypeName returns the C# delegate type a blank-identifier DISCARD must name for a
// func-typed right-hand side that has no C# type of its own, or "" when the RHS types itself. A
// discard infers its type from its RHS, and two forms cannot supply one: a METHOD GROUP
// (`_ = net.ResolveIPAddr`, debug/elf's file_test.go, which Go writes to force dynamic linkage)
// and a LAMBDA — a func literal, or the method VALUE that converts to one. Both are CS8183.
//
// A package named func type whose underlying signature matches is preferred over the structural
// `Func<…>`/`Action<…>` render, so the reader sees the Go type's own name; for a discard either is
// sound, since nothing downstream observes the value. This is the discard analogue of the `:=`
// path's namedFuncTypeNameForSignature branch, which faces the same typeless RHS one statement
// form over — but there the type must be DECLARED, and here it must not be (see visitAssignStmt's
// isDiscarded).
func (v *Visitor) discardTargetTypeName(expr ast.Expr) string {
	switch expr.(type) {
	case *ast.Ident, *ast.SelectorExpr:
		// A bare func/method reference. A func-typed VARIABLE reaching here is already a
		// delegate-typed C# expression and needs no cast.
		if !v.exprIsMethodGroup(expr) {
			return ""
		}
	case *ast.FuncLit:
	default:
		return ""
	}

	sig, ok := v.getExprType(expr).(*types.Signature)

	if !ok {
		return ""
	}

	if namedType := v.namedFuncTypeNameForSignature(sig); namedType != "" {
		return namedType
	}

	return v.iifeDelegateType(sig)
}

func convertToCSTypeName(typeName string) string {
	return renderCSTypeName(typeName, false)
}

// renderCSTypeName is convertToCSTypeName with the ROOTED-NESTING switch the `global using`
// alias RHS needs (see renderCSFullTypeName). With rootNested set the root namespace is KEPT
// rather than elided, so a nested name renders in its fully qualified form.
func renderCSTypeName(typeName string, rootNested bool) string {
	fullTypeName := renderCSFullTypeName(typeName, rootNested)

	if rootNested {
		return fullTypeName
	}

	// If full type name starts with root namespace, remove it
	if strings.HasPrefix(fullTypeName, RootNamespace+".") {
		return fullTypeName[len(RootNamespace)+1:]
	}

	return fullTypeName
}

// importPathStart returns the index at which the package IMPORT PATH begins inside a rendered type
// string whose leading type CONSTRUCTOR has not been peeled off yet: 7 for
// `<-chan go.mongodb.org/…`, 1 for `*io/fs`, 2 for `[]io/fs`, 0 for a bare `io/fs`.
//
// convertToCSFullTypeName rewrites an import path to its C# namespace BEFORE the branches that peel
// `<-chan `/`chan `/`*`/`[]` run, and it used to hand convertImportPathToNamespace everything from
// index 0 — constructor included. That sanitizer maps a HYPHEN to an underscore, because a Go path
// element may legally contain one (mongo-driver, go-isatty), so it rewrote the `-` of `<-chan` as
// well: `<-chan go.mongodb.org/mongo-driver/mongo/description_package.Topology` came back as
// `<_chan go.mongodb.org.mongo_driver.…`. No channel branch recognizes `<_chan`, so the string fell
// into the ARRAY branch, whose `]` was nowhere to be found — and that branch then re-entered on the
// unchanged string forever (issue #33's `fatal error: stack overflow`). Rewriting only the path
// leaves `<-chan ` intact for its own branch, which recurses on the bare qualified element exactly
// as it always has for a single-segment path — `<-chan time_package.Time` never reached this code
// at all, having no slash to trigger it, which is why the whole standard library renders such a
// field correctly and only a MODULE dependency's multi-segment path could expose this.
//
// The scan runs BACKWARD from the end of the candidate region and stops at the first byte no import
// path may contain. `-` cannot serve as that delimiter (mongo-driver would split mid-path), but
// every constructor this renderer emits ends in one that can: a space (`<-chan `, `chan `,
// `chan<- `), `*`, `]` (`[]`, `[2]`, `map[K]`) or `(` (`func(`).
func importPathStart(typeName string) int {
	for i := len(typeName) - 1; i >= 0; i-- {
		if !isImportPathByte(typeName[i]) {
			return i + 1
		}
	}

	return 0
}

// isImportPathByte reports whether c can be part of a package path or the qualified type name that
// follows it — letters, digits and the `-._~+/` set Go allows in a path element, the `@` of an
// escaped keyword segment, and any NON-ASCII byte.
//
// The non-ASCII case is the one worth stating. Every delimiter this predicate exists to find is a
// type CONSTRUCTOR character, and all of those are ASCII; meanwhile the converter's own synthetic
// markers are not (`ᴛ` of a lifted `entryᴛ1`, `ж`, `Ꮡ`, `ꓸ`, `Δ` — see symbols.go). Treating a
// multi-byte rune as a delimiter stops the backward scan INSIDE the type name, freezing the package
// path in front of it verbatim, and a rendered lifted type comes back with its separator unconverted
// (`go.main_package/entryᴛ1` for `go.main_package.entryᴛ1`, CS1002). Caught by CNR on the
// PublicizedInterfaceAnonAlias and NestedAliasUser goldens, which is exactly what those anonymous-
// alias guards are for.
func isImportPathByte(c byte) bool {
	switch {
	case c >= 'a' && c <= 'z', c >= 'A' && c <= 'Z', c >= '0' && c <= '9', c >= 0x80:
		return true
	}

	return strings.IndexByte("-._~+/@", c) >= 0
}

func convertToCSFullTypeName(typeName string) string {
	return renderCSFullTypeName(typeName, false)
}

// delegateRoot returns the namespace qualifier a rendered delegate name needs when the rendering
// must resolve at COMPILATION scope (rootNested — see renderCSFullTypeName). The two delegate
// spellings root to DIFFERENT namespaces: plain `Action`/`Func` are the BCL delegates in `System`,
// while the variadic FAMILY `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` (family == EllipsisOperator) is golib's, in the
// root namespace. Outside that context it contributes nothing, since both are in scope there.
func delegateRoot(rootNested bool, family string) string {
	if !rootNested {
		return ""
	}

	if family == EllipsisOperator {
		return RootNamespace + "."
	}

	return "System."
}

// renderCSFullTypeName is convertToCSFullTypeName plus the ROOTED-NESTING switch.
//
// Every rendering in this file roots only the OUTERMOST name and elides the root namespace from
// each nested one (`go.slice<@string>`), because every code-body rendering lands inside
// `namespace go;` where the elision is both legal and what makes the emitted C# read like Go.
// A `global using` alias RHS is the one context where that is wrong: C# resolves a using-alias
// target as if the compilation unit had NO using directives, so it resolves at COMPILATION scope
// — outside `namespace go` and outside the package class — where a nested `@string`, `slice`,
// `error`, `Header` or `io_package.Reader` names nothing at all (CS0246 ×17 on a nine-alias
// program). With rootNested set every nested name keeps its full qualification, so the whole RHS
// resolves from the global namespace.
//
// Two families are deliberately NOT rooted, both because they are not members of `go` to begin
// with: the golib csproj-level `<Using Alias=…>` names (`uint64`, `float64`, `any`, …), which
// resolve to a C# keyword or BCL type and are mapped straight to it here — the same substitution
// getUsingAliasSafeTypeName applies to the outermost name, for the same reason; and the BCL
// `Action`/`Func` delegates, which root to `System.` rather than to `go.` (the golib variadic
// FAMILY `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` does live in `go`, and is rooted there).
func renderCSFullTypeName(typeName string, rootNested bool) string {
	typeName = strings.TrimPrefix(typeName, "~")

	if strings.HasPrefix(typeName, "untyped ") {
		typeName = strings.TrimPrefix(typeName, "untyped ")

		if strings.HasPrefix(typeName, "int") || strings.HasPrefix(typeName, "uint") || typeName == "rune" || typeName == "byte" {
			return "UntypedInt"
		}

		if strings.HasPrefix(typeName, "float") {
			return "UntypedFloat"
		}

		if strings.HasPrefix(typeName, "complex") {
			return "UntypedComplex"
		}
	}

	if strings.Contains(typeName, "/") {
		// A package-qualified TYPE string carries a subpackage PATH plus a trailing `.TypeName` —
		// `io/fs.DirEntry`, `internal/abi.Type` (a func-type param rendered from t.String(), whose
		// import alias was lost). Converting the WHOLE thing as one import path drops the package CLASS
		// suffix and dots the type straight into the namespace (`io.fs.DirEntry`, CS0234 — `fs` is not a
		// namespace of `go.io`; the type lives in class `fs_package`). Split the trailing type off: the
		// package path ends at the first `.` AFTER the last path `/`, so `io/fs` → `io.fs_package` and
		// the `.DirEntry` (plus any `[…]` generic args) re-appends. Falls back to the whole-path form
		// when there is no trailing type (a bare import path).
		genericStart := strings.IndexByte(typeName, '[')
		scanEnd := len(typeName)

		if genericStart != -1 {
			scanEnd = genericStart
		}

		lastSlash := strings.LastIndex(typeName[:scanEnd], "/")
		dotAfterSlash := -1

		if lastSlash != -1 {
			dotAfterSlash = strings.IndexByte(typeName[lastSlash:scanEnd], '.')
		}

		if dotAfterSlash != -1 {
			splitAt := lastSlash + dotAfterSlash

			// The path starts where the TYPE CONSTRUCTOR in front of it ends — a `<-chan `, `*`,
			// `[]` or `map[K]` that the branches below have not peeled yet is NOT part of the
			// package path and must survive this rewrite untouched (see importPathStart).
			pathStart := importPathStart(typeName[:splitAt])
			pkgPath := typeName[pathStart:splitAt]

			// Some callers hand a path whose last segment ALREADY carries the class suffix
			// (`sync/atomic_package.Uint32`, from a recorded `[GoType]` underlying); others hand the
			// raw path (`io/fs.DirEntry`, from a signature's t.String()). Only append the suffix when
			// it is not already present, or it doubles (`atomic_package_package`).
			suffix := PackageSuffix

			if strings.HasSuffix(pkgPath[strings.LastIndexByte(pkgPath, '/')+1:], PackageSuffix) {
				suffix = ""
			}

			typeName = typeName[:pathStart] + convertImportPathToNamespace(pkgPath, suffix) + typeName[splitAt:]
		} else {
			// A composite whose ELEMENT is the qualified type reaches here too, not only a bare path:
			// the `[` of a leading `[]`/`[N]` constructor is read as the start of a generic argument
			// list above, truncating scanEnd in front of every slash. The constructor is still not
			// part of the path, and skipping it is what keeps `<-chan []description.Topology` from
			// being sanitized into `<_chan <>…` — the same mangling, one nesting level down.
			//
			// The scan is bounded by scanEnd, not by the whole string: past it lie the generic
			// ARGUMENTS, whose `,`/space/`]` are not constructor text and would strand the scan at
			// the tail of the string, leaving the path in front of them unconverted.
			pathStart := importPathStart(typeName[:scanEnd])

			typeName = typeName[:pathStart] + convertImportPathToNamespace(typeName[pathStart:], "")
		}
	}

	// Replace all `[` and `]` with `<` and `>` to handle generic types
	typeName = strings.ReplaceAll(typeName, "[", "<")
	typeName = strings.ReplaceAll(typeName, "]", ">")

	if strings.HasPrefix(typeName, "<>") {
		return fmt.Sprintf("%s.slice<%s>", RootNamespace, renderCSTypeName(typeName[2:], rootNested))
	}

	if strings.HasPrefix(typeName, "chan ") {
		return fmt.Sprintf("%s.channel<%s>", RootNamespace, renderCSTypeName(typeName[5:], rootNested))
	}

	if strings.HasPrefix(typeName, "chan<- ") {
		return fmt.Sprintf("%s.channel/*<-*/<%s>", RootNamespace, renderCSTypeName(typeName[7:], rootNested))
	}

	if strings.HasPrefix(typeName, "<-chan ") {
		return fmt.Sprintf("%s./*<-*/channel<%s>", RootNamespace, renderCSTypeName(typeName[7:], rootNested))
	}

	// Handle array types — the leading `<` is the `[` of `[N]`, so its `>` closes the length.
	//
	// With NO `>` anywhere the string is not an array, and `Index` returning -1 made the slice
	// `typeName[0:]` — a recursion on the IDENTICAL string that can never terminate. That is not a
	// survivable defect: a Go stack overflow is a FATAL error, not a panic, so the per-file recover
	// in the conversion driver cannot catch it and ONE unrenderable type takes the entire run down
	// (issue #33, `-recurse` dying at package 1456 of 1726). Every other branch here consumes at
	// least one byte before recursing, so bounding this one bounds the whole renderer.
	//
	// Falling through is deliberate: the remaining branches are finite and end at the default
	// name render, so the package still converts and the reader gets a named diagnostic plus a
	// C# name that fails visibly at compile time, rather than a dead run with no output at all.
	if strings.HasPrefix(typeName, "<") {
		if closeIndex := strings.IndexByte(typeName, '>'); closeIndex != -1 {
			return fmt.Sprintf("%s.array<%s>", RootNamespace, renderCSTypeName(typeName[closeIndex+1:], rootNested))
		}

		showWarning("Cannot render a C# type name for the unrecognized type expression \"%s\" - emitting it as a name", typeName)
	}

	if strings.HasPrefix(typeName, "map<") {
		innerType := typeName[4:]
		keyType, valueType := splitMapKeyValue(innerType)
		return fmt.Sprintf("%s.map<%s, %s>", RootNamespace, renderCSTypeName(keyType, rootNested), renderCSTypeName(valueType, rootNested))
	}

	// Find all types inside '<T1, T2>' type expressions and recurse into them for conversion
	if start := strings.Index(typeName, "<"); start != -1 {
		// Locate the matching closing '>' by bracket depth rather than the first '>' — the latter
		// mis-handles nested generics (e.g. Pointer<node<K, V>> would stop at the inner '>',
		// extract the unbalanced "node<K, V", and recurse into "node<K", slicing out of range).
		depth := 0
		end := -1

		for i := start; i < len(typeName); i++ {
			if typeName[i] == '<' {
				depth++
			} else if typeName[i] == '>' {
				depth--

				if depth == 0 {
					end = i
					break
				}
			}
		}

		// Only split when a matching '>' exists; otherwise the '<' is not a generic bracket (e.g.
		// the '<-' of a directional channel inside a func type) and is handled by a later branch.
		if end != -1 {
			subTypes := splitTopLevelTypes(typeName[start+1 : end])

			for i := range subTypes {
				// Trim BEFORE converting: splitTopLevelTypes keeps the ", " separator's space, so
				// later args arrive as " string". The conversion switch matches the type name
				// exactly (`case "string"`), so a leading space would miss it and fall through to
				// the default named-type path — emitting C# `string` (System.String) instead of
				// golib `@string`. That violates the generic `new()` constraint the converter adds
				// (CS0310) and breaks string-literal assignment (CS0029).
				subTypes[i] = renderCSTypeName(strings.TrimSpace(subTypes[i]), rootNested)
			}

			base := typeName[:start]

			// The type-vs-method collision Δ-rename keys on the BARE type name; a generic
			// instantiation reaches the default sanitize with its `<args>` attached
			// (`indirect<K, V>`), missing the map — internal/concurrent's `type indirect[K, V]`
			// vs `func (n *node[K, V]) indirect()` renamed the DECLARATION `Δindirect<K, V>`
			// while every use kept the raw name (CS0246 ×33, leaking into net/netip). Rename
			// the bare base at reassembly; a dotted (package-qualified) base never matches the
			// per-package map and keeps its exported-alias route.
			if nameCollisions[base] {
				base = getCollisionAvoidanceIdentifier(base)
			}

			typeName = fmt.Sprintf("%s<%s>%s", base, strings.Join(subTypes, ", "), typeName[end+1:])
		}
	}

	if typeName == "func()" {
		return delegateRoot(rootNested, "") + "Action"
	}

	if strings.HasPrefix(typeName, "func(") {
		// Find the matching closing parenthesis for the parameter list
		depth := 0
		closingParenIndex := -1

		for i := 5; i < len(typeName); i++ {
			if typeName[i] == '(' {
				depth++
			} else if typeName[i] == ')' {
				depth--
				if depth == -1 {
					closingParenIndex = i
					break
				}
			}
		}

		if closingParenIndex == -1 {
			return delegateRoot(rootNested, "") + "Action" // Malformed input (unexpected)
		}

		// Extract parameter types, handling nested functions
		paramString := typeName[5:closingParenIndex]
		paramTypes := extractTypes(paramString, rootNested)

		// extractTypes already renders each parameter in C# form (a NAMED param has its type
		// converted after the name is stripped; the bare-type case is converted in place), so use
		// its output directly. Re-running convertToCSTypeName here DOUBLE-converts an already-C#
		// param — an emitted `map<@string, ж<Object>>` re-fed through the `map<` arm's
		// splitMapKeyValue mis-parses to `map<@string, ж<Object>, >` (spurious trailing empty type
		// arg → CS1031), as in go/ast's `type Importer func(imports map[string]*Object, …)`.
		csTypeNames := paramTypes

		// A VARIADIC tail (marked by extractTypes) hoists into the golib delegate FAMILY —
		// `Actionꓸꓸꓸ<…>`/`Funcꓸꓸꓸ<…>` with the variadic ELEMENT type as the last parameter
		// type argument, matching iifeDelegateType's structural lowering exactly.
		family := ""

		if count := len(csTypeNames); count > 0 {
			if elem, ok := strings.CutPrefix(csTypeNames[count-1], EllipsisOperator); ok {
				family = EllipsisOperator
				csTypeNames[count-1] = elem
			}
		}

		// Check for return type after the closing parenthesis
		remainingType := strings.TrimSpace(typeName[closingParenIndex+1:])

		if len(remainingType) > 0 {
			// Has explicit return type. A PARENTHESIZED result list may carry Go NAMED
			// results (`(importPath string, ok bool)` — go/doc/comment's LookupPackage
			// func field): split the elements, strip the Go-ordered names, and rebuild —
			// ONE result unwraps to its bare type (a C# 1-tuple is CS8124), several yield
			// the C#-ordered named tuple `(@string importPath, bool ok)`.
			csReturnType := convertToCSResultList(remainingType, rootNested)

			if len(csTypeNames) > 0 {
				return fmt.Sprintf("%sFunc%s<%s, %s>", delegateRoot(rootNested, family), family, strings.Join(csTypeNames, ", "), csReturnType)
			}

			return fmt.Sprintf("%sFunc<%s>", delegateRoot(rootNested, ""), csReturnType)
		}

		// No return type, use Action
		if len(csTypeNames) > 0 {
			return fmt.Sprintf("%sAction%s<%s>", delegateRoot(rootNested, family), family, strings.Join(csTypeNames, ", "))
		}

		return delegateRoot(rootNested, "") + "Action"
	}

	// Handle pointer types
	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("%s.%s<%s>", RootNamespace, PointerPrefix, renderCSTypeName(typeName[1:], rootNested))
	}

	// A golib csproj-level `<Using Alias=…>` name is not a member of `go` and cannot be rooted:
	// it stands for a C# keyword or BCL type, and a using-alias RHS cannot see another alias.
	// Substitute the target directly so the rooted rendering is self-contained rather than
	// depending on getUsingAliasSafeTypeName's later sweep of the assembled string (which the
	// outermost name still takes, harmlessly — the substitution is idempotent).
	if rootNested {
		if safeName, isAlias := golibAliasSafeNames[typeName]; isAlias {
			return safeName
		}
	}

	// An ALREADY-ROOTED name is rooted: the default arm below prefixes the root namespace
	// unconditionally, and a white-box test conversion hands this renderer names the test-alias
	// qualifiers built with an explicit `global::` (testAliasShadowOperations) — so a `global using`
	// RHS came out as `go.global::go.<pkg>_package.T`, which is CS7000 "unexpected use of an aliased
	// name" (net/netip's export_test.go `type Uint128 = uint128`, and every same-shaped test alias
	// to a production type). Rooting is idempotent by intent, so say so here rather than at the one
	// caller: `global::` is the root, and prefixing it can only ever produce a name that is not one.
	if strings.HasPrefix(typeName, "global::") {
		return typeName
	}

	switch typeName {
	case "int":
		return "nint"
	case "uint":
		return "nuint"
	case "bool":
		return "bool"
	case "byte":
		return "byte"
	case "float":
		return "float64"
	case "complex64":
		return RootNamespace + ".complex64"
	case "string":
		return RootNamespace + ".@string"
	case "interface{}":
		return "any"
	case "struct{}":
		return RootNamespace + ".EmptyStruct"
	default:
		if strings.Contains(typeName, PackageSuffix) {
			parts := strings.Split(typeName, ".")
			count := len(parts)

			if count > 1 {
				sourcePkg := strings.TrimSuffix(parts[count-2], PackageSuffix)
				targetType := parts[count-1]
				alias := fmt.Sprintf("%s.%s", sourcePkg, targetType)

				packageLock.Lock()
				aliasType, exists := importedTypeAliases[alias]
				packageLock.Unlock()

				if exists {
					return aliasType
				}
			}
		}

		return fmt.Sprintf("%s.%s", RootNamespace, getSanitizedIdentifier(getAliasedTypeName(typeName)))
	}
}

// implicitConvStructTypeName renders the C# name a GoImplicitConv attribute can carry for a
// struct-underlying type: a NAMED type's converted name, or the lifted dynamic-type name for a
// package-level anonymous struct. A lifted name whose declaring file has not been visited yet
// records the DEFERRED MARKER, resolved after the file-visit barrier when the package_info
// lines are emitted (raw Go `struct{…}` text is never attribute-safe C#).
func (v *Visitor) implicitConvStructTypeName(t types.Type) string {
	if named, ok := types.Unalias(t).(*types.Named); ok {
		return v.getCSharpTypeName(named)
	}

	// This visitor's lifted name is type-identity-keyed — precise even when two anonymous
	// structs share a structural signature (Process_data vs main_data); the shared registry
	// and the deferred marker are the cross-file fallbacks.
	if name, ok := v.liftedNameFor(t); ok {
		return name
	}

	signature := t.String()

	if name := lookupDynamicTypeName(signature); name != "" {
		return name
	}

	return dynamicTypeMarker(signature)
}

// resolveImplicitConvTypeName resolves a possibly-deferred implicit-conversion type name after
// the file-visit barrier (the dynamic-type registry is complete). Returns ok=false when the
// marker cannot resolve — a genuinely unlifted anonymous struct has no attribute-safe name and
// its record is dropped.
func resolveImplicitConvTypeName(name string) (string, bool) {
	if strings.HasPrefix(name, dynamicTypeMarkerPrefix) && strings.HasSuffix(name, dynamicTypeMarkerSuffix) {
		payload := name[len(dynamicTypeMarkerPrefix) : len(name)-len(dynamicTypeMarkerSuffix)]
		signature, decoded := dynamicTypeMarkerSignature(payload)

		if !decoded {
			// Dropping the record stays the right outcome — there is still no attribute-safe name
			// — but this is a corrupted marker rather than the ordinary "never lifted" case, and
			// the two want opposite responses: one is a converter bug, the other is expected. Only
			// a report tells them apart, since the drop itself looks identical.
			showWarning("Undecodable dynamic-type marker payload \"%s\" in an implicit-conversion record", payload)
			return "", false
		}

		if resolved := lookupDynamicTypeName(signature); resolved != "" {
			return resolved, true
		}

		return "", false
	}

	return name, true
}
